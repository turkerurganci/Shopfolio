// Deliberate violation sample of the exception argument ban (T411, D-89 K2; 09 18.1; 09 18.2 item 2c): an exception handed to a template as an argument
// writes its whole text, the message included. The exception of a line goes in the exception slot. The scan reads this file as text; it is not
// compiled. Line numbers are part of the test.
internal sealed partial class ExceptionArguments
{
    private readonly ILogger _logger;

    public void Run(Guid id, Result result)
    {
        try
        {
            Work();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("{Event} {Detail}", LogEvents.JobFinished, ex);
            _logger.LogWarning("{Event} {Reason}", LogEvents.JobFinished, ex.Message);
            _logger.LogWarning("{Event} {Detail}", LogEvents.JobFinished, result.InnerException);
            _logger.LogWarning(message: "{Event} {Detail}", args: [LogEvents.JobFinished, ex.ToString()]);
            _logger.LogWarning(ex, "{Event} {JobId}", LogEvents.JobFinished, id);
            _logger.LogWarning(GetException(), "{Event} {JobId}", LogEvents.JobFinished, id);
            _logger.LogWarning("{Event} {JobId}", LogEvents.JobFinished, id);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event} {Failure}")]
    private static partial void LogAsArgument(ILogger logger, string @event, Exception failure);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}")]
    private static partial void LogInSlot(ILogger logger, string @event, Exception failure);
}
