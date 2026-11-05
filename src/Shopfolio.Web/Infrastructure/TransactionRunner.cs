using Microsoft.EntityFrameworkCore.Storage;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The one transaction runner (09 9.1 item 3). It takes the scope's context from its constructor, opens the transaction, and ends it in one of
/// three ways: commit after a result that is not refused, roll back after a refused result, roll back and throw again after an exception.
/// There is no commit in a <c>finally</c> (09 8.1 item 5) and no retry (09 10.7 item 4). The locks the work takes (<see cref="AdvisoryLocks"/>,
/// the order row lock, the counters' conditional updates) are transaction scoped and fall with the transaction.
/// </summary>
internal sealed class TransactionRunner : ITransactionRunner
{
    private readonly AppDbContext _db;

    public TransactionRunner(AppDbContext db)
    {
        _db = db;
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
        T result;
        try
        {
            result = await work(ct);
            if (!result.IsRejected)
            {
                await _db.SaveChangesAsync(ct);
                await transaction.CommitAsync(ct);
                return result;
            }
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            _db.ChangeTracker.Clear();
            throw;
        }

        // A refusal: the statements already sent are undone with the transaction and the tracked changes are dropped, so a later save of
        // the same scope cannot write them (05 3.4.2: a transition happens wholly or not at all).
        await transaction.RollbackAsync(CancellationToken.None);
        _db.ChangeTracker.Clear();
        return result;
    }
}
