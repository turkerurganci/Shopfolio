using System.Net;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Orders;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// E-17 and the order page's IBAN form on the opened application and a real PostgreSQL (T512 criteria 1, 2, 4, 6-8, 10; 07 UC-35, UC-38…UC-41): the two addresses under the
/// order's root and their two operations, the closed operation's 303 with its reason, the unknown root's 404, the script-free last step that writes nothing, the form's 422s
/// (quantity under one, a repeated item row, the transfer's IBAN), the whole request's 303 refusal, the termination that ignores the item fields, and UC-35's 303 with
/// "Kaydedildi", its 422 and the foreign item's 404. The module's rules themselves are <c>CustomerOperationsTests</c>'.
/// </summary>
[Collection(FileEndpointsDefinition.Name)]
public sealed class OrderOperationScreenTests
{
    private const string ValidIban = "TR330006100519786457841326";

    private const string CorrectedIban = "TR320010009999901234567890";

    private readonly CollectionDatabaseFixture _database;

    public OrderOperationScreenTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Uri At(string path) => new(path, UriKind.Relative);

    // ---- KK4, KK7: the order's cancellation on Placed + Pending ----

    [Fact]
    public async Task TheCancellationAddress_OfAnUnpaidOrder_OffersTheOrdersCancellation_TheUnconfirmedPostIsTheLastStep_AndTheConfirmedOneCancels_KK4_KK7()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, _) = await SeedAsync(host, null, 2);
        string root = OrderRoutes.AccessPath(key);
        string screen = OrderRoutes.ScreenPath(root, "iptal");

        using HttpResponseMessage page = await host.Client.GetAsync(At(screen), Ct);
        using HttpResponseMessage lastStep = await PostAsync(host, screen, "CancelOrder", []);
        OrderStatus afterLastStep = await StatusAsync(host, orderId);
        using HttpResponseMessage confirmed = await PostAsync(host, screen, "CancelOrder", [new("Confirmed", "true")]);

        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("handler=CancelOrder", await page.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, lastStep.StatusCode);
        Assert.Contains("name=\"Confirmed\" value=\"true\"", await lastStep.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Equal(OrderStatus.Placed, afterLastStep);
        Assert.Equal(HttpStatusCode.SeeOther, confirmed.StatusCode);
        Assert.Equal(root, confirmed.Headers.Location!.OriginalString);
        Assert.Equal(OrderStatus.Cancelled, await StatusAsync(host, orderId));
    }

    // ---- KK4, K-795 (04 5.17.6; 07 4.17.3): the last step names the items with their units ----

    [Fact]
    public async Task TheLastStep_OfTheOrdersCancellation_ListsEveryItemByNameWithItsUnits_KK4_K795()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid itemId) = await SeedAsync(host, null, 2);
        string screen = OrderRoutes.ScreenPath(OrderRoutes.AccessPath(key), "iptal");

        using HttpResponseMessage lastStep = await PostAsync(host, screen, "CancelOrder", []);

        Assert.Equal(HttpStatusCode.OK, lastStep.StatusCode);
        Assert.Equal([(await NameAsync(host, itemId), "2 adet")], LastStepItems(await lastStep.Content.ReadAsStringAsync(Ct)));
        Assert.Equal(OrderStatus.Placed, await StatusAsync(host, orderId));
    }

    [Fact]
    public async Task TheLastStep_OfTheItemCancellation_ListsTheChosenItemsByNameWithTheChosenUnits_AndOnlyThem_KK4_K795()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, 3);
        Guid otherItemId = await AddItemAsync(host, orderId, 2);
        string screen = OrderRoutes.ScreenPath(OrderRoutes.AccessPath(key), "iptal");
        int index = await ScreenIndexAsync(host, screen, itemId);

        using HttpResponseMessage lastStep = await PostAsync(host, screen, "CancelOrderItems", [.. Line(index, itemId, "2", 3), new("Iban", ValidIban)]);

        Assert.Equal(HttpStatusCode.OK, lastStep.StatusCode);
        string html = await lastStep.Content.ReadAsStringAsync(Ct);
        Assert.Equal([(await NameAsync(host, itemId), "2 adet")], LastStepItems(html));
        Assert.DoesNotContain(await NameAsync(host, otherItemId), WebUtility.HtmlDecode(html[html.IndexOf("confirm__items", StringComparison.Ordinal)..]), StringComparison.Ordinal);
        Assert.Contains("Seçtiğiniz 2 ürün iptal edilecek", WebUtility.HtmlDecode(html), StringComparison.Ordinal);
        await AssertNoCancellationAsync(host, orderId);
    }

    [Fact]
    public async Task TheLastStep_OfTheTermination_ListsEveryUndeliveredItemByNameWithItsOpenUnits_KK4_K795()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, 2);
        await using (AppDbContext db = host.NewContext())
        {
            host.Time.SetUtcNow(DeliveryPeriodRules.LegalDeliveryLimitEnd((await db.Orders.SingleAsync(o => o.Id == orderId, Ct)).CreatedAt).AddHours(1));
        }

        string screen = OrderRoutes.ScreenPath(OrderRoutes.AccessPath(key), "gecikme-feshi");

        using HttpResponseMessage lastStep = await PostAsync(host, screen, "SubmitLateDeliveryTermination", [new("Iban", ValidIban)]);

        Assert.Equal(HttpStatusCode.OK, lastStep.StatusCode);
        Assert.Equal([(await NameAsync(host, itemId), "2 adet")], LastStepItems(await lastStep.Content.ReadAsStringAsync(Ct)));
        await using AppDbContext reader = host.NewContext();
        Assert.False(await reader.LateDeliveryTerminations.AnyAsync(t => t.OrderItemId == itemId, Ct));
    }

    // ---- K1 (tur 1): the unconfirmed post refuses what the confirmed one would refuse ----

    [Fact]
    public async Task TheItemCancellationsUnconfirmedPost_AboveTheOpenUnits_OrOnAStaleOpenQuantity_Is303WithTheReason_NotTheLastStep_K1()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, 2);
        string root = OrderRoutes.AccessPath(key);
        string screen = OrderRoutes.ScreenPath(root, "iptal");

        using HttpResponseMessage exceeds = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, itemId, "3", 2), new("Iban", ValidIban)]);
        using HttpResponseMessage stale = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, itemId, "1", 1), new("Iban", ValidIban)]);

        string exceeded = NoticeParameter.Address(root, NoticeCode.QuantityExceedsOpen);
        Assert.Equal((HttpStatusCode.SeeOther, exceeded), (exceeds.StatusCode, exceeds.Headers.Location!.OriginalString));
        Assert.Equal((HttpStatusCode.SeeOther, exceeded), (stale.StatusCode, stale.Headers.Location!.OriginalString));
        await AssertNoCancellationAsync(host, orderId);
    }

    // ---- KK6: the closed operation and the unknown root ----

    [Fact]
    public async Task AClosedOperation_Is303ToTheRootWithItsReason_OnTheScreenAndOnThePost_AndAnUnknownRootIs404_KK6()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, _) = await SeedAsync(host, null, 2);
        string root = OrderRoutes.AccessPath(key);
        string termination = OrderRoutes.ScreenPath(root, "gecikme-feshi");
        string unknown = OrderRoutes.AccessPath("bilinmeyen-anahtar-" + Guid.NewGuid().ToString("N"));

        using HttpResponseMessage screen = await host.Client.GetAsync(At(termination), Ct);
        using HttpResponseMessage post = await PostAsync(host, termination, "SubmitLateDeliveryTermination", [new("Confirmed", "true")]);
        using HttpResponseMessage missingScreen = await host.Client.GetAsync(At(OrderRoutes.ScreenPath(unknown, "iptal")), Ct);
        using HttpResponseMessage missingPost = await PostAsync(host, OrderRoutes.ScreenPath(unknown, "iptal"), "CancelOrder", [new("Confirmed", "true")]);

        string closed = NoticeParameter.Address(root, NoticeCode.OperationClosed);
        Assert.Equal((HttpStatusCode.SeeOther, closed), (screen.StatusCode, screen.Headers.Location!.OriginalString));
        Assert.Equal((HttpStatusCode.SeeOther, closed), (post.StatusCode, post.Headers.Location!.OriginalString));
        Assert.Equal(HttpStatusCode.NotFound, missingScreen.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missingPost.StatusCode);
        Assert.Equal(OrderStatus.Placed, await StatusAsync(host, orderId));
    }

    // ---- KK8, D-155, VAL-A581: the item cancellation's form ----

    [Fact]
    public async Task TheItemCancellation_AQuantityUnderOne_ARepeatedItemRow_AndNoIbanOnTheTransfer_Are422_AndWriteNothing_KK8_D155()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, 2);
        string screen = OrderRoutes.ScreenPath(OrderRoutes.AccessPath(key), "iptal");

        using HttpResponseMessage zero = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, itemId, "0", 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage missing = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, itemId, string.Empty, 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage repeated = await PostAsync(
            host, screen, "CancelOrderItems", [.. Line(0, itemId, "1", 2), .. Line(1, itemId, "1", 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage noIban = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, itemId, "1", 2)]);
        using HttpResponseMessage badIban = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, itemId, "1", 2), new("Iban", "TR330006100519786457841327"), new("Confirmed", "true")]);
        using HttpResponseMessage noItem = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, Guid.Empty, "1", 2), new("Iban", ValidIban), new("Confirmed", "true")]);

        Assert.Equal(
            [HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity, HttpStatusCode.UnprocessableEntity],
            new[] { zero.StatusCode, missing.StatusCode, repeated.StatusCode, noIban.StatusCode, badIban.StatusCode, noItem.StatusCode });
        Assert.Contains(CancellationPageModel.NoItemText, WebUtility.HtmlDecode(await noItem.Content.ReadAsStringAsync(Ct)), StringComparison.Ordinal);

        // Every refused request writes exactly one rejection line (09 8.2 item 3): a field error's line is the validation filter's, the page writes none beside it.
        Assert.Equal(6, host.Logs.Lines.Count(l => l.Contains("RejectionLog|", StringComparison.Ordinal) && l.Contains(LogEvents.ValidationRejected, StringComparison.Ordinal)));
        await AssertNoCancellationAsync(host, orderId);
    }

    [Fact]
    public async Task TheItemCancellation_AboveTheOpenUnits_OrOnAStaleOpenQuantity_Is303WithTheReason_AndTheConfirmedRequestIsApplied_KK6_KK8()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, 2);
        string root = OrderRoutes.AccessPath(key);
        string screen = OrderRoutes.ScreenPath(root, "iptal");

        using HttpResponseMessage page = await host.Client.GetAsync(At(screen), Ct);
        using HttpResponseMessage exceeds = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, itemId, "3", 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage stale = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, itemId, "1", 1), new("Iban", ValidIban), new("Confirmed", "true")]);
        await AssertNoCancellationAsync(host, orderId);
        using HttpResponseMessage applied = await PostAsync(host, screen, "CancelOrderItems", [.. Line(0, itemId, "1", 2), new("Iban", ValidIban), new("Confirmed", "true")]);

        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.Contains("name=\"Iban\"", await page.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        string exceeded = NoticeParameter.Address(root, NoticeCode.QuantityExceedsOpen);
        Assert.Equal((HttpStatusCode.SeeOther, exceeded), (exceeds.StatusCode, exceeds.Headers.Location!.OriginalString));
        Assert.Equal((HttpStatusCode.SeeOther, exceeded), (stale.StatusCode, stale.Headers.Location!.OriginalString));
        Assert.Equal((HttpStatusCode.SeeOther, root), (applied.StatusCode, applied.Headers.Location!.OriginalString));
        await using AppDbContext reader = host.NewContext();
        Assert.Equal(1, (await reader.Cancellations.SingleAsync(c => c.OrderItemId == itemId, Ct)).Quantity);
    }

    // ---- KK10, VAL-A692: the termination ignores the item fields ----

    [Fact]
    public async Task TheTermination_IgnoresTheItemAndQuantityFields_CoversEveryOpenUnit_AndAnEmptyIbanOnTheTransferIs422_KK10()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, 2);
        DateTimeOffset createdAt;
        await using (AppDbContext db = host.NewContext())
        {
            createdAt = (await db.Orders.SingleAsync(o => o.Id == orderId, Ct)).CreatedAt;
        }

        host.Time.SetUtcNow(DeliveryPeriodRules.LegalDeliveryLimitEnd(createdAt).AddHours(1));
        string root = OrderRoutes.AccessPath(key);
        string screen = OrderRoutes.ScreenPath(root, "gecikme-feshi");

        using HttpResponseMessage page = await host.Client.GetAsync(At(screen), Ct);
        using HttpResponseMessage empty = await PostAsync(host, screen, "SubmitLateDeliveryTermination", [new("Iban", string.Empty), new("Confirmed", "true")]);
        using HttpResponseMessage applied = await PostAsync(
            host, screen, "SubmitLateDeliveryTermination", [.. Line(0, itemId, "1", 2), new("Iban", ValidIban), new("Confirmed", "true")]);

        Assert.Equal(HttpStatusCode.OK, page.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, empty.StatusCode);
        Assert.Equal((HttpStatusCode.SeeOther, root), (applied.StatusCode, applied.Headers.Location!.OriginalString));
        await using AppDbContext reader = host.NewContext();
        Assert.Equal(2, (await reader.LateDeliveryTerminations.SingleAsync(t => t.OrderItemId == itemId, Ct)).Quantity);
    }

    // ---- KK1, KK2: UC-35 on the order page ----

    [Fact]
    public async Task SaveRefundIban_Is303WithSaved_CorrectingInPlace_ABadIbanOrAClosedFieldIs422_AndAForeignItemIs404_KK1_KK2()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, 2);
        (_, string otherKey, Guid otherItemId) = await SeedAsync(host, PaymentStatus.Paid, 2);
        string root = OrderRoutes.AccessPath(key);
        using (HttpResponseMessage cancelled = await PostAsync(
            host, OrderRoutes.ScreenPath(root, "iptal"), "CancelOrderItems", [.. Line(0, itemId, "1", 2), new("Iban", ValidIban), new("Confirmed", "true")]))
        {
            Assert.Equal(HttpStatusCode.SeeOther, cancelled.StatusCode);
        }

        using HttpResponseMessage page = await host.Client.GetAsync(At(root), Ct);
        using HttpResponseMessage bad = await SaveIbanAsync(host, root, itemId, "TR32 0010 0099 9990 1234 5678 9X");
        using HttpResponseMessage closed = await SaveIbanAsync(host, OrderRoutes.AccessPath(otherKey), otherItemId, CorrectedIban);
        using HttpResponseMessage foreign = await SaveIbanAsync(host, root, otherItemId, CorrectedIban);
        using HttpResponseMessage saved = await SaveIbanAsync(host, root, itemId, " tr32 0010 0099 9990 1234 5678 90 ");

        Assert.Contains("handler=SaveRefundIban", await page.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, bad.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, foreign.StatusCode);
        Assert.Equal((HttpStatusCode.SeeOther, NoticeParameter.Address(root, NoticeCode.Saved)), (saved.StatusCode, saved.Headers.Location!.OriginalString));
        await using AppDbContext reader = host.NewContext();
        Cancellation cancellation = await reader.Cancellations.SingleAsync(c => c.OrderItemId == itemId, Ct);
        Assert.Equal(CorrectedIban, (await reader.RefundIbans.SingleAsync(i => i.Id == cancellation.RefundIbanId, Ct)).Iban);
    }

    [Fact]
    public async Task SaveRefundIban_OnAFieldThatIsNotOpen_Is422_WithOneRefundIbanNotRequestedRejectionLine_VALA490()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, 2);
        string root = OrderRoutes.AccessPath(key);
        int before = RejectionLines(host).Count;

        using HttpResponseMessage closed = await SaveIbanAsync(host, root, itemId, CorrectedIban);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, closed.StatusCode);
        Assert.EndsWith(" " + ErrorCode.RefundIbanNotRequested, Assert.Single(RejectionLines(host).Skip(before)), StringComparison.Ordinal);
    }

    // ---- D-1196: the order page's buttons open their screens ----

    [Fact]
    public async Task TheOrderPagesCancellationButton_OpensItsScreen_D1196()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, _) = await SeedAsync(host, PaymentStatus.Paid, 2);
        string root = OrderRoutes.AccessPath(key);
        string screen = OrderRoutes.ScreenPath(root, "iptal");

        using HttpResponseMessage page = await host.Client.GetAsync(At(root), Ct);
        using HttpResponseMessage opened = await host.Client.GetAsync(At(screen), Ct);

        Assert.Contains($"href=\"{screen}\"", await page.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, opened.StatusCode);
    }

    // ---- helpers ----

    /// <summary>A guest transfer order with one physical item of the given units; unpaid (Placed + Pending) when <paramref name="payment"/> is null.</summary>
    private static async Task<(Guid OrderId, string Key, Guid ItemId)> SeedAsync(FileEndpointsHost host, PaymentStatus? payment, int quantity)
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
        OrderItem item = OrderItem.Create(orderId, product.Id, variant.Id, ProductType.Physical, "Kupa " + n, quantity, 100m, 20m, 16.67m, now);
        db.OrderItems.Add(item);
        await db.SaveChangesAsync(Ct);
        if (payment is { } paid)
        {
            await host.SetPaymentAsync(orderId, paid);
        }

        return (orderId, key, item.Id);
    }

    /// <summary>A second physical item of the given units on the order.</summary>
    private static async Task<Guid> AddItemAsync(FileEndpointsHost host, Guid orderId, int quantity)
    {
        DateTimeOffset now = host.Time.GetUtcNow();
        string n = Guid.NewGuid().ToString("N")[..8];
        await using AppDbContext db = host.NewContext();
        Product product = Product.Create("Tabak " + n, "tabak-" + n, ProductType.Physical, 20m, now).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, "SKU-" + n, 10m, now, 5);
        db.Products.Add(product);
        db.Variants.Add(variant);
        OrderItem item = OrderItem.Create(orderId, product.Id, variant.Id, ProductType.Physical, "Tabak " + n, quantity, 100m, 20m, 16.67m, now);
        db.OrderItems.Add(item);
        await db.SaveChangesAsync(Ct);
        return item.Id;
    }

    private static async Task<string> NameAsync(FileEndpointsHost host, Guid itemId)
    {
        await using AppDbContext db = host.NewContext();
        return (await db.OrderItems.SingleAsync(i => i.Id == itemId, Ct)).ProductName;
    }

    /// <summary>The row of the item on the screen as the server draws it: the form's index of its checkbox.</summary>
    private static async Task<int> ScreenIndexAsync(FileEndpointsHost host, string screen, Guid itemId)
    {
        using HttpResponseMessage page = await host.Client.GetAsync(At(screen), Ct);
        Match row = Regex.Match(
            await page.Content.ReadAsStringAsync(Ct), "name=\"Items\\[(\\d+)\\]\\.OrderItemId\" value=\"" + itemId + "\"", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));
        Assert.True(row.Success, "The item is not on the screen.");
        return int.Parse(row.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture);
    }

    /// <summary>The last step's list (04 5.17.6): each item's name and units, in the order drawn.</summary>
    private static List<(string Name, string Units)> LastStepItems(string html) =>
    [
        .. Regex.Matches(
                WebUtility.HtmlDecode(html),
                "<span class=\"confirm__item-name\">([^<]*)</span> · <span class=\"confirm__item-quantity\">([^<]*)</span>",
                RegexOptions.CultureInvariant,
                TimeSpan.FromSeconds(2))
            .Select(m => (m.Groups[1].Value, m.Groups[2].Value)),
    ];

    private static List<string> RejectionLines(FileEndpointsHost host) =>
        [.. host.Logs.Lines.Where(l => l.Contains("RejectionLog|", StringComparison.Ordinal) && l.Contains(LogEvents.ValidationRejected, StringComparison.Ordinal))];

    private static KeyValuePair<string, string>[] Line(int index, Guid itemId, string quantity, int originalOpen) =>
    [
        new($"Items[{index}].OrderItemId", itemId.ToString()),
        new($"Items[{index}].Quantity", quantity),
        new($"Items[{index}].OriginalOpenQuantity", originalOpen.ToString(System.Globalization.CultureInfo.InvariantCulture)),
    ];

    private static async Task<HttpResponseMessage> PostAsync(FileEndpointsHost host, string path, string handler, KeyValuePair<string, string>[] fields)
    {
        using FormUrlEncodedContent form = new(fields);
        return await host.Client.PostAsync(At(path + "?handler=" + handler), form, Ct);
    }

    private static Task<HttpResponseMessage> SaveIbanAsync(FileEndpointsHost host, string root, Guid itemId, string iban) =>
        PostAsync(host, root, "SaveRefundIban", [new(nameof(SaveRefundIbanInput.OrderItemId), itemId.ToString()), new(nameof(SaveRefundIbanInput.Iban), iban)]);

    private static async Task<OrderStatus> StatusAsync(FileEndpointsHost host, Guid orderId)
    {
        await using AppDbContext db = host.NewContext();
        return (await db.Orders.SingleAsync(o => o.Id == orderId, Ct)).OrderStatus;
    }

    private static async Task AssertNoCancellationAsync(FileEndpointsHost host, Guid orderId)
    {
        await using AppDbContext db = host.NewContext();
        Assert.False(await db.Cancellations.AnyAsync(c => db.OrderItems.Any(i => i.Id == c.OrderItemId && i.OrderId == orderId), Ct));
        Assert.False(await db.RefundIbans.AnyAsync(i => i.OrderId == orderId, Ct));
    }
}
