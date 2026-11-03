using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Equality check of the enums (09 21.4 item 1; T102 KK1): the tables of <c>06 5.2</c>…<c>5.6</c> and the enums of the code name the same
/// members, and the audit operation list of <c>09 9.2</c> equals <see cref="AuditOperation"/>. A value added to one side and not the other fails.
/// </summary>
[Trait("Consistency", "true")]
public sealed class EnumTableConsistencyTests
{
    private const string SampleDocument = """
        ## 5. Enum tanımları

        ### 5.3 Sözlüğün listeleri

        | Liste | Biçim |
        |---|---|
        | Ürün tipi `ProductType` | enum |

        **5.3.1 `ProductType` — Ürün tipi.** Ürün düzleminde.

        | Değer | Kod adı | Kaynak |
        |---|---|---|
        | Fiziksel ürün | `Physical` | `02 §1.2` |
        | Hizmet | `Service` | `02 §1.2` |

        **5.3.12 Enum olmayan üç liste.** Metin.

        | Ignored | `Nope` | x |

        ## 6. İlişki diyagramı
        """;

    private const string SampleGuidelines = """
        3. **Kod listesi (3 kod).** Yeni kod.

           | Alan | Kodlar | Konu |
           |---|---|---|
           | `CompanyAndBranding` | `UpdateCompany` (konu `Company`), `UpdateBranding` (konu `Branding`) | Firma ve marka |
           | `Publication` | `ChangePublishStatus` | Kaydın türü |

           Toplam: 3.
        """;

    [Fact]
    public void ReadDataModelEnums_SampleDocument_ReadsOnlyTheTablesUnderANamedParagraph()
    {
        IReadOnlyDictionary<string, IReadOnlyList<string>> enums = EnumTable.ReadDataModelEnums(SampleDocument);

        KeyValuePair<string, IReadOnlyList<string>> only = Assert.Single(enums);
        Assert.Equal("ProductType", only.Key);
        Assert.Equal(["Physical", "Service"], only.Value);
    }

    [Fact]
    public void ReadAuditOperations_SampleDocument_SkipsTheSubjectNotes()
    {
        Assert.Equal(["UpdateCompany", "UpdateBranding", "ChangePublishStatus"], EnumTable.ReadAuditOperations(SampleGuidelines));
    }

    [Fact]
    public void DataModelDocument_AndTheEnums_NameTheSameMembers()
    {
        string document = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "06_DATA_MODEL.md"));
        IReadOnlyDictionary<string, IReadOnlyList<string>> documented = EnumTable.ReadDataModelEnums(document);
        Dictionary<string, Type> enums = ApplicationEnums.ByName();

        Assert.NotEmpty(documented);
        foreach ((string name, IReadOnlyList<string> codes) in documented)
        {
            Assert.True(enums.TryGetValue(name, out Type? type), $"06 5 defines {name}, but no such enum exists.");
            Assert.Equal(codes.Order(StringComparer.Ordinal), Enum.GetNames(type!).Order(StringComparer.Ordinal));
        }
    }

    [Fact]
    public void EveryEnumOfTheModuleContractsAndTheValues_IsDefinedInTheDataModelDocument()
    {
        string document = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "06_DATA_MODEL.md"));
        HashSet<string> documented = EnumTable.ReadDataModelEnums(document).Keys.ToHashSet(StringComparer.Ordinal);

        string[] undocumented = ApplicationEnums.Persistent()
            .Select(t => t.Name)
            .Where(name => name != nameof(AuditOperation) && !documented.Contains(name))
            .ToArray();

        Assert.Empty(undocumented);
    }

    [Fact]
    public void CodingGuidelines_AndAuditOperation_NameTheSameCodes()
    {
        string document = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md"));

        IReadOnlyList<string> documented = EnumTable.ReadAuditOperations(document);

        Assert.Equal(43, documented.Count);
        Assert.Equal(documented.Order(StringComparer.Ordinal), Enum.GetNames<AuditOperation>().Order(StringComparer.Ordinal));
    }
}
