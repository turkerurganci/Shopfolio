using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the search engine data tests (T219): one database copied from the template.</summary>
[CollectionDefinition(SearchEngineDefinition.Name)]
public sealed class SearchEngineDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "SearchEngineData";
}

/// <summary>
/// The search engine data pattern on the opened application and a real PostgreSQL (T219 criteria 1 to 8; KK1, KK3 to KK8; 05 3.7; 09 9.4 item 3): the record page's first
/// HTML carries the title, description, Open Graph and the four JSON-LD blocks made from the database (the price and the stock status are read, not typed), a page
/// without a main image falls back to the logo, a closed page - the panel, a filtered list, the search page, a draft - carries the robots meta tag and nothing else for the
/// engines, an answer that is not HTML carries one <c>X-Robots-Tag: noindex</c>, no firm field for the engines exists (the site map and robots.txt are T313's, SitemapEndpointTests),
/// and two records made with the same name at the same time get <c>celik-kapi</c> and <c>celik-kapi-2</c>. The page behaviour is checked on a real Kestrel
/// server too, whose pipeline is not the test server's.
/// </summary>
[Collection(SearchEngineDefinition.Name)]
public sealed partial class SearchEngineDataTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 6, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;

    public SearchEngineDataTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex("""<meta (?:name|property)="([^"]+)" content="([^"]*)">""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex MetaTag();

    [GeneratedRegex("""<script type="application/ld\+json">(.*?)</script>""", RegexOptions.CultureInvariant | RegexOptions.Singleline, matchTimeoutMilliseconds: 5000)]
    private static partial Regex JsonLdScript();

    [GeneratedRegex("""<input[^>]*\bname="([^"]*)""", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex InputName();

    public async ValueTask InitializeAsync()
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new("UPDATE brandings SET brand_name = NULL, logo_id = NULL, favicon_id = NULL; DELETE FROM faq_items; DELETE FROM product_images; DELETE FROM variants; DELETE FROM products; DELETE FROM images;", connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    /// <summary>What the reverse proxy adds to every request it passes on: the scheme the visitor used (05 6.3.3).</summary>
    private sealed class ProxyHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class Host : IAsyncDisposable
    {
        private readonly ShopfolioWebApplicationFactory _root;

        public Host(string dsn, bool kestrel)
        {
            _root = new ShopfolioWebApplicationFactory(dsn);
            Factory = _root.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.AddSingleton<Shopfolio.IntegrationTests.Pages.Panel.PatternRecordStore>();
                s.AddSingleton<FormatRecordStore>();
                s.AddRazorPages().AddApplicationPart(typeof(SearchDataSampleModel).Assembly);
                if (kestrel)
                {
                    s.PostConfigure<ForwardedHeadersOptions>(o =>
                    {
                        o.KnownProxies.Add(IPAddress.Loopback);
                        o.KnownProxies.Add(IPAddress.IPv6Loopback);
                    });
                }
            }));

            AdminKey = TestSessions.AdminAsync(dsn).GetAwaiter().GetResult().Key;
            if (kestrel)
            {
                Factory.Start();
                using HttpClient probe = Factory.CreateClient();
                HttpClientHandler socket = new() { AllowAutoRedirect = false, UseCookies = false };
                Client = new HttpClient(new ForgeryTokenHandler { InnerHandler = new ProxyHandler { InnerHandler = socket } }) { BaseAddress = probe.BaseAddress };
            }
            else
            {
                Client = ForgeryTokenHandler.CreateClient(Factory);
            }
        }

        public WebApplicationFactory<Program> Factory { get; }

        public HttpClient Client { get; }

        public string AdminKey { get; }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Factory.DisposeAsync();
            await _root.DisposeAsync();
        }
    }

    private sealed record Answer(HttpStatusCode Status, string Body, HttpResponseMessage Message);

    private static async Task<Answer> GetAsync(Host host, string address, bool panel = false)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, new Uri(address, UriKind.Relative));
        if (panel)
        {
            request.Headers.TryAddWithoutValidation("Cookie", $"{CookieNames.AdminSession}={host.AdminKey}");
        }

        HttpResponseMessage response = await host.Client.SendAsync(request, Ct);
        return new Answer(response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), response);
    }

    private static Dictionary<string, string> Metas(string html) =>
        MetaTag().Matches(html).ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value, StringComparer.Ordinal);

    private static JsonElement[] Blocks(string html) =>
        [.. JsonLdScript().Matches(html).Select(m => JsonDocument.Parse(WebUtility.HtmlDecode(m.Groups[1].Value)).RootElement.Clone())];

    private static JsonElement Block(JsonElement[] blocks, string type) => blocks.Single(b => b.GetProperty("@type").GetString() == type);

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _database.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        return services.BuildServiceProvider();
    }

    private async Task<Guid> SeedBrandAsync(bool withLogo)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Branding branding = await db.Brandings.SingleAsync(Ct);
        branding.BumpVersion();
        branding.BrandName = "Yılmaz Mobilya";
        Guid logoId = Guid.Empty;
        if (withLogo)
        {
            Image logo = Image.Create("logo-key", ImageFormat.WebP, 400, 400, Now);
            db.Images.Add(logo);
            logoId = logo.Id;
            branding.LogoId = logo.Id;
        }

        FaqItem faq = FaqItem.Create("İade var mı?", 1, Now);
        faq.SetAnswer("<p>Evet, <strong>14 gün</strong> içinde.</p>");
        faq.PublishStatus = PublishStatus.Published;
        db.FaqItems.Add(faq);
        await db.SaveChangesAsync(Ct);
        return logoId;
    }

    private async Task<(string Slug, Guid ImageId)> SeedProductAsync(string name, decimal price, int? stock, bool withImage, PublishStatus status = PublishStatus.Published)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        string slug = Slug.From(name, AddressRules.ProductPrefix);
        Product product = Product.Create(name, slug, ProductType.Physical, 20m, Now).WithSeededOrigin();
        product.SetDescription("<p>Masif <strong>meşe</strong> masa, el yapımı.</p>");
        product.PublishStatus = status;
        db.Products.Add(product);
        Variant variant = Variant.Create(product.Id, "AHSAP-MASA-01", price, Now, 100);
        variant.StockQuantity = stock;
        db.Variants.Add(variant);
        Guid imageId = Guid.Empty;
        if (withImage)
        {
            Image image = Image.Create("masa-key", ImageFormat.WebP, 800, 600, Now);
            db.Images.Add(image);
            await db.SaveChangesAsync(Ct);
            product.BumpVersion();
            db.ProductImages.Add(ProductImage.Create(product.Id, image.Id, 1, true, Now));
            imageId = image.Id;
        }

        await db.SaveChangesAsync(Ct);
        return (slug, imageId);
    }

    // ---- KK3, KK4, KK5: the first HTML of a record page ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RecordPage_TheFirstHtmlHasTheTitleTheDescriptionTheOpenGraphPreviewAndTheFourBlocks_MadeFromTheDatabase(bool kestrel)
    {
        Guid logo = await SeedBrandAsync(withLogo: true);
        (string slug, Guid image) = await SeedProductAsync("Ahşap Masa", 2450.50m, 5, withImage: true);
        await using Host host = new(_database.Dsn, kestrel);

        Answer page = await GetAsync(host, $"/hizmet/{slug}");

        Assert.Equal(HttpStatusCode.OK, page.Status);
        Assert.Contains("<title>Ahşap Masa – Yılmaz Mobilya</title>", page.Body, StringComparison.Ordinal);
        Dictionary<string, string> metas = Metas(page.Body);
        Assert.Equal("Masif meşe masa, el yapımı.", metas["description"]);
        Assert.Equal("product", metas["og:type"]);
        Assert.Equal("Ahşap Masa – Yılmaz Mobilya", metas["og:title"]);
        Assert.Equal("Masif meşe masa, el yapımı.", metas["og:description"]);
        Assert.Equal($"https://localhost/hizmet/{slug}", metas["og:url"]);
        Assert.Equal($"https://localhost/gorsel/{image:N}", metas["og:image"]);
        Assert.DoesNotContain("robots", metas.Keys);

        JsonElement[] blocks = Blocks(page.Body);
        Assert.Equal(4, blocks.Length);
        Assert.All(blocks, b => Assert.Equal("https://schema.org", b.GetProperty("@context").GetString()));
        JsonElement product = Block(blocks, "Product");
        Assert.Equal("Ahşap Masa", product.GetProperty("name").GetString());
        Assert.Equal("AHSAP-MASA-01", product.GetProperty("sku").GetString());
        Assert.Equal(2450.50m, product.GetProperty("offers").GetProperty("price").GetDecimal());
        Assert.Equal("https://schema.org/InStock", product.GetProperty("offers").GetProperty("availability").GetString());
        Assert.Equal($"https://localhost/gorsel/{image:N}", product.GetProperty("image")[0].GetString());
        JsonElement firm = Block(blocks, "Organization");
        Assert.Equal("Yılmaz Mobilya", firm.GetProperty("name").GetString());
        Assert.Equal($"https://localhost/gorsel/{logo:N}", firm.GetProperty("logo").GetString());
        Assert.Equal(2, Block(blocks, "BreadcrumbList").GetProperty("itemListElement").GetArrayLength());
        JsonElement question = Block(blocks, "FAQPage").GetProperty("mainEntity")[0];
        Assert.Equal("İade var mı?", question.GetProperty("name").GetString());
        Assert.Equal("Evet, 14 gün içinde.", question.GetProperty("acceptedAnswer").GetProperty("text").GetString());
        Assert.False(page.Message.Headers.Contains("X-Robots-Tag"));
    }

    [Fact]
    public async Task RecordPage_ThePriceAndTheStockStatusFollowTheRecord_NotAnythingTyped()
    {
        await SeedBrandAsync(withLogo: false);
        (string slug, _) = await SeedProductAsync("Ahşap Masa", 100m, 1, withImage: false);
        await using Host host = new(_database.Dsn, kestrel: false);
        Assert.Equal("https://schema.org/InStock", Block(Blocks((await GetAsync(host, $"/hizmet/{slug}")).Body), "Product").GetProperty("offers").GetProperty("availability").GetString());

        await using (ServiceProvider provider = BuildProvider())
        await using (AsyncServiceScope scope = provider.CreateAsyncScope())
        {
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await db.Variants.ExecuteUpdateAsync(s => s.SetProperty(v => v.StockQuantity, 0).SetProperty(v => v.Price, 129.90m), Ct);
        }

        JsonElement offer = Block(Blocks((await GetAsync(host, $"/hizmet/{slug}")).Body), "Product").GetProperty("offers");

        Assert.Equal("https://schema.org/OutOfStock", offer.GetProperty("availability").GetString());
        Assert.Equal(129.90m, offer.GetProperty("price").GetDecimal());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task RecordPage_WithoutAMainImage_TheOpenGraphImageIsTheLogo(bool kestrel)
    {
        Guid logo = await SeedBrandAsync(withLogo: true);
        (string slug, _) = await SeedProductAsync("Ahşap Masa", 100m, 5, withImage: false);
        await using Host host = new(_database.Dsn, kestrel);

        Answer page = await GetAsync(host, $"/hizmet/{slug}");

        Assert.Equal($"https://localhost/gorsel/{logo:N}", Metas(page.Body)["og:image"]);
        Assert.False(Block(Blocks(page.Body), "Product").TryGetProperty("image", out _));
    }

    [Fact]
    public async Task RecordPage_WithoutAMainImageAndWithoutALogo_HasNoOpenGraphImage()
    {
        await SeedBrandAsync(withLogo: false);
        (string slug, _) = await SeedProductAsync("Ahşap Masa", 100m, 5, withImage: false);
        await using Host host = new(_database.Dsn, kestrel: false);

        Answer page = await GetAsync(host, $"/hizmet/{slug}");

        Assert.DoesNotContain("og:image", Metas(page.Body).Keys);
    }

    [Fact]
    public async Task RecordPage_OfADraftProduct_IsClosed_TitleAndRobotsOnly()
    {
        await SeedBrandAsync(withLogo: true);
        (string slug, _) = await SeedProductAsync("Taslak Masa", 100m, 5, withImage: true, PublishStatus.Draft);
        await using Host host = new(_database.Dsn, kestrel: false);

        Answer page = await GetAsync(host, $"/hizmet/{slug}");

        Assert.Equal(HttpStatusCode.OK, page.Status);
        Assert.Contains("<title>Taslak Masa – Yılmaz Mobilya</title>", page.Body, StringComparison.Ordinal);
        Assert.Equal(new Dictionary<string, string> { ["robots"] = "noindex" }, Metas(page.Body));
        Assert.Empty(Blocks(page.Body));
    }

    // ---- KK8: the closed pages ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClosedPages_CarryTheRobotsMetaTagInTheHtml_AndTheOpenOnesDoNot(bool kestrel)
    {
        await using Host host = new(_database.Dsn, kestrel);

        Answer panel = await GetAsync(host, "/panel/urun-listesi", panel: true);
        Answer filtered = await GetAsync(host, "/kategori/masalar?minPrice=1");
        Answer search = await GetAsync(host, "/sss/arama-listesi?search=masa");
        Answer category = await GetAsync(host, "/kategori/masalar");
        Answer secondPage = await GetAsync(host, "/kategori/masalar?page=2");

        Assert.All([panel, filtered, search, category, secondPage], a => Assert.Equal(HttpStatusCode.OK, a.Status));
        foreach (Answer closed in new[] { panel, filtered, search })
        {
            Assert.Equal("noindex", Metas(closed.Body)["robots"]);
            Assert.Single(Metas(closed.Body));
        }

        Assert.Contains("closed", filtered.Body, StringComparison.Ordinal);
        foreach (Answer open in new[] { category, secondPage })
        {
            Assert.DoesNotContain("robots", Metas(open.Body).Keys);
            Assert.Contains("og:title", Metas(open.Body).Keys);
            Assert.Contains("<p class=\"closed\">open</p>", open.Body, StringComparison.Ordinal);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ClosedAnswersThatAreNotHtml_CarryOneXRobotsTagNoindex_AndTheOpenOnesCarryNone(bool kestrel)
    {
        await using Host host = new(_database.Dsn, kestrel);

        // The panel's redirect to its sign-in (no session) and the CSV export (a session; the result sets the header itself, the middleware sets it again: one value).
        Answer redirect = await GetAsync(host, "/panel/urun-listesi");
        using HttpRequestMessage export = new(HttpMethod.Post, new Uri("/panel/ornek-form?handler=Export", UriKind.Relative));
        export.Headers.TryAddWithoutValidation("Cookie", $"{CookieNames.AdminSession}={host.AdminKey}");
        using HttpResponseMessage csv = await host.Client.SendAsync(export, Ct);
        Answer notFound = await GetAsync(host, "/panel/yok-boyle-bir-sayfa", panel: true);
        Answer home = await GetAsync(host, "/");

        Assert.True((int)redirect.Status is >= 300 and < 400, redirect.Status.ToString());
        Assert.Equal("noindex", Assert.Single(redirect.Message.Headers.GetValues("X-Robots-Tag")));
        Assert.Equal(HttpStatusCode.OK, csv.StatusCode);
        Assert.Equal("text/csv", csv.Content.Headers.ContentType?.MediaType);
        Assert.Equal("noindex", Assert.Single(csv.Headers.GetValues("X-Robots-Tag")));
        Assert.Equal(HttpStatusCode.NotFound, notFound.Status);
        Assert.Equal("noindex", Metas(notFound.Body)["robots"]);
        Assert.False(home.Message.Headers.Contains("X-Robots-Tag"));
        Assert.False((await GetAsync(host, "/kategori/masalar")).Message.Headers.Contains("X-Robots-Tag"));
    }

    // ---- KK6: no firm field ----

    [Fact]
    public async Task PanelPages_HaveNoSearchEngineFieldAndNoAddressField()
    {
        await using Host host = new(_database.Dsn, kestrel: false);
        string[] pages = ["/panel/ornek-form", "/panel/bicim-formu", $"/panel/urun-kaydi/{Guid.NewGuid()}", "/panel/urun-listesi"];

        foreach (string address in pages)
        {
            Answer page = await GetAsync(host, address, panel: true);

            Assert.Equal(HttpStatusCode.OK, page.Status);
            string[] names = [.. InputName().Matches(page.Body).Select(m => m.Groups[1].Value)];
            Assert.DoesNotContain(names, n => Regex.IsMatch(n, "(?i)(meta|seo|canonical|og[_-]|slug|robots)", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        }
    }

    // ---- KK1: the address, the conflict, the retry (09 9.4 item 3) ----

    private sealed class SavedProduct(bool isRejected, string slug) : IOperationResult
    {
        public bool IsRejected { get; } = isRejected;

        public string Slug { get; } = slug;
    }

    [Fact]
    public async Task TwoRecordsMadeWithTheSameNameAtTheSameTime_GetCelikKapiAndCelikKapi2()
    {
        await using ServiceProvider provider = BuildProvider();
        using Barrier bothHaveRead = new(2);
        int attempts = 0;

        async Task<string> SaveAsync()
        {
            await using AsyncServiceScope scope = provider.CreateAsyncScope();
            AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            int own = 0;
            SavedProduct saved = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
                async ct =>
                {
                    own++;
                    Interlocked.Increment(ref attempts);
                    string slug = await Slug.ForSaveAsync(
                        "Çelik Kapı",
                        AddressRules.ProductPrefix,
                        string.Empty,
                        null,
                        async (baseSlug, token) => await db.Products.Where(p => p.Slug.StartsWith(baseSlug)).Select(p => p.Slug).ToListAsync(token),
                        ct);

                    // Both first attempts have read the table before either writes: the two made the same address.
                    if (own == 1 && !bothHaveRead.SignalAndWait(TimeSpan.FromSeconds(30), ct))
                    {
                        throw new TimeoutException("The second record never read the table.");
                    }

                    db.Products.Add(Product.Create("Çelik Kapı", slug, ProductType.Physical, 20m, Now).WithSeededOrigin());
                    return new SavedProduct(false, slug);
                },
                ["ix_products_slug"],
                Ct);
            return saved.Slug;
        }

        string[] slugs = await Task.WhenAll(Task.Run(SaveAsync, Ct), Task.Run(SaveAsync, Ct));

        Assert.Equal(["celik-kapi", "celik-kapi-2"], slugs.Order(StringComparer.Ordinal));
        Assert.Equal(3, attempts);
        Assert.Equal(["celik-kapi", "celik-kapi-2"], await ReadSlugsAsync());
    }

    [Fact]
    public async Task RunAsyncWithRetry_ApersistentConflictOnTheNamedIndex_IsTriedThreeTimesAndThenThrown()
    {
        await SeedProductAsync("Sabit Masa", 100m, 1, withImage: false);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        int attempts = 0;

        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(async () => await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            _ =>
            {
                attempts++;
                db.Products.Add(Product.Create("Sabit Masa", "sabit-masa", ProductType.Physical, 20m, Now).WithSeededOrigin());
                return Task.FromResult(new SavedProduct(false, "sabit-masa"));
            },
            ["ix_products_slug"],
            Ct));

        Assert.Equal(TransactionRunner.UniqueConflictAttempts, attempts);
        Assert.Equal("ix_products_slug", Assert.IsType<PostgresException>(error.InnerException).ConstraintName);
        Assert.Null(db.Database.CurrentTransaction);
        Assert.Empty(db.ChangeTracker.Entries());
        Assert.Equal(["sabit-masa"], await ReadSlugsAsync());
    }

    [Fact]
    public async Task RunAsyncWithRetry_AUniqueViolationOfAnotherIndex_IsNotRetried()
    {
        await SeedProductAsync("Birinci Masa", 100m, 1, withImage: false);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Guid productId = await db.Products.Select(p => p.Id).SingleAsync(Ct);
        int attempts = 0;

        // The stock code of the seeded variant is taken: the violation is the stock code's unique index, not the address's.
        DbUpdateException error = await Assert.ThrowsAsync<DbUpdateException>(async () => await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            _ =>
            {
                attempts++;
                db.Variants.Add(Variant.Create(productId, "ahsap-masa-01", 5m, Now, 100));
                return Task.FromResult(new SavedProduct(false, string.Empty));
            },
            ["ix_products_slug"],
            Ct));

        Assert.Equal(1, attempts);
        Assert.NotEqual("ix_products_slug", Assert.IsType<PostgresException>(error.InnerException).ConstraintName);
        Assert.Equal("23505", ((PostgresException)error.InnerException).SqlState);
    }

    [Fact]
    public async Task RunAsyncWithRetry_AWorkThatSucceeds_RunsOnce_AndARefusedResultIsReturnedWithoutARetry()
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        int attempts = 0;

        SavedProduct refused = await runner.RunAsync(
            _ =>
            {
                attempts++;
                return Task.FromResult(new SavedProduct(true, string.Empty));
            },
            ["ix_products_slug"],
            Ct);

        Assert.True(refused.IsRejected);
        Assert.Equal(1, attempts);
    }

    [Fact]
    public async Task RunAsyncWithRetry_WithoutAnIndexName_IsRefusedAtOnce()
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();

        await Assert.ThrowsAsync<ArgumentException>(async () => await runner.RunAsync(_ => Task.FromResult(new SavedProduct(false, string.Empty)), [], Ct));
    }

    [Fact]
    public async Task TheAddressOfAFixedRecord_DoesNotChangeWhenTheNameDoes()
    {
        (string slug, _) = await SeedProductAsync("Ahşap Masa", 100m, 1, withImage: false);
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Product product = await db.Products.SingleAsync(p => p.Slug == slug, Ct);
        product.SlugFixedAt = Now;
        product.SetName("Tamamen Başka Bir Ad");
        product.BumpVersion();

        product.Slug = await Slug.ForSaveAsync(product.Name, AddressRules.ProductPrefix, product.Slug, product.SlugFixedAt, (_, _) => throw new InvalidOperationException("Not read."), Ct);
        await db.SaveChangesAsync(Ct);

        Assert.Equal("ahsap-masa", product.Slug);
        Assert.Equal(["ahsap-masa"], await ReadSlugsAsync());
    }

    private async Task<string[]> ReadSlugsAsync()
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return [.. (await db.Products.AsNoTracking().Select(p => p.Slug).ToListAsync(Ct)).Order(StringComparer.Ordinal)];
    }
}
