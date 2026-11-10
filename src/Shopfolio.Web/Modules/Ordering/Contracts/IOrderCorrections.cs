using Shopfolio.Infrastructure;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The administrator's order interventions that are not status transitions (T375, T386; 02 7.6.6, 10.4.1-10.4.5, 10.4.7, 10.4.11, 8.3.6, 8.3.7): the address and the guest order's e-mail
/// (forward records that touch no amount, no item, no status and no period), the removal of an item or the reduction of its quantity (the cancellation's regime), the carrier, the
/// delivery date and a return's arrival date, and the internal note. Each runs in the caller's transaction, after the order's row lock (the note after its shared form, 09 10.9),
/// and - all but the note - raises the order's version once (09 9.1). Each writes a trail row. The panel calls them; the customer has no way to (02 8.3.6, 8.3.7). A refusal writes nothing.
/// </summary>
public interface IOrderCorrections
{
    /// <summary>
    /// The correction of the shipping or the billing address (02 10.4.2, 8.3.7; 03 1.11.23): open while the operation decision says so - a shipping address until "Kargoya verildi", the billing
    /// address of an order with a physical item the same, the billing address of an order with none until "Teslim edildi" - and only when the request says how the firm knows it came from
    /// the order's owner (the order's e-mail, or the firm's own return to the order's e-mail or shipping telephone; the number the request came from is no confirmation). The fields are the
    /// address rules' (<c>IMembershipModule.ResolveOrderAddressesAsync</c>: four required fields, the closed province list, the delivery area for a shipping province, the telephone). The old and the
    /// new value stay on the order's own record (<c>OrderAddressHistory</c>), B-10 goes to the order's e-mail, the trail gets a row without values.
    /// </summary>
    Task<OrderCorrectionResult> CorrectAddressAsync(CorrectAddressRequest request, CancellationToken ct);

    /// <summary>
    /// The correction of a guest order's contact e-mail (02 10.4.11, 8.3.6; 03 1.11.35): only a guest order whose personal data is not disposed, and only with the firm's confirmation of the
    /// buyer by the name, the shipping telephone of an order with a physical item, the items and the amount - the order number alone is no confirmation. The new address becomes the contact
    /// e-mail, the old one stays in <c>OrderEmailHistory</c>, the access key is renewed (the old links open nothing), the account link follows the new address (a verified account of it takes the
    /// order, an account deleted earlier never does), unsent notices go to the new address, B-1 goes to the new address and B-15 to the old one; the trail gets a row without values.
    /// </summary>
    Task<OrderCorrectionResult> CorrectGuestEmailAsync(CorrectGuestEmailRequest request, CancellationToken ct);

    /// <summary>
    /// The removal of an item or the reduction of its quantity (T386; 02 10.4.3; 03 1.11.22): no item is added and no price changes. Open from the payment's approval (before
    /// it the order is cancelled whole, 02 7.2.2); a physical item until "Kargoya verildi", a digital or service item until its delivery mark. The removal follows the
    /// cancellation's regime - the refund of 02 7.1.7 by the 7.2.8 line (the IBAN request with the record on the transfer line), the shipping fee by 7.2.9's trigger, stock,
    /// capacity and coupon right back - asks no reason, goes to the customer with B-11 and to the trail (reduction or removal, with the item and the units). A refund to the card
    /// asks the irreversible-action confirmation (02 5.9).
    /// </summary>
    Task<OrderTransitionResult> RemoveItemAsync(RemoveItemRequest request, CancellationToken ct);

    /// <summary>
    /// The correction of the carrier and the tracking number (T386; 02 10.4.4; 03 1.11.24): open on a shipped order with a physical item; the values are the shipment's (a
    /// listed company's code or an unlisted company's name with the number, or the own-vehicle declaration). B-12 goes with the new values; the trail's row names the field,
    /// never the free text.
    /// </summary>
    Task<OrderCorrectionResult> CorrectShipmentAsync(CorrectShipmentRequest request, CancellationToken ct);

    /// <summary>
    /// The correction of the delivery date at the order level (T386; 02 10.4.5, 3.20.11; 03 1.11.25; K-1474, K-608): every delivered physical item takes the new date - an item's
    /// date is not corrected alone; the date is not in the future, not before the day of the shipment and not after a recorded arrival date of returned goods. The withdrawal
    /// window and the defect period run from it; an earlier declaration stays. On a declaration's day the firm answers which came first; without the answer the correction is
    /// refused (<see cref="OrderCorrectionResult.AsksDeliveryOrder"/>). No notice; the trail keeps the old and the new date.
    /// </summary>
    Task<OrderCorrectionResult> CorrectDeliveryDateAsync(CorrectDeliveryDateRequest request, CancellationToken ct);

    /// <summary>
    /// The correction of a return's arrival date (T386; 02 10.4.5; K-712): the receipt's or the rejection's date in place - not in the future, not before the item's delivery date.
    /// The refund's fourteen days run from it. No notice; the trail keeps the old and the new date.
    /// </summary>
    Task<OrderCorrectionResult> CorrectReturnArrivalDateAsync(CorrectReturnArrivalDateRequest request, CancellationToken ct);

    /// <summary>
    /// The administrator's internal note (T386; 02 10.4.7; 06 4.20; K-1095, K-371): one per order, never shown to the customer (no order page, e-mail or contract reads it). An
    /// empty text deletes it. Writing and changing it go to the trail without the text; an order whose personal data is disposed takes no note. The note does not raise the
    /// order's version (09 10.9): the order's row is read under a shared lock and the note is written only while it still holds the text the form showed
    /// (<see cref="SaveInternalNoteRequest.OriginalText"/>); otherwise <c>DisplayedValueConflict</c> with the current note - two administrators never silently
    /// overwrite each other. The caller runs it with <see cref="SaveInternalNoteRequest.RetryOnUniqueIndexes"/>: two first writes at once meet on the unique
    /// index, and the run again answers the second with the conflict.
    /// </summary>
    Task<SaveInternalNoteResult> SaveInternalNoteAsync(SaveInternalNoteRequest request, CancellationToken ct);
}

/// <summary>The removal's input: the item, the removed units and - on a refund to the card - the irreversible-action confirmation (02 5.9). There is no price and no new item.</summary>
public sealed record RemoveItemRequest(Guid OrderId, Guid OrderItemId, TransitionAdmin? Admin, int? ExpectedVersion, int Quantity, bool Confirmed);

/// <summary>The carrier correction's input: the shipment's values (<see cref="ShipmentInfo"/>).</summary>
public sealed record CorrectShipmentRequest(Guid OrderId, TransitionAdmin? Admin, ShipmentInfo? Shipment, int? ExpectedVersion);

/// <summary>The delivery date correction's input; <paramref name="IsDeliveredBeforeDeclaration"/> is the firm's answer on a declaration's day (K-608), null when not asked.</summary>
public sealed record CorrectDeliveryDateRequest(Guid OrderId, TransitionAdmin? Admin, DateOnly DeliveryDate, bool? IsDeliveredBeforeDeclaration, int? ExpectedVersion);

/// <summary>The arrival date correction's input: the item, its receipt or rejection record and the new date.</summary>
public sealed record CorrectReturnArrivalDateRequest(Guid OrderId, Guid OrderItemId, Guid ArrivalRecordId, TransitionAdmin? Admin, DateOnly ArrivalDate, int? ExpectedVersion);

/// <summary>
/// The internal note's input: the text (an empty one deletes the note) and the text the form showed (<see cref="OriginalText"/>, the hidden <c>Original</c>
/// field; empty when there was no note). The note does not take the screen's version (09 10.9; 06 2.6.5): the shown text is its condition.
/// </summary>
public sealed record SaveInternalNoteRequest(Guid OrderId, TransitionAdmin? Admin, string? Text, string? OriginalText)
{
    /// <summary>The note's unique index on its order, in the form the transaction runner takes (09 10.9 item 3, 10.2): two first writes at once break it.</summary>
    public static IReadOnlyCollection<string> RetryOnUniqueIndexes { get; } = ["ix_internal_notes_order_id"];
}

/// <summary>The address correction's input: the four address fields and, on a shipping address, the telephone (the billing address has none).</summary>
public sealed record CorrectAddressRequest(
    Guid OrderId,
    TransitionAdmin? Admin,
    AddressType AddressType,
    string? RecipientName,
    Guid? ProvinceId,
    string? District,
    string? AddressLine,
    string? Phone,
    AddressRequestConfirmation Confirmation,
    int? ExpectedVersion);

/// <summary>What the firm checked of the buyer before it corrects a guest order's e-mail (02 8.3.6): the name, the shipping telephone (asked only when the order has a physical item), the items and the amount.</summary>
public sealed record GuestIdentityConfirmation(bool Name, bool ShippingPhone, bool Items, bool Amount);

/// <summary>The e-mail correction's input: the new address and the firm's confirmation.</summary>
public sealed record CorrectGuestEmailRequest(Guid OrderId, TransitionAdmin? Admin, string? NewEmail, GuestIdentityConfirmation? Confirmation, int? ExpectedVersion);

/// <summary>
/// The outcome of a correction: applied, or refused with a reason (<see cref="TransitionRefusal"/>; the address rules' refused fields too).
/// <see cref="TransitionRefusal.ConfirmationRequired"/> is the missing confirmation, <see cref="TransitionRefusal.ConditionNotMet"/> a closed operation, <see cref="TransitionRefusal.InvalidInput"/> a value.
/// </summary>
public sealed record OrderCorrectionResult(TransitionRefusal? Refusal, IReadOnlyList<MembershipFieldError> Fields, Guid? CorrectionRecordId) : IOperationResult
{
    public bool IsRejected => Refusal is not null;

    /// <summary>The account the order fell to by the e-mail correction; empty when it stays a guest order.</summary>
    public Guid? LinkedCustomerId { get; init; }

    /// <summary>Why a corrected delivery date was refused (02 10.4.5, 3.20.11); set only on that <see cref="TransitionRefusal.InvalidInput"/> refusal.</summary>
    public DeliveryDateObstacle? DateObstacle { get; init; }

    /// <summary>The corrected delivery date is a withdrawal declaration's day and the request did not say which came first (K-608): the panel asks and sends again.</summary>
    public bool AsksDeliveryOrder { get; init; }
}
