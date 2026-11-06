using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Catalog.Contracts;
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
