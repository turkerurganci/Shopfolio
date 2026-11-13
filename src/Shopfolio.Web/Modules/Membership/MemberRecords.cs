using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// The member record view's reading (02 3.15.5; 07 UC-95; 04 9.12): the customer account found by its e-mail, whole and in its normalized form, so that the case of the letters does not matter (06 4.36
/// <c>NormalizedEmail</c>). It only reads - there is no member-record edit and no export of the account's data in the application (02 3.15.5; K-119): the firm answers a request from this view.
/// Administrator accounts are not searched (02 10.2.5). The reading is not written to the trail (02 10.3.1).
/// </summary>
internal sealed class MemberRecords
{
    private readonly AppDbContext _db;
    private readonly AddressBooks _addresses;
    private readonly TimeProvider _time;

    public MemberRecords(AppDbContext db, AddressBooks addresses, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(addresses);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _addresses = addresses;
        _time = time;
    }

    public async Task<MemberRecord> FindAsync(string email, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(email);
        string normalized = TextNormalizer.NormalizeIdentity(email.Trim());
        var row = await _db.Customers.AsNoTracking()
            .Where(c => c.NormalizedEmail == normalized)
            .Select(c => new { c.Id, c.Name, c.Email, c.CreatedAt, c.EmailVerifiedAt, c.PasswordHash, c.GoogleLinkedAt })
            .SingleOrDefaultAsync(ct);
        if (row is null)
        {
            return MemberRecord.None;
        }

        // An unverified pending registration is no account (02 3.13.5): the view says that and shows nothing of it but the address and the day it was made.
        if (row.EmailVerifiedAt is null)
        {
            return new MemberRecord(true, false, row.Id, null, row.Email, row.CreatedAt, false, false, [], null);
        }

        IReadOnlyList<AddressBookEntry> book = await _addresses.ListAsync(row.Id, ct);
        DateTimeOffset? blockedUntil = await EmailChangeRules.OpenRevertEndAsync(_db, AccountType.Customer, row.Id, _time.GetUtcNow(), ct);
        return new MemberRecord(true, true, row.Id, row.Name, row.Email, row.CreatedAt, !string.IsNullOrEmpty(row.PasswordHash), row.GoogleLinkedAt is not null, book, blockedUntil);
    }

    /// <summary>The end of the open revert link that closes the account's deletion, or nothing (02 3.15.6): the account screen shows the reason and the date.</summary>
    public Task<DateTimeOffset?> DeletionBlockedUntilAsync(Guid customerId, CancellationToken ct) => EmailChangeRules.OpenRevertEndAsync(_db, AccountType.Customer, customerId, _time.GetUtcNow(), ct);
}
