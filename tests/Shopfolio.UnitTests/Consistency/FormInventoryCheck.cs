using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The comparison of the form inventory of <c>04 7.1</c> and <c>7.2</c> with <c>02</c> and the code (T547; 04 7 introduction; 09 3.5 item 1, 19.2 item 3, 21.4 item 1). A problem is a line that says what
/// differs: <c>FormNumbering</c>, <c>FieldRowId</c>, <c>EmptyCell</c> - the blocks and rows are not numbered without a gap or a cell is empty; <c>FormScreenSide</c>, <c>FormScreenUnknown</c>,
/// <c>FormScreenWithoutBlock</c> - a form of the customer side that names a panel screen or the reverse, a screen that is not in 04 4 or that has no block in 6; <c>RequirementWord</c> - a requirement cell
/// that does not start with one of the words the inventory knows; <c>RuleWritesValue</c> - a rule or requirement cell that holds a number where it should point to an id (07 introduction, convention 10);
/// <c>RuleIdUnknown</c> - an id (<c>7.3.2.n</c>, <c>P-n</c>, <c>L-n</c>, <c>Z-n</c>, a form or field row) that is not in its table; <c>ProductConstantNotRead</c> - a product constant of <c>02 11.2</c> a rule
/// points to that no validation of the code reads from <c>ProductConstants</c>; <c>AttributeLimitIsANumber</c> - a length attribute of the code that writes a number (09 3.5 item 1). There is no list of known differences.
/// </summary>
internal static partial class FormInventoryCheck
{
    private static readonly string[] RequirementWords = ["Zorunlu", "İsteğe bağlı", "Seçim", "Yayın için zorunlu", "Salt okunur", "İşaretli gelir", "Meslek odası ve davranış kuralları zorunlu", "—"];

    private static readonly string[] ConstantReaderFolders = ["Pages/", "Modules/", "Infrastructure/"];

    public static IReadOnlyList<string> Problems(string document, IReadOnlyDictionary<string, string> sources, string requirements)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(sources);
        ArgumentNullException.ThrowIfNull(requirements);

        IReadOnlyList<FormBlock> forms = StateRoleDocument.Forms(document);
        IReadOnlyList<ScreenListing> screens = StateRoleDocument.Screens(document);
        IReadOnlyList<ScreenBlock> blocks = StateRoleDocument.Blocks(document);
        List<string> problems = [];

        problems.AddRange(Structure(forms));
        problems.AddRange(Sides(forms, screens, blocks));
        problems.AddRange(Cells(document, forms, requirements));
        problems.AddRange(Constants(forms, sources));
        problems.AddRange(AttributeLimits(sources));
        return problems;
    }

    // 04 7: every form a block, every field a row; the blocks and rows are numbered in order without a gap and every cell says something.
    private static IEnumerable<string> Structure(IReadOnlyList<FormBlock> forms)
    {
        foreach (IGrouping<string, FormBlock> family in forms.GroupBy(f => string.Join('.', f.Block.Split('.')[..2]), StringComparer.Ordinal))
        {
            if (!family.Select(f => f.Block).SequenceEqual(Enumerable.Range(1, family.Count()).Select(n => $"{family.Key}.{n}")))
            {
                yield return $"FormNumbering:{family.Key}: the blocks are not numbered 1..{family.Count()} in order.";
            }
        }

        foreach (FormBlock form in forms)
        {
            if (form.Rows.Count == 0)
            {
                yield return $"FormNumbering:{form.Block}: a form holds at least one field row.";
            }

            for (int i = 0; i < form.Rows.Count; i++)
            {
                FieldRow row = form.Rows[i];
                if (row.Id != $"{form.Block}.{i + 1}")
                {
                    yield return $"FieldRowId:{form.Block}: the row at place {i + 1} is {row.Id}.";
                }

                if (new[] { row.Field, row.Requirement, row.Rule, row.Message, row.Limit }.Any(string.IsNullOrWhiteSpace))
                {
                    yield return $"EmptyCell:{row.Id}: a field row says the field, the requirement, the rule, the error message and the limit (04 7).";
                }
            }
        }
    }

    // 04 7.1 is the customer side's forms and 7.2 the panel's; a form's screen is a screen of 04 4 with a block in 6 (the panel's forms and operations are under /panel, 04 6.1.3).
    private static IEnumerable<string> Sides(IReadOnlyList<FormBlock> forms, IReadOnlyList<ScreenListing> screens, IReadOnlyList<ScreenBlock> blocks)
    {
        foreach (FormBlock form in forms)
        {
            string side = form.Block.StartsWith("7.1.", StringComparison.Ordinal) ? "Customer" : "Panel";
            string lead = form.Heading[..HeadingEnd(form.Heading)];
            string[] named = [.. NavigationDocument.Screens(lead)];
            if (named.Length == 0)
            {
                yield return $"FormScreenUnknown:{form.Block}: the heading names no screen.";
            }

            foreach (string screen in named)
            {
                ScreenListing? listed = screens.FirstOrDefault(s => s.Screen == screen);
                if (listed is null)
                {
                    yield return $"FormScreenUnknown:{form.Block}: {screen} is not a screen of 04 4.";
                }
                else if (listed.Side != side)
                {
                    yield return $"FormScreenSide:{form.Block}: {screen} is a {listed.Side.ToLowerInvariant()} screen; 04 {(side == "Customer" ? "7.1 holds the customer side's forms" : "7.2 holds the panel's forms")}.";
                }

                if (blocks.All(b => b.Screen != screen))
                {
                    yield return $"FormScreenWithoutBlock:{form.Block}: {screen} has no block in 04 6.";
                }

                IReadOnlyList<string>? routes = StateRoleInventory.Screens.FirstOrDefault(e => e.Screen == screen)?.Routes;
                string? route = routes is { Count: > 0 } ? routes[0] : null;
                if (route is not null && route.StartsWith("/panel", StringComparison.Ordinal) != (side == "Panel"))
                {
                    yield return $"FormScreenSide:{form.Block}: the page of {screen} is at '{route}'.";
                }
            }
        }
    }

    // 04 7 introduction and 04 conventions: the rule is an id of 02 (section, P-, L-), never a value; the requirement is one of the inventory's words.
    private static IEnumerable<string> Cells(string document, IReadOnlyList<FormBlock> forms, string requirements)
    {
        HashSet<string> ruleKinds = [.. Ids(DataModelDocument.Tables(DataModelDocument.Section(document, "### 7.3 ")), @"^7\.3\.2\.\d+$", 0)];
        HashSet<string> parameters = [.. Ids(DataModelDocument.Tables(DataModelDocument.Section(requirements, "## 11. ")), @"^P-\d+$", 0)];
        HashSet<string> attemptLimits = [.. Ids(DataModelDocument.Tables(requirements), @"^L-\d+$", 0)];
        HashSet<string> periods = [.. Ids(DataModelDocument.Tables(requirements), @"^Z-\d+$", 0)];
        HashSet<string> rows = [.. forms.SelectMany(f => f.Rows.Select(r => r.Id)), .. forms.Select(f => f.Block)];

        foreach (FieldRow row in forms.SelectMany(f => f.Rows))
        {
            if (!RequirementWords.Any(w => row.Requirement.StartsWith(w, StringComparison.Ordinal)))
            {
                yield return $"RequirementWord:{row.Id}: the requirement '{row.Requirement}' starts with none of: {string.Join(", ", RequirementWords)}.";
            }

            foreach (string cell in new[] { row.Rule, row.Requirement })
            {
                foreach (Match id in RuleKindId().Matches(cell).Where(m => !ruleKinds.Contains(m.Value)))
                {
                    yield return $"RuleIdUnknown:{row.Id}: '{id.Value}' is no field type of 04 7.3.2.";
                }

                foreach (Match id in ParameterId().Matches(cell).Where(m => !parameters.Contains(m.Value)))
                {
                    yield return $"RuleIdUnknown:{row.Id}: '{id.Value}' is no parameter of 02 11.";
                }

                foreach (Match id in AttemptId().Matches(cell).Where(m => !attemptLimits.Contains(m.Value)))
                {
                    yield return $"RuleIdUnknown:{row.Id}: '{id.Value}' is no attempt limit of 02 8.2.";
                }

                foreach (Match id in PeriodId().Matches(cell).Where(m => !periods.Contains(m.Value)))
                {
                    yield return $"RuleIdUnknown:{row.Id}: '{id.Value}' is no period of 02 4.";
                }

                foreach (Match id in RowReference().Matches(cell).Where(m => !rows.Contains(m.Value)))
                {
                    yield return $"RuleIdUnknown:{row.Id}: '{id.Value}' is no form or field row of 04 7.";
                }

                if (Values(cell) is { Length: > 0 } value)
                {
                    yield return $"RuleWritesValue:{row.Id}: '{value}' is a value; the cell points to the id that holds it (04 7 introduction; 02 11).";
                }
            }

            foreach (Match id in AttemptId().Matches(row.Limit).Where(m => !attemptLimits.Contains(m.Value)))
            {
                yield return $"RuleIdUnknown:{row.Id}: '{id.Value}' is no attempt limit of 02 8.2.";
            }
        }
    }

    /// <summary>The words of a cell that are values: what is left of a cell once the ids, the code spans and the references are taken out, and still holds a digit. The document names a rule by id, never by number.</summary>
    public static string Values(string cell)
    {
        // A word inside double quotes is a label of the screen ("Son 30 gün"), not a value of the rule.
        string rest = QuotedLabel().Replace(CodeSpan().Replace(cell, string.Empty), string.Empty);
        rest = AnyId().Replace(rest, string.Empty);
        rest = SectionReference().Replace(rest, string.Empty);
        Match value = DigitWord().Match(rest);
        return value.Success ? value.Value : string.Empty;
    }

    // 09 3.5 item 1: a product constant a field rule points to is read from ProductConstants by the validation, never written again.
    private static IEnumerable<string> Constants(IReadOnlyList<FormBlock> forms, IReadOnlyDictionary<string, string> sources)
    {
        string[] cited = [.. forms.SelectMany(f => f.Rows).SelectMany(r => ParameterId().Matches(r.Rule + " " + r.Requirement).Select(m => m.Value)).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        foreach (string id in cited)
        {
            ParameterEntry? entry = ParameterInventory.Entries.FirstOrDefault(e => e.Id == id);
            if (entry is null || entry.Layer != ParameterLayer.ProductConstant)
            {
                continue;
            }

            foreach (ParameterConstant constant in entry.Constants)
            {
                string use = "ProductConstants." + constant.Name;
                bool read = sources.Any(s => ConstantReaderFolders.Any(f => s.Key.StartsWith(f, StringComparison.Ordinal))
                    && !s.Key.EndsWith("ParameterInventory.cs", StringComparison.Ordinal) && !s.Key.EndsWith("ProductConstants.cs", StringComparison.Ordinal)
                    && s.Value.Contains(use, StringComparison.Ordinal));
                if (!read)
                {
                    yield return $"ProductConstantNotRead:{id}: no validation of the code reads {use}.";
                }
            }
        }
    }

    // 09 3.5 item 1: "the number is written nowhere else - the attribute, the HTML and the test refer to the same constant".
    private static IEnumerable<string> AttributeLimits(IReadOnlyDictionary<string, string> sources)
    {
        foreach ((string file, string text) in sources.Where(s => s.Key.EndsWith(".cs", StringComparison.Ordinal) && !s.Key.EndsWith("ProductConstants.cs", StringComparison.Ordinal)).OrderBy(s => s.Key, StringComparer.Ordinal))
        {
            foreach (Match match in LengthAttribute().Matches(text))
            {
                yield return $"AttributeLimitIsANumber: {file} writes {match.Value.Trim()}; the limit is a ProductConstants constant.";
            }
        }
    }

    private static int HeadingEnd(string heading)
    {
        int open = heading.IndexOf('(', StringComparison.Ordinal);
        return open < 0 ? heading.Length : open;
    }

    private static IEnumerable<string> Ids(IReadOnlyList<IReadOnlyList<string[]>> tables, string pattern, int column)
    {
        foreach (string[] cells in tables.SelectMany(t => t))
        {
            if (cells.Length > column && Regex.IsMatch(cells[column], pattern, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)))
            {
                yield return cells[column];
            }
        }
    }

    [GeneratedRegex(@"`[^`]*`", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CodeSpan();

    [GeneratedRegex(@"\b(?:7\.[12]\.\d+(?:\.\d+)?|7\.3\.\d+(?:\.\d+)*|\d+(?:\.\d+)+|(?:P|L|Z|K|D|S|OB|SK|KP)-\d+)\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AnyId();

    [GeneratedRegex(@"""[^""]*""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex QuotedLabel();

    [GeneratedRegex(@"§\s?\d+(?:\.\d+)*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SectionReference();

    [GeneratedRegex(@"%?\d+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex DigitWord();

    [GeneratedRegex(@"\b7\.3\.2\.\d+\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RuleKindId();

    [GeneratedRegex(@"\bP-\d+\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ParameterId();

    [GeneratedRegex(@"\bL-\d+\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex AttemptId();

    [GeneratedRegex(@"\bZ-\d+\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PeriodId();

    [GeneratedRegex(@"\b7\.[12]\.\d+(?:\.\d+)?\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RowReference();

    [GeneratedRegex(@"\[\s*(?:TextTooLong|StringLength|MaxLength|MinLength)\(\s*\d+[^\]]*\]", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LengthAttribute();
}
