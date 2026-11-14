using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
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

    public async Task<OpenIbanRequestSubjects> ListOpenIbanRequestSubjectsAsync(CancellationToken ct)
    {
        // The open request is the mark (06 4.29): neither fulfilled nor closed. An item request names its item; the itemless request of a late payment's refund names the refund (K-1075).
        var open = await _db.RefundIbanRequests.AsNoTracking()
            .Where(r => r.FulfilledAt == null && r.ClosedAt == null)
            .Select(r => new { r.OrderItemId, r.RefundId })
            .ToListAsync(ct);
        return new OpenIbanRequestSubjects(
            [.. open.Where(r => r.OrderItemId is not null).Select(r => r.OrderItemId!.Value).Distinct()], // non-null: the Where on this line keeps only requests with an item
            [.. open.Where(r => r.OrderItemId is null && r.RefundId is not null).Select(r => r.RefundId!.Value).Distinct()]); // non-null: the Where on this line keeps only requests with a refund
    }

    public Task<int> CountOpenDefectClaimsAsync(CancellationToken ct) =>
        _db.DefectClaims.AsNoTracking().CountAsync(c => c.DefectClaimStatus == DefectClaimStatus.Open, ct);

    public async Task<IReadOnlyList<Guid>> ListOrdersAwaitingReturnGoodsAsync(CancellationToken ct)
    {
        IReadOnlyList<RefundWorkItemFacts> items = await ItemsAwaitingReturnGoodsAsync(ct);
        return [.. items.Select(f => f.OrderId).Distinct()];
    }

    public async Task<int> CountItemsAwaitingReturnGoodsAsync(CancellationToken ct)
    {
        // The counter's set is wider than the mark: it also holds a withdrawal declared before the delivery (or on its day, the firm not yet asked) and a late delivery termination,
        // whose goods come back without the item ever carrying "iade malı bekleniyor" (03 1.7.3.5). The mark's own rule narrows it, a page of items at a time (09 20 item 2).
        IReadOnlyList<RefundWorkItemFacts> items = await ItemsAwaitingReturnGoodsAsync(ct);
        int marked = 0;
        foreach (RefundWorkItemFacts[] page in items.Chunk(AwaitedPageSize))
        {
            IReadOnlyDictionary<Guid, ItemMarks> marks = await ItemMarksAsync([.. page.Select(f => new ItemDelivery(f.OrderItemId, f.ProductType, f.DeliveryDate))], ct);
            marked += marks.Values.Count(m => m.AwaitingReturnGoods);
        }

        return marked;
    }

    // The items whose returned goods are awaited: the one reading behind the panel home's counter and the return address confirmation (T382, T536); the confirmation keeps the marked ones.
    private async Task<IReadOnlyList<RefundWorkItemFacts>> ItemsAwaitingReturnGoodsAsync(CancellationToken ct)
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
        Dictionary<Guid, RefundWorkItemFacts> items = [];
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
                foreach (RefundWorkItemFacts fact in facts.Where(f => f.ProductType == ProductType.Physical && f.IsShipped))
                {
                    items.TryAdd(fact.OrderItemId, fact);
                }

                after = page[^1].Id;
            }
        }

        return [.. items.Values];
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
            .Select(r => r.OrderItemId!.Value).Distinct().ToListAsync(ct)]; // non-null: the Where above keeps only requests with an item
        (List<Subject> owed, Dictionary<Guid, RefundWorkItemFacts> facts) = await OwedWithoutRefundAsync(subjects, itemIds, ct);
        Guid[] orders = [.. owed.Where(s => !awaitingIban.Contains(s.OrderItemId)).Select(s => facts[s.OrderItemId].OrderId).Distinct()];
        return new RefundProcessingPage(last, orders);
    }

    public async Task<IReadOnlyList<OwedRefundRecord>> ListOwedRefundRecordsAsync(IReadOnlyCollection<Guid> orderItemIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderItemIds);
        Guid[] ids = [.. orderItemIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        (List<Subject> owed, _) = await OwedWithoutRefundAsync(await SubjectsOfItemsAsync(ids, ct), ids, ct);
        return
        [
            .. owed.Select(s => s.Kind switch
            {
                OwedKind.Cancellation => new OwedRefundRecord(RefundType.Cancellation, s.Id, s.OrderItemId, s.Quantity, s.CreatedAt),
                OwedKind.ItemRemoval => new OwedRefundRecord(RefundType.ItemRemoval, s.Id, s.OrderItemId, s.Quantity, s.CreatedAt),
                OwedKind.LateDeliveryTermination => new OwedRefundRecord(RefundType.LateDeliveryTermination, s.Id, s.OrderItemId, s.Quantity, s.CreatedAt),
                OwedKind.Withdrawal => new OwedRefundRecord(RefundType.Withdrawal, s.Id, s.OrderItemId, s.Quantity, s.CreatedAt),
                _ => new OwedRefundRecord(RefundType.Withdrawal, s.WithdrawalId ?? throw new InvalidOperationException("A receipt subject is read with its withdrawal."), s.OrderItemId, s.Quantity, s.CreatedAt)
                {
                    ReturnId = s.Id,
                },
            }),
        ];
    }

    /// <summary>
    /// The one rule of a record that owes the firm a refund step (T382; T529; 06 4.33; 02 10.6.1): the order's payment was approved, a cancellation's closing transition was not corrected (06 4.22;
    /// 02 10.4.6), a withdrawal is owed at its declaration only before the delivery - after it the receipt owes -, and the refund row is not born yet: the row is born with the firm's step, and a record
    /// with a row - sent, voided or open - is the payment module's. The panel home's counter and the order screen's refund lines read this one rule.
    /// </summary>
    private async Task<(List<Subject> Owed, Dictionary<Guid, RefundWorkItemFacts> Facts)> OwedWithoutRefundAsync(List<Subject> subjects, Guid[] itemIds, CancellationToken ct)
    {
        Dictionary<Guid, RefundWorkItemFacts> facts = (await _ordering.FindRefundWorkItemFactsAsync(itemIds, ct)).ToDictionary(f => f.OrderItemId);
        Guid[] closing = [.. subjects.Select(s => s.TransitionRecordId).OfType<Guid>().Distinct()];
        IReadOnlySet<Guid> corrected = closing.Length == 0 ? new HashSet<Guid>() : await _ordering.FindCorrectedTransitionRecordsAsync(closing, ct);

        List<Subject> owed = [.. subjects.Where(s =>
            facts.TryGetValue(s.OrderItemId, out RefundWorkItemFacts? f)
            && PaymentApproval.IsApproved(f.PaymentStatus)
            && (s.TransitionRecordId is not { } closedBy || !corrected.Contains(closedBy))
            && (s.Kind != OwedKind.Withdrawal || !AfterSalesPeriodRules.IsAfterDelivery(f.ProductType, f.DeliveryDate, s.DeclarationDate, s.IsDeliveredBeforeDeclaration)))];

        IReadOnlySet<Guid> handled = await _payment.FindSubjectsWithRefundAsync([.. owed.Select(s => s.Id)], ct);
        return ([.. owed.Where(s => !handled.Contains(s.Id))], facts);
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
        Guid[] items = [.. refunds.Where(r => r.OrderItemId != null).Select(r => r.OrderItemId!.Value).Distinct()]; // non-null: the Where on this line keeps only requests with an item
        var open = await _db.RefundIbanRequests.AsNoTracking()
            .Where(r => r.FulfilledAt == null && r.ClosedAt == null
                && ((r.RefundId != null && ids.Contains(r.RefundId.Value)) || (r.RefundId == null && r.OrderItemId != null && items.Contains(r.OrderItemId.Value))))
            .Select(r => new { r.RefundId, r.OrderItemId }).ToListAsync(ct);
        HashSet<Guid> byRefund = [.. open.Where(r => r.RefundId != null).Select(r => r.RefundId!.Value)]; // non-null: the Where on this line keeps only requests with a refund
        HashSet<Guid> byItem = [.. open.Where(r => r.RefundId == null && r.OrderItemId != null).Select(r => r.OrderItemId!.Value)]; // non-null: the Where on this line keeps only requests with an item
        HashSet<Guid> waiting = [.. refunds.Where(r => byRefund.Contains(r.RefundId) || (r.OrderItemId is { } item && byItem.Contains(item))).Select(r => r.RefundId)];
        return waiting;
    }

    public async Task<IReadOnlyList<AwaitingIbanLine>> ListAwaitingIbanLinesAsync(CancellationToken ct)
    {
        var open = await _db.RefundIbanRequests.AsNoTracking()
            .Where(r => r.FulfilledAt == null && r.ClosedAt == null)
            .OrderBy(r => r.CreatedAt).ThenBy(r => r.Id)
            .Select(r => new { r.OrderItemId, r.RefundId, r.Reason, r.CreatedAt }).ToListAsync(ct);

        // The refund period runs from the cancellation's, the removal's or the withdrawal's record; the other reasons have no period (04 2.6.2; K-716).
        return
        [
            .. open.Select(r => new AwaitingIbanLine(
                r.OrderItemId,
                r.RefundId,
                r.Reason,
                r.CreatedAt,
                r.Reason is RefundIbanRequestReason.CancellationOrRemoval or RefundIbanRequestReason.NoticeWithoutIban or RefundIbanRequestReason.ReturnAfterIbanDeleted
                    ? AfterSalesPeriodRules.RefundLastDay(TurkeyTime.DateOf(r.CreatedAt))
                    : null)),
        ];
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

    /// <summary>The records of the given items that may owe a refund step, with what the order screen shows of them (T529): the same five kinds as <see cref="NextSubjectsAsync"/>.</summary>
    private async Task<List<Subject>> SubjectsOfItemsAsync(Guid[] ids, CancellationToken ct)
    {
        List<Subject> all = [];
        all.AddRange(await _db.Cancellations.AsNoTracking().Where(c => ids.Contains(c.OrderItemId))
            .Select(c => new Subject(c.Id, OwedKind.Cancellation, c.OrderItemId, c.TransitionRecordId, default, null) { Quantity = c.Quantity, CreatedAt = c.CreatedAt }).ToListAsync(ct));
        all.AddRange(await _db.ItemRemovals.AsNoTracking().Where(r => ids.Contains(r.OrderItemId))
            .Select(r => new Subject(r.Id, OwedKind.ItemRemoval, r.OrderItemId, null, default, null) { Quantity = r.Quantity, CreatedAt = r.CreatedAt }).ToListAsync(ct));
        all.AddRange(await _db.LateDeliveryTerminations.AsNoTracking().Where(t => ids.Contains(t.OrderItemId))
            .Select(t => new Subject(t.Id, OwedKind.LateDeliveryTermination, t.OrderItemId, null, default, null) { Quantity = t.Quantity, CreatedAt = t.CreatedAt }).ToListAsync(ct));
        all.AddRange(await _db.Withdrawals.AsNoTracking().Where(w => ids.Contains(w.OrderItemId))
            .Select(w => new Subject(w.Id, OwedKind.Withdrawal, w.OrderItemId, null, w.DeclarationDate, w.IsDeliveredBeforeDeclaration) { Quantity = w.Quantity, CreatedAt = w.CreatedAt })
            .ToListAsync(ct));
        all.AddRange(await _db.Returns.AsNoTracking().Where(r => r.WithdrawalId != null && ids.Contains(r.OrderItemId))
            .Select(r => new Subject(r.Id, OwedKind.Receipt, r.OrderItemId, null, default, null) { Quantity = r.Quantity, CreatedAt = r.CreatedAt, WithdrawalId = r.WithdrawalId })
            .ToListAsync(ct));
        return [.. all.OrderBy(s => s.CreatedAt).ThenBy(s => s.Id)];
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

    private sealed record Subject(Guid Id, OwedKind Kind, Guid OrderItemId, Guid? TransitionRecordId, DateOnly DeclarationDate, bool? IsDeliveredBeforeDeclaration)
    {
        public int Quantity { get; init; }

        public DateTimeOffset CreatedAt { get; init; }

        /// <summary>The receipt's withdrawal (06 4.27); null on the other kinds.</summary>
        public Guid? WithdrawalId { get; init; }
    }
}
