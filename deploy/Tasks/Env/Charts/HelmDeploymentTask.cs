using System.Threading.Tasks;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Deploy.Tasks.Env.Charts
{
	public abstract class HelmDeploymentTask : AsyncFrostingTask<BuildContext>
	{
		protected abstract HelmReleaseConfig GetConfig(ICakeContext context);

		protected virtual string? GetNamespace()
		{
			return null;
		}

		protected string Namespace { get; private set; } = "default";

		protected int HelmExitCode = 0;

		protected async Task DeployChart(HelmReleaseConfig config, ICakeContext context)
		{
			var helm = context.Tools.Resolve("helm");

			context.Information($"Deploying {config.Name} chart...");

			Namespace = context.Argument($"clusterNamespaces-{config.Name}", GetNamespace() ?? config.Name);
			
			var failCount = 0;

			while (true)
			{
				var args = new ProcessArgumentBuilder()
					.Append("upgrade")
					.Append("--install")
					.AppendSwitch("--repo", config.Repository)
					.Append(config.Name)
					.Append(config.Chart)
					.Append("--create-namespace")
					.AppendSwitch("-n", Namespace);

				if (config.ValueFiles != null)
				{
					foreach (var valueFile in config.ValueFiles)
					{
						args = args.AppendSwitchQuoted("-f", valueFile.FullPath);
					}
				}

				void IterateValues(string pathPrefix, HelmValues values)
				{
					if (values.Values != null)
					{
						foreach (var (key, value) in values.Values)
						{
							var valuePath = string.Join('.', pathPrefix, key).TrimStart('.');
							args = args.AppendSwitchQuoted("--set", $"{valuePath}={value}");
						}
					}

					if (values.Children != null)
					{
						foreach (var (key, value) in values.Children)
						{
							IterateValues(string.Join('.', pathPrefix, key), value);
						}
					}
				}

				if (config.Values != null)
				{
					IterateValues("", config.Values.Value);
				}

				HelmExitCode = context.StartProcess(helm, new ProcessSettings
				{
					Arguments = args,
					RedirectStandardOutput = true,
					RedirectStandardError = true,
					RedirectedStandardOutputHandler = s =>
					{
						if (!string.IsNullOrWhiteSpace(s))
						{
							context.Debug("\x1b[44mHELM:\x1b[0m {0}", s);
						}

						return s;
					},
					RedirectedStandardErrorHandler = s =>
					{
						if (!string.IsNullOrWhiteSpace(s))
						{
							context.Error("\x1b[44mHELM:\x1b[0m {0}", s);
						}

						return s;
					}
				});

				if (HelmExitCode != 0)
				{
					failCount++;
					await Task.Delay(1000 * 3);
				}
				else
				{
					break;
				}

				if (failCount >= 3)
				{
					context.Error("Helm failed after 3 retries, aborting operation", HelmExitCode);
					throw new CakeException($"Helm exited with code {HelmExitCode}");
				}
				else
				{
					context.Error("Helm exited with code {0}, retrying (attempt {1} of 3)", HelmExitCode, failCount + 1);
				}
			}
		}

		/// <inheritdoc />
		public override Task RunAsync(BuildContext context)
		{
			Namespace = GetNamespace() ?? string.Empty;
			
			var config = GetConfig(context);

			return DeployChart(config, context);
		}
	}
}