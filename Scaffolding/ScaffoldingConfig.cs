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

    /// <summary>When true, generates one independent solution per <see cref="ServiceNames"/> entry instead of a single monolithic solution.</summary>
    public required bool IsMicroservice { get; init; }

    /// <summary>The name of each service to generate. Only used when <see cref="IsMicroservice"/> is true.</summary>
    public IReadOnlyList<string> ServiceNames { get; init; } = [];

    /// <summary>The base HTTP port written to launchSettings.json. In microservice mode, each service is offset from this by 10.</summary>
    public required int HttpPort { get; init; }

    /// <summary>The base HTTPS port written to launchSettings.json. In microservice mode, each service is offset from this by 10.</summary>
    public required int HttpsPort { get; init; }
}
