using System.Diagnostics;

namespace BuildQuickPkg.Utilities;

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
    /// <exception cref="InvalidOperationException">The process could not be started, or exited with a non-zero code.</exception>
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

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Failed to start 'dotnet {arguments}'.");

        // Drain both streams concurrently while waiting for exit; reading only one
        // synchronously risks a deadlock if the other fills its OS pipe buffer.
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();
        process.WaitForExit();
        var stderr = stderrTask.GetAwaiter().GetResult();
        _ = stdoutTask.GetAwaiter().GetResult();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"'dotnet {arguments}' failed with exit code {process.ExitCode} in '{workingDirectory}':\n{stderr}");
        }
    }
}
