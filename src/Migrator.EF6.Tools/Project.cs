using NuGet.Frameworks;
using System.Collections.Generic;
using System.IO;

namespace Migrator.EF6.Tools
{
	public class TargetNuGetFramework
	{
		public TargetNuGetFramework(string tfm, NuGetFramework framework = null)
		{
			TFM = tfm;
			Framework = framework ?? new NuGetFramework(tfm);
		}

		public string TFM { get; set; }

		public NuGetFramework Framework { get; set; }
	}

	public class Project
	{
		public string ProjectFilePath { get; set; }

		public string ProjectDirectory => Path.GetDirectoryName(ProjectFilePath);

		public string ProjectFileName => Path.GetFileName(ProjectFilePath);

		public string AssemblyName { get; set; }

		public string RootNamespace { get; set; }

		public string Name { get; set; }

		public IEnumerable<TargetNuGetFramework> TargetFrameworks { get; set; }
	}
}
