#if NET451

using System;
using Microsoft.Extensions.CommandLineUtils;
using Migrator.EF6.Tools.Extensions;

namespace Migrator.EF6.Tools
{
	public class MigrationsCommand
	{
		public static void Configure(CommandLineApplication command)
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

					enable.OnExecute(() =>
					{
						new Executor().EnableMigrations(outputDir.Value());
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

					var context = add.Option(
						"-c|--context <context>",
						"The DbContext to use. If omitted, the default DbContext is used");

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

						new Executor().AddMigration(
							name.Value,
							context.Value(),
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

					var context = script.Option(
						"-c|--context <context>",
						"The DbContext to use. If omitted, the default DbContext is used");

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

						new Executor().ScriptMigration(
							from.Value,
							to.Value,
							context.Value(),
							output.Value());
					});
				});

			command.Command(
			   "list",
			   list =>
			   {
				   list.Description = "List the migrations";
				   list.HelpOption();

				   var context = list.Option(
					   "-c|--context <context>",
					   "The DbContext to use. If omitted, the default DbContext is used");

				   list.OnExecute(() =>
				   {
					   new Executor().ListMigrations(context.Value());
				   });
			   });

			command.OnExecute(() => command.ShowHelp());
		}
	}
}

#endif
