using System.Collections.Generic;
using Microsoft.DotNet.Cli.Utils;
using NuGet.Frameworks;

namespace Migrator.EF6.Tools
{
	public class BuildCommandFactory
	{
		public static ICommand Create(
			   string project,
			   string configuration,
			   NuGetFramework framework,
			   string buildBasePath,
			   string output)
		{
			var args = new List<string>()
			{
				project,
				"--configuration", configuration,
				"--framework", framework.GetShortFolderName()
			};

			if (buildBasePath != null)
			{
				args.Add("--build-base-path");
				args.Add(buildBasePath);
			}

			if (output != null)
			{
				args.Add("--output");
				args.Add(output);
			}

			return Command.CreateDotNet(
				"build",
				args,
				framework,
				configuration);
		}
	}
}
