using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The chunked upload of a digital product file (05 7.7.3; 07 4.31.2; 09 15.7 items 5-7). The start writes an unlinked, incomplete file row; every chunk is taken
/// under the row's lock, with the received bytes read from the file store inside the lock, so two chunks of one upload never both append; the last chunk
/// completes the upload in the same transaction; the status is a plain read. The row is never linked here: the file is attached to a product or variant by the
/// form's save (UC-114), and an upload that is not completed is swept the next night (05 5.2.9). The methods join the caller's transaction and never open one.
/// </summary>
internal sealed class DigitalFileUploads
{
    private readonly AppDbContext _db;
    private readonly IFileStore _files;
    private readonly DigitalFileLocks _locks;
    private readonly DiskUsageGuard _disk;
    private readonly TimeProvider _time;

    public DigitalFileUploads(AppDbContext db, IFileStore files, DigitalFileLocks locks, DiskUsageGuard disk, TimeProvider time)
    {
        _db = db;
        _files = files;
        _locks = locks;
        _disk = disk;
        _time = time;
    }

    public Task<UploadStartResult> StartAsync(string? fileName, long sizeBytes, CancellationToken ct)
    {
        // The order of 07 4.31.2: a name or size that is no file's first (400), then the ceiling and the disk (422).
        if (!IsValidName(fileName) || sizeBytes <= 0)
        {
            return Task.FromResult(UploadStartResult.InvalidRequest);
        }

        if (DigitalFileDelivery.CheckSize(sizeBytes).IsRejected)
        {
            return Task.FromResult(UploadStartResult.TooLarge);
        }

        if (_disk.WouldExceed(sizeBytes))
        {
            return Task.FromResult(UploadStartResult.DiskFull);
        }

        DigitalFile row = DigitalFile.Create(EntityIds.NewStorageKey(), fileName!, sizeBytes, _time.GetUtcNow()); // non-null: IsValidName refuses an empty name, refused above
        _db.DigitalFiles.Add(row);
        return Task.FromResult(UploadStartResult.Started(row.Id));
    }

    public async Task<UploadChunkResult> ReceiveAsync(Guid id, long start, Stream body, long length, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(body);
        DigitalFile? row = await _locks.LockForUpdateAsync(id, ct);
        if (row is null)
        {
            return UploadChunkResult.NotFound;
        }

        // (a) The received bytes are what the file store holds: the file is the truth, the row says nothing of progress.
        long received = await _files.GetSizeAsync(FileArea.DigitalFile, row.StorageKey, ct);

        // (c) Every byte is there but the completion was not written - a crash after the last chunk: any request completes it, appending nothing (K-1203).
        if (row.UploadCompletedAt is null && received == row.SizeBytes)
        {
            row.UploadCompletedAt = _time.GetUtcNow();
            return UploadChunkResult.Accepted(received, isCompleted: true);
        }

        // A completed upload answers 409 whatever the chunk says; the script reads the status and finds it complete.
        if (row.UploadCompletedAt is not null || start != received)
        {
            return UploadChunkResult.Mismatch(received, row.UploadCompletedAt is not null);
        }

        // (d) The chunk's length is the one the endpoint counted before the lock (the declared length, or the bytes it buffered).
        if (received + length > row.SizeBytes)
        {
            return UploadChunkResult.Overflow;
        }

        // (e) Append, and the last chunk completes the upload in this same transaction; the received bytes are read again from the file.
        await _files.AppendAsync(FileArea.DigitalFile, row.StorageKey, body, ct);
        received = await _files.GetSizeAsync(FileArea.DigitalFile, row.StorageKey, ct);
        bool completed = received == row.SizeBytes;
        if (completed)
        {
            row.UploadCompletedAt = _time.GetUtcNow();
        }

        return UploadChunkResult.Accepted(received, completed);
    }

    public async Task<UploadStatus?> StatusAsync(Guid id, CancellationToken ct)
    {
        var row = await _db.DigitalFiles.AsNoTracking()
            .Where(f => f.Id == id)
            .Select(f => new { f.StorageKey, Completed = f.UploadCompletedAt != null })
            .SingleOrDefaultAsync(ct);
        if (row is null)
        {
            return null;
        }

        return new UploadStatus(id, await _files.GetSizeAsync(FileArea.DigitalFile, row.StorageKey, ct), row.Completed);
    }

    /// <summary>The name of an upload: not empty, no path separator of either kind, no control character (07 4.31.2).</summary>
    internal static bool IsValidName(string? fileName)
    {
        return !string.IsNullOrWhiteSpace(fileName) && fileName.All(c => c is not ('/' or '\\') && !char.IsControl(c));
    }
}
