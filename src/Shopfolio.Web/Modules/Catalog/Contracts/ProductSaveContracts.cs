using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The product form's save (07 UC-114; 03 8.1.1-8.1.6, 8.1.10, 8.1.11): the product's fields of 06 4.5, its categories, options, variants, gallery, product-level file and discount. A new
/// product carries no <see cref="ProductId"/> and no <see cref="Version"/>; an existing one carries both. The publish status is not in it (K-1189 (1)).
/// </summary>
public sealed record ProductSaveCommand(
    Guid? ProductId,
    int? Version,
    ProductType? ProductType,
    string? Name,
    string? Description,
    decimal? VatRate,
    Guid? OriginCountryId,
    MeasurementUnit? MeasurementUnit,
    int? ShippingPeriod,
    int? FulfilmentPeriod,
    int? MaxQuantityPerOrder,
    WithdrawalExemptionReason? WithdrawalExemptionReason,
    IReadOnlyList<Guid> CategoryIds,
    Guid? PrimaryCategoryId,
    IReadOnlyList<ProductSaveOption> Options,
    IReadOnlyList<ProductSaveVariant> Variants,
    IReadOnlyList<GalleryEntry> Gallery,
    Guid? DigitalFileId,
    ProductSaveDiscount? Discount);

/// <summary>An option dimension and its values (06 4.7, 4.8); the keys are present on an existing row.</summary>
public sealed record ProductSaveOption(Guid? OptionId, string? Name, IReadOnlyList<ProductSaveOptionValue> Values);

/// <summary>A value of an option dimension (06 4.8).</summary>
public sealed record ProductSaveOptionValue(Guid? OptionValueId, string? Value);

/// <summary>
/// A variant row (06 4.6, 4.70): no key on a new row. <see cref="ShownStockQuantity"/> is the stock the form showed beside the field (07 2.4.3; K-980); the stock is written against it,
/// never by the record's save.
/// </summary>
public sealed record ProductSaveVariant(
    Guid? VariantId,
    Guid? FirstOptionValueId,
    Guid? SecondOptionValueId,
    string? SKU,
    decimal? Price,
    decimal? NetQuantity,
    int? StockQuantity,
    int? ShownStockQuantity,
    Guid? ImageId,
    Guid? DigitalFileId);

/// <summary>The product's discount (06 4.12): all three filled, or the discount is removed.</summary>
public sealed record ProductSaveDiscount(decimal? Percentage, DateOnly? StartDate, DateOnly? EndDate);

/// <summary>
/// A field's refusal in the product form's save (07 UC-114 422): the field's name as the form posts it (06 4 names; rows indexed, <c>Variants[1].Price</c>), the code of the error registry
/// (07 6.2) and the values its message names - the reserved quantity, the discounted and the reference price - when the code has any.
/// </summary>
public sealed record ProductSaveFieldError(string Field, ErrorCode Code, decimal? Amount = null, decimal? ReferenceAmount = null);

/// <summary>
/// The outcome of the product form's save (07 UC-114): saved, with the product's key; or refused - 404 (the product, or a variant of the form, is not the product's), 422 with the fields'
/// refusals, 409 (the version, or a variant's shown stock, is no longer the record's; 04 2.10.1) or the 422 below the reserved quantity (K-800). The transaction runner rolls a refused
/// save back: a refusal writes nothing.
/// </summary>
public sealed class ProductSaveResult : IOperationResult
{
    private static readonly IReadOnlyList<ProductSaveFieldError> NoErrors = [];

    private ProductSaveResult(Guid? productId, ErrorCode? code, IReadOnlyList<ProductSaveFieldError> fieldErrors)
    {
        ProductId = productId;
        Code = code;
        FieldErrors = fieldErrors;
    }

    /// <summary>The product or a variant of the form is not found (deleted meanwhile, or another product's).</summary>
    public static ProductSaveResult NotFound { get; } = new(null, ErrorCode.NotFound, NoErrors);

    /// <summary>The product's version is no longer the form's (07 6.2.2 <c>RecordVersionConflict</c>).</summary>
    public static ProductSaveResult VersionConflict { get; } = new(null, ErrorCode.RecordVersionConflict, NoErrors);

    /// <summary>The saved product's key; empty on a refusal.</summary>
    public Guid? ProductId { get; }

    /// <summary>The refusal's code; empty when the product was saved.</summary>
    public ErrorCode? Code { get; }

    /// <summary>The fields' refusals (422), in the form's order.</summary>
    public IReadOnlyList<ProductSaveFieldError> FieldErrors { get; }

    public bool IsRejected => Code is not null;

    public static ProductSaveResult Saved(Guid productId) => new(productId, null, NoErrors);

    /// <summary>The fields' refusals (07 UC-114 422); the code is the first field's.</summary>
    public static ProductSaveResult Refused(IReadOnlyList<ProductSaveFieldError> fieldErrors)
    {
        ArgumentNullException.ThrowIfNull(fieldErrors);
        if (fieldErrors.Count == 0)
        {
            throw new ArgumentException("A refusal names at least one field.", nameof(fieldErrors));
        }

        return new ProductSaveResult(null, fieldErrors[0].Code, fieldErrors);
    }

    /// <summary>A variant's stock is no longer the value the form showed (07 2.4.3; 04 2.10.1; <c>DisplayedValueConflict</c>): the 409 of K-800.</summary>
    public static ProductSaveResult ShownStockChanged(string field) => new(null, ErrorCode.DisplayedValueConflict, [new ProductSaveFieldError(field, ErrorCode.DisplayedValueConflict)]);

    /// <summary>A variant's new stock is below its reserved quantity (02 3.6.7; K-653): the last refusal of the order (K-800), with the reserved quantity.</summary>
    public static ProductSaveResult BelowReserved(string field, int reservedQuantity)
        => new(null, ErrorCode.StockBelowReserved, [new ProductSaveFieldError(field, ErrorCode.StockBelowReserved, reservedQuantity)]);
}
