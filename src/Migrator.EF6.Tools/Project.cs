using NuGet.Frameworks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Migrator.EF6.Tools
{
    public class Project
    {
        public string ProjectFilePath { get; set; }

		public string ProjectDirectory
		{
			get
			{
				return Path.GetDirectoryName(ProjectFilePath);
			}
		}
		public string ProjectFileName
		{
			get
			{
				return Path.GetFileName(ProjectFilePath);
			}
		}

		public string Name { get; set; }

		public IEnumerable<NuGetFramework> TargetFrameworks { get; set; }
	}
}
