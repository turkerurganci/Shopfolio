using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The one gate of the panel order writers (09 8.3 item 3; 09 §24 item 4; D-1542): the order's row lock, the version the screen showed (<see cref="ShownVersion.Matches"/>; a change is the conflict
/// and the rule is not asked), then the facts read after the lock and - for <see cref="CheckAsync"/> - the operation's one rule over them, the decision the GET drew the button from
/// (<see cref="OrderOperationCatalog.Check"/>; an item that is not the order's is closed). <see cref="PanelOrderSteps"/>, <see cref="PanelOrderUpkeep"/> and <see cref="PanelItemSteps"/> call it
/// instead of keeping a copy each. Joins the caller's transaction; reads only.
/// </summary>
internal sealed class PanelOrderGate
{
    private readonly AppDbContext _db;
    private readonly OrderLocks _locks;
    private readonly OrderOperations _operations;

    public PanelOrderGate(AppDbContext db, OrderLocks locks, OrderOperations operations)
    {
        _db = db;
        _locks = locks;
        _operations = operations;
    }

    /// <summary>
    /// The lock and the version, then the facts the operation's rule reads. The refusal is <see cref="TransitionRefusal.OrderNotFound"/> or <see cref="TransitionRefusal.VersionConflict"/>;
    /// without a refusal the facts are there (an order whose facts cannot be read is not found).
    /// </summary>
    public async Task<(TransitionRefusal? Refusal, OrderOperationFacts? Facts)> LockAsync(Guid orderId, int expectedVersion, CancellationToken ct)
    {
        await _locks.LockAsync([orderId], ct);
        Order? order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return (TransitionRefusal.OrderNotFound, null);
        }

        if (!ShownVersion.Matches(order, expectedVersion))
        {
            return (TransitionRefusal.VersionConflict, null);
        }

        OrderOperationFacts? facts = await _operations.ReadFactsAsync(orderId, ct);
        return facts is null ? (TransitionRefusal.OrderNotFound, null) : (null, facts);
    }

    /// <summary>The lock, the version and the operation's rule: null when the operation may go on, else the refusal (a closed rule is <see cref="TransitionRefusal.ConditionNotMet"/>).</summary>
    public async Task<TransitionRefusal?> CheckAsync(Guid orderId, int expectedVersion, OrderOperation operation, Guid? orderItemId, CancellationToken ct)
    {
        (TransitionRefusal? refusal, OrderOperationFacts? facts) = await LockAsync(orderId, expectedVersion, ct);
        if (refusal is not null)
        {
            return refusal;
        }

        return OrderOperationCatalog.Check(facts!, operation, orderItemId, null).IsAllowed ? null : TransitionRefusal.ConditionNotMet; // non-null: LockAsync returns the facts when it refuses nothing
    }
}
