using System.Reflection;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The comparison of the field rows of <c>04 7.1</c> and <c>7.2</c> with the code (T547 criteria 6 and 7; 04 7 introduction; 09 19.2 item 3, 21.4 item 1). A problem is a line that says what differs:
/// <c>FieldRowOnlyInDocument</c> and <c>FieldRowOnlyInCode</c> - a row without an entry or an entry without a row; <c>FormFamilyUncovered</c> - a form of the document in a family the inventory does not hold;
/// <c>FieldNotInCode</c> - the property or the identifier the entry names is not in the code; <c>RequirementDiffers</c> and <c>RequirementContradictsCode</c> - the row's requirement word is not the entry's, or
/// the property carries a required mark and the row says it is not required; <c>RuleDiffers</c> and <c>LimitDiffers</c> - the ids of the rule cell or of the limit cell are not the entry's;
/// <c>RuleMissesAttribute</c> - the property carries a validation attribute whose field type the row's rule does not name.
/// </summary>
internal static partial class FormFieldCheck
{
    public static IReadOnlyList<string> Problems(string document, IReadOnlyDictionary<string, string> sources)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(sources);

        IReadOnlyList<FormBlock> forms = StateRoleDocument.Forms(document);
        Dictionary<string, FieldRow> rows = forms.SelectMany(f => f.Rows).ToDictionary(r => r.Id, StringComparer.Ordinal);
        IReadOnlyList<FieldEntry> entries = FormFieldInventory.Entries;
        HashSet<string> knownRows = [.. rows.Keys, .. forms.Select(f => f.Block)];
        List<string> problems = [];

        HashSet<string> coveredForms = [.. entries.Select(e => string.Join('.', e.Row.Split('.')[..3]))];
        foreach (FormBlock uncovered in forms.Where(f => !coveredForms.Contains(f.Block)))
        {
            problems.Add($"FormUncovered:{uncovered.Block}: the form has no field entries in the code inventory.");
        }

        string[] covered = [.. rows.Keys.Where(id => coveredForms.Contains(string.Join('.', id.Split('.')[..3]))).Order(StringComparer.Ordinal)];
        problems.AddRange(SetComparison.Compare("04 7 field rows", covered, entries.Select(e => e.Row).Order(StringComparer.Ordinal))
            .Select(l => l.Replace("04 7 field rows: only in the document: ", "FieldRowOnlyInDocument:", StringComparison.Ordinal).Replace("04 7 field rows: only in the model: ", "FieldRowOnlyInCode:", StringComparison.Ordinal)));

        foreach (FieldEntry entry in entries.Where(e => rows.ContainsKey(e.Row)))
        {
            FieldRow row = rows[entry.Row];
            if (!row.Requirement.StartsWith(entry.Requirement, StringComparison.Ordinal))
            {
                problems.Add($"RequirementDiffers:{entry.Row}: the row says '{row.Requirement}', the entry '{entry.Requirement}'.");
            }

            string[] ruleIds = [.. RuleIds(row.Rule + " " + row.Requirement).Order(StringComparer.Ordinal)];
            string[] pinned = [.. entry.Rules.Split(' ', StringSplitOptions.RemoveEmptyEntries).Order(StringComparer.Ordinal)];
            if (!ruleIds.SequenceEqual(pinned))
            {
                problems.Add($"RuleDiffers:{entry.Row}: the rule cell points to [{string.Join(", ", ruleIds)}], the entry to [{string.Join(", ", pinned)}].");
            }

            string[] limitIds = [.. LimitId().Matches(row.Limit).Select(m => m.Value).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
            string[] pinnedLimits = [.. entry.Limit.Split(' ', StringSplitOptions.RemoveEmptyEntries).Order(StringComparer.Ordinal)];
            if (!limitIds.SequenceEqual(pinnedLimits))
            {
                problems.Add($"LimitDiffers:{entry.Row}: the limit cell names [{string.Join(", ", limitIds)}], the entry [{string.Join(", ", pinnedLimits)}].");
            }

            problems.AddRange(Place(entry, sources, out List<string> marks));
            string[] limits = [.. limitIds];
            string[] enforcement = FormFieldEnforcementTable.Lines.TryGetValue(entry.Row, out string[]? lines) ? lines : [];
            problems.AddRange(FormFieldEnforcement.Problems(entry.Row, row.Requirement, marks, enforcement, ruleIds, limits, knownRows, sources));
        }

        problems.AddRange(OwnLines(FormFieldEnforcementTable.Lines, entries.Select(e => e.Row)));

        foreach (string stray in FormFieldEnforcementTable.Lines.Keys.Where(k => entries.All(e => e.Row != k)))
        {
            problems.Add($"EnforcementNotInCode:{stray}: the enforcement table has lines for a row the inventory does not hold.");
        }

        foreach (IGrouping<string, FieldEntry> twice in entries.GroupBy(e => e.Row, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"FieldRowOnlyInCode:{twice.Key}: the inventory has the row {twice.Count()} times.");
        }

        return problems;
    }

    /// <summary>
    /// A line a row has of its own: one that no row of another form uses (the form is the first three parts of the id). A row whose every line is shared with another form has no evidence that is its own - the
    /// shared boundary call of an attempt limit may stay as a second line, but the form's own call is the evidence (<c>EnforcementNotDistinctive</c>).
    /// </summary>
    internal static IEnumerable<string> OwnLines(IReadOnlyDictionary<string, string[]> table, IEnumerable<string> rows)
    {
        static string FormOf(string row) => string.Join('.', row.Split('.')[..3]);
        static string PieceOf(string line) => line[(line.IndexOf('|', StringComparison.Ordinal) + 1)..];

        Dictionary<string, HashSet<string>> formsOf = new(StringComparer.Ordinal);
        foreach ((string row, string[] lines) in table)
        {
            foreach (string line in lines)
            {
                if (!formsOf.TryGetValue(PieceOf(line), out HashSet<string>? forms))
                {
                    forms = formsOf[PieceOf(line)] = new HashSet<string>(StringComparer.Ordinal);
                }

                forms.Add(FormOf(row));
            }
        }

        foreach (string row in rows.Where(r => table.ContainsKey(r)))
        {
            if (table[row].All(l => formsOf[PieceOf(l)].Count > 1))
            {
                yield return $"EnforcementNotDistinctive:{row}: every line of the row is shared with a row of another form; the row has no line of its own.";
            }
        }
    }

    /// <summary>
    /// The places of one row: <see cref="FieldEntry.Where"/> is one place or several joined with <c>" + "</c> (a row that covers two inputs or two properties); every place must be in the code, and the
    /// attributes of every property place are read against the one row.
    /// </summary>
    private static List<string> Place(FieldEntry entry, IReadOnlyDictionary<string, string> sources, out List<string> ruleMarks)
    {
        List<string> found = [];
        ruleMarks = [];
        foreach (string where in entry.Where.Split(" + ", StringSplitOptions.RemoveEmptyEntries))
        {
            int at = where.IndexOf('#', StringComparison.Ordinal);
            if (at >= 0)
            {
                string file = where[..at];
                string anchor = where[(at + 1)..];
                if (!sources.TryGetValue(file, out string? text) || !Holds(text, anchor))
                {
                    found.Add($"FieldNotInCode:{entry.Row}: '{anchor}' is not in {file}.");
                }

                continue;
            }

            PropertyInfo? property = Find(where);
            if (property is null)
            {
                found.Add($"FieldNotInCode:{entry.Row}: the property '{where}' is not in the code.");
                continue;
            }

            ruleMarks.AddRange(FormFieldEnforcement.RuleMarks(property));
        }

        return found;
    }

    /// <summary>
    /// Whether the text holds the anchor as a whole identifier at each end that is an identifier character: <c>MinPrice</c> is not held by <c>MinPriceText</c> (a renamed field must not stay "found" through
    /// a longer name that happens to start or end with it); an end that is a bracket or a quote needs no boundary.
    /// </summary>
    internal static bool Holds(string text, string anchor) => Count(text, anchor) > 0;

    /// <summary>How many times the text holds the anchor as <see cref="Holds"/> reads it.</summary>
    internal static int Count(string text, string anchor)
    {
        bool startsWord = char.IsLetterOrDigit(anchor[0]) || anchor[0] == '_';
        bool endsWord = char.IsLetterOrDigit(anchor[^1]) || anchor[^1] == '_';
        int count = 0;
        for (int at = text.IndexOf(anchor, StringComparison.Ordinal); at >= 0; at = text.IndexOf(anchor, at + 1, StringComparison.Ordinal))
        {
            int end = at + anchor.Length;
            bool before = !startsWord || at == 0 || !(char.IsLetterOrDigit(text[at - 1]) || text[at - 1] == '_');
            bool after = !endsWord || end >= text.Length || !(char.IsLetterOrDigit(text[end]) || text[end] == '_');
            if (before && after)
            {
                count++;
            }
        }

        return count;
    }

    private static readonly System.Runtime.CompilerServices.ConditionalWeakTable<string, string> Collapsed = new();

    /// <summary>The text with every run of white space read as one space (the code of a statement that runs over several lines is one piece).</summary>
    internal static string Collapse(string text)
    {
        if (Collapsed.TryGetValue(text, out string? known))
        {
            return known;
        }

        string collapsed = WhiteSpace().Replace(text, " ");
        Collapsed.AddOrUpdate(text, collapsed);
        return collapsed;
    }

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex WhiteSpace();

    /// <summary>The property of <c>Namespace.Type.Property</c>: the namespace is the last part of the page namespace (<c>Storefront</c> for <c>Shopfolio.Pages.Storefront</c>).</summary>
    private static PropertyInfo? Find(string where)
    {
        string[] parts = where.Split('.');
        if (parts.Length < 3)
        {
            return null;
        }

        string property = parts[^1];
        string type = parts[^2];
        string space = "." + string.Join('.', ["Pages", parts[^3]]);
        Type? found = typeof(Program).Assembly.GetTypes().SingleOrDefault(t => t.Name == type && t.Namespace is not null && t.Namespace.EndsWith(space, StringComparison.Ordinal));
        return found?.GetProperty(property, BindingFlags.Instance | BindingFlags.Public);
    }

    /// <summary>The ids a rule or requirement cell points to that the code carries a rule for: the field types of 7.3.2, the parameters of 02 11, the periods of 02 4 and the rows of this inventory it refers to.</summary>
    private static IEnumerable<string> RuleIds(string cell)
    {
        return RuleId().Matches(cell).Select(m => m.Value).Distinct(StringComparer.Ordinal);
    }

    [GeneratedRegex(@"\b7\.3\.2\.\d+\b|\bP-\d+\b|\bZ-\d+\b|\b7\.[12]\.\d+(?:\.\d+)?\b|\b8\.2\.3\b|\bK-1013\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RuleId();

    [GeneratedRegex(@"\bL-\d+\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LimitId();
}
