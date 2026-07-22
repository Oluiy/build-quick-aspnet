using System.Diagnostics;

namespace create_aspnet_app.Utilities;

/// <summary>
/// Executes external command-line processes such as the .NET CLI.
/// </summary>
internal static class ProcessRunner
{
    /// <summary>
    /// Runs a <c>dotnet</c> CLI command synchronously in the given working directory.
    /// </summary>
    /// <param name="arguments">The arguments passed to the <c>dotnet</c> executable, e.g. <c>"new sln -n MyApp"</c>.</param>
    /// <param name="workingDirectory">The directory the command is executed from.</param>
    public static void RunDotnetCommand(string arguments, string workingDirectory)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(startInfo);
        process?.WaitForExit();
    }
}
