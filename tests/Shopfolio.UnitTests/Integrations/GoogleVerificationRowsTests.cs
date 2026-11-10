using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The seven outside-fact rows the first Google task observes (T409 criterion 8, KK8; 08 4.1: DV-25, DV-26, DV-27, DV-28, DV-31, DV-37, DV-43): the last cell of each carries a
/// verification date and names what was observed and what still waits for the real application; the runbook exists and has the template's nine sections.
/// </summary>
public sealed class GoogleVerificationRowsTests
{
    private static readonly string[] _rows = ["DV-25", "DV-26", "DV-27", "DV-28", "DV-31", "DV-37", "DV-43"];

    [Fact]
    public void TheSevenRows_CarryAVerificationDate_AndNameWhatWaitsForTheRealApplication()
    {
        string root = RepositoryRoot.Find();
        string[] lines = File.ReadAllLines(Path.Combine(root, "Docs", "08_INTEGRATION_SPEC.md"));

        foreach (string id in _rows)
        {
            string row = Assert.Single(lines, l => l.StartsWith($"| {id} |", StringComparison.Ordinal));
            string lastCell = row.TrimEnd().TrimEnd('|').Split('|')[^1].Trim();

            Assert.DoesNotContain("Bekliyor", lastCell, StringComparison.Ordinal);
            Assert.Matches(@"\(\d{4}-\d{2}-\d{2}, T409", lastCell);
            Assert.Contains("OWNER_INPUT", lastCell, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void TheRunbook_HasTheTemplatesNineSections()
    {
        string root = RepositoryRoot.Find();
        string runbook = File.ReadAllText(Path.Combine(root, "Docs", "INTEGRATION_RUNBOOKS", "GOOGLE.md"));

        for (int number = 1; number <= 9; number++)
        {
            Assert.Matches(new Regex($@"(?m)^## {number}\. ", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), runbook);
        }
    }
}
