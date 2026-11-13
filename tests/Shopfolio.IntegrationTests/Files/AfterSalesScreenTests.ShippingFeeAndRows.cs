using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Notices;
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
/// E-18 and E-19 after the first review round (T513 fix 1): the unconfirmed POST refuses units above or other than the open ones as the confirmed one does (B4); one row per item and
/// the whole request of several items (K2); the shipping fee's lines as the module's rule reads them (B1; 04 5.18.6; K-789); the screen's commands do not grow with its items (B2;
/// 09 20 item 4); the item's units on the row and the defect claim's field in the item's row (B3; 04 2.12.1.8).
/// </summary>
public sealed partial class AfterSalesScreenTests
{
    private const string FeeLine = "id=\"withdrawal-fee\"";

    // ---- B4: the unconfirmed POST refuses units above or other than the open ones ----

    [Fact]
    public async Task TheUnconfirmedPost_WithUnitsAboveTheOpenOnesOrStaleOpenUnits_Is303QuantityExceedsOpen_AndWritesNothing_B4()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 2);
        string screen = Withdrawal(key);

        using HttpResponseMessage exceeds = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, itemId, "3", 2), new("Iban", ValidIban)]);
        using HttpResponseMessage stale = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, itemId, "1", 1), new("Iban", ValidIban)]);

        string exceeded = NoticeParameter.Address(OrderRoutes.AccessPath(key), NoticeCode.QuantityExceedsOpen);
        Assert.Equal((HttpStatusCode.SeeOther, exceeded), (exceeds.StatusCode, exceeds.Headers.Location!.OriginalString));
        Assert.Equal((HttpStatusCode.SeeOther, exceeded), (stale.StatusCode, stale.Headers.Location!.OriginalString));
        Assert.Equal(0, await WithdrawalCountAsync(host, itemId));
    }

    // ---- K2: one row per item; several items are one request ----

    [Fact]
    public async Task TwoRowsOfOneItem_Are422_AndAnExceedingSecondItem_RefusesTheWholeRequest_K2()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid[] items) = await SeedOrderAsync(host, 2, 1);
        string screen = Withdrawal(key);

        using HttpResponseMessage repeated = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, items[0], "1", 2), .. Row(1, items[0], "1", 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage secondExceeds = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, items[0], "2", 2), .. Row(1, items[1], "2", 1), new("Iban", ValidIban), new("Confirmed", "true")]);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, repeated.StatusCode);
        Assert.Equal(
            (HttpStatusCode.SeeOther, NoticeParameter.Address(OrderRoutes.AccessPath(key), NoticeCode.QuantityExceedsOpen)),
            (secondExceeds.StatusCode, secondExceeds.Headers.Location!.OriginalString));
        Assert.Equal(0, await WithdrawalCountAsync(host, items[0]) + await WithdrawalCountAsync(host, items[1]));
    }

    // ---- B1: the shipping fee's lines are the module's answer ----

    [Fact]
    public async Task TheShippingFeeLine_IsOnTheLastStepOnlyWhenTheChoiceCoversEveryShippedUnit_B1()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid[] items) = await SeedOrderAsync(host, 2, 1);
        await ChargeAndShipAsync(host, orderId, 30m);
        string screen = Withdrawal(key);

        using HttpResponseMessage page = await host.Client.GetAsync(At(screen), Ct);
        using HttpResponseMessage all = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, items[0], "2", 2), .. Row(1, items[1], "1", 1), new("Iban", ValidIban)]);
        using HttpResponseMessage partial = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, items[0], "1", 2), .. Row(1, items[1], "1", 1), new("Iban", ValidIban)]);
        using HttpResponseMessage oneItem = await PostAsync(host, screen, "SubmitWithdrawal", [.. Row(0, items[0], "2", 2), new("Iban", ValidIban)]);

        Assert.Contains(OrderScreenTexts.ShippingFeeText, WebUtility.HtmlDecode(await page.Content.ReadAsStringAsync(Ct)), StringComparison.Ordinal);
        Assert.Equal((HttpStatusCode.OK, HttpStatusCode.OK, HttpStatusCode.OK), (all.StatusCode, partial.StatusCode, oneItem.StatusCode));
        Assert.Contains(FeeLine, await all.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.DoesNotContain(FeeLine, await partial.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.DoesNotContain(FeeLine, await oneItem.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheShippingFeeLines_AreAbsentOnFreeShipping_AndCountTheEarlierDeclarations_B1()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid freeOrder, string freeKey, Guid[] freeItems) = await SeedOrderAsync(host, 1);
        await ChargeAndShipAsync(host, freeOrder, 0m);
        (Guid orderId, string key, Guid[] items) = await SeedOrderAsync(host, 2);
        await ChargeAndShipAsync(host, orderId, 30m);

        using HttpResponseMessage freePage = await host.Client.GetAsync(At(Withdrawal(freeKey)), Ct);
        using HttpResponseMessage freeStep = await PostAsync(host, Withdrawal(freeKey), "SubmitWithdrawal", [.. Row(0, freeItems[0], "1", 1), new("Iban", ValidIban)]);
        using HttpResponseMessage first = await PostAsync(host, Withdrawal(key), "SubmitWithdrawal", [.. Row(0, items[0], "1", 2), new("Iban", ValidIban), new("Confirmed", "true")]);
        using HttpResponseMessage rest = await PostAsync(host, Withdrawal(key), "SubmitWithdrawal", [.. Row(0, items[0], "1", 1), new("Iban", ValidIban)]);

        Assert.DoesNotContain(OrderScreenTexts.ShippingFeeText, WebUtility.HtmlDecode(await freePage.Content.ReadAsStringAsync(Ct)), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, freeStep.StatusCode);
        Assert.DoesNotContain(FeeLine, await freeStep.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
        Assert.Equal((HttpStatusCode.SeeOther, OrderRoutes.AccessPath(key)), (first.StatusCode, first.Headers.Location!.OriginalString));
        Assert.Equal(HttpStatusCode.OK, rest.StatusCode);
        Assert.Contains(FeeLine, await rest.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    // ---- B2: the screen's commands do not grow with its items ----

    [Fact]
    public async Task TheWithdrawalScreen_SendsTheSameNumberOfCommands_ForOneItemAndForThree_B2()
    {
        StorefrontSearchAndProductTests.CommandCounter counter = new();
        await using FileEndpointsHost host = new(_database.Dsn, interceptor: counter);
        (Guid oneOrder, string oneKey, Guid[] one) = await SeedOrderAsync(host, 2);
        await ChargeAndShipAsync(host, oneOrder, 30m);
        (Guid threeOrder, string threeKey, Guid[] three) = await SeedOrderAsync(host, 2, 2, 2);
        await ChargeAndShipAsync(host, threeOrder, 30m);
        using (HttpResponseMessage warm = await host.Client.GetAsync(At(Withdrawal(oneKey)), Ct))
        {
            Assert.Equal(HttpStatusCode.OK, warm.StatusCode);
        }

        (int oneGet, int onePost) = await CountScreenAsync(host, counter, oneKey, [.. Row(0, one[0], "2", 2), new("Iban", ValidIban)]);
        (int threeGet, int threePost) = await CountScreenAsync(
            host, counter, threeKey, [.. Row(0, three[0], "2", 2), .. Row(1, three[1], "2", 2), .. Row(2, three[2], "2", 2), new("Iban", ValidIban)]);

        Assert.Equal((oneGet, onePost), (threeGet, threePost));
    }

    // ---- B3: the item's units on the row; the defect claim's field in the item's row ----

    [Fact]
    public async Task TheRows_SayTheItemsUnitsWithTheLimit_AndTheDefectClaimsFieldOpensInTheItemsRow_B3()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 3);

        using HttpResponseMessage withdrawal = await host.Client.GetAsync(At(Withdrawal(key)), Ct);
        using HttpResponseMessage claim = await host.Client.GetAsync(At(ClaimScreen(key)), Ct);

        Assert.Contains(OrderScreenTexts.OpenUnits(3, 3), WebUtility.HtmlDecode(await withdrawal.Content.ReadAsStringAsync(Ct)), StringComparison.Ordinal);
        string html = WebUtility.HtmlDecode(await claim.Content.ReadAsStringAsync(Ct));
        Assert.Contains(OrderScreenTexts.ClaimLimit(3, 3), html, StringComparison.Ordinal);
        int row = html.IndexOf($"data-item=\"{itemId}\"", StringComparison.Ordinal);
        int field = html.IndexOf("name=\"Quantity\"", StringComparison.Ordinal);
        int limit = html.IndexOf(OrderScreenTexts.ClaimLimit(3, 3), StringComparison.Ordinal);
        Assert.True(row >= 0 && row < field && field < limit, "the quantity field opens inside the item's row, before its limit line");
        Assert.Equal(field, html.LastIndexOf("name=\"Quantity\"", StringComparison.Ordinal));
    }

    // ---- helpers ----

    /// <summary>The commands of the screen's GET and of its unconfirmed POST, each counted on its own.</summary>
    private static async Task<(int Get, int Post)> CountScreenAsync(FileEndpointsHost host, StorefrontSearchAndProductTests.CommandCounter counter, string key, KeyValuePair<string, string>[] form)
    {
        int before = counter.Count;
        using HttpResponseMessage page = await host.Client.GetAsync(At(Withdrawal(key)), Ct);
        int get = counter.Count - before;
        before = counter.Count;
        using HttpResponseMessage step = await PostAsync(host, Withdrawal(key), "SubmitWithdrawal", form);
        int post = counter.Count - before;
        Assert.Equal((HttpStatusCode.OK, HttpStatusCode.OK), (page.StatusCode, step.StatusCode));
        return (get, post);
    }

    /// <summary>A guest transfer order, paid and shipped, with one physical item per entry of <paramref name="quantities"/>, in that order.</summary>
    private static async Task<(Guid OrderId, string Key, Guid[] Items)> SeedOrderAsync(FileEndpointsHost host, params int[] quantities)
    {
        DateTimeOffset now = host.Time.GetUtcNow();
        await using AppDbContext db = host.NewContext();
        (Guid orderId, _, string key) = await TestSessions.OrderAsync(host.Dsn, null);
        List<Guid> items = [];
        foreach (int quantity in quantities)
        {
            string n = Guid.NewGuid().ToString("N")[..8];
            Product product = Product.Create("Kupa " + n, "kupa-" + n, ProductType.Physical, 20m, now).WithSeededOrigin();
            Variant variant = Variant.Create(product.Id, "SKU-" + n, 10m, now, 5);
            db.Products.Add(product);
            db.Variants.Add(variant);
            OrderItem item = OrderItem.Create(orderId, product.Id, variant.Id, ProductType.Physical, "Kupa " + n, quantity, 100m, 20m, 16.67m, now.AddSeconds(items.Count));
            db.OrderItems.Add(item);
            items.Add(item.Id);
        }

        await db.SaveChangesAsync(Ct);
        await db.Database.ExecuteSqlAsync($"UPDATE orders SET payment_status = 'Paid', order_status = 'Shipped' WHERE id = {orderId}", Ct);
        return (orderId, key, [.. items]);
    }

    /// <summary>The order's frozen shipping fee (a zero fee is the seeded order's own) and the carrier's taking of it (S5), an hour ago.</summary>
    private static async Task ChargeAndShipAsync(FileEndpointsHost host, Guid orderId, decimal fee)
    {
        await using AppDbContext db = host.NewContext();
        if (fee > 0m)
        {
            await db.Database.ExecuteSqlAsync($"UPDATE orders SET shipping_fee = {fee}, shipping_fee_vat_breakdown = '[]' WHERE id = {orderId}", Ct);
        }

        db.TransitionRecords.Add(TransitionRecord.Create(orderId, TransitionCode.S5, ActorKind.Admin, null, null, null, null, null, null, null, host.Time.GetUtcNow().AddHours(-1)));
        await db.SaveChangesAsync(Ct);
    }
}
