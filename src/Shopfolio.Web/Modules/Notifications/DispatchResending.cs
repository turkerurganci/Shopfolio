using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The panel's re-sending of a marked customer notice (02 9.1.6; 05 4.4.3; 06 4.58; 07 UC-153, UC-154): a new "queued" row in the notice's group - the ordinary queue sends it, with the
/// ordinary three retries - so the mark is never cleared by hand: it is read from the latest row of the group (<see cref="DispatchMarks"/>) and falls when that row is sent. The
/// row copies the type, the related record and the event values of the latest row, carries the first row's id as <c>OriginalDispatchId</c> (a re-sending hands the chain's root on) and the creation moment of
/// the group's first e-mail inside its event values, so the re-send sentence outlives the first row (08 2.2.5 item 6; 06 4.58). The recipient is the order's current contact e-mail -
/// except B-15, the one notice that goes to the address from before the correction: its own row names it, and so does the re-sending (02 9.1.6). The body is made when the row is sent, from the
/// order as it is then: B-1's texts are the frozen versions, the access key is the current one. A re-sending is exempt from the per-recipient limit (08 2.2.4) and is written to the operation trail
/// with the order's number and no customer data (02 10.3.1). The condition ("marked, the latest row of its group") is checked here against the rows read, the order's row lock and the
/// concurrent second request are the caller's (T531; 05 4.4.3, 09 9.1 item 5). The method joins the caller's transaction and never opens one.
/// </summary>
internal sealed class DispatchResending
{
    private readonly AppDbContext _db;
    private readonly IAuditTrail _trail;
    private readonly TimeProvider _time;

    public DispatchResending(AppDbContext db, IAuditTrail trail, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(trail);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _trail = trail;
        _time = time;
    }

    /// <summary>The order's customer notices that carry the mark, oldest first.</summary>
    public async Task<IReadOnlyList<UndeliveredNotice>> FindUndeliveredAsync(Guid orderId, CancellationToken ct) =>
        DispatchMarks.UndeliveredGroups(await DispatchMarks.ReadNoticeRowsAsync(_db, orderId, ct));

    /// <summary>Writes the new row of the notice named by <paramref name="request"/> and its trail row, or says why nothing was written.</summary>
    public async Task<NoticeResendResult> ResendAsync(NoticeResendRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.OrderNumber);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("A re-sending's row is written in the caller's transaction and never alone (09 9.1 item 3).");
        }

        List<DispatchMarks.NoticeRow> rows = await DispatchMarks.ReadNoticeRowsAsync(_db, request.OrderId, ct);
        UndeliveredNotice? notice = DispatchMarks.UndeliveredGroups(rows).FirstOrDefault(n => n.LatestDispatchId == request.LatestDispatchId);
        if (notice is null)
        {
            return NoticeResendResult.NotMarked;
        }

        var latest = await _db.DispatchRecords.AsNoTracking()
            .Where(d => d.Id == request.LatestDispatchId)
            .Select(d => new { d.RecipientEmail, d.EventValues })
            .SingleAsync(ct);

        // B-15 goes to the address before the correction, which its own row names; every other notice goes to the order's contact e-mail now (02 9.1.6).
        string? recipient = notice.Type == DispatchType.B15 ? latest.RecipientEmail : request.CurrentContactEmail;
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return NoticeResendResult.NoRecipient;
        }

        // The group's first e-mail: the moment a B-1 correction row already holds, or the earliest row of the group.
        DateTimeOffset firstEmailAt = CustomerNoticeValues.ReadFirstEmailSentAt(latest.EventValues)
            ?? rows.Where(r => (r.OriginalDispatchId ?? r.Id) == notice.GroupId).Min(r => r.CreatedAt);

        DispatchRecord row = DispatchRecord.Queue(notice.Type, recipient.Trim(), request.OrderId, notice.RelatedRecordType, notice.RelatedRecordId, _time.GetUtcNow());
        row.OriginalDispatchId = notice.GroupId;
        row.EventValues = CustomerNoticeValues.WithFirstEmailSentAt(latest.EventValues, firstEmailAt);
        _db.DispatchRecords.Add(row);

        // The re-sending is a firm operation on the order (02 10.3.1); the mark itself never reaches the trail.
        await _trail.WriteAsync(new AuditEntry(AuditOperation.ResendEmail, "Order", request.OrderId, new AuditDetails(request.OrderNumber)), ct);
        return NoticeResendResult.Queued;
    }
}
