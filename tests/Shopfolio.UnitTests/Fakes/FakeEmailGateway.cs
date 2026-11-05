using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Fakes;

/// <summary>
/// Hand-written stateful fake of <see cref="IEmailGateway"/> (09 19.1 items 3 and 6, 11.1 item 8). It never leaves the process: each job
/// returns the next queued result of its family, or the job's default when none is queued, and keeps every call in order. The messages it was
/// given are kept, so a test reads what would have been sent.
/// </summary>
internal sealed class FakeEmailGateway : IEmailGateway
{
    private readonly object _gate = new();
    private readonly List<FakeCall> _calls = [];
    private readonly Queue<EmailSendResult> _sends = new();
    private readonly Queue<BounceQueryResult> _bounceQueries = new();

    public EmailSendResult DefaultSend { get; set; } = new EmailSendResult.Sent();

    public BounceQueryResult DefaultBounceQuery { get; set; } = new BounceQueryResult.List([]);

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

    /// <summary>The messages given to <see cref="SendAsync"/>, in order, whatever result each got.</summary>
    public IReadOnlyList<EmailMessage> Messages => [.. Calls.Select(c => c.Argument).OfType<EmailMessage>()];

    public void EnqueueSend(params EmailSendResult[] results)
    {
        lock (_gate)
        {
            foreach (EmailSendResult result in results)
            {
                _sends.Enqueue(result);
            }
        }
    }

    public void EnqueueBounceQuery(params BounceQueryResult[] results)
    {
        lock (_gate)
        {
            foreach (BounceQueryResult result in results)
            {
                _bounceQueries.Enqueue(result);
            }
        }
    }

    public Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _calls.Add(new FakeCall(nameof(SendAsync), message));
            return Task.FromResult(_sends.Count > 0 ? _sends.Dequeue() : DefaultSend);
        }
    }

    public Task<BounceQueryResult> QueryBouncesAsync(DateTimeOffset since, CancellationToken ct)
    {
        ct.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _calls.Add(new FakeCall(nameof(QueryBouncesAsync), since));
            return Task.FromResult(_bounceQueries.Count > 0 ? _bounceQueries.Dequeue() : DefaultBounceQuery);
        }
    }
}
