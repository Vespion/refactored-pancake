using Cake.Core;
using Deploy.Tasks.Env.Charts;

namespace Deploy.Tasks.Env.Manifests.IdentityDatabase;

public class DeployIdentityDatabase: ManifestDeploymentTask
{
	/// <inheritdoc />
	protected override (string Name, string Namespace) GetConfig(ICakeContext context)
	{
		return ("identity-cluster", "identity");
	}
}