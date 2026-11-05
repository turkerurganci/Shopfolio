using System.Net;

using Shopfolio.Endpoints;

namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// One beat of the scheduler's heartbeat (05 5.4.1; 09 13.1 item 8). It asks the liveness endpoint of the site's public address, through the
/// reverse proxy, within the user-waiting limit (<see cref="ExternalCallTimeouts.UserWaiting"/>); only when the answer is 200 it writes the
/// heartbeat - a failed request, a refused connection and a timeout write nothing, and that is how an outage is measured. The write is
/// <see cref="OutageRecovery.ApplyAsync"/>, the one place that writes a heartbeat. The request is made with no transaction open and no lock
/// held.
/// </summary>
internal sealed class HeartbeatPulse
{
    /// <summary>The name of the <see cref="IHttpClientFactory"/> client that carries the request.</summary>
    public const string ClientName = "Heartbeat";

    private readonly IHttpClientFactory _clients;
    private readonly IServiceScopeFactory _scopes;
    private readonly TimeProvider _time;

    public HeartbeatPulse(IHttpClientFactory clients, IServiceScopeFactory scopes, TimeProvider time)
    {
        _clients = clients;
        _scopes = scopes;
        _time = time;
    }

    /// <summary>Makes one beat; returns whether the request was answered and the heartbeat written.</summary>
    public async Task<bool> BeatAsync(CancellationToken ct)
    {
        if (!await LivenessAnswersAsync(ct))
        {
            return false;
        }

        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        await OutageRecovery.ApplyAsync(scope.ServiceProvider, _time.GetUtcNow(), ct);
        return true;
    }

    private async Task<bool> LivenessAnswersAsync(CancellationToken ct)
    {
        using HttpClient client = _clients.CreateClient(ClientName);
        using var limit = new CancellationTokenSource(ExternalCallTimeouts.UserWaiting, _time);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(limit.Token, ct);
        try
        {
            using HttpResponseMessage response = await client.GetAsync(LivenessEndpoint.Path, linked.Token);
            return response.StatusCode == HttpStatusCode.OK;
        }
        catch (HttpRequestException)
        {
            // The proxy, the application or the network did not answer: no heartbeat (05 5.4.1).
            return false;
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            // The limit passed: a timeout is a failed request, and the host is not stopping.
            return false;
        }
    }
}
