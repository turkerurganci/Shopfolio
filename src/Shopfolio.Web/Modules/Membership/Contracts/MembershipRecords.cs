using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>
/// The registration form's three values (02 3.13.21): the name, the e-mail and the password, and nothing else. There is no marketing consent field here or in the account
/// (02 3.13.18). The password is never written to a record, a log or a message (09 18.2).
/// </summary>
public sealed record RegisterInput(string Name, string Email, string Password);

/// <summary>The unique index of the registration (kept apart from the form's record, which holds the three values and nothing else; 02 3.13.21).</summary>
public static class RegistrationIndexes
{
    /// <summary>The unique index of the e-mail (06 4.36): the caller runs the registration with it, so a second registration of the same address at the same moment is answered as the first's duplicate (09 9.4 item 3).</summary>
    public static IReadOnlyCollection<string> RetryOnUniqueIndexes { get; } = ["ix_customers_normalized_email"];
}

/// <summary>
/// One refused field of a form: the field's name as the input's and the code of 07 6.2.2. <paramref name="Detail"/> carries the one value the message needs when the code alone is not
/// enough - the province name of a delivery refusal (04 5.13.23) - and is null otherwise.
/// </summary>
public sealed record MembershipFieldError(string Field, ErrorCode Code, string? Detail = null);

/// <summary>
/// The outcome of the registration (02 3.13.5; 07 UC-47): registered with the id of the unverified account and of its first verification link and the moment that link ends,
/// or refused with a code - the closed gate (with its refusal), the fields (with their errors), the limit L-3, or an address that is already an account.
/// </summary>
public sealed class RegistrationResult : IOperationResult
{
    private static readonly MembershipFieldError[] NoFields = [];

    private RegistrationResult(Guid? customerId, Guid? linkId, DateTimeOffset? linkExpiresAt, ErrorCode? code, IReadOnlyList<MembershipFieldError> fields, GateRefusal? gate)
    {
        CustomerId = customerId;
        LinkId = linkId;
        LinkExpiresAt = linkExpiresAt;
        Code = code;
        Fields = fields;
        Gate = gate;
    }

    public bool IsRegistered => Code is null;

    /// <summary>The id of the new unverified account; empty unless registered.</summary>
    public Guid? CustomerId { get; }

    /// <summary>The id of the first verification link record, from which the e-mail body derives the key (T309); empty unless registered.</summary>
    public Guid? LinkId { get; }

    /// <summary>The moment the first link ends, which is also the end of the pending registration (P-12); empty unless registered.</summary>
    public DateTimeOffset? LinkExpiresAt { get; }

    /// <summary>The refusal's code; empty when registered.</summary>
    public ErrorCode? Code { get; }

    /// <summary>The refused fields; empty unless <see cref="Code"/> is a field refusal.</summary>
    public IReadOnlyList<MembershipFieldError> Fields { get; }

    /// <summary>The gate's refusal, with the conditions that wait; filled only when the data-collecting entries are closed.</summary>
    public GateRefusal? Gate { get; }

    public bool IsRejected => !IsRegistered;

    internal static RegistrationResult Registered(Guid customerId, Guid linkId, DateTimeOffset linkExpiresAt) => new(customerId, linkId, linkExpiresAt, null, NoFields, null);

    internal static RegistrationResult Refused(ErrorCode code) => new(null, null, null, code, NoFields, null);

    internal static RegistrationResult GateClosed(GateRefusal gate) => new(null, null, null, gate.Code, NoFields, gate);

    internal static RegistrationResult FieldsRefused(IReadOnlyList<MembershipFieldError> fields) => new(null, null, null, fields[0].Code, fields, null);
}

/// <summary>
/// What the registration screen shows after the form is sent (T514; 04 5.20.4): the address the link went to and the last day of the pending registration (the end of its first link,
/// Z-1). Nothing of the account's name or password.
/// </summary>
public sealed record PendingRegistrationReading(string Email, DateTimeOffset LinkEndsAt);

/// <summary>The reading of a verification link: its status and, when there is a record, the account it belongs to.</summary>
public sealed record VerificationLinkReading(VerificationLinkStatus Status, Guid? CustomerId);

/// <summary>
/// The outcome of the verification button (02 3.13.5; 07 UC-48): verified, with the number of guest orders that fell to the account; or refused because the link is not valid,
/// with its state. A refusal rolls the transaction back.
/// </summary>
public sealed class EmailVerificationResult : IOperationResult
{
    private EmailVerificationResult(Guid? customerId, int linkedOrders, VerificationLinkStatus status)
    {
        CustomerId = customerId;
        LinkedOrders = linkedOrders;
        Status = status;
    }

    public bool IsVerified => CustomerId is not null;

    public Guid? CustomerId { get; }

    /// <summary>The guest orders of the verified address that now belong to the account (02 3.13.2).</summary>
    public int LinkedOrders { get; }

    /// <summary>The link's state: <see cref="VerificationLinkStatus.Valid"/> when verified, else why not.</summary>
    public VerificationLinkStatus Status { get; }

    public bool IsRejected => !IsVerified;

    internal static EmailVerificationResult Verified(Guid customerId, int linkedOrders) => new(customerId, linkedOrders, VerificationLinkStatus.Valid);

    internal static EmailVerificationResult LinkInvalid(VerificationLinkStatus status) => new(null, 0, status);
}

/// <summary>The outcome of a new verification link asked for: sent (the link's id and end), over the limit L-9, or nothing left to verify.</summary>
public sealed class VerificationResendResult : IOperationResult
{
    private VerificationResendResult(Guid? linkId, DateTimeOffset? expiresAt, ErrorCode? code)
    {
        LinkId = linkId;
        ExpiresAt = expiresAt;
        Code = code;
    }

    public bool IsIssued => Code is null;

    public Guid? LinkId { get; }

    /// <summary>The end of the new link: the first link's end, never later (02 3.13.5).</summary>
    public DateTimeOffset? ExpiresAt { get; }

    /// <summary><c>VerificationLinkLimitExceeded</c> or <c>PendingRegistrationUnresolved</c>; empty when issued.</summary>
    public ErrorCode? Code { get; }

    public bool IsRejected => !IsIssued;

    internal static VerificationResendResult Issued(Guid linkId, DateTimeOffset expiresAt) => new(linkId, expiresAt, null);

    internal static VerificationResendResult Refused(ErrorCode code) => new(null, null, code);
}

/// <summary>The outcome of the sign-in's credential check: the customer who may be given a session, or the refusal's code (<c>InvalidCredentials</c> or <c>EmailNotVerified</c>).</summary>
public sealed class CredentialCheckResult : IOperationResult
{
    private CredentialCheckResult(Guid? customerId, ErrorCode? code)
    {
        CustomerId = customerId;
        Code = code;
    }

    public bool Succeeded => CustomerId is not null;

    public Guid? CustomerId { get; }

    public ErrorCode? Code { get; }

    public bool IsRejected => !Succeeded;

    internal static CredentialCheckResult Granted(Guid customerId) => new(customerId, null);

    internal static CredentialCheckResult Refused(ErrorCode code) => new(null, code);
}

/// <summary>The account an order is placed under: the kind and, for the two bound kinds, the customer.</summary>
public sealed record OrderAccountResolution(OrderAccountKind Kind, Guid? CustomerId)
{
    /// <summary>True when the order is placed as a guest's (no sign-in), whether or not it is bound to an account.</summary>
    public bool IsGuestOrder => Kind is OrderAccountKind.Guest or OrderAccountKind.GuestBoundToAccount;

    /// <summary>True when the order may be placed.</summary>
    public bool IsAccepted => Kind != OrderAccountKind.RefusedUnverified;
}

/// <summary>The outcome of the member changing the account's name: changed, refused for the field, or no such account.</summary>
public sealed class NameChangeResult : IOperationResult
{
    private NameChangeResult(bool isChanged, bool isAccountMissing, IReadOnlyList<MembershipFieldError> fields)
    {
        IsChanged = isChanged;
        IsAccountMissing = isAccountMissing;
        Fields = fields;
    }

    public bool IsChanged { get; }

    public bool IsAccountMissing { get; }

    public IReadOnlyList<MembershipFieldError> Fields { get; }

    public bool IsRejected => !IsChanged;

    internal static NameChangeResult Changed() => new(true, false, []);

    internal static NameChangeResult Missing() => new(false, true, []);

    internal static NameChangeResult FieldRefused(MembershipFieldError field) => new(false, false, [field]);
}
