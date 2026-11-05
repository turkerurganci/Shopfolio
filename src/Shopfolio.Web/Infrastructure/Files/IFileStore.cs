namespace Shopfolio.Infrastructure.Files;

/// <summary>The two sections of the file area (09 6 item 6; 05 7.7.1): images are public, digital product files never are.</summary>
internal enum FileArea
{
    Image,
    DigitalFile,
}

/// <summary>
/// The application's only way to the files (09 6 item 6; 05 7.7.1; MK-28). The one implementation is the disk (<see cref="DiskFileStore"/>); the
/// object storage move changes only the implementation. The application makes the key; a key that does not fit its section's shape is refused
/// with an <see cref="ArgumentException"/> (see <see cref="FileKeys"/>). A write never shows a half file.
/// </summary>
internal interface IFileStore
{
    /// <summary>Writes the content to a temporary file of the same volume and names it when complete; an existing file of the key is replaced.</summary>
    Task WriteAsync(FileArea area, string key, Stream content, CancellationToken ct);

    /// <summary>Appends the content to the file (the chunked upload, 05 7.7.3); the digital file section only. The file is made when it is not there.</summary>
    Task AppendAsync(FileArea area, string key, Stream content, CancellationToken ct);

    /// <summary>Opens the file for reading; the stream is seekable, so a range can be served.</summary>
    Task<Stream> OpenReadAsync(FileArea area, string key, CancellationToken ct);

    Task<bool> ExistsAsync(FileArea area, string key, CancellationToken ct);

    /// <summary>The size in bytes; zero when there is no file.</summary>
    Task<long> GetSizeAsync(FileArea area, string key, CancellationToken ct);

    /// <summary>Deletes the file; a file that is not there is not an error.</summary>
    Task DeleteAsync(FileArea area, string key, CancellationToken ct);

    /// <summary>The keys of the files at least <paramref name="minAge"/> old, for the disposal sweep (05 7.4.4). Temporary files are not listed.</summary>
    IAsyncEnumerable<string> ListAsync(FileArea area, TimeSpan minAge, CancellationToken ct);
}
