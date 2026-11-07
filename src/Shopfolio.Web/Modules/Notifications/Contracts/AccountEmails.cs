namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>
/// An account or administration e-mail (08 2.2.8; 02 9.4): one of seven closed kinds, each a record of the event's values. A link e-mail names its link
/// record by id and never carries the key: the key is derived from the id when the body is made (05 4.4.2, 6.4.4; K-1081). <see cref="Recipient"/> is the
/// address the row is sent to, written as it was entered. The text is the same for a customer and an administrator account; the account's type
/// (<see cref="DispatchRelatedRecordType.Customer"/> or <see cref="DispatchRelatedRecordType.Admin"/>) only picks the path of the link.
/// </summary>
public abstract record AccountEmail(string Recipient);

/// <summary>E-mail verification link (08 2.2.8): sent at registration and when the user asks for the link again; the account is a customer's.</summary>
public sealed record VerificationLinkEmail(Guid CustomerId, Guid LinkRecordId, string Recipient) : AccountEmail(Recipient);

/// <summary>
/// Password reset link (08 2.2.8): the account is a customer's or an administrator's (<paramref name="AccountType"/>). <paramref name="AfterRevert"/> is true when
/// the request was born of an e-mail change being reverted (the address is the old one and an extra sentence is written).
/// </summary>
public sealed record PasswordResetLinkEmail(DispatchRelatedRecordType AccountType, Guid AccountId, Guid LinkRecordId, string Recipient, bool AfterRevert) : AccountEmail(Recipient);

/// <summary>Verification of the new address (08 2.2.8): <paramref name="Recipient"/> is the NEW address; the old address is never written.</summary>
public sealed record NewAddressVerificationEmail(DispatchRelatedRecordType AccountType, Guid AccountId, Guid LinkRecordId, string Recipient) : AccountEmail(Recipient);

/// <summary>
/// Notice of the change to the OLD address (08 2.2.8): <paramref name="Recipient"/> is the old address and the new address is not written. The link record holds the
/// previous address and is the revert link; <paramref name="ChangedAt"/> is the moment the change took effect, which is also when the link's life starts.
/// </summary>
public sealed record AddressChangeNoticeEmail(DispatchRelatedRecordType AccountType, Guid AccountId, Guid LinkRecordId, string Recipient, DateTimeOffset ChangedAt) : AccountEmail(Recipient);

/// <summary>
/// Administrator invitation (08 2.2.8): <paramref name="Recipient"/> is the invited address (not the firm's contact e-mail); <paramref name="SenderAdminName"/> is the
/// inviting administrator's name as a value (the sender may be removed before the body is made). The invitation is the dispatch row's related record.
/// </summary>
public sealed record AdminInvitationEmail(Guid InvitationId, Guid LinkRecordId, string Recipient, string SenderAdminName) : AccountEmail(Recipient);

/// <summary>
/// Notice that the account was deleted (08 2.2.8): <paramref name="Recipient"/> is the deleted account's address, which the row keeps as a value although the account
/// is gone. It has no link, no button and no revert path; <paramref name="DeletedAt"/> is the deletion's moment.
/// </summary>
public sealed record AccountDeletedEmail(Guid CustomerId, string Recipient, DateTimeOffset DeletedAt) : AccountEmail(Recipient);

