using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Disposal;

/// <summary>
/// The expired rows of the audit trail (05 7.5.1; 06 9.3.3; 09 9.2 item 6; Z-30). The application user has no privilege to delete them: the one
/// way is the database function <c>dispose_expired_audit_logs</c>, which works out the period itself, clips the given moment to the database
/// clock and touches no row whose period is not over. The part calls it chunk by chunk and writes the number it returns.
/// </summary>
internal sealed class AuditLogDisposalPart : IDisposalPart
{
    public string Name => "AuditLog";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset asOf = run.Now;
        return run.ChunksAsync(
            DisposalRecordType.AuditLog,
            async (db, max, cancel) =>
            {
                List<int> removed = await db.Database
                    .SqlQuery<int>($"SELECT public.dispose_expired_audit_logs({max}, {asOf}) AS \"Value\"")
                    .ToListAsync(cancel);
                return removed.Single();
            },
            ct);
    }
}
