using System.Globalization;
using System.Text.Json;

using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Notifications;

/// <summary>What the dispatch row of a firm notice is made of: its type, the related record (06 4.58 table) and the event's values as the stored list.</summary>
internal sealed record CompanyNoticeRow(DispatchType Type, DispatchRelatedRecordType? RelatedRecordType, Guid? RelatedRecordId, string EventValues);

/// <summary>
/// The event's values of the firm notices F-1…F-6 (08 2.2.7 "Değerlerin saklanışı"; 06 4.58 <c>EventValues</c>; 05 4.4.1): a JSON list of name and value pairs
/// - the same shape as the link e-mails' value list (<see cref="LinkEventValues"/>) - written when the row is made and read when the body is made, so the body
/// does not depend on a record that may be deleted by then. The one writer and the one reader live here. Nothing of a customer's is ever written (the notice
/// records have no such member), and the F-5 list holds the last four digits of the IBAN and nothing more. Moments are round-trip text, amounts and numbers
/// are invariant text, enums are their code names.
/// </summary>
internal static class CompanyNoticeValues
{
    public const string OccurredAt = "OccurredAt";
    public const string InnerKey = "InnerKey";
    public const string OrderNumber = "OrderNumber";
    public const string PaymentMethodName = "PaymentMethod";
    public const string Total = "Total";
    public const string Topic = "Topic";
    public const string Kind = "Kind";
    public const string Reopened = "Reopened";
    public const string RefundTypeName = "RefundType";
    public const string Amount = "Amount";
    public const string ActorName = "ActorName";
    public const string LastFourDigits = "LastFourDigits";
    public const string FirstEntry = "FirstEntry";
    public const string InvitedEmail = "InvitedEmail";
    public const string RemovedName = "RemovedName";

    private const string Yes = "true";
    private const string No = "false";

    private sealed record Entry(string Name, string Value);

    /// <summary>The row of an F-1…F-4 notice; a notice that breaks the rule of its type is refused.</summary>
    public static CompanyNoticeRow ForCompany(CompanyNotice notice)
    {
        ArgumentNullException.ThrowIfNull(notice);
        switch (notice)
        {
            case NewOrderNotice order:
                RequireText(order.OrderNumber, nameof(order.OrderNumber));
                RequireKnown(order.PaymentMethod);
                RequireAmount(order.Total, nameof(order.Total));
                return new CompanyNoticeRow(
                    DispatchType.F1,
                    DispatchRelatedRecordType.Order,
                    order.OrderId,
                    Write(
                        (OccurredAt, Moment(order.OccurredAt)),
                        (InnerKey, Key(order.OrderId)),
                        (OrderNumber, order.OrderNumber),
                        (PaymentMethodName, order.PaymentMethod.ToString()),
                        (Total, Money(order.Total))));
            case NewContactRequestNotice request:
                RequireKnown(request.Topic);
                return new CompanyNoticeRow(
                    DispatchType.F2,
                    DispatchRelatedRecordType.ContactRequest,
                    request.ContactRequestId,
                    Write(
                        (OccurredAt, Moment(request.OccurredAt)),
                        (InnerKey, Key(request.ContactRequestId)),
                        (Topic, request.Topic.ToString())));
            case NewClaimNotice claim:
                RequireText(claim.OrderNumber, nameof(claim.OrderNumber));
                if (claim.Kind is not (DispatchRelatedRecordType.Withdrawal or DispatchRelatedRecordType.LateDeliveryTermination or DispatchRelatedRecordType.DefectClaim))
                {
                    throw new ArgumentException("F-3 is for a withdrawal, a termination for late delivery or a defect claim (08 2.2.7).", nameof(notice));
                }

                if (claim.Reopened && claim.Kind != DispatchRelatedRecordType.DefectClaim)
                {
                    throw new ArgumentException("Only a defect claim is reopened (08 2.2.7 F-3).", nameof(notice));
                }

                return new CompanyNoticeRow(
                    DispatchType.F3,
                    claim.Kind,
                    claim.RecordId,
                    Write(
                        (OccurredAt, Moment(claim.OccurredAt)),
                        (InnerKey, Key(claim.OrderId)),
                        (OrderNumber, claim.OrderNumber),
                        (Kind, claim.Kind.ToString()),
                        (Reopened, claim.Reopened ? Yes : No)));
            case CardRefundNotice refund:
                RequireText(refund.OrderNumber, nameof(refund.OrderNumber));
                RequireAmount(refund.Amount, nameof(refund.Amount));
                if (refund.RefundType is not (RefundType.Cancellation or RefundType.ItemRemoval or RefundType.LateDeliveryTermination or RefundType.LatePayment))
                {
                    throw new ArgumentException("F-4 is for the refunds the system makes by itself (08 2.2.7).", nameof(notice));
                }

                return new CompanyNoticeRow(
                    DispatchType.F4,
                    DispatchRelatedRecordType.Refund,
                    refund.RefundId,
                    Write(
                        (OccurredAt, Moment(refund.OccurredAt)),
                        (InnerKey, Key(refund.OrderId)),
                        (OrderNumber, refund.OrderNumber),
                        (RefundTypeName, refund.RefundType.ToString()),
                        (Amount, Money(refund.Amount))));
            default:
                throw new ArgumentException($"{notice.GetType().Name} is not a notice of the firm's contact e-mail.", nameof(notice));
        }
    }

    /// <summary>The row of an F-5 or F-6 notice (one for every address); a notice that breaks the rule of its type is refused.</summary>
    public static CompanyNoticeRow ForAdmins(AdminNotice notice)
    {
        ArgumentNullException.ThrowIfNull(notice);
        switch (notice)
        {
            case IbanChangedNotice iban:
                // The constructor holds the same rule; a derived or deserialised record is checked once more, so no whole IBAN is ever written.
                if (iban.LastFourDigits.Length != 4 || !iban.LastFourDigits.All(char.IsAsciiDigit))
                {
                    throw new ArgumentException("F-5 carries the last four digits of the IBAN and nothing more (08 2.2.7).", nameof(notice));
                }

                RequireText(iban.ActorName, nameof(iban.ActorName));
                return new CompanyNoticeRow(
                    DispatchType.F5,
                    null,
                    null,
                    Write(
                        (OccurredAt, Moment(iban.OccurredAt)),
                        (ActorName, iban.ActorName),
                        (LastFourDigits, iban.LastFourDigits),
                        (FirstEntry, iban.WasFirstEntry ? Yes : No)));
            case AdminInvitedNotice invited:
                RequireText(invited.ActorName, nameof(invited.ActorName));
                RequireText(invited.InvitedEmail, nameof(invited.InvitedEmail));
                return new CompanyNoticeRow(
                    DispatchType.F6,
                    DispatchRelatedRecordType.AdminInvitation,
                    invited.InvitationId,
                    Write(
                        (OccurredAt, Moment(invited.OccurredAt)),
                        (Kind, nameof(AdminInvitedNotice)),
                        (ActorName, invited.ActorName),
                        (InvitedEmail, invited.InvitedEmail)));
            case AdminRemovedNotice removed:
                RequireText(removed.ActorName, nameof(removed.ActorName));
                RequireText(removed.RemovedName, nameof(removed.RemovedName));
                return new CompanyNoticeRow(
                    DispatchType.F6,
                    DispatchRelatedRecordType.Admin,
                    removed.RemovedAdminId,
                    Write(
                        (OccurredAt, Moment(removed.OccurredAt)),
                        (Kind, nameof(AdminRemovedNotice)),
                        (ActorName, removed.ActorName),
                        (RemovedName, removed.RemovedName)));
            default:
                throw new ArgumentException($"{notice.GetType().Name} is not a notice of the administrators' own addresses.", nameof(notice));
        }
    }

    /// <summary>The value list as a map; null when the list is empty, malformed or not a list of name and value text pairs (the reader is tolerant, 09 12 item 5).</summary>
    public static IReadOnlyDictionary<string, string>? Read(string? eventValues)
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

    /// <summary>A stored moment, or null when the text is not a moment as <see cref="Moment"/> writes it.</summary>
    public static DateTimeOffset? ReadMoment(string? text) =>
        DateTimeOffset.TryParseExact(text, "O", CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out DateTimeOffset moment) ? moment : null;

    /// <summary>A stored amount, or null when the text is not an amount as <see cref="Money"/> writes it.</summary>
    public static decimal? ReadMoney(string? text) =>
        decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out decimal amount) && amount >= 0m ? amount : null;

    private static string Write(params (string Name, string Value)[] entries) =>
        JsonSerializer.Serialize<Entry[]>([.. entries.Select(e => new Entry(e.Name, e.Value))]);

    private static string Moment(DateTimeOffset value) => value.ToString("O", CultureInfo.InvariantCulture);

    private static string Money(decimal value) => value.ToString(CultureInfo.InvariantCulture);

    private static string Key(Guid id) => id.ToString("D");

    private static void RequireText(string? value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("A value of the notice is empty (08 2.2.7).", name);
        }
    }

    private static void RequireAmount(decimal value, string name)
    {
        // An amount is kuruş precision (06 2.5.1; the display never rounds, DisplayFormat.Amount): a finer value is refused here, not when the body is made.
        if (value < 0m || value * 100m % 1 != 0)
        {
            throw new ArgumentOutOfRangeException(name, value, "An amount is not negative and has at most two decimals.");
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
