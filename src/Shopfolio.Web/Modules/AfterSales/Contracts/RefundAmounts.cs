using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// The item record a refund pays back, as the refund's own links name it (06 4.33 type links): the refund type - <see cref="RefundType.Cancellation"/>,
/// <see cref="RefundType.ItemRemoval"/>, <see cref="RefundType.LateDeliveryTermination"/> or <see cref="RefundType.Withdrawal"/> - and the record's id; on the
/// withdrawal after the delivery <paramref name="ReturnId"/> is the receipt that causes the refund (K-1057), empty on the declaration's own refund.
/// </summary>
public sealed record RefundSubject(RefundType Type, Guid RecordId, Guid? ReturnId = null);

/// <summary>
/// What a record's refund pays back (02 7.1.7; 06 4.33 <c>Quantity</c>, <c>Amount</c>): the units, the product amount, the coupon share and the VAT set aside for
/// them; <see cref="Amount"/> is the product amount less the coupon share. The shipping fee is added apart (<see cref="IAfterSalesModule.IsShippingFeeIncludedAsync"/>).
/// </summary>
public sealed record RefundAmount(int Quantity, decimal ProductAmount, decimal CouponShare, decimal VatAmount)
{
    public decimal Amount => ProductAmount - CouponShare;
}

/// <summary>An order's physical item as the shipping fee rule reads it: its id, units and delivery date (null while not delivered).</summary>
public sealed record ShippedItem(Guid OrderItemId, int Quantity, DateOnly? DeliveryDate);

/// <summary>
/// The question of the shipping fee rule (02 7.2.9, 7.4.6): all of the order's physical items, the moment the carrier took the order (null: it never did - the
/// "Kargoya verildi" transition's moment), and the record whose refund is asked about.
/// </summary>
public sealed record ShippingFeeQuery(IReadOnlyCollection<ShippedItem> PhysicalItems, DateTimeOffset? HandedToCarrierAt, RefundSubject Subject);
