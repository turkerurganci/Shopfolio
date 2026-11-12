using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The publish status transitions of a product and of a variant (T321; 03 1.8; 02 3.7.1, 5.1). The two carry the same three values apart from each other, and every
/// move between them is free and the administrator's panel operation: there is no white list to keep, because no move is forbidden by itself. The limits are the publish
/// gate - a move to Published asks it, a refusal leaves the status as it was - and the gate of a published product, which a move must not break: its last Published
/// variant does not leave Published (03 1.8.2). The order of the work is the one of 09 9.1 item 4 where it applies: the record is loaded and its version compared, the
/// condition is checked, the status is written (the product's version rises: the variant is a member of the product's form, 06 2.6.2), then the inner side effects - the
/// carts, which lose the items that left the showcase (03 1.8.3) - and the trail row. It raises no notification (03 1.8.1) and writes no job. The unit joins the
/// caller's transaction and does not open one (09 9.1 item 3); a refusal is a return value, which the runner rolls back.
/// </summary>
internal sealed class ProductTransitions : IProductTransitions
{
    private const string StatusField = nameof(Product.PublishStatus);

    private readonly AppDbContext _db;
    private readonly IPublishGate _gate;
    private readonly IOrderingModule _ordering;
    private readonly IAuditTrail _trail;
    private readonly TimeProvider _time;
    private readonly CacheInvalidations _invalidations;

    public ProductTransitions(AppDbContext db, IPublishGate gate, IOrderingModule ordering, IAuditTrail trail, TimeProvider time, CacheInvalidations invalidations)
    {
        _db = db;
        _gate = gate;
        _ordering = ordering;
        _trail = trail;
        _time = time;
        _invalidations = invalidations;
    }

    public async Task<PublishChangeResult> ChangeProductStatusAsync(Guid productId, PublishStatus target, int shownVersion, CancellationToken ct)
    {
        RequireTransaction();
        RequireDefined(target);

        Product? product = await _db.Products.SingleOrDefaultAsync(p => p.Id == productId, ct);
        if (product is null)
        {
            return PublishChangeResult.NotFound;
        }

        if (product.Version != shownVersion)
        {
            return PublishChangeResult.VersionConflict;
        }

        PublishStatus from = product.PublishStatus;
        if (from == target)
        {
            return PublishChangeResult.Unchanged;
        }

        if (target == PublishStatus.Published && await AskGateAsync(_gate.CheckProductAsync(productId, ct)) is { } refusal)
        {
            return refusal;
        }

        List<Variant> variants = await _db.Variants.Where(v => v.ProductId == productId).ToListAsync(ct);
        DateTimeOffset now = _time.GetUtcNow();
        RecordSaveResult saved = await RecordSave.SaveAsync(_db, product, shownVersion, p =>
        {
            p.ApplyPublishStatus(target, now);
            if (target == PublishStatus.Published)
            {
                // The variant's first sale moment is the first moment it and its product are Published together (06 4.6).
                foreach (Variant variant in variants.Where(v => v.PublishStatus == PublishStatus.Published))
                {
                    variant.MarkFirstOnSale(now);
                }
            }
        }, ct);
        if (saved.IsRejected)
        {
            return PublishChangeResult.VersionConflict;
        }

        // A product that enters or leaves the showcase moves the menu's categories and products part and the site map (02 3.28.6; 05 7.10.1).
        _invalidations.Add(CacheArea.Menu);
        _invalidations.Add(CacheArea.Sitemap);

        if (from == PublishStatus.Published)
        {
            await _ordering.RemoveFromCartsAsync([.. variants.Select(v => v.Id)], product.Name, ct);
        }

        await _trail.WriteAsync(
            new AuditEntry(AuditOperation.ChangePublishStatus, nameof(Product), product.Id, DetailsOf(product.Name, from, target)),
            ct);
        return PublishChangeResult.Changed;
    }

    public async Task<PublishChangeResult> ChangeVariantStatusAsync(Guid variantId, PublishStatus target, int shownVersion, CancellationToken ct)
    {
        RequireTransaction();
        RequireDefined(target);

        Variant? variant = await _db.Variants.SingleOrDefaultAsync(v => v.Id == variantId, ct);
        if (variant is null)
        {
            return PublishChangeResult.NotFound;
        }

        Product product = await _db.Products.SingleAsync(p => p.Id == variant.ProductId, ct);
        if (product.Version != shownVersion)
        {
            return PublishChangeResult.VersionConflict;
        }

        PublishStatus from = variant.PublishStatus;
        if (from == target)
        {
            return PublishChangeResult.Unchanged;
        }

        // The variant's publish keeps the gate only under a published product (07 UC-115; 02 3.7.5): under a draft or archived product the move is free and the product's own move asks
        // every Published variant's file later. Under a published product a fileless digital variant and a variant whose price is zero are refused (11 T525 criterion 8; 02 3.7.3, 3.7.4).
        if (target == PublishStatus.Published && product.PublishStatus == PublishStatus.Published)
        {
            if (await AskGateAsync(_gate.CheckVariantAsync(variantId, ct)) is { } refusal)
            {
                return refusal;
            }

            if (!PublishGateRules.HasSalePrice(variant.Price))
            {
                return PublishChangeResult.ZeroPriceVariant;
            }
        }

        // The gate of a published product is kept while it is published too (03 1.8.2): its last Published variant stays.
        if (await PublishedProductGate.IsLastPublishedVariantAsync(_db, product, variant, ct))
        {
            return PublishChangeResult.LastPublishedVariant;
        }

        DateTimeOffset now = _time.GetUtcNow();
        RecordSaveResult saved = await RecordSave.SaveAsync(_db, product, shownVersion, _ =>
        {
            variant.ApplyPublishStatus(target);
            if (target == PublishStatus.Published && product.PublishStatus == PublishStatus.Published)
            {
                variant.MarkFirstOnSale(now);
            }
        }, ct);
        if (saved.IsRejected)
        {
            return PublishChangeResult.VersionConflict;
        }

        if (from == PublishStatus.Published)
        {
            await _ordering.RemoveFromCartsAsync([variant.Id], product.Name, ct);
        }

        await _trail.WriteAsync(
            new AuditEntry(AuditOperation.ChangePublishStatus, nameof(Variant), variant.Id, DetailsOf($"{product.Name} / {variant.SKU}", from, target)),
            ct);
        return PublishChangeResult.Changed;
    }

    private static async Task<PublishChangeResult?> AskGateAsync(Task<PublishGateVerdict> question)
    {
        PublishGateVerdict verdict = await question;
        return verdict.IsOpen ? null : PublishChangeResult.GateIncomplete(verdict.UnmetConditions);
    }

    private static AuditDetails DetailsOf(string displayName, PublishStatus from, PublishStatus to)
        => new(displayName, [new AuditValueChange(StatusField, from.ToString(), to.ToString())]);

    private static void RequireDefined(PublishStatus target)
    {
        if (!Enum.IsDefined(target))
        {
            throw new ArgumentOutOfRangeException(nameof(target), target, "Not a publish status.");
        }
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A publish status move is written inside a transaction, with its trail row and its cart effect (09 9.1 item 3).");
        }
    }
}
