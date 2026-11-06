using System.Threading.RateLimiting;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// A sliding window limiter whose window moves with the application's time source instead of the framework's own timer (09 15.4 item 1; 09 7.1 item 1;
/// 09 19.2 item 2): the framework's <see cref="SlidingWindowRateLimiter"/> does the counting with its automatic replenishment switched off, and this
/// wrapper replenishes it - one segment for every whole segment of time that has passed since the last replenishment, at most one window's worth -
/// before it decides a request. A test that holds a fake <see cref="TimeProvider"/> therefore moves the window segment by segment without waiting. The
/// wrapper also answers the idle question itself: a partition that has not been used for a whole window has nothing left to count, so the partitioned
/// limiter drops it from memory and an address that comes back starts clean - the counters of the general limit live as long as their window and no longer.
/// </summary>
internal sealed class TimeDrivenSlidingWindowLimiter : RateLimiter
{
    private readonly SlidingWindowRateLimiter _inner;
    private readonly TimeProvider _time;
    private readonly TimeSpan _window;
    private readonly TimeSpan _segment;
    private readonly int _segments;
    private readonly Lock _gate = new();
    private DateTimeOffset _lastReplenished;
    private DateTimeOffset _lastUsed;

    public TimeDrivenSlidingWindowLimiter(int permitLimit, TimeSpan window, int segmentsPerWindow, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);
        _time = time;
        _window = window;
        _segments = segmentsPerWindow;
        _segment = window / segmentsPerWindow;
        _inner = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = window,
            SegmentsPerWindow = segmentsPerWindow,
            QueueLimit = 0,
            AutoReplenishment = false,
        });
        _lastReplenished = time.GetUtcNow();
        _lastUsed = _lastReplenished;
    }

    public override TimeSpan? IdleDuration
    {
        get
        {
            lock (_gate)
            {
                TimeSpan sinceUse = _time.GetUtcNow() - _lastUsed;
                return sinceUse >= _window ? sinceUse : null;
            }
        }
    }

    public override RateLimiterStatistics? GetStatistics() => _inner.GetStatistics();

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        lock (_gate)
        {
            DateTimeOffset now = _time.GetUtcNow();
            Replenish(now);
            _lastUsed = now;
            return _inner.AttemptAcquire(permitCount);
        }
    }

    protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
        => ValueTask.FromResult(AttemptAcquireCore(permitCount));

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _inner.Dispose();
        }

        base.Dispose(disposing);
    }

    private void Replenish(DateTimeOffset now)
    {
        TimeSpan elapsed = now - _lastReplenished;
        if (elapsed < _segment)
        {
            return;
        }

        long whole = (long)(elapsed / _segment);
        if (whole >= _segments)
        {
            // A whole window has passed: every segment is empty, and the next segment boundary counts from now.
            for (int i = 0; i < _segments; i++)
            {
                _inner.TryReplenish();
            }

            _lastReplenished = now;
            return;
        }

        for (long i = 0; i < whole; i++)
        {
            _inner.TryReplenish();
        }

        _lastReplenished += _segment * whole;
    }
}
