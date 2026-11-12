namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The stock column of a product row (04 9.4.3; K-802): a physical product shows its total stock or "Tükendi", a service its quota, "Tükendi" or that it is unlimited, a digital product nothing.
/// </summary>
public enum PanelStockKind
{
    /// <summary>The product counts units and some can still be bought: <see cref="PanelProductRow.StockTotal"/> is the total.</summary>
    Counted,

    /// <summary>The product counts units and none of its variants can be bought - the stock less the reserved units is zero everywhere (02 3.6.3, 3.6.4).</summary>
    SoldOut,

    /// <summary>A service with a variant that has no quota (02 3.6.2).</summary>
    Unlimited,

    /// <summary>A digital product: there is no stock field (02 3.6.2).</summary>
    None,
}

/// <summary>
/// One row of the panel's product list (T525; 04 9.4.3): the main image, the name, the type, the publish status, the variant count, the price - the lowest and the highest when the variants' prices
/// differ - and the stock column. The figures are the server's (07 UC-87); every variant counts, whatever its status, since the panel shows the firm its whole catalog.
/// </summary>
public sealed record PanelProductRow(
    Guid Id,
    string Name,
    ProductType ProductType,
    PublishStatus PublishStatus,
    Guid? MainImageId,
    string? MainImageAltText,
    int VariantCount,
    decimal? LowestPrice,
    decimal? HighestPrice,
    PanelStockKind StockKind,
    int StockTotal);

/// <summary>One page of the panel's product list: the rows, the page shown (a page past the last is the last, 07 2.6.2), the number of pages and the number of rows of the search and filter.</summary>
public sealed record PanelProductPage(IReadOnlyList<PanelProductRow> Rows, int PageNumber, int PageCount, int TotalCount);

/// <summary>
/// The panel's product list (T525; 07 UC-87; 04 9.4): every product - draft and archive too, which only this search finds (04 9.4.5; K-737) - newest first, with the name search and the publish status
/// filter applied together. A plain read.
/// </summary>
public interface IPanelProductList
{
    /// <summary>
    /// One page of the list. <paramref name="search"/> null or blank searches nothing; <paramref name="status"/> null filters nothing. <paramref name="requestedPage"/> is clamped into the range. The
    /// number of reads is the same for every page size: one count and one list, one figure read and one image read for the page (09 20 item 4).
    /// </summary>
    Task<PanelProductPage> ListAsync(string? search, PublishStatus? status, int requestedPage, int pageSize, CancellationToken ct);
}
