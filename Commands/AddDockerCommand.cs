using BuildQuickPkg.Scaffolding;
using BuildQuickPkg.Templates;
using Spectre.Console;

namespace BuildQuickPkg.Commands;

/// <summary>Retrofits a Dockerfile and docker-compose.yml onto an already-generated project via <c>BuildQuickPkg add docker</c>.</summary>
internal static class AddDockerCommand
{
    public static void Run(ExistingProject project)
    {
        var dockerfilePath = Path.Combine(project.RootDirectory, "Dockerfile");
        var composePath = Path.Combine(project.RootDirectory, "docker-compose.yml");

        if (File.Exists(dockerfilePath) || File.Exists(composePath))
        {
            AnsiConsole.MarkupLine("[yellow]Docker files already exist[/] at the project root. Nothing to do.");
            return;
        }

        var apiProjectName = $"{project.ProjectName}_API";
        var httpPort = ReadHttpPort(project.ApiProjectDirectory) ?? 5200;
        var efProvider = DetectEfProvider(project);

        File.WriteAllText(dockerfilePath, DockerTemplate.Dockerfile(apiProjectName, project.TargetFramework));
        File.WriteAllText(composePath, DockerTemplate.DockerCompose(project.ProjectName, httpPort, efProvider));

        AnsiConsole.MarkupLine($"\n[bold green]✨ Done![/] Dockerfile and docker-compose.yml added to [bold yellow]{project.ProjectName}[/].");
        AnsiConsole.MarkupLine("Run it: [bold cyan]docker compose up --build[/]");
    }

    private static int? ReadHttpPort(string apiProjectDirectory)
    {
        var launchSettingsPath = Path.Combine(apiProjectDirectory, "Properties", "launchSettings.json");
        if (!File.Exists(launchSettingsPath))
        {
            return null;
        }

        var root = System.Text.Json.Nodes.JsonNode.Parse(File.ReadAllText(launchSettingsPath));
        var httpUrl = root?["profiles"]?["http"]?["applicationUrl"]?.GetValue<string>();
        var portText = httpUrl?.Split(':').LastOrDefault();

        return int.TryParse(portText, out var port) ? port : null;
    }

    private static EfCoreProvider DetectEfProvider(ExistingProject project)
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
}
