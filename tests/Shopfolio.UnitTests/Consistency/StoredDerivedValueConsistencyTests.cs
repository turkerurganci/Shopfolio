using System.Text.RegularExpressions;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality checks of the stored derived values (09 21.4 item 1; T120 KK6): the model stores a derived value only when the denormalization
/// inventory of <c>06 2.7.1</c> names its kind — reserved quantity and coupon counts (1), the search form of the product name (2), the
/// normalized identity columns (3) and the level of a category (4) — and the panel marks of <c>2.7.3</c> are not stored at all. A property
/// whose name looks like a stored derived value (a counter, a total, a mark, a normalized form, a cache) must be in the inventory or in the
/// short list of the values that are primary data; both directions are checked. The checks that must fail run on a changed copy of the
/// model's properties and of the document (09 19.2 item 3).
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class StoredDerivedValueConsistencyTests
{
    /// <summary>The stored derived values of the inventory (06 2.7.1), each with the number of its kind (the row of the table).</summary>
    private static readonly Dictionary<string, int> Inventory = new(StringComparer.Ordinal)
    {
        ["Variant.ReservedQuantity"] = 1,
        ["Coupon.UsedCount"] = 1,
        ["Coupon.ReservedCount"] = 1,
        ["Product.NameSearchForm"] = 2,
        ["Variant.NormalizedSKU"] = 3,
        ["Coupon.NormalizedCode"] = 3,
        ["Customer.NormalizedEmail"] = 3,
        ["Admin.NormalizedEmail"] = 3,
        ["AdminInvitation.NormalizedEmail"] = 3,
        ["LoginRecord.NormalizedEmail"] = 3,
        ["PendingEmailChange.NormalizedNewEmail"] = 3,
        ["Order.NormalizedContactEmail"] = 3,
        ["AttemptRecord.NormalizedValue"] = 3,
        ["Category.Level"] = 4,
    };

    /// <summary>
    /// Properties that look derived and are not: the record's own data (06 2.7.4 and the field tables of 4), each with the reason. This list is
    /// not an exemption for a new stored value — a property joins it only when the document's field table states it as primary data.
    /// </summary>
    private static readonly Dictionary<string, string> NotDerived = new(StringComparer.Ordinal)
    {
        ["OrderItem.DownloadCount"] = "the download counter (2.7.4; K-884)",
        ["ChannelStateRecord.ConsecutiveChannelErrorCount"] = "the channel state record (2.7.4; K-913)",
        ["DispatchRecord.AttemptCount"] = "the attempt count of the dispatch record (4.58; K-1028 (1))",
        ["Job.AttemptCount"] = "the attempt count of the job row (4.59; K-1028 (6))",
        ["DisposalRecord.Count"] = "the number of the deleted rows, written once with the disposal (4.69; K-975)",
        ["Order.Total"] = "the amount the customer approved, frozen on the order (2.7.4; 02 3.23.4)",
        ["Cart.IsPaymentUnavailableNoticePending"] = "a notice whose source is not in the records, cleared on display (4.15; K-1183)",
        ["CartItem.IsMergeNoticePending"] = "a notice whose source is not in the records, cleared on display (4.16; K-1183)",
    };

    /// <summary>A property name that looks like a stored derived value: a normalized form, a reserved or used count, a counter, a total, a level, a search form, a mark, a cache or a pending flag.</summary>
    [GeneratedRegex(@"^(Normalized|Reserved|Used(?!At$))|(SearchForm|Level|Count|Counter|Total|Mark|Cache|Cached|Pending)$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex Suspicious();

    [Fact]
    public void Inventory_KindsAreTheFourOfTheDocument()
    {
        string document = DataModelDocument.Load();

        Assert.Equal([1, 2, 3, 4], Inventory.Values.Distinct().Order());
        Assert.Equal(4, StoredRows(document).Count);
    }

    [Fact]
    public void StoredDerivedValues_OfTheModel_AreExactlyTheInventory()
    {
        List<string> problems = Problems(ModelFacts.Shared.Properties, DataModelDocument.Load());

        Assert.True(problems.Count == 0, string.Join(Environment.NewLine, problems));
    }

    [Fact]
    public void PanelMarks_OfTheDocument_AreNotStoredByTheModel()
    {
        string document = DataModelDocument.Load();

        Assert.NotEmpty(Marks(document));
        Assert.Empty(MarkProblems(ModelFacts.Shared.Properties, document));
    }

    // ---- the checks must fail (T120 KK6; 09 19.2 item 3: on a changed copy, never on the source) ----

    [Fact]
    public void Model_AStoredValueTheInventoryDoesNotName_IsReported()
    {
        PropertyFact[] extra = [.. ModelFacts.Shared.Properties, new PropertyFact("Variant", "StockCache", typeof(int))];

        string problem = Assert.Single(Problems(extra, DataModelDocument.Load()));
        Assert.Contains("only in the model: Variant.StockCache", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Model_AnInventoryValueTheModelLost_IsReported()
    {
        PropertyFact[] without = [.. ModelFacts.Shared.Properties.Where(p => !(p.Table == "Category" && p.Name == "Level"))];

        string problem = Assert.Single(Problems(without, DataModelDocument.Load()));
        Assert.Contains("only in the inventory: Category.Level", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Model_ADerivedValueMovedToTheNotDerivedList_IsReportedWhenItIsGone()
    {
        PropertyFact[] without = [.. ModelFacts.Shared.Properties.Where(p => !(p.Table == "Order" && p.Name == "Total"))];

        string problem = Assert.Single(Problems(without, DataModelDocument.Load()));
        Assert.Contains("only in the inventory: Order.Total", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Document_AFifthKindInTheInventoryTable_IsReported()
    {
        string document = DataModelDocument.Load();
        string row = "| Kategorinin düzeyi (`Category.Level`) |";
        Assert.Contains(row, document, StringComparison.Ordinal);
        string changed = document.Replace(row, "| Ürün stok önbelleği (`Variant.StockCache`) | x | y | z |\n" + row, StringComparison.Ordinal);

        string problem = Assert.Single(Problems(ModelFacts.Shared.Properties, changed));
        Assert.Contains("the document's table has 5 rows", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Marks_APanelMarkStoredAsAProperty_IsReported()
    {
        PropertyFact[] withMark = [.. ModelFacts.Shared.Properties, new PropertyFact("Order", "EmailUndeliveredMark", typeof(bool))];

        string problem = Assert.Single(MarkProblems(withMark, DataModelDocument.Load()));
        Assert.Contains("Order.EmailUndeliveredMark", problem, StringComparison.Ordinal);
    }

    [Fact]
    public void Marks_TheTableOfTheDocument_IsReadWithEveryMarkName()
    {
        Assert.Equal(
            ["EmailUndeliveredMark", "PaymentOutcomeUnknownMark", "RefundOutcomePendingMark", "RefundFailedMark", "ReturnPendingMark", "IbanPendingMark", "EmailCorrectedMark"],
            Marks(DataModelDocument.Load()));
    }

    // ---- the comparisons ----

    private static List<string> Problems(IEnumerable<PropertyFact> properties, string document)
    {
        string[] suspects = [.. properties.Where(p => Suspicious().IsMatch(p.Name)).Select(p => $"{p.Table}.{p.Name}").Distinct(StringComparer.Ordinal)];
        string[] known = [.. Inventory.Keys, .. NotDerived.Keys];
        List<string> problems = [];
        problems.AddRange(suspects.Except(known, StringComparer.Ordinal).Select(k => $"stored derived values: only in the model: {k}"));
        problems.AddRange(known.Except(suspects, StringComparer.Ordinal).Select(k => $"stored derived values: only in the inventory: {k}"));
        int rows = StoredRows(document).Count;
        if (rows != Inventory.Values.Distinct().Count())
        {
            problems.Add($"stored derived values: the document's table has {rows} rows and the inventory names {Inventory.Values.Distinct().Count()} kinds (06 2.7.1).");
        }

        return problems;
    }

    private static List<string> MarkProblems(IEnumerable<PropertyFact> properties, string document)
    {
        HashSet<string> marks = [.. Marks(document)];
        return [.. properties.Where(p => marks.Contains(p.Name)).Select(p => $"{p.Table}.{p.Name}: a panel mark of 06 2.7.3 is stored as a property.")];
    }

    /// <summary>The body rows of the first table of 06 2.7: the stored derived values.</summary>
    private static IReadOnlyList<string[]> StoredRows(string document)
    {
        return DataModelDocument.Tables(DataModelDocument.Section(document, "### 2.7 "))[0];
    }

    /// <summary>The panel marks of 06 2.7.3: the code spans of the first cell of the table whose rows are all marks.</summary>
    private static IReadOnlyList<string> Marks(string document)
    {
        IReadOnlyList<string[]> rows = [.. DataModelDocument.Tables(DataModelDocument.Section(document, "### 2.7 ")).First(t => t.Count > 0 && t.All(r => r[0].Contains("Mark`", StringComparison.Ordinal)))];
        return [.. rows.SelectMany(r => DataModelDocument.CodeSpans(r[0]))];
    }
}
