using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;

namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>
/// The outcome of an account's deletion (02 3.15.1, 3.15.5, 3.15.6; 07 UC-68, UC-157): deleted, with the number of orders that went on with their own record, or refused with a code -
/// <c>ReauthenticationRequired</c> (the member's own deletion without the one-operation permission), <c>EmailChangeRevertPending</c> (an e-mail change's revert link is open: the screen
/// says why and until when, <see cref="RevertLinkEndsAt"/>) or <c>NotFound</c> (no verified account; the second submit of a deletion that happened). A refusal rolls the caller's transaction back.
/// </summary>
public sealed class AccountDeletionResult : IOperationResult
{
    private AccountDeletionResult(int ordersKept, ErrorCode? code, DateTimeOffset? revertEndsAt)
    {
        OrdersKept = ordersKept;
        Code = code;
        RevertLinkEndsAt = revertEndsAt;
    }

    public bool IsDeleted => Code is null;

    /// <summary>The orders of the account that were marked as an account's deleted one and went on with their own record (02 3.15.2, 3.15.3); zero unless deleted.</summary>
    public int OrdersKept { get; }

    public ErrorCode? Code { get; }

    /// <summary>For <c>EmailChangeRevertPending</c>: when the open revert link ends (Z-45), which the screen shows (02 3.15.6).</summary>
    public DateTimeOffset? RevertLinkEndsAt { get; }

    public bool IsRejected => !IsDeleted;

    internal static AccountDeletionResult Deleted(int ordersKept) => new(ordersKept, null, null);

    internal static AccountDeletionResult Refused(ErrorCode code) => new(0, code, null);

    internal static AccountDeletionResult RevertPending(DateTimeOffset endsAt) => new(0, ErrorCode.EmailChangeRevertPending, endsAt);
}

/// <summary>What the panel's member record view found for an e-mail (02 3.15.5; 04 9.12.13): no account, an unverified pending registration, or a verified account.</summary>
public enum MemberRecordKind
{
    /// <summary>No customer account carries the address (not an error: the view says so).</summary>
    NotFound,

    /// <summary>The address belongs to a pending registration whose e-mail was never verified; it is no account and cannot be deleted from the view.</summary>
    Unverified,

    /// <summary>A verified account.</summary>
    Account,
}

/// <summary>
/// The member record view (02 3.15.5; 07 UC-95; 04 9.12): what the firm reads to answer a personal data request - the account's existence and name (02 3.13.21), the address book, the
/// ways in (password, Google) and the revert link that closes the deletion (02 3.15.6). It is a value: nothing in it is written back, and the module has no member-record edit.
/// </summary>
/// <param name="Kind">What the address is.</param>
/// <param name="CustomerId">The account's id; empty when there is none.</param>
/// <param name="Name">The account's name; empty unless a verified account.</param>
/// <param name="Email">The address as entered; empty unless there is a row.</param>
/// <param name="CreatedAt">When the account (or the pending registration) was made.</param>
/// <param name="HasPassword">The account has a password (a Google-born account has none until it sets one).</param>
/// <param name="IsGoogleLinked">The account has a Google sign-in.</param>
/// <param name="Addresses">The address book, oldest first.</param>
/// <param name="DeletionBlockedUntil">The end of an open revert link (Z-45) while it is open: the deletion is closed until then; empty otherwise.</param>
public sealed record MemberRecord(
    MemberRecordKind Kind,
    Guid? CustomerId,
    string? Name,
    string? Email,
    DateTimeOffset? CreatedAt,
    bool HasPassword,
    bool IsGoogleLinked,
    IReadOnlyList<AddressBookEntry> Addresses,
    DateTimeOffset? DeletionBlockedUntil)
{
    /// <summary>The view of an address with no account.</summary>
    public static MemberRecord None { get; } = new(MemberRecordKind.NotFound, null, null, null, null, false, false, [], null);
}
