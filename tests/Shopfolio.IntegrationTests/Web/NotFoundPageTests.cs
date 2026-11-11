using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the error pages' tests (T504): one database copied from the template, so the products the tests publish and remove touch no other test.</summary>
[CollectionDefinition(NotFoundPageTestsDefinition.Name)]
public sealed class NotFoundPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "NotFoundPageTests";
}

/// <summary>
/// The "page not found" page E-06 and the unexpected error pages on the opened application (T504; 07 UC-06, UC-78; 04 5.6, 2.7.3; 09 8.1, 15.2). The real catching layer draws the real pages; the
/// sources of the 404 are the real unknown address, the real signed download address and the test-only pages that return the one denied-access answer. The statuses and the mechanism of the 500 are
/// T212's (<see cref="UnexpectedErrorLayerTests"/>); here the frame of the 500 page on each surface and the 404's difference from it are read.
/// </summary>
[Collection(NotFoundPageTestsDefinition.Name)]
public sealed class NotFoundPageTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 11, 9, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private readonly CollectionDatabaseFixture _database;

    private readonly string _adminKey;

    public NotFoundPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
        _adminKey = TestSessions.AdminAsync(database.Dsn).GetAwaiter().GetResult().Key;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE products CASCADE", Ct);
        await db.Database.ExecuteSqlRawAsync("TRUNCATE categories CASCADE", Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private ShopfolioWebApplicationFactory OpenBase() => new(_database.Dsn);

    private static WebApplicationFactory<Program> WithSamples(ShopfolioWebApplicationFactory factory) =>
        factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(ErrorSampleModel).Assembly);
            s.AddSingleton(new AccessWriteLog());
        }));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory, bool panel = false)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        if (panel)
        {
            client.DefaultRequestHeaders.Add("Cookie", $"{CookieNames.AdminSession}={_adminKey}");
        }

        return client;
    }

    private static async Task<(HttpResponseMessage Response, string Html)> GetAsync(HttpClient client, string path)
    {
        HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private async Task SeedPublishedProductInCategoryAsync(string categoryName, string categorySlug)
    {
        await using AppDbContext db = NewContext();
        Category category = Category.Create(categoryName, categorySlug, null, 1, Now);
        Product product = Product.Create($"Urun {categorySlug}", $"urun-{categorySlug}", ProductType.Physical, 20m, Now).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        db.Categories.Add(category);
        db.Products.Add(product);
        db.ProductCategories.Add(ProductCategory.Create(product.Id, category.Id, true, Now));
        await db.SaveChangesAsync(Ct);
    }

    // The markup without the response's own nonce and the sign-in link's return address (the frame writes the request's own path there): two 404 answers are the same page when nothing else differs.
    private static string WithoutNonce(string html) =>
        Regex.Replace(Regex.Replace(html, "nonce=\"[^\"]*\"", "nonce=\"-\"", RegexOptions.CultureInvariant, PatternTimeout), "returnUrl=[^\"]*", "returnUrl=-", RegexOptions.CultureInvariant, PatternTimeout);

    private static string Region(string html, string startMarker, string endMarker)
    {
        int start = html.IndexOf(startMarker, StringComparison.Ordinal);
        Assert.True(start >= 0, $"No '{startMarker}' in the response.");
        int end = html.IndexOf(endMarker, start, StringComparison.Ordinal);
        Assert.True(end > start, $"No '{endMarker}' after '{startMarker}'.");
        return html[start..end];
    }

    private static void AssertNoCode(HttpResponseMessage response, string html)
    {
        string headers = response.ToString();
        foreach (string code in Enum.GetNames<ErrorCode>())
        {
            Assert.DoesNotContain(code, html, StringComparison.Ordinal);
            Assert.DoesNotContain(code, headers, StringComparison.Ordinal);
        }
    }

    // ---- KK1: the frame, the way home, the way to the products ----

    [Fact]
    public async Task AnUnknownAddress_IsE06_InTheFullStorefrontFrame_WithTheTitle_TheWayHome_AndTheFirstLevelCategories_T504_KK1()
    {
        await SeedPublishedProductInCategoryAsync("Mobilya", "mobilya");
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        using HttpClient client = ClientOf(baseFactory);

        (HttpResponseMessage response, string html) = await GetAsync(client, "/bilinmeyen-bolum");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.StartsWith("text/html", response.Content.Headers.ContentType?.ToString(), StringComparison.Ordinal);
        Assert.Contains("<header class=\"site-header", html, StringComparison.Ordinal);
        Assert.Contains("<footer class=\"site-footer\">", html, StringComparison.Ordinal);
        Assert.Contains("<title>Sayfa bulunamadı", html, StringComparison.Ordinal);
        string page = Region(html, "<section class=\"not-found\">", "</section>");
        Assert.Contains("<h1 class=\"not-found__title\">Sayfa bulunamadı</h1>", page, StringComparison.Ordinal);
        Assert.Contains("<a class=\"button button--secondary\" href=\"/\">Ana sayfaya dön</a>", page, StringComparison.Ordinal);
        Assert.Contains("Ürünlere dön", page, StringComparison.Ordinal);
        Assert.Matches(new Regex("<li><a href=\"[^\"]*mobilya\">Mobilya</a></li>", RegexOptions.CultureInvariant, PatternTimeout), page);
        Assert.DoesNotContain("button--primary", page, StringComparison.Ordinal);
        Assert.DoesNotContain("<form", page, StringComparison.Ordinal);
        AssertNoCode(response, html);
    }

    [Fact]
    public async Task WhileNoProductIsPublished_TheWayToTheProductsIsNotShown_T504_KK1()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        using HttpClient client = ClientOf(baseFactory);

        (HttpResponseMessage response, string html) = await GetAsync(client, "/bilinmeyen-bolum");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        string page = Region(html, "<section class=\"not-found\">", "</section>");
        Assert.Contains("Ana sayfaya dön", page, StringComparison.Ordinal);
        Assert.DoesNotContain("Ürünlere dön", page, StringComparison.Ordinal);
        Assert.DoesNotContain("not-found__list", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePageIsE06_NotTheUnexpectedErrorPage_AndAnAdministratorsBrowserSeesTheStripAbove_T504_KK1()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        using HttpClient client = ClientOf(baseFactory, panel: true);

        (HttpResponseMessage response, string html) = await GetAsync(client, "/bilinmeyen-bolum");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.DoesNotContain("error-page", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Sayfa açılamadı", html, StringComparison.Ordinal);
        Assert.Contains("<div class=\"admin-strip\"", html, StringComparison.Ordinal);
        Assert.True(html.IndexOf("admin-strip", StringComparison.Ordinal) < html.IndexOf("<header class=\"site-header", StringComparison.Ordinal));
    }

    // ---- KK2, KK3: every source gives the same answer; the reason is not told ----

    [Fact]
    public async Task TheUnknownAddress_TheDeniedOrderPages_AndTheInvalidDownloadValue_AreThePageWithTheSameMarkup_T504_KK2_KK3()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);

        // T206's first path segment, T207's ownership denial (the member root of a visitor and a wrong access key), T312's signed value that is not valid.
        string[] paths = ["/bilinmeyen-bolum", "/siparis/2026-ABC123", "/siparis/erisim/yanlis-anahtar", "/indir/gecersiz-deger"];
        List<string> pages = [];
        foreach (string path in paths)
        {
            (HttpResponseMessage response, string html) = await GetAsync(client, path);
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            Assert.Contains("<h1 class=\"not-found__title\">Sayfa bulunamadı</h1>", html, StringComparison.Ordinal);
            pages.Add(WithoutNonce(html));
        }

        Assert.All(pages, page => Assert.Equal(pages[0], page));
    }

    [Fact]
    public async Task APostThatFailsTheOwnershipCheck_GetsTheSamePage_AndWritesNothing_T504_KK2()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);
        (_, string unknown) = await GetAsync(client, "/bilinmeyen-bolum");

        using HttpResponseMessage response = await client.PostAsync(new Uri("/siparis/erisim/yanlis-anahtar?handler=Cancel", UriKind.Relative), new FormUrlEncodedContent([]), Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(WithoutNonce(unknown), WithoutNonce(html));
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    public async Task ARecordOfThePanelThatIsNotThere_IsThePageToo_WithTheStrip_T504_KK2()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory, panel: true);

        (HttpResponseMessage response, string html) = await GetAsync(client, $"/panel/kayit/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("<h1 class=\"not-found__title\">Sayfa bulunamadı</h1>", html, StringComparison.Ordinal);
        Assert.Contains("<div class=\"admin-strip\"", html, StringComparison.Ordinal);
        Assert.Contains("<meta name=\"robots\" content=\"noindex\"", html, StringComparison.Ordinal);
    }

    // ---- KK3: noindex, no counter, no error line, no cookie, a page that is found stays 200 ----

    [Fact]
    public async Task TheNotFoundPage_IsNoindex_NoStore_WritesNoCookie_AndNeitherCountsNorLogsAnError_T504_KK3()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        using HttpClient client = ClientOf(baseFactory);
        UnexpectedErrorCounter counter = baseFactory.Services.GetRequiredService<UnexpectedErrorCounter>();
        int before = counter.CountWithin(TimeSpan.FromMinutes(15));

        (HttpResponseMessage response, string html) = await GetAsync(client, "/bilinmeyen-bolum");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Contains("<meta name=\"robots\" content=\"noindex\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("og:title", html, StringComparison.Ordinal);
        Assert.Equal(SecurityHeaders.NoStore, response.Headers.CacheControl?.ToString());
        Assert.False(response.Headers.Contains("Set-Cookie"));
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal(before, counter.CountWithin(TimeSpan.FromMinutes(15)));
        Assert.DoesNotContain("UnexpectedPageError", ReadLog(baseFactory), StringComparison.Ordinal);
    }

    [Fact]
    public async Task APageThatIsFound_EvenWithNothingToShow_StaysA200_AndIsNotThePage_T504_KK3()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);

        (HttpResponseMessage response, string html) = await GetAsync(client, "/odeme/hata-ornegi");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.DoesNotContain("not-found", html, StringComparison.Ordinal);
    }

    // ---- The answers that are no page ----

    [Fact]
    public async Task AMachineEndpointsAndAStaticFilesNotFound_StayEmpty_T504_KK2()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        using HttpClient client = ClientOf(baseFactory, panel: true);

        (HttpResponseMessage upload, string uploadBody) = await GetAsync(client, $"/panel/yukleme/{Guid.NewGuid()}");
        (HttpResponseMessage asset, string assetBody) = await GetAsync(client, "/statik/yok-boyle-bir-dosya.css");

        Assert.Equal(HttpStatusCode.NotFound, upload.StatusCode);
        Assert.Equal(string.Empty, uploadBody);
        Assert.Equal(HttpStatusCode.NotFound, asset.StatusCode);
        Assert.Equal(string.Empty, assetBody);
    }

    // ---- KK4: the 500 page in the frame of each surface ----

    [Fact]
    public async Task TheStorefront500_IsInTheStorefrontFrame_AndIsNotThePageNotFound_T504_KK4_KK7()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);
        UnexpectedErrorCounter counter = factory.Services.GetRequiredService<UnexpectedErrorCounter>();
        int before = counter.CountWithin(TimeSpan.FromMinutes(15));

        (HttpResponseMessage response, string html) = await GetAsync(client, "/odeme/hata-ornegi?fail=handler");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("<header class=\"site-header", html, StringComparison.Ordinal);
        Assert.Contains("<footer class=\"site-footer\">", html, StringComparison.Ordinal);
        Assert.Contains("<section class=\"error-page\">", html, StringComparison.Ordinal);
        Assert.Contains("Sayfa açılamadı", html, StringComparison.Ordinal);
        Assert.DoesNotContain("not-found", html, StringComparison.Ordinal);
        Assert.DoesNotContain("t212 failure", html, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(InvalidOperationException), html, StringComparison.Ordinal);
        AssertNoCode(response, html);
        Assert.Equal(before + 1, counter.CountWithin(TimeSpan.FromMinutes(15)));
        Assert.Contains("UnexpectedPageError", ReadLog(baseFactory), StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePanel500_IsInThePanelFrame_T504_KK4()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory, panel: true);

        (HttpResponseMessage response, string html) = await GetAsync(client, "/panel/hata-ornegi?fail=handler");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("<header class=\"panel-top\">", html, StringComparison.Ordinal);
        Assert.Contains("<section class=\"error-page\">", html, StringComparison.Ordinal);
        Assert.DoesNotContain("site-header", html, StringComparison.Ordinal);
        Assert.DoesNotContain("t212 failure", html, StringComparison.Ordinal);
        AssertNoCode(response, html);
    }

    [Fact]
    public async Task TheSignInScreensOfThePanel_HaveNoFrame_AndTheir500_IsBare_WithThePanelTitleOnly_T504_KK4()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);

        (HttpResponseMessage response, string html) = await GetAsync(client, FramelessErrorSampleModel.Path);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("<section class=\"error-page\">", html, StringComparison.Ordinal);
        Assert.Contains("panel-bare__title", html, StringComparison.Ordinal);
        Assert.DoesNotContain("<header class=\"panel-top\">", html, StringComparison.Ordinal);
        Assert.DoesNotContain("site-header", html, StringComparison.Ordinal);
        Assert.DoesNotContain("admin-strip", html, StringComparison.Ordinal);
        Assert.DoesNotContain(FramelessErrorSampleModel.FailureText, html, StringComparison.Ordinal);
        AssertNoCode(response, html);
    }

    // ---- KK5, KK6: the POST 500 and the uncertain outcome (the mechanism is T212's; here the page and the target are read once more) ----

    [Fact]
    public async Task APost500_ShowsTheFormAgainWithTheSentValue_AndALastingMessage_T504_KK5()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(
            new Uri("/odeme/hata-ornegi?handler=Save", UriKind.Relative), new FormUrlEncodedContent([new("Input.Name", "korunan-deger")]), Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("korunan-deger", html, StringComparison.Ordinal);
        Assert.Contains("İşlem tamamlanamadı", html, StringComparison.Ordinal);
        AssertNoCode(response, html);
    }

    [Fact]
    public async Task AnUncertainOperationsError_IsA303ToTheScreenThatShowsTheResult_T504_KK6()
    {
        await using ShopfolioWebApplicationFactory baseFactory = OpenBase();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(
            new Uri("/odeme/hata-ornegi?handler=Confirm", UriKind.Relative), new FormUrlEncodedContent([new("Input.Mode", "render")]), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(ErrorSampleModel.ResultScreen, response.Headers.Location?.OriginalString);
    }

    // The whole application log as text; the writer keeps its file open, so the reader allows its access.
    private static string ReadLog(ShopfolioWebApplicationFactory factory)
    {
        System.Text.StringBuilder text = new();
        foreach (string path in Directory.GetFiles(factory.LogDirectory, "app-*.jsonl"))
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new(stream);
            text.Append(reader.ReadToEnd());
        }

        return text.ToString();
    }
}
