using System.Linq.Expressions;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The rules of the digital item that are not a counter write (02 3.12.1, 3.12.4, 3.12.9): the one reading of "the payment is confirmed" that the download endpoint, the
/// counter and the repurchase check all use, and the quantity of a digital variant. A digital item is delivered by the system when the payment is confirmed
/// (02 3.20.9): the card payment, or the firm's "paid" mark on a bank transfer. Paid and partly refunded are confirmed - a partial refund of other items does not close the
/// delivery (02 3.20.9); Pending and Failed are not, and a fully refunded order is not either (the money went back, the delivery with it).
/// </summary>
internal static class DownloadRules
{
    /// <summary>A digital item is one unit (02 3.12.4; the database's <c>quantity_above_zero_one_if_digital</c> keeps the same rule on the order item).</summary>
    public const int DigitalQuantity = 1;

    /// <summary>The orders whose payment is confirmed (02 3.12.1, 3.20.9); a query condition, translated by the database.</summary>
    public static Expression<Func<Order, bool>> PaymentConfirmed { get; } =
        o => o.PaymentStatus == PaymentStatus.Paid || o.PaymentStatus == PaymentStatus.PartiallyRefunded;

    /// <summary>The same reading for an order already in memory (a unit test, a caller that holds the status).</summary>
    public static bool IsPaymentConfirmed(PaymentStatus status) => status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded;

    /// <summary>Whether the quantity is allowed for the product type: a digital variant is one unit, any other type is the cart's own rules (T347).</summary>
    public static bool IsQuantityAllowed(ProductType type, int quantity) => type != ProductType.Digital || quantity == DigitalQuantity;
}
