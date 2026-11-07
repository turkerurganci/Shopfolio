using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>The four queue paths of the module; every dispatch row is written through exactly one of them (08 2.2.6, 2.2.7, 2.2.8).</summary>
internal enum NoticeFamily
{
    /// <summary>B-1…B-16: the customer, at the order's contact e-mail.</summary>
    Customer,

    /// <summary>F-1…F-4: the firm, at the contact e-mail of its identity.</summary>
    Firm,

    /// <summary>F-5 and F-6: every administrator, at his or her own address.</summary>
    Administrators,

    /// <summary>The account and administration e-mails: the account's or the invitee's address.</summary>
    Account,
}

/// <summary>Whose address a notice is sent to, as the "Alıcı" and "Kanal" columns of 03 7.1 name it (03 7 "Adres"; 02 9.3.1).</summary>
internal enum NoticeAddressee
{
    /// <summary>The order's contact e-mail.</summary>
    OrderContact,

    /// <summary>The new address of a guest order's e-mail correction (B-1 again).</summary>
    CorrectedOrderContact,

    /// <summary>The address the order had before the correction (B-15).</summary>
    OrderContactBeforeCorrection,

    /// <summary>The order's current contact e-mail, on the re-sending of an undelivered notice (B-15: the address before the correction).</summary>
    CurrentOrderContact,

    /// <summary>The contact e-mail of the firm's identity.</summary>
    FirmContact,

    /// <summary>The own address of every administrator account.</summary>
    EachAdministrator,

    /// <summary>The address the registration was made with.</summary>
    RegisteredAddress,

    /// <summary>The account's address.</summary>
    AccountAddress,

    /// <summary>The account's address - the old one when the request is born of an e-mail change being reverted.</summary>
    AccountAddressOrOldOnRevert,

    /// <summary>The new address of an account's e-mail change.</summary>
    NewAddress,

    /// <summary>The old address of an account's e-mail change.</summary>
    OldAccountAddress,

    /// <summary>The address an administrator invitation was sent to.</summary>
    InviteeAddress,

    /// <summary>The address of the account that was deleted.</summary>
    DeletedAccountAddress,
}

/// <summary>
/// A row of 03 7.1: the one trigger a notice is sent from. <paramref name="RowId"/> is the document's row id ("7.1.7"); <paramref name="Type"/> is the notice's
/// <see cref="DispatchType"/> - null on the row that re-sends any of B-1…B-16 (7.1.37).
/// </summary>
internal sealed record NoticeTrigger(string RowId, DispatchType? Type, NoticeAddressee Addressee);

/// <summary>A row of 03 7.2: an event that makes no notice, or a notice that does not exist, by its row id and the sources the row names ("02 §9.2", "K-712").</summary>
internal sealed record NoNoticeRow(string RowId, IReadOnlyList<string> Sources);

/// <summary>A row of 03 7.3: a state transition or an event by its row id and the rows of 03 7.1 and 7.2 that it makes or does not make.</summary>
internal sealed record EventMapping(string RowId, IReadOnlyList<string> Rows);

/// <summary>
/// The notification map as code (T328; 03 7; 02 9.5): the one list of triggers a notice is sent from. A dispatch row is written only through one of the four queues, each of
/// which asks <see cref="Require"/> first, so no e-mail is queued for a type that has no trigger row or through a path that is not its own (03 7.1, 02 9.5). The rows
/// themselves are in <c>NotificationMap.Rows.cs</c>; the consistency tests compare them with 03 7.1, 7.2 and 7.3 in both directions (09 21.4 item 1).
/// </summary>
internal static partial class NotificationMap
{
    /// <summary>The queue path of a type: B-1…B-16 the customer's, F-1…F-4 the firm's, F-5 and F-6 the administrators', the rest the account's. A type that is not named here is refused.</summary>
    public static NoticeFamily FamilyOf(DispatchType type) => type switch
    {
        DispatchType.B1 or DispatchType.B2 or DispatchType.B3 or DispatchType.B4 or DispatchType.B5 or DispatchType.B6 or DispatchType.B7 or DispatchType.B8
            or DispatchType.B9 or DispatchType.B10 or DispatchType.B11 or DispatchType.B12 or DispatchType.B13 or DispatchType.B14 or DispatchType.B15 or DispatchType.B16
            => NoticeFamily.Customer,
        DispatchType.F1 or DispatchType.F2 or DispatchType.F3 or DispatchType.F4 => NoticeFamily.Firm,
        DispatchType.F5 or DispatchType.F6 => NoticeFamily.Administrators,
        DispatchType.EmailVerificationLink or DispatchType.PasswordResetLink or DispatchType.NewEmailVerificationLink or DispatchType.EmailChangeNotice
            or DispatchType.AdminInvitation or DispatchType.AccountDeletedNotice or DispatchType.GoogleLinkConfirmationLink
            => NoticeFamily.Account,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "A dispatch type that is not in the notification map has no queue path (03 7.1; 02 9.5)."),
    };

    /// <summary>Refuses a type that has no trigger row in 03 7.1 or that is queued through another family's path.</summary>
    public static void Require(DispatchType type, NoticeFamily family)
    {
        if (!Triggers.Any(t => t.Type == type))
        {
            throw new InvalidOperationException($"{type} has no trigger row in the notification map; no e-mail is queued outside it (03 7.1; 02 9.5).");
        }

        NoticeFamily owner = FamilyOf(type);
        if (owner != family)
        {
            throw new InvalidOperationException($"{type} is a notice of the {owner} path and is not queued through the {family} path (03 7; 02 9.3.1).");
        }
    }

    /// <summary>The addressee kinds the map names for a type, one for each of its trigger rows, without repeats.</summary>
    public static IReadOnlyList<NoticeAddressee> AddresseesOf(DispatchType type) =>
        [.. Triggers.Where(t => t.Type == type).Select(t => t.Addressee).Distinct()];
}
