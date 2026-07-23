namespace BuildQuickPkg.Utilities;

/// <summary>
/// Adds <c>PackageReference</c> entries to an existing generated <c>.csproj</c> file, used by
/// <c>BuildQuickPkg add</c> to retrofit an optional feature onto an already-generated project.
/// </summary>
internal static class CsprojEditor
{
    /// <summary>
    /// Appends a new <c>ItemGroup</c> of <c>PackageReference</c> entries just before
    /// <c>&lt;/Project&gt;</c>, skipping any package already referenced (so a retry after a
    /// partially-failed <c>add</c> doesn't duplicate entries). Kept as its own ItemGroup rather
    /// than merged into an existing one; that's valid MSBuild and far simpler than parsing and
    /// merging the XML.
    /// </summary>
    public static void AddPackageReferences(string csprojPath, IReadOnlyList<(string Id, string Version)> packages)
    {
        var content = File.ReadAllText(csprojPath);

        var missing = packages.Where(p => !content.Contains($"Include=\"{p.Id}\"", StringComparison.OrdinalIgnoreCase)).ToList();
        if (missing.Count == 0)
        {
            return;
        }

        var itemGroup = "\n  <ItemGroup>\n" +
            string.Join("\n", missing.Select(p => $"""    <PackageReference Include="{p.Id}" Version="{p.Version}" />""")) +
            "\n  </ItemGroup>\n";

        const string closingTag = "</Project>";
        var insertAt = content.LastIndexOf(closingTag, StringComparison.Ordinal);
        if (insertAt < 0)
        {
            throw new InvalidOperationException($"'{csprojPath}' doesn't look like a valid .csproj (no </Project> found).");
        }

        File.WriteAllText(csprojPath, content[..insertAt] + itemGroup + content[insertAt..]);
    }
}
