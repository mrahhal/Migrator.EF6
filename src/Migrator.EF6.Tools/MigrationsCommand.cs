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

					enable.OnExecute(() =>
					{
						new Executor().EnableMigrations();
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

						new Executor().AddMigration(name.Value, ignoreChanges.HasValue());
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
							output.Value());
					});
				});

			command.Command(
			   "list",
			   list =>
			   {
				   list.Description = "List the migrations";
				   list.HelpOption();

				   list.OnExecute(() =>
				   {
					   new Executor().ListMigrations();
				   });
			   });

			command.OnExecute(() => command.ShowHelp());
		}
	}
}

#endif
