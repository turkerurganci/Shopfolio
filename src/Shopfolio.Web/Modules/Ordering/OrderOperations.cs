using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The operation decision's reader (T323; 03 1.11): it builds the plain facts of an order from its own records and from the AfterSales and Payment modules' open reads - the
/// periods and unit standings come from the module that owns the rule - and hands them to <see cref="OrderOperationCatalog"/>. <see cref="ReadAsync"/> is what the order page and
/// the panel draw their buttons from, <see cref="CheckAsync"/> is what each POST asks again; both read the facts the same way, so the screen and the endpoint cannot differ (07 8.1.2).
/// It only reads: a writer calls <see cref="CheckAsync"/> inside its own transaction after the order's row lock and writes only for an allowed verdict (09 8.3 item 3).
/// </summary>
internal sealed class OrderOperations : IOrderOperations
{
    private readonly AppDbContext _db;
    private readonly IAfterSalesModule _afterSales;
    private readonly IPaymentModule _payment;
    private readonly INotificationsModule _notifications;
    private readonly TimeProvider _time;

    public OrderOperations(AppDbContext db, IAfterSalesModule afterSales, IPaymentModule payment, INotificationsModule notifications, TimeProvider time)
    {
        _db = db;
        _afterSales = afterSales;
        _payment = payment;
        _notifications = notifications;
        _time = time;
    }

    public async Task<OrderOperationsView?> ReadAsync(Guid orderId, ActorKind actor, CancellationToken ct)
    {
        if (actor is not (ActorKind.Buyer or ActorKind.Admin))
        {
            throw new ArgumentOutOfRangeException(nameof(actor), actor, "Only the customer and the administrator have operations (03 1.11).");
        }

        OrderOperationFacts? facts = await LoadAsync(orderId, ct);
        return facts is null ? null : new OrderOperationsView(orderId, actor, facts.Version, OrderOperationCatalog.Decide(facts, actor));
    }

    public async Task<OperationVerdict> CheckAsync(Guid orderId, OrderOperation operation, Guid? orderItemId, int? quantity, CancellationToken ct)
    {
        OrderOperationFacts? facts = await LoadAsync(orderId, ct);
        return facts is null
            ? new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed)
            : OrderOperationCatalog.Check(facts, operation, orderItemId, quantity);
    }

    public async Task<WithdrawalScreen?> ReadWithdrawalScreenAsync(Guid orderId, Guid orderItemId, CancellationToken ct)
    {
        OrderOperationFacts? facts = await LoadAsync(orderId, ct);
        if (facts?.Items.FirstOrDefault(i => i.ItemId == orderItemId) is not { } item || WithdrawalOperation(item.ProductType) is not { } operation)
        {
            return null;
        }

        // The screen opens only for the button the decision draws (03 1.11.5, 1.11.6): a physical item not shipped yet has none - its way is the cancellation (02 7.3.7).
        // The same decision the order page's buttons are drawn from (ReadAsync), so the screen and the button cannot differ.
        if (!OrderOperationCatalog.Decide(facts, ActorKind.Buyer).Any(e => e.Operation == operation && e.OrderItemId == orderItemId))
        {
            return null;
        }

        bool inTransit = item.ProductType == ProductType.Physical && item.DeliveryDate is null;
        WithdrawalExemptionReason? exemption = await _db.OrderItems.AsNoTracking().Where(i => i.Id == orderItemId).Select(i => i.WithdrawalExemptionReason).SingleAsync(ct);
        WithdrawalNotes notes = await _afterSales.WithdrawalNotesAsync(item.ProductType, inTransit, exemption, ct);
        return new WithdrawalScreen(
            orderItemId,
            item.ProductType,
            item.OpenQuantity,
            item.Period.WithdrawalLastDay,
            notes.ReturnAddress,
            notes.SendingPeriodDays,
            notes.CarrierReturnsUnreceivedGoods,
            notes.PackagingCondition,
            facts.PaymentMethod == PaymentMethod.BankTransfer && !facts.HasRefundIban);
    }

    /// <summary>The customer's withdrawal operation of an item's type (03 1.11.5, 1.11.6): a digital item has none (1.11.7 is not a button; 02 7.3.2).</summary>
    internal static OrderOperation? WithdrawalOperation(ProductType productType) => productType switch
    {
        ProductType.Physical => OrderOperation.WithdrawPhysicalItem,
        ProductType.Service => OrderOperation.WithdrawServiceItem,
        _ => null,
    };

    /// <summary>
    /// The verdict of one request with the facts it was read from (09 8.3 item 3): the writer - inside its transaction, after the order's row lock - needs the item's
    /// periods too (the warnings of a record from another channel). Null facts: no such order.
    /// </summary>
    internal async Task<(OperationVerdict Verdict, OrderOperationFacts? Facts)> CheckWithFactsAsync(
        Guid orderId,
        OrderOperation operation,
        Guid orderItemId,
        int quantity,
        CancellationToken ct)
    {
        OrderOperationFacts? facts = await LoadAsync(orderId, ct);
        return facts is null
            ? (new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed), null)
            : (OrderOperationCatalog.Check(facts, operation, orderItemId, quantity), facts);
    }

    private async Task<OrderOperationFacts?> LoadAsync(Guid orderId, CancellationToken ct)
    {
        Order? order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return null;
        }

        DateTimeOffset now = _time.GetUtcNow();
        List<OrderItem> items = await _db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).OrderBy(i => i.CreatedAt).ThenBy(i => i.Id).ToListAsync(ct);
        Guid[] itemIds = [.. items.Select(i => i.Id)];

        // The order's delivery date is the physical items' (03 4.1.15: the service window of an order that carried a physical item starts from it).
        DateOnly? orderDeliveryDate = items.Where(i => i.ProductType == ProductType.Physical && i.DeliveryDate is not null).Select(i => i.DeliveryDate).Max();
        bool hasPhysical = items.Any(i => i.ProductType == ProductType.Physical);
        IReadOnlyDictionary<Guid, ItemPeriodState> periods = await _afterSales.ItemPeriodStatesAsync(
            [.. items.Select(i => new ItemPeriodQuery(
                i.Id, i.ProductType, order.OrderStatus, i.DeliveryDate, i.DeliveryMarkedAt, order.CreatedAt, hasPhysical, orderDeliveryDate, i.WithdrawalExemptionReason))],
            now,
            ct);
        RefundIbanState iban = await _afterSales.RefundIbanStateAsync(orderId, itemIds, ct);
        RefundMarks marks = await _payment.ReadRefundMarksAsync(orderId, ct);
        decimal refunded = await _payment.SentRefundTotalAsync(orderId, ct);

        List<OperationItemFacts> itemFacts =
        [
            .. items.Select(i =>
            {
                ItemPeriodState period = periods[i.Id];
                return new OperationItemFacts(
                    i.Id,
                    i.ProductType,
                    i.Quantity,
                    Math.Max(0, i.Quantity - period.ClosedUnits - period.RescindedUnits),
                    i.DeliveryDate,
                    i.DeliveryMarkedAt is not null,
                    i.DownloadAllowance is { } allowance ? Math.Max(0, allowance - i.DownloadCount) : 0,
                    period);
            }),
        ];

        return new OrderOperationFacts(
            order.Id,
            order.Version,
            order.OrderStatus,
            order.PaymentStatus,
            order.PaymentMethod,
            order.CreatedAt,
            now,
            order.IsGuestOrder,
            order.ContactEmail is null,
            order.BankTransferIban is not null,
            EmailNotDelivered: await _notifications.OrderHasUndeliveredMarkAsync(order.Id, ct),
            iban.HasIban,
            iban.HasOpenRequest,
            marks.IsNotRealized,
            marks.IsOutcomePending,
            Math.Max(0m, order.Total - refunded),
            await HasCorrectableTransitionAsync(order, items, ct),
            itemFacts);
    }

    /// <summary>
    /// Whether the correction (03 1.6.2) has a transition to take back (03 1.11.26): the latest uncorrected shipment-axis record that ends in the order's present status and is
    /// one the correction accepts - the shipment, the delivery mark, the "teslim edilemedi" mark, the re-sending, the firm's own cancellation - or the transfer's "paid" mark on a
    /// transfer order. The correction itself reads the obstacles of its record (the refund, the stock, the delivery marks) when it runs and refuses what it cannot take back.
    /// </summary>
    private async Task<bool> HasCorrectableTransitionAsync(Order order, List<OrderItem> items, CancellationToken ct)
    {
        List<TransitionRecord> records = await _db.TransitionRecords.AsNoTracking().Where(t => t.OrderId == order.Id).ToListAsync(ct);
        HashSet<Guid> corrected = [.. records.Where(r => r.CorrectedTransitionRecordId is not null).Select(r => r.CorrectedTransitionRecordId!.Value)];
        TransitionCode[] shipmentCorrectable = [TransitionCode.S3, TransitionCode.S4, TransitionCode.S5, TransitionCode.S6, TransitionCode.S7, TransitionCode.S8, TransitionCode.S9];

        TransitionRecord? latest = records
            .Where(r => r.TransitionCode != TransitionCode.Correction && TransitionTable.FindShipment(r.TransitionCode) is not null && !corrected.Contains(r.Id))
            .OrderByDescending(r => r.CreatedAt).ThenByDescending(r => r.Id)
            .FirstOrDefault();
        if (latest is not null
            && shipmentCorrectable.Contains(latest.TransitionCode)
            && TransitionTable.FindShipment(latest.TransitionCode)!.To == order.OrderStatus
            && (latest.TransitionCode is not (TransitionCode.S3 or TransitionCode.S4 or TransitionCode.S9) || latest.ActorKind == ActorKind.Admin))
        {
            return true;
        }

        // 1.6.2.4: the transfer's mark goes back only on the transfer line and on an order with no digital item (the download opened).
        return order.PaymentMethod == PaymentMethod.BankTransfer
            && order.PaymentStatus == PaymentStatus.Paid
            && !items.Any(i => i.ProductType == ProductType.Digital)
            && records.Any(r => r.TransitionCode == TransitionCode.O1 && r.ActorKind == ActorKind.Admin && !corrected.Contains(r.Id));
    }
}
