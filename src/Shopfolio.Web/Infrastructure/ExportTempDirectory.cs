using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.LogRead;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The one place the export files are made (09 10.10 items 6, 8; 05 6.8.4, 9.3.3): the temporary directory outside the records and the file backup, the setting
/// <c>Export:TempDirectory</c> (default <c>shopfolio-export</c> under the container's temporary directory). A file has a random name that names no firm and no person, is opened for its
/// owner only and is deleted when its stream closes (<see cref="FileOptions.DeleteOnClose"/>). The leftover of a process that fell before the stream closed is swept: every file at start,
/// and the files older than <see cref="ExportLimits.SweepAge"/> at the start of each request. The three exports open the directory only through this class.
/// </summary>
internal sealed partial class ExportTempDirectory
{
    private readonly string _path;
    private readonly TimeProvider _time;
    private readonly ILogger<ExportTempDirectory> _logger;

    public ExportTempDirectory(IConfiguration configuration, TimeProvider time, ILogger<ExportTempDirectory> logger)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _path = LogExportFile.DirectoryOf(configuration);
        _time = time;
        _logger = logger;
    }

    /// <summary>The directory.</summary>
    public string Path => _path;

    /// <summary>Makes the directory and tries one write in it; a directory that cannot be made or written is an <see cref="InvalidOperationException"/> that stops the start (09 3.5 item 2).</summary>
    public void EnsureUsable()
    {
        try
        {
            Directory.CreateDirectory(_path);
            string probe = System.IO.Path.Combine(_path, $"{EntityIds.New():N}.probe");
            File.WriteAllBytes(probe, [0]);
            File.Delete(probe);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or NotSupportedException)
        {
            throw new InvalidOperationException($"The setting {LogExportFile.DirectoryKey} names a directory the application cannot write to.", ex);
        }
    }

    /// <summary>Opens a new file for reading and writing; it is deleted when the stream closes. The caller binds the stream to the lifetime of its request.</summary>
    public FileStream OpenNew()
    {
        Directory.CreateDirectory(_path);
        string file = System.IO.Path.Combine(_path, $"{EntityIds.New():N}.csv");
        FileStreamOptions options = new()
        {
            Mode = FileMode.CreateNew,
            Access = FileAccess.ReadWrite,
            Share = FileShare.None,
            Options = FileOptions.DeleteOnClose | FileOptions.Asynchronous | FileOptions.SequentialScan,
        };
        if (!OperatingSystem.IsWindows())
        {
            options.UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite;
        }

        return new FileStream(file, options);
    }

    /// <summary>At start: every file in the directory goes (one instance runs, so no export is half way).</summary>
    public void SweepAll() => Sweep(null);

    /// <summary>At the start of an export request: the files older than the sweep age go. A failure is logged and does not stop the request.</summary>
    public void SweepOld() => Sweep(_time.GetUtcNow() - ExportLimits.SweepAge);

    private void Sweep(DateTimeOffset? olderThan)
    {
        try
        {
            if (!Directory.Exists(_path))
            {
                return;
            }

            foreach (string file in Directory.EnumerateFiles(_path))
            {
                try
                {
                    if (olderThan is { } limit && File.GetCreationTimeUtc(file) >= limit.UtcDateTime)
                    {
                        continue;
                    }

                    File.Delete(file);
                }
                catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
                {
                    LogSweepFailed(_logger, LogEvents.UnexpectedError, ex);
                }
            }
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            LogSweepFailed(_logger, LogEvents.UnexpectedError, ex);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}: an old export file could not be swept.")]
    private static partial void LogSweepFailed(ILogger logger, string @event, Exception exception);
}

/// <summary>Makes the export directory when the application starts, tries one write and sweeps what a process that fell left (09 10.10 items 6, 8); an unwritable directory refuses the start.</summary>
internal sealed class ExportDirectoryStartup : IHostedService
{
    private readonly ExportTempDirectory _directory;

    public ExportDirectoryStartup(ExportTempDirectory directory)
    {
        _directory = directory;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _directory.EnsureUsable();
        _directory.SweepAll();
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
