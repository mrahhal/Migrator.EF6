#if NET451

using System;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.ProjectModel;

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

		private string GetMigrationsDir(string outputDir)
			=> Path.Combine(_projectDir, outputDir ?? "Migrations");

		private string Combine(params string[] paths) => Path.Combine(paths);

		public void EnableMigrations(string outputDir)
		{
			var migrationsDir = GetMigrationsDir(outputDir);
			Directory.CreateDirectory(migrationsDir);
			var path = Combine(migrationsDir, "Configuration.cs");

			var assembly = Assembly.GetExecutingAssembly();
			var fileContent = default(string);

			using (var resourceStream = assembly.GetManifestResourceStream("Migrator.EF6.Tools.ConfigurationTemplate.txt"))
			using (var reader = new StreamReader(resourceStream))
			{
				fileContent = reader.ReadToEnd();
			}

			// Write Configuration.cs file.
			fileContent = fileContent.Replace("_RootNamespace_", _rootNamespace);
			File.WriteAllText(path, fileContent);
		}

		public void AddMigration(string name, string outputDir, bool ignoreChanges)
		{
			var migrationsDir = GetMigrationsDir(outputDir);
			Directory.CreateDirectory(migrationsDir);
			var config = FindDbMigrationsConfiguration();

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
				.Replace("Resources.GetString(\"Target\")", $"\"{targetValue}\"")
				.Replace("private readonly ResourceManager Resources = new ResourceManager(typeof(InitialCreate));", "");

			// Write the designer code file.
			File.WriteAllText(Path.Combine(migrationsDir, migration.MigrationId + ".Designer.cs"), designerCode);
		}

		public void ScriptMigration(string from, string to, string output)
		{
			var config = FindDbMigrationsConfiguration();
			var migrator = new DbMigrator(config);
			var scriptingDecorator = new MigratorScriptingDecorator(migrator);
			var script = scriptingDecorator.ScriptUpdate(from ?? "0", to);
			File.WriteAllText(output, script);
			Console.WriteLine($"Scripted migration as SQL to file '{output}'.");
		}

		public void UpdateDatabase(string targetMigration)
		{
			var config = FindDbMigrationsConfiguration();
			var migrator = new DbMigrator(config);
			migrator.Update(targetMigration);
		}

		public void ListMigrations()
		{
			var config = FindDbMigrationsConfiguration();
			var migrator = new DbMigrator(config);
			foreach (var migration in migrator.GetDatabaseMigrations())
			{
				Console.WriteLine(migration);
			}
		}

		private DbMigrationsConfiguration FindDbMigrationsConfiguration()
		{
			var configType = _types
				.Where(t => typeof(DbMigrationsConfiguration).IsAssignableFrom(t))
				.FirstOrDefault();
			var config = Activator.CreateInstance(configType) as DbMigrationsConfiguration;
			return config;
		}
	}
}

#endif
