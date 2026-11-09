using System.Globalization;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The event's values of the customer notices B-9…B-16 (T307; 08 2.2.6; 06 4.58): the same list shape and the same one writer and reader as B-1…B-8, kept in this part of
/// the class. What the event holds is stored - the heading and the date of a declaration, its items and return address, the new address, the removed item, the corrected
/// carrier data, the status and the new window end, the reason of an IBAN request, the moment of an e-mail change, the rejected item and its reason - and the order's own
/// data is read when the body is made. A notice that breaks the rule of its type is refused when the row is made.
/// </summary>
internal static partial class CustomerNoticeValues
{
    public const string RecordTypeName = "RecordType";
    public const string Reopened = "Reopened";
    public const string DeclarationDate = "DeclarationDate";
    public const string At = "At";
    public const string ItemCount = "ItemCount";
    public const string AddressTypeName = "AddressType";
    public const string StatusName = "Status";
    public const string NewWindowEndsAt = "NewWindowEndsAt";
    public const string IbanReason = "IbanRequestReason";
    public const string ChangedAt = "ChangedAt";
    public const string ExemptionReason = "ExemptionReason";

    private const string ReturnPrefix = "Return";
    private const string NewPrefix = "New";
    private const string DateFormat = "yyyy-MM-dd";

    private static CustomerNoticeRow ForAfterOrder(CustomerNotice notice)
    {
        switch (notice)
        {
            case DeclarationReceivedNotice declaration:
                return Declaration(declaration);
            case AddressCorrectedNotice address:
                RequireKnown(address.AddressType);
                ArgumentNullException.ThrowIfNull(address.NewAddress);
                List<(string Name, string Value)> addressEntries = [(AddressTypeName, address.AddressType.ToString())];
                WriteAddress(addressEntries, NewPrefix, address.NewAddress, phone: address.AddressType == AddressType.ShippingAddress ? Phone.Required : Phone.Absent);
                return new CustomerNoticeRow(DispatchType.B10, null, Write([.. addressEntries]));
            case ItemRemovedNotice removed:
                List<(string Name, string Value)> removedEntries = [];
                WriteItem(removedEntries, 1, removed.Item);
                return new CustomerNoticeRow(DispatchType.B11, null, Write([.. removedEntries]));
            case CarrierCorrectedNotice carrier:
                // The values have the form of B-5's: one listed company or one named company with its number, or the own-vehicle declaration alone (08 2.2.6 B-12).
                return new CustomerNoticeRow(
                    DispatchType.B12,
                    null,
                    Shipped(new ShippedNotice(carrier.OrderId, carrier.Carrier, carrier.OtherCarrierName, carrier.TrackingNumber, carrier.IsOwnVehicleDelivery)));
            case StatusCorrectedNotice status:
                return new CustomerNoticeRow(DispatchType.B13, null, Status(status));
            case RefundIbanRequestedNotice request:
                RequireKnown(request.Reason);
                return new CustomerNoticeRow(DispatchType.B14, null, Write((IbanReason, request.Reason.ToString())));
            case OrderEmailChangedNotice changed:
                return new CustomerNoticeRow(DispatchType.B15, null, Write((ChangedAt, Moment(changed.ChangedAt))));
            case ReturnRejectedNotice rejected:
                if (!WithdrawalExemptions.IsConditional(rejected.Reason))
                {
                    throw new ArgumentException("A return is rejected for an opened protective package: the item is hygiene-sensitive or sealed media (08 2.2.6 B-16).", nameof(notice));
                }

                List<(string Name, string Value)> rejectedEntries = [];
                WriteItem(rejectedEntries, 1, rejected.Item);
                rejectedEntries.Add((ExemptionReason, rejected.Reason.ToString()));
                return new CustomerNoticeRow(DispatchType.B16, null, Write([.. rejectedEntries]));
            default:
                throw new ArgumentException($"{notice.GetType().Name} is not a customer notice of B-1…B-16.", nameof(notice));
        }
    }

    private static CustomerNoticeRow Declaration(DeclarationReceivedNotice declaration)
    {
        if (!IsDeclarationRecord(declaration.RecordType) || (declaration.IsReopening && declaration.RecordType != DispatchRelatedRecordType.DefectClaim))
        {
            throw new ArgumentException("A declaration is kept in a withdrawal, a termination or a defect claim, and only a claim is reopened (08 2.2.6 B-9).", nameof(declaration));
        }

        if (declaration.RecordId == Guid.Empty)
        {
            throw new ArgumentException("A declaration notice names the record it was kept in (06 4.58).", nameof(declaration));
        }

        if (declaration.Items is null || declaration.Items.Count == 0)
        {
            throw new ArgumentException("A declaration notice names the items and quantities it covers (08 2.2.6 B-9).", nameof(declaration));
        }

        bool dated = declaration.RecordType != DispatchRelatedRecordType.DefectClaim;
        if (dated && (declaration.DeclarationDate is null || declaration.At is not null))
        {
            throw new ArgumentException("A withdrawal and a termination carry the declaration's date and no time (08 2.2.6 B-9).", nameof(declaration));
        }

        if (!dated && (declaration.At is null || declaration.DeclarationDate is not null))
        {
            throw new ArgumentException("A defect claim and its reopening carry the date and the time (08 2.2.6 B-9).", nameof(declaration));
        }

        if (declaration.RecordType == DispatchRelatedRecordType.LateDeliveryTermination && declaration.ReturnAddress is not null)
        {
            throw new ArgumentException("A termination has no return address (08 2.2.6 B-9).", nameof(declaration));
        }

        List<(string Name, string Value)> entries = [(RecordTypeName, declaration.RecordType.ToString())];
        if (declaration.IsReopening)
        {
            entries.Add((Reopened, Yes));
        }

        if (declaration.DeclarationDate is { } date)
        {
            entries.Add((DeclarationDate, date.ToString(DateFormat, CultureInfo.InvariantCulture)));
        }

        if (declaration.At is { } at)
        {
            entries.Add((At, Moment(at)));
        }

        entries.Add((ItemCount, declaration.Items.Count.ToString(CultureInfo.InvariantCulture)));
        for (int i = 0; i < declaration.Items.Count; i++)
        {
            WriteItem(entries, i + 1, declaration.Items[i]);
        }

        if (declaration.ReturnAddress is { } address)
        {
            WriteAddress(entries, ReturnPrefix, address, Phone.Optional);
        }

        return new CustomerNoticeRow(DispatchType.B9, null, Write([.. entries]), declaration.RecordType, declaration.RecordId);
    }

    private static bool IsDeclarationRecord(DispatchRelatedRecordType type) =>
        type is DispatchRelatedRecordType.Withdrawal or DispatchRelatedRecordType.LateDeliveryTermination or DispatchRelatedRecordType.DefectClaim;

    private static string Status(StatusCorrectedNotice status)
    {
        RequireKnown(status.Status);
        List<(string Name, string Value)> entries = [(StatusName, status.Status.ToString())];
        if (status.NewPaymentWindowEndsAt is { } end)
        {
            // The correction of a transfer's "paid" mark: the order is awaiting payment again (08 2.2.6 B-13).
            if (status.Status != OrderStatus.Placed)
            {
                throw new ArgumentException("The correction of a transfer's \"paid\" mark leaves the order \"Placed\" (08 2.2.6 B-13).", nameof(status));
            }

            entries.Add((NewWindowEndsAt, Moment(end)));
        }

        return Write([.. entries]);
    }

    private static CustomerNotice? ReadAfterOrder(DispatchType type, Guid orderId, Dictionary<string, string> values, Guid? relatedRecordId)
    {
        switch (type)
        {
            case DispatchType.B9:
                return ReadDeclaration(orderId, values, relatedRecordId);
            case DispatchType.B10:
                if (Enum.TryParse(Get(values, AddressTypeName), out AddressType addressType) && Enum.IsDefined(addressType)
                    && ReadAddress(values, NewPrefix, addressType == AddressType.ShippingAddress ? Phone.Required : Phone.Absent) is { } address)
                {
                    return new AddressCorrectedNotice(orderId, addressType, address);
                }

                return null;
            case DispatchType.B11:
                return ReadItem(values, 1) is { } item ? new ItemRemovedNotice(orderId, item) : null;
            case DispatchType.B12:
                return ReadShipped(orderId, values) is { } shipped
                    ? new CarrierCorrectedNotice(orderId, shipped.Carrier, shipped.OtherCarrierName, shipped.TrackingNumber, shipped.IsOwnVehicleDelivery)
                    : null;
            case DispatchType.B13:
                return ReadStatus(orderId, values);
            case DispatchType.B14:
                return Enum.TryParse(Get(values, IbanReason), out RefundIbanRequestReason reason) && Enum.IsDefined(reason)
                    ? new RefundIbanRequestedNotice(orderId, reason)
                    : null;
            case DispatchType.B15:
                return ReadMoment(Get(values, ChangedAt)) is { } changedAt ? new OrderEmailChangedNotice(orderId, changedAt) : null;
            case DispatchType.B16:
                if (ReadItem(values, 1) is { } rejectedItem
                    && Enum.TryParse(Get(values, ExemptionReason), out WithdrawalExemptionReason exemption)
                    && WithdrawalExemptions.IsConditional(exemption))
                {
                    return new ReturnRejectedNotice(orderId, rejectedItem, exemption);
                }

                return null;
            default:
                return null;
        }
    }

    private static DeclarationReceivedNotice? ReadDeclaration(Guid orderId, Dictionary<string, string> values, Guid? relatedRecordId)
    {
        if (relatedRecordId is not { } recordId || recordId == Guid.Empty
            || !Enum.TryParse(Get(values, RecordTypeName), out DispatchRelatedRecordType recordType) || !IsDeclarationRecord(recordType)
            || !int.TryParse(Get(values, ItemCount), NumberStyles.None, CultureInfo.InvariantCulture, out int count) || count < 1)
        {
            return null;
        }

        List<NoticeItem> items = [];
        for (int i = 1; i <= count; i++)
        {
            if (ReadItem(values, i) is not { } item)
            {
                return null;
            }

            items.Add(item);
        }

        bool reopening = Get(values, Reopened) == Yes;
        if (reopening && recordType != DispatchRelatedRecordType.DefectClaim)
        {
            return null;
        }

        bool dated = recordType != DispatchRelatedRecordType.DefectClaim;
        DateOnly? date = null;
        DateTimeOffset? at = null;
        if (dated)
        {
            if (!DateOnly.TryParseExact(Get(values, DeclarationDate), DateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly parsed))
            {
                return null;
            }

            date = parsed;
        }
        else if (ReadMoment(Get(values, At)) is { } moment)
        {
            at = moment;
        }
        else
        {
            return null;
        }

        NoticeAddress? address = null;
        if (recordType != DispatchRelatedRecordType.LateDeliveryTermination && values.ContainsKey(ReturnPrefix + "RecipientName"))
        {
            address = ReadAddress(values, ReturnPrefix, Phone.Optional);
            if (address is null)
            {
                return null;
            }
        }

        return new DeclarationReceivedNotice(orderId, recordType, recordId, reopening, date, at, items, address);
    }

    private static StatusCorrectedNotice? ReadStatus(Guid orderId, Dictionary<string, string> values)
    {
        if (!Enum.TryParse(Get(values, StatusName), out OrderStatus status) || !Enum.IsDefined(status))
        {
            return null;
        }

        string? text = Get(values, NewWindowEndsAt);
        if (text is null)
        {
            return new StatusCorrectedNotice(orderId, status, null);
        }

        return status == OrderStatus.Placed && ReadMoment(text) is { } end ? new StatusCorrectedNotice(orderId, status, end) : null;
    }

    private static string ItemKey(int number, string field) => $"Item{number.ToString(CultureInfo.InvariantCulture)}{field}";

    private static void WriteItem(List<(string Name, string Value)> entries, int number, NoticeItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        if (string.IsNullOrWhiteSpace(item.ProductName) || item.Quantity < 1)
        {
            throw new ArgumentException("A notice names an item and a quantity of at least one (08 2.2.6 item 4).", nameof(item));
        }

        entries.Add((ItemKey(number, "Name"), item.ProductName));
        if (!string.IsNullOrWhiteSpace(item.FirstOptionValue))
        {
            entries.Add((ItemKey(number, "FirstOption"), item.FirstOptionValue));
        }

        if (!string.IsNullOrWhiteSpace(item.SecondOptionValue))
        {
            entries.Add((ItemKey(number, "SecondOption"), item.SecondOptionValue));
        }

        entries.Add((ItemKey(number, "Quantity"), item.Quantity.ToString(CultureInfo.InvariantCulture)));
    }

    private static NoticeItem? ReadItem(Dictionary<string, string> values, int number)
    {
        if (Get(values, ItemKey(number, "Name")) is not { Length: > 0 } name
            || !int.TryParse(Get(values, ItemKey(number, "Quantity")), NumberStyles.None, CultureInfo.InvariantCulture, out int quantity) || quantity < 1)
        {
            return null;
        }

        return new NoticeItem(name, Get(values, ItemKey(number, "FirstOption")), Get(values, ItemKey(number, "SecondOption")), quantity);
    }

    private enum Phone
    {
        Absent,
        Optional,
        Required,
    }

    private static void WriteAddress(List<(string Name, string Value)> entries, string prefix, NoticeAddress address, Phone phone)
    {
        ArgumentNullException.ThrowIfNull(address);
        if (string.IsNullOrWhiteSpace(address.RecipientName) || string.IsNullOrWhiteSpace(address.AddressLine) || string.IsNullOrWhiteSpace(address.District) || address.ProvinceId == Guid.Empty)
        {
            throw new ArgumentException("An address a notice writes has the recipient's name, the open address, the district and the province (08 2.2.6 B-9, B-10).", nameof(address));
        }

        bool hasPhone = !string.IsNullOrWhiteSpace(address.Phone);
        if ((phone == Phone.Absent && hasPhone) || (phone == Phone.Required && !hasPhone))
        {
            throw new ArgumentException(
                phone == Phone.Absent ? "A billing address has no telephone (08 2.2.6 B-10)." : "A delivery address has its telephone (08 2.2.6 B-10).",
                nameof(address));
        }

        entries.Add((prefix + "RecipientName", address.RecipientName));
        entries.Add((prefix + "AddressLine", address.AddressLine));
        entries.Add((prefix + "District", address.District));
        entries.Add((prefix + "ProvinceId", address.ProvinceId.ToString()));
        if (hasPhone)
        {
            entries.Add((prefix + "Phone", address.Phone!));
        }
    }

    private static NoticeAddress? ReadAddress(Dictionary<string, string> values, string prefix, Phone phone)
    {
        if (Get(values, prefix + "RecipientName") is not { Length: > 0 } name
            || Get(values, prefix + "AddressLine") is not { Length: > 0 } line
            || Get(values, prefix + "District") is not { Length: > 0 } district
            || !Guid.TryParse(Get(values, prefix + "ProvinceId"), out Guid province) || province == Guid.Empty)
        {
            return null;
        }

        string? phoneText = Get(values, prefix + "Phone");
        bool hasPhone = !string.IsNullOrWhiteSpace(phoneText);
        if ((phone == Phone.Absent && hasPhone) || (phone == Phone.Required && !hasPhone))
        {
            return null;
        }

        return new NoticeAddress(name, line, district, province, hasPhone ? phoneText : null);
    }
}
