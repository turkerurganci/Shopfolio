using System.Net;

using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Catalog.Contracts;

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
    /// True when the installation has the Google application (02 3.13.6; T226's conditional line, D-81): the sign-in and registration screens show "Google ile giriş" only then, and the
    /// panel has no switch for it. When it is false <see cref="SignInWithGoogleAsync"/> answers <see cref="GoogleSignInOutcome.Unavailable"/> and does nothing.
    /// </summary>
    bool IsGoogleSignInOpen { get; }

    /// <summary>
    /// What a verified Google identity means for the accounts (02 3.13.7, 3.13.8, 3.13.19; 07 UC-51), in the caller's transaction: Google's address not verified binds and opens nothing; a
    /// matching verified account signs in (<see cref="GoogleSignInOutcome.SignedIn"/>) - linked now when Google is the authority of the address (<c>@gmail.com</c> or a Workspace
    /// <c>hd</c>) or already linked; a matching account that is not linked, with an address Google is not the authority of, gets a confirmation link and no session
    /// (<see cref="GoogleSignInOutcome.ConfirmationRequired"/>; the link and the e-mail's row are made under the account's row lock, none while a valid link waits); no matching account
    /// opens a passwordless verified linked account named by Google (or by the part of the address before the at sign) when the gate of the data-collecting entries is open; an unverified
    /// pending registration of the address is deleted with its links and the account is born in its place (the gate permitting; closed: nothing changes). A born account is only added
    /// to the context: the caller saves before it opens the session. The sign-in is not counted to L-1. Joins the caller's transaction; on a unique conflict of the e-mail the caller
    /// retries like the registration does.
    /// </summary>
    Task<GoogleSignInResult> SignInWithGoogleAsync(GoogleIdentity identity, IPAddress? ipAddress, CancellationToken ct);

    /// <summary>
    /// The Google confirmation link's button (02 3.13.7; 07 UC-53): consumes the link and links the account to Google; opens no session (the user signs in with Google again). The
    /// link's GET is <see cref="ReadEmailLinkAsync"/>.
    /// </summary>
    Task<GoogleLinkConfirmationResult> ConfirmGoogleLinkAsync(string key, CancellationToken ct);

    /// <summary>
    /// What the shared landing page (07 UC-52, E-21) shows for a registration, new-address or Google confirmation link (05 6.4.6): only reads, writes nothing, so opening a link does
    /// nothing. Another type's key and an administrator's link are <see cref="VerificationLinkStatus.Missing"/>.
    /// </summary>
    Task<EmailLinkReading> ReadEmailLinkAsync(string key, CancellationToken ct);

    /// <summary>
    /// The account's request for a new address (02 3.13.14; 07 UC-66), in the caller's transaction: the address is checked (empty, form), an open revert link closes the change
    /// (<c>EmailChangeRevertPending</c>), an address of a verified account - or the account's own - is <c>EmailTakenByOtherAccount</c> (an unverified pending registration does not
    /// stop it); the pending change is written, taking the place of an earlier one (its link superseded, its life starting now), and the link goes to the NEW address; the account keeps the
    /// old address until the link's button. The limit L-9 is counted only when the request replaces a live pending change. The reauthentication before this is the page's (T517).
    /// </summary>
    Task<EmailChangeRequestResult> RequestEmailChangeAsync(Guid customerId, string newEmail, IPAddress? ipAddress, CancellationToken ct);

    /// <summary>The new address's link asked for again (02 3.13.14; 07 UC-67; L-9): the earlier links are superseded and the new one ends where the pending change ends.</summary>
    Task<EmailChangeResendResult> ResendEmailChangeLinkAsync(Guid customerId, IPAddress? ipAddress, CancellationToken ct);

    /// <summary>
    /// The new address's button (02 3.13.14; 07 UC-53), in the caller's transaction: the link is consumed, the address changes, the Google link goes, an unverified pending registration of
    /// the address is deleted, the old address gets the notice with the revert link (Z-45) and the new address's guest orders fall to the account. A refusal (an invalid link, an address that
    /// became a verified account's) rolls the transaction back.
    /// </summary>
    Task<NewEmailVerificationResult> VerifyNewEmailAsync(string key, CancellationToken ct);

    /// <summary>
    /// The account an order belongs to (02 3.13.1, 3.13.3, 3.13.4; read only): a signed-in verified customer's order is theirs; a guest whose address belongs to a verified
    /// account gets the order bound to it at once, in full; every other guest order stays a guest order and no verification is asked; a signed-in customer whose account is not
    /// verified is refused. A pending registration never receives an order.
    /// </summary>
    Task<OrderAccountResolution> ResolveOrderAccountAsync(Guid? signedInCustomerId, string contactEmail, CancellationToken ct);

    /// <summary>The member changes the account's name (02 3.13.21): an empty name is refused; no verification is asked again and no order's recipient name is touched.</summary>
    Task<NameChangeResult> ChangeNameAsync(Guid customerId, string name, CancellationToken ct);

    /// <summary>
    /// The member's address book (T344; 02 3.14.1; 07 UC-70): the entries, oldest first, each with the province's name; read only. A guest has no book - the caller has no customer id to ask
    /// with.
    /// </summary>
    Task<IReadOnlyList<AddressBookEntry>> GetAddressBookAsync(Guid customerId, CancellationToken ct);

    /// <summary>One entry of the member's book for the edit form (07 UC-70): empty when it is not the member's or is gone - the answer is the same (07 2.3.3).</summary>
    Task<AddressBookEntry?> GetAddressBookEntryAsync(Guid customerId, Guid entryId, CancellationToken ct);

    /// <summary>
    /// A new entry in the member's book (02 3.14.1, 3.14.6; 07 UC-71): in the caller's transaction, which saves. The name, the recipient's name, the province from the closed list, the
    /// district and the open address are required; the telephone is optional and, when given, a Turkish number. A province that is not in the list of 81 is <c>InvalidProvince</c>. A
    /// province the firm does not deliver to is kept - the restriction works at the payment step (T350).
    /// </summary>
    Task<AddressBookSaveResult> CreateAddressBookEntryAsync(Guid customerId, AddressBookInput input, CancellationToken ct);

    /// <summary>
    /// The member's edit of an entry (07 UC-72): the same fields and rules as the new entry. An entry that is not the member's, or is gone, is not found whatever the fields are. No order
    /// is touched - an order froze its addresses (02 3.14.5).
    /// </summary>
    Task<AddressBookSaveResult> UpdateAddressBookEntryAsync(Guid customerId, Guid entryId, AddressBookInput input, CancellationToken ct);

    /// <summary>The member's deletion of an entry (07 UC-73; no confirmation is asked): deleted, or not found when it is not the member's or was gone. No order is touched (02 3.14.5).</summary>
    Task<AddressBookDeleteResult> DeleteAddressBookEntryAsync(Guid customerId, Guid entryId, CancellationToken ct);

    /// <summary>
    /// The addresses a basket asks for by the types of its items (02 3.14.4): at least one physical item asks the shipping and the billing address (the billing one may be the shipping
    /// one); digital and service items only ask the billing address and show no shipping field. A basket with no item is an <see cref="ArgumentException"/>.
    /// </summary>
    AddressRequirement GetAddressRequirement(IReadOnlyCollection<ProductType> itemTypes);

    /// <summary>
    /// The address part of an order (02 3.14.1-3.14.6; 07 UC-27): in the caller's transaction, which saves. The addresses the basket asks for are checked, every refused field together, and the
    /// values to freeze are returned; a member's typed shipping address is written to the book only when the member chose to keep it (K-680), a guest has none. The shipping address always
    /// carries a telephone - a book entry without one takes the one typed on the step - and the billing address carries none. The caller writes the returned values into the order's own
    /// columns: the order keeps no link to a book entry, so an entry's later change or deletion never reaches it (02 3.14.5). A basket that asks for a shipping address (a physical item) is checked against the company's delivery provinces (02 3.20.1): a shipping province it does not deliver to is refused with
    /// <see cref="ErrorCode.DeliveryProvinceNotServed"/> and the province's name; a digital or service basket never reaches that check, and the billing province is never restricted.
    /// </summary>
    Task<OrderAddressResolution> ResolveOrderAddressesAsync(OrderAddressRequest request, CancellationToken ct);

    /// <summary>
    /// The member's own deletion of the account (T345; 02 3.15.1-3.15.3, 3.15.6; 07 UC-68), in the caller's transaction, which saves: an open revert link of an e-mail change closes it
    /// (<c>EmailChangeRevertPending</c>, with the end of the link, nothing deleted, the reauthentication value unspent); the reauthentication value of <see cref="ReauthenticateAsync"/> is
    /// redeemed (none: <c>ReauthenticationRequired</c>); the orders go on with their own record and are never linked again; one e-mail goes to the address; the account and what hangs on it
    /// are deleted and the disposal record counts them. The irreversible-action confirmation is the page's (03 1.6.1.8).
    /// </summary>
    Task<AccountDeletionResult> DeleteAccountAsync(Guid customerId, Guid sessionId, string? reauthenticationValue, CancellationToken ct);

    /// <summary>
    /// The administrator's deletion on the data subject's request (T345; 02 3.15.5; 07 UC-157), in the caller's transaction, which saves: the same rule as the member's own with no
    /// reauthentication, and a row of the operation in the audit trail that names the operation and its date and nothing of the account (02 10.3.1). An unverified pending registration is not found.
    /// </summary>
    Task<AccountDeletionResult> DeleteAccountOnRequestAsync(Guid customerId, CancellationToken ct);

    /// <summary>
    /// The account's deletion applied again after the database was returned to an earlier moment (T388; 05 8.5.10), for the <c>reapply-deletions</c> command, in the caller's transaction, which
    /// saves: the same removal as the two deletions above with none of a user's gates (no revert-link check, no reauthentication, no trail row) and no e-mail - the owner was told the first time.
    /// An account that is not there already is skipped.
    /// </summary>
    Task<DeletionReapplyResult> ReapplyAccountDeletionAsync(Guid customerId, CancellationToken ct);

    /// <summary>The member record view for an e-mail (T345; 02 3.15.5; 07 UC-95): read only, a value that nothing writes back; no account, an unverified pending registration or a verified account.</summary>
    Task<MemberRecord> FindMemberRecordAsync(string email, CancellationToken ct);

    /// <summary>The end of the open revert link that closes the account's deletion, or nothing (T345; 02 3.15.6): the account screen shows the reason and the date.</summary>
    Task<DateTimeOffset?> GetDeletionBlockedUntilAsync(Guid customerId, CancellationToken ct);

    /// <summary>
    /// Writes the member list export's content to <paramref name="destination"/> (T384; 02 10.7.4; 09 10.10): a CSV with the header <c>Ad;E-posta</c> and one line for every account - one that
    /// never ordered and the unverified pending registration included - with no date range. Returns the number of lines (0: no account - the caller makes no file). The stream stays open;
    /// the writing stops with an exception when the file passes <paramref name="maxBytes"/>. A plain read.
    /// </summary>
    Task<int> WriteCustomerExportAsync(Stream destination, long maxBytes, CancellationToken ct);
}
