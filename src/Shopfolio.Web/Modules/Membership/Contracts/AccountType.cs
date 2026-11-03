namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>The closed values of AccountType (06 5.3.3). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum AccountType
{
    Admin,
    Customer,
}
