// Deliberate violation sample of the rejection line scan's wider forms (T411 criterion 5, D-89 K3; 09 8.2 item 3, 18.2 item 4): a code written under another
// property name, a value of the ErrorCode type handed to a log call, a template kept in a constant, a string with the comment marker in it. The scan reads
// this file as text; it is not compiled. Line numbers are part of the test.
internal sealed partial class RejectionLineForms
{
    private const string Kept = "{Event} {Code}";

    private readonly ILogger _logger;

    public void Refuse(ErrorCode code, string reason)
    {
        _logger.LogWarning("see https://example.test {Event} {ErrorCode}", "Anything", reason);
        _logger.LogWarning("{Event} {Reason}", "Anything", ErrorCode.InvalidInput);
        _logger.LogWarning("{Event} {Reason}", "Anything", nameof(ErrorCode.InvalidInput));
        _logger.LogWarning(message: "{Event} {RejectionCodeValue}", args: ["Anything", reason]);
        Log.ForContext("ErrorCode", reason).Warning("{Event}", "Anything");
        _logger.LogWarning("{Event}", "ValidationRejected");
        _logger.LogWarning("{Event} {CodePoint} {Fieldset} {StatusCode}", "Anything", 1, 2, 3);
        _logger.LogWarning($"'{code}' is a hole of the string", "Anything");
        // _logger.LogWarning("{Event} {Code}", LogEvents.ValidationRejected, ErrorCode.InvalidInput);
    }
}
