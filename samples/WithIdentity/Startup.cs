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
			// Set up configuration sources.

			var builder = new ConfigurationBuilder()
				.SetBasePath(env.ContentRootPath)
				.AddJsonFile("appsettings.json")
				.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

			if (env.IsDevelopment())
			{
				// For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
				builder.AddUserSecrets();

				// This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
				//builder.AddApplicationInsightsSettings(developerMode: true);
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

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// Add framework services.
			//services.AddApplicationInsightsTelemetry(Configuration);

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

			// Add application services.
			services.AddTransient<IEmailSender, AuthMessageSender>();
			services.AddTransient<ISmsSender, AuthMessageSender>();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole(Configuration.GetSection("Logging"));
			loggerFactory.AddDebug();

			//app.UseApplicationInsightsRequestTelemetry();

			if (env.IsDevelopment())
			{
				app.UseBrowserLink();
				app.UseDeveloperExceptionPage();
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

			//app.UseIISPlatformHandler(options => options.AuthenticationDescriptions.Clear());

			//app.UseApplicationInsightsExceptionTelemetry();

			app.UseStaticFiles();

			app.UseIdentity();

			// To configure external authentication please see http://go.microsoft.com/fwlink/?LinkID=532715

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
