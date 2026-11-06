namespace Shopfolio.Infrastructure.Operations;

/// <summary>
/// The one rule function of an operation (07 8.1.2, 8.1.5; 09 8.3): from the state of the record it says whether the operation is open now and
/// the values the page carries (<see cref="OperationAvailability"/>). The rule is written once, from the interval of 03 1.11 and the row of 04 6; the
/// page's GET calls it to draw the button and the POST handler calls the same instance through <see cref="OperationGate"/> to apply or refuse. The
/// context is a plain value read from the record - the statuses, the delivery date, the open quantity, today - so the rule reads no clock, no database
/// and no request, and its boundary is tested with values. A rule never decides from what the browser sent.
/// </summary>
/// <typeparam name="TContext">The state the rule reads.</typeparam>
internal interface IOperationRule<in TContext>
{
    /// <summary>The operation's availability for <paramref name="context"/>.</summary>
    OperationAvailability Evaluate(TContext context);
}
