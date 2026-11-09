using Shopfolio.Infrastructure.LogRead;

namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// A line of the audit log or the command log (09 18.1 item 2, 18.2 item 5). Only these shapes exist, and none can carry a field value: the
/// parts are the closed event name, a kind or a command name, internal ids (<see cref="Guid"/>) and, for the read command, closed enum names and
/// days (<see cref="ReadLogs"/>) — an e-mail address, an IP or any other value has no parameter to go into.
/// </summary>
internal sealed record AuditLine
{
    private AuditLine(string eventName, string template, string nameKey, string name, string idKey, Guid id)
        : this(eventName, template, nameKey, name, idKey, id, [])
    {
    }

    private AuditLine(string eventName, string template, string nameKey, string name, string idKey, Guid id, IReadOnlyList<KeyValuePair<string, string>> closedValues)
    {
        EventName = eventName;
        Template = template;
        NameKey = nameKey;
        Name = name;
        IdKey = idKey;
        Id = id;
        ClosedValues = closedValues;
    }

    /// <summary>
    /// The further properties of the read command's line (property name in PascalCase, value): only an enum's name or an ISO day, made here from typed values; empty for the other
    /// shapes.
    /// </summary>
    public IReadOnlyList<KeyValuePair<string, string>> ClosedValues { get; }

    /// <summary>Whether the writer waits until the line is on the disk (<c>FileStream.Flush(true)</c>): the read command's line, before anything is read (05 9.3.4; 09 13.2 item 5).</summary>
    public bool FlushToDisk => ClosedValues.Count > 0;

    /// <summary>The closed event name (<see cref="LogEvents"/>).</summary>
    public string EventName { get; }

    /// <summary>The message template: the event, the name and the id.</summary>
    public string Template { get; }

    /// <summary>The property name (PascalCase) of the kind or command, and the name itself.</summary>
    public string NameKey { get; }

    public string Name { get; }

    /// <summary>The property name (PascalCase) of the internal id, and the id itself.</summary>
    public string IdKey { get; }

    public Guid Id { get; }

    /// <summary>An instant deletion: its kind (<c>kind</c>) and the internal id of the deleted record (<c>recordId</c>); no field value.</summary>
    public static AuditLine InstantDeletion(string kind, Guid recordId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        return new AuditLine(LogEvents.InstantDeletion, "{Event} {Kind} {RecordId}", "Kind", kind, "RecordId", recordId);
    }

    /// <summary>An administration command: its name (<c>command</c>) and the internal id of the target account (<c>accountId</c>); no e-mail address.</summary>
    public static AuditLine AdminCommand(string command, Guid accountId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        return new AuditLine(LogEvents.AdminCommand, "{Event} {Command} {AccountId}", "Command", command, "AccountId", accountId);
    }

    /// <summary>
    /// An administration command with no target account: the command that names an account by e-mail and finds none, and the commands that act on no account (09 13.2 item 5).
    /// The id is the empty id (<see cref="Guid.Empty"/>) - "no target" - so the line has the same shape as any other and still holds no e-mail address.
    /// </summary>
    public static AuditLine AdminCommandWithoutTarget(string command) => AdminCommand(command, Guid.Empty);

    /// <summary>
    /// The read command's line (05 8.5.5, 9.3.4; 09 13.2 item 5): the command, the account the filter names (its internal id; the empty id when the filter names no account), the source,
    /// the filter's kind and the days of the range. The filter's value (an event type, a request id, an IP or an e-mail) is never written: only its kind (D-740).
    /// </summary>
    public static AuditLine ReadLogs(string command, Guid accountId, LogSource source, LogFilterKind filter, DateOnly from, DateOnly to)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        return new AuditLine(
            LogEvents.AdminCommand,
            "{Event} {Command} {AccountId} {LogSource} {FilterKind} {FromDay} {ToDay}",
            "Command",
            command,
            "AccountId",
            accountId,
            [
                new("LogSource", source.ToString()),
                new("FilterKind", filter.ToString()),
                new("FromDay", from.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)),
                new("ToDay", to.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)),
            ]);
    }
}

/// <summary>
/// The one way the audit line and the command line are written (09 18.1 item 2). The line is delivered to the operating system when the call
/// returns (09 18.1 item 5); a write that fails <b>throws to the caller</b> — the caller treats it as an unexpected state (09 8.1). The application
/// process builds the writer on the <c>audit-</c> file, the command process on the <c>command-</c> file (<see cref="LoggingSetup.CreateAuditLineWriter"/>).
/// </summary>
internal interface IAuditLineWriter
{
    void Write(AuditLine line);
}
