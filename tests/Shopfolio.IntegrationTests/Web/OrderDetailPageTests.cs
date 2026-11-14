using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the order screen's tests (T527): one database copied from the template.</summary>
[CollectionDefinition(OrderDetailPageTestsDefinition.Name)]
public sealed class OrderDetailPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "OrderDetailPageTests";
}

/// <summary>
/// E-37, the order's screen in the panel (T527 KK3, KK5, KK6, KK8-KK10; 07 UC-92, UC-123...UC-128) on the opened application: the primary button, the foreign key's 404, the confirmation's 200, the
/// version's 409, the closed step's 422 and the audit row of a done step. Orders, accounts and sessions are real rows of the collection database.
/// </summary>
[Collection(OrderDetailPageTestsDefinition.Name)]
public sealed partial class OrderDetailPageTests
{
    private const string OrdersPath = "/panel/siparisler";

    private readonly CollectionDatabaseFixture _database;

    public OrderDetailPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task TheScreen_ShowsTheOrderAndItsNextStepAsThePrimaryButton_AndAForeignKeyIsNotFound_KK3()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, string number, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}?returnUrl={Uri.EscapeDataString(OrdersPath + "?page=2")}", key);
        (HttpStatusCode foreign, _) = await GetAsync(client, $"{OrdersPath}/{Guid.NewGuid():D}", key);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains($"<h1>{number}</h1>", html, StringComparison.Ordinal);
        Assert.Contains("id=\"mark-paid-form\"", html, StringComparison.Ordinal);
        Assert.Contains($">{OrderDetailModel.MarkPaidLabel}</button>", html, StringComparison.Ordinal);
        Assert.Contains("TR00 0000 0000 0000 0000 0000 00", html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{OrdersPath}?page=2\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"mark-shipped-form\"", html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.NotFound, foreign);
    }

    [Fact]
    public async Task TheScreen_ShowsThePanelOnlyParts_TheInternalNoteAndTheEmailHistory_FromTheServer_KK4()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using (AppDbContext db = NewContext())
        {
            db.InternalNotes.Add(InternalNote.Create(orderId, "Müşteri öğleden sonra aranacak", adminId, "Yönetici Bir", DateTimeOffset.UtcNow));
            db.OrderEmailHistories.Add(OrderEmailHistory.Create(orderId, "eski@example.com", "yeni@example.com", adminId, "Yönetici Bir", DateTimeOffset.UtcNow));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (HttpStatusCode status, string html) = await GetAsync(client, $"{OrdersPath}/{orderId:D}", key);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains(OrderDetailModel.InternalNoteHiddenText, html, StringComparison.Ordinal);
        Assert.Contains("Müşteri öğleden sonra aranacak", html, StringComparison.Ordinal);
        Assert.Contains("eski@example.com", html, StringComparison.Ordinal);
        Assert.Contains("yeni@example.com", html, StringComparison.Ordinal);
        // The corrected e-mail puts its mark on the order (04 9.9.4; 03 1.7.3.6): read from the history, never stored.
        Assert.Contains(PanelMarks.TextOf(PanelMark.EmailCorrected), html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task MarkPaid_Unconfirmed_Is200WithTheFrozenIbanAndTheAmount_AndWritesNothing_KK5()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkPaid", [("Input.Version", "1")], key);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("data-js=\"confirm-window\"", html, StringComparison.Ordinal);
        Assert.Contains("IBAN: TR00 0000 0000 0000 0000 0000 00", html, StringComparison.Ordinal);
        Assert.Contains("Bu işlem geri alınamaz.", html, StringComparison.Ordinal);
        Assert.Contains("name=\"Input.Confirmed\" value=\"true\"", html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal(PaymentStatus.Pending, order.PaymentStatus);
        Assert.Equal(1, order.Version);
    }

    [Fact]
    public async Task MarkPaid_Confirmed_Is303_MovesTheOrder_AndWritesTheAuditRow_KK5_KK10()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string? location) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkPaid", [("Input.Version", "1"), ("Input.Confirmed", "true")], key);

        Assert.Equal(HttpStatusCode.SeeOther, status);
        Assert.Equal($"{OrdersPath}/{orderId:D}", location);
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId && a.Operation == nameof(AuditOperation.MarkBankTransferPaid), Ct));
    }

    [Fact]
    public async Task MarkPaid_Confirmed_CommitsTheReservedUnitsAndTheCoupon_AndTakesOnlyTheOrderedVariantOutOfTheCart_KK5()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string run = Guid.NewGuid().ToString("N")[..8];
        Product product = Product.Create("Kapı", $"kapi-t527-{run}", ProductType.Physical, 20m, now).WithSeededOrigin();
        Variant ordered = Variant.Create(product.Id, $"KAPI-T527-{run}-A", 100m, now, 5);
        Product otherProduct = Product.Create("Pencere", $"pencere-t527-{run}", ProductType.Physical, 20m, now).WithSeededOrigin();
        Variant other = Variant.Create(otherProduct.Id, $"PENCERE-T527-{run}", 100m, now, 5);
        DateOnly today = TurkeyTime.DateOf(now);
        Coupon coupon = Coupon.CreatePercentage($"T527K{run}", 10m, today.AddDays(-1), today.AddDays(30), 10, now);
        Cart cart = Cart.ForGuest($"t527-cart-{run}", now);
        await using (AppDbContext seed = NewContext())
        {
            seed.Products.AddRange(product, otherProduct);
            seed.Variants.AddRange(ordered, other);
            seed.Coupons.Add(coupon);
            seed.Carts.Add(cart);
            seed.CartItems.AddRange(CartItem.Create(cart.Id, ordered.Id, 3, 100m, now), CartItem.Create(cart.Id, other.Id, 1, 100m, now));
            seed.OrderItems.Add(OrderItem.Create(orderId, product.Id, ordered.Id, ProductType.Physical, "Kapı", 2, 100m, 20m, 16.67m, now));
            seed.StockReservations.Add(StockReservation.ForVariant(orderId, ordered.Id, 2, now));
            seed.StockReservations.Add(StockReservation.ForCoupon(orderId, coupon.Id, now));
            await seed.SaveChangesAsync(Ct);
            await seed.Variants.Where(v => v.Id == ordered.Id).ExecuteUpdateAsync(s => s.SetProperty(v => v.ReservedQuantity, 2), Ct);
            await seed.Coupons.Where(c => c.Id == coupon.Id).ExecuteUpdateAsync(s => s.SetProperty(c => c.ReservedCount, 1), Ct);
            await seed.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(s => s.SetProperty(o => o.CartId, cart.Id), Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkPaid", [("Input.Version", "1"), ("Input.Confirmed", "true")], key);

        // Ö1's simultaneous transitions (03 1.4.4.2; 02 3.16.12), through the module's one path: the reserved units fall finally, the reserved right
        // becomes a used one, the reservation rows go, and the cart loses the ordered variant's line only.
        Assert.Equal(HttpStatusCode.SeeOther, status);
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid), (order.OrderStatus, order.PaymentStatus));
        Variant committed = await db.Variants.AsNoTracking().SingleAsync(v => v.Id == ordered.Id, Ct);
        Assert.Equal((3, 0), (committed.StockQuantity!.Value, committed.ReservedQuantity));
        Coupon used = await db.Coupons.AsNoTracking().SingleAsync(c => c.Id == coupon.Id, Ct);
        Assert.Equal((1, 0), (used.UsedCount, used.ReservedCount));
        Assert.False(await db.StockReservations.AnyAsync(r => r.OrderId == orderId, Ct));
        Assert.Equal([other.Id], await db.CartItems.Where(i => i.CartId == cart.Id).Select(i => i.VariantId).ToListAsync(Ct));
    }

    [Fact]
    public async Task ASecondSendingWithTheOldVersion_Is409_AndWritesNothing_KK9()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        (string Name, string Value)[] fields = [("Input.Version", "1"), ("Input.Confirmed", "true")];

        (HttpStatusCode first, _) = await PostForLocationAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkPaid", fields, key);
        (HttpStatusCode second, string html) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkPaid", fields, key);

        Assert.Equal(HttpStatusCode.SeeOther, first);
        Assert.Equal(HttpStatusCode.Conflict, second);
        Assert.Contains(ConflictControlsText, html, StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    [Fact]
    public async Task AStepThatIsNotOpen_Is422_AndTheShipmentWithoutItsCarrierData_Is422_KK6_KK8()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using ShopfolioWebApplicationFactory root = new(_database.Dsn);
        await using WebApplicationFactory<Program> factory = root.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly)));
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string run = Guid.NewGuid().ToString("N")[..8];

        (HttpStatusCode missing, string missingHtml) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkShipped", [("Input.Version", "1")], key, $"t527-missing-{run}");
        (HttpStatusCode closed, string closedHtml) = await PostAsync(
            client,
            $"{OrdersPath}/{orderId:D}?handler=MarkShipped",
            [("Input.Version", "1"), ("Input.IsOwnVehicleDelivery", "true")],
            key,
            $"t527-closed-{run}");
        (HttpStatusCode failed, _) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=MarkDeliveryFailed", [("Input.Version", "1")], key, $"t527-failed-{run}");
        (HttpStatusCode close, _) = await PostAsync(client, $"{OrdersPath}/{orderId:D}?handler=CloseDeliveryFailed", [("Input.Version", "1")], key);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, missing);
        Assert.Contains(OrderDetailModel.ShipmentMissingText, missingHtml, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(ErrorCode.ShipmentInfoMissing), missingHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed);
        Assert.Contains(OrderDetailModel.NotOpenText, closedHtml, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, failed);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, close);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));

        // Each refusal writes its one rejection line with the code of 07 6.2 (07 6.1.3; 09 8.2 item 3); the screen shows only the text.
        Assert.Equal(nameof(ErrorCode.ShipmentInfoMissing), CodeOfRejection(root.LogDirectory, $"t527-missing-{run}"));
        Assert.Equal(nameof(ErrorCode.OperationNotAvailable), CodeOfRejection(root.LogDirectory, $"t527-closed-{run}"));
        Assert.Equal(nameof(ErrorCode.OperationNotAvailable), CodeOfRejection(root.LogDirectory, $"t527-failed-{run}"));
    }

    private static string? CodeOfRejection(string logDirectory, string requestId)
    {
        List<JsonElement> lines = [];
        foreach (string path in Directory.GetFiles(logDirectory, "app-*.jsonl"))
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new(stream);
            while (reader.ReadLine() is { } text)
            {
                JsonElement line = JsonDocument.Parse(text).RootElement.Clone();
                if (line.TryGetProperty("requestId", out JsonElement id) && id.GetString() == requestId && line.TryGetProperty("code", out _))
                {
                    lines.Add(line);
                }
            }
        }

        return Assert.Single(lines).GetProperty("code").GetString();
    }

    // ---- no query in a loop: the screen sends the same number of commands for one item and for three (09 20 item 4, 25) ----

    [Fact]
    public async Task TheScreen_SendsTheSameNumberOfCommands_ForOneItemAndForThree_KK3_KK4()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid oneId, Guid[] oneItems) = await StepOrderAsync(OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, true));
        (Guid threeId, Guid[] threeItems) = await StepOrderAsync(
            OrderStatus.Delivered, PaymentStatus.Paid, (ProductType.Physical, true), (ProductType.Physical, true), (ProductType.Digital, true));
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await using (AppDbContext db = NewContext())
        {
            foreach (Guid orderId in new[] { oneId, threeId })
            {
                db.InternalNotes.Add(InternalNote.Create(orderId, "Not", adminId, "Yönetici Bir", now));
                db.TransitionRecords.Add(TransitionRecord.Create(orderId, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, now.AddDays(-2)));
            }

            db.OrderEmailHistories.Add(OrderEmailHistory.Create(oneId, "eski@example.com", "yeni@example.com", adminId, "Yönetici Bir", now));
            foreach (int i in Enumerable.Range(1, 3))
            {
                db.OrderEmailHistories.Add(OrderEmailHistory.Create(threeId, $"eski{i}@example.com", $"yeni{i}@example.com", adminId, "Yönetici Bir", now.AddMinutes(i)));
            }

            await db.SaveChangesAsync(Ct);
        }

        StorefrontSearchAndProductTests.CommandCounter counter = new();
        await using WebApplicationFactory<Program> factory = Open(counter);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        await GetAsync(client, $"{OrdersPath}/{oneId:D}", key);
        int beforeOne = counter.Count;
        (HttpStatusCode oneStatus, _) = await GetAsync(client, $"{OrdersPath}/{oneId:D}", key);
        int oneCommands = counter.Count - beforeOne;
        int beforeThree = counter.Count;
        (HttpStatusCode threeStatus, string threeHtml) = await GetAsync(client, $"{OrdersPath}/{threeId:D}", key);
        int threeCommands = counter.Count - beforeThree;

        Assert.Equal(HttpStatusCode.OK, oneStatus);
        Assert.Equal(HttpStatusCode.OK, threeStatus);
        Assert.Single(oneItems);
        Assert.Equal(3, threeItems.Length);
        Assert.Contains("yeni3@example.com", threeHtml, StringComparison.Ordinal);
        Assert.True(oneCommands > 0);
        Assert.Equal(oneCommands, threeCommands);
    }

    private static string ConflictControlsText => Shopfolio.Infrastructure.Forms.ConflictControls.OrderMessage;

    private static string AdminCookie(string key) => $"{CookieNames.AdminSession}={key}";

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path, string key)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.Add("Cookie", AdminCookie(key));
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<(HttpStatusCode Status, string Html)> PostAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, string key, string? requestId = null)
    {
        using HttpResponseMessage response = await SendPostAsync(client, path, fields, key, requestId);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<(HttpStatusCode Status, string? Location)> PostForLocationAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, string key)
    {
        using HttpResponseMessage response = await SendPostAsync(client, path, fields, key);
        return (response.StatusCode, response.Headers.Location?.OriginalString);
    }

    private static async Task<HttpResponseMessage> SendPostAsync(HttpClient client, string path, IEnumerable<(string Name, string Value)> fields, string key, string? requestId = null)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))) };
        request.Headers.Add("Cookie", AdminCookie(key));
        if (requestId is not null)
        {
            request.Headers.TryAddWithoutValidation("X-Request-Id", requestId);
        }

        return await client.SendAsync(request, Ct);
    }

    // The counter counts the commands the contexts send (the job runner is off then, so its polling is not counted).
    private WebApplicationFactory<Program> Open(StorefrontSearchAndProductTests.CommandCounter? counter = null) =>
        new ShopfolioWebApplicationFactory(_database.Dsn, withJobRunner: counter is null).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly);
            if (counter is not null)
            {
                s.ConfigureDbContext<AppDbContext>(options => options.AddInterceptors(counter));
            }
        }));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }
}
