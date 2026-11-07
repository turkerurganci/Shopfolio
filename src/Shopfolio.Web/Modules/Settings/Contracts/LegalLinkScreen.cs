namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// A screen on which the privacy notice link and the cookie policy link are shown (02 3.33.5). A closed list, not a stored value: no consent box and no "seen" record exist, so
/// nothing of it reaches the data model.
/// </summary>
public sealed record LegalLinkScreen(string Name)
{
    public static LegalLinkScreen AccountRegistration { get; } = new(nameof(AccountRegistration));

    public static LegalLinkScreen GoogleSignIn { get; } = new(nameof(GoogleSignIn));

    public static LegalLinkScreen OrderConfirmationStep { get; } = new(nameof(OrderConfirmationStep));

    public static LegalLinkScreen CancellationIbanEntry { get; } = new(nameof(CancellationIbanEntry));

    public static LegalLinkScreen DelayTerminationIbanEntry { get; } = new(nameof(DelayTerminationIbanEntry));

    public static LegalLinkScreen WithdrawalIbanEntry { get; } = new(nameof(WithdrawalIbanEntry));

    public static LegalLinkScreen OrderPageIbanField { get; } = new(nameof(OrderPageIbanField));

    public static LegalLinkScreen DefectRequest { get; } = new(nameof(DefectRequest));

    public static LegalLinkScreen ContactForm { get; } = new(nameof(ContactForm));

    public static LegalLinkScreen Footer { get; } = new(nameof(Footer));

    /// <summary>Every screen, the footer last.</summary>
    public static IReadOnlyList<LegalLinkScreen> All { get; } =
    [
        AccountRegistration, GoogleSignIn, OrderConfirmationStep, CancellationIbanEntry, DelayTerminationIbanEntry, WithdrawalIbanEntry, OrderPageIbanField, DefectRequest, ContactForm, Footer,
    ];
}
