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
}
