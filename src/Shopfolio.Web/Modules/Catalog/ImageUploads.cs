using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// An image upload (09 15.6 items 5, 7; 05 7.7.2): the size ceiling, the processing, the source copy written to the file store under the
/// content's SHA-256 fingerprint, then - in the caller's transaction, which the method joins and never opens - the unlinked
/// <see cref="Image"/> row and the derivative job. The file comes before the row, so a failure in between leaves only a file the disposal
/// sweep removes (05 7.7.1). The same content uploaded twice rewrites the same file and gets its own row.
/// </summary>
internal sealed class ImageUploads
{
    private readonly ImageProcessor _processor;
    private readonly IFileStore _files;
    private readonly IJobQueue _jobs;
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public ImageUploads(ImageProcessor processor, IFileStore files, IJobQueue jobs, AppDbContext db, TimeProvider time)
    {
        _processor = processor;
        _files = files;
        _jobs = jobs;
        _db = db;
        _time = time;
    }

    /// <summary>Whether an uploaded image row exists (a plain read): another module that links an image (the brand's logo and site icon) asks before it writes the link.</summary>
    public Task<bool> ExistsAsync(Guid imageId, CancellationToken ct) => _db.Images.AnyAsync(i => i.Id == imageId, ct);

    public async Task<ImageUploadResult> UploadAsync(byte[] content, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (content.LongLength > ProductConstants.ImageMaxBytes)
        {
            return ImageUploadResult.TooLarge;
        }

        ImageProcessing processing = await _processor.ProcessUploadAsync(content, ct);
        if (processing.Rejection is { } rejection)
        {
            return rejection switch
            {
                ImageRejection.UnsupportedType => ImageUploadResult.UnsupportedType,
                ImageRejection.OverPixelLimit => ImageUploadResult.OverPixelLimit,
                _ => ImageUploadResult.Undecodable,
            };
        }

        string key = Convert.ToHexStringLower(SHA256.HashData(processing.SourceCopy));
        using (MemoryStream source = new(processing.SourceCopy, writable: false))
        {
            await _files.WriteAsync(FileArea.Image, key, source, ct);
        }

        ImageFormat format = processing.Kind switch
        {
            ImageKind.Jpeg => ImageFormat.Jpeg,
            ImageKind.Png => ImageFormat.Png,
            _ => ImageFormat.WebP,
        };
        // The row and the job join the caller's transaction (09 9.1 item 3, 6 item 3): the entry point opens it and saves the tracked row at its end.
        Image image = Image.Create(key, format, processing.Width, processing.Height, _time.GetUtcNow());
        _db.Images.Add(image);
        await _jobs.EnqueueAsync(JobType.ImageDerivative, image.Id, notBefore: null, ct);
        return ImageUploadResult.Uploaded(image.Id);
    }
}
