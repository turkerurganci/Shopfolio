namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>The closed values of AuditArea (06 5.6.10). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum AuditArea
{
    CompanyAndBranding,
    Publication,
    Order,
    AdminAccounts,
    LegalTextVersion,
    Settings,
    Export,
    CustomerDeletion,
}
