using System.Reflection;

using Microsoft.AspNetCore.Mvc;

using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Panel;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Administration;

/// <summary>
/// The panel home's own reads and the page that draws them (T521 KK1, KK3; 04 2.6.2, 3.4.3; 02 10.6.1): the order counts of the three order-level warnings, the "IBAN bekleniyor" lines resolved to their orders,
/// the warning lines a count opens and the ten-line cut. The modules are stood in by call-by-name stubs; the database side is the integration tests'.
/// </summary>
public sealed class PanelHomeDetailsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 12, 9, 0, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // ---- KK1, KK2: the three order counts ----

    [Fact]
    public async Task TheMarkedOrders_AreCountedPageByPage_UntilAPageIsEmpty_T521_KK2()
    {
        Guid[] all = [.. Enumerable.Range(0, 450).Select(_ => Guid.NewGuid())];
        List<Guid?> asked = [];
        StubModule<INotificationsModule> notifications = new();
        notifications.On(nameof(INotificationsModule.FindMarkedOrderIdsAsync), args =>
        {
            Guid? after = (Guid?)args[0];
            asked.Add(after);
            int from = after is null ? 0 : Array.IndexOf(all, after.Value) + 1;
            return (IReadOnlyList<Guid>)[.. all.Skip(from).Take((int)args[1]!)];
        });

        PanelHomeDetails details = await Service(notifications, NoCardProblems(), NoIbanLines()).GetAsync(Ct);

        Assert.Equal(450, details.EmailNotDeliveredOrders);
        Assert.Equal(4, asked.Count);
        Assert.Null(asked[0]);
        Assert.Equal(all[199], asked[1]);
    }

    [Fact]
    public async Task TheCardRefundCounts_AreTheOnesThePaymentModuleReports_T521_KK2()
    {
        StubModule<IPaymentModule> payment = new();
        payment.On(nameof(IPaymentModule.CountCardRefundProblemsAsync), _ => new CardRefundProblems(3, 5));
        payment.On(nameof(IPaymentModule.ListOpenRefundWorkAsync), _ => (IReadOnlyList<OpenRefundWork>)[]);

        PanelHomeDetails details = await Service(NoMarkedOrders(), payment, NoIbanLines()).GetAsync(Ct);

        Assert.Equal(3, details.CardRefundFailedOrders);
        Assert.Equal(5, details.CardRefundOutcomePendingOrders);
        Assert.Empty(details.AwaitingIban);
    }

    // ---- KK3: the IBAN lines ----

    [Fact]
    public async Task AnIbanLine_LeadsToTheOrderOfItsItemOrOfItsRefund_AndALineWhoseOrderIsGoneIsLeftOut_T521_KK3()
    {
        Guid itemOrder = Guid.NewGuid();
        Guid refundOrder = Guid.NewGuid();
        Guid goneOrder = Guid.NewGuid();
        Guid item = Guid.NewGuid();
        Guid refund = Guid.NewGuid();
        Guid goneRefund = Guid.NewGuid();
        DateOnly day = new(2026, 11, 20);

        StubModule<IAfterSalesModule> afterSales = new();
        afterSales.On(nameof(IAfterSalesModule.ListAwaitingIbanLinesAsync), _ => (IReadOnlyList<AwaitingIbanLine>)
        [
            new AwaitingIbanLine(item, null, day),
            new AwaitingIbanLine(null, refund, null),
            new AwaitingIbanLine(null, goneRefund, null),
        ]);
        StubModule<IPaymentModule> payment = new();
        payment.On(nameof(IPaymentModule.CountCardRefundProblemsAsync), _ => new CardRefundProblems(0, 0));
        payment.On(nameof(IPaymentModule.ListOpenRefundWorkAsync), _ => (IReadOnlyList<OpenRefundWork>)
        [
            new OpenRefundWork(refund, refundOrder, null),
            new OpenRefundWork(goneRefund, goneOrder, null),
        ]);
        StubModule<IOrderingModule> ordering = new();
        ordering.On(nameof(IOrderingModule.FindOrdersOfItemsAsync), _ => (IReadOnlyList<OrderOfItem>)[new OrderOfItem(item, itemOrder)]);
        ordering.On(nameof(IOrderingModule.GetOrderNumberAsync), args => (Guid)args[0]! == itemOrder ? "SP-1" : (Guid)args[0]! == refundOrder ? "SP-2" : null);

        PanelHomeDetails details = await new PanelHomeDetailsService(NoMarkedOrders().Object, payment.Object, afterSales.Object, ordering.Object).GetAsync(Ct);

        Assert.Equal(
            [new AwaitingIbanRow(itemOrder, "SP-1", day), new AwaitingIbanRow(refundOrder, "SP-2", null)],
            details.AwaitingIban);
    }

    [Fact]
    public async Task TheIbanLines_ShowTheTenWithTheLeastTimeLeftFirst_AndTheRestAreOnTheLink_T521_KK3()
    {
        // Twelve lines: the first eleven have a last day (the later the number, the later the day), the last has none. Offered in reverse so that the order comes from the list, not from the input.
        List<AwaitingIbanRow> rows = [.. Enumerable.Range(0, 11).Select(i => new AwaitingIbanRow(Guid.NewGuid(), $"SP-{i:D2}", new DateOnly(2026, 11, 13).AddDays(i))), new AwaitingIbanRow(Guid.NewGuid(), "SP-99", null)];
        rows.Reverse();
        PanelHomeModel model = Page(new PanelHomeDetails(0, 0, 0, rows));

        await model.OnGetAsync(Ct);

        Assert.Equal(AwaitingCustomerList.ShownLines, model.Awaiting.Shown.Count);
        Assert.True(model.Awaiting.HasMore);
        Assert.Equal(Enumerable.Range(0, 10).Select(i => $"SP-{i:D2}"), model.Awaiting.Shown.Select(r => r.OrderNumber));
        Assert.All(model.Awaiting.Shown, r => Assert.StartsWith("/panel/siparisler/", r.OrderPath, StringComparison.Ordinal));
        Assert.All(model.Awaiting.Shown, r => Assert.Equal("IBAN bekleniyor", r.Text));
        Assert.All(model.Awaiting.Shown, r => Assert.NotNull(r.Period));
    }

    [Fact]
    public async Task ALineWithoutALastDay_HasNoPeriod_AndComesAfterTheOnesWithOne_T521_KK3()
    {
        PanelHomeModel model = Page(new PanelHomeDetails(0, 0, 0, [new AwaitingIbanRow(Guid.NewGuid(), "SP-A", null), new AwaitingIbanRow(Guid.NewGuid(), "SP-B", new DateOnly(2026, 12, 1))]));

        await model.OnGetAsync(Ct);

        Assert.Equal(["SP-B", "SP-A"], model.Awaiting.Shown.Select(r => r.OrderNumber));
        Assert.Null(model.Awaiting.Shown[1].Period);
        Assert.False(model.Awaiting.HasMore);
    }

    // ---- KK1: the warnings ----

    [Fact]
    public async Task ACountOfZero_OpensNoWarning_AndACountAboveZero_OpensItsOwnLine_T521_KK1()
    {
        PanelHomeModel none = Page(new PanelHomeDetails(0, 0, 0, []));
        await none.OnGetAsync(Ct);
        Assert.DoesNotContain(none.Warnings, w => w.Id is "home-warning-email-undelivered" or "home-warning-card-refund-failed" or "home-warning-card-refund-pending");
        Assert.Empty(none.Awaiting.Shown);

        PanelHomeModel some = Page(new PanelHomeDetails(2, 1, 4, []));
        await some.OnGetAsync(Ct);
        Assert.Equal(
            ["2 siparişte müşteriye e-posta ulaşmadı.", "1 siparişte kart iadesi gerçekleşmedi.", "4 siparişte kart iadesinin sonucu bekleniyor."],
            some.Warnings.Where(w => w.Id.StartsWith("home-warning-", StringComparison.Ordinal) && w.Id != "home-warning-sales").Select(w => w.Text));
        Assert.Equal("/panel/siparisler?pendingWork=EmailUndeliveredMark", some.Warnings.Single(w => w.Id == "home-warning-email-undelivered").Href);
        Assert.Equal("/panel/siparisler?pendingWork=AwaitingReturnOrRefund", some.Warnings.Single(w => w.Id == "home-warning-card-refund-failed").Href);
        Assert.Equal("/panel/siparisler?pendingWork=AwaitingReturnOrRefund", some.Warnings.Single(w => w.Id == "home-warning-card-refund-pending").Href);
    }

    private static PanelHomeModel Page(PanelHomeDetails details)
    {
        StubModule<IPanelHome> home = new();
        home.On(nameof(IPanelHome.GetAsync), _ => new PanelHome(PanelHomeRules.Counters(new Dictionary<PanelCounterKind, int>()), false, false, null));
        StubModule<IPanelHomeDetails> detailsReader = new();
        detailsReader.On(nameof(IPanelHomeDetails.GetAsync), _ => details);
        StubModule<ISettingsModule> settings = new();
        settings.On(nameof(ISettingsModule.GetSalesGateAsync), _ => new SalesGate([], []));
        ManualTimeProvider time = new(Now);
        return new PanelHomeModel(home.Object, detailsReader.Object, settings.Object, time);
    }

    private static PanelHomeDetailsService Service(StubModule<INotificationsModule> notifications, StubModule<IPaymentModule> payment, StubModule<IAfterSalesModule> afterSales) =>
        new(notifications.Object, payment.Object, afterSales.Object, new StubModule<IOrderingModule>().Object);

    private static StubModule<INotificationsModule> NoMarkedOrders()
    {
        StubModule<INotificationsModule> notifications = new();
        notifications.On(nameof(INotificationsModule.FindMarkedOrderIdsAsync), _ => (IReadOnlyList<Guid>)[]);
        return notifications;
    }

    private static StubModule<IPaymentModule> NoCardProblems()
    {
        StubModule<IPaymentModule> payment = new();
        payment.On(nameof(IPaymentModule.CountCardRefundProblemsAsync), _ => new CardRefundProblems(0, 0));
        return payment;
    }

    private static StubModule<IAfterSalesModule> NoIbanLines()
    {
        StubModule<IAfterSalesModule> afterSales = new();
        afterSales.On(nameof(IAfterSalesModule.ListAwaitingIbanLinesAsync), _ => (IReadOnlyList<AwaitingIbanLine>)[]);
        return afterSales;
    }
}

