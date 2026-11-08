using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Categories;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>
/// What the menu is built from and the cache holds (05 7.10.1; 09 10.4 item 1): which content types have a published record, the catalog's menu part and the published general pages.
/// An immutable record with no name and no layout in it - those are the firm's settings and are read on every call - so a change of the home page section shows at once whatever
/// the cache holds. Dropped when a content record changes (<see cref="ContentPublishing"/>), when a category changes, and when a product enters or leaves the showcase or changes its
/// categories (the Catalog module's writes).
/// </summary>
internal sealed record MenuStructure(
    bool HasOfferings,
    bool HasPortfolioItems,
    bool HasAboutPage,
    bool HasFaqItems,
    CatalogMenu Catalog,
    IReadOnlyList<GeneralPageLink> Pages);

/// <summary>
/// The storefront's menu (T361; 02 3.28.4-3.28.6; 04 2.2.3; 10 KP-55, KD-39): the fixed skeleton - "Ürünler", the content types and İletişim - with the general pages the firm shows in
/// the menu, in the order the home page layout gives. There is no menu editor: nothing is stored about the menu but the six names and the layout in the company settings and the
/// "show in menu" mark of a general page. A content type with no published record is hidden - a type that has only drafts and an about page that is not published included
/// (02 3.28.4); the products part is hidden when the catalog has no published product, and a category with no published product anywhere below it is not listed (02 3.28.6). The names
/// are the firm's; İletişim is always there. The structure is cached in the menu area; the names and the layout are read from the settings on every call.
/// </summary>
internal sealed class StorefrontMenuReader
{
    private readonly AppDbContext _db;
    private readonly AppCache _cache;
    private readonly ICatalogModule _catalog;
    private readonly ISettingsModule _settings;

    public StorefrontMenuReader(AppDbContext db, AppCache cache, ICatalogModule catalog, ISettingsModule settings)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(cache);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(settings);
        _db = db;
        _cache = cache;
        _catalog = catalog;
        _settings = settings;
    }

    public async Task<StorefrontMenu> GetAsync(CancellationToken ct)
    {
        HomepageSection section = await _settings.GetHomepageAsync(ct);
        MenuStructure structure = await GetStructureAsync(ct);
        return Compose(section, structure);
    }

    /// <summary>The cached structure; the home page's category links read it too.</summary>
    public Task<MenuStructure> GetStructureAsync(CancellationToken ct) => _cache.GetOrLoadAsync(CacheArea.Menu, LoadAsync, ct);

    /// <summary>The menu from the settings and the structure; no read.</summary>
    internal static StorefrontMenu Compose(HomepageSection section, MenuStructure structure)
    {
        ArgumentNullException.ThrowIfNull(section);
        ArgumentNullException.ThrowIfNull(structure);
        MenuNames names = section.MenuNames;

        MenuItem products = structure.Catalog.HasListedProducts
            ? new MenuItem(MenuItemKind.Products, names.Products, null, true, null)
            : new MenuItem(MenuItemKind.Products, names.Products, null, false, MenuHiddenReason.NoPublishedProduct);
        MenuItem offerings = TypeItem(MenuItemKind.Offerings, names.Offerings, "/" + AddressRules.ServiceListPage, structure.HasOfferings);
        MenuItem portfolio = TypeItem(MenuItemKind.PortfolioItems, names.PortfolioItems, "/" + AddressRules.ReferenceListPage, structure.HasPortfolioItems);
        MenuItem about = TypeItem(MenuItemKind.AboutPage, names.AboutPage, "/" + AddressRules.AboutPage, structure.HasAboutPage);
        MenuItem faq = TypeItem(MenuItemKind.FaqItems, names.FaqItems, "/" + AddressRules.FaqPage, structure.HasFaqItems);
        MenuItem contact = new(MenuItemKind.Contact, names.Contact, "/" + AddressRules.ContactPage, true, null);

        // The menu follows the layout (04 2.2.3): store-first puts "Ürünler" before the content types; content-first puts the content types first, the about page at their head.
        List<MenuItem> items = section.Layout == HomepageLayout.StoreFirst
            ? [products, offerings, portfolio, about, faq]
            : [about, offerings, portfolio, faq, products];
        items.AddRange(structure.Pages.Where(p => p.IsInMenu).Select(p => new MenuItem(MenuItemKind.GeneralPage, p.Title, p.Path, true, null)));
        items.Add(contact);

        return new StorefrontMenu(section.Layout, items, products.IsVisible ? structure.Catalog.Categories : [], structure.Pages);
    }

    private static MenuItem TypeItem(MenuItemKind kind, string name, string path, bool hasPublishedRecord) =>
        new(kind, name, path, hasPublishedRecord, hasPublishedRecord ? null : MenuHiddenReason.NoPublishedRecord);

    private async Task<MenuStructure> LoadAsync(CancellationToken ct)
    {
        bool offerings = await _db.Offerings.AsNoTracking().AnyAsync(o => o.PublishStatus == PublishStatus.Published, ct);
        bool portfolio = await _db.PortfolioItems.AsNoTracking().AnyAsync(p => p.PublishStatus == PublishStatus.Published, ct);
        bool about = await _db.AboutPages.AsNoTracking().AnyAsync(a => a.PublishStatus == PublishStatus.Published, ct);
        bool faq = await _db.FaqItems.AsNoTracking().AnyAsync(f => f.PublishStatus == PublishStatus.Published, ct);
        var pages = await _db.CustomPages.AsNoTracking()
            .Where(c => c.PublishStatus == PublishStatus.Published)
            .OrderBy(c => c.SortOrder).ThenBy(c => c.CreatedAt).ThenBy(c => c.Id)
            .Select(c => new { c.Id, c.Title, c.Slug, c.IsShownInMenu })
            .ToListAsync(ct);
        CatalogMenu catalog = await _catalog.GetMenuAsync(ct);
        return new MenuStructure(
            offerings,
            portfolio,
            about,
            faq,
            catalog,
            [.. pages.Select(p => new GeneralPageLink(p.Id, p.Title, AddressRules.RecordAddress(AddressRules.GeneralPagePrefix, p.Slug), p.IsShownInMenu))]);
    }
}
