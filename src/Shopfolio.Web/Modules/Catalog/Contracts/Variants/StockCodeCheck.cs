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
