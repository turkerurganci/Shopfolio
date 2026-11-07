using System.Text.Json.Serialization;

namespace Shopfolio.Modules.Administration.Contracts;

/// <summary>One changed field of an audit row: the field's name and its old and new value (02 10.3.1 (6); 06 4.63).</summary>
public sealed record AuditValueChange(string Field, string? OldValue, string? NewValue);

/// <summary>One value of an operation that is no old-new pair: a reason, a date (06 4.63).</summary>
public sealed record AuditValue(string Name, string? Value);

/// <summary>
/// "What changed" of an audit row (06 4.63; 09 9.2 item 4), stored as a <c>jsonb</c> list of values: the display name of the subject (an order number, the
/// name of a product, the name of a setting, the address an invitation was sent to), the old and new values of the changed fields and the operation's own
/// values. It never holds the personal data of a customer: <see cref="IAuditTrail.WriteAsync"/> refuses an entry whose details do.
/// <para>
/// The form only grows (09 12 item 5): a new member is added at the end, optional, so that a row written by an older version is read by a newer one.
/// </para>
/// </summary>
public sealed record AuditDetails
{
    [JsonConstructor]
    public AuditDetails(string? displayName = null, IReadOnlyList<AuditValueChange>? changes = null, IReadOnlyList<AuditValue>? values = null)
    {
        DisplayName = displayName;
        Changes = changes ?? [];
        Values = values ?? [];
    }

    /// <summary>The subject's display name; empty when the row has none.</summary>
    public string? DisplayName { get; }

    /// <summary>The changed fields with their old and new values; empty when the operation changes no field.</summary>
    public IReadOnlyList<AuditValueChange> Changes { get; }

    /// <summary>The operation's own values.</summary>
    public IReadOnlyList<AuditValue> Values { get; }
}
