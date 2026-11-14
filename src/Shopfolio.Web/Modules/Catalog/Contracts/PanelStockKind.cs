namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// The stock column of a product row (04 9.4.3; K-802): a physical product shows its total stock or "Tükendi", a service its quota, "Tükendi" or that it is unlimited, a digital product nothing.
/// </summary>
public enum PanelStockKind
{
    /// <summary>The product counts units and some can still be bought: <see cref="PanelProductRow.StockTotal"/> is the total.</summary>
    Counted,

    /// <summary>The product counts units and none of its variants can be bought - the stock less the reserved units is zero everywhere (02 3.6.3, 3.6.4).</summary>
    SoldOut,

    /// <summary>A service with a variant that has no quota (02 3.6.2).</summary>
    Unlimited,

    /// <summary>A digital product: there is no stock field (02 3.6.2).</summary>
    None,
}
