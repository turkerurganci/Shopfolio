using Shopfolio.Infrastructure.StartupDatabase;

namespace Shopfolio.UnitTests.InstallationSettings;

/// <summary>
/// The database side of the start-up check as a pure function (T205; 06 8.2.1): what it reports for what the context found. The records are
/// made up here; the real record list is checked against the database in the integration tests.
/// </summary>
public sealed class DatabaseAuditTests
{
    private static readonly StartupRecordPresence[] AllPresent =
    [
        new("FirstList", true, true),
        new("SecondList", true, true),
        new("FirstRow", false, true),
        new("SecondRow", false, true),
    ];

    [Fact]
    public void FindFailures_WhenEveryRecordIsPresent_ReportsNothing()
    {
        Assert.Empty(DatabaseAudit.FindFailures(AllPresent));
    }

    [Fact]
    public void FindFailures_WhenAListIsEmpty_NamesTheListAsEmpty()
    {
        IReadOnlyList<string> failures = DatabaseAudit.FindFailures(Without("SecondList"));

        Assert.Equal([DatabaseFailures.EmptyList("SecondList")], failures);
    }

    [Fact]
    public void FindFailures_WhenARowIsMissing_NamesTheRowAsMissing()
    {
        IReadOnlyList<string> failures = DatabaseAudit.FindFailures(Without("FirstRow"));

        Assert.Equal([DatabaseFailures.MissingRow("FirstRow")], failures);
    }

    [Fact]
    public void FindFailures_WhenSeveralAreAbsent_NamesEachOnceInTheOrderGiven()
    {
        IReadOnlyList<string> failures = DatabaseAudit.FindFailures(Without("SecondRow", "FirstList"));

        Assert.Equal([DatabaseFailures.EmptyList("FirstList"), DatabaseFailures.MissingRow("SecondRow")], failures);
    }

    [Fact]
    public void Failure_NamesTheRecordAndCarriesNoOtherRecordOrValue()
    {
        const string Record = "OnlyRecord";

        string list = DatabaseFailures.EmptyList(Record);
        string row = DatabaseFailures.MissingRow(Record);

        Assert.StartsWith(Record + " ", list, StringComparison.Ordinal);
        Assert.StartsWith(Record + " ", row, StringComparison.Ordinal);
        Assert.DoesNotContain(AllPresent[0].Record, list + row, StringComparison.Ordinal);
    }

    private static IEnumerable<StartupRecordPresence> Without(params string[] absent) =>
        AllPresent.Select(p => absent.Contains(p.Record, StringComparer.Ordinal) ? p with { IsPresent = false } : p);
}
