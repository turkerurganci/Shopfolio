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
}
