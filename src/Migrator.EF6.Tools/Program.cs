using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Internal;
using NuGet.Frameworks;

namespace Migrator.EF6.Tools
{
	public class Program
	{
		public static int Main(string[] args)
		{
			if (DotnetToolDispatcher.IsDispatcher(args))
			{
				Dispatch(args);
				return 0;
			}
			else
			{
				try
				{
					DotnetToolDispatcher.EnsureValidDispatchRecipient(ref args);
					return Worker.Execute(args);
				}
				catch (OperationException ex)
				{
					Console.ForegroundColor = ConsoleColor.Red;
					Console.WriteLine(ex.Message);
					return 1;
				}
			}
		}

		private static void Dispatch(string[] args)
		{
			var projectFile = ProjectReader.GetProject(string.Empty);
			var targetFrameworks = projectFile.TargetFrameworks;

			if (!TryResolveFramework(targetFrameworks, out NuGetFramework framework))
			{
				return;
			}

			// Let's build the project first.
			var buildCommand = BuildCommandFactory.Create(
				projectFile.ProjectFilePath,
				"Debug",
				framework,
				null,
				null);
			var buildExitCode = buildCommand
				.ForwardStdErr()
				.ForwardStdOut()
				.Execute()
				.ExitCode;
			if (buildExitCode != 0)
			{
				throw new Exception($"Building {projectFile.Name} failed...");
			}
			Console.WriteLine();

			var dispatchCommand = DotnetToolDispatcher.CreateDispatchCommand(
				args,
				framework,
				"Debug",
				outputPath: null,
				buildBasePath: null,
				projectDirectory: projectFile.ProjectDirectory);

			using (var errorWriter = new StringWriter())
			{
				var commandExitCode = dispatchCommand
					.ForwardStdErr(errorWriter)
					.ForwardStdOut()
					.Execute()
					.ExitCode;

				if (commandExitCode != 0)
				{
					Console.WriteLine(errorWriter.ToString());
				}

				return;
			}
		}

		private static bool TryResolveFramework(
			IEnumerable<NuGetFramework> availableFrameworks,
			out NuGetFramework resolvedFramework)
		{
			NuGetFramework framework;
			framework = availableFrameworks.First();

			resolvedFramework = framework;
			return true;
		}
	}
}
