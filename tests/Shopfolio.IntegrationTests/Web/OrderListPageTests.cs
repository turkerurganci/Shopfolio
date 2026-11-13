using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
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
/// E-36, the panel's order list (T527 KK1, KK2, KK4; 07 UC-91; 04 9.8) on the opened application: the whole-value search by number and e-mail, the two status filters, a timed pending work filter and its
/// least-time-left order, an invalid value that filters nothing, the newest-first order and its page boundary on equal creation moments, the system-cancelled unpaid order, and the row's marks and period. Each test gives its orders an e-mail of its own
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
    public async Task ATimedPendingWorkFilter_KeepsOnlyItsOrders_OverrunFirstThenLeastTimeLeft_AndAnOrderWithoutAPeriodLast_KK1_KK2()
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        string email = UniqueEmail();
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string running = await PreparingOrderAsync(email, now.AddHours(-3), approvedAt: now.AddHours(-1));
        string overrun = await PreparingOrderAsync(email, now.AddHours(-2), approvedAt: now.AddDays(-30));
        string unapproved = await PreparingOrderAsync(email, now.AddHours(-1), approvedAt: null);
        (Guid cancelledId, string cancelled) = await OrderAsync(email, now);
        await SetStatusAsync(cancelledId, OrderStatus.Cancelled, PaymentStatus.Failed);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        string all = await ListAsync(client, $"search={Uri.EscapeDataString(email)}", key);
        string timed = await ListAsync(client, $"search={Uri.EscapeDataString(email)}&pendingWork={nameof(PanelOrderWork.AwaitingShipment)}", key);

        // Without the filter the list is newest first; under the timed "Kargoya verilecek" filter (04 9.8.7) the overrun period heads it, then the least time left, and the order with no running
        // period comes last; the cancelled order is not in the filter's set.
        Assert.True(all.IndexOf(cancelled, StringComparison.Ordinal) < all.IndexOf(unapproved, StringComparison.Ordinal));
        Assert.True(all.IndexOf(unapproved, StringComparison.Ordinal) < all.IndexOf(overrun, StringComparison.Ordinal));
        Assert.True(all.IndexOf(overrun, StringComparison.Ordinal) < all.IndexOf(running, StringComparison.Ordinal));
        Assert.DoesNotContain(cancelled, timed, StringComparison.Ordinal);
        int overrunAt = timed.IndexOf(overrun, StringComparison.Ordinal);
        int runningAt = timed.IndexOf(running, StringComparison.Ordinal);
        int unapprovedAt = timed.IndexOf(unapproved, StringComparison.Ordinal);
        Assert.True(overrunAt >= 0 && overrunAt < runningAt && runningAt < unapprovedAt);
    }

    [Fact]
    public async Task ThePaymentDeliveryIbanAndEmailFilters_EachKeepOnlyTheirOwnOrder_KK1()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        string email = UniqueEmail();
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // One order per filter, each outside the other three: a transfer awaiting its payment; a shipped order with an open physical item; a delivered order whose item has an open IBAN
        // request (K-621); a cancelled order with an undelivered e-mail.
        (_, string awaiting) = await OrderAsync(email, now.AddMinutes(-4));
        (Guid shippedId, string shipped) = await OrderAsync(email, now.AddMinutes(-3));
        (Guid ibanId, string iban) = await OrderAsync(email, now.AddMinutes(-2));
        (Guid emailId, string undelivered) = await OrderAsync(email, now.AddMinutes(-1));
        await using (AppDbContext db = NewContext())
        {
            db.OrderItems.Add(OrderItem.Create(shippedId, null, null, ProductType.Physical, "Ürün", 1, 100m, 20m, 16.67m, now));
            OrderItem ibanItem = OrderItem.Create(ibanId, null, null, ProductType.Physical, "Ürün", 1, 100m, 20m, 16.67m, now);
            ibanItem.DeliveryMarkedAt = now;
            ibanItem.DeliveryDate = TurkeyTime.DateOf(now);
            db.OrderItems.Add(ibanItem);
            db.RefundIbanRequests.Add(RefundIbanRequest.ForItem(ibanItem.Id, RefundIbanRequestReason.AdminRequest, null, adminId, "Yönetici Bir", now));
            DispatchRecord row = DispatchRecord.Queue(DispatchType.B1, email, emailId, null, null, now);
            row.DispatchState = DispatchState.Undelivered;
            row.LastOutcome = DispatchOutcome.RecipientError;
            row.AttemptCount = 3;
            db.DispatchRecords.Add(row);
            await db.SaveChangesAsync(Ct);
        }

        await SetStatusAsync(shippedId, OrderStatus.Shipped, PaymentStatus.Paid);
        await SetStatusAsync(ibanId, OrderStatus.Delivered, PaymentStatus.Paid);
        await SetStatusAsync(emailId, OrderStatus.Cancelled, PaymentStatus.Failed);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string[] numbers = [awaiting, shipped, iban, undelivered];
        (PanelOrderWork Work, string Number)[] filters =
        [
            (PanelOrderWork.AwaitingPayment, awaiting),
            (PanelOrderWork.AwaitingDeliveryMark, shipped),
            (PanelOrderWork.IbanPendingMark, iban),
            (PanelOrderWork.EmailUndeliveredMark, undelivered),
        ];

        foreach ((PanelOrderWork work, string number) in filters)
        {
            string html = await ListAsync(client, $"search={Uri.EscapeDataString(email)}&pendingWork={work}", key);

            Assert.Equal([number], numbers.Where(n => html.Contains(n, StringComparison.Ordinal)));
        }
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

    // ---- no query in a loop: the list sends the same number of commands for one order and for many (09 20 item 4, 25) ----

    [Fact]
    public async Task TheList_SendsTheSameNumberOfCommands_ForOneOrderAndForFour_KK1_KK4()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        DateTimeOffset now = DateTimeOffset.UtcNow;
        string fewEmail = UniqueEmail();
        string manyEmail = UniqueEmail();
        string one = await PreparingOrderAsync(fewEmail, now.AddHours(-1), approvedAt: now.AddHours(-1));
        List<string> four = [];
        for (int i = 0; i < 4; i++)
        {
            four.Add(await PreparingOrderAsync(manyEmail, now.AddHours(-1).AddMinutes(i), approvedAt: now.AddHours(-1)));
        }

        // Each of the four carries a mark the row reads (the corrected e-mail), so every per-row read has rows to find.
        await using (AppDbContext db = NewContext())
        {
            List<Guid> ids = await db.Orders.Where(o => four.Contains(o.OrderNumber)).Select(o => o.Id).ToListAsync(Ct);
            db.OrderEmailHistories.AddRange(ids.Select(id => OrderEmailHistory.Create(id, "eski@example.com", manyEmail, adminId, "Yönetici Bir", now)));
            await db.SaveChangesAsync(Ct);
        }

        StorefrontSearchAndProductTests.CommandCounter counter = new();
        await using WebApplicationFactory<Program> factory = Open(counter);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        await ListAsync(client, $"search={Uri.EscapeDataString(fewEmail)}", key);
        int beforeFew = counter.Count;
        string fewHtml = await ListAsync(client, $"search={Uri.EscapeDataString(fewEmail)}", key);
        int fewCommands = counter.Count - beforeFew;
        int beforeMany = counter.Count;
        string manyHtml = await ListAsync(client, $"search={Uri.EscapeDataString(manyEmail)}", key);
        int manyCommands = counter.Count - beforeMany;

        Assert.Contains(one, fewHtml, StringComparison.Ordinal);
        Assert.All(four, n => Assert.Contains(n, manyHtml, StringComparison.Ordinal));
        Assert.Contains(PanelMarks.TextOf(PanelMark.EmailCorrected), manyHtml, StringComparison.Ordinal);
        Assert.True(fewCommands > 0);
        Assert.Equal(fewCommands, manyCommands);
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

    /// <summary>A preparing paid order with one physical item and a three business day shipping promise; its payment approved (an Ö1 record) at <paramref name="approvedAt"/> or not at all.</summary>
    private async Task<string> PreparingOrderAsync(string email, DateTimeOffset createdAt, DateTimeOffset? approvedAt)
    {
        (Guid orderId, string number) = await OrderAsync(email, createdAt);
        await using AppDbContext db = NewContext();
        db.OrderItems.Add(OrderItem.Create(orderId, null, null, ProductType.Physical, "Ürün", 1, 100m, 20m, 16.67m, createdAt));
        if (approvedAt is { } at)
        {
            db.TransitionRecords.Add(TransitionRecord.Create(orderId, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, at));
        }

        await db.SaveChangesAsync(Ct);
        await db.Orders.Where(o => o.Id == orderId).ExecuteUpdateAsync(s => s
            .SetProperty(o => o.OrderStatus, OrderStatus.Preparing)
            .SetProperty(o => o.PaymentStatus, PaymentStatus.Paid)
            .SetProperty(o => o.ShippingPeriod, 3), Ct);
        return number;
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
