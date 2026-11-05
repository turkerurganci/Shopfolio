using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The change the return from an outage makes on the bank transfer orders (05 5.4.3; 02 4.3, Z-8): an order whose payment window ran out while
/// the site was down is not cancelled; its window ends at the end of the first business day after the day of the return, and its reminder,
/// which belonged to the old window, is dropped. It runs inside the heartbeat's return transaction (<see cref="OutageRecovery"/>), so it
/// takes the orders' row lock first (09 9.1 item 5), loads them after it, and raises each order's version (06 2.6.4). The window's old end
/// is the check: an order whose window ended before the gap began, or after the return, is not touched.
/// </summary>
internal sealed class TransferWindowDeferral : IOutageEffect
{
    private readonly AppDbContext _db;
    private readonly OrderLocks _locks;

    public TransferWindowDeferral(AppDbContext db, OrderLocks locks)
    {
        _db = db;
        _locks = locks;
    }

    public async Task ApplyAsync(DateTimeOffset startedAt, DateTimeOffset endedAt, CancellationToken ct)
    {
        List<Guid> candidates = await _db.Orders.AsNoTracking()
            .Where(o => o.PaymentMethod == PaymentMethod.BankTransfer
                && o.PaymentStatus == PaymentStatus.Pending
                && o.OrderStatus == OrderStatus.Placed
                && o.PaymentWindowEndsAt > startedAt
                && o.PaymentWindowEndsAt <= endedAt)
            .Select(o => o.Id)
            .ToListAsync(ct);
        if (candidates.Count == 0)
        {
            return;
        }

        await _locks.LockAsync(candidates, ct);

        // The orders are loaded after the lock and the condition is asked again: an order that was paid or cancelled meanwhile is left as it is.
        List<Order> orders = await _db.Orders
            .Where(o => candidates.Contains(o.Id)
                && o.PaymentMethod == PaymentMethod.BankTransfer
                && o.PaymentStatus == PaymentStatus.Pending
                && o.OrderStatus == OrderStatus.Placed
                && o.PaymentWindowEndsAt > startedAt
                && o.PaymentWindowEndsAt <= endedAt)
            .ToListAsync(ct);
        DateTimeOffset newEnd = TransferSchedule.DeferredWindowEnd(endedAt, BusinessCalendar.Product);
        foreach (Order order in orders)
        {
            order.PaymentWindowEndsAt = newEnd;
            order.PaymentReminderAt = null;
            order.BumpVersion();
        }
    }
}
