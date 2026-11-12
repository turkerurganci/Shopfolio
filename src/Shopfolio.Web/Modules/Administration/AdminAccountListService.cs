using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Notifications.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The accounts screen's read side (T522; 07 UC-103; 04 9.20; 05 3.3.9): the administrators in the alphabetical order of their names (the database orders in the Turkish collation, 06 2.8), and the valid
/// invitations, newest first. An invitation is valid when none of its three closing moments is filled and a link of it still lives - used, replaced and run-out links are not live; this is the reading the
/// removal's confirmation counts with (<c>AdminAccounts.LiveLink</c>, the same link reading as the removal's confirmation and <c>LinkRecords.StateOf</c>). The marks of the page's invitations are one read of the Notifications module, so the number of reads does not grow with the list (09 20 item 4).
/// </summary>
internal sealed class AdminAccountListService : IAdminAccountList
{
    private readonly AppDbContext _db;
    private readonly INotificationsModule _notifications;
    private readonly TimeProvider _time;

    public AdminAccountListService(AppDbContext db, INotificationsModule notifications, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _notifications = notifications;
        _time = time;
    }

    public async Task<AdminAccountList> ListAsync(CancellationToken ct)
    {
        DateTimeOffset now = _time.GetUtcNow();
        List<AdminAccountRow> admins = await _db.Admins.AsNoTracking()
            .OrderBy(a => a.Name).ThenBy(a => a.Id)
            .Select(a => new AdminAccountRow(a.Id, a.Name, a.Email))
            .ToListAsync(ct);

        var invitations = await _db.AdminInvitations.AsNoTracking()
            .Where(i => i.WithdrawnAt == null && i.InvalidatedAt == null && i.AcceptedAt == null
                && _db.LinkRecords.Where(AdminAccounts.LiveLink(now)).Any(l => l.AdminInvitationId == i.Id))
            .OrderByDescending(i => i.CreatedAt).ThenBy(i => i.Id)
            .Select(i => new
            {
                i.Id,
                i.Email,
                i.SenderAdminName,
                i.CreatedAt,
                LinkEndsAt = _db.LinkRecords.Where(AdminAccounts.LiveLink(now)).Where(l => l.AdminInvitationId == i.Id).Max(l => l.ExpiresAt),
            })
            .ToListAsync(ct);

        IReadOnlySet<Guid> marked = invitations.Count == 0
            ? new HashSet<Guid>()
            : await _notifications.FindAdminInvitationsWithUndeliveredMarkAsync([.. invitations.Select(i => i.Id)], ct);
        return new AdminAccountList(
            admins,
            [.. invitations.Select(i => new AdminInvitationRow(i.Id, i.Email, i.SenderAdminName, i.CreatedAt, i.LinkEndsAt, marked.Contains(i.Id)))]);
    }
}
