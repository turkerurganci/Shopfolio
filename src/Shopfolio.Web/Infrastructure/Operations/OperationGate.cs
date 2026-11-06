namespace Shopfolio.Infrastructure.Operations;

/// <summary>The decision with the availability it was made from: the refusal's page shows the current values.</summary>
internal readonly record struct OperationVerdict(OperationDecision Decision, OperationAvailability Availability)
{
    /// <summary>True when the operation may be applied.</summary>
    public bool IsAllowed => Decision == OperationDecision.Allowed;
}

/// <summary>
/// The re-check of a POST handler (07 8.1.2; 09 8.3 item 3): the handler asks the same rule instance the GET asked (<see cref="IOperationRule{TContext}"/>),
/// against the state it read under the row lock, and applies the operation only when the verdict is <see cref="OperationDecision.Allowed"/>. A hidden
/// or disabled button does not enter: a request that arrives for a closed operation - written by hand, from an old tab, for a button that was never shown
/// - gets the same verdict as one the screen offered. The gate reads no clock and no database, so it is the pure half of the check; the row lock and the
/// load belong to the sequence that calls it (<c>OperationRunner</c>, with the module's <c>Contracts</c> methods).
/// </summary>
internal static class OperationGate
{
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
