#if NET451
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Migrator.EF6.Tools.Extensions;
using NuGet.Frameworks;

namespace Migrator.EF6.Tools
{
	public class ExecuteCommand
	{
		private const string FrameworkOptionTemplate = "--framework";
		private const string ConfigOptionTemplate = "--configuration";
		private const string VerboseOptionTemplate = "--verbose";

		public static IEnumerable<string> CreateArgs(
			NuGetFramework framework,
			string configuration,
			bool verbose)
			=> new[]
			{
				FrameworkOptionTemplate, framework.GetShortFolderName(),
				ConfigOptionTemplate, configuration,
				verbose ? VerboseOptionTemplate : string.Empty
			};

		public static CommandLineApplication Create()
		{
			var app = new CommandLineApplication()
			{
				Name = "dotnet ef6",
				FullName = "Entity Framework 6 .NET Core CLI Commands"
			};

			app.HelpOption();
			app.VerboseOption();
			app.VersionOption(GetVersion);

			var commonOptions = new CommonCommandOptions
			{
				Framework = app.Option(FrameworkOptionTemplate + " <FRAMEWORK>",
					"Target framework to load",
					CommandOptionType.SingleValue),
				Configuration = app.Option(ConfigOptionTemplate + " <CONFIGURATION>",
					"Configuration under which to load",
					CommandOptionType.SingleValue)
			};

			app.Command("database", c => DatabaseCommand.Configure(c, commonOptions));
			app.Command("migrations", c => MigrationsCommand.Configure(c, commonOptions));

			app.OnExecute(
				() =>
				{
					app.ShowHelp();
				});

			return app;
		}

		private static string GetVersion()
		   => typeof(ExecuteCommand)
			   .GetTypeInfo()
			   .Assembly
			   .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
			   .InformationalVersion;
	}
}
#endif
