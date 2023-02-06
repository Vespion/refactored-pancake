using System;
using System.Reflection;
using System.Threading.Tasks;
using Cake.Core;
using Cake.Core.Diagnostics;
using Cake.Core.IO;
using Cake.Frosting;
using Cake.Frosting.Issues.Recipe;

namespace Deploy
{
	public static class Program
	{
		public static int Main(string[] args)
		{
			return new CakeHost()
				.UseContext<BuildContext>()
				.AddAssembly(Assembly.GetAssembly(typeof(IssuesTask)))
				.Run(args);
		}
	}

	public class BuildContext : IssuesContext
	{
		public BuildContext(ICakeContext context)
			: base(context, RepositoryInfoProviderType.CakeGit)
		{
		}
		
	}

	[TaskName("Default")] 
	[IsDependentOn(typeof(IssuesTask))]
	public class Default : FrostingTask
	{
	}
}