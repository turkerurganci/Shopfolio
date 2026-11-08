using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The order's part of the nightly disposal (06 9.2, 9.3.1; 05 7.4.3 (2), MK-23; T301): the personal data of an order whose period is over goes, the
/// commercial record stays. Which period: a paid order (an uncorrected payment record exists) at the end of Z-28, an order whose payment was never taken
/// (Failed, no uncorrected payment record) three years after the moment it failed (Z-41); the arm is read from the payment record at the moment of the
/// disposal, never stored (06 4.32; K-1030 (3)) - so a Failed order a late card payment was refunded for is a Z-28 order and waits for Z-28.
/// <para>
/// Each chunk is one database transaction (<see cref="DisposalRun.ChunksAsync"/>): it picks the orders that are due, takes their row locks in one
/// statement in id order (<see cref="OrderLocks"/>), reads the orders again and judges the condition again - an order disposed or changed between the pick
/// and the lock is skipped and not counted (06 9.2.1; K-1095) - then empties and deletes what 06 9.2.1 lists, raises each order's version by one and writes one
/// disposal row of type <see cref="DisposalRecordType.Order"/> with the number of orders; a chunk with nothing to dispose writes no row. The other modules' records
/// are reached through their module surfaces (<see cref="IPaymentModule"/>, <see cref="IAfterSalesModule"/>) inside the same transaction, through the
/// change tracker, so the order side's version guard sees every write (09 10.2).
/// </para>
/// </summary>
internal sealed class OrderDisposalPart : IDisposalPart
{
    private readonly OrderLocks _locks;
    private readonly IPaymentModule _payment;
    private readonly IAfterSalesModule _afterSales;
    private readonly OrderPaymentArms _arms;

    public OrderDisposalPart(OrderLocks locks, IPaymentModule payment, IAfterSalesModule afterSales, OrderPaymentArms arms)
    {
        _locks = locks;
        _payment = payment;
        _afterSales = afterSales;
        _arms = arms;
    }

    public string Name => "Orders";

    /// <summary>
    /// A test's seam: called with the picked order ids after the pick and before the lock, so a test can make the race of 06 9.2.1 deterministic (an order
    /// disposed or changed by another transaction in that gap). Null in the application.
    /// </summary>
    internal Func<IReadOnlyList<Guid>, CancellationToken, Task>? AfterPick { get; set; }

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset now = run.Now;
        return run.ChunksAsync(DisposalRecordType.Order, (db, max, cancel) => DisposeChunkAsync(db, now, max, cancel), ct);
    }

    private async Task<int> DisposeChunkAsync(AppDbContext db, DateTimeOffset now, int max, CancellationToken ct)
    {
        // A chunk reads everything it writes fresh: nothing tracked by an earlier chunk of the same scope is trusted.
        db.ChangeTracker.Clear();

        IReadOnlyList<Guid> picked = await PickDueAsync(db, now, max, ct);
        if (picked.Count == 0)
        {
            return 0;
        }

        if (AfterPick is not null)
        {
            await AfterPick(picked, ct);
        }

        // The row locks first (05 7.3.2), then the orders are read: an order read before the lock may be stale.
        await _locks.LockAsync(picked, ct);
        Guid[] pickedIds = [.. picked];
        List<Order> locked = await db.Orders.Where(o => pickedIds.Contains(o.Id) && o.ContactEmail != null).ToListAsync(ct);
        IReadOnlySet<Guid> stillDue = await JudgeAsync(db, now, [.. locked.Select(o => o.Id)], ct);
        List<Order> orders = [.. locked.Where(o => stillDue.Contains(o.Id))];
        if (orders.Count == 0)
        {
            return 0;
        }

        Guid[] orderIds = [.. orders.Select(o => o.Id)];

        // The items are loaded and tracked: the other modules' item records find their order through them (09 10.2). The file link goes with the
        // personal data: the page of a disposed order cannot be opened and the link does not keep the file alive (06 9.2.1; K-1069).
        List<OrderItem> items = await db.OrderItems.Where(i => orderIds.Contains(i.OrderId)).ToListAsync(ct);
        foreach (OrderItem item in items)
        {
            item.DigitalFileId = null;
        }

        List<TransitionRecord> transitions = await db.TransitionRecords
            .Where(t => orderIds.Contains(t.OrderId) && (t.AdminId != null || t.AdminName != null || t.ReasonNote != null))
            .ToListAsync(ct);
        foreach (TransitionRecord transition in transitions)
        {
            transition.ErasePersonalData();
        }

        // The internal note, the e-mail and address histories are deleted by the explicit delete (the order links are restrictive; 06 2.2.5, K-1047).
        db.InternalNotes.RemoveRange(await db.InternalNotes.Where(n => orderIds.Contains(n.OrderId)).ToListAsync(ct));
        db.OrderEmailHistories.RemoveRange(await db.OrderEmailHistories.Where(h => orderIds.Contains(h.OrderId)).ToListAsync(ct));
        db.OrderAddressHistories.RemoveRange(await db.OrderAddressHistories.Where(h => orderIds.Contains(h.OrderId)).ToListAsync(ct));

        IReadOnlyList<Guid> refundIds = await _payment.ErasePersonalDataAsync(orderIds, ct);
        await _afterSales.ErasePersonalDataAsync(new OrderPersonalDataScope(orderIds, [.. items.Select(i => i.Id)], refundIds), ct);

        foreach (Order order in orders)
        {
            order.ErasePersonalData();
            order.BumpVersion();
        }

        return orders.Count;
    }

    /// <summary>
    /// The orders that are due, at most <paramref name="max"/>, not yet disposed, by id: the database narrows to the orders that may be past either period,
    /// and the exact judgement - which arm, from the payment record - is <see cref="JudgeAsync"/>. A page of candidates the judgement refuses (a Failed order
    /// with a late payment, still inside Z-28) is passed over, so it does not hold the chunk back.
    /// </summary>
    private async Task<IReadOnlyList<Guid>> PickDueAsync(AppDbContext db, DateTimeOffset now, int max, CancellationToken ct)
    {
        DateTimeOffset paidBefore = OrderRetention.PaidCreatedBefore(now);
        DateTimeOffset failedBefore = OrderRetention.FailedBefore(now);
        List<Guid> seen = [];
        List<Guid> due = [];
        while (due.Count < max)
        {
            Guid[] passed = [.. seen];
            List<Guid> page = await db.Orders.AsNoTracking()
                .Where(o => o.ContactEmail != null
                    && !passed.Contains(o.Id)
                    && (o.CreatedAt < paidBefore
                        || (o.PaymentStatus == PaymentStatus.Failed
                            && db.TransitionRecords.Any(t => t.OrderId == o.Id && t.TransitionCode == TransitionCode.O2 && t.CreatedAt < failedBefore))))
                .OrderBy(o => o.Id)
                .Select(o => o.Id)
                .Take(max)
                .ToListAsync(ct);
            if (page.Count == 0)
            {
                break;
            }

            seen.AddRange(page);
            IReadOnlySet<Guid> pageDue = await JudgeAsync(db, now, [.. page], ct);
            due.AddRange(page.Where(pageDue.Contains).Take(max - due.Count));
            if (page.Count < max)
            {
                break;
            }
        }

        return due;
    }

    /// <summary>
    /// The exact judgement over the given orders (06 9.1 Z-28, Z-41; 06 4.32): of the orders that still hold their contact, those whose period is over.
    /// An order is paid when it has a payment record that is not corrected - a record whose confirming transition (Ö1) a correction row corrects is not,
    /// a late card payment (no transition) never is corrected; a paid order is judged by Z-28 from its creation, an unpaid Failed order by Z-41 from the
    /// moment of its Ö2 transition; any other order is not due.
    /// </summary>
    private async Task<IReadOnlySet<Guid>> JudgeAsync(AppDbContext db, DateTimeOffset now, Guid[] orderIds, CancellationToken ct)
    {
        if (orderIds.Length == 0)
        {
            return new HashSet<Guid>();
        }

        Guid[] ids = orderIds;
        List<OrderRow> rows = await db.Orders.AsNoTracking()
            .Where(o => ids.Contains(o.Id) && o.ContactEmail != null)
            .Select(o => new OrderRow(o.Id, o.CreatedAt))
            .ToListAsync(ct);

        // The arm is read in the one place that knows it (OrderPaymentArms): the other modules' parts judge with the same reading.
        IReadOnlyList<OrderPaymentArm> arms = await _arms.ReadAsync([.. rows.Select(r => r.Id)], ct);
        Dictionary<Guid, OrderPaymentArm> armOf = arms.ToDictionary(a => a.OrderId);

        DateTimeOffset paidBefore = OrderRetention.PaidCreatedBefore(now);
        DateTimeOffset failedBefore = OrderRetention.FailedBefore(now);
        HashSet<Guid> due = [];
        foreach (OrderRow row in rows)
        {
            OrderPaymentArm arm = armOf[row.Id];
            bool isDue = arm.IsPaid
                ? row.CreatedAt < paidBefore
                : arm.FailedAt is { } at && at < failedBefore;
            if (isDue)
            {
                due.Add(row.Id);
            }
        }

        return due;
    }

    private sealed record OrderRow(Guid Id, DateTimeOffset CreatedAt);
}
