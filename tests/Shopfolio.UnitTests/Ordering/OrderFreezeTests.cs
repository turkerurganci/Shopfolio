using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The freezing of an order's values as pure rules (T355 KK1, KK2, KK4, KK5; 02 3.23.1-3.23.4): every value of the live read is copied flat into the item, each type keeps only its own
/// attributes, the amounts must be of the price that applies, the firm's identity freezes by its type with the versions and the IBAN, and the documents' parties are the confirmation's.
/// The database round trip is the integration tests' (FrozenValuesTests).
/// </summary>
public sealed class OrderFreezeTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 20, 6, 0, 0, TimeSpan.Zero);
    private static readonly Guid Province = Guid.NewGuid();
    private static readonly LegalTextVersionIds Versions = new(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());

    // ---- KK1: the values that produced the paid amount and the description ----

    [Fact]
    public void AnItem_CopiesEveryValueOfTheRead_AndTheCouponShareAndVatOfItsAmounts_KK1()
    {
        DiscountLine discount = new(800m, 950m, 20m, new DateOnly(2026, 11, 20), new DateOnly(2026, 11, 25));
        OrderItemSource source = Source(ProductType.Physical, discount: discount, exemption: WithdrawalExemptionReason.SealedMedia);
        ItemAmounts amounts = ItemPricing.Price(new ItemPriceInput(800m, 2, 30m, 20m));

        OrderItem item = OrderFreeze.FreezeItem(Guid.NewGuid(), source, 2, amounts, 3, Now);

        Assert.Equal(
            (source.ProductId, source.VariantId, source.ProductName, source.FirstOptionValue, source.SecondOptionValue, source.ImageId, 1000m, 20m, 800m, 950m, 20m),
            ((Guid)item.ProductId!, (Guid)item.VariantId!, item.ProductName, item.FirstOptionValue, item.SecondOptionValue, item.ImageId, item.UnitPrice, item.DiscountPercentage!.Value, item.DiscountedUnitPrice!.Value, item.ReferencePrice!.Value, item.VatRate));
        Assert.Equal(30m, item.CouponShare);
        Assert.Equal(amounts.Vat, item.VatAmount);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(Now, item.CreatedAt);
    }

    [Fact]
    public void WithoutADiscount_TheThreeDiscountFieldsStayEmpty_KK1()
    {
        OrderItem item = OrderFreeze.FreezeItem(Guid.NewGuid(), Source(ProductType.Physical), 1, ItemPricing.Price(new ItemPriceInput(1000m, 1, 0m, 20m)), 3, Now);

        Assert.Null(item.DiscountPercentage);
        Assert.Null(item.DiscountedUnitPrice);
        Assert.Null(item.ReferencePrice);
        Assert.Equal(0m, item.CouponShare);
    }

    [Fact]
    public void AmountsOfAnotherPrice_AreAProgrammingError_KK1()
    {
        DiscountLine discount = new(800m, 950m, 20m, new DateOnly(2026, 11, 20), new DateOnly(2026, 11, 25));

        // The amounts of the undiscounted price while the discount runs: the item would not be the amount the customer confirmed.
        Assert.Throws<ArgumentException>(
            () => OrderFreeze.FreezeItem(Guid.NewGuid(), Source(ProductType.Physical, discount: discount), 1, ItemPricing.Price(new ItemPriceInput(1000m, 1, 0m, 20m)), 3, Now));
    }

    // ---- KK2: each type freezes its own delivery line and withdrawal attributes ----

    [Fact]
    public void EachType_FreezesOnlyItsOwnAttributes_KK2()
    {
        Guid file = Guid.NewGuid();
        OrderItem physical = Freeze(Source(ProductType.Physical, exemption: WithdrawalExemptionReason.HygieneSensitive, fulfilment: 4, file: file), 7);
        OrderItem service = Freeze(Source(ProductType.Service, exemption: WithdrawalExemptionReason.HygieneSensitive, fulfilment: 4, file: file), 7);
        OrderItem digital = Freeze(Source(ProductType.Digital, exemption: WithdrawalExemptionReason.HygieneSensitive, fulfilment: 4, file: file), 7);

        Assert.Equal((ProductType.Physical, WithdrawalExemptionReason.HygieneSensitive, (int?)null, (int?)null, (Guid?)null), (physical.ProductType, physical.WithdrawalExemptionReason!.Value, physical.FulfilmentPeriod, physical.DownloadAllowance, physical.DigitalFileId));
        Assert.Equal((ProductType.Service, (WithdrawalExemptionReason?)null, (int?)4, (int?)null, (Guid?)null), (service.ProductType, service.WithdrawalExemptionReason, service.FulfilmentPeriod, service.DownloadAllowance, service.DigitalFileId));
        Assert.Equal((ProductType.Digital, (WithdrawalExemptionReason?)null, (int?)null, (int?)7, (Guid?)file), (digital.ProductType, digital.WithdrawalExemptionReason, digital.FulfilmentPeriod, digital.DownloadAllowance, digital.DigitalFileId));
    }

    [Fact]
    public void ADigitalItem_NeedsADownloadAllowanceAboveZero_KK2()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Freeze(Source(ProductType.Digital), 0));
        Assert.Throws<ArgumentOutOfRangeException>(
            () => OrderFreeze.FreezeItem(Guid.NewGuid(), Source(ProductType.Physical), 0, ItemPricing.Price(new ItemPriceInput(1000m, 1, 0m, 20m)), 3, Now));
    }

    // ---- KK4: the firm's identity by its type, the versions and the IBAN ----

    [Theory]
    [InlineData(CompanyType.Tradesman, "1111111111", null, null)]
    [InlineData(CompanyType.NaturalPersonMerchant, null, "0123456789012345", "12345")]
    [InlineData(CompanyType.LegalEntity, null, "0123456789012345", "12345")]
    public void TheIdentity_FreezesTheFieldsOfItsType_TheAddressContactAndTheVersions_KK4(CompanyType type, string? tax, string? mersis, string? registry)
    {
        Order order = NewOrder(PaymentMethod.Card);

        OrderFreeze.FreezeLegalIdentity(order, Identity(type), Versions, null);

        Assert.Equal((type, "Örnek Ticaret", tax, mersis, registry), (order.CompanyType, order.CompanyLegalName, order.CompanyTaxIdNumber, order.CompanyMersisNumber, order.CompanyTradeRegistryNumber));
        Assert.Equal((Province, "Çankaya", "Kızılay 2", "3121234567", "firma@example.com", "firma@hs01.kep.tr"), (order.CompanyProvinceId, order.CompanyDistrict, order.CompanyAddressLine, order.CompanyPhone, order.CompanyEmail, order.CompanyKepAddress));
        Assert.Equal((Versions.PreInformationForm!.Value, Versions.DistanceSalesContract!.Value, Versions.PrivacyNotice!.Value), (order.PreInformationFormVersionId, order.DistanceSalesContractVersionId, order.PrivacyNoticeVersionId));
        Assert.Null(order.BankTransferIban);
    }

    [Fact]
    public void TheIban_FreezesExactlyOnABankTransferOrder_KK4()
    {
        Order transfer = NewOrder(PaymentMethod.BankTransfer);
        OrderFreeze.FreezeLegalIdentity(transfer, Identity(CompanyType.Tradesman), Versions, "TR330006100519786457841326");
        Assert.Equal("TR330006100519786457841326", transfer.BankTransferIban);

        Assert.Throws<ArgumentException>(() => OrderFreeze.FreezeLegalIdentity(NewOrder(PaymentMethod.BankTransfer), Identity(CompanyType.Tradesman), Versions, null));
        Assert.Throws<ArgumentException>(() => OrderFreeze.FreezeLegalIdentity(NewOrder(PaymentMethod.Card), Identity(CompanyType.Tradesman), Versions, "TR330006100519786457841326"));
    }

    [Fact]
    public void AnEmptyIdentityFieldOrVersion_IsAProgrammingError_KK4()
    {
        Assert.Throws<InvalidOperationException>(() => OrderFreeze.FreezeLegalIdentity(NewOrder(PaymentMethod.Card), Identity(CompanyType.Tradesman) with { TaxIdNumber = null }, Versions, null));
        Assert.Throws<InvalidOperationException>(() => OrderFreeze.FreezeLegalIdentity(NewOrder(PaymentMethod.Card), Identity(CompanyType.LegalEntity) with { MersisNumber = null }, Versions, null));
        Assert.Throws<InvalidOperationException>(() => OrderFreeze.FreezeLegalIdentity(NewOrder(PaymentMethod.Card), Identity(CompanyType.Tradesman) with { CompanyType = null }, Versions, null));
        Assert.Throws<InvalidOperationException>(() => OrderFreeze.FreezeLegalIdentity(NewOrder(PaymentMethod.Card), Identity(CompanyType.Tradesman), Versions with { PrivacyNotice = null }, null));
    }

    [Fact]
    public void TheDocumentsParties_AreTheFirstHistoryRowsOldValues_OrTheOrdersOwnWhenUncorrected_KK4()
    {
        Order order = NewOrder(PaymentMethod.Card);
        order.FreezeAddresses(("Ali Veli", "5321234567", Province, "Mamak", "Son Sokak 3"), ("Ali Veli", Province, "Çankaya", "Atatürk Bulvarı 1"));
        order.SetContactEmail("son@example.com");
        Guid admin = Guid.NewGuid();
        OrderAddressHistory[] addresses =
        [
            OrderAddressHistory.Create(order.Id, AddressType.ShippingAddress, ("Ali Veli", "5329999999", Province, "Keçiören", "Yeni Sokak 2"), ("Ali Veli", "5321234567", Province, "Mamak", "Son Sokak 3"), admin, "Veli", Now.AddHours(2)),
            OrderAddressHistory.Create(order.Id, AddressType.ShippingAddress, ("Ali Veli", "5328888888", Province, "Çankaya", "İlk Sokak 1"), ("Ali Veli", "5329999999", Province, "Keçiören", "Yeni Sokak 2"), admin, "Veli", Now.AddHours(1)),
        ];
        OrderEmailHistory[] emails =
        [
            OrderEmailHistory.Create(order.Id, "ikinci@example.com", "son@example.com", admin, "Veli", Now.AddHours(2)),
            OrderEmailHistory.Create(order.Id, "ilk@example.com", "ikinci@example.com", admin, "Veli", Now.AddHours(1)),
        ];

        ConfirmationParties parties = OrderFreeze.PartiesAtConfirmation(order, addresses, emails);

        Assert.Equal("ilk@example.com", parties.ContactEmail);
        Assert.Equal(new PartyAddress("Ali Veli", "5328888888", Province, "Çankaya", "İlk Sokak 1"), parties.Shipping);
        Assert.Equal(new PartyAddress("Ali Veli", null, Province, "Çankaya", "Atatürk Bulvarı 1"), parties.Billing);

        ConfirmationParties uncorrected = OrderFreeze.PartiesAtConfirmation(order, [], []);
        Assert.Equal("son@example.com", uncorrected.ContactEmail);
        Assert.Equal(new PartyAddress("Ali Veli", "5321234567", Province, "Mamak", "Son Sokak 3"), uncorrected.Shipping);
    }

    [Fact]
    public void ADisposedOrder_HasNoParties_KK4()
    {
        Order order = NewOrder(PaymentMethod.Card);
        order.FreezeAddresses(null, ("Ali Veli", Province, "Çankaya", "Atatürk Bulvarı 1"));
        order.ErasePersonalData();

        Assert.Equal(new ConfirmationParties(null, null, null), OrderFreeze.PartiesAtConfirmation(order, [], []));
    }

    // ---- KK5: the order's frozen promise is written once ----

    [Fact]
    public void TheShippingPromise_IsWrittenOnceAtConfirmation_KK5()
    {
        Order order = NewOrder(PaymentMethod.Card);
        order.FreezeShippingPromise(3);

        Assert.Throws<InvalidOperationException>(() => order.FreezeShippingPromise(5));
        Assert.Equal(3, order.ShippingPeriod);
    }

    private static OrderItem Freeze(OrderItemSource source, int allowance)
        => OrderFreeze.FreezeItem(Guid.NewGuid(), source, 1, ItemPricing.Price(new ItemPriceInput(source.UnitPrice, 1, 0m, source.VatRate)), allowance, Now);

    private static OrderItemSource Source(
        ProductType type, DiscountLine? discount = null, WithdrawalExemptionReason? exemption = null, int? fulfilment = null, Guid? file = null)
        => new(Guid.NewGuid(), Guid.NewGuid(), type, "Masa", "Kırmızı", "M", Guid.NewGuid(), 1000m, 20m, discount, exemption, fulfilment, file);

    private static Order NewOrder(PaymentMethod method)
        => Order.Create("2026-ABCD", "access", "confirm", method, true, "Ali@Example.com", 100m, 0m, Now.AddHours(1), Now);

    private static CompanyIdentity Identity(CompanyType type)
        => new(
            1, type, "Örnek Ticaret", "1111111111", "0123456789012345", "12345", Province, "Çankaya", "Kızılay 2", "3121234567", "firma@example.com", "firma@hs01.kep.tr",
            "Oda", null, "Kurallar", null);
}
