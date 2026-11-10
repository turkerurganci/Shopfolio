using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Scans for the payment provider's keys outside an installation's secret file (T406 KK3, KK4; 08 2.1.10: a firm's keys never reach development or CI and CI does not
/// connect to the provider; 05 8.1.2; 08 2.1.3: without the provider's written 3D confirmation no card keys are defined). Two rules, each a pure function over a file's text:
/// <list type="bullet">
/// <item><see cref="KeyValue"/>: a tracked settings, deploy or script file gives the setting (<c>Payment:ProviderKeys</c>, <c>Payment__ProviderKeys</c>, a JSON
/// <c>"ProviderKeys"</c>) or one of its secret lines (<c>merchant_key</c>, <c>merchant_salt</c>) a value. Naming the secret file (a Compose secrets list) is not a value.</item>
/// <item><see cref="CiMention"/>: a CI file (the workflows, the CI command files) names the provider or its keys at all - CI has no keys and makes no call to the provider.</item>
/// </list>
/// </summary>
internal static partial class PaymentKeysScan
{
    public const string KeyValue = "gives the payment provider's keys a value outside the installation's secret file";

    public const string CiMention = "CI names the payment provider or its keys";

    /// <summary>Findings of the value rule: the rule and the 1-based line.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanForKeyValues(string text) => Scan(text, KeyValuePattern(), KeyValue);

    /// <summary>Findings of the CI rule: the rule and the 1-based line.</summary>
    public static IReadOnlyList<(string Rule, int Line)> ScanCiFile(string text) => Scan(text, CiMentionPattern(), CiMention);

    private static List<(string Rule, int Line)> Scan(string text, Regex pattern, string rule)
    {
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        List<(string Rule, int Line)> findings = [];
        for (int i = 0; i < lines.Length; i++)
        {
            if (pattern.IsMatch(lines[i]))
            {
                findings.Add((rule, i + 1));
            }
        }

        return findings;
    }

    // A value after the name: "=" or ":" and then anything but the end of the line - both spellings of the setting's key and the two secret lines of the file's
    // content (08 2.1.2), a quoted name ("merchant_key"=...) too - or a JSON "ProviderKeys" with a non-empty string. Whatever the case.
    [GeneratedRegex(
        """(?:\bPayment(?::|__)ProviderKeys|\bmerchant_(?:key|salt))["']?\s*[=:]\s*\S|"ProviderKeys"\s*:\s*"[^"]""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex KeyValuePattern();

    [GeneratedRegex("""paytr|payment_provider_keys|ProviderKeys|merchant_(?:id|key|salt)""", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant)]
    private static partial Regex CiMentionPattern();
}
