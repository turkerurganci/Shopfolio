namespace Shopfolio.Infrastructure.Logging;

/// <summary>
/// A line of the audit log or the command log (09 18.1 item 2, 18.2 item 5). Only the two shapes exist, and neither can carry a field value: the
/// parts are the closed event name, a kind or a command name, and internal ids (<see cref="Guid"/>) — an e-mail address or any other value has no
/// parameter to go into.
/// </summary>
internal sealed record AuditLine
{
    private AuditLine(string eventName, string template, string nameKey, string name, string idKey, Guid id)
    {
        EventName = eventName;
        Template = template;
        NameKey = nameKey;
        Name = name;
        IdKey = idKey;
        Id = id;
    }

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
