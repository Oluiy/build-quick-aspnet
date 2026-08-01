using BuildQuickPkg.Scaffolding;
using BuildQuickPkg.Templates;
using BuildQuickPkg.Utilities;
using Spectre.Console;

namespace BuildQuickPkg.Commands;

/// <summary>Retrofits a <c>.env</c> file onto an already-generated project via <c>BuildQuickPkg add env</c>.</summary>
internal static class AddEnvCommand
{
    public static void Run(ExistingProject project)
    {
        var envPath = Path.Combine(project.RootDirectory, ".env");

        if (File.Exists(envPath))
        {
            AnsiConsole.MarkupLine("[yellow].env already exists[/] at the project root. Nothing to do.");
            return;
        }

        var httpPort = ProjectFeatureDetector.ReadHttpPort(project) ?? 5200;
        var efProvider = ProjectFeatureDetector.DetectEfProvider(project);
        var includeJwt = ProjectFeatureDetector.HasJwt(project);

        File.WriteAllText(envPath, EnvTemplate.Generate(httpPort, efProvider, project.ProjectName, includeJwt));
        AddToGitignore(project.RootDirectory);

        AnsiConsole.MarkupLine($"\n[bold green]✨ Done![/] .env added to [bold yellow]{project.ProjectName}[/] with dummy values matching its current setup.");
        AnsiConsole.MarkupLine("Replace the dummy values before deploying; .env has been added to .gitignore.");
    }

    private static void AddToGitignore(string rootDirectory)
    {
        var gitignorePath = Path.Combine(rootDirectory, ".gitignore");
        var content = File.Exists(gitignorePath) ? File.ReadAllText(gitignorePath) : "";

        if (content.Split('\n').Any(line => line.Trim() == ".env"))
        {
            return;
        }

        var separator = content.Length > 0 && !content.EndsWith('\n') ? "\n" : "";
        File.WriteAllText(gitignorePath, content + separator + ".env\n");
    }
}
