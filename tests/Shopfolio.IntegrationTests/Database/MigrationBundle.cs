using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.IntegrationTests.Database;

/// <summary>
/// Builds the migration bundle (a single executable made from the EF Core migrations, 09 10.6) and runs it against a database.
/// Needs the local tool restored (<c>dotnet tool restore</c>) because the bundle is made by <c>dotnet ef migrations bundle</c>.
/// </summary>
/// <remarks>
/// Building the bundle is the most expensive step of an integration run, so <see cref="BuildAsync"/> keeps it in a cache under the
/// temporary directory, named by a digest of everything the bundle is made from: the built application assembly, its lock file and
/// the tool manifest. A run with the same build reuses it; any change to the migrations changes the assembly and so the name.
/// </remarks>
internal sealed class MigrationBundle : IDisposable
{
    private const string _cacheDirectoryName = "shopfolio-bundle-cache";

    private static readonly TimeSpan _buildTimeout = TimeSpan.FromMinutes(5);

    // A cache entry not used for this long belongs to an old build; another run on the same machine may still use a newer one.
    private static readonly TimeSpan _staleAfter = TimeSpan.FromDays(1);

    private readonly string _directory;
    private readonly bool _ownsDirectory;

    private MigrationBundle(string directory, string executablePath, bool ownsDirectory)
    {
        _directory = directory;
        _ownsDirectory = ownsDirectory;
        ExecutablePath = executablePath;
    }

    public string ExecutablePath { get; }

    /// <summary>
    /// Returns the bundle of the current build: the cached one when it exists, otherwise a new one that is then cached.
    /// </summary>
    public static async Task<MigrationBundle> BuildAsync(CancellationToken ct)
    {
        string cacheRoot = Path.Combine(Path.GetTempPath(), _cacheDirectoryName);
        string entry = Path.Combine(cacheRoot, ComputeBuildDigest());
        string cachedExecutable = Path.Combine(entry, ExecutableName);
        if (File.Exists(cachedExecutable))
        {
            Directory.SetLastWriteTimeUtc(entry, DateTime.UtcNow);
            return new MigrationBundle(entry, cachedExecutable, ownsDirectory: false);
        }

        MigrationBundle built = await BuildFreshAsync(ct);
        Directory.CreateDirectory(cacheRoot);
        try
        {
            // Same volume (both under the temporary directory), so the move is a rename; a finished entry appears all at once.
            Directory.Move(built._directory, entry);
        }
        catch (IOException) when (File.Exists(cachedExecutable))
        {
            // Another run cached the same build first: use its entry and let this copy go.
            built.Dispose();
        }
        catch (IOException)
        {
            // The cache could not be written; the fresh bundle still serves this run.
            return built;
        }

        RemoveStaleEntries(cacheRoot, entry);
        return new MigrationBundle(entry, cachedExecutable, ownsDirectory: false);
    }

    /// <summary>
    /// Builds a new bundle in a temporary directory of its own, bypassing the cache; the directory is removed on dispose.
    /// </summary>
    public static async Task<MigrationBundle> BuildFreshAsync(CancellationToken ct)
    {
        string repositoryRoot = RepositoryRoot.Find();
        string project = Path.Combine(repositoryRoot, "src", "Shopfolio.Web", "Shopfolio.Web.csproj");
        string directory = Path.Combine(Path.GetTempPath(), "shopfolio-bundle-" + Convert.ToHexStringLower(RandomNumberGenerator.GetBytes(6)));
        Directory.CreateDirectory(directory);
        string executable = Path.Combine(directory, ExecutableName);

        await RunAsync(
            "dotnet",
            ["ef", "migrations", "bundle", "--project", project, "--output", executable, "--force", "--no-build", "--configuration", Configuration],
            repositoryRoot,
            // The bundle publishes with a runtime identifier, which would add a target (win-x64, linux-x64, ...) to the committed
            // src/Shopfolio.Web/packages.lock.json and break `dotnet restore --locked-mode` on another operating system.
            // Pointing NuGet at a throwaway lock file keeps every tracked file untouched, on every platform.
            new Dictionary<string, string> { ["NuGetLockFilePath"] = Path.Combine(directory, "throwaway.packages.lock.json") },
            ct);

        return new MigrationBundle(directory, executable, ownsDirectory: true);
    }

    /// <summary>
    /// Applies the bundle to the database named in the connection string.
    /// </summary>
    public async Task ApplyAsync(string connectionString, CancellationToken ct)
    {
        await RunAsync(ExecutablePath, ["--connection", connectionString], _directory, environment: null, ct);
    }

    public void Dispose()
    {
        if (!_ownsDirectory)
        {
            return;
        }

        DeleteQuietly(_directory);
    }

    private static string ExecutableName => OperatingSystem.IsWindows() ? "efbundle.exe" : "efbundle";

    // The application was built by the project reference in the configuration this test assembly runs in; the base directory is
    // bin/<configuration>/<framework>.
    private static DirectoryInfo BaseDirectory => new(Path.TrimEndingDirectorySeparator(AppContext.BaseDirectory));

    private static string Configuration => BaseDirectory.Parent!.Name; // '!': see BaseDirectory.

    /// <summary>
    /// Digest of the inputs of <c>dotnet ef migrations bundle --no-build</c>: the application assembly it reads, the lock file that
    /// fixes the packages it publishes with, and the tool manifest that fixes the tool version.
    /// </summary>
    private static string ComputeBuildDigest()
    {
        string repositoryRoot = RepositoryRoot.Find();
        string[] inputs =
        [
            Path.Combine(repositoryRoot, "src", "Shopfolio.Web", "bin", Configuration, BaseDirectory.Name, "Shopfolio.Web.dll"),
            Path.Combine(repositoryRoot, "src", "Shopfolio.Web", "packages.lock.json"),
            Path.Combine(repositoryRoot, ".config", "dotnet-tools.json"),
        ];
        using IncrementalHash hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (string input in inputs)
        {
            hash.AppendData(File.ReadAllBytes(input));
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset())[..16];
    }

    private static void RemoveStaleEntries(string cacheRoot, string current)
    {
        foreach (string entry in Directory.EnumerateDirectories(cacheRoot))
        {
            if (!string.Equals(entry, current, StringComparison.Ordinal) && DateTime.UtcNow - Directory.GetLastWriteTimeUtc(entry) > _staleAfter)
            {
                DeleteQuietly(entry);
            }
        }
    }

    private static void DeleteQuietly(string directory)
    {
        try
        {
            Directory.Delete(directory, recursive: true);
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            // A leftover temporary directory (or one whose bundle is running) does not affect the test run.
        }
    }

    private static async Task RunAsync(string fileName, string[] arguments, string workingDirectory, IReadOnlyDictionary<string, string>? environment, CancellationToken ct)
    {
        ProcessStartInfo startInfo = new(fileName)
        {
            WorkingDirectory = workingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach ((string name, string value) in environment ?? new Dictionary<string, string>())
        {
            startInfo.Environment[name] = value;
        }

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
