using System.ComponentModel.DataAnnotations;
using System.Net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Administration.Contracts.Results;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The administrator's change of their own e-mail address (T379; 02 10.2.4; 07 UC-179, UC-180, UC-186): the customer's regime with the administrator's account - the request writes a
/// pending change (the Membership module's table, reached through <see cref="IAdminPendingEmailChanges"/>) and sends a link to the NEW address, the old address keeps working until the
/// link's button is pressed, the request after a reauthentication, L-9 on a request that replaces a pending one, the old address told with a revert link when the change takes
/// effect. The address is unique among administrator accounts only (a customer account's address is no obstacle, 02 10.2.5). The change is written to the operation trail and does not
/// touch the firm's contact e-mail (06 4.37: the account's address and the firm identity's contact address are two values). The account row is locked first, then the pending and the
/// link rows (05 6.1.2). Everything joins the caller's transaction and never saves it.
/// </summary>
internal sealed class AdminEmailChanges
{
    private static readonly InvalidEmailAttribute EmailRule = new();

    private readonly AppDbContext _db;
    private readonly LinkRecords _links;
    private readonly LinkKeys _keys;
    private readonly AttemptCounter _attempts;
    private readonly INotificationsModule _notifications;
    private readonly IAdminPendingEmailChanges _pending;
    private readonly AdminPasswords _passwords;
    private readonly SessionStore _sessions;
    private readonly AuditTrail _trail;
    private readonly TimeProvider _time;

    public AdminEmailChanges(
        AppDbContext db,
        LinkRecords links,
        LinkKeys keys,
        AttemptCounter attempts,
        INotificationsModule notifications,
        IAdminPendingEmailChanges pending,
        AdminPasswords passwords,
        SessionStore sessions,
        AuditTrail trail,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(attempts);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(pending);
        ArgumentNullException.ThrowIfNull(passwords);
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(trail);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _links = links;
        _keys = keys;
        _attempts = attempts;
        _notifications = notifications;
        _pending = pending;
        _passwords = passwords;
        _sessions = sessions;
        _trail = trail;
        _time = time;
    }

    public async Task<AdminEmailChangeRequestResult> RequestAsync(Guid adminId, Guid sessionId, string newEmail, string? reauthenticationValue, IPAddress? ipAddress, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(newEmail);
        RequireTransaction("The e-mail change is written only inside a transaction (09 6 item 3).");

        if (!await AccountLocks.LockAsync(_db, AccountType.Admin, adminId, ct))
        {
            return AdminEmailChangeRequestResult.Refused(AdminEmailChangeOutcome.NotFound, ErrorCode.NotFound);
        }

        string ownNormalized = await _db.Admins.AsNoTracking().Where(a => a.Id == adminId).Select(a => a.NormalizedEmail).SingleAsync(ct);
        string entered = newEmail.Trim();
        if (entered.Length == 0)
        {
            return AdminEmailChangeRequestResult.Refused(AdminEmailChangeOutcome.FieldRefused, ErrorCode.FieldRequired);
        }

        if (EmailRule.GetValidationResult(entered, new ValidationContext(entered)) is not null)
        {
            return AdminEmailChangeRequestResult.Refused(AdminEmailChangeOutcome.FieldRefused, ErrorCode.InvalidEmail);
        }

        DateTimeOffset now = _time.GetUtcNow();

        // An open revert link closes the change: a following change must not shut the earlier address's way back (02 3.13.14; 07 8.1.3).
        DateTimeOffset? revertEnds = await OpenRevertEndAsync(adminId, now, ct);
        if (revertEnds is { } ends)
        {
            return AdminEmailChangeRequestResult.Reverting(ends);
        }

        string normalizedNew = TextNormalizer.NormalizeIdentity(entered);
        if (normalizedNew == ownNormalized || await _db.Admins.AsNoTracking().AnyAsync(a => a.NormalizedEmail == normalizedNew, ct))
        {
            return AdminEmailChangeRequestResult.Refused(AdminEmailChangeOutcome.FieldRefused, ErrorCode.AdminEmailTaken);
        }

        AdminPendingEmailChange? pending = await _pending.FindAsync(adminId, ct);
        bool replaces = pending is not null && !IsEnded(pending.CreatedAt, now);
        if (replaces && await CountedOverLimitAsync(ownNormalized, ipAddress, ct))
        {
            return AdminEmailChangeRequestResult.Refused(AdminEmailChangeOutcome.Limited, ErrorCode.VerificationLinkLimitExceeded);
        }

        // The single-use value of the reauthentication is taken last, so a refusal above takes nothing (07 2.5.2; K-971).
        if (!await _sessions.TryRedeemReauthenticationAsync(AccountType.Admin, adminId, sessionId, ReauthenticationOperation.EmailChange, reauthenticationValue, ct))
        {
            return AdminEmailChangeRequestResult.Refused(AdminEmailChangeOutcome.ReauthenticationRequired, ErrorCode.ReauthenticationRequired);
        }

        await _pending.SaveAsync(adminId, entered, now, ct);
        DateTimeOffset expiresAt = now.AddHours(ProductConstants.EmailVerificationLinkHours);
        LinkRecord link = LinkRecord.ForAdmin(_keys, LinkType.NewEmailVerification, adminId, expiresAt, now);
        _links.Add(link);
        await _links.SupersedeEarlierAsync(LinkType.NewEmailVerification, null, adminId, link.Id, ct);
        await _notifications.QueueAccountEmailAsync(new NewAddressVerificationEmail(DispatchRelatedRecordType.Admin, adminId, link.Id, entered), ct);
        return AdminEmailChangeRequestResult.Requested(link.Id, expiresAt, replaces);
    }

    public async Task<AdminEmailChangeResendResult> ResendAsync(Guid adminId, IPAddress? ipAddress, CancellationToken ct)
    {
        RequireTransaction("The new address's link is written only inside a transaction (09 6 item 3).");
        if (!await AccountLocks.LockAsync(_db, AccountType.Admin, adminId, ct))
        {
            return AdminEmailChangeResendResult.Nothing();
        }

        string normalizedEmail = await _db.Admins.AsNoTracking().Where(a => a.Id == adminId).Select(a => a.NormalizedEmail).SingleAsync(ct);

        // L-9 is counted on every request, on the e-mail and on the IP (02 8.2); over the limit the earlier link stays valid.
        if (await CountedOverLimitAsync(normalizedEmail, ipAddress, ct))
        {
            return AdminEmailChangeResendResult.Limited();
        }

        DateTimeOffset now = _time.GetUtcNow();
        AdminPendingEmailChange? pending = await _pending.FindAsync(adminId, ct);
        if (pending is null || IsEnded(pending.CreatedAt, now))
        {
            return AdminEmailChangeResendResult.Nothing();
        }

        // The new link ends where the pending change ends: asking again does not lengthen it (02 3.13.5).
        DateTimeOffset expiresAt = pending.CreatedAt.AddHours(ProductConstants.EmailVerificationLinkHours);
        LinkRecord link = LinkRecord.ForAdmin(_keys, LinkType.NewEmailVerification, adminId, expiresAt, now);
        _links.Add(link);
        await _links.SupersedeEarlierAsync(LinkType.NewEmailVerification, null, adminId, link.Id, ct);
        await _notifications.QueueAccountEmailAsync(new NewAddressVerificationEmail(DispatchRelatedRecordType.Admin, adminId, link.Id, pending.NewEmail), ct);
        return AdminEmailChangeResendResult.Issued(link.Id, expiresAt);
    }

    /// <summary>
    /// The new address's button (07 UC-186; 02 10.2.4): the address changes under the account's row lock, the link is consumed with one conditional update, the pending change goes, the
    /// old address gets the notice with the revert link, and the trail row is written for the administrator whose address changed (the link opens no session). When the address
    /// became another administrator's meanwhile nothing is consumed (422). A refusal rolls the caller's transaction back.
    /// </summary>
    public async Task<AdminEmailVerificationResult> VerifyNewEmailAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        RequireTransaction("The new address is verified only inside a transaction (09 6 item 3).");

        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.NewEmailVerification, AdminId: Guid adminId } record || reading.State is not { } state)
        {
            return AdminEmailVerificationResult.LinkInvalid(AccountLinkStatus.Missing);
        }

        if (state != LinkState.Valid)
        {
            return AdminEmailVerificationResult.LinkInvalid(StatusOf(state));
        }

        if (!await AccountLocks.LockAsync(_db, AccountType.Admin, adminId, ct))
        {
            return AdminEmailVerificationResult.LinkInvalid(AccountLinkStatus.Missing);
        }

        // Under the account's row lock the link and the pending change are read again: a request or a resend that ran between the first read and the lock has already replaced them (07 9.25.3).
        LinkReading locked = await _links.ReadAsync(key, ct);
        if (locked.State is not { } lockedState || lockedState != LinkState.Valid)
        {
            return AdminEmailVerificationResult.LinkInvalid(locked.State is { } ended ? StatusOf(ended) : AccountLinkStatus.Missing);
        }

        DateTimeOffset now = _time.GetUtcNow();
        AdminPendingEmailChange? pending = await _pending.FindAsync(adminId, ct);
        if (pending is null || IsEnded(pending.CreatedAt, now))
        {
            return AdminEmailVerificationResult.LinkInvalid(AccountLinkStatus.Expired);
        }

        var account = await _db.Admins.AsNoTracking().Where(a => a.Id == adminId).Select(a => new { a.Name, a.Email, a.NormalizedEmail }).SingleAsync(ct);
        if (await _db.Admins.AsNoTracking().AnyAsync(a => a.NormalizedEmail == pending.NormalizedNewEmail && a.Id != adminId, ct))
        {
            return AdminEmailVerificationResult.Taken();
        }

        // The link is consumed before anything is written: when the one conditional update takes no row nothing has been written (07 9.25.3).
        if (!await _links.TryConsumeAsync(record.Id, ct))
        {
            LinkReading again = await _links.ReadAsync(key, ct);
            return AdminEmailVerificationResult.LinkInvalid(again.State is { } current ? StatusOf(current) : AccountLinkStatus.Missing);
        }

        try
        {
            string newEmail = pending.NewEmail;
            string normalizedNew = pending.NormalizedNewEmail;
            string oldNormalized = account.NormalizedEmail;
            int changed = await _db.Admins
                .Where(a => a.Id == adminId && a.NormalizedEmail == oldNormalized)
                .ExecuteUpdateAsync(set => set.SetProperty(a => a.Email, newEmail).SetProperty(a => a.NormalizedEmail, normalizedNew), ct);
            if (changed != 1)
            {
                // Unreachable under the row lock; the exception rolls the consumption back with the transaction.
                throw new InvalidOperationException("The administrator's address changed under the account's row lock.");
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // The refusal rolls the transaction back, the consumption with it.
            return AdminEmailVerificationResult.Taken();
        }

        await _pending.DeleteAsync(adminId, ct);

        // The old address gets the notice and the revert link; the link holds the old address as entered (06 4.42). The revert's own button is T520's.
        DateTimeOffset revertEnds = now.AddDays(ProductConstants.EmailChangeRevertLinkDays);
        LinkRecord revert = LinkRecord.ForAdmin(_keys, LinkType.EmailChangeRevert, adminId, revertEnds, now);
        revert.PreviousEmail = account.Email;
        _links.Add(revert);
        await _notifications.QueueAccountEmailAsync(new AddressChangeNoticeEmail(DispatchRelatedRecordType.Admin, adminId, revert.Id, account.Email, now), ct);

        await _trail.WriteForAsync(
            new AuditActor(adminId, account.Name, pending.NewEmail),
            new AuditEntry(AuditOperation.ChangeAdminEmail, nameof(Admin), adminId, new AuditDetails(account.Name, changes: [new AuditValueChange("Email", account.Email, pending.NewEmail)])));
        return AdminEmailVerificationResult.Verified(adminId, revert.Id, revertEnds);
    }

    /// <summary>What the new address's landing page shows (07 UC-108): reads only. A valid link whose pending change has ended or is gone reads as expired - the same view the button takes.</summary>
    public async Task<AdminLinkReading> ReadNewEmailLinkAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.NewEmailVerification, AdminId: Guid adminId } || reading.State is not { } state)
        {
            return new AdminLinkReading(AccountLinkStatus.Missing);
        }

        if (state != LinkState.Valid)
        {
            return new AdminLinkReading(StatusOf(state));
        }

        AdminPendingEmailChange? pending = await _pending.FindAsync(adminId, ct);
        return new AdminLinkReading(pending is null || IsEnded(pending.CreatedAt, _time.GetUtcNow()) ? AccountLinkStatus.Expired : AccountLinkStatus.Valid);
    }

    /// <summary>What the revert link's landing page shows (07 UC-108): reads only.</summary>
    public async Task<AdminLinkReading> ReadRevertLinkAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        LinkReading reading = await _links.ReadAsync(key, ct);
        return reading.Record is not { LinkType: LinkType.EmailChangeRevert, AdminId: not null } || reading.State is not { } state
            ? new AdminLinkReading(AccountLinkStatus.Missing)
            : new AdminLinkReading(StatusOf(state));
    }

    /// <summary>
    /// The revert link's button (07 UC-187; 02 10.2.4; K-1210, K-1218): under the account's row lock the earlier address must not be another administrator's (422, nothing consumed); the link is
    /// consumed with one conditional update; the address goes back, every session of the account ends - the open one too -, the recognized browsers' marks end, the password becomes unusable, the
    /// earlier address gets the panel's password reset link, and the trail row is written for the administrator whose address went back (the link opens no session).
    /// </summary>
    public async Task<AdminEmailRevertResult> RestorePreviousEmailAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        RequireTransaction("The revert is written only inside a transaction (09 6 item 3).");

        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.EmailChangeRevert, AdminId: Guid adminId, PreviousEmail: { } previousEmail } record || reading.State is not { } state)
        {
            return AdminEmailRevertResult.LinkInvalid(AccountLinkStatus.Missing);
        }

        if (state != LinkState.Valid)
        {
            return AdminEmailRevertResult.LinkInvalid(StatusOf(state));
        }

        if (!await AccountLocks.LockAsync(_db, AccountType.Admin, adminId, ct))
        {
            return AdminEmailRevertResult.LinkInvalid(AccountLinkStatus.Missing);
        }

        var account = await _db.Admins.AsNoTracking().Where(a => a.Id == adminId).Select(a => new { a.Name, a.Email, a.NormalizedEmail }).SingleAsync(ct);
        string normalizedPrevious = TextNormalizer.NormalizeIdentity(previousEmail);
        if (await _db.Admins.AsNoTracking().AnyAsync(a => a.NormalizedEmail == normalizedPrevious && a.Id != adminId, ct))
        {
            return AdminEmailRevertResult.Taken();
        }

        if (!await _links.TryConsumeAsync(record.Id, ct))
        {
            LinkReading again = await _links.ReadAsync(key, ct);
            return AdminEmailRevertResult.LinkInvalid(again.State is { } current ? StatusOf(current) : AccountLinkStatus.Missing);
        }

        try
        {
            string currentNormalized = account.NormalizedEmail;
            int changed = await _db.Admins
                .Where(a => a.Id == adminId && a.NormalizedEmail == currentNormalized)
                .ExecuteUpdateAsync(set => set.SetProperty(a => a.Email, previousEmail).SetProperty(a => a.NormalizedEmail, normalizedPrevious), ct);
            if (changed != 1)
            {
                return AdminEmailRevertResult.LinkInvalid(AccountLinkStatus.Superseded);
            }
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return AdminEmailRevertResult.Taken();
        }

        await _pending.DeleteAsync(adminId, ct);
        await _sessions.EndAllAsync(AccountType.Admin, adminId, null, ct);
        await _sessions.InvalidateMarksAsync(AccountType.Admin, adminId, null, ct);
        await _passwords.InvalidatePasswordAsync(adminId, ct);

        DateTimeOffset now = _time.GetUtcNow();
        LinkRecord reset = LinkRecord.ForAdmin(_keys, LinkType.PasswordReset, adminId, now.AddMinutes(ProductConstants.PasswordResetLinkMinutes), now);
        _links.Add(reset);
        await _links.SupersedeEarlierAsync(LinkType.PasswordReset, null, adminId, reset.Id, ct);
        await _notifications.QueueAccountEmailAsync(new PasswordResetLinkEmail(DispatchRelatedRecordType.Admin, adminId, reset.Id, previousEmail, true), ct);

        await _trail.WriteForAsync(
            new AuditActor(adminId, account.Name, previousEmail),
            new AuditEntry(AuditOperation.ChangeAdminEmail, nameof(Admin), adminId, new AuditDetails(account.Name, changes: [new AuditValueChange("Email", account.Email, previousEmail)])));
        return AdminEmailRevertResult.Reverted(adminId, reset.Id);
    }

    // The life of a pending change is the first link's: P-12 hours from the request (the nightly deletion reads the same rule).
    private static bool IsEnded(DateTimeOffset createdAt, DateTimeOffset now) => createdAt <= now - TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours);

    private async Task<DateTimeOffset?> OpenRevertEndAsync(Guid adminId, DateTimeOffset now, CancellationToken ct)
    {
        return await _db.LinkRecords.AsNoTracking()
            .Where(l => l.AdminId == adminId && l.LinkType == LinkType.EmailChangeRevert && l.UsedAt == null && l.SupersededAt == null && l.ExpiresAt > now)
            .OrderByDescending(l => l.ExpiresAt)
            .Select(l => (DateTimeOffset?)l.ExpiresAt)
            .FirstOrDefaultAsync(ct);
    }

    private async Task<bool> CountedOverLimitAsync(string normalizedEmail, IPAddress? ipAddress, CancellationToken ct)
    {
        AttemptSubject byEmail = AttemptSubject.Email(normalizedEmail);
        AttemptSubject byIp = AttemptSubject.Ip(ipAddress);
        await _attempts.RecordAsync(AttemptLimit.L9, byEmail, ct);
        await _attempts.RecordAsync(AttemptLimit.L9, byIp, ct);
        return await _attempts.IsExceededAsync(AttemptLimit.L9, byEmail, ct) || await _attempts.IsExceededAsync(AttemptLimit.L9, byIp, ct);
    }

    private static AccountLinkStatus StatusOf(LinkState state) => AdminAccounts.StatusOf(state);

    private void RequireTransaction(string message)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(message);
        }
    }
}
