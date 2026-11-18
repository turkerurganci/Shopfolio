using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// The order's conflicting operations on the real screens (T604 D-1167; 12 VAL-C076, VAL-C077, VAL-C078; 02 3.31.2): the one that completes first holds, and the later
/// one - sent from a screen opened before the first - is not applied when the order changed. The order of the two is set by the test (the second one's form carries the
/// version it was opened at), never by waiting. The administrators and the customer send through their screens; the system's operation runs through the module's
/// transition path the way the job and the installer command run it.
/// </summary>
[Collection(FileEndpointsDefinition.Name)]
public sealed class ConcurrentOrderScreenTests
{
    private const string OrdersPath = "/panel/siparisler";

    private readonly CollectionDatabaseFixture _database;

    public ConcurrentOrderScreenTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // ---- VAL-C076: two administrators ----

    [Fact]
    public async Task TwoAdministrators_TheSecondOperationSentFromTheOlderScreen_IsNotApplied_AndTheCurrentStateIsShown_VALC076()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (string firstKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        (string secondKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, int opened) = await SeedAsync(host, null);

        // Both opened the order at its first version; the first one marks it paid, the second sends the same operation afterwards.
        (HttpStatusCode first, _) = await AdminPostAsync(host, firstKey, orderId, "MarkPaid", opened);
        (HttpStatusCode second, string html) = await AdminPostAsync(host, secondKey, orderId, "MarkPaid", opened);
        (HttpStatusCode current, string currentHtml) = await AdminGetAsync(host, secondKey, orderId);

        Assert.Equal(HttpStatusCode.SeeOther, first);
        Assert.Equal(HttpStatusCode.Conflict, second);
        Assert.Contains(ConflictControls.OrderMessage, html, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, current);
        Assert.Contains("id=\"mark-shipped-form\"", currentHtml, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"mark-paid-form\"", currentHtml, StringComparison.Ordinal);
        await using AppDbContext db = host.NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid), (order.OrderStatus, order.PaymentStatus));
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    // ---- VAL-C077: the administrator and the customer ----

    [Fact]
    public async Task AdministratorFirst_ShippingTheOrder_InvalidatesTheCustomersLaterCancellation_ItIsRefusedWithItsReason_VALC077()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, string accessKey, int opened) = await SeedAsync(host, PaymentStatus.Paid);
        string cancelScreen = OrderRoutes.ScreenPath(OrderRoutes.AccessPath(accessKey), "iptal");

        // The customer's cancellation screen was opened while the order was open; the administrator ships it, then the customer sends the cancellation.
        (HttpStatusCode shipped, _) = await AdminPostAsync(host, key, orderId, "MarkShipped", opened, ("Input.IsOwnVehicleDelivery", "true"));
        using HttpResponseMessage late = await CustomerPostAsync(host, cancelScreen, "CancelOrder", new KeyValuePair<string, string>("Confirmed", "true"));

        Assert.Equal(HttpStatusCode.SeeOther, shipped);
        Assert.Equal(HttpStatusCode.SeeOther, late.StatusCode);
        Assert.Equal(OrderRoutes.AccessPath(accessKey) + "?notice=OperationClosed", late.Headers.Location!.OriginalString);
        await using AppDbContext db = host.NewContext();
        Assert.Equal(OrderStatus.Shipped, (await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct)).OrderStatus);
        Assert.False(await db.Cancellations.AnyAsync(c => db.OrderItems.Any(i => i.Id == c.OrderItemId && i.OrderId == orderId), Ct));
    }

    [Fact]
    public async Task CustomerFirst_CancellingTheOrder_InvalidatesTheAdministratorsLaterOperation_Is409_AndNothingIsWritten_VALC077()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, string accessKey, int opened) = await SeedAsync(host, null);
        string cancelScreen = OrderRoutes.ScreenPath(OrderRoutes.AccessPath(accessKey), "iptal");

        using HttpResponseMessage cancelled = await CustomerPostAsync(host, cancelScreen, "CancelOrder", new KeyValuePair<string, string>("Confirmed", "true"));
        (HttpStatusCode late, string html) = await AdminPostAsync(host, key, orderId, "MarkPaid", opened);

        Assert.Equal(HttpStatusCode.SeeOther, cancelled.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, late);
        Assert.Contains(ConflictControls.OrderMessage, html, StringComparison.Ordinal);
        await using AppDbContext db = host.NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal(OrderStatus.Cancelled, order.OrderStatus);
        Assert.NotEqual(PaymentStatus.Paid, order.PaymentStatus);
        Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    [Fact]
    public async Task AdministratorFirst_MarkingPaid_TheCustomersFormOfTheUnpaidOrdersCancellation_IsRefusedWithItsReason_AndThePaidStateStays_VALC077()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, string accessKey, int opened) = await SeedAsync(host, null);
        string cancelScreen = OrderRoutes.ScreenPath(OrderRoutes.AccessPath(accessKey), "iptal");

        (HttpStatusCode paid, _) = await AdminPostAsync(host, key, orderId, "MarkPaid", opened);
        using HttpResponseMessage late = await CustomerPostAsync(host, cancelScreen, "CancelOrder", new KeyValuePair<string, string>("Confirmed", "true"));

        // The customer's later operation is judged against the current state (02 3.31.2): the whole-order cancellation is the unpaid order's operation, so the form is refused
        // with the closed-operation reason, and the administrator's mark is not undone by it.
        Assert.Equal(HttpStatusCode.SeeOther, paid);
        Assert.Equal(HttpStatusCode.SeeOther, late.StatusCode);
        Assert.Equal(OrderRoutes.AccessPath(accessKey) + "?notice=OperationClosed", late.Headers.Location!.OriginalString);
        await using AppDbContext db = host.NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal(PaymentStatus.Paid, order.PaymentStatus);
        Assert.Equal(1, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    // ---- VAL-C078: the administrator and the system ----

    [Fact]
    public async Task SystemFirst_ThePaymentWindowsCancellation_InvalidatesTheAdministratorsLaterMark_Is409_VALC078()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, int opened) = await SeedAsync(host, null);

        OrderTransitionResult cancelled = await host.InTransactionAsync((services, ct) => services.GetRequiredService<IOrderTransitions>()
            .CancelOrderAsync(CancelOrderRequest.BySystem(orderId, TransitionReason.PaymentWindowExpired), ct));
        (HttpStatusCode late, string html) = await AdminPostAsync(host, key, orderId, "MarkPaid", opened);

        Assert.False(cancelled.IsRejected);
        Assert.Equal(HttpStatusCode.Conflict, late);
        Assert.Contains(ConflictControls.OrderMessage, html, StringComparison.Ordinal);
        await using AppDbContext db = host.NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal(OrderStatus.Cancelled, order.OrderStatus);
        Assert.NotEqual(PaymentStatus.Paid, order.PaymentStatus);
        Assert.Equal(0, await db.AuditLogs.CountAsync(a => a.TargetRecordId == orderId, Ct));
    }

    [Fact]
    public async Task AdministratorFirst_MarkingPaid_InvalidatesTheSystemsLaterExpiryCancellation_ItIsNotApplied_AndThePaidStateStays_VALC078()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        (Guid orderId, _, int opened) = await SeedAsync(host, null);

        (HttpStatusCode paid, _) = await AdminPostAsync(host, key, orderId, "MarkPaid", opened);
        OrderTransitionResult cancelled = await host.InTransactionAsync((services, ct) => services.GetRequiredService<IOrderTransitions>()
            .CancelOrderAsync(CancelOrderRequest.BySystem(orderId, TransitionReason.PaymentWindowExpired), ct));

        Assert.Equal(HttpStatusCode.SeeOther, paid);
        Assert.True(cancelled.IsRejected);
        await using AppDbContext db = host.NewContext();
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid), (order.OrderStatus, order.PaymentStatus));
    }

    // ---- the harness ----

    /// <summary>A guest order with one physical item (04 9.9); with <paramref name="payment"/> set the payment status is put the way a payment would. Returns the order, its customer root key and its version.</summary>
    private static async Task<(Guid OrderId, string AccessKey, int Version)> SeedAsync(FileEndpointsHost host, PaymentStatus? payment)
    {
        DateTimeOffset now = host.Time.GetUtcNow();
        string n = Guid.NewGuid().ToString("N")[..8];
        await using AppDbContext db = host.NewContext();
        Product product = Product.Create("Kupa " + n, "kupa-" + n, ProductType.Physical, 20m, now).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, "SKU-" + n, 10m, now, 5);
        db.Products.Add(product);
        db.Variants.Add(variant);
        await db.SaveChangesAsync(Ct);
        (Guid orderId, _, string key) = await TestSessions.OrderAsync(host.Dsn, null);
        db.OrderItems.Add(OrderItem.Create(orderId, product.Id, variant.Id, ProductType.Physical, "Kupa " + n, 2, 100m, 20m, 16.67m, now));
        await db.SaveChangesAsync(Ct);
        if (payment is { } paid)
        {
            await host.SetPaymentAsync(orderId, paid);
        }

        int version = await db.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct);
        return (orderId, key, version);
    }

    private static async Task<(HttpStatusCode Status, string Html)> AdminGetAsync(FileEndpointsHost host, string key, Guid orderId)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, new Uri($"{OrdersPath}/{orderId:D}", UriKind.Relative));
        request.Headers.Add("Cookie", $"{CookieNames.AdminSession}={key}");
        using HttpResponseMessage response = await host.Client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<(HttpStatusCode Status, string Html)> AdminPostAsync(FileEndpointsHost host, string key, Guid orderId, string handler, int version, params (string Name, string Value)[] extra)
    {
        List<KeyValuePair<string, string>> fields = [new("Input.Version", version.ToString(System.Globalization.CultureInfo.InvariantCulture)), new("Input.Confirmed", "true")];
        fields.AddRange(extra.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        using HttpRequestMessage request = new(HttpMethod.Post, new Uri($"{OrdersPath}/{orderId:D}?handler={handler}", UriKind.Relative)) { Content = new FormUrlEncodedContent(fields) };
        request.Headers.Add("Cookie", $"{CookieNames.AdminSession}={key}");
        using HttpResponseMessage response = await host.Client.SendAsync(request, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<HttpResponseMessage> CustomerPostAsync(FileEndpointsHost host, string path, string handler, params KeyValuePair<string, string>[] fields)
    {
        using FormUrlEncodedContent form = new(fields);
        return await host.Client.PostAsync(new Uri(path + "?handler=" + handler, UriKind.Relative), form, Ct);
    }
}
