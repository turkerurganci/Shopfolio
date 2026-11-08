using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The administrator's side of the one password rule (<see cref="PasswordRegime"/>; 02 3.13.16): an administrator account has no verified state, so every account is eligible; the
/// code that checks, resets and changes the password is the customer's code.
/// </summary>
internal sealed class AdminPasswords : IPasswordAccounts
{
    private readonly AppDbContext _db;

    public AdminPasswords(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public AccountType Type => AccountType.Admin;

    public async Task<PasswordAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        var row = await _db.Admins.AsNoTracking()
            .Where(a => a.NormalizedEmail == normalizedEmail)
            .Select(a => new { a.Id, a.Email, a.PasswordHash })
            .SingleOrDefaultAsync(ct);
        return row is null ? null : new PasswordAccount(row.Id, row.Email, row.PasswordHash, IsVerified: true);
    }

    public async Task<PasswordAccount?> FindAsync(Guid id, CancellationToken ct)
    {
        var row = await _db.Admins.AsNoTracking()
            .Where(a => a.Id == id)
            .Select(a => new { a.Id, a.Email, a.PasswordHash })
            .SingleOrDefaultAsync(ct);
        return row is null ? null : new PasswordAccount(row.Id, row.Email, row.PasswordHash, IsVerified: true);
    }

    public async Task SetPasswordAsync(Guid id, string digest, CancellationToken ct)
    {
        await _db.Admins.Where(a => a.Id == id).ExecuteUpdateAsync(set => set.SetProperty(a => a.PasswordHash, digest), ct);
    }
}
