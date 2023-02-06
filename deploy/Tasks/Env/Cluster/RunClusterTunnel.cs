using Cake.Common;
using Cake.Core;
using Cake.Core.IO;
using Cake.Frosting;

namespace Deploy.Tasks.Env.Cluster
{
	[IsDependentOn(typeof(StartMinikube))]
	public class RunClusterTunnel: FrostingTask
	{
		/// <inheritdoc />
		public override void Run(ICakeContext context)
		{
			var minikube = context.Tools.Resolve("minikube");
			var targetProfile = context.Argument("minikube-profile", "waddle");
			
			context.StartProcess(minikube, new ProcessSettings
			{
				Arguments = new ProcessArgumentBuilder()
					.Append("tunnel")
					.AppendSwitch("-p", targetProfile)
			});
		}
	}
}