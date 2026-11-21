using System.Globalization;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
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
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical, ProductType.Service);
        await ExecuteAsync($"UPDATE orders SET payment_method = 'BankTransfer', bank_transfer_iban = '{TransferIban}' WHERE id = '{order.Id}'");
        order = await ReloadAsync(order.Id);
        Guid physical = await ItemIdAsync(order.Id, ProductType.Physical);
        Guid service = await ItemIdAsync(order.Id, ProductType.Service);

        PanelItemStepResult window = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, false, (physical, 2), (service, 2)), Ct));
        Assert.Equal((TransitionRefusal.ConfirmationRequired, true), (window.Refusal, window.IsTransferLine));

        PanelItemStepResult done = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true, (physical, 2), (service, 2)), Ct));

        Assert.Null(done.Refusal);
        Assert.Equal(2, await CountAsync(c => c.Cancellations.Where(x => x.OrderItemId == physical || x.OrderItemId == service)));
        Assert.Equal(0, await CountAsync(c => c.Refunds.Where(r => r.OrderId == order.Id)));
        Assert.NotEqual(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == physical || r.OrderItemId == service)));
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

        // The second line is a paid digital item, never open to a cancellation (02 7.1.6, 1.11.21): the first, valid line is not applied either (K-1188 (4)).
        PanelItemStepResult partly = await StepAsync(s => s.CancelByFirmAsync(FirmCancellation(order, true, (physical, 1), (digital, 1)), Ct));
        Assert.Equal(TransitionRefusal.ConditionNotMet, partly.Refusal);

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
    public async Task PanelWithdrawalNotice_RefusesTheItemTheUnitsTheDateAndTheOrderAnswer_BeforeTheWindow_T528_KK5()
    {
        DateOnly delivered = TurkeyTime.DateOf(Start).AddDays(-5);
        (Order order, Guid item) = await SeedDeliveredPhysicalAsync(delivered);
        DateOnly yesterday = TurkeyTime.DateOf(Start).AddDays(-1);

        Assert.Equal(PanelItemObstacle.ItemNotSelected, (await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, Guid.Empty, 1, yesterday, false), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.QuantityInvalid, (await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 0, yesterday, false), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.QuantityExceedsOpen, (await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 3, yesterday, false), Ct))).Obstacle);
        Assert.Equal(PanelItemObstacle.DateInvalid, (await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, yesterday.AddDays(2), false), Ct))).Obstacle);
        // A notice IBAN that fails OB-14 is no refusal (K-1631 (5); D-1599): PanelNotice_AnIbanThatFailsTheCheck_IsNoRefusal_… below.
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

    [Fact]
    public async Task PanelLateTermination_OnTheTransferLine_RecordsAWrongIbanWithoutIt_CarriesTheNoticeIban_AndWithoutOne_AsksForIt_T528_KK5_D1590_D1599()
    {
        Order carried = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Order asked = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        Order wrong = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET payment_method = 'BankTransfer', bank_transfer_iban = '{TransferIban}' WHERE id IN ('{carried.Id}', '{asked.Id}', '{wrong.Id}')");
        (carried, asked, wrong) = (await ReloadAsync(carried.Id), await ReloadAsync(asked.Id), await ReloadAsync(wrong.Id));
        _clock.SetUtcNow(DeliveryPeriodRules.LegalDeliveryLimitEnd(Start).AddHours(1));
        DateOnly today = TurkeyTime.DateOf(_clock.GetUtcNow());

        // K-1631 (5); 02 10.4.10: a notice IBAN that fails OB-14 is no refusal - the record is made without it and the item asks for one, as without an IBAN.
        PanelItemStepResult wrongIban = await StepAsync(s => s.RecordLateDeliveryTerminationAsync(Termination(wrong, true, today) with { Iban = "TR000000000000000000000000" }, Ct));
        Assert.Equal(((TransitionRefusal?)null, PanelItemObstacle.None), (wrongIban.Refusal, wrongIban.Obstacle));

        Assert.Null((await StepAsync(s => s.RecordLateDeliveryTerminationAsync(Termination(carried, true, today) with { Iban = TransferIban }, Ct))).Refusal);
        Assert.Null((await StepAsync(s => s.RecordLateDeliveryTerminationAsync(Termination(asked, true, today), Ct))).Refusal);

        Guid carriedItem = await ItemIdAsync(carried.Id, ProductType.Physical);
        Guid askedItem = await ItemIdAsync(asked.Id, ProductType.Physical);
        Guid wrongItem = await ItemIdAsync(wrong.Id, ProductType.Physical);
        Assert.Equal(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == carriedItem)));
        Assert.NotEqual(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == askedItem)));
        Assert.Equal(1, await CountAsync(c => c.LateDeliveryTerminations.Where(t => t.OrderItemId == wrongItem && t.RefundIbanId == null)));
        Assert.Equal(0, await CountAsync(c => c.RefundIbans.Where(i => i.OrderId == wrong.Id)));
        Assert.NotEqual(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == wrongItem)));
        Assert.Contains(await DispatchesAsync(wrong.Id), d => d.Type == DispatchType.B14);
    }

    // ---- D-1599, 12 VAL-A238 (K-1631 (5)): on the transfer line a notice from another address, without an IBAN or with an IBAN that fails OB-14 is recorded without an IBAN ----

    [Theory]
    [InlineData(false, TransferIban)]
    [InlineData(true, null)]
    [InlineData(true, "TR330006100519786457841327")]
    public async Task PanelNotice_FromAnotherAddress_WithoutAnIban_OrWithAnIbanThatFailsTheCheck_IsRecordedWithoutTheIban_AndTheItemAsksForItWithB14_D1599_VALA238(bool fromContactEmail, string? noticeIban)
    {
        (Order order, Guid item) = await SeedDeliveredPhysicalAsync(TurkeyTime.DateOf(Start).AddDays(-5));
        await ExecuteAsync($"UPDATE orders SET payment_method = 'BankTransfer', bank_transfer_iban = '{TransferIban}' WHERE id = '{order.Id}'");
        order = await ReloadAsync(order.Id);
        DateOnly yesterday = TurkeyTime.DateOf(Start).AddDays(-1);

        PanelItemStepResult result = await StepAsync(s => s.RecordWithdrawalAsync(Notice(order, item, 1, yesterday, true) with { CameFromOrderEmail = fromContactEmail, NoticeIban = noticeIban }, Ct));

        Assert.Equal(((TransitionRefusal?)null, PanelItemObstacle.None), (result.Refusal, result.Obstacle));
        Assert.Equal(1, await CountAsync(c => c.Withdrawals.Where(w => w.OrderItemId == item && w.RefundIbanId == null)));
        Assert.Equal(0, await CountAsync(c => c.RefundIbans.Where(i => i.OrderId == order.Id)));
        Assert.Equal(1, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item && r.Reason == RefundIbanRequestReason.NoticeWithoutIban && r.FulfilledAt == null)));
        Assert.Contains(await DispatchesAsync(order.Id), d => d.Type == DispatchType.B14);
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

    [Fact]
    public async Task PanelRemoval_TheWindowsAmount_IsTheRefundTheRecordThenWrites_TheFeeWithTheLastPhysicalUnit_AndNoneBeforeThePaymentCame_D1598()
    {
        // K-797 "{tutar}": the window's read and the record's refund line are one rule (02 7.1.7, 7.2.9). Two units of 300 with a coupon share of 20, a fee of 30.
        Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
        await ExecuteAsync($"UPDATE orders SET shipping_fee = 30, shipping_fee_vat_breakdown = '[]' WHERE id = '{order.Id}'");
        order = await ReloadAsync(order.Id);
        Guid item = await ItemIdAsync(order.Id, ProductType.Physical);

        decimal? first = await PreviewAsync(order.Id, item, 1);
        decimal? both = await PreviewAsync(order.Id, item, 2);
        Assert.Null(await PreviewAsync(order.Id, item, 3));
        Assert.Null((await StepAsync(s => s.RemoveItemAsync(Removal(order, item, 1), Ct))).Refusal);
        decimal firstWritten = await RefundedAsync(item);
        _clock.Advance(TimeSpan.FromMinutes(1)); // two operations, two moments: the fee's trigger reads the moves of one moment as one operation (K-1107)
        order = await ReloadAsync(order.Id);
        decimal? last = await PreviewAsync(order.Id, item, 1);
        Assert.Null((await StepAsync(s => s.RemoveItemAsync(Removal(order, item, 1), Ct))).Refusal);
        decimal lastWritten = await RefundedAsync(item) - firstWritten;

        Assert.Equal(firstWritten, first);
        Assert.Equal(lastWritten, last);
        Assert.Equal(300m - 10m, first); // the first unit: its price less half the coupon share, no fee - a unit still goes
        Assert.Equal(300m - 10m + 30m, last); // the last unit takes what is left of the coupon share and meets the fee's trigger
        Assert.Equal(600m - 20m + 30m, both);

        Order unpaid = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, ProductType.Physical);
        Assert.Null(await PreviewAsync(unpaid.Id, await ItemIdAsync(unpaid.Id, ProductType.Physical), 1));
    }

    // ---- helpers ----

    private async Task<decimal?> PreviewAsync(Guid orderId, Guid item, int quantity)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IPanelItemSteps>().RemovalRefundAsync(orderId, item, quantity, Ct);
    }

    // The sum of the item's refund lines (two lines written in one test moment have no order of their own).
    private async Task<decimal> RefundedAsync(Guid item)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Refunds.AsNoTracking().Where(r => r.OrderItemId == item).SumAsync(r => r.Amount, Ct);
    }

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
