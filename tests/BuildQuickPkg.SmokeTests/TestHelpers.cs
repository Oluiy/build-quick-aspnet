using System.Diagnostics;
using Xunit.Sdk;

namespace BuildQuickPkg.SmokeTests;

/// <summary>A unique temporary folder that is deleted when the test finishes.</summary>
internal sealed class TempDirectory : IDisposable
{
    public string Path { get; } = Directory.CreateTempSubdirectory("buildquick-smoke-").FullName;

    public void Dispose()
    {
        try
        {
            Directory.Delete(Path, recursive: true);
        }
        catch (IOException)
        {
            // A build server can still hold a file for a moment; the OS cleans temp folders anyway.
        }
    }
}

/// <summary>Runs the dotnet CLI against a generated solution and fails the test with its output if it fails.</summary>
internal static class Dotnet
{
    private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(10);

    public static void Run(string workingDirectory, params string[] arguments)
    {
        var startInfo = new ProcessStartInfo("dotnet")
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }
        startInfo.ArgumentList.Add("--nologo");

        // `dotnet test` sets MSBuild variables pointing at its own SDK. If the generated
        // solution inherits them, it can build against the wrong SDK, so clear them.
        foreach (var key in startInfo.Environment.Keys.ToList())
        {
            if (key.StartsWith("MSBUILD", StringComparison.OrdinalIgnoreCase))
            {
                startInfo.Environment.Remove(key);
            }
        }
        startInfo.Environment["DOTNET_CLI_TELEMETRY_OPTOUT"] = "1";
        startInfo.Environment["DOTNET_NOLOGO"] = "1";

        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Could not start the dotnet CLI.");

        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();

        if (!process.WaitForExit(Timeout))
        {
            process.Kill(entireProcessTree: true);
            throw new XunitException($"'dotnet {string.Join(' ', arguments)}' timed out after {Timeout.TotalMinutes} minutes.");
        }

        if (process.ExitCode != 0)
        {
            throw new XunitException(
                $"'dotnet {string.Join(' ', arguments)}' failed (exit code {process.ExitCode}) in {workingDirectory}\n" +
                $"{stdout.Result}\n{stderr.Result}");
        }
    }
}
