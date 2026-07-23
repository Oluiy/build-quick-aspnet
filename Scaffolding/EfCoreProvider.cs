namespace BuildQuickPkg.Scaffolding;

/// <summary>
/// The Entity Framework Core database provider to scaffold, if any.
/// </summary>
public enum EfCoreProvider
{
    /// <summary>No Entity Framework Core packages or DbContext are generated.</summary>
    None,

    /// <summary>Generates a PostgreSQL-backed DbContext using Npgsql.EntityFrameworkCore.PostgreSQL.</summary>
    PostgreSql,

    /// <summary>Generates a SQL Server-backed DbContext using Microsoft.EntityFrameworkCore.SqlServer.</summary>
    SqlServer
}
