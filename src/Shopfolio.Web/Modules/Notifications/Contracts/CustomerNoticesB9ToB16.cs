using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>An item a notice names (08 2.2.6 item 4): the product name and, on a variant, the option values, and the quantity the event concerns ("{n} adet").</summary>
public sealed record NoticeItem(string ProductName, string? FirstOptionValue, string? SecondOptionValue, int Quantity);

/// <summary>
/// An address a notice writes as the event's value (08 2.2.6 B-9, B-10; 06 4.25, 4.30, 4.62): the recipient's name, the open address, the district, the province
/// (by its id - the name is read when the body is made) and the telephone. The telephone is written on a return address when it was given and on a delivery
/// address always; a billing address has none.
/// </summary>
public sealed record NoticeAddress(string RecipientName, string AddressLine, string District, Guid ProvinceId, string? Phone);

/// <summary>
/// B-9: a withdrawal declaration, a late-delivery termination or a defect claim was received, or a resolved defect claim was reopened by the customer (also the
/// declarations the administrator records from another channel) - the four headings of 08 2.2.6 B-9. <paramref name="RecordType"/> is the record the declaration is kept
/// in (<see cref="DispatchRelatedRecordType.Withdrawal"/>, <see cref="DispatchRelatedRecordType.LateDeliveryTermination"/> or <see cref="DispatchRelatedRecordType.DefectClaim"/>)
/// and <paramref name="RecordId"/> its id - a value the dispatch row keeps as its related record (06 4.58). <paramref name="IsReopening"/> is the reopening of a resolved
/// claim, an event of the same record that is told apart by the event's values, not by the row's group. A withdrawal and a termination carry the declaration's
/// <paramref name="DeclarationDate"/> (a date, no time is written); a claim and a reopening carry the moment <paramref name="At"/> (date and time).
/// <paramref name="Items"/> are the items and quantities the declaration covers. <paramref name="ReturnAddress"/> is the address the goods are sent to - on a
/// withdrawal and a claim of a physical item, never on a termination.
/// </summary>
public sealed record DeclarationReceivedNotice(
    Guid OrderId,
    DispatchRelatedRecordType RecordType,
    Guid RecordId,
    bool IsReopening,
    DateOnly? DeclarationDate,
    DateTimeOffset? At,
    IReadOnlyList<NoticeItem> Items,
    NoticeAddress? ReturnAddress) : CustomerNotice(OrderId);

/// <summary>B-10: the administrator corrected the delivery or the billing address - the correction back included. The old address is not a value (08 2.2.6 B-10).</summary>
public sealed record AddressCorrectedNotice(Guid OrderId, AddressType AddressType, NoticeAddress NewAddress) : CustomerNotice(OrderId);

/// <summary>B-11: an item was removed or its quantity reduced; <paramref name="Item"/> holds the removed quantity. The notice writes no amount (08 2.2.6 B-11).</summary>
public sealed record ItemRemovedNotice(Guid OrderId, NoticeItem Item) : CustomerNotice(OrderId);

/// <summary>
/// B-12: the carrier or the tracking number was corrected. The values are those of the correction, in the form of <see cref="ShippedNotice"/> (08 2.2.6 B-5, B-12):
/// <paramref name="Carrier"/> is the code of a listed company, <paramref name="OtherCarrierName"/> the name of an unlisted one, <paramref name="IsOwnVehicleDelivery"/>
/// the own-vehicle declaration.
/// </summary>
public sealed record CarrierCorrectedNotice(Guid OrderId, string? Carrier, string? OtherCarrierName, string? TrackingNumber, bool IsOwnVehicleDelivery) : CustomerNotice(OrderId);

/// <summary>
/// B-13: a wrong status transition was corrected; <paramref name="Status"/> is the order's status after the correction. The correction's reason is not a value (08 2.2.6
/// B-13). On the correction of a transfer's "paid" mark the order is "Placed" again and <paramref name="NewPaymentWindowEndsAt"/> is the window end the order has
/// again (the last payment day is the day before it ends); in every other correction it is null.
/// </summary>
public sealed record StatusCorrectedNotice(Guid OrderId, OrderStatus Status, DateTimeOffset? NewPaymentWindowEndsAt) : CustomerNotice(OrderId);

/// <summary>B-14: the customer is asked for an IBAN on the order page; the six events (<see cref="RefundIbanRequestReason"/>) are one notice. The e-mail never carries an IBAN.</summary>
public sealed record RefundIbanRequestedNotice(Guid OrderId, RefundIbanRequestReason Reason) : CustomerNotice(OrderId);

/// <summary>B-15: the order's e-mail address was corrected; this goes to the old address and writes neither the new address nor a name nor a link (08 2.2.6 B-15). <paramref name="ChangedAt"/> is the correction's moment.</summary>
public sealed record OrderEmailChangedNotice(Guid OrderId, DateTimeOffset ChangedAt) : CustomerNotice(OrderId);

/// <summary>B-16: the returned item was rejected because its protective packaging was opened (<paramref name="Reason"/> is <see cref="WithdrawalExemptionReason.HygieneSensitive"/> or <see cref="WithdrawalExemptionReason.SealedMedia"/>).</summary>
public sealed record ReturnRejectedNotice(Guid OrderId, NoticeItem Item, WithdrawalExemptionReason Reason) : CustomerNotice(OrderId);
