using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Disposal;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the disposal's numbers equal to the rows of the 09 3.5 table that name them (T201; 09 21.4 item 1): the part size and the guest cart's
/// idle days. <c>DisposalTimings</c> is the only place the code has these numbers. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class DisposalTimingsConsistencyTests
{
    private static string TableRow(string start)
    {
        string guidelines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md"));
        return guidelines.ReplaceLineEndings("\n").Split('\n').Single(line => line.StartsWith(start, StringComparison.Ordinal));
    }

    [Fact]
    public void PartRow_EqualsThePartSize()
    {
        Match match = PartRow().Match(TableRow("| İmha ve gecelik temizliğin parçası"));

        Assert.True(match.Success, "The disposal part row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(DisposalTimings.PartSize, int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void GuestCartRow_EqualsTheIdleDays()
    {
        Match match = GuestCartRow().Match(TableRow("| Sunucudaki misafir sepetinin silinme eşiği"));

        Assert.True(match.Success, "The guest cart row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(DisposalTimings.GuestCartIdleDays, int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void InboundNotificationRow_EqualsTheRetentionYears()
    {
        Match match = InboundNotificationRow().Match(TableRow("| Gelen bildirim kaydının saklama süresi"));

        Assert.True(match.Success, "The inbound notification row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(DisposalTimings.InboundNotificationRetentionYears, int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture));
    }

    [GeneratedRegex(@"saklama süresi: ([0-9]+) yıl", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex InboundNotificationRow();

    [GeneratedRegex(@"en çok ([0-9]+) satır", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex PartRow();

    [GeneratedRegex(@"eşiği: ([0-9]+) gün", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex GuestCartRow();
}
