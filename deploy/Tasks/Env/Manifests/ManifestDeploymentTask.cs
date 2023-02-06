using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Cake.Common;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;
using Path = System.IO.Path;

namespace Deploy.Tasks.Env.Manifests
{
	public abstract class ManifestDeploymentTask : AsyncFrostingTask
	{
		protected abstract (string Name, string Namespace) GetConfig(ICakeContext context);
		
		protected virtual Dictionary<string, string>? GetReplacementValues(ICakeContext context)
		{
			return null;
		}

		/// <inheritdoc />
		public override async Task RunAsync(ICakeContext context)
		{
			await base.RunAsync(context);
			
			context.Information($"Deploying {GetConfig(context).Name} manifests...");
			
			var manifestDirectory = context.FileSystem.GetDirectory(
				context.Environment.WorkingDirectory
					.Combine("Tasks")
					.Combine("Env")
					.Combine("Manifests")
					.Combine(this.GetType().Name.Remove(0, 6))
					.Combine("manifests")
			);

			var manifests = manifestDirectory.GetFiles("*.yaml", SearchScope.Recursive);
			
			var replacements = GetReplacementValues(context);
			
			var kubectl = context.Tools.Resolve("kubectl");

			foreach (var manifest in manifests)
			{
				var failCount = 0;
				while (true)
				{
					context.Debug("Deploying manifest {0}...", manifest.Path.GetFilenameWithoutExtension());
					context.Verbose("Transforming manifest {0}...", manifest.Path.GetFilenameWithoutExtension());
					string strManifest;
					using (var reader = new StreamReader(manifest.OpenRead(), leaveOpen: false))
					{
						strManifest = await reader.ReadToEndAsync();
					}

					strManifest = strManifest.Replace("$NAMESPACE", GetConfig(context).Namespace);
					strManifest = strManifest.Replace("$DOMAIN",
						context.Argument($"cluster-domain", "waddle.localhost"));

					if (replacements != null)
					{
						foreach (var (key, value) in replacements)
						{
							strManifest = strManifest.Replace($"${key}", value);
						}
					}

					context.Debug("Submitting manifest {0} to cluster...", manifest.Path.GetFilenameWithoutExtension());

					var tempFilePath = Path.GetTempFileName();

					try
					{
						await File.WriteAllTextAsync(tempFilePath, strManifest);

						var kubectlExitCode = context.StartProcess(kubectl, new ProcessSettings
						{
							Arguments = new ProcessArgumentBuilder()
								.Append("apply")
								.AppendSwitchQuoted("-f", tempFilePath),
							RedirectStandardOutput = true,
							RedirectStandardError = true,
							RedirectedStandardOutputHandler = s =>
							{
								if (!string.IsNullOrWhiteSpace(s))
								{
									context.Debug("\x1b[43;1mKUBECTL:\x1b[0m {0}", s);
								}

								return s;
							},
							RedirectedStandardErrorHandler = s =>
							{
								if (!string.IsNullOrWhiteSpace(s))
								{
									context.Error("\x1b[43;1mKUBECTL:\x1b[0m {0}", s);
								}

								return s;
							}
						});

						if (kubectlExitCode != 0)
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
							context.Error("Kubectrl failed after 3 retries, aborting operation", kubectlExitCode);
							throw new CakeException($"Kubectl exited with code {kubectlExitCode}");
						}
						else
						{
							context.Error("Failed to deploy manifest {0}!, retrying (attempt {1} of 3)",
								manifest.Path.GetFilenameWithoutExtension(), failCount + 1);
						}
					}
					finally
					{
						if (File.Exists(tempFilePath))
							File.Delete(tempFilePath);
					}
				}
			}
		}
	}
}