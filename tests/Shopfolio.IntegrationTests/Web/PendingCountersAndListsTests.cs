using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

using RecordAmounts = Shopfolio.Modules.AfterSales.ItemAmounts;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the pending work counters' tests (T604 A6; D-1798): one database copied from the template, so the six counters count exactly what the test seeds.</summary>
[CollectionDefinition(PendingCountersAndListsTestsDefinition.Name)]
public sealed class PendingCountersAndListsTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PendingCountersAndLists";
}

/// <summary>
/// VAL-A258 ("her sayaç, sayısıyla eşleşen kayıtlar listesini açar"; 04 2.6.1; T521, T527, T542; D-1798): the six counters of the panel home are drawn with different counts, and each counter's own link opens a list whose
/// rows are exactly the records the counter counted - the count equals the number of rows and the rows are the seeded records of that work, not those of another counter. Every counter has its own count (1, 2, 3, 2, 2, 1), so a
/// counter that opens another counter's list, or a list that holds more or less than its count, fails. The open request counter's defect claims are the module test's (<c>PanelHomeTests</c>); the list of requests is read here
/// by its contact requests.
/// </summary>
[Collection(PendingCountersAndListsTestsDefinition.Name)]
public sealed class PendingCountersAndListsTests
{
    private const string HomePath = "/panel";

    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private readonly CollectionDatabaseFixture _database;

    public PendingCountersAndListsTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task EachOfTheSixCounters_EqualsTheRowsOfTheListItsLinkOpens_AndTheRowsAreTheSeededRecordsOfThatWork_D1798_VALA258()
    {
        (string key, Guid adminId) = await TestSessions.AdminAsync(_database.Dsn);
        DateTimeOffset now = DateTimeOffset.UtcNow;

        // Ödeme onayı bekleyen: one transfer order awaiting its payment (the default order).
        (_, string payment) = await OrderAsync(null, OrderStatus.Placed, PaymentStatus.Pending);

        // Kargoya verilecek: a preparing paid order with an open physical item and a delivery failed one (K-590).
        (_, string preparing) = await OrderAsync(ProductType.Physical, OrderStatus.Preparing, PaymentStatus.Paid);
        (_, string failed) = await OrderAsync(null, OrderStatus.DeliveryFailed, PaymentStatus.Paid);

        // Teslim işareti bekleyen: three shipped paid orders with an open physical item.
        List<string> shipped = [];
        for (int i = 0; i < 3; i++)
        {
            shipped.Add((await OrderAsync(ProductType.Physical, OrderStatus.Shipped, PaymentStatus.Paid)).Number);
        }

        // Tamamlanmayı bekleyen hizmet: two paid orders with a service item nobody marked.
        List<string> services = [];
        for (int i = 0; i < 2; i++)
        {
            services.Add((await OrderAsync(ProductType.Service, OrderStatus.Preparing, PaymentStatus.Paid)).Number);
        }

        // İade ve geri ödeme bekleyen: a paid order whose only item was cancelled; the transfer refund of the cancellation is born and not sent.
        (Guid refundOrderId, string refundOrder) = await OrderAsync(ProductType.Physical, OrderStatus.Preparing, PaymentStatus.Paid);
        await using (AppDbContext db = NewContext())
        {
            Guid itemId = await db.OrderItems.Where(i => i.OrderId == refundOrderId).Select(i => i.Id).SingleAsync(Ct);
            Cancellation cancellation = Cancellation.ByBuyer(itemId, 1, new RecordAmounts(100m, 0m, 20m), null, now);
            db.Cancellations.Add(cancellation);
            Refund refund = Refund.ByAdmin(refundOrderId, RefundType.Cancellation, PaymentMethod.BankTransfer, 100m, adminId, "Yönetici Bir", now);
            refund.CancellationId = cancellation.Id;
            refund.OrderItemId = itemId;
            refund.Quantity = 1;
            db.Refunds.Add(refund);
            await db.SaveChangesAsync(Ct);
        }

        // Açık talep: two open contact requests.
        List<Guid> requests = [];
        await using (AppDbContext db = NewContext())
        {
            for (int i = 0; i < 2; i++)
            {
                ContactRequest request = ContactRequest.Create("Ayşe", $"ayse{i}@example.com", ContactTopic.GeneralQuestion, "Açık talep", now.AddMinutes(-i));
                db.ContactRequests.Add(request);
                requests.Add(request.Id);
            }

            await db.SaveChangesAsync(Ct);
        }

        Dictionary<string, HashSet<string>> expected = new(StringComparer.Ordinal)
        {
            ["/panel/siparisler?pendingWork=AwaitingPayment"] = [payment],
            ["/panel/siparisler?pendingWork=AwaitingShipment"] = [preparing, failed],
            ["/panel/siparisler?pendingWork=AwaitingDeliveryMark"] = [.. shipped],
            ["/panel/siparisler?pendingWork=AwaitingServiceCompletion"] = [.. services],
            ["/panel/talepler?requestStatus=Open"] = [.. requests.Select(r => r.ToString("D"))],
            ["/panel/siparisler?pendingWork=AwaitingReturnOrRefund"] = [refundOrder],
        };

        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        string home = await GetAsync(client, HomePath, key);
        MatchCollection counters = Regex.Matches(home, @"<a class=""pending-counters__link"" href=""([^""]+)"">\s*<span class=""pending-counters__count"">([\d\.]+)</span>", RegexOptions.CultureInvariant, PatternTimeout);

        Assert.Equal(6, counters.Count);
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), counters.Select(m => m.Groups[1].Value).Order(StringComparer.Ordinal));
        foreach (Match counter in counters)
        {
            string link = counter.Groups[1].Value;
            int count = int.Parse(counter.Groups[2].Value.Replace(".", string.Empty, StringComparison.Ordinal), CultureInfo.InvariantCulture);
            string list = await GetAsync(client, link, key);
            bool ofRequests = link.StartsWith("/panel/talepler", StringComparison.Ordinal);
            List<string> rows = ofRequests
                ? [.. Regex.Matches(list, @"href=""/panel/talepler/([0-9a-f-]{36})", RegexOptions.CultureInvariant, PatternTimeout).Select(m => m.Groups[1].Value).Distinct()]
                : [.. Regex.Matches(list, @"2026-[0-9A-F]{8}", RegexOptions.CultureInvariant, PatternTimeout).Select(m => m.Value).Distinct()];

            Assert.True(expected[link].Count == count, $"The counter of {link} says {count}; the seed holds {expected[link].Count}.");
            Assert.True(rows.Count == count, $"The list of {link} has {rows.Count} rows; the counter says {count}.");
            Assert.True(expected[link].SetEquals(rows), $"The rows of {link} ({string.Join(", ", rows)}) are not the seeded records ({string.Join(", ", expected[link])}).");
        }
    }

    /// <summary>A guest order in the given states with one open item of the type (none when the type is null); returns its id and its number.</summary>
    private async Task<(Guid Id, string Number)> OrderAsync(ProductType? type, OrderStatus shipping, PaymentStatus payment)
    {
        (Guid id, string number, _) = await TestSessions.OrderAsync(_database.Dsn, null);
        await using AppDbContext db = NewContext();
        if (type is { } itemType)
        {
            OrderItem item = OrderItem.Create(id, null, null, itemType, "Ürün", 1, 100m, 20m, 16.67m, DateTimeOffset.UtcNow);
            item.FulfilmentPeriod = itemType == ProductType.Service ? 5 : null;
            db.OrderItems.Add(item);
            await db.SaveChangesAsync(Ct);
        }

        string shippingName = shipping.ToString();
        string paymentName = payment.ToString();
        await db.Database.ExecuteSqlAsync($"UPDATE orders SET order_status = {shippingName}, payment_status = {paymentName} WHERE id = {id}", Ct);
        return (id, number);
    }

    private static async Task<string> GetAsync(HttpClient client, string path, string key)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.Add("Cookie", $"{CookieNames.AdminSession}={key}");
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));
    }

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn, withJobRunner: false).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(Pages.PanelOrdersModel).Assembly)));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }
}
