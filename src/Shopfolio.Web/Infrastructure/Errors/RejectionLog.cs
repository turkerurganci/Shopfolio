using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Validation;

namespace Shopfolio.Infrastructure.Errors;

/// <summary>
/// The one writer of the rejection line (09 8.2 item 3, 18.2 item 4; 07 6.1.3; D-132): a request that is refused - a field validation, a forgery check,
/// a named business-rule refusal - writes exactly one line, with the code of the refusal or, for a field validation, the <c>fields</c> array of the
/// rejected fields' names and codes. The line carries no value of any field (05 9.1.5) and never the code of an unexpected error: those lines are the
/// catching layer's (09 8.1 item 3). A successful request writes no line. The places that refuse - the validation page filter, the forgery filter and,
/// from the tasks that need it, the helper that turns a refusal into the page's result - call this class and write no line of their own; a source scan
/// keeps the event and the code field out of every other file. The category of the line is this class's.
/// </summary>
internal sealed partial class RejectionLog
{
    private readonly ILogger<RejectionLog> _logger;

    public RejectionLog(ILogger<RejectionLog> logger)
    {
        _logger = logger;
    }

    /// <summary>The line of one refusal: the code and nothing else of the request (the request's own fields come from the log enricher).</summary>
    public void Rejected(ErrorCode code)
    {
        if (code is ErrorCode.UnexpectedPageError or ErrorCode.UnexpectedOperationError or ErrorCode.OutcomeUncertain)
        {
            throw new ArgumentOutOfRangeException(nameof(code), code, "The three codes of the unexpected-error states are written by the catching layer only (09 8.1 item 3).");
        }

        LogRejected(_logger, LogEvents.ValidationRejected, code.ToString());
    }

    /// <summary>
    /// The line of the general rate limit's refusal (429; 09 18.2 item 3): the event is the rate limit's own and the line carries the code
    /// <see cref="ErrorCode.RateLimitExceeded"/>; it is no unexpected error and the error counter does not see it (05 9.4.8).
    /// </summary>
    public void RateLimited()
    {
        LogRejected(_logger, LogEvents.RateLimitExceeded, nameof(ErrorCode.RateLimitExceeded));
    }

    /// <summary>The line of a field validation: one line for the request with every rejected field's name and code (04 7.3.5; 09 18.2 item 4).</summary>
    public void FieldsRejected(IReadOnlyList<FieldRejection> fields)
    {
        ArgumentNullException.ThrowIfNull(fields);
        if (fields.Count == 0)
        {
            throw new ArgumentException("A field validation line carries at least one rejected field.", nameof(fields));
        }

        LogFieldsRejected(_logger, LogEvents.ValidationRejected, fields);
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event} {Code}")]
    private static partial void LogRejected(ILogger logger, string @event, string code);

    [LoggerMessage(Level = LogLevel.Warning, Message = "{Event} {@Fields}")]
    private static partial void LogFieldsRejected(ILogger logger, string @event, IReadOnlyList<FieldRejection> fields);
}
