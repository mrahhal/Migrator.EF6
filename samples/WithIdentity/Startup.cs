using System.Data.Entity;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MR.AspNet.Identity.EntityFramework6;
using WithIdentity.Models;
using WithIdentity.Services;

namespace WithIdentity
{
	public class Startup
	{
		public Startup(IHostingEnvironment env)
		{
			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			if (env.IsDevelopment())
			{
				//builder.AddUserSecrets();
			}

			builder.AddEnvironmentVariables();
			Configuration = builder.Build();

			// MNOTE: You might need to comment out the next line until you do a `dnx ef migrations enable`.
			// Don't forget to remove the comment after enabling migrations.
			Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Migrations.Configuration>());

			// MNOTE: Override the connection string.
			ApplicationDbContext.ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];
		}

		public IConfigurationRoot Configuration { get; set; }

		public void ConfigureServices(IServiceCollection services)
		{
			// MNOTE: Remove this.
			//services.AddEntityFramework()
			//	.AddSqlServer()
			//	.AddDbContext<ApplicationDbContext>(options =>
			//		options.UseSqlServer(Configuration["Data:DefaultConnection:ConnectionString"]));

			// MNOTE: Add this instead.
			services.AddScoped<ApplicationDbContext>();

			services.AddIdentity<ApplicationUser, IdentityRole>()
				.AddEntityFrameworkStores<ApplicationDbContext>()
				.AddDefaultTokenProviders();

			services.AddMvc();

			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();
		}

		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
				// MNOTE: Remove this.
				//app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");

				// MNOTE: Remove this, we are using the MigrateDatabaseToLatestVersion initializer
				// that we've set above.
				//try
				//{
				//	using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>()
				//		.CreateScope())
				//	{
				//		serviceScope.ServiceProvider.GetService<ApplicationDbContext>()
				//			 .Database.Migrate();
				//	}
				//}
				//catch { }
			}

			app.UseStaticFiles();
			app.UseIdentity();

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
