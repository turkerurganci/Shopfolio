namespace Shopfolio.Modules.Settings.Contracts.Gates;

/// <summary>What a surface may do while the gate is closed or open (02 3.1.5, 3.1.6, 6.1.4). The first three are never closed by the gate.</summary>
public enum SalesGateOperation
{
    /// <summary>The storefront and the corporate pages: products stay visible (02 3.1.5).</summary>
    BrowseShowcase,

    /// <summary>The visitor's existing cart: kept and shown as it is (02 3.1.6, 6.1.4).</summary>
    KeepExistingCart,

    /// <summary>An order that is already open: runs on, the bank transfer approval included (02 3.1.6).</summary>
    ContinueOpenOrder,

    /// <summary>Adding a product to the cart.</summary>
    AddToCart,

    /// <summary>Moving from the cart to the payment step.</summary>
    ProceedToCheckout,

    /// <summary>Confirming a new order; a guest order too.</summary>
    ConfirmOrder,
}
