using create_aspnet_app.Scaffolding;
using Spectre.Console;

// Interactive CLI entry point: prompts for solution settings.

AnsiConsole.Write(new FigletText("ASP.NET Core").Color(Color.Cyan1));

var projectName = AnsiConsole.Ask<string>("What is your [bold green]Project Name[/]?", "MyAwesomeApi");
var targetFramework = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Which [bold yellow].NET Target Framework[/] would you like to use?")
        .AddChoices(["net8.0", "net9.0"]));

var archType = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select your preferred [bold cyan]Architecture Pattern[/]:")
        .AddChoices([
            "4-Layer (API, Application, Domain, Infrastructure)",
            "3-Layer (API, Application, Domain)"
        ]));


var isFourLayer = archType.StartsWith("4-Layer");
var includeTests = AnsiConsole.Confirm("Include [bold magenta]xUnit Integration Test project[/] for endpoints?", defaultValue: true);
var httpPort = AnsiConsole.Ask<int>("What is your [bold green]Port[/]?", 5200);
var httpsPort = AnsiConsole.Ask<int>("What is your [bold green]HTTPS Port[/]?", 5201);

// ScaffoldingConfig sent as an object to SolutionScaffolder.
var config = new ScaffoldingConfig
{
    ProjectName = projectName,
    TargetFramework = targetFramework,
    IsFourLayer = isFourLayer,
    IncludeTests = includeTests,
    OutputDirectory = Directory.GetCurrentDirectory(),
    HttpPort = httpPort,
    HttpsPort = httpsPort,
};


await AnsiConsole.Status()
    .StartAsync("Generating Clean Architecture Solution...", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Dots);
        ctx.SpinnerStyle(Style.Parse("green"));

        SolutionScaffolder.Generate(config);
        await Task.Delay(400);
    });

AnsiConsole.MarkupLine($"\n[bold green]✨ Success![/] Solution [bold yellow]{projectName}[/] generated!");
AnsiConsole.MarkupLine($"Run the API:\n  [bold cyan]cd {projectName}/src/{projectName}_API[/]\n  [bold cyan]dotnet run[/]\n");