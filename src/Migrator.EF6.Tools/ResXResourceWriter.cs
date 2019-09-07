using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Migrator.EF6.Tools
{
	public class ResXResourceWriter : IDisposable
	{
		private readonly string _path;
		private readonly Dictionary<string, string> _resources = new Dictionary<string, string>();

		public ResXResourceWriter(string path)
		{
			_path = path;
		}

		public void AddResource(string key, string value)
		{
			_resources.Add(key, value);
		}

		public void Dispose()
		{
			var assembly = Assembly.GetExecutingAssembly();
			var fileContent = default(string);

			using (var resourceStream = assembly.GetManifestResourceStream("Migrator.EF6.Tools.ResourceTemplate.txt"))
			using (var reader = new StreamReader(resourceStream))
			{
				fileContent = reader.ReadToEnd();
			}

			var text = "";

			foreach (var r in _resources)
			{
				text += $@"<data name=""{r.Key}"" xml:space=""preserve"">
    <value>{r.Value}</value>
  </data>\r\n";
			}

			fileContent = fileContent.Replace("{{PLACEHOLDER}}", text);

			File.WriteAllText(_path, fileContent);
		}
	}
}
