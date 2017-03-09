#if NET46

using System.Reflection;
using Microsoft.Extensions.CommandLineUtils;
using Migrator.EF6.Tools.Extensions;

namespace Migrator.EF6.Tools
{
	public class ExecuteCommand
	{
		public static CommandLineApplication Create()
		{
			var app = new CommandLineApplication()
			{
				Name = "dotnet ef",
				FullName = "Entity Framework 6 .NET Core CLI Commands"
			};

			app.HelpOption();
			app.VersionOption(GetVersion);

			app.Command("database", c => DatabaseCommand.Configure(c));
			app.Command("migrations", c => MigrationsCommand.Configure(c));

			app.OnExecute(() => app.ShowHelp());

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
