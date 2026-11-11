using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// A panel page that draws every shared component of T542 with fixed values (04 2.5, 2.6, 2.10): two badges, item records, panel marks, the undelivered e-mail block, the six counters
/// (one of them zero), the IBAN list, the period indicators at a fixed instant and a two-section form whose conflict notice shows only on the section named by <c>?conflict=</c>.
/// The page decides nothing: every text comes from the component's own model.
/// </summary>
public sealed class ComponentSampleModel : PageModel
{
    /// <summary>The fixed instant the period indicators are read at (Turkey day 2026-11-10).</summary>
    public static readonly DateTimeOffset Now = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    public const string ConflictCurrentPath = "/panel/siparisler/bilesen-ornegi";

    public string? ConflictedSection { get; private set; }

    internal OrderBadges Badges { get; } = StatusBadges.ForOrder(OrderStatus.Shipped, PaymentStatus.Paid);

    internal IReadOnlyList<ItemRecordEntry> ItemRecords { get; } =
    [
        new(ItemRecordName.Withdrawal, 2, new DateTimeOffset(2026, 11, 3, 9, 0, 0, TimeSpan.Zero)),
        new(ItemRecordName.DeliveryMark, null, null),
    ];

    internal IReadOnlyList<PanelMark> Marks { get; } = PanelMarks.Of(new PanelMarkConditions(EmailNotDelivered: true, RefundNotCompleted: true), PanelMarkPlace.OrderDetail);

    internal UndeliveredEmailList Emails { get; } = new(
        ConflictCurrentPath,
        "ResendEmail",
        "ResendAllEmails",
        [
            new UndeliveredEmailRow("order-confirmation", "Sipariş onayı", new DateTimeOffset(2026, 11, 8, 9, 0, 0, TimeSpan.Zero), "musteri@example.com", true),
            new UndeliveredEmailRow("shipment", "Kargo bildirimi", new DateTimeOffset(2026, 11, 9, 9, 0, 0, TimeSpan.Zero), "musteri@example.com", true),
            new UndeliveredEmailRow("invitation", "Yönetici daveti", new DateTimeOffset(2026, 11, 9, 9, 0, 0, TimeSpan.Zero), "yonetici@example.com", false),
        ],
        false);

    internal IReadOnlyList<PendingCounterItem> Counters { get; } = PendingCounters.Of(
    [
        new PanelCounter(PanelCounterKind.PaymentApproval, 4, PanelListTarget.OrdersAwaitingPaymentApproval),
        new PanelCounter(PanelCounterKind.ToShip, 0, PanelListTarget.OrdersToShip),
        new PanelCounter(PanelCounterKind.DeliveryMarkPending, 2, PanelListTarget.OrdersAwaitingDeliveryMark),
        new PanelCounter(PanelCounterKind.ServicePending, 1, PanelListTarget.OrdersWithServicePending),
        new PanelCounter(PanelCounterKind.OpenRequest, 3, PanelListTarget.OpenRequests),
        new PanelCounter(PanelCounterKind.RefundAndReturnPending, 5, PanelListTarget.RefundAndReturnWork),
    ]);

    internal AwaitingCustomerList Awaiting { get; } = AwaitingCustomerList.Of(
        Enumerable.Range(1, 12).Select(i => new AwaitingCustomerRow($"SP-{i:0000}", $"/panel/siparisler/{i}", "IBAN bekleniyor", null)));

    internal PeriodIndicator Open { get; } = Indicator("Kargoya verme sözü", CountedPeriod.ShippingPeriod, new DateOnly(2026, 11, 13), true);

    internal PeriodIndicator Over { get; } = Indicator("Kargoya verme sözü", CountedPeriod.ShippingPeriod, new DateOnly(2026, 11, 6), true);

    internal PeriodIndicator Calendar { get; } = Indicator("Cayma süresi", CountedPeriod.WithdrawalWindow, new DateOnly(2026, 11, 14), false);

    public IActionResult OnGet([FromQuery(Name = "conflict")] string? conflict)
    {
        ConflictedSection = conflict;
        return Page();
    }

    private static PeriodIndicator Indicator(string label, CountedPeriod period, DateOnly lastDay, bool warns) =>
        new(label, period, PeriodStanding.Of(period, lastDay, Now, BusinessCalendar.Product), warns);
}

/// <summary>A storefront page that draws the two threshold messages of the cart and the payment summary (T542 KK8; 04 2.6.5).</summary>
public sealed class ComponentThresholdSampleModel : PageModel
{
    internal ThresholdMessage? FreeShipping { get; private set; }

    internal ThresholdMessage? Minimum { get; private set; }

    public IActionResult OnGet([FromQuery(Name = "left")] decimal? left, [FromQuery(Name = "missing")] decimal? missing)
    {
        ShippingAssessment assessment = new(100m, true, 20m, false, left, missing);
        FreeShipping = ThresholdMessages.FreeShipping(assessment);
        Minimum = ThresholdMessages.MinimumOrder(assessment);
        return Page();
    }
}
