namespace Shopfolio.Infrastructure;

/// <summary>
/// The owner of the database transaction (09 9.1 item 3; 05 3.4.2, 7.1.2): the entry point - a page handler, a job, a command - hands its work
/// here and only here a transaction is opened. A module's open method never calls it; it joins the caller's transaction. The work is a lambda:
/// its body is what the transaction body scan reads, and an outside call (payment, e-mail, the link layer) is never inside it (05 3.6.3).
/// </summary>
internal interface ITransactionRunner
{
    /// <summary>
    /// Runs <paramref name="work"/> in one transaction. A result that is not refused is saved and committed; a refused result rolls everything
    /// back - the tracked changes and the statements already sent (<c>ExecuteUpdateAsync</c> included) - and is returned as it is; an exception
    /// rolls back and is thrown again. A call inside a running transaction is a programming error.
    /// </summary>
    Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, CancellationToken ct)
        where T : IOperationResult;

    /// <summary>
    /// <see cref="RunAsync{T}(Func{CancellationToken, Task{T}}, CancellationToken)"/> that runs the whole work again when it fails on a unique index that
    /// <paramref name="retryOnUniqueIndexes"/> names (09 9.4 item 3; 06 4.72.3): two records that made the same page address at the same time, the second
    /// stopped by the unique index. A violation (SQLSTATE <c>23505</c>) of any other index is an unexpected error and is thrown at once. At most
    /// <see cref="TransactionRunner.UniqueConflictAttempts"/> attempts, the first included; the last failure is thrown. Before each new attempt the failed one
    /// is rolled back and the tracked entities are dropped, so the <paramref name="work"/> reads everything again inside itself and holds nothing an earlier
    /// attempt read.
    /// </summary>
    Task<T> RunAsync<T>(Func<CancellationToken, Task<T>> work, IReadOnlyCollection<string> retryOnUniqueIndexes, CancellationToken ct)
        where T : IOperationResult;
}
