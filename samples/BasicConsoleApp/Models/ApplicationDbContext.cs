using System.Data.Entity;

namespace BasicConsoleApp.Models
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext()
			: base("Server=(localdb)\\mssqllocaldb;Database=Migrator.EF6-BasicConsoleApp-8443284d-add8-41f4-acd8-96cae03e401d;Trusted_Connection=True;MultipleActiveResultSets=true")
		{
		}

		public DbSet<Blog> Blogs { get; set; }

		public DbSet<Post> Posts { get; set; }
	}
}
