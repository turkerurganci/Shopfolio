using System.ComponentModel.DataAnnotations;
using System.Net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The customer's change of the account's e-mail address (02 3.13.14, 3.13.17; 07 UC-66, UC-67, UC-53; T343): the request writes a pending change and sends a link to the NEW address; the
/// account keeps working with the old address until the button of that link is pressed. Verifying changes the address, drops the Google link, sends the old address a notice with the
/// revert link (Z-45; its outcome is T519's) and lets the new address's guest orders fall to the account. Everything joins the caller's transaction and never saves; the account's row
/// is locked first, then the pending and link rows (05 6.1.2). The request takes the reauthentication's single-use value last (T517; D-429); the administrator's change is the panel's task.
/// </summary>
internal sealed class EmailChanges
{
    /// <summary>The field name of the new address in a refusal: the account form's input (07 7.1.15).</summary>
    public const string NewEmailField = "NewEmail";

    private static readonly InvalidEmailAttribute EmailRule = new();

    private readonly AppDbContext _db;
    private readonly LinkRecords _links;
    private readonly LinkKeys _keys;
    private readonly AttemptCounter _attempts;
    private readonly INotificationsModule _notifications;
    private readonly IOrderingModule _ordering;
    private readonly TimeProvider _time;
    private readonly ISessionService _sessions;

    public EmailChanges(AppDbContext db, LinkRecords links, LinkKeys keys, AttemptCounter attempts, INotificationsModule notifications, IOrderingModule ordering, TimeProvider time, ISessionService sessions)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        _sessions = sessions;
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(attempts);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _links = links;
        _keys = keys;
        _attempts = attempts;
        _notifications = notifications;
        _ordering = ordering;
        _time = time;
    }

    public async Task<EmailChangeRequestResult> RequestAsync(Guid customerId, Guid sessionId, string newEmail, string? reauthenticationValue, IPAddress? ipAddress, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(newEmail);
        RequireTransaction("The e-mail change is written only inside a transaction (09 6 item 3).");

        if (!await AccountLocks.LockAsync(_db, AccountType.Customer, customerId, ct))
        {
            return EmailChangeRequestResult.Refused(ErrorCode.NotFound);
        }

        // The value of the re-authentication is read first (a plain read; it is taken last, below), so no refusal that follows - the address's format, an address another account holds, the limit - is shown to a
        // session that does not hold the permission, and L-9 counts no line for it (02 3.13.11; K-786; VAL-A504).
        if (!await _sessions.HoldsReauthenticationAsync(customerId, sessionId, ReauthenticationOperation.EmailChange, reauthenticationValue, ct))
        {
            return EmailChangeRequestResult.Refused(ErrorCode.ReauthenticationRequired);
        }

        var account = await _db.Customers.AsNoTracking()
            .Where(c => c.Id == customerId)
            .Select(c => new { c.NormalizedEmail, c.EmailVerifiedAt })
            .SingleAsync(ct);
        if (account.EmailVerifiedAt is null)
        {
            return EmailChangeRequestResult.Refused(ErrorCode.PendingRegistrationUnresolved);
        }

        string entered = newEmail.Trim();
        if (entered.Length == 0)
        {
            return EmailChangeRequestResult.FieldRefused(new MembershipFieldError(NewEmailField, ErrorCode.FieldRequired));
        }

        if (EmailRule.GetValidationResult(entered, new ValidationContext(entered)) is not null)
        {
            return EmailChangeRequestResult.FieldRefused(new MembershipFieldError(NewEmailField, ErrorCode.InvalidEmail));
        }

        DateTimeOffset now = _time.GetUtcNow();

        // An open revert link closes the change (02 3.13.14, 3.15.6): a following change must not shut the earlier address's way back.
        DateTimeOffset? revertEnds = await EmailChangeRules.OpenRevertEndAsync(_db, AccountType.Customer, customerId, now, ct);
        if (revertEnds is { } ends)
        {
            return EmailChangeRequestResult.RevertPending(ends);
        }

        // Only a verified account holds an address; an unverified pending registration does not stop the request (02 3.13.7, 3.13.11; K-1134).
        string normalizedNew = TextNormalizer.NormalizeIdentity(entered);
        if (normalizedNew == account.NormalizedEmail || await _db.Customers.AsNoTracking().AnyAsync(c => c.NormalizedEmail == normalizedNew && c.EmailVerifiedAt != null, ct))
        {
            return EmailChangeRequestResult.Refused(ErrorCode.EmailTakenByOtherAccount);
        }

        PendingEmailChange? pending = await _db.PendingEmailChanges.SingleOrDefaultAsync(p => p.CustomerId == customerId, ct);
        bool replaces = pending is not null && !EmailChangeRules.IsEnded(pending.CreatedAt, now);
        if (replaces && await EmailChangeRules.CountedOverLimitAsync(_attempts, account.NormalizedEmail, ipAddress, ct))
        {
            return EmailChangeRequestResult.Refused(ErrorCode.VerificationLinkLimitExceeded);
        }

        // The single-use value of the reauthentication is taken last, so a refusal above takes nothing and a refusal here rolls the caller's transaction back (07 2.5.2; K-971; D-429).
        if (!await _sessions.TryRedeemReauthenticationAsync(customerId, sessionId, ReauthenticationOperation.EmailChange, reauthenticationValue, ct))
        {
            return EmailChangeRequestResult.Refused(ErrorCode.ReauthenticationRequired);
        }

        // The pending change takes the place of the earlier one and its life starts now; the earlier link is no longer valid (02 3.13.14; K-734).
        if (pending is null)
        {
            _db.PendingEmailChanges.Add(PendingEmailChange.ForCustomer(customerId, entered, now));
        }
        else
        {
            pending.SetNewEmail(entered);
            pending.CreatedAt = now;
        }

        DateTimeOffset expiresAt = now.AddHours(ProductConstants.EmailVerificationLinkHours);
        LinkRecord link = LinkRecord.ForCustomer(_keys, LinkType.NewEmailVerification, customerId, expiresAt, now);
        _links.Add(link);
        await _links.SupersedeEarlierAsync(LinkType.NewEmailVerification, customerId, null, link.Id, ct);
        await _notifications.QueueAccountEmailAsync(new NewAddressVerificationEmail(DispatchRelatedRecordType.Customer, customerId, link.Id, entered), ct);
        return EmailChangeRequestResult.Requested(link.Id, expiresAt, replaces);
    }

    /// <summary>The account's pending change that still lives (T517; 04 5.25.4; read only): the new address and the last day of its link, or nothing when there is none or its life has ended.</summary>
    public async Task<PendingEmailChangeReading?> ReadPendingAsync(Guid customerId, CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        var pending = await _db.PendingEmailChanges.AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .Select(p => new { p.NewEmail, p.CreatedAt })
            .SingleOrDefaultAsync(ct);
        return pending is null || EmailChangeRules.IsEnded(pending.CreatedAt, now)
            ? null
            : new PendingEmailChangeReading(pending.NewEmail, pending.CreatedAt.AddHours(ProductConstants.EmailVerificationLinkHours));
    }

    public async Task<EmailChangeResendResult> ResendAsync(Guid customerId, IPAddress? ipAddress, CancellationToken ct)
    {
        RequireTransaction("The new address's link is written only inside a transaction (09 6 item 3).");
        if (!await AccountLocks.LockAsync(_db, AccountType.Customer, customerId, ct))
        {
            return EmailChangeResendResult.Nothing();
        }

        string normalizedEmail = await _db.Customers.AsNoTracking().Where(c => c.Id == customerId).Select(c => c.NormalizedEmail).SingleAsync(ct);

        // L-9 is counted on every request, on the e-mail and on the IP (02 3.13.14, 8.2); over the limit the earlier link stays valid.
        if (await EmailChangeRules.CountedOverLimitAsync(_attempts, normalizedEmail, ipAddress, ct))
        {
            return EmailChangeResendResult.Limited();
        }

        DateTimeOffset now = _time.GetUtcNow();
        var pending = await _db.PendingEmailChanges.AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .Select(p => new { p.NewEmail, p.CreatedAt })
            .SingleOrDefaultAsync(ct);
        if (pending is null || EmailChangeRules.IsEnded(pending.CreatedAt, now))
        {
            return EmailChangeResendResult.Nothing();
        }

        // The new link ends where the pending change ends: asking again does not lengthen it (02 3.13.5).
        DateTimeOffset expiresAt = EmailChangeRules.LinkEnd(pending.CreatedAt);
        LinkRecord link = LinkRecord.ForCustomer(_keys, LinkType.NewEmailVerification, customerId, expiresAt, now);
        _links.Add(link);
        await _links.SupersedeEarlierAsync(LinkType.NewEmailVerification, customerId, null, link.Id, ct);
        await _notifications.QueueAccountEmailAsync(new NewAddressVerificationEmail(DispatchRelatedRecordType.Customer, customerId, link.Id, pending.NewEmail), ct);
        return EmailChangeResendResult.Issued(link.Id, expiresAt);
    }

    /// <summary>
    /// The new address's button (02 3.13.14; 07 UC-53): the address changes (which holds the account's row), the Google link goes, the link is consumed with one conditional update, the
    /// unverified pending registration of the address is deleted in the same transaction, the old address gets the notice with the revert link and the new address's guest orders fall to
    /// the account. A refusal rolls the caller's transaction back, which takes the consumption back (02 3.13.14, K-1080).
    /// </summary>
    public async Task<NewEmailVerificationResult> VerifyNewEmailAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        RequireTransaction("The new address is verified only inside a transaction (02 3.13.14; 09 6 item 3).");

        // The account's lock family (05 7.3.5) is taken alone, and this command ends in the guest orders' row locks, so the account's row is held by the update of its address
        // below, which comes before the link is consumed: the row first, then the link (05 6.1.2). A sign-in that holds the row makes the update wait; one that comes after sees the address.
        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.NewEmailVerification, CustomerId: Guid customerId } record || reading.State is not { } state)
        {
            return NewEmailVerificationResult.LinkInvalid(VerificationLinkStatus.Missing);
        }

        if (state != LinkState.Valid)
        {
            return NewEmailVerificationResult.LinkInvalid(MembershipModule.StatusOf(state));
        }

        DateTimeOffset now = _time.GetUtcNow();
        var pending = await _db.PendingEmailChanges.AsNoTracking()
            .Where(p => p.CustomerId == customerId)
            .Select(p => new { p.Id, p.NewEmail, p.NormalizedNewEmail, p.CreatedAt })
            .SingleOrDefaultAsync(ct);
        if (pending is null || EmailChangeRules.IsEnded(pending.CreatedAt, now))
        {
            return NewEmailVerificationResult.LinkInvalid(VerificationLinkStatus.Expired);
        }

        var account = await _db.Customers.AsNoTracking().Where(c => c.Id == customerId).Select(c => new { c.Email, c.NormalizedEmail, c.EmailVerifiedAt }).SingleOrDefaultAsync(ct);
        if (account?.EmailVerifiedAt is null)
        {
            return NewEmailVerificationResult.LinkInvalid(VerificationLinkStatus.Missing);
        }

        // The address became a verified account's meanwhile: the change is not valid (02 3.13.14; K-1080).
        if (await _db.Customers.AsNoTracking().AnyAsync(c => c.NormalizedEmail == pending.NormalizedNewEmail && c.Id != customerId && c.EmailVerifiedAt != null, ct))
        {
            return NewEmailVerificationResult.Taken();
        }

        // An unverified pending registration does not hold the address: it goes in this transaction, counted in the disposal record (02 3.13.14; K-1080).
        int removed = await _db.Customers.Where(c => c.NormalizedEmail == pending.NormalizedNewEmail && c.Id != customerId && c.EmailVerifiedAt == null).ExecuteDeleteAsync(ct);
        if (removed > 0)
        {
            _db.DisposalRecords.Add(DisposalRecord.Counted(DisposalRecordType.PendingRegistration, removed, now));
        }

        try
        {
            string newEmail = pending.NewEmail;
            string normalizedNew = pending.NormalizedNewEmail;
            string oldNormalized = account.NormalizedEmail;
            int changed = await _db.Customers
                .Where(c => c.Id == customerId && c.NormalizedEmail == oldNormalized)
                .ExecuteUpdateAsync(
                    set => set.SetProperty(c => c.Email, newEmail).SetProperty(c => c.NormalizedEmail, normalizedNew).SetProperty(c => c.GoogleLinkedAt, (DateTimeOffset?)null),
                    ct);
            if (changed != 1)
            {
                // The address moved under another request that came first: nothing is changed here and the caller's rollback takes the deletion back.
                return NewEmailVerificationResult.LinkInvalid(VerificationLinkStatus.Superseded);
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Another verified account took the address between the check and the write; the unique index stopped it and the caller's rollback takes everything back.
            return NewEmailVerificationResult.Taken();
        }

        // One conditional update: of two requests at once exactly one takes the link (07 2.5.3); a refusal rolls the address back with the caller's transaction.
        if (!await _links.TryConsumeAsync(record.Id, ct))
        {
            LinkReading again = await _links.ReadAsync(key, ct);
            return NewEmailVerificationResult.LinkInvalid(again.State is { } current ? MembershipModule.StatusOf(current) : VerificationLinkStatus.Missing);
        }

        await _db.PendingEmailChanges.Where(p => p.Id == pending.Id).ExecuteDeleteAsync(ct);

        // A Google confirmation link was asked for the old address: the account's address changed, so it is no longer valid (02 3.13.7).
        await _links.SupersedeEarlierAsync(LinkType.GoogleLinkConfirmation, customerId, null, Guid.Empty, ct);

        // The old address gets the notice and the revert link (Z-45); the link holds the old address as entered (06 4.42). Its outcome is T519's.
        DateTimeOffset revertEnds = now.AddDays(ProductConstants.EmailChangeRevertLinkDays);
        LinkRecord revert = LinkRecord.ForCustomer(_keys, LinkType.EmailChangeRevert, customerId, revertEnds, now);
        revert.PreviousEmail = account.Email;
        _links.Add(revert);
        await _notifications.QueueAccountEmailAsync(new AddressChangeNoticeEmail(DispatchRelatedRecordType.Customer, customerId, revert.Id, account.Email, now), ct);

        // The binding rule of 02 3.13.2 works for the new address; the orders of the old address stay on the account and none is unbound (02 3.13.14).
        int linked = await _ordering.LinkGuestOrdersAsync(customerId, pending.NewEmail, ct);
        return NewEmailVerificationResult.Verified(customerId, linked, revert.Id, revertEnds);
    }

    private void RequireTransaction(string message)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(message);
        }
    }
}
