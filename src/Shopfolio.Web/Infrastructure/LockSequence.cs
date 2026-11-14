using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The lock order of 05 7.3.5, held per transaction (MK-21): every place that takes a lock - <see cref="AdvisoryLocks"/>, the order row lock and
/// the Catalog counters' conditional updates - reports it here first, and a lock asked out of order is refused with an exception before any
/// SQL is sent. The order is: the cart's advisory lock first and alone at the head, then the order rows by id, then the variant rows by id, the
/// coupon last. The account's row lock (09 15.1 item 3) is a head of its own: the sign-in and the password flows take it alone, the account's
/// deletion takes it first and then its orders by id (05 6.1.2; T345), never after another lock and never together with the cart's. The product's row (T525; 09 10.2
/// item 5) heads the panel's product writes the same way: one product, then the variant rows by id or one digital file row - never after another lock. The category tree's and the administrator set's advisory locks do not join the order: the transaction that takes one takes no
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

        Check(state.Taken, kind, key);
        state.Taken.Add((kind, key));
    }

    /// <summary>
    /// The rule alone, without a context: throws when <paramref name="kind"/> with <paramref name="key"/> may not follow the locks already
    /// <paramref name="taken"/> in the same transaction. The same lock asked again right after itself is allowed.
    /// </summary>
    public static void Check(IReadOnlyList<(LockKind Kind, Guid? Key)> taken, LockKind kind, Guid? key)
    {
        ArgumentNullException.ThrowIfNull(taken);
        if (kind == LockKind.Product)
        {
            if (!taken.All(t => t.Kind == LockKind.Product && t.Key == key))
            {
                throw new InvalidOperationException(
                    $"The product lock {key} is asked after the {taken[^1].Kind} lock: the product's row heads its transaction, one product per transaction (09 10.2 item 5; T525).");
            }

            return;
        }

        // The product's row heads the panel's product writes (T525): only the variant rows and one digital file row follow it, and they follow it as if they opened the transaction.
        int head = 0;
        while (head < taken.Count && taken[head].Kind == LockKind.Product)
        {
            head++;
        }

        if (head > 0)
        {
            if (kind is not (LockKind.Variant or LockKind.DigitalFile))
            {
                throw new InvalidOperationException(
                    $"The {kind} lock is asked after the product lock: only the variant rows and one digital file row follow the product's row (09 10.2 item 5; T525).");
            }

            taken = [.. taken.Skip(head)];
        }

        if (taken.Count == 0)
        {
            return;
        }

        (LockKind lastKind, Guid? lastKey) = taken[^1];
        if (IsSeparate(kind) || taken.Any(t => IsSeparate(t.Kind)))
        {
            if (!(lastKind == kind && lastKey == key && taken.Count == 1))
            {
                throw new InvalidOperationException(
                    $"The {kind} lock is asked after the {lastKind} lock: the category tree's and the administrator set's locks are taken alone (05 7.3.5).");
            }
        }
        else if (kind == LockKind.Cart)
        {
            if (!(lastKind == LockKind.Cart && lastKey == key && taken.Count == 1))
            {
                throw new InvalidOperationException(
                    $"The cart lock is asked after the {lastKind} lock: the cart's advisory lock is the first lock of the confirmation (05 4.2.2, 7.3.5).");
            }
        }
        else if (kind == LockKind.Account)
        {
            if (!taken.All(t => t.Kind == LockKind.Account && t.Key == key))
            {
                throw new InvalidOperationException(
                    $"The account lock is asked after the {lastKind} lock: the account's row is the first lock of the deletion, before its orders (09 15.1 item 3; 05 6.1.2).");
            }
        }
        else if (Rank(kind) < Rank(lastKind) || (kind == lastKind && key is Guid k && lastKey is Guid l && CompareKeys(k, l) < 0))
        {
            throw new InvalidOperationException(
                $"The {kind} lock {key} is asked after the {lastKind} lock {lastKey}: locks are taken as cart, orders by id, variants by id, coupon last (05 7.3.5).");
        }
    }

    /// <summary>
    /// Takes a lock off the record once its row was deleted in the same transaction (T515: a pending registration is replaced by the account of the Google sign-in, whose own row lock follows).
    /// Nothing can lock a deleted row, so the order rule has nothing left to protect for it; the database keeps the lock to the transaction's end.
    /// </summary>
    public static void Forget(DbContext context, LockKind kind, Guid? key)
    {
        ArgumentNullException.ThrowIfNull(context);
        if (context.Database.CurrentTransaction?.TransactionId is { } transactionId && States.TryGetValue(context, out State? state) && state.TransactionId == transactionId)
        {
            state.Taken.RemoveAll(t => t.Kind == kind && t.Key == key);
        }
    }

    /// <summary>True when the context's open transaction has already taken this lock (so it is not taken twice).</summary>
    public static bool Holds(DbContext context, LockKind kind, Guid? key)
    {
        ArgumentNullException.ThrowIfNull(context);
        Guid? transactionId = context.Database.CurrentTransaction?.TransactionId;
        return transactionId is not null && States.TryGetValue(context, out State? state) && state.TransactionId == transactionId && state.Taken.Contains((kind, key));
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

        return state.Taken.Where(t => t.Kind == LockKind.Order).Select(t => t.Key).OfType<Guid>().Distinct().ToList();
    }

    private static bool IsSeparate(LockKind kind)
    {
        return kind is LockKind.CategoryTree or LockKind.AdminSet or LockKind.DigitalFile or LockKind.CompanySettings;
    }

    // The account's row heads the order the way the cart's advisory lock does (the account's own flows take no cart lock): account, then orders by id, variants, coupon.
    private static int Rank(LockKind kind)
    {
        return kind == LockKind.Account ? -1 : (int)kind;
    }

    private sealed class State
    {
        public Guid TransactionId { get; set; }

        public List<(LockKind Kind, Guid? Key)> Taken { get; } = [];
    }
}
