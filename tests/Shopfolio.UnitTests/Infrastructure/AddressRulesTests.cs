using Shopfolio.Infrastructure.Routing;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The address rules (T206 criteria 1, 2, 4; KK1, KK2, KK4; 07 2.1): the path segments are lower case ASCII letters, digits and single hyphens; the
/// record address is <c>/{prefix}/{slug}</c> with one of five prefixes and no language prefix; the first segment is a closed list; and a storefront
/// template carries no internal key. The document equals the code in <c>AddressListConsistencyTests</c>.
/// </summary>
public sealed class AddressRulesTests
{
    private static readonly string[] _languageCodes = ["tr", "en", "de"];

    [Theory]
    [InlineData("ahsap-masa")]
    [InlineData("a")]
    [InlineData("2026")]
    [InlineData("kategori-1")]
    [InlineData("a-b-c")]
    public void IsValidSegment_OnLowerCaseAsciiDigitsAndSingleHyphens_IsTrue(string segment)
    {
        Assert.True(AddressRules.IsValidSegment(segment));
    }

    [Theory]
    [InlineData("")]
    [InlineData("Ahsap")]
    [InlineData("ahşap")]
    [InlineData("ahsap--masa")]
    [InlineData("-ahsap")]
    [InlineData("ahsap-")]
    [InlineData("ahsap_masa")]
    [InlineData("ahsap masa")]
    [InlineData("ahsap.masa")]
    [InlineData("ahsap/masa")]
    [InlineData("İstanbul")]
    [InlineData("masa\n")]
    public void IsValidSegment_OnAnythingElse_IsFalse(string segment)
    {
        Assert.False(AddressRules.IsValidSegment(segment));
    }

    [Fact]
    public void RecordPrefixes_AreTheFiveAndHaveNoLanguagePrefix()
    {
        Assert.Equal(["urun", "kategori", "hizmet", "referans", "sayfa"], AddressRules.RecordPrefixes);
        Assert.All(AddressRules.RecordPrefixes, p => Assert.DoesNotContain(p, _languageCodes));
        Assert.All(AddressRules.RecordPrefixes, p => Assert.True(AddressRules.IsClosedFirstSegment(p)));
    }

    [Theory]
    [InlineData("urun", "ahsap-masa", "/urun/ahsap-masa")]
    [InlineData("kategori", "mobilya", "/kategori/mobilya")]
    [InlineData("hizmet", "montaj", "/hizmet/montaj")]
    [InlineData("referans", "otel-projesi", "/referans/otel-projesi")]
    [InlineData("sayfa", "kvkk", "/sayfa/kvkk")]
    public void RecordAddress_IsPrefixThenSlug_LowerCaseAndWithoutTrailingSlash(string prefix, string slug, string expected)
    {
        Assert.Equal(expected, AddressRules.RecordAddress(prefix, slug));
    }

    [Theory]
    [InlineData("product", "ahsap-masa")]
    [InlineData("Urun", "ahsap-masa")]
    [InlineData("", "ahsap-masa")]
    [InlineData("tr", "ahsap-masa")]
    [InlineData("urun", "Ahsap-Masa")]
    [InlineData("urun", "ahsap/masa")]
    [InlineData("urun", "")]
    public void RecordAddress_OnAnotherPrefixOrAnInvalidSlug_Throws(string prefix, string slug)
    {
        Assert.Throws<ArgumentException>(() => AddressRules.RecordAddress(prefix, slug));
    }

    [Fact]
    public void ClosedFirstSegments_AreUniqueAndEachIsAValidSegmentOrAProtocolFile()
    {
        Assert.Equal(AddressRules.ClosedFirstSegments.Count, AddressRules.ClosedFirstSegments.Distinct(StringComparer.Ordinal).Count());
        Assert.All(
            AddressRules.ClosedFirstSegments,
            s => Assert.True(AddressRules.IsValidSegment(s) || AddressRules.ProtocolFiles.Contains(s), s));
    }

    [Theory]
    [InlineData("urun", true)]
    [InlineData("sepet", true)]
    [InlineData("panel", true)]
    [InlineData("siparis", true)]
    [InlineData("statik", true)]
    [InlineData("robots.txt", true)]
    [InlineData("sitemap.xml", true)]
    [InlineData("ahsap-masa", false)]
    [InlineData("product", false)]
    [InlineData("api", false)]
    [InlineData("v1", false)]
    [InlineData("css", false)]
    [InlineData("admin", false)]
    [InlineData("yonetim", false)]
    [InlineData("tr", false)]
    [InlineData("URUN", false)]
    [InlineData("", false)]
    public void IsClosedFirstSegment_OnlyForTheListedFirstSegments(string segment, bool expected)
    {
        Assert.Equal(expected, AddressRules.IsClosedFirstSegment(segment));
    }

    [Theory]
    [InlineData("ahsap-masa", "ahsap-masa")]
    [InlineData("AHSAP-MASA", "ahsap-masa")]
    [InlineData("Ahsap-Masa", "ahsap-masa")]
    [InlineData("ISIK", "isik")]
    public void SlugForLookup_FoldsTheLetterCaseInvariantly(string fromPath, string expected)
    {
        using CultureScope turkish = new("tr-TR");

        Assert.Equal(expected, AddressRules.SlugForLookup(fromPath));
    }

    [Theory]
    [InlineData("2026-7K4M9P", "2026-7K4M9P")]
    [InlineData("2026-7k4m9p", "2026-7K4M9P")]
    [InlineData("2026-ijk", "2026-IJK")]
    public void NumberForLookup_FoldsTheLetterCaseInvariantly(string fromPath, string expected)
    {
        using CultureScope turkish = new("tr-TR");

        Assert.Equal(expected, OrderRoutes.NumberForLookup(fromPath));
    }

    // --- Template check (KK1, KK2, KK4) ---

    [Theory]
    [InlineData("")]
    [InlineData("/")]
    [InlineData("/urun/{slug}")]
    [InlineData("/kategori/{slug}")]
    [InlineData("/sepet")]
    [InlineData("/hesap/adresler")]
    [InlineData("/hesap/silindi")]
    [InlineData("/dogrulama/{key}")]
    [InlineData("/odeme/donus/{accessKey}")]
    [InlineData("/indir/{value}")]
    [InlineData("/sistem/canlilik")]
    [InlineData("/robots.txt")]
    [InlineData("/sitemap.xml")]
    [InlineData("/panel/siparisler")]
    [InlineData("/panel/urunler/{id}")]
    [InlineData("/panel/yukleme/{uploadId}")]
    [InlineData("/panel/davet/{key}")]
    [InlineData("/giris/google")]
    public void Check_OnTemplatesThatFollowTheRules_FindsNothing(string template)
    {
        Assert.Empty(AddressTemplates.Check(template));
    }

    [Theory]
    [InlineData("/ahsap-masa", "not in the closed list")]
    [InlineData("/product/{slug}", "not in the closed list")]
    [InlineData("/api/orders", "not in the closed list")]
    [InlineData("/v1/orders", "not in the closed list")]
    [InlineData("/yonetim/siparisler", "not in the closed list")]
    [InlineData("/tr/urun/{slug}", "not in the closed list")]
    [InlineData("/{slug}", "first segment is a parameter")]
    [InlineData("/{prefix}/{slug}", "first segment is a parameter")]
    [InlineData("/Urun/{slug}", "not lower case")]
    [InlineData("/panel/Siparisler", "not lower case")]
    [InlineData("/hesap/adres_defteri", "not lower case")]
    [InlineData("/hesap/adres--defteri", "single hyphens")]
    [InlineData("/hesap/ad-{x}", "mixes literal text and a parameter")]
    [InlineData("/urun", "exactly")]
    [InlineData("/urun/{slug}/detay", "exactly")]
    [InlineData("/urun/{name}", "exactly")]
    [InlineData("/urun/{slug?}", "exactly")]
    [InlineData("/urun/{*slug}", "exactly")]
    [InlineData("/urun/ahsap-masa", "exactly")]
    [InlineData("/siparis", "two order roots")]
    [InlineData("/siparis/{orderNumber}", "two order roots")]
    [InlineData("/siparis/erisim", "two order roots")]
    [InlineData("/siparis/erisim/{accessKey}", "two order roots")]
    [InlineData("/hesap/{*rest}", "catch-all")]
    public void Check_OnTemplatesThatBreakARule_NamesTheRule(string template, string expectedPart)
    {
        IReadOnlyList<string> problems = AddressTemplates.Check(template);

        Assert.Contains(problems, p => p.Contains(expectedPart, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("/hesap/adresler/{id}")]
    [InlineData("/hesap/adresler/{addressId}")]
    [InlineData("/sepet/{cartItemId}")]
    [InlineData("/hesap/siparisler/{orderItemId}")]
    [InlineData("/sepet/{item:guid}")]
    [InlineData("/hesap/adresler/{number:int}")]
    [InlineData("/hesap/adresler/{number:long}")]
    public void Check_OnAStorefrontTemplateWithAnInternalKey_Reports(string template)
    {
        IReadOnlyList<string> problems = AddressTemplates.Check(template);

        Assert.Contains(problems, p => p.Contains("internal key", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("/panel/urunler/{id}")]
    [InlineData("/panel/urunler/{productId}")]
    [InlineData("/panel/siparisler/{orderId:guid}")]
    public void Check_OnAPanelTemplateWithAnInternalKey_FindsNothing(string template)
    {
        Assert.Empty(AddressTemplates.Check(template));
    }

    [Fact]
    public void Check_OnTheStorefrontTemplatesTheTasksAfterThisOneWillWrite_FindsNothing()
    {
        // Member and access keys and their order screens, and the closed forms of the account links (07 2.1.3, 2.1.4).
        Assert.Empty(AddressTemplates.Check(OrderRoutes.MemberRoot));
        Assert.Empty(AddressTemplates.Check(OrderRoutes.AccessRoot));
        foreach (string screen in OrderRoutes.Screens)
        {
            Assert.Empty(AddressTemplates.Check(OrderRoutes.MemberScreen(screen)));
            Assert.Empty(AddressTemplates.Check(OrderRoutes.AccessScreen(screen)));
        }
    }

    [Fact]
    public void Check_OnATemplateThatIsNotARouteTemplate_Reports()
    {
        Assert.NotEmpty(AddressTemplates.Check("/hesap/{unclosed"));
    }

    /// <summary>Runs a block under another current culture; the fold of the lookup must not follow it.</summary>
    private sealed class CultureScope : IDisposable
    {
        private readonly System.Globalization.CultureInfo _original = System.Globalization.CultureInfo.CurrentCulture;

        public CultureScope(string name)
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(name);
        }

        public void Dispose()
        {
            System.Globalization.CultureInfo.CurrentCulture = _original;
        }
    }
}
