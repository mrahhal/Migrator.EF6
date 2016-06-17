#if NET451

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
