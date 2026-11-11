using System.Net;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Infrastructure.SignedEndpoints;

/// <summary>
/// The thinned security line of a request whose signature does not hold (08 3.2 principle 2; 05 4.5.2, 9.1.3; 07 4.30.1; 09 11.1 item 5, 18.2
/// item 3): the first such request of an address writes one <see cref="LogEvents.SignatureFailureSampled"/> line; the later ones of the same
/// address write no line until <see cref="SignedEndpointLimits.FailureLogWindow"/> has passed since that line, and are only counted; the count is
/// a field of the address's first line after the window. The address is the axis of the general rate limit - an IPv4 address, an IPv6 /64
/// (07 2.8.2) - and is not written: the line's request id joins it to the access log, which has the address (05 9.1.2). The line carries the
/// reason and the count only - no body, no field, no signature, no key. The count is written only on its own address's line, never on the line
/// of another address's request (that line carries the other request's id). The windows are kept in memory, as the rate limit's counters are,
/// and are lost at a restart (05 11.2.9). An ended window without a count is dropped at the next sweep, as nothing is lost with it; an ended
/// window with a count stays until its address comes back and its line carries the count - or until the ceiling of
/// <see cref="SignedEndpointLimits.FailureLogMaxAddresses"/> windows is full (D-108; K-1582): then a new address first drops the ended windows without a count,
/// and if the ceiling is still full the oldest window with a count writes its count at once in its own line and is dropped (the count is not lost, only
/// written before its address comes back; that line carries the reason of its address's last counted request); with no window with a count, the oldest
/// window is dropped, which loses nothing.
/// </summary>
internal sealed partial class SignatureFailureLog
{
    private readonly TimeProvider _time;
    private readonly ILogger<SignatureFailureLog> _logger;
    private readonly Dictionary<string, Window> _windows = new(StringComparer.Ordinal);
    private readonly Lock _gate = new();
    private DateTimeOffset _lastSweep;

    public SignatureFailureLog(TimeProvider time, ILogger<SignatureFailureLog> logger)
    {
        _time = time;
        _logger = logger;
        _lastSweep = time.GetUtcNow();
    }

    /// <summary>How many addresses have a window in memory.</summary>
    public int TrackedAddressCount
    {
        get
        {
            lock (_gate)
            {
                return _windows.Count;
            }
        }
    }

    /// <summary>Counts a failed request of <paramref name="address"/> and writes the line when its window allows; true when a line was written.</summary>
    public bool Record(IPAddress? address, SignatureFailureReason reason)
    {
        string key = ClientAddress.Axis(address);
        DateTimeOffset now = _time.GetUtcNow();
        int counted;
        Window? flushed = null;
        lock (_gate)
        {
            Sweep(now);
            if (_windows.TryGetValue(key, out Window? window) && now - window.OpenedAt < SignedEndpointLimits.FailureLogWindow)
            {
                window.Count(reason);
                return false;
            }

            counted = window?.Counted ?? 0;
            if (window is null && _windows.Count >= SignedEndpointLimits.FailureLogMaxAddresses)
            {
                flushed = MakeRoom(now);
            }

            _windows[key] = new Window(now);
        }

        if (flushed is { Counted: > 0, LastReason: { } flushedReason })
        {
            LogSampled(_logger, LogEvents.SignatureFailureSampled, flushedReason, flushed.Counted);
        }

        LogSampled(_logger, LogEvents.SignatureFailureSampled, reason, counted);
        return true;
    }

    // The ceiling is full and a new address comes (D-108; K-1582): the ended windows without a count go first - they lose nothing; if that is not enough, the
    // oldest window with a count is taken out and returned, so its count is written at once; with none, the oldest window goes. Runs only at the ceiling.
    private Window? MakeRoom(DateTimeOffset now)
    {
        foreach (KeyValuePair<string, Window> entry in _windows.ToArray())
        {
            if (entry.Value.Counted == 0 && now - entry.Value.OpenedAt >= SignedEndpointLimits.FailureLogWindow)
            {
                _windows.Remove(entry.Key);
            }
        }

        if (_windows.Count < SignedEndpointLimits.FailureLogMaxAddresses)
        {
            return null;
        }

        KeyValuePair<string, Window>? oldest = null;
        KeyValuePair<string, Window>? oldestCounted = null;
        foreach (KeyValuePair<string, Window> entry in _windows)
        {
            if (oldest is null || entry.Value.OpenedAt < oldest.Value.Value.OpenedAt)
            {
                oldest = entry;
            }

            if (entry.Value.Counted > 0 && (oldestCounted is null || entry.Value.OpenedAt < oldestCounted.Value.Value.OpenedAt))
            {
                oldestCounted = entry;
            }
        }

        KeyValuePair<string, Window> taken = oldestCounted ?? oldest!.Value; // non-null: the map is full here (the guard above), so the loop set oldest
        _windows.Remove(taken.Key);
        return taken.Value;
    }

    // Once a window long: drops every ended window without a count - its address's next line would carry a count of zero, as with no window.
    // An ended window with a count is kept: the count is a field of its address's first line after the window and of no other line.
    private void Sweep(DateTimeOffset now)
    {
        if (now - _lastSweep < SignedEndpointLimits.FailureLogWindow)
        {
            return;
        }

        _lastSweep = now;
        foreach (KeyValuePair<string, Window> entry in _windows.ToArray())
        {
            if (entry.Value.Counted == 0 && now - entry.Value.OpenedAt >= SignedEndpointLimits.FailureLogWindow)
            {
                _windows.Remove(entry.Key);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event} {Reason} {SuppressedCount}")]
    private static partial void LogSampled(ILogger logger, string @event, SignatureFailureReason reason, int suppressedCount);

    private sealed class Window
    {
        public Window(DateTimeOffset openedAt)
        {
            OpenedAt = openedAt;
        }

        public DateTimeOffset OpenedAt { get; }

        public int Counted { get; private set; }

        /// <summary>The reason of the last counted request: the line of a window flushed at the ceiling carries it.</summary>
        public SignatureFailureReason? LastReason { get; private set; }

        public void Count(SignatureFailureReason reason)
        {
            Counted++;
            LastReason = reason;
        }
    }
}
