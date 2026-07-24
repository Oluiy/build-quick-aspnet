using Spectre.Console;

namespace BuildQuickPkg.Commands;

/// <summary>Prints Linux-CLI-style <c>--help</c> output for the tool and its <c>add</c> subcommand.</summary>
internal static class HelpText
{
    public static void PrintRoot()
    {
        AnsiConsole.MarkupLine("[bold cyan]BuildQuickPkg[/] - scaffolds a Clean Architecture ASP.NET Core solution.");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Usage[/]");
        AnsiConsole.MarkupLine("  BuildQuickPkg [[ProjectName]]           Generate a new solution interactively");
        AnsiConsole.MarkupLine("  BuildQuickPkg add <feature> [[option]]  Retrofit a feature onto an existing project");
        AnsiConsole.MarkupLine("  BuildQuickPkg --help, -h               Show this help");
        AnsiConsole.MarkupLine("  BuildQuickPkg --version, -v            Show the installed version");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Commands[/]");
        AnsiConsole.MarkupLine("  [bold]add[/] efcore <postgres|sqlserver>   Add Entity Framework Core to an existing project");
        AnsiConsole.MarkupLine("  [bold]add[/] jwt                          Add JWT bearer authentication to an existing project");
        AnsiConsole.MarkupLine("  [bold]add[/] docker                       Add a Dockerfile & docker-compose.yml to an existing project");
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("Run [bold]BuildQuickPkg add --help[/] for details on retrofitting a feature.");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Examples[/]");
        AnsiConsole.MarkupLine("  [grey]BuildQuickPkg[/]                          Full interactive generation");
        AnsiConsole.MarkupLine("  [grey]BuildQuickPkg MyAwesomeApi[/]             Generate, skipping the project-name prompt");
        AnsiConsole.MarkupLine("  [grey]BuildQuickPkg add efcore postgres[/]     Add EF Core to the project in the current directory");
        AnsiConsole.MarkupLine("  [grey]BuildQuickPkg add jwt[/]                 Add JWT authentication");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("Full docs: [underline]https://github.com/Oluiy/build-quick-aspnet/tree/main/docs[/]");
    }

    public static void PrintAdd()
    {
        AnsiConsole.MarkupLine("[bold cyan]BuildQuickPkg add[/] - retrofit an optional feature onto a project you already generated.");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Usage[/]");
        AnsiConsole.MarkupLine("  BuildQuickPkg add [[feature]] [[option]]");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Features[/]");
        AnsiConsole.MarkupLine("  [bold]efcore[/] <postgres|sqlserver>   Adds the provider package, a starter DbContext, and wires up AddDbContext");
        AnsiConsole.MarkupLine("  [bold]jwt[/]                          Adds JWT bearer auth services, middleware, and sample token/secure endpoints");
        AnsiConsole.MarkupLine("  [bold]docker[/]                       Adds a Dockerfile and docker-compose.yml");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("Leave off [[feature]] or [[option]] and you'll be prompted for it interactively.");
        AnsiConsole.MarkupLine("Run this from the project root, the API project's folder, or a microservice's root -");
        AnsiConsole.MarkupLine("BuildQuickPkg locates the project for you.");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("[bold]Examples[/]");
        AnsiConsole.MarkupLine("  [grey]BuildQuickPkg add efcore postgres[/]");
        AnsiConsole.MarkupLine("  [grey]BuildQuickPkg add efcore sqlserver[/]");
        AnsiConsole.MarkupLine("  [grey]BuildQuickPkg add jwt[/]");
        AnsiConsole.MarkupLine("  [grey]BuildQuickPkg add docker[/]");
        AnsiConsole.MarkupLine("  [grey]BuildQuickPkg add[/]                       Prompts for which feature to add");
        AnsiConsole.WriteLine();

        AnsiConsole.MarkupLine("Full docs: [underline]https://github.com/Oluiy/build-quick-aspnet/blob/main/docs/guides/adding-features-later.md[/]");
    }
}
