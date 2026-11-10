using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The one writer of the refusal line of the system's own transition (02 3.31.2; 05 9.1.3, 4.1.2, 7.3.2; K-1559): when the end of the payment window,
/// a provider notification or the final query finds the order, under its row lock, in a state its transition no longer fits - an administrator marked
/// the transfer paid, the customer cancelled -, nothing is applied, no notice goes, and this line is the only trace: the event, the refusal's code and
/// the order's internal id. No personal data goes in (09 18.2 item 2). The customer's refusal is told by the order page and the administrator's by
/// the panel; neither writes this line.
/// </summary>
internal sealed partial class AutomaticTransitionLog
{
    private readonly ILogger<AutomaticTransitionLog> _logger;

    public AutomaticTransitionLog(ILogger<AutomaticTransitionLog> logger)
    {
        _logger = logger;
    }

    /// <summary>Writes the refusal of the system's transition on <paramref name="orderId"/> with its code.</summary>
    public void Refused(Guid orderId, TransitionRejection reason) => LogRefused(_logger, LogEvents.AutomaticTransitionRefused, reason, orderId);

    [LoggerMessage(Level = LogLevel.Information, Message = "{Event} {Reason} {OrderId}")]
    private static partial void LogRefused(ILogger logger, string @event, TransitionRejection reason, Guid orderId);
}
