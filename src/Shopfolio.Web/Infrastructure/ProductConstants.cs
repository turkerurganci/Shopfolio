namespace Shopfolio.Infrastructure;

/// <summary>
/// The product constants (02 11.2; 09 3.5 item 1): numbers the product fixes and no setting can change. Each constant carries its inventory
/// id and a number is written once, here; the check constraints of the model read it.
/// </summary>
internal static class ProductConstants
{
    /// <summary>P-20 — depth of the category tree, in levels.</summary>
    public const int CategoryLevelMax = 3;

    /// <summary>P-21 — option dimensions of a product.</summary>
    public const int OptionDimensionMax = 2;

    /// <summary>P-27 — character ceiling of a product name (and of a name or title in the corporate content).</summary>
    public const int ProductNameMaxLength = 150;
}
