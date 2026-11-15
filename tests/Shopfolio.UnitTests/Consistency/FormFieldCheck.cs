using System.Reflection;
using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The comparison of the field rows of <c>04 7.1</c> and <c>7.2</c> with the code (T547 criteria 6 and 7; 04 7 introduction; 09 19.2 item 3, 21.4 item 1). A problem is a line that says what differs:
/// <c>FieldRowOnlyInDocument</c> and <c>FieldRowOnlyInCode</c> - a row without an entry or an entry without a row; <c>FormFamilyUncovered</c> - a form of the document in a family the inventory does not hold;
/// <c>FieldNotInCode</c> - the property or the identifier the entry names is not in the code; <c>RequirementDiffers</c> and <c>RequirementContradictsCode</c> - the row's requirement word is not the entry's, or
/// the property carries a required mark and the row says it is not required; <c>RuleDiffers</c> and <c>LimitDiffers</c> - the ids of the rule cell or of the limit cell are not the entry's;
/// <c>RuleMissesAttribute</c> - the property carries a validation attribute whose field type the row's rule does not name; <c>PlaceMarksDiffer</c> and <c>PlaceMarksNotInInventory</c> - a property place of a row
/// with several places carries other rule attributes than its pin (each place is read on its own, not through the union of the row), or a pin is of a place the inventory does not hold that way;
/// <c>PlaceEnforcementMissing</c>, <c>PlaceLineNotInCode</c> and <c>PlaceEnforcementNotInInventory</c> - a property place of such a row that carries no attribute has no entry in
/// <see cref="FormFieldPlaceEnforcement"/>, a line of its entry is not held exactly once by its file (its own check changed or left), or an entry is of a place the inventory does not hold that way.
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
        HashSet<string> usedClauses = new(StringComparer.Ordinal);

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
            problems.AddRange(PlaceLineProblems(entry, sources, FormFieldPlaceEnforcement.Lines, FormFieldPlaceMarks.Marks));
            string[] limits = [.. limitIds];
            string[] enforcement = FormFieldEnforcementTable.Lines.TryGetValue(entry.Row, out string[]? lines) ? lines : [];
            problems.AddRange(FormFieldRuleClauses.Read(entry.Row, row.Rule, out IReadOnlyList<string> demanded, usedClauses));
            problems.AddRange(FormFieldEnforcement.Problems(entry.Row, row.Requirement, marks, enforcement, [.. ruleIds, .. demanded], limits, knownRows, sources));
        }

        problems.AddRange(FormFieldRuleClauses.Stale(usedClauses));

        problems.AddRange(OwnLines(FormFieldEnforcementTable.Lines, entries.Select(e => e.Row)));
        problems.AddRange(StrayPlacePins(entries, FormFieldPlaceMarks.Marks));
        problems.AddRange(StrayPlaceLines(entries, FormFieldPlaceEnforcement.Lines));

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
    /// attributes of every property place are read against the one row and, for a row with several property places, place by place against <see cref="FormFieldPlaceMarks"/>.
    /// </summary>
    private static List<string> Place(FieldEntry entry, IReadOnlyDictionary<string, string> sources, out List<string> ruleMarks)
    {
        List<string> found = [];
        ruleMarks = [];
        List<(string Where, string[] Marks)> properties = [];
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

            string[] marks = [.. FormFieldEnforcement.RuleMarks(property).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
            properties.Add((where, marks));
            ruleMarks.AddRange(marks);
        }

        found.AddRange(PlaceMarkProblems(entry.Row, properties, FormFieldPlaceMarks.Marks));
        return found;
    }

    /// <summary>
    /// The marks of each property place of a row that has several (KK6, T547 correction 3): the row's rule and requirement are read from the union of its places, so a place that loses its attribute would be
    /// "proved" by the other. <see cref="FormFieldPlaceMarks"/> pins the rule attributes of every place of such a row that carries one (a place without a pin has none); a place whose marks differ from its pin -
    /// one lost or one gained - is reported on its own (<c>PlaceMarksDiffer</c>), and a pin of a place that is not one of several property places of its row is <c>PlaceMarksNotInInventory</c>
    /// (see <see cref="StrayPlacePins"/>).
    /// </summary>
    internal static IEnumerable<string> PlaceMarkProblems(string row, IReadOnlyList<(string Where, string[] Marks)> properties, IReadOnlyDictionary<string, string[]> pins)
    {
        if (properties.Count < 2)
        {
            yield break;
        }

        foreach ((string where, string[] marks) in properties)
        {
            string[] pinned = pins.TryGetValue(PlaceKey(row, where), out string[]? held) ? [.. held.Order(StringComparer.Ordinal)] : [];
            if (!pinned.SequenceEqual(marks))
            {
                yield return $"PlaceMarksDiffer:{row}: the place '{where}' carries [{string.Join(", ", marks)}], the table pins [{string.Join(", ", pinned)}].";
            }
        }
    }

    /// <summary>The pins whose row is not in the inventory, has fewer than two property places, or does not hold the place.</summary>
    internal static IEnumerable<string> StrayPlacePins(IEnumerable<FieldEntry> entries, IReadOnlyDictionary<string, string[]> pins)
    {
        HashSet<string> keys = new(StringComparer.Ordinal);
        foreach (FieldEntry entry in entries)
        {
            string[] places = [.. PropertyPlaces(entry)];
            if (places.Length >= 2)
            {
                keys.UnionWith(places.Select(p => PlaceKey(entry.Row, p)));
            }
        }

        return pins.Keys.Where(k => !keys.Contains(k)).Select(k => $"PlaceMarksNotInInventory:{k}: the table pins a place the inventory does not hold as one of several property places of a row.");
    }

    /// <summary>
    /// The lines of each property place of a row that has several (KK6, T547 correction 4, E1): every place that carries no rule attribute (no pin in <paramref name="pins"/>) has an entry in
    /// <paramref name="table"/> - its lines, or an empty entry for no check of its own (<c>PlaceEnforcementMissing</c> otherwise) - and each line of the entry is held exactly once by its file, so the place is
    /// reported on its own when the code of its check changes or leaves (<c>PlaceLineNotInCode</c>), whatever the other places of the row keep. The kind and its token are read at the row (<see cref="FormFieldEnforcement"/>).
    /// </summary>
    internal static IEnumerable<string> PlaceLineProblems(FieldEntry entry, IReadOnlyDictionary<string, string> sources, IReadOnlyDictionary<string, string[]> table, IReadOnlyDictionary<string, string[]> pins)
    {
        string[] places = [.. PropertyPlaces(entry)];
        if (places.Length < 2)
        {
            yield break;
        }

        foreach (string place in places)
        {
            string key = PlaceKey(entry.Row, place);
            if (!table.TryGetValue(key, out string[]? lines))
            {
                if (!pins.ContainsKey(key))
                {
                    yield return $"PlaceEnforcementMissing:{entry.Row}: the place '{place}' carries no rule attribute and has no entry in the place enforcement table (its lines, or an empty entry for no check of its own).";
                }

                continue;
            }

            foreach (string line in lines)
            {
                int bar = line.IndexOf('|', StringComparison.Ordinal);
                int hash = line.IndexOf('#', StringComparison.Ordinal);
                if (bar <= 0 || hash < bar)
                {
                    yield return $"PlaceLineNotInCode:{key}: '{line}' is not of the form Kind|file#fragment.";
                    continue;
                }

                string file = line[(bar + 1)..hash];
                string fragment = line[(hash + 1)..];
                int held = sources.TryGetValue(file, out string? text) ? Count(Collapse(text), Collapse(fragment)) : 0;
                if (held != 1)
                {
                    yield return $"PlaceLineNotInCode:{key}: '{fragment}' is held {held} times in {file} (the place's check must be the one piece of its code).";
                }
            }
        }
    }

    /// <summary>The place entries whose row is not in the inventory with several property places, or that do not hold the place.</summary>
    internal static IEnumerable<string> StrayPlaceLines(IEnumerable<FieldEntry> entries, IReadOnlyDictionary<string, string[]> table)
    {
        HashSet<string> keys = new(StringComparer.Ordinal);
        foreach (FieldEntry entry in entries)
        {
            string[] places = [.. PropertyPlaces(entry)];
            if (places.Length >= 2)
            {
                keys.UnionWith(places.Select(p => PlaceKey(entry.Row, p)));
            }
        }

        return table.Keys.Where(k => !keys.Contains(k)).Select(k => $"PlaceEnforcementNotInInventory:{k}: the place enforcement table has an entry for a place the inventory does not hold as one of several property places of a row.");
    }

    /// <summary>The property places (not the <c>file#anchor</c> ones) of an entry.</summary>
    internal static IEnumerable<string> PropertyPlaces(FieldEntry entry)
        => entry.Where.Split(" + ", StringSplitOptions.RemoveEmptyEntries).Where(w => !w.Contains('#', StringComparison.Ordinal));

    internal static string PlaceKey(string row, string where) => row + "|" + where;

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
    internal static PropertyInfo? Find(string where)
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
