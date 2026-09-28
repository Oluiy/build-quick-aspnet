using BuildQuickPkg.Scaffolding;

namespace BuildQuickPkg.Templates;

/// <summary>The contents of a new <c>.cs</c> file created by <c>BuildQuickPkg new</c>.</summary>
internal static class AddFileTemplate
{
    public static string Generate(string @namespace, string typeName, FileType fileType, NamespaceType style)
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
