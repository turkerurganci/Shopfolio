using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Modules.Membership;

/// <summary>The administrator's pending address change row (06 4.40) for the Administration module's change flow (T379); the rules of the flow are that module's.</summary>
internal sealed class AdminPendingEmailChanges : IAdminPendingEmailChanges
{
    private readonly AppDbContext _db;

    public AdminPendingEmailChanges(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task<AdminPendingEmailChange?> FindAsync(Guid adminId, CancellationToken ct)
    {
        return await _db.PendingEmailChanges.AsNoTracking()
            .Where(p => p.AdminId == adminId)
            .Select(p => new AdminPendingEmailChange(p.NewEmail, p.NormalizedNewEmail, p.CreatedAt))
            .SingleOrDefaultAsync(ct);
    }

    public async Task<bool> SaveAsync(Guid adminId, string newEmail, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(newEmail);
        RequireTransaction();
        PendingEmailChange? pending = await _db.PendingEmailChanges.SingleOrDefaultAsync(p => p.AdminId == adminId, ct);
        if (pending is null)
        {
            _db.PendingEmailChanges.Add(PendingEmailChange.ForAdmin(adminId, newEmail, now));
            return false;
        }

        pending.SetNewEmail(newEmail);
        pending.CreatedAt = now;
        return true;
    }

    public async Task DeleteAsync(Guid adminId, CancellationToken ct)
    {
        RequireTransaction();
        await _db.PendingEmailChanges.Where(p => p.AdminId == adminId).ExecuteDeleteAsync(ct);
    }

    private void RequireTransaction()
    {
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The pending change is written only inside a transaction (09 6 item 3).");
        }
    }
}
