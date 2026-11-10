using System.Text.Json;

using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.SearchEngine;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The payment provider's implementation of the link layer: PayTR's iFrame API (08 2.1; 09 11.1 item 1; KD-64 - the one implementation). T401 builds the start (08 2.1.4):
/// the token request goes server to server, form-encoded, with the keys of the one setting (<see cref="PaymentProviderKeys"/>); without the setting no call is made. A
/// successful answer gives the frame's address and the token is not kept - neither in a log nor in the database (the order has no field for it). T402 builds the
/// notification (08 2.1.5; <see cref="PayTrNotification"/>): the signature with the same keys, the field mapping, the reading of the outcome and the amount check, all
/// local; a failed payment's reason goes to the application log thinned (<see cref="PaymentFailureReasonLog"/>). T403 builds the final query (08 2.1.6;
/// <see cref="PayTrPaymentQuery"/>). T404 builds the start's table (08 2.1.8; <see cref="ReadAnswer"/>): the connection error with the provider's <c>reason</c>
/// written without the request's values, and the second token refusal; the layer still makes each call once and retries none. The refund and its query (T405) are the later provider task; until it comes those jobs refuse with
/// <see cref="NotSupportedException"/>: the refund jobs are not registered yet.
/// </summary>
internal sealed class PayTrPaymentGateway : IPaymentGateway
{
    /// <summary>The reason code of a start without the provider's keys (08 2.1.2): no call is made.</summary>
    public const string KeysMissingReason = "keys-missing";

    /// <summary>The reason code of a client address the provider's form cannot carry (08 2.1.4 <c>user_ip</c>): a code error, no call is made.</summary>
    public const string ClientAddressReason = "client-address";

    /// <summary>
    /// The prefix of a <c>failed</c> answer's reason code (08 2.1.8, second row; 09 11.1 item 6): the code is <c>failed:</c> and the provider's <c>reason</c> text, with
    /// every value the request carried taken out of it (<see cref="FieldValueMask"/>) - the reason is written, a field's value is not.
    /// </summary>
    public const string FailedReason = "failed";

    /// <summary>The reason code of an answer whose body is not the table's JSON (08 2.1.8: a connection error).</summary>
    public const string UnrecognizedBodyReason = "body";

    /// <summary>The reason code of the second token refusal (08 2.1.8, third row; 2.1.7 (1)): not a connection error.</summary>
    public const string SecondTokenReason = "second-token";

    /// <summary>What stands in a written <c>reason</c> for a value the request carried (08 2.1.8: "alan değerleri yazılmaz").</summary>
    public const string FieldValueMask = "[field]";

    /// <summary>
    /// The provider's <c>reason</c> texts that tell the second token refusal apart (08 2.1.8, third row; DV-11): the text is observed on the provider's test store and
    /// written here, in 08 2.1.8's table and in the runbook (PAYTR.md 6.1). Until then the list is empty and a <c>failed</c> answer is a connection error - the table's
    /// rule for a <c>failed</c> answer that cannot be told apart (D-1075).
    /// </summary>
    public static readonly IReadOnlyList<string> SecondTokenReasonTexts = [];

    // A request value this short (a flag, "TL", the installment count) is also an ordinary word or number of a reason text; it is not a personal value or a key.
    private const int MaskedValueMinLength = 3;

    private readonly IHttpClientFactory _clients;
    private readonly ExternalCallRunner _runner;
    private readonly PaymentProviderKeys? _keys;
    private readonly SiteAddress _site;
    private readonly TimeProvider _time;
    private readonly bool _testMode;
    private readonly PaymentFailureReasonLog _failureReasons;

    public PayTrPaymentGateway(
        IHttpClientFactory clients,
        ExternalCallRunner runner,
        IOptions<PaymentOptions> payment,
        SiteAddress site,
        TimeProvider time,
        IHostEnvironment environment,
        PaymentFailureReasonLog failureReasons)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(environment);
        _clients = clients;
        _runner = runner;
        _keys = PaymentProviderKeys.TryParse(payment.Value.ProviderKeys);
        _site = site;
        _time = time;
        _failureReasons = failureReasons;

        // test_mode (08 2.1.10): 0 only on the live installation - the same sense of "live" the notification's test mark is checked against (T311).
        _testMode = !environment.IsProduction();
    }

    /// <summary>The reason code of an HTTP status outside 2xx (08 2.1.8): its number only.</summary>
    public static string HttpReason(int status) => "http:" + status.ToString(System.Globalization.CultureInfo.InvariantCulture);

    public async Task<PaymentStartResult> StartAsync(PaymentStartRequest request, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (_keys is null)
        {
            return _runner.Failed<PaymentStartResult>(IntegrationOperation.PaymentStart, ExternalCallFailure.Unrecognized(KeysMissingReason));
        }

        if (!PayTrStartForm.TryClientAddress(request.ClientIpAddress, out string clientAddress))
        {
            return _runner.Failed<PaymentStartResult>(IntegrationOperation.PaymentStart, ExternalCallFailure.Unrecognized(ClientAddressReason));
        }

        string returnAddress = _site.Absolute(PayTrStartForm.ReturnPath(request.OrderAccessKey));
        IReadOnlyList<KeyValuePair<string, string>> fields = PayTrStartForm.Build(request, _keys, clientAddress, returnAddress, _time.GetUtcNow(), _testMode);

        return await _runner.RunAsync(IntegrationOperation.PaymentStart, token => PostAsync(fields, token), ct);
    }

    /// <summary>
    /// The final query (08 2.1.6; <see cref="PayTrPaymentQuery"/>): one form-encoded request within the job limit (30 seconds), never repeated by the layer. Without the
    /// keys no call is made and the result is a code error class (<see cref="KeysMissingReason"/>); an answer read as "no answer" writes the layer's one failure line
    /// with its reason code - the timeout and the unreachable provider are the runner's.
    /// </summary>
    public async Task<PaymentQueryResult> QueryPaymentAsync(string orderNumber, CancellationToken ct)
    {
        if (_keys is null)
        {
            _runner.WriteFailure(IntegrationOperation.PaymentQuery, ExternalCallFailure.Unrecognized(KeysMissingReason));
            return new PaymentQueryResult.NoResponse(KeysMissingReason, CodeError: true);
        }

        IReadOnlyList<KeyValuePair<string, string>> fields = PayTrPaymentQuery.Build(orderNumber, _keys);
        string merchantOid = Security.OrderNumber.WithoutSeparator(orderNumber);
        return await _runner.RunAsync(IntegrationOperation.PaymentQuery, token => PostQueryAsync(fields, merchantOid, token), ct);
    }

    public Task<RefundRequestResult> RequestRefundAsync(CardRefund refund, CancellationToken ct) =>
        throw new NotSupportedException("The card refund is built by T405.");

    public Task<RefundQueryResult> QueryRefundAsync(CardRefund refund, CancellationToken ct) =>
        throw new NotSupportedException("The card refund's query is built by T405.");

    /// <summary>
    /// The notification's signature and field mapping (08 2.1.5). A verified notification of a failed payment writes its reason, thinned, to the application log
    /// (<c>failed_reason_code</c>, <c>failed_reason_msg</c>: not kept anywhere else); a request whose signature does not hold writes nothing here - its thinned
    /// security line is the signed endpoint pipeline's (09 11.1 item 5).
    /// </summary>
    public NotificationVerification VerifyNotification(IReadOnlyDictionary<string, string> formFields)
    {
        ArgumentNullException.ThrowIfNull(formFields);
        NotificationVerification verification = PayTrNotification.Verify(formFields, _keys);
        if (verification is NotificationVerification.SignatureValid { Notice.Outcome: PayTrNotification.FailedStatus })
        {
            _failureReasons.Record(
                formFields.GetValueOrDefault(PayTrNotification.FailedReasonCodeField),
                formFields.GetValueOrDefault(PayTrNotification.FailedReasonMessageField));
        }

        return verification;
    }

    public NotificationResult ReadNotificationResult(string eventType, string outcome) => PayTrNotification.ReadResult(eventType, outcome);

    public NotificationAmountCheck CheckNotificationAmount(PaymentNotice notice, decimal orderTotal) => PayTrNotification.CheckAmount(notice, orderTotal);

    /// <summary>
    /// The answer of a token request, the start's table (08 2.1.8; 2.1.4): <c>status</c> = <c>success</c> with a <c>token</c> is the frame's address; a <c>failed</c>
    /// answer whose <c>reason</c> is one of <paramref name="secondTokenReasonTexts"/> is the second token refusal; every other answer - any other <c>failed</c>, a status
    /// outside 2xx, a body that is not the table's - is a connection error (the not-reached provider and the 15 second limit are the runner's). A <c>failed</c> answer's
    /// failure carries the provider's <c>reason</c> with the values of <paramref name="sentFields"/> taken out (09 11.1 item 6). Pure: no field of the answer other than
    /// <c>status</c>, <c>token</c> and <c>reason</c> is read.
    /// </summary>
    public static PaymentStartResult? ReadAnswer(
        int status,
        string body,
        IReadOnlyList<KeyValuePair<string, string>> sentFields,
        IReadOnlyList<string> secondTokenReasonTexts,
        out ExternalCallFailure? failure)
    {
        ArgumentNullException.ThrowIfNull(sentFields);
        ArgumentNullException.ThrowIfNull(secondTokenReasonTexts);
        failure = null;
        if (status is < 200 or > 299)
        {
            failure = ExternalCallFailure.Unrecognized(HttpReason(status));
            return null;
        }

        try
        {
            using JsonDocument document = JsonDocument.Parse(body);
            JsonElement root = document.RootElement;
            string? outcome = StringField(root, "status");
            if (outcome == "success" && StringField(root, "token") is { Length: > 0 } token)
            {
                return new PaymentStartResult.Started(new Uri(PayTrStartForm.FrameAddressRoot + Uri.EscapeDataString(token)));
            }

            if (outcome == "failed")
            {
                string reason = StringField(root, "reason")?.Trim() ?? string.Empty;
                if (secondTokenReasonTexts.Any(text => string.Equals(text, reason, StringComparison.OrdinalIgnoreCase)))
                {
                    return new PaymentStartResult.SecondTokenRejected(SecondTokenReason);
                }

                failure = ExternalCallFailure.Unrecognized(FailedReason + ":" + WithoutFieldValues(reason, sentFields));
                return null;
            }

            failure = ExternalCallFailure.Unrecognized(UnrecognizedBodyReason);
            return null;
        }
        catch (JsonException)
        {
            failure = ExternalCallFailure.Unrecognized(UnrecognizedBodyReason);
            return null;
        }
    }

    /// <summary>
    /// The provider's <c>reason</c> as it is written (08 2.1.8: "<c>reason</c> yazılır, alan değerleri yazılmaz"): every value the request carried - the longest first,
    /// so a value inside another goes with it - is replaced by <see cref="FieldValueMask"/>, and a line break or other control character becomes a space so the reason
    /// stays on its one log line.
    /// </summary>
    public static string WithoutFieldValues(string reason, IReadOnlyList<KeyValuePair<string, string>> sentFields)
    {
        ArgumentNullException.ThrowIfNull(reason);
        ArgumentNullException.ThrowIfNull(sentFields);
        string text = reason;
        foreach (string value in sentFields.Select(f => f.Value).Where(v => v.Length >= MaskedValueMinLength).Distinct(StringComparer.Ordinal).OrderByDescending(v => v.Length))
        {
            text = text.Replace(value, FieldValueMask, StringComparison.OrdinalIgnoreCase);
        }

        return string.Concat(text.Select(c => char.IsControl(c) ? ' ' : c));
    }

    private static string? StringField(JsonElement root, string name) =>
        root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out JsonElement value) && value.ValueKind == JsonValueKind.String ? value.GetString() : null;

    private async Task<PaymentStartResult> PostAsync(IReadOnlyList<KeyValuePair<string, string>> fields, CancellationToken ct)
    {
        HttpClient client = _clients.CreateClient(IntegrationHttpClients.PaymentProvider);
        using FormUrlEncodedContent content = new(fields);
        using HttpResponseMessage response = await client.PostAsync(PayTrStartForm.TokenAddress, content, ct);
        string body = await response.Content.ReadAsStringAsync(ct);
        PaymentStartResult? result = ReadAnswer((int)response.StatusCode, body, fields, SecondTokenReasonTexts, out ExternalCallFailure? failure);
        return result ?? _runner.Failed<PaymentStartResult>(IntegrationOperation.PaymentStart, failure!);
    }

    private async Task<PaymentQueryResult> PostQueryAsync(IReadOnlyList<KeyValuePair<string, string>> fields, string merchantOid, CancellationToken ct)
    {
        HttpClient client = _clients.CreateClient(IntegrationHttpClients.PaymentProvider);
        using FormUrlEncodedContent content = new(fields);
        using HttpResponseMessage response = await client.PostAsync(PayTrPaymentQuery.QueryAddress, content, ct);
        string body = await response.Content.ReadAsStringAsync(ct);
        PaymentQueryResult result = PayTrPaymentQuery.ReadAnswer((int)response.StatusCode, body, merchantOid);
        if (result is PaymentQueryResult.NoResponse noResponse)
        {
            _runner.WriteFailure(IntegrationOperation.PaymentQuery, ExternalCallFailure.Unrecognized(noResponse.ReasonCode));
        }

        return result;
    }
}
