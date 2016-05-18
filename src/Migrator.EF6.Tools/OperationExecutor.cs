#if NET451

using System;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Design;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.ProjectModel;
using Microsoft.Extensions.CommandLineUtils;

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
			Directory.CreateDirectory(MigrationsDir);
		}

		private string MigrationsDir => Path.Combine(_projectDir, "Migrations");

		private string Combine(params string[] paths) => Path.Combine(paths);

		public void EnableMigrations()
		{
			var ns = $"{_rootNamespace}.Migrations";
			var path = Combine(MigrationsDir, "Configuration.cs");
			File.WriteAllText(path, @"using System.Data.Entity.Migrations;
using " + _rootNamespace + @".Models;

namespace " + ns + @"
{
	public class Configuration : DbMigrationsConfiguration<ApplicationDbContext>
	{
	}
}
");
		}

		public void AddMigration(string name, bool ignoreChanges)
		{
			var config = FindDbMigrationsConfiguration();

			// Scaffold migration.
			var scaffolder = new MigrationScaffolder(config);
			var migration = scaffolder.Scaffold(name, ignoreChanges);

			// Write the user code file.
			File.WriteAllText(Combine(MigrationsDir, migration.MigrationId + ".cs"), migration.UserCode);

			// Write needed resource values directly inside the designer code file.
			// Apparently, aspnet5 and resource files don't play well (or more specifically,
			// the way ef6 migration generator is interacting with the resources system)
			var targetValue = migration.Resources["Target"];
			var designerCode = migration.DesignerCode
				.Replace("Resources.GetString(\"Target\")", $"\"{targetValue}\"")
				.Replace("private readonly ResourceManager Resources = new ResourceManager(typeof(InitialCreate));", "");

			// Write the designer code file.
			File.WriteAllText(Path.Combine(MigrationsDir, migration.MigrationId + ".Designer.cs"), designerCode);
		}

		public void UpdateDatabase(string targetMigration)
		{
			var config = FindDbMigrationsConfiguration();
			var migrator = new DbMigrator(config);
			migrator.Update(targetMigration);
		}

		private DbMigrationsConfiguration FindDbMigrationsConfiguration()
		{
			var configType = _types
				.Where(t => typeof(DbMigrationsConfiguration).IsAssignableFrom(t))
				.FirstOrDefault();
			var config = Activator.CreateInstance(configType) as DbMigrationsConfiguration;
			return config;
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

		//public void UpdateDatabase(string migration)
		//{
		//}

		//public void DropDatabase(string context, Func<object, object, bool> confirmCheck)
		//	=> _contextOperations.Value.DropDatabase(contextName, confirmCheck);
	}
}

#endif
