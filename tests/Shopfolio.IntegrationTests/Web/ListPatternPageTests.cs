using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The list pattern on the opened application and the test-only sample pages (T210 criteria 1, 2, 3, 5, 6, 7, 8, 9; KK1, KK2, KK3, KK5-KK9; 07 2.6): the
/// parameters are the camelCase names of the query model's fields, the first page is parameterless, a wrong page or filter value is neither an error nor a
/// filter, the two range rules are field messages, a blank search word makes the empty search screen, the closed mark follows the parameters the address
/// carries, and the way back from a record to its list passes the one local-path check at both ends. The sample pages live in the test host only.
/// </summary>
[Collection(ListPagingDefinition.Name)]
public sealed class ListPatternPageTests : IAsyncLifetime
{
    private const string Category = "/sss/kategori-listesi/ornek";
    private const string Search = "/arama";
    private const string PanelList = "/panel/urun-listesi";
    private const string DateList = "/panel/tarih-araligi";
    private const string NoResult = "no-result-list";

    private static readonly Regex _items = new("""<li class="item">(.*?)</li>""", RegexOptions.Singleline | RegexOptions.CultureInvariant);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    public ListPatternPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ListSeed.ClearAsync(_database.Dsn);
        await ListSeed.SeedPublishedAsync(_database.Dsn, 60);
        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // The sample pages live in the test assembly: the host serves them as an application part.
    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ProductQuery).Assembly)));

    private HttpClient PanelClient(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add("Cookie", $"{CookieNames.AdminSession}={_adminKey}");
        return client;
    }

    private static async Task<string> GetAsync(HttpClient client, string address)
    {
        using HttpResponseMessage response = await client.GetAsync(address, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));
    }

    private static string Field(string html, string cssClass)
    {
        Match match = Regex.Match(html, $"""<p class="{cssClass}">(.*?)</p>""", RegexOptions.Singleline | RegexOptions.CultureInvariant);
        Assert.True(match.Success, $"'{cssClass}' is not in the page.");
        return match.Groups[1].Value;
    }

    private static string[] ItemsOf(string html) => [.. _items.Matches(html).Select(m => m.Groups[1].Value.Trim())];

    private static string Number(int n) => ListSeed.NameOf(n);

    // --- KK1, KK2: the parameter names are the camelCase field names; page=1 is the parameterless first page ---

    [Fact]
    public async Task TheParametersAreTheCamelCaseNamesOfTheModelFields_AndEachOneFilters()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        Assert.Equal("6", Field(await GetAsync(client, Category + "?minPrice=55"), "total"));
        Assert.Equal("5", Field(await GetAsync(client, Category + "?maxPrice=5"), "total"));
        Assert.Equal("11", Field(await GetAsync(client, Category + "?minPrice=10&maxPrice=20"), "total"));
        Assert.Equal("30", Field(await GetAsync(client, Category + "?inStock=true"), "total"));
        Assert.Equal("1", Field(await GetAsync(client, Search + "?search=urun%20010"), "total"));
        Assert.Equal("60", Field(await GetAsync(client, Category), "total"));
    }

    [Fact]
    public async Task PageOne_AndNoPage_ShowTheSamePage_AndPageTwoShowsTheNextOne()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string bare = await GetAsync(client, Category);
        string one = await GetAsync(client, Category + "?page=1");
        string two = await GetAsync(client, Category + "?page=2");

        Assert.Equal(ItemsOf(bare), ItemsOf(one));
        Assert.Equal("1", Field(bare, "shown-page"));
        Assert.Equal("1", Field(one, "shown-page"));
        Assert.Equal("2", Field(two, "shown-page"));
        Assert.Equal(PageSizes.StorefrontProducts, ItemsOf(bare).Length);
        Assert.Empty(ItemsOf(bare).Intersect(ItemsOf(two)));
        Assert.Equal("3", Field(bare, "page-count"));
    }

    // --- KK4 on the page: the newest first, the inner key last ---

    [Fact]
    public async Task TheStorefrontList_IsNewestFirst_WithNoSortParameter()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string[] first = ItemsOf(await GetAsync(client, Category));
        string[] withSort = ItemsOf(await GetAsync(client, Category + "?sort=price&orderBy=name"));

        Assert.Equal(Number(60), first[0]);
        Assert.Equal(Number(60 - PageSizes.StorefrontProducts + 1), first[^1]);
        Assert.Equal(first, withSort);
    }

    // --- KK3: abc, 0, -1 and a huge value ---

    [Theory]
    [InlineData("abc", "1")]
    [InlineData("0", "1")]
    [InlineData("-1", "1")]
    [InlineData("", "1")]
    [InlineData("3", "3")]
    [InlineData("4", "3")]
    [InlineData("99999999999", "3")]
    [InlineData("2147483648", "3")]
    public async Task TheShownPage_ForTheValueAsked(string asked, string shown)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Category + "?page=" + Uri.EscapeDataString(asked));

        Assert.Equal(shown, Field(html, "shown-page"));
        Assert.NotEmpty(ItemsOf(html));
    }

    // --- KK5: an invalid value applies no filter and is no error; an unknown parameter is ignored ---

    [Fact]
    public async Task InvalidFilterValues_ApplyNoFilter_AndGiveNoError_AndAnUnknownParameterIsIgnored()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Category + "?minPrice=abc&maxPrice=1%2C2%2C3&inStock=maybe&unknown=1&page=1");

        Assert.Equal("60", Field(html, "total"));
        Assert.DoesNotContain("class=\"error\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ADateRangeThatBreaksTheRule_GivesTheFieldMessage_OfTheEndDate()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = PanelClient(factory);

        string html = await GetAsync(client, DateList + "?startDate=2026-11-10&endDate=2026-11-01");

        Match error = Regex.Match(html, """<span class="error" data-field="([^"]*)">([^<]*)</span>""");
        Assert.True(error.Success);
        Assert.EndsWith("EndDate", error.Groups[1].Value, StringComparison.Ordinal);
        Assert.Equal("Bitiş tarihi başlangıç tarihinden önce olamaz.", error.Groups[2].Value);
        Assert.Equal("|", Field(html, "applied"));
    }

    [Theory]
    [InlineData("?startDate=2026-11-01&endDate=2026-11-10", "2026-11-01|2026-11-10")]
    [InlineData("?startDate=2026-11-10&endDate=2026-11-10", "2026-11-10|2026-11-10")]
    [InlineData("?startDate=abc&endDate=2026-11-10", "|2026-11-10")]
    [InlineData("?startDate=2026-13-45&endDate=", "|")]
    [InlineData("", "|")]
    public async Task AValidOrInvalidDate_GivesNoRangeMessage(string query, string applied)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = PanelClient(factory);

        string html = await GetAsync(client, DateList + query);

        Assert.Equal(applied, Field(html, "applied"));
        Assert.DoesNotContain("class=\"error\"", html, StringComparison.Ordinal);
    }

    // --- KK6: an inverted price range has a message and no filter ---

    [Fact]
    public async Task AnInvertedPriceRange_HasTheMessage_AndNoFilter()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Category + "?minPrice=50&maxPrice=10");

        Assert.Equal("60", Field(html, "total"));
        Match error = Regex.Match(html, """<span class="error" data-field="([^"]*)">([^<]*)</span>""");
        Assert.True(error.Success);
        Assert.EndsWith("MinPrice", error.Groups[1].Value, StringComparison.Ordinal);
        Assert.Equal("En düşük fiyat üst değerden büyük olamaz.", error.Groups[2].Value);
    }

    [Fact]
    public async Task EqualPriceBounds_AreNoInversion()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Category + "?minPrice=10&maxPrice=10");

        Assert.Equal("1", Field(html, "total"));
        Assert.DoesNotContain("class=\"error\"", html, StringComparison.Ordinal);
    }

    // --- KK7: a blank search word is the empty search screen, not an empty result ---

    [Theory]
    [InlineData("")]
    [InlineData("?search=")]
    [InlineData("?search=%20%20%20")]
    [InlineData("?search=%09")]
    public async Task ABlankSearchWord_ShowsTheSearchField_AndMakesNoResultList(string query)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Search + query);

        Assert.Contains(NoResult, html, StringComparison.Ordinal);
        Assert.Contains("class=\"search-field\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"total\"", html, StringComparison.Ordinal);
        Assert.Empty(ItemsOf(html));
    }

    [Fact]
    public async Task AWordWithNoMatch_IsAnEmptyResultList_NotTheEmptySearchScreen()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, Search + "?search=zzzz");

        Assert.DoesNotContain(NoResult, html, StringComparison.Ordinal);
        Assert.Equal("0", Field(html, "total"));
    }

    // --- KK8: the mark follows the parameters the address carries ---

    [Theory]
    [InlineData(Category, "open")]
    [InlineData(Category + "?page=2", "open")]
    [InlineData(Category + "?page=abc", "open")]
    [InlineData(Category + "?minPrice=1", "closed")]
    [InlineData(Category + "?minPrice=1&page=2", "closed")]
    [InlineData(Category + "?minPrice=abc", "closed")]
    [InlineData(Category + "?unknown=1", "closed")]
    [InlineData(Category + "?inStock=true", "closed")]
    [InlineData(Search, "closed")]
    [InlineData(Search + "?search=urun", "closed")]
    [InlineData(Search + "?page=2", "closed")]
    public async Task TheMarkOfTheListPage(string address, string expected)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetAsync(client, address);

        Assert.Equal(expected, Field(html, "closed"));
    }

    // --- KK9: from the panel list to the record and back ---

    [Fact]
    public async Task TheLinkToARecord_CarriesTheAddressOfTheListWithItsPageAndFilter()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = PanelClient(factory);

        string html = await GetAsync(client, PanelList + "?search=urun&publishStatus=Published&page=2");

        string listAddress = Field(html, "list-address");
        Assert.Equal("/panel/urun-listesi?search=urun&publishStatus=Published&page=2", listAddress);
        Assert.Equal("2", Field(html, "shown-page"));
        Assert.Matches("""href="/panel/urun-kaydi/[0-9a-f-]{36}\?returnUrl=""" + Regex.Escape(Uri.EscapeDataString(listAddress)) + "\"", html);
    }

    [Fact]
    public async Task TheFirstPageOfTheList_HasNoPageInItsAddress()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = PanelClient(factory);

        string html = await GetAsync(client, PanelList + "?page=1");

        Assert.Equal(PanelList, Field(html, "list-address"));
    }

    private static readonly Guid _record = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Theory]
    [InlineData("/panel/urun-listesi?search=urun&page=2")]
    [InlineData("/panel/urun-listesi")]
    public async Task ALocalReturnUrl_IsTheWayBack_AndTheRedirectAfterTheSave(string returnUrl)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = PanelClient(factory);
        string record = $"/panel/urun-kaydi/{_record}";

        string html = await GetAsync(client, record + "?returnUrl=" + Uri.EscapeDataString(returnUrl));
        Assert.Contains($"class=\"back-to-list\" href=\"{returnUrl}\"", html, StringComparison.Ordinal);

        using HttpResponseMessage saved = await client.PostAsync(record + "?handler=SaveProductRecord", Form(returnUrl), Ct);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal(returnUrl, saved.Headers.Location!.OriginalString);
    }

    [Theory]
    [InlineData("https://evil.example/panel/urun-listesi")]
    [InlineData("//evil.example/x")]
    [InlineData("/\\evil.example")]
    [InlineData("\\\\evil.example\\share")]
    [InlineData("javascript:alert(1)")]
    [InlineData("urun-listesi")]
    [InlineData("")]
    [InlineData("   ")]
    public async Task ANotLocalOrEmptyReturnUrl_ReturnsToTheRootOfTheList(string returnUrl)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = PanelClient(factory);
        string record = $"/panel/urun-kaydi/{_record}";

        string html = await GetAsync(client, record + "?returnUrl=" + Uri.EscapeDataString(returnUrl));
        Assert.Contains($"class=\"back-to-list\" href=\"{PanelList}\"", html, StringComparison.Ordinal);

        using HttpResponseMessage saved = await client.PostAsync(record + "?handler=SaveProductRecord", Form(returnUrl), Ct);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal(PanelList, saved.Headers.Location!.OriginalString);
    }

    [Fact]
    public async Task ARecordOpenedWithNoReturnUrl_ReturnsToTheRootOfTheList()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = PanelClient(factory);
        string record = $"/panel/urun-kaydi/{_record}";

        string html = await GetAsync(client, record);
        Assert.Contains($"class=\"back-to-list\" href=\"{PanelList}\"", html, StringComparison.Ordinal);

        using HttpResponseMessage saved = await client.PostAsync(record + "?handler=SaveProductRecord", new FormUrlEncodedContent([]), Ct);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
        Assert.Equal(PanelList, saved.Headers.Location!.OriginalString);
    }

    private static FormUrlEncodedContent Form(string returnUrl) => new([new KeyValuePair<string, string>("ReturnUrl", returnUrl)]);
}
