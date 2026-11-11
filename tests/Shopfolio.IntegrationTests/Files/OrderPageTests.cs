using System.Net;
using System.Text.Encodings.Web;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Endpoints.Files;
using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Orders;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// E-16 on the opened application and a real PostgreSQL (T511 criteria 5 to 10; KK5 to KK10; 07 UC-33, UC-34, UC-36): the regions of the page and the frozen documents, a closed
/// operation that has no button, the card order whose result is awaited (said, never refreshed), the download that answers 303 to a one-hour signed address and spends one right
/// per press, the three reasons of the 422 (used-up right, unconfirmed payment, missing file - the last spends nothing) and the 404 of an item that is not this order's digital item.
/// </summary>
[Collection(FileEndpointsDefinition.Name)]
public sealed class OrderPageTests
{
    private static readonly byte[] Content = [.. Enumerable.Range(0, 40).Select(i => (byte)i)];

    private readonly CollectionDatabaseFixture _database;

    public OrderPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static Uri At(string path) => new(path, UriKind.Relative);

    private static async Task<HttpResponseMessage> DownloadAsync(FileEndpointsHost host, string key, Guid itemId)
    {
        using FormUrlEncodedContent form = new([new KeyValuePair<string, string>(nameof(DownloadDigitalFileInput.OrderItemId), itemId.ToString())]);
        return await host.Client.PostAsync(At(OrderRoutes.AccessPath(key) + "?handler=DownloadDigitalFile"), form, Ct);
    }

    private static async Task<int> DownloadCountAsync(FileEndpointsHost host, Guid itemId)
    {
        await using AppDbContext db = host.NewContext();
        return await db.OrderItems.Where(i => i.Id == itemId).Select(i => i.DownloadCount).SingleAsync(Ct);
    }

    /// <summary>A guest order with one digital item of a new product: its file attached (or none), its right and the order's payment status.</summary>
    private static async Task<(Guid OrderId, string Number, string Key, Guid ItemId)> SeedAsync(FileEndpointsHost host, PaymentStatus payment, int allowance = 3, bool withFile = true)
    {
        DateTimeOffset now = host.Time.GetUtcNow();
        string n = Guid.NewGuid().ToString("N")[..8];
        await using AppDbContext db = host.NewContext();
        Product product = Product.Create("E-kitap " + n, "e-kitap-" + n, ProductType.Digital, 20m, now);
        Variant variant = Variant.Create(product.Id, "SKU-" + n, 10m, now);
        db.Products.Add(product);
        db.Variants.Add(variant);
        await db.SaveChangesAsync(Ct);
        DigitalFile? file = withFile ? await host.ReplaceFileAsync(variant.Id, Content) : null;
        (Guid orderId, string number, string key) = await TestSessions.OrderAsync(host.Dsn, null);
        OrderItem item = OrderItem.Create(orderId, product.Id, variant.Id, ProductType.Digital, "E-kitap " + n, 1, 10m, 20m, 1.67m, now);
        item.DigitalFileId = file?.Id;
        item.DownloadAllowance = allowance;
        db.OrderItems.Add(item);
        await db.SaveChangesAsync(Ct);
        await host.SetPaymentAsync(orderId, payment);
        return (orderId, number, key, item.Id);
    }

    // --- KK6, KK7: the regions, the frozen documents, no closed operation ---

    [Fact]
    public async Task ThePage_ShowsTheRegionsAndTheFrozenDocuments_AndNoButtonOfAClosedOperation_KK6_KK7()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string number, string key, _) = await SeedAsync(host, PaymentStatus.Paid);
        await using (AppDbContext db = host.NewContext())
        {
            // The published text changes after the order: the page shows the text frozen at the confirmation, not the current one (04 5.16.9).
            await db.Database.ExecuteSqlAsync($"UPDATE orders SET pre_information_form_text = {"donmus on bilgi metni"}, distance_sales_contract_text = {"donmus sozlesme metni"} WHERE id = {orderId}", Ct);
        }

        using HttpResponseMessage response = await host.Client.GetAsync(At(OrderRoutes.AccessPath(key)), Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains($"Sipariş {number}", body, StringComparison.Ordinal);
        foreach (string region in new[] { "order-items-title", "order-breakdown-title", "order-addresses-title", "order-documents-title" })
        {
            Assert.Contains($"id=\"{region}\"", body, StringComparison.Ordinal);
        }

        Assert.Contains("donmus on bilgi metni", body, StringComparison.Ordinal);
        Assert.Contains("donmus sozlesme metni", body, StringComparison.Ordinal);
        Assert.Contains("Ön Bilgilendirme Formu", body, StringComparison.Ordinal);
        Assert.Contains("Mesafeli Satış Sözleşmesi", body, StringComparison.Ordinal);

        // A paid digital-only order: the download is open; no transfer block and no delivery area; the closed operations of 03 1.11 have no button.
        Assert.Contains("handler=DownloadDigitalFile", body, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"order-transfer\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"order-delivery-title\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("Siparişi iptal et", body, StringComparison.Ordinal);
        Assert.DoesNotContain("Sorun bildir", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AnUnpaidTransferOrder_ShowsTheTransferDetails_AndNoDownloadButton_KK6_KK7()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string number, string key, _) = await SeedAsync(host, PaymentStatus.Pending);

        using HttpResponseMessage response = await host.Client.GetAsync(At(OrderRoutes.AccessPath(key)), Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("id=\"order-transfer\"", body, StringComparison.Ordinal);
        Assert.Contains(number, body, StringComparison.Ordinal);
        Assert.DoesNotContain("handler=DownloadDigitalFile", body, StringComparison.Ordinal);
    }

    // --- KK8: the awaited card result is said, the page never refreshes itself ---

    [Fact]
    public async Task ACardOrderWhoseResultHasNotCome_SaysSo_AndHasNoRefresh_KK8()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, _, string key, _) = await SeedAsync(host, PaymentStatus.Pending);
        await using (AppDbContext db = host.NewContext())
        {
            await db.Database.ExecuteSqlAsync($"UPDATE orders SET payment_method = 'Card', bank_transfer_iban = NULL WHERE id = {orderId}", Ct);
        }

        using HttpResponseMessage response = await host.Client.GetAsync(At(OrderRoutes.AccessPath(key)), Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(await EncodedAsync(host, OrderPageModel.CardAwaitedText), body, StringComparison.Ordinal);
        Assert.DoesNotContain("http-equiv=\"refresh\"", body, StringComparison.OrdinalIgnoreCase);
        Assert.False(response.Headers.Contains("Refresh"));
        Assert.DoesNotContain("id=\"order-transfer\"", body, StringComparison.Ordinal);
    }

    // --- KK9: 303 to a one-hour signed address, every press spends one right ---

    [Fact]
    public async Task TheDownload_Is303ToTheSignedAddress_ThatOpensForOneHour_AndEveryPressSpendsOneRight_KK9()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, _, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid);

        using HttpResponseMessage first = await DownloadAsync(host, key, itemId);
        using HttpResponseMessage second = await DownloadAsync(host, key, itemId);

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        string location = first.Headers.Location!.OriginalString;
        Assert.StartsWith(DownloadEndpoint.Prefix + "/", location, StringComparison.Ordinal);
        Assert.Equal(2, await DownloadCountAsync(host, itemId));

        // The signed address carries its own life of one hour (05 7.7.4; UC-37): the last minute opens, after the hour it is the empty 404.
        host.Time.Advance(TimeSpan.FromMinutes(59));
        using HttpResponseMessage inside = await host.Client.GetAsync(At(location), Ct);
        host.Time.Advance(TimeSpan.FromMinutes(2));
        using HttpResponseMessage spent = await host.Client.GetAsync(At(location), Ct);
        Assert.Equal(HttpStatusCode.OK, inside.StatusCode);
        Assert.Equal(Content, await inside.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal(HttpStatusCode.NotFound, spent.StatusCode);

        // Opening the address spends nothing: only the presses did.
        Assert.Equal(2, await DownloadCountAsync(host, itemId));
    }

    // --- KK10: the three reasons of the 422 and the 404 ---

    [Fact]
    public async Task AUsedUpRight_Is422WithItsReason_AndSpendsNothingMore_KK10()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, _, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, allowance: 1);

        using HttpResponseMessage taken = await DownloadAsync(host, key, itemId);
        using HttpResponseMessage refused = await DownloadAsync(host, key, itemId);

        Assert.Equal(HttpStatusCode.SeeOther, taken.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains(await EncodedAsync(host, OrderPageModel.AllowanceUsedText), await refused.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Equal(1, await DownloadCountAsync(host, itemId));
    }

    [Fact]
    public async Task AnUnconfirmedTransferPayment_Is422WithItsReason_AndSpendsNothing_KK10()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, _, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Pending);

        using HttpResponseMessage refused = await DownloadAsync(host, key, itemId);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains(await EncodedAsync(host, OrderPageModel.PaymentNotConfirmedText), await refused.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Equal(0, await DownloadCountAsync(host, itemId));
    }

    [Fact]
    public async Task AMissingFile_Is422WithItsReason_AndSpendsNoRight_KK10()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, _, string key, Guid itemId) = await SeedAsync(host, PaymentStatus.Paid, withFile: false);

        using HttpResponseMessage refused = await DownloadAsync(host, key, itemId);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.StatusCode);
        Assert.Contains(await EncodedAsync(host, OrderPageModel.NoFileText), await refused.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Equal(0, await DownloadCountAsync(host, itemId));
    }

    [Fact]
    public async Task AnItemOfAnotherOrderOrAPhysicalItem_Is404_AndSpendsNothing_KK10()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, _, string key, _) = await SeedAsync(host, PaymentStatus.Paid);
        (_, _, _, Guid otherItem) = await SeedAsync(host, PaymentStatus.Paid);
        Guid physicalItem;
        await using (AppDbContext db = host.NewContext())
        {
            OrderItem physical = OrderItem.Create(orderId, null, null, ProductType.Physical, "Masa", 1, 10m, 20m, 1.67m, host.Time.GetUtcNow());
            db.OrderItems.Add(physical);
            await db.SaveChangesAsync(Ct);
            physicalItem = physical.Id;
        }

        using HttpResponseMessage foreign = await DownloadAsync(host, key, otherItem);
        using HttpResponseMessage notDigital = await DownloadAsync(host, key, physicalItem);
        using HttpResponseMessage unknown = await DownloadAsync(host, key, Guid.NewGuid());

        foreach (HttpResponseMessage response in new[] { foreign, notDigital, unknown })
        {
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        Assert.Equal(0, await DownloadCountAsync(host, otherItem));
    }

    // --- KK5: the access key's states ---

    [Fact]
    public async Task ARenewedOrDisposedKey_IsTheSame404_KK5()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid renewedOrder, _, string oldKey, _) = await SeedAsync(host, PaymentStatus.Paid);
        (Guid disposedOrder, _, string disposedKey, _) = await SeedAsync(host, PaymentStatus.Paid);
        await using (AppDbContext db = host.NewContext())
        {
            await db.Database.ExecuteSqlAsync($"UPDATE orders SET order_access_key = {Shopfolio.Infrastructure.Security.SecretKey.Create()} WHERE id = {renewedOrder}", Ct);
            Order disposed = await db.Orders.SingleAsync(o => o.Id == disposedOrder, Ct);
            disposed.ErasePersonalData();
            disposed.BumpVersion();
            await db.SaveChangesAsync(Ct);
        }

        using HttpResponseMessage renewed = await host.Client.GetAsync(At(OrderRoutes.AccessPath(oldKey)), Ct);
        using HttpResponseMessage gone = await host.Client.GetAsync(At(OrderRoutes.AccessPath(disposedKey)), Ct);

        Assert.Equal(HttpStatusCode.NotFound, renewed.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, gone.StatusCode);
        Assert.Equal(await renewed.Content.ReadAsStringAsync(Ct), await gone.Content.ReadAsStringAsync(Ct));
    }

    /// <summary>The text the way the application's own encoder writes it in the page.</summary>
    private static Task<string> EncodedAsync(FileEndpointsHost host, string text) => host.InScopeAsync((services, _) => Task.FromResult(services.GetRequiredService<HtmlEncoder>().Encode(text)));
}
