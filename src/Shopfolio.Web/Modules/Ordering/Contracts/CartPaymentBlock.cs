namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>Why the payment step does not open (02 3.16.7). It is the <see cref="CartView.PaymentBlock"/> of the cart page's read; a result code, never stored.</summary>
public enum CartPaymentBlock
{
    /// <summary>The payment step may open.</summary>
    None,

    /// <summary>No line of the cart can enter an order: the cart is empty or every line is sold out.</summary>
    NoOrderableItem,

    /// <summary>The order has a physical item and its basis is under the firm's minimum order amount (02 3.18.1; T349): the cart shows the missing amount in place of the button.</summary>
    MinimumOrderAmountNotMet,
}
