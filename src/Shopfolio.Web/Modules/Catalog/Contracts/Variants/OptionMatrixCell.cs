namespace Shopfolio.Modules.Catalog.Contracts.Variants;

/// <summary>
/// One cell of a product's option matrix (02 3.3.3): a pair of option values and the variant that opens it. A cell with no variant is a combination the firm did not
/// open - the storefront shows it but it cannot be chosen. A product without options has one cell with both values empty; a product with one option has a cell per value.
/// </summary>
public sealed record OptionMatrixCell(Guid? FirstOptionValueId, Guid? SecondOptionValueId, Guid? VariantId)
{
    /// <summary>Whether a variant opens this combination; the others are shown and cannot be chosen.</summary>
    public bool IsOpened => VariantId is not null;
}
