#if NET451

using System;
using Microsoft.Extensions.CommandLineUtils;
using Migrator.EF6.Tools.Extensions;

namespace Migrator.EF6.Tools
{
	public class MigrationsCommand : CommandBase
	{
		public static void Configure(CommandLineApplication command)
		{
			new MigrationsCommand().ConfigureInternal(command);
		}

		private void ConfigureInternal(CommandLineApplication command)
		{
			command.Description = "Commands to manage your migrations";
			command.HelpOption();

			command.Command(
				"enable",
				enable =>
				{
					enable.Description = "Enable migrations";
					enable.HelpOption();

					var outputDir = enable.Option(
						"-o|--output-dir <path>",
						"The directory (and sub-namespace) to use. If omitted, \"Migrations\" is used. Relative paths are relative the directory in which the command is executed.");

					var common = Common(enable);

					enable.OnExecute(() =>
					{
						common.CreateExecutor().EnableMigrations(outputDir.Value());
					});
				});

			command.Command(
				"add",
				add =>
				{
					add.Description = "Add a new migration";
					add.HelpOption();

					var name = add.Argument(
						"[name]",
						"The name of the migration");

					var common = Common(add)
						.AddConnectionStringOption()
						.AddProviderNameOption()
						.AddContextOption();

					var outputDir = add.Option(
						"-o|--output-dir <path>",
						"The directory (and sub-namespace) to use. If omitted, \"Migrations\" is used. Relative paths are relative the directory in which the command is executed");

					var ignoreChanges = add.Option(
						"--ignore-changes",
						"Ignore changes and start with an empty migration",
						CommandOptionType.NoValue);

					add.OnExecute(() =>
					{
						if (string.IsNullOrEmpty(name.Value))
						{
							return 1;
						}

						common.CreateExecutor().AddMigration(
							name.Value,
							outputDir.Value(),
							ignoreChanges.HasValue());
						return 0;
					});
				});

			command.Command(
				"script",
				script =>
				{
					script.Description = "Generate a SQL script from migrations";
					script.HelpOption();

					var from = script.Argument(
						"[from]",
						"The starting migration. If omitted, '0' (the initial database) is used");

					var to = script.Argument(
						"[to]",
						"The ending migration. If omitted, the last migration is used");

					var common = Common(script)
						.AddConnectionStringOption()
						.AddProviderNameOption()
						.AddContextOption();

					var output = script.Option(
						"-o|--output <file>",
						"The file to write the script to instead of stdout");

					script.OnExecute(() =>
					{
						if (!output.HasValue())
						{
							Console.WriteLine("The --output option is required.");
							return;
						}

						common.CreateExecutor().ScriptMigration(
							from.Value,
							to.Value,
							output.Value());
					});
				});

			command.Command(
			   "list",
			   list =>
			   {
				   list.Description = "List the migrations";
				   list.HelpOption();

				   var common = Common(list)
						.AddConnectionStringOption()
						.AddProviderNameOption()
						.AddContextOption();

				   list.OnExecute(() =>
				   {
					   common.CreateExecutor().ListMigrations();
				   });
			   });

			command.OnExecute(() => command.ShowHelp());
		}
	}
}

#endif
