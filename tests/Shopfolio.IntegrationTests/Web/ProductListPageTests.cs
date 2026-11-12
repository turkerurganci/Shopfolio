using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the product screens' tests (T525): one database copied from the template; every test sets the catalog up again.</summary>
[CollectionDefinition(ProductListPageTestsDefinition.Name)]
public sealed class ProductListPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "ProductListPages";
}

/// <summary>
/// The product list (E-32) on the opened application and a real database (T525 criterion 1, KK1; 07 UC-87; 04 9.4): newest first, the name search that finds a draft and an archived product, the publish
/// status filter, the server's price range and stock column, the row's and "Yeni ürün"'s returnUrl, the empty states, the page border and the number of commands.
/// </summary>
[Collection(ProductListPageTestsDefinition.Name)]
public sealed partial class ProductListPageTests : IAsyncLifetime
{
    private const string ListPath = "/panel/urunler";

    private static readonly DateTimeOffset Base = new(2026, 10, 1, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    private string _adminKey = string.Empty;

    public ProductListPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex(@"<tr class=""panel-table__row""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RowStart();

    [GeneratedRegex(@"href=""/panel/urunler/([0-9a-f-]{36})\?", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RecordLink();

    public async ValueTask InitializeAsync()
    {
        await ListSeed.ClearAsync(_database.Dsn);
        _adminKey = (await TestSessions.AdminAsync(_database.Dsn)).Key;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    [Fact]
    public async Task TheList_ShowsEveryStatusNewestFirst_WithTheSearchAndTheStatusFilter_KK1()
    {
        Guid draft = await AddProductAsync("Taslak Kupa", PublishStatus.Draft, Base.AddMinutes(1), (10m, 3));
        Guid published = await AddProductAsync("Yayında Kupa", PublishStatus.Published, Base.AddMinutes(2), (10m, 3));
        Guid archived = await AddProductAsync("Arşiv Kupa", PublishStatus.Archived, Base.AddMinutes(3), (10m, 3));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, ListPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Equal([archived, published, draft], Links(html));
        Assert.Contains("name=\"search\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"publishStatus\"", html, StringComparison.Ordinal);
        Assert.Contains(">Taslak<", html, StringComparison.Ordinal);
        Assert.Contains(">Yayında<", html, StringComparison.Ordinal);
        Assert.Contains(">Arşiv<", html, StringComparison.Ordinal);
        Assert.DoesNotContain("style=", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheSearch_FindsADraftAndAnArchivedProduct_ByTheSearchedFormOfTheName_KK1()
    {
        Guid draft = await AddProductAsync("ÇİÇEKLİ Kupa", PublishStatus.Draft, Base.AddMinutes(1), (10m, 3));
        Guid archived = await AddProductAsync("Çiçekli Tabak", PublishStatus.Archived, Base.AddMinutes(2), (10m, 3));
        Guid other = await AddProductAsync("Düz Bardak", PublishStatus.Published, Base.AddMinutes(3), (10m, 3));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, ListPath + "?search=" + Uri.EscapeDataString("çiçekli"));

        Assert.Equal([archived, draft], Links(html));
        Assert.DoesNotContain(other.ToString("D"), html, StringComparison.Ordinal);
        Assert.Contains("value=\"çiçekli\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheStatusFilter_KeepsOneStatus_AndTogetherWithTheSearchNarrowsBoth_KK1()
    {
        Guid draft = await AddProductAsync("Kupa A", PublishStatus.Draft, Base.AddMinutes(1), (10m, 3));
        Guid published = await AddProductAsync("Kupa B", PublishStatus.Published, Base.AddMinutes(2), (10m, 3));
        Guid otherDraft = await AddProductAsync("Tabak", PublishStatus.Draft, Base.AddMinutes(3), (10m, 3));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string drafts) = await GetAsync(client, ListPath + "?publishStatus=Draft");
        (_, string both) = await GetAsync(client, ListPath + "?publishStatus=Draft&search=kupa");

        Assert.Equal([otherDraft, draft], Links(drafts));
        Assert.Equal([draft], Links(both));
        Assert.DoesNotContain(published.ToString("D"), both, StringComparison.Ordinal);
        Assert.Contains("<option value=\"Draft\" selected=\"selected\">", drafts, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnInvalidFilterValue_AppliesNoFilter_AndAPagePastTheLastIsTheLast_KK1()
    {
        Guid draft = await AddProductAsync("Kupa", PublishStatus.Draft, Base.AddMinutes(1), (10m, 3));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode invalidStatus, string invalid) = await GetAsync(client, ListPath + "?publishStatus=Nope&page=abc");
        (HttpStatusCode hugeStatus, string huge) = await GetAsync(client, ListPath + "?page=99999999999");

        Assert.Equal(HttpStatusCode.OK, invalidStatus);
        Assert.Equal([draft], Links(invalid));
        Assert.Equal(HttpStatusCode.OK, hugeStatus);
        Assert.Equal([draft], Links(huge));
    }

    [Fact]
    public async Task ThePriceAndTheStock_AreTheServersFigures_ByType_KK1()
    {
        Guid range = await AddProductAsync("Aralık", PublishStatus.Published, Base.AddMinutes(1), (30m, 4), (10m, 6));
        Guid single = await AddProductAsync("Tek fiyat", PublishStatus.Draft, Base.AddMinutes(2), (15m, 2), (15m, 2));
        Guid soldOut = await AddProductAsync("Tükenmiş", PublishStatus.Published, Base.AddMinutes(3), (20m, 2));
        await ReserveAllAsync(soldOut);
        Guid unlimited = await AddProductAsync("Danışmanlık", PublishStatus.Draft, Base.AddMinutes(4), ProductType.Service, (50m, null));
        Guid quota = await AddProductAsync("Atölye", PublishStatus.Draft, Base.AddMinutes(5), ProductType.Service, (40m, 8));
        Guid digital = await AddProductAsync("E-kitap", PublishStatus.Draft, Base.AddMinutes(6), ProductType.Digital, (25m, null));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, ListPath);

        AssertRow(html, range, "Fiziksel", "2", $"{DisplayFormat.Amount(10m)} – {DisplayFormat.Amount(30m)}", "10");
        AssertRow(html, single, "Fiziksel", "2", DisplayFormat.Amount(15m), "4");
        AssertRow(html, soldOut, "Fiziksel", "1", DisplayFormat.Amount(20m), ProductsModel.SoldOutText);
        AssertRow(html, unlimited, "Hizmet", "1", DisplayFormat.Amount(50m), ProductsModel.UnlimitedText);
        AssertRow(html, quota, "Hizmet", "1", DisplayFormat.Amount(40m), "8");
        AssertRow(html, digital, "Dijital", "1", DisplayFormat.Amount(25m), string.Empty);
    }

    [Fact]
    public async Task ARowAndNewProduct_OpenTheForm_WithTheListAddressInReturnUrl_AndTheMainImageIsShown_KK1()
    {
        Guid product = await AddProductAsync("Kupa", PublishStatus.Draft, Base.AddMinutes(1), (10m, 3));
        Guid imageId = await AddMainImageAsync(product, "Mavi kupa");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, ListPath + "?publishStatus=Draft");

        string back = Uri.EscapeDataString("/panel/urunler?publishStatus=Draft");
        Assert.Contains($"/panel/urunler/{product:D}?returnUrl={back}", html, StringComparison.Ordinal);
        Assert.Contains($"/panel/urunler/yeni?returnUrl={back}", html, StringComparison.Ordinal);
        Assert.Contains(ProductsModel.NewProductLabel, html, StringComparison.Ordinal);
        Assert.Contains($"src=\"{Shopfolio.Pages.Frame.FrameStyle.ImagePath(imageId)}\"", html, StringComparison.Ordinal);
        Assert.Contains("alt=\"Mavi kupa\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnEmptyCatalog_OpensTheFirstRecord_AndAnEmptyResult_RemovesTheSearchAndTheFilter_KK1()
    {
        await AddProductAsync("Kupa", PublishStatus.Draft, Base.AddMinutes(1), (10m, 3));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string noMatch) = await GetAsync(client, ListPath + "?search=yok&publishStatus=Archived");
        await ListSeed.ClearAsync(_database.Dsn);
        (_, string empty) = await GetAsync(client, ListPath);

        Assert.Contains(ProductsModel.NoMatchText, noMatch, StringComparison.Ordinal);
        Assert.Contains(ProductsModel.RemoveSearchLabel, noMatch, StringComparison.Ordinal);
        Assert.Contains(ProductsModel.NoProductsText, empty, StringComparison.Ordinal);
        Assert.Contains("href=\"/panel/urunler/yeni?returnUrl=", empty, StringComparison.Ordinal);
        Assert.Empty(RowStart().Matches(empty));
    }

    [Fact]
    public async Task ThePageBorder_WithTheSameCreationMoment_NeitherRepeatsNorLosesARow_KK1()
    {
        int count = PageSizes.Panel + 1;
        IReadOnlyList<Guid> ids = await ListSeed.SeedTiedAsync(_database.Dsn, [.. Enumerable.Range(1, count).Select(n => $"Bağ {n:000}")], Base, PublishStatus.Draft);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string first) = await GetAsync(client, ListPath);
        (_, string second) = await GetAsync(client, ListPath + "?page=2");

        List<Guid> firstPage = Links(first);
        List<Guid> secondPage = Links(second);
        Assert.Equal(PageSizes.Panel, firstPage.Count);
        Assert.Single(secondPage);
        Assert.Equal(ids.OrderBy(id => id).ToHashSet(), firstPage.Concat(secondPage).ToHashSet());
        Assert.Equal([.. ids.OrderByDescending(id => id)], firstPage.Concat(secondPage).ToList());
    }

    [Fact]
    public async Task TwoPages_SendTheSameNumberOfCommands_WhateverTheNumberOfVariantsOnThem_KK1()
    {
        for (int n = 0; n < PageSizes.Panel + 5; n++)
        {
            await AddProductAsync($"Ürün {n:000}", PublishStatus.Draft, Base.AddMinutes(n), (10m + n, 2), (20m + n, 3));
        }

        StorefrontSearchAndProductTests.CommandCounter counter = new();
        await using WebApplicationFactory<Program> factory = new ShopfolioWebApplicationFactory(_database.Dsn, withJobRunner: false)
            .WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly);
                s.ConfigureDbContext<AppDbContext>(options => options.AddInterceptors(counter));
            }));
        using HttpClient client = ClientOf(factory);
        await GetAsync(client, ListPath);

        int before = counter.Count;
        (_, string first) = await GetAsync(client, ListPath);
        int firstCommands = counter.Count - before;
        before = counter.Count;
        (_, string second) = await GetAsync(client, ListPath + "?page=2");
        int secondCommands = counter.Count - before;

        Assert.Equal(PageSizes.Panel, Links(first).Count);
        Assert.Equal(5, Links(second).Count);
        Assert.Equal(firstCommands, secondCommands);
    }

    [Fact]
    public async Task AGuest_IsNotShownTheList()
    {
        await AddProductAsync("Kupa", PublishStatus.Draft, Base.AddMinutes(1), (10m, 3));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient guest = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        using HttpResponseMessage response = await guest.GetAsync(new Uri(ListPath, UriKind.Relative), Ct);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    // ---- Helpers ----

    private static void AssertRow(string html, Guid productId, string type, string variants, string price, string stock)
    {
        int at = html.IndexOf($"/panel/urunler/{productId:D}?", StringComparison.Ordinal);
        Assert.True(at >= 0, $"The list has no row for {productId}.");
        int start = html.LastIndexOf("<tr", at, StringComparison.Ordinal);
        int end = html.IndexOf("</tr>", at, StringComparison.Ordinal);
        string row = html[start..end];
        Assert.Matches($"data-label=\"Tip\">\\s*{Regex.Escape(type)}\\s*</td>", row);
        Assert.Matches($"data-label=\"Varyant\">\\s*{Regex.Escape(variants)}\\s*</td>", row);
        Assert.Matches($"data-label=\"Fiyat\">\\s*{Regex.Escape(price)}\\s*</td>", row);
        Assert.Matches($"data-label=\"Stok\">\\s*{Regex.Escape(stock)}\\s*</td>", row);
    }

    private static List<Guid> Links(string html) =>
        [.. RecordLink().Matches(html).Select(m => Guid.Parse(m.Groups[1].Value)).Distinct()];

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={_adminKey}");
        return client;
    }

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private Task<Guid> AddProductAsync(string name, PublishStatus status, DateTimeOffset createdAt, params (decimal Price, int? Stock)[] variants) =>
        AddProductAsync(name, status, createdAt, ProductType.Physical, variants);

    private async Task<Guid> AddProductAsync(string name, PublishStatus status, DateTimeOffset createdAt, ProductType type, params (decimal Price, int? Stock)[] variants)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        Product product = Product.Create(name, $"p-{Guid.NewGuid():N}", type, 20m, createdAt).WithSeededOrigin();
        product.PublishStatus = status;
        db.Products.Add(product);
        for (int i = 0; i < variants.Length; i++)
        {
            Variant variant = Variant.Create(product.Id, $"S-{Guid.NewGuid():N}"[..20], variants[i].Price, createdAt.AddSeconds(i));
            variant.StockQuantity = variants[i].Stock;
            variant.PublishStatus = status == PublishStatus.Published ? PublishStatus.Published : PublishStatus.Draft;
            db.Variants.Add(variant);
        }

        await db.SaveChangesAsync(Ct);
        return product.Id;
    }

    private async Task ReserveAllAsync(Guid productId)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE variants SET reserved_quantity = stock_quantity WHERE product_id = {productId}", Ct);
    }

    private async Task<Guid> AddMainImageAsync(Guid productId, string altText)
    {
        await using AppDbContext db = ListSeed.Open(_database.Dsn);
        Image image = Image.Create($"img-{Guid.NewGuid():N}.jpg", ImageFormat.Jpeg, 800, 800, Base);
        db.Images.Add(image);
        ProductImage link = ProductImage.Create(productId, image.Id, 1, true, Base);
        link.AltText = altText;
        db.ProductImages.Add(link);
        await db.SaveChangesAsync(Ct);
        return image.Id;
    }
}
