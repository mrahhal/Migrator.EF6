using MR.AspNet.Identity.EntityFramework6;

namespace WithIdentity.Models
{
	public class ApplicationUser : IdentityUser
	{
		public string Name { get; set; }
	}
}
