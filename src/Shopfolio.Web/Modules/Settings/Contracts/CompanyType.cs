namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>The closed values of CompanyType (06 5.3.2). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum CompanyType
{
    Tradesman,
    NaturalPersonMerchant,
    LegalEntity,
}
