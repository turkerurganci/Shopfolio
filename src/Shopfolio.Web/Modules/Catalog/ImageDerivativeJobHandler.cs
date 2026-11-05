using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The <c>ImageDerivative</c> job (09 15.6 item 6; 05 7.7.2, 5.2.10; D-97): reads the source copy, writes a WebP derivative for every width of
/// the storefront that is smaller than the source's width, then writes <see cref="Image.DerivativesReadyAt"/>. The work repeats safely: the same
/// keys are written again with the same content and the moment is written once, so the second run changes nothing (05 4.3.3). The file work is
/// outside the transaction; only the moment is written in one.
/// </summary>
internal sealed class ImageDerivativeJobHandler : IJobHandler
{
    private readonly AppDbContext _db;
    private readonly IFileStore _files;
    private readonly ImageProcessor _processor;
    private readonly ITransactionRunner _runner;
    private readonly TimeProvider _time;

    public ImageDerivativeJobHandler(AppDbContext db, IFileStore files, ImageProcessor processor, ITransactionRunner runner, TimeProvider time)
    {
        _db = db;
        _files = files;
        _processor = processor;
        _runner = runner;
        _time = time;
    }

    public async Task<JobOutcome> HandleAsync(JobWork job, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (job.RelatedRecordId is not { } imageId)
        {
            return JobOutcome.Completed;
        }

        var image = await _db.Images.AsNoTracking().Where(i => i.Id == imageId).Select(i => new { i.StorageKey, i.Width }).SingleOrDefaultAsync(ct);
        if (image is null)
        {
            // The disposal job deleted the row: nothing left to make a derivative of.
            return JobOutcome.Completed;
        }

        IReadOnlyList<int> widths = ImageServing.DerivativeWidthsFor(image.Width);
        if (widths.Count > 0)
        {
            byte[] source;
            await using (Stream stream = await _files.OpenReadAsync(FileArea.Image, image.StorageKey, ct))
            using (MemoryStream copy = new())
            {
                await stream.CopyToAsync(copy, ct);
                source = copy.ToArray();
            }

            foreach (int width in widths)
            {
                byte[] derivative = await _processor.RenderDerivativeAsync(source, width, ct);
                using MemoryStream content = new(derivative, writable: false);
                await _files.WriteAsync(FileArea.Image, FileKeys.DerivativeKey(image.StorageKey, width), content, ct);
            }
        }

        DateTimeOffset now = _time.GetUtcNow();
        await _runner.RunAsync(
            async cancel =>
            {
                int written = await _db.Images.Where(i => i.Id == imageId && i.DerivativesReadyAt == null)
                    .ExecuteUpdateAsync(set => set.SetProperty(i => i.DerivativesReadyAt, now), cancel);
                return new DerivativeMomentWritten(written);
            },
            ct);
        return JobOutcome.Completed;
    }

    private sealed record DerivativeMomentWritten(int Written) : IOperationResult
    {
        public bool IsRejected => false;
    }
}
