using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// Builds the migration bundle (a single executable made from the EF Core migrations, 09 10.6) once and runs it against a database.
/// Needs the local tool restored (<c>dotnet tool restore</c>) because the bundle is made by <c>dotnet ef migrations bundle</c>.
/// </summary>
internal sealed class MigrationBundle : IDisposable
{
    private static readonly TimeSpan _buildTimeout = TimeSpan.FromMinutes(5);

    private readonly string _directory;

    private MigrationBundle(string directory, string executablePath)
    {
        _directory = directory;
        ExecutablePath = executablePath;
    }

    public string ExecutablePath { get; }

    public static async Task<MigrationBundle> BuildAsync(CancellationToken ct)
    {
        string repositoryRoot = RepositoryRoot.Find();
        string project = Path.Combine(repositoryRoot, "src", "Shopfolio.Web", "Shopfolio.Web.csproj");
        string directory = Path.Combine(Path.GetTempPath(), "shopfolio-bundle-" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(6)));
        Directory.CreateDirectory(directory);
        string executable = Path.Combine(directory, OperatingSystem.IsWindows() ? "efbundle.exe" : "efbundle");

        // The application was built by the project reference in the configuration this test assembly runs in.
        string configuration = new DirectoryInfo(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory)).Parent!.Name; // '!': the base directory is bin/<configuration>/<framework>.
        await RunAsync(
            "dotnet",
            ["ef", "migrations", "bundle", "--project", project, "--output", executable, "--force", "--no-build", "--configuration", configuration],
            repositoryRoot,
            ct);

        return new MigrationBundle(directory, executable);
    }

    /// <summary>
    /// Applies the bundle to the database named in the connection string.
    /// </summary>
    public async Task ApplyAsync(string connectionString, CancellationToken ct)
    {
        await RunAsync(ExecutablePath, ["--connection", connectionString], _directory, ct);
    }

    public void Dispose()
    {
        try
        {
            Directory.Delete(_directory, recursive: true);
        }
        catch (IOException)
        {
            // A leftover temporary directory does not affect the test run.
        }
    }

    private static async Task RunAsync(string fileName, string[] arguments, string workingDirectory, CancellationToken ct)
    {
        ProcessStartInfo startInfo = new(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (string argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using Process process = Process.Start(startInfo) ?? throw new InvalidOperationException($"{fileName} could not be started.");
        StringBuilder output = new();
        process.OutputDataReceived += (_, e) => output.AppendLine(e.Data);
        process.ErrorDataReceived += (_, e) => output.AppendLine(e.Data);
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using CancellationTokenSource timeoutSource = new();
        timeoutSource.CancelAfter(_buildTimeout);
        try
        {
            await process.WaitForExitAsync(timeoutSource.Token).WaitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            // Timeout or cancellation: do not leave a stuck process (and its children) behind.
            process.Kill(entireProcessTree: true);
            throw;
        }

        if (process.ExitCode != 0)
        {
            // The arguments are not repeated here: one of them can be a connection string with a password.
            throw new InvalidOperationException($"{Path.GetFileName(fileName)} exited with code {process.ExitCode}:{Environment.NewLine}{output}");
        }
    }
}
