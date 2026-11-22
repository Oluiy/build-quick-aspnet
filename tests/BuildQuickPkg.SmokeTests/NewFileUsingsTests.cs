using BuildQuickPkg.Scaffolding;
using BuildQuickPkg.Templates;
using Xunit;

namespace BuildQuickPkg.SmokeTests;

/// <summary>
/// `BuildQuickPkg new` should write the common usings only when the project doesn't already
/// import them through ImplicitUsings.
/// </summary>
public class NewFileUsingsTests
{
    [Theory]
    [InlineData("enable", false)]
    [InlineData("true", false)]
    [InlineData("disable", true)]
    [InlineData(null, true)]
    public void Usings_are_added_only_when_the_project_lacks_implicit_usings(string? implicitUsings, bool expectUsings)
    {
        using var workspace = new TempDirectory();
        var project = WriteProject(workspace.Path, implicitUsings);

        var includeUsings = !AddNewFile.UsesImplicitUsings(project);
        var file = AddFileTemplate.Generate("Demo", "Order", FileType.Class, NamespaceType.FileScoped, includeUsings);

        Assert.Equal(expectUsings, file.Contains("using System;"));
    }

    [Fact]
    public void Setting_in_Directory_Build_props_is_respected()
    {
        using var workspace = new TempDirectory();
        File.WriteAllText(Path.Combine(workspace.Path, "Directory.Build.props"),
            "<Project><PropertyGroup><ImplicitUsings>enable</ImplicitUsings></PropertyGroup></Project>");
        var projectDir = Directory.CreateDirectory(Path.Combine(workspace.Path, "src", "Demo")).FullName;
        var project = WriteProject(projectDir, implicitUsings: null);

        Assert.True(AddNewFile.UsesImplicitUsings(project));
    }

    [Fact]
    public void Enums_never_get_usings()
    {
        var file = AddFileTemplate.Generate("Demo", "Status", FileType.Enum, NamespaceType.FileScoped, includeUsings: true);

        Assert.DoesNotContain("using ", file);
    }

    [Theory]
    [InlineData(NamespaceType.FileScoped)]
    [InlineData(NamespaceType.BlockScoped)]
    public void Generated_file_with_usings_builds_in_a_project_without_implicit_usings(NamespaceType style)
    {
        using var workspace = new TempDirectory();
        WriteProject(workspace.Path, "disable");
        File.WriteAllText(Path.Combine(workspace.Path, "Order.cs"),
            AddFileTemplate.Generate("Demo", "Order", FileType.Class, style, includeUsings: true));

        // Fails if any of the added usings doesn't resolve.
        Dotnet.Run(workspace.Path, "build");
    }

    private static string WriteProject(string directory, string? implicitUsings)
    {
        var setting = implicitUsings is null ? "" : $"<ImplicitUsings>{implicitUsings}</ImplicitUsings>";
        var path = Path.Combine(directory, "Demo.csproj");
        File.WriteAllText(path,
            $"<Project Sdk=\"Microsoft.NET.Sdk\"><PropertyGroup><TargetFramework>net8.0</TargetFramework>{setting}</PropertyGroup></Project>");
        return path;
    }
}
