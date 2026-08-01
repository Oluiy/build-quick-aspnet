using BuildQuickPkg.Scaffolding;
using BuildQuickPkg.Templates;
using BuildQuickPkg.Utilities;
using Spectre.Console;

namespace BuildQuickPkg.Commands;

/// <summary>Retrofits a Caddyfile reverse proxy onto an already-generated project via <c>BuildQuickPkg add caddy</c>.</summary>
internal static class AddCaddyCommand
{
    public static void Run(ExistingProject project)
    {
        var caddyfilePath = Path.Combine(project.RootDirectory, "Caddyfile");

        if (File.Exists(caddyfilePath))
        {
            AnsiConsole.MarkupLine("[yellow]Caddyfile already exists[/] at the project root. Nothing to do.");
            return;
        }

        var httpPort = ProjectFeatureDetector.ReadHttpPort(project) ?? 5200;
        File.WriteAllText(caddyfilePath, CaddyTemplate.Generate(httpPort));

        AnsiConsole.MarkupLine($"\n[bold green]✨ Done![/] Caddyfile added to [bold yellow]{project.ProjectName}[/].");
        AnsiConsole.MarkupLine("Run it: [bold cyan]caddy run[/] (replace `localhost` in the Caddyfile with your real domain first).");
    }
}
