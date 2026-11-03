namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>The closed values of DefectClaimStatus (06 5.2.4). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum DefectClaimStatus
{
    Open,
    Resolved,
}
