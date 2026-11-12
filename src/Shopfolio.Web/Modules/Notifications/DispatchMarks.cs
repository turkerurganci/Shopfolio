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
                return latest.State == DispatchState.Undelivered && !latest.ClosedUnsent;
            });
    }

    /// <summary>Reads the order's rows and answers whether the mark is present.</summary>
    public static async Task<bool> HasMarkAsync(AppDbContext db, Guid orderId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        List<Row> rows = await db.DispatchRecords.AsNoTracking()
            .Where(d => d.OrderId == orderId)
            .Select(d => new Row(d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState, d.AttemptCount == 0 && d.LastOutcome == null))
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
            .Select(d => new Row(d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState, d.AttemptCount == 0 && d.LastOutcome == null))
            .ToListAsync(ct);
        return HasMark(rows);
    }

    /// <summary>
    /// The administrator invitations among <paramref name="invitationIds"/> whose invitation e-mail carries the mark (T522; the panel's invitations list): the reading of <see cref="AccountEmailHasMarkAsync"/> for all
    /// the invitations of a list in one query - each invitation's rows form their own groups and the latest row of any group decides.
    /// </summary>
    public static async Task<IReadOnlySet<Guid>> InvitationsWithMarkAsync(AppDbContext db, IReadOnlyCollection<Guid> invitationIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(invitationIds);
        Guid[] ids = [.. invitationIds.Distinct()];
        if (ids.Length == 0)
        {
            return new HashSet<Guid>();
        }

        var rows = await db.DispatchRecords.AsNoTracking()
            .Where(d => d.DispatchType == DispatchType.AdminInvitation && d.RelatedRecordType == DispatchRelatedRecordType.AdminInvitation && d.RelatedRecordId != null && ids.Contains(d.RelatedRecordId.Value))
            .Select(d => new { InvitationId = d.RelatedRecordId!.Value, d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState, ClosedUnsent = d.AttemptCount == 0 && d.LastOutcome == null }) // non-null: the Where above keeps only records with a related record
            .ToListAsync(ct);
        return rows
            .GroupBy(r => r.InvitationId)
            .Where(g => HasMark(g.Select(r => new Row(r.Id, r.OriginalDispatchId, r.CreatedAt, r.DispatchState, r.ClosedUnsent))))
            .Select(g => g.Key)
            .ToHashSet();
    }

    /// <summary>
    /// The orders among <paramref name="orderIds"/> that carry the mark (T527; the panel's order list): the reading of <see cref="HasMarkAsync"/> for a page of orders in one query - each
    /// order's rows form their own groups and the latest row of any group decides.
    /// </summary>
    public static async Task<IReadOnlySet<Guid>> OrdersWithMarkAsync(AppDbContext db, IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(orderIds);
        Guid[] ids = [.. orderIds.Distinct()];
        if (ids.Length == 0)
        {
            return new HashSet<Guid>();
        }

        var rows = await db.DispatchRecords.AsNoTracking()
            .Where(d => d.OrderId != null && ids.Contains(d.OrderId.Value))
            .Select(d => new { OrderId = d.OrderId!.Value, d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState, ClosedUnsent = d.AttemptCount == 0 && d.LastOutcome == null }) // non-null: the Where above keeps only rows bound to an order
            .ToListAsync(ct);
        return rows
            .GroupBy(r => r.OrderId)
            .Where(g => HasMark(g.Select(r => new Row(r.Id, r.OriginalDispatchId, r.CreatedAt, r.DispatchState, r.ClosedUnsent))))
            .Select(g => g.Key)
            .ToHashSet();
    }

    /// <summary>
    /// The groups of customer notices whose latest row is "undelivered" - the notices the mark stands for and a re-sending can name (02 9.1.6; 05 4.4.3). A group's first row is the one
    /// whose <c>OriginalDispatchId</c> is empty; when that row is gone by its own period the rows left stand under the id they carry.
    /// </summary>
    public static IReadOnlyList<UndeliveredNotice> UndeliveredGroups(IEnumerable<NoticeRow> rowsOfTheOrder)
    {
        ArgumentNullException.ThrowIfNull(rowsOfTheOrder);
        return [.. rowsOfTheOrder
            .GroupBy(r => r.OriginalDispatchId ?? r.Id)
            .Select(group => (GroupId: group.Key, FirstAt: group.Min(r => r.CreatedAt), Latest: group.Aggregate((a, b) => UuidOrder.IsBefore(a.CreatedAt, a.Id, b.CreatedAt, b.Id) ? b : a)))
            .Where(g => g.Latest.State == DispatchState.Undelivered && !g.Latest.ClosedUnsent)
            .OrderBy(g => g.Latest.CreatedAt).ThenBy(g => g.Latest.Id)
            .Select(g => new UndeliveredNotice(g.GroupId, g.Latest.Id, g.Latest.Type, g.Latest.RelatedRecordType, g.Latest.RelatedRecordId, g.FirstAt, g.Latest.RecipientEmail))];
    }

    /// <summary>The customer notices of an order, B-1…B-16, as the groups read them; a firm notice or an account e-mail has no order link and is never among them.</summary>
    public static async Task<List<NoticeRow>> ReadNoticeRowsAsync(AppDbContext db, Guid orderId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        return await db.DispatchRecords.AsNoTracking()
            .Where(d => d.OrderId == orderId)
            .Select(d => new NoticeRow(d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState, d.DispatchType, d.RelatedRecordType, d.RelatedRecordId, d.AttemptCount == 0 && d.LastOutcome == null, d.RecipientEmail))
            .ToListAsync(ct);
    }

    /// <summary>
    /// The mark of a claim's row in the panel (02 9.1.6; 03 1.7.3.1; 06 4.58: the mark of B-9 shows on the defect claim's own row too): a defect claim whose notice B-9
    /// ends in an "undelivered" row. A withdrawal declaration or a late delivery termination shows it on the order's row only; any other related record kind never shows it.
    /// </summary>
    public static async Task<bool> ClaimHasMarkAsync(AppDbContext db, DispatchRelatedRecordType type, Guid relatedRecordId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (type != DispatchRelatedRecordType.DefectClaim)
        {
            return false;
        }

        List<Row> rows = await db.DispatchRecords.AsNoTracking()
            .Where(d => d.DispatchType == DispatchType.B9 && d.RelatedRecordType == type && d.RelatedRecordId == relatedRecordId)
            .Select(d => new Row(d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState, d.AttemptCount == 0 && d.LastOutcome == null))
            .ToListAsync(ct);
        return HasMark(rows);
    }

    /// <summary>
    /// The defect claims among <paramref name="claimIds"/> whose row carries the mark (T532; the requests list): the same reading as <see cref="ClaimHasMarkAsync"/>, for all the claims of a page in one
    /// query - each claim's B-9 rows form their own groups and the latest row of any group decides.
    /// </summary>
    public static async Task<IReadOnlySet<Guid>> ClaimsWithMarkAsync(AppDbContext db, IReadOnlyCollection<Guid> claimIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(claimIds);
        Guid[] ids = [.. claimIds.Distinct()];
        if (ids.Length == 0)
        {
            return new HashSet<Guid>();
        }

        var rows = await db.DispatchRecords.AsNoTracking()
            .Where(d => d.DispatchType == DispatchType.B9 && d.RelatedRecordType == DispatchRelatedRecordType.DefectClaim && d.RelatedRecordId != null && ids.Contains(d.RelatedRecordId.Value))
            .Select(d => new { ClaimId = d.RelatedRecordId!.Value, d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState, ClosedUnsent = d.AttemptCount == 0 && d.LastOutcome == null }) // non-null: the Where above keeps only records with a related record
            .ToListAsync(ct);
        return rows
            .GroupBy(r => r.ClaimId)
            .Where(g => HasMark(g.Select(r => new Row(r.Id, r.OriginalDispatchId, r.CreatedAt, r.DispatchState, r.ClosedUnsent))))
            .Select(g => g.Key)
            .ToHashSet();
    }

    /// <summary>
    /// One page of the orders that carry the mark, by order id after <paramref name="afterOrderId"/> (null starts at the first): the filtered list the home page's row leads to (02 10.6.1;
    /// K-761) - the set is read, not counted, and never taken whole (09 20 item 2). The candidates are the orders with an "undelivered" customer notice row; each candidate's groups
    /// decide, because a later row of the group may have sent it.
    /// </summary>
    public static async Task<IReadOnlyList<Guid>> MarkedOrderIdsAsync(AppDbContext db, Guid? afterOrderId, int max, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentOutOfRangeException.ThrowIfLessThan(max, 1);
        const int batch = 200;
        List<Guid> marked = [];
        Guid? cursor = afterOrderId;
        while (marked.Count < max)
        {
            Guid? from = cursor;
            List<Guid> candidates = await db.DispatchRecords.AsNoTracking()
                .Where(d => d.OrderId != null && d.DispatchState == DispatchState.Undelivered && (from == null || d.OrderId > from))
                .Select(d => d.OrderId!.Value) // non-null: the Where above keeps only records with an order
                .Distinct()
                .OrderBy(id => id)
                .Take(batch)
                .ToListAsync(ct);
            if (candidates.Count == 0)
            {
                break;
            }

            var rows = await db.DispatchRecords.AsNoTracking()
                .Where(d => d.OrderId != null && candidates.Contains(d.OrderId.Value))
                .Select(d => new { OrderId = d.OrderId!.Value, Row = new Row(d.Id, d.OriginalDispatchId, d.CreatedAt, d.DispatchState, d.AttemptCount == 0 && d.LastOutcome == null) }) // non-null: the Where above keeps only records with an order
                .ToListAsync(ct);
            ILookup<Guid, Row> byOrder = rows.ToLookup(r => r.OrderId, r => r.Row);
            foreach (Guid candidate in candidates)
            {
                if (marked.Count < max && HasMark(byOrder[candidate]))
                {
                    marked.Add(candidate);
                }
            }

            cursor = candidates[^1];
            if (candidates.Count < batch)
            {
                break;
            }
        }

        return marked;
    }

    /// <summary>A customer notice row as the re-sending reads it.</summary>
    internal readonly record struct NoticeRow(
        Guid Id,
        Guid? OriginalDispatchId,
        DateTimeOffset CreatedAt,
        DispatchState State,
        DispatchType Type,
        DispatchRelatedRecordType? RelatedRecordType,
        Guid? RelatedRecordId,
        bool ClosedUnsent = false,
        string? RecipientEmail = null);

    /// <summary>A row as the mark reads it.</summary>
    internal readonly record struct Row(Guid Id, Guid? OriginalDispatchId, DateTimeOffset CreatedAt, DispatchState State, bool ClosedUnsent = false);
}
