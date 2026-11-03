namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>The closed values of AddressType (06 5.6.3). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum AddressType
{
    ShippingAddress,
    BillingAddress,
}
