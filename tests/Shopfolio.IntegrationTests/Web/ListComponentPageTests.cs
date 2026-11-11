using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The list components drawn by the real views on the opened application (T545 KK1-KK10; 04 2.11, 2.13): the product and content cards, the grid, the page navigation, the empty-state line, the
/// search field that only the listed lists draw, and the panel table. The pages are test-only samples that hand the views a model; what is read here is the markup of the response.
/// </summary>
[Collection(StorefrontFrameTestsDefinition.Name)]
public sealed class ListComponentPageTests
{
    private const string Grid = "/odeme/liste-urunler";

    private readonly CollectionDatabaseFixture _database;

    public ListComponentPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ProductGridSampleModel).Assembly)));

    private static async Task<string> GetRawAsync(HttpClient client, string path)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync(Ct);
    }

    private static async Task<string> GetAsync(HttpClient client, string path) => WebUtility.HtmlDecode(await GetRawAsync(client, path));

    private static string GridOf(string html)
    {
        int start = html.IndexOf("<ul class=\"card-grid\">", StringComparison.Ordinal);
        Assert.True(start >= 0, "The page has no card grid.");
        return html[start..(html.IndexOf("</ul>", start, StringComparison.Ordinal) + "</ul>".Length)];
    }

    private static string[] Cards(string grid, string cardClass) =>
        [.. Regex.Matches(grid, $"<article class=\"{cardClass}\">.*?</article>", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Select(m => m.Value)];

    private static string[] PagerOf(string html)
    {
        Match nav = Regex.Match(html, "<nav class=\"pager\".*?</nav>", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
        Assert.True(nav.Success, "The page has no page navigation.");
        return [.. Regex.Matches(nav.Value, "<li class=\"pager__item[^\"]*\"[^>]*>(.*?)</li>", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Select(m => Regex.Replace(m.Groups[1].Value, "<[^>]+>", string.Empty, RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Trim())];
    }

    // ---- KK1, KK2, KK3: the cards of the grid ----

    [Fact]
    public async Task TheGrid_DrawsOneCardPerProduct_EachCardIsOneLink_WithNoFormNoButtonNoInputAndNoStockOrCartText_T545_KK1_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string grid = GridOf(await GetAsync(client, Grid));
        string[] cards = Cards(grid, "product-card");

        Assert.Equal(4, cards.Length);
        Assert.All(cards, card => Assert.Equal(1, Regex.Count(card, "<a ", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5))));
        Assert.DoesNotMatch("<(?:form|button|input|select|textarea)", grid);
        Assert.DoesNotContain("Sepete", grid, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("stok", grid, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("yorum", grid, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("href=\"/urun/duz-ampul\"", cards[0], StringComparison.Ordinal);
    }

    [Fact]
    public async Task ACard_CarriesTheImageTheNameAndThePrice_InThatOrder_AnImagelessProductKeepsTheSpace_T545_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string[] cards = Cards(GridOf(await GetAsync(client, Grid)), "product-card");

        string first = cards[0];
        int image = first.IndexOf($"src=\"/gorsel/{ProductGridSampleModel.ImageId:D}\"", StringComparison.Ordinal);
        int name = first.IndexOf("class=\"product-card__name\">Düz ampul<", StringComparison.Ordinal);
        int price = first.IndexOf("class=\"product-card__price\">120 TL<", StringComparison.Ordinal);
        Assert.True(image >= 0 && name > image && price > name, "The image, the name and the price are in that order.");
        Assert.Contains("alt=\"Düz ampul görseli\"", first, StringComparison.Ordinal);
        Assert.Contains("product-card__image--empty", cards[1], StringComparison.Ordinal);
        Assert.DoesNotContain("<img", cards[1], StringComparison.Ordinal);
    }

    [Fact]
    public async Task ADiscountedCard_ShowsTheReferencePriceBesideThePriceAndTheDatesInOneLineRightUnderTheRow_T545_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string card = Cards(GridOf(await GetAsync(client, Grid)), "product-card")[1];

        int row = card.IndexOf("class=\"product-card__price-row\"", StringComparison.Ordinal);
        int reference = card.IndexOf("<s class=\"product-card__reference-price\">100 TL</s>", StringComparison.Ordinal);
        int mark = card.IndexOf("product-card__discount-mark\">İndirim %20<", StringComparison.Ordinal);
        int dates = card.IndexOf("class=\"product-card__discount-dates\">1 Kasım 2026 – 30 Kasım 2026<", StringComparison.Ordinal);
        Assert.True(row >= 0 && reference > row && mark > reference && dates > mark, "The price row holds the price, the reference and the mark; the dates line follows it.");
        Assert.Contains("class=\"product-card__price\">80 TL<", card, StringComparison.Ordinal);
        Assert.Equal(1, Regex.Count(card, "product-card__discount-dates", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Equal(1, Regex.Count(card, "100 TL", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public async Task AProductWithoutADiscount_ShowsOneStruckFreePrice_T545_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string card = Cards(GridOf(await GetAsync(client, Grid)), "product-card")[0];

        Assert.Contains("class=\"product-card__price\">120 TL<", card, StringComparison.Ordinal);
        Assert.DoesNotContain("<s ", card, StringComparison.Ordinal);
        Assert.DoesNotContain("product-card__discount", card, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ASoldOutProduct_StaysInTheListAndSaysTukendi_TheOthersDoNot_T545_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string[] cards = Cards(GridOf(await GetAsync(client, Grid)), "product-card");

        Assert.Contains("Tükenmiş avize", cards[2], StringComparison.Ordinal);
        Assert.Contains("class=\"product-card__sold-out\">Tükendi<", cards[2], StringComparison.Ordinal);
        Assert.Contains("1.500,50 TL", cards[2], StringComparison.Ordinal);
        Assert.DoesNotContain("Tükendi", string.Concat(cards.Where((_, i) => i != 2)), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ALongName_IsInTheMarkupWhole_TheTwoLineCutIsTheStylesAndTheWholeNameIsOnTheProductPage_T545_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string card = Cards(GridOf(await GetAsync(client, Grid)), "product-card")[3];

        Assert.Contains("Çok uzun bir ürün adı ki kartta iki satırdan fazlasına yayılıp kesilmesi gereken bir ad", card, StringComparison.Ordinal);
        Assert.Contains("href=\"/urun/uzun-ad\"", card, StringComparison.Ordinal);
    }

    // ---- KK6: the grid has the list's order and no sort control ----

    [Fact]
    public async Task TheGrid_KeepsTheListsOrder_AndThePageHasNoSortControl_T545_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Grid);
        string[] names = [.. Regex.Matches(html, "product-card__name\">(.*?)<", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Select(m => m.Groups[1].Value)];

        Assert.StartsWith("Düz ampul", names[0], StringComparison.Ordinal);
        Assert.Equal(["Düz ampul", "İndirimli lamba", "Tükenmiş avize"], names[..3]);
        Assert.DoesNotMatch("<select|name=\"(?:sort|orderBy)\"|[?&](?:sort|orderBy|siralama)=", html);
    }

    // ---- KK8: the numbered navigation ----

    [Fact]
    public async Task ThePager_IsNumbered_TheFirstPageHasNoPrevious_AndNoAddressCarriesPageOne_T545_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Grid);

        Assert.Equal(["1", "2", "…", "12", "Sonraki"], PagerOf(html));
        Assert.DoesNotContain("rel=\"prev\"", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/odeme/liste-urunler?page=2\" rel=\"next\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("page=1\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("page=1&", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePagerInTheMiddle_ShowsTheNeighboursAndKeepsTheSearchWord_ThePageShownIsNotALink_T545_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Grid + "?search=ampul&page=6");

        Assert.Equal(["Önceki", "1", "…", "5", "6", "7", "…", "12", "Sonraki"], PagerOf(html));
        Assert.Contains("href=\"/odeme/liste-urunler?search=ampul&page=5\"", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/odeme/liste-urunler?search=ampul\"", html, StringComparison.Ordinal);
        Assert.Contains("<span class=\"pager__current\" aria-current=\"page\">6</span>", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("?page=99", "12")]
    [InlineData("?page=0", "1")]
    [InlineData("?page=abc", "1")]
    [InlineData("?page=99999999999", "12")]
    public async Task APageOutOfRangeOrWrong_IsTheLastOrTheFirstPage_NotAnError_T545_KK8(string query, string shown)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Grid + query);

        Assert.Contains($"aria-current=\"page\">{shown}</span>", html, StringComparison.Ordinal);
    }

    // ---- KK4: the content cards ----

    [Fact]
    public async Task TheContentList_FollowsTheGivenOrder_ARecordWithoutAnImageIsTextOnly_AndNothingSellsOrPrices_T545_KK4_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string grid = GridOf(await GetAsync(client, "/odeme/liste-icerik"));
        string[] cards = Cards(grid, "content-card");

        Assert.Equal(2, cards.Length);
        Assert.Contains("Zeta hizmeti", cards[0], StringComparison.Ordinal);
        Assert.Contains("Alfa hizmeti", cards[1], StringComparison.Ordinal);
        Assert.Contains("<img class=\"content-card__image\"", cards[0], StringComparison.Ordinal);
        Assert.Contains("alt=\"Zeta hizmeti\"", cards[0], StringComparison.Ordinal);
        Assert.Contains("class=\"content-card__description\">Zeta açıklaması.<", cards[0], StringComparison.Ordinal);
        Assert.DoesNotContain("<img", cards[1], StringComparison.Ordinal);
        Assert.DoesNotContain("content-card__description", cards[1], StringComparison.Ordinal);
        Assert.DoesNotMatch("TL|<form|<button|<input|<select", grid);
    }

    // ---- KK7, KK10: the empty-state line, and the search field only in the listed lists ----

    [Theory]
    [InlineData("StorefrontProducts", true)]
    [InlineData("PanelOrders", true)]
    [InlineData("PanelProducts", true)]
    [InlineData("PanelMembers", true)]
    [InlineData("PanelCoupons", false)]
    [InlineData("PanelContent", false)]
    [InlineData("ContentList", false)]
    [InlineData("MemberOrders", false)]
    [InlineData("PanelAuditTrail", false)]
    public async Task TheSearchField_IsDrawnInTheListedListsOnly_AGetFormWithTheOneParameter_T545_KK7(string kind, bool drawn)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, "/odeme/liste-bos?kind=" + kind);

        Assert.Equal(drawn, html.Contains("class=\"list-search\"", StringComparison.Ordinal));
        if (drawn)
        {
            Assert.Contains("method=\"get\" action=\"/odeme/liste-bos\"", html, StringComparison.Ordinal);
            Assert.Contains("name=\"search\"", html, StringComparison.Ordinal);
            Assert.DoesNotContain("name=\"page\"", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task AnEmptyList_IsOneSentenceAndAWayOut_NoNavigationNoErrorMark_T545_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, "/odeme/liste-bos?kind=PanelCoupons");

        Assert.Contains("<p class=\"empty-state__message\">Bu listede kayıt yok.</p>", html, StringComparison.Ordinal);
        Assert.Contains("class=\"button button--secondary empty-state__action\" href=\"/\">Ana sayfaya dön</a>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"pager\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("role=\"alert\"", html, StringComparison.Ordinal);
    }

    // ---- KK9: the panel table ----

    [Fact]
    public async Task ThePanelTable_IsOneMarkupWithATitledCellInEveryColumn_TheRecordLinkIsOnTheFirstPrimaryCell_AndThereIsNoSelectionColumn_T545_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, "/odeme/liste-tablo");

        Assert.Equal(3, Regex.Count(html, "<tr class=\"panel-table__row\">", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Equal(3, Regex.Count(html, "<th scope=\"col\"", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Equal(9, Regex.Count(html, "<td class=\"panel-table__cell panel-table__cell--(?:primary|secondary)\" data-label=\"(?:Sipariş|Durum|E-posta)\">", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Contains("<a href=\"/panel/siparisler/2\">SP-1002</a>", html, StringComparison.Ordinal);
        Assert.Contains("<a href=\"/panel/siparisler/3\">SP-1003</a>", html, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Count(html, "<a ", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.DoesNotMatch("<input|<button|<form|checkbox", html);
        Assert.Contains("panel-table__cell--secondary\" data-label=\"E-posta\">", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ACellOfThePanelTable_IsEncoded_T545_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetRawAsync(client, "/odeme/liste-tablo");

        Assert.DoesNotContain("<b>b</b>", html, StringComparison.Ordinal);
        Assert.Contains("&lt;b&gt;b&lt;/b&gt;@example.test", html, StringComparison.Ordinal);
    }
}
