using System.ComponentModel.DataAnnotations;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Administration.Contracts.Results;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The administrator accounts' own rules (T379; 02 10.2): the invitation that opens every account but the first, its withdrawal, the opening of the account from the invitation's
/// link, the removal of another administrator and the administrator's name. There is no self-registration, no role and no permission field (02 10.2.1, 10.2.3). Everything joins the
/// caller's transaction and never saves it (09 6 item 3). Lock order: the sender's account row for a send (05 6.1.2; K-1084); the administrator set's one advisory lock, alone, for the
/// opening of an account and for a removal (05 7.3.5); a link's row before its invitation's row everywhere, so a withdrawal and an acceptance cannot wait on each other.
/// </summary>
internal sealed class AdminAccounts
{
    // The one home of the e-mail rule (04 7.3.2.1; 09 3.4 item 1), the customer registration's rule too.
    private static readonly InvalidEmailAttribute EmailRule = new();

    private readonly AppDbContext _db;
    private readonly LinkRecords _links;
    private readonly LinkKeys _keys;
    private readonly INotificationsModule _notifications;
    private readonly AuditTrail _trail;
    private readonly IAuditActorSource _actors;
    private readonly PasswordService _hashing;
    private readonly AdvisoryLocks _locks;
    private readonly TimeProvider _time;

    public AdminAccounts(
        AppDbContext db,
        LinkRecords links,
        LinkKeys keys,
        INotificationsModule notifications,
        AuditTrail trail,
        IAuditActorSource actors,
        PasswordService hashing,
        AdvisoryLocks locks,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(links);
        ArgumentNullException.ThrowIfNull(keys);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(trail);
        ArgumentNullException.ThrowIfNull(actors);
        ArgumentNullException.ThrowIfNull(hashing);
        ArgumentNullException.ThrowIfNull(locks);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _links = links;
        _keys = keys;
        _notifications = notifications;
        _trail = trail;
        _actors = actors;
        _hashing = hashing;
        _locks = locks;
        _time = time;
    }

    /// <summary>
    /// Sends an invitation (07 UC-175; 02 10.2.1, 10.2.6): the sender's row is locked first, so a send and the sender's removal queue; the address must be a valid one that is no
    /// administrator account's (a customer account's address is no obstacle, 02 10.2.5); the unused invitation of the same address is made invalid with its link in the same
    /// transaction (Z-3; K-661); the invitation, its link of P-14 (72 hours), the e-mail, the trail row and the F-6 notice to every administrator are written. L-9 does not count it.
    /// </summary>
    public async Task<AdminInvitationSendResult> SendAsync(string email, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        RequireTransaction("The invitation is written only inside a transaction (09 6 item 3).");

        AuditActor? actor = await _actors.CurrentAsync(ct);
        if (actor is null || !await AccountLocks.LockAsync(_db, AccountType.Admin, actor.AdminId, ct))
        {
            return AdminInvitationSendResult.NotAdministrator();
        }

        string entered = email.Trim();
        if (entered.Length == 0)
        {
            return AdminInvitationSendResult.Refused(ErrorCode.FieldRequired);
        }

        if (EmailRule.GetValidationResult(entered, new ValidationContext(entered)) is not null)
        {
            return AdminInvitationSendResult.Refused(ErrorCode.InvalidEmail);
        }

        string normalized = TextNormalizer.NormalizeIdentity(entered);
        if (await _db.Admins.AsNoTracking().AnyAsync(a => a.NormalizedEmail == normalized, ct))
        {
            return AdminInvitationSendResult.Refused(ErrorCode.AdminEmailTaken);
        }

        DateTimeOffset now = _time.GetUtcNow();

        // The address has one valid invitation: the earlier one - valid or run out, it still holds the address - is made invalid with its link, the link's row first (Z-3).
        List<Guid> earlier = await OpenInvitationIds(i => i.NormalizedEmail == normalized).ToListAsync(ct);
        await EndLinksAsync(earlier, now, ct);
        await _db.AdminInvitations.Where(i => earlier.Contains(i.Id)).ExecuteUpdateAsync(set => set.SetProperty(i => i.InvalidatedAt, now), ct);

        AdminInvitation invitation = AdminInvitation.Create(entered, actor.AdminId, actor.Name, now);
        DateTimeOffset expiresAt = now.AddHours(ProductConstants.AdminInvitationLinkHours);
        LinkRecord link = LinkRecord.ForInvitation(_keys, invitation.Id, expiresAt, now);
        _db.AdminInvitations.Add(invitation);
        _links.Add(link);

        await _notifications.QueueAccountEmailAsync(new AdminInvitationEmail(invitation.Id, link.Id, entered, actor.Name), ct);
        List<string> addresses = await _db.Admins.AsNoTracking().Select(a => a.Email).ToListAsync(ct);
        await _notifications.QueueAdminNoticeAsync(new AdminInvitedNotice(invitation.Id, actor.Name, entered, now), addresses, ct);
        await _trail.WriteForAsync(actor, new AuditEntry(AuditOperation.SendAdminInvitation, nameof(AdminInvitation), invitation.Id, new AuditDetails(entered)));
        return AdminInvitationSendResult.Sent(invitation.Id, link.Id, expiresAt, earlier.Count);
    }

    /// <summary>
    /// Withdraws an unused invitation (07 UC-176; 02 10.2.6): the link's life ends at this moment, the withdrawal moment is written, the trail row is written and nobody is notified.
    /// An invitation that is not valid any more - used, made invalid, withdrawn, run out - writes nothing and says so.
    /// </summary>
    public async Task<AdminInvitationWithdrawalResult> WithdrawAsync(Guid invitationId, CancellationToken ct)
    {
        RequireTransaction("The withdrawal is written only inside a transaction (09 6 item 3).");
        AuditActor? actor = await _actors.CurrentAsync(ct);
        if (actor is null)
        {
            return AdminInvitationWithdrawalResult.Of(AdminInvitationWithdrawalOutcome.ActorNotAdministrator);
        }

        var invitation = await _db.AdminInvitations.AsNoTracking().Where(i => i.Id == invitationId).Select(i => new { i.Email }).SingleOrDefaultAsync(ct);
        if (invitation is null)
        {
            return AdminInvitationWithdrawalResult.Of(AdminInvitationWithdrawalOutcome.NotFound);
        }

        DateTimeOffset now = _time.GetUtcNow();

        // The link first (a valid one is the proof that the invitation is unused and has not run out), then the invitation's row, under its own three-moment condition.
        int ended = await _db.LinkRecords
            .Where(l => l.AdminInvitationId == invitationId && l.UsedAt == null && l.SupersededAt == null && l.ExpiresAt > now)
            .ExecuteUpdateAsync(set => set.SetProperty(l => l.ExpiresAt, now), ct);
        int withdrawn = ended == 0
            ? 0
            : await _db.AdminInvitations
                .Where(i => i.Id == invitationId && i.WithdrawnAt == null && i.InvalidatedAt == null && i.AcceptedAt == null)
                .ExecuteUpdateAsync(set => set.SetProperty(i => i.WithdrawnAt, now), ct);
        if (withdrawn != 1)
        {
            return AdminInvitationWithdrawalResult.Of(AdminInvitationWithdrawalOutcome.NoLongerActive);
        }

        await _trail.WriteForAsync(actor, new AuditEntry(AuditOperation.WithdrawAdminInvitation, nameof(AdminInvitation), invitationId, new AuditDetails(invitation.Email)));
        return AdminInvitationWithdrawalResult.Of(AdminInvitationWithdrawalOutcome.Withdrawn);
    }

    /// <summary>What the invitation page shows when the link opens (07 UC-112): only reads. The address is given only while the link is valid and the invitation is unused.</summary>
    public async Task<AdminInvitationReading> ReadAsync(string key, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.AdminInvitation, AdminInvitationId: Guid invitationId } || reading.State is not { } state)
        {
            return new AdminInvitationReading(AccountLinkStatus.Missing, null);
        }

        var invitation = await _db.AdminInvitations.AsNoTracking()
            .Where(i => i.Id == invitationId)
            .Select(i => new { i.Email, i.NormalizedEmail, i.WithdrawnAt, i.InvalidatedAt, i.AcceptedAt })
            .SingleOrDefaultAsync(ct);
        if (invitation is null)
        {
            return new AdminInvitationReading(AccountLinkStatus.Missing, null);
        }

        if (state != LinkState.Valid)
        {
            return new AdminInvitationReading(StatusOf(state), null);
        }

        if (invitation.WithdrawnAt is not null || invitation.InvalidatedAt is not null || invitation.AcceptedAt is not null)
        {
            return new AdminInvitationReading(invitation.AcceptedAt is not null ? AccountLinkStatus.Used : AccountLinkStatus.Expired, null);
        }

        // The address became an administrator's after the invitation: the page shows the run-out message already (K-1085).
        bool taken = await _db.Admins.AsNoTracking().AnyAsync(a => a.NormalizedEmail == invitation.NormalizedEmail, ct);
        return taken ? new AdminInvitationReading(AccountLinkStatus.Expired, null) : new AdminInvitationReading(AccountLinkStatus.Valid, invitation.Email);
    }

    /// <summary>
    /// The invitee's button (07 UC-113; 02 10.2.1): the name is required and the password meets the one policy before anything is consumed; the link is consumed with one conditional
    /// update; the account is born with the invitation's address, the name and the password digest, and no role; the invitation is marked accepted; the trail row names the new
    /// administrator. When the address has become an administrator's meanwhile, no account is opened and the invitation is made invalid (K-1085). No session is opened.
    /// </summary>
    public async Task<AdminInvitationAcceptance> AcceptInvitationAsync(string key, string name, string password, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(key);
        ArgumentNullException.ThrowIfNull(name);
        ArgumentNullException.ThrowIfNull(password);
        RequireTransaction("The account is opened only inside a transaction (09 6 item 3).");

        // The administrator set's lock is alone and first (05 7.3.5): two accounts for one address are opened one after the other.
        await _locks.LockAsync(AdvisoryLockScope.AdminSet, null, ct);

        LinkReading reading = await _links.ReadAsync(key, ct);
        if (reading.Record is not { LinkType: LinkType.AdminInvitation, AdminInvitationId: Guid invitationId } record || reading.State is not { } state)
        {
            return AdminInvitationAcceptance.LinkInvalid(AccountLinkStatus.Missing);
        }

        if (state != LinkState.Valid)
        {
            return AdminInvitationAcceptance.LinkInvalid(StatusOf(state));
        }

        var invitation = await _db.AdminInvitations.AsNoTracking()
            .Where(i => i.Id == invitationId)
            .Select(i => new { i.Email, i.NormalizedEmail, i.WithdrawnAt, i.InvalidatedAt, i.AcceptedAt })
            .SingleOrDefaultAsync(ct);
        if (invitation is null)
        {
            return AdminInvitationAcceptance.LinkInvalid(AccountLinkStatus.Missing);
        }

        if (invitation.WithdrawnAt is not null || invitation.InvalidatedAt is not null || invitation.AcceptedAt is not null)
        {
            return AdminInvitationAcceptance.LinkInvalid(invitation.AcceptedAt is not null ? AccountLinkStatus.Used : AccountLinkStatus.Expired);
        }

        DateTimeOffset now = _time.GetUtcNow();

        // The address became an administrator's after the invitation went out: no account, and the invitation is invalid from now on (K-1085). The write commits.
        if (await _db.Admins.AsNoTracking().AnyAsync(a => a.NormalizedEmail == invitation.NormalizedEmail, ct))
        {
            await EndLinksAsync([invitationId], now, ct);
            await _db.AdminInvitations.Where(i => i.Id == invitationId).ExecuteUpdateAsync(set => set.SetProperty(i => i.InvalidatedAt, now), ct);
            return AdminInvitationAcceptance.LinkInvalid(AccountLinkStatus.Expired);
        }

        string cleanName = name.Trim();
        if (cleanName.Length == 0)
        {
            return AdminInvitationAcceptance.FieldRefused(AdminAccountFields.Name, ErrorCode.FieldRequired);
        }

        if (PasswordPolicy.Validate(password) is { } passwordRefusal)
        {
            return AdminInvitationAcceptance.FieldRefused(AdminAccountFields.Password, passwordRefusal);
        }

        if (!await _links.TryConsumeAsync(record.Id, ct))
        {
            LinkReading again = await _links.ReadAsync(key, ct);
            return AdminInvitationAcceptance.LinkInvalid(again.State is { } current ? StatusOf(current) : AccountLinkStatus.Missing);
        }

        int accepted = await _db.AdminInvitations
            .Where(i => i.Id == invitationId && i.WithdrawnAt == null && i.InvalidatedAt == null && i.AcceptedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(i => i.AcceptedAt, now), ct);
        if (accepted != 1)
        {
            // The invitation was made invalid between the read and the write: the consumption goes back with the caller's rollback.
            return AdminInvitationAcceptance.LinkInvalid(AccountLinkStatus.Expired, rollsBack: true);
        }

        // The account is the whole row: a name, the invitation's address as it was entered, a digest; it has no role and no verified state (06 4.37).
        Admin admin = Admin.Create(cleanName, invitation.Email, _hashing.Hash(password), now);
        _db.Admins.Add(admin);
        await _trail.WriteForAsync(
            new AuditActor(admin.Id, admin.Name, admin.Email),
            new AuditEntry(AuditOperation.CreateAdminAccount, nameof(Admin), admin.Id, new AuditDetails(admin.Name)));
        return AdminInvitationAcceptance.Created(admin.Id, invitation.Email);
    }

    /// <summary>
    /// Removes another administrator (07 UC-177; 02 10.2.2). The administrator set's lock comes first and alone; under it the acting administrator is checked to be one still,
    /// then the target's existence, then the self and last-administrator refusals. Without the confirmation nothing is written and the answer carries what the window says (the
    /// name, the address and the number of unused invitations). With it the account's row is locked by a no-op update (so a send in flight ends first), the unused invitations the
    /// target sent are made invalid with their links, the row is deleted (sessions, marks, links and the pending change go with it by the foreign keys, so the open sessions
    /// end at once), the disposal record, the trail row and the F-6 notice to every administrator - the removed one included - are written.
    /// </summary>
    public async Task<AdminRemovalResult> RemoveAsync(Guid targetAdminId, bool confirmed, CancellationToken ct)
    {
        RequireTransaction("The removal is written only inside a transaction (09 6 item 3).");
        await _locks.LockAsync(AdvisoryLockScope.AdminSet, null, ct);

        AuditActor? actor = await _actors.CurrentAsync(ct);
        if (actor is null)
        {
            return AdminRemovalResult.Of(AdminRemovalOutcome.ActorNotAdministrator);
        }

        var target = await _db.Admins.AsNoTracking().Where(a => a.Id == targetAdminId).Select(a => new { a.Id, a.Name, a.Email }).SingleOrDefaultAsync(ct);
        if (target is null)
        {
            return AdminRemovalResult.Of(AdminRemovalOutcome.NotFound);
        }

        if (target.Id == actor.AdminId || await _db.Admins.AsNoTracking().CountAsync(ct) <= 1)
        {
            return AdminRemovalResult.Of(AdminRemovalOutcome.Refused);
        }

        DateTimeOffset now = _time.GetUtcNow();
        if (!confirmed)
        {
            int unused = await _db.AdminInvitations.AsNoTracking().CountAsync(ValidInvitationOf(targetAdminId, now), ct);
            return AdminRemovalResult.Confirmation(target.Name, target.Email, unused);
        }

        // The row lock without a change: a send by the target that holds the row finishes first and its invitation is seen by the statements below.
        if (await _db.Admins.Where(a => a.Id == targetAdminId).ExecuteUpdateAsync(set => set.SetProperty(a => a.Name, a => a.Name), ct) != 1)
        {
            return AdminRemovalResult.Of(AdminRemovalOutcome.NotFound);
        }

        int validInvitations = await _db.AdminInvitations.AsNoTracking().CountAsync(ValidInvitationOf(targetAdminId, now), ct);
        List<Guid> open = await OpenInvitationIds(i => i.SenderAdminId == targetAdminId).ToListAsync(ct);
        await EndLinksAsync(open, now, ct);
        await _db.AdminInvitations.Where(i => open.Contains(i.Id)).ExecuteUpdateAsync(set => set.SetProperty(i => i.InvalidatedAt, now), ct);

        int sessions = await _db.Sessions.AsNoTracking().CountAsync(s => s.AdminId == targetAdminId, ct);
        List<string> addresses = await _db.Admins.AsNoTracking().Select(a => a.Email).ToListAsync(ct);
        await _db.Admins.Where(a => a.Id == targetAdminId).ExecuteDeleteAsync(ct);
        _db.DisposalRecords.Add(DisposalRecord.Counted(DisposalRecordType.Admin, 1, now));

        await _trail.WriteForAsync(
            actor,
            new AuditEntry(
                AuditOperation.RemoveAdminAccount,
                nameof(Admin),
                targetAdminId,
                new AuditDetails(target.Name, values: [new AuditValue("Email", target.Email), new AuditValue("InvalidatedInvitations", validInvitations.ToString(System.Globalization.CultureInfo.InvariantCulture))])));
        await _notifications.QueueAdminNoticeAsync(new AdminRemovedNotice(targetAdminId, actor.Name, target.Name, now), addresses, ct);
        return AdminRemovalResult.Removed(target.Name, target.Email, validInvitations, sessions);
    }

    /// <summary>The administrator's name from their own account (07 UC-178; 02 10.2.4): empty is refused (K-736); no reauthentication; no trail row (the name is not one of the trail's operations).</summary>
    public async Task<AdminNameChangeResult> ChangeNameAsync(Guid adminId, string name, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(name);
        RequireTransaction("The name is written only inside a transaction (09 6 item 3).");
        string clean = name.Trim();
        if (clean.Length == 0)
        {
            return AdminNameChangeResult.Refused(ErrorCode.FieldRequired);
        }

        if (!await AccountLocks.LockAsync(_db, AccountType.Admin, adminId, ct))
        {
            return AdminNameChangeResult.Refused(ErrorCode.NotFound);
        }

        await _db.Admins.Where(a => a.Id == adminId).ExecuteUpdateAsync(set => set.SetProperty(a => a.Name, clean), ct);
        return AdminNameChangeResult.Done();
    }

    // The invitations that no moment has closed: unused, not withdrawn, not made invalid (their link may have run out by time).
    private IQueryable<Guid> OpenInvitationIds(System.Linq.Expressions.Expression<Func<AdminInvitation, bool>> filter)
    {
        return _db.AdminInvitations.Where(filter).Where(i => i.WithdrawnAt == null && i.InvalidatedAt == null && i.AcceptedAt == null).Select(i => i.Id);
    }

    private System.Linq.Expressions.Expression<Func<AdminInvitation, bool>> ValidInvitationOf(Guid senderId, DateTimeOffset now)
    {
        // Sent by the administrator, closed by no moment, and the link still lives: this is what the confirmation counts (02 10.2.2).
        return i => i.SenderAdminId == senderId && i.WithdrawnAt == null && i.InvalidatedAt == null && i.AcceptedAt == null
            && _db.LinkRecords.Any(l => l.AdminInvitationId == i.Id && l.UsedAt == null && l.SupersededAt == null && l.ExpiresAt > now);
    }

    // A link's life ends now (the invitation's moment is written after, in the same transaction): only links that are unused and still alive are taken.
    private async Task EndLinksAsync(List<Guid> invitationIds, DateTimeOffset now, CancellationToken ct)
    {
        if (invitationIds.Count == 0)
        {
            return;
        }

        await _db.LinkRecords
            .Where(l => l.AdminInvitationId != null && invitationIds.Contains(l.AdminInvitationId.Value) && l.UsedAt == null && l.ExpiresAt > now)
            .ExecuteUpdateAsync(set => set.SetProperty(l => l.ExpiresAt, now), ct);
    }

    internal static AccountLinkStatus StatusOf(LinkState state) => state switch
    {
        LinkState.Valid => AccountLinkStatus.Valid,
        LinkState.Used => AccountLinkStatus.Used,
        LinkState.Superseded => AccountLinkStatus.Superseded,
        _ => AccountLinkStatus.Expired,
    };

    private void RequireTransaction(string message)
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException(message);
        }
    }
}
