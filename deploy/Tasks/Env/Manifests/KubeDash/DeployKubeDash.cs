using System.Collections.Generic;
using Cake.Core;
using Cake.Frosting;
using Deploy.Tasks.Env.Charts;
using Deploy.Tasks.Env.Charts.Traefik;

namespace Deploy.Tasks.Env.Manifests.KubeDash
{
	[IsDependentOn(typeof(DeployTraefik))]
	public class DeployKubeDash: ManifestDeploymentTask
	{
		/// <inheritdoc />
		protected override (string Name, string Namespace) GetConfig(ICakeContext context)
		{
			return ("KubeDash", "kubernetes-dashboard");
		}

		/// <inheritdoc />
		protected override Dictionary<string, string>? GetReplacementValues(ICakeContext context)
		{
			return new Dictionary<string, string>
			{
				{ "KUBE_DASH_NAMESPACE", "kubernetes-dashboard" }
			}; 
		}
	}
}