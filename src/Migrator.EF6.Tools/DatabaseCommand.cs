#if NET451

using System;
using Microsoft.Extensions.CommandLineUtils;
using Migrator.EF6.Tools.Extensions;

namespace Migrator.EF6.Tools
{
	public class DatabaseCommand
	{
		public static void Configure(CommandLineApplication command, CommonCommandOptions commonOptions)
		{
			command.Description = "Commands to manage your database";

			command.HelpOption();
			command.VerboseOption();

			//command.Command("update", c => DatabaseUpdateCommand.Configure(c, commonOptions));
			//command.Command("drop", c => DatabaseDropCommand.Configure(c, commonOptions));

			command.Command(
						"update",
						update =>
						{
							update.Description = "Updates the database to a specified migration";
							var migrationName = update.Argument(
								"[migration]",
								"The target migration. If '0', all migrations will be reverted. If omitted, all pending migrations will be applied");
							update.OnExecute(
								() =>
								{
									new Executor().UpdateDatabase(migrationName.Value);
								});
						});
			command.Command(
				"truncate",
				truncate =>
				{
					truncate.Description = "Truncates all tables in the database. This is basically 'database update 0'";
					truncate.OnExecute(
						() =>
						{
							new Executor().UpdateDatabase("0");
						});
				});
			command.Command(
				"recreate",
				recreate =>
				{
					recreate.Description = "Truncates all tables then updates the database to the latest migration";
					recreate.OnExecute(
						() =>
						{
							Console.WriteLine("Truncating all tables...");
							new Executor().UpdateDatabase("0");
							Console.WriteLine("Updating to latest migration...");
							new Executor().UpdateDatabase(null);
						});
				});

			command.OnExecute(() => command.ShowHelp());
		}
	}
}

#endif
