using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>The state the two sample rules read (T217): plain values of the order and one item, and today as a Turkey day.</summary>
internal sealed record SampleItemState(
    OrderStatus OrderStatus,
    PaymentStatus PaymentStatus,
    ProductType ProductType,
    DateOnly? DeliveryDate,
    int OpenQuantity,
    DateOnly Today);

/// <summary>
/// The test-only rule of 03 1.11 row 1.11.2 (cancelling a physical item): open after the payment is confirmed, until the order passes to Shipped, while the item
/// has an open quantity; the default and the upper limit are the open quantity (07 8.1.4; K-1170 (2)). The screens' real rules are written by their own tasks.
/// </summary>
internal sealed class SampleCancelRule : IOperationRule<SampleItemState>
{
    public OperationAvailability Evaluate(SampleItemState context)
    {
        ArgumentNullException.ThrowIfNull(context);
        bool open = context.ProductType == ProductType.Physical
            && context.PaymentStatus == PaymentStatus.Paid
            && context.OrderStatus is OrderStatus.Placed or OrderStatus.Preparing
            && context.OpenQuantity > 0;
        return open
            ? OperationAvailability.OpenForQuantity(context.OpenQuantity, context.OpenQuantity, context.OpenQuantity)
            : OperationAvailability.Closed;
    }
}

/// <summary>
/// The test-only rule of 03 1.11 row 1.11.5 (withdrawing from a physical item): once the order has passed to Shipped (Shipped, DeliveryFailed, Delivered), until
/// fourteen calendar days after the delivery date (Z-13) - the last day is inside; open while no delivery date is entered. The window length is the
/// rule's own constant: it is the table's legal period, not a product constant.
/// </summary>
internal sealed class SampleWithdrawRule : IOperationRule<SampleItemState>
{
    public const int WindowDays = 14;

    public OperationAvailability Evaluate(SampleItemState context)
    {
        ArgumentNullException.ThrowIfNull(context);
        bool shipped = context.OrderStatus is OrderStatus.Shipped or OrderStatus.DeliveryFailed or OrderStatus.Delivered;
        if (context.ProductType != ProductType.Physical || !shipped || context.OpenQuantity <= 0)
        {
            return OperationAvailability.Closed;
        }

        DateOnly? lastDay = context.DeliveryDate is DateOnly delivered ? TurkeyTime.LastDayOfPeriod(delivered, WindowDays) : null;
        if (lastDay is DateOnly end && context.Today > end)
        {
            return OperationAvailability.Closed;
        }

        return OperationAvailability.OpenForQuantity(context.OpenQuantity, context.OpenQuantity, context.OpenQuantity, lastDay);
    }
}

/// <summary>The two rules the sample page asks, in one object so a test can put recording rules in their place (T217 KK8).</summary>
internal sealed record SampleRules(IOperationRule<SampleItemState> Cancel, IOperationRule<SampleItemState> Withdraw)
{
    public static SampleRules Default() => new(new SampleCancelRule(), new SampleWithdrawRule());
}

/// <summary>What the sample page's GET shows: the state the rules read, the order's version and the item's id.</summary>
internal sealed record SampleOrderView(SampleItemState State, int Version, Guid ItemId);

/// <summary>
/// The stand-in of the module's open methods the sample page calls for the load and the write (09 8.3 item 3). A real screen calls methods of the module's
/// <c>Contracts</c> interface here (<c>IOrderingModule</c> and <c>IAfterSalesModule</c>, joining the page's transaction); the sample has no such method yet, so the
/// test assembly keeps the context and the entities behind this class and the page itself touches neither (09 6 item 2).
/// </summary>
internal sealed class SampleOrderOperations(AppDbContext db, TimeProvider time)
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    public async Task<SampleOrderView> ReadAsync(Guid orderId, CancellationToken ct)
    {
        Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, ct);
        OrderItem item = await db.OrderItems.AsNoTracking().SingleAsync(i => i.OrderId == orderId, ct);
        int closed = await db.Cancellations.AsNoTracking().Where(c => c.OrderItemId == item.Id).SumAsync(c => c.Quantity, ct);
        SampleItemState state = new(
            order.OrderStatus,
            order.PaymentStatus,
            item.ProductType,
            item.DeliveryDate,
            item.Quantity - closed,
            TurkeyTime.DateOf(time.GetUtcNow()));
        return new SampleOrderView(state, order.Version, item.Id);
    }

    public async Task CancelAsync(Guid orderId, int quantity, CancellationToken ct)
    {
        Order order = await db.Orders.SingleAsync(o => o.Id == orderId, ct);
        OrderItem item = await db.OrderItems.SingleAsync(i => i.OrderId == orderId, ct);
        db.Cancellations.Add(Cancellation.ByBuyer(item.Id, quantity, new ItemAmounts(100m, 10m, 15m), null, Now));
        order.BumpVersion();
    }
}
