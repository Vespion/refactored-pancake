using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Frosting;
using Deploy.Tasks.Env.Charts;
using Deploy.Tasks.Env.Charts.CertManager;
using Deploy.Tasks.Env.Charts.Postgres;
using Deploy.Tasks.Env.Charts.SigNoz;
using Deploy.Tasks.Env.Charts.Traefik;
using Deploy.Tasks.Env.Cluster;
using Deploy.Tasks.Env.Manifests;
using Deploy.Tasks.Env.Manifests.IdentityDatabase;
using Deploy.Tasks.Env.Manifests.KubeDash;

namespace Deploy.Tasks.Env
{
	[IsDependentOn(typeof(StartMinikube))]
	[IsDependentOn(typeof(DeploySigNoz))]
	[IsDependentOn(typeof(DeployOpenEBS))]
	[IsDependentOn(typeof(DeployCertManager))]
	[IsDependentOn(typeof(DeployTraefik))]
	[IsDependentOn(typeof(DeployPostgres))]
	[IsDependentOn(typeof(DeployKubeDash))]
	[IsDependentOn(typeof(DeployIdentityDatabase))]
	public class StartDevEnvironment: FrostingTask
	{
		/// <inheritdoc />
		public override void Run(ICakeContext context)
		{
			context.Log.Information("Development environment started");
		}
	}
	
	[IsDependentOn(typeof(DeleteMinikube))]
	public class StopDevEnvironment: FrostingTask
	{
		
	}
}