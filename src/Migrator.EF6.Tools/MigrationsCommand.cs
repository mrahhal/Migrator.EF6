#if NET451

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.CommandLineUtils;
using Migrator.EF6.Tools.Extensions;

namespace Migrator.EF6.Tools
{
	public class MigrationsCommand
	{
		public static void Configure(CommandLineApplication command, CommonCommandOptions commonOptions)
		{
			command.Command(
						"enable",
						enable =>
						{
							enable.Description = "Enable migrations";
							enable.OnExecute(
								() =>
								{
									new Executor().EnableMigrations();
								});
						});
			command.Command(
				"add",
				add =>
				{
					add.Description = "Add a new migration";
					add.HelpOption("-?|-h|--help");
					add.OnExecute(() => add.ShowHelp());
					var name = add.Argument(
						"[name]",
						"The name of the migration");
					var ignoreChanges = add.Option(
						"--ignore-changes",
						"Ignore changes and start with an empty migration",
						CommandOptionType.NoValue);
					add.OnExecute(
						() =>
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
				   list.HelpOption("-?|-h|--help");
				   list.OnExecute(() => list.ShowHelp());
				   list.OnExecute(
					   () =>
					   {
						   new Executor().ListMigrations();
					   });
			   });
		}
	}
}

#endif
