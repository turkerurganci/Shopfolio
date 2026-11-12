using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>The head of the sample pages that are not a record's page: the title and the closed mark go through the one pattern (T219; 05 3.7.2, 3.7.4).</summary>
internal static class SampleSearchHead
{
    public static IHtmlContent Of(HttpContext context, string title, bool closed)
    {
        SearchEngineData engine = context.RequestServices.GetRequiredService<SearchEngineData>();
        return engine.Head(context, new PageSearchData(title, null, context.Request.Path.Value ?? "/", null, null, null, closed, []));
    }
}

/// <summary>
/// The test-only sample of a record page that tells the search engines what the record says (T219; 05 3.7.2): every value comes from the database - the product, its
/// cheapest variant (price, stock net of the reservation), its images, the brand name and logo, the published questions - and goes through the common pattern. The
/// route is a test route of the closed first segment list; the real product page is a later task's.
/// </summary>
public sealed class SearchDataSampleModel : PageModel
{
    internal IHtmlContent Head { get; private set; } = HtmlString.Empty;

    public string ProductName { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(string slug, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        SearchEngineData engine = services.GetRequiredService<SearchEngineData>();

        Product? product = await db.Products.AsNoTracking().SingleOrDefaultAsync(p => p.Slug == slug, ct);
        if (product is null)
        {
            return NotFound();
        }

        Variant? variant = await db.Variants.AsNoTracking().Where(v => v.ProductId == product.Id).OrderBy(v => v.Price).FirstOrDefaultAsync(ct);
        List<Guid> imageIds = await db.ProductImages.AsNoTracking().Where(i => i.ProductId == product.Id)
            .OrderByDescending(i => i.IsMain).ThenBy(i => i.SortOrder).Select(i => i.ImageId).ToListAsync(ct);
        Branding? branding = await db.Brandings.AsNoTracking().FirstOrDefaultAsync(ct);
        List<FaqItem> questions = await db.FaqItems.AsNoTracking().Where(f => f.PublishStatus == PublishStatus.Published).OrderBy(f => f.SortOrder).ToListAsync(ct);

        bool inStock = variant is not null && (variant.StockQuantity is null || variant.StockQuantity - variant.ReservedQuantity > 0);
        string path = $"/sss/ornek-arama/{product.Slug}";
        ProductData data = new(
            product.Name,
            SearchText.Description(null, product.Description),
            [.. imageIds.Select(ImagePath)],
            variant?.SKU,
            variant?.Price ?? 0m,
            inStock ? StockStatus.InStock : StockStatus.OutOfStock,
            path);
        string? brand = branding?.BrandName;
        string? logo = branding?.LogoId is { } logoId ? ImagePath(logoId) : null;

        List<JsonLdBlock> blocks = [engine.Product(data), engine.Organization(new OrganizationData(brand ?? "Mağaza", logo, null, null, []))];
        if (engine.Breadcrumbs([new BreadcrumbItem("Ana Sayfa", "/"), new BreadcrumbItem(product.Name, path)]) is { } trail)
        {
            blocks.Add(trail);
        }

        if (SearchEngineData.Faq([.. questions.Select(q => new FaqEntry(q.Question, q.Answer ?? string.Empty))]) is { } faq)
        {
            blocks.Add(faq);
        }

        ProductName = product.Name;
        Head = engine.Head(
            HttpContext,
            new PageSearchData(
                SearchText.Title(product.Name, brand),
                data.Description,
                path,
                imageIds.Count > 0 ? ImagePath(imageIds[0]) : null,
                logo,
                brand,
                product.PublishStatus != PublishStatus.Published,
                blocks)
            {
                OpenGraphType = "product",
            });
        return Page();
    }

    // The sample's image address; the real image address is the image serving task's.
    private static string ImagePath(Guid imageId) => $"/gorsel/{imageId:N}";
}
