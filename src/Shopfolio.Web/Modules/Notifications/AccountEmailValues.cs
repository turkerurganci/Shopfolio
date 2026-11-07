using System.Globalization;
using System.Text.Json;

using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>What the dispatch row of an account e-mail is made of: its type, the recipient, the related record (06 4.58 table) and the event's values as the stored list.</summary>
internal sealed record AccountEmailRow(DispatchType Type, string Recipient, DispatchRelatedRecordType RelatedRecordType, Guid RelatedRecordId, string EventValues);

/// <summary>
/// The event's values of the seven account and administration e-mails (08 2.2.8; 06 4.58 <c>EventValues</c>; 05 4.4.1, 4.4.2): the same JSON list of name and
/// value pairs as the firm notices' (<see cref="CompanyNoticeValues"/>), written when the row is made and read when the body is made. A link e-mail's list holds
/// the entry <see cref="LinkEventValues.LinkRecordIdName"/> - the link record's id and never the key, which the body derives from the id (05 6.4.4; K-1081) - so the
/// start-up rewrite of the pending revert links finds it by the same name. The administrator's name is the invitation's value (the sender may be removed
/// before the body is made); the new address and the old address never enter a list (the recipient is the one address a row carries). The one writer lives here;
/// the reader is <see cref="CompanyNoticeValues.Read"/>.
/// </summary>
internal static class AccountEmailValues
{
    public const string AfterRevert = "AfterRevert";
    public const string SenderAdminName = "SenderAdminName";
    public const string OccurredAt = CompanyNoticeValues.OccurredAt;

    private const string Yes = "true";
    private const string No = "false";

    private sealed record Entry(string Name, string Value);

    /// <summary>The row of an account e-mail; a record that breaks the rule of its kind is refused.</summary>
    public static AccountEmailRow For(AccountEmail email)
    {
        ArgumentNullException.ThrowIfNull(email);
        RequireText(email.Recipient, nameof(email.Recipient));
        string recipient = email.Recipient.Trim();
        switch (email)
        {
            case VerificationLinkEmail verification:
                RequireIds(verification.CustomerId, verification.LinkRecordId);
                return new AccountEmailRow(DispatchType.EmailVerificationLink, recipient, DispatchRelatedRecordType.Customer, verification.CustomerId, Write((LinkEventValues.LinkRecordIdName, Key(verification.LinkRecordId))));
            case PasswordResetLinkEmail reset:
                RequireAccount(reset.AccountType, reset.AccountId, reset.LinkRecordId);
                return new AccountEmailRow(
                    DispatchType.PasswordResetLink,
                    recipient,
                    reset.AccountType,
                    reset.AccountId,
                    Write((LinkEventValues.LinkRecordIdName, Key(reset.LinkRecordId)), (AfterRevert, reset.AfterRevert ? Yes : No)));
            case NewAddressVerificationEmail newAddress:
                RequireAccount(newAddress.AccountType, newAddress.AccountId, newAddress.LinkRecordId);
                return new AccountEmailRow(DispatchType.NewEmailVerificationLink, recipient, newAddress.AccountType, newAddress.AccountId, Write((LinkEventValues.LinkRecordIdName, Key(newAddress.LinkRecordId))));
            case AddressChangeNoticeEmail notice:
                RequireAccount(notice.AccountType, notice.AccountId, notice.LinkRecordId);
                return new AccountEmailRow(
                    DispatchType.EmailChangeNotice,
                    recipient,
                    notice.AccountType,
                    notice.AccountId,
                    Write((LinkEventValues.LinkRecordIdName, Key(notice.LinkRecordId)), (OccurredAt, Moment(notice.ChangedAt))));
            case AdminInvitationEmail invitation:
                RequireIds(invitation.InvitationId, invitation.LinkRecordId);
                RequireText(invitation.SenderAdminName, nameof(invitation.SenderAdminName));
                return new AccountEmailRow(
                    DispatchType.AdminInvitation,
                    recipient,
                    DispatchRelatedRecordType.AdminInvitation,
                    invitation.InvitationId,
                    Write((LinkEventValues.LinkRecordIdName, Key(invitation.LinkRecordId)), (SenderAdminName, invitation.SenderAdminName)));
            case AccountDeletedEmail deleted:
                RequireIds(deleted.CustomerId, Guid.Empty, needsLink: false);
                return new AccountEmailRow(DispatchType.AccountDeletedNotice, recipient, DispatchRelatedRecordType.Customer, deleted.CustomerId, Write((OccurredAt, Moment(deleted.DeletedAt))));
            default:
                throw new ArgumentException($"{email.GetType().Name} is not an account e-mail.", nameof(email));
        }
    }

    /// <summary>The row of the Google sign-in confirmation link (customer accounts only).</summary>
    public static AccountEmailRow ForGoogleLink(Guid customerId, Guid linkRecordId, string recipient)
    {
        RequireIds(customerId, linkRecordId);
        RequireText(recipient, nameof(recipient));
        return new AccountEmailRow(DispatchType.GoogleLinkConfirmationLink, recipient.Trim(), DispatchRelatedRecordType.Customer, customerId, Write((LinkEventValues.LinkRecordIdName, Key(linkRecordId))));
    }

    /// <summary>A stored moment, or null when the text is not a moment as it is written.</summary>
    public static DateTimeOffset? ReadMoment(string? text) => CompanyNoticeValues.ReadMoment(text);

    /// <summary>True when the stored flag says yes; false when it says no; null when it is neither.</summary>
    public static bool? ReadFlag(string? text) => text is Yes ? true : text is No ? false : null;

    private static string Write(params (string Name, string Value)[] entries) =>
        JsonSerializer.Serialize<Entry[]>([.. entries.Select(e => new Entry(e.Name, e.Value))]);

    private static string Moment(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);

    private static string Key(Guid id) => id.ToString("D");

    private static void RequireText(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value of the e-mail is empty (08 2.2.8).", name);
        }
    }

    private static void RequireIds(Guid recordId, Guid linkRecordId, bool needsLink = true)
    {
        if (recordId == Guid.Empty || (needsLink && linkRecordId == Guid.Empty))
        {
            throw new ArgumentException("An account e-mail names its record and, when it carries a link, the link record (08 2.2.8).");
        }
    }

    private static void RequireAccount(DispatchRelatedRecordType accountType, Guid accountId, Guid linkRecordId)
    {
        if (accountType is not (DispatchRelatedRecordType.Customer or DispatchRelatedRecordType.Admin))
        {
            throw new ArgumentException("The account of this e-mail is a customer's or an administrator's (08 2.2.8).", nameof(accountType));
        }

        RequireIds(accountId, linkRecordId);
    }
}
