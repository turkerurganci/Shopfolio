using System.Net;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// The rules of an e-mail change that the customer's and the administrator's accounts share (02 3.13.14, 3.13.16; 07 UC-66, UC-67, UC-179, UC-180; D-617): the life of a pending change, the open revert
/// link that closes a following change, and the L-9 count of a request that replaces a pending one. Each rule lives here once and both accounts' flows call it (09 24 item 4); the account type only picks
/// the column of the link record that names the account.
/// </summary>
internal static class EmailChangeRules
{
    /// <summary>The life of a pending change is the first link's: P-12 hours from the request (the nightly deletion reads the same rule).</summary>
    public static bool IsEnded(DateTimeOffset createdAt, DateTimeOffset now) => createdAt <= now - TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours);

    /// <summary>The last moment of the pending change's link: a request again does not lengthen it (02 3.13.5).</summary>
    public static DateTimeOffset LinkEnd(DateTimeOffset createdAt) => createdAt.AddHours(ProductConstants.EmailVerificationLinkHours);

    /// <summary>The end of the account's open revert link (not used, not superseded, not ended), or nothing; the customer's deletion reads the same rule (02 3.15.6).</summary>
    public static async Task<DateTimeOffset?> OpenRevertEndAsync(AppDbContext db, AccountType type, Guid accountId, DateTimeOffset now, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(db);
        IQueryable<LinkRecord> open = db.LinkRecords.AsNoTracking()
            .Where(l => l.LinkType == LinkType.EmailChangeRevert && l.UsedAt == null && l.SupersededAt == null && l.ExpiresAt > now);
        open = type == AccountType.Admin ? open.Where(l => l.AdminId == accountId) : open.Where(l => l.CustomerId == accountId);
        return await open.OrderByDescending(l => l.ExpiresAt).Select(l => (DateTimeOffset?)l.ExpiresAt).FirstOrDefaultAsync(ct);
    }

    /// <summary>L-9 is counted on every request, on the e-mail and on the IP (02 8.2); true when either axis is over its limit.</summary>
    public static async Task<bool> CountedOverLimitAsync(AttemptCounter attempts, string normalizedEmail, IPAddress? ipAddress, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(attempts);
        AttemptSubject byEmail = AttemptSubject.Email(normalizedEmail);
        AttemptSubject byIp = AttemptSubject.Ip(ipAddress);
        await attempts.RecordAsync(AttemptLimit.L9, byEmail, ct);
        await attempts.RecordAsync(AttemptLimit.L9, byIp, ct);
        return await attempts.IsExceededAsync(AttemptLimit.L9, byEmail, ct) || await attempts.IsExceededAsync(AttemptLimit.L9, byIp, ct);
    }
}
