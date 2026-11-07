using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Pages.Panel;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the version and shown-value form tests (T208): one database copied from the template.</summary>
[CollectionDefinition(VersionedFormDefinition.Name)]
public sealed class VersionedFormDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "VersionedForms";
}

/// <summary>
/// The version and the shown value in the forms, on the opened application and test-only sample pages (T208 criteria 1-9, KK1-KK9; 07 2.4; 05 7.3;
/// 09 10.2): the form of a record with a version carries it in a hidden field, an old version is refused with 409 and nothing is written, the form shows
/// again with the record's current version and "Yine de kaydet", a value written against the shown value is compared on every send, the order variant
/// has no way to overwrite, and no version or shown value is part of an address or a header.
/// </summary>
[Collection(VersionedFormDefinition.Name)]
public sealed partial class VersionedFormPageTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private static readonly string[] Winners = ["ilk", "ikinci"];

    private readonly CollectionDatabaseFixture _database;
    private readonly string _adminKey;
    // A test class is made again for every test, and the database of the collection is shared: the numbers are the class's.
    private static int _counter;

    public VersionedFormPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
        _adminKey = TestSessions.AdminAsync(database.Dsn).GetAwaiter().GetResult().Key;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex(@"<input[^>]*>", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex InputTag();

    [GeneratedRegex(@"value=""([^""]*)""", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ValueAttribute();

    private WebApplicationFactory<Program> Open()
    {
        ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        return factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(VersionedProductModel).Assembly)));
    }

    private HttpClient ClientOf(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add("Cookie", $"{CookieNames.AdminSession}={_adminKey}");
        return client;
    }

    private static string DecodedBody(string html) => WebUtility.HtmlDecode(html);

    /// <summary>The value of the hidden or text field of that name, or null when the page has no such field.</summary>
    private static string? FieldValue(string html, string name)
    {
        foreach (Match tag in InputTag().Matches(html))
        {
            if (tag.Value.Contains($"name=\"{name}\"", StringComparison.Ordinal))
            {
                Match value = ValueAttribute().Match(tag.Value);
                return value.Success ? WebUtility.HtmlDecode(value.Groups[1].Value) : string.Empty;
            }
        }

        return null;
    }

    private static string FormOf(string html, string id)
    {
        int start = html.IndexOf($"<form id=\"{id}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, id);
        int end = html.IndexOf("</form>", start, StringComparison.Ordinal);
        return html[start..end];
    }

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));

    private static async Task<string> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(path, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync(Ct);
    }

    // ---- KK1, KK4, KK5: the record variant (07 2.4.1, 2.4.5) ----

    [Fact]
    public async Task RecordForm_CarriesTheVersionTheRecordHas_InAHiddenField_AndInNoAddress_KK1()
    {
        Product product = await SeedProductAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.GetAsync($"/panel/surumlu-urun/{product.Id}", Ct);
        string html = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(product.Version.ToString(System.Globalization.CultureInfo.InvariantCulture), FieldValue(html, "Input.Version"));
        Assert.Contains($"action=\"/panel/surumlu-urun/{product.Id}?handler=Save\"", html, StringComparison.Ordinal);
        Assert.Contains("<button type=\"submit\">Kaydet</button>", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Yine de", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RecordPost_WithTheCurrentVersion_Saves_RedirectsWith303_AndRaisesTheVersionByOne_KK1()
    {
        Product product = await SeedProductAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(
            $"/panel/surumlu-urun/{product.Id}?handler=Save",
            Form(("Input.Version", product.Version.ToString(System.Globalization.CultureInfo.InvariantCulture)), ("Input.Description", "yeni")),
            Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal($"/panel/surumlu-urun/{product.Id}", response.Headers.Location?.OriginalString);
        Product saved = await ReadProductAsync(product.Id);
        Assert.Equal("yeni", saved.Description);
        Assert.Equal(product.Version + 1, saved.Version);
    }

    [Fact]
    public async Task RecordPost_WithAnOldVersion_Is409_WritesNothing_ShowsTheSentValuesTheCurrentVersionAndOneButton_KK4()
    {
        Product product = await SeedProductAsync();
        await ChangeProductAsync(product.Id, "baskasinin");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(
            $"/panel/surumlu-urun/{product.Id}?handler=Save",
            Form(("Input.Version", product.Version.ToString(System.Globalization.CultureInfo.InvariantCulture)), ("Input.Description", "benim")),
            Ct);
        string html = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        // T213 KK6: the 409 answer names no code, in the page or in a header (07 2.7.3).
        ErrorCodeLeakCheck.AssertNone(response, html);
        Product current = await ReadProductAsync(product.Id);
        Assert.Equal("baskasinin", current.Description);
        Assert.Equal(product.Version + 1, current.Version);
        Assert.Equal("benim", FieldValue(html, "Input.Description"));
        Assert.Equal(current.Version.ToString(System.Globalization.CultureInfo.InvariantCulture), FieldValue(html, "Input.Version"));
        Assert.Contains(ConflictControls.RecordMessage, DecodedBody(html), StringComparison.Ordinal);
        Assert.Contains($"<a href=\"/panel/surumlu-urun/{product.Id}\" rel=\"noopener noreferrer\" target=\"_blank\">Güncel hâli aç</a>", DecodedBody(html), StringComparison.Ordinal);
        Assert.Equal(1, Regex.Count(html, "<button", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)));
        Assert.Contains("<button type=\"submit\">Yine de kaydet</button>", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task SaveAnyway_SendsTheFormWithTheCurrentVersion_AndSaves_KK5()
    {
        Product product = await SeedProductAsync();
        await ChangeProductAsync(product.Id, "baskasinin");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        using HttpResponseMessage refused = await client.PostAsync(
            $"/panel/surumlu-urun/{product.Id}?handler=Save",
            Form(("Input.Version", product.Version.ToString(System.Globalization.CultureInfo.InvariantCulture)), ("Input.Description", "benim")),
            Ct);
        string html = await refused.Content.ReadAsStringAsync(Ct);

        using HttpResponseMessage again = await client.PostAsync(
            $"/panel/surumlu-urun/{product.Id}?handler=Save",
            Form(("Input.Version", FieldValue(html, "Input.Version")!), ("Input.Description", FieldValue(html, "Input.Description")!)),
            Ct);

        Assert.Equal(HttpStatusCode.Conflict, refused.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, again.StatusCode);
        Product saved = await ReadProductAsync(product.Id);
        Assert.Equal("benim", saved.Description);
        Assert.Equal(product.Version + 2, saved.Version);
    }

    [Fact]
    public async Task SaveAnyway_WhenOneMoreChangeWasMadeInBetween_ShowsTheWarningAgain_KK5()
    {
        Product product = await SeedProductAsync();
        await ChangeProductAsync(product.Id, "baskasinin");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        using HttpResponseMessage refused = await client.PostAsync(
            $"/panel/surumlu-urun/{product.Id}?handler=Save",
            Form(("Input.Version", product.Version.ToString(System.Globalization.CultureInfo.InvariantCulture)), ("Input.Description", "benim")),
            Ct);
        string html = await refused.Content.ReadAsStringAsync(Ct);
        await ChangeProductAsync(product.Id, "ucuncu");

        using HttpResponseMessage again = await client.PostAsync(
            $"/panel/surumlu-urun/{product.Id}?handler=Save",
            Form(("Input.Version", FieldValue(html, "Input.Version")!), ("Input.Description", "benim")),
            Ct);
        string second = await again.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Contains("Yine de kaydet", second, StringComparison.Ordinal);
        Product current = await ReadProductAsync(product.Id);
        Assert.Equal("ucuncu", current.Description);
        Assert.Equal(current.Version.ToString(System.Globalization.CultureInfo.InvariantCulture), FieldValue(second, "Input.Version"));
    }

    [Fact]
    public async Task RecordUpdate_WithAStaleVersion_AffectsNoRow_KK4()
    {
        Product product = await SeedProductAsync();
        await ChangeProductAsync(product.Id, "baskasinin");

        int written = await ExecuteCountAsync(
            "UPDATE products SET description = 'eski', version = version + 1 WHERE id = @id AND version = @version",
            ("id", product.Id),
            ("version", product.Version));

        Assert.Equal(0, written);
        Assert.Equal("baskasinin", (await ReadProductAsync(product.Id)).Description);
    }

    [Fact]
    public async Task RecordSave_OfARecordThatIsNotTracked_IsAProgrammingError()
    {
        Product product = await SeedProductAsync();
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Product detached = await db.Products.AsNoTracking().SingleAsync(p => p.Id == product.Id, Ct);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => RecordSave.SaveAsync(db, detached, detached.Version, p => p.SetDescription("x"), Ct));
        Assert.Null((await ReadProductAsync(product.Id)).Description);
    }

    [Fact]
    public async Task TwoSavesOfTheSameVersion_ThatArriveTogether_OneSavesAndTheOtherIsRefused_KK4()
    {
        Product product = await SeedProductAsync();
        await using ServiceProvider provider = BuildProvider();
        TaskCompletionSource firstLoaded = new(TaskCreationOptions.RunContinuationsAsynchronously);
        TaskCompletionSource secondLoaded = new(TaskCreationOptions.RunContinuationsAsynchronously);

        Task<RecordSaveResult> first = SaveTogetherAsync(provider, product, "ilk", firstLoaded, secondLoaded);
        Task<RecordSaveResult> second = SaveTogetherAsync(provider, product, "ikinci", secondLoaded, firstLoaded);
        RecordSaveResult[] results = await Task.WhenAll(first, second);

        Assert.Equal(1, results.Count(r => r.IsSaved));
        Assert.Equal(1, results.Count(r => !r.IsSaved));
        Product saved = await ReadProductAsync(product.Id);
        Assert.Equal(product.Version + 1, saved.Version);
        Assert.Contains(saved.Description, Winners);
    }

    private static async Task<RecordSaveResult> SaveTogetherAsync(
        ServiceProvider provider,
        Product product,
        string description,
        TaskCompletionSource mine,
        TaskCompletionSource other)
    {
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                Product tracked = await db.Products.SingleAsync(p => p.Id == product.Id, token);
                mine.SetResult();
                await other.Task.WaitAsync(TimeSpan.FromSeconds(30), token);
                return await RecordSave.SaveAsync(db, tracked, product.Version, p => p.SetDescription(description), token);
            },
            Ct);
    }

    // ---- KK7: no version and no shown value in an address or a header (07 2.4.7) ----

    [Fact]
    public async Task NoVersionAndNoShownValue_IsInAnAddressOrAHeader_AndTheRequestsNeedNoConditionalHeader_KK7()
    {
        Product product = await SeedProductAsync();
        Variant variant = await SeedVariantAsync(10);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage get = await client.GetAsync($"/panel/surumlu-urun/{product.Id}", Ct);
        string html = await get.Content.ReadAsStringAsync(Ct);
        using HttpResponseMessage post = await client.PostAsync(
            $"/panel/surumlu-urun/{product.Id}?handler=Save",
            Form(("Input.Version", product.Version.ToString(System.Globalization.CultureInfo.InvariantCulture)), ("Input.Description", "x")),
            Ct);
        using HttpResponseMessage conflict = await client.PostAsync(
            $"/panel/surumlu-urun/{product.Id}?handler=Save",
            Form(("Input.Version", product.Version.ToString(System.Globalization.CultureInfo.InvariantCulture)), ("Input.Description", "y")),
            Ct);
        string stock = await GetAsync(client, $"/panel/surumlu-stok/{variant.Id}");

        foreach (HttpResponseMessage response in new[] { get, post, conflict })
        {
            Assert.Null(response.Headers.ETag);
            Assert.False(response.Headers.Contains("ETag"));
            Assert.False(response.Content.Headers.Contains("Last-Modified"));
            Assert.DoesNotContain(response.Headers, h => h.Key.Contains("Version", StringComparison.OrdinalIgnoreCase) || h.Key.Contains("Original", StringComparison.OrdinalIgnoreCase));
        }

        Assert.DoesNotContain("version", post.Headers.Location?.OriginalString ?? string.Empty, StringComparison.OrdinalIgnoreCase);
        foreach (Match action in Regex.Matches(html + stock, "action=\"([^\"]*)\"", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)))
        {
            Assert.DoesNotContain("version", action.Groups[1].Value, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("original", action.Groups[1].Value, StringComparison.OrdinalIgnoreCase);
        }

        Assert.Equal(HttpStatusCode.SeeOther, post.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, conflict.StatusCode);
        Assert.False(client.DefaultRequestHeaders.Contains("If-Match"));
    }

    // ---- KK3: a value written against the shown value (07 2.4.3; 05 7.3.3) ----

    [Fact]
    public async Task ShownValue_FirstSend_WithAChangedValue_IsRefused_BeforeTheConfirmation_AndWritesNothing_KK3()
    {
        Variant variant = await SeedVariantAsync(10);
        await SetStockAsync(variant.Id, 12);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(
            $"/panel/surumlu-stok/{variant.Id}?handler=Save",
            Form(("Input.OriginalStockQuantity", "10"), ("Input.StockQuantity", "5")),
            Ct);
        string html = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains("<p class=\"step\">changed</p>", html, StringComparison.Ordinal);
        Assert.Equal("12", FieldValue(html, "Input.OriginalStockQuantity"));
        Assert.Equal(12, await ReadStockAsync(variant.Id));
    }

    [Fact]
    public async Task ShownValue_TheLastStep_CarriesTheFirstFormsValue_AndAChangeInBetweenIsNotOverwritten_KK3()
    {
        Variant variant = await SeedVariantAsync(10);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        using HttpResponseMessage first = await client.PostAsync(
            $"/panel/surumlu-stok/{variant.Id}?handler=Save",
            Form(("Input.OriginalStockQuantity", "10"), ("Input.StockQuantity", "5")),
            Ct);
        string confirmation = await first.Content.ReadAsStringAsync(Ct);
        await SetStockAsync(variant.Id, 12);

        // The confirmation form carries the value of the first form (10), not the page's current value.
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Contains("<p class=\"step\">confirm</p>", confirmation, StringComparison.Ordinal);
        Assert.Equal("10", FieldValue(confirmation, "Input.OriginalStockQuantity"));
        Assert.Equal("true", FieldValue(confirmation, "Input.Confirmed"));
        using HttpResponseMessage last = await client.PostAsync(
            $"/panel/surumlu-stok/{variant.Id}?handler=Save",
            Form(
                ("Input.OriginalStockQuantity", FieldValue(confirmation, "Input.OriginalStockQuantity")!),
                ("Input.StockQuantity", FieldValue(confirmation, "Input.StockQuantity")!),
                ("Input.Confirmed", "true")),
            Ct);

        Assert.Equal(HttpStatusCode.Conflict, last.StatusCode);
        Assert.Equal(12, await ReadStockAsync(variant.Id));
    }

    [Fact]
    public async Task ShownValue_TheLastStep_WithTheValueUnchanged_Writes_KK3()
    {
        Variant variant = await SeedVariantAsync(10);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage last = await client.PostAsync(
            $"/panel/surumlu-stok/{variant.Id}?handler=Save",
            Form(("Input.OriginalStockQuantity", "10"), ("Input.StockQuantity", "5"), ("Input.Confirmed", "true")),
            Ct);

        Assert.Equal(HttpStatusCode.SeeOther, last.StatusCode);
        Assert.Equal(5, await ReadStockAsync(variant.Id));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("on")]
    [InlineData("10.5")]
    public async Task ShownValue_MissingEmptyOrNotANumber_IsNotAMatch_AndWritesNothing_KK3(string? original)
    {
        Variant variant = await SeedVariantAsync(10);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        List<(string, string)> fields = [("Input.StockQuantity", "5"), ("Input.Confirmed", "true")];
        if (original is not null)
        {
            fields.Add(("Input.OriginalStockQuantity", original));
        }

        using HttpResponseMessage response = await client.PostAsync($"/panel/surumlu-stok/{variant.Id}?handler=Save", Form([.. fields]), Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal(10, await ReadStockAsync(variant.Id));
    }

    // ---- KK2, KK6, KK8: the order variant (07 2.4.2, 2.4.6; 05 7.3.2) ----

    [Fact]
    public async Task OrderPage_TheAdministratorsFormCarriesTheVersion_TheCustomersCarriesOnlyTheShownOpenQuantity_KK2()
    {
        (Order order, OrderItem item) = await SeedOrderAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        string html = await GetAsync(client, $"/panel/surumlu-siparis/{order.Id}");
        string admin = FormOf(html, "admin");
        string customer = FormOf(html, "customer");

        Assert.Equal(order.Version.ToString(System.Globalization.CultureInfo.InvariantCulture), FieldValue(admin, "Input.Version"));
        Assert.Null(FieldValue(customer, "Customer.Version"));
        Assert.DoesNotContain("Version", customer, StringComparison.Ordinal);
        Assert.Equal(item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture), FieldValue(customer, "Customer.OriginalOpenQuantity"));
    }

    [Fact]
    public async Task OrderPost_ByTheAdministrator_WithAnOldVersion_IsNotApplied_Is409_ShowsTheCurrentOrder_AndHasNoWayToOverwrite_KK6()
    {
        (Order order, _) = await SeedOrderAsync();
        await ChangeDistrictAsync(order.Id, "Kızılay");
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(
            $"/panel/surumlu-siparis/{order.Id}?handler=AdminSave",
            Form(("Input.Version", order.Version.ToString(System.Globalization.CultureInfo.InvariantCulture)), ("Input.District", "Ulus")),
            Ct);
        string html = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Contains(ConflictControls.OrderMessage, DecodedBody(html), StringComparison.Ordinal);
        Assert.Contains("<p class=\"district\">Kızılay</p>", DecodedBody(html), StringComparison.Ordinal);
        Assert.DoesNotContain("Yine de", html, StringComparison.Ordinal);
        Assert.Equal("Kızılay", (await ReadOrderAsync(order.Id)).BillingDistrict);
        Assert.Equal((order.Version + 1).ToString(System.Globalization.CultureInfo.InvariantCulture), FieldValue(FormOf(html, "admin"), "Input.Version"));
    }

    [Fact]
    public async Task OrderPost_ByTheAdministrator_SentTwice_TheSecondIs409_KK8()
    {
        (Order order, _) = await SeedOrderAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        string version = order.Version.ToString(System.Globalization.CultureInfo.InvariantCulture);

        using HttpResponseMessage first = await client.PostAsync(
            $"/panel/surumlu-siparis/{order.Id}?handler=AdminSave", Form(("Input.Version", version), ("Input.District", "Ulus")), Ct);
        using HttpResponseMessage second = await client.PostAsync(
            $"/panel/surumlu-siparis/{order.Id}?handler=AdminSave", Form(("Input.Version", version), ("Input.District", "Ulus")), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
        Order saved = await ReadOrderAsync(order.Id);
        Assert.Equal(order.Version + 1, saved.Version);
        Assert.Equal("Ulus", saved.BillingDistrict);
    }

    [Fact]
    public async Task OrderPost_ByTheCustomer_WithAChangedOpenQuantity_IsNotProcessed_AndWithTheShownOneIsProcessed_KK2()
    {
        (Order order, OrderItem item) = await SeedOrderAsync();
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage stale = await client.PostAsync(
            $"/panel/surumlu-siparis/{order.Id}?handler=CustomerCancel",
            Form(("Customer.ItemId", item.Id.ToString()), ("Customer.OriginalOpenQuantity", "7")),
            Ct);
        int afterStale = await CancellationCountAsync(item.Id);
        using HttpResponseMessage good = await client.PostAsync(
            $"/panel/surumlu-siparis/{order.Id}?handler=CustomerCancel",
            Form(("Customer.ItemId", item.Id.ToString()), ("Customer.OriginalOpenQuantity", "1")),
            Ct);

        // D-130 (07 2.4.3, 2.7.1; VAL-A582, VAL-A689): the customer's shown open quantity that no longer holds is a 303 to the order with the reason, not a 409.
        Assert.Equal(HttpStatusCode.SeeOther, stale.StatusCode);
        Assert.Equal($"/panel/surumlu-siparis/{order.Id}?{NoticeParameter.ParameterName}={NoticeCode.QuantityExceedsOpen}", stale.Headers.Location?.OriginalString);
        Assert.Equal(0, afterStale);
        Assert.Equal(HttpStatusCode.SeeOther, good.StatusCode);
        Assert.Equal(1, await CancellationCountAsync(item.Id));
        Assert.Equal(order.Version + 1, (await ReadOrderAsync(order.Id)).Version);
    }

    // ---- KK9: the records without a version (06 2.6.5; 07 2.4.4) ----

    [Fact]
    public async Task RecordsWithoutAVersion_HaveNoVersionColumn_AndTheirOperationWritesOnlyFromTheStateItNeeds_KK9()
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Assert.Null(db.Model.FindEntityType(typeof(Category))!.FindProperty("Version"));
        Assert.Null(db.Model.FindEntityType(typeof(ContactRequest))!.FindProperty("Version"));
        Assert.False(typeof(Shopfolio.Infrastructure.DataModel.IVersionedRecord).IsAssignableFrom(typeof(Category)));
        Assert.False(typeof(Shopfolio.Infrastructure.DataModel.IVersionedRecord).IsAssignableFrom(typeof(ContactRequest)));
        ContactRequest request = ContactRequest.Create("Ali", "ali@example.com", ContactTopic.GeneralQuestion, "Merhaba", Now);
        Category category = Category.Create("Kapılar", $"kapilar-{Interlocked.Increment(ref _counter)}", null, 1, Now);
        db.ContactRequests.Add(request);
        db.Categories.Add(category);
        await db.SaveChangesAsync(Ct);
        DateTimeOffset closedAt = Now.AddHours(1);

        bool first = await CurrentStateWrite.ApplyAsync(
            db.ContactRequests.Where(c => c.Id == request.Id),
            c => c.ContactRequestStatus == ContactRequestStatus.Open,
            s => s.SetProperty(c => c.ContactRequestStatus, ContactRequestStatus.Closed).SetProperty(c => c.LastClosedAt, closedAt),
            Ct);
        bool second = await CurrentStateWrite.ApplyAsync(
            db.ContactRequests.Where(c => c.Id == request.Id),
            c => c.ContactRequestStatus == ContactRequestStatus.Open,
            s => s.SetProperty(c => c.ContactRequestStatus, ContactRequestStatus.Closed).SetProperty(c => c.LastClosedAt, closedAt.AddHours(1)),
            Ct);

        Assert.True(first);
        Assert.False(second);
        await using AppDbContext reader = NewContext();
        ContactRequest saved = await reader.ContactRequests.AsNoTracking().SingleAsync(c => c.Id == request.Id, Ct);
        Assert.Equal(ContactRequestStatus.Closed, saved.ContactRequestStatus);
        Assert.Equal(closedAt, saved.LastClosedAt);

        // A plain field edit of a record without a version is the last writer's: two edits both succeed (06 2.6.5).
        category.Name = "Kapı ve pencere";
        await db.SaveChangesAsync(Ct);
        await using AppDbContext other = NewContext();
        Category copy = await other.Categories.SingleAsync(c => c.Id == category.Id, Ct);
        copy.Name = "Pencere";
        await other.SaveChangesAsync(Ct);
        Assert.Equal("Pencere", (await reader.Categories.AsNoTracking().SingleAsync(c => c.Id == category.Id, Ct)).Name);
    }

    // ---- Helpers ----

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _database.Dsn));
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton<Shopfolio.Infrastructure.Jobs.IJobSignal, Shopfolio.Infrastructure.Jobs.JobSignal>();
        services.AddScoped<Shopfolio.Infrastructure.Jobs.JobEnqueueTracker>();
        return services.BuildServiceProvider();
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
    }

    private async Task<int> ExecuteCountAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<Product> SeedProductAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Kapı", $"kapi-{n}", ProductType.Physical, 20m, Now);
        await SaveAsync(c => c.Products.Add(product));
        return product;
    }

    private async Task<Variant> SeedVariantAsync(int? stock)
    {
        Product product = await SeedProductAsync();
        int n = Interlocked.Increment(ref _counter);
        Variant variant = Variant.Create(product.Id, $"KAPI-{n}", 100m, Now);
        variant.StockQuantity = stock;
        await SaveAsync(c => c.Variants.Add(variant));
        return variant;
    }

    private async Task<(Order Order, OrderItem Item)> SeedOrderAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Province province;
        await using (AppDbContext reader = NewContext())
        {
            province = await reader.Provinces.AsNoTracking().FirstOrDefaultAsync(Ct) ?? Province.Create("06", "Ankara", Now);
        }

        bool provinceExists = await ProvinceExistsAsync(province.Id);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", 1, 100m, 20m, 16.67m, Now);
        await SaveAsync(c =>
        {
            if (!provinceExists)
            {
                c.Provinces.Add(province);
            }

            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.Add(item);
        });
        return (order, item);
    }

    private async Task<bool> ProvinceExistsAsync(Guid id)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Provinces.AnyAsync(p => p.Id == id, Ct);
    }

    private async Task<Product> ReadProductAsync(Guid id)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Products.AsNoTracking().SingleAsync(p => p.Id == id, Ct);
    }

    private async Task<Order> ReadOrderAsync(Guid id)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == id, Ct);
    }

    private async Task<int?> ReadStockAsync(Guid variantId)
    {
        await using AppDbContext reader = NewContext();
        return (await reader.Variants.AsNoTracking().SingleAsync(v => v.Id == variantId, Ct)).StockQuantity;
    }

    private async Task<int> CancellationCountAsync(Guid itemId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Cancellations.CountAsync(c => c.OrderItemId == itemId, Ct);
    }

    // Another administrator's save: the same sequence the form uses, from its own context.
    private async Task ChangeProductAsync(Guid id, string description)
    {
        await using AppDbContext context = NewContext();
        Product product = await context.Products.SingleAsync(p => p.Id == id, Ct);
        product.SetDescription(description);
        product.BumpVersion();
        await context.SaveChangesAsync(Ct);
    }

    private async Task ChangeDistrictAsync(Guid id, string district)
    {
        await using AppDbContext context = NewContext();
        Order order = await context.Orders.SingleAsync(o => o.Id == id, Ct);
        order.BillingDistrict = district;
        order.BumpVersion();
        await context.SaveChangesAsync(Ct);
    }

    private async Task SetStockAsync(Guid variantId, int stock) =>
        await ExecuteCountAsync("UPDATE variants SET stock_quantity = @stock WHERE id = @id", ("stock", stock), ("id", variantId));
}
