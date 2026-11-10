// Deliberate violation sample of the template scan's string forms (T411, D-89 K1; 09 18.2 item 2a): every form a template can be written in, each with
// a name of personal data, and the clean lines the scan leaves alone. The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed partial class TemplateForms
{
    private readonly ILogger _logger;

    public void Run(Order order)
    {
        _logger.LogWarning("see https://example.test {Email}", order.Id);
        _logger.LogWarning($"see {{Email}} {order.Id}", order.Id);
        _logger.LogWarning(@"verbatim ""quoted"" {PhoneNumber}", order.Id);
        _logger.LogWarning("""{CardNumber} raw""", order.Id);
        _logger.LogWarning(GetException("x"), "{Event} {PasswordHash}", LogEvents.JobFinished);
        _logger.LogWarning(message: "{TokenValue}", args: order.Id);
        _logger.LogWarning("{Event} " + "{Secret,8}", LogEvents.JobFinished);
        _logger.LogWarning($$"""{Email} {{order.Id}}""", order.Id);
        _logger.Error("{Event} {Iban} // not a comment", LogEvents.JobFinished);
        /* a block comment with "{Email}" */ _logger.LogWarning("{Event}", LogEvents.JobFinished);
        _logger.LogWarning("{Event} {MissingKeys} {OrderId}", LogEvents.JobFinished, a, b, order.Id);
        _logger.LogWarning($"{order.Email} is a hole of the string", order.Id);
        // _logger.LogWarning("{Email}", order.Id);
        _logger.LogWarning("{Event} {MissingKeysList}", LogEvents.JobFinished, order.Id);
    }

    [LoggerMessage(LogLevel.Warning, "{Event} {PhoneNumber}")]
    private static partial void LogPositional(ILogger logger, string @event, string phoneNumber);

    [LoggerMessageAttribute(Level = LogLevel.Warning, Message = "{Event} " + "{Address}")]
    private static partial void LogJoined(ILogger logger, string @event, string address);

    [LoggerMessage(
        EventId = 3,
        Level = LogLevel.Warning,
        Message = """
            {Event}
            {CardNumber}
            """)]
    private static partial void LogMultiLine(ILogger logger, string @event, string card);

    private static readonly Action<ILogger, string, Exception?> Defined = LoggerMessage.Define<string>(LogLevel.Warning, new EventId(4), "{Email}");

    private const string Stored = "{Event} {TokenValue}";

    private const string StoredClean = "{Event} {OrderId}";
}
