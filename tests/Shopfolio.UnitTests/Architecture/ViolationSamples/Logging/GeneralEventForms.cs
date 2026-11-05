// Deliberate violation sample of the logging scan (09 18.2 item 3): the general forms of an event name. The scan reads this file as text; it is not compiled.
internal sealed partial class GeneralEventForms
{
    private readonly ILogger _logger;

    public void Run(string name)
    {
        _logger.LogWarning("{Event}: done.", name);
        _logger.LogWarning("{Event}: done.", "free text");
        _logger.LogWarning("{Event}: done.", LogEvents.JobFinished);
        LogDone(_logger, name);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}: done.")]
    private static partial void LogDone(ILogger logger, string text);
}
