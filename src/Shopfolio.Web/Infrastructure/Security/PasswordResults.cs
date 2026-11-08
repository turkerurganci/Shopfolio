using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>The state of an account link that a page shows when it opens (05 6.4.6): no record of the type, or the record's state.</summary>
public enum AccountLinkStatus
{
    Missing,
    Valid,
    Used,
    Superseded,
    Expired,
}

/// <summary>What the password check of the sign-in found (internal: the modules turn it into their own results).</summary>
internal enum VerdictKind
{
    /// <summary>The password matched and the account may sign in.</summary>
    Matched,

    /// <summary>No account, no password on the account, or a wrong password; the attempt counts.</summary>
    Mismatch,

    /// <summary>The right password of an account whose address is not verified (02 3.13.4); the attempt counts.</summary>
    NotEligible,

    /// <summary>The limit L-1 is exceeded: no password was checked (02 3.13.17).</summary>
    Blocked,
}

internal sealed record SignInVerdict(VerdictKind Outcome, Guid? AccountId);

/// <summary>The reading of a password reset link: its status and the account it belongs to.</summary>
public sealed record AccountLinkReading(AccountLinkStatus Status, Guid? AccountId);

/// <summary>
/// The outcome of a password reset asked for (02 3.13.11, 3.13.17): refused only over the limit L-2; otherwise a success whatever the address - when an account is eligible the
/// link's id and end are filled (the e-mail body derives the key from the id, T309), when there is none they are empty and the screen says the same words.
/// </summary>
public sealed class ResetRequested : IOperationResult
{
    private ResetRequested(Guid? accountId, Guid? linkId, DateTimeOffset? expiresAt, ErrorCode? code)
    {
        AccountId = accountId;
        LinkId = linkId;
        ExpiresAt = expiresAt;
        Code = code;
    }

    public Guid? AccountId { get; }

    public Guid? LinkId { get; }

    public DateTimeOffset? ExpiresAt { get; }

    /// <summary><c>PasswordResetLimitExceeded</c> when over the limit; empty otherwise.</summary>
    public ErrorCode? Code { get; }

    /// <summary>True when a link was made and an e-mail is to be sent.</summary>
    public bool IsIssued => LinkId is not null;

    public bool IsRejected => Code is not null;

    internal static ResetRequested Issued(Guid accountId, Guid linkId, DateTimeOffset expiresAt) => new(accountId, linkId, expiresAt, null);

    internal static ResetRequested NotIssued() => new(null, null, null, null);

    internal static ResetRequested Limited() => new(null, null, null, ErrorCode.PasswordResetLimitExceeded);
}

/// <summary>
/// The outcome of the reset's button (02 3.13.9, 3.13.10): done - the password is new, every session of the account has ended and every mark but the presented one is ended, and the
/// mark to write is given - or refused, because the link is not valid (with its state) or the new password breaks the policy (the link is then not consumed). A refusal rolls the transaction back.
/// </summary>
public sealed class ResetOutcome : IOperationResult
{
    private ResetOutcome(Guid? accountId, string? markValue, TimeSpan markLifetime, ErrorCode? code, AccountLinkStatus? linkStatus)
    {
        AccountId = accountId;
        MarkValue = markValue;
        MarkLifetime = markLifetime;
        Code = code;
        LinkStatus = linkStatus;
    }

    public bool IsDone => AccountId is not null;

    public Guid? AccountId { get; }

    /// <summary>The recognized browser mark to write on the browser that made the reset.</summary>
    public string? MarkValue { get; }

    public TimeSpan MarkLifetime { get; }

    /// <summary>The policy's code when the new password was refused; empty otherwise.</summary>
    public ErrorCode? Code { get; }

    /// <summary>The link's state when the link was not valid; empty otherwise.</summary>
    public AccountLinkStatus? LinkStatus { get; }

    public bool IsRejected => !IsDone;

    internal static ResetOutcome Done(Guid accountId, string markValue, TimeSpan markLifetime) => new(accountId, markValue, markLifetime, null, null);

    internal static ResetOutcome Invalid(AccountLinkStatus status) => new(null, null, TimeSpan.Zero, null, status);

    internal static ResetOutcome Refused(ErrorCode code) => new(null, null, TimeSpan.Zero, code, null);
}

/// <summary>The outcome of a reauthentication (02 3.13.13): the value of the operation form's hidden field, or the refusal's code.</summary>
public sealed class ReauthenticationResult : IOperationResult
{
    private ReauthenticationResult(string? value, ErrorCode? code)
    {
        Value = value;
        Code = code;
    }

    public bool Succeeded => Value is not null;

    public string? Value { get; }

    /// <summary><c>ReauthenticationFailed</c>, <c>SignInLimitExceeded</c> or <c>ReauthenticationOperationInvalid</c>; empty when it succeeded.</summary>
    public ErrorCode? Code { get; }

    public bool IsRejected => !Succeeded;

    internal static ReauthenticationResult Granted(string value) => new(value, null);

    internal static ReauthenticationResult Refused(ErrorCode code) => new(null, code);
}

/// <summary>The outcome of the password change at the account (02 3.13.10, 3.13.13): done, with the mark to write, or the refusal's code.</summary>
public sealed class ChangeOutcome : IOperationResult
{
    private ChangeOutcome(string? markValue, TimeSpan markLifetime, ErrorCode? code)
    {
        MarkValue = markValue;
        MarkLifetime = markLifetime;
        Code = code;
    }

    public bool IsDone => Code is null;

    public string? MarkValue { get; }

    public TimeSpan MarkLifetime { get; }

    /// <summary><c>PasswordTooWeak</c> or <c>FieldRequired</c> for the new password, or <c>ReauthenticationRequired</c>; empty when done.</summary>
    public ErrorCode? Code { get; }

    public bool IsRejected => !IsDone;

    internal static ChangeOutcome Done(string markValue, TimeSpan markLifetime) => new(markValue, markLifetime, null);

    internal static ChangeOutcome Refused(ErrorCode code) => new(null, TimeSpan.Zero, code);
}

/// <summary>The outcome of the administrator's sign-in check: the administrator who may be given a session, or the refusal's code (<c>InvalidCredentials</c> or <c>SignInLimitExceeded</c>).</summary>
public sealed class AdminCredentialCheckResult : IOperationResult
{
    private AdminCredentialCheckResult(Guid? adminId, ErrorCode? code)
    {
        AdminId = adminId;
        Code = code;
    }

    public bool Succeeded => AdminId is not null;

    public Guid? AdminId { get; }

    public ErrorCode? Code { get; }

    public bool IsRejected => !Succeeded;

    internal static AdminCredentialCheckResult Granted(Guid adminId) => new(adminId, null);

    internal static AdminCredentialCheckResult Refused(ErrorCode code) => new(null, code);
}

/// <summary>The account a password rule works on, as the account's module gives it: the key, the address, the digest and whether the address is verified.</summary>
internal sealed record PasswordAccount(Guid Id, string Email, string? PasswordHash, bool IsVerified);

/// <summary>
/// The adapter each account module gives the one password rule (<see cref="PasswordRegime"/>; 02 3.13.16): a customer's and an administrator's account are found, and have their
/// password written, only through it, so the administrator goes through the same code as the customer.
/// </summary>
internal interface IPasswordAccounts
{
    AccountType Type { get; }

    Task<PasswordAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken ct);

    /// <summary>
    /// The sign-in's read (09 15.1 item 3; 05 6.1.2): locks the account's row first (<see cref="AccountLocks"/>, held to the end of the transaction) and reads the digest after the
    /// lock, so the digest checked is the digest a concurrent reset or change has left; null when there is no such account.
    /// </summary>
    Task<PasswordAccount?> LockAndFindByEmailAsync(string normalizedEmail, CancellationToken ct);

    /// <summary>
    /// Takes the account's row lock (<see cref="AccountLocks"/>); false when there is no such account. Every flow that writes the digest or the account's session rows (reset, change,
    /// reauthentication) takes it FIRST, before any other row of the account, so all flows lock in the same order: account row, then session rows (09 15.1 item 3; 05 7.3.5).
    /// </summary>
    Task<bool> LockAsync(Guid id, CancellationToken ct);

    Task<PasswordAccount?> FindAsync(Guid id, CancellationToken ct);

    /// <summary>Takes the account's row lock, then writes the new digest with a conditional update in the caller's transaction.</summary>
    Task SetPasswordAsync(Guid id, string digest, CancellationToken ct);
}
