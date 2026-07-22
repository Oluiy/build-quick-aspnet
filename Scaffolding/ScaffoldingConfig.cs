namespace create_aspnet_app.Scaffolding;

/// <summary>
/// User-supplied options that control how a solution is scaffolded.
/// </summary>
public sealed record ScaffoldingConfig
{
    /// <summary>The base name used to derive the solution and each layer's project name.</summary>
    public required string ProjectName { get; init; }

    /// <summary>The target framework moniker applied to every project, e.g. <c>net8.0</c>.</summary>
    public required string TargetFramework { get; init; }

    /// <summary>The directory the solution folder is created in.</summary>
    public required string OutputDirectory { get; init; }

    /// <summary>When true, generates a dedicated Infrastructure project; otherwise persistence concerns are folded into Domain.</summary>
    public required bool IsFourLayer { get; init; }

    /// <summary>When true, generates an xUnit test project exercising the API's endpoints.</summary>
    public required bool IncludeTests { get; init; }

    /// <summary>The HTTP port written to launchSettings.json.</summary>
    public required int HttpPort { get; init; }

    /// <summary>The HTTPS port written to launchSettings.json.</summary>
    public required int HttpsPort { get; init; }
}
