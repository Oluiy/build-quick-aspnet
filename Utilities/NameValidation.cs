namespace BuildQuickPkg.Utilities;

/// <summary>
/// Validates that a user-supplied project or service name is safe to use as a C# namespace
/// segment (e.g. <c>{Name}_API</c>), a class name prefix (e.g. <c>{Name}DbContext</c>), and a
/// filesystem folder/file name, everywhere BuildQuickPkg derives one from the other.
/// </summary>
internal static class NameValidation
{
    /// <summary>True if <paramref name="name"/> is non-empty and contains only letters, digits, and underscores, starting with a letter or underscore.</summary>
    public static bool IsValidIdentifierName(string name) =>
        name.Length > 0
        && (char.IsLetter(name[0]) || name[0] == '_')
        && name.All(c => char.IsLetterOrDigit(c) || c == '_');
}
