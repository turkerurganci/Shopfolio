using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The composition of an order's invoice data as a pure rule (T357 KK2, KK4; 02 3.25.1, 3.25.3): every field comes from the order's own frozen values - the items' amounts, the fee and its
/// VAT split over the rates, the billing address, the frozen identity -, an item's hand-over moment follows its type, and the carrier's name is given once the order is shipped. The
/// database round trip and the identity change of the firm are the integration tests' (InvoiceDataTests).
/// </summary>
public sealed class InvoiceDataTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 6, 0, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset ConfirmedAt = Now.AddHours(1);
    private static readonly DateTimeOffset ShippedAt = Now.AddDays(1);
    private static readonly DateTimeOffset CompletedAt = Now.AddDays(2);
    private static readonly Guid Province = Guid.NewGuid();
    private const string Domain = "magaza.example.com";

    // ---- KK2: every field from the order's frozen values; the fee's VAT split over the rates ----

    [Fact]
    public void TheInvoiceData_GivesEveryFieldFromTheOrdersFrozenValues_AndTheFeesVatSplitOverTheRates_KK2()
    {
        CarrierEntry carrier = CarrierCatalog.Default.Active[0];
        Order order = NewOrder(PaymentMethod.Card, shippingFee: 50m, coupon: "YAZ10");
        order.FreezeShippingFeeVatBreakdown([new ShippingVatPart(20m, 30m, 5m), new ShippingVatPart(10m, 20m, 1.82m)]);
        order.Carrier = carrier.Code;
        order.TrackingNumber = "123456";
        OrderItem shirt = Item(order, ProductType.Physical, "Gömlek", 2, 500m, 20m, 166.67m, discount: (10m, 450m), couponShare: 15m);
        OrderItem book = Item(order, ProductType.Physical, "Kitap", 1, 200m, 10m, 18.18m);

        InvoiceData data = InvoiceDataReader.Compose(order, [shirt, book], ConfirmedAt, ShippedAt, Domain);

        Assert.Equal((order.Id, order.OrderNumber, order.CreatedAt, order.Total, "YAZ10", 50m), (data.OrderId, data.OrderNumber, data.CreatedAt, data.Total, data.CouponCode, data.ShippingFee));
        Assert.Equal([new ShippingVatPart(20m, 30m, 5m), new ShippingVatPart(10m, 20m, 1.82m)], data.ShippingFeeVatBreakdown);
        Assert.Equal(new PartyAddress("Ayşe Yılmaz", null, Province, "Kadıköy", "Moda Caddesi 5"), data.BillingAddress);
        Assert.Equal(
            new FrozenCompanyIdentity(CompanyType.Tradesman, "Örnek Ticaret", "1111111111", null, null, Province, "Çankaya", "Kızılay 2", "3121234567", "firma@example.com", "firma@hs01.kep.tr"),
            data.Company);
        Assert.Equal((PaymentMethod.Card, (DateTimeOffset?)ConfirmedAt, carrier.Name, false, Domain), (data.PaymentMethod, data.PaymentConfirmedAt, data.CarrierName, data.IsOwnVehicleDelivery, data.Domain));

        InvoiceItem first = data.Items[0];
        Assert.Equal(
            (shirt.Id, ProductType.Physical, "Gömlek", "Kırmızı", "M", 2, 500m, (decimal?)10m, (decimal?)450m, 15m, 20m, 166.67m, (DateTimeOffset?)ShippedAt),
            (first.OrderItemId, first.ProductType, first.ProductName, first.FirstOptionValue, first.SecondOptionValue, first.Quantity, first.UnitPrice, first.DiscountPercentage,
                first.DiscountedUnitPrice, first.CouponShare, first.VatRate, first.VatAmount, first.ShippedAt));
        Assert.Equal((book.Id, 10m, 18.18m, (decimal?)null, 0m), (data.Items[1].OrderItemId, data.Items[1].VatRate, data.Items[1].VatAmount, data.Items[1].DiscountedUnitPrice, data.Items[1].CouponShare));
    }

    [Fact]
    public void AFeeOfZero_HasAnEmptyBreakdown_KK2()
    {
        Order order = NewOrder(PaymentMethod.BankTransfer, shippingFee: 0m, coupon: null);
        order.FreezeShippingFeeVatBreakdown([]);

        InvoiceData data = InvoiceDataReader.Compose(order, [], null, null, Domain);

        Assert.Empty(data.ShippingFeeVatBreakdown);
        Assert.Null(data.CouponCode);
        Assert.Null(data.PaymentConfirmedAt);
        Assert.Equal(PaymentMethod.BankTransfer, data.PaymentMethod);
    }

    // ---- KK2: the hand-over moment by type - the shipment, the service's completion, the digital item's payment approval ----

    [Fact]
    public void TheHandOverMoment_IsTheShipmentForAPhysicalItem_TheCompletionForAService_AndThePaymentApprovalForADigitalItem_KK2()
    {
        Order order = NewOrder(PaymentMethod.Card, shippingFee: 0m, coupon: null);
        OrderItem physical = Item(order, ProductType.Physical, "Masa", 1, 100m, 20m, 16.67m);
        OrderItem completed = Item(order, ProductType.Service, "Kurulum", 1, 100m, 20m, 16.67m);
        completed.DeliveryMarkedAt = CompletedAt;
        completed.DeliveryDate = DateOnly.FromDateTime(CompletedAt.UtcDateTime);
        OrderItem waiting = Item(order, ProductType.Service, "Bakım", 1, 100m, 20m, 16.67m);
        OrderItem digital = Item(order, ProductType.Digital, "E-kitap", 1, 100m, 20m, 16.67m);

        InvoiceData shipped = InvoiceDataReader.Compose(order, [physical, completed, waiting, digital], ConfirmedAt, ShippedAt, Domain);
        InvoiceData notYet = InvoiceDataReader.Compose(order, [physical, completed, waiting, digital], ConfirmedAt, null, Domain);

        Assert.Equal([ShippedAt, CompletedAt, null, ConfirmedAt], shipped.Items.Select(i => i.ShippedAt));
        Assert.Equal([null, CompletedAt, null, ConfirmedAt], notYet.Items.Select(i => i.ShippedAt));
    }

    // ---- KK2: the carrier - once shipped; "Diğer" by its typed name; own vehicle without a name ----

    [Fact]
    public void TheCarrierName_IsGivenOnceShipped_DigerByItsTypedName_AndOwnVehicleWithoutAName_KK2()
    {
        Order other = NewOrder(PaymentMethod.Card, shippingFee: 0m, coupon: null);
        other.OtherCarrierName = "Yerel Kurye";
        Order own = NewOrder(PaymentMethod.Card, shippingFee: 0m, coupon: null);
        own.IsOwnVehicleDelivery = true;

        Assert.Equal("Yerel Kurye", InvoiceDataReader.Compose(other, [], ConfirmedAt, ShippedAt, Domain).CarrierName);
        Assert.Null(InvoiceDataReader.Compose(other, [], ConfirmedAt, null, Domain).CarrierName);
        InvoiceData ownData = InvoiceDataReader.Compose(own, [], ConfirmedAt, ShippedAt, Domain);
        Assert.Null(ownData.CarrierName);
        Assert.True(ownData.IsOwnVehicleDelivery);
    }

    [Fact]
    public void ADisposedOrder_HasNoBillingAddress_ButKeepsItsAmountsAndIdentity()
    {
        Order order = NewOrder(PaymentMethod.Card, shippingFee: 0m, coupon: null);
        order.BillingRecipientName = null;
        order.BillingProvinceId = null;
        order.BillingDistrict = null;
        order.BillingAddressLine = null;

        InvoiceData data = InvoiceDataReader.Compose(order, [], ConfirmedAt, null, Domain);

        Assert.Null(data.BillingAddress);
        Assert.Equal("Örnek Ticaret", data.Company.CompanyLegalName);
        Assert.Equal(order.Total, data.Total);
    }

    // ---- KK4: the identity is the order's frozen copy ----

    [Fact]
    public void TheIdentity_IsTheOrdersFrozenCopyByItsType_KK4()
    {
        Order order = Order.Create("2026-ABCD", "access", "confirm", PaymentMethod.Card, true, "ali@example.com", 100m, 0m, Now.AddHours(1), Now);
        OrderFreeze.FreezeLegalIdentity(order, Identity(CompanyType.LegalEntity), new LegalTextVersionIds(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), null);

        FrozenCompanyIdentity company = InvoiceDataReader.Compose(order, [], null, null, Domain).Company;

        Assert.Equal((CompanyType.LegalEntity, (string?)null, "0123456789012345", "12345"), (company.CompanyType, company.CompanyTaxIdNumber, company.CompanyMersisNumber, company.CompanyTradeRegistryNumber));
    }

    private static Order NewOrder(PaymentMethod method, decimal shippingFee, string? coupon)
    {
        Order order = Order.Create("2026-ABCD", "access", "confirm", method, true, "ali@example.com", 1450m, shippingFee, Now.AddHours(1), Now);
        order.FreezeAddresses(("Ayşe Yılmaz", "5321234567", Province, "Kadıköy", "Moda Caddesi 5"), ("Ayşe Yılmaz", Province, "Kadıköy", "Moda Caddesi 5"));
        string? iban = method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null;
        OrderFreeze.FreezeLegalIdentity(order, Identity(CompanyType.Tradesman), new LegalTextVersionIds(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), iban);
        if (coupon is not null)
        {
            order.CouponId = Guid.NewGuid();
            order.CouponCode = coupon;
        }

        return order;
    }

    private static OrderItem Item(
        Order order, ProductType type, string name, int quantity, decimal unitPrice, decimal vatRate, decimal vatAmount, (decimal Percentage, decimal Price)? discount = null, decimal couponShare = 0m)
    {
        OrderItem item = OrderItem.Create(order.Id, Guid.NewGuid(), Guid.NewGuid(), type, name, quantity, unitPrice, vatRate, vatAmount, Now);
        item.FirstOptionValue = "Kırmızı";
        item.SecondOptionValue = "M";
        item.CouponShare = couponShare;
        if (discount is { } d)
        {
            item.DiscountPercentage = d.Percentage;
            item.DiscountedUnitPrice = d.Price;
            item.ReferencePrice = unitPrice;
        }

        return item;
    }

    private static CompanyIdentity Identity(CompanyType type)
        => new(
            1, type, "Örnek Ticaret", "1111111111", "0123456789012345", "12345", Province, "Çankaya", "Kızılay 2", "3121234567", "firma@example.com", "firma@hs01.kep.tr",
            "Oda", null, "Kurallar", null);
}
