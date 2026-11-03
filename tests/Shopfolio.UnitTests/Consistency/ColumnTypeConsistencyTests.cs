using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality check of the storage types (09 21.4 item 1; T101 KK4): the table of <c>06 2.5.1</c> and the code's list of storage
/// types name the same set; a type added to one and not the other fails.
/// </summary>
[Trait("Consistency", "true")]
public sealed class ColumnTypeConsistencyTests
{
    private const string SampleDocument = """
        **2.5.1 Türler ve ölçek tek tablodadır.**

        | Değer | Tür | Kural | Kaynak |
        |---|---|---|---|
        | An | `timestamptz` (UTC) | kural | K-870 |
        | Metin | `varchar(n)` ya da `text` | kural | K-1013 |
        | Telefon | metin, on hane | kural | K-1013 |

        **2.5.2 Sonraki paragraf.**

        | Başka | `ignored` | tablo |
        """;

    [Fact]
    public void ReadDocumentedTypes_SampleDocument_ReadsOnlyTheTableOfTheParagraph()
    {
        IReadOnlySet<string> types = ColumnTypeTable.ReadDocumentedTypes(SampleDocument);

        Assert.Equal(new HashSet<string> { "timestamptz", "varchar(n)", "text" }, types);
    }

    [Fact]
    public void DataModelDocument_AndTheCodeList_NameTheSameStorageTypes()
    {
        string document = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "06_DATA_MODEL.md"));

        IReadOnlySet<string> documented = ColumnTypeTable.ReadDocumentedTypes(document);

        Assert.Equal(ColumnTypeTable.Documented.Order(StringComparer.Ordinal), documented.Order(StringComparer.Ordinal));
    }

    [Fact]
    public void StoreTypeOf_VarcharWithoutLength_Fails()
    {
        Assert.Throws<ArgumentException>(() => ColumnTypeTable.StoreTypeOf("varchar(n)"));
    }
}
