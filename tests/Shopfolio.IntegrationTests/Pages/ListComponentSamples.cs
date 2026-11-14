using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Lists;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>The product grid with a pager (T545 KK1-KK5, KK8): four cards - a plain one with an image, a discounted one, a sold-out one and a long-named one - and the navigation of a list of twelve pages (<c>PageCount</c>).</summary>
public sealed class ProductGridSampleModel : PageModel
{
    public const string Root = "/odeme/liste-urunler";

    public const int PageCount = 12;

    public static readonly Guid ImageId = Guid.Parse("0a1b2c3d-0000-7000-8000-0000000000aa");

    public IReadOnlyList<ProductCard> Cards { get; private set; } = [];

    public Pager Pager { get; private set; } = Pager.Of(Root, new ProductQuery(), 1, 1);

    public IActionResult OnGet([FromQuery(Name = "page")] string? page, [FromQuery(Name = "search")] string? search)
    {
        DiscountLine line = new(80m, 100m, 20m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30));
        ProductListItem[] items =
        [
            new(Guid.NewGuid(), "Düz ampul", "duz-ampul", 120m, true, null),
            new(Guid.NewGuid(), "İndirimli lamba", "indirimli-lamba", 100m, true, line),
            new(Guid.NewGuid(), "Tükenmiş avize", "tukenmis-avize", 1500.5m, false, null),
            new(Guid.NewGuid(), "Çok uzun bir ürün adı ki kartta iki satırdan fazlasına yayılıp kesilmesi gereken bir ad", "uzun-ad", 99m, true, null),
        ];
        Cards = ProductCards.Of(items, item => item.Slug == "duz-ampul" ? ProductCards.ImageOf(ImageId, "Düz ampul görseli") : null);

        ProductQuery query = new() { Search = search, Page = page };
        Pager = Pager.Of(Root, query, query.RequestedPage(), PageCount);
        return Page();
    }
}

/// <summary>The content list (T545 KK4, KK6): a card with an image and a description, and a record without an image that is text only; the order is the one given.</summary>
public sealed class ContentGridSampleModel : PageModel
{
    public IReadOnlyList<ContentCard> Cards { get; private set; } = [];

    public IActionResult OnGet()
    {
        HomepageCard[] records =
        [
            new(Guid.NewGuid(), "Zeta hizmeti", "Zeta açıklaması.", "/sayfa/zeta", new ContentImage(ProductGridSampleModel.ImageId, null)),
            new(Guid.NewGuid(), "Alfa hizmeti", null, "/sayfa/alfa", null),
        ];
        Cards = ContentCards.Of(records);
        return Page();
    }
}

/// <summary>The empty-state line and the search field of a list of a given kind (T545 KK7, KK10); the kind comes from the address.</summary>
public sealed class ListEmptySampleModel : PageModel
{
    public ListKind Kind { get; private set; }

    public EmptyState State { get; } = new("Bu listede kayıt yok.", "Ana sayfaya dön", "/");

    public ListSearch Search => new(Kind, "/odeme/liste-bos", "Ara", null);

    public IActionResult OnGet([FromQuery(Name = "kind")] ListKind kind)
    {
        Kind = kind;
        return Page();
    }
}

/// <summary>A panel table of three rows (T545 KK9): two primary columns, one secondary; the first row has no record link.</summary>
public sealed class PanelTableSampleModel : PageModel
{
    public PanelTable Table { get; } = new(
        "Siparişler",
        [new PanelTableColumn("Sipariş", true), new PanelTableColumn("Durum", true), new PanelTableColumn("E-posta", false)],
        [
            new PanelTableRow(["SP-1001", "Hazırlanıyor", "a@example.test"], null),
            new PanelTableRow(["SP-1002", "Kargoda", "<b>b</b>@example.test"], "/panel/siparisler/2"),
            new PanelTableRow(["SP-1003", "Tamamlandı", "c@example.test"], "/panel/siparisler/3"),
        ]);

    public IActionResult OnGet() => Page();
}
