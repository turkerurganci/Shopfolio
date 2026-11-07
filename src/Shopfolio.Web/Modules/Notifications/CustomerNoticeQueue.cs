using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The writer of the dispatch rows of the customer notices B-1…B-16 (08 2.2.6; 06 4.58; T306, T307): each notice is one row in the "queued" state, written in the
/// transaction of the event that made it and saved by that transaction (09 9.1 item 3; the row commits with the event or goes with it). The row carries the
/// order link and no related record - except B-9's, which names its withdrawal, termination or claim (06 4.58's type table), the event's values (<see cref="CustomerNoticeValues"/>) and, on the B-1 after a correction, the first
/// e-mail's row id (the chain's root: a given row that is itself a re-sending hands on the id it carries) and its creation moment (read from the given row, which must be the order's own B-1; the moment is the event value the re-send sentence is made from, so it
/// outlives the first row's own deletion - 08 2.2.5 item 6). Nothing is sent here, and the body is not stored: it is made when the row is sent, from the order and these values.
/// <para>
/// B-3 is once per window (08 2.2.6 B-3; 03 4.1.9): a second row for the same window end is not written, a window that restarted after the "paid" mark was
/// corrected has a new end and gets its own reminder, and a window that has already run out gets none - the case of a reminder due during an outage (T324).
/// The caller holds the order's row lock while it queues (09 9.1 item 5), which is what keeps two concurrent callers from both finding no earlier B-3.
/// </para>
/// </summary>
internal sealed class CustomerNoticeQueue
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _time;

    public CustomerNoticeQueue(AppDbContext db, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _time = time;
    }

    /// <summary>
    /// Queues a notice B-1…B-16 for <paramref name="recipientEmail"/>, the order's contact e-mail (the new address on a B-1 correction; the old address on B-15).
    /// B-9 also writes its related record's type and id (06 4.58's type table). Returns the number of rows
    /// written: one, or zero for a B-3 that is not due (already sent for this window, or the window has run out).
    /// </summary>
    public async Task<int> QueueAsync(CustomerNotice notice, string recipientEmail, CancellationToken ct)
    {
        CustomerNoticeRow row = CustomerNoticeValues.For(notice);
        NotificationMap.Require(row.Type, NoticeFamily.Customer);
        ArgumentException.ThrowIfNullOrWhiteSpace(recipientEmail);
        RequireTransaction();
        DateTimeOffset now = _time.GetUtcNow();

        if (notice is OrderReceivedNotice { OriginalDispatchId: { } firstId } received)
        {
            // The correction's row keeps the first e-mail's creation moment: the first row may be deleted by its own period before the re-send is made (08 2.2.5 item 6).
            // Its group id is the chain's root: a given row that is itself a re-sending names the root it carries, so a second correction joins the first one's group (06 4.58).
            (Guid rootId, DateTimeOffset firstMoment) = await ReadFirstEmailAsync(received.OrderId, firstId, ct);
            row = CustomerNoticeValues.For(received with { OriginalDispatchId = rootId, FirstEmailSentAt = firstMoment });
        }

        if (row.Type == DispatchType.B3 && !await ReminderIsDueAsync(notice.OrderId, row, now, ct))
        {
            return 0;
        }

        DispatchRecord record = DispatchRecord.Queue(row.Type, recipientEmail.Trim(), notice.OrderId, row.RelatedRecordType, row.RelatedRecordId, now);
        record.OriginalDispatchId = row.OriginalDispatchId;
        record.EventValues = row.EventValues;
        _db.DispatchRecords.Add(record);
        return 1;
    }

    private async Task<(Guid RootId, DateTimeOffset FirstMoment)> ReadFirstEmailAsync(Guid orderId, Guid givenId, CancellationToken ct)
    {
        // The row must be this order's own B-1; a row that is itself a correction already holds the chain's root id and the first e-mail's moment, which are carried on unchanged.
        var given = await _db.DispatchRecords.AsNoTracking()
            .Where(d => d.Id == givenId && d.OrderId == orderId && d.DispatchType == DispatchType.B1)
            .Select(d => new { d.CreatedAt, d.EventValues, d.OriginalDispatchId })
            .SingleOrDefaultAsync(ct)
            ?? throw new ArgumentException("The first e-mail of a B-1 correction is this order's own B-1 row (06 4.58).", nameof(givenId));
        return (given.OriginalDispatchId ?? givenId, CustomerNoticeValues.ReadFirstEmailSentAt(given.EventValues) ?? given.CreatedAt);
    }

    private async Task<bool> ReminderIsDueAsync(Guid orderId, CustomerNoticeRow row, DateTimeOffset now, CancellationToken ct)
    {
        DateTimeOffset windowEnd = CustomerNoticeValues.ReadWindowEnd(row.EventValues) ?? throw new InvalidOperationException("A B-3 row holds its window end.");
        if (windowEnd <= now)
        {
            return false;
        }

        // The rows of this order's earlier reminders: the order has few, and the window end sits in the stored list, which is read and never queried (06 4.58).
        List<string?> earlier = await _db.DispatchRecords.AsNoTracking()
            .Where(d => d.OrderId == orderId && d.DispatchType == DispatchType.B3)
            .Select(d => d.EventValues)
            .ToListAsync(ct);
        return !earlier.Any(values => CustomerNoticeValues.ReadWindowEnd(values) == windowEnd);
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A customer notice's row is written in the caller's transaction and never alone (09 9.1 item 3).");
        }
    }
}
