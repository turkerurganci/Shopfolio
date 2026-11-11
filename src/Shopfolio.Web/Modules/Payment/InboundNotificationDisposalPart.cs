using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The inbound notification record's part of the nightly disposal (06 9.3.1, 4.35; Z-28, Z-41; K-951, K-1030 (3), (4)). The record carries no personal field and is a
/// commercial record of the order it belongs to: it goes <see cref="DisposalTimings.InboundNotificationRetentionYears"/> years after it was made, and a row of an
/// order waits for the order's arm, read from the payment record at the moment of the disposal through the Ordering surface (<see cref="IOrderingModule"/>):
/// <list type="bullet">
/// <item>a row that belongs to no order goes at the ten years of its own moment;</item>
/// <item>a row of an order whose payment was taken goes at the ten years of its own moment, once the order's personal data is disposed - the order's own part
/// has the last word on when the order's records may go;</item>
/// <item>a row of an order whose payment was never taken goes ten years from the moment the order failed (Z-41).</item>
/// </list>
/// A chunk is at most <see cref="DisposalTimings.PartSize"/> rows and one deletion; the disposal row of type <see cref="DisposalRecordType.InboundNotificationRecord"/>
/// is written only for a chunk that deleted something.
/// </summary>
internal sealed class InboundNotificationDisposalPart : IDisposalPart
{
    private readonly IOrderingModule _ordering;

    public InboundNotificationDisposalPart(IOrderingModule ordering)
    {
        _ordering = ordering;
    }

    public string Name => "InboundNotificationRecords";

    public async Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset before = RetentionCutoff.YearsBefore(run.Now, DisposalTimings.InboundNotificationRetentionYears);
        await run.ChunksAsync(DisposalRecordType.InboundNotificationRecord, (db, max, cancel) => DeleteChunkAsync(db, before, max, cancel), ct);
    }

    private async Task<int> DeleteChunkAsync(AppDbContext db, DateTimeOffset before, int max, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        List<Guid> due = await db.InboundNotificationRecords.AsNoTracking()
            .Where(n => n.OrderId == null && n.CreatedAt < before)
            .OrderBy(n => n.CreatedAt)
            .ThenBy(n => n.Id)
            .Select(n => n.Id)
            .Take(max)
            .ToListAsync(ct);
        if (due.Count < max)
        {
            due.AddRange(await PaidArmRowsAsync(db, before, max - due.Count, ct));
        }

        if (due.Count < max)
        {
            due.AddRange(await UnpaidArmRowsAsync(db, before, max - due.Count, ct));
        }

        if (due.Count == 0)
        {
            return 0;
        }

        Guid[] ids = [.. due];
        return await db.InboundNotificationRecords.Where(n => ids.Contains(n.Id)).ExecuteDeleteAsync(ct);
    }

    /// <summary>
    /// The rows of an order older than the ten years whose order was paid and is disposed, at most <paramref name="max"/>. The database narrows to the rows that may
    /// be due; the arm of each order is the exact judgement, so a page of rows of unpaid or not yet disposed orders is passed over (the row ids are kept in order, so
    /// a page never repeats).
    /// </summary>
    private async Task<List<Guid>> PaidArmRowsAsync(AppDbContext db, DateTimeOffset before, int max, CancellationToken ct)
    {
        List<Guid> due = [];
        List<Guid> seen = [];
        while (due.Count < max)
        {
            Guid[] passed = [.. seen];
            List<RowOfOrder> page = await db.InboundNotificationRecords.AsNoTracking()
                .Where(n => n.OrderId != null && n.CreatedAt < before && !passed.Contains(n.Id))
                .OrderBy(n => n.Id)
                .Select(n => new RowOfOrder(n.Id, n.OrderId!.Value)) // non-null: the Where above keeps only records with an order
                .Take(max)
                .ToListAsync(ct);
            if (page.Count == 0)
            {
                break;
            }

            seen.AddRange(page.Select(r => r.Id));
            IReadOnlyList<OrderPaymentArm> arms = await _ordering.FindPaymentArmsAsync([.. page.Select(r => r.OrderId).Distinct()], ct);
            HashSet<Guid> ready = [.. arms.Where(a => a.IsPaid && a.PersonalDataDisposed).Select(a => a.OrderId)];
            due.AddRange(page.Where(r => ready.Contains(r.OrderId)).Select(r => r.Id).Take(max - due.Count));
            if (page.Count < max)
            {
                break;
            }
        }

        return due;
    }

    /// <summary>
    /// The rows of the unpaid orders that failed before the ten years' edge, at most <paramref name="max"/>: the orders come page by page from the Ordering surface
    /// and each order's rows go with it (an order without rows is passed over).
    /// </summary>
    private async Task<List<Guid>> UnpaidArmRowsAsync(AppDbContext db, DateTimeOffset failedBefore, int max, CancellationToken ct)
    {
        List<Guid> due = [];
        List<Guid> seenOrders = [];
        while (due.Count < max)
        {
            IReadOnlyList<Guid> orders = await _ordering.FindUnpaidFailedOrdersAsync(failedBefore, seenOrders, max, ct);
            if (orders.Count == 0)
            {
                break;
            }

            seenOrders.AddRange(orders);
            Guid[] page = [.. orders];
            due.AddRange(await db.InboundNotificationRecords.AsNoTracking()
                .Where(n => n.OrderId != null && page.Contains(n.OrderId.Value))
                .OrderBy(n => n.Id)
                .Select(n => n.Id)
                .Take(max - due.Count)
                .ToListAsync(ct));
        }

        return due;
    }

    private sealed record RowOfOrder(Guid Id, Guid OrderId);
}
