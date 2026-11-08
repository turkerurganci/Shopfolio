using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The periods of the dispatch record (06 9.1 Z-34, Z-43; 02 4.2): a row that belongs to no order is kept <see cref="ProductConstants.DispatchRecordRetentionYears"/>
/// years (P-41, a product constant); a row that belongs to an order is a legal record kept <see cref="OrderLinkedYears"/> years, and on the arm of an order whose
/// payment was never taken its personal data - the recipient's address and the event values - goes after <see cref="UnpaidPersonalDataYears"/> years. They are
/// legal periods, not product constants, so they live with the part that applies them (09 3.5 item 1; 06 9.1.2). Years are counted in Turkey time.
/// </summary>
internal static class DispatchRecordRetention
{
    /// <summary>Z-43: a dispatch row of an order is kept this many years from the moment it was made, on both arms (6563 sayılı Kanun m.11/3).</summary>
    public const int OrderLinkedYears = 10;

    /// <summary>Z-43: on the arm of an order whose payment was never taken the row's recipient address and event values are emptied after this many years.</summary>
    public const int UnpaidPersonalDataYears = 3;

    /// <summary>Z-34's edge: a row of no order made before the returned instant is past its period.</summary>
    public static DateTimeOffset UnboundBefore(DateTimeOffset now) => RetentionCutoff.YearsBefore(now, ProductConstants.DispatchRecordRetentionYears);

    /// <summary>Z-43's edge for the deletion: a row of an order made before the returned instant is past its ten years.</summary>
    public static DateTimeOffset OrderLinkedBefore(DateTimeOffset now) => RetentionCutoff.YearsBefore(now, OrderLinkedYears);

    /// <summary>Z-43's edge for the emptying: a row of an unpaid order made before the returned instant has lost its personal data.</summary>
    public static DateTimeOffset UnpaidPersonalDataBefore(DateTimeOffset now) => RetentionCutoff.YearsBefore(now, UnpaidPersonalDataYears);
}

/// <summary>
/// The dispatch record's part of the nightly disposal (06 9.3.1, 4.58; Z-34, Z-43; K-951, K-1030 (4)). Two kinds of work, both written to the disposal row of
/// type <see cref="DisposalRecordType.DispatchRecord"/> (a chunk that did nothing writes none): the deletion of the rows whose period is over - a row of no order
/// at Z-34, a row of an order at the ten years of Z-43 - and the emptying of the recipient address and the event values of the rows of an unpaid order after
/// three years. Which arm an order is on is read from its payment record at the moment of the disposal, through the Ordering surface (<see cref="IOrderingModule"/>),
/// and never stored: an order whose payment was taken - a late card payment included - keeps its rows' personal data until the ten years.
/// </summary>
internal sealed class DispatchRecordDisposalPart : IDisposalPart
{
    private readonly IOrderingModule _ordering;

    public DispatchRecordDisposalPart(IOrderingModule ordering)
    {
        _ordering = ordering;
    }

    public string Name => "DispatchRecords";

    public async Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset now = run.Now;
        await run.ChunksAsync(DisposalRecordType.DispatchRecord, (db, max, cancel) => DeleteChunkAsync(db, now, max, cancel), ct);
        await run.ChunksAsync(DisposalRecordType.DispatchRecord, (db, max, cancel) => EmptyChunkAsync(db, now, max, cancel), ct);
    }

    private static async Task<int> DeleteChunkAsync(AppDbContext db, DateTimeOffset now, int max, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        DateTimeOffset unboundBefore = DispatchRecordRetention.UnboundBefore(now);
        DateTimeOffset linkedBefore = DispatchRecordRetention.OrderLinkedBefore(now);
        List<Guid> ids = await db.DispatchRecords
            .Where(d => (d.OrderId == null && d.CreatedAt < unboundBefore) || (d.OrderId != null && d.CreatedAt < linkedBefore))
            .OrderBy(d => d.CreatedAt)
            .ThenBy(d => d.Id)
            .Select(d => d.Id)
            .Take(max)
            .ToListAsync(ct);
        return await db.DispatchRecords.Where(d => ids.Contains(d.Id)).ExecuteDeleteAsync(ct);
    }

    /// <summary>
    /// The rows of an order whose payment was never taken, older than the personal data period, that still hold an address or event values, at most
    /// <paramref name="max"/>. The database narrows to the rows that may be due; the arm of each order is the exact judgement, so a page of rows of paid orders
    /// is passed over (the row ids are kept in order, so a page never repeats).
    /// </summary>
    private async Task<int> EmptyChunkAsync(AppDbContext db, DateTimeOffset now, int max, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        DateTimeOffset before = DispatchRecordRetention.UnpaidPersonalDataBefore(now);
        List<Guid> due = [];
        List<Guid> seen = [];
        while (due.Count < max)
        {
            Guid[] passed = [.. seen];
            List<RowOfOrder> page = await db.DispatchRecords.AsNoTracking()
                .Where(d => d.OrderId != null && d.CreatedAt < before && (d.RecipientEmail != null || d.EventValues != null) && !passed.Contains(d.Id))
                .OrderBy(d => d.Id)
                .Select(d => new RowOfOrder(d.Id, d.OrderId!.Value))
                .Take(max)
                .ToListAsync(ct);
            if (page.Count == 0)
            {
                break;
            }

            seen.AddRange(page.Select(r => r.Id));
            IReadOnlyList<OrderPaymentArm> arms = await _ordering.FindPaymentArmsAsync([.. page.Select(r => r.OrderId)], ct);
            HashSet<Guid> unpaid = [.. arms.Where(a => !a.IsPaid).Select(a => a.OrderId)];
            due.AddRange(page.Where(r => unpaid.Contains(r.OrderId)).Select(r => r.Id).Take(max - due.Count));
            if (page.Count < max)
            {
                break;
            }
        }

        if (due.Count == 0)
        {
            return 0;
        }

        Guid[] ids = [.. due];
        return await db.DispatchRecords.Where(d => ids.Contains(d.Id)).ExecuteUpdateAsync(
            set => set.SetProperty(d => d.RecipientEmail, (string?)null).SetProperty(d => d.EventValues, (string?)null),
            ct);
    }

    private sealed record RowOfOrder(Guid Id, Guid OrderId);
}
