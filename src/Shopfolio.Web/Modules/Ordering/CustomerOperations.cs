using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The customer's operations of the order page and of E-17 (T512; 07 UC-35, UC-38…UC-41). The screen is drawn from the one operation decision
/// (<see cref="OrderOperationCatalog.Decide"/>) the POST is checked against, and each write is the transition unit's own operation, which locks the order, asks the
/// decision again and refuses what is no longer open (09 8.3 item 3). A refusal is a rejected <see cref="GatedOperationResult"/>: the caller's runner rolls the
/// transaction back, so a request of several lines is applied whole or not at all (K-1202). The amounts are not worked out here: the refund and the shipping fee are the
/// transition unit's and the refund line's (05 4.6; K-789).
/// </summary>
internal sealed class CustomerOperations : ICustomerOperations
{
    private readonly AppDbContext _db;
    private readonly OrderOperations _operations;
    private readonly IOrderTransitions _transitions;
    private readonly IAfterSalesModule _afterSales;
    private readonly IPaymentModule _payment;
    private readonly OrderLocks _locks;

    public CustomerOperations(AppDbContext db, OrderOperations operations, IOrderTransitions transitions, IAfterSalesModule afterSales, IPaymentModule payment, OrderLocks locks)
    {
        _db = db;
        _operations = operations;
        _transitions = transitions;
        _afterSales = afterSales;
        _payment = payment;
        _locks = locks;
    }

    public async Task<CustomerOperationScreen?> ReadScreenAsync(Guid orderId, CustomerScreen screen, CancellationToken ct)
    {
        OrderOperationFacts? facts = await _operations.ReadFactsAsync(orderId, ct);
        if (facts is null)
        {
            return null;
        }

        IReadOnlyList<OrderOperationEntry> open = OrderOperationCatalog.Decide(facts, ActorKind.Buyer);
        Dictionary<Guid, OrderItem> items = await _db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).ToDictionaryAsync(i => i.Id, ct);
        List<OperationItemFacts> ordered = [.. facts.Items];
        CustomerScreenOperation operation;
        List<CustomerScreenItem> screenItems;
        if (screen == CustomerScreen.Cancellation && open.Any(e => e.Operation == OrderOperation.CancelOrder))
        {
            // The order's cancellation lists every item read-only (04 5.17.4).
            operation = CustomerScreenOperation.CancelOrder;
            screenItems = [.. ordered.Select(f => ReadOnlyItem(items[f.ItemId], f))];
        }
        else if (screen == CustomerScreen.Cancellation)
        {
            operation = CustomerScreenOperation.CancelOrderItems;
            screenItems =
            [
                .. ordered
                    .Select(f => (Facts: f, Entry: open.FirstOrDefault(e => e.OrderItemId == f.ItemId && IsItemCancellation(e.Operation))))
                    .Where(p => p.Entry is not null)
                    .Select(p => SelectableItem(items[p.Facts.ItemId], p.Facts, p.Entry!.Availability)), // non-null: the Where above keeps only the items with an entry
            ];
        }
        else if (open.Any(e => e.Operation == OrderOperation.TerminateForLateDelivery))
        {
            // The termination covers every open unit of the undelivered physical items (02 7.2.3; K-796): the same set the transition terminates.
            operation = CustomerScreenOperation.TerminateForLateDelivery;
            screenItems =
            [
                .. ordered.Where(f => f.ProductType == ProductType.Physical && f.DeliveryDate is null && f.OpenQuantity > 0).Select(f => ReadOnlyItem(items[f.ItemId], f)),
            ];
        }
        else
        {
            return null;
        }

        if (screenItems.Count == 0)
        {
            return null;
        }

        Dictionary<Guid, int> openPhysical = ordered.Where(f => f.ProductType == ProductType.Physical && f.OpenQuantity > 0).ToDictionary(f => f.ItemId, f => f.OpenQuantity);
        return new CustomerOperationScreen(
            operation,
            screenItems,
            facts.PaymentMethod,
            operation != CustomerScreenOperation.CancelOrder && IsPaidTransfer(facts.PaymentMethod, facts.PaymentStatus),
            RefundPreview.RefundPeriodDays,
            openPhysical);
    }

    public async Task<IReadOnlyList<RefundIbanFieldView>> ReadRefundIbanFieldsAsync(Guid orderId, CancellationToken ct)
    {
        List<RefundIbanFieldView> views = [];
        foreach (RefundIbanField field in await ReadFieldsAsync(orderId, ct))
        {
            views.Add(new RefundIbanFieldView(field.OrderItemId, field.Iban, field.RequestId is null ? null : field.Reason));
        }

        return views;
    }

    public async Task<CustomerOperationResult> SaveRefundIbanAsync(Guid orderId, Guid? orderItemId, string iban, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(iban);
        if (orderItemId is { } itemId && !await _db.OrderItems.AsNoTracking().AnyAsync(i => i.Id == itemId && i.OrderId == orderId, ct))
        {
            return Refused(CustomerOperationOutcome.NotFound);
        }

        // The open request of the place is answered first; with none, the IBAN on file there is corrected in place until the refund is processed (02 7.4.5; K-575).
        IReadOnlyList<RefundIbanField> fields = await ReadFieldsAsync(orderId, ct);
        OrderTransitionResult? result = null;
        if (fields.FirstOrDefault(f => f.OrderItemId == orderItemId && f.RequestId is not null) is { RequestId: Guid requestId })
        {
            result = await _transitions.EnterRefundIbanAsync(new EnterRefundIbanRequest(orderId, requestId, iban), ct);
        }
        else if (fields.FirstOrDefault(f => f.OrderItemId == orderItemId && f.RefundIbanId is not null) is { RefundIbanId: Guid refundIbanId })
        {
            result = await _transitions.ChangeRefundIbanAsync(new ChangeRefundIbanRequest(orderId, refundIbanId, iban), ct);
        }

        return result switch
        {
            null => Refused(CustomerOperationOutcome.Closed),
            { Refusal: TransitionRefusal.InvalidInput } => Refused(CustomerOperationOutcome.InvalidIban),
            { IsRejected: true } => Refused(CustomerOperationOutcome.Closed),
            _ => Applied(),
        };
    }

    public async Task<CustomerOperationResult> CancelOrderAsync(Guid orderId, CancellationToken ct)
    {
        await _locks.LockAsync([orderId], ct);
        OrderOperationFacts? facts = await _operations.ReadFactsAsync(orderId, ct);
        if (facts is null || !OrderOperationCatalog.Check(facts, OrderOperation.CancelOrder, null, null).IsAllowed)
        {
            return Refused(CustomerOperationOutcome.Closed);
        }

        OrderTransitionResult result = await _transitions.CancelOrderAsync(CancelOrderRequest.ByCustomer(orderId), ct);
        return result.IsRejected ? Refused(CustomerOperationOutcome.Closed) : Applied();
    }

    public async Task<CustomerOperationResult> CancelOrderItemsAsync(Guid orderId, IReadOnlyList<CancelOrderItemsLine> lines, string? iban, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(lines);
        if (lines.Count == 0 || lines.Any(l => l.Quantity < 1) || lines.Select(l => l.OrderItemId).Distinct().Count() != lines.Count)
        {
            throw new ArgumentException("The page sends at least one line, one per item, each of at least one unit (07 UC-40 422).", nameof(lines));
        }

        await _locks.LockAsync([orderId], ct);
        OrderOperationFacts? facts = await _operations.ReadFactsAsync(orderId, ct);
        if (facts is null)
        {
            return Refused(CustomerOperationOutcome.Closed);
        }

        // The whole request is checked against the state as it is now before any line is written (K-1170 (4), K-1202): an item not the order's is 404, a line whose
        // operation closed is closed, a quantity above the open units or an open quantity other than the one shown refuses the request whole.
        foreach (CancelOrderItemsLine line in lines)
        {
            if (facts.Items.FirstOrDefault(i => i.ItemId == line.OrderItemId) is not { } item)
            {
                return Refused(CustomerOperationOutcome.NotFound);
            }

            OrderOperation operation = OrderTransitions.CancellationOperationOf(item.ProductType);
            OperationVerdict verdict = OrderOperationCatalog.Check(facts, operation, item.ItemId, line.Quantity);
            if (verdict.Decision == OperationDecision.Closed)
            {
                return Refused(CustomerOperationOutcome.Closed);
            }

            if (verdict.Decision == OperationDecision.QuantityExceedsOpen || line.OriginalOpenQuantity != item.OpenQuantity)
            {
                return Refused(CustomerOperationOutcome.QuantityExceedsOpen);
            }
        }

        if (IsPaidTransfer(facts.PaymentMethod, facts.PaymentStatus) && string.IsNullOrWhiteSpace(iban))
        {
            return Refused(CustomerOperationOutcome.InvalidIban);
        }

        // Each line's refund is worked out when its record is written, from the records already there (05 4.6): the line that meets the shipping fee's trigger must be
        // written last, and of records of one moment the fee goes to the smallest item id (K-1107; ShippingFeeRefunds) - so the lines go in descending uuid order and
        // the record that covers the last physical units, whether it shares the moment with the others or not, is the one that carries the fee (K-789).
        foreach (CancelOrderItemsLine line in lines.OrderByDescending(l => l.OrderItemId.ToString("N"), StringComparer.Ordinal))
        {
            OrderTransitionResult result = await _transitions.CancelItemAsync(new CancelItemRequest(orderId, line.OrderItemId, line.Quantity, iban), ct);
            if (result.IsRejected)
            {
                return Refused(result.Refusal == TransitionRefusal.InvalidInput ? CustomerOperationOutcome.InvalidIban : CustomerOperationOutcome.Closed);
            }
        }

        return Applied();
    }

    public async Task<CustomerOperationResult> TerminateForLateDeliveryAsync(Guid orderId, string? iban, CancellationToken ct)
    {
        OrderTransitionResult result = await _transitions.TerminateForLateDeliveryAsync(TerminateForLateDeliveryRequest.ByCustomer(orderId, iban), ct);
        return result switch
        {
            { IsRejected: false } => Applied(),
            { Refusal: TransitionRefusal.InvalidInput } => Refused(CustomerOperationOutcome.InvalidIban),
            _ => Refused(CustomerOperationOutcome.Closed),
        };
    }

    /// <summary>The refund of a paid transfer order goes to the customer's IBAN (02 7.2.8, 7.4.5): the same condition the transition unit asks the IBAN on.</summary>
    private static bool IsPaidTransfer(PaymentMethod method, PaymentStatus status) =>
        method == PaymentMethod.BankTransfer && status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded;

    private static bool IsItemCancellation(OrderOperation operation) =>
        operation is OrderOperation.CancelPhysicalItem or OrderOperation.CancelServiceItem or OrderOperation.CancelDigitalItem;

    private static CustomerScreenItem ReadOnlyItem(OrderItem item, OperationItemFacts facts) =>
        new(item.Id, item.ProductName, item.FirstOptionValue, item.SecondOptionValue, item.ProductType, item.Quantity, facts.OpenQuantity, facts.OpenQuantity, facts.OpenQuantity, false);

    /// <summary>The item of the item cancellation with the decision's three values (K-1170 (2)); a digital item and an item of one unit have no quantity field (04 2.12.1.8).</summary>
    private static CustomerScreenItem SelectableItem(OrderItem item, OperationItemFacts facts, OperationAvailability availability)
    {
        int open = availability.HasQuantity ? availability.OpenQuantity : facts.OpenQuantity;
        int max = availability.HasQuantity ? availability.MaxQuantity : facts.OpenQuantity;
        int defaultQuantity = availability.HasQuantity ? availability.DefaultQuantity : facts.OpenQuantity;
        bool hasField = item.ProductType != ProductType.Digital && item.Quantity > 1;
        return new(item.Id, item.ProductName, item.FirstOptionValue, item.SecondOptionValue, item.ProductType, item.Quantity, open, defaultQuantity, max, hasField);
    }

    /// <summary>The fields of the order: an itemless request is kept only when its refund is the order's (K-1075).</summary>
    private async Task<IReadOnlyList<RefundIbanField>> ReadFieldsAsync(Guid orderId, CancellationToken ct)
    {
        Guid[] itemIds = [.. await _db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).Select(i => i.Id).ToListAsync(ct)];
        List<RefundIbanField> fields = [];
        foreach (RefundIbanField field in await _afterSales.ListRefundIbanFieldsAsync(orderId, itemIds, ct))
        {
            if (field.OrderItemId is null && field.RefundId is { } refundId && await _payment.ReadRefundLineAsync(orderId, refundId, ct) is null)
            {
                continue;
            }

            fields.Add(field);
        }

        return fields;
    }

    private static CustomerOperationResult Applied() =>
        new(GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Allowed, OperationAvailability.Open())), CustomerOperationOutcome.Applied);

    private static CustomerOperationResult Refused(CustomerOperationOutcome outcome) =>
        new(
            GatedOperationResult.Decided(new OperationVerdict(
                outcome == CustomerOperationOutcome.QuantityExceedsOpen ? OperationDecision.QuantityExceedsOpen : OperationDecision.Closed,
                OperationAvailability.Closed)),
            outcome);
}
