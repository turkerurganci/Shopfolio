using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

// The sample module is module-shaped on purpose (T217; 09 8.3 items 1 and 3): the open interface and its values sit in the Ordering Contracts namespace and the
// implementation, with the rule, the lock, the load and the write, in the Ordering namespace, as the real module's will. The page reaches the interface only.
#pragma warning disable IDE0130
namespace Shopfolio.Modules.Ordering.Contracts
{
    /// <summary>What the sample module's read method gives the page's GET: the server's availability of the two operations, the order's version and the item's id.</summary>
    public sealed record SampleOrderOperationsView(OperationAvailability Cancel, OperationAvailability Withdraw, int Version, Guid ItemId);

    /// <summary>
    /// The sample module's open methods (the stand-in of the real module's <c>Contracts</c> interface): the read gives the availability the module's rule decided, the
    /// write methods run the same rule under the order's row lock inside the caller's transaction and apply only when it allows. They open no transaction (09 6 item 3).
    /// </summary>
    public interface ISampleOrderOperations
    {
        Task<SampleOrderOperationsView> ReadAsync(Guid orderId, CancellationToken ct);

        Task<GatedOperationResult> CancelAsync(Guid orderId, int? shownVersion, int quantity, CancellationToken ct);

        Task<GatedOperationResult> WithdrawAsync(Guid orderId, int? shownVersion, int quantity, CancellationToken ct);
    }
}

namespace Shopfolio.IntegrationTests.Pages
{
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

    /// <summary>The two rules the sample module holds, in one object so a test can put recording rules in their place (T217 KK8).</summary>
    internal sealed record SampleRules(IOperationRule<SampleItemState> Cancel, IOperationRule<SampleItemState> Withdraw)
    {
        public static SampleRules Default() => new(new SampleCancelRule(), new SampleWithdrawRule());
    }
}

namespace Shopfolio.Modules.Ordering
{
    using Shopfolio.IntegrationTests.Pages;

    /// <summary>
    /// The test-only stand-in of a module that owns its operations' rules (09 8.3 items 1 and 3): it holds the rule instance, answers the page's GET with the availability
    /// that rule decides and, in each write method, runs the sequence lock, version, load, the same rule instance, write (<see cref="OperationGate.ExecuteAsync{TContext}"/>)
    /// inside the transaction the caller opened. The write is a sample: an allowed operation records a cancellation of the quantity and bumps the order's version.
    /// </summary>
    internal sealed class SampleOrderOperations(AppDbContext db, OrderLocks locks, SampleRules rules, TimeProvider time) : ISampleOrderOperations
    {
        private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

        public async Task<SampleOrderOperationsView> ReadAsync(Guid orderId, CancellationToken ct)
        {
            (SampleItemState state, int version, Guid itemId) = await LoadAsync(orderId, ct);
            return new SampleOrderOperationsView(rules.Cancel.Evaluate(state), rules.Withdraw.Evaluate(state), version, itemId);
        }

        public Task<GatedOperationResult> CancelAsync(Guid orderId, int? shownVersion, int quantity, CancellationToken ct) =>
            RunAsync(orderId, shownVersion, rules.Cancel, quantity, ct);

        public Task<GatedOperationResult> WithdrawAsync(Guid orderId, int? shownVersion, int quantity, CancellationToken ct) =>
            RunAsync(orderId, shownVersion, rules.Withdraw, quantity, ct);

        private Task<GatedOperationResult> RunAsync(Guid orderId, int? shownVersion, IOperationRule<SampleItemState> rule, int quantity, CancellationToken ct) =>
            OperationGate.ExecuteAsync(
                stepCt => LockAsync(orderId, shownVersion, stepCt),
                async stepCt => (await LoadAsync(orderId, stepCt)).State,
                rule,
                quantity,
                (state, availability, stepCt) => WriteAsync(orderId, quantity, stepCt),
                ct);

        private async Task<bool> LockAsync(Guid orderId, int? shownVersion, CancellationToken ct)
        {
            await locks.LockAsync([orderId], ct);
            if (shownVersion is not int shown)
            {
                return true;
            }

            Order order = await db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, ct);
            return ShownVersion.Matches(order, shown);
        }

        private async Task<(SampleItemState State, int Version, Guid ItemId)> LoadAsync(Guid orderId, CancellationToken ct)
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
            return (state, order.Version, item.Id);
        }

        private async Task WriteAsync(Guid orderId, int quantity, CancellationToken ct)
        {
            Order order = await db.Orders.SingleAsync(o => o.Id == orderId, ct);
            OrderItem item = await db.OrderItems.SingleAsync(i => i.OrderId == orderId, ct);
            db.Cancellations.Add(Cancellation.ByBuyer(item.Id, quantity, new ItemAmounts(100m, 10m, 15m), null, Now));
            order.BumpVersion();
        }
    }
}
