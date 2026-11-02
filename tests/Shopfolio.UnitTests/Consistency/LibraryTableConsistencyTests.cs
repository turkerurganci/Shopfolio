using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

[Trait("Consistency", "true")]
public sealed class LibraryTableConsistencyTests
{
    private const string SampleTable = """
        ### Kütüphaneler

        | Kütüphane | Görevi | Lisans |
        |---|---|---|
        | `Alpha` | a | MIT |
        | `Beta` (Ganss) | b | MIT |
        | `Gamma` 3.1.x | **Yedek aday** — g | MIT |
        | Mailpit | tool | MIT |

        **Kütüphanesiz kararlar**: `Delta` yok
        """;

    private static readonly IReadOnlySet<string> NoPending = new HashSet<string>(StringComparer.Ordinal);

    [Fact]
    public void ReadTablePackages_SampleTable_SkipsFallbackAndUnbackticked()
    {
        IReadOnlyList<string> packages = LibraryTableConsistency.ReadTablePackages(SampleTable);

        Assert.Equal(["Alpha", "Beta"], packages);
    }

    [Fact]
    public void ReadPropsPackages_SampleProps_ReturnsIncludes()
    {
        const string props = "<Project><ItemGroup><PackageVersion Include=\"Alpha\" Version=\"1.0.0\" /><PackageVersion Include=\"Beta\" Version=\"2.0.0\" /></ItemGroup></Project>";

        IReadOnlyList<string> packages = LibraryTableConsistency.ReadPropsPackages(props);

        Assert.Equal(["Alpha", "Beta"], packages);
    }

    [Fact]
    public void Compare_Equal_ReturnsNoProblems()
    {
        IReadOnlyList<string> problems = LibraryTableConsistency.Compare(["Alpha", "Beta"], ["Beta", "Alpha"], NoPending);

        Assert.Empty(problems);
    }

    [Fact]
    public void Compare_PackageWithoutTableRow_Fails()
    {
        IReadOnlyList<string> problems = LibraryTableConsistency.Compare(["Alpha"], ["Alpha", "Beta"], NoPending);

        string problem = Assert.Single(problems);
        Assert.Contains("'Beta'", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Compare_ExtraTableRow_Fails()
    {
        IReadOnlyList<string> problems = LibraryTableConsistency.Compare(["Alpha", "Beta"], ["Alpha"], NoPending);

        string problem = Assert.Single(problems);
        Assert.Contains("'Beta'", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Compare_ExtraTableRowOnPendingList_Passes()
    {
        HashSet<string> pending = new(StringComparer.Ordinal) { "Beta" };

        IReadOnlyList<string> problems = LibraryTableConsistency.Compare(["Alpha", "Beta"], ["Alpha"], pending);

        Assert.Empty(problems);
    }

    [Fact]
    public void Compare_PendingPackageNowInProps_Fails()
    {
        HashSet<string> pending = new(StringComparer.Ordinal) { "Beta" };

        IReadOnlyList<string> problems = LibraryTableConsistency.Compare(["Alpha", "Beta"], ["Alpha", "Beta"], pending);

        string problem = Assert.Single(problems);
        Assert.Contains("remove it from the not-yet-added list", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Compare_PendingPackageWithoutTableRow_Fails()
    {
        HashSet<string> pending = new(StringComparer.Ordinal) { "Ghost" };

        IReadOnlyList<string> problems = LibraryTableConsistency.Compare(["Alpha"], ["Alpha"], pending);

        string problem = Assert.Single(problems);
        Assert.Contains("'Ghost'", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void RepositoryFiles_LibraryTableAndPackagesProps_Agree()
    {
        string root = RepositoryRoot.Find();
        string markdown = File.ReadAllText(Path.Combine(root, "Docs", "09_CODING_GUIDELINES.md"));
        string props = File.ReadAllText(Path.Combine(root, "Directory.Packages.props"));

        IReadOnlyList<string> problems = LibraryTableConsistency.Compare(
            LibraryTableConsistency.ReadTablePackages(markdown),
            LibraryTableConsistency.ReadPropsPackages(props),
            LibraryTableConsistency.NotYetAdded);

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }
}
