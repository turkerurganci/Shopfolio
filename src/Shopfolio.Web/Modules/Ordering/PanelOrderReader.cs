using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>The Ordering module's reads for the panel's order screens (T527); internal, reached only through <see cref="IPanelOrderReader"/>.</summary>
internal sealed class PanelOrderReader : IPanelOrderReader
{
    private readonly OrderPeriods _periods;
    private readonly AppDbContext _db;
    private readonly OrderOperations _operations;
    private readonly IAfterSalesModule _afterSales;
    private readonly IPaymentModule _payment;
    private readonly INotificationsModule _notifications;
    private readonly ISettingsModule _settings;
    private readonly TimeProvider _time;

    public PanelOrderReader(
        OrderPeriods periods,
        AppDbContext db,
        OrderOperations operations,
        IAfterSalesModule afterSales,
        IPaymentModule payment,
        INotificationsModule notifications,
        ISettingsModule settings,
        TimeProvider time)
    {
        ArgumentNullException.ThrowIfNull(periods);
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(notifications);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(time);
        _periods = periods;
        _db = db;
        _operations = operations;
        _afterSales = afterSales;
        _payment = payment;
        _notifications = notifications;
        _settings = settings;
        _time = time;
    }

    public async Task<IReadOnlyList<OrderRunningPeriod>> FindRunningPeriodsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        Guid[] ids = [.. orderIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        List<OrderRunningPeriod> periods = [.. await _periods.RunningPeriodsAsync(ids, ct)];

        // Z-8: the last payment day of a transfer order that still waits - the day before its window ends, as the customer's notices write it (B-2, B-3).
        List<Order> awaiting = await _db.Orders.AsNoTracking()
            .Where(o => ids.Contains(o.Id))
            .Where(PaymentWindowRules.Awaiting(PaymentMethod.BankTransfer))
            .ToListAsync(ct);
        periods.AddRange(awaiting.Select(o => new OrderRunningPeriod(o.Id, CountedPeriod.BankTransferPaymentWindow, PaymentWindowRules.LastPaymentDay(o.PaymentWindowEndsAt))));

        // Z-16, Z-17: each refund not yet sent runs from its start, as on the order's page (04 9.9.6) - the AfterSales rule gives the last day.
        IReadOnlyList<PendingRefundStart> refunds = await _payment.ListPendingRefundStartsAsync(ids, ct);
        periods.AddRange(refunds.Select(r => new OrderRunningPeriod(r.OrderId, CountedPeriod.RefundPeriod, _afterSales.RefundLastDay(r.StartedAt))));
        return periods;
    }

    public async Task<IReadOnlyDictionary<Guid, PanelOrderMarkFacts>> FindListMarksAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        Guid[] ids = [.. orderIds.Distinct()];
        if (ids.Length == 0)
        {
            return new Dictionary<Guid, PanelOrderMarkFacts>();
        }

        // The page's marks by the same rules as the order's page (ReadDetailAsync): one query per mark kind, whatever the number of orders (09 20 item 4).
        DateTimeOffset now = _time.GetUtcNow();
        List<Order> orders = await _db.Orders.AsNoTracking().Where(o => ids.Contains(o.Id)).ToListAsync(ct);
        IReadOnlySet<Guid> undelivered = await _notifications.FindMarkedOrdersAmongAsync(ids, ct);
        IReadOnlyDictionary<Guid, RefundMarks> refundMarks = await _payment.ReadRefundMarksOfOrdersAsync(ids, ct);
        HashSet<Guid> corrected = [.. await _db.OrderEmailHistories.AsNoTracking().Where(h => ids.Contains(h.OrderId)).Select(h => h.OrderId).Distinct().ToListAsync(ct)];

        return orders.ToDictionary(
            o => o.Id,
            o => new PanelOrderMarkFacts(
                undelivered.Contains(o.Id),
                PaymentWindowRules.IsPaymentOutcomeUnknown(o, now),
                refundMarks.TryGetValue(o.Id, out RefundMarks? r) && r.IsNotRealized,
                r is not null && r.IsOutcomePending,
                corrected.Contains(o.Id)));
    }

    public async Task<PanelOrderDetailFacts?> ReadDetailAsync(Guid orderId, CancellationToken ct)
    {
        Order? order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct);
        // The operation decision's own facts: the marks and the items' open quantities and periods are the numbers the buttons are decided from (K-790).
        OrderOperationFacts? facts = order is null ? null : await _operations.ReadFactsAsync(orderId, ct);
        if (order is null || facts is null)
        {
            return null;
        }

        Guid[] itemIds = [.. facts.Items.Select(i => i.ItemId)];
        IReadOnlyList<RefundIbanField> ibanFields = await _afterSales.ListRefundIbanFieldsAsync(orderId, itemIds, ct);
        HashSet<Guid> ibanPending = [.. ibanFields.Where(f => f.RequestId is not null && f.OrderItemId is not null).Select(f => f.OrderItemId!.Value)]; // non-null: the Where on this line keeps only fields with an item
        List<PanelItemFacts> items =
        [
            .. facts.Items.Select(i => new PanelItemFacts(
                i.ItemId,
                i.OpenQuantity,
                ibanPending.Contains(i.ItemId),
                i.Period.ReceivableUnits,
                i.Period.WithdrawalLastDay,
                i.Period.DefectLastDay)),
        ];

        IReadOnlyList<PendingRefund> pending = await _payment.ListPendingRefundsAsync(orderId, ct);
        List<PanelPendingRefund> pendingRefunds =
        [
            .. pending.Select(r => new PanelPendingRefund(
                r.RefundId,
                r.OrderItemId,
                r.Quantity,
                r.Amount,
                r.Way,
                r.Way == PaymentMethod.BankTransfer ? IbanOf(ibanFields, r) : null,
                r.HasPeriod ? _afterSales.RefundLastDay(r.StartedAt) : null)),
        ];

        IReadOnlyDictionary<Guid, string> provinces = (await _settings.GetProvincesAsync(ct)).ToDictionary(p => p.Id, p => p.Name);
        List<OrderAddressHistory> addressRows = await _db.OrderAddressHistories.AsNoTracking()
            .Where(h => h.OrderId == orderId).OrderBy(h => h.CreatedAt).ThenBy(h => h.Id).ToListAsync(ct);
        List<PanelAddressChange> addressHistory =
        [
            .. addressRows.Select(h => new PanelAddressChange(
                h.CreatedAt,
                h.AddressType,
                new OrderPageAddress(h.OldRecipientName, h.OldPhone, ProvinceName(provinces, h.OldProvinceId), h.OldDistrict, h.OldAddressLine),
                new OrderPageAddress(h.NewRecipientName, h.NewPhone, ProvinceName(provinces, h.NewProvinceId), h.NewDistrict, h.NewAddressLine),
                h.AdminName)),
        ];

        List<PanelEmailChange> emailCorrections = await _db.OrderEmailHistories.AsNoTracking()
            .Where(h => h.OrderId == orderId).OrderBy(h => h.CreatedAt).ThenBy(h => h.Id)
            .Select(h => new PanelEmailChange(h.CreatedAt, h.OldEmail, h.NewEmail, h.AdminName))
            .ToListAsync(ct);

        PanelInternalNote? note = await _db.InternalNotes.AsNoTracking()
            .Where(n => n.OrderId == orderId)
            .Select(n => new PanelInternalNote(n.Text, n.AdminName))
            .SingleOrDefaultAsync(ct);

        var marks = new PanelOrderMarkFacts(
            facts.EmailNotDelivered,
            PaymentWindowRules.IsPaymentOutcomeUnknown(order, _time.GetUtcNow()),
            facts.CardRefundNotRealized,
            facts.CardRefundOutcomePending,
            emailCorrections.Count > 0);

        return new PanelOrderDetailFacts(
            marks,
            items,
            pendingRefunds,
            await _periods.RunningPeriodsAsync([orderId], ct),
            addressHistory,
            emailCorrections,
            await _notifications.ListOrderNoticesAsync(orderId, ct),
            note,
            facts.PersonalDataDisposed,
            order.PaymentMethod == PaymentMethod.BankTransfer ? PaymentWindowRules.LastPaymentDay(order.PaymentWindowEndsAt) : null);
    }

    // The IBAN a transfer refund goes to: the one on file for the refund itself, else the one on file for its item (04 9.9.6; OB-14, read only in the panel).
    private static string? IbanOf(IReadOnlyList<RefundIbanField> fields, PendingRefund refund) =>
        fields.FirstOrDefault(f => f.Iban is not null && f.RefundId == refund.RefundId)?.Iban
        ?? fields.FirstOrDefault(f => f.Iban is not null && refund.OrderItemId is not null && f.OrderItemId == refund.OrderItemId)?.Iban;

    private static string ProvinceName(IReadOnlyDictionary<Guid, string> provinces, Guid provinceId) =>
        provinces.TryGetValue(provinceId, out string? name) ? name : string.Empty;
}
