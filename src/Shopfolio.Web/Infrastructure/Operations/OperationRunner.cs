namespace Shopfolio.Infrastructure.Operations;

/// <summary>
/// The result of a gated operation (07 8.1.2, 8.1.3): applied, refused because the record's version is no longer the one the screen showed, or refused by the
/// operation's rule. A refusal is a return value; the transaction is rolled back and nothing was written.
/// </summary>
internal sealed class GatedOperationResult : IOperationResult
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
/// The one sequence of a POST handler that applies an operation (07 8.1.2; 05 7.3.2; 09 8.3 item 3), written in the common infrastructure so a page may own it without
/// reaching a module's internals (09 6 item 2). The page - the entry point - opens the transaction here (09 9.1 item 3); inside it, in this order: the record's row lock and,
/// for the administrator's operation, the comparison of the version the form carried (<paramref name="lockRecord"/> - the module's open method, e.g.
/// <c>IOrderingModule.LockOrderAsync</c>), the load of the state the rule reads (<paramref name="load"/> - module open methods), the rule's re-check through
/// <see cref="OperationGate"/>, and only then the write (<paramref name="apply"/> - module open methods). A refusal returns before the write and rolls the transaction
/// back, so the record, its items and its version are as they were. The rule is the instance the page's GET asked, so the button and the check cannot disagree (07 8.1.5).
/// </summary>
internal sealed class OperationRunner
{
    private readonly ITransactionRunner _transactions;

    public OperationRunner(ITransactionRunner transactions)
    {
        _transactions = transactions;
    }

    /// <summary>
    /// Runs the operation. <paramref name="lockRecord"/> takes the row lock and answers whether the version the form carried (none for the customer's operation) still
    /// matches; <paramref name="requestedQuantity"/> is the quantity the request names, or <see langword="null"/> for an operation that has none.
    /// </summary>
    public async Task<GatedOperationResult> RunAsync<TContext>(
        Func<CancellationToken, Task<bool>> lockRecord,
        Func<CancellationToken, Task<TContext>> load,
        IOperationRule<TContext> rule,
        int? requestedQuantity,
        Func<TContext, OperationAvailability, CancellationToken, Task> apply,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(lockRecord);
        ArgumentNullException.ThrowIfNull(load);
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(apply);
        return await _transactions.RunAsync(
            async token =>
            {
                if (!await lockRecord(token))
                {
                    return GatedOperationResult.Conflict;
                }

                TContext context = await load(token);
                OperationVerdict verdict = OperationGate.Decide(rule, context, requestedQuantity);
                if (!verdict.IsAllowed)
                {
                    return GatedOperationResult.Decided(verdict);
                }

                await apply(context, verdict.Availability, token);
                return GatedOperationResult.Decided(verdict);
            },
            ct);
    }
}
