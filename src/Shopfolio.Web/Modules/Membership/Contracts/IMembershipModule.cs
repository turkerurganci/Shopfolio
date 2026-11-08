using System.Net;

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
    /// e-mail and outcome, written in a transaction of its own so a refusal does not take it back. The sign-in is never behind the gate. The limit L-1 and the session are T342's.
    /// </summary>
    Task<CredentialCheckResult> CheckCredentialsAsync(string email, string password, IPAddress? ipAddress, CancellationToken ct);

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
}
