namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>The closed values of ProductType (06 5.3.1). Only the code names live here; the behavior of a value lives in the rule that owns it (06 5.1.3).</summary>
public enum ProductType
{
    Physical,
    Digital,
    Service,
}
