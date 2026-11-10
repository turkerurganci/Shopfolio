using System.Net;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.SignedEndpoints;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Lists;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>The collection of the first end-to-end flow test (T329): one database copied from the template.</summary>
[CollectionDefinition(MainFlowDefinition.Name)]
public sealed class MainFlowDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "MainFlow";
}

/// <summary>
/// The main flow of 02 2.2 - the eight steps of 03 2.10.1 (flow 1 + 6) - in one test, from the service layer, on a real PostgreSQL (T329 KK1-KK7): a physical product's variant is found
/// by the search and its page, added to a member's cart, the payment step takes the addresses, a coupon and the card, the confirmation gives birth to the order, the hand-written
/// <see cref="FakePaymentGateway"/> verifies the provider's success notification, which the application's own endpoint records and the job runner processes, and the administrator ships
/// the order and marks it delivered. After every step the two axes, the side effects and the trail rows are what the documents say. Only the main flow is here: the mixed order,
/// the withdrawal during shipment, the refund without an IBAN and the e-mail correction are T390's (K-1441).
/// </summary>
[Collection(MainFlowDefinition.Name)]
public sealed class MainFlowTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);
    private static readonly ShipmentInfo Shipment = new("YurticiKargo", null, "TR-3290001", false);

    private const string ContactEmail = "Ali@Example.com";
    private const string CouponCode = "T329";
    private const string ProviderTransactionId = "T329-TX-0001";
    private const int StockQuantity = 5;
    private const int Quantity = 2;
    private const decimal UnitPrice = 1000m;
    private const decimal CouponPercentage = 10m;

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Start);
    private readonly FakePaymentGateway _gateway = new();
    private WebApplication? _app;
    private HttpClient? _client;

    private Guid _provinceId;
    private Guid _formId;
    private Guid _contractId;
    private Guid _variantId;
    private Guid _otherVariantId;
    private Guid _couponId;
    private Guid _customerId;
    private TransitionAdmin? _admin;

    public MainFlowTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE inbound_notification_records, jobs, payment_records, dispatch_records, stock_reservations, transition_records, cart_items, carts, order_items, orders, "
            + "coupons, option_values, options, variants, products, customers, admins, legal_text_versions, companies, company_settings, provinces CASCADE");
        await SeedStoreAsync();
        await StartHostAsync();
    }

    public async ValueTask DisposeAsync()
    {
        _client?.Dispose();
        if (_app is not null)
        {
            await _app.StopAsync(Ct);
            await _app.DisposeAsync();
        }
    }

    [Fact]
    public async Task ThePhysicalCardOrder_GoesTheEightStepsOfTheMainFlow_WithTheFakePayment_KK1_To_KK7()
    {
        CartOwner owner = CartOwner.ForMember(_customerId);

        // ---- Step 1 (03 2.10.1.1): the visitor finds the product by a word of its name and chooses the variant on its page ----
        ProductListPage found = (await RunAsync(sp => sp.GetRequiredService<ICatalogModule>().SearchProductsAsync("lamba", null, 1, Ct)))!;
        ProductListItem listed = Assert.Single(found.Items);
        Assert.True(listed.IsPurchasable);
        ProductPage page = await RunAsync(sp => sp.GetRequiredService<ICatalogModule>().GetProductPageAsync(listed.Slug, false, Ct));
        Assert.True(page.OffersCart);
        Assert.Equal(2, page.Variants.Count);
        ProductPageVariant chosen = page.Variants.Single(v => v.Sku == "SKU-T329-BEYAZ");
        Assert.Equal((_variantId, UnitPrice, false), (chosen.Id, chosen.Price, chosen.IsSoldOut));

        // ---- Step 2 (03 2.10.1.2): the customer adds the variant with its quantity; the cart is live and no stock is set aside (03 2.3.1) ----
        CartAddResult added = await RunAsync(sp => sp.GetRequiredService<IOrderingModule>().AddToCartAsync(owner, chosen.Id, Quantity, Ct));
        Assert.Equal(CartAddOutcome.Added, added.Outcome);
        CartView cart = await RunAsync(sp => sp.GetRequiredService<IOrderingModule>().OpenCartAsync(owner, new ShippingFeeSection(0m, null, null, 1), Ct));
        CartLine line = Assert.Single(cart.Lines);
        Assert.Equal((_variantId, Quantity), (line.VariantId, line.Quantity));
        Assert.True(cart.CanOpenPayment);
        await AssertStockAsync(_variantId, stock: StockQuantity, reserved: 0);
        Assert.Equal(0, await CountAsync("stock_reservations"));
        Assert.Equal(0, await CountAsync("orders"));
        Assert.Equal(0, await CountAsync("transition_records"));

        // ---- Step 3 (03 2.10.1.3): the addresses, the coupon and the payment method are entered at the payment step ----
        OrderSummaryInput input = new(
            owner,
            PaymentMethod.Card,
            CouponCode,
            ContactEmail,
            new PartyAddress("Ali Veli", "5321234567", _provinceId, "Çankaya", "Atatürk Bulvarı 1"),
            new PartyAddress("Ali Veli", null, _provinceId, "Çankaya", "Atatürk Bulvarı 1"));
        ConfirmationSummary summary = (await RunAsync(sp => sp.GetRequiredService<OrderConfirmation>().SummarizeAsync(input, Ct)))!;
        decimal expectedTotal = Quantity * UnitPrice * (100m - CouponPercentage) / 100m;
        Assert.Equal(expectedTotal, summary.Total);

        // ---- Step 4 (03 2.10.1.4): the summary, the form and the withdrawal summary above the two boxes; the two boxes are ticked ----
        ConfirmationStepView view = await RunAsync(sp => sp.GetRequiredService<IConfirmationStep>().GetViewAsync(summary, Ct));
        Assert.Equal(2, view.Boxes.Count);
        Assert.Equal((_formId, _contractId), (view.ShownPreInformationFormId, view.ShownDistanceSalesContractId));
        Assert.False(string.IsNullOrWhiteSpace(view.WithdrawalSummaryText));
        Assert.Equal(0, await CountAsync("orders"));

        // ---- Step 5 (03 2.10.1.5): the order is born - number, frozen items, addresses and versions, stock and coupon right reserved; Placed + Pending; B-1 and F-1 ----
        OrderConfirmationResult confirmed = await RunAsync(sp => sp.GetRequiredService<OrderConfirmation>().ConfirmAsync(
            new OrderConfirmationRequest(
                owner, summary, view.ShownPreInformationFormId, view.ShownDistanceSalesContractId, new TickedBoxes(true, true, false, false), CouponCode, ContactEmail,
                input.Shipping, input.Billing, IPAddress.Parse("203.0.113.5")),
            Ct));
        Assert.False(confirmed.IsRejected);
        Guid orderId = confirmed.OrderId!.Value;
        Order placed = await ReadOrderAsync(orderId);
        Assert.Equal(confirmed.OrderNumber, placed.OrderNumber);
        Assert.Matches("^2026-", placed.OrderNumber);
        await AssertAxesAsync(orderId, OrderStatus.Placed, PaymentStatus.Pending, version: 1);
        Assert.Equal((expectedTotal, (Guid?)_couponId, (Guid?)_customerId), (placed.Total, placed.CouponId, placed.CustomerId));
        Assert.Equal(("Ali Veli", (Guid?)_provinceId, "Atatürk Bulvarı 1"), (placed.ShippingRecipientName, placed.ShippingProvinceId, placed.ShippingAddressLine));
        Assert.Equal(("Ali Veli", (Guid?)_provinceId), (placed.BillingRecipientName, placed.BillingProvinceId));
        Assert.Equal((_formId, _contractId), (placed.PreInformationFormVersionId, placed.DistanceSalesContractVersionId));
        Assert.Equal(Start + TimeSpan.FromMinutes(ProductConstants.CardPaymentMinutes), placed.PaymentWindowEndsAt);
        Assert.Equal(0, await CountAsync($"transition_records WHERE order_id = '{orderId}'"));
        await AssertStockAsync(_variantId, stock: StockQuantity, reserved: Quantity);
        await AssertCouponAsync(used: 0, reserved: 1);
        Assert.Equal(2, await CountAsync($"stock_reservations WHERE order_id = '{orderId}'"));
        List<DispatchRecord> born = await ReadDispatchesAsync();
        Assert.Contains(born, d => d.DispatchType == DispatchType.B1 && d.OrderId == orderId && d.RecipientEmail == ContactEmail);
        Assert.Contains(born, d => d.DispatchType == DispatchType.F1 && d.RecipientEmail == OrderingTestServices.CompanyEmail);
        Assert.Equal(2, born.Count);

        // The item is frozen: a later change of the product's name and the variant's price does not reach it.
        await ExecuteAsync($"UPDATE variants SET price = 1500 WHERE id = '{_variantId}'");
        await ExecuteAsync("UPDATE products SET name = 'Yeni Ad'");
        OrderItem item = Assert.Single(await ReadItemsAsync(orderId));
        Assert.Equal(
            ("Masa Lambası", "Beyaz", ProductType.Physical, Quantity, UnitPrice, (Guid?)_variantId),
            (item.ProductName, item.FirstOptionValue, item.ProductType, item.Quantity, item.UnitPrice, item.VariantId));

        // ---- Step 6 (03 2.10.1.6): the fake provider's success notification is recorded at the endpoint and processed in the job: Ö1 + S1, B-4 ----
        _clock.Advance(TimeSpan.FromMinutes(3));
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(new PaymentNotice(
            ProviderTransactionId, placed.OrderNumber, FakePaymentGateway.PaymentEventType, FakePaymentGateway.SucceededOutcome, expectedTotal, expectedTotal, true, true, false)));
        using (HttpResponseMessage response = await _client!.PostAsync(
            new Uri(AntiforgeryExemptions.PaymentNotificationPath, UriKind.Relative), new StringContent("field=value", Encoding.UTF8, "application/x-www-form-urlencoded"), Ct))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(PaymentNotificationEndpoint.ReceivedBody, await response.Content.ReadAsStringAsync(Ct));
        }

        // The endpoint only records: the order has not moved until the job runs (T311 KK7).
        InboundNotificationRecord inbound = await SingleInboundAsync();
        Assert.Equal(((Guid?)orderId, (DateTimeOffset?)null, (UnprocessedNotificationReason?)null), (inbound.OrderId, inbound.ProcessedAt, inbound.UnprocessedReason));
        await AssertAxesAsync(orderId, OrderStatus.Placed, PaymentStatus.Pending, version: 1);

        await RunDueJobsAsync();
        Assert.Equal(nameof(JobState.Completed), await TextAsync($"SELECT job_state FROM jobs WHERE related_record_id = '{inbound.Id}'"));
        Assert.NotNull((await SingleInboundAsync()).ProcessedAt);
        await AssertAxesAsync(orderId, OrderStatus.Preparing, PaymentStatus.Paid, version: 2);
        List<TransitionRecord> paidTrail = await ReadTrailAsync(orderId);
        Assert.Equal(new[] { TransitionCode.O1, TransitionCode.S1 }.Order(), paidTrail.Select(r => r.TransitionCode).Order());
        Assert.All(paidTrail, r => Assert.Equal((ActorKind.System, _clock.GetUtcNow()), (r.ActorKind, r.CreatedAt)));
        PaymentRecord payment;
        await using (AppDbContext reader = NewContext())
        {
            payment = await reader.PaymentRecords.AsNoTracking().SingleAsync(p => p.OrderId == orderId, Ct);
        }

        Assert.Equal((ProviderTransactionId, (Guid?)paidTrail.Single(r => r.TransitionCode == TransitionCode.O1).Id), (payment.ProviderTransactionId, payment.TransitionRecordId));

        // The reserved stock falls finally, the reserved coupon right becomes a used one, the ordered line leaves the cart, and the shipping period starts (02 2.2 step 6).
        await AssertStockAsync(_variantId, stock: StockQuantity - Quantity, reserved: 0);
        await AssertStockAsync(_otherVariantId, stock: StockQuantity, reserved: 0);
        await AssertCouponAsync(used: 1, reserved: 0);
        Assert.Equal(0, await CountAsync($"stock_reservations WHERE order_id = '{orderId}'"));
        Assert.Equal(0, await CountAsync($"cart_items WHERE variant_id = '{_variantId}'"));
        OrderShippingView shipping = (await RunAsync(sp => sp.GetRequiredService<IOrderingModule>().GetShippingViewAsync(orderId, Ct)))!;
        Assert.Equal(_clock.GetUtcNow(), shipping.PaymentConfirmedAt);
        Assert.NotNull(shipping.PromiseEndsAt);
        List<DispatchRecord> paid = await ReadDispatchesAsync();
        Assert.Single(paid, d => d.DispatchType == DispatchType.B4 && d.OrderId == orderId && d.RecipientEmail == ContactEmail);
        Assert.Equal(born.Count + 1, paid.Count);

        // ---- Step 7 (03 2.10.1.7): the administrator ships the order with the carrier and the tracking number: S5, B-5 ----
        _clock.Advance(TimeSpan.FromDays(1));
        OrderTransitionResult shipped = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().ShipOrderAsync(new ShipOrderRequest(orderId, _admin!, 2, true, Shipment), Ct));
        Assert.False(shipped.IsRejected);
        await AssertAxesAsync(orderId, OrderStatus.Shipped, PaymentStatus.Paid, version: 3);
        Order inTransit = await ReadOrderAsync(orderId);
        Assert.Equal((Shipment.Carrier, Shipment.TrackingNumber), (inTransit.Carrier, inTransit.TrackingNumber));
        TransitionRecord shipRecord = (await ReadTrailAsync(orderId)).Single(r => r.TransitionCode == TransitionCode.S5);
        Assert.Equal((ActorKind.Admin, (Guid?)_admin!.AdminId, _admin.AdminName), (shipRecord.ActorKind, shipRecord.AdminId, shipRecord.AdminName));
        List<DispatchRecord> afterShipment = await ReadDispatchesAsync();
        Assert.Single(afterShipment, d => d.DispatchType == DispatchType.B5 && d.OrderId == orderId && d.RecipientEmail == ContactEmail);
        Assert.Equal(paid.Count + 1, afterShipment.Count);

        // ---- Step 8 (03 2.10.1.8): the goods arrive; the administrator enters the delivery mark with a past date: S6, no notification (02 9.2) ----
        _clock.Advance(TimeSpan.FromDays(2));
        DateOnly deliveryDay = DateOnly.FromDateTime(_clock.GetUtcNow().UtcDateTime).AddDays(-1);
        OrderTransitionResult delivered = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().MarkDeliveredAsync(new MarkDeliveredRequest(orderId, _admin!, 3, deliveryDay), Ct));
        Assert.False(delivered.IsRejected);
        await AssertAxesAsync(orderId, OrderStatus.Delivered, PaymentStatus.Paid, version: 4);
        OrderItem deliveredItem = Assert.Single(await ReadItemsAsync(orderId));
        Assert.Equal(((DateOnly?)deliveryDay, (DateTimeOffset?)_clock.GetUtcNow()), (deliveredItem.DeliveryDate, deliveredItem.DeliveryMarkedAt));
        Assert.Equal(afterShipment.Count, (await ReadDispatchesAsync()).Count);

        // ---- The trail of the whole flow (03 1.5.1): Ö1 + S1 by the system, then S5 and S6 by the administrator - the card line's two mandatory manual steps (03 8.5.1) ----
        List<TransitionRecord> trail = await ReadTrailAsync(orderId);
        OrderLineRow cardLine = OrderLines.Rows.Single(r => r.Line == OrderLine.PhysicalCard);
        Assert.Equal(OrderLine.PhysicalCard, OrderLines.LineOf([ProductType.Physical], PaymentMethod.Card, physicalClosed: false));
        Assert.Equal(cardLine.Sequence.SelectMany(s => s.Codes).Order(), trail.Select(r => r.TransitionCode).Order());
        TransitionCode[] adminCodes = [.. trail.Where(r => r.ActorKind == ActorKind.Admin).OrderBy(r => r.CreatedAt).Select(r => r.TransitionCode)];
        Assert.Equal(cardLine.Sequence.Where(s => s.Actor == ActorKind.Admin).SelectMany(s => s.Codes), adminCodes);
        Assert.Equal(OrderLines.StepsOf([ProductType.Physical], PaymentMethod.Card).Count, adminCodes.Length);
        Assert.Equal(2, adminCodes.Length);
    }

    // ---- the host: the shared notification server (D-828) on the manual clock with the fake gateway ----

    private async Task StartHostAsync()
    {
        // The shared notification server (D-828); the card line is open (the provider's gateway is the fake), the transfer line is closed - this flow pays by card.
        _app = await PaymentNotificationHost.StartAsync(_collection.Dsn, _clock, _gateway, Ct, services =>
        {
            services.AddSingleton<IReadOnlyList<ConditionalLine>>([new ConditionalLine(ConditionalLine.CardPayment, [])]);
            services.AddScoped<IBankTransferCondition, ClosedBankTransferCondition>();
        });
        _client = _app.GetTestClient();
    }

    // Runs the job rows that are due on the manual clock, one round at a time, as the runner's loop does; a bound keeps a handler that re-queues itself from looping.
    private async Task RunDueJobsAsync()
    {
        await using NpgsqlDataSource dataSource = NpgsqlDataSource.Create(_collection.Dsn);
        JobRunnerService runner = new(
            dataSource,
            _app!.Services.GetRequiredService<IServiceScopeFactory>(),
            _app.Services.GetRequiredService<JobHandlerRegistry>(),
            [],
            _app.Services.GetRequiredService<IJobSignal>(),
            _clock,
            NullLogger<JobRunnerService>.Instance);
        for (int round = 0; round < 10 && await runner.RunOneAsync(Ct); round++)
        {
        }
    }

    // One request: its own scope and transaction; the owner of the transaction saves (09 6 item 3).
    private async Task<T> RunAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        await using AsyncServiceScope scope = _app!.Services.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        Boxed<T> boxed = await services.GetRequiredService<ITransactionRunner>().RunAsync(async _ => new Boxed<T>(await work(services)), Ct);
        return boxed.Value;
    }

    private sealed record Boxed<T>(T Value) : IOperationResult
    {
        public bool IsRejected => false;
    }

    // ---- the store: the firm, its settings and legal texts, one published physical product with two variants, a coupon, a member and an administrator ----

    private async Task SeedStoreAsync()
    {
        Province province = Province.Create("06", "Ankara", Start);
        CompanySettings settings = CompanySettings.Create(Start);
        settings.BankTransferPaymentWindow = 2;
        settings.DefaultShippingPeriod = 2;
        settings.DownloadAllowance = 1;
        settings.HomepageLayout = Shopfolio.Modules.CorporateContent.Contracts.HomepageLayout.StoreFirst;
        settings.MenuNameProducts = "Ürünler";
        settings.MenuNameOfferings = "Hizmetler";
        settings.MenuNamePortfolioItems = "Çalışmalar";
        settings.MenuNameAboutPage = "Hakkımızda";
        settings.MenuNameFaqItems = "Sıkça Sorulanlar";
        settings.MenuNameContact = "İletişim";
        Company company = Company.Create(Start);
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
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, Start.AddDays(-1), "ön bilgilendirme metni", Start.AddDays(-1));
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, Start.AddDays(-1), "mesafeli satış metni", Start.AddDays(-1));
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, Start.AddDays(-1), "aydınlatma metni", Start.AddDays(-1)).WithHosting();

        Product product = Product.Create("Masa Lambası", "masa-lambasi-t329", ProductType.Physical, 20m, Start.AddDays(-2)).WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        Option color = Option.Create(product.Id, "Renk", 1, Start.AddDays(-2));
        OptionValue white = OptionValue.Create(color.Id, "Beyaz", 1, Start.AddDays(-2));
        OptionValue black = OptionValue.Create(color.Id, "Siyah", 2, Start.AddDays(-2));
        Variant whiteVariant = SeedVariant(product.Id, "SKU-T329-BEYAZ", white.Id);
        Variant blackVariant = SeedVariant(product.Id, "SKU-T329-SIYAH", black.Id);
        Coupon coupon = Coupon.CreatePercentage(CouponCode, CouponPercentage, new DateOnly(2026, 11, 1), new DateOnly(2026, 11, 30), 10, Start.AddDays(-2));
        Customer customer = Customer.Create("Ali Veli", $"ali-{Guid.NewGuid():N}@example.com", "digest-placeholder", Start, null, Start);
        Admin admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);

        await using (AppDbContext context = NewContext())
        {
            context.Provinces.Add(province);
            context.CompanyDeliveryProvinces.Add(CompanyDeliveryProvince.Create(province.Id, Start));
            context.CompanySettings.Add(settings);
            context.Companies.Add(company);
            context.LegalTextVersions.AddRange(form, contract, notice);
            context.Products.Add(product);
            context.Options.Add(color);
            context.OptionValues.AddRange(white, black);
            context.Variants.AddRange(whiteVariant, blackVariant);
            context.Coupons.Add(coupon);
            context.Customers.Add(customer);
            context.Admins.Add(admin);
            await context.SaveChangesAsync(Ct);
        }

        _provinceId = province.Id;
        _formId = form.Id;
        _contractId = contract.Id;
        _variantId = whiteVariant.Id;
        _otherVariantId = blackVariant.Id;
        _couponId = coupon.Id;
        _customerId = customer.Id;
        _admin = new TransitionAdmin(admin.Id, admin.Name);
    }

    private static Variant SeedVariant(Guid productId, string sku, Guid optionValueId)
    {
        Variant variant = Variant.Create(productId, sku, UnitPrice, Start.AddDays(-2));
        variant.PublishStatus = PublishStatus.Published;
        variant.StockQuantity = StockQuantity;
        variant.FirstOptionValueId = optionValueId;
        return variant;
    }

    // ---- reads, each from its own connection after the step ----

    private async Task AssertAxesAsync(Guid orderId, OrderStatus orderStatus, PaymentStatus paymentStatus, int version)
    {
        Order order = await ReadOrderAsync(orderId);
        Assert.Equal((orderStatus, paymentStatus, version), (order.OrderStatus, order.PaymentStatus, order.Version));
    }

    private async Task AssertStockAsync(Guid variantId, int stock, int reserved)
    {
        Assert.Equal(stock, await ScalarAsync($"SELECT stock_quantity FROM variants WHERE id = '{variantId}'"));
        Assert.Equal(reserved, await ScalarAsync($"SELECT reserved_quantity FROM variants WHERE id = '{variantId}'"));
    }

    private async Task AssertCouponAsync(int used, int reserved)
    {
        Assert.Equal(used, await ScalarAsync($"SELECT used_count FROM coupons WHERE id = '{_couponId}'"));
        Assert.Equal(reserved, await ScalarAsync($"SELECT reserved_count FROM coupons WHERE id = '{_couponId}'"));
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

    private async Task<List<TransitionRecord>> ReadTrailAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.TransitionRecords.AsNoTracking().Where(r => r.OrderId == orderId).ToListAsync(Ct);
    }

    private async Task<List<DispatchRecord>> ReadDispatchesAsync()
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.AsNoTracking().ToListAsync(Ct);
    }

    private async Task<InboundNotificationRecord> SingleInboundAsync()
    {
        await using AppDbContext reader = NewContext();
        return await reader.InboundNotificationRecords.AsNoTracking().SingleAsync(Ct);
    }

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private Task<long> CountAsync(string from) => ScalarAsync($"SELECT count(*) FROM {from}");

    private async Task<long> ScalarAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return Convert.ToInt64(await command.ExecuteScalarAsync(Ct), System.Globalization.CultureInfo.InvariantCulture);
    }

    private async Task<string> TextAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (string)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private sealed class ClosedBankTransferCondition : IBankTransferCondition
    {
        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(false);
    }
}
