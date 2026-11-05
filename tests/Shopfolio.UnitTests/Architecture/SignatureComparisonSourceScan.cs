using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the signature comparisons (T203 criterion 2, KK2; 08 3.2 principle 1; 09 11.1 item 5, 19.2 item 3): in the link layer and in
/// the signed endpoints' pipeline - where a signature is checked - a statement that names a signature (<c>hash</c>, <c>signature</c>,
/// <c>hmac</c>, <c>digest</c>, in any case and as part of a name) compares nothing with an operator or a method that can stop early: <c>==</c>,
/// <c>!=</c>, <c>Equals(</c>, <c>SequenceEqual(</c>, <c>Compare(</c>, <c>CompareOrdinal(</c>, <c>CompareTo(</c>, <c>StartsWith(</c>,
/// <c>EndsWith(</c>, <c>Contains(</c>, <c>IndexOf(</c> or a constant pattern (<c>is "..."</c>). The one fixed-time comparison is <c>SignatureComparison.Matches</c>, and its own file
/// is the scan's only exception: it is the file that calls <c>CryptographicOperations.FixedTimeEquals</c>. A statement runs from the end of the
/// previous one (<c>;</c>, <c>{</c>, <c>}</c>) to its own end, so a comparison written over several lines is one statement; full-line
/// comments are blanked first and keep their line. The scan reads text and writes nothing.
/// </summary>
internal static partial class SignatureComparisonSourceScan
{
    public const string EarlyExitComparison =
        "compares a signature with an operator or method that is not fixed time (08 3.2 principle 1: SignatureComparison.Matches only)";

    /// <summary>The folders where a signature is checked: the link layer and the signed endpoints' pipeline.</summary>
    public static readonly string[] ScannedFolders =
        ["src/Shopfolio.Web/Infrastructure/Integrations/", "src/Shopfolio.Web/Infrastructure/SignedEndpoints/"];

    /// <summary>The one file that compares signatures, in fixed time.</summary>
    public const string ComparisonFile = "src/Shopfolio.Web/Infrastructure/Integrations/SignatureComparison.cs";

    public static bool Applies(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.EndsWith(".cs", StringComparison.Ordinal)
            && ScannedFolders.Any(folder => path.StartsWith(folder, StringComparison.Ordinal))
            && !string.Equals(path, ComparisonFile, StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        string code = BlankCommentLines(text.ReplaceLineEndings("\n"));
        List<(string Rule, int Line)> findings = [];
        int start = 0;
        for (int i = 0; i <= code.Length; i++)
        {
            if (i < code.Length && code[i] is not (';' or '{' or '}'))
            {
                continue;
            }

            string statement = code[start..i];
            if (SignatureName().IsMatch(statement) && EarlyExit().IsMatch(statement))
            {
                int offset = start + (statement.Length - statement.TrimStart().Length);
                findings.Add((EarlyExitComparison, code.AsSpan(0, offset).Count('\n') + 1));
            }

            start = i + 1;
        }

        return findings;
    }

    private static string BlankCommentLines(string text)
    {
        string[] lines = text.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].TrimStart();
            if (trimmed.StartsWith("//", StringComparison.Ordinal) || trimmed.StartsWith("///", StringComparison.Ordinal))
            {
                lines[i] = string.Empty;
            }
        }

        return string.Join('\n', lines);
    }

    [GeneratedRegex(@"(?i:hash|signature|hmac|digest)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SignatureName();

    [GeneratedRegex(
        @"==|!=|\bEquals\s*\(|\bSequenceEqual\s*\(|\bCompare\s*\(|\bCompareOrdinal\s*\(|\bCompareTo\s*\(|\bStartsWith\s*\(|\bEndsWith\s*\(|\bContains\s*\(|\bIndexOf\s*\(|\bis\s+(?:not\s+)?""",
        RegexOptions.CultureInvariant,
        matchTimeoutMilliseconds: 5000)]
    private static partial Regex EarlyExit();
}
