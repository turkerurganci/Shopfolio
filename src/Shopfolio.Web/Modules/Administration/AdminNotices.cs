using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The Administration module's side of <see cref="IAdminNotices"/>: the acting administrator from the request, the address of every administrator account from the module's own
/// table, the row through the Notifications module (08 2.2.7). It writes nothing else.
/// </summary>
internal sealed class AdminNotices : IAdminNotices
{
    private readonly AppDbContext _db;
    private readonly IAuditActorSource _actors;
    private readonly INotificationsModule _notifications;
    private readonly TimeProvider _time;

    public AdminNotices(AppDbContext db, IAuditActorSource actors, INotificationsModule notifications, TimeProvider time)
    {
        _db = db;
        _actors = actors;
        _notifications = notifications;
        _time = time;
    }

    public async Task<int> QueueIbanChangedAsync(string newIban, bool wasFirstEntry, CancellationToken ct)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newIban);
        if (await _actors.CurrentAsync(ct) is not { } actor)
        {
            return 0;
        }

        List<string> addresses = await _db.Admins.AsNoTracking().OrderBy(a => a.Id).Select(a => a.Email).ToListAsync(ct);
        return await _notifications.QueueAdminNoticeAsync(IbanChangedNotice.For(newIban, actor.Name, wasFirstEntry, _time.GetUtcNow()), addresses, ct);
    }
}
