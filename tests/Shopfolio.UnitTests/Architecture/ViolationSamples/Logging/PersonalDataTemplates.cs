// Deliberate violation sample of the log template scan (09 18.2 item 2a; D-88): templates with a name of personal data, with a suffixed name, and
// with a request field name. The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed partial class PersonalDataTemplates
{
    private readonly ILogger _logger;

    public void Run(Order order)
    {
        _logger.LogInformation("Order mail {Email} was sent", order.Email);
        _logger.LogWarning(exception, "{Event} for {CustomerPhone} and {@Address}", LogEvents.JobFinished, order.Phone, order.Address);
        _logger.LogWarning("{Event} {JobId} {MissingKeys}", LogEvents.JobFinished, order.Id, "A");
        _logger.LogWarning("{Event} {Path}", LogEvents.JobFinished, order.Path);
        // A comment with {Email} is not a finding.
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event}: reset with {Token:l}")]
    private static partial void LogReset(ILogger logger, string @event, string token);

    public static AuditLine Line(Guid id) => new AuditLine(LogEvents.AdminCommand, "{Event} {Method} {AccountId}", "Method", "x", "AccountId", id);
}
