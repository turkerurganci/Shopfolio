using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The order export file as a pure rule (T384 KK2-KK6; 02 10.7.1; 09 10.10 items 5, 9): every column has a header and a kind, a line is an item with the order's values repeated, the shipping fee's
/// VAT is written as <c>rate=amount</c> pairs, an order whose personal data is disposed has empty address, e-mail and recipient columns with its amounts kept, and the enum terms cover every payment
/// method and carrier. The reads on a real database are the integration tests' (OrderExportTests).
/// </summary>
public sealed class OrderExportTests
{
    private static readonly Guid Province = Guid.NewGuid();
    private static readonly DateTimeOffset Created = new(2026, 11, 3, 21, 30, 0, TimeSpan.Zero);

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    // ---- the columns: one definition for the header and the cell ----

    [Fact]
    public void EveryColumn_HasAHeader_AndTheHeadersAreDistinct()
    {
        IReadOnlyList<string> headers = [.. OrderExportColumns.All.Select(c => c.Header)];

        Assert.All(headers, h => Assert.False(string.IsNullOrWhiteSpace(h)));
        Assert.Equal(headers.Count, headers.Distinct(StringComparer.Ordinal).Count());
        Assert.Contains("Sipariş numarası", headers);
        Assert.Contains("Geri ödeme tutarı", headers);
    }

    [Fact]
    public void ALine_IsAnItem_WithTheOrdersValuesRepeated_AndTheFrozenValuesInTheirCells_KK2_KK3_KK4()
    {
        InvoiceData order = Data(billing: Billing(), items: [Item("Gömlek", 2), Item("Kitap", 1)], breakdown: [new ShippingVatPart(20m, 30m, 5m), new ShippingVatPart(10m, 20m, 1.82m)]);
        OrderExportRow first = Row(order, order.Items[0], contactEmail: "ali@example.com", recipient: "Ali Veli");
        OrderExportRow second = Row(order, order.Items[1], contactEmail: "ali@example.com", recipient: "Ali Veli");

        string[] firstCells = Cells(first);
        string[] secondCells = Cells(second);

        Assert.Equal(OrderExportColumns.All.Count, firstCells.Length);
        Assert.Equal("2026-ABCD", Cell(firstCells, "Sipariş numarası"));
        Assert.Equal("2026-11-04", Cell(firstCells, "Sipariş tarihi"));
        Assert.Equal("Kart", Cell(firstCells, "Ödeme yöntemi"));
        Assert.Equal("1450,00", Cell(firstCells, "Sipariş toplamı"));
        Assert.Equal("50,00", Cell(firstCells, "Kargo ücreti"));
        Assert.Equal("20=5,00|10=1,82", Cell(firstCells, "Kargo ücretinin KDV dökümü"));
        Assert.Equal("Gömlek", Cell(firstCells, "Ürün adı"));
        Assert.Equal("Kitap", Cell(secondCells, "Ürün adı"));
        Assert.Equal("2", Cell(firstCells, "Adet"));
        Assert.Equal("500,00", Cell(firstCells, "Birim fiyat"));
        Assert.Equal("10", Cell(firstCells, "İndirim yüzdesi"));
        Assert.Equal("Örnek Ticaret", Cell(firstCells, "Ticaret unvanı"));
        Assert.Equal("Kadıköy", Cell(firstCells, "Fatura ilçesi"));
        Assert.Equal("ali@example.com", Cell(firstCells, "İletişim e-postası"));
        Assert.Equal("Ali Veli", Cell(firstCells, "Teslimat alıcı adı"));
        Assert.Equal("magaza.example.com", Cell(firstCells, "Sitenin alan adı"));

        // The order's own columns repeat on every line of the order.
        foreach (string header in new[] { "Sipariş numarası", "Sipariş tarihi", "Sipariş toplamı", "Kargo ücreti", "Ticaret unvanı", "Sitenin alan adı", "İletişim e-postası" })
        {
            Assert.Equal(Cell(firstCells, header), Cell(secondCells, header));
        }
    }

    [Fact]
    public void ADisposedOrder_HasEmptyBillingAddressEmailAndRecipientCells_AndKeepsAmountsAndTheFirmsIdentity_KK6()
    {
        InvoiceData order = Data(billing: null, items: [Item("Gömlek", 2)], breakdown: []);
        OrderExportRow row = Row(order, order.Items[0], contactEmail: null, recipient: null, billingProvince: null);

        string[] cells = Cells(row);

        foreach (string header in new[] { "Fatura alıcı adı", "Fatura ili", "Fatura ilçesi", "Fatura adresi", "İletişim e-postası", "Teslimat alıcı adı" })
        {
            Assert.Equal(string.Empty, Cell(cells, header));
        }

        Assert.Equal("1450,00", Cell(cells, "Sipariş toplamı"));
        Assert.Equal("500,00", Cell(cells, "Birim fiyat"));
        Assert.Equal("Örnek Ticaret", Cell(cells, "Ticaret unvanı"));
        Assert.Equal("1111111111", Cell(cells, "Vergi numarası"));
        Assert.Equal(string.Empty, Cell(cells, "Kargo ücretinin KDV dökümü"));
    }

    [Fact]
    public void ALatePayment_FillsTheFourColumns_AndAnOrderWithoutOneLeavesThemEmpty_KK5()
    {
        InvoiceData order = Data(billing: Billing(), items: [Item("Gömlek", 1)], breakdown: []);
        DateTimeOffset paidAt = new(2026, 11, 8, 20, 0, 0, TimeSpan.Zero);
        DateTimeOffset refundedAt = new(2026, 11, 9, 21, 30, 0, TimeSpan.Zero);

        string[] late = Cells(Row(order, order.Items[0], late: new LatePaymentFact(order.OrderId, paidAt, refundedAt, 1450m)));
        string[] none = Cells(Row(order, order.Items[0]));
        string[] unsent = Cells(Row(order, order.Items[0], late: new LatePaymentFact(order.OrderId, paidAt, null, 1450m)));

        Assert.Equal("2026-11-08", Cell(late, "Geç ödeme tarihi"));
        Assert.Equal("1450,00", Cell(late, "Geç ödeme tutarı"));
        Assert.Equal("2026-11-10", Cell(late, "Geri ödeme tarihi"));
        Assert.Equal("1450,00", Cell(late, "Geri ödeme tutarı"));
        foreach (string header in new[] { "Geç ödeme tarihi", "Geç ödeme tutarı", "Geri ödeme tarihi", "Geri ödeme tutarı" })
        {
            Assert.Equal(string.Empty, Cell(none, header));
        }

        // The refund not yet sent: no refund date, the amount stays.
        Assert.Equal(string.Empty, Cell(unsent, "Geri ödeme tarihi"));
        Assert.Equal("1450,00", Cell(unsent, "Geri ödeme tutarı"));
    }

    [Fact]
    public void TheHandOverDay_IsTheTurkeyDayOfTheMoment_AndEmptyWhenThereIsNone_KK4()
    {
        InvoiceData order = Data(billing: Billing(), items: [Item("Gömlek", 1)], breakdown: []);
        DateTimeOffset moment = new(2026, 11, 5, 21, 30, 0, TimeSpan.Zero);

        Assert.Equal("2026-11-06", Cell(Cells(Row(order, order.Items[0], handedOverAt: moment)), "Kargoya verme tarihi"));
        Assert.Equal(string.Empty, Cell(Cells(Row(order, order.Items[0], handedOverAt: null)), "Kargoya verme tarihi"));
        Assert.Equal("2026-11-04", Cell(Cells(Row(order, order.Items[0])), "Ödeme onay tarihi"));
    }

    [Fact]
    public void ACustomerTypedCell_IsMadeSafeAgainstAFormula_InTheOrderFileToo()
    {
        InvoiceData order = Data(billing: Billing() with { RecipientName = "=HYPERLINK(\"x\")" }, items: [Item("+Gömlek", 1)], breakdown: []);

        string[] cells = Cells(Row(order, order.Items[0], recipient: "@ali"));

        Assert.Equal("\"'=HYPERLINK(\"\"x\"\")\"", Cell(cells, "Fatura alıcı adı"));
        Assert.Equal("'+Gömlek", Cell(cells, "Ürün adı"));
        Assert.Equal("'@ali", Cell(cells, "Teslimat alıcı adı"));
    }

    [Fact]
    public async Task TheWholeFile_HasOneHeaderAndOneLinePerItem()
    {
        InvoiceData order = Data(billing: Billing(), items: [Item("Gömlek", 2), Item("Kitap", 1), Item("E-kitap", 1)], breakdown: []);
        using MemoryStream stream = new();

        int count = await CsvExportWriter.WriteAsync(stream, OrderExportColumns.All, Lines(order), ExportCeilings.Default, Ct);

        string text = System.Text.Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal(3, count);
        Assert.Equal(4, text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries).Length);
    }

    // ---- the shipping fee's VAT: rate=amount pairs joined with | ----

    [Fact]
    public void TheShippingVatBreakdown_IsRateEqualsAmountPairs_JoinedWithAPipe_AndEmptyWhenThereIsNone()
    {
        InvoiceData two = Data(billing: null, items: [], breakdown: [new ShippingVatPart(20m, 62.5m, 12.5m), new ShippingVatPart(10m, 35.2m, 3.2m)]);
        InvoiceData fractional = Data(billing: null, items: [], breakdown: [new ShippingVatPart(7.5m, 10m, 0.7m)]);
        InvoiceData none = Data(billing: null, items: [], breakdown: []);

        Assert.Equal("20=12,50|10=3,20", OrderExportColumns.ShippingVatBreakdown(two));
        Assert.Equal("7,5=0,70", OrderExportColumns.ShippingVatBreakdown(fractional));
        Assert.Null(OrderExportColumns.ShippingVatBreakdown(none));
    }

    // ---- the enum terms: every payment method and every carrier has one ----

    [Fact]
    public void EveryPaymentMethod_HasATerm_AndTheTermsAreDistinct()
    {
        PaymentMethod[] methods = Enum.GetValues<PaymentMethod>();

        string[] terms = [.. methods.Select(CsvEnumLabels.Of)];

        Assert.All(terms, t => Assert.False(string.IsNullOrWhiteSpace(t)));
        Assert.Equal(methods.Length, terms.Distinct(StringComparer.Ordinal).Count());
        Assert.Throws<ArgumentOutOfRangeException>(() => CsvEnumLabels.Of((PaymentMethod)99));
    }

    [Fact]
    public void EveryCarrierOfTheCatalog_ListedOrRetired_HasItsNameAsTheTerm_AndAnUnknownCodeIsWrittenAsItIs()
    {
        CarrierCatalog catalog = new(CarrierCatalog.Default.Active, [new RetiredCarrierEntry("EskiKargo", "Eski Kargo")]);

        foreach (CarrierEntry entry in catalog.Active)
        {
            Assert.Equal(entry.Name, CsvEnumLabels.OfCarrier(catalog, entry.Code));
        }

        Assert.Equal("Eski Kargo", CsvEnumLabels.OfCarrier(catalog, "EskiKargo"));
        Assert.Equal("BilinmeyenKargo", CsvEnumLabels.OfCarrier(catalog, "BilinmeyenKargo"));
    }

    // ---- the payment-completed set: one rule (D-847) ----

    [Fact]
    public void ThePaymentCompletedSet_IsThePaidAndTheRefundedStatuses_AndNoOther()
    {
        PaymentStatus[] completed = [PaymentStatus.Paid, PaymentStatus.PartiallyRefunded, PaymentStatus.Refunded];

        Assert.All(completed, s => Assert.True(OrderExport.IsPaymentCompleted(s), s.ToString()));
        Assert.All(Enum.GetValues<PaymentStatus>().Except(completed), s => Assert.False(OrderExport.IsPaymentCompleted(s), s.ToString()));
    }

    private static async IAsyncEnumerable<OrderExportRow> Lines(InvoiceData order)
    {
        foreach (InvoiceItem item in order.Items)
        {
            await Task.Yield();
            yield return Row(order, item, contactEmail: "ali@example.com", recipient: "Ali Veli");
        }
    }

    private static string[] Cells(OrderExportRow row) => [.. OrderExportColumns.All.Select(c => CsvExportWriter.Cell(c, row))];

    private static string Cell(string[] cells, string header)
    {
        int index = Enumerable.Range(0, OrderExportColumns.All.Count).Single(i => OrderExportColumns.All[i].Header == header);
        return cells[index];
    }

    private static OrderExportRow Row(
        InvoiceData order,
        InvoiceItem item,
        DateTimeOffset? handedOverAt = null,
        string? contactEmail = null,
        string? recipient = null,
        string? billingProvince = "Ankara",
        LatePaymentFact? late = null)
        => new(order, item, handedOverAt, contactEmail, recipient, billingProvince, "İstanbul", late);

    private static PartyAddress Billing() => new("Ayşe Yılmaz", null, Province, "Kadıköy", "Moda Caddesi 5");

    private static InvoiceItem Item(string name, int quantity)
        => new(Guid.NewGuid(), ProductType.Physical, name, "Kırmızı", null, quantity, 500m, 10m, 450m, 15m, 20m, 75m, null);

    private static InvoiceData Data(PartyAddress? billing, IReadOnlyList<InvoiceItem> items, IReadOnlyList<ShippingVatPart> breakdown)
        => new(
            Guid.NewGuid(),
            "2026-ABCD",
            Created,
            1450m,
            null,
            50m,
            breakdown,
            billing,
            new FrozenCompanyIdentity(CompanyType.Tradesman, "Örnek Ticaret", "1111111111", null, null, Province, "Çankaya", "Kızılay 2", "3121234567", "firma@example.com", "firma@hs01.kep.tr"),
            PaymentMethod.Card,
            Created.AddHours(1),
            "Yurtiçi Kargo",
            false,
            "magaza.example.com",
            items);
}
