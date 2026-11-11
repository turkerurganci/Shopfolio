namespace Shopfolio.Modules.AfterSales.Contracts;

/// <summary>
/// What the customer's operation screen says about the refund before the last step (T512; 04 5.17.5; K-789): the period in days and whether the selection pays the
/// shipping fee back. Both are the rule homes' answers - the period is <see cref="AfterSalesPeriodRules.RefundPeriodDays"/> (Z-17 for the cancellation, 02 7.2.3's
/// fourteen days for the termination) and the fee is <see cref="ShippingFeeRefunds"/>'s trigger (1) read over the units still open - so the screen works out
/// neither itself (09 3.4 item 1).
/// </summary>
public static class RefundPreview
{
    /// <summary>The refund period of a cancellation and of a late delivery termination in calendar days (02 4.2 Z-17; 02 7.2.3).</summary>
    public const int RefundPeriodDays = AfterSalesPeriodRules.RefundPeriodDays;

    /// <summary>
    /// Whether a cancellation of <paramref name="selectedUnits"/> before the carrier took the order pays the shipping fee back (02 7.2.9; K-789): the trigger (1) of
    /// <see cref="ShippingFeeRefunds"/> with the order's physical items' open units as the units to cover and the selection as the moves of one moment. Units already
    /// closed are not in <paramref name="openPhysicalUnits"/>, so the moves before this moment did not meet the trigger - it is met now or not at all. An order with no
    /// open physical unit has no trigger.
    /// </summary>
    public static bool CancellationRefundsShippingFee(IReadOnlyDictionary<Guid, int> openPhysicalUnits, IReadOnlyDictionary<Guid, int> selectedUnits)
    {
        ArgumentNullException.ThrowIfNull(openPhysicalUnits);
        ArgumentNullException.ThrowIfNull(selectedUnits);
        DateTimeOffset moment = DateTimeOffset.UnixEpoch;
        List<UnitMove> moves =
        [
            .. selectedUnits.Where(s => s.Value > 0 && openPhysicalUnits.ContainsKey(s.Key)).Select(s => new UnitMove(s.Key, s.Key, moment, s.Value)),
        ];
        return moves.Exists(m => ShippingFeeRefunds.OnCancellationBeforeShipment(openPhysicalUnits, moves, m));
    }
}
