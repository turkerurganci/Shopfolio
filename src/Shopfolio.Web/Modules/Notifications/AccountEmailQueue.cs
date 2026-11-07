using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The writer of the account and administration e-mails' dispatch rows (08 2.2.8; 06 4.58; T309): each e-mail is one row in the "queued" state, written in the
/// transaction of the event that made it and saved by that transaction (09 9.1 item 3), to the one address the record names. The row carries the link record's id
/// and never the key (05 6.4.4; K-1081), no order link (so none of these rows ever takes the order's "e-mail did not arrive" mark - the one mark of this family, on the
/// invitation row, is read from the related record by its own screen, 02 9.4), and the event's values. The link record of a link e-mail is made by the event's own
/// module in the same transaction; the Google confirmation link is the one this writer makes itself, because it makes none while a valid one exists
/// (08 2.2.8 "GoogleLinkConfirmationLink"; K-1282). Nothing is sent here.
/// </summary>
internal sealed class AccountEmailQueue
{
    private readonly AppDbContext _db;
    private readonly LinkKeys _keys;
    private readonly TimeProvider _time;

    public AccountEmailQueue(AppDbContext db, LinkKeys keys, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _keys = keys;
        _time = time;
    }

    /// <summary>One row for the e-mail; the record is refused when it breaks the rule of its kind.</summary>
    public int Queue(AccountEmail email)
    {
        AccountEmailRow row = AccountEmailValues.For(email);
        RequireTransaction();
        _db.DispatchRecords.Add(Make(row));
        return 1;
    }

    /// <summary>
    /// The Google sign-in confirmation (K-1281, K-1282): when the customer has a link of this type that is still valid - not used, not superseded, not expired -
    /// no new link is made and no e-mail is queued (false); otherwise a link record born now with the life of P-13 and the row of its e-mail are written (true).
    /// A link whose life ended is not valid, so signing in with Google again makes the new one.
    /// </summary>
    public async Task<bool> QueueGoogleConfirmationAsync(Guid customerId, string email, CancellationToken ct)
    {
        RequireTransaction();
        DateTimeOffset now = _time.GetUtcNow();
        bool pending = await _db.LinkRecords.AsNoTracking().AnyAsync(
            l => l.LinkType == LinkType.GoogleLinkConfirmation && l.CustomerId == customerId && l.UsedAt == null && l.SupersededAt == null && l.ExpiresAt > now,
            ct);
        if (pending)
        {
            return false;
        }

        LinkRecord link = LinkRecord.ForCustomer(_keys, LinkType.GoogleLinkConfirmation, customerId, now.AddMinutes(ProductConstants.PasswordResetLinkMinutes), now);
        AccountEmailRow row = AccountEmailValues.ForGoogleLink(customerId, link.Id, email);
        _db.LinkRecords.Add(link);
        _db.DispatchRecords.Add(Make(row));
        return true;
    }

    private DispatchRecord Make(AccountEmailRow row)
    {
        NotificationMap.Require(row.Type, NoticeFamily.Account);
        DispatchRecord record = DispatchRecord.Queue(row.Type, row.Recipient, null, row.RelatedRecordType, row.RelatedRecordId, _time.GetUtcNow());
        record.EventValues = row.EventValues;
        return record;
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("An account e-mail's row is written in the caller's transaction and never alone (09 9.1 item 3).");
        }
    }
}
