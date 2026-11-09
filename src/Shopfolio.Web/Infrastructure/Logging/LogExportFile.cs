namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// The file the read command writes its result to (05 9.3.3; K-986): a CSV in the temporary directory outside the logs and the file backup - the same setting as the panel's exports,
/// <c>Export:TempDirectory</c>, defaulting to <c>shopfolio-export</c> under the container's temporary directory (09 21 item 6). In an installation the read command's container mounts a
/// directory of the server there that is in no backup (deploy/compose.yaml <c>read-command</c>); its encrypted delivery to the firm and its deletion afterwards are DEPLOY_RUNBOOK steps.
/// The name is a random id (<c>{guid:N}.csv</c>) and carries no name of the firm or of a person; the file is opened for its owner only and is never overwritten.
/// </summary>
internal static class LogExportFile
{
    public const string DirectoryKey = "Export:TempDirectory";

    public const string DefaultDirectoryName = "shopfolio-export";

    public static string DirectoryOf(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        string? directory = configuration[DirectoryKey];
        return string.IsNullOrWhiteSpace(directory) ? Path.Combine(Path.GetTempPath(), DefaultDirectoryName) : directory;
    }

    /// <summary>Creates a new file in <paramref name="directory"/> (made when missing) and returns its path and its stream.</summary>
    public static (string Path, FileStream Stream) Create(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        Directory.CreateDirectory(directory);
        string path = System.IO.Path.Combine(directory, $"{EntityIds.New():N}.csv");
        FileStreamOptions options = new() { Mode = FileMode.CreateNew, Access = FileAccess.Write, Share = FileShare.None };
        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        return (path, new FileStream(path, options));
    }
}
