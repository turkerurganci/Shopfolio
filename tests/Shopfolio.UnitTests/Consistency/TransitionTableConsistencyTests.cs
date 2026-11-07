using System.Text.RegularExpressions;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality check of the white list (09 9.1 item 8, 21.4 item 1; T315 KK1): <c>TransitionTable</c> equals the tables of <c>02 5.4</c> and <c>5.5</c>
/// and the table of <c>09 9.1</c> item 2 row by row (identity, source, target); every code of <c>TransitionCode</c> except <c>Correction</c> is in it
/// exactly once; the five status enums equal the tables of <c>02 5</c>, and the content records take two of the three publish values. The checks that
/// must fail on a changed document read a changed copy of the text, never the file (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class TransitionTableConsistencyTests
{
    private static string Requirements => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "02_PRODUCT_REQUIREMENTS.md")).ReplaceLineEndings("\n");

    private static string Guidelines => File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md")).ReplaceLineEndings("\n");

    private static readonly string[] ContentEntities = ["AboutPage", "Offering", "PortfolioItem", "FaqItem", "CustomPage", "Announcement", "Branch"];

    private static readonly string[] CatalogEntities = ["Product", "Variant"];

    [Fact]
    public void Table_EqualsTheTransitionTablesOfTheRequirementsDocument()
    {
        IReadOnlyList<string> problems = RequirementsProblems(Requirements);

        Assert.Empty(problems);
    }

    [Fact]
    public void Table_EqualsTheTableOfTheCodingGuidelines()
    {
        IReadOnlyList<string> problems = GuidelinesProblems(Guidelines);

        Assert.Empty(problems);
    }

    [Fact]
    public void Table_HasSixteenRows_EachCodeExceptCorrectionOnce()
    {
        TransitionCode[] codes = [.. TransitionTable.Codes];

        Assert.Equal(16, codes.Length);
        Assert.Equal(codes.Length, codes.Distinct().Count());
        Assert.DoesNotContain(TransitionCode.Correction, codes);
        Assert.Equal(
            Enum.GetValues<TransitionCode>().Where(c => c != TransitionCode.Correction).Order(),
            codes.Order());
    }

    [Fact]
    public void RequirementsCheck_OnAChangedTarget_ReportsTheRow()
    {
        string changed = Requirements.Replace("| S4 | Hazırlanıyor → İptal edildi |", "| S4 | Hazırlanıyor → Teslim edildi |", StringComparison.Ordinal);

        IReadOnlyList<string> problems = RequirementsProblems(changed);

        Assert.Contains(problems, p => p.Contains("S4", StringComparison.Ordinal));
        Assert.NotEmpty(problems);
    }

    [Fact]
    public void RequirementsCheck_OnADroppedPaymentRow_ReportsIt()
    {
        string changed = Regex.Replace(Requirements, @"^\| Ö5 \|.*\n", string.Empty, RegexOptions.Multiline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

        IReadOnlyList<string> problems = RequirementsProblems(changed);

        Assert.Contains(problems, p => p.Contains("O5", StringComparison.Ordinal));
    }

    [Fact]
    public void GuidelinesCheck_OnAChangedSource_ReportsTheRow()
    {
        string changed = Guidelines.Replace("| `S8` | `DeliveryFailed` → `Shipped` |", "| `S8` | `Placed` → `Shipped` |", StringComparison.Ordinal);

        IReadOnlyList<string> problems = GuidelinesProblems(changed);

        Assert.Contains(problems, p => p.Contains("S8", StringComparison.Ordinal));
    }

    [Fact]
    public void StatusEnums_EqualTheStateTablesOfTheRequirementsDocument()
    {
        string document = Requirements;
        List<string> problems =
        [
            .. SetComparison.Compare("OrderStatus", StateCodes(document, "### 5.4 "), Enum.GetNames<OrderStatus>()),
            .. SetComparison.Compare("PaymentStatus", StateCodes(document, "### 5.5 "), Enum.GetNames<PaymentStatus>()),
            .. SetComparison.Compare("PublishStatus (product and variant)", StateCodes(document, "### 5.1 "), Enum.GetNames<PublishStatus>()),
            .. SetComparison.Compare("DefectClaimStatus", StateCodes(document, "### 5.10 "), Enum.GetNames<DefectClaimStatus>()),
            .. SetComparison.Compare("ContactRequestStatus", StateCodes(document, "### 5.13 "), Enum.GetNames<ContactRequestStatus>()),
        ];

        Assert.Empty(problems);
    }

    [Fact]
    public void ContentRecords_TakeTwoOfTheThreePublishValues_NeverArchived()
    {
        IReadOnlyList<string> contentCodes = StateCodes(Requirements, "### 5.2 ");
        Assert.Equal(["Draft", "Published"], contentCodes);
        Assert.Equal(contentCodes.Order(), Enum.GetNames<PublishStatus>().Where(n => n != nameof(PublishStatus.Archived)).Order());

        IReadOnlyList<CheckFact> checks = ModelFacts.Shared.Checks;
        List<string> problems = [];
        foreach (string entity in ContentEntities)
        {
            CheckFact? rule = checks.SingleOrDefault(c => c.Table == entity && c.Name.EndsWith("_publish_status", StringComparison.Ordinal));
            if (rule is null)
            {
                problems.Add($"{entity} has no publish status rule.");
            }
            else if (rule.Sql.Contains("Archived", StringComparison.Ordinal) || !rule.Sql.Contains("'Draft'", StringComparison.Ordinal) || !rule.Sql.Contains("'Published'", StringComparison.Ordinal))
            {
                problems.Add($"{entity}: {rule.Sql}");
            }
        }

        foreach (string entity in CatalogEntities)
        {
            CheckFact? rule = checks.SingleOrDefault(c => c.Table == entity && c.Name.EndsWith("_publish_status", StringComparison.Ordinal));
            if (rule is null || !rule.Sql.Contains("'Archived'", StringComparison.Ordinal))
            {
                problems.Add($"{entity} takes all three publish values: {rule?.Sql}");
            }
        }

        Assert.Empty(problems);
    }

    private static IReadOnlyList<string> RequirementsProblems(string document)
    {
        string shipment = DataModelDocument.Section(document, "### 5.4 ");
        string payment = DataModelDocument.Section(document, "### 5.5 ");
        return
        [
            .. SetComparison.Compare("02 5.4 shipment axis", ShipmentRows(shipment), TransitionTable.Shipment.Select(t => $"{t.Code}: {t.From} -> {t.To}")),
            .. SetComparison.Compare("02 5.5 payment axis", PaymentRows(payment), TransitionTable.Payment.Select(t => $"{t.Code}: {t.From} -> {t.To}")),
        ];
    }

    private static IReadOnlyList<string> GuidelinesProblems(string document)
    {
        // The table stands inside a numbered list item, so its lines are indented.
        string section = string.Join('\n', DataModelDocument.Section(document, "### 9.1 ").Split('\n').Select(l => l.TrimStart()));
        List<string> documented = [];
        foreach (string[] row in DataModelDocument.Tables(section).SelectMany(t => t).Where(r => r.Length >= 4))
        {
            for (int i = 0; i + 1 < row.Length; i += 2)
            {
                string[] code = DataModelDocument.CodeSpans(row[i]).ToArray();
                string[] states = DataModelDocument.CodeSpans(row[i + 1]).ToArray();
                if (code.Length == 1 && states.Length == 2 && TransitionCodeName().IsMatch(code[0]))
                {
                    documented.Add($"{code[0]}: {states[0]} -> {states[1]}");
                }
            }
        }

        return SetComparison.Compare(
            "09 9.1 item 2",
            documented,
            [.. TransitionTable.Shipment.Select(t => $"{t.Code}: {t.From} -> {t.To}"), .. TransitionTable.Payment.Select(t => $"{t.Code}: {t.From} -> {t.To}")]);
    }

    /// <summary>The rows of a transition table of 02 as <c>S1: Placed -&gt; Preparing</c>; the state names are turned into codes by the section's own state table.</summary>
    private static IEnumerable<string> TransitionRows(string section, string prefix)
    {
        IReadOnlyList<IReadOnlyList<string[]>> tables = DataModelDocument.Tables(section);
        Dictionary<string, string> codeOfName = tables[0].ToDictionary(r => r[0], r => r[1].Trim('`'), StringComparer.Ordinal);
        foreach (string[] row in tables[1])
        {
            string[] names = row[1].Split(" → ", StringSplitOptions.TrimEntries);
            string id = row[0].Replace('Ö', 'O');
            string? from = null;
            string? to = null;
            if (!id.StartsWith(prefix, StringComparison.Ordinal) || names.Length != 2 || !codeOfName.TryGetValue(names[0], out from) || !codeOfName.TryGetValue(names[1], out to))
            {
                throw new InvalidOperationException($"The transition row '{row[0]} | {row[1]}' cannot be read.");
            }

            yield return $"{id}: {from} -> {to}";
        }
    }

    private static IEnumerable<string> ShipmentRows(string section) => TransitionRows(section, "S");

    private static IEnumerable<string> PaymentRows(string section) => TransitionRows(section, "O");

    /// <summary>The codes of the first (state) table of a section of 02 5.</summary>
    private static IReadOnlyList<string> StateCodes(string document, string heading)
    {
        return [.. DataModelDocument.Tables(DataModelDocument.Section(document, heading))[0].Select(r => r[1].Trim('`'))];
    }

    [GeneratedRegex(@"^(?:S(?:[1-9]|1[01])|O[1-5])$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex TransitionCodeName();
}
