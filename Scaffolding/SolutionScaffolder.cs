using create_aspnet_app.Templates;
using create_aspnet_app.Utilities;

namespace create_aspnet_app.Scaffolding;

/// <summary>
/// Generates a Clean Architecture ASP.NET Core solution on disk and wires the resulting
/// projects into a new <c>.sln</c> file.
/// </summary>
public static class SolutionScaffolder
{
    /// <summary>
    /// Generates the full solution: folders, project files, the API's Program.cs and
    /// launchSettings.json, an optional xUnit test project, root-level artifacts, and the
    /// solution file itself.
    /// </summary>
    /// <param name="config">The options controlling project naming, architecture and output.</param>
    public static void Generate(ScaffoldingConfig config)
    {
        var structure = new ProjectStructure(config.ProjectName, config.OutputDirectory);
        var frameworkPackageVersion = DerivePackageVersion(config.TargetFramework);

        Directory.CreateDirectory(structure.SrcDirectory);

        CreateFolders(structure, config.IsFourLayer);
        WriteProjectFiles(structure, config, frameworkPackageVersion);
        WriteApiArtifacts(structure, config);
        WriteRootArtifacts(structure, config.ProjectName);

        if (config.IncludeTests)
        {
            WriteTestProject(structure, config, frameworkPackageVersion);
        }

        CreateSolutionFile(structure, config);
    }

    private static void CreateFolders(ProjectStructure structure, bool isFourLayer)
    {
        foreach (var folder in structure.GetFolders(isFourLayer))
        {
            Directory.CreateDirectory(folder);
        }
    }

    private static void WriteProjectFiles(ProjectStructure structure, ScaffoldingConfig config, string frameworkPackageVersion)
    {
        var tfm = config.TargetFramework;

        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.DomainProject, $"{structure.DomainProject}.csproj"),
            CsprojTemplates.Domain(tfm));

        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApplicationProject, $"{structure.ApplicationProject}.csproj"),
            CsprojTemplates.Application(structure.DomainProject, tfm));

        if (config.IsFourLayer)
        {
            File.WriteAllText(
                Path.Combine(structure.SrcDirectory, structure.InfrastructureProject, $"{structure.InfrastructureProject}.csproj"),
                CsprojTemplates.Infrastructure(structure.DomainProject, structure.ApplicationProject, tfm));

            File.WriteAllText(
                Path.Combine(structure.SrcDirectory, structure.ApiProject, $"{structure.ApiProject}.csproj"),
                CsprojTemplates.Api(structure.ApplicationProject, structure.InfrastructureProject, tfm, frameworkPackageVersion));
        }
        else
        {
            File.WriteAllText(
                Path.Combine(structure.SrcDirectory, structure.ApiProject, $"{structure.ApiProject}.csproj"),
                CsprojTemplates.ThreeLayerApi(structure.ApplicationProject, structure.DomainProject, tfm, frameworkPackageVersion));
        }
    }

    private static void WriteApiArtifacts(ProjectStructure structure, ScaffoldingConfig config)
    {
        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApiProject, "Properties", "launchSettings.json"),
            LaunchSettingsTemplate.Generate(config.HttpPort, config.HttpsPort));

        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApiProject, "Program.cs"),
            ProgramTemplate.Generate(config.ProjectName));
    }

    private static void WriteRootArtifacts(ProjectStructure structure, string projectName)
    {
        File.WriteAllText(Path.Combine(structure.RootDirectory, ".gitignore"), GitignoreTemplate.Generate());
        File.WriteAllText(Path.Combine(structure.RootDirectory, "README.md"), $"# {projectName}\n\nGenerated with `create-aspnet-app`.");
    }

    private static void WriteTestProject(ProjectStructure structure, ScaffoldingConfig config, string frameworkPackageVersion)
    {
        var testProjectDir = Path.Combine(structure.TestsDirectory, structure.TestProject);
        Directory.CreateDirectory(testProjectDir);

        File.WriteAllText(
            Path.Combine(testProjectDir, $"{structure.TestProject}.csproj"),
            CsprojTemplates.Test(structure.ApiProject, config.TargetFramework, frameworkPackageVersion));

        File.WriteAllText(
            Path.Combine(testProjectDir, "HealthEndpointTests.cs"),
            HealthEndpointTestTemplate.Generate(structure.RootDirectory));
    }

    private static void CreateSolutionFile(ProjectStructure structure, ScaffoldingConfig config)
    {
        ProcessRunner.RunDotnetCommand($"new sln -n {config.ProjectName}", structure.RootDirectory);

        var projectPaths = new List<string>
        {
            $"src/{structure.ApiProject}/{structure.ApiProject}.csproj",
            $"src/{structure.ApplicationProject}/{structure.ApplicationProject}.csproj",
            $"src/{structure.DomainProject}/{structure.DomainProject}.csproj"
        };

        if (config.IsFourLayer)
        {
            projectPaths.Add($"src/{structure.InfrastructureProject}/{structure.InfrastructureProject}.csproj");
        }

        if (config.IncludeTests)
        {
            projectPaths.Add($"tests/{structure.TestProject}/{structure.TestProject}.csproj");
        }

        ProcessRunner.RunDotnetCommand($"sln add {string.Join(" ", projectPaths)}", structure.RootDirectory);
    }

    /// <summary>Derives a floating NuGet version wildcard matching the TFM, e.g. <c>net8.0</c> -&gt; <c>8.0.*</c>.</summary>
    private static string DerivePackageVersion(string targetFramework) => $"{targetFramework.Replace("net", "")}.*";
}
