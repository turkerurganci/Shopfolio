using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Fakes;

/// <summary>
/// Hand-written stateful fake of <see cref="IGoogleIdentityGateway"/> (09 19.1 items 3 and 6, 11.1 item 8). It never leaves the process: it
/// returns the next queued result, or <see cref="DefaultResult"/> when none is queued, and keeps every call in order.
/// </summary>
internal sealed class FakeGoogleIdentityGateway : IGoogleIdentityGateway
{
    private readonly object _gate = new();
    private readonly List<FakeCall> _calls = [];
    private readonly Queue<GoogleIdentityResult> _results = new();

    public GoogleIdentityResult DefaultResult { get; set; } = new GoogleIdentityResult.Failed("not-set");

    /// <summary>The calls in the order they were made.</summary>
    public IReadOnlyList<FakeCall> Calls
    {
        get
        {
            lock (_gate)
            {
                return [.. _calls];
            }
        }
    }

    public void Enqueue(params GoogleIdentityResult[] results)
    {
        lock (_gate)
        {
            foreach (GoogleIdentityResult result in results)
            {
                _results.Enqueue(result);
            }
        }
    }

    public Task<GoogleIdentityResult> VerifyIdentityAsync(IdentityCodeExchange exchange, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _calls.Add(new FakeCall(nameof(VerifyIdentityAsync), exchange));
            return Task.FromResult(_results.Count > 0 ? _results.Dequeue() : DefaultResult);
        }
    }
}
