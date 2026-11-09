using Microsoft.EntityFrameworkCore;

namespace Shopfolio.Infrastructure.Backup;

/// <summary>One table of the order side and the rows of it that transactions committed after the restore point added or changed.</summary>
internal sealed record RollbackTableCount(string Table, long Rows);

/// <summary>What the check found: whether the database records commit moments at all, and the counts per table (empty when it does not).</summary>
internal sealed record RollbackCount(bool CommitMomentsRecorded, IReadOnlyList<RollbackTableCount> Tables)
{
    public long Total => Tables.Sum(t => t.Rows);

    /// <summary>The rollback is allowed only when commit moments are recorded and nothing was written on the order side after the point (05 8.5.11, 8.7.4).</summary>
    public bool AllowsRollback => CommitMomentsRecorded && Total == 0;
}

/// <summary>
/// The rollback check (05 8.5.11, 8.7.4; K-962, K-1096): before the database is returned to an earlier moment, it counts - in every table of the order side that <c>02 3.34.8</c> names and in
/// the inbound notification record - the rows that transactions committed after the restore point added or changed. The measure is the transaction's commit moment
/// (<c>pg_xact_commit_timestamp(xmin)</c>, which needs <c>track_commit_timestamp</c> on), not the row's creation time: a transaction that began before the point and committed after it
/// shows. A row's <c>xmin</c> is the last transaction that wrote it, so an insert and an update count alike, and the emptying and the link updates that a deletion writes on the order side
/// are updates and count. The check only reads: it runs with the application stopped and writes nothing. A nonzero count means the database is not returned (05 8.7.4).
/// </summary>
internal sealed class RollbackAudit
{
    // The tables of 02 3.34.8 (the 06 map row of that rule) with the rows that hang on them: the order and its items, the item-level records, the payments and refunds and the transition
    // record, the contact requests, the frozen legal text versions, the checkbox acceptances, the order's histories and internal notes, the sent record of the order's notices, the
    // operation trail and the inbound notification record. Never a table of the catalog, the content, the settings or the accounts: those writes go back with the restore (05 8.7.4).
    // The sent record counts only rows that belong to an order (Z-43; K-983): the account e-mails' rows are not the order side.
    private static readonly (string Table, string Filter)[] _tables =
    [
        ("orders", string.Empty),
        ("order_items", string.Empty),
        ("cancellations", string.Empty),
        ("item_removals", string.Empty),
        ("late_delivery_terminations", string.Empty),
        ("withdrawals", string.Empty),
        ("returns", string.Empty),
        ("return_rejections", string.Empty),
        ("not_returned_closures", string.Empty),
        ("defect_claims", string.Empty),
        ("payment_records", string.Empty),
        ("refunds", string.Empty),
        ("refund_records", string.Empty),
        ("transition_records", string.Empty),
        ("contact_requests", string.Empty),
        ("legal_text_versions", string.Empty),
        ("checkbox_acceptances", string.Empty),
        ("checkbox_acceptance_order_items", string.Empty),
        ("order_address_histories", string.Empty),
        ("order_email_histories", string.Empty),
        ("internal_notes", string.Empty),
        ("dispatch_records", " AND order_id IS NOT NULL"),
        ("audit_logs", string.Empty),
        ("inbound_notification_records", string.Empty),
    ];

    private readonly AppDbContext _db;

    public RollbackAudit(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <summary>The tables the check counts, in the order it reports them.</summary>
    public static IReadOnlyList<string> Tables { get; } = _tables.Select(t => t.Table).ToArray();

    /// <summary>Counts the rows committed after <paramref name="restorePoint"/>; reads only.</summary>
    public async Task<RollbackCount> CountAfterAsync(DateTimeOffset restorePoint, CancellationToken ct)
    {
        // Without the setting the commit moments are not kept and the function raises an error: the check says so instead of counting nothing.
        List<string> setting = await _db.Database.SqlQuery<string>($"SELECT current_setting('track_commit_timestamp') AS \"Value\"").ToListAsync(ct);
        if (!string.Equals(setting.SingleOrDefault(), "on", StringComparison.Ordinal))
        {
            return new RollbackCount(false, []);
        }

        List<RollbackTableCount> counts = [];
        foreach ((string table, string filter) in _tables)
        {
            // The names are the constants above, never a value from outside; the moment is a parameter.
            string statement = $"SELECT count(*) AS \"Value\" FROM \"{table}\" WHERE pg_xact_commit_timestamp(xmin) > {{0}}{filter}";
            List<long> rows = await _db.Database.SqlQueryRaw<long>(statement, restorePoint.ToUniversalTime()).ToListAsync(ct);
            counts.Add(new RollbackTableCount(table, rows.Single()));
        }

        return new RollbackCount(true, counts);
    }
}
