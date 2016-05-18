using NuGet.Frameworks;

namespace Migrator.EF6.Tools
{
	public class CommonOptions
	{
		public NuGetFramework Framework { get; set; }
		public string Configuration { get; set; }
	}
}
