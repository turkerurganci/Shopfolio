using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// One row of the audit trail (06 4.63): who, when, what changed. The row is only ever inserted: the application user has no update and delete
/// privilege on the table, and the expired rows are removed only by the database function <c>dispose_expired_audit_logs</c> (06 4.63; 09 9.2
/// items 6, 7; 09 10.8 item 3). Every value is a private setter; <see cref="Create"/> is the one way to make a row.
/// <para>
/// The row has <b>no link to any table</b> (06 2.2.5, 2.2.6): the administrator's id, name and email and the target record's kind and id are
/// values, so the rows of a removed administrator, a deleted account and a disposed record stay as they were written.
/// </para>
/// <para>
/// <see cref="Operation"/> is the code name of the operation as text, with no check constraint and no enum conversion: a code written by a
/// later version is read and shown as it is, never an error (09 9.2 item 2; D-48 a). <see cref="AuditOperationText"/> is the one place that
/// turns a code into its text and into its area.
/// </para>
/// </summary>
internal sealed class AuditLog
{
    public Guid Id { get; private set; }

    /// <summary>The moment of the operation: the row's creation moment (06 2.8.4).</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>The administrator's id as a value, not a link: the panel's administrator filter works on it (06 4.63).</summary>
    public Guid AdminId { get; private set; }

    public string AdminName { get; private set; } = string.Empty;

    public string AdminEmail { get; private set; } = string.Empty;

    /// <summary>The area of the operation (06 4.63): derived from the operation by <see cref="AuditOperationText.AreaOf"/>, never passed by the caller.</summary>
    public AuditArea AuditArea { get; private set; }

    /// <summary>The code name of the operation, as text (see the type's remarks).</summary>
    public string Operation { get; private set; } = string.Empty;

    /// <summary>The kind of the target record (06 3's code name); empty when the operation has no target record (an export).</summary>
    public string? TargetRecordType { get; private set; }

    /// <summary>The target record's id as a value; stays when the record is deleted or disposed.</summary>
    public Guid? TargetRecordId { get; private set; }

    /// <summary>
    /// "What changed" as a JSON list of values (06 2.5.1, 4.63), stored in a <c>jsonb</c> column. Never holds the personal data of a customer
    /// (09 9.2 item 4). The text is validated JSON; its typed form (<c>AuditDetails</c> and the shared <c>PayloadJson</c> setting, 09 12 item 5)
    /// arrives with the audit trail interface (T380) and does not change the column.
    /// </summary>
    public string? Details { get; private set; }

    public static AuditLog Create(
        AuditOperation operation,
        Guid adminId,
        string adminName,
        string adminEmail,
        string? targetRecordType,
        Guid? targetRecordId,
        string? details,
        DateTimeOffset now)
    {
        return new AuditLog
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            AdminId = adminId,
            AdminName = adminName,
            AdminEmail = adminEmail,
            AuditArea = AuditOperationText.AreaOf(operation),
            Operation = AuditOperationText.ToCode(operation),
            TargetRecordType = targetRecordType,
            TargetRecordId = targetRecordId,
            Details = details,
        };
    }
}

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(l => l.Id);
        builder.Property(l => l.AdminName).HasColumnType("text");
        builder.Property(l => l.AdminEmail).HasColumnType("text");
        builder.Property(l => l.Operation).HasColumnType("text");
        builder.Property(l => l.TargetRecordType).HasColumnType("text");
        builder.Property(l => l.Details).HasColumnType("jsonb");

        // No relationship to any table: the administrator and the target record are values (06 2.2.6).
        builder.HasIndex(l => l.CreatedAt);
        builder.HasIndex(l => new { l.AdminId, l.CreatedAt });
    }
}
