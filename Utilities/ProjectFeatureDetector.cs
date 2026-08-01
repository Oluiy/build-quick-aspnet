using BuildQuickPkg.Scaffolding;

namespace BuildQuickPkg.Utilities;

/// <summary>
/// Reads an already-generated project's actual current state (EF Core provider, JWT presence,
/// API style, configured HTTP port) from disk, so <c>BuildQuickPkg add</c> commands can generate
/// content that matches what's really there instead of assuming.
/// </summary>
internal static class ProjectFeatureDetector
{
    /// <summary>The Entity Framework Core provider referenced by the project's context-owning .csproj, or <see cref="EfCoreProvider.None"/> if none.</summary>
    public static EfCoreProvider DetectEfProvider(ExistingProject project)
    {
        if (!File.Exists(project.ContextOwnerCsprojPath))
        {
            return EfCoreProvider.None;
        }

        var content = File.ReadAllText(project.ContextOwnerCsprojPath);
        if (content.Contains("Npgsql.EntityFrameworkCore.PostgreSQL", StringComparison.Ordinal))
        {
            return EfCoreProvider.PostgreSql;
        }

        if (content.Contains("Microsoft.EntityFrameworkCore.SqlServer", StringComparison.Ordinal))
        {
            return EfCoreProvider.SqlServer;
        }

        return EfCoreProvider.None;
    }

    /// <summary>True if JWT bearer authentication is already wired up in Program.cs.</summary>
    public static bool HasJwt(ExistingProject project)
    {
        var programCsPath = Path.Combine(project.ApiProjectDirectory, "Program.cs");
        return File.Exists(programCsPath) && File.ReadAllText(programCsPath).Contains("JwtBearerDefaults", StringComparison.Ordinal);
    }

    /// <summary><see cref="ApiStyle.Controller"/> if Program.cs already registers <c>AddControllers()</c>, otherwise <see cref="ApiStyle.Minimal"/>.</summary>
    public static ApiStyle DetectApiStyle(ExistingProject project)
    {
        var programCsPath = Path.Combine(project.ApiProjectDirectory, "Program.cs");
        return File.Exists(programCsPath) && File.ReadAllText(programCsPath).Contains("AddControllers()", StringComparison.Ordinal)
            ? ApiStyle.Controller
            : ApiStyle.Minimal;
    }

    /// <summary>The HTTP port configured in the API project's launchSettings.json, or null if it can't be read.</summary>
    public static int? ReadHttpPort(ExistingProject project)
    {
        var launchSettingsPath = Path.Combine(project.ApiProjectDirectory, "Properties", "launchSettings.json");
        if (!File.Exists(launchSettingsPath))
        {
            return null;
        }

        var root = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(launchSettingsPath));
        var httpUrl = root?["profiles"]?["http"]?["applicationUrl"]?.GetValue<string>();
        var portText = httpUrl?.Split(':').LastOrDefault();

        return int.TryParse(portText, out var port) ? port : null;
    }
}
