using System;
using System.Collections.Generic;
using System.IO;
using Cake.Common.Diagnostics;
using Cake.Core;
using Cake.Core.IO;

namespace Deploy.Tasks.Env.Charts.CertManager
{
	public class DeployCertManager: HelmAndManifestDeploymentTask
	{
		/// <inheritdoc />
		protected override HelmReleaseConfig GetConfig(ICakeContext context)
		{
			return new HelmReleaseConfig("cert-manager", "https://charts.jetstack.io", "cert-manager", new HelmValues(
				new Dictionary<string, string>
				{
					{"installCRDs", "true"},
					{"featureGates","ExperimentalCertificateSigningRequestControllers=true"}
				},
				new Dictionary<string, HelmValues>()
				));
		}

		/// <inheritdoc />
		protected override Dictionary<string, string> GetReplacementValues(ICakeContext context)
		{
			context.Debug("Loading certificate data...");

			var dir = context.Environment.WorkingDirectory
				.Combine("Tasks")
				.Combine("Env")
				.Combine("Charts")
				.Combine("CertManager");
			
			var crtFilePath = dir.GetFilePath("cert.crt");
			var keyFilePath = dir.GetFilePath("cert.key");

			var crtFile = context.FileSystem.GetFile(crtFilePath);
			var keyFile = context.FileSystem.GetFile(keyFilePath);

			string ConvertToBase64(Stream stream)
			{
				byte[] bytes;
				using (var memoryStream = new MemoryStream())
				{
					stream.CopyTo(memoryStream);
					bytes = memoryStream.ToArray();
				}

				return Convert.ToBase64String(bytes);
			}

			string crt;
			using (var stream = crtFile.OpenRead())
			{
				crt = ConvertToBase64(stream);
			}
			
			string key;
			using (var stream = keyFile.OpenRead())
			{
				key = ConvertToBase64(stream);
			}
			
			context.Debug("Certificate data loaded");
			
			return new Dictionary<string, string>
			{
				{ "CLUSTER_ISSUER_NAME", "ca-issuer" },
				{ "CA_CRT", crt },
				{ "CA_KEY", key }
			};
		}
	}
}