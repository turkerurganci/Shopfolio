using System.Globalization;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The provider's payment notification (08 2.1.5; 09 11.1 items 3, 5 and 6, 14.2): the signature, the field mapping and the two readings the core asks of the layer. Pure:
/// nothing is called, nothing is written, nothing throws for what came in - every input is a class (<see cref="NotificationVerification"/>,
/// <see cref="NotificationResult"/>, <see cref="NotificationAmountCheck"/>), and the answer to the provider (<c>OK</c>, 400, 500) is the endpoint's (T311; 05 MK-39).
/// <list type="bullet">
/// <item>The signature: <c>merchant_oid</c> + <c>merchant_salt</c> + <c>status</c> + <c>total_amount</c>, HMAC-SHA256 with <c>merchant_key</c>, Base64
/// (<see cref="PayTrSignature"/>), compared with <c>hash</c> in fixed time (<see cref="SignatureComparison.Matches"/>). A missing <c>hash</c>, a missing signed field or
/// missing keys is <see cref="NotificationVerification.SignatureInvalid"/>: no signature can be computed over a field that is not there.</item>
/// <item><c>payment_amount</c>, <c>payment_type</c>, <c>currency</c> and <c>test_mode</c> are not signed: they are read only for the consistency checks of the
/// classification (rows 4 and 6), never as a guarantee.</item>
/// <item>The mapping: <c>merchant_oid</c> as it came is the transaction id and the order reference, and maps back to the order number (<see cref="Security.OrderNumber"/>);
/// <c>status</c> is the outcome as it came; <c>total_amount</c> is the amount in whole kuruş (3456 → 34.56); the event type is <see cref="SaleEventType"/>, the only
/// one (the provider sends no refund notification). <c>failed_reason_code</c> and <c>failed_reason_msg</c> are not mapped: the gateway writes them, thinned, to the
/// application log (<see cref="PaymentFailureReasonLog"/>).</item>
/// </list>
/// </summary>
internal static class PayTrNotification
{
    public const string OrderReferenceField = "merchant_oid";

    public const string StatusField = "status";

    public const string TotalAmountField = "total_amount";

    public const string HashField = "hash";

    public const string PaymentAmountField = "payment_amount";

    public const string PaymentTypeField = "payment_type";

    public const string CurrencyField = "currency";

    public const string TestModeField = "test_mode";

    public const string FailedReasonCodeField = "failed_reason_code";

    public const string FailedReasonMessageField = "failed_reason_msg";

    /// <summary>The one event type (08 2.1.5 "Olay türü ve sonuç değerleri"): the provider's notification is a sale's; there is no refund notification.</summary>
    public const string SaleEventType = "Sale";

    /// <summary>The <c>status</c> of a payment that succeeded.</summary>
    public const string SucceededStatus = "success";

    /// <summary>The <c>status</c> of a payment that failed.</summary>
    public const string FailedStatus = "failed";

    /// <summary>The <c>payment_type</c> of a card payment; any other (<c>eft</c>) is an unrecognized event (08 2.1.3 item 2).</summary>
    public const string CardPaymentType = "card";

    /// <summary>The <c>test_mode</c> of a test transaction (08 2.1.10).</summary>
    public const string TestModeOn = "1";

    /// <summary>The message the notification's signature is computed over (08 2.1.5): the four signed fields and the salt, in the provider's order.</summary>
    public static string SignedMessage(string merchantOid, string merchantSalt, string status, string totalAmount) =>
        merchantOid + merchantSalt + status + totalAmount;

    /// <summary>The verification of a notification's form fields with the one setting of keys; null keys verify nothing (08 2.1.2).</summary>
    public static NotificationVerification Verify(IReadOnlyDictionary<string, string> fields, PaymentProviderKeys? keys)
    {
        ArgumentNullException.ThrowIfNull(fields);
        if (keys is null
            || !TryField(fields, OrderReferenceField, out string merchantOid)
            || !TryField(fields, StatusField, out string status)
            || !TryField(fields, TotalAmountField, out string totalAmount))
        {
            return new NotificationVerification.SignatureInvalid();
        }

        string computed = PayTrSignature.Of(keys.MerchantKey, SignedMessage(merchantOid, keys.MerchantSalt, status, totalAmount));
        if (!SignatureComparison.Matches(computed, fields.GetValueOrDefault(HashField)))
        {
            return new NotificationVerification.SignatureInvalid();
        }

        return new NotificationVerification.SignatureValid(new PaymentNotice(
            merchantOid,
            Security.OrderNumber.FromWithoutSeparator(merchantOid),
            SaleEventType,
            status,
            Kurus(totalAmount),
            Kurus(fields.GetValueOrDefault(PaymentAmountField)),
            FieldIs(fields, PaymentTypeField, CardPaymentType),
            FieldIs(fields, CurrencyField, PayTrStartForm.Currency),
            FieldIs(fields, TestModeField, TestModeOn)));
    }

    /// <summary>
    /// What a recorded event type and outcome mean (08 2.1.5): <see cref="SaleEventType"/> with <see cref="SucceededStatus"/> or <see cref="FailedStatus"/>, as they
    /// came (ordinal, no case folding); every other pair is <see cref="NotificationResult.Unrecognized"/>.
    /// </summary>
    public static NotificationResult ReadResult(string eventType, string outcome) =>
        (eventType, outcome) switch
        {
            (SaleEventType, SucceededStatus) => NotificationResult.PaymentSucceeded,
            (SaleEventType, FailedStatus) => NotificationResult.PaymentFailed,
            _ => NotificationResult.Unrecognized,
        };

    /// <summary>
    /// The amount condition of a successful payment (08 2.1.5; 09 14.2 item 3): <see cref="PaymentAmounts.NotificationMatches"/> over the two amounts in whole kuruş. An
    /// amount that could not be read does not hold (09 14.2 item 3: "tutar çözülemeyen bildirim de tutar koşulu tutmuyor sayılır").
    /// </summary>
    public static NotificationAmountCheck CheckAmount(PaymentNotice notice, decimal orderTotal)
    {
        ArgumentNullException.ThrowIfNull(notice);
        if (notice.PaymentAmount is not { } started || notice.TotalAmount is not { } collected || started < 0m || collected < 0m)
        {
            return NotificationAmountCheck.Mismatch;
        }

        return PaymentAmounts.NotificationMatches(PaymentAmounts.ToKurus(started), PaymentAmounts.ToKurus(collected), orderTotal)
            ? NotificationAmountCheck.Holds
            : NotificationAmountCheck.Mismatch;
    }

    // An amount field is whole kuruş, digits only (08 2.1.5: 3456 → 34.56); anything else - empty, a sign, a separator, too long for a long - is not read.
    private static decimal? Kurus(string? text) =>
        long.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out long kurus) ? PaymentAmounts.FromKurus(kurus) : null;

    // An unsigned mark (08 2.1.5): the field as it came, ordinal, no case folding.
    private static bool FieldIs(IReadOnlyDictionary<string, string> fields, string name, string expected) =>
        string.Equals(fields.GetValueOrDefault(name), expected, StringComparison.Ordinal);

    private static bool TryField(IReadOnlyDictionary<string, string> fields, string name, out string value)
    {
        value = fields.GetValueOrDefault(name) ?? string.Empty;
        return value.Length > 0;
    }
}
