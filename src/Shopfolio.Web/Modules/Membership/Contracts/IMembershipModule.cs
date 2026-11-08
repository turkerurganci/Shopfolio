using System.Net;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>
/// The open surface of the Membership module (09 6 item 1; 05 3.4.1): what other modules, the pages and the commands may call. The module owns the customer account and its address book (05 3.3.6).
/// The members arrive with the tasks that own the module's rules, as methods that take and give immutable records and join the caller's
/// database transaction (09 6 item 3); a refusal is a returned value (09 8.1), never an exception. The registration, the e-mail verification, the sign-in check, the guest
/// order's account and the account name are T341's (02 3.13.1 to 3.13.5, 3.13.18 to 3.13.21); the password policy, the reset, the session and the sign-in limit L-1 are T342's.
/// </summary>
public interface IMembershipModule
{
    /// <summary>
    /// The registration form's save (02 3.13.5, 3.13.18, 3.13.19, 3.13.21; 07 UC-47): in the caller's open transaction. In this order: the gate of the data-collecting
    /// entries (closed: <c>DataCollectionClosed</c>); the fields (the name and the password are required, the e-mail is a valid address); the registration limit L-3 on the
    /// e-mail and on the IP, counted on every request in a transaction of its own; then the address. An address that is a pending registration whose first link has ended is
    /// freed - the pending row goes with its links, with a count in the disposal record - and the registration goes on; any other known address is
    /// <c>EmailAlreadyRegistered</c>. The account is born unverified with the password's digest, the first verification link lives P-12 hours. The caller sends the e-mail
    /// (T309) and may run this with a retry on the unique index of the e-mail, so a second registration of the same address at the same moment is answered as the first's
    /// duplicate. There is no marketing consent field in the input or in the account (02 3.13.18).
    /// </summary>
    Task<RegistrationResult> RegisterAsync(RegisterInput input, IPAddress? ipAddress, CancellationToken ct);

    /// <summary>
    /// What the verification page shows when the link opens (02 3.13.5; 07 UC-48): only reads, writes nothing, so opening the link verifies nothing. A key of another link
    /// type, an unknown key and an empty key are all <see cref="VerificationLinkStatus.Missing"/>.
    /// </summary>
    Task<VerificationLinkReading> ReadVerificationLinkAsync(string key, CancellationToken ct);

    /// <summary>
    /// The verification button's POST (02 3.13.5; 07 UC-48): in the caller's transaction, one conditional update consumes the link, the account's moment is set and the
    /// guest orders of the verified address - not those of a deleted account - fall to the account (02 3.13.2). A used, superseded, expired or unknown link is a refusal
    /// that names the state, and nothing is written.
    /// </summary>
    Task<EmailVerificationResult> VerifyEmailAsync(string key, CancellationToken ct);

    /// <summary>
    /// A new verification link asked for on the screen (02 3.13.5; L-9): counted on the e-mail and on the IP in a transaction of its own; the earlier links of the account
    /// are superseded and the new one ends when the first one ends - it does not lengthen the pending registration. A verified account, a missing one and a registration
    /// whose first link has ended are <c>PendingRegistrationUnresolved</c>.
    /// </summary>
    Task<VerificationResendResult> ResendVerificationLinkAsync(Guid customerId, IPAddress? ipAddress, CancellationToken ct);

    /// <summary>
    /// The sign-in's check of the e-mail and the password (02 3.13.4, 3.13.19, 3.13.20): the same work is done whether or not the address has an account; an unverified account
    /// is refused even with the right password (<c>EmailNotVerified</c>) and the attempt is written as failed. Every attempt is one sign-in record row with its moment, IP,
    /// e-mail and outcome, written in a transaction of its own so a refusal does not take it back. The sign-in is never behind the gate. The limit L-1 (02 3.13.17) is counted
    /// before the work, on the IP and on the e-mail - or, when <paramref name="recognizedBrowserMark"/> is a valid mark of the account, on that mark alone with the IP; over the
    /// threshold nothing is checked, the account is not locked and the refusal is <c>SignInLimitExceeded</c> (the attempt is written as failed). The session is
    /// <see cref="ISessionService.SignInAsync"/>.
    /// </summary>
    Task<CredentialCheckResult> CheckCredentialsAsync(string email, string password, IPAddress? ipAddress, string? recognizedBrowserMark, CancellationToken ct);

    /// <summary>
    /// The reset's request (02 3.13.11, 3.13.17; L-2): counted on the e-mail and the IP on every request; over the limit the refusal is <c>PasswordResetLimitExceeded</c>; otherwise
    /// a success that does not tell whether the address has an account - the link's id is filled only for an eligible (verified) account, the caller sends the e-mail (T309).
    /// </summary>
    Task<ResetRequested> RequestPasswordResetAsync(string email, IPAddress? ipAddress, CancellationToken ct);

    /// <summary>What the reset page shows when the link opens (02 3.13.9): only reads; a key of another type is missing.</summary>
    Task<AccountLinkReading> ReadPasswordResetLinkAsync(string key, CancellationToken ct);

    /// <summary>
    /// The reset's button (02 3.13.9, 3.13.10): the new password is checked against the policy first and a refusal consumes nothing; then the link is consumed, the password written,
    /// every session of the account ended and every browser mark but the presented one ended. A refusal rolls the caller's transaction back.
    /// </summary>
    Task<ResetOutcome> ResetPasswordAsync(string key, string newPassword, string? recognizedBrowserMark, CancellationToken ct);

    /// <summary>The reauthentication of a signed-in customer for one operation (02 3.13.13): a wrong password counts to L-1; success returns the value of the operation form's hidden field.</summary>
    Task<ReauthenticationResult> ReauthenticateAsync(Guid customerId, Guid sessionId, ReauthenticationOperation operation, string password, IPAddress? ipAddress, string? recognizedBrowserMark, CancellationToken ct);

    /// <summary>
    /// The password change at the account (02 3.13.10, 3.13.13): the policy, then the reauthentication value of <see cref="ReauthenticateAsync"/> (none: <c>ReauthenticationRequired</c>);
    /// the other sessions end, the session that changed the password stays, the other browsers' marks end. A change of the name asks for none of this.
    /// </summary>
    Task<ChangeOutcome> ChangePasswordAsync(Guid customerId, Guid sessionId, string newPassword, string? reauthenticationValue, string? recognizedBrowserMark, CancellationToken ct);

    /// <summary>
    /// The account an order belongs to (02 3.13.1, 3.13.3, 3.13.4; read only): a signed-in verified customer's order is theirs; a guest whose address belongs to a verified
    /// account gets the order bound to it at once, in full; every other guest order stays a guest order and no verification is asked; a signed-in customer whose account is not
    /// verified is refused. A pending registration never receives an order.
    /// </summary>
    Task<OrderAccountResolution> ResolveOrderAccountAsync(Guid? signedInCustomerId, string contactEmail, CancellationToken ct);

    /// <summary>The member changes the account's name (02 3.13.21): an empty name is refused; no verification is asked again and no order's recipient name is touched.</summary>
    Task<NameChangeResult> ChangeNameAsync(Guid customerId, string name, CancellationToken ct);
}
