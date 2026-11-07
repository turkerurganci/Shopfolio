using System.Text;

using Microsoft.Net.Http.Headers;

namespace Shopfolio.Infrastructure.Files;

/// <summary>
/// The outcome of the size check of a digital product file (05 6.6.3; 09 8.1 item 1): within P-26, or the named refusal <see cref="TooLarge"/>.
/// Fixed instances, like the other write results; the refusal is a return value, not an exception.
/// </summary>
internal sealed class DigitalFileSizeResult : IOperationResult
{
    private DigitalFileSizeResult(string? reason)
    {
        Reason = reason;
    }

    public static DigitalFileSizeResult Within { get; } = new(null);

    public static DigitalFileSizeResult TooLarge { get; } = new("TooLarge");

    /// <summary>Why the file was refused, for the log; null when it is within the ceiling.</summary>
    public string? Reason { get; }

    public bool IsRejected => Reason is not null;
}

/// <summary>A digital product file ready to be served: the open stream and the headers that make it an attachment and nothing else.</summary>
internal sealed class DigitalFileDownload : IAsyncDisposable
{
    public DigitalFileDownload(Stream content, string fileName, long length)
    {
        Content = content;
        FileName = fileName;
        Length = length;
    }

    public Stream Content { get; }

    /// <summary>The name shown to the buyer, made safe for a header (<see cref="DigitalFileDelivery.SafeFileName"/>).</summary>
    public string FileName { get; }

    public long Length { get; }

    /// <summary>
    /// <c>attachment</c> with the file name; a name outside ASCII goes in the <c>filename*</c> form as well. The two other headers are constants:
    /// <see cref="DigitalFileDelivery.ContentType"/> (a general binary type) and <see cref="DigitalFileDelivery.NoSniff"/>.
    /// </summary>
    public string ContentDisposition => DigitalFileDelivery.DispositionFor(FileName);

    public ValueTask DisposeAsync() => Content.DisposeAsync();
}

/// <summary>
/// The delivery of a digital product file (05 6.6.3; 09 15.6 neighbours): there is no type limit - a seller may sell an e-book, software, a design
/// file or an archive - the size ceiling is P-26, and the file is always served as a download with a general binary type and the header that
/// forbids guessing the type, so it never opens in the browser and never runs on the site. The content is not scanned; it belongs to the seller
/// (02 3.21.1). The download endpoint (T312) gives the framework's file result (<c>Results.File</c>, 09 15.7 items 2 and 3) this class's type, safe name and
/// open stream: the framework writes <c>Content-Disposition</c> from the safe name, so <see cref="DigitalFileDownload.ContentDisposition"/> is the same header
/// for a test to compare and no second way to write it; the start of the chunked upload (T312) takes the size ceiling from <see cref="CheckSize"/>.
/// </summary>
internal static class DigitalFileDelivery
{
    public const string ContentType = "application/octet-stream";

    public const string NoSniff = "nosniff";

    public const string FallbackFileName = "download";

    /// <summary>The chunk of the chunked upload: 5 MB (09 3.5 table; 07 2.8.1). The one place of the number; the chunk's body ceiling reads it.</summary>
    public const long UploadChunkSizeBytes = 5L * 1024 * 1024;

    private const int FileNameMaxLength = 150;

    /// <summary>The size check against P-26; a file of exactly the ceiling is within it.</summary>
    public static DigitalFileSizeResult CheckSize(long sizeBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sizeBytes);
        return sizeBytes > ProductConstants.DigitalFileMaxBytes ? DigitalFileSizeResult.TooLarge : DigitalFileSizeResult.Within;
    }

    /// <summary>Opens the file of the key for download; null when there is no file. The name is the one the seller gave, made safe.</summary>
    public static async Task<DigitalFileDownload?> OpenAsync(IFileStore files, string key, string? fileName, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(files);
        if (!await files.ExistsAsync(FileArea.DigitalFile, key, ct))
        {
            return null;
        }

        long length = await files.GetSizeAsync(FileArea.DigitalFile, key, ct);
        Stream content = await files.OpenReadAsync(FileArea.DigitalFile, key, ct);
        return new DigitalFileDownload(content, SafeFileName(fileName), length);
    }

    /// <summary>
    /// The name a header can carry: no directory part (either separator), no control character, no quote or other character a header or a file
    /// system treats specially, no leading dot, at most 150 characters with the extension kept; an empty result is <see cref="FallbackFileName"/>.
    /// </summary>
    public static string SafeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return FallbackFileName;
        }

        string name = fileName[(fileName.LastIndexOfAny(['/', '\\']) + 1)..];
        StringBuilder builder = new(name.Length);
        foreach (char c in name)
        {
            if (char.IsControl(c) || c is '"' or ';' or ':' or '*' or '?' or '<' or '>' or '|' or '%' || char.IsSurrogate(c))
            {
                continue;
            }

            builder.Append(c);
        }

        string cleaned = builder.ToString().Trim().TrimStart('.').Trim();
        if (cleaned.Length > FileNameMaxLength)
        {
            string extension = Path.GetExtension(cleaned);
            cleaned = extension.Length is > 0 and < 20
                ? cleaned[..(FileNameMaxLength - extension.Length)] + extension
                : cleaned[..FileNameMaxLength];
        }

        return cleaned.Length == 0 ? FallbackFileName : cleaned;
    }

    internal static string DispositionFor(string safeFileName)
    {
        ContentDispositionHeaderValue header = new("attachment");
        header.SetHttpFileName(safeFileName);
        return header.ToString();
    }
}
