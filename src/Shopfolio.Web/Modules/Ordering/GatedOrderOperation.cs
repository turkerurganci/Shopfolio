using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The result of a gated order operation (07 8.1.2, 8.1.3): applied, refused because the order's version is no longer the one the screen showed, or refused by the
/// operation's rule. A refusal is a return value; the transaction is rolled back and nothing was written.
/// </summary>
internal sealed class GatedOrderResult : IOperationResult
{
    private GatedOrderResult(bool versionChanged, OperationVerdict verdict)
    {
        VersionChanged = versionChanged;
        Verdict = verdict;
    }

    /// <summary>The order's version is not the one the form carried: the conflict (409, 07 2.4.6).</summary>
    public bool VersionChanged { get; }

    /// <summary>The rule's verdict; the default value of a version conflict is not read.</summary>
    public OperationVerdict Verdict { get; }

    public bool IsRejected => VersionChanged || !Verdict.IsAllowed;

    internal static GatedOrderResult Conflict { get; } = new(true, default);

    internal static GatedOrderResult Decided(OperationVerdict verdict) => new(false, verdict);
}

/// <summary>
/// The one sequence of a POST handler that applies an operation on an order (07 8.1.2; 05 7.3.2; 09 8.3 item 3): under the order's row lock, against the order as it is
/// now - the lock, then the version the administrator's form carried (an operation of the customer carries none), then the load of the state the rule reads, then the
/// rule's re-check through <see cref="OperationGate"/>, and only then the write. A refusal returns before the write and rolls the transaction back, so the order,
/// its items and its version are as they were. The write (<paramref name="apply"/>) raises the order's version like every order write (09 10.2 item 3).
/// The rule is the instance the page's GET asked, so the button and the check cannot disagree (07 8.1.5).
/// </summary>
internal sealed class GatedOrderOperation
{
    private readonly ITransactionRunner _transactions;
    private readonly OrderLocks _locks;
    private readonly AppDbContext _db;

    public GatedOrderOperation(ITransactionRunner transactions, OrderLocks locks, AppDbContext db)
    {
        _transactions = transactions;
        _locks = locks;
        _db = db;
    }

    /// <summary>
    /// Runs the operation. <paramref name="shownVersion"/> is the version the panel's form carried, or <see langword="null"/> for the customer's operation;
    /// <paramref name="requestedQuantity"/> is the quantity the request names, or <see langword="null"/> for an operation that has none.
    /// </summary>
    public async Task<GatedOrderResult> RunAsync<TContext>(
        Guid orderId,
        int? shownVersion,
        Func<CancellationToken, Task<TContext>> load,
        IOperationRule<TContext> rule,
        int? requestedQuantity,
        Func<TContext, OperationAvailability, CancellationToken, Task> apply,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(load);
        ArgumentNullException.ThrowIfNull(rule);
        ArgumentNullException.ThrowIfNull(apply);
        return await _transactions.RunAsync(
            async token =>
            {
                await _locks.LockAsync([orderId], token);
                if (shownVersion is int shown)
                {
                    Order order = await _db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, token);
                    if (!ShownVersion.Matches(order, shown))
                    {
                        return GatedOrderResult.Conflict;
                    }
                }

                TContext context = await load(token);
                OperationVerdict verdict = OperationGate.Decide(rule, context, requestedQuantity);
                if (!verdict.IsAllowed)
                {
                    return GatedOrderResult.Decided(verdict);
                }

                await apply(context, verdict.Availability, token);
                return GatedOrderResult.Decided(verdict);
            },
            ct);
    }
}
