using System.Text.Json;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The PayTR final query (08 2.1.6; 09 11.1 items 3 and 6, 14.2): a pure function from the order number to the request's fields with their <c>paytr_token</c> over three
/// inputs, and a pure reading of the answer into the family's class (<see cref="PaymentQueryResult"/>). Nothing is called, nothing is written, nothing throws for what
/// came in.
/// <list type="bullet">
/// <item><c>status</c> = <c>success</c>: a successful payment exists. <c>payment_amount</c> and <c>payment_total</c> are read with
/// <see cref="PaymentAmounts.TryParseProviderText"/> - a text it does not recognize makes the answer a code error class (<see cref="AmountTextReason"/>) -,
/// <c>odeme_tipi</c>, <c>currency</c> and <c>test_mode</c> are read as marks; whether the payment is applied is the core's rule. The payment's id is the
/// <c>merchant_oid</c> that was asked (the notification's same value).</item>
/// <item><c>status</c> = <c>error</c>: <c>004</c> is "no payment"; <c>001</c>, <c>002</c> and <c>003</c> are the request's errors - no answer, repeated by the code error
/// rule (<see cref="PaymentQueryResult.NoResponse.CodeError"/>); <c>err_msg</c> is not read (it may repeat a field).</item>
/// <item>Any other answer - a status outside 2xx, a body that is not the table's JSON, an unknown <c>status</c> or <c>err_no</c> - is no answer, asked again after five
/// minutes, with a short reason code.</item>
/// </list>
/// The other fields of the answer - the date, the net amount and the deduction, the instalments, the card's brand and masked number, the authorisation, the refunds - are
/// not read (08 2.1.6; 05 4.5.2; K-12): the class has no field for them.
/// </summary>
internal static class PayTrPaymentQuery
{
    /// <summary>The query's address: server to server, form-encoded (08 2.1.6).</summary>
    public static readonly Uri QueryAddress = new("https://www.paytr.com/odeme/durum-sorgu");

    /// <summary>The <c>status</c> of an answer that found a successful payment.</summary>
    public const string SucceededStatus = "success";

    /// <summary>The <c>status</c> of an answer whose <c>err_no</c> says why.</summary>
    public const string ErrorStatus = "error";

    /// <summary>The <c>err_no</c> of "no transaction or successful payment for this <c>merchant_oid</c>" (08 2.1.6): it is an answer, "no payment".</summary>
    public const string NoPaymentCode = "004";

    /// <summary>The <c>odeme_tipi</c> of a card payment; the other value is <c>EFT</c> (08 2.1.6).</summary>
    public const string CardPaymentType = "KART";

    /// <summary>The two spellings of lira the query's <c>currency</c> may carry (08 2.1.6: <c>TL (veya TRY)</c>; the observed one is DV-12's).</summary>
    public static readonly IReadOnlyList<string> LiraCurrencies = ["TL", "TRY"];

    /// <summary>The <c>test_mode</c> of a test transaction (08 2.1.10).</summary>
    public const string TestModeOn = "1";

    /// <summary>The reason code of an amount text <see cref="PaymentAmounts.TryParseProviderText"/> does not recognize (08 2.1.6, DV-12): a code error class.</summary>
    public const string AmountTextReason = "amount-text";

    /// <summary>The reason code of a body that is not the table's JSON or has an unknown <c>status</c> (08 2.1.8 "Tablosuz her yanıt").</summary>
    public const string UnrecognizedBodyReason = "body";

    /// <summary>The request errors of the table (08 2.1.6): <c>001</c> and <c>003</c> an invalid request or <c>paytr_token</c>, <c>002</c> an invalid <c>merchant_oid</c>.</summary>
    public static readonly IReadOnlyList<string> RequestErrorCodes = ["001", "002", "003"];

    /// <summary>The reason code of an <c>err_no</c>: the provider's code, never its message (09 11.1 item 6).</summary>
    public static string ErrorReason(string code) => "err:" + code;

    /// <summary>The message the query's <c>paytr_token</c> is computed over (08 2.1.6): <c>merchant_id</c> + <c>merchant_oid</c> + <c>merchant_salt</c>.</summary>
    public static string SignedMessage(string merchantId, string merchantOid, string merchantSalt) => merchantId + merchantOid + merchantSalt;

    /// <summary>The query's fields for an order number, in the provider's names: <c>merchant_id</c>, <c>merchant_oid</c> (the number without its separator), <c>paytr_token</c>.</summary>
    public static IReadOnlyList<KeyValuePair<string, string>> Build(string orderNumber, PaymentProviderKeys keys)
    {
        ArgumentNullException.ThrowIfNull(keys);
        string merchantOid = OrderNumber.WithoutSeparator(orderNumber);
        string signature = PayTrSignature.Of(keys.MerchantKey, SignedMessage(keys.MerchantId, merchantOid, keys.MerchantSalt));
        return
        [
            new("merchant_id", keys.MerchantId),
            new("merchant_oid", merchantOid),
            new("paytr_token", signature),
        ];
    }

    /// <summary>The answer of a query for <paramref name="merchantOid"/>, as its class (see the class's list). Pure.</summary>
    public static PaymentQueryResult ReadAnswer(int status, string body, string merchantOid)
    {
        if (status is < 200 or > 299)
        {
            return new PaymentQueryResult.NoResponse(PayTrPaymentGateway.HttpReason(status));
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                return new PaymentQueryResult.NoResponse(UnrecognizedBodyReason);
            }

            return Text(root, "status") switch
            {
                SucceededStatus => ReadPaid(root, merchantOid),
                ErrorStatus => ReadError(root),
                _ => new PaymentQueryResult.NoResponse(UnrecognizedBodyReason),
            };
        }
        catch (JsonException)
        {
            return new PaymentQueryResult.NoResponse(UnrecognizedBodyReason);
        }
    }

    private static PaymentQueryResult ReadPaid(JsonElement root, string merchantOid)
    {
        if (PaymentAmounts.TryParseProviderText(Text(root, "payment_amount")) is not { } paymentAmount
            || PaymentAmounts.TryParseProviderText(Text(root, "payment_total")) is not { } paymentTotal)
        {
            return new PaymentQueryResult.NoResponse(AmountTextReason, CodeError: true);
        }

        string? currency = Text(root, "currency");
        return new PaymentQueryResult.Paid(
            merchantOid,
            paymentAmount,
            paymentTotal,
            string.Equals(Text(root, "odeme_tipi"), CardPaymentType, StringComparison.Ordinal),
            currency is not null && LiraCurrencies.Contains(currency, StringComparer.Ordinal),
            string.Equals(Text(root, "test_mode"), TestModeOn, StringComparison.Ordinal));
    }

    private static PaymentQueryResult ReadError(JsonElement root)
    {
        string? code = Text(root, "err_no");
        if (code == NoPaymentCode)
        {
            return new PaymentQueryResult.NoPayment();
        }

        if (code is not null && RequestErrorCodes.Contains(code, StringComparer.Ordinal))
        {
            return new PaymentQueryResult.NoResponse(ErrorReason(code), CodeError: true);
        }

        // A code outside the table is written when it is the provider's short numeric code (DV-12: the unlisted answers are written and the list is completed); any
        // other text is not.
        return new PaymentQueryResult.NoResponse(code is { Length: > 0 and <= 4 } && code.All(char.IsAsciiDigit) ? ErrorReason(code) : UnrecognizedBodyReason);
    }

    // A field as text: a JSON string as it came, a JSON number as its literal text (the provider's documents show quoted values; a number is read the same way).
    private static string? Text(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out JsonElement value))
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
