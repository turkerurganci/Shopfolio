namespace Shopfolio.Modules.Ordering;

/// <summary>The nine moments of 03 1.4.4 at which a transition moves the stock, the service capacity or the coupon right (02 4.3).</summary>
internal enum CounterMoment
{
    /// <summary>1.4.4.1: the order is born (Placed + Pending).</summary>
    OrderPlaced,

    /// <summary>1.4.4.2: the payment's approval (Ö1).</summary>
    PaymentApproved,

    /// <summary>1.4.4.3: the unpaid cancellation (S3 + Ö2).</summary>
    UnpaidCancellation,

    /// <summary>1.4.4.4: a paid item's cancellation and the removal.</summary>
    PaidItemCancellation,

    /// <summary>1.4.4.5: the late delivery termination.</summary>
    LateDeliveryTermination,

    /// <summary>1.4.4.6: the withdrawal and the return.</summary>
    WithdrawalAndReturn,

    /// <summary>1.4.4.7: the wrong cancellation's correction.</summary>
    CancellationCorrection,

    /// <summary>1.4.4.8: the transfer mark's correction.</summary>
    TransferMarkCorrection,

    /// <summary>1.4.4.9: the variant's permanent deletion.</summary>
    VariantDeletion,
}

/// <summary>What a moment does to the stock and the service capacity (03 1.4.4, the second column).</summary>
internal enum StockEffect
{
    /// <summary>Reserved (06 4.14 path 1).</summary>
    Reserved,

    /// <summary>The reserved units fall finally (path 2).</summary>
    Committed,

    /// <summary>The reservation is freed (path 3).</summary>
    Released,

    /// <summary>The cancelled or removed units go back by themselves.</summary>
    Returned,

    /// <summary>Back by themselves only for an item not shipped; goods coming back from the carrier enter with the receipt's "add to stock".</summary>
    ReturnedUnlessShipped,

    /// <summary>Physical goods by the received quantity with the firm's "add to stock", rejected goods not; service capacity by the withdrawn quantity.</summary>
    ReturnedOnReceipt,

    /// <summary>Taken again, the correction refused when not enough (path 6).</summary>
    Retaken,

    /// <summary>Kept: the finally decreased units go back to the reservation (path 5).</summary>
    KeptReserved,

    /// <summary>The deleted variant's reservation drops with it (path 4).</summary>
    ReservationDropped,
}

/// <summary>What a moment does to the coupon right (03 1.4.4, the third column).</summary>
internal enum CouponEffect
{
    /// <summary>Reserved.</summary>
    Reserved,

    /// <summary>Counts as used.</summary>
    Used,

    /// <summary>Given back while unpaid.</summary>
    Released,

    /// <summary>Given back only by the record that brings every unit of the order to its last state, and never after a rejected or not returned unit.</summary>
    ReturnedWhenOrderFinal,

    /// <summary>Counts as used again.</summary>
    UsedAgain,

    /// <summary>Kept: the used right goes back to a reserved one.</summary>
    KeptReserved,

    /// <summary>Untouched.</summary>
    None,
}

/// <summary>A row of the moment table: the document's number, the moment and its two effects.</summary>
internal sealed record CounterMomentRow(string Number, CounterMoment Moment, StockEffect Stock, CouponEffect Coupon);

/// <summary>
/// The moment table of 03 1.4.4 (02 4.3; 06 4.14): the one list of what each transition moment does to the counters. <see cref="OrderCounters"/>
/// is its one writer on the order's side - a method per row - and the transition that owns the moment calls it in its own transaction (09 9.1
/// item 6). The equality with the document is <c>CounterMomentTableConsistencyTests</c>.
/// </summary>
internal static class CounterMomentTable
{
    public static IReadOnlyList<CounterMomentRow> Rows { get; } =
    [
        new("1.4.4.1", CounterMoment.OrderPlaced, StockEffect.Reserved, CouponEffect.Reserved),
        new("1.4.4.2", CounterMoment.PaymentApproved, StockEffect.Committed, CouponEffect.Used),
        new("1.4.4.3", CounterMoment.UnpaidCancellation, StockEffect.Released, CouponEffect.Released),
        new("1.4.4.4", CounterMoment.PaidItemCancellation, StockEffect.Returned, CouponEffect.ReturnedWhenOrderFinal),
        new("1.4.4.5", CounterMoment.LateDeliveryTermination, StockEffect.ReturnedUnlessShipped, CouponEffect.ReturnedWhenOrderFinal),
        new("1.4.4.6", CounterMoment.WithdrawalAndReturn, StockEffect.ReturnedOnReceipt, CouponEffect.ReturnedWhenOrderFinal),
        new("1.4.4.7", CounterMoment.CancellationCorrection, StockEffect.Retaken, CouponEffect.UsedAgain),
        new("1.4.4.8", CounterMoment.TransferMarkCorrection, StockEffect.KeptReserved, CouponEffect.KeptReserved),
        new("1.4.4.9", CounterMoment.VariantDeletion, StockEffect.ReservationDropped, CouponEffect.None),
    ];

    public static CounterMomentRow Of(CounterMoment moment) => Rows.Single(r => r.Moment == moment);
}
