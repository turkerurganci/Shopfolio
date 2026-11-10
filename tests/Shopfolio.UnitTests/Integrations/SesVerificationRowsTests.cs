using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The five outside-fact rows the first SES task observes (T407 criterion 9, KK9; 08 4.1: DV-16, DV-17, DV-19, DV-21, DV-23): the last cell of each carries a verification
/// date and names what was observed and what still waits for the real account; the runbook exists and has the template's nine sections.
/// </summary>
public sealed class SesVerificationRowsTests
{
    private static readonly string[] _rows = ["DV-16", "DV-17", "DV-19", "DV-21", "DV-23"];

    [Fact]
    public void TheFiveRows_CarryAVerificationDate_AndNameWhatWaitsForTheRealAccount()
    {
        string root = RepositoryRoot.Find();
        string[] lines = File.ReadAllLines(Path.Combine(root, "Docs", "08_INTEGRATION_SPEC.md"));

        foreach (string id in _rows)
        {
            string row = Assert.Single(lines, l => l.StartsWith($"| {id} |", StringComparison.Ordinal));
            string lastCell = row.TrimEnd().TrimEnd('|').Split('|')[^1].Trim();

            Assert.DoesNotContain("Bekliyor", lastCell, StringComparison.Ordinal);
            Assert.Matches(@"\(\d{4}-\d{2}-\d{2}, T407", lastCell);
            Assert.Contains("OWNER_INPUT", lastCell, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheRunbook_HasTheTemplatesNineSections()
    {
        string root = RepositoryRoot.Find();
        string runbook = File.ReadAllText(Path.Combine(root, "Docs", "INTEGRATION_RUNBOOKS", "AMAZON_SES.md"));

        for (int number = 1; number <= 9; number++)
        {
            Assert.Matches(new Regex($@"(?m)^## {number}\. ", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), runbook);
        }
    }
}
