using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the places that name the same log fact equal (T230; 09 18.1 item 2, 18.2 items 2b and 3, 21.4 item 1): the closed event list of the code
/// against the list in the guidelines, the keyed paths of the path masker against the list in the guidelines, and the default log directory
/// against the Compose volume and the image's directory. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class LoggingConsistencyTests
{
    [Fact]
    public void LogEvents_EqualTheClosedListOfTheGuidelines()
    {
        string item = Item("3. **Olay adları kapalı bir listedir**");

        string[] documented = [.. BacktickedWords().Matches(item).Select(m => m.Groups[1].Value).Where(word => word != nameof(LogEvents)).Distinct(StringComparer.Ordinal)];

        Assert.Equal(documented.Order(StringComparer.Ordinal), LogEvents.All.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void KeyedPaths_EqualTheListOfTheGuidelines()
    {
        string item = Item("2. **Maskeleme kodda uygulanır**");

        string[] documented =
        [
            .. BacktickedPaths().Matches(item)
                .Select(m => BracedPart().Replace(m.Groups[1].Value, PathMasker.KeyPlaceholder)),
        ];

        Assert.NotEmpty(documented);
        Assert.Equal(documented.Order(StringComparer.Ordinal), PathMasker.KeyedPaths.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void DefaultLogDirectory_IsTheComposeVolumeOfTheApplicationAndTheImageDirectory()
    {
        string compose = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "deploy", "compose.yaml"));
        string dockerfile = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "deploy", "images", "app", "Dockerfile"));

        Match mount = ApplicationLogVolume().Match(compose);

        Assert.True(mount.Success, "The application's log volume is not in deploy/compose.yaml.");
        Assert.Equal(LoggingOptions.DefaultDirectory, mount.Groups[1].Value);
        Assert.Contains(LoggingOptions.DefaultDirectory, dockerfile, StringComparison.Ordinal);
    }

    private static string Item(string start)
    {
        string guidelines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md"));
        int section = guidelines.IndexOf("### 18.2 ", StringComparison.Ordinal);
        Assert.True(section >= 0, "Section 18.2 was not found.");
        string text = guidelines[section..].ReplaceLineEndings("\n");
        return text.Split('\n').First(line => line.StartsWith(start, StringComparison.Ordinal));
    }

    [GeneratedRegex("`([A-Z][A-Za-z]+)`", RegexOptions.CultureInvariant)]
    private static partial Regex BacktickedWords();

    [GeneratedRegex("`(/[^`]+)`", RegexOptions.CultureInvariant)]
    private static partial Regex BacktickedPaths();

    [GeneratedRegex(@"\{[^}]*\}", RegexOptions.CultureInvariant)]
    private static partial Regex BracedPart();

    [GeneratedRegex(@"^\s*-\s*app-logs:(\S+)\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex ApplicationLogVolume();
}
