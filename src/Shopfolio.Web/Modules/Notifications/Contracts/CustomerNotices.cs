using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>
/// An event the customer is told about at the order's contact e-mail (B-1…B-8; 08 2.2.6; 02 9.2): the values of the event, which the dispatch row keeps because
/// the body is made at the time of sending and the event's record may be changed or gone by then (06 4.58 <c>EventValues</c>; 05 4.4.2). The order itself
/// (its number, access key, frozen IBAN and documents) is read when the body is made; the dispatch row's order link points at it. The customer's name is not
/// a value of any of these notices: the greeting has no name (08 2.2.6 item 1).
/// </summary>
public abstract record CustomerNotice(Guid OrderId);

/// <summary>
/// B-1: the order was placed, or the firm corrected a guest order's e-mail address. <paramref name="OriginalDispatchId"/> is the row id of the order's first B-1 on
/// the correction (06 4.58): the new row is a re-sending of it (08 2.2.6 B-1 "Düzeltme tetiği"); on the placement it is null. A caller may also give the row of an
/// earlier correction: the queue resolves the chain's root and writes the root's id, so every row of the e-mail is in one group.
/// <paramref name="FirstEmailSentAt"/> is the creation moment of that first row: the queue reads it from the first row and writes it into the new row's event values
/// (06 4.58 <c>EventValues</c>; 08 2.2.5 item 6), so the re-send sentence keeps its date when the first row is deleted by its own period; callers leave it null,
/// and the reader of a stored row fills it.
/// </summary>
public sealed record OrderReceivedNotice(Guid OrderId, Guid? OriginalDispatchId = null, DateTimeOffset? FirstEmailSentAt = null) : CustomerNotice(OrderId);

/// <summary>B-2: the bank transfer was chosen. <paramref name="PaymentWindowEndsAt"/> is the order's window end at the event (the last payment day is the day before it ends).</summary>
public sealed record TransferChosenNotice(Guid OrderId, DateTimeOffset PaymentWindowEndsAt) : CustomerNotice(OrderId);

/// <summary>B-3: the transfer reminder. <paramref name="PaymentWindowEndsAt"/> is the window end at the event - the new one in a window that restarted (08 2.2.6 B-3 "Değerler").</summary>
public sealed record TransferReminderNotice(Guid OrderId, DateTimeOffset PaymentWindowEndsAt) : CustomerNotice(OrderId);

/// <summary>B-4: the payment was approved (the card was confirmed, or the administrator marked the transfer as paid). A digital item is read from the order's items.</summary>
public sealed record PaymentConfirmedNotice(Guid OrderId) : CustomerNotice(OrderId);

/// <summary>
/// B-5: the order was shipped, or its open physical items were sent again with new tracking data. The values are those of the moment (a later correction is B-12):
/// <paramref name="Carrier"/> is the code of a listed company (null for "Diğer" and for own-vehicle delivery), <paramref name="OtherCarrierName"/> the name of an
/// unlisted company, <paramref name="IsOwnVehicleDelivery"/> the "kendi aracımızla teslim" declaration, in which no company or number is written.
/// </summary>
public sealed record ShippedNotice(Guid OrderId, string? Carrier, string? OtherCarrierName, string? TrackingNumber, bool IsOwnVehicleDelivery) : CustomerNotice(OrderId);

/// <summary>B-6: the shipment could not be delivered and came back to the firm.</summary>
public sealed record DeliveryFailedNotice(Guid OrderId) : CustomerNotice(OrderId);

/// <summary>The item of an item cancellation (B-7): the product name and, on a variant, the option values, and the cancelled quantity (08 2.2.6 item 4).</summary>
public sealed record CancelledItem(string ProductName, string? FirstOptionValue, string? SecondOptionValue, int Quantity);

/// <summary>
/// B-7: the order or one of its items was cancelled. <paramref name="Actor"/> chooses the wording (customer, administrator, system - <see cref="ActorKind"/>).
/// An administrator's cancellation carries <paramref name="AdministratorReason"/> (and <paramref name="ReasonNote"/> for "Other"); a system cancellation carries
/// <paramref name="SystemReason"/> - never <see cref="TransitionReason.Closure"/>, which makes no notice; a customer's cancellation carries no reason.
/// <paramref name="Item"/> is set only on an item cancellation, which only the customer and the administrator make.
/// </summary>
public sealed record CancelledNotice(
    Guid OrderId,
    ActorKind Actor,
    CancellationReason? AdministratorReason,
    string? ReasonNote,
    TransitionReason? SystemReason,
    CancelledItem? Item) : CustomerNotice(OrderId);

/// <summary>
/// B-8: a refund was processed. <paramref name="RefundType"/> is the reason (<see cref="RefundType.LatePayment"/> has its own sentence),
/// <paramref name="Method"/> the way the money went (card or transfer); the IBAN of a transfer refund is never written.
/// </summary>
public sealed record RefundProcessedNotice(Guid OrderId, RefundType RefundType, decimal Amount, PaymentMethod Method) : CustomerNotice(OrderId);
