using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Ordering.Documents;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The lists of the customer's order page (07 UC-33; 04 5.16.3; T511): every value is read from the order's own frozen columns (02 3.23.1, 3.23.2), never from the product or the current
/// settings. The open quantity of an item is the operation decision's own (<see cref="OrderOperations.ReadOpenQuantitiesAsync"/>), so the row and the buttons cannot disagree (09 25, one
/// rule one place). The refunds are the Payment module's carried-out refunds (<see cref="IPaymentModule.ListSentRefundsAsync"/>).
/// </summary>
internal sealed class OrderPageReader : IOrderPage
{
    private readonly AppDbContext _db;
    private readonly OrderOperations _operations;
    private readonly IPaymentModule _payment;
    private readonly ISettingsModule _settings;
    private readonly OrderLocks _locks;
    private readonly IDigitalDelivery _delivery;

    public OrderPageReader(AppDbContext db, OrderOperations operations, IPaymentModule payment, ISettingsModule settings, OrderLocks locks, IDigitalDelivery delivery)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(locks);
        ArgumentNullException.ThrowIfNull(delivery);
        _db = db;
        _operations = operations;
        _payment = payment;
        _settings = settings;
        _locks = locks;
        _delivery = delivery;
    }

    public async Task<DownloadOperationResult> TakeDownloadAsync(Guid orderId, Guid orderItemId, CancellationToken ct)
    {
        OperationRow row = OrderOperationCatalog.Find(OrderOperation.DownloadDigitalFile)
            ?? throw new InvalidOperationException("The operation table has no download row (03 1.11.14).");
        OperationInput? input = null;
        DownloadLinkIssue? issue = null;
        GatedOperationResult gate = await OperationGate.ExecuteAsync<OperationInput>(
            async stepCt =>
            {
                // The customer's operation carries no version (09 8.3 item 3): the lock is taken and nothing is compared.
                await _locks.LockAsync([orderId], stepCt);
                return true;
            },
            async stepCt =>
            {
                OrderOperationFacts facts = await _operations.ReadFactsAsync(orderId, stepCt)
                    ?? throw new InvalidOperationException("The order of an opened order page does not exist.");
                OperationInput loaded = new(facts, facts.Items.FirstOrDefault(i => i.ItemId == orderItemId));
                input = loaded;
                return loaded;
            },
            row.Rule,
            null,
            async (_, _, stepCt) => issue = await _delivery.IssueLinkAsync(orderItemId, stepCt),
            ct);

        if (gate.IsRejected)
        {
            return new DownloadOperationResult(gate, null, ClosedReason(input));
        }

        // The rule said open; the file's absence and a right used up by a request of the same moment are found by the write itself, which then wrote nothing (05 7.7.4).
        if (issue is { Value: string value })
        {
            return new DownloadOperationResult(gate, value, null);
        }

        DownloadRefusal refusal = ReferenceEquals(issue, DownloadLinkIssue.NoFile) ? DownloadRefusal.NoFile
            : ReferenceEquals(issue, DownloadLinkIssue.AllowanceUsed) ? DownloadRefusal.AllowanceUsed
            : ClosedReason(input);
        return new DownloadOperationResult(GatedOperationResult.Decided(new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed)), null, refusal);
    }

    // The reason of a closed download, from the same facts the rule read: an unconfirmed payment first (the button is not there at all), then the used-up right (04 5.16.13).
    private static DownloadRefusal ClosedReason(OperationInput? input) =>
        input is not { } read || !DownloadRules.IsPaymentConfirmed(read.Order.PaymentStatus)
            ? DownloadRefusal.PaymentNotConfirmed
            : DownloadRefusal.AllowanceUsed;

    public async Task<OrderPageDetail?> ReadAsync(Guid orderId, CancellationToken ct)
    {
        Order? order = await _db.Orders.AsNoTracking().SingleOrDefaultAsync(o => o.Id == orderId, ct);
        if (order is null)
        {
            return null;
        }

        List<OrderItem> items = await _db.OrderItems.AsNoTracking()
            .Where(i => i.OrderId == orderId)
            .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
            .ToListAsync(ct);
        IReadOnlyDictionary<Guid, int> open = await _operations.ReadOpenQuantitiesAsync(orderId, ct) ?? new Dictionary<Guid, int>();

        List<OrderPageItem> rows = [];
        foreach (OrderItem item in items)
        {
            ConfirmationSummaryLine line = ConfirmationSummaryText.LineOf(item);
            string? exemption = item.ProductType == Catalog.Contracts.ProductType.Physical && item.WithdrawalExemptionReason is { } reason
                ? ConfirmationSummaryText.ExemptionNote(reason)
                : null;
            rows.Add(new OrderPageItem(
                item.Id,
                ConfirmationSummaryText.Name(line),
                item.ImageId,
                item.ProductType,
                item.Quantity,
                open.TryGetValue(item.Id, out int units) ? units : 0,
                line.UnitPrice,
                line.Payable,
                exemption,
                item.DeliveryMarkedAt,
                item.DeliveryDate));
        }

        decimal itemsTotal = items.Sum(i => (i.DiscountedUnitPrice ?? i.UnitPrice) * i.Quantity);
        decimal couponShare = items.Sum(i => i.CouponShare);
        decimal vat = items.Sum(i => i.VatAmount) + (order.ShippingFeeVatBreakdown?.Sum(s => s.Vat) ?? 0m);
        OrderPageBreakdown breakdown = new(
            itemsTotal,
            couponShare,
            couponShare > 0 ? order.CouponCode : null,
            vat,
            order.ShippingFee,
            order.ShippingFee == 0 && order.ShippingPeriod is not null,
            order.Total);

        Dictionary<Guid, string> names = rows.ToDictionary(r => r.OrderItemId, r => r.Name);
        IReadOnlyList<SentRefund> sent = await _payment.ListSentRefundsAsync(orderId, ct);
        List<OrderPageRefund> refunds =
        [
            .. sent.Select(r => new OrderPageRefund(
                r.SentAt,
                r.Amount,
                r.Way,
                r.OrderItemId is Guid itemId && names.TryGetValue(itemId, out string? name) ? name : null,
                r.Quantity)),
        ];

        IReadOnlyDictionary<Guid, string> provinces = (await _settings.GetProvincesAsync(ct)).ToDictionary(p => p.Id, p => p.Name);
        OrderPageAddresses addresses = new(
            order.ContactEmail,
            AddressOf(order.ShippingRecipientName, order.ShippingPhone, order.ShippingProvinceId, order.ShippingDistrict, order.ShippingAddressLine, provinces),
            AddressOf(order.BillingRecipientName, null, order.BillingProvinceId, order.BillingDistrict, order.BillingAddressLine, provinces));

        var boxes = await _db.CheckboxAcceptances.AsNoTracking()
            .Where(c => c.OrderId == orderId)
            .OrderBy(c => c.CreatedAt).ThenBy(c => c.Id)
            .Select(c => new
            {
                c.CheckboxType,
                c.ShownText,
                ItemIds = _db.CheckboxAcceptanceOrderItems.Where(x => x.CheckboxAcceptanceId == c.Id).Select(x => x.OrderItemId).ToList(),
            })
            .ToListAsync(ct);
        List<OrderPageCheckbox> checkboxes =
        [
            .. boxes.Select(b => new OrderPageCheckbox(
                b.CheckboxType,
                b.ShownText,
                [.. rows.Where(r => b.ItemIds.Contains(r.OrderItemId)).Select(r => r.Name)])),
        ];

        bool isCard = order.PaymentMethod == PaymentMethod.Card;
        return new OrderPageDetail(
            order.Id,
            order.CreatedAt,
            order.PaymentMethod,
            isCard && order.OrderStatus == OrderStatus.Placed && order.PaymentStatus == PaymentStatus.Pending,
            isCard && order.OrderStatus == OrderStatus.Cancelled && order.PaymentStatus == PaymentStatus.Failed,
            rows,
            breakdown,
            refunds,
            addresses,
            checkboxes,
            order.PrivacyNoticeVersionId);
    }

    private static OrderPageAddress? AddressOf(
        string? recipient, string? phone, Guid? provinceId, string? district, string? line, IReadOnlyDictionary<Guid, string> provinces)
    {
        // A disposed order's address fields are empty (06 4.17): the area shows no address then.
        if (recipient is null || provinceId is not Guid province || district is null || line is null)
        {
            return null;
        }

        return new OrderPageAddress(recipient, phone, provinces.TryGetValue(province, out string? name) ? name : string.Empty, district, line);
    }
}
