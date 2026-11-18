// Deliberate violation sample of the rejection line scan for the forms a string scan cannot type (T604 A3, D-1005 forms 2 and 3; 09 8.2 item 3, 18.2
// item 4): a variable that holds an error code handed to a log call (declared with the type, taken from an expression that names it, or named for it),
// and an anonymous scope object with a Code member. The scan reads this file as text; it is not compiled. Line numbers are part of the test.
internal sealed class ErrorCodeVariables
{
    public void Refuse(ErrorCode refusal, string reason)
    {
        var code = Refusals.Find(reason).ErrorCode;
        var plain = Refusals.Count(reason);
        _logger.LogWarning("{Event} {Reason}", "Anything", refusal);
        _logger.LogWarning("{Event} {Reason}", "Anything", code);
        _logger.LogWarning("{Event} {Reason}", "Anything", plain);
        _logger.LogWarning("{Event} {Reason}", "Anything", errorCode);
        using (_logger.BeginScope(new { Code = reason })) { }
        using (_logger.BeginScope(new { Reason = reason })) { }
    }
}
