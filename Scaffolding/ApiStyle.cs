namespace BuildQuickPkg.Scaffolding;

/// <summary>
/// The style of API surface to generate for the sample endpoints (and any endpoints
/// retrofitted later via <c>BuildQuickPkg add</c>).
/// </summary>
public enum ApiStyle
{
    /// <summary>Top-level <c>app.MapGet</c>/<c>app.MapPost</c> endpoints directly in Program.cs.</summary>
    Minimal,

    /// <summary>Controllers backed by an interface/service pair (service-controller pattern).</summary>
    Controller
}
