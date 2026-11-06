using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Infrastructure.Security;

/// <summary>The result of the counter's own short transaction: never refused; a failure is an exception.</summary>
internal sealed record AttemptWriteResult(Guid AttemptId) : IOperationResult
{
    public bool IsRejected => false;
}

/// <summary>
/// The counter of the attempt limits L-1 to L-6 and L-9 (09 15.4 items 5 and 6; 05 6.3.1, 6.3.2; 06 4.43): the only code that writes the attempt
/// record, and the only place a subject's value reaches a row (through <see cref="AttemptSubject"/>). The thresholds and windows are the table of
/// <see cref="AttemptLimits"/>, read from the product constants; a call passes a limit and a subject and no number. L-7 and L-8 are counted from the
/// orders and do not use this class.
/// <para>
/// A row is written in a short transaction of its own, on a connection of its own, and committed before the count is made, so a caller that holds an
/// open transaction - or one that rolls back - leaves the row where it is, and attempts arriving at the same moment see each other's rows (K-972).
/// The use is two-fold. For the limits counted on a failure (L-1, L-5, L-6): <see cref="RecordAsync"/>, then <see cref="IsExceededAsync"/>; the handler
/// does the work unless it is exceeded; when the work succeeds, or is not done because the limit was exceeded, it calls <see cref="ForgetAsync"/> in the
/// same request. For the limits counted on every request (L-2, L-3, L-4, L-9): <see cref="RecordAsync"/> before the work, every time, and no forgetting.
/// </para>
/// </summary>
internal sealed class AttemptCounter
{
    private readonly AppDbContext _db;
    private readonly IServiceScopeFactory _scopes;
    private readonly TimeProvider _time;

    public AttemptCounter(AppDbContext db, IServiceScopeFactory scopes, TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(scopes);
        ArgumentNullException.ThrowIfNull(time);
        _db = db;
        _scopes = scopes;
        _time = time;
    }

    /// <summary>Writes one row for the attempt in a transaction of its own and commits it; returns the row's identifier.</summary>
    public async Task<Guid> RecordAsync(AttemptLimit limit, AttemptSubject subject, CancellationToken ct)
    {
        AttemptLimits.Of(limit, subject.Axis);
        AttemptRecord row = AttemptRecord.Create(limit, subject, _time.GetUtcNow());
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        await runner.RunAsync(
            _ =>
            {
                db.AttemptRecords.Add(row);
                return Task.FromResult(new AttemptWriteResult(row.Id));
            },
            ct);
        return row.Id;
    }

    /// <summary>
    /// True when the rows of the subject in the window - the attempt's own row included - are more than the threshold: with "5 in 15 minutes" the sixth
    /// attempt is exceeded. The window ends now and starts one window ago; a row exactly one window old still counts (the clean-up deletes the rows older).
    /// </summary>
    public async Task<bool> IsExceededAsync(AttemptLimit limit, AttemptSubject subject, CancellationToken ct)
    {
        AttemptRule rule = AttemptLimits.Of(limit, subject.Axis);
        DateTimeOffset from = _time.GetUtcNow() - rule.Window;
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        int count = await db.AttemptRecords.AsNoTracking()
            .CountAsync(a => a.AttemptLimit == limit && a.AttemptAxis == subject.Axis && a.NormalizedValue == subject.Value && a.CreatedAt >= from, ct);
        return count > rule.Threshold;
    }

    /// <summary>Deletes the row of an attempt that did not count after all (it succeeded, or the work was not done because the limit was exceeded).</summary>
    public async Task ForgetAsync(Guid attemptId, CancellationToken ct)
    {
        await using AsyncServiceScope scope = _scopes.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        ITransactionRunner runner = scope.ServiceProvider.GetRequiredService<ITransactionRunner>();
        await runner.RunAsync(
            async cancel =>
            {
                await db.AttemptRecords.Where(a => a.Id == attemptId).ExecuteDeleteAsync(cancel);
                return new AttemptWriteResult(attemptId);
            },
            ct);
    }

    /// <summary>
    /// The cart merge at sign-in (05 6.3.2; 02 8.2.2): runs in the merge's own transaction, on the caller's context. The cart with more rows keeps them,
    /// with their moments, under the account cart's identifier; the other cart's rows are deleted; the counters are not added and not reset. With the
    /// same number of rows in the window the account cart's rows stay. Only the invalid coupon limit (L-6) counts on the cart axis.
    /// </summary>
    public async Task MergeCartAsync(Guid guestCartId, Guid accountCartId, CancellationToken ct)
    {
        if (guestCartId == accountCartId)
        {
            return;
        }

        AttemptSubject guest = AttemptSubject.Cart(guestCartId);
        AttemptSubject account = AttemptSubject.Cart(accountCartId);
        DateTimeOffset from = _time.GetUtcNow() - AttemptLimits.Of(AttemptLimit.L6, AttemptAxis.Cart).Window;
        IQueryable<AttemptRecord> rows = _db.AttemptRecords.Where(a => a.AttemptLimit == AttemptLimit.L6 && a.AttemptAxis == AttemptAxis.Cart);

        int guestCount = await rows.CountAsync(a => a.NormalizedValue == guest.Value && a.CreatedAt >= from, ct);
        int accountCount = await rows.CountAsync(a => a.NormalizedValue == account.Value && a.CreatedAt >= from, ct);
        if (guestCount > accountCount)
        {
            await rows.Where(a => a.NormalizedValue == account.Value).ExecuteDeleteAsync(ct);
            await rows.Where(a => a.NormalizedValue == guest.Value).ExecuteUpdateAsync(s => s.SetProperty(a => a.NormalizedValue, account.Value), ct);
        }
        else
        {
            await rows.Where(a => a.NormalizedValue == guest.Value).ExecuteDeleteAsync(ct);
        }
    }
}
