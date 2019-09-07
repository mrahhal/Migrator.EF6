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

		public CommandOption Verbose { get; set; }
		public CommandOption ConnectionString { get; set; }
		public CommandOption ProviderName { get; set; }
		public CommandOption Context { get; set; }
		public CommandOption Runtime { get; set; }

		public CommonConfiguration AddCommonOptions()
		{
			Verbose = _command.Option(
				"-v|--verbose",
				"Run in verbose mode",
				CommandOptionType.NoValue);
			return this;
		}

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

		public CommonConfiguration AddContextAndRuntimeOptions()
		{
			return
				AddContextOption()
				.AddRuntimeOption();
		}

		public CommonConfiguration AddContextOption()
		{
			Context = _command.Option(
				"-c|--context <context>",
				"The DbContext to use. If omitted, the default DbContext is used");
			return this;
		}

		public CommonConfiguration AddRuntimeOption()
		{
			Runtime = _command.Option(
				"-r|--runtime <runtime>",
				"The runtime to use.");
			return this;
		}

		public Executor CreateExecutor()
		{
			return new Executor(
				Verbose.HasValue(),
				ConnectionString?.Value(),
				ProviderName?.Value(),
				Context?.Value());
		}
	}
}
