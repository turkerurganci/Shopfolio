using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the code that counts attempts (T376 criterion 2, KK2; 09 3.5 item 1, 15.4 item 5): no threshold and no window is written in it. Every number of a
/// limit is a product constant (<c>ProductConstants</c>, 02 11.2), so the code of the counter, the table of the limits, the inventory and the order count holds no
/// integer literal but 0 and 1 (a "first" or an "at least one"). Comments and string literals are dropped first, so a row id such as "P-31" in a string is not a number.
/// </summary>
internal static partial class AttemptNumberScan
{
    public const string NumberWritten = "writes a number; the threshold or the window is a product constant";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string withoutBlocks = BlockComment().Replace(text.ReplaceLineEndings("\n"), match => new string('\n', match.Value.Count(c => c == '\n')));
        string[] lines = withoutBlocks.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string code = StringLiteral().Replace(LineComment().Replace(lines[i], string.Empty), "\"\"");
            foreach (Match number in IntegerLiteral().Matches(code))
            {
                if (number.Value is not ("0" or "1"))
                {
                    findings.Add((NumberWritten, i + 1));
                    break;
                }
            }
        }

        return findings;
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BlockComment();

    [GeneratedRegex(@"//.*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex LineComment();

    [GeneratedRegex("\\$?@?\"(?:[^\"\\\\]|\\\\.)*\"", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex StringLiteral();

    [GeneratedRegex(@"(?<![\w.])(?:0[xXbB][0-9a-fA-F_]+|[0-9][0-9_]*(?:\.[0-9_]+)?)[mMdDfFuUlL]*(?![\w])", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IntegerLiteral();
}
