using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality checks of the check constraints (09 21.4 item 1; T120 KK4): the tables of <c>06 7.3.2</c> and the model's table-specific
/// constraints name the same tables, the code list names the same constraints as the model and the same number of items as the document's
/// row of each table, the phone list of <c>7.3.1</c> and the page address row equal the model's, and every enum column carries its value set.
/// Both directions are checked; the checks that must fail run on a changed copy of the document or of the model's facts (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed class CheckConstraintConsistencyTests
{
    private const string SampleDocument = """
        ### 7.3 Kontrol kısıtları

        - **Enum sütunu** izinli değerlerin kısıtını taşır.
        - **Telefon** on rakamdır: `Company.Phone`, `CompanySettings.ReturnPhone` ve `WhatsAppNumber`, `Order.ShippingPhone` ve `CompanyPhone`.

        **7.3.2 Tabloya özgü kısıtlar**

        | Tablo | Kontrol kısıtları | §4 |
        |---|---|---|
        | `Province` | `Code` iki rakam | §4.3 |
        | `Variant` | İkinci değer doluysa birinci dolu; `Price` > 0; stok boş ya da (0 ≤ stok; ve stok ≥ ayrılan) | §4.6 |
        | `ReturnRejection`, `NotReturnedClosure` | `Quantity` > 0 | §4.27 |
        | `Product`, `Category` | `Slug`'ın biçimi | §4.72.4 |

        Kendine özgü kontrol kısıtı taşımayan tablolar — ortak kısıtlar dışında: `Company` (telefon), `OptionValue`, `CustomPage` (sayfa adresi dışında) ve galeri bağları.

        **7.3.3 Sonraki** metin
        """;

    [Fact]
    public void ReadRows_SampleDocument_CountsTheItemsOutsideParentheses()
    {
        IReadOnlyDictionary<string, int> rows = DocumentedChecks.ReadRows(SampleDocument);

        Assert.Equal(
            new Dictionary<string, int> { ["Province"] = 1, ["Variant"] = 3, ["ReturnRejection"] = 1, ["NotReturnedClosure"] = 1 },
            rows);
    }

    [Fact]
    public void ReadSlugTables_ReadWithoutSpecific_ReadPhones_SampleDocument()
    {
        Assert.Equal(["Product", "Category"], DocumentedChecks.ReadSlugTables(SampleDocument));
        Assert.Equal(
            ["Company", "OptionValue", "CustomPage", .. DocumentedChecks.GalleryLinkTables],
            DocumentedChecks.ReadWithoutSpecific(SampleDocument));
        Assert.Equal(["Company.Phone", "CompanySettings.ReturnPhone", "CompanySettings.WhatsAppNumber", "Order.ShippingPhone", "Order.CompanyPhone"], DocumentedChecks.ReadPhones(SampleDocument));
    }

    [Fact]
    public void Classify_ConstraintsOfTheThreeCommonKindsAndTheRest()
    {
        CheckFact enumSet = new("Order", "ck_orders_order_status", "\"order_status\" IN ('Placed', 'Cancelled')");
        CheckFact phone = new("Branch", "ck_branches_phone_ten_digits", "phone ~ '^[0-9]{10}$'");
        CheckFact slug = new("Product", "ck_products_slug_format", "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
        CheckFact other = new("Variant", "ck_variants_price_above_zero", "price > 0");

        Assert.True(DocumentedChecks.IsEnumSet(enumSet));
        Assert.True(DocumentedChecks.IsPhone(phone));
        Assert.True(DocumentedChecks.IsSlug(slug));
        Assert.Equal([other], DocumentedChecks.Specific([enumSet, phone, slug, other]));
    }

    [Fact]
    public void TablesOfTheDocumentAndTheModel_AgreeOnWhichCarryASpecificConstraint()
    {
        string document = DataModelDocument.Load();
        ModelFacts model = ModelFacts.Shared;

        List<string> problems = TableProblems(document, model);

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void CodeList_AndTheModel_NameTheSameSpecificConstraints_AndTheDocumentsItemCounts()
    {
        List<string> problems = ListProblems(DataModelDocument.Load(), ModelFacts.Shared.Checks);

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void SlugRowOfTheDocument_AndTheModel_NameTheSameTables()
    {
        IReadOnlyList<string> problems = SlugProblems(DataModelDocument.Load(), ModelFacts.Shared.Checks);

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void PhoneBulletOfTheDocument_AndTheModel_NameTheSameColumns()
    {
        IReadOnlyList<string> problems = PhoneProblems(DataModelDocument.Load(), ModelFacts.Shared);

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void EnumColumns_EachCarryTheirValueSet_AndEveryValueSetBelongsToAnEnumColumn()
    {
        List<string> problems = EnumProblems(ModelFacts.Shared);

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    // ---- the checks must fail (T120 KK4; 09 19.2 item 3: on a changed copy, never on the source) ----

    [Fact]
    public void CodeList_AConstraintMissingFromTheModel_IsReported()
    {
        CheckFact[] without = [.. ModelFacts.Shared.Checks.Where(c => c.Name != "ck_variants_price_above_zero")];

        IReadOnlyList<string> problems = ListProblems(DataModelDocument.Load(), without);

        string problem = Assert.Single(problems);
        Assert.Contains("only in the code list: ck_variants_price_above_zero", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void CodeList_AConstraintTheListDoesNotKnow_IsReported()
    {
        CheckFact[] extra = [.. ModelFacts.Shared.Checks, new CheckFact("Variant", "ck_variants_new_rule", "price < 1000")];

        IReadOnlyList<string> problems = ListProblems(DataModelDocument.Load(), extra);

        string problem = Assert.Single(problems);
        Assert.Contains("only in the model: ck_variants_new_rule", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Tables_ADocumentRowDeleted_OrAModelTableThatLostItsConstraints_IsReported()
    {
        string document = DataModelDocument.Load();
        string row = "| `PriceHistory` | `Price` > 0 | §4.11 |\n";
        Assert.Contains(row, document, StringComparison.Ordinal);
        string changed = document.Replace(row, string.Empty, StringComparison.Ordinal);
        ModelFacts model = ModelFacts.Shared;
        CheckFact[] lost = [.. model.Checks.Where(c => c.Table != "Discount" || DocumentedChecks.IsEnumSet(c))];

        IReadOnlyList<string> deleted = TableProblems(changed, model);
        IReadOnlyList<string> lostProblems = TableProblems(document, new ModelFactsSample(model.Tables, lost));

        Assert.Contains(deleted, p => p.Contains("PriceHistory", StringComparison.Ordinal));
        Assert.Contains(lostProblems, p => p.Contains("Discount", StringComparison.Ordinal));
    }

    [Fact]
    public void Items_AnItemAddedToOrRemovedFromARow_IsReported()
    {
        string document = DataModelDocument.Load();
        string row = "| `PriceHistory` | `Price` > 0 | §4.11 |";
        Assert.Contains(row, document, StringComparison.Ordinal);
        string added = document.Replace(row, "| `PriceHistory` | `Price` > 0; `StartedAt` ≥ 0 | §4.11 |", StringComparison.Ordinal);
        string variantRow = Regex.Match(document, @"^\| `Variant` \| (?<text>İkinci değer.*) \| §4\.6 \|$", RegexOptions.Multiline, TimeSpan.FromSeconds(5)).Groups["text"].Value;
        Assert.NotEmpty(variantRow);
        string removed = document.Replace(variantRow, variantRow[..variantRow.LastIndexOf(';')], StringComparison.Ordinal);

        Assert.Single(ListProblems(added, ModelFacts.Shared.Checks), p => p.Contains("PriceHistory", StringComparison.Ordinal));
        Assert.Single(ListProblems(removed, ModelFacts.Shared.Checks), p => p.Contains("Variant", StringComparison.Ordinal));
    }

    [Fact]
    public void Phones_AColumnDroppedFromTheBulletOrTheModel_IsReported()
    {
        string document = DataModelDocument.Load();
        string bullet = "`Branch.Phone`, ";
        Assert.Contains(bullet, document, StringComparison.Ordinal);
        string changed = document.Replace(bullet, string.Empty, StringComparison.Ordinal);
        ModelFacts model = ModelFacts.Shared;
        CheckFact[] lost = [.. model.Checks.Where(c => c.Name != "ck_branches_phone_ten_digits")];

        Assert.Single(PhoneProblems(changed, model), p => p.Contains("only in the model: Branch.Phone", StringComparison.Ordinal));
        Assert.Single(PhoneProblems(document, new ModelFactsSample(model.Tables, lost, model.Properties)), p => p.Contains("only in the document: Branch.Phone", StringComparison.Ordinal));
    }

    [Fact]
    public void Slugs_ATableDroppedFromTheRowOrTheModel_IsReported()
    {
        string document = DataModelDocument.Load();
        Match row = Regex.Match(document, @"\| `Product`, `Category`, `Offering`, `PortfolioItem`, `CustomPage` \|", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
        Assert.True(row.Success);
        string changed = document.Replace(row.Value, "| `Product`, `Category`, `Offering`, `PortfolioItem` |", StringComparison.Ordinal);

        Assert.Single(SlugProblems(changed, ModelFacts.Shared.Checks), p => p.Contains("only in the model: CustomPage", StringComparison.Ordinal));
        Assert.Single(SlugProblems(document, [.. ModelFacts.Shared.Checks.Where(c => c.Name != "ck_offerings_slug_format")]), p => p.Contains("only in the document: Offering", StringComparison.Ordinal));
    }

    [Fact]
    public void Enums_AnEnumColumnWithoutItsValueSet_OrAValueNotInTheEnum_IsReported()
    {
        ModelFacts model = ModelFacts.Shared;
        CheckFact first = model.Checks.First(DocumentedChecks.IsEnumSet);
        CheckFact[] without = [.. model.Checks.Where(c => c != first)];
        CheckFact[] foreign = [.. model.Checks.Select(c => c == first ? c with { Sql = c.Sql[..^1] + ", 'NoSuchValue')" } : c)];

        Assert.Single(EnumProblems(new ModelFactsSample(model.Tables, without, model.Properties)), p => p.Contains("has no value set", StringComparison.Ordinal));
        Assert.Single(EnumProblems(new ModelFactsSample(model.Tables, foreign, model.Properties)), p => p.Contains("'NoSuchValue'", StringComparison.Ordinal));
    }

    // ---- the comparisons ----

    private static List<string> TableProblems(string document, ModelFacts model) => TableProblems(document, new ModelFactsSample(model.Tables, model.Checks));

    private static List<string> TableProblems(string document, ModelFactsSample model)
    {
        string[] rows = [.. DocumentedChecks.ReadRows(document).Keys];
        string[] withoutSpecific = [.. DocumentedChecks.ReadWithoutSpecific(document)];
        string[] withSpecific = [.. DocumentedChecks.Specific(model.Checks).Select(c => c.Table).Distinct(StringComparer.Ordinal)];
        List<string> problems = [.. SetComparison.Compare("06 7.3.2 rows", rows, withSpecific)];
        problems.AddRange(SetComparison.Compare("06 7.3.2 rows and the list without a specific constraint", [.. rows, .. withoutSpecific], model.Tables));
        return problems;
    }

    private static List<string> ListProblems(string document, IEnumerable<CheckFact> checks)
    {
        IReadOnlyDictionary<string, int> rows = DocumentedChecks.ReadRows(document);
        List<string> problems = [.. SetComparison.Compare("06 7.3.2 rows and the code list", rows.Keys, CheckConstraintTable.Specific.Keys)];
        foreach ((string table, (int items, _)) in CheckConstraintTable.Specific.Where(e => rows.ContainsKey(e.Key)))
        {
            if (rows[table] != items)
            {
                problems.Add($"{table}: the document's row states {rows[table]} items and the code list says {items}.");
            }
        }

        problems.AddRange(SetComparison.Compare(
            "specific constraints",
            CheckConstraintTable.Specific.SelectMany(e => e.Value.Constraints),
            DocumentedChecks.Specific(checks).Select(c => c.Name)).Select(p => p.Replace("only in the document", "only in the code list", StringComparison.Ordinal)));
        return problems;
    }

    private static IReadOnlyList<string> SlugProblems(string document, IEnumerable<CheckFact> checks)
    {
        return SetComparison.Compare("06 7.3.2 page address row", DocumentedChecks.ReadSlugTables(document), checks.Where(DocumentedChecks.IsSlug).Select(c => c.Table));
    }

    private static IReadOnlyList<string> PhoneProblems(string document, ModelFacts model) => PhoneProblems(document, new ModelFactsSample(model.Tables, model.Checks, model.Properties));

    private static IReadOnlyList<string> PhoneProblems(string document, ModelFactsSample model)
    {
        return SetComparison.Compare("06 7.3.1 phone bullet", DocumentedChecks.ReadPhones(document), model.Checks.Where(DocumentedChecks.IsPhone).Select(c => DocumentedChecks.PhoneOf(c, model.Properties)));
    }

    private static List<string> EnumProblems(ModelFacts model) => EnumProblems(new ModelFactsSample(model.Tables, model.Checks, model.Properties));

    private static List<string> EnumProblems(ModelFactsSample model)
    {
        List<string> problems = [];
        Dictionary<string, CheckFact> sets = model.Checks.Where(DocumentedChecks.IsEnumSet).ToDictionary(c => $"{c.Table}.{c.Sql.Split(' ')[0].Trim('"')}", StringComparer.Ordinal);
        HashSet<string> enumColumns = new(StringComparer.Ordinal);
        foreach (PropertyFact property in model.Properties.Where(p => (Nullable.GetUnderlyingType(p.ClrType) ?? p.ClrType).IsEnum))
        {
            Type enumType = Nullable.GetUnderlyingType(property.ClrType) ?? property.ClrType;
            string key = $"{property.Table}.{DataModelDocument.Snake(property.Name)}";
            enumColumns.Add(key);
            if (!sets.TryGetValue(key, out CheckFact? set))
            {
                problems.Add($"{property.Table}.{property.Name}: the enum column has no value set constraint (06 7.3.1).");
                continue;
            }

            string[] values = [.. Regex.Matches(set.Sql, "'([^']*)'", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Select(m => m.Groups[1].Value)];
            problems.AddRange(values.Where(v => !Enum.GetNames(enumType).Contains(v)).Select(v => $"{set.Name}: the value '{v}' is not a member of {enumType.Name}."));
        }

        problems.AddRange(sets.Keys.Where(k => !enumColumns.Contains(k)).Select(k => $"{k}: a value set constraint on a column that is not an enum column."));
        return problems;
    }

    /// <summary>A model fact set with a changed list of constraints, for the checks that must fail.</summary>
    private sealed record ModelFactsSample(IReadOnlyList<string> Tables, IReadOnlyList<CheckFact> Checks, IReadOnlyList<PropertyFact>? PropertyList = null)
    {
        public IReadOnlyList<PropertyFact> Properties => PropertyList ?? [];
    }
}
