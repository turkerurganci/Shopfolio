using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the lives of the account links in the code equal to the rows of the product constants table in <c>02</c> (T309 criterion 7; 08 2.2.8; 09 21.4 item 1, 3.5
/// item 1): P-12, P-13, P-14 and P-47 are read from the document and compared with <see cref="ProductConstants"/>, whose values the e-mail bodies write. The expected
/// values are read from the document, never typed in the test.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class LinkLifetimesConsistencyTests
{
    private static string Row(string id)
    {
        string[] candidates = Directory.GetFiles(Path.Combine(RepositoryRoot.Find(), "Docs"), "02_*.md");
        string document = File.ReadAllText(Assert.Single(candidates)).ReplaceLineEndings("\n");
        return document.Split('\n').Single(line => line.StartsWith($"| {id} |", StringComparison.Ordinal));
    }

    private static TimeSpan LifeOf(string id)
    {
        Match match = Life().Match(Row(id));
        Assert.True(match.Success, $"{id} carries no bold life such as **24 saat**.");
        int amount = int.Parse(match.Groups["amount"].Value, CultureInfo.InvariantCulture);
        return match.Groups["unit"].Value switch
        {
            "saat" => TimeSpan.FromHours(amount),
            "dakika" => TimeSpan.FromMinutes(amount),
            "gün" => TimeSpan.FromDays(amount),
            _ => throw new InvalidOperationException("Unknown unit in the 02 row: " + match.Groups["unit"].Value),
        };
    }

    [Fact]
    public void TheEmailVerificationLinkLife_IsRowP12()
    {
        Assert.Equal(LifeOf("P-12"), TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours));
    }

    [Fact]
    public void ThePasswordResetLinkLife_IsRowP13_AndTheGoogleConfirmationHasTheSame()
    {
        Assert.Equal(LifeOf("P-13"), TimeSpan.FromMinutes(ProductConstants.PasswordResetLinkMinutes));
        Assert.Contains("Google girişinin teyit bağlantısı da bu ömrü taşır", Row("P-13"), StringComparison.Ordinal);
    }

    [Fact]
    public void TheAdminInvitationLinkLife_IsRowP14()
    {
        Assert.Equal(LifeOf("P-14"), TimeSpan.FromHours(ProductConstants.AdminInvitationLinkHours));
    }

    [Fact]
    public void TheEmailChangeRevertLinkLife_IsRowP47()
    {
        Assert.Equal(LifeOf("P-47"), TimeSpan.FromDays(ProductConstants.EmailChangeRevertLinkDays));
    }

    [GeneratedRegex(@"\*\*(?<amount>\d+) (?<unit>saat|dakika|gün)\*\*")]
    private static partial Regex Life();
}
