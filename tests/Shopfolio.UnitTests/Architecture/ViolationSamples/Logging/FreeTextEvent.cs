// Deliberate violation sample of the logging scan (09 18.2 item 3): a log method that takes the event name as a parameter is called with a
// free text and with a variable. The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed partial class FreeTextEvent
{
    private readonly ILogger _logger;

    public void Run(string name)
    {
        LogDone(_logger, "something happened");
        LogDone(_logger, name);
        LogDone(_logger, LogEvents.JobFinished);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}: done.")]
    private static partial void LogDone(ILogger logger, string @event);
}
