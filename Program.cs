using create_aspnet_app.Scaffolding;
using Spectre.Console;
using System.Diagnostics;

// Interactive CLI entry point: prompts for solution settings.
long startTime = Stopwatch.GetTimestamp();

AnsiConsole.Write(new FigletText("ASP.NET Core").Color(Color.Cyan1));

var projectName = args.Length > 0
    ? args[0]
    : AnsiConsole.Ask<string>("What is your [bold green]Project Name[/]?", "MyAwesomeApi");

var targetFramework = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Which [bold yellow].NET Target Framework[/] would you like to use?")
        .AddChoices(["net8.0", "net9.0", "net10.0"]));

var archType = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select your preferred [bold cyan]Architecture Pattern[/]:")
        .AddChoices([
            "4-Layer (API, Application, Domain, Infrastructure)",
            "3-Layer (API, Application, Domain)"
        ]));
var isFourLayer = archType.StartsWith("4-Layer");

var deploymentStyle = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Select your [bold cyan]Deployment Style[/]:")
        .AddChoices([
            "Monolithic (single solution)",
            "Microservice (multiple independent services)"
        ]));
var isMicroservice = deploymentStyle.StartsWith("Microservice");

var serviceNames = new List<string>();
if (isMicroservice)
{
    var serviceCount = AnsiConsole.Ask<int>("How many [bold green]services[/] would you like to create?", 2);
    for (var i = 1; i <= serviceCount; i++)
    {
        serviceNames.Add(AnsiConsole.Ask<string>($"What is the name of [bold green]service {i}[/]?", $"Service{i}"));
    }
}

var includeTests = AnsiConsole.Confirm("Include [bold magenta]xUnit Integration Test project[/] for endpoints?", defaultValue: true);
var httpPort = AnsiConsole.Ask<int>("What is your [bold green]Port[/]?", 5200);
var httpsPort = AnsiConsole.Ask<int>("What is your [bold green]HTTPS Port[/]?", 5201);

var efChoice = AnsiConsole.Prompt(
    new SelectionPrompt<string>()
        .Title("Add [bold cyan]Entity Framework Core[/]?")
        .AddChoices(["None", "PostgreSQL", "SQL Server"]));
var efProvider = efChoice switch
{
    "PostgreSQL" => EfCoreProvider.PostgreSql,
    "SQL Server" => EfCoreProvider.SqlServer,
    _ => EfCoreProvider.None
};

var includeDocker = AnsiConsole.Confirm("Add [bold magenta]Dockerfile & docker-compose.yml[/]?", defaultValue: false);
var includeJwt = AnsiConsole.Confirm("Add [bold magenta]JWT Authentication[/] boilerplate?", defaultValue: false);

// ScaffoldingConfig sent as an object to SolutionScaffolder.
var config = new ScaffoldingConfig
{
    ProjectName = projectName,
    TargetFramework = targetFramework,
    IsFourLayer = isFourLayer,
    IncludeTests = includeTests,
    IsMicroservice = isMicroservice,
    ServiceNames = serviceNames,
    OutputDirectory = Directory.GetCurrentDirectory(),
    HttpPort = httpPort,
    HttpsPort = httpsPort,
    EfProvider = efProvider,
    IncludeDocker = includeDocker,
    IncludeJwt = includeJwt,
};

await AnsiConsole.Status()
    .StartAsync("Generating Clean Architecture Solution...", async ctx =>
    {
        ctx.Spinner(Spinner.Known.Dots);
        ctx.SpinnerStyle(Style.Parse("green"));

        SolutionScaffolder.Generate(config);
        await Task.Delay(400);
    });

if (isMicroservice)
{
    AnsiConsole.MarkupLine($"\n[bold green]✨ Success![/] {serviceNames.Count} services generated under [bold yellow]{projectName}/services[/]!");
    foreach (var serviceName in serviceNames)
    {
        AnsiConsole.MarkupLine($"  [bold cyan]cd {projectName}/services/{serviceName}/src/{serviceName}_API[/] && [bold cyan]dotnet run[/]");
    }
}
else
{
    AnsiConsole.MarkupLine($"\n[bold green]✨ Success![/] Solution [bold yellow]{projectName}[/] generated!");
    AnsiConsole.MarkupLine($"Run the API:\n  [bold cyan]cd {projectName}/src/{projectName}_API[/]\n  [bold cyan]dotnet run[/]\n");
}

TimeSpan elapsed = Stopwatch.GetElapsedTime(startTime);
AnsiConsole.MarkupLine($"[bold cyan]Total time: {elapsed.TotalSeconds} seconds[/]");
