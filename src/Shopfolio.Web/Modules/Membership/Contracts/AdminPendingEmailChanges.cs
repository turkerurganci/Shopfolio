namespace Shopfolio.Modules.Membership.Contracts;

/// <summary>The administrator's pending address change as the Administration module reads it (06 4.40): the new address as entered and the moment the change was asked for.</summary>
public sealed record AdminPendingEmailChange(string NewEmail, string NormalizedNewEmail, DateTimeOffset CreatedAt);

/// <summary>
/// The Membership module's open surface for the administrator's pending address change (T379; 02 10.2.4; 06 4.40): the table is the Membership module's (05 3.3), the administrator's
/// account and the rules of the change are the Administration module's, so that module reads and writes the row only through this interface. At most one row per administrator. Every
/// member joins the caller's transaction and never saves it (09 6 item 3).
/// </summary>
public interface IAdminPendingEmailChanges
{
    /// <summary>The administrator's pending change; null when there is none.</summary>
    Task<AdminPendingEmailChange?> FindAsync(Guid adminId, CancellationToken ct);

    /// <summary>Writes the pending change; a row that exists takes the new address and the new moment (its life starts now). Returns true when an earlier row was replaced.</summary>
    Task<bool> SaveAsync(Guid adminId, string newEmail, DateTimeOffset now, CancellationToken ct);

    /// <summary>Deletes the administrator's pending change, if any.</summary>
    Task DeleteAsync(Guid adminId, CancellationToken ct);
}
