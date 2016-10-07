#if NET451

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.ProjectModel;
using Migrator.EF6.Tools.Extensions;

namespace Migrator.EF6.Tools
{
	public class Executor
	{
		private string _projectDir;
		private string _rootNamespace;
		private Assembly _startupAssembly;
		private string _targetName;
		private Type[] _types;

		public Executor()
		{
			var projectFile = Path.Combine(Directory.GetCurrentDirectory(), Project.FileName);
			var project = ProjectReader.GetProject(projectFile);

			_targetName = project.Name;
			_projectDir = project.ProjectDirectory;
			_startupAssembly = Assembly.Load(new AssemblyName(_targetName));
			_rootNamespace = project.Name;
			_types = _startupAssembly.GetTypes();
		}

		public void EnableMigrations(string outputDir)
		{
			var migrationsDir = GetMigrationsDir(outputDir);
			Directory.CreateDirectory(migrationsDir);
			var path = Combine(migrationsDir, "Configuration.cs");
			var appDbContextTypeName = FindAppDbContextTypeName();

			var assembly = Assembly.GetExecutingAssembly();
			var fileContent = default(string);

			using (var resourceStream = assembly.GetManifestResourceStream("Migrator.EF6.Tools.ConfigurationTemplate.txt"))
			using (var reader = new StreamReader(resourceStream))
			{
				fileContent = reader.ReadToEnd();
			}

			// Write Configuration.cs file.
			fileContent = fileContent.Replace("_RootNamespace_", _rootNamespace);

			if (appDbContextTypeName != null)
			{
				fileContent = fileContent.Replace("ApplicationDbContext", appDbContextTypeName);
			}

			File.WriteAllText(path, fileContent);
		}

		public void AddMigration(string name, string context, string outputDir, bool ignoreChanges)
		{
			var config = FindDbMigrationsConfiguration(context);
			var migrationsDir = GetMigrationsDir(outputDir ?? config.MigrationsDirectory);
			Directory.CreateDirectory(migrationsDir);

			// Scaffold migration.
			var scaffolder = new MigrationScaffolder(config);
			var migration = scaffolder.Scaffold(name, ignoreChanges);

			// Write the user code file.
			File.WriteAllText(Combine(migrationsDir, migration.MigrationId + ".cs"), migration.UserCode);

			// Write needed resource values directly inside the designer code file.
			// Apparently, aspnet and resource files don't play well (or more specifically,
			// the way ef6 migration generator is interacting with the resources system)
			var targetValue = migration.Resources["Target"];
			var designerCode = migration.DesignerCode
				.Replace("Resources.GetString(\"Target\")", $"\"{targetValue}\"");

			// Write the designer code file.
			File.WriteAllText(Path.Combine(migrationsDir, migration.MigrationId + ".Designer.cs"), designerCode);
		}

		public void ScriptMigration(string from, string to, string context, string output)
		{
			var config = FindDbMigrationsConfiguration(context);
			var migrator = new DbMigrator(config);
			var scriptingDecorator = new MigratorScriptingDecorator(migrator);
			var script = scriptingDecorator.ScriptUpdate(from ?? "0", to);
			File.WriteAllText(output, script);
			Console.WriteLine($"Scripted migration as SQL to file '{output}'.");
		}

		public void UpdateDatabase(string targetMigration, string context, bool force = false)
		{
			var resolvedMigrationName = ResolveMigrationName(context, targetMigration);
			var config = FindDbMigrationsConfiguration(context);
			config.AutomaticMigrationDataLossAllowed = force;

			var targetMigrationFriendlyName = resolvedMigrationName ?? "latest";
			Console.WriteLine($"Target migration: {targetMigrationFriendlyName}");

			var migrator = new DbMigrator(config);
			if (resolvedMigrationName == null)
			{
				migrator.Update();
			}
			else
			{
				migrator.Update(resolvedMigrationName);
			}
		}

		public void ListMigrations(string context)
		{
			foreach (var migration in GetMigrations(context))
			{
				Console.WriteLine(migration);
			}
		}

		private string ResolveMigrationName(string context, string targetMigration)
		{
			if (string.IsNullOrWhiteSpace(targetMigration))
			{
				// If null or empty, just return the same value.
				return targetMigration;
			}

			// Otherwise, let's see if it's of the form "~[number of migrations to go back]".
			if (targetMigration[0] != '~')
			{
				// Probably a migration name, so just return it.
				return targetMigration;
			}

			// A relative migration name, let's resolve it.
			var numberString = targetMigration.Substring(1);
			var number = default(int);
			if (string.IsNullOrWhiteSpace(numberString))
			{
				// i.e "~"
				number = 1;
			}
			else
			{
				// i.e "~2"
				int.TryParse(numberString, out number);
			}

			// We have a relative migration.
			var migrations = GetMigrations(context);
			if (number >= migrations.Count)
			{
				// Go back all the way.
				return "0";
			}

			var resolvedTargetMigration = migrations.ElementAt(number);
			return resolvedTargetMigration;
		}

		private IReadOnlyCollection<string> GetMigrations(string context)
		{
			var config = FindDbMigrationsConfiguration(context);
			var migrator = new DbMigrator(config);
			return migrator.GetDatabaseMigrations().ToList().AsReadOnly();
		}

		private List<DbMigrationsConfiguration> FindAllDbMigrationsConfiguration()
		{
			var configTypes = GetConstructablesOfType<DbMigrationsConfiguration>(_types).ToList();
			return configTypes.Select(t => Activator.CreateInstance(t) as DbMigrationsConfiguration)
				.ToList();
		}

		private DbMigrationsConfiguration FindDbMigrationsConfiguration(string context)
		{
			var configTypes = GetConstructablesOfType<DbMigrationsConfiguration>(_types).ToList();
			var configType = default(Type);

			if (string.IsNullOrEmpty(context))
			{
				if (configTypes.Count == 0)
				{
					throw new OperationException("No DbMigrationsConfiguration types found.");
				}
				else if (configTypes.Count == 1)
				{
					configType = configTypes.First();
				}
				else
				{
					throw new OperationException("Found multiple contexts, you should specify one using the -c option.");
				}
			}
			else
			{
				configType = configTypes
					.First(t => t.BaseType.GenericTypeArguments[0].Name == context);
			}
			return Activator.CreateInstance(configType) as DbMigrationsConfiguration;
		}

		private string FindAppDbContextTypeName()
		{
			var dbContextType = GetConstructablesOfType<DbContext>(_types).FirstOrDefault();
			return dbContextType?.Name;
		}

		private IEnumerable<Type> GetConstructablesOfType<TType>(IEnumerable<Type> types)
			where TType : class
		{
			return types.Where(t => typeof(TType).IsAssignableFrom(t) && t.IsConstructable());
		}

		private string GetMigrationsDir(string outputDir)
			=> Path.Combine(_projectDir, outputDir ?? "Migrations");

		private string Combine(params string[] paths) => Path.Combine(paths);
	}
}

#endif
