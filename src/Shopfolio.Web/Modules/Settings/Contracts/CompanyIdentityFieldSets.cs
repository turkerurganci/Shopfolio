namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The fields a company type adds to the common set (02 3.1.3), by the names of <see cref="CompanyIdentityInput"/>'s members: the one place the set is written, read by the identity rules and by the
/// identity screen, which tells the firm which of the type's fields are required.
/// </summary>
public static class CompanyIdentityFieldSets
{
    /// <summary>The fields only the tradesman must give: the owner's name and the tax number.</summary>
    private static readonly string[] TradesmanFields = [nameof(CompanyIdentityInput.LegalName), nameof(CompanyIdentityInput.TaxIdNumber)];

    /// <summary>The fields both merchant types must give: the trade title, the MERSIS and the trade registry numbers.</summary>
    private static readonly string[] MerchantFields =
    [
        nameof(CompanyIdentityInput.LegalName),
        nameof(CompanyIdentityInput.MersisNumber),
        nameof(CompanyIdentityInput.TradeRegistryNumber),
    ];

    /// <summary>The fields a type adds to the common set; empty for a type that is not chosen yet.</summary>
    public static IReadOnlyList<string> TypeFields(CompanyType? type)
    {
        return type switch
        {
            CompanyType.Tradesman => TradesmanFields,
            CompanyType.NaturalPersonMerchant or CompanyType.LegalEntity => MerchantFields,
            _ => [],
        };
    }
}
