using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Lists;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the order list's tests (T527): one database copied from the template.</summary>
[CollectionDefinition(OrderListPageTestsDefinition.Name)]
public sealed class OrderListPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "OrderListPageTests";
}

/// <summary>
/// E-36, the panel's order list (T527 KK1, KK2, KK4; 07 UC-91; 04 9.8) on the opened application: the whole-value search by number and e-mail, the two status filters, an invalid value that filters
/// nothing, the newest-first order and its page boundary on equal creation moments, the system-cancelled unpaid order, and the row's marks and period. Each test gives its orders an e-mail of its own
/// and searches by it, so the tests of the collection do not see each other's orders.
/// </summary>
[Collection(OrderListPageTestsDefinition.Name)]
public sealed class OrderListPageTests
{
    private const string OrdersPath = "/panel/siparisler";

    private readonly CollectionDatabaseFixture _database;

    public OrderListPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task TheSearch_MatchesTheWholeNumberOrEmail_TheStatusFiltersApply_AndAnInvalidValueFiltersNothing_KK1()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        string email = UniqueEmail();
        (Guid placedId, string placed) = await OrderAsync(email, DateTimeOffset.UtcNow.AddMinutes(-2));
        (Guid cancelledId, string cancelled) = await OrderAsync(email, DateTimeOffset.UtcNow.AddMinutes(-1));
        await SetStatusAsync(cancelledId, OrderStatus.Cancelled, PaymentStatus.Failed);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        string byNumber = await ListAsync(client, $"search={Uri.EscapeDataString(placed.ToLowerInvariant())}", key);
        string byEmail = await ListAsync(client, $"search={Uri.EscapeDataString(email.ToUpperInvariant())}", key);
        string partial = await ListAsync(client, $"search={Uri.EscapeDataString(email[..^4])}", key);
        string filtered = await ListAsync(client, $"search={Uri.EscapeDataString(email)}&orderStatus=Cancelled&paymentStatus=Failed", key);
        string invalid = await ListAsync(client, $"search={Uri.EscapeDataString(email)}&orderStatus=Bogus&paymentStatus=1&pendingWork=Nothing", key);

        Assert.Contains(placed, byNumber, StringComparison.Ordinal);
        Assert.DoesNotContain(cancelled, byNumber, StringComparison.Ordinal);
        Assert.Contains(placed, byEmail, StringComparison.Ordinal);
        Assert.Contains(cancelled, byEmail, StringComparison.Ordinal);
        Assert.Contains(OrdersModel.NoMatchText, partial, StringComparison.Ordinal);
        Assert.Contains(cancelled, filtered, StringComparison.Ordinal);
        Assert.DoesNotContain(placed, filtered, StringComparison.Ordinal);
        Assert.Contains(placed, invalid, StringComparison.Ordinal);
        Assert.Contains(cancelled, invalid, StringComparison.Ordinal);
        Assert.NotEqual(Guid.Empty, placedId);
    }

    [Fact]
    public async Task TheList_IsNewestFirst_ShowsTheSystemCancelledUnpaidOrder_AndPagesWithoutLossOnEqualMoments_KK2()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        string email = UniqueEmail();
        DateTimeOffset same = DateTimeOffset.UtcNow.AddHours(-1);
        int pageSize = ListCatalog.PageSizeOf(ListKind.PanelOrders);
        List<string> numbers = [];
        for (int i = 0; i <= pageSize; i++)
        {
            numbers.Add((await OrderAsync(email, same)).Number);
        }

        (Guid newestId, string newest) = await OrderAsync(email, same.AddMinutes(5));
        await SetStatusAsync(newestId, OrderStatus.Cancelled, PaymentStatus.Failed);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        string first = await ListAsync(client, $"search={Uri.EscapeDataString(email)}&page=1", key);
        string second = await ListAsync(client, $"search={Uri.EscapeDataString(email)}&page=2", key);
        string past = await ListAsync(client, $"search={Uri.EscapeDataString(email)}&page=99", key);

        // The newest - the system-cancelled unpaid order (04 9.8.12) - heads the first page; the equal moments split over the two pages with no order lost or shown twice (09 10.5 item 1).
        int newestAt = first.IndexOf(newest, StringComparison.Ordinal);
        Assert.True(newestAt >= 0);
        Assert.All(numbers.Where(n => first.Contains(n, StringComparison.Ordinal)), n => Assert.True(first.IndexOf(n, StringComparison.Ordinal) > newestAt));
        string[] onFirst = [.. numbers.Where(n => first.Contains(n, StringComparison.Ordinal))];
        string[] onSecond = [.. numbers.Where(n => second.Contains(n, StringComparison.Ordinal))];
        Assert.Equal(pageSize - 1, onFirst.Length);
        Assert.Equal(numbers.Count - onFirst.Length, onSecond.Length);
        Assert.Empty(onFirst.Intersect(onSecond));
        Assert.Equal(onSecond.Order(), numbers.Where(n => past.Contains(n, StringComparison.Ordinal)).Order());
    }

    [Fact]
    public async Task TheRow_ShowsTheListMarks_AndTheRunningPeriodTheServerWorkedOut_KK4()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        string email = UniqueEmail();
        (Guid orderId, string number) = await OrderAsync(email, DateTimeOffset.UtcNow);
        await using (AppDbContext db = NewContext())
        {
            db.OrderEmailHistories.Add(OrderEmailHistory.Create(orderId, "eski@example.com", email, adminId, "Yönetici Bir", DateTimeOffset.UtcNow));
            await db.SaveChangesAsync(Ct);
        }

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string html = await ListAsync(client, $"search={Uri.EscapeDataString(email)}", key);

        Assert.Contains(number, html, StringComparison.Ordinal);
        Assert.Contains(PanelMarks.TextOf(PanelMark.EmailCorrected), html, StringComparison.Ordinal);
        Assert.Contains("Son ödeme günü", html, StringComparison.Ordinal);
    }

    private static string UniqueEmail() => $"liste-{Guid.NewGuid():N}@example.com";

    private async Task<(Guid OrderId, string Number)> OrderAsync(string email, DateTimeOffset createdAt)
    {
        (Guid orderId, string number, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using AppDbContext db = NewContext();
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, Ct);
        order.SetContactEmail(email);
        order.CreatedAt = createdAt;
        await db.SaveChangesAsync(Ct);
        return (orderId, number);
    }

    private async Task SetStatusAsync(Guid orderId, OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        await using AppDbContext db = NewContext();
        await db.Orders.Where(o => o.Id == orderId)
            .ExecuteUpdateAsync(s => s.SetProperty(o => o.OrderStatus, orderStatus).SetProperty(o => o.PaymentStatus, paymentStatus), Ct);
    }

    private static async Task<string> ListAsync(HttpClient client, string query, string key)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, $"{OrdersPath}?{query}");
        request.Headers.Add("Cookie", $"{CookieNames.AdminSession}={key}");
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));
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
