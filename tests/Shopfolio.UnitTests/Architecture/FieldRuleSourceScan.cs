using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The source scan of the input validation pattern (09 3.4 items 1 and 6; 09 19.2): a limit on a rule attribute is a product constant and never a number
/// written on the attribute, and the framework's client-side validation output stays off. The scan reads a file's text line by line, skips comment lines,
/// and names each hit by rule and line.
/// </summary>
internal static partial class FieldRuleSourceScan
{
    public const string NumberOnLimitRule = "NumberOnLimitRule";
    public const string ClientValidationOn = "ClientValidationOn";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        List<(string, int)> findings = [];
        string[] lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.TrimStart().StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (NumberOnLimit().IsMatch(line))
            {
                findings.Add((NumberOnLimitRule, i + 1));
            }

            if (ClientValidation().IsMatch(line))
            {
                findings.Add((ClientValidationOn, i + 1));
            }
        }

        return findings;
    }

    // [TextTooLong(150)], [InvalidQuantity(1000)], [InvalidAmount(Max = 99999)] - also second in a list ([FieldRequired, TextTooLong(150)]), with the
    // Attribute suffix (TextTooLongAttribute(150)) and with a named argument (TextTooLong(max: 150)).
    [GeneratedRegex(@"\b(TextTooLong|InvalidQuantity)(Attribute)?\s*\(\s*(\w+\s*:\s*)?\d|\bInvalidAmount(Attribute)?\s*\([^)\]]*Max\s*=\s*\d")]
    private static partial Regex NumberOnLimit();

    // The framework's own output: data-val attributes, the switch turned on, the validation scripts.
    [GeneratedRegex(@"data-val|ClientValidationEnabled\s*=\s*true|jquery[.-]validat|jquery[.-]unobtrusive", RegexOptions.IgnoreCase)]
    private static partial Regex ClientValidation();
}
