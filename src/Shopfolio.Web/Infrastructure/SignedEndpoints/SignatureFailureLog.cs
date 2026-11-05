using System.Net;
using System.Net.Sockets;

using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.SignedEndpoints;

/// <summary>
/// The thinned security line of a request whose signature does not hold (08 3.2 principle 2; 05 4.5.2, 9.1.3; 07 4.30.1; 09 11.1 item 5, 18.2
/// item 3): the first such request of an address writes one <see cref="LogEvents.SignatureFailureSampled"/> line; the later ones of the same
/// address write no line until <see cref="SignedEndpointLimits.FailureLogWindow"/> has passed since that line, and are only counted; the count is
/// a field of the address's first line after the window. The address is the axis of the general rate limit - an IPv4 address, an IPv6 /64
/// (07 2.8.2) - and is not written: the line's request id joins it to the access log, which has the address (05 9.1.2). The line carries the
/// reason and the count only - no body, no field, no signature, no key. The windows are kept in memory, as the rate limit's counters are
/// (05 11.2.9), and every ended window is dropped at the next sweep, so the memory does not grow with the addresses seen; an ended window with
/// a count writes that count as its address's line at the sweep, when the address has not come back by then.
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

    /// <summary>The key of the address on the rate limit's axis: the IPv4 address (an IPv4-mapped IPv6 address included), or the IPv6 /64.</summary>
    public static string AddressKey(IPAddress? address)
    {
        if (address is null)
        {
            return "unknown";
        }

        if (address.IsIPv4MappedToIPv6)
        {
            address = address.MapToIPv4();
        }

        if (address.AddressFamily != AddressFamily.InterNetworkV6)
        {
            return address.ToString();
        }

        byte[] bytes = address.GetAddressBytes();
        Array.Clear(bytes, 8, 8);
        return new IPAddress(bytes) + "/64";
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
        string key = AddressKey(address);
        DateTimeOffset now = _time.GetUtcNow();
        int counted;
        List<Window> ended;
        lock (_gate)
        {
            ended = Sweep(now, key);
            if (_windows.TryGetValue(key, out Window? window) && now - window.OpenedAt < SignedEndpointLimits.FailureLogWindow)
            {
                window.Count(reason);
                counted = -1;
            }
            else
            {
                counted = window?.Counted ?? 0;
                _windows[key] = new Window(now);
            }
        }

        foreach (Window flushed in ended)
        {
            LogSampled(_logger, LogEvents.SignatureFailureSampled, flushed.LastReason, flushed.Counted);
        }

        if (counted < 0)
        {
            return false;
        }

        LogSampled(_logger, LogEvents.SignatureFailureSampled, reason, counted);
        return true;
    }

    // Once a window long: drops every window that ended, so the memory holds only the addresses of the last windows. An ended window with a
    // count is not lost: it is returned, and its count is written as the address's first line after the window - with the reason of its last
    // counted request. The current request's own address is left to the caller, whose new line carries the count.
    private List<Window> Sweep(DateTimeOffset now, string currentKey)
    {
        List<Window> ended = [];
        if (now - _lastSweep < SignedEndpointLimits.FailureLogWindow)
        {
            return ended;
        }

        _lastSweep = now;
        foreach (KeyValuePair<string, Window> entry in _windows.ToArray())
        {
            if (!string.Equals(entry.Key, currentKey, StringComparison.Ordinal) && now - entry.Value.OpenedAt >= SignedEndpointLimits.FailureLogWindow)
            {
                _windows.Remove(entry.Key);
                if (entry.Value.Counted > 0)
                {
                    ended.Add(entry.Value);
                }
            }
        }

        return ended;
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

        public SignatureFailureReason LastReason { get; private set; }

        public void Count(SignatureFailureReason reason)
        {
            Counted++;
            LastReason = reason;
        }
    }
}
