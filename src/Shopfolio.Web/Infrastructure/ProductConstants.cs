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

    /// <summary>P-27 — character ceiling of a product name (and of a name or title in the corporate content).</summary>
    public const int ProductNameMaxLength = 150;
}
