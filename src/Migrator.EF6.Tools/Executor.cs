#if NET46

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Infrastructure;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;
using System.Text.RegularExpressions;
using Migrator.EF6.Tools.Extensions;

namespace Migrator.EF6.Tools
{
	public class Executor
	{
		private string _projectDir;
		private string _rootNamespace;
		private Assembly _startupAssembly;
		private Type[] _types;
		private string _connectionString;
		private string _providerName;
		private string _context;
		private Regex _resourcesTypeRegex = new Regex(@"typeof\((.+?)\)");
		private Regex _classNameRegex = new Regex("class (.+) :");

		public Executor(bool verbose, string connectionString, string providerName, string context)
		{
			Program.Verbose = verbose;

			var project = ProjectReader.GetProject(string.Empty);

			_connectionString = connectionString;
			_providerName = providerName ?? "System.Data.SqlClient";
			_context = context;
			_projectDir = project.ProjectDirectory;
			_startupAssembly = Assembly.Load(new AssemblyName(project.AssemblyName));
			_rootNamespace = project.RootNamespace;
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
			fileContent = fileContent.Replace("_MigrationsNamespace_", GetMigrationsNamespaceFromPath(outputDir));

			if (appDbContextTypeName != null)
			{
				fileContent = fileContent.Replace("ApplicationDbContext", appDbContextTypeName);
			}

			File.WriteAllText(path, fileContent);
		}

		public void AddMigration(string name, string outputDir, bool ignoreChanges)
		{
			var config = FindDbMigrationsConfiguration();
			var migrationsDir = GetMigrationsDir(outputDir ?? config.MigrationsDirectory);
			Directory.CreateDirectory(migrationsDir);

			// Scaffold migration.
			var scaffolder = new MigrationScaffolder(config);
			var migration = scaffolder.Scaffold(name, ignoreChanges);

			// Write the user code file.
			File.WriteAllText(Combine(migrationsDir, migration.MigrationId + ".cs"), migration.UserCode);

			var className = GetClassNameFromDesignerFile(migration.DesignerCode);
			var ns = $"{_rootNamespace}.{GetMigrationsNamespaceFromPath(outputDir)}";
			var designerCode = _resourcesTypeRegex.Replace(
				migration.DesignerCode,
				$"\"{ns}.{migration.MigrationId}\", typeof({className}).Assembly");

			// Write the designer code file.
			File.WriteAllText(Path.Combine(migrationsDir, migration.MigrationId + ".Designer.cs"), designerCode);

			// Write resources file.
			var resourcesPath = Combine(migrationsDir, migration.MigrationId + ".resx");
			WriteResources(resourcesPath, migration.Resources);
		}

		private string GetClassNameFromDesignerFile(string designerCode)
		{
			return _classNameRegex.Match(designerCode)
				.Groups[1]
				.Value;
		}

		private void WriteResources(string resourcesPath, IDictionary<string, object> resources)
		{
			using (var writer = new ResXResourceWriter(resourcesPath))
			{
				foreach (var r in resources)
				{
					writer.AddResource(r.Key, r.Value);
				}
			}
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

		public void UpdateDatabase(string targetMigration, bool force = false)
		{
			var resolvedMigrationName = ResolveMigrationName(targetMigration);
			var config = FindDbMigrationsConfiguration();
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

		public void ListMigrations()
		{
			foreach (var migration in GetMigrations())
			{
				Console.WriteLine(migration);
			}
		}

		private string ResolveMigrationName(string targetMigration)
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
			var migrations = GetMigrations();
			if (number >= migrations.Count)
			{
				// Go back all the way.
				return "0";
			}

			var resolvedTargetMigration = migrations.ElementAt(number);
			return resolvedTargetMigration;
		}

		private IReadOnlyCollection<string> GetMigrations()
		{
			var config = FindDbMigrationsConfiguration();
			var migrator = new DbMigrator(config);
			return migrator.GetDatabaseMigrations().ToList().AsReadOnly();
		}

		private List<DbMigrationsConfiguration> FindAllDbMigrationsConfiguration()
		{
			var configTypes = GetConstructablesOfType<DbMigrationsConfiguration>(_types).ToList();
			return configTypes.Select(t => Activator.CreateInstance(t) as DbMigrationsConfiguration)
				.ToList();
		}

		private DbMigrationsConfiguration FindDbMigrationsConfiguration()
		{
			var configTypes = GetConstructablesOfType<DbMigrationsConfiguration>(_types).ToList();
			var configType = default(Type);

			if (string.IsNullOrEmpty(_context))
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
					.FirstOrDefault(t => t.BaseType.GenericTypeArguments[0].Name == _context);

				if (configType == null)
				{
					var available = configTypes
						.Select(t => t.BaseType.GenericTypeArguments[0].Name)
						.Aggregate((t1, t2) => t1 + ", " + t2);
					throw new OperationException(
						$"Could not find DbContext of name '{_context}'. Available contexts: {available}.");
				}
			}
			var dbMigrationsConfiguration = Activator.CreateInstance(configType) as DbMigrationsConfiguration;
			if (_connectionString != null)
			{
				Console.WriteLine("Using provided connection string as an override.");
				dbMigrationsConfiguration.TargetDatabase = new DbConnectionInfo(_connectionString, _providerName);
			}
			return dbMigrationsConfiguration;
		}

		private string FindAppDbContextTypeName()
		{
			if (_context != null)
			{
				return _context;
			}

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

		private string GetMigrationsNamespaceFromPath(string outputDir)
		{
			if (outputDir == null)
			{
				return "Migrations";
			}

			return outputDir.Replace('/', '.');
		}

		private string Combine(params string[] paths) => Path.Combine(paths);

		private void LogVerbose(string message) => Program.LogVerbose(message);
	}
}

#endif
