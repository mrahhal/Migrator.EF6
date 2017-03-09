#if NET46

using System;
using Microsoft.Extensions.CommandLineUtils;
using Migrator.EF6.Tools.Extensions;

namespace Migrator.EF6.Tools
{
	public class DatabaseCommand : CommandBase
	{
		public static void Configure(CommandLineApplication command)
		{
			new DatabaseCommand().ConfigureInternal(command);
		}

		private void ConfigureInternal(CommandLineApplication command)
		{
			command.Description = "Commands to manage your database";
			command.HelpOption();

			command.Command(
				"update",
				update =>
				{
					update.Description = "Updates the database to a specified migration";
					update.HelpOption();

					var migrationName = update.Argument(
						"[migration]",
						"The target migration. If '0', all migrations will be reverted. If omitted, all pending migrations will be applied");

					var common = Common(update)
						.AddConnectionStringOption()
						.AddProviderNameOption()
						.AddContextOption();

					var force = update.Option(
						"--force",
						"Force update, ignoring possible data loss",
						CommandOptionType.NoValue);

					update.OnExecute(() =>
					{
						common.CreateExecutor().UpdateDatabase(migrationName.Value, force.HasValue());
					});
				});

			command.Command(
				"truncate",
				truncate =>
				{
					truncate.Description = "Truncates all tables in the database. This is basically 'database update 0'";
					truncate.HelpOption();

					var common = Common(truncate)
						.AddConnectionStringOption()
						.AddProviderNameOption()
						.AddContextOption();

					truncate.OnExecute(() =>
					{
						common.CreateExecutor().UpdateDatabase("0", true);
					});
				});

			command.Command(
				"recreate",
				recreate =>
				{
					recreate.Description = "Truncates all tables then updates the database to the latest migration";
					recreate.HelpOption();

					var common = Common(recreate)
						.AddConnectionStringOption()
						.AddProviderNameOption()
						.AddContextOption();

					recreate.OnExecute(() =>
					{
						Console.WriteLine("Truncating all tables...");
						common.CreateExecutor().UpdateDatabase("0", true);
						Console.WriteLine("Updating to latest migration...");
						common.CreateExecutor().UpdateDatabase(null);
					});
				});

			command.OnExecute(() => command.ShowHelp());
		}
	}
}

#endif
