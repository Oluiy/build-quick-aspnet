using BuildQuickPkg.Scaffolding;

namespace BuildQuickPkg.Templates;

/// <summary>
/// Produces the <c>.env</c> file written by <c>BuildQuickPkg add env</c>, with dummy values that
/// reflect whatever the project actually has set up (EF Core provider, JWT) rather than a fixed
/// static template, so it's never wrong about what it's supposedly configuring.
/// </summary>
internal static class EnvTemplate
{
    public static string Generate(int httpPort, EfCoreProvider efProvider, string projectName, bool includeJwt)
    {
        var lines = new List<string>
        {
            "ASPNETCORE_ENVIRONMENT=Development",
            $"ASPNETCORE_URLS=http://+:{httpPort}"
        };

        if (efProvider != EfCoreProvider.None)
        {
            lines.Add("");
            lines.Add("# Entity Framework Core (dummy value, replace before deploying)");
            lines.Add($"ConnectionStrings__DefaultConnection={EfCoreTemplate.DevelopmentConnectionString(projectName, efProvider)}");
        }

        if (includeJwt)
        {
            lines.Add("");
            lines.Add("# JWT (dummy values, replace before deploying)");
            lines.Add("Jwt__Issuer=https://localhost");
            lines.Add("Jwt__Audience=https://localhost");
            lines.Add("Jwt__Key=dev-only-signing-key-do-not-use-in-production-32chars-min");
            lines.Add("Jwt__ExpiryMinutes=60");
        }

        return string.Join("\n", lines) + "\n";
    }
}
