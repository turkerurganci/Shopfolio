using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// The panel home's data (02 10.6.1, 10.8.2; 11 T382; KP-50, KP-65): the six pending work counters, the two warnings and the setup checklist. It is computed from the records at every
/// call and never stored; no new screen and no new data (K-405, K-467). The page that shows it is E-31's (T52x).
/// </summary>
public interface IPanelHome
{
    /// <summary>The home's data now; nothing is written.</summary>
    Task<PanelHome> GetAsync(CancellationToken ct);
}

/// <summary>
/// The six counters that wait for the firm, the two read warnings, and the setup checklist - null once every item is complete (02 10.8.2).
/// </summary>
/// <param name="Counters">Exactly six, one per <see cref="PanelCounterKind"/>, in the enum's order; a seventh counter is never opened (02 10.6.1; K-535, K-467).</param>
/// <param name="ChannelWarningOpen">The e-mail channel warning (05 10.2.3).</param>
/// <param name="FirmNoticesWarningOpen">The firm notices warning (05 10.2.4).</param>
/// <param name="SetupChecklist">The five items of 02 10.8.2 with the completed ones marked; null when all are complete.</param>
public sealed record PanelHome(
    IReadOnlyList<PanelCounter> Counters,
    bool ChannelWarningOpen,
    bool FirmNoticesWarningOpen,
    IReadOnlyList<SetupChecklistItem>? SetupChecklist);

/// <summary>
/// What only the home page shows beyond <see cref="IPanelHome"/> (T521; 04 2.6.2, 3.4.3; 02 10.6.1): the order counts of the three order-level warnings and the "IBAN bekleniyor" lines. Kept apart from
/// <see cref="IPanelHome"/>, which every panel screen reads for its frame: the heavier reads below are the home page's own. Computed at every call, nothing is stored (K-405).
/// </summary>
public interface IPanelHomeDetails
{
    /// <summary>The home's extra data now; nothing is written.</summary>
    Task<PanelHomeDetails> GetAsync(CancellationToken ct);
}

/// <summary>The orders whose customer e-mail did not arrive, the orders with a failed card refund, the orders whose card refund awaits its result, and every open "IBAN bekleniyor" line.</summary>
public sealed record PanelHomeDetails(int EmailNotDeliveredOrders, int CardRefundFailedOrders, int CardRefundOutcomePendingOrders, IReadOnlyList<AwaitingIbanRow> AwaitingIban);

/// <summary>
/// One "IBAN bekleniyor" line (04 9.3.6): the order and its number, the item's name (null on the refund of a late payment, which has no item), the reason of the request, the instant it was opened, and the
/// last day of the refund period when the line has one.
/// </summary>
public sealed record AwaitingIbanRow(
    Guid OrderId,
    string OrderNumber,
    string? ItemName,
    RefundIbanRequestReason Reason,
    DateTimeOffset RequestedAt,
    DateOnly? RefundLastDay);

/// <summary>One counter: its name, the count and the list it leads to. The unit is the order, except the open request, whose unit is the request.</summary>
public sealed record PanelCounter(PanelCounterKind Kind, int Count, PanelListTarget ListTarget);

/// <summary>One item: done, or waiting - and for the identity, the missing field names.</summary>
public sealed record SetupChecklistItem(SetupChecklistItemKind Kind, bool IsDone, IReadOnlyList<string> MissingFields);
