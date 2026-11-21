// Deliberate violation sample of the exception argument ban for what the text scan cannot type (T604 A3, D-1005 form 1, D-1020; 09 18.2 item 2a): an
// exception that is a member of another file, a variable taken from an expression, a template kept in a constant, the exception or its text in the
// place of the template. The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class ExceptionArgumentsUntyped
{
    private const string Template = "{Event} {Detail}";

    private readonly ILogger _logger;
    private readonly Failures _failures;

    public void Run(Guid id)
    {
        var failure = _failures.Last.Exception;
        _logger.LogWarning("{Event} {Detail}", LogEvents.JobFinished, failure);
        _logger.LogWarning("{Event} {Detail}", LogEvents.JobFinished, _failures.LastException);
        _logger.LogWarning(Template, LogEvents.JobFinished, _failures.FirstEx);
        _logger.LogWarning(Template, LogEvents.JobFinished, failure);
        _logger.LogWarning(_failures.LastException.Message);
        _logger.LogWarning(failure.ToString());
        _logger.LogWarning(Template, LogEvents.JobFinished, id);
        _logger.LogWarning(failure, Template, LogEvents.JobFinished, id);
        _logger.LogWarning(_failures.LastException, Template, LogEvents.JobFinished);
        _logger.LogWarning("{Event} {Type}", LogEvents.JobFinished, nameof(SqlException));
        _logger.LogWarning("{Event} {Count}", LogEvents.JobFinished, _failures.ExceptionCount);
        _logger.LogWarning(exception: failure, message: Template, args: [LogEvents.JobFinished]);
    }
}
