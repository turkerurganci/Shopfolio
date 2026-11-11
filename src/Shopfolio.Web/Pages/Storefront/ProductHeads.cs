using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// What the product page and the search page tell the search engines (05 3.7.2, 3.7.4; 02 3.30.5, 3.30.6), made from the record's values by the one pattern of <see cref="RecordPages"/> and
/// <see cref="SearchEngineData"/>. The firm types none of it. A published product's page without a selection parameter is open and carries the product's structured data - price and stock state - in the
/// first response; a selection address, the preview and the archived page are closed (K-1178); the search page is closed always (07 2.6.7).
/// </summary>
internal static class ProductHeads
{
    /// <summary>The head data of the product page (D-691, D-692): the image is the page's main image and falls back to the logo.</summary>
    public static PageSearchData Product(SearchEngineData search, StorefrontFrame frame, ProductView view, bool closed)
    {
        ArgumentNullException.ThrowIfNull(search);
        ArgumentNullException.ThrowIfNull(frame);
        ArgumentNullException.ThrowIfNull(view);

        string? logoPath = frame.Brand.LogoId is Guid logoId ? FrameStyle.ImagePath(logoId) : null;
        bool open = view.Kind == ProductPageKind.Published && !closed;
        List<JsonLdBlock> blocks = [];
        if (open && view.PriceAmount is { } price)
        {
            blocks.Add(search.Product(new ProductData(
                view.Name,
                view.SearchDescription,
                view.LeadImageForSearch is { } image ? [image] : [],
                view.Sku,
                price,
                view.InStock ? StockStatus.InStock : StockStatus.OutOfStock,
                view.Path)));
            List<BreadcrumbItem> steps = [new("Ana sayfa", "/")];
            steps.AddRange(view.Trail.Select(c => new BreadcrumbItem(c.Name, c.Path)));
            steps.Add(new BreadcrumbItem(view.Name, view.Path));
            if (search.Breadcrumbs(steps) is { } crumbs)
            {
                blocks.Add(crumbs);
            }
        }

        PageSearchData data = RecordPages.Product(view.Name, view.SearchDescription, null, view.Slug, view.LeadImageForSearch, logoPath, frame.Brand.BrandName, open, blocks);
        return data;
    }

    /// <summary>The head data of the search page (07 2.6.7; D-693): the screen's name and the brand name, closed, nothing else.</summary>
    public static PageSearchData SearchScreen(StorefrontFrame frame)
    {
        ArgumentNullException.ThrowIfNull(frame);
        return RecordPages.ClosedScreen("Arama", frame.Brand.BrandName, Frame.FramePaths.Search);
    }
}
