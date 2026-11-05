namespace Shopfolio.UnitTests.Fakes;

/// <summary>
/// Hand-written stateful fake clock (09 7.1 item 4, 19.1 item 3): <see cref="GetUtcNow"/> returns the set instant,
/// <see cref="SetUtcNow"/> and <see cref="Advance"/> move it. Tests never wait; they move this clock. The timers it makes - the ones
/// <c>Task.Delay(span, timeProvider, token)</c> uses - fire when the clock reaches them, so a wait of the code under test ends by
/// <see cref="Advance"/> and not by real time.
/// </summary>
public sealed class ManualTimeProvider : TimeProvider
{
    private readonly List<ManualTimer> _timers = [];
    private readonly object _gate = new();
    private DateTimeOffset _utcNow;

    public ManualTimeProvider(DateTimeOffset utcNow)
    {
        _utcNow = utcNow.ToUniversalTime();
    }

    /// <summary>The number of timers that are set and have not fired (a test waits for the code to be waiting before it moves the clock).</summary>
    public int ActiveTimerCount
    {
        get
        {
            lock (_gate)
            {
                return _timers.Count(t => t.IsActive);
            }
        }
    }

    public override DateTimeOffset GetUtcNow()
    {
        lock (_gate)
        {
            return _utcNow;
        }
    }

    public override ITimer CreateTimer(TimerCallback callback, object? state, TimeSpan dueTime, TimeSpan period)
    {
        ArgumentNullException.ThrowIfNull(callback);
        ManualTimer timer = new(this, callback, state);
        lock (_gate)
        {
            _timers.Add(timer);
        }

        timer.Change(dueTime, period);
        return timer;
    }

    public void SetUtcNow(DateTimeOffset utcNow)
    {
        lock (_gate)
        {
            _utcNow = utcNow.ToUniversalTime();
        }

        FireDueTimers();
    }

    public void Advance(TimeSpan by)
    {
        lock (_gate)
        {
            _utcNow = _utcNow.Add(by);
        }

        FireDueTimers();
    }

    private void FireDueTimers()
    {
        List<ManualTimer> due;
        lock (_gate)
        {
            due = [.. _timers.Where(t => t.IsDue(_utcNow))];
        }

        foreach (ManualTimer timer in due)
        {
            timer.Fire(GetUtcNow());
        }
    }

    private void Remove(ManualTimer timer)
    {
        lock (_gate)
        {
            _timers.Remove(timer);
        }
    }

    private sealed class ManualTimer : ITimer
    {
        private readonly ManualTimeProvider _owner;
        private readonly TimerCallback _callback;
        private readonly object? _state;
        private DateTimeOffset? _dueAt;
        private TimeSpan _period = Timeout.InfiniteTimeSpan;

        public ManualTimer(ManualTimeProvider owner, TimerCallback callback, object? state)
        {
            _owner = owner;
            _callback = callback;
            _state = state;
        }

        public bool IsActive
        {
            get
            {
                lock (_owner._gate)
                {
                    return _dueAt is not null;
                }
            }
        }

        public bool Change(TimeSpan dueTime, TimeSpan period)
        {
            lock (_owner._gate)
            {
                _period = period;
                _dueAt = dueTime == Timeout.InfiniteTimeSpan ? null : _owner._utcNow + dueTime;
            }

            // A due time of zero fires at once, as a real timer does.
            if (dueTime == TimeSpan.Zero)
            {
                Fire(_owner.GetUtcNow());
            }

            return true;
        }

        public bool IsDue(DateTimeOffset now)
        {
            return _dueAt is { } due && due <= now;
        }

        public void Fire(DateTimeOffset now)
        {
            lock (_owner._gate)
            {
                if (_dueAt is not { } due || due > now)
                {
                    return;
                }

                _dueAt = _period == Timeout.InfiniteTimeSpan ? null : now + _period;
            }

            _callback(_state);
        }

        public void Dispose()
        {
            lock (_owner._gate)
            {
                _dueAt = null;
            }

            _owner.Remove(this);
        }

        public ValueTask DisposeAsync()
        {
            Dispose();
            return ValueTask.CompletedTask;
        }
    }
}
