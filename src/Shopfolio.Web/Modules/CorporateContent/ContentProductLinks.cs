using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>The two content records that link products from the catalog (02 3.27.10): the service introduction and the reference work.</summary>
internal enum ContentLinkOwner
{
    Offering,
    PortfolioItem,
}

/// <summary>What became of a product link save (T359).</summary>
internal enum ProductLinksOutcome
{
    Saved,

    /// <summary>The service introduction or reference work does not exist.</summary>
    OwnerNotFound,

    /// <summary>The list names one product twice; nothing was written (a product is linked to a record once, 06 4.53).</summary>
    DuplicateProduct,
}

/// <summary>The result of a product link save; a refusal is a return value and the transaction runner rolls back (09 9.1 item 3).</summary>
internal sealed class ProductLinksResult : IOperationResult
{
    private ProductLinksResult(ProductLinksOutcome outcome)
    {
        Outcome = outcome;
    }

    public static ProductLinksResult Saved { get; } = new(ProductLinksOutcome.Saved);

    public static ProductLinksResult OwnerNotFound { get; } = new(ProductLinksOutcome.OwnerNotFound);

    public static ProductLinksResult DuplicateProduct { get; } = new(ProductLinksOutcome.DuplicateProduct);

    public ProductLinksOutcome Outcome { get; }

    public bool IsRejected => Outcome != ProductLinksOutcome.Saved;
}

/// <summary>
/// The products a service introduction or a reference work links from the catalog (T359; 02 3.27.10, 6.8.4; 06 4.53; 10 KD-8, KD-34): a link is a relation the firm sets by hand, at the product
/// level - it carries no variant - and its place in the list is the order of the choice. The page shows the linked products under "İlgili ürünler" as storefront cards, and only the published
/// ones: a product put back to draft or archived stays linked and is not shown, and comes back when it is published again; a deleted product's link is gone with it (the foreign key
/// cascades, 06 2.2.5). The link is one-way: a product has no reverse list of the records it appears in, and nothing is suggested from what visitors do. The save joins the caller's
/// transaction and never saves (09 9.1 item 3); the read is a plain read.
/// </summary>
internal sealed class ContentProductLinks
{
    private readonly AppDbContext _db;
    private readonly ICatalogModule _catalog;
    private readonly TimeProvider _time;

    public ContentProductLinks(AppDbContext db, ICatalogModule catalog, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _catalog = catalog;
        _time = time;
    }

    /// <summary>
    /// Makes the links of the record match the list (the position in the list is the order): the products not in it are unlinked, the new ones are linked, the kept ones take their new place.
    /// A product of any status can be linked - a draft is kept and not shown. An id that is no product (deleted meanwhile) is skipped and the rest are linked in the order of the choice; a product named twice refuses the whole list and writes nothing.
    /// </summary>
    public async Task<ProductLinksResult> SaveAsync(ContentLinkOwner owner, Guid ownerId, IReadOnlyList<Guid> productIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(productIds);
        if (!await OwnerExistsAsync(owner, ownerId, ct))
        {
            return ProductLinksResult.OwnerNotFound;
        }

        if (productIds.Distinct().Count() != productIds.Count)
        {
            return ProductLinksResult.DuplicateProduct;
        }

        // A product that no longer exists (deleted since the form was opened) is skipped, not an error; the rest keep the order of the choice (06 4.53; 07 9.13.2).
        HashSet<Guid> existing = [.. await _catalog.FindProductIdsAsync(productIds, ct)];
        List<Guid> kept = [.. productIds.Where(existing.Contains)];

        DateTimeOffset now = _time.GetUtcNow();
        if (owner == ContentLinkOwner.Offering)
        {
            List<OfferingProduct> links = await _db.OfferingProducts.Where(l => l.OfferingId == ownerId).ToListAsync(ct);
            _db.OfferingProducts.RemoveRange(links.Where(l => !kept.Contains(l.ProductId)));
            for (int i = 0; i < kept.Count; i++)
            {
                OfferingProduct? link = links.FirstOrDefault(l => l.ProductId == kept[i]);
                if (link is null)
                {
                    _db.OfferingProducts.Add(OfferingProduct.Create(ownerId, kept[i], i + 1, now));
                }
                else
                {
                    link.SortOrder = i + 1;
                }
            }
        }
        else
        {
            List<PortfolioItemProduct> links = await _db.PortfolioItemProducts.Where(l => l.PortfolioItemId == ownerId).ToListAsync(ct);
            _db.PortfolioItemProducts.RemoveRange(links.Where(l => !kept.Contains(l.ProductId)));
            for (int i = 0; i < kept.Count; i++)
            {
                PortfolioItemProduct? link = links.FirstOrDefault(l => l.ProductId == kept[i]);
                if (link is null)
                {
                    _db.PortfolioItemProducts.Add(PortfolioItemProduct.Create(ownerId, kept[i], i + 1, now));
                }
                else
                {
                    link.SortOrder = i + 1;
                }
            }
        }

        return ProductLinksResult.Saved;
    }

    /// <summary>The linked products of the record in the order of the choice, whatever their status - what the panel's form shows, drafts included.</summary>
    public async Task<IReadOnlyList<Guid>> ListLinkedIdsAsync(ContentLinkOwner owner, Guid ownerId, CancellationToken ct)
    {
        return owner == ContentLinkOwner.Offering
            ? await _db.OfferingProducts.AsNoTracking()
                .Where(l => l.OfferingId == ownerId)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.CreatedAt).ThenBy(l => l.Id)
                .Select(l => l.ProductId)
                .ToListAsync(ct)
            : await _db.PortfolioItemProducts.AsNoTracking()
                .Where(l => l.PortfolioItemId == ownerId)
                .OrderBy(l => l.SortOrder).ThenBy(l => l.CreatedAt).ThenBy(l => l.Id)
                .Select(l => l.ProductId)
                .ToListAsync(ct);
    }

    /// <summary>The "İlgili ürünler" of the record's page: the cards of the linked products that are published, in the order of the choice. Empty when none is linked or none is published.</summary>
    public async Task<IReadOnlyList<ShowcaseProduct>> ListRelatedAsync(ContentLinkOwner owner, Guid ownerId, CancellationToken ct)
    {
        IReadOnlyList<Guid> linked = await ListLinkedIdsAsync(owner, ownerId, ct);
        if (linked.Count == 0)
        {
            return [];
        }

        IReadOnlyList<ShowcaseProduct> cards = await _catalog.ListPublishedProductsAsync(linked, ct);
        Dictionary<Guid, ShowcaseProduct> byId = cards.ToDictionary(c => c.Id);
        return [.. linked.Where(byId.ContainsKey).Select(id => byId[id])];
    }

    private Task<bool> OwnerExistsAsync(ContentLinkOwner owner, Guid ownerId, CancellationToken ct) =>
        owner == ContentLinkOwner.Offering ? _db.Offerings.AnyAsync(o => o.Id == ownerId, ct) : _db.PortfolioItems.AnyAsync(p => p.Id == ownerId, ct);
}
