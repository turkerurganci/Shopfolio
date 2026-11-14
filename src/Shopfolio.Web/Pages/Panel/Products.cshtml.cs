using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The query model of the product list (E-32; 07 5.3.2 <c>AdminProductQuery</c>; UC-87): the name searched, the publish status and <c>page</c>. Every field is text so that a wrong value is never a binding
/// error (07 2.6.6): <see cref="ProductsModel"/> reads the closed list and drops a value outside it. The search is free text with no format check (04 9.4.7); there is no sort (07 2.6.5).
/// </summary>
public sealed class AdminProductQuery : PagedQuery
{
    [SpecAssignedName]
    public string? Search { get; init; }

    public string? PublishStatus { get; init; }
}

/// <summary>One option of the publish status filter (04 9.4.3): the parameter's value, its name and whether the request selected it; the empty value is "every status".</summary>
public sealed record ProductStatusOption(string Value, string Label, bool IsSelected);

/// <summary>
/// E-32, the product list (T525; 04 9.4; 07 UC-87): every product newest first, the name search - which alone finds a draft and an archived product (04 9.4.5; K-737) - and the publish status filter. A row and
/// "Yeni ürün" open the product form (E-33) carrying the list's address in <c>returnUrl</c> (07 2.6.8). The row has no operation: the publish moves and the deletion are on the form (04 9.4.4; K-802). The
/// GET writes nothing.
/// </summary>
public sealed class ProductsModel : PageModel
{
    public const string Title = "Ürünler";

    public const string NewProductLabel = "Yeni ürün";

    public const string SearchLabel = "Ürün adı";

    public const string StatusLabel = "Yayın durumu";

    public const string AnyStatusLabel = "Tümü";

    /// <summary>The sentence of a list with no product at all (04 9.4.9): its way out opens the first record.</summary>
    public const string NoProductsText = "Henüz ürün yok.";

    /// <summary>The sentence of a search or filter whose result is empty; its way out removes both.</summary>
    public const string NoMatchText = "Bu arama ya da süzgece uyan ürün yok.";

    public const string RemoveSearchLabel = "Aramayı ve süzgeci kaldır";

    public const string SoldOutText = "Tükendi";

    public const string UnlimitedText = "Sınırsız";

    private readonly IPanelProductList _products;

    public ProductsModel(IPanelProductList products)
    {
        ArgumentNullException.ThrowIfNull(products);
        _products = products;
    }

    public PanelTable? Table { get; private set; }

    public Pager Pager { get; private set; } = new(null, null, []);

    public EmptyStateView? Empty { get; private set; }

    public string? SearchValue { get; private set; }

    public IReadOnlyList<ProductStatusOption> StatusOptions { get; private set; } = [];

    /// <summary>The address of "Yeni ürün": the empty form, carrying the list's address back (07 UC-87).</summary>
    public string NewProductPath { get; private set; } = PanelFramePaths.Products + "/yeni";

    public string ListRoot => PanelFramePaths.Products;

    public async Task<IActionResult> OnGetAsync(AdminProductQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        PublishStatus? status = ListFilters.ReadCode<PublishStatus>(query.PublishStatus);
        string? search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        SearchValue = search;
        bool isNarrowed = search is not null || status is not null;

        PanelProductPage page = await _products.ListAsync(search, status, query.RequestedPage(), ListCatalog.PageSizeOf(ListKind.PanelProducts), ct);
        string listAddress = ListAddress.Of(ListRoot, query, page.PageNumber);
        NewProductPath = ListReturn.LinkTo(PanelFramePaths.Products + "/yeni", listAddress);
        StatusOptions =
        [
            new(string.Empty, AnyStatusLabel, status is null),
            .. Enum.GetValues<PublishStatus>().Select(s => new ProductStatusOption(s.ToString(), StatusBadges.For(s).Text, s == status)),
        ];

        if (page.Rows.Count == 0)
        {
            Empty = isNarrowed
                ? EmptyStates.WithExit(NoMatchText, RemoveSearchLabel, ListRoot)
                : EmptyStates.WithExit(NoProductsText, NewProductLabel, NewProductPath);
            return Page();
        }

        Table = new PanelTable(Title, Columns, [.. page.Rows.Select(row => RowOf(row, listAddress))], NameColumn);
        Pager = Pager.Of(ListRoot, query, page.PageNumber, page.PageCount);
        return Page();
    }

    // The columns in the order of 04 9.4.3; the type and the variant count go under the row on the medium class (04 9.4.10).
    private static readonly PanelTableColumn[] Columns =
    [
        new("Görsel", true),
        new("Ürün", true),
        new("Tip", false),
        new("Durum", true),
        new("Varyant", false),
        new("Fiyat", true),
        new("Stok", true),
    ];

    private const int ImageColumn = 0;

    private const int NameColumn = 1;

    private const int StatusColumn = 3;

    public static string TypeOf(ProductType type) => type switch
    {
        ProductType.Physical => "Fiziksel",
        ProductType.Digital => "Dijital",
        ProductType.Service => "Hizmet",
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "A product has one of three types."),
    };

    /// <summary>The price cell (04 9.4.3): the one price, or the lowest and the highest when the variants' prices differ; empty for a product without a variant.</summary>
    public static string PriceOf(decimal? lowest, decimal? highest) => (lowest, highest) switch
    {
        ({ } low, { } high) when low != high => $"{DisplayFormat.Amount(low)} – {DisplayFormat.Amount(high)}",
        ({ } low, _) => DisplayFormat.Amount(low),
        _ => string.Empty,
    };

    /// <summary>The stock cell (04 9.4.3; K-802): the total, "Tükendi", "Sınırsız" or empty for a digital product.</summary>
    public static string StockOf(PanelStockKind kind, int total) => kind switch
    {
        PanelStockKind.Counted => total.ToString(System.Globalization.CultureInfo.InvariantCulture),
        PanelStockKind.SoldOut => SoldOutText,
        PanelStockKind.Unlimited => UnlimitedText,
        PanelStockKind.None => string.Empty,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The stock column has four kinds."),
    };

    private static PanelTableRow RowOf(PanelProductRow row, string listAddress)
    {
        Badge badge = StatusBadges.For(row.PublishStatus);
        Dictionary<int, CardImage>? images = row.MainImageId is { } imageId
            ? new() { [ImageColumn] = ProductCards.ImageOf(imageId, row.MainImageAltText ?? row.Name) }
            : null;
        return new PanelTableRow(
            [
                string.Empty,
                row.Name,
                TypeOf(row.ProductType),
                badge.Text,
                row.VariantCount.ToString(System.Globalization.CultureInfo.InvariantCulture),
                PriceOf(row.LowestPrice, row.HighestPrice),
                StockOf(row.StockKind, row.StockTotal),
            ],
            ListReturn.LinkTo($"{PanelFramePaths.Products}/{row.Id}", listAddress),
            new Dictionary<int, Badge> { [StatusColumn] = badge },
            images);
    }
}
