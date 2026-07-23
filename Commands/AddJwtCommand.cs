using BuildQuickPkg.Scaffolding;
using BuildQuickPkg.Templates;
using BuildQuickPkg.Utilities;
using Spectre.Console;

namespace BuildQuickPkg.Commands;

/// <summary>Retrofits JWT bearer authentication onto an already-generated project via <c>BuildQuickPkg add jwt</c>.</summary>
internal static class AddJwtCommand
{
    public static void Run(ExistingProject project)
    {
        var programCsPath = Path.Combine(project.ApiProjectDirectory, "Program.cs");

        if (ProgramCsEditor.Contains(programCsPath, "JwtBearerDefaults"))
        {
            AnsiConsole.MarkupLine("[yellow]JWT authentication is already set up[/] in Program.cs. Nothing to do.");
            return;
        }

        var frameworkPackageVersion = $"{project.TargetFramework.Replace("net", "")}.*";

        // Patch Program.cs first: it's the one step that can fail (missing markers), and it
        // writes atomically, so if it throws, nothing below has touched disk yet.
        var usings = ProgramTemplate.BuildExtraUsings(EfCoreProvider.None, dbContextNamespace: null, includeJwt: true);
        var registration = ProgramTemplate.BuildAuthRegistration(includeJwt: true);
        var middleware = ProgramTemplate.BuildAuthMiddleware(includeJwt: true);
        var endpoints = ProgramTemplate.BuildAuthSampleEndpoints(includeJwt: true);
        ProgramCsEditor.ApplyInsertions(programCsPath,
        [
            (ProgramCsEditor.UsingsMarker, usings + "\n"),
            (ProgramCsEditor.ServicesMarker, registration + "\n"),
            (ProgramCsEditor.MiddlewareMarker, middleware),
            (ProgramCsEditor.EndpointsMarker, endpoints + "\n"),
        ]);

        CsprojEditor.AddPackageReferences(project.ApiCsprojPath,
        [
            ("Microsoft.AspNetCore.Authentication.JwtBearer", frameworkPackageVersion),
        ]);

        AppSettingsEditor.AddJwtSection(
            Path.Combine(project.ApiProjectDirectory, "appsettings.json"),
            issuer: "https://localhost", audience: "https://localhost", key: "", expiryMinutes: 60);

        AppSettingsEditor.AddJwtSection(
            Path.Combine(project.ApiProjectDirectory, "appsettings.Development.json"),
            issuer: null, audience: null, key: "dev-only-signing-key-do-not-use-in-production-32chars-min", expiryMinutes: null);

        AppSettingsEditor.AddJwtSection(
            Path.Combine(project.ApiProjectDirectory, "appsettings.Production.json"),
            issuer: null, audience: null, key: "", expiryMinutes: null);

        AnsiConsole.MarkupLine($"\n[bold green]✨ Done![/] JWT authentication added to [bold yellow]{project.ProjectName}[/].");
        AnsiConsole.MarkupLine("Try it: [bold cyan]POST /api/auth/token?username=alice[/] then call [bold cyan]GET /api/secure[/] with the returned token.");
    }
}
