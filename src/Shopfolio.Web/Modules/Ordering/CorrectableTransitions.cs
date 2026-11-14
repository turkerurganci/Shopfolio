using Shopfolio.Infrastructure;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The one rule of which transitions the status correction offers (03 1.11.26; 03 1.6.2): the latest uncorrected shipment-axis record that ends in the order's present status and is one the correction
/// accepts - the shipment, the delivery mark, the "teslim edilemedi" mark, the re-sending, the firm's own cancellation - and the transfer's "paid" mark on a transfer order with no digital item. The
/// operation decision (<see cref="OrderOperations"/>: open when there is one) and the panel's form (T530: the record and the state it goes back to) read it here. The correction itself reads the obstacles
/// of its record (the refund, the stock, the delivery marks) when it runs and refuses what it cannot take back.
/// </summary>
internal static class CorrectableTransitions
{
    private static readonly TransitionCode[] ShipmentCorrectable = [TransitionCode.S3, TransitionCode.S4, TransitionCode.S5, TransitionCode.S6, TransitionCode.S7, TransitionCode.S8, TransitionCode.S9];

    /// <summary>The correctable records of the order, the shipment-axis one first; empty when the correction is not open.</summary>
    public static IReadOnlyList<TransitionRecord> Find(Order order, IReadOnlyList<TransitionRecord> records, bool hasDigitalItem)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(records);
        HashSet<Guid> corrected = [.. records.Where(r => r.CorrectedTransitionRecordId is not null).Select(r => r.CorrectedTransitionRecordId!.Value)]; // non-null: the Where before it keeps only records that correct another
        List<TransitionRecord> found = [];
        TransitionRecord? latest = records
            .Where(r => r.TransitionCode != TransitionCode.Correction && TransitionTable.FindShipment(r.TransitionCode) is not null && !corrected.Contains(r.Id))
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .FirstOrDefault();
        if (latest is not null
            && ShipmentCorrectable.Contains(latest.TransitionCode)
            && TransitionTable.FindShipment(latest.TransitionCode)!.To == order.OrderStatus // non-null: the Where above keeps only records whose code is a shipment transition
            && (latest.TransitionCode is not (TransitionCode.S3 or TransitionCode.S4 or TransitionCode.S9) || latest.ActorKind == ActorKind.Admin))
        {
            found.Add(latest);
        }

        // 1.6.2.4: the transfer's mark goes back only on the transfer line and on an order with no digital item (the download opened).
        if (order.PaymentMethod == PaymentMethod.BankTransfer && order.PaymentStatus == PaymentStatus.Paid && !hasDigitalItem)
        {
            TransitionRecord? paid = records
                .Where(r => r.TransitionCode == TransitionCode.O1 && r.ActorKind == ActorKind.Admin && !corrected.Contains(r.Id))
                .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
                .FirstOrDefault();
            if (paid is not null)
            {
                found.Add(paid);
            }
        }

        return found;
    }
}
