// Deliberate violation sample of the rejection line scan (09 8.2 item 3, 18.2 item 4): refusals that write a line of their own instead of the one writer.
// The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed partial class RejectionLineElsewhere
{
    private readonly ILogger _logger;

    public void Refuse(string reason)
    {
        _logger.LogWarning(LogEvents.ValidationRejected);
        _logger.LogWarning("{Event} {Code}", "Anything", reason);
        _logger.LogWarning("{Event} {code}", "Anything", reason);
        _logger.LogWarning("{Event} {Code:l}", "Anything", reason);
        _logger.LogWarning("{Event} {@Fields}", "Anything", reason);
        _logger.LogWarning(@"{Event} {$Code}", "Anything", reason);
        _logger.LogWarning("{Event} " + "{Code,8}", "Anything", reason);
        _logger.LogWarning("""
            {Event}
            {Fields}
            """, "Anything", reason);
        using IDisposable? scope = _logger.BeginScope(new Dictionary<string, object> { ["Code"] = reason });
        Log.ForContext("Fields", reason).Warning("x");
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event} {Code}")]
    private static partial void LogRefused(ILogger logger, string @event, string code);

    [LoggerMessage(LogLevel.Warning, "{Event} {Code}")]
    private static partial void LogRefusedPositional(ILogger logger, string @event, string code);

    public void NotAViolation()
    {
        _logger.LogWarning("{Event} {CodePoint} {Fieldset}", "Anything", 1, 2);
        _logger.LogInformation("{Event} {Count}", "Anything", 3);
    }
}
