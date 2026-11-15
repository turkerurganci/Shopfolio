using System.Reflection;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Validation;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The two-way equality of a field row and the code's validation (T547 criterion 6, KK6): the row names a rule, a limit or a requirement - the code must hold the check (row to code); the code holds a check - the row must
/// name it (code to row); and no kind of check or rule is skipped (the mapping tables hold every one, or the test fails). The checks that must fail read a changed copy of the sources or call the comparison with a
/// check removed, never the files (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class FormFieldEnforcementTests
{
    private static readonly Lazy<string> Document = new(StateRoleDocument.Load);

    private static readonly Lazy<IReadOnlyDictionary<string, string>> Sources = new(() => NavigationSites.Load(RepositoryRoot.Find()));

    private static readonly IReadOnlySet<string> KnownRows = new HashSet<string>(StringComparer.Ordinal) { "7.2.7", "7.2.8.2", "7.2.8.3", "7.1.11.4" };

    private static IEnumerable<string> Run(string requirement, string[] marks, string[] lines, string[] ids, string[] limits)
        => FormFieldEnforcement.Problems("7.9.9.9", requirement, marks, lines, ids, limits, KnownRows, Sources.Value);

    // KK6: the attribute that carries a rule leaves the property - the row names the rule and the code no longer checks it.
    [Fact]
    public void Attribute_RemovedFromTheProperty_IsReported()
    {
        Assert.Contains(Run("Zorunlu", [], [], ["7.3.2.1"], []), p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal) && p.Contains("7.3.2.1", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", ["InvalidEmailAttribute"], [], ["7.3.2.1"], []), p => p.StartsWith("RequirementWithoutEnforcement:", StringComparison.Ordinal));
        Assert.Empty(Run("Zorunlu", ["FieldRequiredAttribute", "InvalidEmailAttribute"], [], ["7.3.2.1"], []));
    }

    // KK6: the requirement leaves the code - the row says "Zorunlu" and nothing makes the field required.
    [Fact]
    public void Requirement_RemovedFromTheCode_IsReported()
    {
        Assert.Contains(Run("Zorunlu", ["InvalidIbanAttribute"], [], ["7.3.2.7"], []), p => p.StartsWith("RequirementWithoutEnforcement:", StringComparison.Ordinal));
        Assert.Contains(Run("Yayın için zorunlu", [], [], [], []), p => p.StartsWith("RequirementWithoutEnforcement:", StringComparison.Ordinal));
        Assert.DoesNotContain(Run("İsteğe bağlı", ["InvalidIbanAttribute"], [], ["7.3.2.7"], []), p => p.StartsWith("Requirement", StringComparison.Ordinal));
    }

    // KK6: the code requires what the row calls optional.
    [Fact]
    public void Requirement_AddedToTheCode_IsReported()
    {
        Assert.Contains(Run("İsteğe bağlı", ["FieldRequiredAttribute"], [], [], []), p => p.StartsWith("RequirementContradictsCode:", StringComparison.Ordinal));
        Assert.Contains(Run("Seçim", [], ["Required|Pages/Product.cshtml.cs#nameof(AddCartItemInput.VariantId), FieldCodes.FieldRequired"], [], []), p => p.StartsWith("RequirementContradictsCode:", StringComparison.Ordinal));
    }

    // KK6: the code checks a kind the row does not name; a rule the row names is another kind than the code's.
    [Fact]
    public void Check_TheRowDoesNotName_IsReported()
    {
        Assert.Contains(Run("Zorunlu", ["FieldRequiredAttribute", "InvalidIbanAttribute"], [], [], []), p => p.StartsWith("RuleMissesAttribute:", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", ["FieldRequiredAttribute", "InvalidIbanAttribute"], [], ["7.3.2.8"], []), p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal) && p.Contains("7.3.2.8", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", ["FieldRequiredAttribute"], ["P-27|Modules/Catalog/ProductSaves.cs#nameof(ProductSaveCommand.Name), ErrorCode.TextTooLong, ProductConstants.ProductNameMaxLength"], [], []), p => p.StartsWith("RuleMissesAttribute:", StringComparison.Ordinal));
    }

    // KK6: a parameter or a limit the row names is read by a line of the field's check; without the line the row is reported.
    [Fact]
    public void Parameter_AndLimit_WithoutALine_AreReported()
    {
        Assert.Contains(Run("İsteğe bağlı", [], [], ["P-9"], []), p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal) && p.Contains("P-9", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], [], ["L-4"]), p => p.StartsWith("RuleWithoutEnforcement:", StringComparison.Ordinal) && p.Contains("L-4", StringComparison.Ordinal));
        Assert.DoesNotContain(Run("İsteğe bağlı", [], ["L-4|Modules/CorporateContent/ContactFormSending.cs#AttemptLimit.L4, byIp"], [], ["L-4"]), p => p.StartsWith("Rule", StringComparison.Ordinal));
    }

    // KK6: a kind of check or a rule that the tables do not hold is never skipped.
    [Fact]
    public void KindOrRule_NotInTheTables_IsReported()
    {
        Assert.Contains(Run("İsteğe bağlı", ["InvalidSomethingAttribute"], [], [], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], ["Unknown|Pages/Product.cshtml.cs#FieldCodes.FieldRequired"], [], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], ["7.3.2.99"], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], ["P-99"], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], [], ["L-99"]), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
        Assert.Contains(Run("İsteğe bağlı", [], [], ["7.9.1.1"], []), p => p.StartsWith("RuleKindUnmapped:", StringComparison.Ordinal));
    }

    // KK6: an enforcement line whose token is not of its kind, or that is not in the file, is reported.
    [Fact]
    public void EnforcementLine_OfAnotherKindOrNotInTheFile_IsReported()
    {
        Assert.Contains(Run("Zorunlu", [], ["Email|Pages/Product.cshtml.cs#nameof(AddCartItemInput.VariantId), FieldCodes.FieldRequired"], [], []), p => p.StartsWith("EnforcementNotInCode:", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", [], ["Required|Pages/Product.cshtml.cs#nameof(AddCartItemInput.NoSuchField), FieldCodes.FieldRequired"], [], []), p => p.StartsWith("EnforcementNotInCode:", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", [], ["Required|Pages/NoSuchFile.cs#FieldCodes.FieldRequired"], [], []), p => p.StartsWith("EnforcementNotInCode:", StringComparison.Ordinal));
        Assert.Contains(Run("Zorunlu", [], ["Required without a separator"], [], []), p => p.StartsWith("EnforcementNotInCode:", StringComparison.Ordinal));
    }

    // KK6 on the real table: the code line of every enforcement line of every row is held by the code; removing the field's part of a line from the sources reports that row.
    [Fact]
    public void EveryEnforcementLine_RemovedFromTheCode_IsReported()
    {
        List<string> silent = [];
        Dictionary<string, IReadOnlyList<string>> reportedBy = new(StringComparer.Ordinal);
        foreach ((string row, string[] lines) in FormFieldEnforcementTable.Lines)
        {
            foreach (string line in lines)
            {
                int hash = line.IndexOf('#', StringComparison.Ordinal);
                string file = line[(line.IndexOf('|', StringComparison.Ordinal) + 1)..hash];
                string first = line[(hash + 1)..].Split(["@@@", "@@"], StringSplitOptions.None)[0];
                string key = file + "|" + first;
                if (!reportedBy.TryGetValue(key, out IReadOnlyList<string>? problems))
                {
                    problems = FormFieldCheck.Problems(Document.Value, new Dictionary<string, string>(Sources.Value, StringComparer.Ordinal) { [file] = Sources.Value[file].Replace(first, "RemovedFromTheCode", StringComparison.Ordinal) });
                    reportedBy[key] = problems;
                }

                if (!problems.Any(p => p.StartsWith($"EnforcementNotInCode:{row}:", StringComparison.Ordinal)))
                {
                    silent.Add($"{row}: {line}");
                }
            }
        }

        Assert.True(silent.Count == 0, "A line removed from the code was not reported:\n" + string.Join('\n', silent));
    }

    // KK6 on the real code: every rule attribute of every property place of the inventory, taken off the property (the marks given to the comparison without it), reports the row unless the same kind is
    // still checked by another attribute or line of the row.
    [Fact]
    public void EveryRuleAttribute_TakenOffTheProperty_IsNoLongerEvidence()
    {
        List<string> silent = [];
        foreach (FieldEntry entry in FormFieldInventory.Entries)
        {
            string[] marks = [.. entry.Where.Split(" + ", StringSplitOptions.RemoveEmptyEntries).Where(w => !w.Contains('#', StringComparison.Ordinal)).SelectMany(w => RuleMarksOf(w))];
            foreach (string mark in marks.Distinct(StringComparer.Ordinal))
            {
                string[] without = [.. marks.Where(m => m != mark)];
                string kind = FormFieldEnforcement.AttributeKinds[mark];
                string[] lines = FormFieldEnforcementTable.Lines.TryGetValue(entry.Row, out string[]? held) ? held : [];
                bool stillChecked = without.Any(m => FormFieldEnforcement.AttributeKinds[m] == kind) || lines.Any(l => l.StartsWith(kind + "|", StringComparison.Ordinal));
                if (stillChecked)
                {
                    continue;
                }

                // The row's own claim: the field was required by this attribute, or a rule of this kind was named.
                bool claimed = kind == "Required" || FormFieldEnforcement.RuleKinds.Values.Any(v => v.Contains(kind, StringComparer.Ordinal));
                bool reported = FormFieldEnforcement.Problems(entry.Row, kind == "Required" ? "Zorunlu" : "İsteğe bağlı", without, lines, kind == "Required" ? [] : RuleIdsOf(kind), [], KnownRows, Sources.Value)
                    .Any(p => p.StartsWith("Rule", StringComparison.Ordinal) || p.StartsWith("Requirement", StringComparison.Ordinal));
                if (!claimed || !reported)
                {
                    silent.Add($"{entry.Row}: {mark}");
                }
            }
        }

        Assert.True(silent.Count == 0, "An attribute taken off a property was not reported:\n" + string.Join('\n', silent));
    }

    // No rule attribute of the code is outside the mapping table: a new one fails here until it is mapped.
    [Fact]
    public void EveryRuleAttributeOfTheCode_IsInTheKindTable()
    {
        string[] missing =
        [
            .. typeof(Program).Assembly.GetTypes()
                .Where(t => !t.IsAbstract && typeof(FieldRuleAttribute).IsAssignableFrom(t))
                .Select(t => t.Name)
                .Where(n => !FormFieldEnforcement.AttributeKinds.ContainsKey(n)),
        ];

        Assert.True(missing.Length == 0, "Rule attributes without a kind: " + string.Join(", ", missing));
    }

    // No field type of 04 7.3.2 is outside the rule table: a new row of the table fails here until it is mapped.
    [Fact]
    public void EveryFieldType_OfTheDocument_IsInTheRuleTable()
    {
        string[] types = [.. FieldTypeRow().Matches(Document.Value).Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal)];
        string[] missing = [.. types.Where(t => !FormFieldEnforcement.RuleKinds.ContainsKey(t))];

        Assert.NotEmpty(types);
        Assert.True(missing.Length == 0, "Field types without a rule kind: " + string.Join(", ", missing));
    }

    // Every kind the tables hold has tokens, and every id the rule table names a kind for names kinds that the kind table holds.
    [Fact]
    public void RuleTable_NamesOnlyKindsOfTheKindTable()
    {
        string[] unknown =
        [
            .. FormFieldEnforcement.RuleKinds.Values.SelectMany(v => v).Concat(FormFieldEnforcement.AttributeKinds.Values)
                .Where(k => !FormFieldEnforcement.KindTokens.ContainsKey(k) && k is not "Color" && k is not "CheckboxAccepted")
                .Distinct(StringComparer.Ordinal),
        ];

        Assert.True(unknown.Length == 0, "Kinds without tokens: " + string.Join(", ", unknown));
    }

    private static IEnumerable<string> RuleMarksOf(string where)
    {
        string[] parts = where.Split('.');
        if (parts.Length < 3)
        {
            return [];
        }

        string space = ".Pages." + parts[^3];
        Type? type = typeof(Program).Assembly.GetTypes().SingleOrDefault(t => t.Name == parts[^2] && t.Namespace is not null && t.Namespace.EndsWith(space, StringComparison.Ordinal));
        PropertyInfo? property = type?.GetProperty(parts[^1], BindingFlags.Instance | BindingFlags.Public);
        return property is null ? [] : FormFieldEnforcement.RuleMarks(property);
    }

    private static string[] RuleIdsOf(string kind) => [.. FormFieldEnforcement.RuleKinds.Where(r => r.Value.Contains(kind, StringComparer.Ordinal)).Select(r => r.Key).Take(1)];

    [GeneratedRegex(@"^\| (7\.3\.2\.\d+) \|", RegexOptions.Multiline | RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex FieldTypeRow();
}
