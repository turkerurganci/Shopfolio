using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Modules.Notifications;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the per-recipient sending limit of <see cref="RecipientSendLimit"/> equal to its documents (T304; 09 3.5 item 4, 21.4 item 1): the
/// threshold and the window are the numbers of 08 2.2.4's "Eşik" row and of the 09 3.5 table's row, and the log window is the 09 row's seconds.
/// Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class RecipientSendLimitConsistencyTests
{
    private static string[] Lines(string document) =>
        [.. File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", document)).ReplaceLineEndings("\n").Split('\n')];

    private static int Number(Regex pattern, string text, string what)
    {
        Match match = pattern.Match(text);
        Assert.True(match.Success, what);
        return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    }

    [Fact]
    public void TheThresholdRowOf08_HasTheWindowAndTheThresholdOfTheCode()
    {
        string row = Assert.Single(Lines("08_INTEGRATION_SPEC.md"), line => line.StartsWith("| Eşik | **", StringComparison.Ordinal));

        Match match = HoursAndCount().Match(row);
        Assert.True(match.Success, "The Eşik row of 08 2.2.4 has changed shape.");
        Assert.Equal(RecipientSendLimit.Window, TimeSpan.FromHours(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)));
        Assert.Equal(RecipientSendLimit.Threshold, int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
    }

    [Fact]
    public void TheCodingGuidelinesRow_HasTheThresholdTheWindowAndTheLogWindowOfTheCode()
    {
        string row = Assert.Single(Lines("09_CODING_GUIDELINES.md"), line => line.StartsWith("| Müşteri bildiriminin alıcı başına gönderim sınırı:", StringComparison.Ordinal));

        Match match = HoursAndCount().Match(row);
        Assert.True(match.Success, "The 09 3.5 row of the recipient limit has changed shape.");
        Assert.Equal(RecipientSendLimit.Window, TimeSpan.FromHours(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)));
        Assert.Equal(RecipientSendLimit.Threshold, int.Parse(match.Groups[2].Value, CultureInfo.InvariantCulture));
        Assert.Equal(RecipientSendLimit.LogWindow, TimeSpan.FromSeconds(Number(LogSeconds(), row, "the seconds of the 09 3.5 row")));
    }

    // 08 and 09 say the same words: "24 saatte 20 e-posta" (the hours first, the count second).
    [GeneratedRegex(@"(\d+) saatte (\d+) e-posta", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex HoursAndCount();

    [GeneratedRegex(@"penceresi (\d+) saniye", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex LogSeconds();
}
