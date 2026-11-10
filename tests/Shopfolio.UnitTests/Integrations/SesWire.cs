using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

using Amazon.Runtime;
using Amazon.SimpleEmailV2;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>One request the library made: its method, address and body text, and the headers that matter.</summary>
internal sealed record WireRequest(string Method, Uri Uri, string Body, IReadOnlyDictionary<string, string> Headers)
{
    public bool IsSend => Method == "POST" && Uri.AbsolutePath == "/v2/email/outbound-emails";

    public bool IsQuery => Method == "GET" && Uri.AbsolutePath.StartsWith("/v2/email/suppression/addresses/", StringComparison.Ordinal);

    /// <summary>The list call (ListSuppressedDestinations): the collection address with no address after it.</summary>
    public bool IsList => Method == "GET" && Uri.AbsolutePath == "/v2/email/suppression/addresses";

    /// <summary>The query string as name and values (a name may repeat).</summary>
    public ILookup<string, string> QueryString => Uri.Query.TrimStart('?')
        .Split('&', StringSplitOptions.RemoveEmptyEntries)
        .Select(pair => pair.Split('=', 2))
        .ToLookup(p => Uri.UnescapeDataString(p[0]), p => p.Length > 1 ? Uri.UnescapeDataString(p[1]) : string.Empty, StringComparer.Ordinal);

    public JsonDocument Json() => JsonDocument.Parse(Body);
}

/// <summary>
/// What a test makes the wire do for one request: answer with a status, an AWS error type and a body, hang until the token is cancelled, or break the
/// connection. The real library client runs on top of it, so no test opens a socket and the request is the one the application would send.
/// </summary>
internal abstract record WireAnswer
{
    private WireAnswer()
    {
    }

    public sealed record Reply(HttpStatusCode Status, string Body, string? ErrorType = null) : WireAnswer;

    public sealed record Hang : WireAnswer;

    public sealed record Refuse : WireAnswer;

    public static WireAnswer Sent() => new Reply(HttpStatusCode.OK, """{"MessageId":"0102-made-up-message-id-SECRETID"}""");

    public static WireAnswer Listed(string address, string reason = "BOUNCE") =>
        new Reply(HttpStatusCode.OK, "{\"SuppressedDestination\":{\"EmailAddress\":" + JsonSerializer.Serialize(address) + ",\"Reason\":\"" + reason + "\",\"LastUpdateTime\":1.7E9}}");

    /// <summary>One page of the account list: the summaries as (address, reason) and the token of the next page, if any.</summary>
    public static WireAnswer ListPage(IEnumerable<(string Address, string Reason)> summaries, string? nextToken = null)
    {
        string items = string.Join(",", summaries.Select(s => "{\"EmailAddress\":" + JsonSerializer.Serialize(s.Address) + ",\"Reason\":\"" + s.Reason + "\",\"LastUpdateTime\":1.7E9}"));
        string continuation = nextToken is null ? string.Empty : ",\"NextToken\":" + JsonSerializer.Serialize(nextToken);
        return new Reply(HttpStatusCode.OK, "{\"SuppressedDestinationSummaries\":[" + items + "]" + continuation + "}");
    }

    public static WireAnswer NotListed() => Error(HttpStatusCode.NotFound, "NotFoundException", "Not found");

    public static WireAnswer Error(HttpStatusCode status, string errorType, string message) =>
        new Reply(status, "{\"message\":" + JsonSerializer.Serialize(message) + "}", errorType);
}

/// <summary>The HTTP handler under the library client: it records every request and answers each by the test's script.</summary>
internal sealed class SesWire : HttpMessageHandler
{
    private readonly Func<WireRequest, WireAnswer> _script;
    private readonly SemaphoreSlim _arrived = new(0);
    private readonly List<WireRequest> _requests = [];

    public SesWire(Func<WireRequest, WireAnswer> script)
    {
        _script = script;
    }

    public IReadOnlyList<WireRequest> Requests
    {
        get
        {
            lock (_requests)
            {
                return [.. _requests];
            }
        }
    }

    /// <summary>Completes when one more request has reached the wire than the number of earlier waits.</summary>
    public Task NextRequestAsync(CancellationToken ct) => _arrived.WaitAsync(ct);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        string body = request.Content is null ? string.Empty : await request.Content.ReadAsStringAsync(cancellationToken);
        Dictionary<string, string> headers = request.Headers.ToDictionary(h => h.Key, h => string.Join(",", h.Value), StringComparer.OrdinalIgnoreCase);
        WireRequest seen = new(request.Method.Method, request.RequestUri!, body, headers);
        lock (_requests)
        {
            _requests.Add(seen);
        }

        _arrived.Release();
        switch (_script(seen))
        {
            case WireAnswer.Reply reply:
                HttpResponseMessage response = new(reply.Status) { Content = new StringContent(reply.Body, Encoding.UTF8, "application/json") };
                if (reply.ErrorType is not null)
                {
                    response.Headers.TryAddWithoutValidation("x-amzn-ErrorType", reply.ErrorType);
                }

                return response;
            case WireAnswer.Hang:
                TaskCompletionSource never = new(TaskCreationOptions.RunContinuationsAsynchronously);
                await using (cancellationToken.Register(() => never.TrySetCanceled(cancellationToken)))
                {
                    await never.Task;
                }

                throw new UnreachableException();
            case WireAnswer.Refuse:
                throw new HttpRequestException(HttpRequestError.ConnectionError, "refused");
            default:
                throw new UnreachableException();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _arrived.Dispose();
        }

        base.Dispose(disposing);
    }
}

/// <summary>Gives the library client the test's wire instead of a socket (the library's own hook for an HTTP source).</summary>
internal sealed class SesWireSource : HttpClientFactory
{
    private readonly HttpMessageHandler _wire;

    public SesWireSource(HttpMessageHandler wire)
    {
        _wire = wire;
    }

    public override HttpClient CreateHttpClient(IClientConfig clientConfig) => new(_wire, disposeHandler: false);

    public override bool UseSDKHttpClientCaching(IClientConfig clientConfig) => false;

    public override bool DisposeHttpClientsAfterUse(IClientConfig clientConfig) => true;
}

/// <summary>The made-up installation values the gateway tests share, and the client made the way the application makes it - on the test's wire.</summary>
internal static class SesTestSetup
{
    public const string SenderAddress = "bilgi@example.com";

    public const string AccessKeyId = "AKIAMADEUPKEYID0001";

    public const string SecretKey = "made-up-secret-key-SECRETKEY-0002";

    public const string Region = "eu-central-1";

    public static EmailOptions Options() => new()
    {
        SenderAddress = SenderAddress,
        Region = Region,
        Credential = AccessKeyId + "\n" + SecretKey + "\n",
    };

    public static AmazonSimpleEmailServiceV2Client Client(SesWire wire, EmailOptions? options = null) =>
        SesClientFactory.Create(options ?? Options(), new SesWireSource(wire));
}
