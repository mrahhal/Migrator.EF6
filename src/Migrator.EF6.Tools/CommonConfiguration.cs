#if NET46

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

		public CommandOption ConnectionString { get; set; }
		public CommandOption ProviderName { get; set; }
		public CommandOption Context { get; set; }

		public CommonConfiguration AddConnectionStringOption()
		{
			ConnectionString = _command.Option(
				"-cs|--connection-string <connectionString>",
				"The connection string to use");
			return this;
		}

		public CommonConfiguration AddProviderNameOption()
		{
			ProviderName = _command.Option(
				"-p|--provider <providerName>",
				"The provider name to use. If omitted, \"System.Data.SqlClient\" will be used");
			return this;
		}

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
				ConnectionString?.Value(),
				ProviderName?.Value(),
				Context?.Value());
		}
	}
}

#endif
