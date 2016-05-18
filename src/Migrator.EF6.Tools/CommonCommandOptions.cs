using Microsoft.Extensions.CommandLineUtils;
using NuGet.Frameworks;

namespace Migrator.EF6.Tools
{
	public class CommonCommandOptions
	{
		public CommandOption Framework { get; set; }
		public CommandOption Configuration { get; set; }

		public CommonOptions Value()
			=> new CommonOptions
			{
				Framework = Framework.HasValue()
					? NuGetFramework.Parse(Framework.Value())
					: null,
				Configuration = Configuration.Value()
			};
	}
}
