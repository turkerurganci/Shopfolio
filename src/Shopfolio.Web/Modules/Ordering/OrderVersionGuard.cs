using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The order's version covers the whole order side (06 2.6.4, 4.17 "Sürüm"; 05 7.3.2; MK-21): the order, its items, the item records, the
/// payment record, the refund and the refund record's state, the customer's input bound to the order (the refund IBAN), the correction
/// histories and the reservations - every record linked to the order by a chain of foreign keys. A save made under an order's row lock
/// (<see cref="OrderLocks"/>) that writes any of them without raising that order's version by one is refused before any SQL is sent. The writes
/// that do not raise it are the closed list of 06 2.6.4: the records marked <see cref="IOrderVersionExempt"/> and <see cref="NonRaisingFields"/>, plus the links the
/// database empties when a product or a variant is deleted (<see cref="EmptiedLinks"/>). A new order needs no raise. A save without an order
/// lock (a record's own conditional update, a test's seed) is not looked at here; every order operation takes the lock (05 7.3.2). It does only
/// this check: events and cache invalidation are not attached to it.
/// </summary>
internal sealed class OrderVersionGuard : SaveChangesInterceptor
{
    /// <summary>
    /// Fields whose system write does not raise the order's version (06 2.6.4): the customer's download counter (its conditional update), the
    /// item's file link written when a file is removed or bound to a plane, and the order's account link and its deleted-account mark written when the account is deleted or
    /// a guest order is bound to an account.
    /// </summary>
    public static readonly IReadOnlyList<(string Type, string Field)> NonRaisingFields =
    [
        (VersionScope.NameOf(typeof(OrderItem)), nameof(OrderItem.DownloadCount)),
        (VersionScope.NameOf(typeof(OrderItem)), nameof(OrderItem.DigitalFileId)),
        (VersionScope.NameOf(typeof(Order)), nameof(Order.CustomerId)),
        (VersionScope.NameOf(typeof(Order)), nameof(Order.IsAccountDeleted)),
    ];

    /// <summary>
    /// The item's links emptied when a product or a variant is deleted (06 2.6.4: the database's <c>SET NULL</c>); the reservation goes with the
    /// variant by the database's cascade, outside any save.
    /// </summary>
    public static readonly IReadOnlyList<(string Type, string Field)> EmptiedLinks =
    [
        (VersionScope.NameOf(typeof(OrderItem)), nameof(OrderItem.ProductId)),
        (VersionScope.NameOf(typeof(OrderItem)), nameof(OrderItem.VariantId)),
    ];

    public static OrderVersionGuard Instance { get; } = new();

    /// <summary>
    /// The records whose writing never raises the order's version carry <see cref="IOrderVersionExempt"/> (06 2.6.4; 4.17 "Sürüm"): the
    /// internal note (its own conditional update), the dispatch record and its outcome, the transition record (written in a transition, which raises it), the audit trail,
    /// the job row and the inbound notification record (its handling raises the version if it applies a payment).
    /// </summary>
    public static VersionScope Scope { get; } = new(VersionScope.NameOf(typeof(Order)), null, typeof(IOrderVersionExempt), NonRaisingFields, EmptiedLinks);

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Check(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Check(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Check(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        IReadOnlyCollection<Guid> locked = LockSequence.LockedOrders(context);
        if (locked.Count == 0)
        {
            return;
        }

        foreach ((EntityEntry entry, Guid? found) in Scope.Writes(context))
        {
            // A record whose order the tracked records do not tell (its item or refund was not loaded) belongs to the one locked order;
            // with several locked orders the operation must load the record's parent.
            Guid orderId = found ?? (locked.Count == 1
                ? locked.First()
                : throw new InvalidOperationException(
                    $"The order of the {entry.Metadata.ClrType.Name} record cannot be told: load its parent record under the order lock (06 2.6.4)."));
            if (Scope.IsRaised(context, orderId))
            {
                continue;
            }

            if (!locked.Contains(orderId))
            {
                throw new InvalidOperationException(
                    $"A {entry.Metadata.ClrType.Name} record of order {orderId} is written without that order's row lock (05 7.3.2).");
            }

            throw new InvalidOperationException(
                $"A {entry.Metadata.ClrType.Name} record of order {orderId} is written and the order's version is not raised by one: call BumpVersion (06 2.6.4; 09 10.2 item 3).");
        }
    }
}
