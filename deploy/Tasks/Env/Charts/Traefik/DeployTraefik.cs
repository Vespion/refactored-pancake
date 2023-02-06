using System.Collections.Generic;
using Cake.Common;
using Cake.Core;
using Cake.Frosting;
using Deploy.Tasks.Env.Charts.CertManager;

namespace Deploy.Tasks.Env.Charts.Traefik
{
	[IsDependentOn(typeof(DeployCertManager))]
	public class DeployTraefik: HelmAndManifestDeploymentTask
	{
		/// <inheritdoc />
		protected override Dictionary<string, string>? GetReplacementValues(ICakeContext context)
		{
			return new Dictionary<string, string>
			{
				{ "CLUSTER_ISSUER_NAME", "ca-issuer" }
			};
		}

		/// <inheritdoc />
		protected override HelmReleaseConfig GetConfig(ICakeContext context)
		{
			var ns = context.Argument($"clusterNamespaces-traefik", GetNamespace() ?? "traefik");
			var valuesFile = context.Environment.WorkingDirectory
				.Combine("Tasks")
				.Combine("Env")
				.Combine("Charts")
				.Combine("Traefik")
				.CombineWithFilePath("values.yaml");

			return new HelmReleaseConfig("traefik", "https://traefik.github.io/charts", "traefik", new HelmValues(
				new Dictionary<string, string>
				{
					{"ports.web.middlewares[0]", $"{ns}-common-chain@kubernetescrd"},
					{"ports.websecure.middlewares[0]", $"{ns}-common-chain@kubernetescrd"},
					{"ports.metrics.middlewares[0]", $"{ns}-common-chain@kubernetescrd"},
					{"ports.traefik.middlewares[0]", $"{ns}-common-chain@kubernetescrd"},
				}, null), new []
			{
				valuesFile
			});
		}
	}
}