using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// What the order records say about one period for the sales summary (02 10.6.2, 10.6.3; 11 T383): the facts only, read from the orders, their items and their transition records
/// at the moment of the call - nothing is stored. The rules that turn them into the summary's numbers and ratios are the Administration module's. The period is the half-open range
/// <c>[From, To)</c> of instants the caller made from Turkey days.
/// </summary>
/// <param name="OrderCount">The orders born in the period whose payment was approved (Paid, PartiallyRefunded or Refunded); a Failed or pending order is not counted.</param>
/// <param name="Revenue">The approved totals of those orders; a later refund is not subtracted (K-821).</param>
/// <param name="PaymentMethods">How many of those orders each payment method has.</param>
/// <param name="Products">The units and amounts of those orders' items per product, variants together (04 9.22.6).</param>
/// <param name="PaidItems">The items of those orders: the cohort of the cancellation and return ratio.</param>
/// <param name="Attempts">The orders of every cart that had an order born in the period - the cart's earlier and later orders too - and the cartless orders born in the period.</param>
/// <param name="ShippingPromises">The orders with a physical item whose shipping promise ran out in the period.</param>
public sealed record SalesPeriodFacts(
    int OrderCount,
    decimal Revenue,
    IReadOnlyList<PaymentMethodCount> PaymentMethods,
    IReadOnlyList<ProductSalesFact> Products,
    IReadOnlyList<OrderOfItem> PaidItems,
    IReadOnlyList<PaymentAttemptOrder> Attempts,
    IReadOnlyList<ShippingPromiseOrder> ShippingPromises);

/// <summary>The number of approved orders paid with one method in the period.</summary>
public sealed record PaymentMethodCount(PaymentMethod Method, int OrderCount);

/// <summary>
/// One product's sales in the period: the units sold and their amount (the item's unit price after the discount times the quantity, less its coupon share). The key is the
/// product's id; an item whose product was deleted keeps its name and has no id.
/// </summary>
public sealed record ProductSalesFact(Guid? ProductId, string ProductName, int Quantity, decimal Amount);

/// <summary>
/// An order as the payment completion ratio reads it (02 10.6.3; K-534, K-1052): its cart (null when the disposal emptied the link or there never was one), its birth, its payment
/// axis and the end of its payment window.
/// </summary>
public sealed record PaymentAttemptOrder(Guid OrderId, Guid? CartId, DateTimeOffset CreatedAt, PaymentStatus PaymentStatus, DateTimeOffset PaymentWindowEndsAt);

/// <summary>
/// An order whose shipping promise ran out in the period (02 10.6.3; K-535): when it ran out (counted from the last uncorrected payment approval), when the order was first
/// shipped (the first uncorrected S5) and when it was cancelled (the uncorrected S3 or S4); either is null when it did not happen.
/// </summary>
public sealed record ShippingPromiseOrder(Guid OrderId, DateTimeOffset PromiseEndsAt, DateTimeOffset? FirstShippedAt, DateTimeOffset? CancelledAt);
