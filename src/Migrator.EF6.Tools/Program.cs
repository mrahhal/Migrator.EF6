using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.DotNet.ProjectModel;
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
				catch (Exception ex)
				{
					if (ex is TargetInvocationException)
					{
						ex = ex.InnerException;
					}

					Console.WriteLine(ex.Message);
					return 1;
				}
			}
		}

		private static void Dispatch(string[] args)
		{
			var projectFile = ProjectReader.GetProject(string.Empty);
			var targetFrameworks = projectFile
				.GetTargetFrameworks()
				.Select(frameworkInformation => frameworkInformation.FrameworkName);
			var framework = default(NuGetFramework);
			if (!TryResolveFramework(targetFrameworks, out framework))
			{
				return;
			}

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
