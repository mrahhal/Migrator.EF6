using System;
using System.ComponentModel.DataAnnotations;

namespace BasicConsoleApp.Models
{
	public class Post
	{
		public int Id { get; set; }

		[Required]
		public string Title { get; set; }

		public DateTimeOffset Created { get; set; }

		public int BlogId { get; set; }
		public Blog Blog { get; set; }
	}
}
