using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one place of the gate a published product keeps while it is published (02 3.7.1, 3.7.8; 03 1.8.2): its last Published variant stays - it is not archived, not taken to
/// draft and not deleted. The status move and the deletion of a variant both ask it, so the three ways of losing the last variant are one rule and one refusal
/// (<c>LastPublishedVariant</c>, 07 6.2). A plain read; it writes nothing.
/// </summary>
internal static class PublishedProductGate
{
    /// <summary>True when <paramref name="variant"/> is a Published variant of a Published product and no other variant of the product is Published.</summary>
    public static async Task<bool> IsLastPublishedVariantAsync(AppDbContext db, Product product, Variant variant, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(product);
        ArgumentNullException.ThrowIfNull(variant);
        return variant.PublishStatus == PublishStatus.Published
            && product.PublishStatus == PublishStatus.Published
            && !await db.Variants.AnyAsync(v => v.ProductId == product.Id && v.Id != variant.Id && v.PublishStatus == PublishStatus.Published, ct);
    }
}
