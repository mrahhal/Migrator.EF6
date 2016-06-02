# Migrator.EF6

[![Build status](https://img.shields.io/appveyor/ci/mrahhal/migrator-ef6/dnx.svg)](https://ci.appveyor.com/project/mrahhal/migrator-ef6)
[![NuGet version](https://badge.fury.io/nu/Migrator.EF6.svg)](https://www.nuget.org/packages/Migrator.EF6)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

### Looking for Asp.Net Core RC2 support?

Checkout the [master](https://github.com/mrahhal/Migrator.EF6) tree version of this repository.

DNX command line tool to enable EF6 migrations in an Asp.Net Core app.

## Getting EF6 migrations to work

**Note:** make sure you have the following nuget feed configured if dependencies are not being resolved:
```
https://www.myget.org/F/aspnetmaster/api/v3/index.json
```

In visual studio you can go to `Tools → Options` and then `NuGet Package Manager → Package Sources` to configure feeds.

Steps needed (nothing hard, just a lot of inital steps that you'll have to do one time):

- Inside `project.json`:
    - Remove `dnxcore50` from the target `frameworks`.
    - Remove everything `EF7` and add `Migrator.EF6` + `EF6` to your `dependencies`.
    In your dependencies section:
    ```diff
    - "EntityFramework.Commands": "7.0.0-rc1-final",
    - "EntityFramework.MicrosoftSqlServer": "7.0.0-rc1-final",
    + "EntityFramework": "6.1.3",
    + "Migrator.EF6": "1.1.0",
    ```
    - `"ef": "EntityFramework.Commands"` → `"ef": "Migrator.EF6"` in the `commands` section.
- Inside `Startup.cs`:
    - Remove the line of code that starts with `services.AddEntityFramework` completely (this belong to EF7). Also remove `serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate()` if it exists.
    ```diff
    - services.AddEntityFramework()...
    - serviceScope.ServiceProvider.GetService<ApplicationDbContext>().Database.Migrate()
    ```
    - Simply add your db context to services:
    ```c#
    services.AddScoped<ApplicationDbContext>();
    ```
- Replace all `Microsoft.AspNet.Identity.EntityFramework` usings with `MR.AspNet.Identity.EntityFramework6` if you're using Identity 3.0 (check out the section below).
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
    public static string ConnectionString { get; set; } = "Server=(localdb)\\mssqllocaldb;Database=aspnet5-Web1-8443284d-add8-41f4-acd8-96cae03e401d;Trusted_Connection=True;MultipleActiveResultSets=true";

    public ApplicationDbContext() : base(ConnectionString)
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
Database.SetInitializer(new MigrateDatabaseToLatestVersion<ApplicationDbContext, Web1.Migrations.Configuration>());
ApplicationDbContext.ConnectionString = Configuration["Data:DefaultConnection:ConnectionString"];
```

This is really important for the following reasons (not really necessary to read):
EF6 migrations implementation can read the connection string from `web.config` and that's why in an Asp.Net < 5 app we were able to just specify the connection's name and EF6 would fetch that. In EF7, migrations know about dependency injection and can instantiate a `DbContext` correctly, EF6 just activates the default ctor so we have to provide the connection string there.

## More commands
These commands do not exist in the normal migrator:

#### `database truncate`:
Truncates all tables in the database. This is basically 'database update 0'.

#### `database recreate`:
Truncates all tables then updates the database to the latest migration. This is basically a drop then update. Really helpful in development if you find yourself always dropping the database from SQL Server Object Explorer and then reapplying migrations. 

## If you're working with Identity 3.0 RC1

Check out [MR.AspNet.Identity.EntityFramework6](https://github.com/mrahhal/MR.AspNet.Identity.EntityFramework6). It enables you to use Identity 3.0 with EF6 (by using an EF6 provider for Identity instead of the EF7 one).

## Samples
Samples are in the `samples/` directory. Watch out for `MNOTE:` occurrences for notes.

#### [`WithIdentity`](samples/WithIdentity)
A sample using `Migrator.EF6` and [`MR.AspNet.Identity.EntityFramework6`](https://github.com/mrahhal/MR.AspNet.Identity.EntityFramework6) to enable EF6 + migrations + Identity 3.0 in your Asp.Net Core app.
