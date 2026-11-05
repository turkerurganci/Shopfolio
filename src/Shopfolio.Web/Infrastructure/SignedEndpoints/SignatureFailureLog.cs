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
/// (05 11.2.9); a window that ended without a counted request is dropped.
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

    /// <summary>Counts a failed request of <paramref name="address"/> and writes the line when its window allows; true when a line was written.</summary>
    public bool Record(IPAddress? address, SignatureFailureReason reason)
    {
        string key = AddressKey(address);
        DateTimeOffset now = _time.GetUtcNow();
        int counted;
        lock (_gate)
        {
            Sweep(now);
            if (_windows.TryGetValue(key, out Window? window) && now - window.OpenedAt < SignedEndpointLimits.FailureLogWindow)
            {
                window.Counted++;
                return false;
            }

            counted = window?.Counted ?? 0;
            _windows[key] = new Window(now);
        }

        LogSampled(_logger, LogEvents.SignatureFailureSampled, reason, counted);
        return true;
    }

    // Once a window long: drops the windows that ended with nothing counted; a window with a count stays until its address's next line.
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

        public int Counted { get; set; }
    }
}
