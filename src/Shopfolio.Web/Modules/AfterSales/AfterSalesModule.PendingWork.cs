using Microsoft.EntityFrameworkCore;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// What the panel home's counters read of the item records (T382; 02 10.6.1; 11 T382 criteria 2, 4, 5): the open defect claims, the orders whose returned goods are awaited and the orders
/// that owe the firm a refund step. All are plain reads at the moment of the call; nothing is written, locked or kept - the counters are computed, not stored (K-405, K-467).
/// </summary>
internal sealed partial class AfterSalesModule
{
    private const int AwaitedPageSize = 500;

    public Task<int> CountOpenDefectClaimsAsync(CancellationToken ct) =>
        _db.DefectClaims.AsNoTracking().CountAsync(c => c.DefectClaimStatus == DefectClaimStatus.Open, ct);

    public async Task<IReadOnlyList<Guid>> ListOrdersAwaitingReturnGoodsAsync(CancellationToken ct)
    {
        // The units a withdrawal or a termination still awaits, counted by the database: declared minus received, rejected and closed as "mal dönmedi" (a closure that was
        // reopened is not closed). A closed or rejected unit is no longer awaited (02 10.4.9, 7.4.9; criterion 5); a reopened one is awaited again.
        // Both sets are walked one page of records at a time by id (09 20 item 2): no read takes the growing set whole.
        Func<Guid?, Guid, Task<List<AwaitedRecord>>> withdrawals = (after, cursor) => _db.Withdrawals.AsNoTracking()
            .Where(w => w.Quantity
                - (_db.Returns.Where(r => r.WithdrawalId == w.Id).Sum(r => (int?)r.Quantity) ?? 0)
                - (_db.ReturnRejections.Where(r => r.WithdrawalId == w.Id).Sum(r => (int?)r.Quantity) ?? 0)
                - (_db.NotReturnedClosures.Where(n => n.WithdrawalId == w.Id && n.ReopenedAt == null).Sum(n => (int?)n.Quantity) ?? 0) > 0)
            .Where(w => after == null || w.Id.CompareTo(cursor) > 0)
            .OrderBy(w => w.Id)
            .Take(AwaitedPageSize)
            .Select(w => new AwaitedRecord(w.Id, w.OrderItemId))
            .ToListAsync(ct);
        Func<Guid?, Guid, Task<List<AwaitedRecord>>> terminations = (after, cursor) => _db.LateDeliveryTerminations.AsNoTracking()
            .Where(t => t.Quantity - (_db.Returns.Where(r => r.LateDeliveryTerminationId == t.Id).Sum(r => (int?)r.Quantity) ?? 0) > 0)
            .Where(t => after == null || t.Id.CompareTo(cursor) > 0)
            .OrderBy(t => t.Id)
            .Take(AwaitedPageSize)
            .Select(t => new AwaitedRecord(t.Id, t.OrderItemId))
            .ToListAsync(ct);

        // Only a shipped physical item has goods that can come back; a service item or an order never shipped awaits nothing.
        HashSet<Guid> orders = [];
        foreach (Func<Guid?, Guid, Task<List<AwaitedRecord>>> source in new[] { withdrawals, terminations })
        {
            Guid? after = null;
            while (true)
            {
                Guid cursor = after ?? Guid.Empty;
                List<AwaitedRecord> page = await source(after, cursor);
                if (page.Count == 0)
                {
                    break;
                }

                IReadOnlyList<RefundWorkItemFacts> facts = await _ordering.FindRefundWorkItemFactsAsync([.. page.Select(r => r.OrderItemId).Distinct()], ct);
                orders.UnionWith(facts.Where(f => f.ProductType == ProductType.Physical && f.IsShipped).Select(f => f.OrderId));
                after = page[^1].Id;
            }
        }

        return [.. orders];
    }

    private sealed record AwaitedRecord(Guid Id, Guid OrderItemId);

    public async Task<RefundProcessingPage> ListOrdersAwaitingRefundProcessingAsync(Guid? afterId, int maxSubjects, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(maxSubjects, 1);
        List<Subject> subjects = await NextSubjectsAsync(afterId, maxSubjects, ct);
        if (subjects.Count == 0)
        {
            return new RefundProcessingPage(null, []);
        }

        Guid? last = subjects[^1].Id;
        Guid[] itemIds = [.. subjects.Select(s => s.OrderItemId).Distinct()];

        // A customer who still owes the firm an IBAN holds the work: the item waits for the customer, not the firm (02 10.6.1; K-525).
        HashSet<Guid> awaitingIban = [.. await _db.RefundIbanRequests.AsNoTracking()
            .Where(r => r.OrderItemId != null && itemIds.Contains(r.OrderItemId.Value) && r.FulfilledAt == null && r.ClosedAt == null)
            .Select(r => r.OrderItemId!.Value).Distinct().ToListAsync(ct)];
        Dictionary<Guid, RefundWorkItemFacts> facts = (await _ordering.FindRefundWorkItemFactsAsync(itemIds, ct)).ToDictionary(f => f.OrderItemId);

        // A cancellation whose closing transition was corrected is undone: nothing is owed for it (06 4.22; 02 10.4.6).
        Guid[] closing = [.. subjects.Select(s => s.TransitionRecordId).OfType<Guid>().Distinct()];
        IReadOnlySet<Guid> corrected = closing.Length == 0 ? new HashSet<Guid>() : await _ordering.FindCorrectedTransitionRecordsAsync(closing, ct);

        List<Subject> owed = [.. subjects.Where(s =>
            !awaitingIban.Contains(s.OrderItemId)
            && facts.TryGetValue(s.OrderItemId, out RefundWorkItemFacts? f)
            && PaymentApproval.IsApproved(f.PaymentStatus)
            && (s.TransitionRecordId is not { } closedBy || !corrected.Contains(closedBy))
            && (s.Kind != OwedKind.Withdrawal || !AfterSalesPeriodRules.IsAfterDelivery(f.ProductType, f.DeliveryDate, s.DeclarationDate, s.IsDeliveredBeforeDeclaration)))];

        // The refund row is born with the firm's step (06 4.33): a record with no refund row is a step still to take; one with a row - sent, voided or open - is the payment module's (below).
        IReadOnlySet<Guid> handled = await _payment.FindSubjectsWithRefundAsync([.. owed.Select(s => s.Id)], ct);
        Guid[] orders = [.. owed.Where(s => !handled.Contains(s.Id)).Select(s => facts[s.OrderItemId].OrderId).Distinct()];
        return new RefundProcessingPage(last, orders);
    }

    public async Task<IReadOnlySet<Guid>> FindRefundsAwaitingIbanAsync(IReadOnlyCollection<RefundOfItem> refunds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(refunds);
        if (refunds.Count == 0)
        {
            return new HashSet<Guid>();
        }

        // An open request names the refund, or names the refund's item before any refund is linked (02 10.6.1; K-686).
        Guid[] ids = [.. refunds.Select(r => r.RefundId).Distinct()];
        Guid[] items = [.. refunds.Where(r => r.OrderItemId != null).Select(r => r.OrderItemId!.Value).Distinct()];
        var open = await _db.RefundIbanRequests.AsNoTracking()
            .Where(r => r.FulfilledAt == null && r.ClosedAt == null
                && ((r.RefundId != null && ids.Contains(r.RefundId.Value)) || (r.RefundId == null && r.OrderItemId != null && items.Contains(r.OrderItemId.Value))))
            .Select(r => new { r.RefundId, r.OrderItemId }).ToListAsync(ct);
        HashSet<Guid> byRefund = [.. open.Where(r => r.RefundId != null).Select(r => r.RefundId!.Value)];
        HashSet<Guid> byItem = [.. open.Where(r => r.RefundId == null && r.OrderItemId != null).Select(r => r.OrderItemId!.Value)];
        HashSet<Guid> waiting = [.. refunds.Where(r => byRefund.Contains(r.RefundId) || (r.OrderItemId is { } item && byItem.Contains(item))).Select(r => r.RefundId)];
        return waiting;
    }

    /// <summary>The next subjects after the cursor, by id across the five record kinds: each kind gives its next <paramref name="max"/> and the merge keeps the first <paramref name="max"/>.</summary>
    private async Task<List<Subject>> NextSubjectsAsync(Guid? afterId, int max, CancellationToken ct)
    {
        Guid after = afterId ?? Guid.Empty;
        List<Subject> all = [];
        all.AddRange(await _db.Cancellations.AsNoTracking().Where(c => afterId == null || c.Id.CompareTo(after) > 0).OrderBy(c => c.Id).Take(max)
            .Select(c => new Subject(c.Id, OwedKind.Cancellation, c.OrderItemId, c.TransitionRecordId, default, null)).ToListAsync(ct));
        all.AddRange(await _db.ItemRemovals.AsNoTracking().Where(r => afterId == null || r.Id.CompareTo(after) > 0).OrderBy(r => r.Id).Take(max)
            .Select(r => new Subject(r.Id, OwedKind.ItemRemoval, r.OrderItemId, null, default, null)).ToListAsync(ct));
        all.AddRange(await _db.LateDeliveryTerminations.AsNoTracking().Where(t => afterId == null || t.Id.CompareTo(after) > 0).OrderBy(t => t.Id).Take(max)
            .Select(t => new Subject(t.Id, OwedKind.LateDeliveryTermination, t.OrderItemId, null, default, null)).ToListAsync(ct));
        all.AddRange(await _db.Withdrawals.AsNoTracking().Where(w => afterId == null || w.Id.CompareTo(after) > 0).OrderBy(w => w.Id).Take(max)
            .Select(w => new Subject(w.Id, OwedKind.Withdrawal, w.OrderItemId, null, w.DeclarationDate, w.IsDeliveredBeforeDeclaration)).ToListAsync(ct));
        all.AddRange(await _db.Returns.AsNoTracking().Where(r => r.WithdrawalId != null && (afterId == null || r.Id.CompareTo(after) > 0)).OrderBy(r => r.Id).Take(max)
            .Select(r => new Subject(r.Id, OwedKind.Receipt, r.OrderItemId, null, default, null)).ToListAsync(ct));
        return [.. all.OrderBy(s => s.Id, Comparer<Guid>.Create(CompareAsDatabase)).Take(max)];
    }

    /// <summary>The order of two ids as PostgreSQL's <c>uuid</c> type makes it (the cursor is compared by the database): the bytes, big-endian.</summary>
    private static int CompareAsDatabase(Guid left, Guid right)
    {
        Span<byte> a = stackalloc byte[16];
        Span<byte> b = stackalloc byte[16];
        left.TryWriteBytes(a, bigEndian: true, out _);
        right.TryWriteBytes(b, bigEndian: true, out _);
        return a.SequenceCompareTo(b);
    }

    private enum OwedKind
    {
        Cancellation,
        ItemRemoval,
        LateDeliveryTermination,
        Withdrawal,
        Receipt,
    }

    private sealed record Subject(Guid Id, OwedKind Kind, Guid OrderItemId, Guid? TransitionRecordId, DateOnly DeclarationDate, bool? IsDeliveredBeforeDeclaration);
}
