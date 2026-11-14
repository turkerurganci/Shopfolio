using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The order screen's return and claim steps on a real PostgreSQL (T528 KK7-KK10; 07 UC-140, UC-141, UC-142, UC-144, UC-145): every receipt is a record of its own and starts no refund,
/// the rejection only on a conditional exemption and only confirmed, "Stoğa ekle" once per receipt, the "mal dönmedi" closure only after Z-42 and without a refund decision, the claim's
/// resolution and reopening with the status history and never a new claim. Each step runs in the runner's transaction like the page's <c>OperationRunner</c>: a refusal writes nothing.
/// </summary>
public sealed partial class ItemRecordsTests
{
    // ---- KK7: each receipt its own record; no refund; the bounds ----

    [Fact]
    public async Task PanelReceipt_IsARecordOfItsOwnEachTime_StartsNoRefund_AndRefusesTheDateAndQuantityBounds_T528_KK7()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, null);
        Assert.False((await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 3, DeliveryDay.AddDays(1)), t))).IsRejected);
        int version = await VersionAsync(order.Id);

        PanelItemStepResult future = await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, version, 1, TurkeyTime.DateOf(Start).AddDays(1)), t));
        PanelItemStepResult beforeDelivery = await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, version, 1, DeliveryDay.AddDays(-1)), t));
        PanelItemStepResult none = await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, version, 0, DeliveryDay.AddDays(2)), t));
        PanelItemStepResult tooMany = await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, version, 4, DeliveryDay.AddDays(2)), t));
        PanelItemStepResult stale = await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, version - 1, 1, DeliveryDay.AddDays(2)), t));

        Assert.Equal((TransitionRefusal.InvalidInput, PanelItemObstacle.DateInvalid), (future.Refusal, future.Obstacle));
        Assert.Equal((TransitionRefusal.InvalidInput, PanelItemObstacle.DateInvalid), (beforeDelivery.Refusal, beforeDelivery.Obstacle));
        Assert.Equal((TransitionRefusal.InvalidInput, PanelItemObstacle.QuantityInvalid), (none.Refusal, none.Obstacle));
        Assert.Equal((TransitionRefusal.InvalidInput, PanelItemObstacle.QuantityExceedsOpen), (tooMany.Refusal, tooMany.Obstacle));
        Assert.Equal(TransitionRefusal.VersionConflict, stale.Refusal);
        Assert.Equal(0, await CountAsync(c => c.Returns.Where(r => r.OrderItemId == item.Id)));

        Assert.Null((await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, version, 1, DeliveryDay.AddDays(2)), t))).Refusal);
        int next = await VersionAsync(order.Id);
        Assert.Null((await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, next, 2, DeliveryDay.AddDays(3)), t))).Refusal);

        await using AppDbContext reader = NewContext();
        List<int> receipts = await reader.Returns.Where(r => r.OrderItemId == item.Id).OrderBy(r => r.CreatedAt).ThenBy(r => r.Quantity).Select(r => r.Quantity).ToListAsync(Ct);
        Assert.Equal([1, 2], receipts);
        Assert.Empty(await RefundsOfOrderAsync(order.Id));
        Assert.Equal(2, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
    }

    [Fact]
    public async Task PanelRejection_IsOnlyOnAConditionalExemption_AndOnlyConfirmed_T528_KK7()
    {
        (Order plain, OrderItem plainItem) = await SeedDeliveredAsync(2, null);
        Assert.False((await WriteAsync(plain.Id, (m, t) => m.AddWithdrawalAsync(Declaration(plain, plainItem, 2, DeliveryDay.AddDays(1)), t))).IsRejected);
        PanelItemStepResult notOpen = await StepAsync((s, t) => s.RejectReturnAsync(Arrival(plain, plainItem, 2, 1, DeliveryDay.AddDays(2)) with { Confirmed = true }, t));
        Assert.Equal(TransitionRefusal.ConditionNotMet, notOpen.Refusal);

        (Order order, OrderItem item) = await SeedDeliveredAsync(2, WithdrawalExemptionReason.HygieneSensitive);
        Assert.False((await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 2, DeliveryDay.AddDays(1)), t))).IsRejected);

        PanelItemStepResult window = await StepAsync((s, t) => s.RejectReturnAsync(Arrival(order, item, 2, 1, DeliveryDay.AddDays(2)), t));
        Assert.Equal(TransitionRefusal.ConfirmationRequired, window.Refusal);
        Assert.Equal(0, await CountAsync(c => c.ReturnRejections.Where(r => r.OrderItemId == item.Id)));

        PanelItemStepResult rejected = await StepAsync((s, t) => s.RejectReturnAsync(Arrival(order, item, 2, 1, DeliveryDay.AddDays(2)) with { Confirmed = true }, t));

        Assert.Null(rejected.Refusal);
        Assert.Equal(1, await CountAsync(c => c.ReturnRejections.Where(r => r.OrderItemId == item.Id && r.Quantity == 1)));
        Assert.Equal(0, await CountAsync(c => c.Returns.Where(r => r.OrderItemId == item.Id)));
        Assert.Equal(1, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
    }

    // ---- KK8: "Stoğa ekle" once per receipt; the rejection and the deleted variant are not open ----

    [Fact]
    public async Task PanelRestock_AddsTheReceivedUnitsOnce_AndTheSecondAddition_IsRefused_T528_KK8()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, null);
        Guid variantId = await SeedReturnVariantAsync(item.Id, 5);
        Assert.False((await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 3, DeliveryDay.AddDays(1)), t))).IsRejected);
        Assert.Null((await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, 2, 2, DeliveryDay.AddDays(2)), t))).Refusal);
        Guid receiptId = await ReceiptIdAsync(item.Id);
        Assert.Equal(5, await StockAsync(variantId));

        PanelItemStepResult added = await StepAsync((s, t) => s.RestockReturnAsync(order.Id, receiptId, AdminCaller, 3, t));
        PanelItemStepResult again = await StepAsync((s, t) => s.RestockReturnAsync(order.Id, receiptId, AdminCaller, 4, t));
        PanelItemStepResult unknown = await StepAsync((s, t) => s.RestockReturnAsync(order.Id, Guid.NewGuid(), AdminCaller, 4, t));

        Assert.Null(added.Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, again.Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, unknown.Refusal);
        Assert.Equal(5 + 2, await StockAsync(variantId));
    }

    [Fact]
    public async Task PanelRestock_OfAReceiptWhoseVariantIsGone_IsRefused_T528_KK8()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(1, null);
        Assert.False((await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 1, DeliveryDay.AddDays(1)), t))).IsRejected);
        Assert.Null((await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, 2, 1, DeliveryDay.AddDays(2)), t))).Refusal);
        Guid receiptId = await ReceiptIdAsync(item.Id);

        PanelItemStepResult added = await StepAsync((s, t) => s.RestockReturnAsync(order.Id, receiptId, AdminCaller, 3, t));

        Assert.Equal(TransitionRefusal.ConditionNotMet, added.Refusal);
        await using AppDbContext reader = NewContext();
        Assert.Null((await reader.Returns.SingleAsync(r => r.Id == receiptId, Ct)).StockAddedAt);
    }

    // ---- KK9: the Z-42 boundary; no refund decision; a later receipt reopens the item ----

    [Fact]
    public async Task PanelNotReturnedClosure_IsRefusedBeforeZ42_ClosesTheUnitsAfterIt_DecidesNoRefund_AndALaterReceiptReopens_T528_KK9_KK7()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(2, null);
        Assert.False((await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 2, DeliveryDay.AddDays(1)), t))).IsRejected);

        _clock.SetUtcNow(TurkeyTime.StartOf(DeliveryDay.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + 1)).AddHours(12));
        PanelItemStepResult early = await StepAsync((s, t) => s.CloseNotReturnedAsync(order.Id, item.Id, null, AdminCaller, 2, t));
        Assert.Equal(TransitionRefusal.ConditionNotMet, early.Refusal);

        _clock.SetUtcNow(TurkeyTime.StartOf(DeliveryDay.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + 2)).AddHours(12));
        PanelItemStepResult none = await StepAsync((s, t) => s.CloseNotReturnedAsync(order.Id, item.Id, 0, AdminCaller, 2, t));
        PanelItemStepResult tooMany = await StepAsync((s, t) => s.CloseNotReturnedAsync(order.Id, item.Id, 3, AdminCaller, 2, t));
        Assert.Equal((TransitionRefusal.InvalidInput, PanelItemObstacle.QuantityInvalid), (none.Refusal, none.Obstacle));
        Assert.Equal((TransitionRefusal.InvalidInput, PanelItemObstacle.QuantityExceedsOpen), (tooMany.Refusal, tooMany.Obstacle));
        Assert.Equal(0, await CountAsync(c => c.NotReturnedClosures.Where(n => n.OrderItemId == item.Id)));

        PanelItemStepResult closed = await StepAsync((s, t) => s.CloseNotReturnedAsync(order.Id, item.Id, null, AdminCaller, 2, t));

        Assert.Null(closed.Refusal);
        Assert.Equal(2, await CountAsync(c => c.NotReturnedClosures.Where(n => n.OrderItemId == item.Id && n.Quantity == 2)));
        Assert.Empty(await RefundsOfOrderAsync(order.Id));

        // The goods arrive after all: the receipt reopens the closed units (07 UC-140), still without a refund of its own.
        PanelItemStepResult received = await StepAsync((s, t) => s.ReceiveReturnAsync(Arrival(order, item, 3, 1, DeliveryDay.AddDays(20)), t));
        Assert.Null(received.Refusal);
        Assert.Equal(1, await CountAsync(c => c.Returns.Where(r => r.OrderItemId == item.Id)));
        Assert.Empty(await RefundsOfOrderAsync(order.Id));
    }

    // ---- KK10: resolve, reopen inside Z-18, the history; never a new claim ----

    [Fact]
    public async Task PanelClaimSteps_ResolveAndReopenTheSameClaim_WithTheStatusHistory_AndRefuseTheWrongStatusAndZ18_T528_KK10()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(2, null);
        Assert.Null((await RunAsync((u, t) => u.OpenDefectClaimAsync(new OpenDefectClaimRequest(order.Id, item.Id, 1, "Kusur"), t))).Refusal);
        Guid claimId = (await FindClaimAsync(item.Id))!.Id;

        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync((s, t) => s.ReopenDefectClaimAsync(order.Id, claimId, AdminCaller, 2, t))).Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync((s, t) => s.ResolveDefectClaimAsync(order.Id, Guid.NewGuid(), AdminCaller, 2, t))).Refusal);
        Assert.Equal(TransitionRefusal.VersionConflict, (await StepAsync((s, t) => s.ResolveDefectClaimAsync(order.Id, claimId, AdminCaller, 1, t))).Refusal);

        _clock.SetUtcNow(Start.AddHours(1));
        Assert.Null((await StepAsync((s, t) => s.ResolveDefectClaimAsync(order.Id, claimId, AdminCaller, 2, t))).Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync((s, t) => s.ResolveDefectClaimAsync(order.Id, claimId, AdminCaller, 3, t))).Refusal);
        _clock.SetUtcNow(Start.AddHours(2));
        Assert.Null((await StepAsync((s, t) => s.ReopenDefectClaimAsync(order.Id, claimId, AdminCaller, 3, t))).Refusal);

        await using (AppDbContext reader = NewContext())
        {
            DefectClaim claim = await reader.DefectClaims.SingleAsync(c => c.OrderItemId == item.Id, Ct);
            Assert.Equal((claimId, DefectClaimStatus.Open), (claim.Id, claim.DefectClaimStatus));
            (DateTimeOffset, DefectClaimStatus, ActorKind)[] expected = [(Start.AddHours(1), DefectClaimStatus.Resolved, ActorKind.Admin), (Start.AddHours(2), DefectClaimStatus.Open, ActorKind.Admin)];
            Assert.Equal(expected, claim.StatusHistory.Select(e => (e.At, e.Status, e.ActorKind)));
        }

        // Z-18: the day after the delivery date two years on, the resolved claim is not reopened.
        Assert.Null((await StepAsync((s, t) => s.ResolveDefectClaimAsync(order.Id, claimId, AdminCaller, 4, t))).Refusal);
        _clock.SetUtcNow(DeliveredAt.AddYears(AfterSalesPeriodRules.DefectClaimYears).AddDays(1));
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync((s, t) => s.ReopenDefectClaimAsync(order.Id, claimId, AdminCaller, 5, t))).Refusal);
        Assert.Equal(1, await CountAsync(c => c.DefectClaims.Where(c2 => c2.OrderItemId == item.Id)));
        Assert.Equal(DefectClaimStatus.Resolved, (await FindClaimAsync(item.Id))!.Status);
    }

    // ---- helpers ----

    private PanelReturnArrival Arrival(Order order, OrderItem item, int version, int? quantity, DateOnly arrival) =>
        new(order.Id, item.Id, AdminCaller, version, quantity, arrival);

    private async Task<Guid> ReceiptIdAsync(Guid itemId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Returns.Where(r => r.OrderItemId == itemId).Select(r => r.Id).SingleAsync(Ct);
    }

    private async Task<PanelItemStepResult> StepAsync(Func<IPanelItemSteps, CancellationToken, Task<PanelItemStepResult>> work)
    {
        await using ServiceProvider provider = BuildProvider(new PanelActor(_admin!));
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IPanelItemSteps steps = scope.ServiceProvider.GetRequiredService<IPanelItemSteps>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(steps, token), Ct);
    }

    private sealed class PanelActor : IAuditActorSource
    {
        private readonly Admin _signedIn;

        public PanelActor(Admin signedIn)
        {
            _signedIn = signedIn;
        }

        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(_signedIn.Id, _signedIn.Name, _signedIn.Email));
    }
}
