using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// T364 KK4 (criterion 4; 02 3.31.2; 09 10.2 item 3; 07 2.4.2): every one of the administrator's six interventions carries the version its screen showed.
/// Without it nothing applies (<see cref="TransitionRefusal.InvalidInput"/>); with an older one nothing applies and the order stays as the other change left
/// it (<see cref="TransitionRefusal.VersionConflict"/>); with the current one the intervention applies. A refusal writes no trail row and raises no version.
/// </summary>
public sealed partial class OrderCorrectionTests
{
    public static TheoryData<string> Interventions { get; } =
        ["RemoveItem", "CorrectShipment", "CorrectDeliveryDate", "CorrectReturnArrivalDate", "CorrectAddress", "CorrectGuestEmail"];

    [Theory]
    [MemberData(nameof(Interventions))]
    public async Task AnIntervention_WithoutTheShownVersion_IsRefused_WithAnOlderOne_KeepsTheCurrentState_WithTheCurrentOne_IsApplied_T364_KK4(string intervention)
    {
        (Order order, Func<int?, Task<TransitionRefusal?>> run) = await ArrangeInterventionAsync(intervention);
        int shown = order.Version;
        int trail = (await VersionAndTrailAsync(order.Id)).Trail;

        // No version: refused, nothing written.
        Assert.Equal(TransitionRefusal.InvalidInput, await run(null));
        Assert.Equal((shown, trail), await VersionAndTrailAsync(order.Id));

        // Another administrator changed the order after this screen: the older version is refused and the order stays as that change left it.
        await ExecuteAsync($"UPDATE orders SET version = version + 1 WHERE id = '{order.Id}'");
        Assert.Equal(TransitionRefusal.VersionConflict, await run(shown));
        Assert.Equal((shown + 1, trail), await VersionAndTrailAsync(order.Id));

        // The screen shows the current state again: its version applies the intervention, once.
        Assert.Null(await run(shown + 1));
        (int version, int written) = await VersionAndTrailAsync(order.Id);
        Assert.Equal(shown + 2, version);
        Assert.True(written > trail);
    }

    private async Task<(Order Order, Func<int?, Task<TransitionRefusal?>> Run)> ArrangeInterventionAsync(string intervention)
    {
        DateOnly today = TurkeyTime.DateOf(Start);
        switch (intervention)
        {
            case "RemoveItem":
                {
                    Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
                    Guid item = await ItemIdAsync(order.Id, ProductType.Physical);
                    return (order, async v => (await RunAsync(c => c.RemoveItemAsync(Removal(order, item, 1) with { ExpectedVersion = v }, Ct))).Refusal);
                }

            case "CorrectShipment":
                {
                    Order order = await SeedAsync(OrderStatus.Shipped, PaymentStatus.Paid, ProductType.Physical);
                    await ExecuteAsync($"UPDATE orders SET other_carrier_name = 'Eski Kargo', tracking_number = 'ESKI-1' WHERE id = '{order.Id}'");
                    order = await ReloadAsync(order.Id);
                    Guid id = order.Id;
                    return (order, async v => (await RunAsync(c => c.CorrectShipmentAsync(new CorrectShipmentRequest(id, AdminCaller, new ShipmentInfo(null, "Yeni Kargo", "YENI-77", false), v), Ct))).Refusal);
                }

            case "CorrectDeliveryDate":
                {
                    (Order order, _) = await DeliveredAsync(shippedDaysAgo: 6, deliveredDaysAgo: 4);
                    order = await ReloadAsync(order.Id);
                    return (order, async v => (await RunAsync(c => c.CorrectDeliveryDateAsync(DateRequest(order, today.AddDays(-2)) with { ExpectedVersion = v }, Ct))).Refusal);
                }

            case "CorrectReturnArrivalDate":
                {
                    (Order order, Guid item) = await DeliveredAsync(shippedDaysAgo: 8, deliveredDaysAgo: 6);
                    Guid receipt = await SeedReturnAsync(item, declarationDaysAgo: 4, arrivalDaysAgo: 1);
                    order = await ReloadAsync(order.Id);
                    return (order, async v => (await RunAsync(c => c.CorrectReturnArrivalDateAsync(ArrivalRequest(order, item, receipt, today.AddDays(-3)) with { ExpectedVersion = v }, Ct))).Refusal);
                }

            case "CorrectAddress":
                {
                    Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
                    return (order, async v => (await RunAsync(c => c.CorrectAddressAsync(AddressRequest(order, AddressType.ShippingAddress, "Yeni Mahalle 5") with { ExpectedVersion = v }, Ct))).Refusal);
                }

            default:
                {
                    Assert.Equal("CorrectGuestEmail", intervention);
                    Order order = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, ProductType.Physical);
                    return (order, async v => (await RunAsync(c => c.CorrectGuestEmailAsync(EmailRequest(order, "yeni@example.com") with { ExpectedVersion = v }, Ct))).Refusal);
                }
        }
    }

    private async Task<(int Version, int Trail)> VersionAndTrailAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        int version = await reader.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.Version).SingleAsync(Ct);
        int trail = await reader.AuditLogs.AsNoTracking().CountAsync(a => a.TargetRecordId == orderId, Ct);
        return (version, trail);
    }

    /// <summary>The version the panel shows now - read from the database, the way the order's page reads it before the administrator acts.</summary>
    private int ShownVersionOf(Guid orderId)
    {
        using AppDbContext reader = NewContext();
        return reader.Orders.AsNoTracking().Where(o => o.Id == orderId).Select(o => o.Version).SingleOrDefault();
    }
}
