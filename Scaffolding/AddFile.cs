using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace BuildQuickPkg.Scaffolding;

public enum FileType
{
    Class,
    Interface,
    Struct,
    Enum,
    Record,
}


public enum NamespaceType
{
    /// <summary>namespace A.B { ... } - works on every C# version.</summary>
    BlockScoped,
    /// <summary>namespace A.B; - C# 10+ (.NET 6+), one less indentation level.</summary>
    FileScoped,
}

/// <summary>
/// What a new file should look like: its namespace and namespace style. Reducing boilerplate
/// by automatically detecting the project's namespace and style.
/// </summary>
internal static class AddNewFile
{
    /// <summary>
    /// Find the current project directory by searching for a .csproj file.
    /// </summary>
    public static string FindProjectFile(string startDirectory)
    {
        for (var dir = new DirectoryInfo(startDirectory); dir is not null; dir = dir.Parent)
        {
            var projects = dir.GetFiles("*.csproj");
            if (projects.Length == 1) return projects[0].FullName;
            if (projects.Length > 1)
            {
                throw new InvalidOperationException(
                    $"Found {projects.Length} .csproj files in {dir.FullName} ({string.Join(", ", projects.Select(p => p.Name))}) - run this from inside one project's folder.");
            }
        }
        throw new InvalidOperationException("No .csproj found in this directory or any parent. Run this from inside a C# project.");
    }

    /// <summary>
    /// Resolve the namespace for a new file in the given directory, based on the project's RootNamespace.
    /// </summary>
    public static string ResolveNamespace(string projectFile, string directory)
    {
        var rootNamespace = ReadProperty(projectFile, "RootNamespace") ?? Path.GetFileNameWithoutExtension(projectFile);
        var relative = Path.GetRelativePath(Path.GetDirectoryName(projectFile)!, directory);
        var folders = relative == "."
            ? []
            : relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);

        return string.Join(".", rootNamespace.Split('.').Concat(folders).Select(ToIdentifier));
    }

    /// <summary>
    /// It checks the files in the project to determine the namespace style (file-scoped or block-scoped).
    /// When style is not explicitly set, it defaults to file-scoped on .NET 6+, block-scoped on older targets.
    /// </summary>
    public static NamespaceType DetectNamespaceStyle(string projectFile)
    {
        int fileScoped = 0, block = 0;
        var fileScopedPattern = new Regex(@"^\s*namespace\s+[\w.]+\s*;", RegexOptions.Multiline);
        var blockPattern = new Regex(@"^\s*namespace\s+[\w.]+\s*(\{|$)", RegexOptions.Multiline);

        foreach (var file in Directory.EnumerateFiles(Path.GetDirectoryName(projectFile)!, "*.cs", SearchOption.AllDirectories))
        {
            if (IsBuildOutput(file)) continue;
            var text = File.ReadAllText(file);
            if (fileScopedPattern.IsMatch(text)) fileScoped++;
            else if (blockPattern.IsMatch(text)) block++;
        }

        if (fileScoped != block) return fileScoped > block ? NamespaceType.FileScoped : NamespaceType.BlockScoped;
        return CSharpVersion(projectFile) >= 10 ? NamespaceType.FileScoped : NamespaceType.BlockScoped;
    }

    /// <summary>
    /// The Target Language Version for this project is first checked then the LangVersion property is parsed.
    /// This enables detecting the language version from the project file.
    /// </summary>
    public static double CSharpVersion(string projectFile)
    {
        var langVersion = ReadProperty(projectFile, "LangVersion");
        if (langVersion is "latest" or "latestMajor" or "preview") return 99;
        if (langVersion is not null && double.TryParse(langVersion, System.Globalization.CultureInfo.InvariantCulture, out var v)) return v;
        var tfm = ReadProperty(projectFile, "TargetFramework") ?? ReadProperty(projectFile, "TargetFrameworks")?.Split(';')[0] ?? "";
        var match = Regex.Match(tfm, @"^net(\d+)\.\d+");
        return match.Success && int.Parse(match.Groups[1].Value) >= 5 ? int.Parse(match.Groups[1].Value) + 4 : 8;
    }

    // === Private Helpers ===

    private static bool IsBuildOutput(string path) =>
        path.Split(Path.DirectorySeparatorChar).Any(segment => segment is "bin" or "obj");

    // Reads a property from the project file using XDocument --> A Concept I learnt while building this feature
    private static string? ReadProperty(string projectFile, string name) =>
        XDocument.Load(projectFile).Descendants().FirstOrDefault(e => e.Name.LocalName == name)?.Value.Trim() is { Length: > 0 } value
            ? value
            : null;
  
    private static string ToIdentifier(string segment)
    {
        var cleaned = Regex.Replace(segment, @"[^\w]", "_");
        return char.IsDigit(cleaned[0]) ? "_" + cleaned : cleaned;
    }
}
