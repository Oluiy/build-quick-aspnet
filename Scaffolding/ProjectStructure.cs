namespace create_aspnet_app.Scaffolding;

/// <summary>
/// Resolves the Clean Architecture project names and folder layout for a generated solution.
/// </summary>
internal sealed class ProjectStructure
{
    /// <summary>The root directory the solution is generated into.</summary>
    public string RootDirectory { get; }

    /// <summary>The directory containing every buildable project (<c>{Root}/src</c>).</summary>
    public string SrcDirectory { get; }

    /// <summary>The directory containing the test project, if generated (<c>{Root}/tests</c>).</summary>
    public string TestsDirectory { get; }

    /// <summary>The API (presentation) project name, e.g. <c>MyApp_API</c>.</summary>
    public string ApiProject { get; }

    /// <summary>The Application layer project name, e.g. <c>MyApp_Application</c>.</summary>
    public string ApplicationProject { get; }

    /// <summary>The Domain layer project name, e.g. <c>MyApp_Domain</c>.</summary>
    public string DomainProject { get; }

    /// <summary>The Infrastructure layer project name, e.g. <c>MyApp_Infrastructure</c>.</summary>
    public string InfrastructureProject { get; }

    /// <summary>The xUnit test project name, e.g. <c>MyApp_API.Tests</c>.</summary>
    public string TestProject { get; }

    public ProjectStructure(string projectName, string outputDirectory)
    {
        RootDirectory = Path.Combine(outputDirectory, projectName);
        SrcDirectory = Path.Combine(RootDirectory, "src");
        TestsDirectory = Path.Combine(RootDirectory, "tests");

        ApiProject = $"{projectName}_API";
        ApplicationProject = $"{projectName}_Application";
        DomainProject = $"{projectName}_Domain";
        InfrastructureProject = $"{projectName}_Infrastructure";
        TestProject = $"{ApiProject}.Tests";
    }

    /// <summary>
    /// Returns every folder that must exist before project files are written, following the
    /// Clean Architecture layout. When <paramref name="isFourLayer"/> is false, Infrastructure
    /// concerns are folded into the Domain project instead of a dedicated project.
    /// </summary>
    public List<string> GetFolders(bool isFourLayer)
    {
        var folders = new List<string>
        {
            // {Project}_API
            Path.Combine(SrcDirectory, ApiProject, "Controllers"),
            Path.Combine(SrcDirectory, ApiProject, "Extensions"),
            Path.Combine(SrcDirectory, ApiProject, "Middlewares"),
            Path.Combine(SrcDirectory, ApiProject, "Properties"),

            // {Project}_Application
            Path.Combine(SrcDirectory, ApplicationProject, "Services", "Implementation"),
            Path.Combine(SrcDirectory, ApplicationProject, "Services", "Interfaces"),
            Path.Combine(SrcDirectory, ApplicationProject, "Utilities"),

            // {Project}_Domain
            Path.Combine(SrcDirectory, DomainProject, "Dtos", "RequestDtos"),
            Path.Combine(SrcDirectory, DomainProject, "Dtos", "ResponseDtos"),
            Path.Combine(SrcDirectory, DomainProject, "Entity"),
            Path.Combine(SrcDirectory, DomainProject, "Enums")
        };

        if (isFourLayer)
        {
            // Dedicated Infrastructure layer
            folders.Add(Path.Combine(SrcDirectory, InfrastructureProject, "Context"));
            folders.Add(Path.Combine(SrcDirectory, InfrastructureProject, "Migrations"));
        }
        else
        {
            // Infrastructure folded into Domain
            folders.Add(Path.Combine(SrcDirectory, DomainProject, "Infrastructure", "Context"));
            folders.Add(Path.Combine(SrcDirectory, DomainProject, "Infrastructure", "Migrations"));
        }

        return folders;
    }
}
