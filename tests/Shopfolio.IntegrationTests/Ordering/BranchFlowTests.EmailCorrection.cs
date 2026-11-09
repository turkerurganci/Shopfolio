using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>03 2.10.5 - the guest order's e-mail correction (T390 KK9).</summary>
public sealed partial class BranchFlowTests
{
    private const string RightEmail = "Dogru@Example.com";

    [Fact]
    public async Task TheGuestsEmailCorrection_MakesANewKey_SendsB1ToTheNewAndB15ToTheOld_AndChangesNoStateNoPeriodNoItemNoIban_KK1_KK9_KK10()
    {
        // ---- 1. Wrong address (2.10.5 step 1): the guest's transfer order is placed with the wrong e-mail ----
        (Order order, List<OrderItem> items) = await SeedOrderAsync(
            PaymentMethod.BankTransfer, true, OrderStatus.Placed, PaymentStatus.Pending, ParcelShippingFee, Start, new ItemSpec(ProductType.Physical, 2, ParcelPrice));
        await AssertAxesAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending);

        // ---- 2. Payment (step 2): the administrator's "paid" mark; B-4 goes to the wrong address too ----
        _clock.Advance(TimeSpan.FromHours(2));
        int version = await VersionAsync(order.Id);
        PaymentTransitionResult paid = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().MarkTransferPaidAsync(
            new TransferPaidMark(order.Id, _admin!.Id, _admin.Name, version, true), Ct));
        Assert.False(paid.IsRejected);
        await AssertAxesAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
        Assert.Contains(await DispatchesAsync(order.Id), d => IsSent(d, DispatchType.B4, ContactEmail));

        // What the correction must not touch: the states, the periods, the items, the IBAN and the trail.
        Order before = await ReadOrderAsync(order.Id);
        List<OrderItem> itemsBefore = await ReadItemsAsync(order.Id);
        OrderShippingView shippingBefore = (await ReadAsync(sp => sp.GetRequiredService<IOrderingModule>().GetShippingViewAsync(order.Id, Ct)))!;
        List<TransitionCode> trailBefore = await CodesAsync(order.Id);
        long ibansBefore = await CountAsync($"refund_ibans WHERE order_id = '{order.Id}'");
        string oldKey = before.OrderAccessKey!;

        // ---- 3-4. Confirmation and correction (steps 3-4): the identity is confirmed by the order's data; a new key, B-1 to the new address, B-15 to the old ----
        _clock.Advance(TimeSpan.FromDays(1));
        OrderCorrectionResult corrected = await RunAsync(sp => sp.GetRequiredService<IOrderCorrections>().CorrectGuestEmailAsync(
            new CorrectGuestEmailRequest(order.Id, AdminCaller, RightEmail, new GuestIdentityConfirmation(true, true, true, true), before.Version), Ct));
        Assert.Null(corrected.Refusal);
        Assert.Null(corrected.LinkedCustomerId);

        Order after = await ReadOrderAsync(order.Id);
        Assert.Equal(RightEmail, after.ContactEmail);
        Assert.False(string.IsNullOrEmpty(after.OrderAccessKey));
        Assert.NotEqual(oldKey, after.OrderAccessKey);
        List<(DispatchType Type, string? To)> sent = await DispatchesAsync(order.Id);
        Assert.Single(sent, d => IsSent(d, DispatchType.B1, RightEmail));
        Assert.Single(sent, d => IsSent(d, DispatchType.B15, ContactEmail));
        Assert.DoesNotContain(sent, d => d.Type == DispatchType.B15 && !IsSent(d, DispatchType.B15, ContactEmail));

        // ---- 5. Continuing (step 5): the old links are dead; the new key and the new address open the order ----
        Assert.Null(await ReadAsync(sp => sp.GetRequiredService<IOrderingModule>().FindOrderByAccessKeyAsync(oldKey, Ct)));
        Assert.Equal(order.Id, await ReadAsync(sp => sp.GetRequiredService<IOrderingModule>().FindOrderByAccessKeyAsync(after.OrderAccessKey!, Ct)));
        Assert.Null(await ReadAsync(sp => sp.GetRequiredService<IOrderingModule>().FindOrderByNumberAndEmailAsync(order.OrderNumber, ContactEmail, Ct)));
        Assert.NotNull(await ReadAsync(sp => sp.GetRequiredService<IOrderingModule>().FindOrderByNumberAndEmailAsync(order.OrderNumber, RightEmail, Ct)));
        await using (AppDbContext reader = NewContext())
        {
            OrderEmailHistory history = await reader.OrderEmailHistories.AsNoTracking().SingleAsync(h => h.OrderId == order.Id, Ct);
            Assert.Equal((ContactEmail, RightEmail), (history.OldEmail, history.NewEmail));
        }

        // KK9: the states, the periods, the items and the IBAN are unchanged (02 10.4.11); the correction is no transition.
        await AssertAxesAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid);
        Assert.Equal(
            (before.PaymentWindowEndsAt, before.ShippingPeriod, before.BankTransferIban, before.Total, before.ShippingFee),
            (after.PaymentWindowEndsAt, after.ShippingPeriod, after.BankTransferIban, after.Total, after.ShippingFee));
        OrderShippingView shippingAfter = (await ReadAsync(sp => sp.GetRequiredService<IOrderingModule>().GetShippingViewAsync(order.Id, Ct)))!;
        Assert.Equal((shippingBefore.PaymentConfirmedAt, shippingBefore.PromiseEndsAt), (shippingAfter.PaymentConfirmedAt, shippingAfter.PromiseEndsAt));
        Assert.Equal(
            itemsBefore.Select(i => (i.Id, i.Quantity, i.UnitPrice, i.CouponShare, i.DeliveryDate, i.DeliveryMarkedAt)),
            (await ReadItemsAsync(order.Id)).Select(i => (i.Id, i.Quantity, i.UnitPrice, i.CouponShare, i.DeliveryDate, i.DeliveryMarkedAt)));
        Assert.Equal(ibansBefore, await CountAsync($"refund_ibans WHERE order_id = '{order.Id}'"));
        Assert.Equal(trailBefore, await CodesAsync(order.Id));
        Assert.Equal(items.Count, (await ReadItemsAsync(order.Id)).Count);
    }

    // The recipient is stored in its normalized form (02 3.13): the address is compared without case.
    private static bool IsSent((DispatchType Type, string? To) dispatch, DispatchType type, string to) =>
        dispatch.Type == type && string.Equals(dispatch.To, to, StringComparison.OrdinalIgnoreCase);
}
