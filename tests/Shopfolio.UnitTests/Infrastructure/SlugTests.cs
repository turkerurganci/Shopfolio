using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The page address made from a record's name (T219 criteria 1 and 2; KK1, KK2; 09 9.4 items 2 and 3; 05 3.7.1, MK-18; 06 4.72): Turkish letters are folded, a run of
/// other characters is one hyphen, a name that leaves nothing gives the record type's root, a taken address gets the first free number from 2, a fixed address never
/// changes, and the suggested stock code starts from the address the same function made.
/// </summary>
public sealed class SlugTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Theory]
    [InlineData("Çiçekli Şal", "cicekli-sal")]
    [InlineData("ÇELİK KAPI", "celik-kapi")]
    [InlineData("çelik-kapı", "celik-kapi")]
    [InlineData("  Ağaç   Masa  ", "agac-masa")]
    [InlineData("Öğrenci Ünitesi", "ogrenci-unitesi")]
    [InlineData("Ahşap Masa (180 x 90 cm)", "ahsap-masa-180-x-90-cm")]
    [InlineData("A&B--C", "a-b-c")]
    [InlineData("Café Crème", "cafe-creme")]
    [InlineData("2026 Koleksiyonu", "2026-koleksiyonu")]
    public void From_FoldsTurkishLettersAndMakesOneHyphenOfEveryOtherRun(string name, string expected)
    {
        Assert.Equal(expected, Slug.From(name, AddressRules.ProductPrefix));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("***")]
    [InlineData("你好")]
    [InlineData("—–—")]
    public void From_ANameWithoutALatinLetterOrDigit_GivesTheRootOfTheRecordType(string name)
    {
        foreach (string root in AddressRules.RecordPrefixes)
        {
            Assert.Equal(root, Slug.From(name, root));
        }
    }

    [Fact]
    public void From_ARootThatIsNotOneOfTheFivePrefixes_IsRefused()
    {
        Assert.Throws<ArgumentException>(() => Slug.From("Ad", "panel"));
        Assert.Throws<ArgumentException>(() => Slug.From("Ad", "Urun"));
    }

    [Fact]
    public void From_EveryAddressItMakes_IsAValidPathSegment()
    {
        string[] names = ["Çiçekli Şal", "", "İĞNE & İPLİK", "Ürün/1", "ı i İ I", new string('a', 150)];
        foreach (string name in names)
        {
            Assert.True(AddressRules.IsValidSegment(Slug.From(name, AddressRules.ProductPrefix)), name);
        }
    }

    [Fact]
    public void From_ANameThatCarriesANumber_KeepsItAsAWord()
    {
        Assert.Equal("celik-kapi-2", Slug.From("Çelik Kapı 2", AddressRules.ProductPrefix));
    }

    [Fact]
    public void FirstFree_AFreeBase_IsTheBase()
    {
        Assert.Equal("celik-kapi", Slug.FirstFree("celik-kapi", []));
        Assert.Equal("celik-kapi", Slug.FirstFree("celik-kapi", ["celik-kapi-2", "baska"]));
    }

    [Fact]
    public void FirstFree_ATakenBase_GetsTheFirstFreeNumberFromTwo()
    {
        Assert.Equal("celik-kapi-2", Slug.FirstFree("celik-kapi", ["celik-kapi"]));
        Assert.Equal("celik-kapi-3", Slug.FirstFree("celik-kapi", ["celik-kapi", "celik-kapi-2"]));
        Assert.Equal("celik-kapi-2", Slug.FirstFree("celik-kapi", ["celik-kapi", "celik-kapi-3"]));
    }

    [Theory]
    [InlineData("celik-kapi", "celik-kapi", true)]
    [InlineData("celik-kapi", "celik-kapi-2", true)]
    [InlineData("celik-kapi", "celik-kapi-120", true)]
    [InlineData("celik-kapi", "celik-kapi-", false)]
    [InlineData("celik-kapi", "celik-kapi-x", false)]
    [InlineData("celik-kapi", "celik-kapi-2a", false)]
    [InlineData("celik-kapi", "celik-kapisi", false)]
    [InlineData("celik-kapi", "celik-kapi-ozel", false)]
    [InlineData("celik-kapi", "celik", false)]
    public void IsBaseOrNumbered_IsTheBaseOrTheBaseWithDigits(string baseSlug, string slug, bool expected)
    {
        Assert.Equal(expected, Slug.IsBaseOrNumbered(baseSlug, slug));
    }

    [Fact]
    public async Task ForSaveAsync_ANotFixedAddress_IsMadeAgainFromTheNameAndTheTableReadOnce()
    {
        int reads = 0;

        string slug = await Slug.ForSaveAsync(
            "Çelik Kapı",
            AddressRules.ProductPrefix,
            "eski-ad",
            null,
            (baseSlug, _) =>
            {
                reads++;
                Assert.Equal("celik-kapi", baseSlug);
                return Task.FromResult<IReadOnlyCollection<string>>(["celik-kapi", "celik-kapi-2", "celik-kapisi"]);
            },
            Ct);

        Assert.Equal("celik-kapi-3", slug);
        Assert.Equal(1, reads);
    }

    [Fact]
    public async Task ForSaveAsync_ARecordSavedAgainWithTheSameName_KeepsItsOwnAddress()
    {
        string slug = await Slug.ForSaveAsync(
            "Çelik Kapı",
            AddressRules.ProductPrefix,
            "celik-kapi-2",
            null,
            (_, _) => Task.FromResult<IReadOnlyCollection<string>>(["celik-kapi", "celik-kapi-2"]),
            Ct);

        Assert.Equal("celik-kapi-2", slug);
    }

    [Fact]
    public async Task ForSaveAsync_AFixedAddress_NeverChanges_WhateverTheNameHasBecome_AndTheTableIsNotRead()
    {
        string slug = await Slug.ForSaveAsync(
            "Tamamen Başka Bir Ad",
            AddressRules.ProductPrefix,
            "celik-kapi",
            new DateTimeOffset(2026, 11, 5, 9, 0, 0, TimeSpan.Zero),
            (_, _) => throw new InvalidOperationException("A fixed address reads nothing."),
            Ct);

        Assert.Equal("celik-kapi", slug);
    }

    [Theory]
    [InlineData("ahsap-masa", 1, "AHSAP-MASA-01")]
    [InlineData("ahsap-masa", 12, "AHSAP-MASA-12")]
    [InlineData("cok-uzun-bir-urun-adresi-burada", 1, "COK-UZUN-BIR-URUN-AD-01")]
    [InlineData("on-dokuz-karakterli-a-b", 3, "ON-DOKUZ-KARAKTERLI-03")]
    public void Suggest_IsTheAddressInCapitalsCutAtTwentyCharactersAndTheTwoDigitOrder(string slug, int order, string expected)
    {
        Assert.Equal(expected, StockCodeSuggestion.Suggest(slug, order, []));
    }

    [Fact]
    public void Suggest_ACutThatEndsOnAHyphen_DropsTheHyphen()
    {
        // The twentieth character of this address is a hyphen.
        string slug = "abcdefghijklmnopqrs-tuvw";
        Assert.Equal(StockCodeSuggestion.AddressLength, 20);
        Assert.Equal("ABCDEFGHIJKLMNOPQRS-01", StockCodeSuggestion.Suggest(slug, 1, []));
    }

    [Fact]
    public void Suggest_ACodeAnotherVariantHolds_RaisesTheOrderUntilItIsFree_ComparedInTheIdentityForm()
    {
        Assert.Equal("AHSAP-MASA-03", StockCodeSuggestion.Suggest("ahsap-masa", 1, ["ahsap-masa-01", "AHSAP-MASA-02"]));
    }

    [Fact]
    public void Suggest_StartsFromTheAddressTheSlugFunctionMade()
    {
        string slug = Slug.From("Ahşap Masa", AddressRules.ProductPrefix);
        Assert.Equal("AHSAP-MASA-01", StockCodeSuggestion.Suggest(slug, 1, []));
    }

    [Fact]
    public void Suggest_AnOrderBelowOne_IsRefused()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StockCodeSuggestion.Suggest("ahsap-masa", 0, []));
    }
}
