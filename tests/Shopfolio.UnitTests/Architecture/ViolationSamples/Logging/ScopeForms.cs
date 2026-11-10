// Deliberate violation sample of the scope and enricher forms (T411, D-89 K1; 09 18.2 item 2a): a property name of personal data or a request field name
// handed to a scope, a context or an enricher. The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class ScopeForms
{
    private readonly ILogger _logger;

    public void Run(string rawPath, string value, Guid id)
    {
        using IDisposable? first = _logger.BeginScope(new Dictionary<string, object> { ["Email"] = value });
        using IDisposable? second = _logger.BeginScope("{PhoneNumber}", value);
        Log.ForContext("Path", rawPath).Warning("{Event}", LogEvents.JobFinished);
        using IDisposable pushed = LogContext.PushProperty("CardNumber", value);
        configuration.Enrich.WithProperty("Token", value);
        using IDisposable? clean = _logger.BeginScope(new Dictionary<string, object> { ["OrderId"] = id });
        Log.ForContext("JobId", id).Warning("{Event}", LogEvents.JobFinished);
        Log.ForContext<ScopeForms>().Warning("{Event}", LogEvents.JobFinished);
        // Log.ForContext("Email", value) in a comment is nothing.
    }
}
