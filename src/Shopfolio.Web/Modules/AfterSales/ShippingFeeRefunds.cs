using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The units one record of an order's physical item moves at its moment, as the shipping fee rule counts them: cancelled or removed before the carrier took the order, or
/// back with the firm after a withdrawal (a declaration before the delivery, a receipt after it).
/// </summary>
internal readonly record struct UnitMove(Guid OrderItemId, Guid RecordId, DateTimeOffset At, int Quantity);

/// <summary>
/// Whether a refund carries the order's frozen shipping fee (02 7.2.9, 7.4.6; 06 4.33 <c>IsShippingFeeIncluded</c>; K-501, K-789, K-1076, K-1107): the one place the
/// rule is written, pure functions over the records' unit moves. The fee is never split over units; it goes whole to the one refund at the trigger, or to none.
/// (1) Cancellation before the carrier took the order: the trigger is all the units of all the physical items cancelled or removed - customer or firm, at once or one after
/// the other; digital and service items may stay. A single unit that still goes keeps the fee. (2) The S9 cancellation of an undelivered parcel: the fee is paid back
/// unless the firm's reason is "gönderi teslim edilemedi - müşteri kaynaklı"; the carriage was done. (3) The late delivery termination: always. (4) Withdrawal: the
/// trigger is all the units the carrier took - the physical items' units less those cancelled before it - back with the firm: declared before the delivery, received after
/// it; a unit whose return was rejected or closed as not come back stayed with the customer and keeps the fee. When the record that meets the trigger is one of several
/// written at the same moment (one operation over several items), the fee goes to the one of the smallest item id (K-1107) - the uuid order of the database.
/// </summary>
internal static class ShippingFeeRefunds
{
    /// <summary>(1): <paramref name="subject"/> is one of <paramref name="cancelled"/>, the cancellations and removals before the carrier took the order.</summary>
    public static bool OnCancellationBeforeShipment(IReadOnlyDictionary<Guid, int> physicalUnits, IReadOnlyCollection<UnitMove> cancelled, UnitMove subject) =>
        IsTriggeredBy(physicalUnits, cancelled, subject);

    /// <summary>
    /// (2): the firm's S9 cancellation, after the carrier took the order - <paramref name="afterShipment"/> are the cancellations after it; a cancellation without a reason
    /// after the shipment is not S9 and carries no fee. Of the cancellations of one operation the first of its moment carries it.
    /// </summary>
    public static bool OnCancellationAfterShipment(CancellationReason? reason, IReadOnlyCollection<UnitMove> afterShipment, UnitMove subject) =>
        reason is { } given && given != CancellationReason.UndeliveredBuyerCaused && IsFirstOfItsMoment(afterShipment, subject);

    /// <summary>(3): always; of the terminations written at one moment the first carries it.</summary>
    public static bool OnTermination(IReadOnlyCollection<UnitMove> terminations, UnitMove subject) => IsFirstOfItsMoment(terminations, subject);

    /// <summary>
    /// (1) read before the customer's last step (T512; 04 5.17.5; K-789): whether cancelling <paramref name="selectedUnits"/> now meets the trigger, with the physical
    /// items' open units as the units to cover and the selection as the moves of one moment. Units already closed are not in <paramref name="openPhysicalUnits"/>, so the
    /// moves before this moment did not meet the trigger - it is met now or not at all. An order with no open physical unit has no trigger.
    /// </summary>
    public static bool OnCancellationPreview(IReadOnlyDictionary<Guid, int> openPhysicalUnits, IReadOnlyDictionary<Guid, int> selectedUnits)
    {
        ArgumentNullException.ThrowIfNull(openPhysicalUnits);
        ArgumentNullException.ThrowIfNull(selectedUnits);
        DateTimeOffset moment = DateTimeOffset.UnixEpoch;
        List<UnitMove> moves =
        [
            .. selectedUnits.Where(s => s.Value > 0 && openPhysicalUnits.ContainsKey(s.Key)).Select(s => new UnitMove(s.Key, s.Key, moment, s.Value)),
        ];
        return moves.Exists(m => OnCancellationBeforeShipment(openPhysicalUnits, moves, m));
    }

    private static bool IsFirstOfItsMoment(IReadOnlyCollection<UnitMove> moves, UnitMove subject)
    {
        ArgumentNullException.ThrowIfNull(moves);
        return moves.Contains(subject) && FirstAt(moves, subject.At) == subject;
    }

    /// <summary>
    /// (4): <paramref name="shippedUnits"/> are the units the carrier took per physical item, <paramref name="back"/> the units back with the firm and
    /// <paramref name="keptUnits"/> the units whose return was rejected or closed as not come back (a reopened closure no longer counts).
    /// </summary>
    public static bool OnWithdrawal(IReadOnlyDictionary<Guid, int> shippedUnits, IReadOnlyCollection<UnitMove> back, int keptUnits, UnitMove subject) =>
        keptUnits == 0 && IsTriggeredBy(shippedUnits, back, subject);

    /// <summary>
    /// The trigger is met at <paramref name="subject"/>'s moment: the moves up to it cover every item's units and the moves before it did not; and the subject is the first
    /// of the moves at that moment. An order without units to cover has no trigger.
    /// </summary>
    internal static bool IsTriggeredBy(IReadOnlyDictionary<Guid, int> units, IReadOnlyCollection<UnitMove> moves, UnitMove subject)
    {
        ArgumentNullException.ThrowIfNull(units);
        ArgumentNullException.ThrowIfNull(moves);
        if (!units.Values.Any(q => q > 0) || !units.ContainsKey(subject.OrderItemId) || !moves.Contains(subject))
        {
            return false;
        }

        bool CoveredBy(Func<UnitMove, bool> counted) =>
            units.All(u => u.Value <= 0 || moves.Where(m => m.OrderItemId == u.Key && counted(m)).Sum(m => m.Quantity) >= u.Value);

        if (!CoveredBy(m => m.At <= subject.At) || CoveredBy(m => m.At < subject.At))
        {
            return false;
        }

        return FirstAt([.. moves.Where(m => units.ContainsKey(m.OrderItemId))], subject.At) == subject;
    }

    private static UnitMove FirstAt(IReadOnlyCollection<UnitMove> moves, DateTimeOffset at) =>
        moves.Where(m => m.At == at).OrderBy(m => Uuid(m.OrderItemId), StringComparer.Ordinal).ThenBy(m => Uuid(m.RecordId), StringComparer.Ordinal).First();

    // The database orders uuids by their bytes as written; the hexadecimal text keeps that order, the framework's Guid comparison does not.
    private static string Uuid(Guid id) => id.ToString("N");
}
