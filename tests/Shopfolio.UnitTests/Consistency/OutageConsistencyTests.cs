using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Modules.Membership;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the numbers and names of the outage rules equal to their documents (T204; 09 3.5 item 1, 21.4 item 1): the three P-38 constants are
/// the P-38 row of 02 11.2 - 5 minutes, 30 minutes and 2 hours, in that order - and every code of the Google failure rule is a code of 07 6.3's
/// notice tables, and the codes 08 2.3's error table gives an unavailable Google are among them. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class OutageConsistencyTests
{
    private static string[] Lines(string document)
    {
        string text = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", document));
        return text.ReplaceLineEndings("\n").Split('\n');
    }

    private static IReadOnlyList<TimeSpan> P38Row()
    {
        string row = Lines("02_PRODUCT_REQUIREMENTS.md").Single(line => line.StartsWith("| P-38 |", StringComparison.Ordinal));
        MatchCollection values = P38Value().Matches(row);
        Assert.True(values.Count == 3, "The P-38 row of 02 11.2 has changed shape (three bold durations expected); update this check with it.");
        return [.. values.Select(v => Duration(int.Parse(v.Groups[1].Value, CultureInfo.InvariantCulture), v.Groups[2].Value))];
    }

    private static TimeSpan Duration(int number, string unit)
    {
        return unit switch
        {
            "dakika" => TimeSpan.FromMinutes(number),
            "saat" => TimeSpan.FromHours(number),
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, "The unit of the P-38 row is not known to this check."),
        };
    }

    private static TimeSpan[] Constants() =>
    [
        TimeSpan.FromMinutes(ProductConstants.DispatchFirstRetryMinutes),
        TimeSpan.FromMinutes(ProductConstants.DispatchSecondRetryMinutes),
        TimeSpan.FromMinutes(ProductConstants.DispatchThirdRetryMinutes),
    ];

    [Fact]
    public void DispatchRetryConstants_AreTheP38Row()
    {
        Assert.Equal(P38Row(), Constants());
    }

    [Fact]
    public void DispatchRetryCheck_CanGoRed_WhenAConstantDiffers()
    {
        // The check can go red: one minute more on the last retry is not what the row says.
        TimeSpan[] changed = Constants();
        changed[^1] += TimeSpan.FromMinutes(1);

        Assert.NotEqual(P38Row(), changed);
    }

    // Every code the rule can answer with: both flows against both kinds of failure (T214 / D-109: the rule answers with the NoticeCode itself).
    private static readonly string[] GoogleFailureCodes =
    [
        .. Enum.GetValues<GoogleIdentityFlow>()
            .SelectMany(flow => new Shopfolio.Infrastructure.Integrations.GoogleIdentityResult[]
            {
                new Shopfolio.Infrastructure.Integrations.GoogleIdentityResult.Failed("x"),
                new Shopfolio.Infrastructure.Integrations.GoogleIdentityResult.Unavailable("x"),
            }.Select(answer => GoogleIdentityFailureRule.FailureOf(flow, answer)))
            .Select(code => code!.Value.ToString())
            .Distinct(),
    ];

    [Fact]
    public void GoogleFailureCodes_AreNoticeCodesOf07()
    {
        // Only the rows of 6.3, from its heading to the next section's: the error codes of 6.2 are another list.
        string[] lines = Lines("07_API_DESIGN.md");
        int start = Array.FindIndex(lines, line => line.StartsWith("### 6.3 ", StringComparison.Ordinal));
        Assert.True(start >= 0, "07 has no 6.3 heading; update this check with it.");
        int end = Array.FindIndex(lines, start + 1, line => line.StartsWith("### ", StringComparison.Ordinal) || line.StartsWith("## ", StringComparison.Ordinal));
        HashSet<string> noticeCodes = [.. lines[start..(end < 0 ? lines.Length : end)].Select(line => NoticeRow().Match(line)).Where(m => m.Success).Select(m => m.Groups[1].Value)];

        Assert.Contains("SocialLoginFailed", noticeCodes);
        Assert.All(GoogleFailureCodes, name => Assert.Contains(name, noticeCodes));
    }

    [Fact]
    public void GoogleUnavailableRow_Of08_NamesTheCodesOfTheRule()
    {
        string row = Lines("08_INTEGRATION_SPEC.md").Single(line => line.StartsWith("| Google'a ulaşılamadı", StringComparison.Ordinal));

        Assert.Contains($"`{nameof(NoticeCode.SocialLoginFailed)}`", row, StringComparison.Ordinal);
        Assert.Contains($"`{nameof(NoticeCode.SocialReauthenticationUnavailable)}`", row, StringComparison.Ordinal);
    }

    [GeneratedRegex(@"\*\*(\d+) (dakika|saat)\*\*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex P38Value();

    [GeneratedRegex(@"^\| `(\w+)` \|", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NoticeRow();
}
