namespace Shopfolio.Infrastructure.Notices;

/// <summary>
/// The closed list of the <c>notice</c> parameter (07 6.3; 09 5.3 item 6, 8.2 item 1): one member for every code of the tables of 07 6.3.1-6.3.3, spelled
/// as the document spells it. A code is an English PascalCase name, is unique and does not change once given; the number of a member means nothing - the
/// code travels in the address by its name, so a member is never renumbered and never removed (07 2.7.3, 6.1.1). A new code is written to 07 6.3 first and
/// added here second; the lock file test and the consistency test keep the three lists equal. The codes of the three kinds are three lists
/// (<see cref="NoticeKind"/>, <see cref="NoticeKinds"/>): a short-lived notice, a result state and the reason of a return are not read in one another's place.
/// The error codes are another enum (<c>ErrorCode</c>) and never travel in an address (09 8.2 item 4).
/// </summary>
public enum NoticeCode
{
    // 07 6.3.1 - short-lived notices
    CartItemAdded,
    Saved,
    InvitationSent,
    ContactRequestClosed,
    ContactRequestReopened,

    // 07 6.3.2 - result states
    ContactRequestSent,
    RegistrationSubmitted,
    LinkResent,
    PasswordResetRequested,
    PasswordResetCompleted,
    EmailVerified,
    GoogleLinkConfirmed,
    EmailChangeReverted,
    PasswordChanged,
    AdminAccountCreated,
    CustomerDeleted,
    SessionExpired,
    SocialLoginEmailUnverified,
    SocialLoginDataCollectionClosed,
    SocialLoginLinkConfirmationSent,
    SocialLoginFailed,
    SocialReauthenticationFailed,
    SocialReauthenticationUnavailable,

    // 07 6.3.3 - the reason of a return to the order page
    OperationClosed,
    QuantityExceedsOpen,
}
