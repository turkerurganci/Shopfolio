using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Source scan for the test patterns that are forbidden in the test projects (09 19.2 item 6): skipped and explicit tests,
/// run-time skips, a committed runner filter that selects one test, tests without an assertion and assertions that always pass.
/// The scan is text based and needs neither a database nor the network.
/// </summary>
internal static class ForbiddenTestPatterns
{
    public const string SkippedTest = "SkippedTest";
    public const string ExplicitTest = "ExplicitTest";
    public const string RuntimeSkip = "RuntimeSkip";
    public const string AlwaysTrue = "AlwaysPassingAssertion";
    public const string NoAssertion = "TestWithoutAssertion";
    public const string OnlyNotNull = "TestWithOnlyNotNullAssertion";
    public const string RunnerFilter = "RunnerFilterSelectingTests";

    private static readonly TimeSpan _matchTimeout = TimeSpan.FromSeconds(5);

    private static readonly (string Rule, Regex Pattern)[] _sourceRules =
    [
        (SkippedTest, new Regex(@"\[(?:Fact|Theory)\s*\((?:[^\]])*?\bSkip(?:Unless|When|Type|Exceptions)?\s*=", RegexOptions.CultureInvariant, _matchTimeout)),
        (ExplicitTest, new Regex(@"\bExplicit\s*=\s*true\b", RegexOptions.CultureInvariant, _matchTimeout)),
        (RuntimeSkip, new Regex(@"\bAssert\s*\.\s*Skip\w*\s*\(|\bSkip\s*\.\s*(?:If|IfNot|Unless|When)\s*\(", RegexOptions.CultureInvariant, _matchTimeout)),
        (AlwaysTrue, new Regex(@"\bAssert\s*\.\s*True\s*\(\s*true\s*\)|\bAssert\s*\.\s*False\s*\(\s*false\s*\)", RegexOptions.CultureInvariant, _matchTimeout)),
    ];

    private static readonly Regex _runnerFilterPattern = new(
        @"--filter(?:-(?:method|class|namespace|trait|query|uid))?\b|(?:^|\s)-(?:method|class|namespace|trait)\s",
        RegexOptions.CultureInvariant | RegexOptions.Multiline,
        _matchTimeout);

    private static readonly Regex _testAttributePattern = new(@"\[(?:Fact|Theory)\b[^\]]*\]", RegexOptions.CultureInvariant, _matchTimeout);

    private static readonly Regex _methodStartPattern = new(@"\)\s*(\{|=>)", RegexOptions.CultureInvariant, _matchTimeout);

    private static readonly Regex _assertCallPattern = new(@"\bAssert\s*\.\s*(\w+)", RegexOptions.CultureInvariant, _matchTimeout);

    /// <summary>
    /// Scans one file and returns the rule names that fired, with the 1-based line where the match starts.
    /// C# files get the source rules; other test project files (project, props, json, run settings) get the runner filter rule.
    /// </summary>
    public static IReadOnlyList<(string Rule, int Line)> Scan(string fileName, string text)
    {
        List<(string Rule, int Line)> findings = [];

        if (fileName.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            foreach ((string rule, Regex pattern) in _sourceRules)
            {
                foreach (Match match in pattern.Matches(text))
                {
                    findings.Add((rule, LineOf(text, match.Index)));
                }
            }

            ScanTestMethods(text, findings);
        }
        else
        {
            foreach (Match match in _runnerFilterPattern.Matches(text))
            {
                findings.Add((RunnerFilter, LineOf(text, match.Index)));
            }
        }

        return findings;
    }

    private static void ScanTestMethods(string text, List<(string Rule, int Line)> findings)
    {
        foreach (Match attribute in _testAttributePattern.Matches(text))
        {
            Match start = _methodStartPattern.Match(text, attribute.Index + attribute.Length);
            if (!start.Success)
            {
                continue;
            }

            int bodyStart = start.Groups[1].Index;
            int bodyEnd = start.Groups[1].Value == "{" ? FindClosingBrace(text, bodyStart) : text.IndexOf(';', bodyStart);
            if (bodyEnd < 0)
            {
                continue;
            }

            string body = text.Substring(bodyStart, bodyEnd - bodyStart);
            List<string> assertions = _assertCallPattern.Matches(body).Select(m => m.Groups[1].Value).ToList();
            int line = LineOf(text, attribute.Index);

            if (assertions.Count == 0)
            {
                findings.Add((NoAssertion, line));
            }
            else if (assertions.All(name => name == "NotNull"))
            {
                findings.Add((OnlyNotNull, line));
            }
        }
    }

    // Finds the brace that closes the one at openIndex; braces inside comments and string or character literals do not count.
    private static int FindClosingBrace(string text, int openIndex)
    {
        int depth = 0;
        int i = openIndex;
        while (i < text.Length)
        {
            char c = text[i];
            if (c == '/' && i + 1 < text.Length && text[i + 1] == '/')
            {
                int newline = text.IndexOf('\n', i);
                i = newline < 0 ? text.Length : newline;
                continue;
            }

            if (c == '/' && i + 1 < text.Length && text[i + 1] == '*')
            {
                int end = text.IndexOf("*/", i + 2, StringComparison.Ordinal);
                i = end < 0 ? text.Length : end + 2;
                continue;
            }

            if (c == '"' || c == '\'')
            {
                bool verbatim = c == '"' && i > 0 && text[i - 1] == '@';
                i = SkipLiteral(text, i, c, verbatim);
                continue;
            }

            if (c == '{')
            {
                depth++;
            }
            else if (c == '}')
            {
                depth--;
                if (depth == 0)
                {
                    return i;
                }
            }

            i++;
        }

        return -1;
    }

    private static int SkipLiteral(string text, int quoteIndex, char quote, bool verbatim)
    {
        int i = quoteIndex + 1;
        while (i < text.Length)
        {
            if (!verbatim && text[i] == '\\')
            {
                i += 2;
                continue;
            }

            if (text[i] == quote)
            {
                if (verbatim && i + 1 < text.Length && text[i + 1] == quote)
                {
                    i += 2;
                    continue;
                }

                return i + 1;
            }

            i++;
        }

        return text.Length;
    }

    private static int LineOf(string text, int index)
    {
        int line = 1;
        for (int i = 0; i < index && i < text.Length; i++)
        {
            if (text[i] == '\n')
            {
                line++;
            }
        }

        return line;
    }
}
