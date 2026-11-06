using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.IntegrationTests.Pages.Panel;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the available-operations pattern tests (T217): one database copied from the template.</summary>
[CollectionDefinition(OperationPatternDefinition.Name)]
public sealed class OperationPatternDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "OperationPattern";
}

/// <summary>
/// The pattern of the operations a screen offers, on the test-only order page (T217 criteria 1-8, KK1-KK8; 07 8.1; 03 1.11; 09 8.3): the GET draws a button only for
/// an open operation and writes the open quantity, the default, the upper limit and the last day; every POST re-checks the same rule under the order's row lock
/// and a request for a closed operation - the button was never shown, or the order changed since - writes nothing and is answered by the operation's class.
/// </summary>
[Collection(OperationPatternDefinition.Name)]
public sealed partial class OperationPatternTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    private static int _counter;

    private readonly CollectionDatabaseFixture _database;
    private readonly string _adminKey;

    public OperationPatternTests(CollectionDatabaseFixture database)
    {
        _database = database;
        _adminKey = TestSessions.AdminAsync(database.Dsn).GetAwaiter().GetResult().Key;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex(@"<(?<tag>p|form|section)\b[^>]*\bclass=""(?<name>[a-z-]+)""[^>]*>(?<text>[^<]*)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ClassedText();

    // ---- KK1, KK6: the closed operation has no button; the open one carries the server's values ----

    [Fact]
    public async Task Get_AnOrderBeingPrepared_ShowsTheCancelFormOnly_WithTheOpenQuantityTheDefaultAndTheUpperLimitFromTheServer_KK1_KK6()
    {
        const int quantity = 3;
        (Order order, _) = await SeedAsync(quantity, OrderStatus.Preparing, PaymentStatus.Paid, null);
        await using WebApplicationFactory<Program> factory = Open(new SampleRules(new SampleCancelRule(), new SampleWithdrawRule()), new ManualTimeProvider(Now));
        using HttpClient client = ClientOf(factory);

        string html = await GetAsync(client, order.Id);

        Assert.Contains("id=\"customer-cancel\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"admin-cancel\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"customer-withdraw\"", html, StringComparison.Ordinal);
        Assert.Equal(quantity.ToString(System.Globalization.CultureInfo.InvariantCulture), TextOf(html, "open-quantity"));
        Assert.Equal(quantity.ToString(System.Globalization.CultureInfo.InvariantCulture), TextOf(html, "max-quantity"));
        Assert.Contains($"name=\"Input.Quantity\" value=\"{quantity}\" max=\"{quantity}\"", html, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(OrderStatus.Placed, PaymentStatus.Pending)]
    [InlineData(OrderStatus.Shipped, PaymentStatus.Paid)]
    [InlineData(OrderStatus.Delivered, PaymentStatus.Paid)]
    [InlineData(OrderStatus.Cancelled, PaymentStatus.Refunded)]
    public async Task Get_AClosedCancel_HasNoCancelButton_KK1(OrderStatus status, PaymentStatus payment)
    {
        (Order order, _) = await SeedAsync(1, status, payment, null);
        await using WebApplicationFactory<Program> factory = Open(SampleRules.Default(), new ManualTimeProvider(Now));
        using HttpClient client = ClientOf(factory);

        string html = await GetAsync(client, order.Id);

        Assert.DoesNotContain("id=\"customer-cancel\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"admin-cancel\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("Iptal et", html, StringComparison.Ordinal);
    }

    // ---- KK7: the last day of the interval is the table's last day ----

    [Fact]
    public async Task Get_TheWithdrawalOnItsLastDay_IsOpenAndTheLastDayIsWritten_TheDayAfterItIsNot_KK6_KK7()
    {
        ManualTimeProvider clock = new(Now);
        DateOnly today = TurkeyTime.DateOf(clock.GetUtcNow());
        DateOnly lastDelivery = today.AddDays(-SampleWithdrawRule.WindowDays);
        (Order open, _) = await SeedAsync(2, OrderStatus.Delivered, PaymentStatus.Paid, lastDelivery);
        (Order closed, _) = await SeedAsync(2, OrderStatus.Delivered, PaymentStatus.Paid, lastDelivery.AddDays(-1));
        await using WebApplicationFactory<Program> factory = Open(SampleRules.Default(), clock);
        using HttpClient client = ClientOf(factory);

        string openHtml = await GetAsync(client, open.Id);
        string closedHtml = await GetAsync(client, closed.Id);

        Assert.Contains("id=\"customer-withdraw\"", openHtml, StringComparison.Ordinal);
        Assert.Equal(DisplayFormat.Date(TurkeyTime.LastDayOfPeriod(lastDelivery, SampleWithdrawRule.WindowDays)), TextOf(openHtml, "last-day"));
        Assert.Equal(DisplayFormat.Date(today), TextOf(openHtml, "last-day"));
        Assert.DoesNotContain("id=\"customer-withdraw\"", closedHtml, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Post_TheWithdrawalOnTheLastDayIsApplied_AndOnTheDayAfterIsRefused_KK7()
    {
        ManualTimeProvider clock = new(Now);
        DateOnly today = TurkeyTime.DateOf(clock.GetUtcNow());
        (Order onLastDay, OrderItem lastItem) = await SeedAsync(1, OrderStatus.Delivered, PaymentStatus.Paid, today.AddDays(-SampleWithdrawRule.WindowDays));
        (Order afterIt, OrderItem lateItem) = await SeedAsync(1, OrderStatus.Delivered, PaymentStatus.Paid, today.AddDays(-SampleWithdrawRule.WindowDays - 1));
        await using WebApplicationFactory<Program> factory = Open(SampleRules.Default(), clock);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage applied = await PostAsync(client, onLastDay.Id, "CustomerWithdraw", ("Input.Quantity", "1"));
        using HttpResponseMessage refused = await PostAsync(client, afterIt.Id, "CustomerWithdraw", ("Input.Quantity", "1"));

        Assert.Equal(HttpStatusCode.SeeOther, applied.StatusCode);
        Assert.Equal(OrderPage(onLastDay.Id), applied.Headers.Location?.OriginalString);
        Assert.Equal(1, await CancellationCountAsync(lastItem.Id));
        Assert.Equal(HttpStatusCode.SeeOther, refused.StatusCode);
        Assert.Equal(NoticeAddress(afterIt.Id, NoticeCode.OperationClosed), refused.Headers.Location?.OriginalString);
        Assert.Equal(0, await CancellationCountAsync(lateItem.Id));
    }

    // ---- KK3, KK4, KK5: a request for a closed operation is not applied and writes nothing ----

    [Fact]
    public async Task Post_ByHandForAClosedOperation_WhoseButtonWasNeverShown_IsNotApplied_303WithTheReason_AndTheDatabaseIsUnchanged_KK3_KK4_KK5()
    {
        (Order order, OrderItem item) = await SeedAsync(2, OrderStatus.Shipped, PaymentStatus.Paid, null);
        await using WebApplicationFactory<Program> factory = Open(SampleRules.Default(), new ManualTimeProvider(Now));
        using HttpClient client = ClientOf(factory);
        string page = await GetAsync(client, order.Id);
        Assert.DoesNotContain("id=\"customer-cancel\"", page, StringComparison.Ordinal);

        using HttpResponseMessage response = await PostAsync(client, order.Id, "CustomerCancel", ("Input.Quantity", "1"));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(NoticeAddress(order.Id, NoticeCode.OperationClosed), response.Headers.Location?.OriginalString);
        Assert.Equal(0, await CancellationCountAsync(item.Id));
        Assert.Equal(order.Version, (await ReadOrderAsync(order.Id)).Version);
    }

    [Fact]
    public async Task Post_AQuantityAboveTheOpenQuantity_IsRefusedWhole_303WithTheReason_AndNothingIsWritten_KK5()
    {
        (Order order, OrderItem item) = await SeedAsync(2, OrderStatus.Preparing, PaymentStatus.Paid, null);
        await using WebApplicationFactory<Program> factory = Open(SampleRules.Default(), new ManualTimeProvider(Now));
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await PostAsync(client, order.Id, "CustomerCancel", ("Input.Quantity", "3"));

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(NoticeAddress(order.Id, NoticeCode.QuantityExceedsOpen), response.Headers.Location?.OriginalString);
        Assert.Equal(0, await CancellationCountAsync(item.Id));
        Assert.Equal(order.Version, (await ReadOrderAsync(order.Id)).Version);
    }

    [Fact]
    public async Task Post_AnOpenOperation_IsAppliedUnderTheLock_TheOrderVersionRisesByOne_ThenTheSecondSendFindsItClosed_KK3()
    {
        (Order order, OrderItem item) = await SeedAsync(1, OrderStatus.Preparing, PaymentStatus.Paid, null);
        await using WebApplicationFactory<Program> factory = Open(SampleRules.Default(), new ManualTimeProvider(Now));
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage first = await PostAsync(client, order.Id, "CustomerCancel", ("Input.Quantity", "1"));
        using HttpResponseMessage second = await PostAsync(client, order.Id, "CustomerCancel", ("Input.Quantity", "1"));

        Assert.Equal(OrderPage(order.Id), first.Headers.Location?.OriginalString);
        Assert.Equal(NoticeAddress(order.Id, NoticeCode.OperationClosed), second.Headers.Location?.OriginalString);
        Assert.Equal(1, await CancellationCountAsync(item.Id));
        Assert.Equal(order.Version + 1, (await ReadOrderAsync(order.Id)).Version);
    }

    [Fact]
    public async Task Post_ThePanelsOperation_WithTheVersionEqualAndTheOperationClosed_Is422WithTheScreensMessage_WithAnOldVersionIs409_AndWritesNothing_KK3_KK4_KK5()
    {
        (Order order, OrderItem item) = await SeedAsync(1, OrderStatus.Shipped, PaymentStatus.Paid, null);
        await using WebApplicationFactory<Program> factory = Open(SampleRules.Default(), new ManualTimeProvider(Now));
        using HttpClient client = ClientOf(factory);
        string version = order.Version.ToString(System.Globalization.CultureInfo.InvariantCulture);

        using HttpResponseMessage closed = await PostAsync(client, order.Id, "AdminCancel", ("Input.Version", version), ("Input.Quantity", "1"));
        string closedHtml = WebUtility.HtmlDecode(await closed.Content.ReadAsStringAsync(Ct));
        using HttpResponseMessage stale = await PostAsync(client, order.Id, "AdminCancel", ("Input.Version", (order.Version + 5).ToString(System.Globalization.CultureInfo.InvariantCulture)), ("Input.Quantity", "1"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed.StatusCode);
        Assert.Contains(OperationSampleModel.AdminClosedMessage, closedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
        Assert.Equal(0, await CancellationCountAsync(item.Id));
        Assert.Equal(order.Version, (await ReadOrderAsync(order.Id)).Version);
    }

    [Fact]
    public async Task Post_ThePanelsOperation_WhenOpenAndTheVersionIsTheShownOne_IsApplied_AndTheOrderChangedInBetweenIsTheConflict_KK3()
    {
        (Order order, OrderItem item) = await SeedAsync(1, OrderStatus.Preparing, PaymentStatus.Paid, null);
        await using WebApplicationFactory<Program> factory = Open(SampleRules.Default(), new ManualTimeProvider(Now));
        using HttpClient client = ClientOf(factory);
        string version = order.Version.ToString(System.Globalization.CultureInfo.InvariantCulture);

        using HttpResponseMessage applied = await PostAsync(client, order.Id, "AdminCancel", ("Input.Version", version), ("Input.Quantity", "1"));
        using HttpResponseMessage again = await PostAsync(client, order.Id, "AdminCancel", ("Input.Version", version), ("Input.Quantity", "1"));

        Assert.Equal(HttpStatusCode.SeeOther, applied.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, again.StatusCode);
        Assert.Equal(1, await CancellationCountAsync(item.Id));
    }

    // ---- KK8: the GET and the POST call the same rule ----

    [Fact]
    public async Task TheGetAndThePost_CallTheSameRuleInstance_KK8()
    {
        (Order order, _) = await SeedAsync(1, OrderStatus.Preparing, PaymentStatus.Paid, null);
        RecordingRule cancel = new(new SampleCancelRule());
        RecordingRule withdraw = new(new SampleWithdrawRule());
        await using WebApplicationFactory<Program> factory = Open(new SampleRules(cancel, withdraw), new ManualTimeProvider(Now));
        using HttpClient client = ClientOf(factory);

        await GetAsync(client, order.Id);
        int afterGet = cancel.Calls;
        using HttpResponseMessage post = await PostAsync(client, order.Id, "CustomerCancel", ("Input.Quantity", "1"));

        Assert.Equal(1, afterGet);
        Assert.Equal(1, withdraw.Calls);
        Assert.Equal(afterGet + 1, cancel.Calls);
        Assert.Equal(HttpStatusCode.SeeOther, post.StatusCode);
        Assert.All(cancel.Contexts, state => Assert.Equal(OrderStatus.Preparing, state.OrderStatus));
    }

    // ---- helpers ----

    private static string OrderPage(Guid id) => "/panel/islem-ornegi/" + id;

    private static string NoticeAddress(Guid id, NoticeCode code) => $"{OrderPage(id)}?{NoticeParameter.ParameterName}={code}";

    private static string? TextOf(string html, string name)
    {
        foreach (Match match in ClassedText().Matches(html))
        {
            if (match.Groups["name"].Value == name)
            {
                return WebUtility.HtmlDecode(match.Groups["text"].Value);
            }
        }

        return null;
    }

    private WebApplicationFactory<Program> Open(SampleRules rules, TimeProvider clock)
    {
        ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        return factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddSingleton(clock);
            s.AddSingleton(rules);
            s.AddScoped<ISampleOrderOperations, SampleOrderOperations>();
            s.AddRazorPages().AddApplicationPart(typeof(OperationSampleModel).Assembly);
        }));
    }

    private HttpClient ClientOf(WebApplicationFactory<Program> factory)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add("Cookie", $"{CookieNames.AdminSession}={_adminKey}");
        return client;
    }

    private static async Task<string> GetAsync(HttpClient client, Guid id)
    {
        using HttpResponseMessage response = await client.GetAsync(OrderPage(id), Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return await response.Content.ReadAsStringAsync(Ct);
    }

    private static Task<HttpResponseMessage> PostAsync(HttpClient client, Guid id, string handler, params (string Name, string Value)[] fields) =>
        client.PostAsync($"{OrderPage(id)}?handler={handler}", new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))), Ct);

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<Order> ReadOrderAsync(Guid id)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == id, Ct);
    }

    private async Task<int> CancellationCountAsync(Guid itemId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Cancellations.CountAsync(c => c.OrderItemId == itemId, Ct);
    }

    private async Task<(Order Order, OrderItem Item)> SeedAsync(int quantity, OrderStatus status, PaymentStatus payment, DateOnly? deliveryDate)
    {
        int n = Interlocked.Increment(ref _counter);
        await using AppDbContext db = NewContext();
        Province province = await db.Provinces.AsNoTracking().FirstOrDefaultAsync(Ct) ?? Province.Create("06", "Ankara", Now);
        bool provinceExists = await db.Provinces.AnyAsync(p => p.Id == province.Id, Ct);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Now, "metin", Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Now, "metin", Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Now, "metin", Now).WithHosting();
        Order order = Order.Create(
            $"2026-9{n:D5}", $"op-access-placeholder-{n}", $"op-confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Now.AddHours(1), Now);
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
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Physical, "Ürün", quantity, 100m, 20m, 16.67m, Now);
        item.DeliveryDate = deliveryDate;
        item.DeliveryMarkedAt = deliveryDate is null ? null : Now;
        if (!provinceExists)
        {
            db.Provinces.Add(province);
        }

        db.LegalTextVersions.AddRange(form, contract, notice);
        db.Orders.Add(order);
        db.OrderItems.Add(item);
        db.Entry(order).Property(o => o.OrderStatus).CurrentValue = status;
        db.Entry(order).Property(o => o.PaymentStatus).CurrentValue = payment;
        await db.SaveChangesAsync(Ct);
        return (order, item);
    }

    /// <summary>A rule that counts its calls and keeps the contexts it was asked about, over the real one.</summary>
    private sealed class RecordingRule : IOperationRule<SampleItemState>
    {
        private readonly IOperationRule<SampleItemState> _inner;
        private readonly List<SampleItemState> _contexts = [];

        public RecordingRule(IOperationRule<SampleItemState> inner)
        {
            _inner = inner;
        }

        public int Calls => _contexts.Count;

        public IReadOnlyList<SampleItemState> Contexts => _contexts;

        public OperationAvailability Evaluate(SampleItemState context)
        {
            _contexts.Add(context);
            return _inner.Evaluate(context);
        }
    }
}
