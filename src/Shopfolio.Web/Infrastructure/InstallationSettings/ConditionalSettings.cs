namespace Shopfolio.Infrastructure.InstallationSettings;

/// <summary>
/// The payment provider's keys (05 8.4.3, 8.4.6; 08 2.1.2): a conditional installation setting. The keys are the whole content of the secret
/// file <c>payment_provider_keys</c>. When they are missing the application opens and the card payment line is closed.
/// </summary>
internal sealed class PaymentOptions
{
    public const string SectionName = "Payment";

    public const string ProviderKeysKey = SectionName + ":" + nameof(ProviderKeys);

    /// <summary>The provider's keys; they come from a secret file only.</summary>
    public string ProviderKeys { get; set; } = string.Empty;
}

/// <summary>
/// The Google application of the installation (05 8.4.3, 8.4.6; 08 2.3): a conditional installation setting. The client id is in the
/// settings file, the client secret is the whole content of the secret file <c>google_client_secret</c>. Both must be present; with either
/// one missing the application opens and the Google sign-in line is closed.
/// </summary>
internal sealed class GoogleOptions
{
    public const string SectionName = "Google";

    public const string ClientIdKey = SectionName + ":" + nameof(ClientId);

    public const string ClientSecretKey = SectionName + ":" + nameof(ClientSecret);

    public string ClientId { get; set; } = string.Empty;

    /// <summary>The client secret; it comes from a secret file only.</summary>
    public string ClientSecret { get; set; } = string.Empty;
}

/// <summary>
/// The state of one line that a conditional setting opens (05 8.4.6): open when all its settings are present, otherwise closed with the keys
/// that are missing. The start-up check writes a closed line to the log; the status command shows it (T389 item 6).
/// </summary>
internal sealed record ConditionalLine(string Name, IReadOnlyList<string> MissingKeys)
{
    public const string CardPayment = "CardPayment";

    public const string GoogleSignIn = "GoogleSignIn";

    public bool IsOpen => MissingKeys.Count == 0;
}

/// <summary>The conditional lines of an installation, worked out from its settings (05 8.4.6).</summary>
internal static class ConditionalLines
{
    public static IReadOnlyList<ConditionalLine> Evaluate(PaymentOptions payment, GoogleOptions google)
    {
        return
        [
            new ConditionalLine(ConditionalLine.CardPayment, MissingOf((PaymentOptions.ProviderKeysKey, payment.ProviderKeys))),
            new ConditionalLine(
                ConditionalLine.GoogleSignIn,
                MissingOf((GoogleOptions.ClientIdKey, google.ClientId), (GoogleOptions.ClientSecretKey, google.ClientSecret))),
        ];
    }

    private static string[] MissingOf(params (string Key, string Value)[] settings)
    {
        return [.. settings.Where(s => string.IsNullOrWhiteSpace(s.Value)).Select(s => s.Key)];
    }
}
