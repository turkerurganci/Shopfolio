using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Membership;

/// <summary>The customer's side of the one password rule (<see cref="PasswordRegime"/>): finds the account and writes its digest.</summary>
internal sealed class CustomerPasswords : IPasswordAccounts
{
    private readonly AppDbContext _db;

    public CustomerPasswords(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public AccountType Type => AccountType.Customer;

    public async Task<PasswordAccount?> FindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        var row = await _db.Customers.AsNoTracking()
            .Where(c => c.NormalizedEmail == normalizedEmail)
            .Select(c => new { c.Id, c.Email, c.PasswordHash, c.EmailVerifiedAt })
            .SingleOrDefaultAsync(ct);
        return row is null ? null : new PasswordAccount(row.Id, row.Email, row.PasswordHash, row.EmailVerifiedAt is not null);
    }

    public async Task<PasswordAccount?> LockAndFindByEmailAsync(string normalizedEmail, CancellationToken ct)
    {
        return await AccountLocks.LockByEmailAsync(_db, AccountType.Customer, normalizedEmail, ct) is { } id ? await FindAsync(id, ct) : null;
    }

    public Task<bool> LockAsync(Guid id, CancellationToken ct) => AccountLocks.LockAsync(_db, AccountType.Customer, id, ct);

    public async Task<PasswordAccount?> FindAsync(Guid id, CancellationToken ct)
    {
        var row = await _db.Customers.AsNoTracking()
            .Where(c => c.Id == id)
            .Select(c => new { c.Id, c.Email, c.PasswordHash, c.EmailVerifiedAt })
            .SingleOrDefaultAsync(ct);
        return row is null ? null : new PasswordAccount(row.Id, row.Email, row.PasswordHash, row.EmailVerifiedAt is not null);
    }

    /// <summary>
    /// Empties the digest: the password is unusable until the password reset writes a new one (T519; 02 3.13.14; 06 4.35). The caller holds the account's row lock; it is asked again here, like
    /// <see cref="SetPasswordAsync"/>, so the lock is always taken before the digest is written.
    /// </summary>
    public async Task InvalidatePasswordAsync(Guid id, CancellationToken ct)
    {
        await AccountLocks.LockAsync(_db, AccountType.Customer, id, ct);
        await _db.Customers.Where(c => c.Id == id).ExecuteUpdateAsync(set => set.SetProperty(c => c.PasswordHash, (string?)null), ct);
    }

    public async Task SetPasswordAsync(Guid id, string digest, CancellationToken ct)
    {
        await AccountLocks.LockAsync(_db, AccountType.Customer, id, ct);
        await _db.Customers.Where(c => c.Id == id).ExecuteUpdateAsync(set => set.SetProperty(c => c.PasswordHash, digest), ct);
    }
}
