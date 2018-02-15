using System.Data.Entity;

namespace BasicConsoleApp.Models
{
	public class AnotherDbContext : DbContext
	{
		public AnotherDbContext()
			: base("Server=(localdb)\\mssqllocaldb;Database=Migrator.EF6-BasicConsoleApp-a;Trusted_Connection=True;MultipleActiveResultSets=true")
		{
		}

		public DbSet<Foo> Foos { get; set; }
	}

	public class Foo
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public int Some { get; set; }
	}
}
