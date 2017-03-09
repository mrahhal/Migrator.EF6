public class Util
{
	public Util(ICakeContext context, BuildParameters build)
	{
		Context = context;
		Build = build;
	}

	public ICakeContext Context { get; set; }
	public BuildParameters Build { get; set; }

	public void PatchProjectFileVersions()
	{
		var v = Build.Version.Version();
		var projects = Context.GetFiles("./src/*/project.json");
		foreach (var project in projects)
		{
			PatchProjectFileVersion(project, v);
		}
	}

	private void PatchProjectFileVersion(FilePath project, string version)
	{
		var content = System.IO.File.ReadAllText(project.FullPath, Encoding.UTF8);
		var node = Newtonsoft.Json.Linq.JObject.Parse(content);
		if(node["version"] != null)
		{
			node["version"].Replace(string.Concat(version, "-*"));
			System.IO.File.WriteAllText(project.FullPath, node.ToString(), Encoding.UTF8);
		};
	}

	public void PrintInfo()
	{
		Context.Information($@"
Version:       {Build.FullVersion()}
Configuration: {Build.Configuration}
");
	}

	public static string CreateStamp()
	{
		var seconds = (long)(DateTime.UtcNow - new DateTime(2017, 1, 1)).TotalSeconds;
		return seconds.ToString().PadLeft(11, (char)'0');
	}
}
