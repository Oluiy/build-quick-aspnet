using BuildQuickPkg.Scaffolding;
using BuildQuickPkg.Templates;
using BuildQuickPkg.Utilities;
using Spectre.Console;

namespace BuildQuickPkg.Commands;

/// <summary>Retrofits Entity Framework Core onto an already-generated project via <c>BuildQuickPkg add efcore [postgres|sqlserver]</c>.</summary>
internal static class AddEfCoreCommand
{
    public static void Run(ExistingProject project, string? providerArg)
    {
        var dbContextName = $"{project.ProjectName}DbContext";
        var contextDirectory = Path.Combine(project.ContextOwnerDirectory, "Context");
        var dbContextPath = Path.Combine(contextDirectory, $"{dbContextName}.cs");

        if (File.Exists(dbContextPath))
        {
            AnsiConsole.MarkupLine($"[yellow]Entity Framework Core is already set up[/] ({dbContextName} already exists). Nothing to do.");
            return;
        }

        var provider = ResolveProvider(providerArg);
        var frameworkPackageVersion = $"{project.TargetFramework.Replace("net", "")}.*";

        // Patch Program.cs first: it's the one step that can fail (missing markers), and it
        // writes atomically, so if it throws, nothing below has touched disk yet.
        var programCsPath = Path.Combine(project.ApiProjectDirectory, "Program.cs");
        var usings = ProgramTemplate.BuildExtraUsings(provider, project.DbContextNamespace, includeJwt: false);
        var registration = ProgramTemplate.BuildDbContextRegistration(provider, dbContextName);
        ProgramCsEditor.ApplyInsertions(programCsPath,
        [
            (ProgramCsEditor.UsingsMarker, usings + "\n"),
            (ProgramCsEditor.ServicesMarker, registration + "\n"),
        ]);

        Directory.CreateDirectory(contextDirectory);
        File.WriteAllText(dbContextPath, EfCoreTemplate.DbContext(dbContextName, project.DbContextNamespace));

        CsprojEditor.AddPackageReferences(project.ContextOwnerCsprojPath,
        [
            (EfCoreTemplate.PackageName(provider), frameworkPackageVersion),
            ("Microsoft.EntityFrameworkCore.Design", frameworkPackageVersion),
        ]);

        AppSettingsEditor.AddConnectionString(
            Path.Combine(project.ApiProjectDirectory, "appsettings.Development.json"),
            EfCoreTemplate.DevelopmentConnectionString(project.ProjectName, provider));

        AppSettingsEditor.AddConnectionString(
            Path.Combine(project.ApiProjectDirectory, "appsettings.Production.json"),
            connectionString: "");

        var relativeContextProject = Path.GetRelativePath(project.RootDirectory, project.ContextOwnerDirectory);
        var relativeApiProject = Path.GetRelativePath(project.RootDirectory, project.ApiProjectDirectory);

        AnsiConsole.MarkupLine($"\n[bold green]✨ Done![/] Entity Framework Core ([bold]{provider}[/]) added to [bold yellow]{project.ProjectName}[/].");
        AnsiConsole.MarkupLine("Next: add your entities, register them as DbSet<T> on the DbContext, then:");
        AnsiConsole.MarkupLine("  [bold cyan]dotnet tool install --global dotnet-ef[/]  [grey](once per machine)[/]");
        AnsiConsole.MarkupLine($"  [bold cyan]dotnet ef migrations add InitialCreate --project {relativeContextProject} --startup-project {relativeApiProject}[/]");
    }

    private static EfCoreProvider ResolveProvider(string? providerArg)
    {
        if (providerArg is not null)
        {
            return providerArg.Trim().ToLowerInvariant() switch
            {
                "postgres" or "postgresql" or "pg" => EfCoreProvider.PostgreSql,
                "sqlserver" or "sql-server" or "mssql" => EfCoreProvider.SqlServer,
                _ => throw new InvalidOperationException($"Unknown EF Core provider '{providerArg}'. Use 'postgres' or 'sqlserver'.")
            };
        }

        var choice = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Which [bold cyan]Entity Framework Core[/] provider?")
                .AddChoices(["PostgreSQL", "SQL Server"]));

        return choice == "PostgreSQL" ? EfCoreProvider.PostgreSql : EfCoreProvider.SqlServer;
    }
}
