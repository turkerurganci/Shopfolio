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

    /// <summary>The Published variants of a digital product that no file serves (02 3.7.4); empty for a product of another type or one that is gone.</summary>
    public async Task<IReadOnlyList<Guid>> ListPublishedWithoutFileAsync(Guid productId, CancellationToken ct)
    {
        bool isDigital = await _db.Products.AsNoTracking().AnyAsync(p => p.Id == productId && p.ProductType == ProductType.Digital, ct);
        return isDigital ? await DigitalFileCoverage.FindPublishedWithoutFileAsync(_db, productId, null, ct) : [];
    }

    public async Task<DigitalFileLocation?> FindAsync(Guid? variantId, Guid? lastKnownFileId, CancellationToken ct)
    {
        DigitalFile? file = variantId is Guid variant ? await CurrentFileAsync(variant, ct) : null;
        if (file is null && lastKnownFileId is Guid last)
        {
            file = await _db.DigitalFiles.AsNoTracking().Where(f => f.Id == last).SingleOrDefaultAsync(ct);
        }

        return file?.UploadCompletedAt is DateTimeOffset completedAt
            ? new DigitalFileLocation(file.StorageKey, file.FileName, file.SizeBytes, completedAt)
            : null;
    }

    /// <summary>
    /// The variant's current file (02 3.12.7): the file linked to the variant and not detached, else its product's; null when there is none or the variant is gone. The file may
    /// not have completed its upload - such a file is no file to download (<see cref="FindAsync"/>) and none to freeze (T355). The one home of this rule.
    /// </summary>
    public async Task<DigitalFile?> CurrentFileAsync(Guid variantId, CancellationToken ct)
    {
        DigitalFile? file = await _db.DigitalFiles.AsNoTracking()
            .Where(f => f.VariantId == variantId && f.DetachedAt == null)
            .SingleOrDefaultAsync(ct);
        if (file is not null)
        {
            return file;
        }

        Guid? productId = await _db.Variants.AsNoTracking()
            .Where(v => v.Id == variantId)
            .Select(v => (Guid?)v.ProductId)
            .SingleOrDefaultAsync(ct);
        return productId is Guid product
            ? await _db.DigitalFiles.AsNoTracking().Where(f => f.ProductId == product && f.DetachedAt == null).SingleOrDefaultAsync(ct)
            : null;
    }
}
