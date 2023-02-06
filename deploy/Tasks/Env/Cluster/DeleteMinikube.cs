using System.IO;
using System.Linq;
using Cake.Common;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;

namespace Deploy.Tasks.Env.Cluster
{
	public class DeleteMinikube: FrostingTask
	{
		/// <inheritdoc />
		public override void Run(ICakeContext context)
		{
			var minikube = context.Tools.Resolve("minikube");
			var targetProfile = context.Argument("minikube-profile", "waddle");
			
			var process = context.StartAndReturnProcess(minikube, new ProcessSettings
			{
				Arguments = new ProcessArgumentBuilder()
					.Append("delete")
					.AppendSwitch("-p", targetProfile)
			});

			process.WaitForExit();
		}

		/// <inheritdoc />
		public override bool ShouldRun(ICakeContext context)
		{
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

			if (!validProfiles.Contains(targetProfile) && !invalidProfiles.Contains(targetProfile))
			{
				context.Log.Warning("Profile {0} does not exist, skipping minikube delete", targetProfile);
				return false;
			}
			
			return true;
		}
	}
}