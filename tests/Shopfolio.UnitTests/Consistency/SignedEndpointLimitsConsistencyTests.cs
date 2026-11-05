using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the two values of the signed machine endpoints equal to their documents (T203 criterion 9, KK9, criterion 4; 09 3.5 item 4, 21.4
/// item 1): the body limit and the log window of <see cref="SignedEndpointLimits"/> are the numbers of the 09 3.5 table's row, and the body
/// limit is also the number 07 2.8.1 gives the payment provider's notification and 08 3.2 principle 5 repeats.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class SignedEndpointLimitsConsistencyTests
{
    private const string RowLead = "| İmzalı makine ucunun (ödeme sağlayıcısının bildirimi) gövde tavanı";

    private static string Document(string name) =>
        File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", name)).ReplaceLineEndings("\n");

    private static int Number(Regex pattern, string text, string what)
    {
        Match match = pattern.Match(text);
        Assert.True(match.Success, what);
        return int.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
    }

    [Fact]
    public void TheCodingGuidelinesRow_HasTheBodyLimitAndTheWindowOfTheCode()
    {
        string row = Assert.Single(Document("09_CODING_GUIDELINES.md").Split('\n'), line => line.StartsWith(RowLead, StringComparison.Ordinal));

        Assert.Equal(SignedEndpointLimits.BodyMaxBytes, Number(Kilobytes(), row, "the KB of the 09 3.5 row") * 1024);
        Assert.Equal(SignedEndpointLimits.FailureLogWindow, TimeSpan.FromSeconds(Number(Seconds(), row, "the seconds of the 09 3.5 row")));
    }

    [Fact]
    public void TheApiDesignAndTheIntegrationSpecification_GiveTheNotificationTheSameBodyLimit()
    {
        Assert.Equal(SignedEndpointLimits.BodyMaxBytes, Number(ApiDesignLimit(), Document("07_API_DESIGN.md"), "07 2.8.1's notification limit") * 1024);
        Assert.Equal(SignedEndpointLimits.BodyMaxBytes, Number(IntegrationLimit(), Document("08_INTEGRATION_SPEC.md"), "08 3.2 principle 5's limit") * 1024);
    }

    [GeneratedRegex(@"gövde tavanı (\d+) KB", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Kilobytes();

    [GeneratedRegex(@"penceresi (\d+) saniye", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex Seconds();

    [GeneratedRegex(@"ödeme sağlayıcısının bildirimi (\d+) KB", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex ApiDesignLimit();

    [GeneratedRegex(@"\*\*Koruma istisnaları:\*\*[^\n]*?gövde tavanı (\d+) KB", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex IntegrationLimit();
}
