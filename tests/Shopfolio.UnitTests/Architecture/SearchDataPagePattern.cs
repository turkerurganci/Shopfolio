using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.SearchEngine;

// The namespaces are deliberately the application's: the page in the page namespace, the module's open interface in the Catalog Contracts namespace; the page rule and the
// module rule of ModuleBoundaryTests read types by those namespaces (09 6 item 2).
#pragma warning disable IDE0130
namespace Shopfolio.Modules.Catalog.Contracts
{
    /// <summary>What the Catalog module hands a product page for the search engines: the values of the record, already decided (the stock status net of the reservations, the main category's trail).</summary>
    internal sealed record PatternProductSearchView(ProductData Product, IReadOnlyList<BreadcrumbItem> Trail, string? BrandName, string? LogoPath, bool Published);

    /// <summary>The stand-in of a module's open interface for a product page's search engine values (the real module's <c>Contracts</c> interface in a real page).</summary>
    internal interface IPatternProductSearchSource
    {
        Task<PatternProductSearchView?> GetAsync(string slug, CancellationToken ct);
    }
}

// The search engine data pattern as a REAL page of the application's page namespace uses it (T219; 09 6 items 2 and 7; 05 3.7.2). The type lives in the unit test assembly
// but in the namespace the page rule of ModuleBoundaryTests reads (Shopfolio.Pages), so the architecture test sees it and proves the shape clean: the page takes the record's
// values from the module's Contracts interface, builds the head data and the JSON-LD blocks with the common pattern (Infrastructure/SearchEngine), and reaches no module
// internal, no entity and no database context.
namespace Shopfolio.Pages.Storefront
{
    using Shopfolio.Modules.Catalog.Contracts;

    /// <summary>A product page that tells the search engines what the record says: title, description, Open Graph preview and the product and breadcrumb JSON-LD.</summary>
    internal sealed class SearchDataPatternPage(IPatternProductSearchSource source, SearchEngineData searchEngine) : PageModel
    {
        internal IHtmlContent Head { get; private set; } = HtmlString.Empty;

        public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
        {
            PatternProductSearchView? view = await source.GetAsync(slug, ct);
            if (view is null)
            {
                return NotFound();
            }

            List<JsonLdBlock> blocks = [searchEngine.Product(view.Product)];
            if (searchEngine.Breadcrumbs(view.Trail) is { } trail)
            {
                blocks.Add(trail);
            }

            Head = searchEngine.Head(
                HttpContext,
                new PageSearchData(
                    SearchText.Title(view.Product.Name, view.BrandName),
                    SearchText.Description(view.Product.Description, null),
                    view.Product.Path,
                    view.Product.ImagePaths.Count > 0 ? view.Product.ImagePaths[0] : null,
                    view.LogoPath,
                    view.BrandName,
                    !view.Published,
                    blocks)
                {
                    OpenGraphType = "product",
                });
            return Page();
        }
    }
}
