using System.Collections.Generic;
using System.Threading.Tasks;
using Cake.Common;
using Cake.Core;

namespace Deploy.Tasks.Env.Charts.SigNoz;

public class DeploySigNoz: HelmDeploymentTask
{
	/// <inheritdoc />
	protected override HelmReleaseConfig GetConfig(ICakeContext context)
	{
		return new HelmReleaseConfig(
			"signoz",
			"https://charts.signoz.io",
			"signoz",
			new HelmValues(
				new Dictionary<string, string>
				{
					{"frontend.ingress.enabled", "true"},
					{"frontend.ingress.hosts[0].host", $"health.{context.Argument("cluster-domain", "waddle.localhost")}"},
					{"frontend.ingress.hosts[0].paths[0].path", "/"},
					{"frontend.ingress.hosts[0].paths[0].pathType", "ImplementationSpecific"},
					{"frontend.ingress.hosts[0].paths[0].port", "3301"},
					{"frontend.ingress.tls[0].secretName", "signoz-frontend-cert"},
					{"frontend.ingress.annotations.\"traefik\\.ingress\\.kubernetes\\.io/router\\.entrypoints\"", "websecure"}
				},
				null
			)
		);
	}

	/// <inheritdoc />
	public override Task RunAsync(BuildContext context)
	{
		return base.RunAsync(context);
	}
}