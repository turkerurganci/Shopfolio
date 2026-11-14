using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Administration;

/// <summary>
/// The order of the panel's order list under a timed pending work filter and the row's least period (T527 KK2, KK4; 04 9.8.5, 9.8.7): the overrun order first, then the least time left, an order
/// with no running period last, ties newest first and by the inner key; and the list's mark mapping, which drops the two item marks the list has no place for (04 2.5.3).
/// </summary>
public sealed class PanelOrderListOrderTests
{
    private static readonly DateTimeOffset Created = new(2026, 11, 1, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void TheTimedOrder_PutsTheOverrunFirst_ThenTheLeastTimeLeft_AndTheOrderWithNoPeriodLast_KK2()
    {
        Guid overrun = Guid.NewGuid();
        Guid soon = Guid.NewGuid();
        Guid later = Guid.NewGuid();
        Guid none = Guid.NewGuid();
        PanelOrderKey[] keys =
        [
            new(none, Created.AddDays(3)),
            new(later, Created.AddDays(2)),
            new(soon, Created),
            new(overrun, Created.AddDays(1)),
        ];
        OrderRunningPeriod[] periods =
        [
            new(later, CountedPeriod.ShippingPeriod, new DateOnly(2026, 11, 20)),
            new(later, CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 18)),
            new(soon, CountedPeriod.ServiceFulfilmentPeriod, new DateOnly(2026, 11, 14)),
            new(overrun, CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 2)),
        ];

        IReadOnlyList<Guid> ordered = PanelOrderListService.OrderByTimeLeft(keys, periods);

        Assert.Equal([overrun, soon, later, none], ordered);
    }

    [Fact]
    public void TheTimedOrder_BreaksEqualLastDaysNewestFirst_AndByTheInnerKey_KK2()
    {
        Guid older = Guid.NewGuid();
        Guid newer = Guid.NewGuid();
        Guid[] twins = [.. new[] { Guid.NewGuid(), Guid.NewGuid() }.OrderDescending()];
        DateOnly day = new(2026, 11, 14);
        PanelOrderKey[] keys = [new(older, Created), new(twins[1], Created.AddDays(1)), new(newer, Created.AddDays(2)), new(twins[0], Created.AddDays(1))];
        OrderRunningPeriod[] periods = [.. keys.Select(k => new OrderRunningPeriod(k.OrderId, CountedPeriod.ShippingPeriod, day))];

        IReadOnlyList<Guid> ordered = PanelOrderListService.OrderByTimeLeft(keys, periods);

        Assert.Equal([newer, twins[0], twins[1], older], ordered);
    }

    [Fact]
    public void TheRowsPeriod_IsTheOneWithTheEarliestLastDay_AndNoneWhenNothingRuns_KK4()
    {
        Guid order = Guid.NewGuid();
        OrderRunningPeriod least = new(order, CountedPeriod.BankTransferPaymentWindow, new DateOnly(2026, 11, 13));
        OrderRunningPeriod[] periods = [new(order, CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 20)), least, new(Guid.NewGuid(), CountedPeriod.ShippingPeriod, new DateOnly(2026, 11, 1))];

        Assert.Equal(least, PanelOrderListService.LeastOf(periods, order));
        Assert.Null(PanelOrderListService.LeastOf(periods, Guid.NewGuid()));
    }

    [Theory]
    [InlineData(CountedPeriod.ShippingPeriod, true)]
    [InlineData(CountedPeriod.ServiceFulfilmentPeriod, true)]
    [InlineData(CountedPeriod.BankTransferPaymentWindow, false)]
    [InlineData(CountedPeriod.RefundPeriod, false)]
    public void EveryListPeriod_HasALabel_AndOnlyTheDeliveryPromisesWarnOfLegalInterest_KK4(CountedPeriod period, bool warns)
    {
        PeriodIndicator indicator = OrderListTexts.PeriodOf(new OrderRunningPeriod(Guid.NewGuid(), period, new DateOnly(2026, 11, 20)), Created);

        Assert.False(string.IsNullOrWhiteSpace(indicator.Label));
        Assert.Equal(warns, indicator.WarnsLegalInterest);
    }

    [Fact]
    public void TheListsMarks_AreTheFiveOrderMarks_InTheDocumentsOrder_KK4()
    {
        PanelOrderMarkFacts all = new(true, true, true, true, true);

        IReadOnlyList<PanelMark> onList = OrderListTexts.MarksOf(all, PanelMarkPlace.OrderList);

        Assert.Equal(
            [PanelMark.EmailNotDelivered, PanelMark.PaymentResultMissing, PanelMark.RefundNotCompleted, PanelMark.EmailCorrected, PanelMark.RefundResultPending],
            onList);
        Assert.Empty(OrderListTexts.MarksOf(new PanelOrderMarkFacts(false, false, false, false, false), PanelMarkPlace.OrderList));
    }
}
