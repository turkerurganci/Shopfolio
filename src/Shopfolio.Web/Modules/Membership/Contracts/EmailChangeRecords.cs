using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Membership.Contracts.Results;

namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>The reading of one of the three links of the landing page: its kind and status and the account it belongs to. Only reads (MK-51); a key of another type is <see cref="VerificationLinkStatus.Missing"/>.</summary>
public sealed record EmailLinkReading(EmailLinkKind? Kind, VerificationLinkStatus Status, Guid? CustomerId);

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
