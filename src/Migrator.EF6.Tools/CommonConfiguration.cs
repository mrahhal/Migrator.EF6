#if NET451

using Microsoft.Extensions.CommandLineUtils;
using Migrator.EF6.Tools.Extensions;

namespace Migrator.EF6.Tools
{
	public class CommonConfiguration
	{
		private CommandLineApplication _command;

		public CommonConfiguration(CommandLineApplication command)
		{
			_command = command;
		}

		public CommandOption Context { get; set; }

		public CommonConfiguration AddContextOption()
		{
			Context = _command.Option(
				"-c|--context <context>",
				"The DbContext to use. If omitted, the default DbContext is used");
			return this;
		}

		public Executor CreateExecutor()
		{
			return new Executor(
				Context?.Value());
		}
	}
}

#endif
