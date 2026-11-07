using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The operation trail (05 4.7; 09 9.2). The one class outside the database context that touches the <c>AuditLogs</c> set (architecture test). It only adds
/// rows and reads them: nothing here updates or deletes a row, and the database would refuse it if it did (05 7.5.1).
/// </summary>
internal sealed class AuditTrail : IAuditTrail
{
    private readonly AppDbContext _db;
    private readonly IAuditActorSource _actors;
    private readonly TimeProvider _time;

    public AuditTrail(AppDbContext db, IAuditActorSource actors, TimeProvider time)
    {
        _db = db;
        _actors = actors;
        _time = time;
    }

    public async Task WriteAsync(AuditEntry entry, CancellationToken ct)
    {
        AuditEntryRules.Validate(entry);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("An audit row is written in the caller's transaction and never alone (05 4.7.2; 09 9.2 item 5).");
        }

        AuditActor actor = await _actors.CurrentAsync(ct)
            ?? throw new InvalidOperationException("There is no signed-in administrator: the system events and the installation commands write no audit row (02 10.3.3; 09 9.2 item 1).");

        // The row is saved by the caller's transaction (09 9.1 item 3): it commits with the operation or goes with it.
        _db.AuditLogs.Add(AuditLog.Create(
            entry.Operation,
            actor.AdminId,
            actor.Name,
            actor.Email,
            string.IsNullOrWhiteSpace(entry.TargetRecordType) ? null : entry.TargetRecordType,
            entry.TargetRecordId,
            entry.Details,
            _time.GetUtcNow()));
    }

    public async Task<AuditPage> ListAsync(AuditQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        IQueryable<AuditLog> rows = _db.AuditLogs.AsNoTracking();

        // The range is Turkey days (02 4.1.1): the first day from its 00:00, the last day up to the next day's 00:00 (09 7.1 item 3).
        if (query.From is { } from)
        {
            DateTimeOffset start = TurkeyTime.StartOf(from);
            rows = rows.Where(l => l.CreatedAt >= start);
        }

        if (query.To is { } to)
        {
            DateTimeOffset end = TurkeyTime.StartOf(to.AddDays(1));
            rows = rows.Where(l => l.CreatedAt < end);
        }

        if (query.AdminId is { } adminId)
        {
            rows = rows.Where(l => l.AdminId == adminId);
        }

        // Newest first, ended with the inner key (09 10.5 item 1); there is no other order (04 9.23.6).
        PagedList<AuditLog> page = await rows
            .OrderByDescending(l => l.CreatedAt)
            .ThenByDescending(l => l.Id)
            .ToPageAsync(query.Page, PageSizes.Panel, ct);

        return new AuditPage([.. page.Items.Select(ToRow)], page.PageNumber, page.PageSize, page.TotalCount, page.PageCount);
    }

    public async Task<IReadOnlyList<AuditAdminOption>> ListAdminsAsync(CancellationToken ct)
    {
        // The name written in the newest row of each administrator in the trail (a removed administrator keeps it, 02 10.3.5).
        var written = await _db.AuditLogs.AsNoTracking()
            .GroupBy(l => l.AdminId)
            .Select(g => new
            {
                AdminId = g.Key,
                Name = g.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id).Select(l => l.AdminName).First(),
                Email = g.OrderByDescending(l => l.CreatedAt).ThenByDescending(l => l.Id).Select(l => l.AdminEmail).First(),
            })
            .ToListAsync(ct);
        var current = await _db.Admins.AsNoTracking().Select(a => new { a.Id, a.Name, a.Email }).ToListAsync(ct);

        Dictionary<Guid, AuditAdminOption> options = written.ToDictionary(w => w.AdminId, w => new AuditAdminOption(w.AdminId, w.Name, w.Email, IsRemoved: true));
        foreach (var admin in current)
        {
            options[admin.Id] = new AuditAdminOption(admin.Id, admin.Name, admin.Email, IsRemoved: false);
        }

        return [.. options.Values.OrderBy(o => o.IsRemoved).ThenBy(o => o.Name, StringComparer.Create(RequestFormat.Culture, ignoreCase: true)).ThenBy(o => o.AdminId)];
    }

    private static AuditRow ToRow(AuditLog log)
    {
        return new AuditRow(
            log.Id, log.CreatedAt, log.AdminId, log.AdminName, log.AdminEmail, log.AuditArea, AuditOperationText.Display(log.Operation), log.TargetRecordType, log.TargetRecordId, log.Details);
    }
}
