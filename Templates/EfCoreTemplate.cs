using BuildQuickPkg.Scaffolding;

namespace BuildQuickPkg.Templates;

/// <summary>
/// Produces the generated Entity Framework Core <c>DbContext</c> and the provider-specific
/// package name, connection string, and <c>UseX</c> call shared by the csproj, appsettings, and
/// Program.cs templates.
/// </summary>
internal static class EfCoreTemplate
{
    /// <summary>Builds a starter <c>DbContext</c> class with no <c>DbSet</c> entities registered yet.</summary>
    public static string DbContext(string dbContextName, string namespaceName) => $$"""
        using Microsoft.EntityFrameworkCore;

        namespace {{namespaceName}};

        public class {{dbContextName}} : DbContext
        {
            public {{dbContextName}}(DbContextOptions<{{dbContextName}}> options) : base(options)
            {
            }

            // Register your DbSet<T> entities here.
            // example: public DbSet<User> User { get; set; } `Standard naming convention is `PascalCase` for DbSet properties
            // and for naming entities `PascalCase` with singular names and not plural`
        }
        """;

    /// <summary>The NuGet package that provides <paramref name="provider"/> to Entity Framework Core.</summary>
    public static string PackageName(EfCoreProvider provider) => provider switch
    {
        EfCoreProvider.PostgreSql => "Npgsql.EntityFrameworkCore.PostgreSQL",
        EfCoreProvider.SqlServer => "Microsoft.EntityFrameworkCore.SqlServer",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "No package is required for EfCoreProvider.None.")
    };

    /// <summary>The <c>DbContextOptionsBuilder</c> extension method used to select <paramref name="provider"/>, e.g. <c>UseNpgsql</c>.</summary>
    public static string UseProviderMethod(EfCoreProvider provider) => provider switch
    {
        EfCoreProvider.PostgreSql => "UseNpgsql",
        EfCoreProvider.SqlServer => "UseSqlServer",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "No provider method exists for EfCoreProvider.None.")
    };

    /// <summary>A working local connection string for <paramref name="provider"/>, suitable for appsettings.Development.json.</summary>
    public static string DevelopmentConnectionString(string projectName, EfCoreProvider provider) => provider switch
    {
        EfCoreProvider.PostgreSql => $"Host=localhost;Port=5432;Database={projectName}_Dev;Username=postgres;Password=postgres",
        EfCoreProvider.SqlServer => $"Server=localhost,1433;Database={projectName}_Dev;User Id=sa;Password=Your_password123;TrustServerCertificate=True",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "No connection string exists for EfCoreProvider.None.")
    };

    /// <summary>A connection string for <paramref name="provider"/> that targets the <c>db</c> service in the generated docker-compose.yml, rather than <c>localhost</c>.</summary>
    public static string ComposeConnectionString(string projectName, EfCoreProvider provider) => provider switch
    {
        EfCoreProvider.PostgreSql => $"Host=db;Port=5432;Database={projectName}_Dev;Username=postgres;Password=postgres",
        EfCoreProvider.SqlServer => $"Server=db,1433;Database={projectName}_Dev;User Id=sa;Password=Your_password123;TrustServerCertificate=True",
        _ => throw new ArgumentOutOfRangeException(nameof(provider), provider, "No connection string exists for EfCoreProvider.None.")
    };
}
