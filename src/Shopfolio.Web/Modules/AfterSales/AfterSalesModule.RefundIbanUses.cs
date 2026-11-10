using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// What the order's refund IBANs are used by (02 7.4.5; 03 4.2.8; K-1110, K-1127; D-903, D-910): the records, requests and refunds that may still be paid to an
/// IBAN, read once per order and decided in memory for each IBAN. The refund line deletes by <see cref="RefundIbanUses.IsInUse"/>; the deletion day (Z-38)
/// reads the subjects of the same snapshot (<see cref="RefundIbanUses.SubjectsOf"/>).
/// </summary>
internal sealed partial class AfterSalesModule
{
    /// <summary>
    /// Loads the users of the given IBANs of the order in a fixed number of reads, whatever the number of IBANs (09 20 items 2-4): the records linked to them, the
    /// item requests' records without an IBAN of their own, the refunds of all of them, the withdrawals' closed units and receipts, and the order's refunds pending
    /// on the transfer way with the IBAN each would be paid to.
    /// </summary>
    private async Task<RefundIbanUses> LoadRefundIbanUsesAsync(Guid orderId, Guid[] ibanIds, CancellationToken ct)
    {
        List<RequestUse> requests = await _db.RefundIbanRequests
            .Where(r => r.RefundIbanId != null && ibanIds.Contains(r.RefundIbanId.Value))
            .Select(r => new RequestUse(r.OrderItemId, r.RefundId, r.RefundIbanId!.Value, r.FulfilledAt))
            .ToListAsync(ct);
        Guid[] requestItems = [.. requests.Where(r => r.RefundId == null).Select(r => r.OrderItemId).OfType<Guid>().Distinct()];

        // A cancellation undone by the correction of the firm's wrong cancellation needs no refund any more (06 4.22; K-1056, K-1131).
        Guid[] linkedItems = [.. await _db.Cancellations.Where(c => c.RefundIbanId != null && ibanIds.Contains(c.RefundIbanId.Value)).Select(c => c.OrderItemId).Distinct().ToListAsync(ct)];
        List<RecordUse> cancellations = [.. (await StandingCancellationsAsync([.. linkedItems.Union(requestItems)], ct))
            .Where(c => (c.RefundIbanId is { } linked && ibanIds.Contains(linked)) || (c.RefundIbanId is null && requestItems.Contains(c.OrderItemId)))
            .Select(c => new RecordUse(c.Id, c.OrderItemId, c.RefundIbanId))];
        List<RecordUse> terminations = await _db.LateDeliveryTerminations
            .Where(t => (t.RefundIbanId != null && ibanIds.Contains(t.RefundIbanId.Value)) || (t.RefundIbanId == null && requestItems.Contains(t.OrderItemId)))
            .Select(t => new RecordUse(t.Id, t.OrderItemId, t.RefundIbanId))
            .ToListAsync(ct);
        List<RecordUse> removals = await _db.ItemRemovals.Where(r => requestItems.Contains(r.OrderItemId)).Select(r => new RecordUse(r.Id, r.OrderItemId, null)).ToListAsync(ct);
        List<WithdrawalUse> withdrawals = await _db.Withdrawals
            .Where(w => (w.RefundIbanId != null && ibanIds.Contains(w.RefundIbanId.Value)) || (w.RefundIbanId == null && requestItems.Contains(w.OrderItemId)))
            .Select(w => new WithdrawalUse(w.Id, w.OrderItemId, w.Quantity, w.RefundIbanId, w.DeclarationDate, w.IsDeliveredBeforeDeclaration))
            .ToListAsync(ct);
        Guid[] withdrawalIds = [.. withdrawals.Select(w => w.Id)];
        List<ReceiptUse> receipts = withdrawalIds.Length == 0
            ? []
            : await _db.Returns.Where(r => r.WithdrawalId != null && withdrawalIds.Contains(r.WithdrawalId.Value)).Select(r => new ReceiptUse(r.Id, r.WithdrawalId!.Value)).ToListAsync(ct);

        List<Guid> asked = [.. cancellations.Select(c => c.Id), .. terminations.Select(t => t.Id), .. removals.Select(r => r.Id), .. withdrawalIds, .. requestItems];
        asked.AddRange(requests.Select(r => r.RefundId).OfType<Guid>());
        IReadOnlyList<RefundOfRecord> refunds = await _payment.FindRefundsOfRecordsAsync(asked, ct);
        Dictionary<Guid, int> closed = await ClosedWithdrawalUnitsAsync(withdrawalIds, ct);
        IReadOnlyList<PendingPayee> pending = await PendingTransferPayeesAsync(orderId, ct);
        return new RefundIbanUses(cancellations, terminations, removals, withdrawals, receipts, requests, refunds, closed, pending);
    }

    /// <summary>
    /// The order's refunds still pending on the transfer way - neither sent nor voided, whatever their source: a record's, a defect remedy's rescission or price
    /// reduction (T374), the late payment's - and where each would be paid (02 7.4.5): to its record's IBAN; without one, to the IBANs fulfilled on its own
    /// requests, then on its item's requests; without any, to the IBAN on the order's file - so it holds every IBAN of the order.
    /// </summary>
    private async Task<IReadOnlyList<PendingPayee>> PendingTransferPayeesAsync(Guid orderId, CancellationToken ct)
    {
        IReadOnlyList<RefundOfRecord> pending = await _payment.FindPendingTransferRefundsAsync(orderId, ct);
        if (pending.Count == 0)
        {
            return [];
        }

        Guid[] cancellationIds = [.. pending.Select(r => r.CancellationId).OfType<Guid>()];
        Guid[] terminationIds = [.. pending.Select(r => r.LateDeliveryTerminationId).OfType<Guid>()];
        Guid[] withdrawalIds = [.. pending.Select(r => r.WithdrawalId).OfType<Guid>()];
        Dictionary<Guid, Guid> recordIbans = new(
        [
            .. await _db.Cancellations.Where(c => cancellationIds.Contains(c.Id) && c.RefundIbanId != null)
                .Select(c => new KeyValuePair<Guid, Guid>(c.Id, c.RefundIbanId!.Value)).ToListAsync(ct),
            .. await _db.LateDeliveryTerminations.Where(t => terminationIds.Contains(t.Id) && t.RefundIbanId != null)
                .Select(t => new KeyValuePair<Guid, Guid>(t.Id, t.RefundIbanId!.Value)).ToListAsync(ct),
            .. await _db.Withdrawals.Where(w => withdrawalIds.Contains(w.Id) && w.RefundIbanId != null)
                .Select(w => new KeyValuePair<Guid, Guid>(w.Id, w.RefundIbanId!.Value)).ToListAsync(ct),
        ]);

        Guid[] refundIds = [.. pending.Select(r => r.RefundId)];
        Guid[] itemIds = [.. pending.Select(r => r.OrderItemId).OfType<Guid>().Distinct()];
        List<RequestUse> fulfilled = await _db.RefundIbanRequests
            .Where(r => r.RefundIbanId != null
                && ((r.RefundId != null && refundIds.Contains(r.RefundId.Value)) || (r.RefundId == null && r.OrderItemId != null && itemIds.Contains(r.OrderItemId.Value))))
            .Select(r => new RequestUse(r.OrderItemId, r.RefundId, r.RefundIbanId!.Value, r.FulfilledAt))
            .ToListAsync(ct);

        List<PendingPayee> payees = [];
        foreach (RefundOfRecord refund in pending)
        {
            Guid? recordId = refund.CancellationId ?? refund.LateDeliveryTerminationId ?? refund.WithdrawalId;
            if (recordId is { } record && recordIbans.TryGetValue(record, out Guid recordIban))
            {
                payees.Add(new PendingPayee(refund.RefundId, recordIban, []));
                continue;
            }

            List<Guid> own = [.. fulfilled.Where(r => r.RefundId == refund.RefundId).Select(r => r.RefundIbanId)];
            if (own.Count == 0 && refund.OrderItemId is { } itemId)
            {
                own = [.. fulfilled.Where(r => r.RefundId == null && r.OrderItemId == itemId).Select(r => r.RefundIbanId)];
            }

            payees.Add(new PendingPayee(refund.RefundId, null, own));
        }

        return payees;
    }

    /// <summary>
    /// The withdrawal records whose refund is born with the goods' receipt (03 4.1.17; Z-16): a physical item's declaration after its delivery. Before the
    /// delivery and on a service item the refund is born with the declaration. The item's type and delivery date are the order module's (09 6).
    /// </summary>
    private async Task<IReadOnlySet<Guid>> WithdrawalsAwaitingGoodsAsync(IReadOnlyList<WithdrawalUse> withdrawals, CancellationToken ct)
    {
        if (withdrawals.Count == 0)
        {
            return new HashSet<Guid>();
        }

        Dictionary<Guid, RefundWorkItemFacts> items = (await _ordering.FindRefundWorkItemFactsAsync([.. withdrawals.Select(w => w.OrderItemId).Distinct()], ct))
            .ToDictionary(f => f.OrderItemId);
        return withdrawals
            .Where(w => items.TryGetValue(w.OrderItemId, out RefundWorkItemFacts? item)
                && AfterSalesPeriodRules.IsAfterDelivery(item.ProductType, item.DeliveryDate, w.DeclarationDate, w.IsDeliveredBeforeDeclaration))
            .Select(w => w.Id)
            .ToHashSet();
    }

    /// <summary>An item record that may be paid to an IBAN: its id, item and own IBAN (none: the item's request IBAN pays it).</summary>
    private sealed record RecordUse(Guid Id, Guid OrderItemId, Guid? RefundIbanId);

    /// <summary>A withdrawal that may be paid to an IBAN: its units, own IBAN, and what tells whether its refund waits for the goods.</summary>
    private sealed record WithdrawalUse(Guid Id, Guid OrderItemId, int Quantity, Guid? RefundIbanId, DateOnly DeclarationDate, bool? IsDeliveredBeforeDeclaration);

    /// <summary>A goods' receipt of a withdrawal: the subject of the receipt's refund.</summary>
    private sealed record ReceiptUse(Guid Id, Guid WithdrawalId);

    /// <summary>A request fulfilled with an IBAN: its item or refund, the IBAN and the moment it came.</summary>
    private sealed record RequestUse(Guid? OrderItemId, Guid? RefundId, Guid RefundIbanId, DateTimeOffset? FulfilledAt);

    /// <summary>A refund pending on the transfer way: the record's IBAN it is paid to, or else the IBANs of its requests (none: every IBAN of the order).</summary>
    private sealed record PendingPayee(Guid RefundId, Guid? RecordIban, IReadOnlyList<Guid> RequestIbans)
    {
        public bool PaysTo(Guid ibanId) => RecordIban is { } record ? record == ibanId : RequestIbans.Count == 0 || RequestIbans.Contains(ibanId);
    }

    /// <summary>The users of the order's IBANs as one read loaded them; each IBAN is decided here without a further query.</summary>
    private sealed class RefundIbanUses(
        List<RecordUse> cancellations,
        List<RecordUse> terminations,
        List<RecordUse> removals,
        List<WithdrawalUse> withdrawals,
        List<ReceiptUse> receipts,
        List<RequestUse> requests,
        IReadOnlyList<RefundOfRecord> refunds,
        Dictionary<Guid, int> closedWithdrawalUnits,
        IReadOnlyList<PendingPayee> pending)
    {
        public IReadOnlyList<WithdrawalUse> Withdrawals => withdrawals;

        /// <summary>
        /// Whether a pending refund still uses the IBAN (02 7.4.5; K-1110, K-1127): the one definition the refund line deletes by.
        /// <list type="bullet">
        /// <item>a refund of the order pending on the transfer way, whatever its source, that would be paid to it;</item>
        /// <item>a cancellation or a late delivery termination linked to it whose refund is not settled - on the transfer line the refund is born with the firm's
        /// step, so a record without a refund is pending;</item>
        /// <item>a withdrawal linked to it with units neither paid back by a settled refund nor closed by a rejection or a "goods did not come back" closure -
        /// the receipt-by-receipt refunds of a later receipt (02 7.1.7);</item>
        /// <item>a request fulfilled with it: on a refund (the transfer path of a failed card refund, the late payment), until that refund settles; on an item,
        /// while the item has a record without an IBAN of its own whose refund is not settled, or until an item refund settles after the IBAN came.</item>
        /// </list>
        /// </summary>
        public bool IsInUse(Guid ibanId)
        {
            if (pending.Any(p => p.PaysTo(ibanId)))
            {
                return true;
            }

            if (cancellations.Any(c => c.RefundIbanId == ibanId && RecordPending(c.Id))
                || terminations.Any(t => t.RefundIbanId == ibanId && RecordPending(t.Id))
                || withdrawals.Any(w => w.RefundIbanId == ibanId && WithdrawalPending(w)))
            {
                return true;
            }

            foreach (RequestUse request in requests.Where(r => r.RefundIbanId == ibanId))
            {
                if (request.RefundId is { } refundId)
                {
                    if (!Settled(r => r.RefundId == refundId))
                    {
                        return true;
                    }

                    continue;
                }

                if (request.OrderItemId is not { } itemId)
                {
                    continue;
                }

                List<RecordUse> unlinked = [.. UnlinkedRecordsOf(itemId)];
                List<WithdrawalUse> unlinkedWithdrawals = [.. withdrawals.Where(w => w.RefundIbanId is null && w.OrderItemId == itemId)];

                // An item whose records were all undone by a status correction owes nothing any more (D-517): the request no longer holds the IBAN.
                if (unlinked.Count == 0 && unlinkedWithdrawals.Count == 0)
                {
                    continue;
                }

                bool itemRecordPending = unlinked.Any(u => RecordPending(u.Id)) || unlinkedWithdrawals.Any(WithdrawalPending);
                bool settledAfter = refunds.Any(r => r.OrderItemId == itemId && r.SettledAt >= request.FulfilledAt);
                if (itemRecordPending || !settledAfter)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// The refund subjects that may still be paid to the IBAN, as the deletion day (Z-38) reads them (03 4.1.47, 4.2.8; D-910): the cancellations and late
        /// delivery terminations linked to it; its withdrawals - a goods' receipt where the refund waits for the goods (no receipt, no subject: Z-38's case), the
        /// declaration itself before the delivery and on a service item; the refund a request fulfilled with it names (a failed card refund, the late payment);
        /// the records without an IBAN of their own of an item whose request it fulfilled; and the order's pending transfer refunds that would be paid to it.
        /// </summary>
        public IReadOnlyList<Guid> SubjectsOf(Guid ibanId, IReadOnlySet<Guid> awaitingGoods)
        {
            List<Guid> subjects = [];
            subjects.AddRange(cancellations.Where(c => c.RefundIbanId == ibanId).Select(c => c.Id));
            subjects.AddRange(terminations.Where(t => t.RefundIbanId == ibanId).Select(t => t.Id));
            subjects.AddRange(withdrawals.Where(w => w.RefundIbanId == ibanId).SelectMany(w => WithdrawalSubjects(w, awaitingGoods)));
            foreach (RequestUse request in requests.Where(r => r.RefundIbanId == ibanId))
            {
                if (request.RefundId is { } refundId)
                {
                    subjects.Add(refundId);
                }
                else if (request.OrderItemId is { } itemId)
                {
                    subjects.AddRange(UnlinkedRecordsOf(itemId).Select(u => u.Id));
                    subjects.AddRange(withdrawals.Where(w => w.RefundIbanId is null && w.OrderItemId == itemId).SelectMany(w => WithdrawalSubjects(w, awaitingGoods)));
                }
            }

            subjects.AddRange(pending.Where(p => p.PaysTo(ibanId)).Select(p => p.RefundId));
            return [.. subjects.Distinct()];
        }

        private IEnumerable<Guid> WithdrawalSubjects(WithdrawalUse withdrawal, IReadOnlySet<Guid> awaitingGoods) =>
            awaitingGoods.Contains(withdrawal.Id) ? receipts.Where(r => r.WithdrawalId == withdrawal.Id).Select(r => r.Id) : [withdrawal.Id];

        /// <summary>The item's records that carry no IBAN of their own: an item request's IBAN pays them.</summary>
        private IEnumerable<RecordUse> UnlinkedRecordsOf(Guid itemId) =>
            cancellations.Concat(terminations).Where(r => r.RefundIbanId is null && r.OrderItemId == itemId).Concat(removals.Where(r => r.OrderItemId == itemId));

        private bool Settled(Func<RefundOfRecord, bool> of) => refunds.Any(r => r.IsSettled && of(r));

        private bool RecordPending(Guid id) => !Settled(r => r.CancellationId == id || r.ItemRemovalId == id || r.LateDeliveryTerminationId == id);

        private bool WithdrawalPending(WithdrawalUse withdrawal) =>
            refunds.Where(r => r.IsSettled && r.WithdrawalId == withdrawal.Id).Sum(r => r.Quantity ?? 0) + closedWithdrawalUnits.GetValueOrDefault(withdrawal.Id) < withdrawal.Quantity;
    }
}
