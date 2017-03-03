using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Diagnostics;
using NuGet.Frameworks;

namespace Migrator.EF6.Tools
{
    public class ProjectReader
    {
		public static Project GetProject(string projectPath)
		{
			projectPath = NormalizeProjectFilePath(projectPath);
			string fileName = Path.GetFileName(Path.GetDirectoryName(projectPath));
			Project result;
			using (FileStream fileStream = new FileStream(projectPath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				result = new ProjectReader().ReadProject(fileStream, fileName, projectPath);
			}
			return result;
		}

		public static string NormalizeProjectFilePath(string path)
		{
			if (string.IsNullOrEmpty(path))
			{
				path = Directory.GetCurrentDirectory();
			}

			var attr = File.GetAttributes(path);
			if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
			{
				path = Directory.EnumerateFiles(
				path,
				"*.*proj")
				.Where(f => !f.EndsWith(".xproj")) // filter xproj files, which are MSBuild meta-projects
				.FirstOrDefault();
			}

			return Path.GetFullPath(path);
		}

		public Project ReadProject(Stream stream, string projectName, string projectPath)
		{
			var project = new Project();
			project.ProjectFilePath = Path.GetFullPath(projectPath);

			var targetFileName = $"{project.ProjectFileName}.dotnet-names.targets";
			var projectExtPath = Path.Combine(Path.GetDirectoryName(projectPath), "obj");
			var targetFile = Path.Combine(projectExtPath, targetFileName);

			File.WriteAllText(targetFile,
@"<Project>
      <Target Name=""_GetDotNetNames"">
         <ItemGroup>
            <_DotNetNamesOutput Include=""AssemblyName: $(AssemblyName)"" />
            <_DotNetNamesOutput Include=""RootNamespace: $(RootNamespace)"" />
            <_DotNetNamesOutput Include=""TargetFramework: $(TargetFramework)"" />
            <_DotNetNamesOutput Include=""TargetFrameworks: $(TargetFrameworks)"" />
         </ItemGroup>
         <WriteLinesToFile File=""$(_DotNetNamesFile)"" Lines=""@(_DotNetNamesOutput)"" Overwrite=""true"" />
      </Target>
  </Project>");

			var tmpFile = Path.GetTempFileName();
			var psi = new ProcessStartInfo
			{
				FileName = "dotnet",
				Arguments = $"msbuild \"{project.ProjectFileName}\" /t:_GetDotNetNames /nologo \"/p:_DotNetNamesFile={tmpFile}\""
			};
			var process = Process.Start(psi);
			process.WaitForExit();
			if (process.ExitCode != 0)
			{
				Console.Error.WriteLine("Invoking MSBuild target failed");
				throw new Exception();
			}

			var lines = File.ReadAllLines(tmpFile);
			File.Delete(tmpFile); // cleanup

			var properties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			foreach (var line in lines)
			{
				var idx = line.IndexOf(':');
				if (idx <= 0) continue;
				var name = line.Substring(0, idx)?.Trim();
				var value = line.Substring(idx + 1)?.Trim();
				properties.Add(name, value);
			}

			// ProjectName
			project.Name = properties["AssemblyName"] ?? properties["RootNamespace"] ?? projectName;

			// TargetFrameworks
			var targetFrameworks = new List<NuGetFramework>();
			if (properties.TryGetValue("TargetFramework", out var framework)
				&& !string.IsNullOrEmpty(framework))
			{
				targetFrameworks.Add(GetTargetFramework(framework));
			}

			if (properties.TryGetValue("TargetFrameworks", out var tfms)
				&& !string.IsNullOrEmpty(tfms))
			{
				foreach (var tfm in tfms.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
				{
					targetFrameworks.Add(GetTargetFramework(tfm));
				}
			}
			project.TargetFrameworks = targetFrameworks;

			return project;
		}

		public static NuGetFramework GetTargetFramework(string framework)
		{
			switch (framework)
			{
				case "net45":
					return new NuGetFramework(".NETFramework", new Version("4.5"));
				case "net451":
					return new NuGetFramework(".NETFramework", new Version("4.5.1"));
				case "net452":
					return new NuGetFramework(".NETFramework", new Version("4.5.2"));
				case "net46":
					return new NuGetFramework(".NETFramework", new Version("4.6"));
				case "net461":
					return new NuGetFramework(".NETFramework", new Version("4.6.1"));
				case "net462":
					return new NuGetFramework(".NETFramework", new Version("4.6.2"));
				case "netstandard1.0":
					return new NuGetFramework(".NETStandard", new Version("1.0"));
				case "netstandard1.1":
					return new NuGetFramework(".NETStandard", new Version("1.1"));
				case "netstandard1.2":
					return new NuGetFramework(".NETStandard", new Version("1.2"));
				case "netstandard1.3":
					return new NuGetFramework(".NETStandard", new Version("1.3"));
				case "netstandard1.4":
					return new NuGetFramework(".NETStandard", new Version("1.4"));
				case "netstandard1.5":
					return new NuGetFramework(".NETStandard", new Version("1.5"));
				case "netstandard1.6":
					return new NuGetFramework(".NETStandard", new Version("1.6"));
				case "netcoreapp1.0":
					return new NuGetFramework(".NETStandard", new Version("1.6"));
				default:
					return new NuGetFramework(framework);
			}
		}
	}
}
