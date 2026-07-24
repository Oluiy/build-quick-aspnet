# Entity Framework Core

Answering `PostgreSQL` or `SQL Server` to the "Add Entity Framework Core?" prompt gets you:

- The right NuGet packages (`Npgsql.EntityFrameworkCore.PostgreSQL` or `Microsoft.EntityFrameworkCore.SqlServer`, plus `Microsoft.EntityFrameworkCore.Design` for migrations tooling) added to whichever project owns `Infrastructure/Context` for your chosen [architecture](architecture-guide.md): the dedicated Infrastructure project in 4-layer, or `Domain/Infrastructure/Context` in 3-layer.
- A starter `DbContext` class, named `{ProjectName}DbContext`, in that `Context/` folder, with no `DbSet<T>` properties yet, since it doesn't know your entities.
- `builder.Services.AddDbContext<{ProjectName}DbContext>(...)` wired into `Program.cs`, reading the connection string from configuration.
- A working local connection string in `appsettings.Development.json`, and a blank one in `appsettings.Production.json` (see below).

Already generated a project without it? Run `BuildQuickPkg add efcore postgres` (or `sqlserver`) from the project root; see [Adding a Feature Later](adding-features-later.md).

## 1. Add your entities

Nothing is scaffolded here on purpose; this is the part that's actually your application. Add entity classes under `Domain/Entity/`, then register them as `DbSet<T>` properties on the generated context:

```csharp
// src/MyApp_Infrastructure/Context/MyAppDbContext.cs
public class MyAppDbContext : DbContext
{
    public MyAppDbContext(DbContextOptions<MyAppDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
}
```

## 2. Get a database running

**PostgreSQL** and **SQL Server** both need an actual server to connect to; EF Core doesn't run one for you. Your options:

- Say yes to the Docker prompt too, and run `docker compose up db` (or just `docker compose up`, which starts the API alongside it); see [Docker](docker.md).
- Install PostgreSQL or SQL Server locally and match the connection string in `appsettings.Development.json` (or edit it to match your local setup).
- Point at a hosted/managed instance and put its connection string in `appsettings.Development.json` (or override it; see step 4).

## 3. Install the `dotnet-ef` tool and add a migration

The `Microsoft.EntityFrameworkCore.Design` package is already referenced, so you only need the CLI tool itself, once per machine:

```bash
dotnet tool install --global dotnet-ef
```

Then, from the solution root, create your first migration. The `--project` flag points at whichever project holds the `DbContext` (Infrastructure in 4-layer, Domain in 3-layer); `--startup-project` always points at the API project:

```bash
# 4-layer
dotnet ef migrations add InitialCreate \
  --project src/MyApp_Infrastructure \
  --startup-project src/MyApp_API

# 3-layer
dotnet ef migrations add InitialCreate \
  --project src/MyApp_Domain \
  --startup-project src/MyApp_API
```

Apply it to your database:

```bash
dotnet ef database update \
  --project src/MyApp_Infrastructure \
  --startup-project src/MyApp_API
```

## 4. Connection strings by environment

| File | Value |
| --- | --- |
| `appsettings.Development.json` | A working default pointing at `localhost` (PostgreSQL: `Host=localhost;Port=5432;Database={ProjectName}_Dev;Username=postgres;Password=postgres`; SQL Server: `Server=localhost,1433;...;User Id=sa;Password=Your_password123;TrustServerCertificate=True`); change the credentials if yours differ |
| `appsettings.Production.json` | Left blank on purpose. Supply it via the `ConnectionStrings__DefaultConnection` environment variable, a secret manager, or your hosting platform's connection-string configuration; never commit a real production connection string |
| `docker-compose.yml` (if generated) | A separate value pointing at the `db` service by hostname instead of `localhost`, injected as an environment variable |

## Notes

- SQL Server doesn't ship a native macOS/Linux server binary; on non-Windows machines, run it via the generated `docker-compose.yml`, or use Azure SQL Edge / SQL Server on Docker directly.
- Passwords in the generated `appsettings.Development.json` and `docker-compose.yml` (`postgres`/`postgres`, `Your_password123`) are dev-only defaults, meant for a throwaway local container; change them before using anything beyond your own machine.
- The `DbSet<T>` naming convention: PascalCase, plural for the property (`Products`), PascalCase singular for the entity class (`Product`).
