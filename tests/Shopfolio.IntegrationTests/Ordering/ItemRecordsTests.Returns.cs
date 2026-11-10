using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The return on a real PostgreSQL (T373 KK3-KK10; 02 7.4.4, 7.4.5, 7.4.7, 7.4.9, 10.4.9): the receipt's date bounds, the separate "Stoğa ekle" step, the "mal dönmedi"
/// closure after Z-42, the IBAN's deletion with the rejection and the closure (unless another pending refund uses it) and its request again at a later receipt.
/// The sending period comes from <see cref="AfterSalesPeriodRules.ReturnShipmentPeriodDays"/>.
/// </summary>
public sealed partial class ItemRecordsTests
{
    private const string ReturnIban = "TR330006100519786457841326";

    // ---- KK4: the arrival date's bounds ----

    [Fact]
    public async Task Receipt_RefusesAFutureDate_AndADateBeforeTheDelivery_ButTakesOneBeforeTheDeclaration_KK4()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, null);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 3, DeliveryDay.AddDays(3)), t))).RecordId!.Value;

        ItemRecordResult future = await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 2, 1, TurkeyTime.DateOf(Start).AddDays(1)), t));
        ItemRecordResult beforeDelivery = await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 2, 1, DeliveryDay.AddDays(-1)), t));
        Assert.Equal(TransitionRefusal.InvalidInput, future.Refusal);
        Assert.Equal(TransitionRefusal.InvalidInput, beforeDelivery.Refusal);

        // The goods may have been sent before the declaration (K-554): the delivery day itself and a day before the declaration are taken.
        Assert.Null((await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 2, 1, DeliveryDay), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 3, 1, DeliveryDay.AddDays(1)), t))).Refusal);
        Assert.Equal(2, await CountAsync(c => c.Returns.Where(r => r.WithdrawalId == withdrawalId)));
    }

    [Fact]
    public async Task Receipt_OfAParcelWithdrawnBeforeItsDelivery_HasNoLowerBound_KK4()
    {
        (Order order, OrderItem item) = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, ProductType.Physical, 2, delivered: false, exemption: null);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 2, DeliveryDay), t))).RecordId!.Value;
        int version = await VersionAsync(order.Id);

        ItemRecordResult received = await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, version, 2, DeliveryDay.AddYears(-1)), t));

        Assert.Null(received.Refusal);
    }

    // ---- KK5: the stock is a step of its own, as many units as were received, once ----

    [Fact]
    public async Task Receipt_LeavesTheStock_AndTheStockStepAddsTheReceivedUnitsOnce_KK5()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(3, null);
        Guid variantId = await SeedReturnVariantAsync(item.Id, 5);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 3, DeliveryDay.AddDays(1)), t))).RecordId!.Value;

        ItemRecordResult received = await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 2, 2, DeliveryDay.AddDays(2)), t));
        Assert.Null(received.Refusal);
        Assert.Equal(5, await StockAsync(variantId));

        ItemRecordResult added = await RunAsync((u, t) => u.AddReturnedStockAsync(new AddReturnedStockRequest(order.Id, item.Id, received.RecordId!.Value, AdminCaller, 3), t));
        ItemRecordResult again = await RunAsync((u, t) => u.AddReturnedStockAsync(new AddReturnedStockRequest(order.Id, item.Id, received.RecordId!.Value, AdminCaller, 4), t));

        Assert.Null(added.Refusal);
        Assert.Equal(TransitionRefusal.ConditionNotMet, again.Refusal);
        Assert.Equal(5 + 2, await StockAsync(variantId));
        await using AppDbContext reader = NewContext();
        Assert.NotNull((await reader.Returns.SingleAsync(r => r.Id == received.RecordId, Ct)).StockAddedAt);
    }

    [Fact]
    public async Task StockStep_IsNotOffered_WhenTheVariantIsGone_KK5()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(1, null);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 1, DeliveryDay.AddDays(1)), t))).RecordId!.Value;
        ItemRecordResult received = await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 2, 1, DeliveryDay.AddDays(2)), t));

        ItemRecordResult added = await RunAsync((u, t) => u.AddReturnedStockAsync(new AddReturnedStockRequest(order.Id, item.Id, received.RecordId!.Value, AdminCaller, 3), t));

        Assert.Equal(TransitionRefusal.ConditionNotMet, added.Refusal);
    }

    // ---- KK3, KK10: the closure only after Z-42, no notice, the IBAN goes; a later receipt reopens it and asks for the IBAN again (KK6) ----

    [Fact]
    public async Task Closure_IsRefusedBeforeTheSendingPeriodEnds_NothingHappensByItself_AndAfterItTheIbanGoes_KK3_KK10()
    {
        (Order order, OrderItem item) = await SeedTransferDeliveredAsync(2, null);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(TransferDeclaration(order, item, 2), t))).RecordId!.Value;
        int dispatchesBefore = await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == order.Id));

        // The last day of the sending period (the declaration day plus Z-42): the closure is refused, and nothing was closed or deleted by the time itself (K-494).
        _clock.SetUtcNow(TurkeyTime.StartOf(DeliveryDay.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + 1)).AddHours(12));
        ItemRecordResult early = await RunAsync((u, t) => u.CloseAsNotReturnedAsync(new CloseAsNotReturnedRequest(order.Id, item.Id, withdrawalId, AdminCaller, 2, 2), t));
        Assert.Equal(TransitionRefusal.ConditionNotMet, early.Refusal);
        Assert.Equal(0, await CountAsync(c => c.NotReturnedClosures.Where(n => n.WithdrawalId == withdrawalId)));
        Assert.Equal(1, await CountAsync(c => c.RefundIbans.Where(i => i.OrderId == order.Id)));

        _clock.SetUtcNow(TurkeyTime.StartOf(DeliveryDay.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + 2)).AddHours(12));
        ItemRecordResult tooMany = await RunAsync((u, t) => u.CloseAsNotReturnedAsync(new CloseAsNotReturnedRequest(order.Id, item.Id, withdrawalId, AdminCaller, 2, 3), t));
        ItemRecordResult closed = await RunAsync((u, t) => u.CloseAsNotReturnedAsync(new CloseAsNotReturnedRequest(order.Id, item.Id, withdrawalId, AdminCaller, 2, 2), t));

        Assert.Equal(TransitionRefusal.InvalidInput, tooMany.Refusal);
        Assert.Null(closed.Refusal);
        Assert.Equal(0, await CountAsync(c => c.RefundIbans.Where(i => i.OrderId == order.Id)));
        Assert.Equal(dispatchesBefore, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == order.Id)));
        Assert.Empty(await RefundsOfOrderAsync(order.Id));
        Assert.False((await MarksAsync(item)).AwaitingReturnGoods);
    }

    [Fact]
    public async Task Receipt_AfterTheClosure_ReopensIt_AndAsksForTheDeletedIbanAgain_WithB14_KK6_KK10()
    {
        (Order order, OrderItem item) = await SeedTransferDeliveredAsync(2, null);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(TransferDeclaration(order, item, 2), t))).RecordId!.Value;
        _clock.SetUtcNow(TurkeyTime.StartOf(DeliveryDay.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + 2)).AddHours(12));
        Assert.Null((await RunAsync((u, t) => u.CloseAsNotReturnedAsync(new CloseAsNotReturnedRequest(order.Id, item.Id, withdrawalId, AdminCaller, 2, 2), t))).Refusal);

        ItemRecordResult received = await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 3, 1, TurkeyTime.DateOf(_clock.GetUtcNow())), t));

        Assert.Null(received.Refusal);
        await using AppDbContext reader = NewContext();
        NotReturnedClosure closure = await reader.NotReturnedClosures.SingleAsync(n => n.WithdrawalId == withdrawalId, Ct);
        Assert.NotNull(closure.ReopenedAt);
        RefundIbanRequest request = await reader.RefundIbanRequests.SingleAsync(r => r.OrderItemId == item.Id, Ct);
        Assert.Equal((RefundIbanRequestReason.ReturnAfterIbanDeleted, (DateTimeOffset?)null), (request.Reason, request.FulfilledAt));
        Assert.Equal(1, await reader.DispatchRecords.CountAsync(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B14, Ct));

        // The reopened closure's other unit awaits its receipt again and may be closed by a new row (06 4.28).
        Assert.True((await MarksAsync(item)).AwaitingReturnGoods);
        Assert.Null((await RunAsync((u, t) => u.CloseAsNotReturnedAsync(new CloseAsNotReturnedRequest(order.Id, item.Id, withdrawalId, AdminCaller, 4, 1), t))).Refusal);
        Assert.Equal(2, await CountAsync(c => c.NotReturnedClosures.Where(n => n.WithdrawalId == withdrawalId)));
    }

    [Fact]
    public async Task Receipt_WhileTheIbanStands_AsksForNoIban_KK6()
    {
        (Order order, OrderItem item) = await SeedTransferDeliveredAsync(2, null);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(TransferDeclaration(order, item, 2), t))).RecordId!.Value;

        Assert.Null((await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 2, 1, DeliveryDay.AddDays(2)), t))).Refusal);

        Assert.Equal(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item.Id)));
        Assert.Equal(0, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B14)));
    }

    [Fact]
    public async Task Closure_KeepsTheIban_WhileAnotherPendingWithdrawalUsesIt_KK10()
    {
        (Order order, OrderItem item) = await SeedTransferDeliveredAsync(2, null);
        Guid first = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(TransferDeclaration(order, item, 1), t))).RecordId!.Value;

        // The second declaration uses the IBAN on file (02 7.4.5; K-1110): one IBAN for both.
        WithdrawalDeclaration second = Declaration(order, item, 1, DeliveryDay.AddDays(1)) with { Iban = new WithdrawalIban(null, true, false) };
        Guid secondId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(second, t))).RecordId!.Value;
        Assert.Null((await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, secondId, 3, 1, DeliveryDay.AddDays(2)), t))).Refusal);

        _clock.SetUtcNow(TurkeyTime.StartOf(DeliveryDay.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + 2)).AddHours(12));
        Assert.Null((await RunAsync((u, t) => u.CloseAsNotReturnedAsync(new CloseAsNotReturnedRequest(order.Id, item.Id, first, AdminCaller, 4, 1), t))).Refusal);

        // The received unit's refund is still to be processed: it keeps the shared IBAN (K-1127).
        Assert.Equal(1, await CountAsync(c => c.RefundIbans.Where(i => i.OrderId == order.Id)));
    }

    // ---- KK8: the rejection's consequences on the transfer line ----

    [Fact]
    public async Task Rejection_DeletesTheUnsharedIban_AddsNoStock_AndStartsNoRefund_KK8()
    {
        (Order order, OrderItem item) = await SeedTransferDeliveredAsync(2, WithdrawalExemptionReason.HygieneSensitive);
        Guid variantId = await SeedReturnVariantAsync(item.Id, 4);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(TransferDeclaration(order, item, 2), t))).RecordId!.Value;

        ItemRecordResult rejected = await RunAsync((u, t) => u.RejectReturnAsync(Rejection(order.Id, item.Id, withdrawalId, 2, 2), t));

        Assert.Null(rejected.Refusal);
        Assert.Equal(0, await CountAsync(c => c.RefundIbans.Where(i => i.OrderId == order.Id)));
        Assert.Equal(4, await StockAsync(variantId));
        Assert.Empty(await RefundsOfOrderAsync(order.Id));
        Assert.Equal(1, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B16)));
        Assert.False((await MarksAsync(item)).AwaitingReturnGoods);
    }

    [Fact]
    public async Task Rejection_AfterTheClosure_RejectsTheClosedUnits_AndReopensTheClosure_T412_KK5()
    {
        (Order order, OrderItem item) = await SeedTransferDeliveredAsync(2, WithdrawalExemptionReason.HygieneSensitive);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(TransferDeclaration(order, item, 2), t))).RecordId!.Value;
        _clock.SetUtcNow(TurkeyTime.StartOf(DeliveryDay.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + 2)).AddHours(12));
        Assert.Null((await RunAsync((u, t) => u.CloseAsNotReturnedAsync(new CloseAsNotReturnedRequest(order.Id, item.Id, withdrawalId, AdminCaller, 2, 2), t))).Refusal);

        // The goods of the closed units arrive with their protective packaging opened: the rejection takes them (06 4.26 "ret de"; D-915).
        ItemRecordResult tooMany = await RunAsync((u, t) => u.RejectReturnAsync(Rejection(order.Id, item.Id, withdrawalId, 3, 3), t));
        ItemRecordResult rejected = await RunAsync((u, t) => u.RejectReturnAsync(Rejection(order.Id, item.Id, withdrawalId, 3, 1), t));

        Assert.NotNull(tooMany.Refusal);
        Assert.Null(rejected.Refusal);
        await using AppDbContext reader = NewContext();
        Assert.NotNull((await reader.NotReturnedClosures.SingleAsync(n => n.WithdrawalId == withdrawalId, Ct)).ReopenedAt);
        Assert.Equal(1, await reader.ReturnRejections.CountAsync(r => r.WithdrawalId == withdrawalId, Ct));

        // The reopened closure's other unit awaits its receipt again and may be closed by a new row (06 4.28), as after a receipt.
        Assert.True((await MarksAsync(item)).AwaitingReturnGoods);
        Assert.Null((await RunAsync((u, t) => u.CloseAsNotReturnedAsync(new CloseAsNotReturnedRequest(order.Id, item.Id, withdrawalId, AdminCaller, 4, 1), t))).Refusal);
        Assert.Equal(2, await CountAsync(c => c.NotReturnedClosures.Where(n => n.WithdrawalId == withdrawalId)));
    }

    // ---- KK10, 06 4.14: the coupon right goes back once with the receipt that brings the order's last unit, a reopened closure included ----

    [Fact]
    public async Task Receipt_OfTheOrdersLastUnit_GivesTheCouponRightBack_Once()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(1, null);
        Guid couponId = await SeedUsedCouponAsync(order.Id);
        await SeedReturnVariantAsync(item.Id, 5);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 1, DeliveryDay.AddDays(1)), t))).RecordId!.Value;
        Assert.Equal(2, await UsedCountAsync(couponId));

        ItemRecordResult received = await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 2, 1, DeliveryDay.AddDays(2)), t));
        Assert.Null(received.Refusal);
        Assert.Equal(1, await UsedCountAsync(couponId));

        // The stock step after it returns the units, never the right a second time.
        Assert.Null((await RunAsync((u, t) => u.AddReturnedStockAsync(new AddReturnedStockRequest(order.Id, item.Id, received.RecordId!.Value, AdminCaller, 3), t))).Refusal);
        Assert.Equal(1, await UsedCountAsync(couponId));
    }

    [Fact]
    public async Task Receipt_ThatReopensTheClosure_OfTheOrdersLastUnit_GivesTheCouponRightBack_Once_KK10()
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(1, null);
        Guid couponId = await SeedUsedCouponAsync(order.Id);
        await SeedReturnVariantAsync(item.Id, 5);
        Guid withdrawalId = (await WriteAsync(order.Id, (m, t) => m.AddWithdrawalAsync(Declaration(order, item, 1, DeliveryDay.AddDays(1)), t))).RecordId!.Value;

        // The "mal dönmedi" closing ends the order with a loss: the right stays used (06 4.14; K-1104).
        _clock.SetUtcNow(TurkeyTime.StartOf(DeliveryDay.AddDays(AfterSalesPeriodRules.ReturnShipmentPeriodDays + 2)).AddHours(12));
        Assert.Null((await RunAsync((u, t) => u.CloseAsNotReturnedAsync(new CloseAsNotReturnedRequest(order.Id, item.Id, withdrawalId, AdminCaller, 2, 1), t))).Refusal);
        Assert.Equal(2, await UsedCountAsync(couponId));

        // The late goods' receipt opens the closing (06 4.28; K-1055) and brings the last unit: the right goes back with it.
        ItemRecordResult received = await RunAsync((u, t) => u.ReceiveReturnAsync(Receipt(order.Id, item.Id, withdrawalId, 3, 1, TurkeyTime.DateOf(_clock.GetUtcNow())), t));
        Assert.Null(received.Refusal);
        Assert.Equal(1, await UsedCountAsync(couponId));
        await using (AppDbContext reader = NewContext())
        {
            Assert.NotNull((await reader.NotReturnedClosures.SingleAsync(n => n.WithdrawalId == withdrawalId, Ct)).ReopenedAt);
        }

        Assert.Null((await RunAsync((u, t) => u.AddReturnedStockAsync(new AddReturnedStockRequest(order.Id, item.Id, received.RecordId!.Value, AdminCaller, 4), t))).Refusal);
        Assert.Equal(1, await UsedCountAsync(couponId));
    }

    // ---- helpers ----

    /// <summary>A coupon the order used, and another order too: <c>used_count</c> 2, so a second return of the order's right would show as 0.</summary>
    private async Task<Guid> SeedUsedCouponAsync(Guid orderId)
    {
        string key = EntityIds.New().ToString("N")[^10..].ToUpperInvariant(); // the random tail: a UUIDv7's head is its time (D-942)
        Coupon coupon = Coupon.CreateAmount($"IADE{key}", 10m, 100m, DeliveryDay.AddDays(-30), DeliveryDay.AddDays(365), 5, Start);
        await SaveAsync(c => c.Coupons.Add(coupon));
        await ExecuteAsync($"UPDATE coupons SET used_count = 2 WHERE id = '{coupon.Id}'");
        await ExecuteAsync($"UPDATE orders SET coupon_id = '{coupon.Id}', coupon_code = '{coupon.Code}' WHERE id = '{orderId}'");
        return coupon.Id;
    }

    private async Task<int> UsedCountAsync(Guid couponId)
    {
        await using AppDbContext reader = NewContext();
        return (await reader.Coupons.SingleAsync(c => c.Id == couponId, Ct)).UsedCount;
    }

    private ReceiveReturnRequest Receipt(Guid orderId, Guid itemId, Guid withdrawalId, int version, int quantity, DateOnly arrival) =>
        new(orderId, itemId, withdrawalId, AdminCaller, version, quantity, arrival);

    /// <summary>A declaration on the transfer line, after the delivery, with the customer's IBAN entered: its Z-38 day is written (T372).</summary>
    private static WithdrawalDeclaration TransferDeclaration(Order order, OrderItem item, int quantity) =>
        Declaration(order, item, quantity, DeliveryDay.AddDays(1)) with { Iban = new WithdrawalIban(ReturnIban, false, false) };

    private async Task<(Order Order, OrderItem Item)> SeedTransferDeliveredAsync(int quantity, WithdrawalExemptionReason? exemption)
    {
        (Order order, OrderItem item) = await SeedDeliveredAsync(quantity, exemption);
        await ExecuteAsync($"UPDATE orders SET payment_method = 'BankTransfer', bank_transfer_iban = '{ReturnIban}' WHERE id = '{order.Id}'");
        return (order, item);
    }

    private async Task<Guid> SeedReturnVariantAsync(Guid itemId, int stock)
    {
        string key = EntityIds.New().ToString("N")[^12..];
        Product product = Product.Create("Kaynak", $"iade-{key}", ProductType.Physical, 20m, Start);
        product.WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"IAD-{key}", 300m, Start, stock);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        await ExecuteAsync($"UPDATE order_items SET product_id = '{product.Id}', variant_id = '{variant.Id}' WHERE id = '{itemId}'");
        return variant.Id;
    }

    private async Task<int> VersionAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return (await reader.Orders.SingleAsync(o => o.Id == orderId, Ct)).Version;
    }

    private async Task<int?> StockAsync(Guid variantId)
    {
        await using AppDbContext reader = NewContext();
        return (await reader.Variants.SingleAsync(v => v.Id == variantId, Ct)).StockQuantity;
    }

    private async Task<List<Guid>> RefundsOfOrderAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Refunds.Where(r => r.OrderId == orderId).Select(r => r.Id).ToListAsync(Ct);
    }
}
