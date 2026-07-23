using BuildQuickPkg.Scaffolding;

namespace BuildQuickPkg.Templates;

/// <summary>
/// Produces the generated API project's <c>appsettings.json</c>, <c>appsettings.Development.json</c>
/// and <c>appsettings.Production.json</c>.
/// </summary>
internal static class AppSettingsTemplate
{
    /// <summary>
    /// Builds the base <c>appsettings.json</c>, loaded in every environment and layered under the
    /// environment-specific files. Holds settings that don't vary by environment.
    /// </summary>
    public static string Base(bool includeJwt)
    {
        var sections = new List<string>
        {
            """
              "Logging": {
                "LogLevel": {
                  "Default": "Information",
                  "Microsoft.AspNetCore": "Warning"
                }
              }
            """,
            """
              "AllowedHosts": "*"
            """
        };

        if (includeJwt)
        {
            sections.Add("""
                  "Jwt": {
                    "Issuer": "https://localhost", //Generate your own JWT Key
                    "Audience": "https://localhost",
                    "Key": "",
                    "ExpiryMinutes": 60
                  }
                """);
        }

        return BuildJson(sections);
    }

    /// <summary>
    /// Builds <c>appsettings.Development.json</c>. Loaded when <c>ASPNETCORE_ENVIRONMENT=Development</c>
    /// (the default for <c>dotnet run</c>) and safe to commit: it only ever points at a local
    /// database and a dev-only JWT signing key, never real secrets.
    /// </summary>
    public static string Development(string projectName, EfCoreProvider efProvider, bool includeJwt)
    {
        var sections = new List<string>
        {
            """
              "Logging": {
                "LogLevel": {
                  "Default": "Debug",
                  "Microsoft.AspNetCore": "Information"
                }
              }
            """
        };

        if (efProvider != EfCoreProvider.None)
        {
            var connectionString = EfCoreTemplate.DevelopmentConnectionString(projectName, efProvider);
            sections.Add($$"""
                  "ConnectionStrings": {
                    "DefaultConnection": "{{connectionString}}"
                  }
                """);
        }

        if (includeJwt)
        {
            sections.Add("""
                  "Jwt": {
                    "Key": "dev-only-signing-key-do-not-use-in-production-32chars-min"
                  }
                """);
        }

        return BuildJson(sections);
    }

    /// <summary>
    /// Builds <c>appsettings.Production.json</c>. Loaded when <c>ASPNETCORE_ENVIRONMENT=Production</c>.
    /// Secrets are left blank here and are expected to be supplied via environment variables
    /// (e.g. <c>ConnectionStrings__DefaultConnection</c>, <c>Jwt__Key</c>) or a secret manager,
    /// never committed to source control.
    /// </summary>
    public static string Production(EfCoreProvider efProvider, bool includeJwt)
    {
        var sections = new List<string>
        {
            """
              "Logging": {
                "LogLevel": {
                  "Default": "Warning",
                  "Microsoft.AspNetCore": "Warning"
                }
              }
            """
        };

        if (efProvider != EfCoreProvider.None)
        {
            sections.Add("""
                  "ConnectionStrings": {
                    "DefaultConnection": ""
                  }
                """);
        }

        if (includeJwt)
        {
            sections.Add("""
                  "Jwt": {
                    "Key": ""
                  }
                """);
        }

        return BuildJson(sections);
    }

    private static string BuildJson(List<string> sections) => $$"""
        {
        {{string.Join(",\n", sections)}}
        }
        """;
}
