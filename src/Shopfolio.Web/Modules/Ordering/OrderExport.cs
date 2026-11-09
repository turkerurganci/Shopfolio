using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Export;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// One line of the order export file (02 10.7.1): an order item with the order's values repeated. The values are the invoice data's own frozen values (T357) and the few the file adds
/// that the invoice data does not carry: the frozen contact e-mail and recipient name (empty on an order whose personal data is disposed), the province names, the item's hand-over
/// moment as the file writes it and the late payment of an order that was cancelled.
/// </summary>
internal sealed record OrderExportRow(
    InvoiceData Invoice,
    InvoiceItem Item,
    DateTimeOffset? HandedOverAt,
    string? ContactEmail,
    string? ShippingRecipientName,
    string? BillingProvince,
    string? CompanyProvince,
    LatePaymentFact? LatePayment);

/// <summary>The Turkish terms of the enum values a column writes (09 10.10 item 5; 06 5): one dictionary, and a value without a term is caught by a test.</summary>
internal static class CsvEnumLabels
{
    private static readonly Dictionary<PaymentMethod, string> PaymentMethods = new()
    {
        [PaymentMethod.Card] = "Kart",
        [PaymentMethod.BankTransfer] = "Havale/EFT",
    };

    /// <summary>The term of a payment method (06 5.3.5).</summary>
    public static string Of(PaymentMethod method) => PaymentMethods.TryGetValue(method, out string? label)
        ? label
        : throw new ArgumentOutOfRangeException(nameof(method), method, "The payment method has no export term.");

    /// <summary>The term of a carrier company: the name of the listed or retired company (08 2.4); a typed "Diğer" name is not an enum and is written as it is.</summary>
    public static string OfCarrier(CarrierCatalog catalog, string code)
    {
        ArgumentNullException.ThrowIfNull(catalog);
        return catalog.Describe(code, null, null, isOwnVehicleDelivery: false).CarrierName ?? code;
    }
}

/// <summary>
/// The columns of the order export (09 10.10 item 5; 02 10.7.1), one definition for the header and the cell. Each column names its source field in its comment; the kind of a cell is its
/// definition's (item 1). An order whose personal data is disposed has no billing address, e-mail or recipient name, so those cells are empty (<see langword="null"/>) and the firm's
/// identity and the amounts stay (06 9.2.1).
/// </summary>
internal static class OrderExportColumns
{
    public static IReadOnlyList<CsvColumn<OrderExportRow>> All { get; } =
    [
        // Order: Order.OrderNumber, CreatedAt, PaymentMethod
        new("Sipariş numarası", CsvColumnKind.Text, r => r.Invoice.OrderNumber),
        new("Sipariş tarihi", CsvColumnKind.Date, r => r.Invoice.CreatedAt),
        new("Ödeme yöntemi", CsvColumnKind.Text, r => CsvEnumLabels.Of(r.Invoice.PaymentMethod)),

        // Item: OrderItem.ProductName, FirstOptionValue, SecondOptionValue, Quantity, UnitPrice, DiscountPercentage, DiscountedUnitPrice, CouponShare, VatRate, VatAmount
        new("Ürün adı", CsvColumnKind.Text, r => r.Item.ProductName),
        new("Birinci seçenek", CsvColumnKind.Text, r => r.Item.FirstOptionValue),
        new("İkinci seçenek", CsvColumnKind.Text, r => r.Item.SecondOptionValue),
        new("Adet", CsvColumnKind.Quantity, r => r.Item.Quantity),
        new("Birim fiyat", CsvColumnKind.Amount, r => r.Item.UnitPrice),
        new("İndirim yüzdesi", CsvColumnKind.Rate, r => r.Item.DiscountPercentage),
        new("İndirimli birim fiyat", CsvColumnKind.Amount, r => r.Item.DiscountedUnitPrice),
        new("Kupon payı", CsvColumnKind.Amount, r => r.Item.CouponShare),
        new("KDV oranı", CsvColumnKind.Rate, r => r.Item.VatRate),
        new("KDV tutarı", CsvColumnKind.Amount, r => r.Item.VatAmount),

        // Order total: Order.Total, ShippingFee, ShippingFeeVatBreakdown
        new("Sipariş toplamı", CsvColumnKind.Amount, r => r.Invoice.Total),
        new("Kargo ücreti", CsvColumnKind.Amount, r => r.Invoice.ShippingFee),
        new("Kargo ücretinin KDV dökümü", CsvColumnKind.Text, r => ShippingVatBreakdown(r.Invoice)),

        // Billing address: Order.BillingRecipientName, BillingProvinceId (name), BillingDistrict, BillingAddressLine
        new("Fatura alıcı adı", CsvColumnKind.Text, r => r.Invoice.BillingAddress?.RecipientName),
        new("Fatura ili", CsvColumnKind.Text, r => r.BillingProvince),
        new("Fatura ilçesi", CsvColumnKind.Text, r => r.Invoice.BillingAddress?.District),
        new("Fatura adresi", CsvColumnKind.Text, r => r.Invoice.BillingAddress?.AddressLine),

        // Firm identity: Order.CompanyLegalName, CompanyTaxIdNumber, CompanyMersisNumber, CompanyTradeRegistryNumber, CompanyProvinceId (name), CompanyDistrict, CompanyAddressLine
        new("Ticaret unvanı", CsvColumnKind.Text, r => r.Invoice.Company.CompanyLegalName),
        new("Vergi numarası", CsvColumnKind.Text, r => r.Invoice.Company.CompanyTaxIdNumber),
        new("Mersis numarası", CsvColumnKind.Text, r => r.Invoice.Company.CompanyMersisNumber),
        new("Ticaret sicil numarası", CsvColumnKind.Text, r => r.Invoice.Company.CompanyTradeRegistryNumber),
        new("Firma ili", CsvColumnKind.Text, r => r.CompanyProvince),
        new("Firma ilçesi", CsvColumnKind.Text, r => r.Invoice.Company.CompanyDistrict),
        new("Firma adresi", CsvColumnKind.Text, r => r.Invoice.Company.CompanyAddressLine),

        // Reaching the recipient: Order.ContactEmail, ShippingRecipientName
        new("İletişim e-postası", CsvColumnKind.Text, r => r.ContactEmail),
        new("Teslimat alıcı adı", CsvColumnKind.Text, r => r.ShippingRecipientName),

        // Site: the installation's domain (Site:Domain), the same on every line
        new("Sitenin alan adı", CsvColumnKind.Text, r => r.Invoice.Domain),

        // Shipping: Order.Carrier or OtherCarrierName; the item's hand-over day
        new("Kargo şirketi", CsvColumnKind.Text, r => r.Invoice.CarrierName),
        new("Kargoya verme tarihi", CsvColumnKind.Date, r => r.HandedOverAt),

        // Payment: the last confirmation transition's moment
        new("Ödeme onay tarihi", CsvColumnKind.Date, r => r.Invoice.PaymentConfirmedAt),

        // Late payment: PaymentRecord.CreatedAt, Order.Total, Refund.SentAt, Refund.Amount - only on an order that a late card payment came to
        new("Geç ödeme tarihi", CsvColumnKind.Date, r => r.LatePayment?.PaidAt),
        new("Geç ödeme tutarı", CsvColumnKind.Amount, r => r.LatePayment is null ? null : r.Invoice.Total),
        new("Geri ödeme tarihi", CsvColumnKind.Date, r => r.LatePayment?.RefundSentAt),
        new("Geri ödeme tutarı", CsvColumnKind.Amount, r => r.LatePayment?.RefundAmount),
    ];

    /// <summary>The shipping fee's VAT as <c>rate=amount</c> pairs joined with <c>|</c> (<c>20=12,50|10=3,20</c>); empty when the fee has none (09 10.10 item 5).</summary>
    internal static string? ShippingVatBreakdown(InvoiceData invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        return invoice.ShippingFeeVatBreakdown.Count == 0
            ? null
            : string.Join('|', invoice.ShippingFeeVatBreakdown.Select(p => CsvExportWriter.RateCell(p.VatRate) + "=" + CsvExportWriter.AmountCell(p.Vat)));
    }
}

/// <summary>
/// The read behind the order export (T384; 02 10.7.1; 09 10.10 item 4): the orders created in a Turkey day range whose payment was completed - paid, partly or wholly paid back, and the
/// order a late card payment came to that the system paid back (the same set the sales summary counts, 02 10.6.2) - page by page in the order of their id, each order's items written one
/// line each. The set is never held whole: a page is 500 orders, its items, approvals, shipments, late payments and closings are read for the page in a handful of queries.
/// </summary>
internal sealed class OrderExport
{
    private readonly AppDbContext _db;
    private readonly InvoiceDataReader _invoices;
    private readonly IPaymentModule _payment;
    private readonly IAfterSalesModule _afterSales;
    private readonly ISettingsModule _settings;

    public OrderExport(AppDbContext db, IOptions<SiteOptions> site, IPaymentModule payment, IAfterSalesModule afterSales, ISettingsModule settings)
    {
        _db = db;
        _invoices = new InvoiceDataReader(db, site);
        _payment = payment;
        _afterSales = afterSales;
        _settings = settings;
    }

    /// <summary>Whether the order's payment was completed: the payment was confirmed at some moment, so the paid statuses and the refunded ones after them (02 10.6.2).</summary>
    public static bool IsPaymentCompleted(PaymentStatus status) => status is PaymentStatus.Paid or PaymentStatus.PartiallyRefunded or PaymentStatus.Refunded;

    /// <summary>Writes the file's lines to <paramref name="destination"/> and returns the number of lines.</summary>
    public async Task<int> WriteAsync(DateOnly startDay, DateOnly endDay, Stream destination, long maxBytes, CancellationToken ct)
    {
        return await CsvExportWriter.WriteAsync(destination, OrderExportColumns.All, ReadAsync(startDay, endDay, ct), new ExportCeilings(maxBytes), ct);
    }

    /// <summary>The lines of the range as an asynchronous sequence (09 10.10 item 4).</summary>
    public async IAsyncEnumerable<OrderExportRow> ReadAsync(DateOnly startDay, DateOnly endDay, [EnumeratorCancellation] CancellationToken ct)
    {
        if (endDay < startDay)
        {
            throw new ArgumentException("The range ends before it starts.", nameof(endDay));
        }

        Dictionary<Guid, string> provinces = (await _settings.GetProvincesAsync(ct)).ToDictionary(p => p.Id, p => p.Name);
        DateTimeOffset from = TurkeyTime.StartOf(startDay);
        DateTimeOffset to = TurkeyTime.EndOfRange(endDay);
        Guid? after = null;
        while (true)
        {
            IQueryable<Order> query = _db.Orders.AsNoTracking().Where(o => o.CreatedAt >= from && o.CreatedAt < to);
            if (after is { } last)
            {
                query = query.Where(o => o.Id.CompareTo(last) > 0);
            }

            List<Order> page = await query.OrderBy(o => o.Id).Take(ExportLimits.PageSize).ToListAsync(ct);
            if (page.Count == 0)
            {
                yield break;
            }

            after = page[^1].Id;
            Dictionary<Guid, LatePaymentFact> late = (await _payment.ListLatePaymentsAsync([.. page.Select(o => o.Id)], ct)).ToDictionary(l => l.OrderId);
            List<Order> included = [.. page.Where(o => IsPaymentCompleted(o.PaymentStatus) || late.ContainsKey(o.Id))];
            if (included.Count > 0)
            {
                IReadOnlyList<InvoiceData> invoices = await _invoices.ComposeAsync(included, ct);
                HashSet<Guid> closedMoments = await FullyClosedBeforeShipmentAsync(invoices, ct);
                Dictionary<Guid, Order> orders = included.ToDictionary(o => o.Id);
                foreach (InvoiceData invoice in invoices)
                {
                    Order order = orders[invoice.OrderId];
                    late.TryGetValue(invoice.OrderId, out LatePaymentFact? latePayment);
                    foreach (InvoiceItem item in invoice.Items)
                    {
                        yield return new OrderExportRow(
                            invoice,
                            item,
                            closedMoments.Contains(item.OrderItemId) ? null : item.ShippedAt,
                            order.ContactEmail,
                            order.ShippingRecipientName,
                            Name(provinces, order.BillingProvinceId),
                            Name(provinces, invoice.Company.CompanyProvinceId),
                            latePayment);
                    }
                }
            }

            if (page.Count < ExportLimits.PageSize)
            {
                yield break;
            }
        }
    }

    /// <summary>
    /// The items that were closed whole by a cancellation or a late delivery termination before the order's shipment (D-736; 02 3.25.1): the line stays in the file (the sale is a
    /// commercial record) but nothing was handed over for it, so its hand-over date is empty. An item closed only in part, or after the shipment, keeps the date.
    /// </summary>
    private async Task<HashSet<Guid>> FullyClosedBeforeShipmentAsync(IReadOnlyList<InvoiceData> invoices, CancellationToken ct)
    {
        Guid[] physical = [.. invoices.SelectMany(i => i.Items).Where(i => i.ProductType == Catalog.Contracts.ProductType.Physical && i.ShippedAt is not null).Select(i => i.OrderItemId)];
        if (physical.Length == 0)
        {
            return [];
        }

        IReadOnlyDictionary<Guid, int> closed = await _afterSales.ClosedQuantitiesAsync(physical, ct);
        Guid[] candidates = [.. invoices.SelectMany(i => i.Items).Where(i => closed.TryGetValue(i.OrderItemId, out int quantity) && quantity >= i.Quantity).Select(i => i.OrderItemId)];
        if (candidates.Length == 0)
        {
            return [];
        }

        Dictionary<Guid, DateTimeOffset> shipped = invoices.SelectMany(i => i.Items).Where(i => i.ShippedAt is not null).ToDictionary(i => i.OrderItemId, i => i.ShippedAt!.Value);
        IReadOnlyList<ClosingRecordMoment> moments = await _afterSales.ClosingRecordMomentsAsync(candidates, ct);
        return [.. moments.Where(m => m.At <= shipped[m.OrderItemId]).Select(m => m.OrderItemId)];
    }

    private static string? Name(Dictionary<Guid, string> provinces, Guid? id) => id is { } key && provinces.TryGetValue(key, out string? name) ? name : null;
}
