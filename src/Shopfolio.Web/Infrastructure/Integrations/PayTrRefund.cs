using System.Text.Json;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The PayTR card refund and its query (08 2.1.9; 09 11.1 items 3 and 6, 14.2): pure functions from a refund to the request's fields with their <c>paytr_token</c> over
/// four inputs, and pure readings of the two answers into the families' classes (<see cref="RefundRequestResult"/>, <see cref="RefundQueryResult"/>). Nothing is
/// called, nothing is written, nothing throws for what came in.
/// <list type="bullet">
/// <item>The request: <c>merchant_id</c>, <c>merchant_oid</c> (the order number without its separator), <c>return_amount</c> (<see cref="PaymentAmounts.ToPlainText"/>:
/// two decimals, a point, no thousands separator, whatever the server's culture - the same text goes into the signature), <c>paytr_token</c> over <c>merchant_id</c>,
/// <c>merchant_oid</c>, <c>return_amount</c> and <c>merchant_salt</c> - the reference does not go into it - and <c>reference_no</c>, the refund record's id without
/// dashes. The reference is the matching key of the query, not a guard against a repeat: the provider has none, the product's guard is its refund record.</item>
/// <item>The request's answer: <c>status</c> = <c>success</c> is accepted; <c>failed</c> or <c>error</c> is the provider's refusal (its <c>err_no</c> is the reason
/// code); any other answer - a status outside 2xx, a body that is not the table's - is an unknown outcome: money may have moved, the request is not sent again.</item>
/// <item>The query: the final query's request (<see cref="PayTrPaymentQuery.Build"/>); an item of <c>returns</c> whose <c>reference_no</c> is the record's reference and
/// whose <c>return_amount</c> (<see cref="PaymentAmounts.TryParseProviderText"/>, compared in kuruş) is the refund's amount is "completed"; everything else -
/// <c>004</c>, no item, an item of another amount - is "no result". The provider never shows a refused refund in the query.</item>
/// </list>
/// The item's field names are the provider page's marketplace example, the one that carries <c>reference_no</c> (08 2.1.9); the store's own names are DV-6's
/// observation. A name the provider does not send makes every answer "no result": the refund stays pending for the firm's mark, it is never taken as completed.
/// </summary>
internal static class PayTrRefund
{
    /// <summary>The refund's address: server to server, form-encoded (08 2.1.9).</summary>
    public static readonly Uri RefundAddress = new("https://www.paytr.com/odeme/iade");

    /// <summary>The <c>status</c> of an accepted refund.</summary>
    public const string SucceededStatus = "success";

    /// <summary>The two <c>status</c> values of a refused refund (08 2.1.9: <c>failed</c> or <c>error</c>, <c>err_no</c> <c>000</c>...<c>011</c>).</summary>
    public static readonly IReadOnlyList<string> RefusedStatuses = ["failed", "error"];

    /// <summary>The reason code of a refusal without a short numeric <c>err_no</c>: the provider refused, its text is not a code.</summary>
    public const string RefusedReason = "refused";

    /// <summary>The reason code of a body that is not the table's JSON or has an unknown <c>status</c> (08 2.1.8 "Tablosuz her yanıt").</summary>
    public const string UnrecognizedBodyReason = "body";

    /// <summary>The reason code of an accepted refund the provider marked as a test on the live installation (08 2.1.9: <c>is_test</c> = 1 is written).</summary>
    public const string TestOnLiveReason = "is-test";

    /// <summary>The <c>is_test</c> of a test refund (08 2.1.9, 2.1.10).</summary>
    public const string TestModeOn = "1";

    /// <summary>The array of the query's answer that carries the order's refunds (08 2.1.9).</summary>
    public const string ReturnsField = "returns";

    /// <summary>The item field that carries the request's reference (08 2.1.9; DV-6).</summary>
    public const string ReferenceField = "reference_no";

    /// <summary>The item field that carries the refunded amount (08 2.1.9; DV-6).</summary>
    public const string AmountField = "return_amount";

    /// <summary>The message the refund's <c>paytr_token</c> is computed over (08 2.1.9): <c>merchant_id</c> + <c>merchant_oid</c> + <c>return_amount</c> + <c>merchant_salt</c>.</summary>
    public static string SignedMessage(string merchantId, string merchantOid, string returnAmount, string merchantSalt) => merchantId + merchantOid + returnAmount + merchantSalt;

    /// <summary>The request's reference: the refund record's id without dashes (08 2.1.9 <c>reference_no</c>: letters and digits, at most 64 characters).</summary>
    public static string ReferenceOf(Guid refundRecordId) => refundRecordId.ToString("N");

    /// <summary>The refund's fields, in the provider's names and order: <c>merchant_id</c>, <c>merchant_oid</c>, <c>return_amount</c>, <c>paytr_token</c>, <c>reference_no</c>.</summary>
    public static IReadOnlyList<KeyValuePair<string, string>> Build(CardRefund refund, PaymentProviderKeys keys)
    {
        ArgumentNullException.ThrowIfNull(refund);
        ArgumentNullException.ThrowIfNull(keys);
        string merchantOid = OrderNumber.WithoutSeparator(refund.OrderNumber);
        string returnAmount = PaymentAmounts.ToPlainText(refund.Amount);
        string signature = PayTrSignature.Of(keys.MerchantKey, SignedMessage(keys.MerchantId, merchantOid, returnAmount, keys.MerchantSalt));
        return
        [
            new("merchant_id", keys.MerchantId),
            new("merchant_oid", merchantOid),
            new("return_amount", returnAmount),
            new("paytr_token", signature),
            new("reference_no", ReferenceOf(refund.RefundRecordId)),
        ];
    }

    /// <summary>
    /// The answer of a refund request as its class (see the class's list). <paramref name="isTest"/> is the answer's <c>is_test</c> mark; <paramref name="failure"/> is the
    /// reason written to the application log - the refusal's code with its message taken through <paramref name="maskMessage"/>, or the unknown outcome's reason; null
    /// for an accepted refund. Pure.
    /// </summary>
    public static RefundRequestResult ReadRequestAnswer(int status, string body, Func<string, string> maskMessage, out bool isTest, out string? failure)
    {
        ArgumentNullException.ThrowIfNull(maskMessage);
        isTest = false;
        failure = null;
        if (status is < 200 or > 299)
        {
            failure = PayTrPaymentGateway.HttpReason(status);
            return new RefundRequestResult.Unknown(failure);
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            JsonElement root = document.RootElement;
            string? outcome = root.ValueKind == JsonValueKind.Object ? Text(root, "status") : null;
            if (outcome == SucceededStatus)
            {
                isTest = Text(root, "is_test") == TestModeOn;
                return new RefundRequestResult.Accepted();
            }

            if (outcome is not null && RefusedStatuses.Contains(outcome, StringComparer.Ordinal))
            {
                string? code = Text(root, "err_no");
                string reason = code is { Length: > 0 and <= 4 } && code.All(char.IsAsciiDigit) ? PayTrPaymentQuery.ErrorReason(code) : RefusedReason;
                string? message = Text(root, "err_msg");
                failure = string.IsNullOrWhiteSpace(message) ? reason : reason + " " + maskMessage(message.Trim());
                return new RefundRequestResult.NotCompleted(reason);
            }

            failure = UnrecognizedBodyReason;
            return new RefundRequestResult.Unknown(UnrecognizedBodyReason);
        }
        catch (JsonException)
        {
            failure = UnrecognizedBodyReason;
            return new RefundRequestResult.Unknown(UnrecognizedBodyReason);
        }
    }

    /// <summary>
    /// The answer of a refund's query as its class: "completed" only for an item of <c>returns</c> with the reference and the amount of <paramref name="refund"/>.
    /// <paramref name="failure"/> is the reason of an answer outside the table (a status outside 2xx, a body that is not the table's, an unknown <c>status</c>), which the
    /// layer writes; null for a classified answer - a successful query, <c>004</c> or another <c>err_no</c> of the table. Pure.
    /// </summary>
    public static RefundQueryResult ReadQueryAnswer(int status, string body, CardRefund refund, out string? failure)
    {
        ArgumentNullException.ThrowIfNull(refund);
        failure = null;
        if (status is < 200 or > 299)
        {
            failure = PayTrPaymentGateway.HttpReason(status);
            return new RefundQueryResult.NoResult();
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            JsonElement root = document.RootElement;
            string? outcome = root.ValueKind == JsonValueKind.Object ? Text(root, "status") : null;
            if (outcome == PayTrPaymentQuery.ErrorStatus)
            {
                // 004 and every other err_no is "no result": the refund may be on its way (08 2.1.9; K-1238 (2)).
                return new RefundQueryResult.NoResult();
            }

            if (outcome != PayTrPaymentQuery.SucceededStatus)
            {
                failure = UnrecognizedBodyReason;
                return new RefundQueryResult.NoResult();
            }

            return HasItem(root, ReferenceOf(refund.RefundRecordId), PaymentAmounts.ToKurus(refund.Amount))
                ? new RefundQueryResult.Completed()
                : new RefundQueryResult.NoResult();
        }
        catch (JsonException)
        {
            failure = UnrecognizedBodyReason;
            return new RefundQueryResult.NoResult();
        }
    }

    private static bool HasItem(JsonElement root, string reference, long amountKurus)
    {
        if (!root.TryGetProperty(ReturnsField, out JsonElement returns) || returns.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (JsonElement item in returns.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object || !string.Equals(Text(item, ReferenceField), reference, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (PaymentAmounts.TryParseProviderText(Text(item, AmountField)) is { } amount && PaymentAmounts.ToKurus(amount) == amountKurus)
            {
                return true;
            }
        }

        return false;
    }

    // A field as text: a JSON string as it came, a JSON number as its literal text (the provider's documents show quoted values; a number is read the same way).
    private static string? Text(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out JsonElement value))
        {
            return null;
        }

        return value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetRawText(),
            _ => null,
        };
    }
}
