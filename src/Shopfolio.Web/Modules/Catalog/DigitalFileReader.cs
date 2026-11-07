using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The file an order item downloads (05 7.7.3; 02 3.12.7; 06 4.70, 4.18): the variant's current file is the one linked to it and not detached, else its
/// product's current file; when the variant is gone (an item's variant link is emptied by the database) the item's last known file link answers - a variant
/// that stands but has no current file also falls back to it, so a buyer keeps the last file. A file whose upload did not complete is no file. The read is
/// untracked and writes nothing: the right to download dropped at the order page's button (05 7.7.4).
/// </summary>
internal sealed class DigitalFileReader
{
    private readonly AppDbContext _db;

    public DigitalFileReader(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DigitalFileLocation?> FindAsync(Guid? variantId, Guid? lastKnownFileId, CancellationToken ct)
    {
        DigitalFile? file = null;
        if (variantId is Guid variant)
        {
            file = await _db.DigitalFiles.AsNoTracking()
                .Where(f => f.VariantId == variant && f.DetachedAt == null)
                .SingleOrDefaultAsync(ct);
            if (file is null)
            {
                Guid? productId = await _db.Variants.AsNoTracking()
                    .Where(v => v.Id == variant)
                    .Select(v => (Guid?)v.ProductId)
                    .SingleOrDefaultAsync(ct);
                if (productId is Guid product)
                {
                    file = await _db.DigitalFiles.AsNoTracking()
                        .Where(f => f.ProductId == product && f.DetachedAt == null)
                        .SingleOrDefaultAsync(ct);
                }
            }
        }

        if (file is null && lastKnownFileId is Guid last)
        {
            file = await _db.DigitalFiles.AsNoTracking().Where(f => f.Id == last).SingleOrDefaultAsync(ct);
        }

        return file?.UploadCompletedAt is DateTimeOffset completedAt
            ? new DigitalFileLocation(file.StorageKey, file.FileName, file.SizeBytes, completedAt)
            : null;
    }
}
