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

    public int RefundPeriodDays => AfterSalesPeriodRules.RefundPeriodDays;

    public bool CancellationRefundsShippingFee(IReadOnlyDictionary<Guid, int> openPhysicalUnits, IReadOnlyDictionary<Guid, int> selectedUnits) =>
        ShippingFeeRefunds.OnCancellationPreview(openPhysicalUnits, selectedUnits);

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

    public async Task<IReadOnlyList<RefundIbanField>> ListRefundIbanFieldsAsync(Guid orderId, IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] itemIds = [.. orderItemIds];
        List<RefundIbanField> fields =
        [
            .. await _db.RefundIbanRequests.AsNoTracking()
                .Where(r => r.FulfilledAt == null && r.ClosedAt == null
                    && ((r.OrderItemId != null && itemIds.Contains(r.OrderItemId.Value)) || (r.OrderItemId == null && r.RefundId != null)))
                .OrderBy(r => r.CreatedAt).ThenBy(r => r.Id)
                .Select(r => new RefundIbanField(r.OrderItemId, r.Id, r.RefundId, r.Reason, null, null))
                .ToListAsync(ct),
        ];

        // An IBAN on file names its items through the records that use it: the fulfilled request, the cancellation, the termination, the withdrawal (06 4.22, 4.24,
        // 4.25, 4.29). One without an item (the late payment's refund) is the itemless field.
        foreach (RefundIban iban in await _db.RefundIbans.AsNoTracking().Where(i => i.OrderId == orderId).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).ToListAsync(ct))
        {
            Guid id = iban.Id;
            List<Guid> linked =
            [
                .. await _db.RefundIbanRequests.AsNoTracking().Where(r => r.RefundIbanId == id && r.OrderItemId != null).Select(r => r.OrderItemId!.Value).ToListAsync(ct), // non-null: the Where keeps only requests on an item
                .. await _db.Cancellations.AsNoTracking().Where(c => c.RefundIbanId == id).Select(c => c.OrderItemId).ToListAsync(ct),
                .. await _db.LateDeliveryTerminations.AsNoTracking().Where(t => t.RefundIbanId == id).Select(t => t.OrderItemId).ToListAsync(ct),
                .. await _db.Withdrawals.AsNoTracking().Where(w => w.RefundIbanId == id).Select(w => w.OrderItemId).ToListAsync(ct),
            ];
            Guid[] items = [.. linked.Where(itemIds.Contains).Distinct()];
            if (items.Length == 0)
            {
                fields.Add(new RefundIbanField(null, null, null, null, iban.Id, iban.Iban));
            }

            fields.AddRange(items.Select(itemId => new RefundIbanField(itemId, null, null, null, iban.Id, iban.Iban)));
        }

        return fields;
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
        if (ibans.Count == 0)
        {
            return deleted;
        }

        // The order's records, requests and refunds are read once; each IBAN is decided in memory (D-903; 09 20 items 2-4).
        RefundIbanUses uses = await LoadRefundIbanUsesAsync(orderId, [.. ibans.Select(i => i.Id)], ct);
        foreach (RefundIban iban in ibans)
        {
            if (!uses.IsInUse(iban.Id))
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
        // The rejection or closure the caller added in this transaction counts too: the IBAN goes with it (02 7.4.9, 10.4.9; K-1127).
        IEnumerable<ClosedPart> unsaved = _db.ChangeTracker.Entries<ReturnRejection>()
            .Where(e => e.State == EntityState.Added && withdrawalIds.Contains(e.Entity.WithdrawalId))
            .Select(e => new ClosedPart(e.Entity.WithdrawalId, e.Entity.Quantity))
            .Concat(_db.ChangeTracker.Entries<NotReturnedClosure>()
                .Where(e => e.State == EntityState.Added && e.Entity.ReopenedAt == null && withdrawalIds.Contains(e.Entity.WithdrawalId))
                .Select(e => new ClosedPart(e.Entity.WithdrawalId, e.Entity.Quantity)));
        foreach (ClosedPart part in rejected.Concat(notReturned).Concat(unsaved))
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
