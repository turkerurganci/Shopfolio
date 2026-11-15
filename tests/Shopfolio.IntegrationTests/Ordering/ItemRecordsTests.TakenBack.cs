using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The receipt of taken-back goods on a real PostgreSQL (T528 KK7, KK8; D-656, D-911; 03 1.11.28, 8.3.2.1; K-808): the goods of a late-delivery termination and of a defect claim's
/// defective units (the rescission starts with their receipt) pass the same receipt step as a withdrawal's - each arrival a record of its own linked to the termination or the claim, at most the units still to come back,
/// no refund and no IBAN - and the panel's "İade malını teslim al" takes them in when no declaration's goods are on the way back. "Stoğa ekle" reads the receipt like any other.
/// </summary>
public sealed partial class ItemRecordsTests
{
    [Fact]
    public async Task TerminationGoods_AreReceivedPerArrival_UpToTheTerminatedUnits_WithoutARefund_T528_KK7_D656()
    {
        (Order order, OrderItem item) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, ProductType.Physical, 3, delivered: false, exemption: null);
        LateDeliveryTermination termination = LateDeliveryTermination.ByBuyer(item.Id, 2, DeliveryDay, Amounts, null, Start);
        await SaveAsync(c => c.LateDeliveryTerminations.Add(termination));
        TakenBackSource source = TakenBackSource.Termination(termination.Id);
        Assert.Equal([2], await TakenBackAwaitingAsync(item.Id));

        int version = await VersionAsync(order.Id);
        DateOnly arrival = TurkeyTime.DateOf(Start);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReceiveTakenBackAsync(TakenBack(order, item, source, version, 3, arrival), t))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReceiveTakenBackAsync(TakenBack(order, item, source, version, 0, arrival), t))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReceiveTakenBackAsync(TakenBack(order, item, source, version, 1, arrival.AddDays(1)), t))).Refusal);
        Assert.Equal(0, await CountAsync(c => c.Returns.Where(r => r.OrderItemId == item.Id)));

        // Every arrival is its own record (KK7); the second takes the last unit, a third finds nothing on the way back.
        Assert.Null((await RunAsync((u, t) => u.ReceiveTakenBackAsync(TakenBack(order, item, source, version, 1, arrival.AddDays(-1)), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.ReceiveTakenBackAsync(TakenBack(order, item, source, version + 1, 1, arrival), t))).Refusal);
        Assert.Equal(
            TransitionRefusal.ConditionNotMet, (await RunAsync((u, t) => u.ReceiveTakenBackAsync(TakenBack(order, item, source, version + 2, 1, arrival), t))).Refusal);

        await using AppDbContext reader = NewContext();
        List<Return> receipts = await reader.Returns.Where(r => r.OrderItemId == item.Id).OrderBy(r => r.ArrivalDate).ToListAsync(Ct);
        Assert.Equal(2, receipts.Count);
        Assert.All(receipts, r => Assert.Equal((termination.Id, (Guid?)null, (Guid?)null, 1), (r.LateDeliveryTerminationId!.Value, r.WithdrawalId, r.DefectClaimId, r.Quantity)));
        Assert.Empty(await TakenBackAwaitingAsync(item.Id));
        Assert.Empty(await RefundsOfOrderAsync(order.Id));
        Assert.Equal(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item.Id)));
        Assert.Equal(version + 2, await VersionAsync(order.Id));
    }

    [Fact]
    public async Task ClaimedGoods_AreReceivedAgainstTheClaim_BeforeAnyRefund_UpToTheClaimsDefectiveUnits_AndTheReceiptIsOfferedToTheStockStep_T528_KK7_KK8_D911()
    {
        (Order order, OrderItem item) = await SeedClaimedAsync(3, PaymentMethod.Card);
        Guid claimId = (await FindClaimAsync(item.Id))!.Id;
        TakenBackSource source = TakenBackSource.Claim(claimId);
        DateOnly arrival = DeliveryDay.AddDays(2);

        // K-808, 04 7.2.12.2: the rescission starts with the receipt - its bound is the claim's defective units not taken back yet, with no refund made before it.
        int refunds = await CountAsync(c => c.Refunds.Where(r => r.OrderItemId == item.Id));
        Assert.Equal(0, refunds);
        Assert.Equal([ClaimUnits], await TakenBackAwaitingAsync(item.Id));
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReceiveTakenBackAsync(TakenBack(order, item, source, 2, ClaimUnits + 1, arrival), t))).Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, (await RunAsync((u, t) => u.ReceiveTakenBackAsync(TakenBack(order, item, source, 2, 1, DeliveryDay.AddDays(-1)), t))).Refusal);

        // The panel's step takes the claim's goods in when no declaration awaits (the quantity defaults to the defective units still to come back).
        PanelItemStepResult received = await StepAsync((s, t) => s.ReceiveReturnAsync(new PanelReturnArrival(order.Id, item.Id, AdminCaller, 2, null, arrival), t));
        Assert.Null(received.Refusal);

        await using AppDbContext reader = NewContext();
        Return receipt = await reader.Returns.SingleAsync(r => r.OrderItemId == item.Id, Ct);
        Assert.Equal(((Guid?)claimId, (Guid?)null, (Guid?)null, ClaimUnits), (receipt.DefectClaimId, receipt.WithdrawalId, receipt.LateDeliveryTerminationId, receipt.Quantity));
        Assert.Empty(await TakenBackAwaitingAsync(item.Id));
        Assert.Equal(refunds, await CountAsync(c => c.Refunds.Where(r => r.OrderItemId == item.Id)));

        // The refund follows the receipt (K-808); a rescission written afterwards opens no second receipt.
        Assert.Null((await RunAsync((u, t) => u.RescindDefectClaimAsync(new RescindDefectClaimRequest(order.Id, item.Id, AdminCaller, 3, ClaimUnits), t))).Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync((s, t) => s.ReceiveReturnAsync(new PanelReturnArrival(order.Id, item.Id, AdminCaller, 4, 1, arrival), t))).Refusal);

        // KK8: "Stoğa ekle" reads the receipt like a withdrawal's - not added yet, so the step is offered once.
        ReturnStepFacts steps = await ReturnStepsAsync(item.Id);
        Assert.Equal([(receipt.Id, 2, (DateTimeOffset?)null)], steps.Receipts.Select(r => (r.ReturnId, r.Quantity, r.StockAddedAt)));
    }

    [Fact]
    public async Task PanelReceipt_OfATerminationsGoods_TakesTheTerminatedUnits_AndARejectionIsNotOpen_T528_KK7_D656()
    {
        (Order order, OrderItem item) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, ProductType.Physical, 3, delivered: false, exemption: null);
        LateDeliveryTermination termination = LateDeliveryTermination.ByBuyer(item.Id, 3, DeliveryDay, Amounts, null, Start);
        await SaveAsync(c => c.LateDeliveryTerminations.Add(termination));
        int version = await VersionAsync(order.Id);
        DateOnly arrival = TurkeyTime.DateOf(Start);

        PanelItemStepResult rejection = await StepAsync((s, t) => s.RejectReturnAsync(new PanelReturnArrival(order.Id, item.Id, AdminCaller, version, 1, arrival, true), t));
        PanelItemStepResult tooMany = await StepAsync((s, t) => s.ReceiveReturnAsync(new PanelReturnArrival(order.Id, item.Id, AdminCaller, version, 4, arrival), t));
        Assert.NotNull(rejection.Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, tooMany.Refusal);
        Assert.Equal(0, await CountAsync(c => c.Returns.Where(r => r.OrderItemId == item.Id)));

        Assert.Null((await StepAsync((s, t) => s.ReceiveReturnAsync(new PanelReturnArrival(order.Id, item.Id, AdminCaller, version, null, arrival), t))).Refusal);
        await using AppDbContext reader = NewContext();
        Return receipt = await reader.Returns.SingleAsync(r => r.OrderItemId == item.Id, Ct);
        Assert.Equal(((Guid?)termination.Id, 3), (receipt.LateDeliveryTerminationId, receipt.Quantity));
        Assert.Equal(1, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
    }

    // ---- helpers ----

    private ReceiveTakenBackRequest TakenBack(Order order, OrderItem item, TakenBackSource source, int version, int quantity, DateOnly arrival) =>
        new(order.Id, item.Id, source, AdminCaller, version, quantity, arrival);

    private async Task<ReturnStepFacts> ReturnStepsAsync(Guid itemId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>().ReturnStepFactsAsync([itemId], Ct);
    }

    private async Task<List<int>> TakenBackAwaitingAsync(Guid itemId) => [.. (await ReturnStepsAsync(itemId)).TakenBack.Select(t => t.Awaiting)];
}
