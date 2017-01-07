public class BuildParameters
{
	public BuildParameters(ICakeContext context)
	{
		Context = context;
	}

	public ICakeContext Context { get; }
	public BuildVersion Version { get; private set; }
	public string Configuration { get; private set; }
	public bool IsTagged { get; private set; }
	public bool IsCI { get; private set; }
	public DirectoryPathCollection Projects { get; set; }
	public DirectoryPathCollection TestProjects { get; set; }
	public FilePathCollection ProjectFiles { get; set; }
	public FilePathCollection TestProjectFiles { get; set; }

	public static BuildParameters Create(ICakeContext context)
	{
		var buildParameters = new BuildParameters(context);
		buildParameters.Initialize();
		return buildParameters;
	}

	public string FullVersion()
	{
		return Version.VersionWithSuffix();
	}

	private void Initialize()
	{
		InitializeCore();
		InitializeVersion();
	}

	private void InitializeCore()
	{
		Projects = Context.GetDirectories("./src/*");
		TestProjects = Context.GetDirectories("./test/*");
		ProjectFiles = Context.GetFiles("./src/*/project.json");
		TestProjectFiles = Context.GetFiles("./test/*/project.json");

		var buildSystem = Context.BuildSystem();
		if (!buildSystem.IsLocalBuild)
		{
			IsCI = true;
			if ((buildSystem.IsRunningOnAppVeyor && buildSystem.AppVeyor.Environment.Repository.Tag.IsTag) ||
				(buildSystem.IsRunningOnTravisCI && string.IsNullOrWhiteSpace(buildSystem.TravisCI.Environment.Build.Tag)))
			{
				IsTagged = true;
			}
		}

		Configuration = Context.Argument("Configuration", "Debug");
		if (IsCI)
		{
			Configuration = "Release";
		}
	}

	private void InitializeVersion()
	{
		var versionFile = Context.File("./build/version.json");
		var content = System.IO.File.ReadAllText(versionFile.Path.FullPath);
		Version = Newtonsoft.Json.JsonConvert.DeserializeObject<BuildVersion>(content);
		var suffix = default(string);
		if (!IsTagged)
		{
			suffix = (IsCI ? "ci-" : "dv-") + Util.CreateStamp();
		}
		Version.Suffix = suffix;
	}
}

public class BuildVersion
{
	public BuildVersion(int major, int minor, int patch, string quality)
	{
		Major = major;
		Minor = minor;
		Patch = patch;
		Quality = quality;
	}

	public int Major { get; set; }
	public int Minor { get; set; }
	public int Patch { get; set; }
	public string Quality { get; set; }
	public string Suffix { get; set; }

	public string VersionWithoutQuality()
	{
		return $"{Major}.{Minor}.{Patch}";
	}

	public string Version()
	{
		return VersionWithoutQuality() + (Quality == null ? string.Empty : $"-{Quality}");
	}

	public string VersionWithSuffix()
	{
		return Version() + (Suffix == null ? string.Empty : $"-{Suffix}");
	}
}
