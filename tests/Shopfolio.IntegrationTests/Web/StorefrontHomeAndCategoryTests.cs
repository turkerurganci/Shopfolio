using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the home page and category page tests (T502): one database copied from the template.</summary>
[CollectionDefinition(StorefrontHomeAndCategoryDefinition.Name)]
public sealed class StorefrontHomeAndCategoryDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "StorefrontHomeAndCategory";
}

/// <summary>
/// The home page and the category page on the opened application with a real PostgreSQL (T502 criteria 1-9; KK1-KK9; 07 UC-01, UC-02, UC-06; 04 5.1, 5.2): the four blocks in the order of the
/// firm's layout, the flagged records, the showcase of the newest products (eight or four), the category links that hold a published product, the closed-sales page that stays as it is with the
/// structured data in the first response, the category list with its branches, newest first, the page, the filters and the closed mark, the wrong price range, the empty category and the empty
/// filter result, and the 404 of a slug that is no category's.
/// </summary>
[Collection(StorefrontHomeAndCategoryDefinition.Name)]
public sealed class StorefrontHomeAndCategoryTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 8, 9, 0, 0, TimeSpan.Zero);

    private static readonly Regex _cardNames = new("""<span class="product-card__name">(.*?)</span>""", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private static readonly Regex _blocks = new("""<section class="home-block (home-intro|home-cards|home-showcase)[ "]""", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));

    private readonly CollectionDatabaseFixture _database;

    private int _counter;

    public StorefrontHomeAndCategoryTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync(
            "TRUNCATE cart_items, carts, order_items, orders, variants, products, categories, about_pages, offerings, portfolio_items, faq_items, custom_pages CASCADE", Ct);
        await db.Database.ExecuteSqlRawAsync(
            "UPDATE company_settings SET homepage_layout = 'ContentFirst', is_sales_temporarily_closed = false, menu_name_products = 'Ürünler', menu_name_offerings = 'Hizmetlerimiz', "
            + "menu_name_portfolio_items = 'Referanslarımız', menu_name_about_page = 'Hakkımızda'", Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private WebApplicationFactory<Program> Open() => new ShopfolioWebApplicationFactory(_database.Dsn);

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private static async Task<(HttpStatusCode Status, string Body)> GetAsync(HttpClient client, string address)
    {
        using HttpResponseMessage response = await client.GetAsync(address, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<string> GetOkAsync(HttpClient client, string address)
    {
        (HttpStatusCode status, string body) = await GetAsync(client, address);
        Assert.Equal(HttpStatusCode.OK, status);
        return body;
    }

    // The homepage view is cached by the module and a change made through the module drops it; a layout written straight to the table is read by a host opened after it.
    private async Task<string> HomeUnderAsync(string layout)
    {
        await ExecuteAsync($"UPDATE company_settings SET homepage_layout = '{layout}'");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();
        return await GetOkAsync(client, "/");
    }

    private static string[] CardNamesOf(string html) => [.. _cardNames.Matches(html).Select(m => m.Groups[1].Value.Trim())];

    // --- KK1: four blocks, two layouts ---

    [Fact]
    public async Task TheHomePage_ShowsTheFourBlocks_InTheOrderOfTheLayoutTheFirmChose_KK1()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        await SeedProductAsync("Masa", category, 100m, 5);
        await SeedAboutAsync("Yirmi yıllık atölyeyiz.");
        await SeedOfferingAsync("Montaj", 1);
        await SeedPortfolioAsync("Otel işi", 1);

        string contentFirst = await HomeUnderAsync("ContentFirst");
        string storeFirst = await HomeUnderAsync("StoreFirst");

        Assert.Equal(["home-intro", "home-cards", "home-cards", "home-showcase"], _blocks.Matches(contentFirst).Select(m => m.Groups[1].Value));
        Assert.Equal(["home-showcase", "home-intro", "home-cards", "home-cards"], _blocks.Matches(storeFirst).Select(m => m.Groups[1].Value));
        Assert.Contains("home-intro--wide", contentFirst, StringComparison.Ordinal);
        Assert.Contains("home-intro--band", storeFirst, StringComparison.Ordinal);
        Assert.Contains("Yirmi yıllık atölyeyiz.", contentFirst, StringComparison.Ordinal);
        Assert.Contains("home-showcase--rows-1", contentFirst, StringComparison.Ordinal);
        Assert.Contains("home-showcase--rows-2", storeFirst, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ASiteWithNothingYet_ShowsTheCorporateBlockWithTheBrandFallback_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetOkAsync(client, "/");

        Assert.Equal(["home-intro"], _blocks.Matches(html).Select(m => m.Groups[1].Value));
        Assert.Contains("home-intro__name", html, StringComparison.Ordinal);
        Assert.DoesNotContain("home-showcase", html, StringComparison.Ordinal);
    }

    // --- KK2: the flagged records, eight and four products ---

    [Fact]
    public async Task TheServiceAndReferenceBlocks_ShowTheFirstThreeFlaggedRecords_InTheManualOrder_KK2()
    {
        await SeedOfferingAsync("D", 5);
        await SeedOfferingAsync("A", 2);
        await SeedOfferingAsync("B", 3);
        await SeedOfferingAsync("C", 4);
        await SeedOfferingAsync("İşaretsiz", 1, flagged: false);
        await SeedPortfolioAsync("Y", 2);
        await SeedPortfolioAsync("X", 1);
        await SeedPortfolioAsync("Z", 3);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetOkAsync(client, "/");

        string[] titles = [.. Regex.Matches(html, """<span class="content-card__title">(.*?)</span>""", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Select(m => m.Groups[1].Value.Trim())];
        Assert.Equal(["A", "B", "C", "X", "Y", "Z"], titles);
        Assert.DoesNotContain("İşaretsiz", html, StringComparison.Ordinal);
        Assert.Equal(1, Regex.Count(html, "Tümünü gör", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Contains("""<a href="/hizmetler">Hizmetlerimiz</a>""", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheShowcase_IsTheNewestPublishedProducts_EightInStoreFirstAndFourInContentFirst_KK2()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        for (int i = 1; i <= 10; i++)
        {
            await SeedProductAsync($"Ürün {i:00}", category, 10m + i, 3);
        }

        await SeedProductAsync("Taslak ürün", category, 5m, 3, PublishStatus.Draft);
        await SeedProductAsync("Arşiv ürün", category, 5m, 3, PublishStatus.Archived);

        string contentFirst = await HomeUnderAsync("ContentFirst");
        string storeFirst = await HomeUnderAsync("StoreFirst");

        Assert.Equal(["Ürün 10", "Ürün 09", "Ürün 08", "Ürün 07"], CardNamesOf(contentFirst));
        Assert.Equal(["Ürün 10", "Ürün 09", "Ürün 08", "Ürün 07", "Ürün 06", "Ürün 05", "Ürün 04", "Ürün 03"], CardNamesOf(storeFirst));
    }

    // --- KK3: the category links ---

    [Fact]
    public async Task TheCategoryLinks_AreTheFirstLevelCategoriesWithAPublishedProductInThemOrBelow_NoOthers_KK3()
    {
        Guid furniture = await SeedCategoryAsync("Mobilya", null);
        Guid chairs = await SeedCategoryAsync("Sandalyeler", furniture);
        Guid empty = await SeedCategoryAsync("Boş kategori", null);
        Guid draftOnly = await SeedCategoryAsync("Yalnız taslak", null);
        await SeedProductAsync("Sandalye", chairs, 10m, 1);
        await SeedProductAsync("Taslak", draftOnly, 10m, 1, PublishStatus.Draft);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string page = await GetOkAsync(client, "/");
        string html = Regex.Match(page, """<ul class="home-showcase__categories".*?</ul>""", RegexOptions.Singleline | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)).Value;
        Assert.NotEmpty(html);

        Assert.Contains(">Mobilya</a>", html, StringComparison.Ordinal);
        Assert.Contains("/kategori/" + await SlugOfAsync(furniture), html, StringComparison.Ordinal);
        Assert.DoesNotContain("Boş kategori", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yalnız taslak", html, StringComparison.Ordinal);
        Assert.DoesNotContain("/kategori/" + await SlugOfAsync(empty), html, StringComparison.Ordinal);
        Assert.DoesNotContain("Sandalyeler", html, StringComparison.Ordinal);
    }

    // --- KK4: closed sales, structured data ---

    [Fact]
    public async Task WhileSalesAreClosed_TheShowcaseIsTheSame_AndTheStructuredDataIsInTheFirstResponse_KK4()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        await SeedProductAsync("Masa", category, 100m, 5);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string defaultState = await GetOkAsync(client, "/");
        await ExecuteAsync("UPDATE company_settings SET is_sales_temporarily_closed = true");
        string closed = await GetOkAsync(client, "/");

        Assert.Equal(["Masa"], CardNamesOf(closed));
        Assert.Equal(CardNamesOf(defaultState), CardNamesOf(closed));
        Assert.Equal(_blocks.Matches(defaultState).Select(m => m.Groups[1].Value), _blocks.Matches(closed).Select(m => m.Groups[1].Value));
        Assert.Contains("""<script type="application/ld+json">""", closed, StringComparison.Ordinal);
        Assert.Contains("\"@type\":\"Organization\"", closed.Replace(" ", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.True(closed.IndexOf("application/ld+json", StringComparison.Ordinal) < closed.IndexOf("<body", StringComparison.Ordinal), "The structured data is in the head.");
    }

    // --- KK5: the branch's products in one list, newest first ---

    [Fact]
    public async Task TheCategoryPage_ListsTheProductsOfTheCategoryAndOfItsBranches_NewestFirst_KK5()
    {
        Guid furniture = await SeedCategoryAsync("Mobilya", null);
        Guid chairs = await SeedCategoryAsync("Sandalyeler", furniture);
        Guid kitchen = await SeedCategoryAsync("Mutfak", null);
        await SeedProductAsync("Eski masa", furniture, 10m, 1);
        await SeedProductAsync("Sandalye", chairs, 10m, 1);
        await SeedProductAsync("Yeni dolap", furniture, 10m, 1);
        await SeedProductAsync("Tencere", kitchen, 10m, 1);
        await SeedProductAsync("Taslak masa", furniture, 10m, 1, PublishStatus.Draft);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string html = await GetOkAsync(client, "/kategori/" + await SlugOfAsync(furniture));
        string branch = await GetOkAsync(client, "/kategori/" + await SlugOfAsync(chairs));

        Assert.Equal(["Yeni dolap", "Sandalye", "Eski masa"], CardNamesOf(html));
        Assert.Equal(["Sandalye"], CardNamesOf(branch));
        Assert.Contains("""<h1 class="category-title">Mobilya</h1>""", html, StringComparison.Ordinal);
        Assert.Contains("breadcrumb__item", branch, StringComparison.Ordinal);
        Assert.Contains("Mobilya", branch, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"sort\"", html, StringComparison.Ordinal);
    }

    // --- KK6: the page, the filters, the closed mark ---

    [Fact]
    public async Task APageBeyondTheLast_ShowsTheLastPage_AndAnAddressWithAnyOtherParameterIsClosed_KK6()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        for (int i = 1; i <= 30; i++)
        {
            await SeedProductAsync($"Ürün {i:00}", category, i, i % 2 == 0 ? 5 : 0);
        }

        string path = "/kategori/" + await SlugOfAsync(category);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        string first = await GetOkAsync(client, path);
        string beyond = await GetOkAsync(client, path + "?page=99");
        string second = await GetOkAsync(client, path + "?page=2");
        string pageOnly = await GetOkAsync(client, path + "?page=2");
        string minPrice = await GetOkAsync(client, path + "?minPrice=26");
        string maxPrice = await GetOkAsync(client, path + "?maxPrice=3");
        string stock = await GetOkAsync(client, path + "?inStock=true");

        Assert.Equal(PageSizes.StorefrontProducts, CardNamesOf(first).Length);
        Assert.Equal(CardNamesOf(second), CardNamesOf(beyond));
        Assert.Equal(30 - PageSizes.StorefrontProducts, CardNamesOf(beyond).Length);
        Assert.DoesNotContain("noindex", pageOnly, StringComparison.Ordinal);
        Assert.DoesNotContain("noindex", first, StringComparison.Ordinal);
        Assert.Equal(5, CardNamesOf(minPrice).Length);
        Assert.Equal(3, CardNamesOf(maxPrice).Length);
        Assert.Equal(15, CardNamesOf(stock).Length);
        foreach (string closed in new[] { minPrice, maxPrice, stock, await GetOkAsync(client, path + "?foo=1") })
        {
            Assert.Contains("""<meta name="robots" content="noindex""", closed, StringComparison.Ordinal);
        }
    }

    // --- KK7: the inverted range ---

    [Fact]
    public async Task AnInvertedPriceRange_IsNotApplied_ShowsTheFieldMessage_AndAnswers200_KK7()
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        for (int i = 1; i <= 5; i++)
        {
            await SeedProductAsync($"Ürün {i}", category, i * 10m, 1);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode status, string html) = await GetAsync(client, "/kategori/" + await SlugOfAsync(category) + "?minPrice=40&maxPrice=20");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("En düşük fiyat üst değerden büyük olamaz.", html, StringComparison.Ordinal);
        Assert.Equal(5, CardNamesOf(html).Length);
    }

    // --- KK8: the empty category, the empty filter ---

    [Fact]
    public async Task ACategoryWithoutAPublishedProduct_IsAnEmptyPage_NotA404_AndAnEmptyFilterResultShowsTheWayOut_KK8()
    {
        Guid empty = await SeedCategoryAsync("Boş kategori", null);
        Guid full = await SeedCategoryAsync("Dolu", null);
        await SeedProductAsync("Masa", full, 10m, 1);
        await SeedProductAsync("Taslak", empty, 10m, 1, PublishStatus.Draft);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode emptyStatus, string emptyHtml) = await GetAsync(client, "/kategori/" + await SlugOfAsync(empty));
        (HttpStatusCode filterStatus, string filterHtml) = await GetAsync(client, "/kategori/" + await SlugOfAsync(full) + "?minPrice=1000");

        Assert.Equal(HttpStatusCode.OK, emptyStatus);
        Assert.Contains("Bu kategoride henüz ürün yok.", emptyHtml, StringComparison.Ordinal);
        Assert.Contains("Ana sayfaya dön", emptyHtml, StringComparison.Ordinal);
        Assert.Empty(CardNamesOf(emptyHtml));
        Assert.Equal(HttpStatusCode.OK, filterStatus);
        Assert.Contains("Seçtiğiniz süzgece uyan ürün yok.", filterHtml, StringComparison.Ordinal);
        Assert.Contains("Süzgeci kaldır", filterHtml, StringComparison.Ordinal);
        Assert.Contains("""href="/kategori/""" + await SlugOfAsync(full) + "\"", filterHtml, StringComparison.Ordinal);
    }

    // --- KK9: a slug that is no category's ---

    [Theory]
    [InlineData("/kategori/yok-boyle-bir-kategori")]
    [InlineData("/kategori/urun-0001")]
    public async Task ASlugThatIsNoCategorys_Is404_KK9(string address)
    {
        Guid category = await SeedCategoryAsync("Mobilya", null);
        await SeedProductAsync("Masa", category, 10m, 1);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode status, _) = await GetAsync(client, address);

        Assert.Equal(HttpStatusCode.NotFound, status);
    }

    // --- seeding ---

    private async Task<Guid> SeedCategoryAsync(string name, Guid? parent)
    {
        int level = 1;
        await using AppDbContext db = NewContext();
        if (parent is Guid parentId)
        {
            level = await db.Categories.Where(c => c.Id == parentId).Select(c => c.Level).SingleAsync(Ct) + 1;
        }

        int n = Interlocked.Increment(ref _counter);
        Category category = Category.Create(name, $"kategori-{n:D3}", parent, level, Now.AddMinutes(n));
        db.Categories.Add(category);
        await db.SaveChangesAsync(Ct);
        return category.Id;
    }

    private async Task<string> SlugOfAsync(Guid categoryId)
    {
        await using AppDbContext db = NewContext();
        return await db.Categories.Where(c => c.Id == categoryId).Select(c => c.Slug).SingleAsync(Ct);
    }

    private async Task SeedProductAsync(string name, Guid category, decimal price, int stock, PublishStatus status = PublishStatus.Published)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create(name, $"urun-{n:D4}", ProductType.Physical, 20m, Now.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = status;
        product.FulfilmentPeriod = 3;
        Variant variant = Variant.Create(product.Id, $"SKU-{n:D4}", price, Now.AddMinutes(n));
        variant.PublishStatus = PublishStatus.Published;
        variant.StockQuantity = stock;
        await using AppDbContext db = NewContext();
        db.Products.Add(product);
        db.ProductCategories.Add(ProductCategory.Create(product.Id, category, true, Now));
        db.Variants.Add(variant);
        await db.SaveChangesAsync(Ct);
    }

    private async Task SeedAboutAsync(string shortIntroduction)
    {
        AboutPage page = AboutPage.Create(Now);
        page.ShortIntroduction = shortIntroduction;
        page.PublishStatus = PublishStatus.Published;
        await using AppDbContext db = NewContext();
        db.AboutPages.Add(page);
        await db.SaveChangesAsync(Ct);
    }

    private async Task SeedOfferingAsync(string name, int order, bool flagged = true)
    {
        int n = Interlocked.Increment(ref _counter);
        Offering offering = Offering.Create(name, $"hizmet-{n:D3}", order, Now.AddMinutes(n));
        offering.PublishStatus = PublishStatus.Published;
        offering.IsShownOnHomepage = flagged;
        await using AppDbContext db = NewContext();
        db.Offerings.Add(offering);
        await db.SaveChangesAsync(Ct);
    }

    private async Task SeedPortfolioAsync(string title, int order)
    {
        int n = Interlocked.Increment(ref _counter);
        PortfolioItem item = PortfolioItem.Create(title, $"referans-{n:D3}", order, Now.AddMinutes(n));
        item.PublishStatus = PublishStatus.Published;
        item.IsShownOnHomepage = true;
        await using AppDbContext db = NewContext();
        db.PortfolioItems.Add(item);
        await db.SaveChangesAsync(Ct);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync(sql, Ct);
    }
}
