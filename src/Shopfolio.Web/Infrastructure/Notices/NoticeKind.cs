using System.Collections.Frozen;

namespace Shopfolio.Infrastructure.Notices;

/// <summary>
/// The three lists a notice code belongs to (07 6.3; 11 T214 criterion 4): a short-lived notice shows at the head of the page for a short time and may repeat
/// when the page is reloaded (07 2.2.3, 2.2.8); a result state stays in the place of the form or the section (07 6.3.2); a reason is the reason of a return from
/// a customer's operation screen to the order page, which the order page says in the place of the operation (07 6.3.3).
/// </summary>
public enum NoticeKind
{
    /// <summary>07 6.3.1.</summary>
    ShortLived,

    /// <summary>07 6.3.2.</summary>
    Result,

    /// <summary>07 6.3.3.</summary>
    Reason,
}

/// <summary>The one table of <see cref="NoticeCode"/> to <see cref="NoticeKind"/>; a consistency test keeps it equal to the three tables of 07 6.3.</summary>
public static class NoticeKinds
{
    private static readonly FrozenDictionary<NoticeCode, NoticeKind> _kinds = new Dictionary<NoticeCode, NoticeKind>
    {
        [NoticeCode.CartItemAdded] = NoticeKind.ShortLived,
        [NoticeCode.Saved] = NoticeKind.ShortLived,
        [NoticeCode.InvitationSent] = NoticeKind.ShortLived,
        [NoticeCode.ContactRequestClosed] = NoticeKind.ShortLived,
        [NoticeCode.ContactRequestReopened] = NoticeKind.ShortLived,

        [NoticeCode.ContactRequestSent] = NoticeKind.Result,
        [NoticeCode.RegistrationSubmitted] = NoticeKind.Result,
        [NoticeCode.LinkResent] = NoticeKind.Result,
        [NoticeCode.PasswordResetRequested] = NoticeKind.Result,
        [NoticeCode.PasswordResetCompleted] = NoticeKind.Result,
        [NoticeCode.EmailVerified] = NoticeKind.Result,
        [NoticeCode.GoogleLinkConfirmed] = NoticeKind.Result,
        [NoticeCode.EmailChangeReverted] = NoticeKind.Result,
        [NoticeCode.PasswordChanged] = NoticeKind.Result,
        [NoticeCode.AdminAccountCreated] = NoticeKind.Result,
        [NoticeCode.CustomerDeleted] = NoticeKind.Result,
        [NoticeCode.SessionExpired] = NoticeKind.Result,
        [NoticeCode.SocialLoginEmailUnverified] = NoticeKind.Result,
        [NoticeCode.SocialLoginDataCollectionClosed] = NoticeKind.Result,
        [NoticeCode.SocialLoginLinkConfirmationSent] = NoticeKind.Result,
        [NoticeCode.SocialLoginFailed] = NoticeKind.Result,
        [NoticeCode.SocialReauthenticationFailed] = NoticeKind.Result,
        [NoticeCode.SocialReauthenticationUnavailable] = NoticeKind.Result,

        [NoticeCode.OperationClosed] = NoticeKind.Reason,
        [NoticeCode.QuantityExceedsOpen] = NoticeKind.Reason,
    }.ToFrozenDictionary();

    /// <summary>Every member that has a kind; a consistency test requires it to be every member of <see cref="NoticeCode"/>.</summary>
    public static IReadOnlyCollection<NoticeCode> Codes => _kinds.Keys;

    /// <summary>The list a code belongs to.</summary>
    public static NoticeKind Of(NoticeCode code) =>
        _kinds.TryGetValue(code, out NoticeKind kind) ? kind : throw new ArgumentOutOfRangeException(nameof(code), code, "A notice code has no kind.");

    /// <summary>The members of one list.</summary>
    public static IEnumerable<NoticeCode> Members(NoticeKind kind) => _kinds.Where(p => p.Value == kind).Select(p => p.Key);
}
