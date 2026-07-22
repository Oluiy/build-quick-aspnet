namespace create_aspnet_app.Templates;

/// <summary>
/// Produces the <c>.gitignore</c> written to the root of every generated solution.
/// </summary>
internal static class GitignoreTemplate
{
    /// <summary>Builds a .gitignore covering standard .NET build output and IDE folders.</summary>
    public static string Generate() => """
        *.user
        *.userosscache
        *.sln.docstates
        bin/
        obj/
        TestResults/
        .vs/
        .idea/
        .vscode/
        """;
}
