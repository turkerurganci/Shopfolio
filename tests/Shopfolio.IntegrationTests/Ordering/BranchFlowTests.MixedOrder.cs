using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>03 2.10.2 - the mixed order (T390 KK2-KK4).</summary>
public sealed partial class BranchFlowTests
{
    private const decimal LampPrice = 1000m;
    private const decimal LampCouponShare = 100m;
    private const decimal GuidePrice = 100m;
    private const decimal GuideCouponShare = 10m;
    private const decimal ConsultancyPrice = 500m;
    private const decimal ConsultancyCouponShare = 50m;
    private const decimal MixedShippingFee = 30m;
    private const int LampStock = 5;

    [Fact]
    public async Task TheMixedOrder_PaidByTheFakeCard_DeliversTheGuide_CancelsTheLampBeforeTheCarrier_AndClosesWithTheConsultancysMark_KK1_KK2_KK3_KK4_KK10()
    {
        // ---- 1. Confirmation (2.10.2 step 1): a member's card order with a lamp, its guide and a consultancy, a coupon used; Placed + Pending ----
        Customer customer = Customer.Create("Ali Veli", $"ali-{Guid.NewGuid():N}@example.com", "digest-placeholder", Start, null, Start);
        Coupon coupon = Coupon.CreatePercentage("T390", 10m, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 10, Start.AddDays(-2));
        await SaveAsync(c =>
        {
            c.Customers.Add(customer);
            c.Coupons.Add(coupon);
        });
        (Order order, List<OrderItem> items) = await SeedOrderAsync(
            PaymentMethod.Card,
            false,
            OrderStatus.Placed,
            PaymentStatus.Pending,
            MixedShippingFee,
            Start,
            new ItemSpec(ProductType.Physical, 1, LampPrice, LampCouponShare),
            new ItemSpec(ProductType.Digital, 1, GuidePrice, GuideCouponShare),
            new ItemSpec(ProductType.Service, 1, ConsultancyPrice, ConsultancyCouponShare));
        OrderItem lamp = items[0];
        OrderItem guide = items[1];
        OrderItem consultancy = items[2];
        Guid lampVariantId = await SeedVariantAsync(order.Id, lamp.Id, LampStock, 1);
        await SaveAsync(c => c.StockReservations.Add(StockReservation.ForCoupon(order.Id, coupon.Id, Start)));
        await ExecuteAsync($"UPDATE coupons SET reserved_count = 1 WHERE id = '{coupon.Id}'");
        await ExecuteAsync($"UPDATE orders SET customer_id = '{customer.Id}', coupon_id = '{coupon.Id}', coupon_code = '{coupon.Code}' WHERE id = '{order.Id}'");
        await AssertAxesAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending);

        // ---- 2. Payment approval (step 2; KK2): the fake provider's success notification; Ö1 and S1 together, the guide is delivered at once and B-4 goes ----
        _clock.Advance(TimeSpan.FromMinutes(3));
        await PayByCardAsync(order, "T390-TX-0001");
        DateTimeOffset approvedAt = _clock.GetUtcNow();
        await AssertAxesAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
        Assert.Equal(new[] { TransitionCode.O1, TransitionCode.S1 }.Order(), (await CodesAsync(order.Id)).Order());
        OrderItem deliveredGuide = await ReadItemAsync(guide.Id);
        Assert.Equal(((DateTimeOffset?)approvedAt, (DateOnly?)TurkeyTime.DateOf(approvedAt)), (deliveredGuide.DeliveryMarkedAt, deliveredGuide.DeliveryDate));
        Assert.Null((await ReadItemAsync(lamp.Id)).DeliveryMarkedAt);
        Assert.Null((await ReadItemAsync(consultancy.Id)).DeliveryMarkedAt);
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B4));
        Assert.Equal(LampStock - 1, await ScalarAsync($"SELECT stock_quantity FROM variants WHERE id = '{lampVariantId}'"));
        Assert.Equal((1L, 0L), (await ScalarAsync($"SELECT used_count FROM coupons WHERE id = '{coupon.Id}'"), await ScalarAsync($"SELECT reserved_count FROM coupons WHERE id = '{coupon.Id}'")));

        // The guide's cancellation and withdrawal are closed - on the page and at the operation itself; the consultancy's withdrawal is open.
        OrderOperationsView paidView = await BuyerViewAsync(order.Id);
        Assert.False(paidView.IsOpen(OrderOperation.CancelDigitalItem, guide.Id));
        Assert.False(paidView.IsOpen(OrderOperation.WithdrawDigitalItem, guide.Id));
        Assert.True(paidView.IsOpen(OrderOperation.WithdrawServiceItem, consultancy.Id));
        OrderTransitionResult guideCancel = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CancelItemAsync(new CancelItemRequest(order.Id, guide.Id, 1), Ct));
        WithdrawalResult guideWithdrawal = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, guide.Id, 1, true), Ct));
        Assert.Equal(TransitionRefusal.ConditionNotMet, guideCancel.Refusal);
        Assert.NotNull(guideWithdrawal.Refusal);
        Assert.Equal(0L, await CountAsync($"withdrawals WHERE order_item_id = '{guide.Id}'"));
        await AssertAxesAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);

        // ---- 3. The branch (step 5; KK3): the customer cancels the lamp while the order is Preparing; B-7 goes, the system starts the card refund with the fee ----
        _clock.Advance(TimeSpan.FromHours(1));
        OrderTransitionResult lampCancel = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CancelItemAsync(new CancelItemRequest(order.Id, lamp.Id, 1), Ct));
        Assert.False(lampCancel.IsRejected);
        Assert.Null(lampCancel.ClosureCode);
        await AssertAxesAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B7));
        Refund refund;
        await using (AppDbContext reader = NewContext())
        {
            refund = await reader.Refunds.AsNoTracking().SingleAsync(r => r.OrderId == order.Id, Ct);
        }

        // No approval: the system's refund of the record, on the card, with the fee because every physical unit was cancelled before the carrier (02 7.2.8, 7.2.9).
        Assert.Equal(
            (RefundType.Cancellation, PaymentMethod.Card, ActorKind.System, true, LampPrice - LampCouponShare + MixedShippingFee, (Guid?)lamp.Id, lampCancel.ItemRecordId),
            (refund.RefundType, refund.PaymentMethod, refund.ActorKind, refund.IsShippingFeeIncluded, refund.Amount, refund.OrderItemId, refund.CancellationId));
        Assert.Equal(LampStock, await ScalarAsync($"SELECT stock_quantity FROM variants WHERE id = '{lampVariantId}'"));

        PaymentTransitionResult refunded = await RunCardRefundAsync(order, refund);
        Assert.Equal([TransitionCode.O3], refunded.Applied);
        await AssertAxesAsync(order.Id, OrderStatus.Preparing, PaymentStatus.PartiallyRefunded);
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.B8));
        Assert.Equal(1, await CountDispatchesAsync(order.Id, DispatchType.F4));

        // The coupon right does not come back - the order lives on (02 5.11.3).
        Assert.Equal(1L, await ScalarAsync($"SELECT used_count FROM coupons WHERE id = '{coupon.Id}'"));

        // ---- 4. The consultancy's window (step 5; KK4): no delivery date will come, so the right lasts until the mark - well past fourteen days ----
        _clock.Advance(TimeSpan.FromDays(30));
        Assert.True((await BuyerViewAsync(order.Id)).IsOpen(OrderOperation.WithdrawServiceItem, consultancy.Id));
        await AssertAxesAsync(order.Id, OrderStatus.Preparing, PaymentStatus.PartiallyRefunded);

        // The administrator marks the consultancy completed while the payment is Partially refunded (K-671): it is the last open item and the guide was delivered - S10.
        int version = await VersionAsync(order.Id);
        OrderTransitionResult completed = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().MarkServiceCompletedAsync(
            new MarkServiceCompletedRequest(order.Id, consultancy.Id, AdminCaller, version), Ct));
        Assert.False(completed.IsRejected);
        await AssertAxesAsync(order.Id, OrderStatus.Delivered, PaymentStatus.PartiallyRefunded);
        Assert.NotNull((await ReadItemAsync(consultancy.Id)).DeliveryMarkedAt);
        Assert.False((await BuyerViewAsync(order.Id)).IsOpen(OrderOperation.WithdrawServiceItem, consultancy.Id));

        // No shipment and no delivery date of the order ever came: the trail is Ö1, S1, Ö3 and S10.
        Assert.Null((await ReadItemAsync(lamp.Id)).DeliveryDate);
        List<TransitionCode> trail = await CodesAsync(order.Id);
        Assert.Equal(new[] { TransitionCode.O1, TransitionCode.S1, TransitionCode.O3, TransitionCode.S10 }.Order(), trail.Order());
        Assert.Equal(TransitionCode.S10, trail[^1]);
    }
}
