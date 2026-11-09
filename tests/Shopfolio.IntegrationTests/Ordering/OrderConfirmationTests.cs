using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>The collection of the confirmation's tests (T348): one database copied from the template.</summary>
[CollectionDefinition(OrderConfirmationDefinition.Name)]
public sealed class OrderConfirmationDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "OrderConfirmation";
}

/// <summary>
/// The confirmation on a real PostgreSQL with the real Catalog, Settings and Notifications modules (T348 KK1-KK8; 02 3.17.1-3.17.8): the order is born with its number, its frozen
/// items, addresses and versions, its reservation and its window in one go, and B-1 and F-1 are queued; any difference from the confirmed summary stops it and nothing is born; a cart
/// emptied at confirmation makes no order; the coupon right and the stock are reserved and freed with the order; a retry is a new order with a new number; a new confirmation from the
/// same cart cancels the earlier unpaid order and is not stopped by its reservation, while a merged guest cart's order stays; the never-paid order the system cancelled leaves the
/// customer's list and stays in the panel.
/// </summary>
[Collection(OrderConfirmationDefinition.Name)]
public sealed class OrderConfirmationTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Now = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);
    private const string Iban = "TR330006100519786457841326";

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Now);

    private int _counter;
    private bool _cardLineOpen = true;
    private Guid _provinceId;
    private Guid _formId;
    private Guid _contractId;

    public OrderConfirmationTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, stock_reservations, transition_records, cart_items, carts, order_items, orders, coupons, variants, products, customers, "
            + "legal_text_versions, companies, company_settings, provinces CASCADE");
        Province province = Province.Create("06", "Ankara", Now);
        CompanySettings settings = CompanySettings.Create(Now);
        settings.BankTransferPaymentWindow = 2;
        settings.DefaultShippingPeriod = 2;
        settings.DownloadAllowance = 1;
        settings.IsBankTransferEnabled = true;
        settings.BankTransferIban = Iban;
        settings.HomepageLayout = Shopfolio.Modules.CorporateContent.Contracts.HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        Company company = Company.Create(Now);
        company.CompanyType = CompanyType.Tradesman;
        company.LegalName = "Örnek Ticaret";
        company.TaxIdNumber = "1234567890";
        company.ProvinceId = province.Id;
        company.District = "Çankaya";
        company.AddressLine = "Kızılay 2";
        company.Phone = "3121234567";
        company.Email = "firma@example.com";
        company.KepAddress = "firma@hs01.kep.tr";
        company.ChamberName = "Ankara Esnaf Odası";
        company.ProfessionalConductRules = "Meslek kuralları";
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Now.AddDays(-1), "ön bilgilendirme metni", Now.AddDays(-1));
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Now.AddDays(-1), "mesafeli satış metni", Now.AddDays(-1));
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Now.AddDays(-1), "aydınlatma metni", Now.AddDays(-1)).WithHosting();
        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(province.Id, Now));
            c.CompanySettings.Add(settings);
            c.Companies.Add(company);
            c.LegalTextVersions.AddRange(form, contract, notice);
        });
        _provinceId = province.Id;
        _formId = form.Id;
        _contractId = contract.Id;
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: number, frozen values and reservation together; B-1 and F-1 are queued ----

    [Fact]
    public async Task Confirmation_IssuesTheNumber_FreezesTheOrder_ReservesStockAndCoupon_StartsTheWindow_AndQueuesB1AndF1_KK1_KK3()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        Guid couponId = await SeedCouponAsync("T348K", 10m);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        Guid cartId = await SeedCartAsync(owner, (variantId, 2));

        OrderSummaryInput input = Input(owner, PaymentMethod.Card, "T348K");
        ConfirmationSummary summary = (await SummarizeAsync(input))!;
        OrderConfirmationResult result = await ConfirmAsync(input, summary);

        Assert.False(result.IsRejected);
        Order order = await ReadOrderAsync(result.OrderId!.Value);
        Assert.Equal(result.OrderNumber, order.OrderNumber);
        Assert.Matches("^2026-", order.OrderNumber);
        Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending), (order.OrderStatus, order.PaymentStatus));
        Assert.Equal(summary.Total, order.Total);
        Assert.Equal(1800m, order.Total);
        Assert.Equal((cartId, couponId, "T348K"), (order.CartId!.Value, order.CouponId!.Value, order.CouponCode));
        Assert.Equal(Now + TimeSpan.FromMinutes(ProductConstants.CardPaymentMinutes), order.PaymentWindowEndsAt);
        Assert.Equal((_formId, _contractId), (order.PreInformationFormVersionId, order.DistanceSalesContractVersionId));
        Assert.Equal(("Ali Veli", _provinceId), (order.ShippingRecipientName, order.ShippingProvinceId!.Value));
        Assert.Contains("ön bilgilendirme metni", order.PreInformationFormText, StringComparison.Ordinal);
        Assert.Equal(2, order.ShippingPeriod);
        Assert.Null(order.BankTransferIban);

        OrderItem item = Assert.Single(await ReadItemsAsync(order.Id));
        Assert.Equal(("Lamba", 2, 1000m, 200m), (item.ProductName, item.Quantity, item.UnitPrice, item.CouponShare));

        // KK3: the stock and the coupon right are reserved, not yet deducted.
        Assert.Equal(2, await ScalarAsync($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
        Assert.Equal(5, await ScalarAsync($"SELECT stock_quantity FROM variants WHERE id = '{variantId}'"));
        Assert.Equal(1, await ScalarAsync($"SELECT reserved_count FROM coupons WHERE id = '{couponId}'"));
        Assert.Equal(2, await ScalarAsync($"SELECT count(*) FROM stock_reservations WHERE order_id = '{order.Id}'"));

        // B-1 to the contact e-mail and F-1 to the firm.
        List<DispatchRecord> dispatches = await ReadDispatchesAsync();
        Assert.Contains(dispatches, d => d.DispatchType == DispatchType.B1 && d.OrderId == order.Id && d.RecipientEmail == "Ali@Example.com");
        Assert.Contains(dispatches, d => d.DispatchType == DispatchType.F1 && d.RecipientEmail == OrderingTestServices.CompanyEmail);
    }

    [Fact]
    public async Task BankTransferConfirmation_FreezesTheIban_AndStartsTheWindowInBusinessDays_KK4()
    {
        Guid variantId = await SeedVariantAsync("Masa", 500m, stock: 3);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 1));

        OrderSummaryInput input = Input(owner, PaymentMethod.BankTransfer, null);
        OrderConfirmationResult result = await ConfirmAsync(input, (await SummarizeAsync(input))!);

        Order order = await ReadOrderAsync(result.OrderId!.Value);
        PaymentWindow expected = await RunAsync(sp => sp.GetRequiredService<PaymentWindows>().StartAsync(PaymentMethod.BankTransfer, Now, Ct));
        Assert.Equal(Iban, order.BankTransferIban);
        Assert.Equal(expected.EndsAt, order.PaymentWindowEndsAt);
        Assert.Equal(expected.ReminderAt, order.PaymentReminderAt);
        Assert.Equal(1, await ScalarAsync($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
    }

    // ---- T353: B-2, the frozen IBAN after a change, and the open unpaid order ceiling (02 3.21.5, 3.21.6, 9.2) ----

    [Fact]
    public async Task TransferConfirmation_QueuesB2WithTheWindowEnd_AndACardConfirmationDoesNot_T353_KK4_D770()
    {
        Guid variantId = await SeedVariantAsync("Masa", 500m, stock: 5);
        CartOwner byTransferOwner = CartOwner.ForMember(await SeedCustomerAsync());
        CartOwner byCardOwner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(byTransferOwner, (variantId, 1));
        await SeedCartAsync(byCardOwner, (variantId, 1));
        OrderSummaryInput transfer = Input(byTransferOwner, PaymentMethod.BankTransfer, null);
        OrderSummaryInput card = Input(byCardOwner, PaymentMethod.Card, null);

        OrderConfirmationResult byTransfer = await ConfirmAsync(transfer, (await SummarizeAsync(transfer))!);
        OrderConfirmationResult byCard = await ConfirmAsync(card, (await SummarizeAsync(card))!);

        List<DispatchRecord> dispatches = await ReadDispatchesAsync();
        DispatchRecord b2 = Assert.Single(dispatches, d => d.DispatchType == DispatchType.B2);
        Assert.Equal(byTransfer.OrderId, b2.OrderId);
        Assert.Equal("Ali@Example.com", b2.RecipientEmail);
        Order order = await ReadOrderAsync(byTransfer.OrderId!.Value);
        Assert.Equal(new TransferChosenNotice(order.Id, order.PaymentWindowEndsAt), CustomerNoticeValues.Read(b2.DispatchType, order.Id, b2.EventValues));
        Assert.DoesNotContain(dispatches, d => d.DispatchType == DispatchType.B2 && d.OrderId == byCard.OrderId);
    }

    [Fact]
    public async Task AChangedIban_ReachesOnlyTheNewOrders_TheOpenOneKeepsTheIbanItFroze_T353_KK3()
    {
        Guid variantId = await SeedVariantAsync("Masa", 500m, stock: 5);
        CartOwner first = CartOwner.ForMember(await SeedCustomerAsync());
        CartOwner second = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(first, (variantId, 1));
        await SeedCartAsync(second, (variantId, 1));
        OrderSummaryInput before = Input(first, PaymentMethod.BankTransfer, null);
        OrderConfirmationResult older = await ConfirmAsync(before, (await SummarizeAsync(before))!);

        await ExecuteAsync("UPDATE company_settings SET bank_transfer_iban = 'TR320010009999901234567890'");
        OrderSummaryInput after = Input(second, PaymentMethod.BankTransfer, null);
        OrderConfirmationResult newer = await ConfirmAsync(after, (await SummarizeAsync(after))!);

        Assert.Equal(Iban, (await ReadOrderAsync(older.OrderId!.Value)).BankTransferIban);
        Assert.Equal("TR320010009999901234567890", (await ReadOrderAsync(newer.OrderId!.Value)).BankTransferIban);
    }

    [Fact]
    public async Task AFullOpenUnpaidOrderCeiling_ClosesTheTransfer_LeavesTheCardOpen_AndTheSameCartsNewConfirmationIsNotCountedTwice_T353_KK2()
    {
        Guid variantId = await SeedVariantAsync("Masa", 500m, stock: 20);
        List<CartOwner> owners = [];
        for (int i = 0; i < OpenUnpaidOrderLimit.Ceiling + 1; i++)
        {
            CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
            await SeedCartAsync(owner, (variantId, 1));
            owners.Add(owner);
        }

        List<Guid> open = [];
        for (int i = 0; i < OpenUnpaidOrderLimit.Ceiling; i++)
        {
            OrderSummaryInput input = Input(owners[i], PaymentMethod.BankTransfer, null);
            OrderConfirmationResult placed = await ConfirmAsync(input, (await SummarizeAsync(input))!);
            Assert.False(placed.IsRejected);
            open.Add(placed.OrderId!.Value);
        }

        // The ceiling is full on the IP axis: the next customer cannot choose the transfer and nothing is born.
        long ordersBefore = await ScalarAsync("SELECT count(*) FROM orders");
        OrderSummaryInput lastByTransfer = Input(owners[^1], PaymentMethod.BankTransfer, null);
        OrderConfirmationResult refused = await ConfirmAsync(lastByTransfer, (await SummarizeAsync(lastByTransfer))!);
        Assert.Equal(OrderConfirmationRefusal.AttemptLimitExceeded, refused.Refusal);
        Assert.Equal(AttemptLimit.L8, refused.ExceededLimit);
        Assert.Equal(ordersBefore, await ScalarAsync("SELECT count(*) FROM orders"));

        // The first customer's new confirmation from the same cart replaces its own open order, so it is not stopped by it (02 3.17.7).
        OrderSummaryInput again = Input(owners[0], PaymentMethod.BankTransfer, null);
        OrderConfirmationResult replaced = await ConfirmAsync(again, (await SummarizeAsync(again))!);
        Assert.False(replaced.IsRejected);
        Assert.Equal(OrderStatus.Cancelled, (await ReadOrderAsync(open[0])).OrderStatus);

        // The card stays open to the customer the transfer was closed to.
        OrderSummaryInput lastByCard = Input(owners[^1], PaymentMethod.Card, null);
        OrderConfirmationResult byCard = await ConfirmAsync(lastByCard, (await SummarizeAsync(lastByCard))!);
        Assert.False(byCard.IsRejected);
        Assert.Equal(PaymentMethod.Card, (await ReadOrderAsync(byCard.OrderId!.Value)).PaymentMethod);
    }

    [Fact]
    public async Task AMixedConfirmation_FreezesTheDigitalAndTheServiceBox_WithTheTextShownAndTheItemsTheyCover_InTheSameGo_KK1()
    {
        Guid lamp = await SeedVariantAsync("Lamba", 100m, stock: 5);
        Guid template = await SeedVariantAsync("Şablon", 80m, stock: null, ProductType.Digital);
        Guid guide = await SeedVariantAsync("Rehber", 60m, stock: null, ProductType.Digital);
        Guid fitting = await SeedVariantAsync("Montaj", 200m, stock: null, ProductType.Service);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (lamp, 1), (template, 1), (guide, 1), (fitting, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;

        // The step offers four boxes; the digital one names both digital items, the service one the service.
        IReadOnlyList<ConfirmationBoxOffer> shown = await RunAsync(async sp => (await sp.GetRequiredService<IConfirmationStep>().GetViewAsync(seen, Ct)).Boxes);
        Assert.Equal(4, shown.Count);
        OrderConfirmationResult result = await ConfirmAsync(input, seen, new TickedBoxes(true, true, true, true));

        Assert.False(result.IsRejected);
        Guid orderId = result.OrderId!.Value;
        List<OrderItem> items = await ReadItemsAsync(orderId);
        await using AppDbContext reader = NewContext();
        List<CheckboxAcceptance> records = await reader.CheckboxAcceptances.AsNoTracking().Where(a => a.OrderId == orderId).OrderBy(a => a.CheckboxType).ToListAsync(Ct);
        Assert.Equal([CheckboxType.Digital, CheckboxType.Service], [.. records.Select(r => r.CheckboxType)]);
        Assert.Equal(shown.Single(b => b.Box == ConfirmationBox.DigitalWithdrawalWaiver).Text, records[0].ShownText);
        Assert.Equal(shown.Single(b => b.Box == ConfirmationBox.ServiceFulfilmentConsent).Text, records[1].ShownText);

        Guid[] recordIds = [.. records.Select(r => r.Id)];
        List<CheckboxAcceptanceOrderItem> links = await reader.CheckboxAcceptanceOrderItems.AsNoTracking().Where(l => recordIds.Contains(l.CheckboxAcceptanceId)).ToListAsync(Ct);
        Assert.Equal(
            items.Where(i => i.ProductType == ProductType.Digital).Select(i => i.Id).Order(),
            links.Where(l => l.CheckboxAcceptanceId == records[0].Id).Select(l => l.OrderItemId).Order());
        Assert.Equal(items.Single(i => i.ProductType == ProductType.Service).Id, links.Single(l => l.CheckboxAcceptanceId == records[1].Id).OrderItemId);
        Assert.DoesNotContain(links, l => l.OrderItemId == items.Single(i => i.ProductType == ProductType.Physical).Id);
    }

    [Fact]
    public async Task AnUntickedDigitalBox_StopsTheOrder_AndNoBoxRecordIsBorn_KK1_KK2()
    {
        Guid template = await SeedVariantAsync("Şablon", 80m, stock: null, ProductType.Digital);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (template, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);

        OrderConfirmationResult result = await ConfirmAsync(input, (await SummarizeAsync(input))!);

        Assert.Equal(OrderConfirmationRefusal.ConfirmationStepNotReady, result.Refusal);
        Assert.Equal([ConfirmationBox.DigitalWithdrawalWaiver], result.StepCheck!.MissingBoxes);
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM checkbox_acceptances"));
    }

    // ---- KK2: every difference stops the order; an emptied cart makes nothing ----

    [Fact]
    public async Task APriceChange_EitherWay_StopsTheOrder_AndTellsWhatChanged_NothingIsBorn_KK2()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;

        foreach (decimal price in new[] { 1200m, 900m })
        {
            await ExecuteAsync($"UPDATE variants SET price = {price} WHERE id = '{variantId}'");
            OrderConfirmationResult result = await ConfirmAsync(input, seen);

            Assert.Equal(OrderConfirmationRefusal.SummaryChanged, result.Refusal);
            Assert.Contains(result.Changes, c => c.Kind == SummaryChangeKind.UnitPrice && c.ProductName == "Lamba" && c.Before == 1000m && c.After == price);
            Assert.Equal(price, result.CurrentSummary!.Total);
        }

        await AssertNothingBornAsync(variantId);
    }

    [Fact]
    public async Task ACartEmptiedAtConfirmation_BearsNoOrderNumberPaymentOrReservation_KK2()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;

        await ExecuteAsync($"UPDATE variants SET stock_quantity = 0 WHERE id = '{variantId}'");
        OrderConfirmationResult result = await ConfirmAsync(input, seen);

        Assert.Equal(OrderConfirmationRefusal.NoOrderableItem, result.Refusal);
        await AssertNothingBornAsync(variantId);
    }

    [Fact]
    public async Task ARisenLegalTextVersion_StopsTheOrder_KK2()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;
        await SaveAsync(c => c.LegalTextVersions.Add(LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 2, Now, "yeni sözleşme", Now)));

        OrderConfirmationResult result = await ConfirmAsync(input, seen);

        Assert.Equal(OrderConfirmationRefusal.ConfirmationStepNotReady, result.Refusal);
        Assert.True(result.StepCheck!.LegalTextVersionRaised);
        await AssertNothingBornAsync(variantId);
    }

    [Fact]
    public async Task ADeliveryAreaThatNoLongerServesTheShippingProvince_OrAPhysicalOrderWithoutAShippingAddress_StopsTheOrder_KK2()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;

        // The firm stops delivering to Ankara after the summary was shown (02 3.17.2, 6.2.3).
        await ExecuteAsync("DELETE FROM company_delivery_provinces");
        OrderConfirmationResult stopped = await ConfirmAsync(input, seen);

        Assert.Equal(OrderConfirmationRefusal.SummaryChanged, stopped.Refusal);
        Assert.Equal([SummaryChangeKind.DeliveryArea], [.. stopped.Changes.Select(c => c.Kind)]);
        Assert.Equal(seen.Total, stopped.CurrentSummary!.Total);
        await AssertNothingBornAsync(variantId);

        // Ankara is served again, but a physical order arrives with no shipping address.
        await SaveAsync(c => c.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(_provinceId, Now)));
        OrderSummaryInput noShipping = input with { Shipping = null };
        OrderConfirmationResult unaddressed = await ConfirmAsync(noShipping, (await SummarizeAsync(noShipping))!);

        Assert.Equal(OrderConfirmationRefusal.SummaryChanged, unaddressed.Refusal);
        Assert.Contains(unaddressed.Changes, c => c.Kind == SummaryChangeKind.DeliveryArea);
        await AssertNothingBornAsync(variantId);

        // With the province served and the address given, the same confirmation is born.
        Assert.False((await ConfirmAsync(input, (await SummarizeAsync(input))!)).IsRejected);
    }

    [Fact]
    public async Task TheDeliveryArea_RestrictsTheShippingProvinceOnly_ADigitalOrderWithNoShippingAddressIsBorn_WhenNoProvinceIsServed_KK2()
    {
        Guid template = await SeedVariantAsync("Şablon", 80m, stock: null, ProductType.Digital);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (template, 1));
        await ExecuteAsync("DELETE FROM company_delivery_provinces");
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null) with { Shipping = null };

        OrderConfirmationResult result = await ConfirmAsync(input, (await SummarizeAsync(input))!, new TickedBoxes(true, true, true, false));

        Assert.False(result.IsRejected);
        Assert.Null((await ReadOrderAsync(result.OrderId!.Value)).ShippingProvinceId);
    }

    [Fact]
    public async Task AShippingFeeOrAMinimumAmountSetAfterTheSummary_StopsTheOrder_KK2()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;

        // A shipping fee the summary did not carry (02 3.17.2: "kargo ücretinin ... değişmesi").
        await ExecuteAsync("UPDATE company_settings SET shipping_fee = 50");
        OrderConfirmationResult feeChanged = await ConfirmAsync(input, seen);
        Assert.Equal(OrderConfirmationRefusal.SummaryChanged, feeChanged.Refusal);
        Assert.Contains(new SummaryChange(SummaryChangeKind.ShippingFee, null, 0m, 50m), feeChanged.Changes);

        // A minimum order amount the cart no longer meets (02 3.18.1; T349 ShippingCharge.Assess through the cart's read): the total is no longer payable.
        await ExecuteAsync("UPDATE company_settings SET shipping_fee = 0, minimum_order_amount = 2000");
        OrderConfirmationResult belowMinimum = await ConfirmAsync(input, seen);
        Assert.Equal(OrderConfirmationRefusal.SummaryChanged, belowMinimum.Refusal);
        Assert.Contains(belowMinimum.Changes, c => c.Kind == SummaryChangeKind.Total);
        await AssertNothingBornAsync(variantId);
    }

    [Fact]
    public async Task AShippingPeriodChange_AfterTheSummary_StopsTheOrder_AndTellsBothPeriods_KK2_K4()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;
        Assert.Equal(2, seen.ShippingPeriod);

        // The firm's default period rises after the summary was shown: the form would freeze a promise the customer did not see (02 3.24.3).
        await ExecuteAsync("UPDATE company_settings SET default_shipping_period = 4");
        OrderConfirmationResult result = await ConfirmAsync(input, seen);

        Assert.Equal(OrderConfirmationRefusal.SummaryChanged, result.Refusal);
        Assert.Equal([new SummaryChange(SummaryChangeKind.ShippingPeriod, null, 2, 4)], result.Changes);
        Assert.Equal(4, result.CurrentSummary!.ShippingPeriod);
        await AssertNothingBornAsync(variantId);
    }

    // ---- KK5, KK6: the expired order is cancelled with its reservation freed; the retry is a new order with a new number ----

    [Fact]
    public async Task ExpiredOrder_FreesItsReservation_AndTheRetryIsANewOrderWithANewNumber_KK5_KK6()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        Guid couponId = await SeedCouponAsync("T348R", 10m);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 2));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, "T348R");
        OrderConfirmationResult first = await ConfirmAsync(input, (await SummarizeAsync(input))!);

        await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CancelOrderAsync(CancelOrderRequest.BySystem(first.OrderId!.Value, TransitionReason.CardPaymentFailed), Ct));
        Order cancelled = await ReadOrderAsync(first.OrderId!.Value);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed), (cancelled.OrderStatus, cancelled.PaymentStatus));
        Assert.Equal(0, await ScalarAsync($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
        Assert.Equal(0, await ScalarAsync($"SELECT reserved_count FROM coupons WHERE id = '{couponId}'"));

        OrderConfirmationResult retry = await ConfirmAsync(input, (await SummarizeAsync(input))!);

        Assert.False(retry.IsRejected);
        Assert.NotEqual(first.OrderId, retry.OrderId);
        Assert.NotEqual(first.OrderNumber, retry.OrderNumber);
        Assert.Equal(OrderStatus.Cancelled, (await ReadOrderAsync(first.OrderId!.Value)).OrderStatus);
        Assert.Equal(2, await ScalarAsync($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
    }

    // ---- KK7: a new confirmation from the same cart cancels the earlier unpaid order; the merged guest cart's order stays ----

    [Fact]
    public async Task ANewConfirmationFromTheSameCart_CancelsTheEarlierUnpaidOrder_AndIsNotStoppedByItsReservation_KK7()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 2);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 2));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;
        OrderConfirmationResult first = await ConfirmAsync(input, seen);
        Assert.False(first.IsRejected);

        // The whole stock is the earlier order's reservation; the same cart's new confirmation is not stopped by it.
        OrderConfirmationResult second = await ConfirmAsync(input, seen);

        Assert.False(second.IsRejected);
        Order earlier = await ReadOrderAsync(first.OrderId!.Value);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed), (earlier.OrderStatus, earlier.PaymentStatus));
        Assert.Contains(await ReadRecordsAsync(earlier.Id), r => r.TransitionCode == TransitionCode.S3 && r.TransitionReason == TransitionReason.SameCartReorder);
        Assert.Equal(0, await ScalarAsync($"SELECT count(*) FROM stock_reservations WHERE order_id = '{earlier.Id}'"));
        Assert.Equal(2, await ScalarAsync($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
    }

    [Fact]
    public async Task TheMergedGuestCartsOrder_IsNotCancelledByTheAccountCartsConfirmation_KK7()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        string guestKey = "guest-" + Guid.NewGuid().ToString("N");
        CartOwner guest = CartOwner.ForGuest(guestKey);
        await SeedCartAsync(guest, (variantId, 1));
        OrderSummaryInput guestInput = Input(guest, PaymentMethod.Card, null);
        OrderConfirmationResult guestOrder = await ConfirmAsync(guestInput, (await SummarizeAsync(guestInput))!);

        // The sign-in merges the guest cart into the account's and deletes it (02 3.16.3); the guest order keeps the deleted cart's id.
        Guid customerId = await SeedCustomerAsync();
        await RunAsync(sp => sp.GetRequiredService<IOrderingModule>().MergeGuestCartAsync(customerId, guestKey, Ct));
        CartOwner member = CartOwner.ForMember(customerId);
        OrderSummaryInput memberInput = Input(member, PaymentMethod.Card, null);
        OrderConfirmationResult memberOrder = await ConfirmAsync(memberInput, (await SummarizeAsync(memberInput))!);

        Assert.False(memberOrder.IsRejected);
        Order kept = await ReadOrderAsync(guestOrder.OrderId!.Value);
        Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending), (kept.OrderStatus, kept.PaymentStatus));
        Assert.Equal(2, await ScalarAsync($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
    }

    // ---- KK8: the never-paid order the system cancelled is not in the customer's list; the panel shows it ----

    [Fact]
    public async Task TheNeverPaidOrderTheSystemCancelled_LeavesTheCustomersList_AndStaysInThePanel_KK8()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;
        OrderConfirmationResult first = await ConfirmAsync(input, seen);
        OrderConfirmationResult second = await ConfirmAsync(input, seen);

        await using AppDbContext reader = NewContext();
        List<Guid> shown = await OrderVisibility.ShownToCustomer(reader.Orders, reader.TransitionRecords).Select(o => o.Id).ToListAsync(Ct);
        List<Guid> panel = await reader.Orders.Select(o => o.Id).ToListAsync(Ct);

        Assert.DoesNotContain(first.OrderId!.Value, shown);
        Assert.Contains(second.OrderId!.Value, shown);
        Assert.Contains(first.OrderId!.Value, panel);
    }

    // ---- T352: the open payment methods; the cart stays when the payment is left half way or the provider is out ----

    [Fact]
    public async Task KeylessInstallation_OffersOnlyTheTransfer_RefusesTheCard_AndOpensTheGateWithTheTransfer_T352_KK2_KK4()
    {
        _cardLineOpen = false;
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        Guid cartId = await SeedCartAsync(owner, (variantId, 1));

        IReadOnlyList<PaymentMethod> open = await RunAsync(sp => sp.GetRequiredService<IPaymentMethods>().ListOpenAsync(Ct));
        SalesGate gate = await RunAsync(sp => sp.GetRequiredService<ISettingsModule>().GetSalesGateAsync(Ct));
        OrderSummaryInput byCard = Input(owner, PaymentMethod.Card, null);
        OrderConfirmationResult card = await ConfirmAsync(byCard, (await SummarizeAsync(byCard))!);

        Assert.Equal([PaymentMethod.BankTransfer], open);
        Assert.DoesNotContain(SalesGateCondition.PaymentMethod, gate.UnmetConditions);
        Assert.Equal(OrderConfirmationRefusal.PaymentMethodUnavailable, card.Refusal);
        await AssertNothingBornAsync(variantId);
        Assert.Equal(1, await ScalarAsync($"SELECT count(*) FROM cart_items WHERE cart_id = '{cartId}'"));

        OrderSummaryInput byTransfer = Input(owner, PaymentMethod.BankTransfer, null);
        OrderConfirmationResult transfer = await ConfirmAsync(byTransfer, (await SummarizeAsync(byTransfer))!);
        Assert.False(transfer.IsRejected);
        Assert.Equal(PaymentMethod.BankTransfer, (await ReadOrderAsync(transfer.OrderId!.Value)).PaymentMethod);
    }

    [Fact]
    public async Task NoOpenMethod_ClosesTheGate_AndASwitchedOffTransferIsRefusedWhileTheCardIsTaken_T352_KK2_KK4()
    {
        await ExecuteAsync("UPDATE company_settings SET is_bank_transfer_enabled = false");
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        await SeedCartAsync(owner, (variantId, 1));

        IReadOnlyList<PaymentMethod> cardOnly = await RunAsync(sp => sp.GetRequiredService<IPaymentMethods>().ListOpenAsync(Ct));
        OrderSummaryInput byTransfer = Input(owner, PaymentMethod.BankTransfer, null);
        OrderConfirmationResult transfer = await ConfirmAsync(byTransfer, (await SummarizeAsync(byTransfer))!);
        Assert.Equal([PaymentMethod.Card], cardOnly);
        Assert.Equal(OrderConfirmationRefusal.PaymentMethodUnavailable, transfer.Refusal);
        await AssertNothingBornAsync(variantId);

        _cardLineOpen = false;
        IReadOnlyList<PaymentMethod> none = await RunAsync(sp => sp.GetRequiredService<IPaymentMethods>().ListOpenAsync(Ct));
        SalesGate gate = await RunAsync(sp => sp.GetRequiredService<ISettingsModule>().GetSalesGateAsync(Ct));
        Assert.Empty(none);
        Assert.Contains(SalesGateCondition.PaymentMethod, gate.UnmetConditions);

        _cardLineOpen = true;
        OrderSummaryInput byCard = Input(owner, PaymentMethod.Card, null);
        Assert.False((await ConfirmAsync(byCard, (await SummarizeAsync(byCard))!)).IsRejected);
    }

    [Theory]
    [InlineData(TransitionReason.CardPaymentFailed)]
    [InlineData(TransitionReason.PaymentPageNotOpened)]
    [InlineData(TransitionReason.PaymentWindowExpired)]
    public async Task UnpaidCardOrderCancelledBySystem_LeavesTheCartAsItIs_T352_KK6_KK8(TransitionReason reason)
    {
        Guid variantId = await SeedVariantAsync("Lamba", 1000m, stock: 5);
        CartOwner owner = CartOwner.ForMember(await SeedCustomerAsync());
        Guid cartId = await SeedCartAsync(owner, (variantId, 2));
        OrderSummaryInput input = Input(owner, PaymentMethod.Card, null);
        OrderConfirmationResult placed = await ConfirmAsync(input, (await SummarizeAsync(input))!);
        Guid orderId = placed.OrderId!.Value;

        // Left half way: the order waits for its payment, the cart is untouched and the order is not in preparation.
        Order waiting = await ReadOrderAsync(orderId);
        Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending), (waiting.OrderStatus, waiting.PaymentStatus));
        Assert.Equal(2, await ScalarAsync($"SELECT quantity FROM cart_items WHERE cart_id = '{cartId}'"));

        // A failed 3D check, the provider's page that never opened (the provider out) or the window's end: cancelled, the cart stays (02 6.1.4, 6.2.1, 6.2.2, 6.2.15).
        await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CancelOrderAsync(CancelOrderRequest.BySystem(orderId, reason), Ct));

        Order cancelled = await ReadOrderAsync(orderId);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed), (cancelled.OrderStatus, cancelled.PaymentStatus));
        Assert.Equal(2, await ScalarAsync($"SELECT quantity FROM cart_items WHERE cart_id = '{cartId}'"));
        Assert.Equal(0, await ScalarAsync($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
    }

    // ---- T377: the confirmation's one gate for L-7 and L-8 (02 8.2.2, 8.2.3) ----

    [Fact]
    public async Task AFullOpenCeiling_StopsTheNextBankTransfer_ButTheCardIsAccepted_T377_KK5()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 100m, stock: 50);
        for (int i = 0; i < ProductConstants.OpenUnpaidOrdersMax; i++)
        {
            CartOwner guest = CartOwner.ForGuest("guest-" + Guid.NewGuid().ToString("N"));
            await SeedCartAsync(guest, (variantId, 1));
            OrderSummaryInput open = Input(guest, PaymentMethod.BankTransfer, null);
            Assert.False((await ConfirmAsync(open, (await SummarizeAsync(open))!)).IsRejected);
        }

        CartOwner next = CartOwner.ForGuest("guest-" + Guid.NewGuid().ToString("N"));
        await SeedCartAsync(next, (variantId, 1));
        OrderSummaryInput byTransfer = Input(next, PaymentMethod.BankTransfer, null);
        OrderConfirmationResult stopped = await ConfirmAsync(byTransfer, (await SummarizeAsync(byTransfer))!);

        Assert.True(stopped.IsRejected);
        Assert.Equal(OrderConfirmationRefusal.AttemptLimitExceeded, stopped.Refusal);
        Assert.Equal(AttemptLimit.L8, stopped.ExceededLimit);
        Assert.Equal(ProductConstants.OpenUnpaidOrdersMax, await ScalarAsync("SELECT count(*) FROM orders"));

        OrderSummaryInput byCard = Input(next, PaymentMethod.Card, null);
        Assert.False((await ConfirmAsync(byCard, (await SummarizeAsync(byCard))!)).IsRejected);
    }

    [Fact]
    public async Task ARepeatedConfirmationFromTheSameCart_LowersL8_AndNeverFillsL7_T377_KK4()
    {
        Guid variantId = await SeedVariantAsync("Lamba", 100m, stock: 50);
        CartOwner owner = CartOwner.ForGuest("guest-" + Guid.NewGuid().ToString("N"));
        await SeedCartAsync(owner, (variantId, 1));
        OrderSummaryInput input = Input(owner, PaymentMethod.BankTransfer, null);
        ConfirmationSummary seen = (await SummarizeAsync(input))!;

        // More repeats than both limits allow: each one cancels the earlier unpaid order of the same cart, so the open count stays one and L-7 counts none of them.
        for (int i = 0; i < UnpaidCancellationLimit.Max + 1; i++)
        {
            Assert.False((await ConfirmAsync(input, seen)).IsRejected);
        }

        await using (AppDbContext reader = NewContext())
        {
            Assert.Equal(1, await reader.Orders.CountAsync(o => o.OrderStatus == OrderStatus.Placed, Ct));
        }

        Assert.Equal(0, await RunAsync(sp => sp.GetRequiredService<UnpaidCancellationLimit>().CountByIpAsync(IPAddress.Parse("203.0.113.5"), Ct)));
    }

    [Theory]
    [InlineData(PaymentMethod.Card)]
    [InlineData(PaymentMethod.BankTransfer)]
    public async Task AFullL7_StopsTheNextConfirmationWhateverTheMethod_T377_KK4(PaymentMethod method)
    {
        Guid variantId = await SeedVariantAsync("Lamba", 100m, stock: 50);
        TransitionReason[] reasons = [TransitionReason.PaymentWindowExpired, TransitionReason.CardPaymentFailed];
        for (int i = 0; i < UnpaidCancellationLimit.Max; i++)
        {
            CartOwner guest = CartOwner.ForGuest("guest-" + Guid.NewGuid().ToString("N"));
            await SeedCartAsync(guest, (variantId, 1));
            OrderSummaryInput done = Input(guest, PaymentMethod.Card, null);
            OrderConfirmationResult placed = await ConfirmAsync(done, (await SummarizeAsync(done))!);
            Assert.False(placed.IsRejected);
            await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().CancelOrderAsync(CancelOrderRequest.BySystem(placed.OrderId!.Value, reasons[i % 2]), Ct));
        }

        CartOwner next = CartOwner.ForGuest("guest-" + Guid.NewGuid().ToString("N"));
        await SeedCartAsync(next, (variantId, 1));
        OrderSummaryInput input = Input(next, method, null);
        OrderConfirmationResult stopped = await ConfirmAsync(input, (await SummarizeAsync(input))!);

        Assert.True(stopped.IsRejected);
        Assert.Equal(OrderConfirmationRefusal.AttemptLimitExceeded, stopped.Refusal);
        Assert.Equal(AttemptLimit.L7, stopped.ExceededLimit);
    }

    // ---- helpers ----

    private OrderSummaryInput Input(CartOwner owner, PaymentMethod method, string? coupon) =>
        new(owner, method, coupon, "Ali@Example.com", new PartyAddress("Ali Veli", "5321234567", _provinceId, "Çankaya", "Atatürk Bulvarı 1"), new PartyAddress("Ali Veli", null, _provinceId, "Çankaya", "Atatürk Bulvarı 1"));

    private async Task AssertNothingBornAsync(Guid variantId)
    {
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM orders"));
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM stock_reservations"));
        Assert.Equal(0, await ScalarAsync("SELECT count(*) FROM dispatch_records"));
        Assert.Equal(0, await ScalarAsync($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
    }

    private Task<ConfirmationSummary?> SummarizeAsync(OrderSummaryInput input) =>
        RunAsync(sp => sp.GetRequiredService<OrderConfirmation>().SummarizeAsync(input, Ct));

    private async Task<OrderConfirmationResult> ConfirmAsync(OrderSummaryInput input, ConfirmationSummary seen, TickedBoxes? ticked = null)
    {
        OrderConfirmationRequest request = new(
            input.Owner, seen, _formId, _contractId, ticked ?? new TickedBoxes(true, true, false, false), input.CouponCode, input.ContactEmail, input.Shipping, input.Billing, IPAddress.Parse("203.0.113.5"));
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn, _clock, _cardLineOpen);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        OrderConfirmation confirmation = scope.ServiceProvider.GetRequiredService<OrderConfirmation>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => confirmation.ConfirmAsync(request, token), Ct);
    }

    // One request: its own scope and transaction; the owner of the transaction saves (09 6 item 3).
    private async Task<T> RunAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn, _clock, _cardLineOpen);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        Boxed<T> boxed = await services.GetRequiredService<ITransactionRunner>().RunAsync(async _ => new Boxed<T>(await work(services)), Ct);
        return boxed.Value;
    }

    private sealed record Boxed<T>(T Value) : IOperationResult
    {
        public bool IsRejected => false;
    }

    private async Task<Guid> SeedVariantAsync(string name, decimal price, int? stock, ProductType type = ProductType.Physical)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create(name, $"urun-t348-{n:D4}", type, 20m, Now.AddDays(-2));
        if (type == ProductType.Service)
        {
            // A service has no place of origin and is published with a fulfilment period (P-44).
            product.FulfilmentPeriod = 3;
        }
        else
        {
            product = product.WithSeededOrigin();
        }

        product.PublishStatus = PublishStatus.Published;
        Variant variant = Variant.Create(product.Id, $"SKU-T348-{n:D4}", price, Now.AddDays(-2));
        variant.PublishStatus = PublishStatus.Published;
        variant.StockQuantity = stock;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return variant.Id;
    }

    private async Task<Guid> SeedCouponAsync(string code, decimal percentage)
    {
        Coupon coupon = Coupon.CreatePercentage(code, percentage, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 10, Now.AddDays(-2));
        await SaveAsync(c => c.Coupons.Add(coupon));
        return coupon.Id;
    }

    private async Task<Guid> SeedCustomerAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        Customer customer = Customer.Create($"Ali {n}", $"ali-{n}-{Guid.NewGuid():N}@example.com", "digest-placeholder", Now, null, Now);
        await SaveAsync(c => c.Customers.Add(customer));
        return customer.Id;
    }

    private async Task<Guid> SeedCartAsync(CartOwner owner, params (Guid VariantId, int Quantity)[] items)
    {
        Cart cart = owner.CustomerId is Guid id ? Cart.ForCustomer(id, Now) : Cart.ForGuest(Shopfolio.Infrastructure.Security.SecretKey.Hash(owner.GuestCartKey!), Now);
        Dictionary<Guid, decimal> prices;
        await using (AppDbContext reader = NewContext())
        {
            Guid[] ids = [.. items.Select(i => i.VariantId)];
            prices = await reader.Variants.Where(v => ids.Contains(v.Id)).ToDictionaryAsync(v => v.Id, v => v.Price, Ct);
        }

        await SaveAsync(c =>
        {
            c.Carts.Add(cart);
            foreach ((Guid variantId, int quantity) in items)
            {
                c.CartItems.Add(CartItem.Create(cart.Id, variantId, quantity, prices[variantId], Now));
            }
        });
        return cart.Id;
    }

    private async Task<Order> ReadOrderAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
    }

    private async Task<List<OrderItem>> ReadItemsAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).ToListAsync(Ct);
    }

    private async Task<List<TransitionRecord>> ReadRecordsAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.TransitionRecords.AsNoTracking().Where(r => r.OrderId == orderId).ToListAsync(Ct);
    }

    private async Task<List<DispatchRecord>> ReadDispatchesAsync()
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.AsNoTracking().ToListAsync(Ct);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task SaveAsync(Action<AppDbContext> change)
    {
        await using AppDbContext context = NewContext();
        change(context);
        await context.SaveChangesAsync(Ct);
    }

    private async Task<long> ScalarAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return Convert.ToInt64(await command.ExecuteScalarAsync(Ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }
}
