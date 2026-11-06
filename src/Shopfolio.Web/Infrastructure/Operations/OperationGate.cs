namespace Shopfolio.Infrastructure.Operations;

/// <summary>The decision with the availability it was made from: the refusal's page shows the current values.</summary>
public readonly record struct OperationVerdict(OperationDecision Decision, OperationAvailability Availability)
{
    /// <summary>True when the operation may be applied.</summary>
    public bool IsAllowed => Decision == OperationDecision.Allowed;
}

/// <summary>
/// The re-check of an operation's write (07 8.1.2; 09 8.3 items 1 and 3), written for the module that owns the rule. The module's <c>Contracts</c> write method calls
/// <see cref="ExecuteAsync{TContext}"/> inside the transaction the page opened: the record's row lock and version comparison, the load of the state, the module's one rule
/// instance (<see cref="IOperationRule{TContext}"/> - the instance its read method asked for the page's GET), and only when the verdict is
/// <see cref="OperationDecision.Allowed"/> the write. A hidden or disabled button does not enter: a request that arrives for a closed operation - written by hand, from
/// an old tab, for a button that was never shown - gets the same verdict as one the screen offered. <see cref="Decide{TContext}"/> reads no clock and no database, so it
/// is the pure half of the check.
/// </summary>
internal static class OperationGate
{
    /// <summary>
    /// The sequence of the module's write method (09 8.3 item 3), in this order and opening no transaction (09 6 item 3): <paramref name="lockRecord"/> takes the row lock and
    /// answers whether the version the form carried (none for the customer's operation) still matches - <see langword="false"/> is the conflict and nothing is read or
    /// written; <paramref name="load"/> reads the state the rule reads, after the lock; the rule decides; <paramref name="apply"/> writes only for an allowed verdict.
    /// <paramref name="requestedQuantity"/> is the quantity the request names, or <see langword="null"/> for an operation that has none. The result tells the
    /// caller's transaction to roll back when it is a refusal.
    /// </summary>
    public static async Task<GatedOperationResult> ExecuteAsync<TContext>(
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
        if (!await lockRecord(ct))
        {
            return GatedOperationResult.Conflict;
        }

        TContext context = await load(ct);
        OperationVerdict verdict = Decide(rule, context, requestedQuantity);
        if (!verdict.IsAllowed)
        {
            return GatedOperationResult.Decided(verdict);
        }

        await apply(context, verdict.Availability, ct);
        return GatedOperationResult.Decided(verdict);
    }

    /// <summary>
    /// Decides one request. <paramref name="requestedQuantity"/> is the quantity the request names, already read and range-checked by the field
    /// rule (at least one), or <see langword="null"/> for an operation that has no quantity; a quantity above the rule's upper limit is refused whole.
    /// </summary>
    public static OperationVerdict Decide<TContext>(IOperationRule<TContext> rule, TContext context, int? requestedQuantity)
    {
        ArgumentNullException.ThrowIfNull(rule);
        OperationAvailability availability = rule.Evaluate(context);
        if (!availability.IsOpen)
        {
            return new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed);
        }

        if (requestedQuantity is int quantity)
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
            if (quantity > availability.MaxQuantity)
            {
                return new OperationVerdict(OperationDecision.QuantityExceedsOpen, availability);
            }
        }
        else if (availability.HasQuantity)
        {
            // An operation that has a quantity field does not apply without one: the field rule makes it required (09 3.4), so reaching here is a programming error.
            throw new ArgumentException("The rule's operation carries a quantity; the request names none.", nameof(requestedQuantity));
        }

        return new OperationVerdict(OperationDecision.Allowed, availability);
    }
}
