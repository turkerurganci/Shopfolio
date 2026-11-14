using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The order side of the panel's order list filter (T527; 07 UC-91; 04 9.8.3, 9.8.4): the search - the order number or the order's contact e-mail, matched whole; the e-mail without regard to
/// letter case (06 4.17 <c>NormalizedContactEmail</c>) -, the order and payment statuses (none named: no filter on that axis) and, for a pending work filter, the set of orders the work names
/// (<see cref="Within"/>; null: every order). The parts apply together (04 9.8.4).
/// </summary>
public sealed record PanelOrderFilter(
    string? Search,
    IReadOnlyCollection<OrderStatus> OrderStatuses,
    IReadOnlyCollection<PaymentStatus> PaymentStatuses,
    IReadOnlyCollection<Guid>? Within);

/// <summary>An order the filter keeps, with the moment it was placed: the key a list sorted outside the database (the timed pending work filters, 04 9.8.7) cuts its page from.</summary>
public sealed record PanelOrderKey(Guid OrderId, DateTimeOffset CreatedAt);

/// <summary>
/// One row of the panel's order list as the order gives it (T527; 04 9.8.5): the number and the moment, the recipient and the contact e-mail - empty once the order's personal data is disposed -,
/// the first item's name with its option values and the number of the other items, the total, the payment method and the two status axes.
/// </summary>
public sealed record PanelOrderRow(
    Guid OrderId,
    string OrderNumber,
    DateTimeOffset CreatedAt,
    string? RecipientName,
    string? ContactEmail,
    string FirstItemName,
    int OtherItemCount,
    decimal Total,
    PaymentMethod PaymentMethod,
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus);

/// <summary>
/// One running period of an order the firm owes (T527; 04 9.8.5, 2.6.3): the shipping promise (Z-10) of a preparing order or a service item's fulfilment period (Z-39), with its last day; the caller
/// places the day against its own instant (<c>PeriodStanding.Of</c>). An order may carry several.
/// </summary>
public sealed record OrderRunningPeriod(Guid OrderId, Shopfolio.Infrastructure.CountedPeriod Period, DateOnly LastDay);

/// <summary>One page of the panel's order list: the rows, the page shown (a page past the last is the last, 07 2.6.2), the number of pages and the number of orders of the filter.</summary>
public sealed record PanelOrderPage(IReadOnlyList<PanelOrderRow> Rows, int PageNumber, int PageCount, int TotalCount);

/// <summary>
/// What the panel's order page shows beside the customer's order page (T527; 07 UC-92; 04 9.9.4, 9.9.6, 9.9.10-9.9.12; K-788, K-1188 (3)): the order's marks, each item's facts the
/// operation decision reads (open quantity, the item's marks, its periods' last days), the refunds still pending with the amount the system worked out, the firm's running periods, and the
/// three panel-only parts - the address history, the e-mail history and the internal note. Every number is the server's; the page works out none. Read at every drawing, never stored.
/// <see cref="TransferLastPaymentDay"/> is a transfer order's last payment day (Z-8), the same day the list shows (D-1436); null on a card order.
/// </summary>
public sealed record PanelOrderDetailFacts(
    PanelOrderMarkFacts Marks,
    IReadOnlyList<PanelItemFacts> Items,
    IReadOnlyList<PanelPendingRefund> PendingRefunds,
    IReadOnlyList<OrderRunningPeriod> RunningPeriods,
    IReadOnlyList<PanelAddressChange> AddressHistory,
    IReadOnlyList<PanelEmailChange> EmailCorrections,
    IReadOnlyList<Shopfolio.Modules.Notifications.Contracts.OrderNoticeLine> Notices,
    PanelInternalNote? InternalNote,
    bool PersonalDataDisposed,
    DateOnly? TransferLastPaymentDay);

/// <summary>The order-level marks whose condition holds now (04 9.9.4; 03 1.7.3.1, 1.7.3.2, 1.7.3.3, 1.7.3.6, 1.7.3.7).</summary>
public sealed record PanelOrderMarkFacts(bool EmailNotDelivered, bool PaymentResultMissing, bool RefundNotRealized, bool RefundOutcomePending, bool EmailCorrected);

/// <summary>
/// One item's panel facts (04 9.9.6): the open quantity the operation decision reads (K-790), "IBAN bekleniyor" (an open IBAN request on the item), the units of "iade malı bekleniyor" (on
/// their way back, 03 1.7.3.5) and the last days of the withdrawal window (Z-13, Z-14) and of the defect channel (Z-18); null while the period does not run.
/// </summary>
public sealed record PanelItemFacts(Guid OrderItemId, int OpenQuantity, bool IbanPending, int ReturnGoodsPendingUnits, DateOnly? WithdrawalLastDay, DateOnly? DefectLastDay);

/// <summary>
/// One pending refund (04 9.9.6 "bekleyen geri ödeme"; 02 7.1.7; K-788, K-491): the item and units it covers, the amount the system worked out, the way, the customer's IBAN on a transfer -
/// read only - and the refund period's last day (Z-16, Z-17).
/// </summary>
public sealed record PanelPendingRefund(Guid RefundId, Guid? OrderItemId, int? Quantity, decimal Amount, PaymentMethod Way, string? Iban, DateOnly? LastDay);

/// <summary>One address correction (04 9.9.10; 06 4.62): the moment, the address type, the old and the new value (the shipping phone included) and the administrator.</summary>
public sealed record PanelAddressChange(DateTimeOffset At, AddressType AddressType, OrderPageAddress Old, OrderPageAddress New, string AdminName);

/// <summary>One correction of a guest order's e-mail (04 9.9.12; 02 10.4.11; K-586): the moment, the old and the new address and the administrator.</summary>
public sealed record PanelEmailChange(DateTimeOffset At, string OldEmail, string NewEmail, string AdminName);

/// <summary>The order's internal note (04 9.9.11; 06 4.20): the text and the administrator who last wrote it.</summary>
public sealed record PanelInternalNote(string Text, string AdminName);
