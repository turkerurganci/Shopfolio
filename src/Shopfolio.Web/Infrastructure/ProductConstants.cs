namespace Shopfolio.Infrastructure;

/// <summary>
/// The product constants (02 11.2; 09 3.5 item 1): numbers the product fixes and no setting can change. Each constant carries its inventory
/// id and a number is written once, here; the check constraints of the model read it.
/// </summary>
internal static class ProductConstants
{
    /// <summary>P-20 — depth of the category tree, in levels.</summary>
    public const int CategoryLevelMax = 3;

    /// <summary>P-21 — option dimensions of a product.</summary>
    public const int OptionDimensionMax = 2;

    /// <summary>P-12 — lifetime of the e-mail verification link, in hours from the first link of the registration (Z-1; 02 3.13.5).</summary>
    public const int VerificationLinkLifetimeHours = 24;

    /// <summary>P-15 — short session (no "remember me"): hours from the last operation (Z-4).</summary>
    public const int ShortSessionHours = 2;

    /// <summary>P-16 — long session ("remember me"): days from the last operation (Z-5).</summary>
    public const int LongSessionDays = 30;

    /// <summary>P-31 — window of the failed sign-in attempts limit L1, in minutes (Z-27).</summary>
    public const int SignInAttemptWindowMinutes = 15;

    /// <summary>P-32 — window of the password reset request limit L2, in minutes (Z-27).</summary>
    public const int PasswordResetAttemptWindowMinutes = 60;

    /// <summary>P-33 — window of the account registration limit L3, in minutes (Z-27).</summary>
    public const int RegistrationAttemptWindowMinutes = 60;

    /// <summary>P-34 — window of the contact form limit L4, in minutes (Z-27).</summary>
    public const int ContactFormAttemptWindowMinutes = 60;

    /// <summary>P-35 — window of the failed guest order lookup limit L5, in minutes (Z-27).</summary>
    public const int GuestOrderLookupAttemptWindowMinutes = 15;

    /// <summary>P-36 — window of the invalid coupon code limit L6, in minutes (Z-27).</summary>
    public const int InvalidCouponAttemptWindowMinutes = 15;

    /// <summary>P-31 — failed sign-in attempts the e-mail axis allows in the window; the recognized browser's counter per account and browser carries the same number (02 8.2.6).</summary>
    public const int SignInAttemptsPerEmail = 5;

    /// <summary>P-31 — failed sign-in attempts the IP axis allows in the window.</summary>
    public const int SignInAttemptsPerIp = 20;

    /// <summary>P-32 — password reset requests the e-mail axis allows in the window.</summary>
    public const int PasswordResetAttemptsPerEmail = 3;

    /// <summary>P-32 — password reset requests the IP axis allows in the window.</summary>
    public const int PasswordResetAttemptsPerIp = 10;

    /// <summary>P-33 — account registrations the e-mail axis allows in the window.</summary>
    public const int RegistrationAttemptsPerEmail = 3;

    /// <summary>P-33 — account registrations the IP axis allows in the window.</summary>
    public const int RegistrationAttemptsPerIp = 5;

    /// <summary>P-34 — contact form submissions the IP axis allows in the window.</summary>
    public const int ContactFormAttemptsPerIp = 5;

    /// <summary>P-35 — failed guest order lookups the e-mail axis allows in the window.</summary>
    public const int GuestOrderLookupAttemptsPerEmail = 5;

    /// <summary>P-35 — failed guest order lookups the IP axis allows in the window.</summary>
    public const int GuestOrderLookupAttemptsPerIp = 10;

    /// <summary>P-36 — invalid coupon code attempts the cart axis allows in the window.</summary>
    public const int InvalidCouponAttemptsPerCart = 5;

    /// <summary>P-37 — unpaid orders cancelled on their own that one e-mail (a signed-in member's orders only) or one IP may have in the window (L7; counted from the orders).</summary>
    public const int UnpaidOrderCancellationsMax = 5;

    /// <summary>P-37 — window of L7, in hours.</summary>
    public const int UnpaidOrderCancellationWindowHours = 24;

    /// <summary>P-46 — open unpaid orders that one e-mail (a signed-in member's orders only) or one IP may have at the same time (L8; no window; counted from the orders).</summary>
    public const int OpenUnpaidOrdersMax = 3;

    /// <summary>P-49 — verification link requests the e-mail axis allows in the window (the values of L-2).</summary>
    public const int VerificationResendAttemptsPerEmail = 3;

    /// <summary>P-49 — verification link requests the IP axis allows in the window.</summary>
    public const int VerificationResendAttemptsPerIp = 10;

    /// <summary>P-38 — first retry of an e-mail that could not be sent, in minutes from the first attempt (Z-26; <c>DispatchRecord.NextAttemptAt</c>).</summary>
    public const int DispatchFirstRetryMinutes = 5;

    /// <summary>P-38 — second retry of an e-mail that could not be sent, in minutes from the first attempt (Z-26; <c>DispatchRecord.NextAttemptAt</c>).</summary>
    public const int DispatchSecondRetryMinutes = 30;

    /// <summary>P-38 — third and last retry of an e-mail that could not be sent, in minutes from the first attempt (2 hours; Z-26; <c>DispatchRecord.NextAttemptAt</c>).</summary>
    public const int DispatchThirdRetryMinutes = 120;

    /// <summary>P-45 — retention of the sign-in record and of the system logs, in calendar months from the moment the record is made (Z-40).</summary>
    public const int SystemLogRetentionMonths = 6;

    /// <summary>P-49 — window of the verification link request limit L9, in minutes (Z-27).</summary>
    public const int VerificationResendAttemptWindowMinutes = 60;

    /// <summary>P-12 — life of the e-mail verification link, in hours (Z-1); also the life of the new address verification link (the change request's moment, Z-1).</summary>
    public const int EmailVerificationLinkHours = 24;

    /// <summary>P-13 — life of the password reset link, in minutes (Z-2); the Google sign-in confirmation link has the same life.</summary>
    public const int PasswordResetLinkMinutes = 60;

    /// <summary>P-14 — life of the administrator invitation link, in hours (Z-3).</summary>
    public const int AdminInvitationLinkHours = 72;

    /// <summary>P-47 — life of the e-mail change revert link, in days (Z-45).</summary>
    public const int EmailChangeRevertLinkDays = 7;

    /// <summary>P-27 — character ceiling of a product name (and of a name or title in the corporate content).</summary>
    public const int ProductNameMaxLength = 150;

    /// <summary>
    /// P-28 — character ceiling of a short description, an FAQ question and a branch's working hours; the page description of the search engines is cut at
    /// the same ceiling (02 3.30.5).
    /// </summary>
    public const int ShortTextMaxLength = 200;

    /// <summary>P-23 — size ceiling of one image file, in bytes: 10 MB (05 6.6.1).</summary>
    public const long ImageMaxBytes = 10L * 1024 * 1024;

    /// <summary>P-26 — size ceiling of a digital product file, in bytes: 500 MB (05 6.6.3).</summary>
    public const long DigitalFileMaxBytes = 500L * 1024 * 1024;
}
