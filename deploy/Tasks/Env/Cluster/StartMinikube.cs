using System;
using System.IO;
using System.Linq;
using Cake.Common;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;

namespace Deploy.Tasks.Env.Cluster
{
	public class StartMinikube: FrostingTask<BuildContext>
	{
		/// <inheritdoc />
		public override void Run(BuildContext context)
		{
			var minikube = context.Tools.Resolve("minikube");
			var targetProfile = context.Argument("minikube-profile", "waddle");
			
			var gcMemoryInfo = GC.GetGCMemoryInfo();
			var installedMemory = gcMemoryInfo.TotalAvailableMemoryBytes;
			// it will give the size of memory in MB
			var physicalMemory = installedMemory / 1048576.0;
			
			var process = context.StartAndReturnProcess(minikube, new ProcessSettings
			{
				Arguments = new ProcessArgumentBuilder()
					.Append("start")
					.AppendSwitch("-p", targetProfile)
					.Append("--delete-on-failure")
					.AppendSwitch("-n", context.Argument("minikube-nodes", 3).ToString())
					// .AppendSwitch("--container-runtime", context.Argument("minikube-runtime", "containerd"))
					.AppendSwitch("--addons", "metrics-server")
					.AppendSwitch("--extra-config", "kubelet.housekeeping-interval=10s")
					.AppendSwitch("--addons", "dashboard")
					.AppendSwitch("--cpus", (Environment.ProcessorCount >= 4 ? Environment.ProcessorCount / 4 : 1).ToString())
					.AppendSwitch("--memory", $"{physicalMemory / 4}m")
					.AppendSwitch("--wait", "all")
					.AppendSwitch("--wait-timeout", "6m0s")
			});

			process.WaitForExit();

			context.StartProcess(minikube, new ProcessSettings
			{
				Arguments = new ProcessArgumentBuilder()
					.AppendSwitch("-p", targetProfile)
					.Append("addons")
					.Append("disable")
					.Append("default-storageclass")
			});
		}

		/// <inheritdoc />
		public override bool ShouldRun(BuildContext context)
		{
			var skipCheck = context.Argument("minikube-skipProfileCheck", false);

			if (skipCheck)
			{
				return true;
			}
			
			var targetProfile = context.Argument("minikube-profile", "waddle");
			
			var minikube = context.Tools.Resolve("minikube");
			
			var process = context.StartAndReturnProcess(minikube, new ProcessSettings
			{
				Arguments = new ProcessArgumentBuilder()
					.Append("profile list")
					.AppendSwitch("-o", "json"),
				RedirectStandardOutput = true
			});

			process.WaitForExit();
			
			var json = process.GetStandardOutput().First();

			var doc = System.Text.Json.JsonDocument.Parse(json);
		
			var validProfiles = doc.RootElement.GetProperty("valid")
				.EnumerateArray()
				.Select(x => x.GetProperty("Name"))
				.Select(x => x.GetString()?? throw new InvalidDataException())
				.ToArray();
		
			var invalidProfiles = doc.RootElement.GetProperty("invalid")
				.EnumerateArray()
				.Select(x => x.GetProperty("Name"))
				.Select(x => x.GetString() ?? throw new InvalidDataException())
				.ToArray();

			if (validProfiles.Contains(targetProfile))
			{
				context.Log.Information("Profile {0} already exists, skipping minikube create", targetProfile);
				return false;
			}
				
			if (invalidProfiles.Contains(targetProfile))
			{
				context.Log.Error("Profile {0} is in an invalid state, you will need to destroy the cluster and may lose data", targetProfile);
				throw new OperationCanceledException($"Profile {targetProfile} is in an invalid state");
			}
			
			return true;
		}
	}
}