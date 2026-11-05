using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The lock order of 05 7.3.5, held per transaction (MK-21): every place that takes a lock - <see cref="AdvisoryLocks"/>, the order row lock and
/// the Catalog counters' conditional updates - reports it here first, and a lock asked out of order is refused with an exception before any
/// SQL is sent. The order is: the cart's advisory lock first and alone at the head, then the order rows by id, then the variant rows by id, the
/// coupon last. The category tree's and the administrator set's advisory locks do not join the order: the transaction that takes one takes no
/// other lock. A reversed request is a programming error (it could wait on the reverse of another transaction); the tests that run the code
/// catch it (T228 criterion 7). The state hangs on the context and is reset when the context's transaction changes.
/// </summary>
internal static class LockSequence
{
    private static readonly ConditionalWeakTable<DbContext, State> States = [];

    /// <summary>The key order of 05 7.3.5 ("by id"): the order PostgreSQL gives a <c>uuid</c>, which is the order of its lower-case text.</summary>
    public static int CompareKeys(Guid left, Guid right)
    {
        return string.CompareOrdinal(left.ToString("D"), right.ToString("D"));
    }

    /// <summary>Records the lock about to be taken; throws when no transaction is open or the lock breaks the order.</summary>
    public static void Enter(DbContext context, LockKind kind, Guid? key)
    {
        ArgumentNullException.ThrowIfNull(context);
        Guid transactionId = context.Database.CurrentTransaction?.TransactionId
            ?? throw new InvalidOperationException($"The {kind} lock is transaction scoped and is taken only inside a transaction (09 10.2 item 5).");
        State state = States.GetValue(context, _ => new State());
        if (state.TransactionId != transactionId)
        {
            state.TransactionId = transactionId;
            state.Taken.Clear();
        }

        if (state.Taken.Count > 0)
        {
            (LockKind lastKind, Guid? lastKey) = state.Taken[^1];
            if (IsSeparate(kind) || state.Taken.Any(t => IsSeparate(t.Kind)))
            {
                if (!(lastKind == kind && lastKey == key && state.Taken.Count == 1))
                {
                    throw new InvalidOperationException(
                        $"The {kind} lock is asked after the {lastKind} lock: the category tree's and the administrator set's locks are taken alone (05 7.3.5).");
                }
            }
            else if (kind == LockKind.Cart)
            {
                throw new InvalidOperationException(
                    $"The cart lock is asked after the {lastKind} lock: the cart's advisory lock is the first lock of the confirmation (05 4.2.2, 7.3.5).");
            }
            else if (kind < lastKind || (kind == lastKind && key is Guid k && lastKey is Guid l && CompareKeys(k, l) < 0))
            {
                throw new InvalidOperationException(
                    $"The {kind} lock {key} is asked after the {lastKind} lock {lastKey}: locks are taken as cart, orders by id, variants by id, coupon last (05 7.3.5).");
            }
        }

        state.Taken.Add((kind, key));
    }

    /// <summary>The orders whose row lock the context's open transaction holds; empty without a transaction.</summary>
    public static IReadOnlyCollection<Guid> LockedOrders(DbContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        Guid? transactionId = context.Database.CurrentTransaction?.TransactionId;
        if (transactionId is null || !States.TryGetValue(context, out State? state) || state.TransactionId != transactionId)
        {
            return [];
        }

        return state.Taken.Where(t => t.Kind == LockKind.Order && t.Key is not null).Select(t => t.Key!.Value).Distinct().ToList();
    }

    private static bool IsSeparate(LockKind kind)
    {
        return kind is LockKind.CategoryTree or LockKind.AdminSet;
    }

    private sealed class State
    {
        public Guid TransactionId { get; set; }

        public List<(LockKind Kind, Guid? Key)> Taken { get; } = [];
    }
}
