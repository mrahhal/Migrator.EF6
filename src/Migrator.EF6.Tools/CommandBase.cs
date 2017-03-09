#if NET46

using Microsoft.Extensions.CommandLineUtils;

namespace Migrator.EF6.Tools
{
	public abstract class CommandBase
	{
		public CommonConfiguration Common(CommandLineApplication command) => new CommonConfiguration(command);
	}
}

#endif
