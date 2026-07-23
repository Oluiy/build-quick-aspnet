namespace BuildQuickPkg.Scaffolding;

/// <summary>
/// Locates a BuildQuickPkg-generated project relative to the current working directory, so
/// <c>BuildQuickPkg add</c> can retrofit an optional feature onto an already-generated solution.
/// </summary>
internal static class ExistingProjectLocator
{
    /// <summary>
    /// Resolves an <see cref="ExistingProject"/> from <paramref name="startDirectory"/>, which may
    /// be the API project's own folder, a solution root, or a microservice aggregate root.
    /// </summary>
    /// <exception cref="InvalidOperationException">No project (or more than one candidate) was found.</exception>
    public static ExistingProject Locate(string startDirectory)
    {
        var apiCsprojPath = FindApiCsproj(startDirectory);

        var apiProjectDirectory = Path.GetDirectoryName(apiCsprojPath)!;
        var srcDirectory = Path.GetDirectoryName(apiProjectDirectory)!;
        var rootDirectory = Path.GetDirectoryName(srcDirectory)!;

        var apiProjectName = Path.GetFileNameWithoutExtension(apiCsprojPath);
        if (!apiProjectName.EndsWith("_API", StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"'{apiProjectName}' doesn't look like a BuildQuickPkg-generated API project (expected a '..._API.csproj').");
        }

        var projectName = apiProjectName[..^"_API".Length];
        var infrastructureDirectory = Path.Combine(srcDirectory, $"{projectName}_Infrastructure");
        var isFourLayer = Directory.Exists(infrastructureDirectory);
        var domainDirectory = Path.Combine(srcDirectory, $"{projectName}_Domain");

        var contextOwnerDirectory = isFourLayer ? infrastructureDirectory : domainDirectory;
        var contextOwnerProjectName = isFourLayer ? $"{projectName}_Infrastructure" : $"{projectName}_Domain";
        var contextOwnerCsprojPath = Path.Combine(contextOwnerDirectory, $"{contextOwnerProjectName}.csproj");

        var dbContextNamespace = isFourLayer
            ? $"{projectName}_Infrastructure.Context"
            : $"{projectName}_Domain.Infrastructure.Context";

        return new ExistingProject
        {
            ProjectName = projectName,
            RootDirectory = rootDirectory,
            SrcDirectory = srcDirectory,
            ApiProjectDirectory = apiProjectDirectory,
            ApiCsprojPath = apiCsprojPath,
            IsFourLayer = isFourLayer,
            TargetFramework = ReadTargetFramework(apiCsprojPath),
            ContextOwnerDirectory = contextOwnerDirectory,
            ContextOwnerCsprojPath = contextOwnerCsprojPath,
            DbContextNamespace = dbContextNamespace,
        };
    }

    private static string FindApiCsproj(string startDirectory)
    {
        // Running from inside the API project itself.
        var direct = Directory.GetFiles(startDirectory, "*_API.csproj", SearchOption.TopDirectoryOnly);
        if (direct.Length == 1)
        {
            return direct[0];
        }

        // Running from a solution root: {Root}/src/{Name}_API/{Name}_API.csproj
        var srcDirectory = Path.Combine(startDirectory, "src");
        if (Directory.Exists(srcDirectory))
        {
            var underSrc = Directory.GetFiles(srcDirectory, "*_API.csproj", SearchOption.AllDirectories);
            if (underSrc.Length == 1)
            {
                return underSrc[0];
            }
        }

        // Running from a microservice aggregate root: {Root}/services/{Service}/src/{Name}_API/{Name}_API.csproj
        var servicesDirectory = Path.Combine(startDirectory, "services");
        if (Directory.Exists(servicesDirectory))
        {
            var underServices = Directory.GetFiles(servicesDirectory, "*_API.csproj", SearchOption.AllDirectories);
            if (underServices.Length == 1)
            {
                return underServices[0];
            }

            if (underServices.Length > 1)
            {
                var names = string.Join(", ", underServices.Select(Path.GetFileNameWithoutExtension));
                throw new InvalidOperationException(
                    $"Found multiple services here ({names}). cd into the specific service's folder first, e.g. 'cd services/{{ServiceName}}'.");
            }
        }

        throw new InvalidOperationException(
            "Couldn't find a BuildQuickPkg-generated project here. Run 'BuildQuickPkg add' from the solution root, a microservice's root, or the API project folder.");
    }

    private static string ReadTargetFramework(string csprojPath)
    {
        var content = File.ReadAllText(csprojPath);
        const string openTag = "<TargetFramework>";
        const string closeTag = "</TargetFramework>";

        var start = content.IndexOf(openTag, StringComparison.Ordinal);
        var end = content.IndexOf(closeTag, StringComparison.Ordinal);
        if (start < 0 || end < 0 || end <= start)
        {
            throw new InvalidOperationException($"Couldn't read <TargetFramework> from '{csprojPath}'.");
        }

        return content[(start + openTag.Length)..end].Trim();
    }
}
