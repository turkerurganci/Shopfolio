namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>What the add of a variant to a cart came to. It is the <see cref="CartAddResult.Outcome"/>; a result code, never stored.</summary>
public enum CartAddOutcome
{
    /// <summary>The variant is in the cart: a new line, or the quantity of the existing line went up.</summary>
    Added,

    /// <summary>The quantity is not above zero.</summary>
    InvalidQuantity,

    /// <summary>No such variant.</summary>
    VariantNotFound,

    /// <summary>The variant or its product is not on the showcase.</summary>
    NotOnSale,

    /// <summary>The variant is sold out and cannot be added (02 3.6.4).</summary>
    SoldOut,

    /// <summary>The quantity the line would hold is above what can be bought now ("Bu adette stok yok"; 02 3.16.8). The cart is not changed and the message gives no number.</summary>
    NoStock,

    /// <summary>The quantity of the product's lines together would pass the firm's per-order limit (02 3.16.9); the result carries the limit. It wins over <see cref="NoStock"/>.</summary>
    OverOrderLimit,
}
