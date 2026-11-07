namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>
/// What one administrator operation writes into the trail (09 9.2 item 1). The area is not here: <see cref="AuditOperationText.AreaOf"/> derives it. The
/// administrator's id, name and email are not here either: they come from the signed-in administrator of the request.
/// </summary>
/// <param name="Operation">The operation's code.</param>
/// <param name="TargetRecordType">The code name of the target record's kind (06 3); empty when the operation has no target record (an export).</param>
/// <param name="TargetRecordId">The target record's id; empty exactly when the kind is.</param>
/// <param name="Details">What changed; <see langword="null"/> when the row needs no details.</param>
public sealed record AuditEntry(AuditOperation Operation, string? TargetRecordType, Guid? TargetRecordId, AuditDetails? Details);

/// <summary>The filter of the panel's audit list (02 10.3.4): a Turkey day range and one administrator. There is no search and no grouping.</summary>
/// <param name="From">The first Turkey day shown; <see langword="null"/> for no lower bound.</param>
/// <param name="To">The last Turkey day shown, the whole day included; <see langword="null"/> for no upper bound.</param>
/// <param name="AdminId">The administrator's id as written in the rows; <see langword="null"/> for every administrator.</param>
/// <param name="Page">The page asked for; the list clamps it to the last page.</param>
public sealed record AuditQuery(DateOnly? From = null, DateOnly? To = null, Guid? AdminId = null, int Page = 1);

/// <summary>One row of the panel's audit list: who, when, what changed (02 10.3.1). The operation is the stored code, recognized or not.</summary>
public sealed record AuditRow(
    Guid Id,
    DateTimeOffset At,
    Guid AdminId,
    string AdminName,
    string AdminEmail,
    AuditArea Area,
    string Operation,
    string? TargetRecordType,
    Guid? TargetRecordId,
    AuditDetails? Details);

/// <summary>One page of the audit list.</summary>
public sealed record AuditPage(IReadOnlyList<AuditRow> Items, int PageNumber, int PageSize, int TotalCount, int PageCount);

/// <summary>An entry of the administrator filter (04 9.23.3): an administrator who has rows in the trail, or still has an account; a removed one is listed by the name written in the rows.</summary>
public sealed record AuditAdminOption(Guid AdminId, string Name, string Email, bool IsRemoved);

/// <summary>
/// The operation trail (05 4.7; 09 9.2). The module owns the table: the trail is written only here, in the caller's transaction, and read only here. There is no
/// update and no delete: the application user has no privilege for them and the expired rows leave through the database function only (05 7.5.1).
/// </summary>
public interface IAuditTrail
{
    /// <summary>
    /// Adds one row to the caller's transaction (05 4.7.2): the operation happens with its row or not at all. The administrator is the signed-in one of the request; no
    /// administrator (a job, a command, a customer's request) is an <see cref="InvalidOperationException"/> - the system events and the installation commands
    /// write no row (02 10.3.3). An entry that breaks the rules of 09 9.2 item 4 (a customer's personal data, an old-new pair where none is allowed) is an
    /// <see cref="ArgumentException"/>; both are programming errors. The call outside a transaction is an <see cref="InvalidOperationException"/> too.
    /// </summary>
    Task WriteAsync(AuditEntry entry, CancellationToken ct);

    /// <summary>The panel's flat list, newest first, 25 rows a page (04 9.23.5, 9.23.6).</summary>
    Task<AuditPage> ListAsync(AuditQuery query, CancellationToken ct);

    /// <summary>The closed list of the administrator filter: the current administrators and the removed ones that left rows, each once.</summary>
    Task<IReadOnlyList<AuditAdminOption>> ListAdminsAsync(CancellationToken ct);
}
