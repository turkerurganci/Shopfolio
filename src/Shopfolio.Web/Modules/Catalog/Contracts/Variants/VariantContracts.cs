namespace Shopfolio.Modules.Catalog.Contracts.Variants;

/// <summary>
/// The state of a stock code typed in the variant form (02 3.3.4; 05 7.8.1): free, missing (the field is required and an empty one is not a code) or already held by
/// another variant. The comparison is the identity one - letter case does not count and "I", "İ", "ı", "i" are one letter. Not a stored value.
/// </summary>
public enum StockCodeCheck
{
    Free,
    Missing,
    Taken,
}

/// <summary>
/// The verdict on the option values a variant is bound to (02 3.3.2, 3.3.3; 06 4.6 "Uygulamada"): the values must be the product's own, the first value of the
/// option of order 1 and the second of the option of order 2; every option of the product needs its value; and the combination is held by one variant at most.
/// Not a stored value.
/// </summary>
public enum VariantStructureCheck
{
    Valid,
    ValueMissing,
    ValueNotOfTheProduct,
    ValueForAbsentOption,
    CombinationTaken,
}

/// <summary>
/// One cell of a product's option matrix (02 3.3.3): a pair of option values and the variant that opens it. A cell with no variant is a combination the firm did not
/// open - the storefront shows it but it cannot be chosen. A product without options has one cell with both values empty; a product with one option has a cell per value.
/// </summary>
public sealed record OptionMatrixCell(Guid? FirstOptionValueId, Guid? SecondOptionValueId, Guid? VariantId)
{
    /// <summary>Whether a variant opens this combination; the others are shown and cannot be chosen.</summary>
    public bool IsOpened => VariantId is not null;
}
