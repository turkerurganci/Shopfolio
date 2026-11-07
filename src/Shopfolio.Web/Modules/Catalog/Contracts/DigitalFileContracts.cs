using Shopfolio.Infrastructure;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The file an order item downloads (05 7.7.3; 02 3.12.7): the key in the file store, the name shown to the buyer, the size declared at the upload
/// and the moment the upload completed (the download's <c>Last-Modified</c>, 09 15.7 item 2). Never a row of the entity itself (09 6 item 2).
/// </summary>
public sealed record DigitalFileLocation(string StorageKey, string FileName, long SizeBytes, DateTimeOffset UploadCompletedAt);

/// <summary>What the status request of a chunked upload answers (07 4.31.2): the received bytes read from the file store, and whether the upload completed.</summary>
public sealed record UploadStatus(Guid Id, long ReceivedBytes, bool IsCompleted);

/// <summary>
/// The result of starting a chunked upload (07 4.31.2; 09 15.7 item 5): the new file row's id, or a named refusal - an invalid request (400), a size above
/// P-26 or a disk that would reach its threshold (both 422, told apart by the endpoint's body, which carries P-26's value only). The refusals are fixed
/// instances; the reason is for the log.
/// </summary>
public sealed class UploadStartResult : IOperationResult
{
    private UploadStartResult(Guid? id, string? reason)
    {
        Id = id;
        Reason = reason;
    }

    public static UploadStartResult InvalidRequest { get; } = new(null, "InvalidRequest");

    public static UploadStartResult TooLarge { get; } = new(null, "TooLarge");

    public static UploadStartResult DiskFull { get; } = new(null, "DiskFull");

    public Guid? Id { get; }

    /// <summary>Why the upload was refused; null when it started.</summary>
    public string? Reason { get; }

    public bool IsRejected => Reason is not null;

    public static UploadStartResult Started(Guid id) => new(id, null);
}

/// <summary>
/// The result of a chunk (07 4.31.2; 09 15.7 item 6): accepted with the bytes received so far and whether the upload is complete now, or refused - no such
/// upload (404), a chunk that does not start at the received byte (409, with the received bytes), a chunk that would pass the declared size (400). A refused
/// chunk wrote nothing.
/// </summary>
public sealed class UploadChunkResult : IOperationResult
{
    private UploadChunkResult(string? reason, long receivedBytes, bool isCompleted)
    {
        Reason = reason;
        ReceivedBytes = receivedBytes;
        IsCompleted = isCompleted;
    }

    public static UploadChunkResult NotFound { get; } = new("NotFound", 0, false);

    public static UploadChunkResult Overflow { get; } = new("Overflow", 0, false);

    /// <summary>The bytes received so far: of an accepted chunk and of a refused one that tells the sender where to go on (409).</summary>
    public long ReceivedBytes { get; }

    public bool IsCompleted { get; }

    /// <summary>Why the chunk was refused; null when it was accepted.</summary>
    public string? Reason { get; }

    public bool IsRejected => Reason is not null;

    public static UploadChunkResult Accepted(long receivedBytes, bool isCompleted) => new(null, receivedBytes, isCompleted);

    public static UploadChunkResult Mismatch(long receivedBytes, bool isCompleted) => new("Mismatch", receivedBytes, isCompleted);
}
