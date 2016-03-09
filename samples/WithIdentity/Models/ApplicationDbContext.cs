using System.Data.Entity;
using MR.AspNet.Identity.EntityFramework6;

namespace WithIdentity.Models
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		// MNOTE: This will only ever be used by when calling the migrator tool (so only in production).
		public static string ConnectionString { get; set; } =
			"Server=(localdb)\\mssqllocaldb;Database=aspnet5-WithIdentity-8167e428-d8a7-4cae-931f-6899730e0ae8;Trusted_Connection=True;MultipleActiveResultSets=true";

		public ApplicationDbContext() : base(ConnectionString)
		{
		}

		protected override void OnModelCreating(DbModelBuilder builder)
		{
			base.OnModelCreating(builder);
		}
	}
}
