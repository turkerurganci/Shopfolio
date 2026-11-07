using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Pure comparison and readers for the library table equality check (09 3.2, 21.4 item 1): every <c>PackageVersion</c> in
/// <c>Directory.Packages.props</c> has a row in the "Kütüphaneler" table of <c>09_CODING_GUIDELINES.md</c>.
/// </summary>
internal static partial class LibraryTableConsistency
{
    private const string SectionHeading = "### Kütüphaneler";
    private const string FallbackMarker = "Yedek aday";

    /// <summary>A row kept for the record of a candidate that was weighed and not taken (09 3.3 item 4, 5): not a package of the props file.</summary>
    private const string RejectedMarker = "**Elendi**";

    /// <summary>
    /// Packages the table lists ahead of time that no task has added to the props file yet. A task that adds one removes it
    /// from this list in the same PR; any other table row without a package is a failure (T03 decision: the table is
    /// written ahead of the code, so strict two-way equality cannot hold until the last library is added).
    /// </summary>
    public static readonly IReadOnlySet<string> NotYetAdded = new HashSet<string>(StringComparer.Ordinal)
    {
        "AWSSDK.SimpleEmailV2",
        "Microsoft.IdentityModel.Protocols.OpenIdConnect",
        "Microsoft.IdentityModel.JsonWebTokens",
    };

    /// <summary>Returns the problems found; an empty list means the table and the props file agree.</summary>
    public static IReadOnlyList<string> Compare(
        IReadOnlyCollection<string> tablePackages,
        IReadOnlyCollection<string> propsPackages,
        IReadOnlySet<string> notYetAdded)
    {
        List<string> problems = [];

        foreach (string package in propsPackages.Order(StringComparer.Ordinal))
        {
            if (!tablePackages.Contains(package, StringComparer.Ordinal))
            {
                problems.Add($"'{package}' is in Directory.Packages.props but has no row in the library table.");
            }
        }

        foreach (string package in tablePackages.Order(StringComparer.Ordinal))
        {
            bool inProps = propsPackages.Contains(package, StringComparer.Ordinal);
            bool listedAsPending = notYetAdded.Contains(package);

            if (!inProps && !listedAsPending)
            {
                problems.Add($"'{package}' has a table row but is not in Directory.Packages.props and is not on the not-yet-added list.");
            }

            if (inProps && listedAsPending)
            {
                problems.Add($"'{package}' is now in Directory.Packages.props: remove it from the not-yet-added list.");
            }
        }

        foreach (string package in notYetAdded.Order(StringComparer.Ordinal))
        {
            if (!tablePackages.Contains(package, StringComparer.Ordinal))
            {
                problems.Add($"'{package}' is on the not-yet-added list but has no row in the library table.");
            }
        }

        return problems;
    }

    /// <summary>
    /// Reads the backticked package name of the first cell of every table row. Rows without backticks in the first cell
    /// (tools that are not NuGet packages) and the fallback and rejected candidate rows are not read.
    /// </summary>
    public static IReadOnlyList<string> ReadTablePackages(string markdown)
    {
        string[] lines = markdown.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        int start = Array.FindIndex(lines, l => l.StartsWith(SectionHeading, StringComparison.Ordinal));
        if (start < 0)
        {
            throw new InvalidOperationException($"'{SectionHeading}' was not found in the coding guidelines.");
        }

        List<string> packages = [];
        for (int i = start + 1; i < lines.Length; i++)
        {
            string line = lines[i];
            if (line.StartsWith("## ", StringComparison.Ordinal) || line.StartsWith("**", StringComparison.Ordinal))
            {
                break;
            }

            if (!line.StartsWith('|'))
            {
                continue;
            }

            string[] cells = line.Split('|');
            if (cells.Length < 3 || (cells[2].Contains(FallbackMarker, StringComparison.Ordinal) || cells[2].Contains(RejectedMarker, StringComparison.Ordinal)))
            {
                continue;
            }

            Match name = FirstBacktickedName().Match(cells[1].Trim());
            if (name.Success)
            {
                packages.Add(name.Groups[1].Value);
            }
        }

        return packages;
    }

    public static IReadOnlyList<string> ReadPropsPackages(string propsXml)
    {
        XDocument document = XDocument.Parse(propsXml);
        return document.Descendants("PackageVersion")
            .Select(e => (string?)e.Attribute("Include"))
            .OfType<string>()
            .ToList();
    }

    [GeneratedRegex("^`([^`]+)`")]
    private static partial Regex FirstBacktickedName();
}
