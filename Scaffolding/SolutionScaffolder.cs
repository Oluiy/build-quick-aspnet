using BuildQuickPkg.Templates;
using BuildQuickPkg.Utilities;

namespace BuildQuickPkg.Scaffolding;

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
    /// <exception cref="ArgumentException"><see cref="ScaffoldingConfig.ProjectName"/> or one of <see cref="ScaffoldingConfig.ServiceNames"/> isn't safe to use as a C# namespace segment and folder name (e.g. contains a space).</exception>
    public static void Generate(ScaffoldingConfig config)
    {
        ValidateName(config.ProjectName, nameof(config.ProjectName));
        foreach (var serviceName in config.ServiceNames)
        {
            ValidateName(serviceName, nameof(config.ServiceNames));
        }

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

    /// <summary>Generates one full Clean Architecture project set (folders, csproj files, API artifacts, optional tests, EF Core, Docker) for a single project or service.</summary>
    private static ProjectStructure GenerateProjectSet(string projectName, string outputDirectory, ScaffoldingConfig config, int httpPort, int httpsPort)
    {
        var structure = new ProjectStructure(projectName, outputDirectory);
        var frameworkPackageVersion = DerivePackageVersion(config.TargetFramework);

        Directory.CreateDirectory(structure.SrcDirectory);

        CreateFolders(structure, config.IsFourLayer);
        WriteProjectFiles(structure, config, frameworkPackageVersion);
        WriteApiArtifacts(structure, projectName, httpPort, httpsPort, config);

        if (config.ApiStyle == ApiStyle.Controller)
        {
            WriteControllerArtifacts(structure, projectName, config.IncludeJwt);
        }

        if (config.IncludeTests)
        {
            WriteTestProject(structure, projectName, config, frameworkPackageVersion);
        }

        if (config.EfProvider != EfCoreProvider.None)
        {
            WriteEfCoreArtifacts(structure, projectName, config);
        }

        if (config.IncludeDocker)
        {
            WriteDockerArtifacts(structure, projectName, config, httpPort);
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

        // In the 3-layer architecture, Infrastructure/Context is folded into Domain, so Domain
        // gets the EF Core packages instead of a dedicated Infrastructure project.
        var domainEfProvider = config.IsFourLayer ? EfCoreProvider.None : config.EfProvider;

        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.DomainProject, $"{structure.DomainProject}.csproj"),
            CsprojTemplates.Domain(tfm, domainEfProvider, frameworkPackageVersion));

        var applicationNeedsJwtPackages = config.ApiStyle == ApiStyle.Controller && config.IncludeJwt;
        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApplicationProject, $"{structure.ApplicationProject}.csproj"),
            CsprojTemplates.Application(structure.DomainProject, tfm, applicationNeedsJwtPackages, frameworkPackageVersion));

        if (config.IsFourLayer)
        {
            File.WriteAllText(
                Path.Combine(structure.SrcDirectory, structure.InfrastructureProject, $"{structure.InfrastructureProject}.csproj"),
                CsprojTemplates.Infrastructure(structure.DomainProject, structure.ApplicationProject, tfm, config.EfProvider, frameworkPackageVersion));

            File.WriteAllText(
                Path.Combine(structure.SrcDirectory, structure.ApiProject, $"{structure.ApiProject}.csproj"),
                CsprojTemplates.Api(structure.ApplicationProject, structure.InfrastructureProject, tfm, frameworkPackageVersion, config.IncludeJwt));
        }
        else
        {
            File.WriteAllText(
                Path.Combine(structure.SrcDirectory, structure.ApiProject, $"{structure.ApiProject}.csproj"),
                CsprojTemplates.ThreeLayerApi(structure.ApplicationProject, structure.DomainProject, tfm, frameworkPackageVersion, config.IncludeJwt));
        }
    }

    private static void WriteApiArtifacts(ProjectStructure structure, string projectName, int httpPort, int httpsPort, ScaffoldingConfig config)
    {
        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApiProject, "Properties", "launchSettings.json"),
            LaunchSettingsTemplate.Generate(httpPort, httpsPort));

        var dbContextNamespace = config.EfProvider == EfCoreProvider.None
            ? null
            : DbContextNamespace(structure, config.IsFourLayer);

        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApiProject, "Program.cs"),
            ProgramTemplate.Generate(projectName, config.EfProvider, dbContextNamespace, config.IncludeJwt, config.ApiStyle));

        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApiProject, "appsettings.json"),
            AppSettingsTemplate.Base(config.IncludeJwt));

        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApiProject, "appsettings.Development.json"),
            AppSettingsTemplate.Development(projectName, config.EfProvider, config.IncludeJwt));

        File.WriteAllText(
            Path.Combine(structure.SrcDirectory, structure.ApiProject, "appsettings.Production.json"),
            AppSettingsTemplate.Production(config.EfProvider, config.IncludeJwt));
    }

    /// <summary>
    /// Writes the Controller + service pair for Controller-style API projects: a
    /// <c>HealthController</c> always, plus an <c>AuthController</c> when JWT is also selected.
    /// Both the API project's <c>Controllers/</c> folder and the Application project's
    /// <c>Services/Interfaces</c> and <c>Services/Implementation</c> folders already exist
    /// (created by <see cref="ProjectStructure.GetFolders"/>).
    /// </summary>
    private static void WriteControllerArtifacts(ProjectStructure structure, string projectName, bool includeJwt)
    {
        var controllersDirectory = Path.Combine(structure.SrcDirectory, structure.ApiProject, "Controllers");
        var serviceInterfacesDirectory = Path.Combine(structure.SrcDirectory, structure.ApplicationProject, "Services", "Interfaces");
        var serviceImplementationDirectory = Path.Combine(structure.SrcDirectory, structure.ApplicationProject, "Services", "Implementation");

        File.WriteAllText(Path.Combine(controllersDirectory, "HealthController.cs"), ControllerTemplate.HealthController(projectName));
        File.WriteAllText(Path.Combine(serviceInterfacesDirectory, "IHealthService.cs"), ControllerTemplate.IHealthService(projectName));
        File.WriteAllText(Path.Combine(serviceImplementationDirectory, "HealthService.cs"), ControllerTemplate.HealthService(projectName));

        if (includeJwt)
        {
            File.WriteAllText(Path.Combine(controllersDirectory, "AuthController.cs"), ControllerTemplate.AuthController(projectName));
            File.WriteAllText(Path.Combine(serviceInterfacesDirectory, "IAuthService.cs"), ControllerTemplate.IAuthService(projectName));
            File.WriteAllText(Path.Combine(serviceImplementationDirectory, "AuthService.cs"), ControllerTemplate.AuthService(projectName));
        }
    }

    /// <summary>The namespace the generated <c>DbContext</c> lives in: <c>{Infrastructure}.Context</c> in the 4-layer architecture, or <c>{Domain}.Infrastructure.Context</c> in the 3-layer architecture.</summary>
    private static string DbContextNamespace(ProjectStructure structure, bool isFourLayer) => isFourLayer
        ? $"{structure.InfrastructureProject}.Context"
        : $"{structure.DomainProject}.Infrastructure.Context";

    private static void WriteEfCoreArtifacts(ProjectStructure structure, string projectName, ScaffoldingConfig config)
    {
        var contextDirectory = config.IsFourLayer
            ? Path.Combine(structure.SrcDirectory, structure.InfrastructureProject, "Context")
            : Path.Combine(structure.SrcDirectory, structure.DomainProject, "Infrastructure", "Context");

        var dbContextName = $"{projectName}DbContext";
        var dbContextNamespace = DbContextNamespace(structure, config.IsFourLayer);

        File.WriteAllText(
            Path.Combine(contextDirectory, $"{dbContextName}.cs"),
            EfCoreTemplate.DbContext(dbContextName, dbContextNamespace));
    }

    private static void WriteDockerArtifacts(ProjectStructure structure, string projectName, ScaffoldingConfig config, int httpPort)
    {
        File.WriteAllText(
            Path.Combine(structure.RootDirectory, "Dockerfile"),
            DockerTemplate.Dockerfile(structure.ApiProject, config.TargetFramework));

        File.WriteAllText(
            Path.Combine(structure.RootDirectory, "docker-compose.yml"),
            DockerTemplate.DockerCompose(projectName, httpPort, config.EfProvider));
    }

    private static void WriteRootArtifacts(string rootDirectory, string name)
    {
        File.WriteAllText(Path.Combine(rootDirectory, ".gitignore"), GitignoreTemplate.Generate());
        File.WriteAllText(Path.Combine(rootDirectory, "README.md"), $"# {name}\n\nGenerated with `BuildQuickPkg`.");
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
        ProcessRunner.RunDotnetCommand(["new", "sln", "-n", slnName], slnDirectory);

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

        ProcessRunner.RunDotnetCommand(["sln", "add", .. projectPaths], slnDirectory);
    }

    private static string RelativeCsprojPath(string slnDirectory, string srcDirectory, string projectName)
    {
        var csprojPath = Path.Combine(srcDirectory, projectName, $"{projectName}.csproj");
        return Path.GetRelativePath(slnDirectory, csprojPath);
    }

    /// <summary>Derives a floating NuGet version wildcard matching the TFM, e.g. <c>net8.0</c> -&gt; <c>8.0.*</c>.</summary>
    private static string DerivePackageVersion(string targetFramework) => $"{targetFramework.Replace("net", "")}.*";
    
    private static void ValidateName(string name, string paramName)
    {
        if (!NameValidation.IsValidIdentifierName(name))
        {
            throw new ArgumentException(
                $"'{name}' isn't a valid name: use only letters, digits, and underscores, starting with a letter.", paramName);
        }
    }
}
