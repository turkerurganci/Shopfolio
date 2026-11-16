using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>One row of the screen inventory of <c>04 4.1</c> and <c>4.2</c>: the id (<c>E-04</c>), the name and the side (<c>Customer</c> or <c>Panel</c>).</summary>
internal sealed record ScreenListing(string Screen, string Name, string Side);

/// <summary>One row of a block of <c>04 6.2</c> or <c>6.3</c>: the id (<c>6.2.4.1</c>) and the five cells the block's header names - role, state, what is shown, which actions are active, the source.</summary>
internal sealed record MatrixRow(string Id, string Role, string Status, string Shown, string Actions, string Source);

/// <summary>One block of the state by role matrix: the block id (<c>6.2.4</c>), the screen it belongs to (<c>E-04</c>), the title and the rows.</summary>
internal sealed record ScreenBlock(string Block, string Screen, string Title, IReadOnlyList<MatrixRow> Rows);

/// <summary>
/// Reader of the state by role matrix and of the form inventory of <c>04</c> (T547; 04 6.1 to 6.3, 7.1, 7.2; 09 21.4 item 1): the screen inventory of 4.1 and 4.2, the blocks and rows of 6.2 and 6.3, the
/// two sentences of 6.1.5 that name the screens a gate or the administrator changes, and the blocks and field rows of 7.1 and 7.2. It reads text only; the checks that must fail on a changed
/// document read a changed copy of the text, never the file (09 19.2 item 3).
/// </summary>
internal static partial class StateRoleDocument
{
    public static string Load()
    {
        return NavigationDocument.Load();
    }

    /// <summary>The screens of 4.1 (customer side) and 4.2 (panel), in document order; a table line whose first cell is not a screen id is not a row.</summary>
    public static IReadOnlyList<ScreenListing> Screens(string document)
    {
        List<ScreenListing> screens = [];
        foreach ((string heading, string side) in new[] { ("### 4.1 ", "Customer"), ("### 4.2 ", "Panel") })
        {
            foreach (IReadOnlyList<string[]> table in DataModelDocument.Tables(DataModelDocument.Section(document, heading)))
            {
                foreach (string[] cells in table)
                {
                    if (cells.Length >= 2 && ScreenId().IsMatch(cells[0]))
                    {
                        screens.Add(new ScreenListing(cells[0], cells[1], side));
                    }
                }
            }
        }

        return screens;
    }

    /// <summary>The blocks of 6.2 and 6.3 in document order. A row whose id does not start with its block's id, or that has not six cells, is an error of the document the reader reports at once.</summary>
    public static IReadOnlyList<ScreenBlock> Blocks(string document)
    {
        List<ScreenBlock> blocks = [];
        foreach (string heading in new[] { "### 6.2 ", "### 6.3 " })
        {
            string? block = null;
            string? screen = null;
            string? title = null;
            List<MatrixRow> rows = [];
            foreach (string line in DataModelDocument.Section(document, heading).Split('\n'))
            {
                Match lead = BlockLead().Match(line);
                if (lead.Success)
                {
                    Flush(blocks, block, screen, title, rows);
                    block = lead.Groups[1].Value;
                    screen = lead.Groups[2].Value;
                    title = lead.Groups[3].Value;
                    rows = [];
                    continue;
                }

                if (block is not null && RowLine().IsMatch(line))
                {
                    string[] cells = DataModelDocument.Cells(line);
                    if (cells.Length != 6)
                    {
                        throw new InvalidOperationException($"04 6 row {cells[0]} has {cells.Length} cells; a row has id, role, state, shown, actions, source.");
                    }

                    rows.Add(new MatrixRow(cells[0], cells[1], cells[2], cells[3], cells[4], cells[5]));
                }
            }

            Flush(blocks, block, screen, title, rows);
        }

        return blocks;
    }

    /// <summary>The screens the sentence of 6.1.5.2 names for the sales gate and for the gate of the data-collecting entries.</summary>
    public static (IReadOnlyList<string> Sales, IReadOnlyList<string> Data) GateScreens(string document)
    {
        string line = Bullet(document, "- **6.1.5.2");
        int data = line.IndexOf("veri toplayan girişlerin kapısı yalnız", StringComparison.Ordinal);
        int sales = line.IndexOf("vitrinde yalnız", StringComparison.Ordinal);
        int end = line.IndexOf("değiştirir", StringComparison.Ordinal);
        if (data < 0 || sales < 0 || end < data || sales > data)
        {
            throw new InvalidOperationException("04 6.1.5.2 does not name the screens of the two gates in the shape the reader knows.");
        }

        return (NavigationDocument.Screens(line[sales..data]), NavigationDocument.Screens(line[data..end]));
    }

    /// <summary>The screens whose blocks may hold an administrator row (the list in the sentence of 6.1.5.1) and the screen whose row says the screen is the same for the administrator.</summary>
    public static (IReadOnlyList<string> Changed, IReadOnlyList<string> Same) AdministratorScreens(string document)
    {
        string line = Bullet(document, "- **6.1.5.1");
        int list = line.IndexOf("ekranlarda vardır (", StringComparison.Ordinal);
        int same = line.IndexOf("; E-05'in Yönetici'yi anan satırı", StringComparison.Ordinal);
        if (list < 0 || same < list)
        {
            throw new InvalidOperationException("04 6.1.5.1 does not name the administrator screens in the shape the reader knows.");
        }

        int close = line.IndexOf(')', list);
        return (NavigationDocument.Screens(line[list..close]), NavigationDocument.Screens(line[same..(same + 12)]));
    }

    /// <summary>The panel screens whose state the sales gate shows (the last sentence of 6.1.5.2: E-31 and E-44 to E-47).</summary>
    public static IReadOnlyList<string> PanelGateScreens(string document)
    {
        string line = Bullet(document, "- **6.1.5.2");
        int panel = line.IndexOf("Panelde hâl", StringComparison.Ordinal);
        if (panel < 0)
        {
            throw new InvalidOperationException("04 6.1.5.2 does not say where the panel shows the gate.");
        }

        string tail = line[panel..];
        int end = tail.IndexOf("görünür", StringComparison.Ordinal);
        string[] numbers = [.. NumberInTail().Matches(tail[..end]).Select(m => m.Groups[1].Value)];
        // "E-31, E-44…E-47": the range between two screens is every screen of the inventory between them.
        int from = int.Parse(numbers[1], System.Globalization.CultureInfo.InvariantCulture);
        int to = int.Parse(numbers[2], System.Globalization.CultureInfo.InvariantCulture);
        return [.. new[] { numbers[0] }.Concat(Enumerable.Range(from, to - from + 1).Select(n => n.ToString(System.Globalization.CultureInfo.InvariantCulture))).Select(n => "E-" + n)];
    }

    /// <summary>The blocks of 7.1 and 7.2: the screens the heading names, the heading line and the field rows (<c>7.1.3.2</c>) with their five cells.</summary>
    public static IReadOnlyList<FormBlock> Forms(string document)
    {
        List<FormBlock> forms = [];
        foreach (string heading in new[] { "### 7.1 ", "### 7.2 " })
        {
            string? block = null;
            string? title = null;
            List<FieldRow> rows = [];
            foreach (string line in DataModelDocument.Section(document, heading).Split('\n'))
            {
                Match lead = FormLead().Match(line);
                if (lead.Success)
                {
                    FlushForm(forms, block, title, rows);
                    block = lead.Groups[1].Value;
                    title = line;
                    rows = [];
                    continue;
                }

                if (block is not null && FieldLine().IsMatch(line))
                {
                    string[] cells = DataModelDocument.Cells(line);
                    if (cells.Length != 6)
                    {
                        throw new InvalidOperationException($"04 7 row {cells[0]} has {cells.Length} cells; a row has id, field, requirement, rule, error message, limit.");
                    }

                    rows.Add(new FieldRow(cells[0], cells[1], cells[2], cells[3], cells[4], cells[5]));
                }
            }

            FlushForm(forms, block, title, rows);
        }

        return forms;
    }

    private static string Bullet(string document, string start)
    {
        string[] lines = [.. DataModelDocument.Section(document, "### 6.1 ").Split('\n').Where(l => l.StartsWith(start, StringComparison.Ordinal))];
        return lines.Length == 1 ? lines[0] : throw new InvalidOperationException($"04 6.1 has {lines.Length} paragraphs starting '{start}'.");
    }

    private static void Flush(List<ScreenBlock> blocks, string? block, string? screen, string? title, List<MatrixRow> rows)
    {
        if (block is not null)
        {
            blocks.Add(new ScreenBlock(block, screen!, title!, [.. rows]));
        }
    }

    private static void FlushForm(List<FormBlock> forms, string? block, string? title, List<FieldRow> rows)
    {
        if (block is not null)
        {
            forms.Add(new FormBlock(block, title!, [.. rows]));
        }
    }

    [GeneratedRegex(@"^E-\d\d$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ScreenId();

    [GeneratedRegex(@"^\*\*(6\.[23]\.\d+) (E-\d\d) — ([^*]+?)\.?\*\*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BlockLead();

    [GeneratedRegex(@"^\| 6\.[23]\.\d+\.\d+ \|", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RowLine();

    [GeneratedRegex(@"^\*\*(7\.[12]\.\d+) ", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FormLead();

    [GeneratedRegex(@"^\| 7\.[12]\.\d+\.\d+ \|", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FieldLine();

    [GeneratedRegex(@"E-(\d\d)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex NumberInTail();
}

/// <summary>One field row of a form block of <c>04 7.1</c> or <c>7.2</c>: the id (<c>7.1.3.2</c>), the field, the requirement, the rule, the error message and the limit.</summary>
internal sealed record FieldRow(string Id, string Field, string Requirement, string Rule, string Message, string Limit);

/// <summary>One form block: the id (<c>7.1.3</c>), its heading line and the field rows.</summary>
internal sealed record FormBlock(string Block, string Heading, IReadOnlyList<FieldRow> Rows);
