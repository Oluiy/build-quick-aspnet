using BuildQuickPkg.Scaffolding;
using BuildQuickPkg.Templates;
using BuildQuickPkg.Utilities;
using Spectre.Console;

namespace BuildQuickPkg.Commands;

/// <summary>
/// <c>BuildQuickPkg new <class|interface|struct|enum|record> Name [--file-scoped | --block]</c>:
/// creates Name.cs in the current directory with the right namespace already filled in.
/// </summary>
internal static class NewFileCommand
{
    public static bool Run(string[] args)
    {
        var positional = args.Where(a => !a.StartsWith("--", StringComparison.Ordinal)).ToList();
        var flags = args.Where(a => a.StartsWith("--", StringComparison.Ordinal)).Select(a => a.ToLowerInvariant()).ToList();

        var unknownFlag = flags.FirstOrDefault(f => f is not ("--file-scoped" or "--block"));
        if (unknownFlag is not null || (flags.Contains("--file-scoped") && flags.Contains("--block")))
        {
            AnsiConsole.MarkupLine(unknownFlag is not null
                ? $"[red]Unknown option '{Markup.Escape(unknownFlag)}'.[/] Use --file-scoped or --block."
                : "[red]Pick one of --file-scoped or --block, not both.[/]");
            return false;
        }

        var fileType = positional.Count > 0 ? ParseFileType(positional[0]) : PromptFileType();
        if (fileType is null)
        {
            AnsiConsole.MarkupLine($"[red]Unknown type '{Markup.Escape(positional[0])}'.[/] Use class, interface, struct, enum, or record.");
            return false;
        }

        var typeName = positional.Count > 1 ? positional[1] : AnsiConsole.Ask<string>($"{fileType.Value} name:");
        // "Order.cs" typed out of habit is still clearly meant as "Order".
        if (typeName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) typeName = typeName[..^3];
        if (!NameValidation.IsValidIdentifierName(typeName))
        {
            AnsiConsole.MarkupLine($"[red]'{Markup.Escape(typeName)}' isn't a valid C# type name or extension(use .cs as the file extension).[/] Use letters, digits, and underscores, starting with a letter.");
            return false;
        }

        var directory = Directory.GetCurrentDirectory();
        var filePath = Path.Combine(directory, $"{typeName}.cs");
        // Never overwrite: an existing file is someone's work, and nothing is written on failure.
        if (File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(typeName)}.cs already exists[/] in this directory. Nothing was written.");
            return false;
        }

        string projectFile;
        try
        {
            projectFile = AddNewFile.FindProjectFile(directory);
        }
        catch (InvalidOperationException ex)
        {
            AnsiConsole.MarkupLine($"[red]{Markup.Escape(ex.Message)}[/]");
            return false;
        }

        if (fileType == FileType.Record && AddNewFile.CSharpVersion(projectFile) < 9)
        {
            AnsiConsole.MarkupLine($"[red]Records need C# 9 (.NET 5+)[/], and {Markup.Escape(Path.GetFileName(projectFile))} targets an older version. Use a class instead, or raise <LangVersion>. Nothing was written.");
            return false;
        }

        var @namespace = AddNewFile.ResolveNamespace(projectFile, directory);
        var style = flags.Contains("--file-scoped") ? NamespaceType.FileScoped
            : flags.Contains("--block") ? NamespaceType.BlockScoped
            : AddNewFile.DetectNamespaceStyle(projectFile);

        File.WriteAllText(filePath, AddFileTemplate.Generate(@namespace, typeName, fileType.Value, style));

        var styleLabel = style == NamespaceType.FileScoped ? "file-scoped" : "block-scoped";
        AnsiConsole.MarkupLine($"[green]Created[/] {Markup.Escape(typeName)}.cs [grey]({fileType.Value.ToString().ToLowerInvariant()} in {Markup.Escape(@namespace)}, {styleLabel} namespace)[/]");
        if (fileType == FileType.Interface && !(typeName.Length > 1 && typeName[0] == 'I' && char.IsUpper(typeName[1])))
        {
            AnsiConsole.MarkupLine($"[yellow]Tip:[/] .NET convention names interfaces with an I prefix, e.g. I{Markup.Escape(typeName)}.");
        }
        return true;
    }

    private static FileType? ParseFileType(string value) => value.ToLowerInvariant() switch
    {
        "class" => FileType.Class,
        "interface" => FileType.Interface,
        "struct" => FileType.Struct,
        "enum" => FileType.Enum,
        "record" => FileType.Record,
        _ => null,
    };

    private static FileType? PromptFileType() =>
        AnsiConsole.Prompt(new SelectionPrompt<FileType>().Title("What kind of type?").AddChoices(Enum.GetValues<FileType>()));
}
