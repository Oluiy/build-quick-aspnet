namespace BuildQuickPkg.Utilities;

/// <summary>
/// Inserts new code into an existing generated <c>Program.cs</c> at the stable
/// <c>// BuildQuickPkg:*</c> markers left by <c>Templates.ProgramTemplate</c>, used by
/// <c>BuildQuickPkg add</c> to retrofit an optional feature onto an already-generated project.
/// </summary>
internal static class ProgramCsEditor
{
    public const string UsingsMarker = "// BuildQuickPkg:usings";
    public const string SwaggerMarker = "// BuildQuickPkg:swagger";
    public const string ServicesMarker = "// BuildQuickPkg:services";
    public const string MiddlewareMarker = "// BuildQuickPkg:middleware";
    public const string EndpointsMarker = "// BuildQuickPkg:endpoints";

    /// <summary>True if the file already contains <paramref name="indicator"/> (e.g. a type name unique to the feature), so callers can skip re-adding it.</summary>
    public static bool Contains(string programCsPath, string indicator) =>
        File.ReadAllText(programCsPath).Contains(indicator, StringComparison.Ordinal);

    /// <summary>
    /// Applies every <paramref name="insertions"/> pair (marker, code to insert after it) to
    /// <paramref name="programCsPath"/> in one atomic write. Every marker is validated up front,
    /// before anything is written, so a missing marker (e.g. because Program.cs was hand-edited
    /// past what this can safely patch) leaves the file completely untouched rather than
    /// half-patched, and callers can safely add other files (csproj, appsettings) after this
    /// succeeds without risking a partially-applied feature.
    /// </summary>
    public static void ApplyInsertions(string programCsPath, IReadOnlyList<(string Marker, string Code)> insertions)
    {
        var content = File.ReadAllText(programCsPath);

        foreach (var (marker, _) in insertions)
        {
            if (!content.Contains(marker, StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Couldn't find the '{marker}' marker in '{programCsPath}'. Program.cs has likely been edited past what " +
                    "'BuildQuickPkg add' can safely patch; add the required code there by hand instead. See the docs for what's needed.");
            }
        }

        foreach (var (marker, code) in insertions)
        {
            var markerIndex = content.IndexOf(marker, StringComparison.Ordinal);
            var lineEnd = content.IndexOf('\n', markerIndex);
            lineEnd = lineEnd < 0 ? content.Length : lineEnd + 1;
            content = content[..lineEnd] + code + content[lineEnd..];
        }

        File.WriteAllText(programCsPath, content);
    }
}
