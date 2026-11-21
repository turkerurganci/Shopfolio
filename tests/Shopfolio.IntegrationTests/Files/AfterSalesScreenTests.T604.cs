using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Orders;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// T604's money items on the customer's after-sales screens and the panel's reader (D-1702, D-1439, D-1440, D-1195, D-1382): the IBAN field's 422 says the catalog text, a mixed
/// choice's last step names both refund starts, the shipping fee line is gone once a unit stayed with the customer (K-1076), the return rejection's line on the item's row says its
/// reason and the goods' way back, and a refund born after its record runs its period from the record.
/// </summary>
public sealed partial class AfterSalesScreenTests
{
    // ---- D-1702: the IBAN field's 422 is the catalog text, not the code ----

    [Fact]
    public async Task TheWithdrawalsEmptyIban_Is422WithTheCatalogText_NotTheCode_D1702()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 2);

        using HttpResponseMessage noIban = await PostAsync(host, Withdrawal(key), "SubmitWithdrawal", [.. Row(0, itemId, "1", 2), new("Confirmed", "true")]);

        string html = WebUtility.HtmlDecode(await noIban.Content.ReadAsStringAsync(Ct));
        Assert.Equal(HttpStatusCode.UnprocessableEntity, noIban.StatusCode);
        Assert.Contains(FieldMessageCatalog.Build(FieldCodes.FieldRequired, IbanFields.Label, null), html, StringComparison.Ordinal);
        Assert.DoesNotContain(">" + FieldCodes.FieldRequired + "<", html, StringComparison.Ordinal);
    }

    // ---- D-1439: a mixed choice's last step names each refund start with its units ----

    [Fact]
    public async Task TheLastStepOfAMixedChoice_NamesBothRefundStartsWithTheirUnits_D1439()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid[] items) = await SeedOrderAsync(host, 2, 1);
        await ChargeAndShipAsync(host, orderId, 0m);
        // The customer's declaration counts its delivery day as before it (02 7.4.1): the item was delivered yesterday, so its refund starts from the goods' arrival.
        DateOnly yesterday = TurkeyTime.DateOf(host.Time.GetUtcNow()).AddDays(-1);
        await using (AppDbContext db = host.NewContext())
        {
            await db.Database.ExecuteSqlAsync($"UPDATE order_items SET delivery_date = {yesterday}, delivery_marked_at = {host.Time.GetUtcNow().AddDays(-1)} WHERE id = {items[0]}", Ct);
        }

        using HttpResponseMessage step = await PostAsync(host, Withdrawal(key), "SubmitWithdrawal", [.. Row(0, items[0], "2", 2), .. Row(1, items[1], "1", 1), new("Iban", ValidIban)]);

        Assert.Equal(HttpStatusCode.OK, step.StatusCode);
        string html = WebUtility.HtmlDecode(await step.Content.ReadAsStringAsync(Ct));
        int from = html.IndexOf("Seçtiğiniz ", StringComparison.Ordinal);
        string sentence = from < 0 ? html : html[from..(html.IndexOf("yapılacak.", from, StringComparison.Ordinal) + "yapılacak.".Length)];
        Assert.Equal(OrderScreenTexts.WithdrawalSentence(1, 2, Modules.Payment.Contracts.PaymentMethod.BankTransfer, AfterSalesPeriodRules.RefundPeriodDays), sentence);
    }

    // ---- D-1440: the declarations' write order (K-1107) and the fee line once a unit stayed with the customer (K-1076) ----

    [Fact]
    public void TheDeclarationsOfOneRequest_AreWrittenInDescendingUuidOrder_SoTheSmallestItemIsTheLast_D1440()
    {
        Guid[] items = [.. Enumerable.Range(0, 5).Select(_ => Guid.NewGuid())];

        Guid[] written = [.. WithdrawalPageModel.InWriteOrder(items, i => i)];

        Assert.Equal(items.OrderBy(i => i.ToString("N"), StringComparer.Ordinal).First(), written[^1]);
        Assert.Equal([.. items.OrderByDescending(i => i.ToString("N"), StringComparer.Ordinal)], written);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TheShippingFeeLines_AreGone_OnceAUnitWasRejectedOrClosedAsNotComeBack_K1076_D1440(bool rejected)
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, string key, Guid[] items) = await SeedOrderAsync(host, 2, 1);
        await ChargeAndShipAsync(host, orderId, 30m);
        using (HttpResponseMessage first = await PostAsync(host, Withdrawal(key), "SubmitWithdrawal", [.. Row(0, items[0], "1", 2), new("Iban", ValidIban), new("Confirmed", "true")]))
        {
            Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        }

        await using (AppDbContext db = host.NewContext())
        {
            Admin admin = await SeedAdminAsync(db, host.Time.GetUtcNow());
            Modules.AfterSales.Withdrawal withdrawal = await db.Withdrawals.SingleAsync(w => w.OrderItemId == items[0], Ct);
            DateTimeOffset now = host.Time.GetUtcNow();
            if (rejected)
            {
                db.ReturnRejections.Add(ReturnRejection.Create(items[0], withdrawal.Id, 1, TurkeyTime.DateOf(now), admin.Id, admin.Name, now));
            }
            else
            {
                db.NotReturnedClosures.Add(NotReturnedClosure.Create(items[0], withdrawal.Id, 1, admin.Id, admin.Name, now));
            }

            await db.SaveChangesAsync(Ct);
        }

        using HttpResponseMessage page = await host.Client.GetAsync(At(Withdrawal(key)), Ct);
        using HttpResponseMessage rest = await PostAsync(host, Withdrawal(key), "SubmitWithdrawal", [.. Row(0, items[0], "1", 1), .. Row(1, items[1], "1", 1), new("Iban", ValidIban)]);

        Assert.Equal((HttpStatusCode.OK, HttpStatusCode.OK), (page.StatusCode, rest.StatusCode));
        Assert.DoesNotContain(OrderScreenTexts.ShippingFeeText, WebUtility.HtmlDecode(await page.Content.ReadAsStringAsync(Ct)), StringComparison.Ordinal);
        Assert.DoesNotContain(FeeLine, await rest.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    // ---- D-1195: the return rejection's line on the customer's item row ----

    [Fact]
    public async Task TheReturnRejection_OnTheItemsRow_SaysItsReasonAndTheGoodsWayBack_D1195()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 1);
        await using (AppDbContext db = host.NewContext())
        {
            DateTimeOffset now = host.Time.GetUtcNow();
            Admin admin = await SeedAdminAsync(db, now);
            Modules.AfterSales.Withdrawal withdrawal = Modules.AfterSales.Withdrawal.ByBuyer(itemId, 1, TurkeyTime.DateOf(now), null, new(100m, 0m, 16.67m), null, now.AddDays(-2));
            db.Withdrawals.Add(withdrawal);
            db.ReturnRejections.Add(ReturnRejection.Create(itemId, withdrawal.Id, 1, TurkeyTime.DateOf(now), admin.Id, admin.Name, now));
            await db.SaveChangesAsync(Ct);
        }

        using HttpResponseMessage response = await host.Client.GetAsync(At(OrderRoutes.AccessPath(key)), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(ItemRecordLabels.ReturnRejectionDetail, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), StringComparison.Ordinal);
    }

    // ---- D-1382: a refund born after its record runs its period from the record ----

    [Fact]
    public async Task APendingRefundBornAfterItsRecord_RunsItsPeriodFromTheRecordsMoment_D1382()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid orderId, _, Guid itemId, _) = await SeedAsync(host, "Shipped", 2);
        DateTimeOffset now = host.Time.GetUtcNow();
        DateTimeOffset recordedAt = now.AddDays(-10);
        await using (AppDbContext db = host.NewContext())
        {
            Admin admin = await SeedAdminAsync(db, now);
            Modules.AfterSales.Withdrawal withdrawal = Modules.AfterSales.Withdrawal.ByBuyer(itemId, 1, TurkeyTime.DateOf(recordedAt), null, new(100m, 0m, 16.67m), null, recordedAt);
            db.Withdrawals.Add(withdrawal);
            Refund refund = Refund.ByAdmin(orderId, Modules.Payment.Contracts.RefundType.Withdrawal, Modules.Payment.Contracts.PaymentMethod.BankTransfer, 100m, admin.Id, admin.Name, now);
            refund.WithdrawalId = withdrawal.Id;
            refund.OrderItemId = itemId;
            refund.Quantity = 1;
            db.Refunds.Add(refund);
            await db.SaveChangesAsync(Ct);
        }

        (DateOnly expected, DateOnly? lastDay, DateOnly? detailDay) = await host.InScopeAsync(async (services, ct) =>
        {
            IAfterSalesModule afterSales = services.GetRequiredService<IAfterSalesModule>();
            IPanelOrderReader reader = services.GetRequiredService<IPanelOrderReader>();
            IReadOnlyList<OrderRunningPeriod> periods = await reader.FindRunningPeriodsAsync([orderId], ct);
            PanelOrderDetailFacts? detail = await reader.ReadDetailAsync(orderId, ct);
            return (afterSales.RefundLastDay(recordedAt), periods.SingleOrDefault(p => p.Period == CountedPeriod.RefundPeriod)?.LastDay, detail?.PendingRefunds.Single().LastDay);
        });

        Assert.NotEqual(expected, AfterSalesPeriodRules.RefundLastDay(TurkeyTime.DateOf(now)));
        Assert.Equal((expected, (DateOnly?)expected), (lastDay!.Value, detailDay));
    }

    // ---- D-1401: a foreign item is 404 before the field rule's 422, on the validation filter's path too ----

    [Fact]
    public async Task AForeignItem_WithABrokenFieldRule_Is404_NotTheFields422_OnBothScreens_AndTheOwnItemKeepsIts422_K1624_D1401()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (_, string key, Guid itemId, _) = await SeedAsync(host, "Shipped", 2);
        (_, _, Guid foreignItem, _) = await SeedAsync(host, "Shipped", 1);

        // E-19: the description is empty ([FieldRequired]); E-18: the IBAN is malformed ([InvalidIban]). The filter answers both before the handler.
        using HttpResponseMessage claimForeign = await PostAsync(host, ClaimScreen(key), "SubmitDefectClaim", [new("OrderItemId", foreignItem.ToString()), new("Description", "  ")]);
        using HttpResponseMessage claimOwn = await PostAsync(host, ClaimScreen(key), "SubmitDefectClaim", [new("OrderItemId", itemId.ToString()), new("Quantity", "1"), new("Description", "  ")]);
        using HttpResponseMessage withdrawalForeign = await PostAsync(host, Withdrawal(key), "SubmitWithdrawal", [.. Row(0, foreignItem, "1", 1), new("Iban", "TR00"), new("Confirmed", "true")]);
        using HttpResponseMessage withdrawalOwn = await PostAsync(host, Withdrawal(key), "SubmitWithdrawal", [.. Row(0, itemId, "1", 2), new("Iban", "TR00"), new("Confirmed", "true")]);

        Assert.Equal(
            [HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity, HttpStatusCode.NotFound, HttpStatusCode.UnprocessableEntity],
            new[] { claimForeign.StatusCode, claimOwn.StatusCode, withdrawalForeign.StatusCode, withdrawalOwn.StatusCode });
        await using AppDbContext db = host.NewContext();
        Assert.Equal(0, await db.DefectClaims.CountAsync(c => c.OrderItemId == itemId || c.OrderItemId == foreignItem, Ct));
        Assert.Equal(0, await WithdrawalCountAsync(host, itemId) + await WithdrawalCountAsync(host, foreignItem));
    }

    private static async Task<Admin> SeedAdminAsync(AppDbContext db, DateTimeOffset now)
    {
        Admin admin = Admin.Create("Firma Yöneticisi", $"yonetici-{Guid.NewGuid():N}@example.com", "digest-placeholder", now);
        db.Admins.Add(admin);
        await db.SaveChangesAsync(Ct);
        return admin;
    }
}
