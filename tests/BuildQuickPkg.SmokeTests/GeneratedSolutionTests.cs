using BuildQuickPkg.Scaffolding;
using Xunit;

namespace BuildQuickPkg.SmokeTests;

/// <summary>
/// Generates a real solution for a spread of option combinations, then checks that the
/// generated code builds and its own tests pass. This is the promise the tool makes to users,
/// so it is the thing worth testing.
/// </summary>
public class GeneratedSolutionTests
{
    // Every option value shows up in at least one case, and every target framework is covered.
    // Testing all combinations (~300) would take hours, so this picks a spread instead.
    private static readonly Dictionary<string, ScaffoldOptions> Cases = new()
    {
        ["net8 · 4-layer · minimal · defaults"] =
            new("net8.0", FourLayer: true, ApiStyle.Minimal, Microservice: false, Tests: true, EfCoreProvider.None, Docker: false, Jwt: false),
        ["net8 · 3-layer · controllers · postgres · docker · jwt"] =
            new("net8.0", FourLayer: false, ApiStyle.Controller, Microservice: false, Tests: true, EfCoreProvider.PostgreSql, Docker: true, Jwt: true),
        ["net9 · 4-layer · controllers · sqlserver · jwt"] =
            new("net9.0", FourLayer: true, ApiStyle.Controller, Microservice: false, Tests: true, EfCoreProvider.SqlServer, Docker: false, Jwt: true),
        ["net9 · 3-layer · minimal · postgres · docker · no tests"] =
            new("net9.0", FourLayer: false, ApiStyle.Minimal, Microservice: false, Tests: false, EfCoreProvider.PostgreSql, Docker: true, Jwt: false),
        ["net10 · 4-layer · minimal · sqlserver · docker · jwt"] =
            new("net10.0", FourLayer: true, ApiStyle.Minimal, Microservice: false, Tests: true, EfCoreProvider.SqlServer, Docker: true, Jwt: true),
        ["net10 · 3-layer · controllers · defaults"] =
            new("net10.0", FourLayer: false, ApiStyle.Controller, Microservice: false, Tests: true, EfCoreProvider.None, Docker: false, Jwt: false),
        ["net8 · microservices · 4-layer · controllers · postgres · docker · jwt"] =
            new("net8.0", FourLayer: true, ApiStyle.Controller, Microservice: true, Tests: true, EfCoreProvider.PostgreSql, Docker: true, Jwt: true),
        ["net10 · microservices · 3-layer · minimal · jwt"] =
            new("net10.0", FourLayer: false, ApiStyle.Minimal, Microservice: true, Tests: true, EfCoreProvider.None, Docker: false, Jwt: true),
    };

    public static IEnumerable<object[]> CaseNames => Cases.Keys.Select(name => new object[] { name });

    [Theory]
    [MemberData(nameof(CaseNames))]
    public void Generated_solution_builds_and_its_tests_pass(string caseName)
    {
        var options = Cases[caseName];
        using var workspace = new TempDirectory();
        const string projectName = "SmokeApp";
        string[] serviceNames = options.Microservice ? ["Orders", "Billing"] : [];

        SolutionScaffolder.Generate(new ScaffoldingConfig
        {
            ProjectName = projectName,
            TargetFramework = options.TargetFramework,
            OutputDirectory = workspace.Path,
            IsFourLayer = options.FourLayer,
            ApiStyle = options.ApiStyle,
            IsMicroservice = options.Microservice,
            ServiceNames = serviceNames,
            IncludeTests = options.Tests,
            EfProvider = options.EfProvider,
            IncludeDocker = options.Docker,
            IncludeJwt = options.Jwt,
            HttpPort = 5200,
            HttpsPort = 5201,
        });

        var solutionDirectory = Path.Combine(workspace.Path, projectName);

        if (options.Docker)
        {
            // Building the images needs Docker, so only check the files were written.
            var dockerDirs = options.Microservice
                ? serviceNames.Select(s => Path.Combine(solutionDirectory, "services", s))
                : [solutionDirectory];
            foreach (var dir in dockerDirs)
            {
                Assert.True(
                    Directory.EnumerateFiles(dir, "Dockerfile", SearchOption.AllDirectories).Any(),
                    $"No Dockerfile was generated under {dir}");
            }
        }

        Dotnet.Run(solutionDirectory, "build");

        if (options.Tests)
        {
            Dotnet.Run(solutionDirectory, "test", "--no-build");
        }
    }

    public sealed record ScaffoldOptions(
        string TargetFramework,
        bool FourLayer,
        ApiStyle ApiStyle,
        bool Microservice,
        bool Tests,
        EfCoreProvider EfProvider,
        bool Docker,
        bool Jwt);
}
