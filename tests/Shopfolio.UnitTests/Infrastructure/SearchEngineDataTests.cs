using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.SearchEngine;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The search engine data of a page (T219 criteria 3 to 5 and 8; KK3 to KK5, KK8; 05 3.7.2, 3.7.4): the four JSON-LD blocks are valid JSON with schema.org types and
/// carry the record's values; the head of an open page has the title, description and Open Graph preview with the logo as the fallback image; a closed page's head has
/// the title and the robots meta tag only; nothing the record holds can leave a script element or an attribute.
/// </summary>
public sealed partial class SearchEngineDataTests
{
    private const string Domain = "magaza.example.com";

    private static readonly SearchEngineData Data = new(new SiteAddress(Options.Create(new SiteOptions { Domain = Domain })));

    [GeneratedRegex("""<meta (?:name|property)="([^"]+)" content="([^"]*)">""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MetaTag();

    [GeneratedRegex("""<script type="application/ld\+json">(.*?)</script>""", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex JsonLdScript();

    private static ProductData Product(string name = "Ahşap Masa", StockStatus availability = StockStatus.InStock) =>
        new(name, "Masif meşe masa.", ["/gorsel/1-orta.webp", "/gorsel/2-orta.webp"], "AHSAP-MASA-01", 2450.50m, availability, "/urun/ahsap-masa");

    private static JsonElement Parse(JsonLdBlock block)
    {
        using JsonDocument document = JsonDocument.Parse(block.Json);
        return document.RootElement.Clone();
    }

    private static DefaultHttpContext Request(string path)
    {
        DefaultHttpContext context = new();
        context.Request.Path = path;
        return context;
    }

    private static PageSearchData Page(bool closed = false, string? image = "/gorsel/1-orta.webp", string? logo = "/gorsel/logo.webp", IReadOnlyList<JsonLdBlock>? blocks = null) =>
        new("Ahşap Masa – Yılmaz Mobilya", "Masif meşe masa.", "/urun/ahsap-masa", image, logo, "Yılmaz Mobilya", closed, blocks ?? []) { OpenGraphType = "product" };

    private static Dictionary<string, string> Metas(string head) =>
        MetaTag().Matches(head).ToDictionary(m => m.Groups[1].Value, m => System.Net.WebUtility.HtmlDecode(m.Groups[2].Value), StringComparer.Ordinal);

    // ---- SiteAddress ----

    [Fact]
    public void SiteAddress_IsHttpsAndTheInstallationDomain_NeverTheRequestHost()
    {
        SiteAddress site = new(Options.Create(new SiteOptions { Domain = Domain }));

        Assert.Equal("https://magaza.example.com/urun/ahsap-masa", site.Absolute("/urun/ahsap-masa"));
        Assert.Equal("https://magaza.example.com/", site.Absolute("/"));
    }

    [Theory]
    [InlineData("")]
    [InlineData("urun/x")]
    [InlineData("//evil.example/x")]
    [InlineData("/a\\b")]
    [InlineData("https://evil.example/x")]
    public void SiteAddress_AValueThatIsNotAPathOfTheSite_IsRefused(string path)
    {
        SiteAddress site = new(Options.Create(new SiteOptions { Domain = Domain }));

        Assert.Throws<ArgumentException>(() => site.Absolute(path));
    }

    // ---- JSON-LD (KK4) ----

    [Fact]
    public void Product_IsValidJsonWithSchemaOrgContext_AndCarriesTheRecordsValues()
    {
        JsonElement root = Parse(Data.Product(Product()));

        Assert.Equal("https://schema.org", root.GetProperty("@context").GetString());
        Assert.Equal("Product", root.GetProperty("@type").GetString());
        Assert.Equal("Ahşap Masa", root.GetProperty("name").GetString());
        Assert.Equal("Masif meşe masa.", root.GetProperty("description").GetString());
        Assert.Equal("AHSAP-MASA-01", root.GetProperty("sku").GetString());
        Assert.Equal(
            ["https://magaza.example.com/gorsel/1-orta.webp", "https://magaza.example.com/gorsel/2-orta.webp"],
            root.GetProperty("image").EnumerateArray().Select(e => e.GetString()!).ToArray());
        JsonElement offer = root.GetProperty("offers");
        Assert.Equal("Offer", offer.GetProperty("@type").GetString());
        Assert.Equal(2450.50m, offer.GetProperty("price").GetDecimal());
        Assert.Equal("TRY", offer.GetProperty("priceCurrency").GetString());
        Assert.Equal("https://schema.org/InStock", offer.GetProperty("availability").GetString());
        Assert.Equal("https://magaza.example.com/urun/ahsap-masa", offer.GetProperty("url").GetString());
    }

    [Fact]
    public void Product_OutOfStock_SaysOutOfStock_AndAPageWithoutImageDescriptionOrCodeLeavesThemOut()
    {
        ProductData bare = new("Masa", null, [], null, 100m, StockStatus.OutOfStock, "/urun/masa");

        JsonElement root = Parse(Data.Product(bare));

        Assert.Equal("https://schema.org/OutOfStock", root.GetProperty("offers").GetProperty("availability").GetString());
        Assert.False(root.TryGetProperty("image", out _));
        Assert.False(root.TryGetProperty("description", out _));
        Assert.False(root.TryGetProperty("sku", out _));
    }

    [Fact]
    public void Organization_IsValidJson_WithTheBrandNameTheSiteAddressTheLogoAndTheContactValuesTheRecordHas()
    {
        JsonElement root = Parse(Data.Organization(new OrganizationData("Yılmaz Mobilya", "/gorsel/logo.webp", "02121234567", "bilgi@yilmaz.example", ["https://instagram.com/yilmaz", "javascript:alert(1)", "not a url"])));

        Assert.Equal("Organization", root.GetProperty("@type").GetString());
        Assert.Equal("Yılmaz Mobilya", root.GetProperty("name").GetString());
        Assert.Equal("https://magaza.example.com/", root.GetProperty("url").GetString());
        Assert.Equal("https://magaza.example.com/gorsel/logo.webp", root.GetProperty("logo").GetString());
        Assert.Equal("02121234567", root.GetProperty("telephone").GetString());
        Assert.Equal("bilgi@yilmaz.example", root.GetProperty("email").GetString());
        Assert.Equal(["https://instagram.com/yilmaz"], root.GetProperty("sameAs").EnumerateArray().Select(e => e.GetString()!).ToArray());
    }

    [Fact]
    public void Organization_WithoutLogoContactOrLinks_LeavesThemOut()
    {
        JsonElement root = Parse(Data.Organization(new OrganizationData("Yılmaz Mobilya", null, null, " ", [])));

        Assert.False(root.TryGetProperty("logo", out _));
        Assert.False(root.TryGetProperty("telephone", out _));
        Assert.False(root.TryGetProperty("email", out _));
        Assert.False(root.TryGetProperty("sameAs", out _));
    }

    [Fact]
    public void Breadcrumbs_AreAListInOrderWithPositionNameAndAbsoluteAddress()
    {
        JsonLdBlock? block = Data.Breadcrumbs([new("Ana Sayfa", "/"), new("Masalar", "/kategori/masalar"), new("Ahşap Masa", "/urun/ahsap-masa")]);

        Assert.NotNull(block);
        JsonElement root = Parse(block);
        Assert.Equal("BreadcrumbList", root.GetProperty("@type").GetString());
        JsonElement[] items = [.. root.GetProperty("itemListElement").EnumerateArray()];
        Assert.Equal([1, 2, 3], items.Select(i => i.GetProperty("position").GetInt32()).ToArray());
        Assert.Equal(["Ana Sayfa", "Masalar", "Ahşap Masa"], items.Select(i => i.GetProperty("name").GetString()!).ToArray());
        Assert.Equal("https://magaza.example.com/kategori/masalar", items[1].GetProperty("item").GetString());
        Assert.All(items, i => Assert.Equal("ListItem", i.GetProperty("@type").GetString()));
    }

    [Fact]
    public void Breadcrumbs_WithoutAStep_AreNull()
    {
        Assert.Null(Data.Breadcrumbs([]));
    }

    [Fact]
    public void Faq_IsAQuestionListWithTheAnswersInPlainText()
    {
        JsonLdBlock? block = SearchEngineData.Faq([new("İade var mı?", "<p>Evet, <b>14 gün</b> içinde.</p>"), new("Kargo?", "<p>Ücretsiz &amp; hızlı</p>")]);

        Assert.NotNull(block);
        JsonElement root = Parse(block);
        Assert.Equal("FAQPage", root.GetProperty("@type").GetString());
        JsonElement[] questions = [.. root.GetProperty("mainEntity").EnumerateArray()];
        Assert.Equal(["İade var mı?", "Kargo?"], questions.Select(q => q.GetProperty("name").GetString()!).ToArray());
        Assert.All(questions, q => Assert.Equal("Question", q.GetProperty("@type").GetString()));
        Assert.Equal("Evet, 14 gün içinde.", questions[0].GetProperty("acceptedAnswer").GetProperty("text").GetString());
        Assert.Equal("Ücretsiz & hızlı", questions[1].GetProperty("acceptedAnswer").GetProperty("text").GetString());
        Assert.Equal("Answer", questions[0].GetProperty("acceptedAnswer").GetProperty("@type").GetString());
    }

    [Fact]
    public void Faq_WithoutAQuestion_IsNull()
    {
        Assert.Null(SearchEngineData.Faq([]));
    }

    [Fact]
    public void AValueThatHoldsAScriptEnd_CannotLeaveTheJsonLdBlock()
    {
        string hostile = "</script><script>alert(1)</script>&<>'\"";

        JsonLdBlock block = Data.Product(Product(hostile));

        Assert.DoesNotContain("<", block.Json, StringComparison.Ordinal);
        Assert.DoesNotContain(">", block.Json, StringComparison.Ordinal);
        Assert.DoesNotContain("&", block.Json, StringComparison.Ordinal);
        Assert.Equal(hostile, Parse(block).GetProperty("name").GetString());
        string head = Data.Head(Request("/urun/ahsap-masa"), Page(blocks: [block])).ToString()!;
        Assert.Single(JsonLdScript().Matches(head));
        Assert.DoesNotContain("<script>alert", head, StringComparison.Ordinal);
    }

    [Fact]
    public void TurkishLettersInTheBlocks_StayAsTheyAre()
    {
        Assert.Contains("Ahşap Çiçek", Data.Product(Product("Ahşap Çiçek")).Json, StringComparison.Ordinal);
    }

    // ---- The head (KK3, KK5) ----

    [Fact]
    public void Head_OfAnOpenPage_HasTheTitleTheDescriptionAndTheOpenGraphPreview()
    {
        string head = Data.Head(Request("/urun/ahsap-masa"), Page()).ToString()!;

        Assert.Contains("<title>Ahşap Masa – Yılmaz Mobilya</title>", System.Net.WebUtility.HtmlDecode(head), StringComparison.Ordinal);
        Dictionary<string, string> metas = Metas(head);
        Assert.Equal("Masif meşe masa.", metas["description"]);
        Assert.Equal("product", metas["og:type"]);
        Assert.Equal("Yılmaz Mobilya", metas["og:site_name"]);
        Assert.Equal("tr_TR", metas["og:locale"]);
        Assert.Equal("Ahşap Masa – Yılmaz Mobilya", metas["og:title"]);
        Assert.Equal("Masif meşe masa.", metas["og:description"]);
        Assert.Equal("https://magaza.example.com/urun/ahsap-masa", metas["og:url"]);
        Assert.Equal("https://magaza.example.com/gorsel/1-orta.webp", metas["og:image"]);
        Assert.DoesNotContain("robots", metas.Keys);
    }

    [Fact]
    public void Head_APageWithoutAMainImage_UsesTheLogo()
    {
        Dictionary<string, string> metas = Metas(Data.Head(Request("/urun/ahsap-masa"), Page(image: null)).ToString()!);

        Assert.Equal("https://magaza.example.com/gorsel/logo.webp", metas["og:image"]);
    }

    [Fact]
    public void Head_WithNeitherImageNorLogo_HasNoImageTag()
    {
        Dictionary<string, string> metas = Metas(Data.Head(Request("/urun/ahsap-masa"), Page(image: null, logo: null)).ToString()!);

        Assert.DoesNotContain("og:image", metas.Keys);
    }

    [Fact]
    public void Head_WritesEveryJsonLdBlockInItsOwnScriptElement()
    {
        JsonLdBlock[] blocks = [Data.Product(Product()), Data.Organization(new OrganizationData("Yılmaz Mobilya", null, null, null, []))];

        string head = Data.Head(Request("/urun/ahsap-masa"), Page(blocks: blocks)).ToString()!;

        MatchCollection scripts = JsonLdScript().Matches(head);
        Assert.Equal(2, scripts.Count);
        Assert.All(scripts, s => Assert.Equal(JsonValueKind.Object, JsonDocument.Parse(s.Groups[1].Value).RootElement.ValueKind));
    }

    [Fact]
    public void Head_AValueWithQuotesAndTags_IsEncodedInTheTitleAndTheAttributes()
    {
        PageSearchData hostile = Page() with { Title = "A\"><script>x</script>", Description = "\" onload=\"x" };

        string head = Data.Head(Request("/urun/ahsap-masa"), hostile).ToString()!;

        Assert.DoesNotContain("<script>x", head, StringComparison.Ordinal);
        Assert.DoesNotContain("\" onload=\"x", head, StringComparison.Ordinal);
        Assert.Equal("\" onload=\"x", Metas(head)["description"]);
    }

    [Fact]
    public void Head_APageWithoutADescription_HasNoDescriptionTags()
    {
        Dictionary<string, string> metas = Metas(Data.Head(Request("/urun/ahsap-masa"), Page() with { Description = null }).ToString()!);

        Assert.DoesNotContain("description", metas.Keys);
        Assert.DoesNotContain("og:description", metas.Keys);
    }

    // ---- Closed pages (KK8) ----

    [Fact]
    public void Head_OfAPageTheDataClosesOrTheAddressCloses_HasTheTitleAndTheRobotsMetaTagOnly()
    {
        JsonLdBlock[] blocks = [Data.Product(Product())];
        (string Path, bool Closed)[] cases = [("/urun/ahsap-masa", true), ("/panel/siparisler", false), ("/sepet", false), ("/arama", false)];

        foreach ((string path, bool closed) in cases)
        {
            string head = Data.Head(Request(path), Page(closed: closed, blocks: blocks)).ToString()!;

            Assert.Contains("<title>", head, StringComparison.Ordinal);
            Assert.Equal("noindex", Metas(head)["robots"]);
            Assert.Single(Metas(head));
            Assert.DoesNotContain("ld+json", head, StringComparison.Ordinal);
            Assert.DoesNotContain("og:", head, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void Head_OfAPageTheRequestMarkedClosed_HasTheRobotsMetaTag()
    {
        HttpContext context = Request("/urun/ahsap-masa");
        SearchEngineClosure.MarkClosed(context);

        Assert.Equal("noindex", Metas(Data.Head(context, Page()).ToString()!)["robots"]);
    }

    [Fact]
    public void ClosedFirstSegments_AreAllInTheReservedFirstSegmentList_SoNoRecordAddressCanBeOneOfThem()
    {
        Assert.All(SearchEngineClosure.ClosedFirstSegments, segment => Assert.True(AddressRules.IsClosedFirstSegment(segment), segment));
    }

    [Theory]
    [InlineData("/panel", true)]
    [InlineData("/panel/siparisler/abc", true)]
    [InlineData("/PANEL/siparisler", true)]
    [InlineData("/sepet", true)]
    [InlineData("/odeme/bicim", true)]
    [InlineData("/siparis-takibi", true)]
    [InlineData("/arama", true)]
    [InlineData("/siparis/erisim/xyz", true)]
    [InlineData("/hesap/adresler", true)]
    [InlineData("/giris", true)]
    [InlineData("/indir/abc", true)]
    [InlineData("/", false)]
    [InlineData("/urun/ahsap-masa", false)]
    [InlineData("/kategori/masalar", false)]
    [InlineData("/hizmet/montaj", false)]
    [InlineData("/paneller", false)]
    [InlineData("/sepetim", false)]
    [InlineData("", false)]
    public void IsClosedPath_DependsOnTheFirstSegmentOnly(string path, bool expected)
    {
        Assert.Equal(expected, SearchEngineClosure.IsClosedPath(new PathString(path)));
    }

    [Theory]
    [InlineData("text/html; charset=utf-8", true)]
    [InlineData("TEXT/HTML", true)]
    [InlineData("application/xhtml+xml", true)]
    [InlineData("text/csv; charset=utf-8", false)]
    [InlineData("application/json", false)]
    [InlineData("application/octet-stream", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsHtml_TellsTheAnswerWhoseClosingIsTheMetaTag(string? contentType, bool expected)
    {
        Assert.Equal(expected, SearchEngineClosure.IsHtml(contentType));
    }

    [Fact]
    public void Head_ReturnsHtmlContentTheViewWritesUnencoded()
    {
        IHtmlContent head = Data.Head(Request("/"), Page());

        Assert.IsType<HtmlString>(head);
    }
}
