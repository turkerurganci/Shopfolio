using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;

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
