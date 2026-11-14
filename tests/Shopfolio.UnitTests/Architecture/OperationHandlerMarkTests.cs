using System.Reflection;

using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Operations;
using Shopfolio.Pages.Orders;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Every POST handler of the customer's order pages and of the panel's order page (E-37) is an operation of 03 1.11 and carries <c>[GatedOperation]</c> (09 8.3; D-153, T512, T527): the source scan
/// only checks that a marked handler reaches the gate, so an unmarked operation POST would slip past it. The rule runs over the page models of the order
/// pages by reflection; a POST that is not an operation is named one by one with its reason, never by a pattern, and a named exemption that no longer
/// exists is a finding too. A deliberate sample shows the rule going red (09 19.2 item 3), the panel page's own sample among them.
/// </summary>
public sealed class OperationHandlerMarkTests
{
    // The order lookup finds an order by its number and e-mail address; it applies no operation of 03 1.11 (07 UC-32). The payment page (07 UC-29; T510) starts the confirmed
    // order's payment: it is the ordering flow's step, not an operation of 03 1.11, and an order that no longer awaits its payment is the screen's 303 (K-1209 (1)).
    private static readonly string[] _notOperations =
    [
        $"{nameof(TrackingModel)}.{nameof(TrackingModel.OnPostFindOrderAsync)}",
        $"{nameof(ConfirmationPageModel)}.{nameof(ConfirmationPageModel.OnPostStartPaymentAsync)}",
    ];

    // The panel's order page: its six line steps are 03 1.11.15-1.11.19 and 1.11.40 (07 UC-123...UC-128). Every POST of the page is an operation (07 9.9: the page's ends are UC-123...UC-154).
    private static readonly Type[] _panelOrderPages = [typeof(OrderDetailModel)];

    [Fact]
    public void EveryPostHandlerOfTheOrderPages_CarriesTheGatedOperationMark()
    {
        IEnumerable<Type> pages = typeof(Program).Assembly.GetTypes()
            .Where(t => t.Namespace == typeof(OrderPageModel).Namespace && typeof(PageModel).IsAssignableFrom(t));

        Assert.Empty(Unmarked(pages, _notOperations));
    }

    [Fact]
    public void EveryPostHandlerOfThePanelOrderPage_CarriesTheGatedOperationMark()
    {
        Assert.Empty(Unmarked(_panelOrderPages, []));
    }

    [Fact]
    public void TheOrderPages_HaveOperationHandlers_SoTheRuleIsNotVacuous()
    {
        Assert.Contains(Handlers(typeof(CancellationPageModel)), m => m.GetCustomAttribute<GatedOperationAttribute>() is not null);
        Assert.Contains(Handlers(typeof(LateDeliveryTerminationPageModel)), m => m.GetCustomAttribute<GatedOperationAttribute>() is not null);
        Assert.Equal(
            [
                nameof(OrderDetailModel.OnPostCloseDeliveryFailedAsync),
                nameof(OrderDetailModel.OnPostMarkBankTransferRefundFailedAsync),
                nameof(OrderDetailModel.OnPostMarkCardRefundFailedAsync),
                nameof(OrderDetailModel.OnPostMarkDeliveredAsync),
                nameof(OrderDetailModel.OnPostMarkDeliveryFailedAsync),
                nameof(OrderDetailModel.OnPostMarkPaidAsync),
                nameof(OrderDetailModel.OnPostMarkReshippedAsync),
                nameof(OrderDetailModel.OnPostMarkShippedAsync),
                nameof(OrderDetailModel.OnPostProcessAmountBasedRefundAsync),
                nameof(OrderDetailModel.OnPostProcessRefundAsync),
                nameof(OrderDetailModel.OnPostRequestRefundIbanAsync),
                nameof(OrderDetailModel.OnPostRetryCardRefundAsync),
                nameof(OrderDetailModel.OnPostSwitchRefundToBankTransferAsync),
            ],
            Handlers(typeof(OrderDetailModel)).Select(m => m.Name).Order(StringComparer.Ordinal));
    }

    [Fact]
    public void AnUnmarkedStepOfAPanelOrderPage_IsAFinding_AndItsMarkedStepIsNot()
    {
        IReadOnlyList<string> findings = Unmarked([typeof(PanelSamplePage)], []);

        Assert.Equal([$"{nameof(PanelSamplePage)}.{nameof(PanelSamplePage.OnPostMarkShippedAsync)}"], findings);
    }

    [Fact]
    public void AnUnmarkedPostHandler_IsAFinding_AndAMarkedOneOrANamedExemptionIsNot()
    {
        IReadOnlyList<string> findings = Unmarked([typeof(SamplePage)], [$"{nameof(SamplePage)}.{nameof(SamplePage.OnPostLookup)}"]);

        Assert.Equal([$"{nameof(SamplePage)}.{nameof(SamplePage.OnPostCancel)}"], findings);
    }

    [Fact]
    public void ANamedExemptionThatNoLongerExists_IsAFinding()
    {
        IReadOnlyList<string> findings = Unmarked([typeof(MarkedSamplePage)], ["MarkedSamplePage.OnPostGone"]);

        Assert.Equal(["exemption without a handler: MarkedSamplePage.OnPostGone"], findings);
    }

    private static IEnumerable<MethodInfo> Handlers(Type page) =>
        page.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m.Name.StartsWith("OnPost", StringComparison.Ordinal));

    private static List<string> Unmarked(IEnumerable<Type> pages, IReadOnlyCollection<string> exempt)
    {
        List<string> handlers = [.. pages.SelectMany(p => Handlers(p).Select(m => (Page: p, Method: m)))
            .Select(h => (Name: $"{h.Page.Name}.{h.Method.Name}", Marked: h.Method.GetCustomAttribute<GatedOperationAttribute>() is not null))
            .Where(h => !h.Marked)
            .Select(h => h.Name)];

        List<string> all = [.. pages.SelectMany(p => Handlers(p).Select(m => $"{p.Name}.{m.Name}"))];

        return [.. handlers.Where(h => !exempt.Contains(h)), .. exempt.Where(e => !all.Contains(e)).Select(e => $"exemption without a handler: {e}")];
    }

    private sealed class SamplePage : PageModel
    {
        public void OnPostCancel() => _ = HttpContext;

        [GatedOperation]
        public void OnPostTerminate() => _ = HttpContext;

        public void OnPostLookup() => _ = HttpContext;

        public void OnGet() => _ = HttpContext;
    }

    private sealed class MarkedSamplePage : PageModel
    {
        [GatedOperation]
        public void OnPostCancel() => _ = HttpContext;
    }

    // The T527 round 1 finding as a sample: a panel order page whose shipment step went through a plain transaction, unmarked, beside a marked step.
    private sealed class PanelSamplePage : PageModel
    {
        [GatedOperation]
        public void OnPostMarkPaidAsync() => _ = HttpContext;

        public void OnPostMarkShippedAsync() => _ = HttpContext;

        public void OnGet() => _ = HttpContext;
    }
}
