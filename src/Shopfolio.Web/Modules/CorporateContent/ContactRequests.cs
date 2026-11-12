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
            KeptRows(now).Where(c => c.Id == requestId),
            c => c.ContactRequestStatus == ContactRequestStatus.Open,
            set => set.SetProperty(c => c.ContactRequestStatus, ContactRequestStatus.Closed).SetProperty(c => c.LastClosedAt, now),
            ct);
        return await ResultAsync(requestId, written, now, ct);
    }

    /// <summary>Closed to open, by an administrator; the last closing moment is not emptied, so the retention keeps running from it. Writes no notice.</summary>
    public async Task<ContactRequestResult> ReopenAsync(Guid requestId, Guid adminId, CancellationToken ct)
    {
        RequireAdministrator(adminId);
        DateTimeOffset now = _time.GetUtcNow();
        bool written = await CurrentStateWrite.ApplyAsync(
            KeptRows(now).Where(c => c.Id == requestId),
            c => c.ContactRequestStatus == ContactRequestStatus.Closed,
            set => set.SetProperty(c => c.ContactRequestStatus, ContactRequestStatus.Open),
            ct);
        return await ResultAsync(requestId, written, now, ct);
    }

    // A request past its period is not there for the panel, whatever the night's run has reached (T532; 07 UC-94, UC-155, UC-156: K-1192).
    private IQueryable<ContactRequest> KeptRows(DateTimeOffset now) => _db.ContactRequests.Where(ContactRequestRetention.Kept(now));

    private async Task<ContactRequestResult> ResultAsync(Guid requestId, bool written, DateTimeOffset now, CancellationToken ct)
    {
        if (written)
        {
            return new ContactRequestResult(ContactRequestOutcome.Done, requestId);
        }

        bool exists = await KeptRows(now).AsNoTracking().AnyAsync(c => c.Id == requestId, ct);
        return new ContactRequestResult(exists ? ContactRequestOutcome.Unchanged : ContactRequestOutcome.NotFound, requestId);
    }

    /// <summary>
    /// The topics the sales summary's contact request count takes (02 10.6.3; K-302, K-457): the storefront's output - General question, About a product, Other. "KVKK talebi" and
    /// "Sipariş hakkında" are after-sale and personal data applications and stay out.
    /// </summary>
    internal static readonly ContactTopic[] SummaryTopics = [ContactTopic.GeneralQuestion, ContactTopic.AboutProduct, ContactTopic.Other];

    /// <summary>
    /// The requests still open, whatever their topic (T382; 02 10.6.1: the panel home's "açık talep" counts every open contact request); an index on (status, created at) serves it. A request past its
    /// period is not counted (T532; 07 UC-93): the count is the "Açık" list's row count.
    /// </summary>
    public Task<int> CountOpenAsync(CancellationToken ct) => CountAsync([ContactRequestStatus.Open], ct);

    /// <summary>The kept requests in one of <paramref name="statuses"/> (none named: any status) - the panel list's share of the row count (T532; 07 UC-93).</summary>
    public Task<int> CountAsync(IReadOnlyCollection<ContactRequestStatus> statuses, CancellationToken ct) =>
        Filtered(statuses).CountAsync(ct);

    /// <summary>
    /// The first <paramref name="take"/> kept requests in one of <paramref name="statuses"/> (none named: any status), newest first - the moment the request reached the firm, equal moments by id
    /// as the database sorts it (04 9.10.6; 06 2.3.1). The panel merges them with the defect claims and cuts the page.
    /// </summary>
    public async Task<IReadOnlyList<PanelContactRequestRow>> ListAsync(IReadOnlyCollection<ContactRequestStatus> statuses, int take, CancellationToken ct)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(take);
        return await Filtered(statuses)
            .OrderByDescending(c => c.CreatedAt)
            .ThenByDescending(c => c.Id)
            .Take(take)
            .Select(c => new PanelContactRequestRow(c.Id, c.ContactTopic, c.Name, c.Email, c.ContactRequestStatus, c.CreatedAt))
            .ToListAsync(ct);
    }

    /// <summary>The detail of one kept request; null when there is none - never there, or past its period (K-1192).</summary>
    public async Task<ContactRequestDetail?> FindAsync(Guid requestId, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        return await KeptRows(now).AsNoTracking()
            .Where(c => c.Id == requestId)
            .Select(c => new ContactRequestDetail(c.Id, c.ContactTopic, c.ContactRequestStatus, c.Name, c.Email, c.Phone, c.Message, c.CreatedAt, c.LastClosedAt))
            .SingleOrDefaultAsync(ct);
    }

    private IQueryable<ContactRequest> Filtered(IReadOnlyCollection<ContactRequestStatus> statuses)
    {
        ArgumentNullException.ThrowIfNull(statuses);
        IQueryable<ContactRequest> rows = KeptRows(_time.GetUtcNow()).AsNoTracking();
        if (statuses.Count == 0)
        {
            return rows;
        }

        ContactRequestStatus[] named = [.. statuses];
        return rows.Where(c => named.Contains(c.ContactRequestStatus));
    }

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
