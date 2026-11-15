using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>One row of a navigation table of <c>04 3</c>: the id (<c>3.3.7</c>), the cell of the source place, the trigger and the cell of the target place (the menu table of 3.2 triggers by the menu entry itself).</summary>
internal sealed record NavigationRow(string Id, string From, string Trigger, string To);

/// <summary>
/// Reader of the navigation map (<c>04 3.1</c> to <c>3.7</c>; T546; 09 21.4 item 1): the rows of the tables 3.1 to 3.5 and 3.7 as (id, from, to) - the menu table of 3.2 reads the menu entry and
/// the sub entry as "from" and the screen column as "to" - and the five paragraphs of 3.6 by their id. The checks that must fail on a changed document read a changed copy of the text,
/// never the file (09 19.2 item 3).
/// </summary>
internal static partial class NavigationDocument
{
    private static readonly string[] TableSections = ["### 3.1 ", "### 3.2 ", "### 3.3 ", "### 3.4 ", "### 3.5 ", "### 3.7 "];

    public static string Load()
    {
        return File.ReadAllText(Path.Combine(Support.RepositoryRoot.Find(), "Docs", "04_UI_SPECS.md")).ReplaceLineEndings("\n");
    }

    /// <summary>The rows of 3.1-3.5 and 3.7, in document order; a table line whose first cell is not a row id (<c>3.n.k</c>) is not a row.</summary>
    public static IReadOnlyList<NavigationRow> Rows(string document)
    {
        List<NavigationRow> rows = [];
        foreach (string heading in TableSections)
        {
            string section = DataModelDocument.Section(document, heading);
            IReadOnlyList<IReadOnlyList<string[]>> tables = DataModelDocument.Tables(section);
            for (int t = 0; t < tables.Count; t++)
            {
                foreach (string[] cells in tables[t])
                {
                    if (cells.Length < 4 || !RowId().IsMatch(cells[0]))
                    {
                        continue;
                    }

                    bool menuTable = heading == "### 3.2 " && t == 0;
                    string from = menuTable ? (cells[2] == "—" ? cells[1] : cells[1] + " / " + cells[2]) : cells[1];
                    rows.Add(new NavigationRow(cells[0], from, menuTable ? cells[1] : cells[2], cells[3]));
                }
            }
        }

        return rows;
    }

    /// <summary>The paragraphs of 3.6 by id (<c>3.6.1</c> to <c>3.6.5</c>): the bold lead and the text of the paragraph.</summary>
    public static IReadOnlyDictionary<string, string> Returns(string document)
    {
        Dictionary<string, string> paragraphs = [];
        foreach (string line in DataModelDocument.Section(document, "### 3.6 ").Split('\n'))
        {
            Match match = ReturnLead().Match(line);
            if (match.Success)
            {
                paragraphs[match.Groups[1].Value] = line;
            }
        }

        return paragraphs;
    }

    /// <summary>The screens a cell names (<c>E-04</c>), distinct and ordered.</summary>
    public static IReadOnlyList<string> Screens(string cell)
    {
        return [.. ScreenId().Matches(cell).Select(m => m.Value).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
    }

    [GeneratedRegex(@"^3\.[1-7]\.\d+$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RowId();

    [GeneratedRegex(@"^\*\*(3\.6\.\d)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ReturnLead();

    [GeneratedRegex(@"E-\d\d", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScreenId();
}
