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

    public Task<IReadOnlyList<OrderRunningPeriod>> FindRunningPeriodsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct) => _periods.RunningPeriodsAsync(orderIds, ct);

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
                _afterSales.RefundLastDay(r.StartedAt))),
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
            facts.PersonalDataDisposed);
    }

    // The IBAN a transfer refund goes to: the one on file for the refund itself, else the one on file for its item (04 9.9.6; OB-14, read only in the panel).
    private static string? IbanOf(IReadOnlyList<RefundIbanField> fields, PendingRefund refund) =>
        fields.FirstOrDefault(f => f.Iban is not null && f.RefundId == refund.RefundId)?.Iban
        ?? fields.FirstOrDefault(f => f.Iban is not null && refund.OrderItemId is not null && f.OrderItemId == refund.OrderItemId)?.Iban;

    private static string ProvinceName(IReadOnlyDictionary<Guid, string> provinces, Guid provinceId) =>
        provinces.TryGetValue(provinceId, out string? name) ? name : string.Empty;
}
