using BuildQuickPkg.Scaffolding;

namespace BuildQuickPkg.Templates;

/// <summary>The contents of a new <c>.cs</c> file created by <c>BuildQuickPkg new</c>.</summary>
internal static class AddFileTemplate
{
    /// <summary>
    /// The usings nearly every new type needs. Only written when the project doesn't already get
    /// them from ImplicitUsings; there they'd be flagged as unnecessary (IDE0005).
    /// </summary>
    internal static readonly string[] EssentialUsings =
    [
        "System",
        "System.Collections.Generic",
        "System.Linq",
        "System.Threading.Tasks",
    ];

    public static string Generate(string @namespace, string typeName, FileType fileType, NamespaceType style, bool includeUsings = false)
    {
        var body = GenerateBody(@namespace, typeName, fileType, style);

        // Enums never need these, so they stay clean either way.
        if (!includeUsings || fileType == FileType.Enum) return body;

        var usings = string.Concat(EssentialUsings.Select(u => $"using {u};\n"));
        return $"{usings}\n{body}";
    }

    private static string GenerateBody(string @namespace, string typeName, FileType fileType, NamespaceType style)
    {
        var keyword = fileType switch
        {
            FileType.Class => "class",
            FileType.Interface => "interface",
            FileType.Struct => "struct",
            FileType.Enum => "enum",
            FileType.Record => "record",
            _ => throw new ArgumentOutOfRangeException(nameof(fileType)),
        };

        var declaration = $"public {keyword} {typeName}\n{{\n}}\n";

        return style == NamespaceType.FileScoped
            ? $"namespace {@namespace};\n\n{declaration}"
            : $"namespace {@namespace}\n{{\n{Indent(declaration)}}}\n";
    }

    private static string Indent(string block) =>
        string.Concat(block.Split('\n').Where(line => line.Length > 0).Select(line => $"    {line}\n"));
}
