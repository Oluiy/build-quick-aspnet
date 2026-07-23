namespace BuildQuickPkg.Scaffolding;

/// <summary>
/// Describes a BuildQuickPkg-generated project resolved from disk, so <c>BuildQuickPkg add</c>
/// can retrofit an optional feature onto it after the fact.
/// </summary>
internal sealed record ExistingProject
{
    /// <summary>The base project name, e.g. <c>MyAwesomeApi</c> (derived from the <c>..._API</c> project folder).</summary>
    public required string ProjectName { get; init; }

    /// <summary>The directory containing the <c>.sln</c>, and (if present) the <c>Dockerfile</c>/<c>docker-compose.yml</c>.</summary>
    public required string RootDirectory { get; init; }

    /// <summary>The directory containing every buildable project (<c>{Root}/src</c>).</summary>
    public required string SrcDirectory { get; init; }

    /// <summary>The API project's directory.</summary>
    public required string ApiProjectDirectory { get; init; }

    /// <summary>The API project's <c>.csproj</c> path.</summary>
    public required string ApiCsprojPath { get; init; }

    /// <summary>When true, the dedicated Infrastructure project owns <c>Context/</c>; otherwise it's folded into Domain.</summary>
    public required bool IsFourLayer { get; init; }

    /// <summary>The target framework moniker read from the API project, e.g. <c>net8.0</c>.</summary>
    public required string TargetFramework { get; init; }

    /// <summary>The directory of the project that owns (or would own) <c>Infrastructure/Context</c>: the dedicated Infrastructure project in 4-layer, or Domain in 3-layer.</summary>
    public required string ContextOwnerDirectory { get; init; }

    /// <summary>The <c>.csproj</c> path of <see cref="ContextOwnerDirectory"/>.</summary>
    public required string ContextOwnerCsprojPath { get; init; }

    /// <summary>The namespace the generated <c>DbContext</c> lives (or would live) in.</summary>
    public required string DbContextNamespace { get; init; }
}
