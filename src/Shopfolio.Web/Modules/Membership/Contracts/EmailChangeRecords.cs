using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Membership.Contracts.Results;

namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>The reading of one of the three links of the landing page: its kind and status and the account it belongs to. Only reads (MK-51); a key of another type is <see cref="VerificationLinkStatus.Missing"/>.</summary>
public sealed record EmailLinkReading(EmailLinkKind? Kind, VerificationLinkStatus Status, Guid? CustomerId);

/// <summary>The account's pending e-mail change that still lives (T517; 04 5.25.4): the new address as entered and the end of its link, which the account screen shows as the last day.</summary>
public sealed record PendingEmailChangeReading(string NewEmail, DateTimeOffset LinkEndsAt);

/// <summary>
/// The outcome of the account's request for a new e-mail address (02 3.13.14; 07 UC-66): requested, with the id of the new address's link and the moment it ends, or refused with a
/// code - a field (<c>FieldRequired</c>, <c>InvalidEmail</c>), <c>EmailTakenByOtherAccount</c>, <c>EmailChangeRevertPending</c> or the limit <c>VerificationLinkLimitExceeded</c> (L-9,
/// counted only when the request replaces a pending one). A refusal rolls the caller's transaction back; the counters survive it.
/// </summary>
public sealed class EmailChangeRequestResult : IOperationResult
{
    private EmailChangeRequestResult(Guid? linkId, DateTimeOffset? expiresAt, bool replaced, ErrorCode? code, IReadOnlyList<MembershipFieldError> fields, DateTimeOffset? revertEndsAt)
    {
        LinkId = linkId;
        ExpiresAt = expiresAt;
        Replaced = replaced;
        Code = code;
        Fields = fields;
        RevertLinkEndsAt = revertEndsAt;
    }

    public bool IsRequested => Code is null;

    /// <summary>The id of the link sent to the new address (the key is derived from it when the e-mail is made); empty unless requested.</summary>
    public Guid? LinkId { get; }

    /// <summary>The moment the pending change and its link end (P-12 hours from this request).</summary>
    public DateTimeOffset? ExpiresAt { get; }

    /// <summary>True when this request took the place of an earlier pending change (02 3.13.14; K-734).</summary>
    public bool Replaced { get; }

    public ErrorCode? Code { get; }

    public IReadOnlyList<MembershipFieldError> Fields { get; }

    /// <summary>For <c>EmailChangeRevertPending</c>: when the open revert link ends (Z-45), which the screen shows (02 3.15.6).</summary>
    public DateTimeOffset? RevertLinkEndsAt { get; }

    public bool IsRejected => !IsRequested;

    internal static EmailChangeRequestResult Requested(Guid linkId, DateTimeOffset expiresAt, bool replaced) => new(linkId, expiresAt, replaced, null, [], null);

    internal static EmailChangeRequestResult Refused(ErrorCode code) => new(null, null, false, code, [], null);

    internal static EmailChangeRequestResult RevertPending(DateTimeOffset endsAt) => new(null, null, false, ErrorCode.EmailChangeRevertPending, [], endsAt);

    internal static EmailChangeRequestResult FieldRefused(MembershipFieldError field) => new(null, null, false, field.Code, [field], null);
}

/// <summary>The outcome of asking the new address's link again (02 3.13.14; 07 UC-67): a new link whose end is the first one's, the limit L-9, or no pending change.</summary>
public sealed class EmailChangeResendResult : IOperationResult
{
    private EmailChangeResendResult(Guid? linkId, DateTimeOffset? expiresAt, ErrorCode? code, bool nothingPending)
    {
        LinkId = linkId;
        ExpiresAt = expiresAt;
        Code = code;
        NothingPending = nothingPending;
    }

    public bool IsIssued => Code is null && !NothingPending;

    public Guid? LinkId { get; }

    /// <summary>The end of the new link: the pending change's end; asking again does not lengthen it (02 3.13.5).</summary>
    public DateTimeOffset? ExpiresAt { get; }

    /// <summary><c>VerificationLinkLimitExceeded</c>; empty otherwise.</summary>
    public ErrorCode? Code { get; }

    /// <summary>The account has no pending change, or its life has ended (the 422 of UC-67).</summary>
    public bool NothingPending { get; }

    public bool IsRejected => !IsIssued;

    internal static EmailChangeResendResult Issued(Guid linkId, DateTimeOffset expiresAt) => new(linkId, expiresAt, null, false);

    internal static EmailChangeResendResult Limited() => new(null, null, ErrorCode.VerificationLinkLimitExceeded, false);

    internal static EmailChangeResendResult Nothing() => new(null, null, null, true);
}

/// <summary>
/// The outcome of the new address's button (02 3.13.14; 07 UC-53): verified, with the number of guest orders of the new address that fell to the account and the id of the revert link
/// sent to the old address; or refused - the link is no longer valid (its state; the transaction rolls back), or the address became a verified account's meanwhile
/// (<c>EmailTakenByOtherAccount</c>; the change is not valid and the link's consumption is taken back by the rollback).
/// </summary>
public sealed class NewEmailVerificationResult : IOperationResult
{
    private NewEmailVerificationResult(Guid? customerId, int linkedOrders, Guid? revertLinkId, DateTimeOffset? revertEndsAt, VerificationLinkStatus status, bool addressTaken)
    {
        CustomerId = customerId;
        LinkedOrders = linkedOrders;
        RevertLinkId = revertLinkId;
        RevertLinkEndsAt = revertEndsAt;
        Status = status;
        AddressTaken = addressTaken;
    }

    public bool IsVerified => CustomerId is not null;

    public Guid? CustomerId { get; }

    public int LinkedOrders { get; }

    /// <summary>The id of the revert link that went to the old address (Z-45); empty unless verified.</summary>
    public Guid? RevertLinkId { get; }

    public DateTimeOffset? RevertLinkEndsAt { get; }

    public VerificationLinkStatus Status { get; }

    /// <summary>The address belongs to another verified account (<c>EmailTakenByOtherAccount</c>, K-1080).</summary>
    public bool AddressTaken { get; }

    public bool IsRejected => !IsVerified;

    internal static NewEmailVerificationResult Verified(Guid customerId, int linkedOrders, Guid revertLinkId, DateTimeOffset revertEndsAt) =>
        new(customerId, linkedOrders, revertLinkId, revertEndsAt, VerificationLinkStatus.Valid, false);

    internal static NewEmailVerificationResult LinkInvalid(VerificationLinkStatus status) => new(null, 0, null, null, status, false);

    internal static NewEmailVerificationResult Taken() => new(null, 0, null, null, VerificationLinkStatus.Valid, true);
}

/// <summary>The reading of the revert link ("bu değişikliği ben yapmadım") for its landing page (T519; 07 UC-75): only the link's status. Another type's key, an administrator's link and an unknown key are <see cref="VerificationLinkStatus.Missing"/>.</summary>
public sealed record EmailRevertLinkReading(VerificationLinkStatus Status);

/// <summary>
/// The outcome of the revert link's button (02 3.13.14; 07 UC-76): reverted, with the account's id (the page ends the browser's own session cookie only when it is this account's) and
/// whether a password reset link went to the earlier address (only an account with a password gets one); or refused - the link is no longer valid (its state; the page answers 200 with
/// the state it is in now), or the earlier address became another verified account's meanwhile (<c>EmailTakenByOtherAccount</c>, K-1210: nothing is written and the consumption rolls back, 422).
/// </summary>
public sealed class EmailRevertResult : IOperationResult
{
    private EmailRevertResult(Guid? customerId, bool resetLinkSent, VerificationLinkStatus status, bool addressTaken)
    {
        CustomerId = customerId;
        ResetLinkSent = resetLinkSent;
        Status = status;
        AddressTaken = addressTaken;
    }

    public bool IsReverted => CustomerId is not null;

    public Guid? CustomerId { get; }

    /// <summary>True when the account had a password: it became unusable and the earlier address got the reset link (02 3.13.14).</summary>
    public bool ResetLinkSent { get; }

    public VerificationLinkStatus Status { get; }

    /// <summary>The earlier address belongs to another verified account (<c>EmailTakenByOtherAccount</c>).</summary>
    public bool AddressTaken { get; }

    public bool IsRejected => !IsReverted;

    internal static EmailRevertResult Reverted(Guid customerId, bool resetLinkSent) => new(customerId, resetLinkSent, VerificationLinkStatus.Used, false);

    internal static EmailRevertResult LinkInvalid(VerificationLinkStatus status) => new(null, false, status, false);

    internal static EmailRevertResult Taken() => new(null, false, VerificationLinkStatus.Valid, true);
}
