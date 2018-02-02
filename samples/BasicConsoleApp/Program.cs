using System.Linq;
using BasicConsoleApp.Models;

namespace BasicConsoleApp
{
	public class Program
	{
		public static void Main(string[] args)
		{
			using (var context = new ApplicationDbContext())
			{
				var blogs = context.Blogs.ToList();
			}
		}
	}
}
