using System.Net;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the search and product page tests (T503): one database copied from the template.</summary>
[CollectionDefinition(StorefrontSearchAndProductTestsDefinition.Name)]
public sealed class StorefrontSearchAndProductTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "StorefrontSearchAndProductTests";
}

/// <summary>
/// The search page and the product page with the add-to-cart form on the opened application (T503 KK1-KK9; 07 UC-03..UC-05; 04 5.3, 5.4): the rows are real rows of the collection database, the
/// requests are real requests. The sales gate is the one thing the test sets: the firm's real gate depends on settings this suite does not make, so the settings module answers the gate from the test
/// and everything else from the real module.
/// </summary>
[Collection(StorefrontSearchAndProductTestsDefinition.Name)]
public sealed partial class StorefrontSearchAndProductTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    private readonly CollectionDatabaseFixture _database;
    private readonly GateHolder _gate = new();

    private int _counter;

    public StorefrontSearchAndProductTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE cart_items, carts, variants, products, legal_text_versions CASCADE", Ct);

        // Until a cookie policy is published the storefront writes no cookie; the cart's key is a cookie, so the policy is published here.
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO legal_text_versions (id, created_at, legal_text_type, version_number, published_at, text) VALUES (gen_random_uuid(), now(), 'CookiePolicy', 1, now(), 'policy')", Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(FrameSampleModel).Assembly);
            s.AddSingleton(_gate);
            ServiceDescriptor original = s.Single(d => d.ServiceType == typeof(ISettingsModule));
            s.Remove(original);
            s.AddScoped<ISettingsModule>(provider =>
            {
                ISettingsModule real = (ISettingsModule)ActivatorUtilities.CreateInstance(provider, original.ImplementationType!);
                GateForwarder proxy = (GateForwarder)(object)DispatchProxy.Create<ISettingsModule, GateForwarder>();
                proxy.Target = real;
                proxy.Holder = provider.GetRequiredService<GateHolder>();
                return (ISettingsModule)(object)proxy;
            });
        }));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    // The markup without the response's own nonce and the sign-in link's return address: two 404 answers are the same page when nothing else differs (the frame writes a fresh nonce per response).
    private static string WithoutNonce(string html) =>
        Regex.Replace(Regex.Replace(html, "nonce=\"[^\"]*\"", "nonce=\"-\"", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2)), "returnUrl=[^\"]*", "returnUrl=-", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));

    private sealed record Reply(HttpStatusCode Status, string Body, HttpResponseMessage Message)
    {
        public string Text => WebUtility.HtmlDecode(Body);

        public string? Location => Message.Headers.Location?.OriginalString;
    }

    private static async Task<Reply> SendAsync(HttpClient client, HttpMethod method, string path, string? cookie = null, params (string Name, string Value)[] fields)
    {
        using HttpRequestMessage request = new(method, path);
        if (cookie is not null)
        {
            request.Headers.Add("Cookie", cookie);
        }

        if (method == HttpMethod.Post)
        {
            request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        }

        HttpResponseMessage response = await client.SendAsync(request, Ct);
        return new Reply(response.StatusCode, await response.Content.ReadAsStringAsync(Ct), response);
    }

    private static Task<Reply> GetAsync(HttpClient client, string path, string? cookie = null) => SendAsync(client, HttpMethod.Get, path, cookie);

    private static Task<Reply> PostAsync(HttpClient client, string path, string? cookie, params (string Name, string Value)[] fields) =>
        SendAsync(client, HttpMethod.Post, path, cookie, fields);


    private static bool IsClosed(Reply reply) => reply.Body.Contains("<meta name=\"robots\" content=\"noindex\">", StringComparison.Ordinal);

    private static string GuestCartCookieOf(Reply reply)
    {
        string? set = reply.Message.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values)
            ? values.FirstOrDefault(v => v.StartsWith(CookieNames.Cart + "=", StringComparison.Ordinal))
            : null;
        Assert.True(set is not null, string.Join(" | ", reply.Message.Headers.Select(h => h.Key + "=" + string.Join(",", h.Value))));
        return set.Split(';', 2)[0];
    }

    private static string AddPath(string slug) => $"/urun/{slug}?handler=AddCartItem";

    private sealed record SeededProduct(Guid ProductId, string Slug, IReadOnlyList<Guid> VariantIds);

    private sealed record VariantSpec(decimal Price = 100m, int? Stock = 10, int Reserved = 0, PublishStatus Status = PublishStatus.Published, int? First = null, int? Second = null);

    /// <summary>A product with its variants; with a first (and second) place the variants get one value each in an option of their own, the place being the value's order.</summary>
    private async Task<SeededProduct> SeedAsync(
        string name,
        PublishStatus status = PublishStatus.Published,
        ProductType type = ProductType.Physical,
        int? maxPerOrder = null,
        params VariantSpec[] variants)
    {
        int n = Interlocked.Increment(ref _counter);
        string slug = $"urun-{n:D4}";
        Product product = Product.Create(name, slug, type, 20m, Now.AddMinutes(n)).WithSeededOrigin();
        product.PublishStatus = status;
        product.MaxQuantityPerOrder = maxPerOrder;
        VariantSpec[] specs = variants.Length == 0 ? [new VariantSpec()] : variants;
        Option? first = specs.Any(v => v.First is not null) ? Option.Create(product.Id, "Renk", 1, Now) : null;
        Option? second = specs.Any(v => v.Second is not null) ? Option.Create(product.Id, "Beden", 2, Now) : null;
        Dictionary<int, OptionValue> firstValues = [];
        Dictionary<int, OptionValue> secondValues = [];
        List<Variant> rows = [];
        foreach (VariantSpec spec in specs)
        {
            Variant variant = Variant.Create(product.Id, $"SKU-{n:D4}-{rows.Count}", spec.Price, Now.AddMinutes(n));
            variant.PublishStatus = spec.Status;
            variant.StockQuantity = type == ProductType.Digital ? null : spec.Stock;
            if (first is not null && spec.First is int firstOrder)
            {
                if (!firstValues.TryGetValue(firstOrder, out OptionValue? value))
                {
                    firstValues[firstOrder] = value = OptionValue.Create(first.Id, $"Renk {firstOrder}", firstOrder, Now);
                }

                variant.FirstOptionValueId = value.Id;
            }

            if (second is not null && spec.Second is int secondOrder)
            {
                if (!secondValues.TryGetValue(secondOrder, out OptionValue? value))
                {
                    secondValues[secondOrder] = value = OptionValue.Create(second.Id, $"Beden {secondOrder}", secondOrder, Now);
                }

                variant.SecondOptionValueId = value.Id;
            }

            rows.Add(variant);
        }

        await using AppDbContext db = NewContext();
        db.Products.Add(product);
        if (first is not null)
        {
            db.Options.Add(first);
            db.OptionValues.AddRange(firstValues.Values);
        }

        if (second is not null)
        {
            db.Options.Add(second);
            db.OptionValues.AddRange(secondValues.Values);
        }

        db.Variants.AddRange(rows);
        await db.SaveChangesAsync(Ct);
        foreach ((Variant row, VariantSpec spec) in rows.Zip(specs))
        {
            if (spec.Reserved > 0)
            {
                await db.Database.ExecuteSqlRawAsync("UPDATE variants SET reserved_quantity = {0} WHERE id = {1}", [spec.Reserved, row.Id], Ct);
            }
        }

        return new SeededProduct(product.Id, slug, [.. rows.Select(v => v.Id)]);
    }

    private async Task<int> CartQuantityOfAsync(Guid variantId)
    {
        await using AppDbContext db = NewContext();
        return await db.CartItems.Where(i => i.VariantId == variantId).SumAsync(i => i.Quantity, Ct);
    }

    private async Task<int> CartCountAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Carts.CountAsync(Ct);
    }

    // ---- KK1: the search is blind to case and Turkish characters; % and _ are letters; the word is cut at 150 ----

    [Fact]
    public async Task TheSearchPage_IsBlindToCaseAndTurkishCharacters_AndTakesPercentAndUnderscoreAsLetters_T503_KK1()
    {
        SeededProduct lamp = await SeedAsync("Işık Lambası");
        SeededProduct percent = await SeedAsync("100% Pamuk");
        SeededProduct underscore = await SeedAsync("masa_alti");
        SeededProduct plain = await SeedAsync("masa alti");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply turkish = await GetAsync(client, "/arama?search=" + Uri.EscapeDataString("ISIK"));
        Assert.Contains($"/urun/{lamp.Slug}\"", turkish.Body, StringComparison.Ordinal);

        Reply lower = await GetAsync(client, "/arama?search=" + Uri.EscapeDataString("ışık lambası"));
        Assert.Contains($"/urun/{lamp.Slug}\"", lower.Body, StringComparison.Ordinal);

        Reply percentOnly = await GetAsync(client, "/arama?search=" + Uri.EscapeDataString("%"));
        Assert.Contains($"/urun/{percent.Slug}\"", percentOnly.Body, StringComparison.Ordinal);
        Assert.DoesNotContain($"/urun/{lamp.Slug}\"", percentOnly.Body, StringComparison.Ordinal);

        Reply underscoreOnly = await GetAsync(client, "/arama?search=" + Uri.EscapeDataString("masa_alti"));
        Assert.Contains($"/urun/{underscore.Slug}\"", underscoreOnly.Body, StringComparison.Ordinal);
        Assert.DoesNotContain($"/urun/{plain.Slug}\"", underscoreOnly.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ASearchWord_LongerThan150Characters_IsCutAt150_T503_KK1()
    {
        string name = new('a', 150);
        SeededProduct product = await SeedAsync(name);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply cut = await GetAsync(client, "/arama?search=" + name + "bbbbbbbbbb");

        Assert.Contains($"/urun/{product.Slug}\"", cut.Body, StringComparison.Ordinal);
    }

    // ---- KK2: empty, no result, archive, filters, noindex ----

    [Fact]
    public async Task AnEmptyWord_ShowsOnlyTheSearchField_AndNoSearchResult_AndThePageIsNoindex_T503_KK2()
    {
        SeededProduct product = await SeedAsync("Masa");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply plain = await GetAsync(client, "/arama");
        Reply blank = await GetAsync(client, "/arama?search=%20%20");

        foreach (Reply reply in new[] { plain, blank })
        {
            Assert.Equal(HttpStatusCode.OK, reply.Status);
            Assert.Contains(SearchPage.SearchLabel, reply.Text, StringComparison.Ordinal);
            Assert.DoesNotContain($"/urun/{product.Slug}\"", reply.Body, StringComparison.Ordinal);
            Assert.DoesNotContain(SearchPage.EmptySearchText, reply.Text, StringComparison.Ordinal);
            Assert.True(IsClosed(reply));
        }
    }

    [Fact]
    public async Task ASearchWithNoProduct_IsTheEmptyStateLine_AndAnArchivedProductIsNotInTheResult_T503_KK2()
    {
        SeededProduct archived = await SeedAsync("Eski masa", PublishStatus.Archived);
        SeededProduct draft = await SeedAsync("Taslak masa", PublishStatus.Draft);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply none = await GetAsync(client, "/arama?search=masa");

        Assert.Equal(HttpStatusCode.OK, none.Status);
        Assert.Contains(SearchPage.EmptySearchText, none.Text, StringComparison.Ordinal);
        Assert.DoesNotContain($"/urun/{archived.Slug}\"", none.Body, StringComparison.Ordinal);
        Assert.DoesNotContain($"/urun/{draft.Slug}\"", none.Body, StringComparison.Ordinal);
        Assert.True(IsClosed(none));
    }

    [Fact]
    public async Task TheFilters_TakeThePriceRangeAndTheStockSwitch_AndAWrongRangeIsAMessage_T503_KK2()
    {
        SeededProduct cheap = await SeedAsync("Masa ucuz", variants: new VariantSpec(Price: 10m, Stock: 5));
        SeededProduct dear = await SeedAsync("Masa pahali", variants: new VariantSpec(Price: 500m, Stock: 0));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply min = await GetAsync(client, "/arama?search=masa&minPrice=100");
        Assert.Contains($"/urun/{dear.Slug}\"", min.Body, StringComparison.Ordinal);
        Assert.DoesNotContain($"/urun/{cheap.Slug}\"", min.Body, StringComparison.Ordinal);

        Reply inStock = await GetAsync(client, "/arama?search=masa&inStock=true");
        Assert.Contains($"/urun/{cheap.Slug}\"", inStock.Body, StringComparison.Ordinal);
        Assert.DoesNotContain($"/urun/{dear.Slug}\"", inStock.Body, StringComparison.Ordinal);

        Reply filteredEmpty = await GetAsync(client, "/arama?search=masa&minPrice=900");
        Assert.Contains(SearchPage.EmptyFilterText, filteredEmpty.Text, StringComparison.Ordinal);

        Reply wrong = await GetAsync(client, "/arama?search=masa&minPrice=500&maxPrice=100");
        Assert.Equal(HttpStatusCode.OK, wrong.Status);
        Assert.Matches(new Regex("filters__|minPrice"), wrong.Body);
    }

    // ---- KK3: the two publish states and the 404 ----

    [Fact]
    public async Task APublishedProduct_IsE04_ADraftIsAPreviewForAnAdministrator_AndAVisitorGetsTheSame404AsAnUnknownAddress_T503_KK3()
    {
        SeededProduct published = await SeedAsync("Masa yayinda");
        SeededProduct draft = await SeedAsync("Masa taslak", PublishStatus.Draft);
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply live = await GetAsync(client, $"/urun/{published.Slug}");
        Reply preview = await GetAsync(client, $"/urun/{draft.Slug}", $"{CookieNames.AdminSession}={adminKey}");
        Reply hidden = await GetAsync(client, $"/urun/{draft.Slug}");
        Reply unknown = await GetAsync(client, "/urun/yok-boyle-bir-urun");

        Assert.Equal(HttpStatusCode.OK, live.Status);
        Assert.Contains("Masa yayinda", live.Text, StringComparison.Ordinal);
        Assert.Contains("Sepete ekle", live.Text, StringComparison.Ordinal);
        Assert.False(IsClosed(live));

        Assert.Equal(HttpStatusCode.OK, preview.Status);
        Assert.Contains(ProductView.PreviewText, preview.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Sepete ekle", preview.Text, StringComparison.Ordinal);
        Assert.True(IsClosed(preview));

        Assert.Equal(HttpStatusCode.NotFound, hidden.Status);
        Assert.Equal(HttpStatusCode.NotFound, unknown.Status);
        Assert.Equal(WithoutNonce(unknown.Body), WithoutNonce(hidden.Body));
    }

    [Fact]
    public async Task AnArchivedProduct_ShowsTheNoLongerSoldPage_WithNoPriceAndNoCartButton_T503_KK3()
    {
        SeededProduct archived = await SeedAsync("Eski masa", PublishStatus.Archived);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply page = await GetAsync(client, $"/urun/{archived.Slug}");

        Assert.Equal(HttpStatusCode.OK, page.Status);
        Assert.Contains(ProductView.NoLongerSoldText, page.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Sepete ekle", page.Text, StringComparison.Ordinal);
        Assert.True(IsClosed(page));
    }

    // ---- KK4: a wrong parameter falls back to the default variant ----

    [Fact]
    public async Task TheSelection_IsTheTwoOrders_AndAWrongOrMissingOrSoldOutValueGivesTheDefaultVariant_T503_KK4()
    {
        SeededProduct product = await SeedAsync(
            "Masa secimli",
            variants:
            [
                new VariantSpec(Price: 111m, First: 1, Second: 1),
                new VariantSpec(Price: 222m, First: 2, Second: 1),
                new VariantSpec(Price: 333m, Stock: 0, First: 2, Second: 2),
            ]);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        static string Price(decimal amount) => DisplayFormat.Amount(amount);

        Reply named = await GetAsync(client, $"/urun/{product.Slug}?firstOptionValue=2&secondOptionValue=1");
        Assert.Contains(Price(222m), named.Text, StringComparison.Ordinal);

        foreach (string wrong in new[] { "?firstOptionValue=abc&secondOptionValue=1", "?firstOptionValue=2", "?secondOptionValue=1", "?firstOptionValue=9&secondOptionValue=9", "?firstOptionValue=2&secondOptionValue=2", "?firstOptionValue=-1&secondOptionValue=%00" })
        {
            Reply fallback = await GetAsync(client, $"/urun/{product.Slug}{wrong}");
            Assert.Equal(HttpStatusCode.OK, fallback.Status);
            Assert.Contains(Price(111m), fallback.Text, StringComparison.Ordinal);
            Assert.True(IsClosed(fallback));
        }
    }

    // ---- KK5: the variant's price, delivery by type, "Tükendi", the reserved quantity, the T501 logo ----

    [Fact]
    public async Task TheDeliveryLine_IsByTheProductsType_AndTheLogoIsT501s_T503_KK5()
    {
        SeededProduct physical = await SeedAsync("Masa fiziksel");
        SeededProduct digital = await SeedAsync("Kitap dijital", type: ProductType.Digital);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply physicalPage = await GetAsync(client, $"/urun/{physical.Slug}");
        Reply digitalPage = await GetAsync(client, $"/urun/{digital.Slug}");

        Assert.Contains("Yerli üretim logosu", physicalPage.Text, StringComparison.Ordinal);
        Assert.Contains(DomesticProductionLogo.Address, physicalPage.Body, StringComparison.Ordinal);
        Assert.Contains("name=\"Quantity\"", physicalPage.Body, StringComparison.Ordinal);
        Assert.Contains("İndirme, ödeme onaylandığında açılır", digitalPage.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("name=\"Quantity\"", digitalPage.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AVariantWhoseStockIsAllReserved_IsSoldOut_AndASoldOutProductShowsTukendiInPlaceOfTheButton_T503_KK5()
    {
        SeededProduct reserved = await SeedAsync("Masa ayrilmis", variants: new VariantSpec(Stock: 3, Reserved: 3));
        SeededProduct partly = await SeedAsync("Masa kismen", variants: new VariantSpec(Stock: 3, Reserved: 2));
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply soldOut = await GetAsync(client, $"/urun/{reserved.Slug}");
        Reply buyable = await GetAsync(client, $"/urun/{partly.Slug}");

        Assert.Contains(ProductView.SoldOutText, soldOut.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("Sepete ekle", soldOut.Text, StringComparison.Ordinal);
        Assert.DoesNotContain(ProductView.SoldOutText, buyable.Text, StringComparison.Ordinal);
        Assert.Contains("Sepete ekle", buyable.Text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ASoldOutValue_CarriesTukendiInTheSelection_AndCannotBeChosen_T503_KK5()
    {
        SeededProduct product = await SeedAsync(
            "Masa renkli",
            variants:
            [
                new VariantSpec(First: 1),
                new VariantSpec(Stock: 0, First: 2),
            ]);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply page = await GetAsync(client, $"/urun/{product.Slug}");

        Assert.Contains("Renk 2 (Tükendi)", page.Text.Replace("\r", string.Empty, StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Replace("  ", " ", StringComparison.Ordinal), StringComparison.Ordinal);
        Assert.Matches(new Regex("<option[^>]*disabled[^>]*>[^<]*Renk 2", RegexOptions.Singleline), page.Body);
    }

    // ---- KK6: the page's data has no stock count; "Paylaş" ----

    [Fact]
    public async Task ThePageData_CarriesThePublishedVariantsWithNoStockCount_AndThereIsAShareButton_T503_KK6()
    {
        SeededProduct product = await SeedAsync(
            "Masa veri",
            variants:
            [
                new VariantSpec(Price: 111m, Stock: 7, First: 1),
                new VariantSpec(Price: 222m, Stock: 0, First: 2),
                new VariantSpec(Price: 333m, Stock: 4, Status: PublishStatus.Draft, First: 3),
            ]);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply page = await GetAsync(client, $"/urun/{product.Slug}");

        Match data = Regex.Match(page.Body, "data-variants=\"([^\"]*)\"");
        Assert.True(data.Success);
        using JsonDocument json = JsonDocument.Parse(WebUtility.HtmlDecode(data.Groups[1].Value));
        Assert.Equal(2, json.RootElement.GetArrayLength());
        Assert.DoesNotContain("stock", json.RootElement.GetRawText(), StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("\"7\"", json.RootElement.GetRawText(), StringComparison.Ordinal);
        Assert.Contains("Paylaş", page.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("<script>", page.Body.Replace("<script type=\"module\"", string.Empty, StringComparison.Ordinal), StringComparison.Ordinal);
    }

    // ---- KK7, KK9: the 303, the notice, the cart; a second send is a second add ----

    [Fact]
    public async Task AnAdd_Is303BackToThePageWithTheTwoPlacesAndTheNotice_AndTheCartHoldsTheLine_T503_KK7()
    {
        SeededProduct product = await SeedAsync(
            "Masa ekle",
            variants:
            [
                new VariantSpec(First: 1, Second: 1),
                new VariantSpec(First: 2, Second: 3),
            ]);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply added = await PostAsync(client, AddPath(product.Slug), null, ("VariantId", product.VariantIds[1].ToString("D")), ("Quantity", "2"));

        Assert.Equal(HttpStatusCode.SeeOther, added.Status);
        Assert.StartsWith($"/urun/{product.Slug}?firstOptionValue=2&secondOptionValue=3", added.Location, StringComparison.Ordinal);
        Assert.Contains("notice=", added.Location, StringComparison.Ordinal);
        string cookie = GuestCartCookieOf(added);
        Assert.Equal(2, await CartQuantityOfAsync(product.VariantIds[1]));

        Reply page = await GetAsync(client, added.Location!, cookie);
        Assert.Contains(CartAddMessages.AddedText, page.Text, StringComparison.Ordinal);
        Assert.True(IsClosed(page));
    }

    [Fact]
    public async Task ASecondSend_IsASecondAdd_AndTheQuantityGrows_T503_KK9()
    {
        SeededProduct product = await SeedAsync("Masa iki kez");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply first = await PostAsync(client, AddPath(product.Slug), null, ("VariantId", product.VariantIds[0].ToString("D")), ("Quantity", "1"));
        string cookie = GuestCartCookieOf(first);
        Reply second = await PostAsync(client, AddPath(product.Slug), cookie, ("VariantId", product.VariantIds[0].ToString("D")), ("Quantity", "1"));

        Assert.Equal(HttpStatusCode.SeeOther, second.Status);
        Assert.Equal(2, await CartQuantityOfAsync(product.VariantIds[0]));
        Assert.Equal(1, await CartCountAsync());
    }

    // ---- KK8: every refusal ----

    [Fact]
    public async Task EachRefusal_Is422_WithItsMessageInPlace_AndNoCartIsOpened_T503_KK8()
    {
        SeededProduct stock = await SeedAsync("Masa stoklu", variants: new VariantSpec(Stock: 2));
        SeededProduct limited = await SeedAsync("Masa sinirli", maxPerOrder: 2, variants: new VariantSpec(Stock: 1));
        SeededProduct soldOut = await SeedAsync("Masa tukenmis", variants: [new VariantSpec(First: 1), new VariantSpec(Stock: 0, First: 2)]);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply noStock = await PostAsync(client, AddPath(stock.Slug), null, ("VariantId", stock.VariantIds[0].ToString("D")), ("Quantity", "3"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, noStock.Status);
        Assert.Contains(CartAddMessages.NoStockText, noStock.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("2 adet", noStock.Text, StringComparison.Ordinal);

        // P-9: the limit comes before the stock (the stock is 1, the limit 2, the request 3: the limit is told).
        Reply limit = await PostAsync(client, AddPath(limited.Slug), null, ("VariantId", limited.VariantIds[0].ToString("D")), ("Quantity", "3"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, limit.Status);
        Assert.Contains(CartAddMessages.OrderLimitText(2), limit.Text, StringComparison.Ordinal);

        Reply sold = await PostAsync(client, AddPath(soldOut.Slug), null, ("VariantId", soldOut.VariantIds[1].ToString("D")), ("Quantity", "1"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, sold.Status);
        Assert.Contains(CartAddMessages.SoldOutText, sold.Text, StringComparison.Ordinal);

        Reply unselected = await PostAsync(client, AddPath(soldOut.Slug), null, ("VariantId", string.Empty), ("Quantity", "1"));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unselected.Status);
        Assert.Contains(CartAddMessages.SelectOptionText, unselected.Text, StringComparison.Ordinal);

        foreach (string quantity in new[] { "0", "-1", "abc", "1.5", string.Empty, "99999999999" })
        {
            Reply invalid = await PostAsync(client, AddPath(stock.Slug), null, ("VariantId", stock.VariantIds[0].ToString("D")), ("Quantity", quantity));
            Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.Status);
            Assert.Contains(CartQuantity.InvalidText, invalid.Text, StringComparison.Ordinal);
        }

        Assert.Equal(0, await CartCountAsync());
        Assert.False(noStock.Message.Headers.Contains("Set-Cookie") && noStock.Message.Headers.GetValues("Set-Cookie").Any(c => c.StartsWith(CookieNames.Cart + "=", StringComparison.Ordinal)));
    }

    [Fact]
    public async Task ASalesClosedGate_RefusesTheAdd_WithTheClosedMessage_T503_KK8()
    {
        SeededProduct product = await SeedAsync("Masa kapali");
        _gate.Value = new SalesGate([SalesGateCondition.TemporaryClosure], []);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply page = await GetAsync(client, $"/urun/{product.Slug}");
        Reply closed = await PostAsync(client, AddPath(product.Slug), null, ("VariantId", product.VariantIds[0].ToString("D")), ("Quantity", "1"));

        Assert.DoesNotContain("Sepete ekle", page.Text, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed.Status);
        Assert.Contains(SalesClosedMessages.StorefrontText, closed.Text, StringComparison.Ordinal);
        Assert.Equal(0, await CartCountAsync());
    }

    [Fact]
    public async Task ADigitalVariantAlreadyInTheCart_IsRefused_AndAnyQuantityIsOne_T503_KK8()
    {
        SeededProduct product = await SeedAsync("Kitap dijital", type: ProductType.Digital);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply first = await PostAsync(client, AddPath(product.Slug), null, ("VariantId", product.VariantIds[0].ToString("D")), ("Quantity", "5"));
        Assert.Equal(HttpStatusCode.SeeOther, first.Status);
        Assert.Equal(1, await CartQuantityOfAsync(product.VariantIds[0]));

        Reply again = await PostAsync(client, AddPath(product.Slug), GuestCartCookieOf(first), ("VariantId", product.VariantIds[0].ToString("D")));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, again.Status);
        Assert.Contains(CartAddMessages.AlreadyInCartText, again.Text, StringComparison.Ordinal);
        Assert.Equal(1, await CartQuantityOfAsync(product.VariantIds[0]));
    }

    [Fact]
    public async Task ANotPublishedProductOrAnUnknownVariant_Is404_ForEveryReason_T503_KK8()
    {
        SeededProduct draft = await SeedAsync("Masa taslak", PublishStatus.Draft);
        SeededProduct archived = await SeedAsync("Masa arsiv", PublishStatus.Archived, variants: new VariantSpec());
        SeededProduct live = await SeedAsync("Masa yayinda");
        SeededProduct draftVariant = await SeedAsync("Masa taslak varyant", variants: [new VariantSpec(), new VariantSpec(Status: PublishStatus.Draft, First: 1)]);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        Reply draftReply = await PostAsync(client, AddPath(draft.Slug), null, ("VariantId", draft.VariantIds[0].ToString("D")), ("Quantity", "1"));
        Reply archivedReply = await PostAsync(client, AddPath(archived.Slug), null, ("VariantId", archived.VariantIds[0].ToString("D")), ("Quantity", "1"));
        Reply missingProduct = await PostAsync(client, AddPath("yok-boyle-bir-urun"), null, ("VariantId", live.VariantIds[0].ToString("D")), ("Quantity", "1"));
        Reply foreignVariant = await PostAsync(client, AddPath(live.Slug), null, ("VariantId", draft.VariantIds[0].ToString("D")), ("Quantity", "1"));
        Reply unknownVariant = await PostAsync(client, AddPath(live.Slug), null, ("VariantId", Guid.NewGuid().ToString("D")), ("Quantity", "1"));
        Reply draftVariantReply = await PostAsync(client, AddPath(draftVariant.Slug), null, ("VariantId", draftVariant.VariantIds[1].ToString("D")), ("Quantity", "1"));

        Assert.All(
            new[] { draftReply, archivedReply, missingProduct, foreignVariant, unknownVariant, draftVariantReply },
            r => Assert.Equal(HttpStatusCode.NotFound, r.Status));
        Assert.Equal(0, await CartCountAsync());
    }

    public sealed class GateHolder
    {
        public SalesGate Value { get; set; } = new([], []);
    }

    /// <summary>The settings module as it is, except for the sales gate, which the test sets.</summary>
    public class GateForwarder : DispatchProxy
    {
        internal ISettingsModule Target { get; set; } = null!;

        internal GateHolder Holder { get; set; } = null!;

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(ISettingsModule.GetSalesGateAsync))
            {
                return Task.FromResult(Holder.Value);
            }

            try
            {
                return targetMethod!.Invoke(Target, args);
            }
            catch (TargetInvocationException exception)
            {
                ExceptionDispatchInfo.Capture(exception.InnerException!).Throw();
                throw;
            }
        }
    }
}
