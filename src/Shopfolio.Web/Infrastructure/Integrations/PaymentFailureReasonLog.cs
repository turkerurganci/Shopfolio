using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The thinned line of a failed payment's reason (08 2.1.5 "Bildirim — eşleme": <c>failed_reason_code</c> and <c>failed_reason_msg</c> are not kept, they are written to
/// the application log thinned, for debugging; K-941 (1)'s pattern; 09 18.2 item 3): the first verified notification of a failed payment writes one
/// <see cref="LogEvents.PaymentFailedReasonSampled"/> line with the provider's reason code and message; the later ones write no line until <see cref="Window"/> has
/// passed since that line, and are only counted; the count is a field of the first line after the window. The window is one for the whole product - the reason is
/// no address and nothing of the order is written (no reference, no amount), so the line's request id joins it to the access log. Neither value is kept anywhere
/// else: no record field, no notice field. The window is kept in memory and is lost at a restart (05 11.2.9).
/// </summary>
internal sealed partial class PaymentFailureReasonLog
{
    /// <summary>The window of the line: 60 seconds, the general rate limit's sliding window (07 2.8.2; K-1570 - one kind of window; 09 3.5).</summary>
    public static readonly TimeSpan Window = TimeSpan.FromSeconds(60);

    private readonly TimeProvider _time;
    private readonly ILogger<PaymentFailureReasonLog> _logger;
    private readonly Lock _gate = new();
    private DateTimeOffset? _openedAt;
    private int _counted;

    public PaymentFailureReasonLog(TimeProvider time, ILogger<PaymentFailureReasonLog> logger)
    {
        _time = time;
        _logger = logger;
    }

    /// <summary>Counts a failed payment's reason and writes the line when the window allows; true when a line was written. A missing value is written empty.</summary>
    public bool Record(string? reasonCode, string? reasonMessage)
    {
        DateTimeOffset now = _time.GetUtcNow();
        int counted;
        lock (_gate)
        {
            if (_openedAt is { } openedAt && now - openedAt < Window)
            {
                _counted++;
                return false;
            }

            counted = _counted;
            _counted = 0;
            _openedAt = now;
        }

        LogSampled(_logger, LogEvents.PaymentFailedReasonSampled, reasonCode ?? string.Empty, reasonMessage ?? string.Empty, counted);
        return true;
    }

    // Information: a failed payment is a customer's outcome, not a fault of the product; the line is for debugging (08 2.1.5).
    [LoggerMessage(Level = LogLevel.Information, Message = "{Event} {ReasonCode} {ReasonMessage} {SuppressedCount}")]
    private static partial void LogSampled(ILogger logger, string @event, string reasonCode, string reasonMessage, int suppressedCount);
}
