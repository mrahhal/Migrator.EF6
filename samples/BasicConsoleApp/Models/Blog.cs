using System.ComponentModel.DataAnnotations;

namespace BasicConsoleApp.Models
{
	public class Blog
	{
		public int Id { get; set; }

		[Required]
		public string Name { get; set; }
	}
}
