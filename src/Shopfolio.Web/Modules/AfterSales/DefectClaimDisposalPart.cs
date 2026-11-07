using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The defect claim's period (06 9.1 Z-29; 6563 sayılı Kanun m.11/3): ten years from the last change of its status, from its opening when the status never changed.
/// A legal period, not a product constant, so it lives with the part that applies it (09 3.5 item 1; 06 9.1.2). Years are counted in Turkey time.
/// </summary>
internal static class DefectClaimRetention
{
    /// <summary>Z-29: a defect claim is kept this many years from the moment of its last status change.</summary>
    public const int Years = 10;

    /// <summary>The edge: a claim whose last change is before the returned instant is past its period.</summary>
    public static DateTimeOffset Before(DateTimeOffset now) => RetentionCutoff.YearsBefore(now, Years);

    /// <summary>
    /// The moment of a claim's last status change (K-1054; K-1092): the moment of the last entry of its status history, or its opening moment when the history
    /// has no entry. The entry's moment field is named <c>At</c>; the task that writes the history (T320) keeps that name.
    /// </summary>
    public static DateTimeOffset LastChange(DateTimeOffset createdAt, string statusHistory)
    {
        ArgumentNullException.ThrowIfNull(statusHistory);
        List<DefectClaimHistoryEntry>? entries = PayloadJson.Read<List<DefectClaimHistoryEntry>>(statusHistory);
        return entries is { Count: > 0 } ? entries[^1].At : createdAt;
    }
}

/// <summary>The one field of a status history entry that the disposal reads (06 4.30): the moment of the change.</summary>
internal sealed record DefectClaimHistoryEntry(DateTimeOffset At);

/// <summary>
/// The defect claim's part of the nightly disposal (06 9.3.1, 4.30; Z-29; K-951, K-1054). A claim is deleted when its last status change is ten years old and its
/// order is disposed: it is never deleted before the order's own disposal (Z-29), and the order's arm is read through the Ordering surface
/// (<see cref="IOrderingModule"/>) at the moment of the disposal. A chunk of at most <see cref="DisposalTimings.PartSize"/> claims is removed as tracked rows and the
/// transaction runner saves them with the disposal row (no bulk statement: the order side's rows are written only through a save, 09 10.2); the return and refund
/// links to the claim are emptied by the database (SetNull, 06 7.2). The part takes no order lock - the order is already disposed and nothing writes it - so the
/// order's version does not move. The disposal row of type <see cref="DisposalRecordType.DefectClaim"/> is written only for a chunk that deleted something.
/// </summary>
internal sealed class DefectClaimDisposalPart : IDisposalPart
{
    private readonly IOrderingModule _ordering;

    public DefectClaimDisposalPart(IOrderingModule ordering)
    {
        _ordering = ordering;
    }

    public string Name => "DefectClaims";

    public async Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset before = DefectClaimRetention.Before(run.Now);
        await run.ChunksAsync(DisposalRecordType.DefectClaim, (db, max, cancel) => DeleteChunkAsync(db, before, max, cancel), ct);
    }

    /// <summary>
    /// The claims opened before the edge are the candidates (a claim cannot have changed before it was opened); the last change is read from the history and the
    /// order's disposal from the payment record, so a page of claims that are not due is passed over (the claim ids are kept in order, so a page never repeats).
    /// </summary>
    private async Task<int> DeleteChunkAsync(AppDbContext db, DateTimeOffset before, int max, CancellationToken ct)
    {
        db.ChangeTracker.Clear();
        List<Guid> due = [];
        List<Guid> seen = [];
        while (due.Count < max)
        {
            Guid[] passed = [.. seen];
            List<Candidate> page = await db.DefectClaims.AsNoTracking()
                .Where(c => c.CreatedAt < before && !passed.Contains(c.Id))
                .OrderBy(c => c.Id)
                .Select(c => new Candidate(c.Id, c.OrderItemId, c.CreatedAt, c.StatusHistory))
                .Take(max)
                .ToListAsync(ct);
            if (page.Count == 0)
            {
                break;
            }

            seen.AddRange(page.Select(c => c.Id));
            List<Candidate> old = [.. page.Where(c => DefectClaimRetention.LastChange(c.CreatedAt, c.StatusHistory) < before)];
            if (old.Count > 0)
            {
                IReadOnlyList<OrderOfItem> orders = await _ordering.FindOrdersOfItemsAsync([.. old.Select(c => c.OrderItemId).Distinct()], ct);
                IReadOnlyList<OrderPaymentArm> arms = await _ordering.FindPaymentArmsAsync([.. orders.Select(o => o.OrderId).Distinct()], ct);
                HashSet<Guid> disposedOrders = [.. arms.Where(a => a.PersonalDataDisposed).Select(a => a.OrderId)];
                HashSet<Guid> disposedItems = [.. orders.Where(o => disposedOrders.Contains(o.OrderId)).Select(o => o.OrderItemId)];
                due.AddRange(old.Where(c => disposedItems.Contains(c.OrderItemId)).Select(c => c.Id).Take(max - due.Count));
            }

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
        List<DefectClaim> claims = await db.DefectClaims.Where(c => ids.Contains(c.Id)).ToListAsync(ct);
        db.DefectClaims.RemoveRange(claims);
        return claims.Count;
    }

    private sealed record Candidate(Guid Id, Guid OrderItemId, DateTimeOffset CreatedAt, string StatusHistory);
}
