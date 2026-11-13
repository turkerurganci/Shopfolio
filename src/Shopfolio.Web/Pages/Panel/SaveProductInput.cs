using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The product form's model (07 5.1.5 <c>SaveProductInput</c>; UC-114, UC-117): the product's fields of 06 4.5, its categories, options, variants, gallery, product-level file and discount,
/// the record's <c>Version</c> and UC-117's target field (07 5.2.3). The publish status is not in the form (K-1189 (1)). The image upload binds the same model, reads the file
/// <c>ImageFile</c> through the upload component (<see cref="Shopfolio.Pages.Components.UploadRequests"/>) and <see cref="ImageTarget"/>; the save reads neither (K-1171 (5)).
/// </summary>
public sealed class SaveProductInput : IFormInput
{
    public ProductType? ProductType { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    [InvalidPercentage(AllowZero = true)]
    public decimal? VatRate { get; set; }

    public Guid? OriginCountryId { get; set; }

    public MeasurementUnit? MeasurementUnit { get; set; }

    public int? ShippingPeriod { get; set; }

    public int? FulfilmentPeriod { get; set; }

    public int? MaxQuantityPerOrder { get; set; }

    public WithdrawalExemptionReason? WithdrawalExemptionReason { get; set; }

    [SpecAssignedName]
    public IList<SaveProductCategoryRow> Categories { get; set; } = [];

    [SpecAssignedName]
    public IList<SaveProductOptionRow> Options { get; set; } = [];

    [SpecAssignedName]
    public IList<SaveProductVariantRow> Variants { get; set; } = [];

    [SpecAssignedName]
    public IList<SaveProductImageRow> Images { get; set; } = [];

    public Guid? DigitalFileId { get; set; }

    [SpecAssignedName]
    public SaveProductDiscountRow? Discount { get; set; }

    public int? Version { get; set; }

    /// <summary>UC-117: the form's image field the upload goes to - <c>Images</c> (the gallery) or <c>Variants[n].ImageId</c> (07 5.2.3).</summary>
    [SpecAssignedName]
    public string? ImageTarget { get; set; }
}

/// <summary>A category of the product (06 4.10; 04 7.2.4.3).</summary>
public sealed class SaveProductCategoryRow
{
    public Guid CategoryId { get; set; }

    public bool IsPrimaryCategory { get; set; }
}

/// <summary>An option dimension and its values (06 4.7, 4.8; 04 7.2.4.6); the keys are present on an existing row.</summary>
public sealed class SaveProductOptionRow
{
    public Guid? OptionId { get; set; }

    public string? Name { get; set; }

    [SpecAssignedName]
    public IList<SaveProductOptionValueRow> Values { get; set; } = [];
}

/// <summary>A value of an option dimension (06 4.8).</summary>
public sealed class SaveProductOptionValueRow
{
    public Guid? OptionValueId { get; set; }

    public string? Value { get; set; }
}

/// <summary>
/// A variant row (06 4.6, 4.70; 04 7.2.4.7-7.2.4.10, 7.2.4.13): the stock carries the value the form showed beside it (<c>OriginalStockQuantity</c>, 07 2.4.3; K-980), read as text by the shown
/// value rule (<see cref="Shopfolio.Infrastructure.Forms.ShownValue"/>).
/// </summary>
public sealed class SaveProductVariantRow
{
    public Guid? VariantId { get; set; }

    public Guid? FirstOptionValueId { get; set; }

    public Guid? SecondOptionValueId { get; set; }

    public string? SKU { get; set; }

    [InvalidAmount]
    public decimal? Price { get; set; }

    [InvalidNetQuantity]
    public decimal? NetQuantity { get; set; }

    public int? StockQuantity { get; set; }

    [SpecAssignedName]
    public string? OriginalStockQuantity { get; set; }

    public Guid? ImageId { get; set; }

    public Guid? DigitalFileId { get; set; }
}

/// <summary>An image of the gallery (06 4.71; 04 7.2.4.5).</summary>
public sealed class SaveProductImageRow
{
    public Guid ImageId { get; set; }

    public int SortOrder { get; set; }

    public bool IsMain { get; set; }

    public string? AltText { get; set; }
}

/// <summary>The product's discount (06 4.12; 04 7.2.4.18).</summary>
public sealed class SaveProductDiscountRow
{
    [InvalidPercentage]
    public decimal? Percentage { get; set; }

    [InvalidDate]
    public DateOnly? StartDate { get; set; }

    [InvalidDate]
    public DateOnly? EndDate { get; set; }
}
