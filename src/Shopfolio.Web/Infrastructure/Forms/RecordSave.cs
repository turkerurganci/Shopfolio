using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The result of a record variant's save (07 2.4.5; 09 10.2 item 2): saved, or refused because the record's version is no longer the version the
/// form carried. The refusal is a return value, not an exception (09 8.1): the transaction runner rolls back and hands it to the page, which shows
/// the conflict (<see cref="FormResults.Conflict"/>). The code the refusal carries on the wire is <c>RecordVersionConflict</c> (07 6.2.2); the error
/// code registry that holds it is T213's.
/// </summary>
internal sealed class RecordSaveResult : IOperationResult
{
    private RecordSaveResult(bool isSaved)
    {
        IsSaved = isSaved;
    }

    public static RecordSaveResult Saved { get; } = new(true);

    public static RecordSaveResult VersionConflict { get; } = new(false);

    public bool IsSaved { get; }

    public bool IsRejected => !IsSaved;
}

/// <summary>
/// The save of the record variant (05 7.3.1; MK-20; 09 10.2 item 2) - the one sequence every edit form of a record with a version follows: the
/// record is loaded tracked inside the transaction, the version the form carried is compared with the loaded one (the early refusal), the values
/// are applied, the version is raised by one, and <c>SaveChanges</c> sends the single conditional update (<c>UPDATE ... SET ..., version = version + 1
/// WHERE id = @id AND version = @loaded</c>): EF puts the loaded version in the condition because the version column is a concurrency token. The
/// early comparison is the refusal; the token is the guarantee against two saves that arrive together: the second one affects no row and is
/// refused the same way. There is no read between the comparison and the write that could be trusted - and no lock (05 7.3.1).
/// <para>
/// "Save anyway" (07 2.4.5) is not a second code path: the re-displayed form carries the record's current version, and its send is this same save. If
/// one more change was made in between, the version differs again and the warning comes again.
/// </para>
/// <para>
/// The record must be tracked: a product's version covers its variants and links, and the product version guard looks only at a tracked product
/// (D-95 (c)); an untracked record would let a form record be written without the raise being seen, so it is a programming error here.
/// </para>
/// </summary>
internal static class RecordSave
{
    /// <summary>
    /// Compares, applies, raises and saves. <paramref name="apply"/> writes the form's values to the tracked record (and to the member records
    /// loaded with it); it does not touch the version.
    /// </summary>
    public static async Task<RecordSaveResult> SaveAsync<TRecord>(
        AppDbContext db,
        TRecord record,
        int formVersion,
        Action<TRecord> apply,
        CancellationToken ct)
        where TRecord : class, IVersionedRecord
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(record);
        ArgumentNullException.ThrowIfNull(apply);
        if (db.Entry(record).State == EntityState.Detached)
        {
            throw new InvalidOperationException("The record of a form save is loaded tracked, with the rows its version covers (09 10.2 item 2; D-95).");
        }

        if (record.Version != formVersion)
        {
            return RecordSaveResult.VersionConflict;
        }

        apply(record);
        record.BumpVersion();
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Two saves of the same version arrived together; the second affected no row (09 10.2 item 2: the only place this exception is a refusal).
            return RecordSaveResult.VersionConflict;
        }

        return RecordSaveResult.Saved;
    }
}

/// <summary>
/// The administrator's order operation compares the version the screen showed with the order's version after the row lock and the load (05 7.3.2;
/// 09 10.2 item 3). The customer's operations carry no version (07 2.4.2): they check the transition's condition against the order as it is now.
/// A version that does not match applies nothing and shows the order as it is (07 2.4.6) - there is no "save anyway" for an order.
/// </summary>
internal static class ShownVersion
{
    /// <summary>True when the order, loaded after its row lock, still has the version the administrator's form showed.</summary>
    public static bool Matches(IVersionedRecord order, int shown)
    {
        ArgumentNullException.ThrowIfNull(order);
        return order.Version == shown;
    }

    /// <summary>
    /// The one place that says what an order operation does with the version it carries (T364 KK4; 09 10.2 item 3; 02 3.31.2): the administrator's
    /// operation always carries the version its screen showed - an administrator's request without one is <see cref="ShownVersionCheck.Missing"/> and
    /// applies nothing, never a request that skips the comparison - and is compared through <see cref="Matches"/>. The customer's and the system's
    /// operations carry none (07 2.4.2) and are checked against the order as it is now; their entry points refuse a version they were sent.
    /// </summary>
    public static ShownVersionCheck Check(IVersionedRecord order, int? shown, bool byAdministrator)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (!byAdministrator)
        {
            return ShownVersionCheck.Current;
        }

        if (shown is not { } version)
        {
            return ShownVersionCheck.Missing;
        }

        return Matches(order, version) ? ShownVersionCheck.Current : ShownVersionCheck.Changed;
    }
}

/// <summary>The outcome of <see cref="ShownVersion.Check"/>: the operation goes on, the administrator's request carried no version, or the order changed since the screen.</summary>
internal enum ShownVersionCheck
{
    Current,
    Missing,
    Changed,
}
