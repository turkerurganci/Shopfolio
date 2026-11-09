using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>03 2.10.3 - the parcel withdrawn in transit and returned (T390 KK5-KK7); 03 2.10.4 - the return goods that arrive after the IBAN was deleted (KK8).</summary>
public sealed partial class BranchFlowTests
{
    private const decimal ParcelPrice = 300m;
    private const decimal ParcelShippingFee = 30m;
    private const int ParcelStock = 5;

    [Fact]
    public async Task TheParcelWithdrawnInTransit_ClosesTheItem_ReturnsByS9WithoutACancellation_AndIsRefundedToTheIbanWithTheFee_KK1_KK5_KK6_KK7_KK10()
    {
        // The guest's single physical item paid by transfer; the administrator ships it: Shipped + Paid (the narrative's start).
        (Order order, List<OrderItem> items) = await SeedOrderAsync(
            PaymentMethod.BankTransfer, true, OrderStatus.Preparing, PaymentStatus.Paid, ParcelShippingFee, Start.AddDays(-1), new ItemSpec(ProductType.Physical, 1, ParcelPrice));
        OrderItem item = items[0];
        Guid variantId = await SeedVariantAsync(order.Id, item.Id, ParcelStock, 0);
        int version = await VersionAsync(order.Id);
        Assert.False((await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().ShipOrderAsync(new ShipOrderRequest(order.Id, AdminCaller, version, true, Shipment), Ct))).IsRejected);
        await AssertAxesAsync(order.Id, OrderStatus.Shipped, PaymentStatus.Paid);

        // ---- 1. Declaration (2.10.3 step 1; KK5): the cancellation is closed, the withdrawal open; the IBAN and the return address are written, B-9 and F-3 go ----
        _clock.Advance(TimeSpan.FromDays(1));
        DateOnly declarationDay = TurkeyTime.DateOf(_clock.GetUtcNow());
        OrderOperationsView inTransit = await BuyerViewAsync(order.Id);
        Assert.False(inTransit.IsOpen(OrderOperation.CancelPhysicalItem, item.Id));
        Assert.True(inTransit.IsOpen(OrderOperation.WithdrawPhysicalItem, item.Id));
        WithdrawalResult declared = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().DeclareWithdrawalAsync(
            new DeclareWithdrawalRequest(order.Id, item.Id, 1, true, RefundIbanText), Ct));
        Assert.False(declared.IsRejected);
        Assert.Null(declared.Closure);
        Guid withdrawalId = declared.WithdrawalId!.Value;
        Withdrawal withdrawal;
        RefundIban iban;
        await using (AppDbContext reader = NewContext())
        {
            withdrawal = await reader.Withdrawals.AsNoTracking().SingleAsync(w => w.Id == withdrawalId, Ct);
            iban = await reader.RefundIbans.AsNoTracking().SingleAsync(i => i.Id == withdrawal.RefundIbanId, Ct);
        }

        Assert.Equal(RefundIbanText, iban.Iban);
        Assert.Equal(("Depo Sokak 3", (Guid?)_returnProvince!.Id, declarationDay), (withdrawal.ReturnAddressLine, withdrawal.ReturnProvinceId, withdrawal.DeclarationDate));
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B9));
        Assert.Equal(1, await CountFirmDispatchesAsync(DispatchType.F3, withdrawalId));

        // The item is closed, but the parcel is with the carrier: the order stays Shipped; the refund period runs from the declaration, not from the goods' return.
        Assert.Equal(1, await ClosedUnitsAsync(item.Id));
        await AssertAxesAsync(order.Id, OrderStatus.Shipped, PaymentStatus.Paid);
        bool afterDelivery = AfterSalesPeriodRules.IsAfterDelivery(ProductType.Physical, null, withdrawal.DeclarationDate, withdrawal.IsDeliveredBeforeDeclaration);
        Assert.False(afterDelivery);
        Assert.Equal(declarationDay, AfterSalesPeriodRules.WithdrawalRefundStartDay(afterDelivery, withdrawal.DeclarationDate, null));

        // ---- 2. Return (step 2; KK6): "teslim edilemedi" sends B-6; the closure is S9 with no cancellation record, no second refund and no B-7 ----
        _clock.Advance(TimeSpan.FromDays(2));
        version = await VersionAsync(order.Id);
        Assert.False((await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().MarkDeliveryFailedAsync(new AdminOrderRequest(order.Id, AdminCaller, version), Ct))).IsRejected);
        await AssertAxesAsync(order.Id, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B6));

        version = await VersionAsync(order.Id);
        OrderTransitionResult closed = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CloseReturnedOrderAsync(new AdminOrderRequest(order.Id, AdminCaller, version), Ct));
        Assert.False(closed.IsRejected);
        await AssertAxesAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Paid);
        Assert.Equal(TransitionCode.S9, (await CodesAsync(order.Id))[^1]);
        Assert.Equal(0L, await CountAsync($"cancellations WHERE order_item_id = '{item.Id}'"));
        Assert.Equal(0L, await CountAsync($"refunds WHERE order_id = '{order.Id}'"));
        Assert.Equal(0, await CountDispatchesAsync(order.Id, DispatchType.B7));

        // ---- 3. Receipt (step 3; KK6): no delivery date, so the arrival date has no lower bound; the receipt does not start the period; the stock step adds the unit ----
        DateOnly arrival = declarationDay.AddDays(-30);
        version = await VersionAsync(order.Id);
        ItemRecordResult received = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().ReceiveReturnAsync(
            new ReceiveReturnRequest(order.Id, item.Id, withdrawalId, AdminCaller, version, 1, arrival), Ct));
        Assert.Null(received.Refusal);
        Assert.Equal(declarationDay, AfterSalesPeriodRules.WithdrawalRefundStartDay(afterDelivery, withdrawal.DeclarationDate, arrival));
        version = await VersionAsync(order.Id);
        Assert.Null((await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().AddReturnedStockAsync(
            new AddReturnedStockRequest(order.Id, item.Id, received.RecordId!.Value, AdminCaller, version), Ct))).Refusal);
        Assert.Equal(ParcelStock + 1, await ScalarAsync($"SELECT stock_quantity FROM variants WHERE id = '{variantId}'"));
        await AssertAxesAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Paid);

        // ---- 4. Refund (step 4; KK7): to the IBAN, with the fee since every shipped physical unit was withdrawn; Refunded (Ö4) and B-8 ----
        (Refund refund, PaymentTransitionResult processed) = await ProcessWithdrawalRefundAsync(order, item, withdrawalId, null);
        Assert.Equal((true, ParcelPrice + ParcelShippingFee, order.Total), (refund.IsShippingFeeIncluded, refund.Amount, refund.Amount));
        Assert.Equal([TransitionCode.O4], processed.Applied);
        await AssertAxesAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Refunded);
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B8));
        Assert.Equal(0L, await CountAsync($"refund_ibans WHERE order_id = '{order.Id}'"));
        Assert.Equal([TransitionCode.S5, TransitionCode.S7, TransitionCode.S9, TransitionCode.O4], await CodesAsync(order.Id));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task TheReturnGoods_ArrivingAfterTheIbanWasDeleted_ReopenTheItem_AskForTheIbanWithB14_AndThePeriodDoesNotStop_KK1_KK8_KK10(bool closedByFirm)
    {
        // A member's physical item paid by transfer, delivered three days ago: Delivered + Paid.
        Customer customer = Customer.Create("Ali Veli", $"ali-{Guid.NewGuid():N}@example.com", "digest-placeholder", Start, null, Start);
        await SaveAsync(c => c.Customers.Add(customer));
        DateOnly deliveryDay = TurkeyTime.DateOf(Start).AddDays(-3);
        (Order order, List<OrderItem> items) = await SeedOrderAsync(
            PaymentMethod.BankTransfer, false, OrderStatus.Delivered, PaymentStatus.Paid, 0m, Start.AddDays(-7), new ItemSpec(ProductType.Physical, 1, ParcelPrice, 0m, deliveryDay));
        await ExecuteAsync($"UPDATE orders SET customer_id = '{customer.Id}' WHERE id = '{order.Id}'");
        OrderItem item = items[0];

        // ---- 1. Declaration (2.10.4 step 1): the IBAN with it; the item awaits the goods, the refund period does not start before they arrive ----
        WithdrawalResult declared = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().DeclareWithdrawalAsync(
            new DeclareWithdrawalRequest(order.Id, item.Id, 1, true, RefundIbanText), Ct));
        Assert.False(declared.IsRejected);
        Guid withdrawalId = declared.WithdrawalId!.Value;
        Withdrawal withdrawal;
        RefundIban iban;
        await using (AppDbContext reader = NewContext())
        {
            withdrawal = await reader.Withdrawals.AsNoTracking().SingleAsync(w => w.Id == withdrawalId, Ct);
            iban = await reader.RefundIbans.AsNoTracking().SingleAsync(i => i.Id == withdrawal.RefundIbanId, Ct);
        }

        bool afterDelivery = AfterSalesPeriodRules.IsAfterDelivery(ProductType.Physical, deliveryDay, withdrawal.DeclarationDate, withdrawal.IsDeliveredBeforeDeclaration);
        Assert.True(afterDelivery);
        Assert.Null(AfterSalesPeriodRules.WithdrawalRefundStartDay(afterDelivery, withdrawal.DeclarationDate, null));
        Assert.Equal(new ItemMarks(true, false), await MarksAsync(item));
        await AssertAxesAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid);

        // ---- 2. The goods do not come (step 2): the system closes nothing; the IBAN goes by the firm's closure after Z-42, or by Z-38 ----
        int dispatchesBefore = (await DispatchesAsync(order.Id)).Count;
        if (closedByFirm)
        {
            _clock.SetUtcNow(TurkeyTime.StartOf(withdrawal.DeclarationDate.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + 1)).AddHours(12));
            Assert.Equal(1L, await CountAsync($"refund_ibans WHERE order_id = '{order.Id}'"));
            int version = await VersionAsync(order.Id);
            ItemRecordResult closure = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CloseAsNotReturnedAsync(
                new CloseAsNotReturnedRequest(order.Id, item.Id, withdrawalId, AdminCaller, version, 1), Ct));
            Assert.Null(closure.Refusal);
            Assert.False((await MarksAsync(item)).AwaitingReturnGoods);
        }
        else
        {
            _clock.SetUtcNow(iban.ScheduledDeletionAt!.Value.AddMinutes(1));
            await using (AsyncServiceScope scope = _app!.Services.CreateAsyncScope())
            {
                RefundIbanDeletionDay deletion = new();
                Assert.Contains(iban.Id, (await deletion.FindDueAsync(scope.ServiceProvider, _clock.GetUtcNow(), Ct)).Select(d => d.Id));
                await deletion.ProcessAsync(scope.ServiceProvider, iban.Id, Ct);
            }

            // The item stays open and the withdrawal keeps its validity; only the IBAN went.
            Assert.True((await MarksAsync(item)).AwaitingReturnGoods);
            Assert.Equal(0L, await CountAsync($"not_returned_closures WHERE withdrawal_id = '{withdrawalId}'"));
            Assert.Equal(1L, await CountAsync($"withdrawals WHERE id = '{withdrawalId}' AND refund_iban_id IS NULL"));
        }

        Assert.Equal(0L, await CountAsync($"refund_ibans WHERE order_id = '{order.Id}'"));
        Assert.Equal(dispatchesBefore, (await DispatchesAsync(order.Id)).Count);
        await AssertAxesAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid);

        // ---- 3. The goods arrive (step 3; KK8): the receipt reopens a closed item and asks for the deleted IBAN with B-14 ----
        DateOnly arrival = TurkeyTime.DateOf(_clock.GetUtcNow());
        int receiptVersion = await VersionAsync(order.Id);
        ItemRecordResult received = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().ReceiveReturnAsync(
            new ReceiveReturnRequest(order.Id, item.Id, withdrawalId, AdminCaller, receiptVersion, 1, arrival), Ct));
        Assert.Null(received.Refusal);
        RefundIbanRequest request;
        await using (AppDbContext reader = NewContext())
        {
            if (closedByFirm)
            {
                Assert.NotNull((await reader.NotReturnedClosures.AsNoTracking().SingleAsync(n => n.WithdrawalId == withdrawalId, Ct)).ReopenedAt);
            }

            request = await reader.RefundIbanRequests.AsNoTracking().SingleAsync(r => r.OrderItemId == item.Id, Ct);
        }

        Assert.Equal((RefundIbanRequestReason.ReturnAfterIbanDeleted, (DateTimeOffset?)null), (request.Reason, request.FulfilledAt));
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B14));
        Assert.Equal(new ItemMarks(false, true), await MarksAsync(item));
        await AssertAxesAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid);

        // ---- 4. Waiting (step 4; KK8): the period runs from the arrival and does not stop while the IBAN is awaited ----
        DateOnly periodStart = AfterSalesPeriodRules.WithdrawalRefundStartDay(afterDelivery, withdrawal.DeclarationDate, arrival)!.Value;
        Assert.Equal(arrival, periodStart);
        int daysLeftAtArrival = AfterSalesPeriodRules.RefundStanding(periodStart, _clock.GetUtcNow()).DaysLeft;
        _clock.Advance(TimeSpan.FromDays(5));
        Assert.True((await MarksAsync(item)).AwaitingIban);
        Assert.Equal(daysLeftAtArrival - 5, AfterSalesPeriodRules.RefundStanding(periodStart, _clock.GetUtcNow()).DaysLeft);

        // ---- 5. Closing (step 5): the customer enters the IBAN on the order page; the firm processes the refund; the IBAN goes with it ----
        Assert.False((await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().EnterRefundIbanAsync(new EnterRefundIbanRequest(order.Id, request.Id, RefundIbanText), Ct))).IsRejected);
        Assert.False((await MarksAsync(item)).AwaitingIban);
        (Refund refund, PaymentTransitionResult processed) = await ProcessWithdrawalRefundAsync(order, item, withdrawalId, received.RecordId);
        Assert.Equal(order.Total, refund.Amount);
        Assert.Equal([TransitionCode.O4], processed.Applied);
        await AssertAxesAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Refunded);
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B8));
        Assert.Equal(0L, await CountAsync($"refund_ibans WHERE order_id = '{order.Id}'"));
    }
}
