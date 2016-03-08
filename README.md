# Migrator.EF6

[![Build status](https://img.shields.io/appveyor/ci/mrahhal/migrator-ef6/master.svg)](https://ci.appveyor.com/project/mrahhal/migrator-ef6)
[![Nuget version](https://img.shields.io/nuget/v/Migrator.EF6.svg)](https://www.nuget.org/packages/Migrator.EF6)
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
    services.AddScoped(p =>
        new ApplicationDbContext(Configuration["Data:DefaultConnection:ConnectionString"]));
    ```
- For your db context, make sure you write a default ctor that calls `base("your connection string")` (this ctor will only be called by the migrator so you can just hard wire your dev connection string for now).
- Replace all `Microsoft.AspNet.Identity.EntityFramework` usings with `MR.AspNet.Identity.EntityFramework6`.
- Finally:
    ```
    dnx ef migrations enable
    dnx ef migrations add InitialCreate
    dnx ef database update
    ```
You might have to edit the db context's name after enabling migrations if there are errors, so do that before going on.

As a final note, make sure your db context looks like this:
```c#
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext() : base("Server=(localdb)\\mssqllocaldb;Database=aspnet5-WebApplication1-84bb2ccf-6f5b-4d01-b5ea-cbf91fb3a9a2;Trusted_Connection=True;MultipleActiveResultSets=true")
    {
        Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, WebApplication1.Migrations.Configuration>());
    }

    public ApplicationDbContext(string nameOrConnectionString) : base(nameOrConnectionString)
    {
    }

    protected override void OnModelCreating(DbModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
```

## If you're working with Identity 3.0 RC1

Check out [MR.AspNet.Identity.EntityFramework6](https://github.com/mrahhal/MR.AspNet.Identity.EntityFramework6).
