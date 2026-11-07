using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure;

/// <summary>The three layers of a parameter (02 10.1.3; 09 3.5): who may change it and where it lives.</summary>
public enum ParameterLayer
{
    /// <summary>Changed from the panel, takes effect at once; lives in the company settings row (02 11.1).</summary>
    CompanySetting,

    /// <summary>Comes from the installation (settings file and secrets), changed by restarting the application; never changed from the panel.</summary>
    InstallationSetting,

    /// <summary>Fixed by the product, changed by a new version only (02 11.2); lives in <see cref="ProductConstants"/>.</summary>
    ProductConstant,
}

/// <summary>What a wrong value of a new parameter would harm (02 10.1.3): the three things that make it a product constant. A parameter that harms none of them harms only the company.</summary>
[Flags]
public enum ParameterHarm
{
    None = 0,
    Security = 1,
    LegalCompliance = 2,
    Customer = 4,
}

/// <summary>The assignment rule of the layers (02 10.1.3, K-412, K-08, K-478): the layer of a new parameter is read from the criterion, never argued.</summary>
internal static class ParameterLayers
{
    /// <summary>
    /// A value that comes from the installation is an installation setting. Otherwise the parameter is a firm setting only when its wrong value harms
    /// nothing but the company and the decision is a business decision; a harm to security, legal compliance or the customer - or a value that is not
    /// the company's business to decide - makes it a product constant.
    /// </summary>
    public static ParameterLayer Assign(bool comesFromInstallation, ParameterHarm harm, bool isBusinessDecision)
    {
        if (comesFromInstallation)
        {
            return ParameterLayer.InstallationSetting;
        }

        return harm == ParameterHarm.None && isBusinessDecision ? ParameterLayer.CompanySetting : ParameterLayer.ProductConstant;
    }
}

/// <summary>The unit a product constant is written in, so the equality check can compare it with the unit the inventory table names.</summary>
internal enum ParameterUnit
{
    Count,
    Characters,
    Minutes,
    Hours,
    Days,
    Months,
    Years,
    Bytes,
    CssPixels,
    PerDay,
    Text,

    /// <summary>A threshold or window of an attempt limit: its value is compared by the attempt limit table's own check.</summary>
    AttemptLimit,
}

/// <summary>One constant of <see cref="ProductConstants"/> and the unit its value is written in.</summary>
internal sealed record ParameterConstant(string Name, ParameterUnit Unit);

/// <summary>One row of the parameter inventory (02 11): its id, its layer - <see langword="null"/> for a removed row - and, for a product constant, the constants that carry its values in the order the table writes them.</summary>
internal sealed record ParameterEntry(string Id, ParameterLayer? Layer, IReadOnlyList<ParameterConstant> Constants)
{
    public bool IsRemoved => Layer is null;
}

/// <summary>One installation setting of 02 10.1.3 and the keys of the settings file or the secret files that carry it.</summary>
internal sealed record InstallationSettingEntry(string Name, IReadOnlyList<string> Keys, bool IsRequired);

/// <summary>
/// The parameter inventory of the code (02 11; 09 3.5 item 1; T381): every parameter id P-1 to P-49 with its layer, so the three layers and the removed rows
/// are one fact written once. The firm settings (02 11.1) carry their defaults in the start-up data and their fences in <see cref="FirmSettingFences"/>; the product
/// constants (02 11.2) carry their values in <see cref="ProductConstants"/>, named here; the installation settings are not numbered and are listed apart. The
/// equality of this inventory with the table of 02 11 is a <c>Consistency</c> test (09 21.4 item 1). P-2 and P-11 were removed and their ids are not reused (02 11.1).
/// </summary>
internal static class ParameterInventory
{
    private static ParameterEntry Firm(string id) => new(id, ParameterLayer.CompanySetting, []);

    private static ParameterEntry Removed(string id) => new(id, null, []);

    private static ParameterEntry Constant(string id, params ParameterConstant[] constants) => new(id, ParameterLayer.ProductConstant, constants);

    private static ParameterConstant C(string name, ParameterUnit unit) => new(name, unit);

    /// <summary>The 49 rows of 02 11, in the order of the table: the firm settings first, then the product constants.</summary>
    public static IReadOnlyList<ParameterEntry> Entries { get; } =
    [
        Firm("P-1"),
        Removed("P-2"),
        Firm("P-3"),
        Firm("P-4"),
        Firm("P-5"),
        Firm("P-6"),
        Firm("P-7"),
        Firm("P-8"),
        Firm("P-9"),
        Firm("P-10"),
        Removed("P-11"),
        Firm("P-44"),
        Constant("P-12", C(nameof(ProductConstants.EmailVerificationLinkHours), ParameterUnit.Hours)),
        Constant("P-13", C(nameof(ProductConstants.PasswordResetLinkMinutes), ParameterUnit.Minutes)),
        Constant("P-14", C(nameof(ProductConstants.AdminInvitationLinkHours), ParameterUnit.Hours)),
        Constant("P-47", C(nameof(ProductConstants.EmailChangeUndoLinkDays), ParameterUnit.Days)),
        Constant("P-15", C(nameof(ProductConstants.ShortSessionHours), ParameterUnit.Hours)),
        Constant("P-16", C(nameof(ProductConstants.LongSessionDays), ParameterUnit.Days)),
        Constant("P-48", C(nameof(ProductConstants.RecognizedBrowserMarkDays), ParameterUnit.Days)),
        Constant("P-17", C(nameof(ProductConstants.PasswordMinLength), ParameterUnit.Characters)),
        Constant("P-18", C(nameof(ProductConstants.CardPaymentMinutes), ParameterUnit.Minutes)),
        Constant("P-19", C(nameof(ProductConstants.OrderNumberRandomBlockLength), ParameterUnit.Characters)),
        Constant("P-20", C(nameof(ProductConstants.CategoryLevelMax), ParameterUnit.Count)),
        Constant("P-21", C(nameof(ProductConstants.OptionDimensionMax), ParameterUnit.Count)),
        Constant("P-22", C(nameof(ProductConstants.ImageFormats), ParameterUnit.Text)),
        Constant("P-23", C(nameof(ProductConstants.ImageMaxBytes), ParameterUnit.Bytes)),
        Constant("P-24", C(nameof(ProductConstants.ImagesPerProductMax), ParameterUnit.Count)),
        Constant("P-25", C(nameof(ProductConstants.ImagesPerContentRecordMax), ParameterUnit.Count)),
        Constant("P-26", C(nameof(ProductConstants.DigitalFileMaxBytes), ParameterUnit.Bytes)),
        Constant("P-27", C(nameof(ProductConstants.ProductNameMaxLength), ParameterUnit.Characters)),
        Constant("P-28", C(nameof(ProductConstants.ShortTextMaxLength), ParameterUnit.Characters)),
        Constant("P-29", C(nameof(ProductConstants.ShortIntroductionMaxLength), ParameterUnit.Characters)),
        Constant("P-30", C(nameof(ProductConstants.AnnouncementTextMaxLength), ParameterUnit.Characters)),
        Constant(
            "P-31",
            C(nameof(ProductConstants.SignInAttemptsPerEmail), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.SignInAttemptsPerIp), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.SignInAttemptWindowMinutes), ParameterUnit.AttemptLimit)),
        Constant(
            "P-32",
            C(nameof(ProductConstants.PasswordResetAttemptsPerEmail), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.PasswordResetAttemptsPerIp), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.PasswordResetAttemptWindowMinutes), ParameterUnit.AttemptLimit)),
        Constant(
            "P-33",
            C(nameof(ProductConstants.RegistrationAttemptsPerEmail), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.RegistrationAttemptsPerIp), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.RegistrationAttemptWindowMinutes), ParameterUnit.AttemptLimit)),
        Constant(
            "P-34",
            C(nameof(ProductConstants.ContactFormAttemptsPerIp), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.ContactFormAttemptWindowMinutes), ParameterUnit.AttemptLimit)),
        Constant(
            "P-35",
            C(nameof(ProductConstants.GuestOrderLookupAttemptsPerEmail), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.GuestOrderLookupAttemptsPerIp), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.GuestOrderLookupAttemptWindowMinutes), ParameterUnit.AttemptLimit)),
        Constant(
            "P-36",
            C(nameof(ProductConstants.InvalidCouponAttemptsPerCart), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.InvalidCouponAttemptWindowMinutes), ParameterUnit.AttemptLimit)),
        Constant(
            "P-37",
            C(nameof(ProductConstants.UnpaidOrderCancellationsMax), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.UnpaidOrderCancellationWindowHours), ParameterUnit.AttemptLimit)),
        Constant("P-46", C(nameof(ProductConstants.OpenUnpaidOrdersMax), ParameterUnit.AttemptLimit)),
        Constant(
            "P-49",
            C(nameof(ProductConstants.VerificationResendAttemptsPerEmail), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.VerificationResendAttemptsPerIp), ParameterUnit.AttemptLimit),
            C(nameof(ProductConstants.VerificationResendAttemptWindowMinutes), ParameterUnit.AttemptLimit)),
        Constant(
            "P-38",
            C(nameof(ProductConstants.DispatchFirstRetryMinutes), ParameterUnit.Minutes),
            C(nameof(ProductConstants.DispatchSecondRetryMinutes), ParameterUnit.Minutes),
            C(nameof(ProductConstants.DispatchThirdRetryMinutes), ParameterUnit.Minutes)),
        Constant("P-39", C(nameof(ProductConstants.DisposalIntervalDays), ParameterUnit.PerDay)),
        Constant(
            "P-40",
            C(nameof(ProductConstants.ContactRequestRetentionYears), ParameterUnit.Years),
            C(nameof(ProductConstants.OrderContactRequestRetentionYears), ParameterUnit.Years)),
        Constant("P-41", C(nameof(ProductConstants.DispatchRecordRetentionYears), ParameterUnit.Years)),
        Constant("P-43", C(nameof(ProductConstants.ReturnIbanRetentionDays), ParameterUnit.Days)),
        Constant("P-45", C(nameof(ProductConstants.SystemLogRetentionMonths), ParameterUnit.Months)),
        Constant("P-42", C(nameof(ProductConstants.NarrowestScreenCssPixels), ParameterUnit.CssPixels)),
    ];

    /// <summary>
    /// The installation settings of 02 10.1.3 (T381 criterion 3): the domain, the sender address, the e-mail sending identity, the payment provider's keys and the
    /// Google application's credentials. The first three are required (the application does not open without them), the last two are conditional (the line they open is closed
    /// without them; 05 8.4.6). They are keys of the settings file and the secret files, never a column of the company settings and never a panel field.
    /// </summary>
    public static IReadOnlyList<InstallationSettingEntry> InstallationSettings { get; } =
    [
        new("Domain", [SiteOptions.DomainKey], IsRequired: true),
        new("SenderAddress", [EmailOptions.SenderAddressKey], IsRequired: true),
        new("EmailSendingIdentity", [EmailOptions.RegionKey, EmailOptions.CredentialKey], IsRequired: true),
        new("PaymentProviderKeys", [PaymentOptions.ProviderKeysKey], IsRequired: false),
        new("GoogleApplication", [GoogleOptions.ClientIdKey, GoogleOptions.ClientSecretKey], IsRequired: false),
    ];

    /// <summary>The entry of an id; an id that is not in the inventory is a programming error.</summary>
    public static ParameterEntry Of(string id)
    {
        return Entries.SingleOrDefault(e => string.Equals(e.Id, id, StringComparison.Ordinal))
            ?? throw new ArgumentException($"{id} is not a row of the parameter inventory (02 11).", nameof(id));
    }
}
