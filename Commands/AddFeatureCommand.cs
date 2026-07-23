using BuildQuickPkg.Scaffolding;
using Spectre.Console;

namespace BuildQuickPkg.Commands;

/// <summary>
/// Entry point for <c>BuildQuickPkg add &lt;feature&gt; [option]</c>, which retrofits an optional
/// feature (Entity Framework Core, JWT authentication, Docker) onto an already-generated project.
/// </summary>
internal static class AddFeatureCommand
{
    public static void Run(string[] args)
    {
        ExistingProject project;
        try
        {
            project = ExistingProjectLocator.Locate(Directory.GetCurrentDirectory());
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
            return;
        }

        var feature = args.Length > 0 ? args[0] : ResolveFeatureInteractively(project.ProjectName);

        try
        {
            switch (feature.Trim().ToLowerInvariant())
            {
                case "efcore" or "ef" or "entityframework" or "entityframeworkcore":
                    AddEfCoreCommand.Run(project, args.Length > 1 ? args[1] : null);
                    break;

                case "jwt" or "jwtauth" or "auth":
                    AddJwtCommand.Run(project);
                    break;

                case "docker":
                    AddDockerCommand.Run(project);
                    break;

                default:
                    AnsiConsole.MarkupLine($"[red]Unknown feature '{feature}'.[/] Use one of: efcore, jwt, docker.");
                    break;
            }
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]{ex.Message}[/]");
        }
    }

    private static string ResolveFeatureInteractively(string projectName) =>
        AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title($"What would you like to [bold cyan]add[/] to [bold yellow]{projectName}[/]?")
                .AddChoices(["efcore", "jwt", "docker"]));
}
