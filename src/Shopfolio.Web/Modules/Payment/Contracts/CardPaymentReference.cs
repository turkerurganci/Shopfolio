namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>
/// The provider's reference of an order's card payment (08 2.1.4, 2.1.5, 2.1.6): the provider has no transaction id of its own, so the payment record keeps <c>merchant_oid</c> - the order number
/// without its dash (<c>2026-7K4M9P</c> → <c>20267K4M9P</c>), the value sent and the value the notification brings back. A payment the installer applies from the provider's panel
/// (05 8.5.12) carries the same reference, so the same payment arriving later by the notification or the final query is a repeat and is not applied twice (05 4.5.3; K-970).
/// </summary>
public static class CardPaymentReference
{
    /// <summary>The rule's one home is the order number's (<see cref="Infrastructure.Security.OrderNumber.WithoutSeparator"/>); the link layer sends the same value.</summary>
    public static string OfOrderNumber(string orderNumber) => Infrastructure.Security.OrderNumber.WithoutSeparator(orderNumber);
}
