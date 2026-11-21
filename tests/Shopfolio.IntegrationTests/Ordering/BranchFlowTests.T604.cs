using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>T604 (D-1283 (2); K-1631 (4)): the IBAN entered with one declaration is one row that the declaration's items share (06 4.31).</summary>
public sealed partial class BranchFlowTests
{
    [Fact]
    public async Task TwoLinesCancelledInOneRequest_ShareOneIbanRow_AndALaterRequestEntersItsOwn_D1283()
    {
        (Order order, List<OrderItem> items) = await SeedOrderAsync(
            PaymentMethod.BankTransfer,
            true,
            OrderStatus.Preparing,
            PaymentStatus.Paid,
            ParcelShippingFee,
            Start.AddDays(-1),
            new ItemSpec(ProductType.Physical, 1, ParcelPrice),
            new ItemSpec(ProductType.Physical, 1, ParcelPrice),
            new ItemSpec(ProductType.Physical, 1, ParcelPrice));

        // One request cancels two lines with one IBAN, as the order page's cancellation of several lines does (CustomerOperations: one transaction, a line at a time).
        _clock.Advance(TimeSpan.FromHours(1));
        (bool first, bool second) = await RunAsync(async sp =>
        {
            IOrderTransitions unit = sp.GetRequiredService<IOrderTransitions>();
            OrderTransitionResult a = await unit.CancelItemAsync(new CancelItemRequest(order.Id, items[0].Id, 1, RefundIbanText), Ct);
            OrderTransitionResult b = await unit.CancelItemAsync(new CancelItemRequest(order.Id, items[1].Id, 1, RefundIbanText), Ct);
            return (a.IsRejected, b.IsRejected);
        });
        Assert.False(first);
        Assert.False(second);

        RefundIban shared = Assert.Single(await ReadIbansAsync(order.Id));
        Assert.Equal(RefundIbanText, shared.Iban);
        Assert.Equal(2L, await CountAsync($"cancellations WHERE refund_iban_id = '{shared.Id}'"));

        // A later request is a new declaration: its record enters its own row.
        _clock.Advance(TimeSpan.FromHours(1));
        OrderTransitionResult later = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CancelItemAsync(
            new CancelItemRequest(order.Id, items[2].Id, 1, RefundIbanText), Ct));
        Assert.False(later.IsRejected);

        List<RefundIban> ibans = await ReadIbansAsync(order.Id);
        Assert.Equal(2, ibans.Count);
        RefundIban own = Assert.Single(ibans, i => i.Id != shared.Id);
        Assert.Equal(1L, await CountAsync($"cancellations WHERE refund_iban_id = '{own.Id}'"));
        Assert.Equal(2L, await CountAsync($"cancellations WHERE refund_iban_id = '{shared.Id}'"));
    }
}
