using System.Text.Json;
using Microsoft.Extensions.Options;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.SearchEngine;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The payment provider's implementation of the link layer: PayTR's iFrame API (08 2.1; 09 11.1 item 1; KD-64 - the one implementation). T401 builds the start (08 2.1.4):
/// the token request goes server to server, form-encoded, with the keys of the one setting (<see cref="PaymentProviderKeys"/>); without the setting no call is made. A
/// successful answer gives the frame's address and the token is not kept - neither in a log nor in the database (the order has no field for it). The notification's
/// signature and reading (T402), the final query (T403), the refund and its query (T405) are the later provider tasks; until they come those jobs refuse with
/// <see cref="NotSupportedException"/>: the final query's handler and the refund jobs are not registered yet (D-449), and the notification endpoint answers 500 - "not
/// received" - as it did without an implementation (07 4.30.1: a notification that cannot be recorded is not acknowledged).
/// </summary>
internal sealed class PayTrPaymentGateway : IPaymentGateway
{
    /// <summary>The reason code of a start without the provider's keys (08 2.1.2): no call is made.</summary>
    public const string KeysMissingReason = "keys-missing";

    /// <summary>The reason code of a client address the provider's form cannot carry (08 2.1.4 <c>user_ip</c>): a code error, no call is made.</summary>
    public const string ClientAddressReason = "client-address";

    /// <summary>The reason code of a <c>failed</c> answer (08 2.1.8): the provider's <c>reason</c> text may repeat a field and is not written.</summary>
    public const string FailedReason = "failed";

    /// <summary>The reason code of an answer whose body is not the table's JSON (08 2.1.8: a connection error).</summary>
    public const string UnrecognizedBodyReason = "body";

    private readonly IHttpClientFactory _clients;
    private readonly ExternalCallRunner _runner;
    private readonly PaymentProviderKeys? _keys;
    private readonly SiteAddress _site;
    private readonly TimeProvider _time;
    private readonly bool _testMode;

    public PayTrPaymentGateway(
        IHttpClientFactory clients, ExternalCallRunner runner, IOptions<PaymentOptions> payment, SiteAddress site, TimeProvider time, IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(environment);
        _clients = clients;
        _runner = runner;
        _keys = PaymentProviderKeys.TryParse(payment.Value.ProviderKeys);
        _site = site;
        _time = time;

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

    public Task<PaymentQueryResult> QueryPaymentAsync(string orderNumber, CancellationToken ct) =>
        throw new NotSupportedException("The final query is built by T403.");

    public Task<RefundRequestResult> RequestRefundAsync(CardRefund refund, CancellationToken ct) =>
        throw new NotSupportedException("The card refund is built by T405.");

    public Task<RefundQueryResult> QueryRefundAsync(CardRefund refund, CancellationToken ct) =>
        throw new NotSupportedException("The card refund's query is built by T405.");

    public NotificationVerification VerifyNotification(IReadOnlyDictionary<string, string> formFields) =>
        throw new NotSupportedException("The notification's signature is built by T402.");

    public NotificationResult ReadNotificationResult(string eventType, string outcome) =>
        throw new NotSupportedException("The notification's reading is built by T402.");

    public NotificationAmountCheck CheckNotificationAmount(PaymentNotice notice, decimal orderTotal) =>
        throw new NotSupportedException("The notification's amount check is built by T402.");

    /// <summary>
    /// The answer of a token request (08 2.1.4): <c>status</c> = <c>success</c> with a <c>token</c> is the frame's address; every other answer - <c>failed</c>, a
    /// status outside 2xx, a body that is not the table's - is a connection error with a short reason code (08 2.1.8; the full table is T404's). Pure: no field of the
    /// answer other than <c>status</c> and <c>token</c> is read.
    /// </summary>
    public static PaymentStartResult? ReadAnswer(int status, string body, out ExternalCallFailure? failure)
    {
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
            string? outcome = root.ValueKind == JsonValueKind.Object && root.TryGetProperty("status", out JsonElement s) && s.ValueKind == JsonValueKind.String ? s.GetString() : null;
            if (outcome == "success"
                && root.TryGetProperty("token", out JsonElement t)
                && t.ValueKind == JsonValueKind.String
                && t.GetString() is { Length: > 0 } token)
            {
                return new PaymentStartResult.Started(new Uri(PayTrStartForm.FrameAddressRoot + Uri.EscapeDataString(token)));
            }

            failure = ExternalCallFailure.Unrecognized(outcome == "failed" ? FailedReason : UnrecognizedBodyReason);
            return null;
        }
        catch (JsonException)
        {
            failure = ExternalCallFailure.Unrecognized(UnrecognizedBodyReason);
            return null;
        }
    }

    private async Task<PaymentStartResult> PostAsync(IReadOnlyList<KeyValuePair<string, string>> fields, CancellationToken ct)
    {
        HttpClient client = _clients.CreateClient(IntegrationHttpClients.PaymentProvider);
        using FormUrlEncodedContent content = new(fields);
        using HttpResponseMessage response = await client.PostAsync(PayTrStartForm.TokenAddress, content, ct);
        string body = await response.Content.ReadAsStringAsync(ct);
        PaymentStartResult? result = ReadAnswer((int)response.StatusCode, body, out ExternalCallFailure? failure);
        return result ?? _runner.Failed<PaymentStartResult>(IntegrationOperation.PaymentStart, failure!);
    }
}
