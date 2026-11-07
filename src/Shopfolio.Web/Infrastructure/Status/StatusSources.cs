using System.Globalization;
using System.Text.Json;

using Npgsql;

using Shopfolio.Infrastructure.Files;

namespace Shopfolio.Infrastructure.Status;

/// <summary>The share of the data disk in use (05 9.4.3, 9.4.10): one disk holds the database, the files and the logs, so one measure.</summary>
internal interface IDiskUsageReader
{
    double UsedFraction();
}

/// <summary>The state of the log archiving read from the database's own statistics (05 9.4.4, 9.4.10).</summary>
internal interface ILogShippingReader
{
    Task<LogShippingState> ReadAsync(CancellationToken ct);
}

/// <summary>The last successful file backup into the repository outside the server, read from the backup tool's status file (05 9.4.5, 9.4.10).</summary>
internal interface IFileBackupStatusReader
{
    Task<DateTimeOffset?> ReadLastSuccessAsync(CancellationToken ct);
}

/// <summary>The disk the file volumes are on (the images volume stands for it: the database, files and logs share one disk, 05 0.5).</summary>
internal sealed class DriveDiskUsageReader : IDiskUsageReader
{
    private readonly string _path;

    public DriveDiskUsageReader(FileStoreDirectories directories)
    {
        ArgumentNullException.ThrowIfNull(directories);
        _path = directories.Images;
    }

    public double UsedFraction()
    {
        // The directory is made on the first write; the nearest directory that exists is on the same disk.
        string? probe = _path;
        while (probe is not null && !Directory.Exists(probe))
        {
            probe = Path.GetDirectoryName(probe);
        }

        DriveInfo drive = new(probe ?? throw new DirectoryNotFoundException("No directory of the data disk exists."));
        long total = drive.TotalSize;
        if (total <= 0)
        {
            throw new IOException("The disk reports no size.");
        }

        return 1.0 - ((double)drive.AvailableFreeSpace / total);
    }
}

/// <summary>
/// Reads <c>pg_stat_archiver</c> and the archive status directory (05 9.4.10). The right to list the waiting files is granted to the application's
/// database user for this reading only (deploy/images/database/bootstrap/40-schema.sql); the monitoring role as a whole is not.
/// </summary>
internal sealed class PostgresLogShippingReader : ILogShippingReader
{
    private const string Query = """
        SELECT last_archived_time, last_failed_time,
               (SELECT count(*) FROM pg_ls_archive_statusdir() WHERE name LIKE '%.ready')
        FROM pg_stat_archiver
        """;

    private readonly NpgsqlDataSource _dataSource;

    public PostgresLogShippingReader(NpgsqlDataSource dataSource)
    {
        ArgumentNullException.ThrowIfNull(dataSource);
        _dataSource = dataSource;
    }

    public async Task<LogShippingState> ReadAsync(CancellationToken ct)
    {
        await using NpgsqlConnection connection = await _dataSource.OpenConnectionAsync(ct);
        await using NpgsqlCommand command = new(Query, connection);
        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(ct);
        if (!await reader.ReadAsync(ct))
        {
            throw new InvalidOperationException("The archiver statistics returned no row.");
        }

        DateTimeOffset? archived = await reader.IsDBNullAsync(0, ct) ? null : await reader.GetFieldValueAsync<DateTimeOffset>(0, ct);
        DateTimeOffset? failed = await reader.IsDBNullAsync(1, ct) ? null : await reader.GetFieldValueAsync<DateTimeOffset>(1, ct);
        long waiting = await reader.GetFieldValueAsync<long>(2, ct);
        return new LogShippingState(archived, failed, (int)Math.Min(waiting, int.MaxValue));
    }
}

/// <summary>
/// The file backup tool's status file (deploy/images/shared/write-status.sh): <c>backup-status.json</c> in the volume the application mounts read-only.
/// <c>lastSuccessAt</c> is an ISO-8601 UTC instant, or the empty text when there has been no success. A missing or damaged file is an unreadable source.
/// </summary>
internal sealed class JsonFileBackupStatusReader : IFileBackupStatusReader
{
    public const string DefaultDirectory = "/var/lib/shopfolio/backup-status";

    public const string FileName = "backup-status.json";

    private readonly string _path;

    public JsonFileBackupStatusReader(string directory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        _path = Path.Combine(directory, FileName);
    }

    public async Task<DateTimeOffset?> ReadLastSuccessAsync(CancellationToken ct)
    {
        string json = await File.ReadAllTextAsync(_path, ct);
        return ParseLastSuccess(json);
    }

    /// <summary>The <c>lastSuccessAt</c> of the file's text; empty for the empty text; <see cref="JsonException"/> or <see cref="FormatException"/> when the text is not the file's shape.</summary>
    public static DateTimeOffset? ParseLastSuccess(string json)
    {
        ArgumentNullException.ThrowIfNull(json);
        using JsonDocument document = JsonDocument.Parse(json);
        if (document.RootElement.ValueKind != JsonValueKind.Object
            || !document.RootElement.TryGetProperty("lastSuccessAt", out JsonElement value)
            || value.ValueKind != JsonValueKind.String)
        {
            throw new FormatException("The status file has no lastSuccessAt text.");
        }

        string text = value.GetString() ?? string.Empty;
        if (text.Length == 0)
        {
            return null;
        }

        if (!DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out DateTimeOffset parsed))
        {
            throw new FormatException("The status file's lastSuccessAt is not a moment.");
        }

        return parsed;
    }
}
