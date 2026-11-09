using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Administration;

/// <summary>
/// The sales summary's measures as pure rules (02 10.6.2, 10.6.3; 11 T383): the facts come from the records through the modules' surfaces, the clock from the caller; nothing here
/// reads a table, a clock or a setting, and nothing is kept.
/// </summary>
internal static class SalesSummaryRules
{
    /// <summary>The number of best-selling products the summary lists (04 9.22.6; K-821).</summary>
    public const int TopSellerCount = 10;

    /// <summary>
    /// The best-selling products (04 9.22.6; K-821): the most units sold first, an equal number by the higher amount, then by name so the order is stable; the first
    /// <see cref="TopSellerCount"/>.
    /// </summary>
    public static IReadOnlyList<TopSeller> TopSellers(IEnumerable<ProductSalesFact> products)
    {
        ArgumentNullException.ThrowIfNull(products);
        return
        [
            .. products
                .OrderByDescending(p => p.Quantity)
                .ThenByDescending(p => p.Amount)
                .ThenBy(p => p.ProductName, StringComparer.Ordinal)
                .Take(TopSellerCount)
                .Select(p => new TopSeller(p.ProductId, p.ProductName, p.Quantity, p.Amount)),
        ];
    }

    /// <summary>
    /// The payment completion ratio (02 10.6.3; K-181, K-182, K-296, K-534, K-1052): the attempts that reached the confirmation in the period, and of them those that ended with an
    /// approved payment. The orders of one cart, in order of birth, are one attempt until an order of it is paid - a failed payment's retry with a new order and the new order that
    /// cancelled the earlier one of the same cart are the same attempt; an order after a paid one starts a new attempt (the cart was emptied by the payment). An order without a
    /// cart (the disposal emptied its link) is an attempt of its own. The attempt belongs to the period its first order was born in. An attempt with an order whose payment is still
    /// pending and whose payment window has not run out at <paramref name="now"/> is not counted yet; it enters when the window runs out.
    /// </summary>
    public static SummaryRatio PaymentCompletion(IEnumerable<PaymentAttemptOrder> orders, DateTimeOffset from, DateTimeOffset to, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(orders);
        int attempts = 0;
        int completed = 0;
        foreach (List<PaymentAttemptOrder> attempt in Attempts(orders))
        {
            DateTimeOffset bornAt = attempt[0].CreatedAt;
            if (bornAt < from || bornAt >= to)
            {
                continue;
            }

            if (attempt.Any(o => o.PaymentStatus == PaymentStatus.Pending && o.PaymentWindowEndsAt > now))
            {
                continue;
            }

            attempts++;
            if (attempt.Any(o => IsApproved(o.PaymentStatus)))
            {
                completed++;
            }
        }

        return new SummaryRatio(completed, attempts);
    }

    /// <summary>
    /// The shipping promise compliance ratio (02 10.6.3; K-139, K-140, K-484, K-535): of the orders whose promise ran out in the period and that still had a physical unit to ship
    /// when it ran out, those first shipped before it ran out. An order is left out when it was cancelled (S3 or S4) before the promise ran out, or when its physical items were
    /// all closed by item records before then - <paramref name="openAtPromiseEnd"/>, the open quantity of each physical item at its order's promise end, is zero for every one
    /// of them (the mixed order whose physical items were cancelled or removed one by one; a service or digital item left open does not bring it in). An order not yet shipped
    /// when the promise ran out has not kept it - shipped later, cancelled or terminated later alike; an item missing from <paramref name="openAtPromiseEnd"/> counts as open.
    /// </summary>
    public static SummaryRatio ShippingPromiseCompliance(IEnumerable<ShippingPromiseOrder> orders, IReadOnlyDictionary<Guid, int> openAtPromiseEnd)
    {
        ArgumentNullException.ThrowIfNull(orders);
        ArgumentNullException.ThrowIfNull(openAtPromiseEnd);
        int counted = 0;
        int kept = 0;
        foreach (ShippingPromiseOrder order in orders)
        {
            bool shippedInTime = order.FirstShippedAt is { } shippedAt && shippedAt < order.PromiseEndsAt;
            bool cancelledInTime = order.CancelledAt is { } cancelledAt && cancelledAt < order.PromiseEndsAt;
            bool physicalClosedInTime = order.PhysicalItems.Count > 0
                && order.PhysicalItems.All(i => openAtPromiseEnd.TryGetValue(i.OrderItemId, out int open) && open == 0);
            if (!shippedInTime && (cancelledInTime || physicalClosedInTime))
            {
                continue;
            }

            counted++;
            if (shippedInTime)
            {
                kept++;
            }
        }

        return new SummaryRatio(kept, counted);
    }

    /// <summary>
    /// The cancellation and return ratio (02 10.6.3; K-790): of the approved orders born in the period, those with at least one item - or one unit of an item - cancelled, removed,
    /// terminated for late delivery or withdrawn. An order counts once whatever the number of its records.
    /// </summary>
    public static SummaryRatio CancellationAndReturn(IReadOnlyCollection<OrderOfItem> paidItems, IReadOnlySet<Guid> itemsWithLoss)
    {
        ArgumentNullException.ThrowIfNull(paidItems);
        ArgumentNullException.ThrowIfNull(itemsWithLoss);
        int orders = paidItems.Select(i => i.OrderId).Distinct().Count();
        int withLoss = paidItems.Where(i => itemsWithLoss.Contains(i.OrderItemId)).Select(i => i.OrderId).Distinct().Count();
        return new SummaryRatio(withLoss, orders);
    }

    private static IEnumerable<List<PaymentAttemptOrder>> Attempts(IEnumerable<PaymentAttemptOrder> orders)
    {
        foreach (IGrouping<Guid?, PaymentAttemptOrder> cart in orders.GroupBy(o => o.CartId))
        {
            IOrderedEnumerable<PaymentAttemptOrder> born = cart.OrderBy(o => o.CreatedAt).ThenBy(o => o.OrderId);
            if (cart.Key is null)
            {
                foreach (PaymentAttemptOrder order in born)
                {
                    yield return [order];
                }

                continue;
            }

            List<PaymentAttemptOrder> current = [];
            foreach (PaymentAttemptOrder order in born)
            {
                current.Add(order);
                if (IsApproved(order.PaymentStatus))
                {
                    yield return current;
                    current = [];
                }
            }

            if (current.Count > 0)
            {
                yield return current;
            }
        }
    }

    private static bool IsApproved(PaymentStatus status) => status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded;
}
