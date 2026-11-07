using System.Reflection;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// The closed list of log event names (09 18.2 item 3). A free text never goes into the event field: the formatter writes an event only when
/// it is one of these constants, and a source scan keeps a call site from passing anything else. A new event name is added here.
/// </summary>
internal static class LogEvents
{
    /// <summary>A beyond-expectation failure that the exception handler logged (09 8.1; 05 10.3.6).</summary>
    public const string UnexpectedError = "UnexpectedError";

    /// <summary>A request was refused for want of permission (05 9.1.3).</summary>
    public const string AccessDenied = "AccessDenied";

    /// <summary>A request was refused by the rate limit (05 9.1.3).</summary>
    public const string RateLimitExceeded = "RateLimitExceeded";

    /// <summary>The thinned line of a signature failure: the first request of an address writes a line, the later ones of the 60 second window are only counted (05 4.5.2).</summary>
    public const string SignatureFailureSampled = "SignatureFailureSampled";

    /// <summary>The thinned line of a customer notification refused by the per-recipient sending limit: the first refusal writes a line, the later ones of the 60 second window are only counted; no address (08 2.2.4).</summary>
    public const string RecipientLimitExceededSampled = "RecipientLimitExceededSampled";

    /// <summary>A request was rejected by validation; the line carries a single code or the <c>fields</c> array (09 18.2 item 4).</summary>
    public const string ValidationRejected = "ValidationRejected";

    /// <summary>A background job started (05 9.1.3).</summary>
    public const string JobStarted = "JobStarted";

    /// <summary>A background job finished (05 9.1.3).</summary>
    public const string JobFinished = "JobFinished";

    /// <summary>The runner found pending rows of a job type it has no handler for and left them alone; written once at the runner's start (09 13.1 item 3).</summary>
    public const string JobTypeUnknown = "JobTypeUnknown";

    /// <summary>An instant deletion was committed; written by the audit writer, with the kind and the internal id of the record (09 18.2 item 5).</summary>
    public const string InstantDeletion = "InstantDeletion";

    /// <summary>An administration command ran; written by the audit writer in the command process, with the command and the internal id of the target account (09 18.2 item 5).</summary>
    public const string AdminCommand = "AdminCommand";

    /// <summary>The start-up settings check refused the start or closed a conditional line (05 8.4.6).</summary>
    public const string StartupSettingsChecked = "StartupSettingsChecked";

    /// <summary>The start-up data check refused the start or did not run because the database was out of reach (06 8.2.1).</summary>
    public const string StartupDatabaseChecked = "StartupDatabaseChecked";

    /// <summary>The liveness probe's database check ran out of time (09 10.7 item 6).</summary>
    public const string DbTimeout = "DbTimeout";

    /// <summary>A call of the outside services' link layer ended without a classified answer; the line carries the job, the kind and the reason code only (08 3.1 rule 5).</summary>
    public const string ExternalCallFailed = "ExternalCallFailed";

    /// <summary>Every event name of the list, read from the constants above so the list and its members cannot drift apart.</summary>
    public static IReadOnlyList<string> All { get; } = typeof(LogEvents)
        .GetFields(BindingFlags.Public | BindingFlags.Static)
        .Where(field => field.IsLiteral && field.FieldType == typeof(string))
        .Select(field => (string)field.GetRawConstantValue()!)
        .ToArray();

    public static bool IsKnown(string? name) => name is not null && All.Contains(name, StringComparer.Ordinal);
}
