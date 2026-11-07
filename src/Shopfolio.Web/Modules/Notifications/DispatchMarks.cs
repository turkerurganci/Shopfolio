using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The order's "e-mail did not arrive" mark, read from the dispatch record (06 4.58 "İşaretlerin ve uyarıların okunuşu", "Siparişin satırı";
/// 02 9.1.6): it is not stored. An e-mail's group is its first row and the rows whose <c>OriginalDispatchId</c> is that row's id (a resending or
/// the B-1 after a correction); the group's latest row - by <c>CreatedAt</c>, equal moments by <c>Id</c> (06 2.3.1) - decides. The mark is
/// present when the latest row of any group bound to the order is "undelivered"; a row the per-recipient limit refused is such a row, so the
/// mark falls without the order, the payment or the transition being touched (08 2.2.4 "Aşım").
/// </summary>
internal static class DispatchMarks
{
    /// <summary>True when any e-mail group of the order ends in an "undelivered" row.</summary>
    public static bool HasMark(IEnumerable<Row> rowsOfTheOrder)
    {
        ArgumentNullException.ThrowIfNull(rowsOfTheOrder);
        return rowsOfTheOrder
            .GroupBy(r => r.OriginalDispatchId ?? r.Id)
            .Any(group =>
            {
                Row latest = group.Aggregate((a, b) => UuidOrder.IsBefore(a.CreatedAt, a.Id, b.CreatedAt, b.Id) ? b : a);
                return latest.State == DispatchState.Undelivered;
            });
    }

    /// <summary>Reads the order's rows and answers whether the mark is present.</summary>
    public static async Task<bool> HasMarkAsync(AppDbContext db, Guid orderId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        List<Row> rows = await db.DispatchRecords.AsNoTracking()
            .Where(d => d.OrderId == orderId)
            .Select(d => new Row(d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState))
            .ToListAsync(ct);
        return HasMark(rows);
    }

    /// <summary>
    /// The "e-mail did not arrive" mark of an account or administration e-mail (08 2.2.8; 02 9.4): none of the seven puts a mark except the administrator invitation,
    /// whose mark sits on the invitation's own row - the latest invitation e-mail row of the invitation (the related record) decides, and the row of the firm notice
    /// F-6 that names the same invitation is not an invitation e-mail. Every other type gives false whatever its rows say: the user asks for the link again and the
    /// panel has no row to show it on.
    /// </summary>
    public static async Task<bool> AccountEmailHasMarkAsync(AppDbContext db, DispatchType type, Guid relatedRecordId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (type != DispatchType.AdminInvitation)
        {
            return false;
        }

        List<Row> rows = await db.DispatchRecords.AsNoTracking()
            .Where(d => d.DispatchType == DispatchType.AdminInvitation && d.RelatedRecordType == DispatchRelatedRecordType.AdminInvitation && d.RelatedRecordId == relatedRecordId)
            .Select(d => new Row(d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState))
            .ToListAsync(ct);
        return HasMark(rows);
    }

    /// <summary>A row as the mark reads it.</summary>
    internal readonly record struct Row(Guid Id, Guid? OriginalDispatchId, DateTimeOffset CreatedAt, DispatchState State);
}
