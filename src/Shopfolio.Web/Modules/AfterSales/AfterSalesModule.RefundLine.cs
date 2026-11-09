using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The refund line's IBAN (T370; 02 7.2.8, 7.4.5; 06 4.29, 4.31; Z-44; K-356, K-569, K-575, K-690, K-1110): the requests that open the order page's IBAN
/// field, the customer's entry and correction, the IBAN's deletion when no pending refund uses it, and the firm's "havale gerçekleşmedi". The order module
/// holds the order's row lock and calls these in its transaction; nothing here saves.
/// </summary>
internal sealed partial class AfterSalesModule
{
    /// <summary>The IBAN's one format rule (04 7.3.2.7): the field rule the order page's form uses, asked again by the writer.</summary>
    private static readonly InvalidIbanAttribute IbanRule = new();

    public async Task<RefundIbanRequestOpened> OpenRefundIbanRequestAsync(RefundIbanRequestEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);

        // At most one open request per item and, itemless, per refund (06 7.1): a second event uses the open request - the field is already open.
        RefundIbanRequest? open = entry.OrderItemId is { } itemId
            ? await OpenRequestsTracked().FirstOrDefaultAsync(r => r.OrderItemId == itemId, ct)
            : await OpenRequestsTracked().FirstOrDefaultAsync(r => r.OrderItemId == null && r.RefundId == entry.RefundId, ct);
        open ??= _db.RefundIbanRequests.Local.FirstOrDefault(r => r.FulfilledAt is null && r.ClosedAt is null
            && (entry.OrderItemId is not null ? r.OrderItemId == entry.OrderItemId : r.OrderItemId is null && r.RefundId == entry.RefundId));
        if (open is not null)
        {
            return new RefundIbanRequestOpened(open.Id, false);
        }

        RefundIbanRequest request = entry.OrderItemId is { } item
            ? RefundIbanRequest.ForItem(item, entry.Reason, entry.RefundId, entry.AdminId, entry.AdminName, entry.At)
            : RefundIbanRequest.ForRefund(
                entry.RefundId ?? throw new ArgumentException("An itemless request names its refund (K-1075).", nameof(entry)),
                entry.Reason,
                entry.AdminId,
                entry.AdminName,
                entry.At);
        _db.RefundIbanRequests.Add(request);
        return new RefundIbanRequestOpened(request.Id, true);
    }

    public async Task<OpenRefundIbanRequest?> FindOpenRefundIbanRequestAsync(Guid requestId, CancellationToken ct)
    {
        return await _db.RefundIbanRequests.AsNoTracking()
            .Where(r => r.Id == requestId && r.FulfilledAt == null && r.ClosedAt == null)
            .Select(r => new OpenRefundIbanRequest(r.Id, r.OrderItemId, r.RefundId, r.Reason))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<bool> IsRefundIbanEnteredForRefundAsync(Guid refundId, CancellationToken ct)
    {
        return await _db.RefundIbanRequests.AnyAsync(r => r.RefundId == refundId && r.FulfilledAt != null, ct)
            || _db.RefundIbanRequests.Local.Any(r => r.RefundId == refundId && r.FulfilledAt is not null);
    }

    public async Task<RefundIbanWriteResult> EnterRefundIbanAsync(RefundIbanEntry entry, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(entry);
        RequireLocked(entry.OrderId);
        if (!IbanRule.Allows(entry.Iban))
        {
            return RefundIbanWriteResult.InvalidIban;
        }

        RefundIbanRequest? request = await OpenRequestsTracked().SingleOrDefaultAsync(r => r.Id == entry.RequestId, ct);
        if (request is null)
        {
            return RefundIbanWriteResult.NotFound;
        }

        RefundIban iban = RefundIban.Create(entry.OrderId, RequestFormat.CompactIban(entry.Iban), entry.At);
        _db.RefundIbans.Add(iban);
        request.FulfilledAt = entry.At;
        request.RefundIbanId = iban.Id;
        return new RefundIbanWriteResult(iban.Id, false);
    }

    public async Task<RefundIbanWriteResult> ChangeRefundIbanAsync(RefundIbanCorrection correction, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(correction);
        RequireLocked(correction.OrderId);
        if (!IbanRule.Allows(correction.Iban))
        {
            return RefundIbanWriteResult.InvalidIban;
        }

        // The IBAN lives only while a pending refund uses it (DeleteUnusedRefundIbansAsync): one still on file has not been processed yet.
        RefundIban? iban = await _db.RefundIbans.SingleOrDefaultAsync(i => i.Id == correction.RefundIbanId && i.OrderId == correction.OrderId, ct);
        if (iban is null)
        {
            return RefundIbanWriteResult.NotFound;
        }

        iban.Iban = RequestFormat.CompactIban(correction.Iban);
        iban.EnteredAt = correction.At;
        return new RefundIbanWriteResult(iban.Id, false);
    }

    public async Task CloseRefundIbanRequestsOfRefundAsync(Guid refundId, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        foreach (RefundIbanRequest request in await OpenRequestsTracked().Where(r => r.RefundId == refundId).ToListAsync(ct))
        {
            request.ClosedAt = now;
        }
    }

    public async Task<IReadOnlyList<Guid>> DeleteUnusedRefundIbansAsync(Guid orderId, CancellationToken ct)
    {
        RequireLocked(orderId);
        List<RefundIban> ibans = await _db.RefundIbans.Where(i => i.OrderId == orderId).ToListAsync(ct);
        List<Guid> deleted = [];
        foreach (RefundIban iban in ibans)
        {
            if (!await IsRefundIbanInUseAsync(iban.Id, ct))
            {
                DeleteIban(iban);
                deleted.Add(iban.Id);
            }
        }

        return deleted;
    }

    public async Task<RefundIbanRequestOpened?> ReportTransferFailedAsync(TransferFailure failure, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(failure);
        RequireLocked(failure.OrderId);
        if ((failure.OrderItemId is null) == (failure.RefundId is null))
        {
            throw new ArgumentException("The failed transfer names the item, or the itemless refund of a late payment (K-1131).", nameof(failure));
        }

        // The IBAN the refund would use: the item's records' and the requests fulfilled on the item - or, itemless, on the refund.
        List<Guid?> candidates = failure.OrderItemId is { } itemId
            ? [
                .. await _db.Cancellations.Where(c => c.OrderItemId == itemId).Select(c => c.RefundIbanId).ToListAsync(ct),
                .. await _db.LateDeliveryTerminations.Where(t => t.OrderItemId == itemId).Select(t => t.RefundIbanId).ToListAsync(ct),
                .. await _db.Withdrawals.Where(w => w.OrderItemId == itemId).Select(w => w.RefundIbanId).ToListAsync(ct),
                .. await _db.RefundIbanRequests.Where(r => r.OrderItemId == itemId).Select(r => r.RefundIbanId).ToListAsync(ct),
            ]
            : [.. await _db.RefundIbanRequests.Where(r => r.RefundId == failure.RefundId).Select(r => r.RefundIbanId).ToListAsync(ct)];
        Guid[] ids = [.. candidates.OfType<Guid>().Distinct()];
        List<RefundIban> ibans = await _db.RefundIbans.Where(i => ids.Contains(i.Id) && i.OrderId == failure.OrderId).ToListAsync(ct);
        if (ibans.Count == 0)
        {
            return null;
        }

        // The bank refused the IBAN: it goes for the item, with its disposal record (Z-44); the field opens again with B-14 and the period runs on (02 7.4.5).
        foreach (RefundIban iban in ibans)
        {
            DeleteIban(iban);
        }

        return await OpenRefundIbanRequestAsync(
            new RefundIbanRequestEntry(failure.OrderItemId, failure.RefundId, RefundIbanRequestReason.TransferFailed, failure.AdminId, failure.AdminName, failure.At),
            ct);
    }

    /// <summary>
    /// Whether a pending refund still uses the IBAN (02 7.4.5; K-1110, K-1127): the one definition the refund line deletes by.
    /// <list type="bullet">
    /// <item>a cancellation or a late delivery termination linked to it whose refund is not settled - on the transfer line the refund is born with the firm's
    /// step, so a record without a refund is pending;</item>
    /// <item>a withdrawal linked to it with units neither paid back by a settled refund nor closed by a rejection or a "goods did not come back" closure -
    /// the receipt-by-receipt refunds of a later receipt (02 7.1.7);</item>
    /// <item>a request fulfilled with it: on a refund (the transfer path of a failed card refund, the late payment), until that refund settles; on an item,
    /// while the item has a record without an IBAN of its own whose refund is not settled, or until an item refund settles after the IBAN came.</item>
    /// </list>
    /// </summary>
    private async Task<bool> IsRefundIbanInUseAsync(Guid refundIbanId, CancellationToken ct)
    {
        // A cancellation undone by the correction of the firm's wrong cancellation needs no refund any more (06 4.22; K-1056, K-1131).
        Guid[] linkedItems = [.. await _db.Cancellations.Where(c => c.RefundIbanId == refundIbanId).Select(c => c.OrderItemId).Distinct().ToListAsync(ct)];
        List<Guid> cancellations = [.. (await StandingCancellationsAsync(linkedItems, ct)).Where(c => c.RefundIbanId == refundIbanId).Select(c => c.Id)];
        List<Guid> terminations = await _db.LateDeliveryTerminations.Where(t => t.RefundIbanId == refundIbanId).Select(t => t.Id).ToListAsync(ct);
        List<WithdrawalRef> withdrawals = await _db.Withdrawals.Where(w => w.RefundIbanId == refundIbanId).Select(w => new WithdrawalRef(w.Id, w.OrderItemId, w.Quantity)).ToListAsync(ct);
        List<RequestRef> requests = await _db.RefundIbanRequests.Where(r => r.RefundIbanId == refundIbanId)
            .Select(r => new RequestRef(r.OrderItemId, r.RefundId, r.FulfilledAt))
            .ToListAsync(ct);
        Guid[] requestItems = [.. requests.Where(r => r.RefundId == null).Select(r => r.OrderItemId).OfType<Guid>().Distinct()];

        // The item requests' own users: the item's records that carry no IBAN of their own.
        List<ItemRecordRef> unlinked =
        [
            .. (await StandingCancellationsAsync(requestItems, ct)).Where(c => c.RefundIbanId == null).Select(c => new ItemRecordRef(c.Id, c.OrderItemId)),
            .. await _db.ItemRemovals.Where(r => requestItems.Contains(r.OrderItemId)).Select(r => new ItemRecordRef(r.Id, r.OrderItemId)).ToListAsync(ct),
            .. await _db.LateDeliveryTerminations.Where(t => requestItems.Contains(t.OrderItemId) && t.RefundIbanId == null)
                .Select(t => new ItemRecordRef(t.Id, t.OrderItemId)).ToListAsync(ct),
        ];
        List<WithdrawalRef> unlinkedWithdrawals = await _db.Withdrawals.Where(w => requestItems.Contains(w.OrderItemId) && w.RefundIbanId == null)
            .Select(w => new WithdrawalRef(w.Id, w.OrderItemId, w.Quantity))
            .ToListAsync(ct);

        Guid[] allWithdrawals = [.. withdrawals.Select(w => w.Id), .. unlinkedWithdrawals.Select(w => w.Id)];
        List<Guid> asked = [.. cancellations, .. terminations, .. allWithdrawals, .. unlinked.Select(u => u.Id), .. requestItems];
        asked.AddRange(requests.Select(r => r.RefundId).OfType<Guid>());
        IReadOnlyList<RefundOfRecord> refunds = await _payment.FindRefundsOfRecordsAsync(asked, ct);
        Dictionary<Guid, int> closedByOutcome = await ClosedWithdrawalUnitsAsync(allWithdrawals, ct);

        bool Settled(Func<RefundOfRecord, bool> of) => refunds.Any(r => r.IsSettled && of(r));
        bool WithdrawalPending(Guid id, int quantity) =>
            refunds.Where(r => r.IsSettled && r.WithdrawalId == id).Sum(r => r.Quantity ?? 0) + closedByOutcome.GetValueOrDefault(id) < quantity;
        bool RecordPending(Guid id) => !Settled(r => r.CancellationId == id || r.ItemRemovalId == id || r.LateDeliveryTerminationId == id);

        if (cancellations.Any(RecordPending) || terminations.Any(RecordPending) || withdrawals.Any(w => WithdrawalPending(w.Id, w.Quantity)))
        {
            return true;
        }

        foreach (RequestRef request in requests)
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

            // An item whose records were all undone by a status correction owes nothing any more (D-517): the request no longer holds the IBAN.
            if (!unlinked.Any(u => u.OrderItemId == itemId) && !unlinkedWithdrawals.Any(w => w.OrderItemId == itemId))
            {
                continue;
            }

            bool itemRecordPending = unlinked.Any(u => u.OrderItemId == itemId && RecordPending(u.Id))
                || unlinkedWithdrawals.Any(w => w.OrderItemId == itemId && WithdrawalPending(w.Id, w.Quantity));
            bool settledAfter = refunds.Any(r => r.OrderItemId == itemId && r.SettledAt >= request.FulfilledAt);
            if (itemRecordPending || !settledAfter)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>An item record's id and item, as the IBAN's use reads it.</summary>
    private sealed record ItemRecordRef(Guid Id, Guid OrderItemId);

    /// <summary>A withdrawal's id, item and units, as the IBAN's use reads it.</summary>
    private sealed record WithdrawalRef(Guid Id, Guid OrderItemId, int Quantity);

    /// <summary>A request fulfilled with the IBAN: its item or refund and the moment the IBAN came.</summary>
    private sealed record RequestRef(Guid? OrderItemId, Guid? RefundId, DateTimeOffset? FulfilledAt);

    /// <summary>Units of a withdrawal closed without a refund.</summary>
    private sealed record ClosedPart(Guid WithdrawalId, int Quantity);

    /// <summary>The units of the withdrawals closed without a refund: rejected at the receipt, or closed as "goods did not come back" and not reopened.</summary>
    private async Task<Dictionary<Guid, int>> ClosedWithdrawalUnitsAsync(Guid[] withdrawalIds, CancellationToken ct)
    {
        Dictionary<Guid, int> closed = [];
        if (withdrawalIds.Length == 0)
        {
            return closed;
        }

        List<ClosedPart> rejected = await _db.ReturnRejections.Where(r => withdrawalIds.Contains(r.WithdrawalId)).Select(r => new ClosedPart(r.WithdrawalId, r.Quantity)).ToListAsync(ct);
        List<ClosedPart> notReturned = await _db.NotReturnedClosures.Where(c => withdrawalIds.Contains(c.WithdrawalId) && c.ReopenedAt == null)
            .Select(c => new ClosedPart(c.WithdrawalId, c.Quantity))
            .ToListAsync(ct);
        foreach (ClosedPart part in rejected.Concat(notReturned))
        {
            closed[part.WithdrawalId] = closed.GetValueOrDefault(part.WithdrawalId) + part.Quantity;
        }

        return closed;
    }

    /// <summary>The IBAN goes with its disposal record in the same transaction (Z-44; K-708): the records' and requests' links are emptied by the database.</summary>
    private void DeleteIban(RefundIban iban)
    {
        _db.RefundIbans.Remove(iban);
        _db.DisposalRecords.Add(DisposalRecord.Counted(DisposalRecordType.RefundIban, 1, _time.GetUtcNow()));
    }

    private IQueryable<RefundIbanRequest> OpenRequestsTracked() => _db.RefundIbanRequests.Where(r => r.FulfilledAt == null && r.ClosedAt == null);

    private void RequireLocked(Guid orderId)
    {
        if (_db.Database.CurrentTransaction is null || !LockSequence.LockedOrders(_db).Contains(orderId))
        {
            throw new InvalidOperationException("The refund line's IBAN writes run inside the caller's transaction, under the order's row lock (05 7.3.2; 09 6 item 3).");
        }
    }
}
