using System.Globalization;
using System.Text.Json;

using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>What the dispatch row of a customer notice is made of: its type, the first e-mail's row id (B-1 after a correction) and the event's values as the stored list (null when the notice has none).</summary>
internal sealed record CustomerNoticeRow(DispatchType Type, Guid? OriginalDispatchId, string? EventValues);

/// <summary>
/// The event's values of the customer notices B-1…B-8 (08 2.2.6 item 4; 06 4.58 <c>EventValues</c>; 05 4.4.2): a JSON list of name and value pairs - the same shape as
/// the firm notices' and the link e-mails' lists - written when the row is made and read when the body is made. The one writer and the one reader live here.
/// Only the values the event itself holds are stored (the window end, the carrier data, the actor and reason, the item and quantity, the refund's type, amount and
/// method); the order's own data is read from the order when the body is made. A notice that breaks the rule of its type is refused when the row is made, so a
/// row that cannot be written into a body never exists. Moments are round-trip text, amounts are invariant text, enums are their code names.
/// </summary>
internal static class CustomerNoticeValues
{
    public const string WindowEndsAt = "WindowEndsAt";
    public const string Carrier = "Carrier";
    public const string OtherCarrierName = "OtherCarrierName";
    public const string TrackingNumber = "TrackingNumber";
    public const string OwnVehicle = "OwnVehicle";
    public const string Actor = "Actor";
    public const string AdministratorReason = "AdministratorReason";
    public const string ReasonNote = "ReasonNote";
    public const string SystemReason = "SystemReason";
    public const string ItemName = "ItemName";
    public const string FirstOption = "FirstOption";
    public const string SecondOption = "SecondOption";
    public const string Quantity = "Quantity";
    public const string RefundTypeName = "RefundType";
    public const string Amount = "Amount";
    public const string Method = "Method";

    private const string Yes = "true";

    private sealed record Entry(string Name, string Value);

    /// <summary>The row of a notice B-1…B-8; a notice that breaks the rule of its type is refused.</summary>
    public static CustomerNoticeRow For(CustomerNotice notice)
    {
        ArgumentNullException.ThrowIfNull(notice);
        if (notice.OrderId == Guid.Empty)
        {
            throw new ArgumentException("A customer notice belongs to an order.", nameof(notice));
        }

        switch (notice)
        {
            case OrderReceivedNotice received:
                return new CustomerNoticeRow(DispatchType.B1, received.OriginalDispatchId, null);
            case TransferChosenNotice chosen:
                return new CustomerNoticeRow(DispatchType.B2, null, Write((WindowEndsAt, Moment(chosen.PaymentWindowEndsAt))));
            case TransferReminderNotice reminder:
                return new CustomerNoticeRow(DispatchType.B3, null, Write((WindowEndsAt, Moment(reminder.PaymentWindowEndsAt))));
            case PaymentConfirmedNotice:
                return new CustomerNoticeRow(DispatchType.B4, null, null);
            case ShippedNotice shipped:
                return new CustomerNoticeRow(DispatchType.B5, null, Shipped(shipped));
            case DeliveryFailedNotice:
                return new CustomerNoticeRow(DispatchType.B6, null, null);
            case CancelledNotice cancelled:
                return new CustomerNoticeRow(DispatchType.B7, null, Cancelled(cancelled));
            case RefundProcessedNotice refund:
                RequireKnown(refund.RefundType);
                RequireKnown(refund.Method);
                RequireAmount(refund.Amount, nameof(refund.Amount));
                return new CustomerNoticeRow(
                    DispatchType.B8,
                    null,
                    Write((RefundTypeName, refund.RefundType.ToString()), (Amount, Money(refund.Amount)), (Method, refund.Method.ToString())));
            default:
                throw new ArgumentException($"{notice.GetType().Name} is not a customer notice of B-1…B-8.", nameof(notice));
        }
    }

    /// <summary>
    /// The notice a stored row means, or null when the type is not one of B-1…B-8 or the stored list is missing a value its type needs or holds one that is not
    /// of the form the writer makes (the reader is tolerant, 09 12 item 5). The order is the row's order link.
    /// </summary>
    public static CustomerNotice? Read(DispatchType type, Guid orderId, string? eventValues)
    {
        Dictionary<string, string>? values = ReadList(eventValues);
        switch (type)
        {
            case DispatchType.B1:
                return new OrderReceivedNotice(orderId);
            case DispatchType.B4:
                return new PaymentConfirmedNotice(orderId);
            case DispatchType.B6:
                return new DeliveryFailedNotice(orderId);
        }

        if (values is null)
        {
            return null;
        }

        switch (type)
        {
            case DispatchType.B2 when ReadMoment(Get(values, WindowEndsAt)) is { } end:
                return new TransferChosenNotice(orderId, end);
            case DispatchType.B3 when ReadMoment(Get(values, WindowEndsAt)) is { } end:
                return new TransferReminderNotice(orderId, end);
            case DispatchType.B5:
                return ReadShipped(orderId, values);
            case DispatchType.B7:
                return ReadCancelled(orderId, values);
            case DispatchType.B8
                when Enum.TryParse(Get(values, RefundTypeName), out RefundType refundType) && Enum.IsDefined(refundType)
                    && ReadMoney(Get(values, Amount)) is { } amount
                    && Enum.TryParse(Get(values, Method), out PaymentMethod method) && Enum.IsDefined(method):
                return new RefundProcessedNotice(orderId, refundType, amount, method);
            default:
                return null;
        }
    }

    /// <summary>The window end a B-2 or B-3 row holds; null for any other row or a malformed list.</summary>
    public static DateTimeOffset? ReadWindowEnd(string? eventValues) => ReadMoment(Get(ReadList(eventValues), WindowEndsAt));

    /// <summary>A stored moment, or null when the text is not a moment as <see cref="Moment"/> writes it.</summary>
    public static DateTimeOffset? ReadMoment(string? text) =>
        DateTimeOffset.TryParseExact(text, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset moment) ? moment : null;

    private static string Shipped(ShippedNotice shipped)
    {
        if (shipped.IsOwnVehicleDelivery)
        {
            // The declaration stands alone: no company and no number is written (08 2.2.6 B-5), so none is kept.
            if (!string.IsNullOrWhiteSpace(shipped.Carrier) || !string.IsNullOrWhiteSpace(shipped.OtherCarrierName) || !string.IsNullOrWhiteSpace(shipped.TrackingNumber))
            {
                throw new ArgumentException("Own-vehicle delivery carries no company and no tracking number (08 2.2.6 B-5).", nameof(shipped));
            }

            return Write((OwnVehicle, Yes));
        }

        bool listed = !string.IsNullOrWhiteSpace(shipped.Carrier);
        bool other = !string.IsNullOrWhiteSpace(shipped.OtherCarrierName);
        if (listed == other)
        {
            throw new ArgumentException("A shipment names exactly one company: a listed one by its code, or another one by its name (08 2.2.6 B-5).", nameof(shipped));
        }

        List<(string Name, string Value)> entries = [];
        if (listed)
        {
            entries.Add((Carrier, shipped.Carrier!));
        }
        else
        {
            entries.Add((OtherCarrierName, shipped.OtherCarrierName!));
        }

        if (!string.IsNullOrWhiteSpace(shipped.TrackingNumber))
        {
            entries.Add((TrackingNumber, shipped.TrackingNumber));
        }

        return Write([.. entries]);
    }

    private static string Cancelled(CancelledNotice cancelled)
    {
        RequireKnown(cancelled.Actor);
        List<(string Name, string Value)> entries = [(Actor, cancelled.Actor.ToString())];
        switch (cancelled.Actor)
        {
            case ActorKind.Buyer:
                if (cancelled.AdministratorReason is not null || cancelled.SystemReason is not null)
                {
                    throw new ArgumentException("A customer's cancellation carries no reason (08 2.2.6 B-7).", nameof(cancelled));
                }

                break;
            case ActorKind.Admin:
                if (cancelled.AdministratorReason is not { } reason || !Enum.IsDefined(reason) || cancelled.SystemReason is not null)
                {
                    throw new ArgumentException("An administrator's cancellation carries one of the administrator's reasons and none of the system's (08 2.2.6 B-7).", nameof(cancelled));
                }

                entries.Add((AdministratorReason, reason.ToString()));
                if (reason == CancellationReason.Other)
                {
                    if (string.IsNullOrWhiteSpace(cancelled.ReasonNote))
                    {
                        throw new ArgumentException("The reason \"Other\" is written as the administrator's own words (08 2.2.6 B-7).", nameof(cancelled));
                    }

                    entries.Add((ReasonNote, cancelled.ReasonNote));
                }

                break;
            default:
                if (cancelled.SystemReason is not { } system || system == TransitionReason.Closure || !Enum.IsDefined(system) || cancelled.AdministratorReason is not null)
                {
                    throw new ArgumentException("A system cancellation carries one of the four reasons of 08 2.2.6 B-7; Closure makes no notice.", nameof(cancelled));
                }

                if (cancelled.Item is not null)
                {
                    throw new ArgumentException("The system cancels the whole order, never one item (08 2.2.6 B-7).", nameof(cancelled));
                }

                entries.Add((SystemReason, system.ToString()));
                break;
        }

        if (cancelled.Item is { } item)
        {
            if (string.IsNullOrWhiteSpace(item.ProductName) || item.Quantity < 1)
            {
                throw new ArgumentException("An item cancellation names the item and a quantity of at least one (08 2.2.6 B-7).", nameof(cancelled));
            }

            entries.Add((ItemName, item.ProductName));
            if (!string.IsNullOrWhiteSpace(item.FirstOptionValue))
            {
                entries.Add((FirstOption, item.FirstOptionValue));
            }

            if (!string.IsNullOrWhiteSpace(item.SecondOptionValue))
            {
                entries.Add((SecondOption, item.SecondOptionValue));
            }

            entries.Add((Quantity, item.Quantity.ToString(CultureInfo.InvariantCulture)));
        }

        return Write([.. entries]);
    }

    private static ShippedNotice? ReadShipped(Guid orderId, Dictionary<string, string> values)
    {
        if (Get(values, OwnVehicle) == Yes)
        {
            return new ShippedNotice(orderId, null, null, null, true);
        }

        string? carrier = Get(values, Carrier);
        string? other = Get(values, OtherCarrierName);
        if (string.IsNullOrWhiteSpace(carrier) == string.IsNullOrWhiteSpace(other))
        {
            return null;
        }

        return new ShippedNotice(orderId, carrier, other, Get(values, TrackingNumber), false);
    }

    private static CancelledNotice? ReadCancelled(Guid orderId, Dictionary<string, string> values)
    {
        if (!Enum.TryParse(Get(values, Actor), out ActorKind actor) || !Enum.IsDefined(actor))
        {
            return null;
        }

        CancelledItem? item = null;
        if (Get(values, ItemName) is { Length: > 0 } name)
        {
            if (!int.TryParse(Get(values, Quantity), NumberStyles.None, CultureInfo.InvariantCulture, out int quantity) || quantity < 1)
            {
                return null;
            }

            item = new CancelledItem(name, Get(values, FirstOption), Get(values, SecondOption), quantity);
        }

        switch (actor)
        {
            case ActorKind.Buyer:
                return new CancelledNotice(orderId, actor, null, null, null, item);
            case ActorKind.Admin:
                if (!Enum.TryParse(Get(values, AdministratorReason), out CancellationReason reason) || !Enum.IsDefined(reason))
                {
                    return null;
                }

                return new CancelledNotice(orderId, actor, reason, Get(values, ReasonNote), null, item);
            default:
                if (item is not null || !Enum.TryParse(Get(values, SystemReason), out TransitionReason system) || !Enum.IsDefined(system) || system == TransitionReason.Closure)
                {
                    return null;
                }

                return new CancelledNotice(orderId, actor, null, null, system, null);
        }
    }

    private static string? Get(Dictionary<string, string>? values, string name) =>
        values is not null && values.TryGetValue(name, out string? value) ? value : null;

    private static Dictionary<string, string>? ReadList(string? eventValues)
    {
        if (string.IsNullOrWhiteSpace(eventValues))
        {
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(eventValues);
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return null;
            }

            Dictionary<string, string> values = new(StringComparer.Ordinal);
            foreach (JsonElement entry in document.RootElement.EnumerateArray())
            {
                if (entry.ValueKind != JsonValueKind.Object
                    || !entry.TryGetProperty(nameof(Entry.Name), out JsonElement name) || name.ValueKind != JsonValueKind.String
                    || !entry.TryGetProperty(nameof(Entry.Value), out JsonElement value) || value.ValueKind != JsonValueKind.String
                    || name.GetString() is not { } key || value.GetString() is not { } text)
                {
                    return null;
                }

                values[key] = text;
            }

            return values;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static decimal? ReadMoney(string? text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) && amount > 0m && amount * 100m % 1 == 0 ? amount : null;

    private static string Write(params (string Name, string Value)[] entries) =>
        JsonSerializer.Serialize<Entry[]>([.. entries.Select(e => new Entry(e.Name, e.Value))]);

    private static string Moment(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);

    private static string Money(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static void RequireAmount(decimal value, string name)
    {
        // An amount is kuruş precision (06 2.5.1; the display never rounds, DisplayFormat.Amount): a finer value is refused here, not when the body is made.
        if (value <= 0m || value * 100m % 1 != 0)
        {
            throw new ArgumentOutOfRangeException(name, value, "A refund amount is positive and has at most two decimals.");
        }
    }

    private static void RequireKnown<TEnum>(TEnum value)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "Unknown value.");
        }
    }
}
