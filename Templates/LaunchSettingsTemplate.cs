namespace BuildQuickPkg.Templates;

/// <summary>
/// Produces the generated API project's <c>Properties/launchSettings.json</c>.
/// </summary>
internal static class LaunchSettingsTemplate
{
    /// <summary>Builds launchSettings.json with HTTP and HTTPS profiles on the given ports.</summary>
    public static string Generate(int httpPort = 5200, int httpsPort = 5201) => $$"""
    {
        "$schema": "https://json.schemastore.org/launchsettings.json",
        "profiles": {
        "https": {
            "commandName": "Project",
            "dotnetRunMessages": true,
            "launchBrowser": true,
            "launchUrl": "swagger",
            "applicationUrl": "https://localhost:{{httpsPort}};http://localhost:{{httpPort}}",
            "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development"
            }
        },
        "http": {
            "commandName": "Project",
            "dotnetRunMessages": true,
            "launchBrowser": true,
            "launchUrl": "swagger",
            "applicationUrl": "http://localhost:{{httpPort}}",
            "environmentVariables": {
            "ASPNETCORE_ENVIRONMENT": "Development"
            }
        }
        }
    }
    """;
}
