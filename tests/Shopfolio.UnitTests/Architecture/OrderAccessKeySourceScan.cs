using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The scan of who writes an order's access key (T375; D-567; 02 10.4.11, 8.3.5): the key is written by the order's own methods in <c>Order.cs</c> (the first value, the disposal's
/// emptying and the renewal), and the renewal is called by one file only - the administrator's correction of a guest order's e-mail (<c>OrderCorrections.cs</c>). Any other assignment
/// of the property, a bulk update of it or a call of the renewal is a finding: a key that changes anywhere else would kill the links of an order without the firm's confirmation.
/// </summary>
internal static partial class OrderAccessKeySourceScan
{
    public const string OrderFile = "src/Shopfolio.Web/Modules/Ordering/Order.cs";

    public const string CorrectionsFile = "src/Shopfolio.Web/Modules/Ordering/OrderCorrections.cs";

    public const string KeyWrite = "writes the order's access key outside Order.cs (the key is the order's own: first value, disposal, renewal)";

    public const string RenewalCall = "calls the access key's renewal outside OrderCorrections.cs (only the firm's confirmed e-mail correction renews it)";

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.StartsWith("src/", StringComparison.Ordinal) && path.EndsWith(".cs", StringComparison.Ordinal)
            && !path.Contains("/obj/", StringComparison.Ordinal) && !path.Contains("/bin/", StringComparison.Ordinal)
            && !path.Contains("/Infrastructure/Migrations/", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();
            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith('*') || trimmed.StartsWith("/*", StringComparison.Ordinal))
            {
                continue;
            }

            if (path != OrderFile && Write().IsMatch(trimmed))
            {
                findings.Add((KeyWrite, i + 1));
            }

            if (path != CorrectionsFile && path != OrderFile && Renewal().IsMatch(trimmed))
            {
                findings.Add((RenewalCall, i + 1));
            }
        }

        return findings;
    }

    // An assignment of a property named OrderAccessKey (not a comparison), a bulk update of it and the column's own name in a statement.
    [GeneratedRegex(@"\bOrderAccessKey\s*=(?![=>])|SetProperty\(\s*\w+\s*=>\s*\w+\.OrderAccessKey\b|\border_access_key\s*=", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Write();

    [GeneratedRegex(@"\bRenewAccessKey\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Renewal();
}
