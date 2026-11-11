using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Modules.Administration.Contracts.Results;

/// <summary>The field names of the administrator account forms (07 7.2.23, 7.2.24; 04 7.2.2): plain constants, nothing is persisted.</summary>
public static class AdminAccountFields
{
    public const string Email = "Email";
    public const string Name = "Name";
    public const string Password = "Password";
    public const string NewEmail = "NewEmail";
}

/// <summary>
/// The outcome of sending an invitation (07 UC-175). A refusal names the field and the code and writes nothing. The invitation of the same address that was still unused
/// is invalid from the same transaction on (Z-3; K-661).
/// </summary>
public sealed class AdminInvitationSendResult : IOperationResult
{
    private AdminInvitationSendResult(AdminInvitationSendOutcome outcome, Guid? invitationId, Guid? linkId, DateTimeOffset? expiresAt, string? field, ErrorCode? code, int invalidated)
    {
        Outcome = outcome;
        InvitationId = invitationId;
        LinkId = linkId;
        ExpiresAt = expiresAt;
        Field = field;
        Code = code;
        InvalidatedEarlierInvitations = invalidated;
    }

    public AdminInvitationSendOutcome Outcome { get; }

    public Guid? InvitationId { get; }

    /// <summary>The link record of the invitation; the e-mail body derives the key from it.</summary>
    public Guid? LinkId { get; }

    public DateTimeOffset? ExpiresAt { get; }

    public string? Field { get; }

    public ErrorCode? Code { get; }

    /// <summary>The number of earlier unused invitations of the same address this one made invalid.</summary>
    public int InvalidatedEarlierInvitations { get; }

    public bool IsSent => Outcome == AdminInvitationSendOutcome.Sent;

    public bool IsRejected => !IsSent;

    internal static AdminInvitationSendResult Sent(Guid invitationId, Guid linkId, DateTimeOffset expiresAt, int invalidated) =>
        new(AdminInvitationSendOutcome.Sent, invitationId, linkId, expiresAt, null, null, invalidated);

    internal static AdminInvitationSendResult Refused(ErrorCode code) => new(AdminInvitationSendOutcome.Refused, null, null, null, AdminAccountFields.Email, code, 0);

    internal static AdminInvitationSendResult NotAdministrator() => new(AdminInvitationSendOutcome.ActorNotAdministrator, null, null, null, null, null, 0);
}

/// <summary>The outcome of withdrawing an unused invitation (07 UC-176; 02 10.2.6): the link is invalid from the moment and the invitee gets no notice.</summary>
public sealed class AdminInvitationWithdrawalResult : IOperationResult
{
    private AdminInvitationWithdrawalResult(AdminInvitationWithdrawalOutcome outcome)
    {
        Outcome = outcome;
    }

    public AdminInvitationWithdrawalOutcome Outcome { get; }

    public ErrorCode? Code => Outcome == AdminInvitationWithdrawalOutcome.NoLongerActive ? ErrorCode.InvitationNoLongerActive : null;

    public bool IsWithdrawn => Outcome == AdminInvitationWithdrawalOutcome.Withdrawn;

    public bool IsRejected => !IsWithdrawn;

    internal static AdminInvitationWithdrawalResult Of(AdminInvitationWithdrawalOutcome outcome) => new(outcome);
}

/// <summary>What the invitation page shows when its link opens (07 UC-112): the link's status and, for a valid link, the address the account will be opened for.</summary>
public sealed record AdminInvitationReading(AccountLinkStatus Status, string? Email);

/// <summary>
/// The outcome of opening an administrator account from an invitation (07 UC-113; 02 10.2.1, 10.2.6). Only a field refusal asks for the caller's rollback; an invalid link
/// commits what it wrote - the invitation made invalid when the address became an administrator's (K-1085).
/// </summary>
public sealed class AdminInvitationAcceptance : IOperationResult
{
    private AdminInvitationAcceptance(AdminInvitationAcceptanceOutcome outcome, Guid? adminId, string? email, string? field, ErrorCode? code, AccountLinkStatus? status, bool rollsBack)
    {
        Outcome = outcome;
        AdminId = adminId;
        Email = email;
        Field = field;
        Code = code;
        LinkStatus = status;
        RollsBack = rollsBack;
    }

    public AdminInvitationAcceptanceOutcome Outcome { get; }

    public Guid? AdminId { get; }

    /// <summary>The address of the new account (the sign-in form is filled with it); empty unless created.</summary>
    public string? Email { get; }

    public string? Field { get; }

    public ErrorCode? Code { get; }

    public AccountLinkStatus? LinkStatus { get; }

    internal bool RollsBack { get; }

    public bool IsCreated => Outcome == AdminInvitationAcceptanceOutcome.Created;

    public bool IsRejected => Outcome == AdminInvitationAcceptanceOutcome.FieldRefused || RollsBack;

    internal static AdminInvitationAcceptance Created(Guid adminId, string email) => new(AdminInvitationAcceptanceOutcome.Created, adminId, email, null, null, null, false);

    internal static AdminInvitationAcceptance FieldRefused(string field, ErrorCode code) => new(AdminInvitationAcceptanceOutcome.FieldRefused, null, null, field, code, null, true);

    internal static AdminInvitationAcceptance LinkInvalid(AccountLinkStatus status, bool rollsBack = false) => new(AdminInvitationAcceptanceOutcome.LinkInvalid, null, null, null, null, status, rollsBack);
}

/// <summary>
/// The outcome of removing an administrator (07 UC-177; 02 10.2.2). The confirmation state carries what the window says: the name and address of the one removed and the number
/// of unused invitations that die with the account; the removed state carries the numbers of what ended with it.
/// </summary>
public sealed class AdminRemovalResult : IOperationResult
{
    private AdminRemovalResult(AdminRemovalOutcome outcome, string? name, string? email, int invitations, int sessions)
    {
        Outcome = outcome;
        RemovedName = name;
        RemovedEmail = email;
        InvalidatedInvitations = invitations;
        EndedSessions = sessions;
    }

    public AdminRemovalOutcome Outcome { get; }

    public string? RemovedName { get; }

    public string? RemovedEmail { get; }

    /// <summary>The unused invitations the removed administrator sent: what the confirmation says, then what became invalid.</summary>
    public int InvalidatedInvitations { get; }

    /// <summary>The open sessions that ended at once; filled on a removal.</summary>
    public int EndedSessions { get; }

    public ErrorCode? Code => Outcome == AdminRemovalOutcome.Refused ? ErrorCode.LastAdminRemoval : Outcome == AdminRemovalOutcome.NotFound ? ErrorCode.NotFound : null;

    public bool IsRemoved => Outcome == AdminRemovalOutcome.Removed;

    public bool IsRejected => Outcome is AdminRemovalOutcome.Refused or AdminRemovalOutcome.NotFound or AdminRemovalOutcome.ActorNotAdministrator;

    internal static AdminRemovalResult Removed(string name, string email, int invitations, int sessions) => new(AdminRemovalOutcome.Removed, name, email, invitations, sessions);

    internal static AdminRemovalResult Confirmation(string name, string email, int invitations) => new(AdminRemovalOutcome.ConfirmationNeeded, name, email, invitations, 0);

    internal static AdminRemovalResult Of(AdminRemovalOutcome outcome) => new(outcome, null, null, 0, 0);
}

/// <summary>The outcome of changing the administrator's own name (07 UC-178): done, or refused on the field (K-736).</summary>
public sealed class AdminNameChangeResult : IOperationResult
{
    private AdminNameChangeResult(bool done, string? field, ErrorCode? code)
    {
        IsDone = done;
        Field = field;
        Code = code;
    }

    public bool IsDone { get; }

    public string? Field { get; }

    public ErrorCode? Code { get; }

    public bool IsRejected => !IsDone;

    internal static AdminNameChangeResult Done() => new(true, null, null);

    internal static AdminNameChangeResult Refused(ErrorCode code) => new(false, AdminAccountFields.Name, code);
}

/// <summary>The outcome of the administrator's address change request (07 UC-179; 02 10.2.4): the new address waits for verification, the old one keeps working.</summary>
public sealed class AdminEmailChangeRequestResult : IOperationResult
{
    private AdminEmailChangeRequestResult(AdminEmailChangeOutcome outcome, Guid? linkId, DateTimeOffset? expiresAt, bool replaced, ErrorCode? code, DateTimeOffset? revertEndsAt)
    {
        Outcome = outcome;
        LinkId = linkId;
        ExpiresAt = expiresAt;
        ReplacedPending = replaced;
        Code = code;
        RevertEndsAt = revertEndsAt;
    }

    public AdminEmailChangeOutcome Outcome { get; }

    public Guid? LinkId { get; }

    public DateTimeOffset? ExpiresAt { get; }

    /// <summary>True when the request took the place of an earlier pending change.</summary>
    public bool ReplacedPending { get; }

    public ErrorCode? Code { get; }

    public DateTimeOffset? RevertEndsAt { get; }

    public bool IsRequested => Outcome == AdminEmailChangeOutcome.Requested;

    public bool IsRejected => !IsRequested;

    internal static AdminEmailChangeRequestResult Requested(Guid linkId, DateTimeOffset expiresAt, bool replaced) => new(AdminEmailChangeOutcome.Requested, linkId, expiresAt, replaced, null, null);

    internal static AdminEmailChangeRequestResult Refused(AdminEmailChangeOutcome outcome, ErrorCode code) => new(outcome, null, null, false, code, null);

    internal static AdminEmailChangeRequestResult Reverting(DateTimeOffset endsAt) => new(AdminEmailChangeOutcome.RevertPending, null, null, false, ErrorCode.EmailChangeRevertPending, endsAt);
}

/// <summary>The outcome of asking the pending change's link again (07 UC-180).</summary>
public sealed class AdminEmailChangeResendResult : IOperationResult
{
    private AdminEmailChangeResendResult(Guid? linkId, DateTimeOffset? expiresAt, bool limited)
    {
        LinkId = linkId;
        ExpiresAt = expiresAt;
        IsLimited = limited;
    }

    public Guid? LinkId { get; }

    public DateTimeOffset? ExpiresAt { get; }

    /// <summary>L-9 is over its limit (429): the earlier link stays valid.</summary>
    public bool IsLimited { get; }

    public bool IsIssued => LinkId is not null;

    public bool IsRejected => IsLimited;

    internal static AdminEmailChangeResendResult Issued(Guid linkId, DateTimeOffset expiresAt) => new(linkId, expiresAt, false);

    internal static AdminEmailChangeResendResult Limited() => new(null, null, true);

    internal static AdminEmailChangeResendResult Nothing() => new(null, null, false);
}

/// <summary>The outcome of the new address's button (07 UC-186): the address changed, or the link was not valid, or the address became another administrator's.</summary>
public sealed class AdminEmailVerificationResult : IOperationResult
{
    private AdminEmailVerificationResult(Guid? adminId, Guid? revertLinkId, DateTimeOffset? revertEndsAt, AccountLinkStatus? status, bool taken)
    {
        AdminId = adminId;
        RevertLinkId = revertLinkId;
        RevertEndsAt = revertEndsAt;
        LinkStatus = status;
        IsTaken = taken;
    }

    public Guid? AdminId { get; }

    public Guid? RevertLinkId { get; }

    public DateTimeOffset? RevertEndsAt { get; }

    public AccountLinkStatus? LinkStatus { get; }

    /// <summary>The new address belongs to another administrator account (422): nothing is consumed.</summary>
    public bool IsTaken { get; }

    public bool IsVerified => AdminId is not null;

    /// <summary>Only the taken address asks for the caller's rollback; an invalid link wrote nothing.</summary>
    public bool IsRejected => IsTaken;

    internal static AdminEmailVerificationResult Verified(Guid adminId, Guid revertLinkId, DateTimeOffset revertEndsAt) => new(adminId, revertLinkId, revertEndsAt, null, false);

    internal static AdminEmailVerificationResult LinkInvalid(AccountLinkStatus status) => new(null, null, null, status, false);

    internal static AdminEmailVerificationResult Taken() => new(null, null, null, null, true);
}

/// <summary>What the landing page of a new-address or revert link shows when it opens (07 UC-108; 05 6.4.6): only reads. The status is the link's state; a key of another type is missing.</summary>
public sealed record AdminLinkReading(AccountLinkStatus Status);

/// <summary>The outcome of the revert link's button (07 UC-187): the address went back, or the link was not valid, or the old address became another administrator's account (nothing is written).</summary>
public sealed class AdminEmailRevertResult : IOperationResult
{
    private AdminEmailRevertResult(Guid? adminId, Guid? resetLinkId, AccountLinkStatus? status, bool taken)
    {
        AdminId = adminId;
        ResetLinkId = resetLinkId;
        LinkStatus = status;
        IsTaken = taken;
    }

    public Guid? AdminId { get; }

    /// <summary>The password reset link sent to the earlier address.</summary>
    public Guid? ResetLinkId { get; }

    public AccountLinkStatus? LinkStatus { get; }

    /// <summary>The earlier address is another administrator account's (422): the revert is not applied and the link's consumption is rolled back.</summary>
    public bool IsTaken { get; }

    public bool IsReverted => AdminId is not null;

    public bool IsRejected => IsTaken;

    internal static AdminEmailRevertResult Reverted(Guid adminId, Guid resetLinkId) => new(adminId, resetLinkId, null, false);

    internal static AdminEmailRevertResult LinkInvalid(AccountLinkStatus status) => new(null, null, status, false);

    internal static AdminEmailRevertResult Taken() => new(null, null, null, true);
}
