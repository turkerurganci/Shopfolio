using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality checks of the indexes (09 21.4 item 1; T120 KK2, KK5): the unique index table of <c>06 7.1</c> and the unique indexes of the
/// database, the filters of the filtered ones included; the query index table of <c>06 7.5.1</c> and the database's other indexes. Both
/// directions are checked. The database's indexes are the model's and the ones only the migrations create (the single-row indexes and the
/// expression index). The checks that must fail run on a changed copy of the document or of the model's facts (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed class IndexConsistencyTests
{
    private const string SampleUnique = """
        ### 7.1 Tekillikler

        | Tablo | Tekil dizin | Filtre | §4 |
        |---|---|---|---|
        | `Company` | Tek satır | — | §4.1 |
        | `Variant` | (`ProductId`, `FirstOptionValueId`, `SecondOptionValueId`) — `NULLS NOT DISTINCT` | — | §4.6 |
        | `StockReservation` | `OrderId` | `CouponId` dolu | §4.14 |
        | `CartItem` | (`CartId`, `VariantId`) | `RemovedProductName` boş | §4.16 |
        | `RefundIbanRequest` | `RefundId` — kalemsiz | `OrderItemId`, `FulfilledAt` ve `ClosedAt` boş | §4.29 |
        | `Refund` | `OrderId` — kargo | `IsShippingFeeIncluded` doğru ve `VoidedAt` boş | §4.33 |
        | `RefundRecord` | `RefundId` — açık | `State` `Sending` ya da `OutcomePending` | §4.34 |
        | `RefundRecord` | `RefundId` — başarılı | `State` `Succeeded` | §4.34 |
        | `PaymentRecord` | `ProviderTransactionId` | dolu satırlar | §4.32 |

        **7.1.2 Sonraki** metin
        """;

    private const string SampleQuery = """
        ### 7.5 Sorgu dizinleri

        | Tablo | Dizin | Amaç | §4 |
        |---|---|---|---|
        | `Product` | `NameSearchForm` — trigram (`pg_trgm`) | Arama | §4.5 |
        | `Product` | (`PublishStatus`, `CreatedAt`, `Id`) | Liste | §4.5 |
        | `Cart` | `LastAccessedAt` — misafir sepetlerinde filtreli | Temizlik | §4.15 |
        | `Offering`, `PortfolioItem` | (`IsShownOnHomepage`, `SortOrder`) — işaretli ve yayındaki satırlarda filtreli | Ana sayfa | §4.47 |
        | `LinkRecord` | `ExpiresAt` — `PreviousEmail` dolu satırlarda filtreli | Temizlik | §4.42 |
        | `RefundIban` | `ScheduledDeletionAt` — dolu satırlarda filtreli | Tarama | §4.31 |
        | `ContactRequest` | (`ContactTopic`, `COALESCE(LastClosedAt, CreatedAt)`) — ifade dizini | Tarama | §4.57 |

        **7.5.2 Sonraki** metin
        """;

    [Fact]
    public void ReadUnique_SampleDocument_ReadsColumnsFiltersAndTheNullsRule()
    {
        (IReadOnlyList<IndexFact> facts, IReadOnlyList<string> problems) = DocumentedIndexes.ReadUnique(SampleUnique);

        Assert.Empty(problems);
        Assert.Equal(
            [
                "Company ((true)) unique single-row",
                "Variant (product_id,first_option_value_id,second_option_value_id) unique nulls-not-distinct",
                "StockReservation (order_id) unique where coupon_id IS NOT NULL",
                "CartItem (cart_id,variant_id) unique where removed_product_name IS NULL",
                "RefundIbanRequest (refund_id) unique where closed_at IS NULL AND fulfilled_at IS NULL AND order_item_id IS NULL",
                "Refund (order_id) unique where is_shipping_fee_included AND voided_at IS NULL",
                "RefundRecord (refund_id) unique where state IN ('Sending', 'OutcomePending')",
                "RefundRecord (refund_id) unique where state = 'Succeeded'",
                "PaymentRecord (provider_transaction_id) unique where provider_transaction_id IS NOT NULL",
            ],
            facts.Select(f => f.Key));
    }

    [Fact]
    public void ReadUnique_AFilterTheReaderDoesNotKnow_IsAProblem()
    {
        string changed = SampleUnique.Replace("`CouponId` dolu", "`CouponId` bazen", StringComparison.Ordinal);

        (IReadOnlyList<IndexFact> facts, IReadOnlyList<string> problems) = DocumentedIndexes.ReadUnique(changed);

        Assert.Equal(8, facts.Count);
        Assert.Single(problems);
    }

    [Fact]
    public void ReadQuery_SampleDocument_ReadsColumnsFiltersKindsAndTableLists()
    {
        (IReadOnlyList<IndexFact> facts, IReadOnlyList<string> problems) = DocumentedIndexes.ReadQuery(SampleQuery);

        Assert.Empty(problems);
        Assert.Equal(
            [
                "Product (name_search_form) trigram",
                "Product (publish_status,created_at,id)",
                "Cart (last_accessed_at) where guest_cart_key_hash IS NOT NULL",
                "Offering (is_shown_on_homepage,sort_order) where is_shown_on_homepage AND publish_status = 'Published'",
                "PortfolioItem (is_shown_on_homepage,sort_order) where is_shown_on_homepage AND publish_status = 'Published'",
                "LinkRecord (expires_at) where previous_email IS NOT NULL",
                "RefundIban (scheduled_deletion_at) where scheduled_deletion_at IS NOT NULL",
                "ContactRequest (contact_topic,coalesce(last_closed_at,created_at)) expression",
            ],
            facts.Select(f => f.Key));
    }

    [Fact]
    public void ReadQuery_AFilterPhraseTheReaderDoesNotKnow_IsAProblem()
    {
        string changed = SampleQuery.Replace("misafir sepetlerinde filtreli", "yeni sepetlerde filtreli", StringComparison.Ordinal);

        (_, IReadOnlyList<string> problems) = DocumentedIndexes.ReadQuery(changed);

        Assert.Single(problems);
    }

    [Fact]
    public void ReadRawIndexesOf_SingleRowAndExpressionIndexes_AreReadFromMigrationText()
    {
        Dictionary<string, string> entities = new(StringComparer.Ordinal) { ["companies"] = "Company", ["contact_requests"] = "ContactRequest" };
        const string text = """
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_companies_single_row ON public.companies ((true));");
            migrationBuilder.Sql("CREATE INDEX ix_contact_requests_x ON public.contact_requests (contact_topic, (COALESCE(last_closed_at, created_at)));");
            """;

        IReadOnlyList<IndexFact> indexes = ModelFacts.ReadRawIndexesOf(text, entities);

        Assert.Equal(["Company ((true)) unique single-row", "ContactRequest (contact_topic,coalesce(last_closed_at,created_at)) expression"], indexes.Select(i => i.Key));
    }

    [Fact]
    public void UniqueIndexTable_AndTheUniqueIndexesOfTheModel_AreTheSameSet()
    {
        (IReadOnlyList<IndexFact> facts, IReadOnlyList<string> readProblems) = DocumentedIndexes.ReadUnique(DataModelDocument.Load());

        IReadOnlyList<string> problems = [.. readProblems, .. DocumentedIndexes.Compare("06 7.1 table", facts, UniqueIndexesOf(ModelFacts.Shared))];

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void QueryIndexTable_AndTheOtherIndexesOfTheModel_AreTheSameSet()
    {
        (IReadOnlyList<IndexFact> facts, IReadOnlyList<string> readProblems) = DocumentedIndexes.ReadQuery(DataModelDocument.Load());

        IReadOnlyList<string> problems = [.. readProblems, .. DocumentedIndexes.Compare("06 7.5.1 table", facts, QueryIndexesOf(ModelFacts.Shared))];

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    // ---- the checks must fail (T120 KK2, KK5; 09 19.2 item 3: on a changed copy, never on the source) ----

    [Fact]
    public void UniqueIndexTable_ADeletedRow_IsReported()
    {
        string document = DataModelDocument.Load();
        Match row = Regex.Match(document, @"^\| `Coupon` \| `NormalizedCode` \|.*\n", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
        Assert.True(row.Success);

        (IReadOnlyList<IndexFact> facts, _) = DocumentedIndexes.ReadUnique(document.Remove(row.Index, row.Length));
        IReadOnlyList<string> problems = DocumentedIndexes.Compare("06 7.1 table", facts, UniqueIndexesOf(ModelFacts.Shared));

        string problem = Assert.Single(problems);
        Assert.Contains("only in the model: Coupon (normalized_code) unique", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void UniqueIndexTable_AnExtraRowAndAChangedFilter_AreReported()
    {
        string document = DataModelDocument.Load();
        string row = "| `CartItem` | (`CartId`, `VariantId`) | `RemovedProductName` boş | §4.16 |";
        Assert.Contains(row, document, StringComparison.Ordinal);
        string extra = document.Replace(row, row + "\n| `CartItem` | `Quantity` | — | §4.16 |", StringComparison.Ordinal);
        string filter = document.Replace(row, row.Replace("`RemovedProductName` boş", "`RemovedProductName` dolu", StringComparison.Ordinal), StringComparison.Ordinal);

        (IReadOnlyList<IndexFact> extraFacts, _) = DocumentedIndexes.ReadUnique(extra);
        (IReadOnlyList<IndexFact> filterFacts, _) = DocumentedIndexes.ReadUnique(filter);

        string added = Assert.Single(DocumentedIndexes.Compare("06 7.1 table", extraFacts, UniqueIndexesOf(ModelFacts.Shared)));
        Assert.Contains("only in the document: CartItem (quantity) unique", added, StringComparison.Ordinal);
        IReadOnlyList<string> changed = DocumentedIndexes.Compare("06 7.1 table", filterFacts, UniqueIndexesOf(ModelFacts.Shared));
        Assert.Equal(2, changed.Count);
        Assert.Contains(changed, p => p.Contains("where removed_product_name IS NOT NULL", StringComparison.Ordinal));
    }

    [Fact]
    public void UniqueIndexTable_AFilterChangedInTheModel_IsReported()
    {
        (IReadOnlyList<IndexFact> facts, _) = DocumentedIndexes.ReadUnique(DataModelDocument.Load());
        List<IndexFact> changed = [.. UniqueIndexesOf(ModelFacts.Shared)];
        int index = changed.FindIndex(i => i is { Table: "ProductImage", Columns: "product_id", Unique: true });
        changed[index] = changed[index] with { Filter = string.Empty };

        IReadOnlyList<string> problems = DocumentedIndexes.Compare("06 7.1 table", facts, changed);

        Assert.Equal(2, problems.Count);
        Assert.Contains(problems, p => p.Contains("only in the document: ProductImage (product_id) unique where is_main", StringComparison.Ordinal));
    }

    [Fact]
    public void UniqueIndexTable_ASingleRowIndexOrAUniqueIndexMissingInTheModel_IsReported()
    {
        (IReadOnlyList<IndexFact> facts, _) = DocumentedIndexes.ReadUnique(DataModelDocument.Load());
        IndexFact[] singleRowMissing = [.. UniqueIndexesOf(ModelFacts.Shared).Where(i => !(i.Kind == "single-row" && i.Table == "Branding"))];
        IndexFact[] uniqueMissing = [.. UniqueIndexesOf(ModelFacts.Shared).Where(i => !(i.Table == "Order" && i.Columns == "order_number"))];

        Assert.Single(DocumentedIndexes.Compare("06 7.1 table", facts, singleRowMissing));
        Assert.Single(DocumentedIndexes.Compare("06 7.1 table", facts, uniqueMissing));
    }

    [Fact]
    public void QueryIndexTable_ADeletedRow_IsReported()
    {
        string document = DataModelDocument.Load();
        Match row = Regex.Match(document, @"^\| `Session` \| `LastActivityAt` \|.*\n", RegexOptions.Multiline, TimeSpan.FromSeconds(5));
        Assert.True(row.Success);

        (IReadOnlyList<IndexFact> facts, _) = DocumentedIndexes.ReadQuery(document.Remove(row.Index, row.Length));
        IReadOnlyList<string> problems = DocumentedIndexes.Compare("06 7.5.1 table", facts, QueryIndexesOf(ModelFacts.Shared));

        string problem = Assert.Single(problems);
        Assert.Contains("only in the model: Session (last_activity_at)", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void QueryIndexTable_APlainIndexOnALinkTheDocumentDoesNotPromiseAnOwnIndexFor_IsReported()
    {
        (IReadOnlyList<IndexFact> facts, _) = DocumentedIndexes.ReadQuery(DataModelDocument.Load());
        IndexFact[] extra = [.. ModelFacts.Shared.Indexes, new IndexFact("OptionValue", "option_id", false, string.Empty, false, "plain")];

        IReadOnlyList<string> problems = DocumentedIndexes.Compare("06 7.5.1 table", facts, QueryIndexesOf(ModelFacts.Shared.ForeignKeys, extra, DataModelDocument.Load()));

        string problem = Assert.Single(problems);
        Assert.Contains("only in the model: OptionValue (option_id)", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void QueryIndexTable_AnIndexMissingFromTheModelOrWithAChangedFilter_IsReported()
    {
        (IReadOnlyList<IndexFact> facts, _) = DocumentedIndexes.ReadQuery(DataModelDocument.Load());
        IndexFact[] missing = [.. QueryIndexesOf(ModelFacts.Shared).Where(i => !(i.Table == "Order" && i.Columns == "ip_address,created_at"))];
        List<IndexFact> changed = [.. QueryIndexesOf(ModelFacts.Shared)];
        int index = changed.FindIndex(i => i is { Table: "Job", Columns: "job_state" });
        changed[index] = changed[index] with { Filter = "job_state = 'Pending'" };

        IReadOnlyList<string> missingProblems = DocumentedIndexes.Compare("06 7.5.1 table", facts, missing);
        IReadOnlyList<string> changedProblems = DocumentedIndexes.Compare("06 7.5.1 table", facts, changed);

        Assert.Single(missingProblems);
        Assert.Contains("only in the document: Order (ip_address,created_at)", missingProblems[0], StringComparison.Ordinal);
        Assert.Equal(2, changedProblems.Count);
    }

    /// <summary>The unique indexes of the database: <c>06 7.1</c> lists every one of them, the one-to-one links' and the single-row indexes included.</summary>
    private static IEnumerable<IndexFact> UniqueIndexesOf(ModelFacts model) => model.Indexes.Where(i => i.Unique);

    /// <summary>
    /// The other indexes of the database (<c>06 7.5.1</c>): not unique, and not the plain index a link has on its own column (that one is a
    /// promise of <c>7.2.1</c>, checked with the links). Only the links whose "Dizin" column promises the plain index ("kendi dizini" or "... ve düz dizini") are left out: a plain single-column
    /// index on any other link is not promised by the document, so it belongs to the <c>7.5.1</c> comparison and breaks it.
    /// </summary>
    private static IEnumerable<IndexFact> QueryIndexesOf(ModelFacts model) => QueryIndexesOf(model.ForeignKeys, model.Indexes, DataModelDocument.Load());

    private static IEnumerable<IndexFact> QueryIndexesOf(IReadOnlyList<ForeignKeyFact> foreignKeys, IReadOnlyList<IndexFact> indexes, string document)
    {
        (_, IReadOnlyDictionary<string, string> indexText) = DocumentedRelationships.ReadTable(document);
        HashSet<string> ownIndexes =
        [
            .. foreignKeys
                .Where(f => indexText.TryGetValue(f.Key, out string? text) && (text.StartsWith("kendi dizini", StringComparison.Ordinal) || text.EndsWith("düz dizini", StringComparison.Ordinal)))
                .Select(f => $"{f.Table}|{DataModelDocument.Snake(f.Field)}"),
        ];
        return indexes.Where(i => !i.Unique && !(i.Kind == "plain" && i.Filter.Length == 0 && ownIndexes.Contains($"{i.Table}|{i.Columns}")));
    }
}
