using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Modules.Membership;

/// <summary>
/// Z-1 (03 4.1.1; 02 3.13.5, 4.2 Z-1; P-12): the life of the e-mail verification link starts with the first link and a link asked again does not
/// lengthen it. When it ends, the unverified registration is gone and the address is free again; a pending change of an account's address drops. One
/// home for both conditions: the registration's lazy deletion (<see cref="MembershipModule.RegisterAsync"/>) and the nightly deletion
/// (<see cref="PendingRegistrationDisposalPart"/>, <see cref="PendingEmailChangeDisposalPart"/>) read the same expression.
/// </summary>
internal static class PendingRecordExpiry
{
    /// <summary>
    /// An unverified registration whose first verification link has ended at <paramref name="now"/>, or that has none: the first link is the oldest
    /// one; the later ones end where it ends (02 3.13.5).
    /// </summary>
    public static Expression<Func<Customer, bool>> EndedRegistration(AppDbContext db, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(db);
        return c => c.EmailVerifiedAt == null
            && !db.LinkRecords
                .Where(l => l.CustomerId == c.Id && l.LinkType == LinkType.EmailVerification)
                .OrderBy(l => l.CreatedAt)
                .Take(1)
                .Any(l => l.ExpiresAt > now);
    }

    /// <summary>
    /// A pending address change whose life has ended at <paramref name="now"/>: the change is written with its first link, so its life is the link's,
    /// the product constant from its creation - for a customer and for an administrator alike (03 4.1.1; P-12).
    /// </summary>
    public static Expression<Func<PendingEmailChange, bool>> EndedEmailChange(DateTimeOffset now)
    {
        DateTimeOffset createdAtOrBefore = now - TimeSpan.FromHours(ProductConstants.EmailVerificationLinkHours);
        return c => c.CreatedAt <= createdAtOrBefore;
    }
}

/// <summary>
/// The nightly deletion of the unverified registrations whose link has ended (06 9.4.1; 02 3.13.5; Z-1; D-253): the account row goes with its links and
/// its cart, and the count is written to the disposal record in the same transaction. The registration's lazy deletion does the same for one address.
/// </summary>
internal sealed class PendingRegistrationDisposalPart : IDisposalPart
{
    public string Name => "PendingRegistrations";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        return run.ChunksAsync(
            DisposalRecordType.PendingRegistration,
            async (db, max, cancel) =>
            {
                IQueryable<Customer> ended = db.Customers.Where(PendingRecordExpiry.EndedRegistration(db, run.Now));
                List<Guid> ids = await ended.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id).Select(c => c.Id).Take(max).ToListAsync(cancel);
                return await ended.Where(c => ids.Contains(c.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}

/// <summary>The nightly drop of the pending address changes whose link has ended (06 9.4.1; 03 4.1.1; Z-1), with the count in the disposal record.</summary>
internal sealed class PendingEmailChangeDisposalPart : IDisposalPart
{
    public string Name => "PendingEmailChanges";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        return run.ChunksAsync(
            DisposalRecordType.PendingEmailChange,
            async (db, max, cancel) =>
            {
                IQueryable<PendingEmailChange> ended = db.PendingEmailChanges.Where(PendingRecordExpiry.EndedEmailChange(run.Now));
                List<Guid> ids = await ended.OrderBy(c => c.CreatedAt).ThenBy(c => c.Id).Select(c => c.Id).Take(max).ToListAsync(cancel);
                return await ended.Where(c => ids.Contains(c.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}
