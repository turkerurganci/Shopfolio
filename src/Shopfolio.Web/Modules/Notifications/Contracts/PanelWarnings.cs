namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>
/// The panel home's two warnings (T382; 05 10.2.1, 10.2.3, 10.2.4): the e-mail channel warning from the channel state record and the firm notices warning from the dispatch rows of F-1 to
/// F-4 - the last settled one is "undelivered" and went to the firm's current contact e-mail. Both are read at the moment of the call and never stored, so each lifts by itself.
/// </summary>
public interface IPanelWarnings
{
    /// <summary>
    /// The two warnings now. <paramref name="currentCompanyEmail"/> is the firm's current contact e-mail, compared without case; null or empty means the firm has none and the firm notices
    /// warning stays closed. A plain read; nothing is written.
    /// </summary>
    Task<PanelWarnings> ReadAsync(string? currentCompanyEmail, CancellationToken ct);
}

/// <summary>The two warnings of the panel home (T382); they are not counters and can both show (05 10.2.5).</summary>
/// <param name="ChannelWarningOpen">At least five consecutive channel errors since the last success and the first of them at least fifteen minutes old.</param>
/// <param name="FirmNoticesWarningOpen">The last settled firm notice (F-1 to F-4) is "undelivered" and its recipient is the firm's current contact e-mail.</param>
public sealed record PanelWarnings(bool ChannelWarningOpen, bool FirmNoticesWarningOpen);
