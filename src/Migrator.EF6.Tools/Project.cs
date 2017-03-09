using NuGet.Frameworks;
using System.Collections.Generic;
using System.IO;

namespace Migrator.EF6.Tools
{
	public class Project
	{
		public string ProjectFilePath { get; set; }

		public string ProjectDirectory => Path.GetDirectoryName(ProjectFilePath);

		public string ProjectFileName => Path.GetFileName(ProjectFilePath);

		public string Name { get; set; }

		public IEnumerable<NuGetFramework> TargetFrameworks { get; set; }
	}
}
