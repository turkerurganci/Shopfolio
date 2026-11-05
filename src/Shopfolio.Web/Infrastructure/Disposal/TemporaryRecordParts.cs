using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Ordering;

namespace Shopfolio.Infrastructure.Disposal;

/// <summary>
/// The attempt records whose window has passed (06 9.4.3; Z-27). Each limit has its own window, a product constant (P-31..P-36, P-49); the part
/// goes through the limits one after the other, so a row is judged by its own limit's window and by nothing else. L7 and L8 are counted from
/// the orders and have no rows here (06 4.43).
/// </summary>
internal sealed class AttemptRecordDisposalPart : IDisposalPart
{
    /// <summary>The window of every limit the table counts, in minutes; the numbers are the product constants.</summary>
    public static IReadOnlyDictionary<AttemptLimit, int> WindowMinutes { get; } = new Dictionary<AttemptLimit, int>
    {
        [AttemptLimit.L1] = ProductConstants.SignInAttemptWindowMinutes,
        [AttemptLimit.L2] = ProductConstants.PasswordResetAttemptWindowMinutes,
        [AttemptLimit.L3] = ProductConstants.RegistrationAttemptWindowMinutes,
        [AttemptLimit.L4] = ProductConstants.ContactFormAttemptWindowMinutes,
        [AttemptLimit.L5] = ProductConstants.GuestOrderLookupAttemptWindowMinutes,
        [AttemptLimit.L6] = ProductConstants.InvalidCouponAttemptWindowMinutes,
        [AttemptLimit.L9] = ProductConstants.VerificationResendAttemptWindowMinutes,
    };

    public string Name => "AttemptRecords";

    public async Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        foreach ((AttemptLimit limit, int minutes) in WindowMinutes)
        {
            DateTimeOffset cutoff = run.Now - TimeSpan.FromMinutes(minutes);
            await run.ChunksAsync(
                DisposalRecordType.AttemptRecord,
                async (db, max, cancel) =>
                {
                    IQueryable<AttemptRecord> expired = db.AttemptRecords.Where(a => a.AttemptLimit == limit && a.CreatedAt < cutoff);
                    List<Guid> ids = await expired.OrderBy(a => a.CreatedAt).Select(a => a.Id).Take(max).ToListAsync(cancel);
                    return await expired.Where(a => ids.Contains(a.Id)).ExecuteDeleteAsync(cancel);
                },
                ct);
        }
    }
}

/// <summary>The sessions whose life is over (06 9.4.3; Z-4, Z-5): a short one two hours and a long one thirty days after the last operation (P-15, P-16).</summary>
internal sealed class SessionDisposalPart : IDisposalPart
{
    public string Name => "Sessions";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset shortCutoff = run.Now - TimeSpan.FromHours(ProductConstants.ShortSessionHours);
        DateTimeOffset longCutoff = run.Now - TimeSpan.FromDays(ProductConstants.LongSessionDays);
        return run.ChunksAsync(
            DisposalRecordType.Session,
            async (db, max, cancel) =>
            {
                IQueryable<Session> expired = db.Sessions.Where(s => s.IsLongLived ? s.LastActivityAt < longCutoff : s.LastActivityAt < shortCutoff);
                List<Guid> ids = await expired.OrderBy(s => s.LastActivityAt).Select(s => s.Id).Take(max).ToListAsync(cancel);
                return await expired.Where(s => ids.Contains(s.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}

/// <summary>The recognized browser marks that expired or were invalidated (06 9.4.3; Z-46): an invalidation pulls the expiry to its moment, so one test covers both.</summary>
internal sealed class RecognizedBrowserMarkDisposalPart : IDisposalPart
{
    public string Name => "RecognizedBrowserMarks";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset now = run.Now;
        return run.ChunksAsync(
            DisposalRecordType.RecognizedBrowserMark,
            async (db, max, cancel) =>
            {
                IQueryable<RecognizedBrowserMark> expired = db.RecognizedBrowserMarks.Where(m => m.ExpiresAt < now);
                List<Guid> ids = await expired.OrderBy(m => m.ExpiresAt).Select(m => m.Id).Take(max).ToListAsync(cancel);
                return await expired.Where(m => ids.Contains(m.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}

/// <summary>The guest carts nobody opened for 400 days (06 9.4.3; Z-6; 05 6.7.8); the cart's items go with it. A member's cart lives with the account and is never judged here.</summary>
internal sealed class GuestCartDisposalPart : IDisposalPart
{
    public string Name => "GuestCarts";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset cutoff = run.Now - TimeSpan.FromDays(DisposalTimings.GuestCartIdleDays);
        return run.ChunksAsync(
            DisposalRecordType.Cart,
            async (db, max, cancel) =>
            {
                IQueryable<Cart> expired = db.Carts.Where(c => c.GuestCartKeyHash != null && c.LastAccessedAt < cutoff);
                List<Guid> ids = await expired.OrderBy(c => c.LastAccessedAt).Select(c => c.Id).Take(max).ToListAsync(cancel);
                return await expired.Where(c => ids.Contains(c.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}

/// <summary>
/// The administrator invitations that were accepted, withdrawn, invalidated or whose link ran out (06 9.4.3; Z-3): the invitation's link record
/// goes with it by the foreign key. The invitation's life is its link record's expiry, so it is read from there and not repeated on the invitation.
/// </summary>
internal sealed class AdminInvitationDisposalPart : IDisposalPart
{
    public string Name => "AdminInvitations";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset now = run.Now;
        return run.ChunksAsync(
            DisposalRecordType.AdminInvitation,
            async (db, max, cancel) =>
            {
                IQueryable<AdminInvitation> expired = db.AdminInvitations.Where(
                    i => i.AcceptedAt != null
                        || i.WithdrawnAt != null
                        || i.InvalidatedAt != null
                        || db.LinkRecords.Any(l => l.AdminInvitationId == i.Id && l.ExpiresAt < now));
                List<Guid> ids = await expired.OrderBy(i => i.CreatedAt).Select(i => i.Id).Take(max).ToListAsync(cancel);
                return await expired.Where(i => ids.Contains(i.Id)).ExecuteDeleteAsync(cancel);
            },
            ct);
    }
}

/// <summary>The old address a revert link carries, emptied once the link's life is over (06 9.4.3; Z-45): the row stays with the account (06 2.2.5), only the address goes.</summary>
internal sealed class RevertLinkAddressDisposalPart : IDisposalPart
{
    public string Name => "RevertLinkAddresses";

    public Task RunAsync(DisposalRun run, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(run);
        DateTimeOffset now = run.Now;
        return run.ChunksAsync(
            DisposalRecordType.LinkRecord,
            async (db, max, cancel) =>
            {
                IQueryable<LinkRecord> expired = db.LinkRecords.Where(l => l.LinkType == LinkType.EmailChangeRevert && l.PreviousEmail != null && l.ExpiresAt < now);
                List<Guid> ids = await expired.OrderBy(l => l.ExpiresAt).Select(l => l.Id).Take(max).ToListAsync(cancel);
                return await expired.Where(l => ids.Contains(l.Id)).ExecuteUpdateAsync(s => s.SetProperty(l => l.PreviousEmail, (string?)null), cancel);
            },
            ct);
    }
}
