using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The read behind the order export's invoice columns (T357; 02 3.25.1-3.25.3, 10.7.1): the product issues no invoice, integrates with no e-Arşiv or e-Fatura service and stores no
/// invoice - it hands out the order's own frozen values. The firm's identity is the order's frozen copy, never the company row of today (K-220). The moments of the payment's approval
/// and of the shipment are read from the transition records through <see cref="OrderPeriods"/>' definitions, the carrier's name through <see cref="CarrierCatalog"/>, the domain from
/// the installation setting. Nothing is written and no lock is taken.
/// </summary>
internal sealed class InvoiceDataReader
{
    private readonly AppDbContext _db;
    private readonly string _domain;

    public InvoiceDataReader(AppDbContext db, IOptions<SiteOptions> site)
    {
        _db = db;
        _domain = site.Value.Domain;
    }

    /// <summary>The invoice data of <paramref name="orderIds"/> in the orders' creation order; an id with no order is skipped.</summary>
    public async Task<IReadOnlyList<InvoiceData>> ListAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        Guid[] ids = [.. orderIds.Distinct()];
        if (ids.Length == 0)
        {
            return [];
        }

        List<Order> orders = await _db.Orders.AsNoTracking()
            .Where(o => ids.Contains(o.Id))
            .OrderBy(o => o.CreatedAt).ThenBy(o => o.Id)
            .ToListAsync(ct);
        List<OrderItem> items = await _db.OrderItems.AsNoTracking()
            .Where(i => ids.Contains(i.OrderId))
            .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
            .ToListAsync(ct);
        Dictionary<Guid, DateTimeOffset> approvals = await LatestAsync(OrderPeriods.PaymentApprovals(_db), ids, ct);
        Dictionary<Guid, DateTimeOffset> shipments = await LatestAsync(OrderPeriods.Shipments(_db), ids, ct);

        ILookup<Guid, OrderItem> itemsOf = items.ToLookup(i => i.OrderId);
        return [.. orders.Select(o => Compose(
            o,
            [.. itemsOf[o.Id]],
            approvals.TryGetValue(o.Id, out DateTimeOffset approved) ? approved : null,
            shipments.TryGetValue(o.Id, out DateTimeOffset shipped) ? shipped : null,
            _domain))];
    }

    /// <summary>
    /// One order's invoice data from its own values (02 3.25.1): the items' frozen amounts, the frozen fee and its breakdown, the billing address, the frozen identity, the payment and
    /// the carrier. An item's hand-over moment follows its type: the order's shipment (<paramref name="shippedAt"/>) for a physical item, its "completed" mark for a service item,
    /// the payment's approval (<paramref name="paymentConfirmedAt"/>) for a digital item. The carrier's name is given once the order is shipped. Pure: no database, no clock.
    /// </summary>
    public static InvoiceData Compose(Order order, IReadOnlyList<OrderItem> items, DateTimeOffset? paymentConfirmedAt, DateTimeOffset? shippedAt, string domain)
    {
        ArgumentNullException.ThrowIfNull(order);
        ArgumentNullException.ThrowIfNull(items);

        PartyAddress? billing = order.BillingRecipientName is { } name && order.BillingProvinceId is { } province
            ? new PartyAddress(name, null, province, order.BillingDistrict ?? string.Empty, order.BillingAddressLine ?? string.Empty)
            : null;
        FrozenCompanyIdentity company = new(
            order.CompanyType,
            order.CompanyLegalName,
            order.CompanyTaxIdNumber,
            order.CompanyMersisNumber,
            order.CompanyTradeRegistryNumber,
            order.CompanyProvinceId,
            order.CompanyDistrict,
            order.CompanyAddressLine,
            order.CompanyPhone,
            order.CompanyEmail,
            order.CompanyKepAddress);
        string? carrierName = shippedAt is null
            ? null
            : CarrierCatalog.Default.Describe(order.Carrier, order.OtherCarrierName, order.TrackingNumber, order.IsOwnVehicleDelivery).CarrierName;

        return new InvoiceData(
            order.Id,
            order.OrderNumber,
            order.CreatedAt,
            order.Total,
            order.CouponCode,
            order.ShippingFee,
            [.. (order.ShippingFeeVatBreakdown ?? []).Select(s => new ShippingVatPart(s.VatRate, s.Part, s.Vat))],
            billing,
            company,
            order.PaymentMethod,
            paymentConfirmedAt,
            carrierName,
            order.IsOwnVehicleDelivery,
            domain,
            [.. items.Select(i => new InvoiceItem(
                i.Id,
                i.ProductType,
                i.ProductName,
                i.FirstOptionValue,
                i.SecondOptionValue,
                i.Quantity,
                i.UnitPrice,
                i.DiscountPercentage,
                i.DiscountedUnitPrice,
                i.CouponShare,
                i.VatRate,
                i.VatAmount,
                HandedOverAt(i, paymentConfirmedAt, shippedAt)))]);
    }

    private static DateTimeOffset? HandedOverAt(OrderItem item, DateTimeOffset? paymentConfirmedAt, DateTimeOffset? shippedAt) => item.ProductType switch
    {
        ProductType.Physical => shippedAt,
        ProductType.Service => item.DeliveryMarkedAt,
        ProductType.Digital => paymentConfirmedAt,
        _ => throw new ArgumentOutOfRangeException(nameof(item), item.ProductType, "Unknown product type."),
    };

    private static async Task<Dictionary<Guid, DateTimeOffset>> LatestAsync(IQueryable<TransitionRecord> records, Guid[] ids, CancellationToken ct)
    {
        return await records
            .Where(t => ids.Contains(t.OrderId))
            .GroupBy(t => t.OrderId)
            .Select(g => new { OrderId = g.Key, At = g.Max(t => t.CreatedAt) })
            .ToDictionaryAsync(x => x.OrderId, x => x.At, ct);
    }
}
