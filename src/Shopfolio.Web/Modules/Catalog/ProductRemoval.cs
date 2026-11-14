using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The permanent deletion of a product and of a variant, and the removal of a digital file (T335; 02 3.7.4, 3.7.7, 3.7.8; 03 1.8.2; 06 4.5, 4.6, 4.70). Deleting is physical and the
/// database does the rest: the variants, options, category and image links and the price history go with the product, the reservation rows go with their variant (the reserved
/// units fall with them - nothing is left to commit at payment or to give back at a cancellation, 02 3.7.7), the order items and the files lose only their link. The unit marks the
/// deleted variants' cart items first, so the carts lose them and the customer is told once (03 1.8.3) - a cart item cannot lose its variant without that mark (06 4.16). An open
/// order stops nothing: it goes on with its frozen items. The one stop is the gate of a published product - the last Published variant is not deleted (<see cref="PublishedProductGate"/>,
/// the rule the status move asks too). The product's version is compared and raised as every form record's is (<see cref="RecordSave"/>; the variant is a member of the form). The
/// file removal keeps the digital product's gate through <see cref="DigitalFileCoverage"/>, the reading the publish gate uses. The methods join the caller's transaction and do
/// not open one (09 9.1 item 3); a refusal is a return value, which the runner rolls back. Neither raises a notification nor writes a trail row: the trail's areas are closed (02 10.3.1).
/// </summary>
internal sealed class ProductRemoval : IProductRemoval
{
    private readonly AppDbContext _db;
    private readonly IOrderingModule _ordering;
    private readonly DigitalFileLocks _fileLocks;
    private readonly TimeProvider _time;
    private readonly CacheInvalidations _invalidations;
    private readonly ProductLocks _productLocks;

    public ProductRemoval(AppDbContext db, IOrderingModule ordering, DigitalFileLocks fileLocks, TimeProvider time, CacheInvalidations invalidations, ProductLocks productLocks)
    {
        _productLocks = productLocks;
        _db = db;
        _ordering = ordering;
        _fileLocks = fileLocks;
        _time = time;
        _invalidations = invalidations;
    }

    public async Task<RemovalImpact?> GetProductImpactAsync(Guid productId, CancellationToken ct)
    {
        if (!await _db.Products.AsNoTracking().AnyAsync(p => p.Id == productId, ct))
        {
            return null;
        }

        int reserved = await _db.Variants.AsNoTracking().Where(v => v.ProductId == productId).SumAsync(v => v.ReservedQuantity, ct);
        return new RemovalImpact(await _ordering.CountOpenOrdersOfProductAsync(productId, ct), reserved, false);
    }

    public async Task<RemovalImpact?> GetVariantImpactAsync(Guid variantId, CancellationToken ct)
    {
        Variant? variant = await _db.Variants.AsNoTracking().SingleOrDefaultAsync(v => v.Id == variantId, ct);
        if (variant is null)
        {
            return null;
        }

        Product product = await _db.Products.AsNoTracking().SingleAsync(p => p.Id == variant.ProductId, ct);
        return new RemovalImpact(
            await _ordering.CountOpenOrdersOfVariantAsync(variantId, ct),
            variant.ReservedQuantity,
            await PublishedProductGate.IsLastPublishedVariantAsync(_db, product, variant, ct));
    }

    public async Task<RemovalResult> DeleteProductAsync(Guid productId, int shownVersion, CancellationToken ct)
    {
        RequireTransaction();

        // The product's row lock first (D-401; 09 10.2 item 5): a publish change, a form save or a file removal of the same product waits.
        Product? product = await _productLocks.LockAsync(productId, ct) ? await _db.Products.SingleOrDefaultAsync(p => p.Id == productId, ct) : null;
        if (product is null)
        {
            return RemovalResult.NotFound;
        }

        if (product.Version != shownVersion)
        {
            return RemovalResult.VersionConflict;
        }

        Guid[] variantIds = await _db.Variants.Where(v => v.ProductId == productId).Select(v => v.Id).ToArrayAsync(ct);
        await _ordering.RemoveFromCartsAsync(variantIds, product.Name, ct);

        // The items keep the file their variant served last, written before the deletion empties their variant link (D-527; 06 4.70 "Uygulamada").
        await _ordering.AdvanceItemFilesAsync(OrderItemFiles.LastFiles(variantIds, await OrderItemFiles.ReadAsync(_db, productId, ct)), ct);
        RecordSaveResult saved = await RecordSave.SaveAsync(_db, product, shownVersion, p => _db.Products.Remove(p), ct);
        if (saved.IsRejected)
        {
            return RemovalResult.VersionConflict;
        }

        // A published product that goes leaves the showcase: the menu's categories and products part and the site map move (02 3.28.6; 05 7.10.1).
        _invalidations.Add(CacheArea.Menu);
        _invalidations.Add(CacheArea.Sitemap);
        return RemovalResult.Done;
    }

    public async Task<RemovalResult> DeleteVariantAsync(Guid variantId, int shownVersion, CancellationToken ct)
    {
        RequireTransaction();

        // The product's row lock first (D-401; 09 10.2 item 5); a variant never moves to another product, so its product is read before the lock and the variant after it.
        Guid? owner = await _db.Variants.AsNoTracking().Where(v => v.Id == variantId).Select(v => (Guid?)v.ProductId).SingleOrDefaultAsync(ct);
        Variant? variant = owner is Guid ownerId && await _productLocks.LockAsync(ownerId, ct)
            ? await _db.Variants.SingleOrDefaultAsync(v => v.Id == variantId, ct)
            : null;
        if (variant is null)
        {
            return RemovalResult.NotFound;
        }

        Product product = await _db.Products.SingleAsync(p => p.Id == variant.ProductId, ct);
        if (product.Version != shownVersion)
        {
            return RemovalResult.VersionConflict;
        }

        if (await PublishedProductGate.IsLastPublishedVariantAsync(_db, product, variant, ct))
        {
            return RemovalResult.LastPublishedVariant;
        }

        await _ordering.RemoveFromCartsAsync([variantId], product.Name, ct);
        await _ordering.AdvanceItemFilesAsync(OrderItemFiles.LastFiles([variantId], await OrderItemFiles.ReadAsync(_db, product.Id, ct)), ct);
        RecordSaveResult saved = await RecordSave.SaveAsync(_db, product, shownVersion, _ => _db.Variants.Remove(variant), ct);
        return saved.IsRejected ? RemovalResult.VersionConflict : RemovalResult.Done;
    }

    public async Task<RemovalResult> RemoveDigitalFileAsync(Guid fileId, CancellationToken ct)
    {
        RequireTransaction();

        // The owner's row lock first, then the file's (D-401; 09 10.2 item 5): a publish change or a form save of the same product waits, so the gate this removal keeps is the
        // one the move would write over. An attached file never moves to another owner (a new upload is a new row, 06 4.70), so its owner is read before the locks.
        var link = await _db.DigitalFiles.AsNoTracking().Where(f => f.Id == fileId).Select(f => new { f.ProductId, f.VariantId }).SingleOrDefaultAsync(ct);
        Guid? ownerId = link?.ProductId;
        if (ownerId is null && link?.VariantId is Guid linkedVariant)
        {
            ownerId = await _db.Variants.AsNoTracking().Where(v => v.Id == linkedVariant).Select(v => (Guid?)v.ProductId).SingleOrDefaultAsync(ct);
        }

        if (ownerId is Guid lockedOwner)
        {
            await _productLocks.LockAsync(lockedOwner, ct);
        }

        DigitalFile? file = await _fileLocks.LockForUpdateAsync(fileId, ct);
        if (file is null)
        {
            return RemovalResult.NotFound;
        }

        if (file.DetachedAt is not null)
        {
            return RemovalResult.Unchanged;
        }

        // An upload that was never attached is not a current file: the nightly clean-up takes it, this removal does not.
        if (file.AttachedAt is null)
        {
            return RemovalResult.NotFound;
        }

        Guid? productId = file.ProductId;
        if (productId is null && file.VariantId is Guid variantId)
        {
            productId = await _db.Variants.AsNoTracking().Where(v => v.Id == variantId).Select(v => (Guid?)v.ProductId).SingleOrDefaultAsync(ct);
        }

        if (productId is Guid owner)
        {
            var product = await _db.Products.AsNoTracking().Where(p => p.Id == owner).Select(p => new { p.ProductType, p.PublishStatus }).SingleOrDefaultAsync(ct);
            if (product is { ProductType: ProductType.Digital, PublishStatus: PublishStatus.Published })
            {
                IReadOnlyList<Guid> without = await DigitalFileCoverage.FindPublishedWithoutFileAsync(_db, owner, file.Id, ct);
                if (without.Count > 0)
                {
                    return RemovalResult.FileRequired(without);
                }
            }
        }

        // The items whose variant this file served move to the file they download now - the product's when a variant's own file goes - or keep it as their last (D-527; 06 4.70).
        if (productId is Guid filesOwner)
        {
            OrderItemFiles.FilePlanes before = await OrderItemFiles.ReadAsync(_db, filesOwner, ct);
            await _ordering.AdvanceItemFilesAsync(OrderItemFiles.Changes(before.Variants, before, before.Without(file.Id)), ct);
        }

        // The row stays: an order item's last known file may point to it (06 4.70). The caller's transaction saves it.
        file.DetachedAt = _time.GetUtcNow();
        return RemovalResult.Done;
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A deletion or a file removal is written inside a transaction, with its cart effect (09 9.1 item 3).");
        }
    }
}
