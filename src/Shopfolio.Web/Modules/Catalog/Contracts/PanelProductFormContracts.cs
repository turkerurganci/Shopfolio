using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// One variant row of the product form (T525; 04 9.5.5; 07 UC-88): the option values' names, the stock code, the price, the net quantity, the stock or quota - whose value is also the form's
/// shown value (<c>OriginalStockQuantity</c>, 07 2.4.3; K-980) - the units reserved by orders awaiting payment (06 4.6 <c>ReservedQuantity</c>; K-653) and the variant's own status.
/// </summary>
public sealed record PanelVariantRow(
    Guid Id,
    IReadOnlyList<string> OptionValues,
    string StockCode,
    decimal Price,
    decimal? NetQuantity,
    int? StockQuantity,
    int ReservedQuantity,
    PublishStatus PublishStatus,
    Guid? ImageId);

/// <summary>One image of the product's gallery (T525; 06 4.71; 04 7.2.4.5): the image, its place, the main mark and the alternative text.</summary>
public sealed record PanelImageRow(Guid ImageId, int SortOrder, bool IsMain, string? AltText);

/// <summary>
/// The product form's record as it stands when the form opens (T525; 07 UC-88): the product's fields of 06 4.5, its <c>Version</c> (06 2.6), its variants, its gallery, the publish gate's unmet conditions
/// by name (04 9.5.10; 02 3.7.3) and the address - fixed at the first publish (02 3.30.2), before that the draft preview's. The figures are the server's (07 8.1.4).
/// </summary>
public sealed record PanelProductRecord(
    Guid Id,
    string Name,
    string? Description,
    ProductType ProductType,
    PublishStatus PublishStatus,
    decimal VatRate,
    Guid? OriginCountryId,
    MeasurementUnit? MeasurementUnit,
    int? ShippingPeriod,
    int? FulfilmentPeriod,
    int? MaxQuantityPerOrder,
    WithdrawalExemptionReason? WithdrawalExemptionReason,
    int Version,
    string Slug,
    bool IsSlugFixed,
    IReadOnlyList<PanelVariantRow> Variants,
    IReadOnlyList<PanelImageRow> Images,
    IReadOnlyList<string> UnmetGateConditions);

/// <summary>The panel's product form read (T525; 07 UC-88). A plain read; it writes nothing.</summary>
public interface IPanelProductForm
{
    /// <summary>The record the form shows; null when the key is no product (07 UC-88: 404). Five reads whatever the variant count (09 20 item 4).</summary>
    Task<PanelProductRecord?> GetAsync(Guid productId, CancellationToken ct);

    /// <summary>
    /// The product's current <c>Version</c>, for the side column's forms that carry none (07 UC-115, UC-116; K-1189 (1)); null when the key is no product or, with
    /// <paramref name="variantId"/>, when that variant is not the product's (07: 404).
    /// </summary>
    Task<int?> GetVersionAsync(Guid productId, Guid? variantId, CancellationToken ct);
}
