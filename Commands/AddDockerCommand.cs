using BuildQuickPkg.Scaffolding;
using BuildQuickPkg.Templates;
using BuildQuickPkg.Utilities;
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
        var httpPort = ProjectFeatureDetector.ReadHttpPort(project) ?? 5200;
        var efProvider = ProjectFeatureDetector.DetectEfProvider(project);

        File.WriteAllText(dockerfilePath, DockerTemplate.Dockerfile(apiProjectName, project.TargetFramework));
        File.WriteAllText(composePath, DockerTemplate.DockerCompose(project.ProjectName, httpPort, efProvider));

        AnsiConsole.MarkupLine($"\n[bold green]✨ Done![/] Dockerfile and docker-compose.yml added to [bold yellow]{project.ProjectName}[/].");
        AnsiConsole.MarkupLine("Run it: [bold cyan]docker compose up --build[/]");
    }
}
