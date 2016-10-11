# Migrator.EF6

[![Build status](https://img.shields.io/appveyor/ci/mrahhal/migrator-ef6/master.svg)](https://ci.appveyor.com/project/mrahhal/migrator-ef6)
[![NuGet version](https://img.shields.io/nuget/v/Migrator.EF6.Tools.svg)](https://www.nuget.org/packages/Migrator.EF6.Tools)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](https://opensource.org/licenses/MIT)

.NET Core CLI tool to enable EF6 migrations in an Asp.Net Core app (RC2 and onwards).

## Looking for the Dnx version?
Checkout the [dnx](https://github.com/mrahhal/Migrator.EF6/tree/dnx) tree version of this repository.

## Getting EF6 migrations to work

**Make sure to update both entries inside `dependencies` and `tools` to the latest version.**

**It's important to note** that the only required steps to get EF6 migrations to work are the ones concerning the addition of `Migrator.EF6.Tools` to your `project.json`. All other steps are there to help you get rid of EF Core from the default template.

You can read the release notes at the end of this file.

Steps needed (nothing hard, just a lot of inital steps that you'll have to do one time):

- Inside `project.json`:
    - Remove `netcoreapp1.0` from the target `frameworks` and add `net451`.
    - Remove everything `EF Core` and add `Migrator.EF6.Tools` + `EF6` to your `dependencies`.
    In your `dependencies` section:
    ```diff
    - "Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore": "1.0.0",
    - "Microsoft.AspNetCore.Identity.EntityFrameworkCore": "1.0.0",
    - "Microsoft.EntityFrameworkCore.SqlServer": "1.0.0",
    - "Microsoft.EntityFrameworkCoMre.Tools": {
    -   "version": "1.0.0-preview2-final",
    -   "type": "build"
    - },
    + "EntityFramework": "6.1.3",
    + "Migrator.EF6.Tools": {
    +   "version": "1.0.7",
    +   "type": "build"
    + },
    ```

    In your `tools` section:
    ```diff
    - "Microsoft.EntityFrameworkCore.Tools": {
    -    ...
    - }
    + "Migrator.EF6.Tools": {
    +   "version": "1.0.7",
    +   "imports": "portable-net45+win8+dnxcore50"
    + }
    ```
- Inside `Startup.cs`:
    - Remove everything EF Core related.
    - Simply add your db context to services:
    ```c#
    services.AddScoped<ApplicationDbContext>();
    ```
- Replace all `Microsoft.AspNetCore.Identity.EntityFramework` usings with `MR.AspNet.Identity.EntityFramework6` if you're using Identity 3.0 (check out the section below).
- Remove the "Migrations" or the "Data/Migrations" folder that EF Core generated.
- Finally:

    ```
    dotnet ef migrations enable
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```

The tool will automatically build your project but if something goes wrong make sure to build manually with `dotnet build`.

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
ApplicationDbContext.ConnectionString = Configuration["ConnectionStrings:DefaultConnection"];
```

This is really important for the following reasons (not really necessary to read):
EF6 migrations implementation can read the connection string from `web.config` and that's why in an Asp.Net < 5 app we were able to just specify the connection's name and EF6 would fetch that. In EF Core, migrations know about dependency injection and can instantiate a `DbContext` correctly, EF6 just activates the default ctor so we have to provide the connection string there.

## More commands
These commands do not exist in the normal migrator:

#### `database update ~[number of migrations to revert]`:
Reverts a number of migrations. `database update ~` will revert one migration, and `database update ~2` will revert 2 migrations.

#### `database truncate`:
Truncates all tables in the database. This is basically 'database update 0'.

#### `database recreate`:
Truncates all tables then updates the database to the latest migration. This is basically a drop then update. Really helpful in development if you find yourself always dropping the database from SQL Server Object Explorer and then reapplying migrations.

## If you're working with Identity 3.0

Check out [MR.AspNet.Identity.EntityFramework6](https://github.com/mrahhal/MR.AspNet.Identity.EntityFramework6). It enables you to use Identity 3.0 with EF6 (by using an EF6 provider for Identity instead of the EF Core one).

## Samples
Samples are in the `samples/` directory. Watch out for `MNOTE:` occurrences for notes.

#### [`BasicConsoleApp`](samples/BasicConsoleApp)
A basic sample that shows how to add `Migrator.EF6.Tools` to your `project.json`.

#### [`WithIdentity`](samples/WithIdentity)
A sample using `Migrator.EF6` and [`MR.AspNet.Identity.EntityFramework6`](https://github.com/mrahhal/MR.AspNet.Identity.EntityFramework6) to enable EF6 + migrations + Identity 3.0 in your Asp.Net Core app.

## Note about versioning
I'm aware that I should have aligned the version of `Migrator.EF6.Tools` to the dotnet tooling instead of aligning it to the runtime release cycle (I kinda did that without thinking about the whole thing). But, what's done is done and I doubt that the tooling will change that much from now on so all is good for now.

## Release notes

The `1.0.*` releases align with .NET Core tooling `1.0.0-preview2`.

#### `1.0.7`
- Fixed: Embed the "Source" resource when it's available. [#30](https://github.com/mrahhal/Migrator.EF6/issues/30)

#### `1.0.6`
- Support overriding the connection string from the command line through the `-cs` option. This way you won't have to hard code the string inside the `DbContext`. [#28](https://github.com/mrahhal/Migrator.EF6/issues/28)

#### `1.0.5`
- Support multiple `DbContext`s in the same project by using the `-c` option to specify which one to target. [#27](https://github.com/mrahhal/Migrator.EF6/issues/27)

#### `1.0.4`
- Only look for constructable `DbContext`s and `DbMigrationsConfiguration`s. [#25](https://github.com/mrahhal/Migrator.EF6/issues/25)
- Use `MigrationsDirectory` in `DbMigrationConfiguration` if it's available and no directory is specified. [#26](https://github.com/mrahhal/Migrator.EF6/issues/26)

#### `1.0.3`
- Allow relative database updates to migrations using "~". So `database update ~` will revert one migration, and `database update ~2` will revert 2 migrations.

#### `1.0.2`
- `database update` now has a `--force` option to ignore possible data loss while updating the database.

#### `1.0.1`
- Added an output directory option for migrations. [#24](https://github.com/mrahhal/Migrator.EF6/issues/24)
- `migrations enable` now automatically finds the app's DbContex type name to use in the "Configuration.cs" generated file.

#### `1.0.0`
- Initial release supporting .NET Core `1.0.0`.

The `1.0.0-rc2*` releases align with .NET Core `RC2`.

#### `1.0.0-rc2-3`
- Added support for the following commands:
  - `migrations script`: Generate a SQL script from migrations.

#### `1.0.0-rc2-2`
- Fixed: calling the tool required building the project before invocations. Now the tool automatically builds the target project so that it's always up to date. [#11](https://github.com/mrahhal/Migrator.EF6/issues/11)

#### `1.0.0-rc2`
- Initial release supporting .NET Core RC2.
