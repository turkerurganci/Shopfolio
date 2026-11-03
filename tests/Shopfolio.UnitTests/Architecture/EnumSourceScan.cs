using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the enum files (T102 criterion 6, KK6; 06 5.1.3): a file that declares an enum holds the enum and nothing else —
/// its code names, bare, without a number. The behavior of a value (which transition is allowed, which reason is absolute) lives in the
/// rule that owns it, so an enum file has no method, no second type, no explicit value and no attribute. The scan is line based: after the
/// comment lines are dropped, every line is the namespace, the enum head, a brace, a bare member or blank.
/// </summary>
internal static partial class EnumSourceScan
{
    public const string ForeignLine = "has a line that is not the namespace, the enum head, a brace or a bare code name";

    public const string WrongFileName = "is named differently from the enum it declares";

    /// <summary>The scan applies to a file that declares an enum under a module's <c>Contracts</c> folder or the infrastructure's <c>Values</c> folder.</summary>
    public static bool Applies(string relativePath, string text)
    {
        string path = relativePath.Replace('\\', '/');
        bool inScope = (path.Contains("/Modules/", StringComparison.Ordinal) && path.Contains("/Contracts/", StringComparison.Ordinal))
            || path.Contains("/Infrastructure/Values/", StringComparison.Ordinal);
        return inScope && EnumHead().IsMatch(text);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string fileName, string text)
    {
        List<(string Rule, int Line)> findings = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string trimmed = lines[i].Trim();
            if (trimmed.Length == 0 || trimmed.StartsWith("//", StringComparison.Ordinal))
            {
                continue;
            }

            if (!AllowedLine().IsMatch(trimmed))
            {
                findings.Add((ForeignLine, i + 1));
            }
        }

        Match head = EnumHead().Match(text);
        if (head.Success && head.Groups[1].Value + ".cs" != fileName)
        {
            findings.Add((WrongFileName, 1));
        }

        return findings;
    }

    [GeneratedRegex(@"^public enum (\w+)\s*$", RegexOptions.Multiline)]
    private static partial Regex EnumHead();

    [GeneratedRegex(@"^(namespace [\w.]+;|public enum \w+|\{|\}|[A-Z][A-Za-z0-9]*,)$")]
    private static partial Regex AllowedLine();
}
