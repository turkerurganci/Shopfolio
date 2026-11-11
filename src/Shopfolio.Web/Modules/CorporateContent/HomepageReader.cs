using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>The numbers of the home page's blocks (04 5.1.5, 5.1.6; K-755), written here once: how many cards a content block shows and how many products the showcase shows in each layout.</summary>
internal static class HomepageRules
{
    /// <summary>The service and the reference block show at most three records, the first three of the firm's order among the flagged ones (04 5.1.5).</summary>
    public const int CardsPerBlock = 3;

    /// <summary>The showcase of the content-first layout is one row: at most four products (04 5.1.6).</summary>
    public const int ShowcaseProductsContentFirst = 4;

    /// <summary>The showcase of the store-first layout is two rows: at most eight products (04 5.1.6).</summary>
    public const int ShowcaseProductsStoreFirst = 8;

    /// <summary>How many showcase products the layout shows.</summary>
    public static int ShowcaseProducts(HomepageLayout layout) => layout == HomepageLayout.StoreFirst ? ShowcaseProductsStoreFirst : ShowcaseProductsContentFirst;
}

/// <summary>
/// The corporate part of the home page the cache holds (05 7.10.1; 09 10.4 item 1): the about page's block and the cards of the two content blocks. An immutable record with no title
/// and no layout in it. Dropped when a content record changes (<see cref="ContentPublishing"/>).
/// </summary>
internal sealed record HomepageContent(
    HomepageIntro? Intro,
    IReadOnlyList<HomepageCard> Offerings,
    bool OfferingsHaveMore,
    IReadOnlyList<HomepageCard> PortfolioItems,
    bool PortfolioItemsHaveMore);

/// <summary>
/// The home page (T361; 02 3.28.1-3.28.3; 04 5.1; 10 KP-31, KD-40): two ready-made layouts that differ only in the order and the weight of the same blocks. The corporate block is the
/// about page's short introduction and main image; the service and reference blocks show the published records the firm flagged "Ana sayfada göster", in its manual order, at most
/// three, and are not there when none is flagged; the showcase is the newest published products - nobody picks them and a product has no home page mark - and is not there when no
/// product is published. A draft is never on the page. There is no banner, no campaign picture and no page builder. The corporate part is cached in the home page blocks area; the
/// layout, the block titles and the showcase - whose sold-out mark follows the stock - are read on every call.
/// </summary>
internal sealed class HomepageReader
{
    private readonly AppDbContext _db;
    private readonly AppCache _cache;
    private readonly ICatalogModule _catalog;
    private readonly ISettingsModule _settings;
    private readonly StorefrontMenuReader _menu;

    public HomepageReader(AppDbContext db, AppCache cache, ICatalogModule catalog, ISettingsModule settings, StorefrontMenuReader menu)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(menu);
        _db = db;
        _cache = cache;
        _catalog = catalog;
        _settings = settings;
        _menu = menu;
    }

    public async Task<HomepageView> GetAsync(CancellationToken ct)
    {
        HomepageSection section = await _settings.GetHomepageAsync(ct);
        HomepageContent content = await _cache.GetOrLoadAsync(CacheArea.HomepageBlocks, LoadAsync, ct);

        // The showcase's cards are the list cards (D-1178): price, running discount, sold-out mark and main image come with the product.
        IReadOnlyList<ProductListItem> newest = await _catalog.ListNewestProductsAsync(HomepageRules.ShowcaseProducts(section.Layout), ct);
        HomepageShowcase? showcase = null;
        if (newest.Count > 0)
        {
            MenuStructure structure = await _menu.GetStructureAsync(ct);
            showcase = new HomepageShowcase(
                section.MenuNames.Products,
                newest,
                [.. structure.Catalog.Categories.Where(c => c.Level == 1)]);
        }

        HomepageCardBlock? offerings = content.Offerings.Count == 0
            ? null
            : new HomepageCardBlock(section.MenuNames.Offerings, "/" + AddressRules.ServiceListPage, content.Offerings, content.OfferingsHaveMore);
        HomepageCardBlock? portfolio = content.PortfolioItems.Count == 0
            ? null
            : new HomepageCardBlock(section.MenuNames.PortfolioItems, "/" + AddressRules.ReferenceListPage, content.PortfolioItems, content.PortfolioItemsHaveMore);

        return new HomepageView(section.Layout, OrderOf(section.Layout, offerings is not null, portfolio is not null, showcase is not null), content.Intro, offerings, portfolio, showcase);
    }

    /// <summary>The blocks that show, in the order of the layout (04 5.1.3, 5.1.4); the corporate block is always among them.</summary>
    internal static IReadOnlyList<HomepageBlockKind> OrderOf(HomepageLayout layout, bool hasOfferings, bool hasPortfolioItems, bool hasShowcase)
    {
        List<HomepageBlockKind> blocks = [];
        if (layout == HomepageLayout.StoreFirst && hasShowcase)
        {
            blocks.Add(HomepageBlockKind.ProductShowcase);
        }

        blocks.Add(HomepageBlockKind.CorporateIntro);
        if (hasOfferings)
        {
            blocks.Add(HomepageBlockKind.Offerings);
        }

        if (hasPortfolioItems)
        {
            blocks.Add(HomepageBlockKind.PortfolioItems);
        }

        if (layout != HomepageLayout.StoreFirst && hasShowcase)
        {
            blocks.Add(HomepageBlockKind.ProductShowcase);
        }

        return blocks;
    }

    private async Task<HomepageContent> LoadAsync(CancellationToken ct)
    {
        HomepageIntro? intro = await LoadIntroAsync(ct);

        var offeringRows = await _db.Offerings.AsNoTracking()
            .Where(o => o.PublishStatus == PublishStatus.Published && o.IsShownOnHomepage)
            .OrderBy(o => o.SortOrder).ThenBy(o => o.CreatedAt).ThenBy(o => o.Id)
            .Take(HomepageRules.CardsPerBlock + 1)
            .Select(o => new { o.Id, o.Name, o.ShortDescription, o.Slug })
            .ToListAsync(ct);
        Guid[] offeringIds = [.. offeringRows.Select(o => o.Id)];
        Dictionary<Guid, ContentImage> offeringImages = ContentMainImages.Of(
            await _db.OfferingImages.AsNoTracking()
                .Where(i => offeringIds.Contains(i.OfferingId))
                .Select(i => new ContentGalleryRow(i.OfferingId, i.ImageId, i.IsMain, i.SortOrder, i.CreatedAt, i.AltText))
                .ToListAsync(ct));

        var portfolioRows = await _db.PortfolioItems.AsNoTracking()
            .Where(p => p.PublishStatus == PublishStatus.Published && p.IsShownOnHomepage)
            .OrderBy(p => p.SortOrder).ThenBy(p => p.CreatedAt).ThenBy(p => p.Id)
            .Take(HomepageRules.CardsPerBlock + 1)
            .Select(p => new { p.Id, p.Title, p.ShortDescription, p.Slug })
            .ToListAsync(ct);
        Guid[] portfolioIds = [.. portfolioRows.Select(p => p.Id)];
        Dictionary<Guid, ContentImage> portfolioImages = ContentMainImages.Of(
            await _db.PortfolioItemImages.AsNoTracking()
                .Where(i => portfolioIds.Contains(i.PortfolioItemId))
                .Select(i => new ContentGalleryRow(i.PortfolioItemId, i.ImageId, i.IsMain, i.SortOrder, i.CreatedAt, i.AltText))
                .ToListAsync(ct));

        return new HomepageContent(
            intro,
            [.. offeringRows.Take(HomepageRules.CardsPerBlock).Select(o => new HomepageCard(
                o.Id, o.Name, o.ShortDescription, AddressRules.RecordAddress(AddressRules.ServicePrefix, o.Slug), offeringImages.GetValueOrDefault(o.Id)))],
            offeringRows.Count > HomepageRules.CardsPerBlock,
            [.. portfolioRows.Take(HomepageRules.CardsPerBlock).Select(p => new HomepageCard(
                p.Id, p.Title, p.ShortDescription, AddressRules.RecordAddress(AddressRules.ReferencePrefix, p.Slug), portfolioImages.GetValueOrDefault(p.Id)))],
            portfolioRows.Count > HomepageRules.CardsPerBlock);
    }

    /// <summary>The corporate block: the published about page with its short introduction and main image; null when the page is not published or its short introduction is empty.</summary>
    private async Task<HomepageIntro?> LoadIntroAsync(CancellationToken ct)
    {
        var about = await _db.AboutPages.AsNoTracking()
            .Where(a => a.PublishStatus == PublishStatus.Published)
            .Select(a => new { a.Id, a.ShortIntroduction })
            .SingleOrDefaultAsync(ct);
        if (about is null || string.IsNullOrWhiteSpace(about.ShortIntroduction))
        {
            return null;
        }

        Dictionary<Guid, ContentImage> images = ContentMainImages.Of(
            await _db.AboutPageImages.AsNoTracking()
                .Where(i => i.AboutPageId == about.Id)
                .Select(i => new ContentGalleryRow(i.AboutPageId, i.ImageId, i.IsMain, i.SortOrder, i.CreatedAt, i.AltText))
                .ToListAsync(ct));
        return new HomepageIntro(about.ShortIntroduction, "/" + AddressRules.AboutPage, images.GetValueOrDefault(about.Id));
    }
}
