using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The customer's withdrawal declaration from the order page (03 1.11.5, 1.11.6, 1.7.1.5; 02 7.3.5): the item, the withdrawn units and the last step's confirmation
/// (02 5.9: the declaration is irreversible and never done with one touch - K-732). On an order paid by bank transfer the customer enters the refund IBAN with the
/// declaration (02 7.4.5); with an IBAN already on the order's file it may be left empty. The declaration's date is the moment it is sent - nothing else is asked.
/// </summary>
public sealed record DeclareWithdrawalRequest(Guid OrderId, Guid OrderItemId, int Quantity, bool Confirmed, string? Iban = null);

/// <summary>
/// The administrator's record of a withdrawal notified by another channel - e-mail, letter or the model form (03 1.11.34; 02 10.4.10, 8.3.8): the item and units, the
/// date the notice reached the firm (past allowed, future never), whether the notice came from the order's contact e-mail (only then is an IBAN in it carried over, and
/// only on the transfer line), the IBAN the notice carried, the firm's answer whether the delivery came first when the date is the item's delivery day (K-608), the
/// irreversible-action confirmation (02 5.9; K-715) and the separate confirmation the warnings ask for (outside the window, beyond its year, an absolute exemption; K-709,
/// K-1563). The firm's check that the notice is the order owner's is the firm's own (K-607): the product neither asks nor checks it, and it never changes the date.
/// </summary>
public sealed record RecordWithdrawalRequest(
    Guid OrderId,
    Guid OrderItemId,
    TransitionAdmin Admin,
    int ExpectedVersion,
    bool Confirmed,
    int Quantity,
    DateOnly NoticeDate,
    bool CameFromOrderEmail,
    string? NoticeIban = null,
    bool? IsDeliveredBeforeDeclaration = null,
    bool WarningsConfirmed = false);

/// <summary>
/// What the panel tells the firm before it records a notice from another channel (02 10.4.10; K-709, K-1563): the date is after the window's last day (a notice sent in
/// time that arrived late, or the incomplete information of m.10 - both counted together); the date is more than a year after the window's end (the notice must have been
/// sent within that year and the proof is the consumer's); the item carries an absolute exemption. Any of them asks the firm to confirm the record once more; none of
/// them refuses it. <see cref="AsksDeliveryOrder"/> - the date is the item's delivery day, so the firm answers whether the delivery came before the notice.
/// </summary>
public sealed record WithdrawalNoticeWarnings(bool OutsideWindow, bool BeyondOneYear, bool AbsoluteExemption, bool AsksDeliveryOrder)
{
    public static WithdrawalNoticeWarnings None { get; } = new(false, false, false, false);

    /// <summary>True when the record needs the firm's separate confirmation.</summary>
    public bool NeedsConfirmation => OutsideWindow || BeyondOneYear || AbsoluteExemption;
}

/// <summary>
/// The result of a withdrawal (09 9.1 item 3): a refusal is a value and the runner rolls the transaction back; <see cref="WithdrawalId"/> is the declaration written.
/// <see cref="Warnings"/> is what the firm was (or must be) told on a record from another channel; <see cref="Closure"/> is the transition the closure rule wrote when
/// the declaration closed the order's last open item (S2, S10, S11 to Delivered, or S3, S4 to Cancelled; 02 5.6.6) - the declaration itself changes no axis.
/// </summary>
public sealed record WithdrawalResult(TransitionRefusal? Refusal, Guid? WithdrawalId, WithdrawalNoticeWarnings Warnings, TransitionCode? Closure) : IOperationResult
{
    public bool IsRejected => Refusal is not null;
}

/// <summary>
/// The order page's declaration screen of one item (02 7.3.5; 04 5.19): the units open to the withdrawal and the window's last day, and what the screen tells the customer
/// before the last step. A physical item's screen shows the firm's current return address and the period to send the goods back (<see cref="SendingPeriodDays"/>); on a
/// shipped item not yet marked delivered it first says the carrier brings the goods back when they are not taken in (K-838); a service item's shows neither (K-530). An item
/// frozen with a conditional exemption shows the protective packaging condition (K-509). <see cref="AsksIban"/> - the order was paid by bank transfer and no IBAN is on file.
/// </summary>
public sealed record WithdrawalScreen(
    Guid OrderItemId,
    ProductType ProductType,
    int OpenQuantity,
    DateOnly? LastDay,
    ItemReturnAddress? ReturnAddress,
    int? SendingPeriodDays,
    bool CarrierReturnsUnreceivedGoods,
    WithdrawalExemptionReason? PackagingCondition,
    bool AsksIban);
