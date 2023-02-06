using System.Collections.Generic;
using Cake.Core;

namespace Deploy.Tasks.Env.Charts
{
	public class DeployOpenEBS: HelmDeploymentTask
	{
		/// <inheritdoc />
		protected override HelmReleaseConfig GetConfig(ICakeContext context)
		{
			return new HelmReleaseConfig(
				"openebs",
				"https://openebs.github.io/charts",
				"openebs",
				new HelmValues(
					null,
					new Dictionary<string, HelmValues>
					{
						{
							"localprovisioner", new HelmValues(
								new Dictionary<string, string>
								{
									{ "basePath", "/data"},
									{ "enableDeviceClass", "false"}
								},
								new Dictionary<string, HelmValues>
								{
									{
										"hostpathClass", new HelmValues(
											new Dictionary<string, string>
											{
												{"isDefaultClass", "true"}
											},
											null
										)
									}
								}
							)
						},
						{
							"jiva", new HelmValues(
								new Dictionary<string, string>
								{
									{ "enabled", "false"}
								},
								null
							)
						},
						{
							"ndm", new HelmValues(
								new Dictionary<string, string>
								{
									{ "enabled", "false"}
								},
								null
							)
						},
						{
							"ndmOperator", new HelmValues(
								new Dictionary<string, string>
								{
									{ "enabled", "false"}
								},
								null
							)
						}
					}
				)
			);
		}
	}
}