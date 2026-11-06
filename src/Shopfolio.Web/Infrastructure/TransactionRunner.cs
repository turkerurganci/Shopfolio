using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

using Npgsql;

using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The one transaction runner (09 9.1 item 3). It takes the scope's context from its constructor, opens the transaction, and ends it in one of
/// three ways: commit after a result that is not refused, roll back after a refused result, roll back and throw again after an exception.
/// There is no commit in a <c>finally</c> (09 8.1 item 5) and no retry (09 10.7 item 4), except the one overload that names unique indexes and runs the whole work again at most <see cref="UniqueConflictAttempts"/> times (09 9.4 item 3). The locks the work takes (<see cref="AdvisoryLocks"/>,
/// the order row lock, the counters' conditional updates) are transaction scoped and fall with the transaction.
/// </summary>
internal sealed class TransactionRunner : ITransactionRunner
{
    private readonly AppDbContext _db;
    private readonly JobEnqueueTracker _jobs;
    private readonly IJobSignal _signal;

    public TransactionRunner(AppDbContext db, JobEnqueueTracker jobs, IJobSignal signal)
    {
        _db = db;
        _jobs = jobs;
        _signal = signal;
    }

    /// <summary>The attempts of a work that fails on a unique index it may retry, the first included (09 3.5 item 4; K-1336): a code constant, not a setting.</summary>
    public const int UniqueConflictAttempts = 3;

    private const string UniqueViolation = "23505";

    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, IReadOnlyCollection<string> retryOnUniqueIndexes, CancellationToken ct)
        where T : IOperationResult
    {
        ArgumentNullException.ThrowIfNull(work);
        ArgumentNullException.ThrowIfNull(retryOnUniqueIndexes);
        if (retryOnUniqueIndexes.Count == 0)
        {
            throw new ArgumentException("There is no unique index to retry on: use the call without the list.", nameof(retryOnUniqueIndexes));
        }

        for (int attempt = 1; ; attempt++)
        {
            try
            {
                return await RunAsync(work, ct);
            }
            catch (DbUpdateException ex) when (attempt < UniqueConflictAttempts && IsRetryableUniqueViolation(ex, retryOnUniqueIndexes))
            {
                // RunAsync has rolled the failed attempt back and dropped the tracked entities; the next attempt reads everything again.
            }
            catch (PostgresException ex) when (attempt < UniqueConflictAttempts && IsRetryableUniqueViolation(ex, retryOnUniqueIndexes))
            {
                // The same for a statement sent outside the change tracker (ExecuteUpdateAsync, a raw statement).
            }
        }
    }

    private static bool IsRetryableUniqueViolation(Exception exception, IReadOnlyCollection<string> indexes)
    {
        for (Exception? current = exception; current is not null; current = current.InnerException)
        {
            if (current is PostgresException { SqlState: UniqueViolation } violation)
            {
                return violation.ConstraintName is { } name && indexes.Contains(name, StringComparer.Ordinal);
            }
        }

        return false;
    }

    public async Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)
        where T : IOperationResult
    {
        ArgumentNullException.ThrowIfNull(work);
        if (_db.Database.CurrentTransaction is not null)
        {
            throw new InvalidOperationException(
                "A transaction is already open: a module method joins the caller's transaction and never calls the runner again (09 9.1 item 3).");
        }

        await using IDbContextTransaction transaction = await _db.Database.BeginTransactionAsync(ct);
        _jobs.Clear();
        bool committed = false;
        try
        {
            T result = await work(ct);
            if (!result.IsRejected)
            {
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                committed = true;

                // The job runner is woken after the commit and only when this transaction wrote a job row (09 13.1 item 2): a rolled back
                // transaction wakes nobody, and the row is not there to find.
                if (_jobs.IsMarked)
                {
                    _signal.Notify();
                }
            }

            return result;
        }
        finally
        {
            _jobs.Clear();
            // A refusal or an exception: the statements already sent are undone with the transaction and the tracked changes are dropped, so
            // a later save of the same scope cannot write them (05 3.4.2: a transition happens wholly or not at all). The exception goes on
            // up unchanged: there is no catch here (09 8.1 item 6).
            if (!committed)
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _db.ChangeTracker.Clear();
            }
        }
    }
}
