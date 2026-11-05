using System.Security.Cryptography;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// An image upload (09 15.6 items 5, 7; 05 7.7.2): the size ceiling, the processing, the source copy written to the file store under the
/// content's SHA-256 fingerprint, then - in one transaction - the unlinked <see cref="Image"/> row and the derivative job. The file comes
/// before the row, so a failure in between leaves only a file the disposal sweep removes (05 7.7.1). The same content uploaded twice rewrites
/// the same file and gets its own row.
/// </summary>
internal sealed class ImageUploads
{
    private readonly ImageProcessor _processor;
    private readonly IFileStore _files;
    private readonly ITransactionRunner _runner;
    private readonly IJobQueue _jobs;
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public ImageUploads(ImageProcessor processor, IFileStore files, ITransactionRunner runner, IJobQueue jobs, AppDbContext db, TimeProvider time)
    {
        _processor = processor;
        _files = files;
        _runner = runner;
        _jobs = jobs;
        _db = db;
        _time = time;
    }

    public async Task<ImageUploadResult> UploadAsync(byte[] content, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(content);
        if (content.LongLength > ProductConstants.ImageMaxBytes)
        {
            return ImageUploadResult.Refused(ImageUploadRefusal.TooLarge);
        }

        ImageProcessing processing = await _processor.ProcessUploadAsync(content, ct);
        if (processing.Rejection is { } rejection)
        {
            return ImageUploadResult.Refused(rejection switch
            {
                ImageRejection.UnsupportedType => ImageUploadRefusal.UnsupportedType,
                ImageRejection.OverPixelLimit => ImageUploadRefusal.OverPixelLimit,
                _ => ImageUploadRefusal.Undecodable,
            });
        }

        string key = Convert.ToHexStringLower(SHA256.HashData(processing.SourceCopy));
        using (MemoryStream source = new(processing.SourceCopy, writable: false))
        {
            await _files.WriteAsync(FileArea.Image, key, source, ct);
        }

        Image image = Image.Create(key, processing.ImageFormat, processing.Width, processing.Height, _time.GetUtcNow());
        return await _runner.RunAsync(
            async cancel =>
            {
                _db.Images.Add(image);
                await _jobs.EnqueueAsync(JobType.ImageDerivative, image.Id, notBefore: null, cancel);
                return ImageUploadResult.Uploaded(image.Id);
            },
            ct);
    }
}
