using System.Threading.RateLimiting;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// A sliding window limiter whose window moves with the application's time source (09 15.4 item 1; 09 7.1 item 1; 09 19.2 item 2). The window is cut
/// into equal segments counted from the moment the limiter was made; the permits taken are kept per segment, and a segment leaves the window when the
/// window's last segment is one whole window later. A test that holds a fake <see cref="TimeProvider"/> therefore moves the window segment by segment
/// without waiting. The framework's <see cref="SlidingWindowRateLimiter"/> is not used: with its automatic replenishment off it still refuses a
/// <c>TryReplenish</c> that comes sooner than a real segment after the last one (it reads the operating system's clock), so it cannot follow a fake clock.
/// The limiter also answers the idle question itself: a partition that has not been used for a whole window has nothing left to count, so the
/// partitioned limiter drops it from memory and an address that comes back starts clean - the counters of the general limit live as long as their window
/// and no longer. No request waits (there is no queue).
/// </summary>
internal sealed class TimeDrivenSlidingWindowLimiter : RateLimiter
{
    private readonly int _permitLimit;
    private readonly TimeProvider _time;
    private readonly TimeSpan _window;
    private readonly TimeSpan _segment;
    private readonly int _segments;
    private readonly long[] _taken;
    private readonly DateTimeOffset _origin;
    private readonly Lock _gate = new();
    private long _currentSegment;
    private long _used;
    private DateTimeOffset _lastUsed;
    private long _successful;
    private long _failed;

    public TimeDrivenSlidingWindowLimiter(int permitLimit, TimeSpan window, int segmentsPerWindow, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(time);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(permitLimit, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(segmentsPerWindow, 0);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(window, TimeSpan.Zero);
        _permitLimit = permitLimit;
        _time = time;
        _window = window;
        _segments = segmentsPerWindow;
        _segment = window / segmentsPerWindow;
        _taken = new long[segmentsPerWindow];
        _origin = time.GetUtcNow();
        _lastUsed = _origin;
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

    public override RateLimiterStatistics? GetStatistics()
    {
        lock (_gate)
        {
            Move(_time.GetUtcNow());
            return new RateLimiterStatistics
            {
                CurrentAvailablePermits = _permitLimit - _used,
                CurrentQueuedCount = 0,
                TotalFailedLeases = _failed,
                TotalSuccessfulLeases = _successful,
            };
        }
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        ArgumentOutOfRangeException.ThrowIfGreaterThan(permitCount, _permitLimit);
        lock (_gate)
        {
            DateTimeOffset now = _time.GetUtcNow();
            Move(now);
            _lastUsed = now;
            if (_used + permitCount > _permitLimit)
            {
                _failed++;
                return new Lease(acquired: false);
            }

            _taken[_currentSegment % _segments] += permitCount;
            _used += permitCount;
            _successful++;
            return new Lease(acquired: true);
        }
    }

    protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
        => ValueTask.FromResult(AttemptAcquireCore(permitCount));

    // Moves the window to the segment the time is in: every segment passed over is emptied (all of them after a whole window or more).
    private void Move(DateTimeOffset now)
    {
        TimeSpan sinceOrigin = now - _origin;
        long target = sinceOrigin < TimeSpan.Zero ? 0 : (long)(sinceOrigin / _segment);
        if (target <= _currentSegment)
        {
            return;
        }

        long passed = target - _currentSegment;
        if (passed >= _segments)
        {
            Array.Clear(_taken);
            _used = 0;
        }
        else
        {
            for (long segment = _currentSegment + 1; segment <= target; segment++)
            {
                int slot = (int)(segment % _segments);
                _used -= _taken[slot];
                _taken[slot] = 0;
            }
        }

        _currentSegment = target;
    }

    private sealed class Lease : RateLimitLease
    {
        private readonly bool _acquired;

        public Lease(bool acquired)
        {
            _acquired = acquired;
        }

        public override bool IsAcquired => _acquired;

        public override IEnumerable<string> MetadataNames => [];

        public override bool TryGetMetadata(string metadataName, out object? metadata)
        {
            metadata = null;
            return false;
        }
    }
}
