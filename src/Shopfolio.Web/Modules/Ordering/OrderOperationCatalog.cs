using System.Collections.Frozen;

using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>What an operation is about: the order as a whole, one item of it, or both (the firm's cancellation is the whole unpaid order before the payment and an item after it).</summary>
internal enum OperationScope
{
    Order,
    Item,
    OrderOrItem,
}

/// <summary>The two steps 03 1.11.32 holds in one row: the retry or the transfer route of a card refund that did not come through, and the administrator's "gerçekleşmedi" mark on one whose outcome is awaited.</summary>
internal enum CardRefundOperationStep
{
    RetryOrRedirect,
    MarkNotRealized,
}

/// <summary>One row of the decision table: the operation, its row identity in 03 1.11, whose operation it is, what it is about and its one rule function (09 8.3 item 1).</summary>
internal sealed record OperationRow(OrderOperation Operation, string Id, ActorKind Actor, OperationScope Scope, IOperationRule<OperationInput> Rule);

/// <summary>
/// The decision table of the forty-one operations of 03 1.11 (T323; 09 8.3 item 1): the only place that says which operation is open in which state. A row is the row identity of
/// 03 1.11, the side it belongs to and its rule, written once from the table's "açık olduğu aralık" column; the order page and the panel draw from <see cref="Decide"/> and every POST
/// asks <see cref="Check"/>, both over the same facts and the same rule instance. The rule reads plain values (<see cref="OperationInput"/>) and no clock, database or request.
/// An operation that is not in the table is closed (<see cref="Check"/>): the table is the list of what can be open, not a list of what is shut. The periods (Z-13, Z-14, Z-18, Z-42)
/// are worked out by the AfterSales module's own rules and arrive in the item's <see cref="ItemPeriodState"/>; Z-11 is <see cref="DeliveryPeriodRules"/>'s. The equality with the document
/// is <c>OperationTableConsistencyTests</c> (09 21.4 item 1).
/// </summary>
internal static class OrderOperationCatalog
{
    private sealed class Rule(Func<OperationInput, OperationAvailability> evaluate) : IOperationRule<OperationInput>
    {
        public OperationAvailability Evaluate(OperationInput context) => evaluate(context);
    }

    private static OperationRow Row(OrderOperation operation, ActorKind actor, OperationScope scope, Func<OperationInput, OperationAvailability> evaluate) =>
        new(operation, "1.11." + ((int)operation + 1), actor, scope, new Rule(evaluate));

    private static OperationRow Customer(OrderOperation operation, OperationScope scope, Func<OperationInput, OperationAvailability> evaluate) => Row(operation, ActorKind.Buyer, scope, evaluate);

    private static OperationRow Firm(OrderOperation operation, OperationScope scope, Func<OperationInput, OperationAvailability> evaluate) => Row(operation, ActorKind.Admin, scope, evaluate);

    /// <summary>The forty-one rows in the document's order: the customer's fourteen, then the administrator's twenty-seven.</summary>
    public static IReadOnlyList<OperationRow> Rows { get; } =
    [
        // ---- Customer - the order page (03 1.11.1-1.11.14) ----
        Customer(OrderOperation.CancelOrder, OperationScope.Order, i => Open(i.Order.OrderStatus == OrderStatus.Placed && i.Order.PaymentStatus == PaymentStatus.Pending)),
        Customer(OrderOperation.CancelPhysicalItem, OperationScope.Item, i => ItemCancellable(i, ProductType.Physical)),
        Customer(OrderOperation.CancelServiceItem, OperationScope.Item, i => ItemCancellable(i, ProductType.Service)),
        Customer(OrderOperation.CancelDigitalItem, OperationScope.Item, _ => OperationAvailability.Closed),
        Customer(OrderOperation.WithdrawPhysicalItem, OperationScope.Item, WithdrawPhysical),
        Customer(OrderOperation.WithdrawServiceItem, OperationScope.Item, WithdrawService),
        Customer(OrderOperation.WithdrawDigitalItem, OperationScope.Item, _ => OperationAvailability.Closed),
        Customer(OrderOperation.TerminateForLateDelivery, OperationScope.Order, i => Open(IsLateTerminationOpen(i.Order))),
        Customer(OrderOperation.OpenDefectClaim, OperationScope.Item, OpenDefectClaim),
        Customer(OrderOperation.ReopenDefectClaimByCustomer, OperationScope.Item, i => i.Item is { Period: { DefectClaim: DefectClaimStatus.Resolved, DefectChannelOpen: true } p } ? OperationAvailability.Open(p.DefectLastDay) : OperationAvailability.Closed),
        Customer(OrderOperation.EnterRefundIban, OperationScope.Order, i => Open(i.Order.HasOpenIbanRequest)),
        Customer(OrderOperation.CorrectRefundIban, OperationScope.Order, i => Open(i.Order.HasRefundIban)),
        Customer(OrderOperation.ViewTransferDetails, OperationScope.Order, i => Open(i.Order.PaymentMethod == PaymentMethod.BankTransfer && i.Order.HasTransferDetails && i.Order.OrderStatus == OrderStatus.Placed && i.Order.PaymentStatus == PaymentStatus.Pending)),
        Customer(OrderOperation.DownloadDigitalFile, OperationScope.Item, i => Open(i.Item is { ProductType: ProductType.Digital, DownloadsLeft: > 0 } && DownloadRules.IsPaymentConfirmed(i.Order.PaymentStatus))),

        // ---- Administrator - the panel (03 1.11.15-1.11.41) ----
        Firm(OrderOperation.MarkTransferPaid, OperationScope.Order, i => Open(i.Order.PaymentMethod == PaymentMethod.BankTransfer && i.Order.OrderStatus == OrderStatus.Placed && i.Order.PaymentStatus == PaymentStatus.Pending)),
        Firm(OrderOperation.ShipOrder, OperationScope.Order, i => Open(i.Order.OrderStatus == OrderStatus.Preparing && i.Order.HasOpenPhysicalItem)),
        Firm(OrderOperation.MarkDelivered, OperationScope.Order, i => Open(i.Order.OrderStatus == OrderStatus.Shipped)),
        Firm(OrderOperation.MarkDeliveryFailed, OperationScope.Order, i => Open(i.Order.OrderStatus == OrderStatus.Shipped)),
        Firm(OrderOperation.ResendOrder, OperationScope.Order, i => Open(i.Order.OrderStatus == OrderStatus.DeliveryFailed && i.Order.HasOpenPhysicalItem)),
        Firm(OrderOperation.MarkServiceCompleted, OperationScope.Item, i => Open(i.Item is { ProductType: ProductType.Service, IsDeliveryMarked: false, OpenQuantity: > 0 } && i.Order.PaymentApproved)),
        Firm(OrderOperation.CancelByFirm, OperationScope.OrderOrItem, CancelByFirm),
        Firm(OrderOperation.RemoveOrReduceItem, OperationScope.Item, RemoveOrReduce),
        Firm(OrderOperation.CorrectAddress, OperationScope.Order, i => Open(IsAddressCorrectable(i.Order))),
        Firm(OrderOperation.CorrectCarrier, OperationScope.Order, i => Open(i.Order.HasPhysicalItem && i.Order.HasShipped)),
        Firm(OrderOperation.CorrectDeliveryOrArrivalDate, OperationScope.Order, i => Open(i.Order.Items.Any(item => item.DeliveryDate is not null || item.Period.HasArrivalRecord))),
        Firm(OrderOperation.CorrectTransition, OperationScope.Order, i => Open(i.Order.HasCorrectableTransition)),
        Firm(OrderOperation.WriteInternalNote, OperationScope.Order, i => Open(!i.Order.PersonalDataDisposed)),
        Firm(OrderOperation.ReceiveReturnedGoods, OperationScope.Item, i => Units(i.Item?.Period.ReceivableUnits ?? 0)),
        Firm(OrderOperation.RejectReturnedGoods, OperationScope.Item, i => Units(i.Item?.Period.RejectableUnits ?? 0)),
        Firm(OrderOperation.CloseAsNotReturned, OperationScope.Item, i => Units(i.Item?.Period.NotReturnedClosableUnits ?? 0)),
        Firm(OrderOperation.ProcessRefund, OperationScope.Order, i => Open(IsRefundProcessable(i.Order))),
        Firm(OrderOperation.RetryOrRedirectCardRefund, OperationScope.Order, i => Open(IsCardRefundStepOpen(i.Order, CardRefundOperationStep.RetryOrRedirect) || IsCardRefundStepOpen(i.Order, CardRefundOperationStep.MarkNotRealized))),
        Firm(OrderOperation.ReportTransferRefundFailed, OperationScope.Order, i => Open(i.Order.PaymentMethod == PaymentMethod.BankTransfer && i.Order.HasRefundIban)),
        Firm(OrderOperation.RecordWithdrawalFromOtherChannel, OperationScope.Item, RecordWithdrawal),
        Firm(OrderOperation.CorrectGuestEmail, OperationScope.Order, i => Open(i.Order.IsGuestOrder && !i.Order.PersonalDataDisposed)),
        Firm(OrderOperation.ResendEmail, OperationScope.Order, i => Open(i.Order.EmailNotDelivered)),
        Firm(OrderOperation.ResolveOrReopenDefectClaim, OperationScope.Item, ResolveOrReopen),
        Firm(OrderOperation.RenewDownloadAllowance, OperationScope.Item, i => Open(i.Item is { ProductType: ProductType.Digital })),
        Firm(OrderOperation.RequestRefundIban, OperationScope.Order, i => Open(i.Order.PaymentMethod == PaymentMethod.BankTransfer && !i.Order.HasRefundIban && !i.Order.HasOpenIbanRequest && IsRefundProcessableAmount(i.Order))),
        Firm(OrderOperation.CloseOrder, OperationScope.Order, i => Open(i.Order.OrderStatus == OrderStatus.DeliveryFailed && i.Order.Items.All(item => item.OpenQuantity == 0 && !item.IsDeliveryMarked))),
        Firm(OrderOperation.RecordLateTerminationFromOtherChannel, OperationScope.Order, i => Open(IsLateTerminationOpen(i.Order))),
    ];

    private static readonly FrozenDictionary<OrderOperation, OperationRow> ByOperation = Rows.ToFrozenDictionary(r => r.Operation);

    /// <summary>The row of the operation, or null when the value is no operation of the table.</summary>
    public static OperationRow? Find(OrderOperation operation) => ByOperation.GetValueOrDefault(operation);

    /// <summary>
    /// The operations open to <paramref name="actor"/> now: for each row of that side, the order-level availability when the row is about the order, and the item-level one for each
    /// item when it is about the items. A closed operation is not in the list. The customer's rows and the administrator's rows never mix (no role model; 02 10.1.2).
    /// </summary>
    public static IReadOnlyList<OrderOperationEntry> Decide(OrderOperationFacts facts, ActorKind actor)
    {
        ArgumentNullException.ThrowIfNull(facts);
        List<OrderOperationEntry> open = [];
        foreach (OperationRow row in Rows.Where(r => r.Actor == actor))
        {
            if (row.Scope is OperationScope.Order or OperationScope.OrderOrItem)
            {
                Add(open, row, null, row.Rule.Evaluate(new OperationInput(facts, null)));
            }

            if (row.Scope is OperationScope.Item or OperationScope.OrderOrItem)
            {
                foreach (OperationItemFacts item in facts.Items)
                {
                    Add(open, row, item.ItemId, row.Rule.Evaluate(new OperationInput(facts, item)));
                }
            }
        }

        return open;
    }

    /// <summary>
    /// The POST's check of one request (09 8.3 item 3): the operation must be in the table, the item (when the row is about an item) must be the order's, and the same rule function
    /// the screen drew from must say open; the quantity is compared with the rule's upper limit through the gate. Anything else is closed - a forged operation number, an item of
    /// another order, an item named on an order-level operation.
    /// </summary>
    public static OperationVerdict Check(OrderOperationFacts facts, OrderOperation operation, Guid? orderItemId, int? quantity)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (Find(operation) is not { } row)
        {
            return Closed();
        }

        OperationItemFacts? item = null;
        if (orderItemId is { } itemId)
        {
            item = facts.Items.FirstOrDefault(i => i.ItemId == itemId);
            if (item is null || row.Scope == OperationScope.Order)
            {
                return Closed();
            }
        }
        else if (row.Scope == OperationScope.Item)
        {
            return Closed();
        }

        return OperationGate.Decide(row.Rule, new OperationInput(facts, item), quantity);
    }

    /// <summary>
    /// Whether one of the two steps of 03 1.11.32 is open: retrying the card refund or opening the transfer route needs the "geri ödeme gerçekleşmedi" mark and no IBAN yet; marking
    /// the refund as not realized needs the "geri ödeme sonucu bekleniyor" mark (K-940). While the outcome is awaited only the second is open. The row's rule and the screen's button
    /// ask this one function.
    /// </summary>
    public static bool IsCardRefundStepOpen(OrderOperationFacts facts, CardRefundOperationStep step)
    {
        ArgumentNullException.ThrowIfNull(facts);
        if (facts.PaymentMethod != PaymentMethod.Card)
        {
            return false;
        }

        return step switch
        {
            CardRefundOperationStep.RetryOrRedirect => facts.CardRefundNotRealized && !facts.HasRefundIban && !facts.CardRefundOutcomePending,
            CardRefundOperationStep.MarkNotRealized => facts.CardRefundOutcomePending,
            _ => false,
        };
    }

    /// <summary>The POST's check of one step of 03 1.11.32: the operation's row must be open and the step itself must be the open one.</summary>
    public static OperationVerdict CheckCardRefundStep(OrderOperationFacts facts, CardRefundOperationStep step)
    {
        ArgumentNullException.ThrowIfNull(facts);
        return IsCardRefundStepOpen(facts, step) ? Check(facts, OrderOperation.RetryOrRedirectCardRefund, null, null) : Closed();
    }

    private static OperationVerdict Closed() => new(OperationDecision.Closed, OperationAvailability.Closed);

    private static void Add(List<OrderOperationEntry> open, OperationRow row, Guid? itemId, OperationAvailability availability)
    {
        if (availability.IsOpen)
        {
            open.Add(new OrderOperationEntry(row.Operation, itemId, availability));
        }
    }

    private static OperationAvailability Open(bool condition) => condition ? OperationAvailability.Open() : OperationAvailability.Closed;

    /// <summary>An operation on the units of one item: open for the units the AfterSales records still allow, with the quantity chosen up to them.</summary>
    private static OperationAvailability Units(int units) => units > 0 ? OperationAvailability.OpenForQuantity(units, units, units) : OperationAvailability.Closed;

    private static OperationAvailability UnitsUntil(int units, DateOnly? lastDay) => units > 0 ? OperationAvailability.OpenForQuantity(units, units, units, lastDay) : OperationAvailability.Closed;

    /// <summary>1.11.2, 1.11.3 - the customer's cancellation of a paid physical or service item (a digital item has none: 1.11.4).</summary>
    private static OperationAvailability ItemCancellable(OperationInput input, ProductType type)
    {
        (OrderOperationFacts order, OperationItemFacts? item) = (input.Order, input.Item);
        if (item is null || item.ProductType != type || !order.PaymentApproved || order.OrderStatus == OrderStatus.Cancelled)
        {
            return OperationAvailability.Closed;
        }

        // A physical item is cancelled until the order passes to Shipped; a service item until its "tamamlandı" mark, whatever the shipment status is (1.11.3).
        bool inInterval = type == ProductType.Physical ? order.OrderStatus is OrderStatus.Placed or OrderStatus.Preparing : !item.IsDeliveryMarked;
        return inInterval ? Units(item.OpenQuantity) : OperationAvailability.Closed;
    }

    /// <summary>1.11.5 - withdrawing from a shipped physical item inside Z-13; the absolute exemption has no withdrawal, the conditional one is open under its condition.</summary>
    private static OperationAvailability WithdrawPhysical(OperationInput input)
    {
        if (input.Item is not { ProductType: ProductType.Physical } item || !input.Order.HasShipped)
        {
            return OperationAvailability.Closed;
        }

        bool excluded = item.Period.IsExempt && !item.Period.IsConditionallyExempt;
        return !excluded && item.Period.WithdrawalWindowOpen ? UnitsUntil(item.OpenQuantity, item.Period.WithdrawalLastDay) : OperationAvailability.Closed;
    }

    /// <summary>1.11.6 - withdrawing from a paid service item until its "tamamlandı" mark or the end of Z-14, whichever comes first.</summary>
    private static OperationAvailability WithdrawService(OperationInput input)
    {
        if (input.Item is not { ProductType: ProductType.Service } item || !input.Order.PaymentApproved || input.Order.OrderStatus == OrderStatus.Cancelled)
        {
            return OperationAvailability.Closed;
        }

        return item.Period.WithdrawalWindowOpen ? UnitsUntil(item.OpenQuantity, item.Period.WithdrawalLastDay) : OperationAvailability.Closed;
    }

    /// <summary>1.11.8, 1.11.41 - the late delivery termination (Z-11): Hazırlanıyor, Kargoya verildi and Teslim edilemedi, with a physical item that has no delivery date and open units.</summary>
    private static bool IsLateTerminationOpen(OrderOperationFacts order) =>
        order.OrderStatus is OrderStatus.Preparing or OrderStatus.Shipped or OrderStatus.DeliveryFailed
        && DeliveryPeriodRules.IsLateDeliveryTerminationOpen(
            order.OrderStatus,
            order.PaymentStatus,
            order.CreatedAt,
            order.Items.Select(i => new ItemDeliveryState(i.ProductType, i.OpenQuantity, i.DeliveryDate)),
            order.Now);

    /// <summary>1.11.9 - a defect claim: one claim per item (K-676), the item's channel open (Z-18), the units chosen from the open ones, one by default.</summary>
    private static OperationAvailability OpenDefectClaim(OperationInput input)
    {
        if (input.Item is not { } item || item.Period.DefectClaim is not null || !item.Period.DefectChannelOpen || item.OpenQuantity <= 0)
        {
            return OperationAvailability.Closed;
        }

        return OperationAvailability.OpenForQuantity(item.OpenQuantity, 1, item.OpenQuantity, item.Period.DefectLastDay);
    }

    /// <summary>1.11.21 - the firm's cancellation: the whole unpaid order before the payment (S3); after it, by item and units inside the item type's cancellation interval, a returned open physical item included.</summary>
    private static OperationAvailability CancelByFirm(OperationInput input)
    {
        (OrderOperationFacts order, OperationItemFacts? item) = (input.Order, input.Item);
        if (item is null)
        {
            return Open(order.OrderStatus == OrderStatus.Placed && order.PaymentStatus == PaymentStatus.Pending);
        }

        if (!order.PaymentApproved || order.OrderStatus == OrderStatus.Cancelled)
        {
            return OperationAvailability.Closed;
        }

        bool inInterval = item.ProductType switch
        {
            ProductType.Physical => order.OrderStatus is OrderStatus.Placed or OrderStatus.Preparing or OrderStatus.DeliveryFailed,
            ProductType.Service => !item.IsDeliveryMarked,
            _ => false,
        };
        return inInterval ? Units(item.OpenQuantity) : OperationAvailability.Closed;
    }

    /// <summary>1.11.22 - removing an item or lowering its quantity after the payment: a physical item until Kargoya verildi, a digital or service item until its delivery mark.</summary>
    private static OperationAvailability RemoveOrReduce(OperationInput input)
    {
        (OrderOperationFacts order, OperationItemFacts? item) = (input.Order, input.Item);
        if (item is null || !order.PaymentApproved || order.OrderStatus == OrderStatus.Cancelled)
        {
            return OperationAvailability.Closed;
        }

        bool inInterval = item.ProductType == ProductType.Physical ? order.OrderStatus is OrderStatus.Placed or OrderStatus.Preparing : !item.IsDeliveryMarked;
        return inInterval ? Units(item.OpenQuantity) : OperationAvailability.Closed;
    }

    /// <summary>1.11.23 - the address: the shipping address until Kargoya verildi; on an order with no physical item the billing address until Teslim edildi.</summary>
    private static bool IsAddressCorrectable(OrderOperationFacts order)
    {
        if (order.PersonalDataDisposed)
        {
            return false;
        }

        return order.HasPhysicalItem
            ? order.OrderStatus is OrderStatus.Placed or OrderStatus.Preparing
            : order.OrderStatus is OrderStatus.Placed or OrderStatus.Preparing or OrderStatus.Shipped or OrderStatus.DeliveryFailed;
    }

    /// <summary>The money the order took and has not given back: a payment was confirmed and some of it is left. The refund is the firm's duty from here (02 7.4).</summary>
    private static bool IsRefundProcessableAmount(OrderOperationFacts order) =>
        order.PaymentStatus is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded && order.RefundableRemaining > 0;

    /// <summary>1.11.31 - processing a refund: money is left to give back, and on the transfer line the customer's IBAN is on file (without it 1.11.39 comes first).</summary>
    private static bool IsRefundProcessable(OrderOperationFacts order) =>
        IsRefundProcessableAmount(order) && (order.PaymentMethod == PaymentMethod.Card || order.HasRefundIban);

    /// <summary>1.11.34 - recording a withdrawal that came by another channel: a physical item once the order has shipped, a service item once paid; the period is the firm's to confirm, so it is not a condition here.</summary>
    private static OperationAvailability RecordWithdrawal(OperationInput input)
    {
        (OrderOperationFacts order, OperationItemFacts? item) = (input.Order, input.Item);
        if (item is null || item.OpenQuantity <= 0)
        {
            return OperationAvailability.Closed;
        }

        bool inInterval = item.ProductType switch
        {
            ProductType.Physical => order.HasShipped,
            ProductType.Service => order.PaymentApproved && order.OrderStatus != OrderStatus.Cancelled,
            _ => false,
        };
        return inInterval ? Units(item.OpenQuantity) : OperationAvailability.Closed;
    }

    /// <summary>1.11.37 - the administrator marks an open claim resolved, or reopens a resolved one while its channel is open (Z-18).</summary>
    private static OperationAvailability ResolveOrReopen(OperationInput input)
    {
        return input.Item?.Period switch
        {
            { DefectClaim: DefectClaimStatus.Open } => OperationAvailability.Open(),
            { DefectClaim: DefectClaimStatus.Resolved, DefectChannelOpen: true } => OperationAvailability.Open(),
            _ => OperationAvailability.Closed,
        };
    }
}
