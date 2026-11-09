using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The defect claim's money remedies on a real PostgreSQL (T374 KK8; 02 7.5.3, 7.1.6, 7.1.7; K-1077, K-686): the rescission starts the rescission's refund with the units'
/// shares and its units leave the open quantity without closing the item; the price reduction starts an amount-based refund within the paid money; on the bank transfer
/// line without an IBAN the item's request is written once and B-14 goes. The expected shares come from <see cref="ItemShares"/>, the one rule of 02 7.1.7.
/// </summary>
public sealed partial class ItemRecordsTests
{
    private const int ClaimUnits = 2;

    /// <summary>The paid total seeded on the remedies' orders: three units of 100 less the item's coupon share of 20 (the seed's 20 is the VAT rate).</summary>
    private const decimal PaidTotal = 280m;

    /// <summary>The firm's IBAN a bank transfer order freezes (06 4.17: the transfer order carries it).</summary>
    private const string FirmIban = "TR330006100519786457841326";

    [Fact]
    public async Task Rescission_StartsTheRescissionRefund_WithTheUnitsShares_AndTheUnitsLeaveTheOpenQuantity_WithoutClosingTheItem_KK8()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.Card);

        ItemRecordResult result = await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 2, 1), t));

        Assert.Null(result.Refusal);
        Refund refund = await RefundAsync(result.RecordId!.Value);
        ItemAmounts expected = ItemShares.SetAside(new ItemFrozenAmounts(100m, 20m, 16.67m), 3, 1, false, 0m, 0m);
        DefectClaimInfo claim = (await FindClaimAsync(item.Id))!;
        Assert.Equal(
            (RefundType.DefectClaimRescission, PaymentMethod.Card, (Guid?)item.Id, (int?)1, (decimal?)expected.CouponShare, (decimal?)expected.VatAmount, (Guid?)claim.Id),
            (refund.RefundType, refund.PaymentMethod, refund.OrderItemId, refund.Quantity, refund.CouponShare, refund.VatAmount, refund.DefectClaimId));
        Assert.Equal(expected.ProductAmount - expected.CouponShare, refund.Amount);
        Assert.Null(refund.SentAt);
        Assert.Equal(ActorKind.Admin, refund.ActorKind);

        // K-1077: the unit is no other operation's subject, but the item is not closed by it - the closure rule keeps reading the closed units.
        Assert.Equal(0, await ClosedAsync(item.Id));
        Assert.Equal(1, await UnavailableAsync(item.Id));
        Assert.Equal(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item.Id)));
        await AssertOrderAsync(order.Id, 3);
    }

    [Fact]
    public async Task Rescission_TakesAtMostTheClaimsUnitsNotRescindedYet_AndOnlyOnAnOpenClaim_KK8()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.Card);

        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 2, ClaimUnits + 1), t))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 2, 0), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 2, ClaimUnits), t))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 3, 1), t))).Refusal);

        Assert.Null((await RunAsync((u, t) => u.ResolveDefectClaimAsync(new ResolveDefectClaimRequest(order.Id, item.Id, AdminCaller, 3), t))).Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 4, 10m), t))).Refusal);

        (Order other, OrderItem unclaimed) = await SeedDeliveredAsync(2, null);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(other.Id, unclaimed.Id, AdminCaller, 1, 1), t))).Refusal);
        Assert.Equal(1, await CountAsync(c => c.Refunds.Where(r => r.OrderItemId == item.Id)));
    }

    [Fact]
    public async Task Rescission_OfTheLastOpenUnits_TakesWhatTheEarlierRecordsLeft_KK8()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.Card, claimUnits: 3);

        Assert.Null((await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 2, 2), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 3, 1), t))).Refusal);

        await using AppDbContext reader = NewContext();
        List<Refund> refunds = await reader.Refunds.Where(r => r.OrderItemId == item.Id).ToListAsync(Ct);
        Assert.Equal((3, 20m, 16.67m, 280m), (refunds.Sum(r => r.Quantity ?? 0), refunds.Sum(r => r.CouponShare ?? 0m), refunds.Sum(r => r.VatAmount ?? 0m), refunds.Sum(r => r.Amount)));
        Assert.Equal(3, await UnavailableAsync(item.Id));
    }

    [Fact]
    public async Task PriceReduction_StartsAnAmountBasedRefundOfTheItem_AndTakesNoUnit_KK8()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.Card);

        ItemRecordResult result = await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 2, 40m), t));

        Assert.Null(result.Refusal);
        Refund refund = await RefundAsync(result.RecordId!.Value);
        Assert.Equal(
            (RefundType.AmountBased, 40m, (Guid?)item.Id, (int?)null, (Guid?)null, (DateTimeOffset?)null),
            (refund.RefundType, refund.Amount, refund.OrderItemId, refund.Quantity, refund.DefectClaimId, refund.SentAt));
        Assert.Equal(0, await UnavailableAsync(item.Id));
        await AssertOrderAsync(order.Id, 3);
    }

    [Fact]
    public async Task PriceReduction_IsCappedAtThePaidMoneyNotRefunded_TheStartedRefundsIncluded_KK8()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.Card);

        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 2, 0m), t))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 2, 1.005m), t))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 2, PaidTotal + 0.01m), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 2, 200m), t))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 3, PaidTotal - 200m + 0.01m), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 3, PaidTotal - 200m), t))).Refusal);
        Assert.Equal(2, await CountAsync(c => c.Refunds.Where(r => r.OrderId == order.Id)));
    }

    [Fact]
    public async Task Remedies_OnTheTransferLineWithoutAnIban_RequestItOnce_AndSendB14Once_KK8()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.BankTransfer);

        ItemRecordResult rescinded = await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 2, 1), t));
        Assert.Null(rescinded.Refusal);
        Assert.Null((await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 3, 10m), t))).Refusal);

        await using AppDbContext reader = NewContext();
        RefundIbanRequest request = await reader.RefundIbanRequests.SingleAsync(r => r.OrderItemId == item.Id, Ct);
        Assert.Equal((RefundIbanRequestReason.AdminRequest, (Guid?)null, (Guid?)_admin!.Id), (request.Reason, request.RefundId, request.AdminId));
        Assert.Equal(1, await reader.DispatchRecords.CountAsync(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B14, Ct));
        Assert.Equal(PaymentMethod.BankTransfer, (await RefundAsync(rescinded.RecordId!.Value)).PaymentMethod);
    }

    [Fact]
    public async Task Remedies_OnTheTransferLineWithAnIbanOnFile_AskNothing_KK8()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.BankTransfer);
        await SaveAsync(c => c.RefundIbans.Add(RefundIban.Create(order.Id, "TR330006100519786457841326", Start)));

        Assert.Null((await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 2, 10m), t))).Refusal);

        Assert.Equal(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item.Id)));
        Assert.Equal(0, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B14)));
    }

    [Fact]
    public async Task Remedies_WithoutMoneyToPayBack_OrByANonAdministrator_AreRefused_AndWriteNothing_KK8()
    {
        const PaymentStatus payment = PaymentStatus.Refunded;
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.Card);
        TransitionAdmin nobody = new(Guid.Empty, " ");
        Assert.Equal(TransitionRefusal.ActorNotAllowed, (await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, nobody, 2, 1), t))).Refusal);
        Assert.Equal(TransitionRefusal.ActorNotAllowed, (await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, nobody, 2, 1m), t))).Refusal);

        await ExecuteAsync($"UPDATE orders SET payment_status = '{payment}' WHERE id = '{order.Id}'");
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 2, 1), t))).Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await RunAsync((u, t) => u.ReduceDefectClaimPriceAsync(new ReduceDefectClaimPriceRequest(order.Id, item.Id, AdminCaller, 2, 1m), t))).Refusal);
        Assert.Equal(0, await CountAsync(c => c.Refunds.Where(r => r.OrderId == order.Id)));
    }

    [Fact]
    public async Task OperationDecision_ReadsTheRescindedUnitsOutOfTheOpenQuantity_LikeTheWriters_D842()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.Card);
        Assert.Null((await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 2, 1), t))).Refusal);

        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderOperations operations = scope.ServiceProvider.GetRequiredService<IOrderOperations>();
        Assert.Equal(OperationDecision.QuantityExceedsOpen, (await operations.CheckAsync(order.Id, OrderOperation.WithdrawPhysicalItem, item.Id, 3, Ct)).Decision);
        Assert.Equal(OperationDecision.Allowed, (await operations.CheckAsync(order.Id, OrderOperation.WithdrawPhysicalItem, item.Id, 2, Ct)).Decision);
    }

    // ---- helpers ----

    /// <summary>A delivered physical item with the customer's open claim on <paramref name="claimUnits"/> units; the order's version is 2 after the claim.</summary>
    private async Task<(Order Order, OrderItem Item)> SeedClaimedAsync(int quantity, PaymentMethod method, int claimUnits = ClaimUnits)
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(quantity, null);
        string iban = method == PaymentMethod.BankTransfer ? $"'{FirmIban}'" : "NULL";
        string total = PaidTotal.ToString(System.Globalization.CultureInfo.InvariantCulture);
        await ExecuteAsync($"UPDATE orders SET payment_method = '{method}', bank_transfer_iban = {iban}, total = {total} WHERE id = '{order.Id}'");
        await ExecuteAsync($"UPDATE order_items SET coupon_share = 20 WHERE id = '{item.Id}'");
        ItemRecordResult opened = await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, claimUnits, "Kusurlu"), t));
        Assert.Null(opened.Refusal);
        return (order, item);
    }

    private async Task<Refund> RefundAsync(Guid refundId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Refunds.SingleAsync(r => r.Id == refundId, Ct);
    }

    private async Task<int> UnavailableAsync(Guid itemId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IReadOnlyDictionary<Guid, int> units = await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>().UnavailableQuantitiesAsync([itemId], Ct);
        return units.GetValueOrDefault(itemId);
    }
}
