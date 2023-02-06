using Cake.Core;

namespace Deploy.Tasks.Env.Charts.Postgres
{
	public class DeployPostgres: HelmDeploymentTask
	{
		/// <inheritdoc />
		protected override HelmReleaseConfig GetConfig(ICakeContext context)
		{
			return new HelmReleaseConfig(
				"cnpg",
				"https://cloudnative-pg.github.io/charts",
				"cloudnative-pg",
				null,
				null
			);
		}
	}
}