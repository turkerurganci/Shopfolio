using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.SearchEngine;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The search engine data of a record page, made from the record by one set of rules (T363 criteria 5 and 6; KK5, KK6; 02 3.30.5, 3.30.6): the five items - title, description, site
/// address, structured data and preview - are the record's own; a page without an image previews with the logo; a draft, an archived product and the screens the search engines may not
/// read carry the robots meta tag and nothing else for the search engines to read.
/// </summary>
public sealed partial class RecordPagesTests
{
    private const string Brand = "Yılmaz Mobilya";

    private static readonly SearchEngineData Data = new(new SiteAddress(Options.Create(new SiteOptions { Domain = "magaza.example.com" })));

    [GeneratedRegex("""<meta (?:name|property)="([^"]+)" content="([^"]*)">""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MetaTag();

    private static Dictionary<string, string> Metas(string head) =>
        MetaTag().Matches(head).ToDictionary(m => m.Groups[1].Value, m => System.Net.WebUtility.HtmlDecode(m.Groups[2].Value), StringComparer.Ordinal);

    private static string HeadOf(PageSearchData page)
    {
        DefaultHttpContext context = new();
        context.Request.Path = page.Path;
        return Data.Head(context, page).ToString()!;
    }

    [Fact]
    public void AProductPage_HasTheNameAndTheBrandAsTitle_TheShortFieldAsDescription_AndItsOwnAddress_KK5()
    {
        PageSearchData page = RecordPages.Product("Ahşap Masa", "Masif meşe masa.", "<p>Uzun metin</p>", "ahsap-masa", "/gorsel/1", "/gorsel/logo", Brand, open: true, []);

        Assert.Equal("Ahşap Masa – Yılmaz Mobilya", page.Title);
        Assert.Equal("Masif meşe masa.", page.Description);
        Assert.Equal("/urun/ahsap-masa", page.Path);
        Assert.Equal("product", page.OpenGraphType);
        Assert.False(page.Closed);
        Dictionary<string, string> metas = Metas(HeadOf(page));
        Assert.Equal("Ahşap Masa – Yılmaz Mobilya", metas["og:title"]);
        Assert.Equal("Masif meşe masa.", metas["og:description"]);
        Assert.Equal("Masif meşe masa.", metas["description"]);
        Assert.Equal("https://magaza.example.com/urun/ahsap-masa", metas["og:url"]);
        Assert.Equal("https://magaza.example.com/gorsel/1", metas["og:image"]);
    }

    [Fact]
    public void AProductWithoutAShortField_TakesThePlainStartOfItsText_CutAtAWordWithinTheShortFieldCeiling_KK5()
    {
        string word = "kelime ";
        string body = "<p>" + string.Concat(Enumerable.Repeat(word, 60)) + "</p><p>ikinci</p>";

        PageSearchData page = RecordPages.Product("Masa", null, body, "masa", null, null, Brand, open: true, []);

        Assert.NotNull(page.Description);
        Assert.True(page.Description.Length <= ProductConstants.ShortTextMaxLength);
        Assert.DoesNotContain("<", page.Description, StringComparison.Ordinal);
        Assert.EndsWith("kelime", page.Description, StringComparison.Ordinal);
    }

    [Fact]
    public void APageWithoutAnImage_PreviewsWithTheLogo_KK5()
    {
        PageSearchData page = RecordPages.Product("Masa", "Kısa.", null, "masa", null, "/gorsel/logo", Brand, open: true, []);

        Assert.Equal("https://magaza.example.com/gorsel/logo", Metas(HeadOf(page))["og:image"]);
    }

    [Fact]
    public void ADraftAndAnArchivedProduct_AreClosed_TheirHeadHasTheTitleAndTheRobotsTagOnly_KK6()
    {
        JsonLdBlock block = Data.Product(new ProductData("Masa", null, [], null, 10m, StockStatus.InStock, "/urun/masa"));

        PageSearchData closed = RecordPages.Product("Masa", "Kısa.", null, "masa", "/gorsel/1", "/gorsel/logo", Brand, open: false, [block]);
        string head = HeadOf(closed);

        Assert.True(closed.Closed);
        Assert.Contains("<title>", head, StringComparison.Ordinal);
        Assert.Equal("noindex", Metas(head)["robots"]);
        Assert.Single(Metas(head));
        Assert.DoesNotContain("ld+json", head, StringComparison.Ordinal);
    }

    [Fact]
    public void ACategoryPage_IsOpen_AndItsDescriptionIsMadeFromTheNameAndTheBrand_KK5()
    {
        PageSearchData page = RecordPages.Category("Çelik Kapı", "celik-kapi", null, "/gorsel/logo", Brand, []);

        Assert.False(page.Closed);
        Assert.Equal("/kategori/celik-kapi", page.Path);
        Assert.Equal(SearchText.CategoryDescription("Çelik Kapı", Brand), page.Description);
        Assert.Equal("website", page.OpenGraphType);
        Assert.Contains("Çelik Kapı", page.Description, StringComparison.Ordinal);
        Assert.Contains(Brand, page.Description, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("hizmet", "/hizmet/web-tasarim")]
    [InlineData("referans", "/referans/web-tasarim")]
    [InlineData("sayfa", "/sayfa/web-tasarim")]
    public void AContentRecordPage_HasItsPrefixAddress_AndAPublishedOneIsOpenWhileADraftIsClosed_KK5_KK6(string prefix, string path)
    {
        PageSearchData published = RecordPages.Content(prefix, "Web Tasarım", "Kısa tanıtım.", null, "web-tasarim", null, null, Brand, open: true, []);
        PageSearchData draft = RecordPages.Content(prefix, "Web Tasarım", "Kısa tanıtım.", null, "web-tasarim", null, null, Brand, open: false, []);

        Assert.Equal(path, published.Path);
        Assert.False(published.Closed);
        Assert.Equal("website", published.OpenGraphType);
        Assert.True(draft.Closed);
    }

    [Fact]
    public void AClosedScreen_HasTheScreenNameAndTheBrandAsTitle_AndTheRobotsTagOnly_KK6()
    {
        PageSearchData cart = RecordPages.ClosedScreen("Sepet", Brand, "/sepet");
        string head = HeadOf(cart);

        Assert.Equal("Sepet – Yılmaz Mobilya", cart.Title);
        Assert.True(cart.Closed);
        Assert.Contains("<title>Sepet – Yılmaz Mobilya</title>", head, StringComparison.Ordinal);
        Assert.Equal("noindex", Metas(head)["robots"]);
        Assert.Single(Metas(head));
    }

    [Fact]
    public void ARecordWithAnAddressThatIsNotAValidSegment_IsAProgrammingError()
    {
        Assert.Throws<ArgumentException>(() => RecordPages.Product("Masa", null, null, "Masa Ürünü", null, null, Brand, open: true, []));
        Assert.Throws<ArgumentException>(() => RecordPages.Content("urun-x", "Masa", null, null, "masa", null, null, Brand, open: true, []));
    }
}
