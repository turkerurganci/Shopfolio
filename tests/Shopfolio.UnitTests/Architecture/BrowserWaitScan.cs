using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan of the browser tests for the fixed-length wait (D-1704; 09 19.2 item 6: a test does not wait with <c>Task.Delay</c>). A browser test waits for
/// what it needs - a navigation (<c>BrowserTestBase.NavigateAsync</c>), a load state, an assertion with its own time limit (<c>Assertions.Expect(...)</c>) -
/// never for a length of time. The scan reads the browser folder with no exception: a fixed wait there is a finding. It sees the four spellings a wait has:
/// <c>Task.Delay</c> (with the namespace in front, a line break after the dot, or the bare <c>Delay(</c> that a static using lets it be), <c>Thread.Sleep</c>
/// (and the bare <c>Sleep(</c>), the page's <c>WaitForTimeoutAsync</c>, and the in-page <c>setTimeout</c> a script string could carry. Comments and string
/// literals are dropped first, except that a string's <c>setTimeout</c> is looked for before it is dropped (an in-page script is a string).
/// </summary>
internal static partial class BrowserWaitScan
{
    public const string FixedWait = "waits for a length of time; wait for a navigation, a load state or an assertion instead (09 19.2 item 6)";

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        List<(string Rule, int Line)> findings = [];
        string normalized = BlockComment().Replace(text.ReplaceLineEndings("\n"), match => new string('\n', match.Value.Count(c => c == '\n')));
        string[] lines = normalized.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string noComment = LineComment().Replace(lines[i], string.Empty);
            string code = StringLiteral().Replace(noComment, "\"\"");
            if (CodeWait().IsMatch(code) || InPageTimer().IsMatch(noComment))
            {
                findings.Add((FixedWait, i + 1));
            }
        }

        return findings;
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex BlockComment();

    // A line comment is a "//" outside a string: what follows it up to the line end holds only whole strings (a "//" inside a string is followed by an unclosed quote).
    [GeneratedRegex("(?<!:)//(?:[^\"]|\"(?:[^\"\\\\]|\\\\.)*\")*$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex LineComment();

    [GeneratedRegex("\\$?@?\"(?:[^\"\\\\]|\\\\.)*\"", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex StringLiteral();

    // Task.Delay( / Delay( (bare, static using) / Thread.Sleep( / Sleep( / WaitForTimeoutAsync( / WaitForTimeout(
    [GeneratedRegex(@"\b(?:Delay|Sleep|WaitForTimeoutAsync|WaitForTimeout)\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex CodeWait();

    // An in-page script is a string of the test: setTimeout / setInterval inside it is a fixed wait in the page.
    [GeneratedRegex(@"\b(?:setTimeout|setInterval)\s*\(", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex InPageTimer();
}
