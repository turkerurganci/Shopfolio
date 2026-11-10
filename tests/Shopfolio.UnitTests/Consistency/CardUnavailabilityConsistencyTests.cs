using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Modules.Payment;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the card's "şu an kullanılamıyor" duration equal to its row of the 09 3.5 table and to 08 2.1.8's text (T404; 09 21.4 item 1): <c>CardUnavailability.Duration</c>
/// is the only place the code has the number. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class CardUnavailabilityConsistencyTests
{
    private static string Doc(string name) => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", name)).ReplaceLineEndings("\n");

    [Fact]
    public void GuidelinesRow_EqualsTheDuration()
    {
        string row = Doc("09_CODING_GUIDELINES.md").Split('\n').Single(line => line.StartsWith("| Jeton isteğinin bağlantı hatasından sonra kartın", StringComparison.Ordinal));
        Match match = MinutesRow().Match(row);

        Assert.True(match.Success, "The card unavailability row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(CardUnavailability.Duration, TimeSpan.FromMinutes(int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture)));
    }

    [Fact]
    public void IntegrationSpec_NamesTheSameTwoMinutes()
    {
        string spec = Doc("08_INTEGRATION_SPEC.md");

        Assert.Equal(TimeSpan.FromMinutes(2), CardUnavailability.Duration);
        Assert.Contains("ödeme modülü anı **bellekte iki dakika** tutar", spec, StringComparison.Ordinal);
    }

    [GeneratedRegex(@"hâli ([0-9]+) dakika", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex MinutesRow();
}
