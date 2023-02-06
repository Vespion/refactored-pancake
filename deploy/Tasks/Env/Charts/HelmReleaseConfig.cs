using System.Collections.Generic;
using Cake.Core.IO;

namespace Deploy.Tasks.Env.Charts
{
	public struct HelmValues
	{
		public HelmValues(Dictionary<string, string>? values, Dictionary<string, HelmValues>? children)
		{
			Values = values;
			Children = children;
		}

		public Dictionary<string, string>? Values { get; set; }
		
		public Dictionary<string, HelmValues>? Children { get; set; }
	}

	public readonly struct HelmReleaseConfig
	{
		public HelmReleaseConfig(string name, string repository, string chart,
			HelmValues? values = null,
			FilePath[]? valueFiles = null)
		{
			Name = name;
			Repository = repository;
			Chart = chart;
			Values = values;
			ValueFiles = valueFiles;
		}

		public string Name { get; }
		
		public string Repository { get; }
		
		public string Chart { get; }
		
		public HelmValues? Values { get; }
		public FilePath[]? ValueFiles { get; }
	}
}