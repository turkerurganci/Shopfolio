namespace Shopfolio.Modules.Catalog.Contracts;

/// <summary>
/// What is wrong with the quantity of a cart line (02 3.16.8-3.16.10). A result code, never stored.
/// None: the quantity is allowed - neither the stock nor the per-order limit is exceeded.
/// NoStock: the quantity is above what can be bought now - the stock less the reserved units, or the service capacity; the message does not say how many can be bought.
/// OverOrderLimit: the quantity of the product's lines together is above the firm's "bir siparişte en fazla" limit; the message says the limit.
/// </summary>
public enum CartQuantityProblem
{
    None,
    NoStock,
    OverOrderLimit,
}
