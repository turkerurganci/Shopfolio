using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// The scan of the disposal code for a retention number written in place (T201 criterion 3, KK3; 09 3.5 item 1): the disposal step, its parts
/// and its registration never write a duration of their own. A period is a product constant (<c>ProductConstants</c>), a row of
/// <c>DisposalTimings</c> or the part that owns a legal period; here the code names that constant. The scan reads
/// <c>src/Shopfolio.Web/Infrastructure/Disposal/</c> and finds a time span, a month, a year or a day written with a digit literal. Comments are not scanned.
/// </summary>
internal static partial class DisposalNumberSourceScan
{
    public const string PeriodLiteral = "RetentionPeriodWrittenAsANumber";

    private const string DisposalFolder = "src/Shopfolio.Web/Infrastructure/Disposal/";

    /// <summary>True when the scan applies to the file: a source file of the disposal folder, except the file that holds the numbers of the table.</summary>
    public static bool AppliesTo(string relativePath)
    {
        string path = relativePath.Replace('\\', '/');
        return path.Contains(DisposalFolder, StringComparison.Ordinal) && !path.EndsWith("/DisposalTimings.cs", StringComparison.Ordinal);
    }

    public static IReadOnlyList<(string Rule, int Line)> Scan(string text)
    {
        string withoutBlocks = BlockComment().Replace(text.ReplaceLineEndings("\n"), match => new string('\n', match.Value.Count(c => c == '\n')));
        string[] lines = withoutBlocks.Split('\n');
        List<(string Rule, int Line)> findings = [];
        for (int i = 0; i < lines.Length; i++)
        {
            int comment = lines[i].IndexOf("//", StringComparison.Ordinal);
            string code = comment < 0 ? lines[i] : lines[i][..comment];
            if (PeriodCall().IsMatch(code))
            {
                findings.Add((PeriodLiteral, i + 1));
            }
        }

        return findings;
    }

    [GeneratedRegex(@"/\*.*?\*/", RegexOptions.Singleline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockComment();

    // TimeSpan.FromDays(7), AddMonths(-6), AddYears(3), AddDays(30), AddHours(2), AddMinutes(15), new TimeSpan(3, 0, 0): a digit as the first argument.
    // AddDays(1) and AddDays(-1) are the step to the next or the previous calendar day, not a period.
    [GeneratedRegex(@"\b(?:From(?:Days|Hours|Minutes|Seconds)|Add(?:Years|Months|Hours|Minutes)|AddDays(?=\s*\(\s*-?\s*(?!1\s*\))[0-9])|TimeSpan)\s*\(\s*-?\s*[0-9]",RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PeriodCall();
}
