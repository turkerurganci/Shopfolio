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
    /// <summary>The validation attributes of the code and the field type of <c>04 7.3.2</c> each stands for.</summary>
    private static readonly Dictionary<string, string> AttributeRules = new(StringComparer.Ordinal)
    {
        ["InvalidEmailAttribute"] = "7.3.2.1",
        ["PasswordTooWeakAttribute"] = "7.3.2.2",
        ["InvalidIbanAttribute"] = "7.3.2.7",
        ["InvalidAmountAttribute"] = "7.3.2.8",
        ["InvalidPercentageAttribute"] = "7.3.2.9",
        ["InvalidQuantityAttribute"] = "7.3.2.10",
    };

    public static IReadOnlyList<string> Problems(string document, IReadOnlyDictionary<string, string> sources)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(sources);

        IReadOnlyList<FormBlock> forms = StateRoleDocument.Forms(document);
        Dictionary<string, FieldRow> rows = forms.SelectMany(f => f.Rows).ToDictionary(r => r.Id, StringComparer.Ordinal);
        IReadOnlyList<FieldEntry> entries = FormFieldInventory.Entries;
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

            problems.AddRange(Place(entry, row, ruleIds, sources));
        }

        foreach (IGrouping<string, FieldEntry> twice in entries.GroupBy(e => e.Row, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"FieldRowOnlyInCode:{twice.Key}: the inventory has the row {twice.Count()} times.");
        }

        return problems;
    }

    private static IEnumerable<string> Place(FieldEntry entry, FieldRow row, string[] documentedRules, IReadOnlyDictionary<string, string> sources)
    {
        int at = entry.Where.IndexOf('#', StringComparison.Ordinal);
        if (at >= 0)
        {
            string file = entry.Where[..at];
            string anchor = entry.Where[(at + 1)..];
            if (!sources.TryGetValue(file, out string? text) || !text.Contains(anchor, StringComparison.Ordinal))
            {
                yield return $"FieldNotInCode:{entry.Row}: '{anchor}' is not in {file}.";
            }

            yield break;
        }

        PropertyInfo? property = Find(entry.Where);
        if (property is null)
        {
            yield return $"FieldNotInCode:{entry.Row}: the property '{entry.Where}' is not in the code.";
            yield break;
        }

        string[] marks = [.. property.GetCustomAttributes().Select(a => a.GetType().Name)];
        if (marks.Contains("FieldRequiredAttribute", StringComparer.Ordinal) && !row.Requirement.StartsWith("Zorunlu", StringComparison.Ordinal) && !row.Requirement.StartsWith("Yayın için zorunlu", StringComparison.Ordinal))
        {
            yield return $"RequirementContradictsCode:{entry.Row}: the property {entry.Where} is marked required, the row says '{row.Requirement}'.";
        }

        foreach (string mark in marks.Where(AttributeRules.ContainsKey))
        {
            if (!documentedRules.Contains(AttributeRules[mark], StringComparer.Ordinal))
            {
                yield return $"RuleMissesAttribute:{entry.Row}: the property {entry.Where} carries {mark}, the row does not point to {AttributeRules[mark]}.";
            }
        }
    }

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

    [GeneratedRegex(@"\b7\.3\.2\.\d+\b|\bP-\d+\b|\bZ-\d+\b|\b7\.[12]\.\d+(?:\.\d+)?\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RuleId();

    [GeneratedRegex(@"\bL-\d+\b", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex LimitId();
}
