using create_aspnet_app.Templates;
using create_aspnet_app.Utilities;

namespace create_aspnet_app.Scaffolding;

/// <summary>
/// Generates a Clean Architecture ASP.NET Core solution (or one solution per microservice) on
/// disk and wires the resulting projects into <c>.sln</c> file(s).
/// </summary>
public static class SolutionScaffolder
{
    /// <summary>
    /// Generates either a single monolithic solution or one solution per configured microservice
    /// (plus an aggregate root solution), depending on <see cref="ScaffoldingConfig.IsMicroservice"/>.
    /// </summary>
    /// <param name="config">The options controlling project naming, architecture and output.</param>
    public static void Generate(ScaffoldingConfig config)
    {
        if (config.IsMicroservice)
        {
            GenerateMicroservices(config);
        }
        else
        {
            var structure = GenerateProjectSet(config.ProjectName, config.OutputDirectory, config, config.HttpPort, config.HttpsPort);
            WriteRootArtifacts(structure.RootDirectory, config.ProjectName);
            CreateSolutionFile(structure.RootDirectory, config.ProjectName, [structure], config);
        }
    }

    private static void GenerateMicroservices(ScaffoldingConfig config)
    {
        var rootDirectory = Path.Combine(config.OutputDirectory, config.ProjectName);
        var servicesDirectory = Path.Combine(rootDirectory, "services");
        Directory.CreateDirectory(servicesDirectory);

        var structures = new List<ProjectStructure>();

        for (var i = 0; i < config.ServiceNames.Count; i++)
        {
            var serviceName = config.ServiceNames[i];
            var httpPort = config.HttpPort + i * 10;
            var httpsPort = config.HttpsPort + i * 10;

            var structure = GenerateProjectSet(serviceName, servicesDirectory, config, httpPort, httpsPort);
            WriteRootArtifacts(structure.RootDirectory, serviceName);
            CreateSolutionFile(structure.RootDirectory, serviceName, [structure], config);

            structures.Add(structure);
        }

        // Aggregate root solution so the whole system can be opened/built in one go.
        WriteRootArtifacts(rootDirectory, config.ProjectName);
        CreateSolutionFile(rootDirectory, config.ProjectName, structures, config);
    }

    /// <summary>Generates one full Clean Architecture project set (folders, csproj files, API artifacts, optional tests) for a single project or service.</summary>
    private static ProjectStructure GenerateProjectSet(string projectName, string outputDirectory, ScaffoldingConfig config, int httpPort, int httpsPort)
    {
        var structure = new ProjectStructure(projectName, outputDirectory);
        var frameworkPackageVersion = DerivePackageVersion(config.TargetFramework);

        Directory.CreateDirectory(structure.SrcDirectory);

        CreateFolders(structure, config.IsFourLayer);
        WriteProjectFiles(structure, config, frameworkPackageVersion);
        WriteApiArtifacts(structure, projectName, httpPort, httpsPort);

        if (config.IncludeTests)
        {
            WriteTestProject(structure, projectName, config, frameworkPackageVersion);
        }

        return structure;
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

    private static void WriteApiArtifacts(ProjectStructure structure, string projectName, int httpPort, int httpsPort)
    {
        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApiProject, "Properties", "launchSettings.json"),
            LaunchSettingsTemplate.Generate(httpPort, httpsPort));

        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApiProject, "Program.cs"),
            ProgramTemplate.Generate(projectName));
    }

    private static void WriteRootArtifacts(string rootDirectory, string name)
    {
        File.WriteAllText(Path.Combine(rootDirectory, ".gitignore"), GitignoreTemplate.Generate());
        File.WriteAllText(Path.Combine(rootDirectory, "README.md"), $"# {name}\n\nGenerated with `create-aspnet-app`.");
    }

    private static void WriteTestProject(ProjectStructure structure, string projectName, ScaffoldingConfig config, string frameworkPackageVersion)
    {
        var testProjectDir = Path.Combine(structure.TestsDirectory, structure.TestProject);
        Directory.CreateDirectory(testProjectDir);

        File.WriteAllText(
            Path.Combine(testProjectDir, $"{structure.TestProject}.csproj"),
            CsprojTemplates.Test(structure.ApiProject, config.TargetFramework, frameworkPackageVersion));

        File.WriteAllText(
            Path.Combine(testProjectDir, "HealthEndpointTests.cs"),
            HealthEndpointTestTemplate.Generate(projectName));
    }

    private static void CreateSolutionFile(string slnDirectory, string slnName, IReadOnlyList<ProjectStructure> structures, ScaffoldingConfig config)
    {
        ProcessRunner.RunDotnetCommand($"new sln -n {slnName}", slnDirectory);

        var projectPaths = new List<string>();
        foreach (var structure in structures)
        {
            projectPaths.Add(RelativeCsprojPath(slnDirectory, structure.SrcDirectory, structure.ApiProject));
            projectPaths.Add(RelativeCsprojPath(slnDirectory, structure.SrcDirectory, structure.ApplicationProject));
            projectPaths.Add(RelativeCsprojPath(slnDirectory, structure.SrcDirectory, structure.DomainProject));

            if (config.IsFourLayer)
            {
                projectPaths.Add(RelativeCsprojPath(slnDirectory, structure.SrcDirectory, structure.InfrastructureProject));
            }

            if (config.IncludeTests)
            {
                var testCsproj = Path.Combine(structure.TestsDirectory, structure.TestProject, $"{structure.TestProject}.csproj");
                projectPaths.Add(Path.GetRelativePath(slnDirectory, testCsproj));
            }
        }

        ProcessRunner.RunDotnetCommand($"sln add {string.Join(" ", projectPaths)}", slnDirectory);
    }

    private static string RelativeCsprojPath(string slnDirectory, string srcDirectory, string projectName)
    {
        var csprojPath = Path.Combine(srcDirectory, projectName, $"{projectName}.csproj");
        return Path.GetRelativePath(slnDirectory, csprojPath);
    }

    /// <summary>Derives a floating NuGet version wildcard matching the TFM, e.g. <c>net8.0</c> -&gt; <c>8.0.*</c>.</summary>
    private static string DerivePackageVersion(string targetFramework) => $"{targetFramework.Replace("net", "")}.*";
}
