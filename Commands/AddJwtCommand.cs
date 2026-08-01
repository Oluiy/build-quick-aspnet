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

        var apiStyle = ProjectFeatureDetector.DetectApiStyle(project);
        var frameworkPackageVersion = $"{project.TargetFramework.Replace("net", "")}.*";

        // Patch Program.cs first: it's the one step that can fail (missing markers), and it
        // writes atomically, so if it throws, nothing below has touched disk yet.
        var usings = ProgramTemplate.BuildExtraUsings(EfCoreProvider.None, dbContextNamespace: null, includeJwt: true, apiStyle);
        var swaggerSecurity = ProgramTemplate.BuildSwaggerJwtSecurity(includeJwt: true);
        var middleware = ProgramTemplate.BuildAuthMiddleware(includeJwt: true);

        // Controller style already has AddControllers()/IHealthService registered from generation
        // time, so only the IAuthService line needs adding there; Minimal style has nothing yet.
        var servicesInsertion = apiStyle == ApiStyle.Controller
            ? ProgramTemplate.BuildAuthRegistration(includeJwt: true) + ProgramTemplate.BuildAuthServiceRegistration(includeJwt: true)
            : ProgramTemplate.BuildAuthRegistration(includeJwt: true);

        var insertions = new List<(string Marker, string Code)>
        {
            (ProgramCsEditor.UsingsMarker, usings + "\n"),
            (ProgramCsEditor.SwaggerMarker, swaggerSecurity + "\n"),
            (ProgramCsEditor.ServicesMarker, servicesInsertion + "\n"),
            (ProgramCsEditor.MiddlewareMarker, middleware),
        };

        if (apiStyle == ApiStyle.Minimal)
        {
            insertions.Add((ProgramCsEditor.EndpointsMarker, ProgramTemplate.BuildAuthSampleEndpoints(includeJwt: true) + "\n"));
        }

        ProgramCsEditor.ApplyInsertions(programCsPath, insertions);

        if (apiStyle == ApiStyle.Controller)
        {
            var applicationProjectDirectory = Path.Combine(project.SrcDirectory, $"{project.ProjectName}_Application");
            var controllersDirectory = Path.Combine(project.ApiProjectDirectory, "Controllers");
            var serviceInterfacesDirectory = Path.Combine(applicationProjectDirectory, "Services", "Interfaces");
            var serviceImplementationDirectory = Path.Combine(applicationProjectDirectory, "Services", "Implementation");

            File.WriteAllText(Path.Combine(controllersDirectory, "AuthController.cs"), ControllerTemplate.AuthController(project.ProjectName));
            File.WriteAllText(Path.Combine(serviceInterfacesDirectory, "IAuthService.cs"), ControllerTemplate.IAuthService(project.ProjectName));
            File.WriteAllText(Path.Combine(serviceImplementationDirectory, "AuthService.cs"), ControllerTemplate.AuthService(project.ProjectName));

            // AuthService.cs needs these here too: the Application project uses the plain
            // Microsoft.NET.Sdk (not Sdk.Web), so it doesn't get IConfiguration for free, and it
            // has no other reason to already reference the JWT package.
            var applicationCsprojPath = Path.Combine(applicationProjectDirectory, $"{project.ProjectName}_Application.csproj");
            CsprojEditor.AddPackageReferences(applicationCsprojPath,
            [
                ("Microsoft.AspNetCore.Authentication.JwtBearer", frameworkPackageVersion),
                ("Microsoft.Extensions.Configuration.Abstractions", frameworkPackageVersion),
            ]);
        }

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
        AnsiConsole.MarkupLine("Swagger UI now shows an [bold cyan]Authorize[/] button and lock icons; paste the token there to call protected endpoints from the browser.");
    }
}
