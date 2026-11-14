using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;
using Shopfolio.Pages.Lists;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The query model of the order list (E-36; 07 5.3.2 <c>OrderQuery</c>; UC-91): the search, the order and payment statuses - both repeating (07 2.6.4) -, the one pending work filter and <c>page</c>.
/// Every field is text so a wrong value is never a binding error (07 2.6.6): <see cref="OrdersModel"/> reads the closed lists and drops a value outside them. There is no sort (07 2.6.5).
/// </summary>
public sealed class OrderQuery : PagedQuery
{
    [SpecAssignedName]
    public string? Search { get; init; }

    public string[]? OrderStatus { get; init; }

    public string[]? PaymentStatus { get; init; }

    [SpecAssignedName]
    public string? PendingWork { get; init; }
}

/// <summary>One option of a filter (04 9.8.4): the parameter's value, its name and whether the request selected it.</summary>
public sealed record OrderFilterOption(string Value, string Label, bool IsSelected);

/// <summary>A selected filter value shown at the top of the list with the address that removes it (04 9.8.9; 2.13.3).</summary>
public sealed record OrderFilterChip(string Label, string RemovePath);

/// <summary>
/// E-36, the order list (T527; 04 9.8; 07 UC-91): every order newest first - the unpaid order the system cancelled by itself too (04 9.8.12) -, the search, the two status filters and the pending work
/// filter, applied together; a row opens the order (E-37) carrying the list's address in <c>returnUrl</c> (07 2.6.8). The screen has no primary button, no bulk action and no action on a row (04 9.8.2,
/// 9.8.8). The GET writes nothing.
/// </summary>
public sealed class OrdersModel : PageModel
{
    /// <summary>The sentence of a list with no order at all (04 9.8.13): the orders come from the storefront, so there is no button that opens one.</summary>
    public const string NoOrdersText = "Henüz sipariş yok.";

    /// <summary>The sentence of a search or filter whose result is empty; its way out removes them (04 9.8.13).</summary>
    public const string NoMatchText = "Bu arama ya da süzgece uyan sipariş yok.";

    public const string RemoveFilterLabel = "Aramayı ve süzgeci kaldır";

    public const string HomeLabel = "Ana sayfaya dön";

    private readonly IPanelOrderList _orders;
    private readonly TimeProvider _time;

    public OrdersModel(IPanelOrderList orders, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(time);
        _orders = orders;
        _time = time;
    }

    /// <summary>The table of the page's rows; null when the list is empty (the empty-state line stands in its place).</summary>
    public PanelTable? Table { get; private set; }

    public Pager Pager { get; private set; } = new(null, null, []);

    public EmptyStateView? Empty { get; private set; }

    public string? Search { get; private set; }

    public IReadOnlyList<OrderFilterOption> OrderStatusOptions { get; private set; } = [];

    public IReadOnlyList<OrderFilterOption> PaymentStatusOptions { get; private set; } = [];

    public IReadOnlyList<OrderFilterOption> WorkOptions { get; private set; } = [];

    /// <summary>The search and the selected filter values, each with the address that removes only it (04 9.8.9).</summary>
    public IReadOnlyList<OrderFilterChip> Chips { get; private set; } = [];

    /// <summary>True when the request carries a search or a filter that applies; a value outside the closed lists applies nothing (07 2.6.6).</summary>
    public bool IsFilterApplied { get; private set; }

    public string ListRoot => PanelFramePaths.Orders;

    public async Task<IActionResult> OnGetAsync(OrderQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        string? search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim();
        IReadOnlyList<OrderStatus> orderStatuses = ListFilters.ReadCodes<OrderStatus>(query.OrderStatus);
        IReadOnlyList<PaymentStatus> paymentStatuses = ListFilters.ReadCodes<PaymentStatus>(query.PaymentStatus);
        PanelOrderWork? work = ListFilters.ReadCode<PanelOrderWork>(query.PendingWork);
        Search = search;
        IsFilterApplied = search is not null || orderStatuses.Count > 0 || paymentStatuses.Count > 0 || work is not null;

        PanelOrderListPage page = await _orders.ListAsync(
            new PanelOrderListQuery(search, orderStatuses, paymentStatuses, work), query.RequestedPage(), ListCatalog.PageSizeOf(ListKind.PanelOrders), ct);
        string listAddress = ListAddress.Of(ListRoot, query, page.PageNumber);

        OrderStatusOptions = [.. Enum.GetValues<OrderStatus>().Select(s => new OrderFilterOption(s.ToString(), StatusBadges.For(s).Text, orderStatuses.Contains(s)))];
        PaymentStatusOptions = [.. Enum.GetValues<PaymentStatus>().Select(s => new OrderFilterOption(s.ToString(), StatusBadges.For(s).Text, paymentStatuses.Contains(s)))];
        WorkOptions = [.. Enum.GetValues<PanelOrderWork>().Select(w => new OrderFilterOption(w.ToString(), OrderListTexts.WorkOf(w), w == work))];
        Chips =
        [
            .. search is null ? [] : new[] { new OrderFilterChip($"Arama: {search}", ListAddress.Of(ListRoot, With(null, orderStatuses, paymentStatuses, work), PageNumber.First)) },
            .. orderStatuses.Select(s => new OrderFilterChip(StatusBadges.For(s).Text, ListAddress.Of(ListRoot, With(search, orderStatuses.Where(x => x != s), paymentStatuses, work), PageNumber.First))),
            .. paymentStatuses.Select(s => new OrderFilterChip(StatusBadges.For(s).Text, ListAddress.Of(ListRoot, With(search, orderStatuses, paymentStatuses.Where(x => x != s), work), PageNumber.First))),
            .. work is { } w ? new[] { new OrderFilterChip(OrderListTexts.WorkOf(w), ListAddress.Of(ListRoot, With(search, orderStatuses, paymentStatuses, null), PageNumber.First)) } : [],
        ];

        if (page.Rows.Count == 0)
        {
            Empty = IsFilterApplied
                ? EmptyStates.WithExit(NoMatchText, RemoveFilterLabel, ListRoot)
                : EmptyStates.WithExit(NoOrdersText, HomeLabel, PanelFramePaths.Home);
            return Page();
        }

        DateTimeOffset now = _time.GetUtcNow();
        Table = new PanelTable("Siparişler", Columns, [.. page.Rows.Select(row => RowOf(row, listAddress, now))]);
        Pager = Pager.Of(ListRoot, query, page.PageNumber, page.PageCount);
        return Page();
    }

    private static readonly PanelTableColumn[] Columns =
    [
        new("Sipariş", true),
        new("Tarih", true),
        new("Alıcı", false),
        new("Kalemler", false),
        new("Toplam", true),
        new("Ödeme yöntemi", false),
        new("Sipariş durumu", true),
        new("Ödeme durumu", true),
        new("İşaretler", false),
        new("Süre", true),
    ];

    private static OrderQuery With(string? search, IEnumerable<OrderStatus> orderStatuses, IEnumerable<PaymentStatus> paymentStatuses, PanelOrderWork? work) => new()
    {
        Search = search,
        OrderStatus = [.. orderStatuses.Select(s => s.ToString())],
        PaymentStatus = [.. paymentStatuses.Select(s => s.ToString())],
        PendingWork = work?.ToString(),
    };

    private static PanelTableRow RowOf(PanelOrderListRow row, string listAddress, DateTimeOffset now)
    {
        PanelOrderRow order = row.Order;
        Badge shipping = StatusBadges.For(order.OrderStatus);
        Badge payment = StatusBadges.For(order.PaymentStatus);
        string buyer = string.Join(" · ", new[] { order.RecipientName, order.ContactEmail }.Where(v => !string.IsNullOrWhiteSpace(v)));
        string items = order.OtherItemCount > 0 ? $"{order.FirstItemName} ve {order.OtherItemCount} kalem daha" : order.FirstItemName;
        string period = row.LeastPeriod is { } least ? OrderListTexts.PeriodTextOf(OrderListTexts.PeriodOf(least, now)) : string.Empty;
        return new PanelTableRow(
            [
                order.OrderNumber,
                DisplayFormat.DateAndTime(order.CreatedAt),
                buyer,
                items,
                DisplayFormat.Amount(order.Total),
                OrderListTexts.MethodOf(order.PaymentMethod),
                shipping.Text,
                payment.Text,
                string.Join(" · ", OrderListTexts.MarksOf(row.Marks, PanelMarkPlace.OrderList).Select(PanelMarks.TextOf)),
                period,
            ],
            ListReturn.LinkTo($"{PanelFramePaths.Orders}/{order.OrderId}", listAddress),
            new Dictionary<int, Badge> { [6] = shipping, [7] = payment });
    }
}

/// <summary>The order list's own words (04 9.8.4, 9.8.5): the pending work filters' names, the payment method and the running period's label.</summary>
internal static class OrderListTexts
{
    /// <summary>The order's marks whose condition holds and that have a place in <paramref name="place"/> (04 2.5.3; 9.8.5, 9.9.4): the one mapping of the order's mark facts the list and the page share.</summary>
    public static IReadOnlyList<PanelMark> MarksOf(PanelOrderMarkFacts marks, PanelMarkPlace place)
    {
        ArgumentNullException.ThrowIfNull(marks);
        return PanelMarks.Of(
            new PanelMarkConditions(
                EmailNotDelivered: marks.EmailNotDelivered,
                PaymentResultMissing: marks.PaymentResultMissing,
                RefundNotCompleted: marks.RefundNotRealized,
                EmailCorrected: marks.EmailCorrected,
                RefundResultPending: marks.RefundOutcomePending),
            place);
    }

    /// <summary>The pending work filter's name: the first five are the home counters' names (02 10.6.1), the two marks their own (04 2.5.3).</summary>
    public static string WorkOf(PanelOrderWork work) => work switch
    {
        PanelOrderWork.AwaitingPayment => PendingCounters.NameOf(PanelCounterKind.PaymentApproval),
        PanelOrderWork.AwaitingShipment => PendingCounters.NameOf(PanelCounterKind.ToShip),
        PanelOrderWork.AwaitingDeliveryMark => PendingCounters.NameOf(PanelCounterKind.DeliveryMarkPending),
        PanelOrderWork.AwaitingServiceCompletion => PendingCounters.NameOf(PanelCounterKind.ServicePending),
        PanelOrderWork.AwaitingReturnOrRefund => PendingCounters.NameOf(PanelCounterKind.RefundAndReturnPending),
        PanelOrderWork.IbanPendingMark => "IBAN bekleniyor",
        PanelOrderWork.EmailUndeliveredMark => "E-posta ulaşmadı",
        _ => throw new ArgumentOutOfRangeException(nameof(work), work, "Not a pending work filter."),
    };

    /// <summary>The payment method as the panel writes it.</summary>
    public static string MethodOf(PaymentMethod method) => method switch
    {
        PaymentMethod.Card => "Kart",
        PaymentMethod.BankTransfer => "Havale",
        _ => throw new ArgumentOutOfRangeException(nameof(method), method, "Not a payment method."),
    };

    /// <summary>The list's period cell (04 9.8.5): the period's name, then where it stands - an overrun says so in words (04 2.6.6).</summary>
    public static string PeriodTextOf(PeriodIndicator indicator)
    {
        ArgumentNullException.ThrowIfNull(indicator);
        return $"{indicator.Label}: {indicator.Text}";
    }

    /// <summary>The running period's indicator with its name (04 2.6.3): the shipping promise and the service fulfilment period carry the legal interest warning on overrun.</summary>
    public static PeriodIndicator PeriodOf(OrderRunningPeriod period, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(period);
        PeriodStanding standing = PeriodStanding.Of(period.Period, period.LastDay, now, BusinessCalendar.Product);
        string label = period.Period switch
        {
            CountedPeriod.ShippingPeriod => "Kargoya verme sözü",
            CountedPeriod.ServiceFulfilmentPeriod => "Hizmetin ifa süresi",
            CountedPeriod.BankTransferPaymentWindow => "Son ödeme günü",
            CountedPeriod.RefundPeriod => OrderDetailModel.RefundPeriodLabel,
            _ => throw new ArgumentOutOfRangeException(nameof(period), period.Period, "Not a running period of the order list."),
        };

        // The legal interest warning belongs to the firm's delivery promises (04 2.6.3; 02 7.2.3); the order's page shows the refund period without it, and so does the list.
        return new PeriodIndicator(label, period.Period, standing, WarnsLegalInterest: period.Period is CountedPeriod.ShippingPeriod or CountedPeriod.ServiceFulfilmentPeriod);
    }
}
