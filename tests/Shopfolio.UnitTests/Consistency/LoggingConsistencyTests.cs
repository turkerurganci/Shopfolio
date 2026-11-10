using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

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
    public void ForbiddenPropertyNames_EqualTheNamesOfTheGuidelines()
    {
        string item = Item("2. **Maskeleme kodda uygulanır**");
        int start = item.IndexOf("kişisel veri alan adları yok", StringComparison.Ordinal);
        Assert.True(start >= 0, "The list of personal data names was not found in 09 18.2 item 2a.");
        string list = item[start..item.IndexOf(')', start)];

        string[] documented = [.. BacktickedWords().Matches(list).Select(m => m.Groups[1].Value)];

        Assert.NotEmpty(documented);
        Assert.Equal(documented, ApplicationLogFormatter.ForbiddenPropertyNames);
    }

    [Fact]
    public void SuppressedDataAccessFailures_EqualTheEventsOfTheGuidelinesAndTheirCount()
    {
        // T411 criterion 6 (D-90 K3): 09 18.1 item 4 names the suppressed events (RelationalEventId.X, CoreEventId.Y) and item 8 says how many.
        string guidelines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md")).ReplaceLineEndings("\n");
        string section = guidelines[guidelines.IndexOf("### 18.1 ", StringComparison.Ordinal)..guidelines.IndexOf("### 18.2 ", StringComparison.Ordinal)];
        string[] lines = section.Split('\n');
        string item4 = lines.First(line => line.StartsWith("4. **Düzey.**", StringComparison.Ordinal));
        string item8 = lines.First(line => line.StartsWith("8. **Test.**", StringComparison.Ordinal));

        int[] documented =
        [
            .. EventIdName().Matches(item4).Select(m =>
            {
                Type owner = m.Groups["type"].Value == nameof(CoreEventId) ? typeof(CoreEventId) : typeof(RelationalEventId);
                return ((EventId)owner.GetField(m.Groups["name"].Value)!.GetValue(null)!).Id;
            }),
        ];

        Assert.NotEmpty(documented);
        Assert.Equal(documented.Distinct().Order(), LoggingSetup.SuppressedDataAccessFailures.Select(f => f.EventId).Order());
        // The number in words ("üç") is the count of the list; item 4 and item 8 both say it.
        string[] words = ["", "bir", "iki", "üç", "dört"];
        Assert.Contains("**Yalnız " + words[LoggingSetup.SuppressedDataAccessFailures.Count] + " hata olayı yazılmaz**", item4, StringComparison.Ordinal);
        Assert.Contains("yalnız " + words[LoggingSetup.SuppressedDataAccessFailures.Count] + " olayı eler", item8, StringComparison.Ordinal);
    }

    [Fact]
    public void CommonLogFields_EqualTheFirstColumnOfTheFieldTable()
    {
        string guidelines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md")).ReplaceLineEndings("\n");
        string section = guidelines[guidelines.IndexOf("### 18.2 ", StringComparison.Ordinal)..];
        string[] rows = [.. section.Split('\n').TakeWhile(line => !line.StartsWith("2. **Maskeleme", StringComparison.Ordinal)).Where(line => line.TrimStart().StartsWith("| `", StringComparison.Ordinal))];

        string[] documented = [.. rows.SelectMany(row => BacktickedNames().Matches(row.Split('|')[1]).Select(m => m.Groups[1].Value))];

        Assert.NotEmpty(documented);
        // The table's first column names every field but the array of the field rejection, which its description column names.
        Assert.Equal(
            documented.Append(CommonLogFields.FieldsArray).Order(StringComparer.Ordinal),
            CommonLogFields.All.Select(field => field.Name).Order(StringComparer.Ordinal));
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

    // `RelationalEventId.ConnectionError`, `CoreEventId.SaveChangesFailed`
    [GeneratedRegex(@"`(?<type>RelationalEventId|CoreEventId)\.(?<name>[A-Za-z]+)`", RegexOptions.CultureInvariant)]
    private static partial Regex EventIdName();

    [GeneratedRegex("`([a-z][A-Za-z]+)`", RegexOptions.CultureInvariant)]
    private static partial Regex BacktickedNames();

    [GeneratedRegex("`(/[^`]+)`", RegexOptions.CultureInvariant)]
    private static partial Regex BacktickedPaths();

    [GeneratedRegex(@"\{[^}]*\}", RegexOptions.CultureInvariant)]
    private static partial Regex BracedPart();

    [GeneratedRegex(@"^\s*-\s*app-logs:(\S+)\s*$", RegexOptions.Multiline | RegexOptions.CultureInvariant)]
    private static partial Regex ApplicationLogVolume();
}
