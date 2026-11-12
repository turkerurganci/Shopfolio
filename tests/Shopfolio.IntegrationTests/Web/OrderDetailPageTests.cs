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

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly)));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }
}
