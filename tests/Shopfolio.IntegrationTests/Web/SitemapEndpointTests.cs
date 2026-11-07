using System.Net;
using System.Xml.Linq;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the site map tests (T313): one database copied from the template.</summary>
[CollectionDefinition(SitemapEndpointDefinition.Name)]
public sealed class SitemapEndpointDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "SitemapEndpoint";
}

/// <summary>
/// UC-79 on the opened application and a real PostgreSQL (T313 criteria 1 to 5, 8 and 9; KK1 to KK5, KK8, KK9; 07 4.32.1; 05 3.7.3, 3.7.4, 7.10.1): <c>/sitemap.xml</c>
/// and <c>/robots.txt</c> answer 200; the map holds the published products, the categories, the content pages and the fixed list pages and neither an archived
/// product, a draft nor a closed page; every address is lower case and carries the installation's domain, not the request's host; robots.txt is one line that names the
/// map and blocks nothing - the archived product's address included; the map comes from the cache, and a saved product (in a transaction that names the area) makes
/// the next answer new while a rolled back save leaves it. The answers are checked on a real Kestrel server too.
/// </summary>
[Collection(SitemapEndpointDefinition.Name)]
public sealed class SitemapEndpointTests : IAsyncLifetime
{
    private const string InstallationDomain = "magaza.example.com";

    private static readonly DateTimeOffset Now = new(2026, 11, 7, 9, 0, 0, TimeSpan.Zero);

    private static readonly XNamespace Ns = "http://www.sitemaps.org/schemas/sitemap/0.9";

    private static readonly byte[] ByteOrderMark = [0xEF, 0xBB, 0xBF];

    private static readonly string[] FixedLists = ["/", "/hizmetler", "/referanslar", "/sss", "/iletisim"];

    private static readonly string[] ProtocolPaths = ["/sitemap.xml", "/robots.txt"];

    private readonly CollectionDatabaseFixture _database;

    public SitemapEndpointTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(
            "TRUNCATE products, categories, offerings, portfolio_items, custom_pages, about_pages CASCADE", connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private sealed class Host : IAsyncDisposable
    {
        public Host(string dsn, bool kestrel)
        {
            Factory = new ShopfolioWebApplicationFactory(dsn, new Dictionary<string, string> { [SiteOptions.DomainKey] = InstallationDomain });
            if (kestrel)
            {
                Factory.UseKestrel(0);
                Factory.StartServer();
                using HttpClient probe = Factory.CreateClient();
                Client = new HttpClient(new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false }) { BaseAddress = probe.BaseAddress };
            }
            else
            {
                Client = Factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
            }
        }

        public ShopfolioWebApplicationFactory Factory { get; }

        public HttpClient Client { get; }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Factory.DisposeAsync();
        }
    }

    private static async Task<IReadOnlyList<string>> LocsAsync(Host host)
    {
        using HttpResponseMessage response = await host.Client.GetAsync(new Uri("/sitemap.xml", UriKind.Relative), Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return [.. XDocument.Parse(await response.Content.ReadAsStringAsync(Ct)).Descendants(Ns + "loc").Select(l => l.Value)];
    }

    private static string Url(string path) => $"https://{InstallationDomain}{path}";

    private static async Task SeedAsync(AppDbContext db)
    {
        // Products: one of each state. A product's address is the one the record page answers; an archived one is read by its direct address only.
        foreach ((string slug, PublishStatus status) in new[] { ("masa", PublishStatus.Published), ("taslak-masa", PublishStatus.Draft), ("arsiv-masa", PublishStatus.Archived) })
        {
            Product product = Product.Create(slug, slug, ProductType.Physical, 20m, Now).WithSeededOrigin();
            product.PublishStatus = status;
            db.Products.Add(product);
        }

        db.Categories.Add(Category.Create("Mobilya", "mobilya", null, 1, Now));

        Offering offering = Offering.Create("Montaj", "montaj", 1, Now);
        offering.PublishStatus = PublishStatus.Published;
        db.Offerings.Add(offering);
        db.Offerings.Add(Offering.Create("Taslak Hizmet", "taslak-hizmet", 2, Now));

        PortfolioItem work = PortfolioItem.Create("Ofis", "ofis", 1, Now);
        work.PublishStatus = PublishStatus.Published;
        db.PortfolioItems.Add(work);
        db.PortfolioItems.Add(PortfolioItem.Create("Taslak Referans", "taslak-referans", 2, Now));

        CustomPage page = CustomPage.Create("Teslimat", "teslimat", 1, Now);
        page.PublishStatus = PublishStatus.Published;
        db.CustomPages.Add(page);
        db.CustomPages.Add(CustomPage.Create("Taslak Sayfa", "taslak-sayfa", 2, Now));

        AboutPage about = AboutPage.Create(Now);
        about.PublishStatus = PublishStatus.Published;
        db.AboutPages.Add(about);

        await db.SaveChangesAsync(Ct);
    }

    private static async Task SeedProductAsync(AppDbContext db, string slug)
    {
        Product product = Product.Create(slug, slug, ProductType.Physical, 20m, Now).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        db.Products.Add(product);
        await db.SaveChangesAsync(Ct);
    }

    private sealed class Outcome : IOperationResult
    {
        private Outcome(bool isRejected)
        {
            IsRejected = isRejected;
        }

        public static Outcome Done { get; } = new(false);

        public static Outcome Refused { get; } = new(true);

        public bool IsRejected { get; }
    }

    // ---- KK1: the two addresses answer 200 ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TheTwoAddresses_Answer200_WithTheirTypes_NoByteOrderMark_AndAreNotClosedToTheSearchEngines(bool kestrel)
    {
        await using Host host = new(_database.Dsn, kestrel);

        using HttpResponseMessage sitemap = await host.Client.GetAsync(new Uri("/sitemap.xml", UriKind.Relative), Ct);
        using HttpResponseMessage robots = await host.Client.GetAsync(new Uri("/robots.txt", UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.OK, sitemap.StatusCode);
        Assert.Equal(HttpStatusCode.OK, robots.StatusCode);
        Assert.Equal("application/xml", sitemap.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", sitemap.Content.Headers.ContentType?.CharSet);
        Assert.Equal("text/plain", robots.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", robots.Content.Headers.ContentType?.CharSet);
        Assert.False((await sitemap.Content.ReadAsByteArrayAsync(Ct)).AsSpan().StartsWith(ByteOrderMark));
        Assert.False((await robots.Content.ReadAsByteArrayAsync(Ct)).AsSpan().StartsWith(ByteOrderMark));

        // What the search engines read is open: neither answer carries the closing header.
        Assert.False(sitemap.Headers.Contains("X-Robots-Tag"));
        Assert.False(robots.Headers.Contains("X-Robots-Tag"));
    }

    [Fact]
    public async Task TheTwoAddresses_AreGetsOnly()
    {
        await using Host host = new(_database.Dsn, kestrel: false);

        foreach (string path in ProtocolPaths)
        {
            using HttpResponseMessage post = await host.Client.PostAsync(new Uri(path, UriKind.Relative), new StringContent(string.Empty), Ct);
            Assert.True(post.StatusCode is HttpStatusCode.MethodNotAllowed or HttpStatusCode.BadRequest, $"{path} {post.StatusCode}");
        }
    }

    // ---- KK2, KK3, KK4: the four lists, nothing closed, one lower-case form ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TheMap_HasTheFourLists_AndNeitherTheArchivedNorTheDraftNorAClosedPage(bool kestrel)
    {
        await using (ServiceProvider seedProvider = BuildProvider())
        await using (AsyncServiceScope seedScope = seedProvider.CreateAsyncScope())
        {
            await SeedAsync(seedScope.ServiceProvider.GetRequiredService<AppDbContext>());
        }

        await using Host host = new(_database.Dsn, kestrel);
        IReadOnlyList<string> locs = await LocsAsync(host);

        // The product list, the category list, the content pages and the fixed list pages.
        Assert.Contains(Url("/urun/masa"), locs);
        Assert.Contains(Url("/kategori/mobilya"), locs);
        Assert.Contains(Url("/hizmet/montaj"), locs);
        Assert.Contains(Url("/referans/ofis"), locs);
        Assert.Contains(Url("/sayfa/teslimat"), locs);
        Assert.Contains(Url("/hakkimizda"), locs);
        Assert.All(FixedLists, path => Assert.Contains(Url(path), locs));

        // Not the archived product, not a draft of any kind.
        Assert.DoesNotContain(Url("/urun/arsiv-masa"), locs);
        Assert.DoesNotContain(Url("/urun/taslak-masa"), locs);
        Assert.DoesNotContain(Url("/hizmet/taslak-hizmet"), locs);
        Assert.DoesNotContain(Url("/referans/taslak-referans"), locs);
        Assert.DoesNotContain(Url("/sayfa/taslak-sayfa"), locs);

        // Not a page the search engines may not index: the cart, the payment step, the tracking, the search, the account, the panel, the downloads, the orders.
        string[] closed = ["/sepet", "/odeme", "/siparis-takibi", "/arama", "/giris", "/kayit", "/hesap", "/panel", "/indir", "/siparis"];
        Assert.All(locs, loc => Assert.DoesNotContain(closed, c => loc == Url(c) || loc.StartsWith(Url(c) + "/", StringComparison.Ordinal)));

        // Each address once.
        Assert.Equal(locs.Count, locs.Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public async Task EveryAddress_IsLowerCase_AndCarriesTheInstallationDomainAndNotTheRequestHost()
    {
        await using (ServiceProvider seedProvider = BuildProvider())
        await using (AsyncServiceScope seedScope = seedProvider.CreateAsyncScope())
        {
            await SeedAsync(seedScope.ServiceProvider.GetRequiredService<AppDbContext>());
        }

        await using Host host = new(_database.Dsn, kestrel: false);
        using HttpRequestMessage request = new(HttpMethod.Get, new Uri("/sitemap.xml", UriKind.Relative));
        request.Headers.Host = "ATTACKER.example.net";
        using HttpResponseMessage response = await host.Client.SendAsync(request, Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);
        string[] locs = [.. XDocument.Parse(body).Descendants(Ns + "loc").Select(l => l.Value)];

        Assert.NotEmpty(locs);
        Assert.All(locs, loc => Assert.StartsWith($"https://{InstallationDomain}/", loc, StringComparison.Ordinal));
        Assert.All(locs, loc => Assert.Equal(loc.ToLowerInvariant(), loc));
        Assert.DoesNotContain("attacker", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task AnEmptyShop_StillAnswersTheMapWithTheFixedListPages()
    {
        await using Host host = new(_database.Dsn, kestrel: false);

        IReadOnlyList<string> locs = await LocsAsync(host);

        Assert.Equal(Shopfolio.Infrastructure.SearchEngine.SiteMapFiles.FixedListPaths.Select(Url), locs);
    }

    // ---- KK5, KK8: robots.txt ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Robots_IsOneLineWithTheMapAddress_BlocksNothing_AndNotTheArchivedProduct(bool kestrel)
    {
        await using (ServiceProvider seedProvider = BuildProvider())
        await using (AsyncServiceScope seedScope = seedProvider.CreateAsyncScope())
        {
            await SeedAsync(seedScope.ServiceProvider.GetRequiredService<AppDbContext>());
        }

        await using Host host = new(_database.Dsn, kestrel);

        string robots = await host.Client.GetStringAsync(new Uri("/robots.txt", UriKind.Relative), Ct);

        Assert.Equal($"Sitemap: https://{InstallationDomain}/sitemap.xml\n", robots);
        Assert.DoesNotContain("Disallow", robots, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("arsiv-masa", robots, StringComparison.Ordinal);
        Assert.DoesNotContain("/panel", robots, StringComparison.Ordinal);
        Assert.DoesNotContain("/hesap", robots, StringComparison.Ordinal);
        Assert.Single(robots.Split('\n', StringSplitOptions.RemoveEmptyEntries));
    }

    // ---- KK9: the cache ----

    [Fact]
    public async Task TheMap_ComesFromTheCache_ASavedProductThatNamesTheAreaDropsIt_AndARolledBackSaveDoesNot()
    {
        await using Host host = new(_database.Dsn, kestrel: false);
        AppCache cache = host.Factory.Services.GetRequiredService<AppCache>();
        Assert.DoesNotContain(Url("/urun/ilk"), await LocsAsync(host));
        long generation = cache.GenerationOf(CacheArea.Sitemap);

        // A row written outside a transaction that names the area: the cached map is still the one that was read.
        await using (AsyncServiceScope direct = host.Factory.Services.CreateAsyncScope())
        {
            await SeedProductAsync(direct.ServiceProvider.GetRequiredService<AppDbContext>(), "ilk");
        }

        Assert.DoesNotContain(Url("/urun/ilk"), await LocsAsync(host));
        Assert.Equal(generation, cache.GenerationOf(CacheArea.Sitemap));

        // A save that is rolled back leaves the cache as it was.
        await using (AsyncServiceScope refused = host.Factory.Services.CreateAsyncScope())
        {
            AppDbContext db = refused.ServiceProvider.GetRequiredService<AppDbContext>();
            CacheInvalidations invalidations = refused.ServiceProvider.GetRequiredService<CacheInvalidations>();
            Outcome outcome = await refused.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
                async token =>
                {
                    Product product = Product.Create("Geri Alinan", "geri-alinan", ProductType.Physical, 20m, Now).WithSeededOrigin();
                    product.PublishStatus = PublishStatus.Published;
                    db.Products.Add(product);
                    await db.SaveChangesAsync(token);
                    invalidations.Add(CacheArea.Sitemap);
                    return Outcome.Refused;
                },
                Ct);
            Assert.True(outcome.IsRejected);
        }

        Assert.Equal(generation, cache.GenerationOf(CacheArea.Sitemap));
        Assert.DoesNotContain(Url("/urun/geri-alinan"), await LocsAsync(host));

        // A save that commits and names the area: the next answer is new.
        await using (AsyncServiceScope saved = host.Factory.Services.CreateAsyncScope())
        {
            AppDbContext db = saved.ServiceProvider.GetRequiredService<AppDbContext>();
            CacheInvalidations invalidations = saved.ServiceProvider.GetRequiredService<CacheInvalidations>();
            Outcome outcome = await saved.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
                async token =>
                {
                    Product product = Product.Create("Yeni Masa", "yeni-masa", ProductType.Physical, 20m, Now).WithSeededOrigin();
                    product.PublishStatus = PublishStatus.Published;
                    db.Products.Add(product);
                    await db.SaveChangesAsync(token);
                    invalidations.Add(CacheArea.Sitemap);
                    return Outcome.Done;
                },
                Ct);
            Assert.False(outcome.IsRejected);
        }

        IReadOnlyList<string> after = await LocsAsync(host);
        Assert.Equal(generation + 1, cache.GenerationOf(CacheArea.Sitemap));
        Assert.Contains(Url("/urun/yeni-masa"), after);
        Assert.Contains(Url("/urun/ilk"), after);
        Assert.DoesNotContain(Url("/urun/geri-alinan"), after);
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _database.Dsn));
        return services.BuildServiceProvider();
    }
}
