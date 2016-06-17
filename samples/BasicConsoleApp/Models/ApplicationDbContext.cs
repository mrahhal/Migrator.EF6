using System.Data.Entity;

namespace BasicConsoleApp.Models
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext()
			: base("Server=(localdb)\\mssqllocaldb;Database=Migrator.EF6-BasicConsoleApp;Trusted_Connection=True;MultipleActiveResultSets=true")
		{
		}

		public DbSet<Blog> Blogs { get; set; }

		public DbSet<Post> Posts { get; set; }
	}
}
