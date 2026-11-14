namespace Shopfolio.Infrastructure.Operations;

/// <summary>
/// The result of a gated operation (07 8.1.2, 8.1.3): applied, refused because the record's version is no longer the one the screen showed, or refused by the
/// operation's rule. A refusal is a return value; the transaction is rolled back and nothing was written. It is the value the module's <c>Contracts</c> write
/// method returns to the page, so it is public (09 6 item 1); the page reads <see cref="IsRejected"/>, <see cref="VersionChanged"/> and <see cref="Verdict"/>.
/// </summary>
public sealed class GatedOperationResult : IOperationResult
{
    private GatedOperationResult(bool versionChanged, OperationVerdict verdict)
    {
        VersionChanged = versionChanged;
        Verdict = verdict;
    }

    /// <summary>The record's version is not the one the form carried: the conflict (409, 07 2.4.6).</summary>
    public bool VersionChanged { get; }

    /// <summary>The rule's verdict; the default value of a version conflict is not read.</summary>
    public OperationVerdict Verdict { get; }

    public bool IsRejected => VersionChanged || !Verdict.IsAllowed;

    internal static GatedOperationResult Conflict { get; } = new(true, default);

    internal static GatedOperationResult Decided(OperationVerdict verdict) => new(false, verdict);
}

/// <summary>
/// The page's half of an operation's POST (07 8.1.2; 05 7.3.2; 09 8.3 item 3): the page - the entry point - opens the transaction here (09 9.1 item 3) and, inside it,
/// calls the module's <c>Contracts</c> write method, which runs the rule (<see cref="OperationGate.ExecuteAsync{TContext}"/>). The runner knows no rule and no module type:
/// the product rule lives in the module and the page reaches it only through that method. A refusal the method returns rolls the transaction back, so the record, its
/// items and its version are as they were. The method opens no transaction of its own (09 6 item 3).
/// </summary>
internal sealed class OperationRunner
{
    private readonly ITransactionRunner _transactions;

    public OperationRunner(ITransactionRunner transactions)
    {
        _transactions = transactions;
    }

    /// <summary>Runs the module's write method <paramref name="operation"/> in one transaction; a rejected result rolls it back.</summary>
    public async Task<GatedOperationResult> RunAsync(Func<CancellationToken, Task<GatedOperationResult>> operation, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return await _transactions.RunAsync(token => operation(token), ct);
    }

    /// <summary>
    /// As <see cref="RunAsync(Func{CancellationToken, Task{GatedOperationResult}}, CancellationToken)"/>, run once more as a whole when it fails on one of <paramref name="retryOnUniqueIndexes"/> (09 10.2):
    /// the internal note's two first writes at once meet on its unique index, and the second run answers the later one with the conflict (09 10.9 item 3; T531).
    /// </summary>
    public async Task<GatedOperationResult> RunAsync(Func<CancellationToken, Task<GatedOperationResult>> operation, IReadOnlyCollection<string> retryOnUniqueIndexes, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(operation);
        return await _transactions.RunAsync(token => operation(token), retryOnUniqueIndexes, ct);
    }
}
