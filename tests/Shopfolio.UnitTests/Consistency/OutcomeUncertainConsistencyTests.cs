using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the set of the handlers marked <c>[OutcomeUncertain]</c> equal to the use case list of 07 6.4.2 in both directions (T212; 09 8.1 item 4, 21.4 item 1).
/// The marked handlers arrive with the tasks of their pages, so the list is split in two: the marked ones the code carries and <see cref="NotMarkedYet"/>,
/// the use cases whose page does not exist yet. The two together are exactly the document's list, and a use case is in one of them only: the task that marks
/// a handler removes its use case from <see cref="NotMarkedYet"/>, or this turns red. Reads repository files and the application assembly only.
/// Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class OutcomeUncertainConsistencyTests
{
    /// <summary>The use cases of 07 6.4.2 whose handler is not written yet (D-137: the list is empty when the last marked page is built).</summary>
    private static readonly string[] NotMarkedYet =
    [
        "UC-39", "UC-40", "UC-41", "UC-43", "UC-68", "UC-53", "UC-76", "UC-186", "UC-187",
        "UC-160", "UC-165", "UC-167", "UC-172", "UC-177",
    ];

    [Fact]
    public void MarkedHandlers_AndTheNotYetMarked_EqualTheUseCaseListOf07_InBothDirections()
    {
        string[] documented = DocumentedUseCases();
        string[] marked = MarkedUseCases();

        Assert.Empty(marked.Intersect(NotMarkedYet, StringComparer.Ordinal));
        Assert.Equal(documented.Order(StringComparer.Ordinal), marked.Concat(NotMarkedYet).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void TheListOf07_NamesTheUseCasesOfTheFiveGroups()
    {
        string[] documented = DocumentedUseCases();

        Assert.Equal(documented.Length, documented.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("UC-27", documented);
        Assert.Contains("UC-68", documented);
        Assert.Contains("UC-177", documented);
    }

    private static string[] DocumentedUseCases()
    {
        string[] lines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "07_API_DESIGN.md")).ReplaceLineEndings("\n").Split('\n');
        string paragraph = Assert.Single(lines, l => l.StartsWith("**6.4.2 Sonucu belirsiz işlem**", StringComparison.Ordinal));
        int end = paragraph.IndexOf("Uygulama kaydı hâli", StringComparison.Ordinal);
        Assert.True(end > 0, "07 6.4.2 no longer ends its list with the sentence about the log.");

        return [.. UseCase().Matches(paragraph[..end]).Select(m => m.Value).Distinct(StringComparer.Ordinal)];
    }

    private static string[] MarkedUseCases()
    {
        return [.. typeof(OutcomeUncertainAttribute).Assembly.GetTypes()
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .Select(m => m.GetCustomAttribute<OutcomeUncertainAttribute>()?.Operation)
            .OfType<string>()];
    }

    [GeneratedRegex(@"UC-\d+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex UseCase();
}
