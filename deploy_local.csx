using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Linq;

var cd = Environment.CurrentDirectory;
var projectName = "Migrator.EF6.Tools";
var userName = Environment.UserName;
var userProfileDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
var deploymentFolder = Environment.GetEnvironmentVariable("MIGRATOR_EF6_LOCAL_PACKAGES_FEED_PATH") ?? $@"C:\D\dev\src\Local Packages";
Directory.CreateDirectory(deploymentFolder);
var nugetDirectory = Path.Combine(userProfileDirectory, ".nuget/packages");
var nugetToolsDirectory = Path.Combine(nugetDirectory, ".tools");
var nugetProjectDirectory = Path.Combine(nugetDirectory, projectName);
var nugetToolsProjectDirectory = Path.Combine(nugetToolsDirectory, projectName);

var regexFolder = new Regex(@"(.\..\..)-(.+)");
var regexFile = new Regex($@"{projectName}\.(.\..\..)-(.+).nupkg");

if (!Pack())
{
	Console.WriteLine("A problem occurred while packing.");
	return;
}

RemoveDevPackageFilesIn(deploymentFolder);
RemoveDevPackagesIn(nugetProjectDirectory);
RemoveDevPackagesIn(nugetToolsProjectDirectory);

var package = GetPackagePath();
if (package == null)
{
	Console.WriteLine("Package file not found. A problem might have occurred with the build script.");
	return;
}

var packageFileName = Path.GetFileName(package);
var deployedPackagePath = Path.Combine(deploymentFolder, packageFileName);
File.Copy(package, deployedPackagePath);

Console.WriteLine($"{packageFileName} -> {deployedPackagePath}");

//------------------------------------------------------------------------------

void RemoveDevPackagesIn(string directory)
{
	var folders = Directory.EnumerateDirectories(directory);
	foreach (var folder in folders)
	{
		var name = Path.GetFileName(folder);
		if (regexFolder.IsMatch(name))
		{
			Directory.Delete(folder, true);
		}
	}
}

void RemoveDevPackageFilesIn(string directory)
{
	var files = Directory.EnumerateFiles(directory);
	foreach (var file in files)
	{
		var name = Path.GetFileName(file);
		if (regexFile.IsMatch(name))
		{
			File.Delete(file);
		}
	}
}

bool Pack()
{
	var process = Process.Start("powershell", "build.ps1");
	process.WaitForExit();
	var exitCode = process.ExitCode;
	return exitCode == 0;
}

string GetPackagePath()
{
	var packagesDirectory = Path.Combine(cd, "artifacts/packages");
	if (!Directory.Exists(packagesDirectory))
	{
		return null;
	}

	var files = Directory.EnumerateFiles(packagesDirectory);
	return files.OrderBy(f => f.Length).FirstOrDefault();
}
