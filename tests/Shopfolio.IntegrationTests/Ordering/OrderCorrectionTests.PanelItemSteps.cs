using System.Globalization;

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
/// The order screen's item steps on a real PostgreSQL (T528 KK1-KK6; 07 UC-129, UC-130, UC-138): the firm's cancellation stops at its window unconfirmed and applies the whole request or
/// nothing - the whole unpaid order without an item, the chosen units of a paid one, the card's refund or the transfer's IBAN request; the notice from another channel takes the item and
/// its units (the withdrawal) or none (the late delivery termination) and refuses its 422 reasons before the window; the removal asks the confirmation only on the card line. Each step
/// runs in the runner's transaction like the page's <c>OperationRunner</c>: a refusal writes nothing.
/// </summary>
public sealed partial class OrderCorrectionTests
{
    private const string TransferIban = "TR330006100519786457841326";

    // ---- KK1, KK3: the window, the two lines, the unpaid order ----

    [Fact]
    public async Task PanelFirmCancellation_AfterThePayment_StopsAtTheWindowUnconfirmed_ThenCancelsTheUnits_AndStartsTheCardRefund_T528_KK1_KK3()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        PanelItemStepResult window = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, false, (item, 1)), Ct));

        Assert.Equal((TransitionRefusal.ConfirmationRequired, false), (window.Refusal, window.IsTransferLine));
        Assert.Equal(0, await CountAsync(c => c.Cancellations));
        Assert.Equal(0, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));

        PanelItemStepResult done = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true, (item, 1)), Ct));

        Assert.Null(done.Refusal);
        Assert.Equal(1, await CountAsync(c => c.Cancellations.Where(x => x.OrderItemId == item && x.Quantity == 1)));
        Assert.Equal(1, await CountAsync(c => c.Refunds.Where(r => r.OrderId == order.Id)));
        Assert.Equal(1, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
        Assert.Equal(OrderStatus.Preparing, (await ReloadAsync(order.Id)).OrderStatus);
    }

    [Fact]
    public async Task PanelFirmCancellation_OnTheTransferLine_SaysSoInTheWindow_AndAsksTheIban_ForEveryChosenItem_InsteadOfARefund_T528_KK1_KK3()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical, ProductType.Digital);
        await ExecuteAsync($"UPDATE orders SET payment_method = 'BankTransfer', bank_transfer_iban = '{TransferIban}' WHERE id = '{order.Id}'");
        order = await ReloadAsync(order.Id);
        Guid physical = await ItemIdAsync(order.Id, ProductType.Physical);
        Guid digital = await ItemIdAsync(order.Id, ProductType.Digital);

        PanelItemStepResult window = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, false, (physical, 2), (digital, 1)), Ct));
        Assert.Equal((TransitionRefusal.ConfirmationRequired, true), (window.Refusal, window.IsTransferLine));

        PanelItemStepResult done = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true, (physical, 2), (digital, 1)), Ct));

        Assert.Null(done.Refusal);
        Assert.Equal(2, await CountAsync(c => c.Cancellations.Where(x => x.OrderItemId == physical || x.OrderItemId == digital)));
        Assert.Equal(0, await CountAsync(c => c.Refunds.Where(r => r.OrderId == order.Id)));
        Assert.NotEqual(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == physical || r.OrderItemId == digital)));
        Assert.Equal(2, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
        Assert.Equal(OrderStatus.Cancelled, (await ReloadAsync(order.Id)).OrderStatus);
    }

    [Fact]
    public async Task PanelFirmCancellation_BeforeThePayment_TakesNoItem_AndCancelsTheWholeOrder_T528_KK1_KK3()
    {
        Order order = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        PanelItemStepResult withItem = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true, (item, 1)), Ct));
        PanelItemStepResult window = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, false), Ct));
        Assert.Equal(TransitionRefusal.ConditionNotMet, withItem.Refusal);
        Assert.Equal((TransitionRefusal.ConfirmationRequired, false), (window.Refusal, window.IsTransferLine));
        Assert.Equal(OrderStatus.Placed, (await ReloadAsync(order.Id)).OrderStatus);

        PanelItemStepResult done = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true), Ct));

        Assert.Null(done.Refusal);
        Assert.Equal(OrderStatus.Cancelled, (await ReloadAsync(order.Id)).OrderStatus);
        Assert.Equal(0, await CountAsync(c => c.Refunds.Where(r => r.OrderId == order.Id)));
        Assert.Equal(1, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
    }

    // ---- KK2: the four 422 reasons; nothing applied ----

    [Fact]
    public async Task PanelFirmCancellation_RefusesTheReasonTheItemAndTheUnits_AndAppliesNoPartOfTheRequest_T528_KK2()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical, ProductType.Digital);
        Guid physical = await ItemIdAsync(order.Id, ProductType.Physical);
        Guid digital = await ItemIdAsync(order.Id, ProductType.Digital);

        PanelFirmCancellation valid = FirmCancellation(order, true, (physical, 1));
        Assert.Equal(PanelItemObstacle.ReasonIncomplete, (await StepAsync(s => s.CancelByFirmAsync(valid with { Reason = null }, Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.ReasonIncomplete, (await StepAsync(s => s.CancelByFirmAsync(valid with { Reason = CancellationReason.Other, ReasonNote = " " }, Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.ReasonIncomplete, (await StepAsync(s => s.CancelByFirmAsync(valid with { Reason = CancellationReason.AgreedWithBuyer }, Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.ItemNotSelected, (await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.QuantityInvalid, (await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true, (physical, 0)), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.QuantityExceedsOpen, (await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true, (physical, 3)), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.ItemSelectionInvalid, (await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true, (physical, 1), (physical, 1)), Ct))).Obstacle);

        // The second line is over its open units: the first, valid one is not applied either (K-1188 (4)).
        PanelItemStepResult partly = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true, (physical, 1), (digital, 2)), Ct));
        Assert.Equal((TransitionRefusal.InvalidInput, PanelItemObstacle.QuantityExceedsOpen), (partly.Refusal, partly.Obstacle));

        // The item is not open to the cancellation once the order was handed to the carrier.
        Order shipped = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, ProductType.Physical);
        Guid shippedItem = await ItemIdAsync(shipped.Id, ProductType.Physical);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(shipped, true, (shippedItem, 1)), Ct))).Refusal);

        Assert.Equal(0, await CountAsync(c => c.Cancellations));
        Assert.Equal(0, await CountAsync(c => c.Refunds));
        Assert.Equal(0, await CountAsync(c => c.AuditLogs));
    }

    // ---- KK4, KK5: the withdrawal notice ----

    [Fact]
    public async Task PanelWithdrawalNotice_WritesTheItemAndItsUnits_AfterTheWindow_T528_KK4()
    {
        (Order order, Guid item) = await SeedDeliveredPhysicalAsync(TurkeyTime.DateOf(Start).AddDays(-5));

        PanelItemStepResult window = await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, TurkeyTime.DateOf(Start).AddDays(-1), false), Ct));
        Assert.Equal(TransitionRefusal.ConfirmationRequired, window.Refusal);
        Assert.Equal(0, await CountAsync(c => c.Withdrawals));

        PanelItemStepResult done = await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, TurkeyTime.DateOf(Start).AddDays(-1), true), Ct));

        Assert.Null(done.Refusal);
        await using AppDbContext reader = NewContext();
        Withdrawal withdrawal = await reader.Withdrawals.SingleAsync(w => w.OrderItemId == item, Ct);
        Assert.Equal((1, ActorKind.Admin), (withdrawal.Quantity, withdrawal.ActorKind));
        Assert.Equal(1, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
    }

    [Fact]
    public async Task PanelWithdrawalNotice_RefusesTheItemTheUnitsTheDateTheIbanAndTheOrderAnswer_BeforeTheWindow_T528_KK5()
    {
        DateOnly delivered = TurkeyTime.DateOf(Start).AddDays(-5);
        (Order order, Guid item) = await SeedDeliveredPhysicalAsync(delivered);
        DateOnly yesterday = TurkeyTime.DateOf(Start).AddDays(-1);

        Assert.Equal(PanelItemObstacle.ItemNotSelected, (await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, Guid.Empty, 1, yesterday, false), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.QuantityInvalid, (await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 0, yesterday, false), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.QuantityExceedsOpen, (await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 3, yesterday, false), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.DateInvalid, (await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, yesterday.AddDays(2), false), Ct))).Obstacle);
        Assert.Equal(
            PanelItemObstacle.IbanInvalid,
            (await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, yesterday, false) with { CameFromOrderEmail = true, NoticeIban = "TR000006100519786457841326" }, Ct))).Obstacle);
        PanelItemStepResult sameDay = await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, delivered, false), Ct));
        Assert.Equal((TransitionRefusal.InvalidInput, PanelItemObstacle.DeliveryOrderMissing), (sameDay.Refusal, sameDay.Obstacle));

        // A physical item not handed to the carrier is the "müşteriyle anlaşıldı" cancellation's (K-604).
        Order preparing = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Guid unshipped = await ItemIdAsync(preparing.Id, ProductType.Physical);
        Assert.Equal(PanelItemObstacle.ItemNotShipped, (await StepAsync(s => s.RecordWithdrawalAsync(Notice(preparing, unshipped, 1, yesterday, false), Ct))).Obstacle);

        Assert.Equal(0, await CountAsync(c => c.Withdrawals));
        Assert.Equal(0, await CountAsync(c => c.AuditLogs));
    }

    [Fact]
    public async Task PanelWithdrawalNotice_OutsideTheWindow_WithoutTheSeparateBox_Is422BeforeTheWindow_AndWithItOpensTheWindow_T528_KK5()
    {
        (Order order, Guid item) = await SeedDeliveredPhysicalAsync(TurkeyTime.DateOf(Start).AddDays(-40));
        DateOnly yesterday = TurkeyTime.DateOf(Start).AddDays(-1);

        PanelItemStepResult unticked = await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, yesterday, false), Ct));
        PanelItemStepResult ticked = await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, yesterday, false) with { WarningsConfirmed = true }, Ct));

        Assert.Equal((TransitionRefusal.InvalidInput, PanelItemObstacle.WarningsUnconfirmed), (unticked.Refusal, unticked.Obstacle));
        Assert.True(unticked.Warnings.NeedsConfirmation);
        Assert.Equal(TransitionRefusal.ConfirmationRequired, ticked.Refusal);
        Assert.Null((await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, yesterday, true) with { WarningsConfirmed = true }, Ct))).Refusal);
        Assert.Equal(1, await CountAsync(c => c.Withdrawals.Where(w => w.OrderItemId == item)));
    }

    // ---- KK4, KK5: the late delivery termination notice ----

    [Fact]
    public async Task PanelLateTermination_TakesNoItem_FallsOnTheOpenUnits_AsksTheConfirmation_AndRefusesItsDateAndRange_T528_KK4_KK5()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);
        Order delivered = await SeedAsync(OrderStatus.Delivered, PaymentStatus.Paid, ProductType.Physical);
        _clock.SetUtcNow(DeliveryPeriodRules.LegalDeliveryLimitEnd(Start).AddHours(1));
        DateOnly today = TurkeyTime.DateOf(_clock.GetUtcNow());

        Assert.Equal(PanelItemObstacle.DateInvalid, (await StepAsync(s => s.RecordLateDeliveryTerminationAsync(Termination(order, true, null), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.DateInvalid, (await StepAsync(s => s.RecordLateDeliveryTerminationAsync(Termination(order, true, today.AddDays(1)), Ct))).Obstacle);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync(s => s.RecordLateDeliveryTerminationAsync(Termination(delivered, true, today), Ct))).Refusal);
        Assert.Equal(TransitionRefusal.ConfirmationRequired, (await StepAsync(s => s.RecordLateDeliveryTerminationAsync(Termination(order, false, today), Ct))).Refusal);
        Assert.Equal(0, await CountAsync(c => c.LateDeliveryTerminations));

        PanelItemStepResult done = await StepAsync(s => s.RecordLateDeliveryTerminationAsync(Termination(order, true, today), Ct));

        Assert.Null(done.Refusal);
        Assert.Equal(1, await CountAsync(c => c.LateDeliveryTerminations.Where(t => t.OrderItemId == item && t.Quantity == 2)));
        Assert.Equal(OrderStatus.Cancelled, (await ReloadAsync(order.Id)).OrderStatus);
        Assert.Equal(1, await CountAsync(c => c.AuditLogs.Where(a => a.TargetRecordId == order.Id)));
    }

    // ---- KK6: the removal's two lines and its refusals ----

    [Fact]
    public async Task PanelRemoval_AsksTheConfirmationOnTheCardLine_AndNotOnTheTransferLine_T528_KK6()
    {
        Order card = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Guid cardItem = await ItemIdAsync(card.Id, ProductType.Physical);

        PanelItemStepResult window = await StepAsync(s => s.RemoveItemAsync(Removal(card, cardItem, 1) with { Confirmed = false }, Ct));
        Assert.Equal(TransitionRefusal.ConfirmationRequired, window.Refusal);
        Assert.Equal(0, await CountAsync(c => c.ItemRemovals));
        Assert.Null((await StepAsync(s => s.RemoveItemAsync(Removal(card, cardItem, 1), Ct))).Refusal);
        Assert.Equal(1, await CountAsync(c => c.ItemRemovals.Where(r => r.OrderItemId == cardItem && r.Quantity == 1)));

        Order transfer = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET payment_method = 'BankTransfer', bank_transfer_iban = '{TransferIban}' WHERE id = '{transfer.Id}'");
        transfer = await ReloadAsync(transfer.Id);
        Guid transferItem = await ItemIdAsync(transfer.Id, ProductType.Physical);

        PanelItemStepResult direct = await StepAsync(s => s.RemoveItemAsync(Removal(transfer, transferItem, 1) with { Confirmed = false }, Ct));

        Assert.Null(direct.Refusal);
        Assert.Equal(1, await CountAsync(c => c.ItemRemovals.Where(r => r.OrderItemId == transferItem && r.Quantity == 1)));
    }

    [Fact]
    public async Task PanelRemoval_RefusesTheUnitsTheDeliveredDigitalItemTheUnpaidOrderAndTheShippedOne_T528_KK6()
    {
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical, ProductType.Digital);
        Guid physical = await ItemIdAsync(order.Id, ProductType.Physical);
        Guid digital = await ItemIdAsync(order.Id, ProductType.Digital);
        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = now(), delivery_date = current_date WHERE id = '{digital}'");
        order = await ReloadAsync(order.Id);

        Assert.Equal(PanelItemObstacle.QuantityInvalid, (await StepAsync(s => s.RemoveItemAsync(Removal(order, physical, 0), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.QuantityExceedsOpen, (await StepAsync(s => s.RemoveItemAsync(Removal(order, physical, 3), Ct))).Obstacle);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync(s => s.RemoveItemAsync(Removal(order, digital, 1), Ct))).Refusal);

        Order unpaid = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, ProductType.Physical);
        Guid unpaidItem = await ItemIdAsync(unpaid.Id, ProductType.Physical);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync(s => s.RemoveItemAsync(Removal(unpaid, unpaidItem, 1), Ct))).Refusal);

        Order shipped = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, ProductType.Physical);
        Guid shippedItem = await ItemIdAsync(shipped.Id, ProductType.Physical);
        Assert.Equal(TransitionRefusal.ConditionNotMet, (await StepAsync(s => s.RemoveItemAsync(Removal(shipped, shippedItem, 1), Ct))).Refusal);

        Assert.Equal(0, await CountAsync(c => c.ItemRemovals));
    }

    // ---- helpers ----

    private PanelFirmCancellation FirmCancellation(Order order, bool confirmed, params (Guid Item, int Quantity)[] items) =>
        new(order.Id, AdminCaller, ShownVersionOf(order.Id), confirmed, CancellationReason.OutOfStock, null, false, [.. items.Select(i => new PanelItemUnits(i.Item, i.Quantity))]);

    private RecordWithdrawalRequest Notice(Order order, Guid item, int quantity, DateOnly date, bool confirmed) =>
        new(order.Id, item, AdminCaller, ShownVersionOf(order.Id), confirmed, quantity, date, false);

    private TerminateForLateDeliveryRequest Termination(Order order, bool confirmed, DateOnly? date) =>
        new(order.Id, AdminCaller, ShownVersionOf(order.Id), confirmed, date);

    /// <summary>A delivered card order whose physical item (two units) carries <paramref name="deliveryDate"/>.</summary>
    private async Task<(Order Order, Guid Item)> SeedDeliveredPhysicalAsync(DateOnly deliveryDate)
    {
        Order order = await SeedAsync(OrderStatus.Delivered, PaymentStatus.Paid, ProductType.Physical);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);
        string day = deliveryDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = '{day} 12:00:00+00', delivery_date = '{day}' WHERE id = '{item}'");
        return (await ReloadAsync(order.Id), item);
    }

    private async Task<PanelItemStepResult> StepAsync(Func<IPanelItemSteps, Task<PanelItemStepResult>> work)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IPanelItemSteps steps = scope.ServiceProvider.GetRequiredService<IPanelItemSteps>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(_ => work(steps), Ct);
    }
}
