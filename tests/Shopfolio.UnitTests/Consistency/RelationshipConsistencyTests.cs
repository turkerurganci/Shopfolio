using System.Text.RegularExpressions;

using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality checks of the relationships (09 21.4 item 1; T120 KK1, KK3, KK5): the diagram lines of <c>06 6</c>, the foreign key table of
/// <c>06 7.2</c> and the model's foreign keys are the same set in both directions; the delete behaviors are the three of <c>06 2.2.5</c> and
/// never RESTRICT; the index each link promises exists. The document is read from the repository, the model from the finished context
/// without a database; the checks that must fail run on a changed copy of the text or of the model's facts (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed class RelationshipConsistencyTests
{
    private const string SampleDocument = """
        ## 6. İlişki diyagramı

        Bu bölüm — 3 çizgi, 3 satır; diyagramlar.

        ### 6.1 Okunuş

        | Diyagram | Grup | Çizgi |
        |---|---|---|
        | §6.2 | Birinci | 2 |
        | §6.3 | İkinci | 1 |
        | | **Toplam** | **3** |

        ### 6.2 Birinci

        ```mermaid
        erDiagram
            Province |o--o{ Company : "ProvinceId · kısıtlayıcı"
            Customer |o--o| Cart : "CustomerId · basamaklı"
        ```

        ### 6.3 İkinci

        ```mermaid
        erDiagram
            Order ||--o{ OrderItem : "OrderId · kısıtlayıcı"
        ```

        ## 7. İndeks

        ### 7.2 Yabancı anahtarlar

        **3 bağ** — 1 basamaklı, 0 boşaltılır, 2 kısıtlayıcı. (3 bağ — 1 basamaklı, 0 boşaltılır, 2 kısıtlayıcı)

        | Tablo | Alan | Hedef | Zorunlu | Silme davranışı | Dizin | §4 |
        |---|---|---|---|---|---|---|
        | `Company` | `ProvinceId` | Province | Hayır | kısıtlayıcı | gerekmez — tek satır | §4.1 |
        | `Cart` | `CustomerId` | Customer | Hayır | basamaklı | kendi dizini | §4.15 |
        | `OrderItem` | `OrderId` | Order | Evet | kısıtlayıcı | (`OrderId`, `CreatedAt`) dizininin ilk sütunu | §4.18 |

        ### 7.3 Sonraki
        """;

    [Fact]
    public void ReadDiagrams_SampleDocument_ReadsEachLineWithItsMarks()
    {
        (IReadOnlyList<IReadOnlyList<string>> blocks, IReadOnlyList<string> problems) = DocumentedRelationships.ReadDiagrams(SampleDocument);

        Assert.Empty(problems);
        Assert.Equal(
            [
                ["Company.ProvinceId -> Province | optional | NoAction | many", "Cart.CustomerId -> Customer | optional | Cascade | one"],
                ["OrderItem.OrderId -> Order | required | NoAction | many"],
            ],
            blocks);
    }

    [Fact]
    public void ReadDiagrams_AWordWhereTheBehaviorShouldBe_IsKeptAsTheWord()
    {
        string changed = SampleDocument.Replace("ProvinceId · kısıtlayıcı", "ProvinceId · restrict", StringComparison.Ordinal);

        (IReadOnlyList<IReadOnlyList<string>> blocks, _) = DocumentedRelationships.ReadDiagrams(changed);

        Assert.Equal("Company.ProvinceId -> Province | optional | restrict | many", blocks[0][0]);
    }

    [Fact]
    public void ReadDiagrams_ALineThatIsNotARelationship_IsAProblem()
    {
        string changed = SampleDocument.Replace("Order ||--o{ OrderItem : \"OrderId · kısıtlayıcı\"", "Order ||--o{ OrderItem", StringComparison.Ordinal);

        (_, IReadOnlyList<string> problems) = DocumentedRelationships.ReadDiagrams(changed);

        Assert.Single(problems);
    }

    [Fact]
    public void ReadCounts_SampleDocument_ReadsTheStatedCounts()
    {
        DocumentedCounts counts = DocumentedRelationships.ReadCounts(SampleDocument);

        Assert.Equal(new DocumentedCounts(3, 3, [2, 1], 3, 3, 1, 0, 2), counts, new CountsComparer());
    }

    [Fact]
    public void ReadTable_SampleDocument_ReadsTheKeysAndTheIndexText()
    {
        (IReadOnlyList<string> keys, IReadOnlyDictionary<string, string> indexText) = DocumentedRelationships.ReadTable(SampleDocument);

        Assert.Equal(
            ["Company.ProvinceId -> Province | optional | NoAction", "Cart.CustomerId -> Customer | optional | Cascade", "OrderItem.OrderId -> Order | required | NoAction"],
            keys);
        Assert.Equal("kendi dizini", indexText["Cart.CustomerId -> Customer | optional | Cascade"]);
    }

    [Fact]
    public void Diagrams_AndTheForeignKeysOfTheModel_AreTheSameSet()
    {
        ModelFacts model = ModelFacts.Shared;
        (IReadOnlyList<IReadOnlyList<string>> blocks, IReadOnlyList<string> readProblems) = DocumentedRelationships.ReadDiagrams(DataModelDocument.Load());

        IReadOnlyList<string> problems = [.. readProblems, .. SetComparison.Compare("06 6 diagrams", blocks.SelectMany(b => b), model.ForeignKeys.Select(f => f.DiagramKey))];

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void Diagrams_LineCountsAreTheStatedOnes_AndEqualTheModelsForeignKeyCount()
    {
        string document = DataModelDocument.Load();
        DocumentedCounts counts = DocumentedRelationships.ReadCounts(document);
        (IReadOnlyList<IReadOnlyList<string>> blocks, _) = DocumentedRelationships.ReadDiagrams(document);
        ModelFacts model = ModelFacts.Shared;

        Assert.Equal(counts.PerDiagram, blocks.Select(b => b.Count));
        Assert.Equal(counts.Total, counts.PerDiagram.Sum());
        Assert.Equal(counts.Total, blocks.Sum(b => b.Count));
        Assert.Equal(counts.Total, counts.Lines);
        Assert.Equal(counts.Total, counts.Rows);
        Assert.Equal(counts.Total, counts.Relationships);
        Assert.Equal(counts.Total, model.ForeignKeys.Count);
    }

    [Fact]
    public void ForeignKeyTable_AndTheForeignKeysOfTheModel_AreTheSameSet()
    {
        (IReadOnlyList<string> keys, _) = DocumentedRelationships.ReadTable(DataModelDocument.Load());

        IReadOnlyList<string> problems = SetComparison.Compare("06 7.2 table", keys, ModelFacts.Shared.ForeignKeys.Select(f => f.Key));

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void ForeignKeyTable_BehaviorCounts_AreTheStatedOnesAndTheModels()
    {
        DocumentedCounts counts = DocumentedRelationships.ReadCounts(DataModelDocument.Load());
        IReadOnlyList<ForeignKeyFact> keys = ModelFacts.Shared.ForeignKeys;

        Assert.Equal(counts.Cascade, keys.Count(k => k.Behavior == "Cascade"));
        Assert.Equal(counts.SetNull, keys.Count(k => k.Behavior == "SetNull"));
        Assert.Equal(counts.Restrictive, keys.Count(k => k.Behavior == "NoAction"));
    }

    [Fact]
    public void Model_UsesTheThreeBehaviorsOnly_AndNoRestrict()
    {
        IReadOnlyList<string> problems = [.. BehaviorProblems(ModelFacts.Shared.ForeignKeys), .. MigrationRestrictProblems(MigrationTexts())];

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void ForeignKeyIndexes_ThatTheTablePromises_ExistInTheModel()
    {
        (IReadOnlyList<string> keys, IReadOnlyDictionary<string, string> indexText) = DocumentedRelationships.ReadTable(DataModelDocument.Load());
        ModelFacts model = ModelFacts.Shared;

        IReadOnlyList<string> problems = DocumentedRelationships.CheckIndexPromises(model.ForeignKeys, indexText, model.Indexes);

        Assert.Equal(keys.Count, model.ForeignKeys.Count);
        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    // ---- the checks must fail (T120 KK1, KK3, KK5; 09 19.2 item 3: on a changed copy, never on the source) ----

    [Fact]
    public void Diagrams_ADeletedLine_IsReported()
    {
        string document = DataModelDocument.Load();
        string line = "    Province |o--o{ Company : \"ProvinceId · kısıtlayıcı\"\n";
        Assert.Contains(line, document, StringComparison.Ordinal);
        string changed = document.Replace(line, string.Empty, StringComparison.Ordinal);

        (IReadOnlyList<IReadOnlyList<string>> blocks, _) = DocumentedRelationships.ReadDiagrams(changed);
        IReadOnlyList<string> problems = SetComparison.Compare("06 6 diagrams", blocks.SelectMany(b => b), ModelFacts.Shared.ForeignKeys.Select(f => f.DiagramKey));

        string problem = Assert.Single(problems);
        Assert.Contains("only in the model: Company.ProvinceId -> Province", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Diagrams_AnExtraLineNoModelForeignKeyHas_IsReported()
    {
        string document = DataModelDocument.Load();
        string line = "    Province |o--o{ Company : \"ProvinceId · kısıtlayıcı\"\n";
        string changed = document.Replace(line, line + "    Country |o--o{ Company : \"CountryId · kısıtlayıcı\"\n", StringComparison.Ordinal);

        (IReadOnlyList<IReadOnlyList<string>> blocks, _) = DocumentedRelationships.ReadDiagrams(changed);
        IReadOnlyList<string> problems = SetComparison.Compare("06 6 diagrams", blocks.SelectMany(b => b), ModelFacts.Shared.ForeignKeys.Select(f => f.DiagramKey));

        string problem = Assert.Single(problems);
        Assert.Contains("only in the document: Company.CountryId -> Country", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Diagrams_AChangedMarkOrBehavior_IsReported()
    {
        string document = DataModelDocument.Load();
        string line = "    Product ||--o{ Variant : \"ProductId · basamaklı\"";
        Assert.Contains(line, document, StringComparison.Ordinal);
        string behavior = document.Replace(line, line.Replace("basamaklı", "kısıtlayıcı", StringComparison.Ordinal), StringComparison.Ordinal);
        string mark = document.Replace(line, line.Replace("||--o{", "|o--o{", StringComparison.Ordinal), StringComparison.Ordinal);

        foreach (string changed in new[] { behavior, mark })
        {
            (IReadOnlyList<IReadOnlyList<string>> blocks, _) = DocumentedRelationships.ReadDiagrams(changed);
            IReadOnlyList<string> problems = SetComparison.Compare("06 6 diagrams", blocks.SelectMany(b => b), ModelFacts.Shared.ForeignKeys.Select(f => f.DiagramKey));
            Assert.Equal(2, problems.Count);
        }
    }

    [Fact]
    public void ForeignKeyTable_ABehaviorChangedInTheModel_IsReported()
    {
        (IReadOnlyList<string> keys, _) = DocumentedRelationships.ReadTable(DataModelDocument.Load());
        List<ForeignKeyFact> changed = [.. ModelFacts.Shared.ForeignKeys];
        int index = changed.FindIndex(f => f is { Table: "Variant", Field: "ProductId" });
        changed[index] = changed[index] with { Behavior = "NoAction" };

        IReadOnlyList<string> problems = SetComparison.Compare("06 7.2 table", keys, changed.Select(f => f.Key));

        Assert.Equal(2, problems.Count);
        Assert.Contains(problems, p => p.Contains("only in the document: Variant.ProductId -> Product | required | Cascade", StringComparison.Ordinal));
        Assert.Contains(problems, p => p.Contains("only in the model: Variant.ProductId -> Product | required | NoAction", StringComparison.Ordinal));
    }

    [Fact]
    public void ForeignKeyTable_ABehaviorChangedInTheDocument_IsReported()
    {
        string document = DataModelDocument.Load();
        string row = "| `Variant` | `ProductId` | Product | Evet | basamaklı |";
        Assert.Contains(row, document, StringComparison.Ordinal);
        string changed = document.Replace(row, row.Replace("basamaklı", "boşaltılır", StringComparison.Ordinal), StringComparison.Ordinal);

        (IReadOnlyList<string> keys, _) = DocumentedRelationships.ReadTable(changed);
        IReadOnlyList<string> problems = SetComparison.Compare("06 7.2 table", keys, ModelFacts.Shared.ForeignKeys.Select(f => f.Key));

        Assert.Equal(2, problems.Count);
    }

    [Fact]
    public void ForeignKeyTable_ADeletedRow_IsReported()
    {
        string document = DataModelDocument.Load();
        Match match = Regex.Match(document, @"^\| `Option` \| `ProductId` \|.*\n", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
        Assert.True(match.Success);
        string changed = document.Remove(match.Index, match.Length);

        (IReadOnlyList<string> keys, _) = DocumentedRelationships.ReadTable(changed);
        IReadOnlyList<string> problems = SetComparison.Compare("06 7.2 table", keys, ModelFacts.Shared.ForeignKeys.Select(f => f.Key));

        string problem = Assert.Single(problems);
        Assert.Contains("only in the model: Option.ProductId -> Product", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Behaviors_ARestrictiveForeignKeyOrMigrationText_IsReported()
    {
        List<ForeignKeyFact> changed = [.. ModelFacts.Shared.ForeignKeys];
        changed[0] = changed[0] with { Behavior = "Restrict" };

        Assert.Single(BehaviorProblems(changed));
        Assert.Single(MigrationRestrictProblems(["x.AddForeignKey(onDelete: ReferentialAction.Restrict);"]));
        Assert.Single(MigrationRestrictProblems(["migrationBuilder.Sql(\"ALTER TABLE t ADD FOREIGN KEY (a) REFERENCES u (id) ON DELETE RESTRICT;\");"]));
        Assert.Empty(MigrationRestrictProblems(["x.AddForeignKey(onDelete: ReferentialAction.NoAction);"]));
    }

    [Fact]
    public void ForeignKeyIndexes_ADeletedOwnIndexOrComposite_IsReported()
    {
        (_, IReadOnlyDictionary<string, string> indexText) = DocumentedRelationships.ReadTable(DataModelDocument.Load());
        ModelFacts model = ModelFacts.Shared;
        IndexFact own = model.Indexes.Single(i => i is { Table: "Product", Columns: "origin_country_id" });
        IndexFact composite = model.Indexes.Single(i => i is { Table: "OptionValue", Columns: "option_id,sort_order" });
        IndexFact unique = model.Indexes.Single(i => i is { Table: "ProductImage", Columns: "product_id,image_id" });

        foreach ((IndexFact removed, string expected) in new[] { (own, "Product.OriginCountryId"), (composite, "OptionValue.OptionId"), (unique, "ProductImage.ProductId") })
        {
            IndexFact[] without = [.. model.Indexes.Where(i => i != removed)];

            IReadOnlyList<string> problems = DocumentedRelationships.CheckIndexPromises(model.ForeignKeys, indexText, without);

            Assert.Contains(problems, p => p.StartsWith(expected, StringComparison.Ordinal));
        }
    }

    private static IReadOnlyList<string> BehaviorProblems(IEnumerable<ForeignKeyFact> keys)
    {
        string[] allowed = ["Cascade", "SetNull", "NoAction"];
        return [.. keys.Where(k => !allowed.Contains(k.Behavior)).Select(k => $"{k.Key}: the delete behavior is not one of the three of 06 2.2.5 (RESTRICT is never used).")];
    }

    private static IReadOnlyList<string> MigrationRestrictProblems(IEnumerable<string> migrationTexts)
    {
        return [.. migrationTexts.Where(t => t.Contains("ReferentialAction.Restrict", StringComparison.Ordinal) || t.Contains("ON DELETE RESTRICT", StringComparison.OrdinalIgnoreCase)).Select(_ => "A migration text uses RESTRICT (06 2.2.5, 7.2.1).")];
    }

    private static IEnumerable<string> MigrationTexts()
    {
        string folder = Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web", "Infrastructure", "Migrations");
        return Directory.EnumerateFiles(folder, "*.cs").Select(File.ReadAllText);
    }

    private sealed class CountsComparer : IEqualityComparer<DocumentedCounts>
    {
        public bool Equals(DocumentedCounts? x, DocumentedCounts? y)
        {
            return x is not null && y is not null && x.Lines == y.Lines && x.Rows == y.Rows && x.PerDiagram.SequenceEqual(y.PerDiagram) && x.Total == y.Total
                && x.Relationships == y.Relationships && x.Cascade == y.Cascade && x.SetNull == y.SetNull && x.Restrictive == y.Restrictive;
        }

        public int GetHashCode(DocumentedCounts obj) => obj.Total;
    }
}
