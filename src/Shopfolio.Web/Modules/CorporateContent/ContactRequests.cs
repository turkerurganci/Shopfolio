using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.CorporateContent;

/// <summary>What became of a contact request write (T322).</summary>
internal enum ContactRequestOutcome
{
    /// <summary>The request was born open, or its status changed.</summary>
    Done,

    /// <summary>The request was already in the asked status (a second close of a closed request, a reopening of an open one); nothing was written.</summary>
    Unchanged,

    /// <summary>No such request - never there, or already destroyed at the end of its retention.</summary>
    NotFound,
}

/// <summary>The result of a contact request write; a refusal is a return value (09 9.1 item 3).</summary>
internal sealed record ContactRequestResult(ContactRequestOutcome Outcome, Guid? RequestId = null) : IOperationResult
{
    public bool IsRejected => Outcome == ContactRequestOutcome.NotFound;
}

/// <summary>The contact form's values at the moment of the send (07 UC-15); the phone is the ten-digit stored form or empty (06 2.5.1).</summary>
internal sealed record NewContactRequest(string Name, string Email, string? Phone, ContactTopic Topic, string Message);

/// <summary>
/// The two statuses of the contact request, KVKK request included (T322; 02 5.13; 03 1.9.2): the request is born open and reaches the firm by the
/// notice F-2; only an administrator closes it and reopens it, and neither produces a notice. There is no state machine and no period counter here:
/// the retention runs from <see cref="ContactRequest.RetentionStart"/>. A close and a reopening are one conditional update that names the status the
/// request must be in now (06 2.6.5; 09 10.2 item 4), so a request has no version and two administrators acting together write once. Every method joins the
/// caller's transaction and never opens one (09 9.1 item 3).
/// </summary>
internal sealed class ContactRequests
{
    private readonly AppDbContext _db;
    private readonly INotificationsModule _notifications;
    private readonly TimeProvider _time;

    public ContactRequests(AppDbContext db, INotificationsModule notifications, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _notifications = notifications;
        _time = time;
    }

    /// <summary>
    /// The send of the contact form: the request is written open and the notice F-2 is queued for the firm in the same transaction, so the request and its
    /// notice are written together or not at all. The notice carries the topic only; the sender's name, e-mail and message stay in the request.
    /// </summary>
    public async Task<ContactRequestResult> OpenAsync(NewContactRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        DateTimeOffset now = _time.GetUtcNow();
        ContactRequest row = ContactRequest.Create(request.Name, request.Email, request.Topic, request.Message, now, request.Phone);
        _db.ContactRequests.Add(row);
        await _notifications.QueueCompanyNoticeAsync(new NewContactRequestNotice(row.Id, row.ContactTopic, now), ct);
        return new ContactRequestResult(ContactRequestOutcome.Done, row.Id);
    }

    /// <summary>Open to closed, by an administrator; writes the closing moment again every time, writes no notice. A closed request stays as it is.</summary>
    public async Task<ContactRequestResult> CloseAsync(Guid requestId, Guid adminId, CancellationToken ct)
    {
        RequireAdministrator(adminId);
        DateTimeOffset now = _time.GetUtcNow();
        bool written = await CurrentStateWrite.ApplyAsync(
            _db.ContactRequests.Where(c => c.Id == requestId),
            c => c.ContactRequestStatus == ContactRequestStatus.Open,
            set => set.SetProperty(c => c.ContactRequestStatus, ContactRequestStatus.Closed).SetProperty(c => c.LastClosedAt, now),
            ct);
        return await ResultAsync(requestId, written, ct);
    }

    /// <summary>Closed to open, by an administrator; the last closing moment is not emptied, so the retention keeps running from it. Writes no notice.</summary>
    public async Task<ContactRequestResult> ReopenAsync(Guid requestId, Guid adminId, CancellationToken ct)
    {
        RequireAdministrator(adminId);
        bool written = await CurrentStateWrite.ApplyAsync(
            _db.ContactRequests.Where(c => c.Id == requestId),
            c => c.ContactRequestStatus == ContactRequestStatus.Closed,
            set => set.SetProperty(c => c.ContactRequestStatus, ContactRequestStatus.Open),
            ct);
        return await ResultAsync(requestId, written, ct);
    }

    private async Task<ContactRequestResult> ResultAsync(Guid requestId, bool written, CancellationToken ct)
    {
        if (written)
        {
            return new ContactRequestResult(ContactRequestOutcome.Done, requestId);
        }

        bool exists = await _db.ContactRequests.AsNoTracking().AnyAsync(c => c.Id == requestId, ct);
        return new ContactRequestResult(exists ? ContactRequestOutcome.Unchanged : ContactRequestOutcome.NotFound, requestId);
    }

    /// <summary>
    /// The topics the sales summary's contact request count takes (02 10.6.3; K-302, K-457): the storefront's output - General question, About a product, Other. "KVKK talebi" and
    /// "Sipariş hakkında" are after-sale and personal data applications and stay out.
    /// </summary>
    internal static readonly ContactTopic[] SummaryTopics = [ContactTopic.GeneralQuestion, ContactTopic.AboutProduct, ContactTopic.Other];

    /// <summary>The requests opened in <c>[from, to)</c> with one of <see cref="SummaryTopics"/>; a request destroyed at the end of its retention is no longer there to count.</summary>
    public Task<int> CountForSummaryAsync(DateTimeOffset from, DateTimeOffset to, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(to, from);
        return _db.ContactRequests.AsNoTracking().CountAsync(c => c.CreatedAt >= from && c.CreatedAt < to && SummaryTopics.Contains(c.ContactTopic), ct);
    }

    /// <summary>Only the firm's administrator closes and reopens a request (03 1.9.2); a call without an administrator's identity is a programming error.</summary>
    private static void RequireAdministrator(Guid adminId)
    {
        if (adminId == Guid.Empty)
        {
            throw new ArgumentException("A contact request is closed and reopened by an administrator, who is named (03 1.9.2).", nameof(adminId));
        }
    }
}
