using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Catalog;

/// <summary>What the publish gate says (02 5.1; 03 1.8.1): the conditions that are not met, by name. No unmet condition means the gate is open.</summary>
internal sealed record PublishGateVerdict(IReadOnlyList<string> UnmetConditions)
{
    public static PublishGateVerdict Open { get; } = new([]);

    public bool IsOpen => UnmetConditions.Count == 0;
}

/// <summary>
/// The publish gate (02 5.1, 3.7.3, 3.7.4; 03 1.8.1): the conditions a product or a variant must meet to move to Published. The transition unit asks it before the move and
/// leaves the status as it was when it refuses; the removals ask the same facts to keep the gate of a published record (<see cref="DigitalFileCoverage"/>).
/// </summary>
internal interface IPublishGate
{
    /// <summary>The gate of a product's move to Published.</summary>
    Task<PublishGateVerdict> CheckProductAsync(Guid productId, CancellationToken ct);

    /// <summary>The gate of a variant's move to Published (a digital variant needs its file).</summary>
    Task<PublishGateVerdict> CheckVariantAsync(Guid variantId, CancellationToken ct);
}

/// <summary>
/// The conditions of the publish gate as pure functions (02 3.7.3, 3.7.4, 3.2.2; 11 T335 criteria 3, 4 and 10): the name, the main category and the type of the product, at
/// least one Published variant, the file of every Published variant of a digital product, and the fulfilment period of a service (P-44, through the one rule of
/// <see cref="ProductDefinitionRules"/>). The description and the image are not conditions, and nothing asks what the product is - no age, permission, blocked category or legal
/// document (10 KD-28): the set of the conditions is <see cref="PublishCondition.All"/>, closed.
/// </summary>
internal static class PublishGateRules
{
    /// <summary>The unmet conditions of a product, in the order of <see cref="PublishCondition.All"/>; empty when the gate is open.</summary>
    public static IReadOnlyList<string> UnmetOfProduct(
        string? name,
        bool hasMainCategory,
        ProductType type,
        int? fulfilmentPeriod,
        int publishedVariantCount,
        int publishedVariantsWithoutFileCount)
    {
        List<string> unmet = [];
        if (string.IsNullOrWhiteSpace(name))
        {
            unmet.Add(PublishCondition.Name);
        }

        if (!hasMainCategory)
        {
            unmet.Add(PublishCondition.MainCategory);
        }

        if (!Enum.IsDefined(type))
        {
            unmet.Add(PublishCondition.ProductType);
        }

        if (publishedVariantCount == 0)
        {
            unmet.Add(PublishCondition.PublishedVariant);
        }

        if (type == ProductType.Digital && publishedVariantsWithoutFileCount > 0)
        {
            unmet.Add(PublishCondition.DigitalFile);
        }

        if (type == ProductType.Service && !ProductDefinitionRules.IsFulfilmentPeriodEntered(fulfilmentPeriod))
        {
            unmet.Add(PublishCondition.FulfilmentPeriod);
        }

        return unmet;
    }

    /// <summary>The unmet conditions of a variant's move to Published: a digital variant has no gate without its file (02 3.7.4); the other types add nothing.</summary>
    public static IReadOnlyList<string> UnmetOfVariant(ProductType type, bool hasFile)
        => type == ProductType.Digital && !hasFile ? [PublishCondition.DigitalFile] : [];
}

/// <summary>
/// The gate of T335 (02 3.7.3, 3.7.4): it reads the product, its category links, its variants and its files and applies <see cref="PublishGateRules"/>. A plain read; it writes
/// nothing and takes no lock - the move that asks it is the transition unit's, inside its own transaction and under the product's version.
/// </summary>
internal sealed class CatalogPublishGate : IPublishGate
{
    private readonly AppDbContext _db;

    public CatalogPublishGate(AppDbContext db)
    {
        _db = db;
    }

    public async Task<PublishGateVerdict> CheckProductAsync(Guid productId, CancellationToken ct)
    {
        var product = await _db.Products.AsNoTracking()
            .Where(p => p.Id == productId)
            .Select(p => new { p.Name, p.ProductType, p.FulfilmentPeriod })
            .SingleOrDefaultAsync(ct)
            ?? throw new InvalidOperationException($"The publish gate is asked about product {productId}, which does not exist.");
        bool hasMainCategory = await _db.ProductCategories.AsNoTracking().AnyAsync(c => c.ProductId == productId && c.IsPrimaryCategory, ct);
        int published = await _db.Variants.AsNoTracking().CountAsync(v => v.ProductId == productId && v.PublishStatus == PublishStatus.Published, ct);
        int withoutFile = product.ProductType == ProductType.Digital
            ? (await DigitalFileCoverage.FindPublishedWithoutFileAsync(_db, productId, null, ct)).Count
            : 0;
        return Verdict(PublishGateRules.UnmetOfProduct(product.Name, hasMainCategory, product.ProductType, product.FulfilmentPeriod, published, withoutFile));
    }

    public async Task<PublishGateVerdict> CheckVariantAsync(Guid variantId, CancellationToken ct)
    {
        var variant = await _db.Variants.AsNoTracking()
            .Where(v => v.Id == variantId)
            .Select(v => new { v.ProductId })
            .SingleOrDefaultAsync(ct)
            ?? throw new InvalidOperationException($"The publish gate is asked about variant {variantId}, which does not exist.");
        ProductType type = await _db.Products.AsNoTracking().Where(p => p.Id == variant.ProductId).Select(p => p.ProductType).SingleAsync(ct);
        bool hasFile = type != ProductType.Digital || await DigitalFileCoverage.HasFileAsync(_db, variantId, variant.ProductId, null, ct);
        return Verdict(PublishGateRules.UnmetOfVariant(type, hasFile));
    }

    private static PublishGateVerdict Verdict(IReadOnlyList<string> unmet) => unmet.Count == 0 ? PublishGateVerdict.Open : new PublishGateVerdict(unmet);
}
