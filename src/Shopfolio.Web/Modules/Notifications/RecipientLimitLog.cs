using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Modules.Notifications;

/// <summary>
/// The thinned security line of a customer notification that the per-recipient limit refused (08 2.2.4 "Kayıt"; K-941 (1)'s pattern; 09 18.2
/// item 3): the first refusal writes one <see cref="LogEvents.RecipientLimitExceededSampled"/> line; the later refusals write no line until
/// <see cref="RecipientSendLimit.LogWindow"/> has passed since that line, and are only counted; the count is a field of the first line after the
/// window. The line carries the count only - the address is neither written nor kept, so there is one window for the whole product, not one
/// per address; the line's request id or job id joins it to the rest of the log. The window is kept in memory and is lost at a restart (05
/// 11.2.9).
/// </summary>
internal sealed partial class RecipientLimitLog
{
    private readonly TimeProvider _time;
    private readonly ILogger<RecipientLimitLog> _logger;
    private readonly Lock _gate = new();
    private DateTimeOffset? _openedAt;
    private int _counted;

    public RecipientLimitLog(TimeProvider time, ILogger<RecipientLimitLog> logger)
    {
        _time = time;
        _logger = logger;
    }

    /// <summary>Counts a refusal and writes the line when the window allows; true when a line was written.</summary>
    public bool Record()
    {
        DateTimeOffset now = _time.GetUtcNow();
        int counted;
        lock (_gate)
        {
            if (_openedAt is { } openedAt && now - openedAt < RecipientSendLimit.LogWindow)
            {
                _counted++;
                return false;
            }

            counted = _counted;
            _counted = 0;
            _openedAt = now;
        }

        LogSampled(_logger, LogEvents.RecipientLimitExceededSampled, counted);
        return true;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event} {SuppressedCount}")]
    private static partial void LogSampled(ILogger logger, string @event, int suppressedCount);
}
