# Migrator.EF6

[![Build status](https://img.shields.io/appveyor/ci/mrahhal/migrator-ef6/master.svg)](https://ci.appveyor.com/project/mrahhal/migrator-ef6)
[![NuGet version](https://badge.fury.io/nu/Migrator.EF6.svg)](https://www.nuget.org/packages/Migrator.EF6)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

DNX command line tool to enable EF6 migrations in an Asp.Net Core app.

## Getting EF6 migrations to work

Steps needed (nothing hard, just a lot of inital steps that you'll have to do one time):

- Inside `project.json`:
    - Remove `dnxcore50` from the target `frameworks`.
    - Remove everything `EF7` and add `Migrator.EF6` + `EF6` to your `dependencies`.
    - `"ef": "EntityFramework.Commands"` -> `"ef": "Migrator.EF6"` in the `commands` section.
- Inside `Startup.cs`:
    - Remove the line of code that starts with `services.AddEntityFramework` completely (this belong to EF7). Also remove `serviceScope.ServiceProvider.GetService<ApplicationDbContext>
    ().Database.Migrate()` if it exists.
    - Simply add your db context to services:
    ```c#
    services.AddScoped<ApplicationDbContext>();
    ```
- Replace all `Microsoft.AspNet.Identity.EntityFramework` usings with `MR.AspNet.Identity.EntityFramework6`.
- Remove the Migrations folder that EF7 generated.
- Finally:

    ```
    dnx ef migrations enable
    dnx ef migrations add InitialCreate
    dnx ef database update
    ```

You might have to edit the db context's name after enabling migrations if there are errors, so do that before going on.

As a final note, make sure your db context looks like this:
```c#
public class ApplicationDbContext : DbContext // Or IdentityDbContext<ApplicationUser> if you're using Identity
{
    public static string ConnectionString { get; set; } = "Server=(localdb)\\mssqllocaldb;Database=aspnet5-Ulfg-8443284d-add8-41f4-acd8-96cae03e401d;Trusted_Connection=True;MultipleActiveResultSets=true";

    public AppDbContext() : base(ConnectionString)
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
```

And in `Startup.cs`, in `Configure`:

```c#
Database.SetInitializer(new MigrateDatabaseToLatestVersion<AppDbContext, Ulfg.Migrations.Configuration>());
AppDbContext.ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];
```

This is really important for various reasons.

## If you're working with Identity 3.0 RC1

Check out [MR.AspNet.Identity.EntityFramework6](https://github.com/mrahhal/MR.AspNet.Identity.EntityFramework6).
