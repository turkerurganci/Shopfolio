namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>What the change of a cart line's quantity came to (07 UC-20). It is the <see cref="CartItemUpdateResult.Outcome"/>; a result code, never stored.</summary>
public enum CartItemUpdateOutcome
{
    /// <summary>The quantity was written; a quantity equal to the one held writes the same value (the quantity is absolute).</summary>
    Updated,

    /// <summary>The line is not a line of this owner's cart: another cart's, one that never was, or one just removed (07 UC-20 404; K-1179 (2)).</summary>
    NotInCart,

    /// <summary>The quantity is under one, or a digital line's quantity is other than one (04 7.1.4.1; 02 3.12.4).</summary>
    InvalidQuantity,

    /// <summary>The raised quantity is above what can be bought now ("Bu adette stok yok", no number); nothing was written (03 2.3.3; 02 3.16.8).</summary>
    NoStock,

    /// <summary>The raised quantity passes the product's per-order limit; the result carries the limit; nothing was written (02 3.16.9).</summary>
    OverOrderLimit,

    /// <summary>The line was taken off the showcase (<c>RemovedProductName</c>): no quantity is written in either direction; the cart's read tells it once and deletes it (K-1179 (3)).</summary>
    TakenOffShowcase,
}
