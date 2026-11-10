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
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
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

/// <summary>The collection of the end-to-end branch flow tests (T390): one database copied from the template.</summary>
[CollectionDefinition(BranchFlowDefinition.Name)]
public sealed class BranchFlowDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "BranchFlows";
}

/// <summary>
/// The four branch narratives of 03 2.10.2-2.10.5, each in one test, from the service layer, on a real PostgreSQL and the manual clock (T390 KK1-KK10; K-1441): the mixed
/// order paid by card through the hand-written <see cref="FakePaymentGateway"/> whose lamp is cancelled before the carrier, the parcel withdrawn in transit and returned,
/// the return goods that arrive after the IBAN was deleted, and the guest order's e-mail correction. The main flow is T329's (<see cref="MainFlowTests"/>). The orders
/// are seeded at the state the narrative starts from; every later step goes through the application's own operations, and after each step the two axes, the
/// notifications and the records are read back from the tables. This file is the harness; the narratives are in the partial files.
/// </summary>
[Collection(BranchFlowDefinition.Name)]
public sealed partial class BranchFlowTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);
    private static readonly ShipmentInfo Shipment = new("YurticiKargo", null, "TR-3900001", false);

    private const string ContactEmail = "Ali@Example.com";
    private const string RefundIbanText = "TR330006100519786457841326";
    private const string FirmIban = "TR320010009999901234567890";

    private readonly CollectionDatabaseFixture _collection;
    private readonly ManualTimeProvider _clock = new(Start);
    private readonly FakePaymentGateway _gateway = new();
    private WebApplication? _app;
    private HttpClient? _client;
    private Admin? _admin;
    private Province? _returnProvince;
    private int _counter;

    public BranchFlowTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionAdmin AdminCaller => new(_admin!.Id, _admin.Name);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE inbound_notification_records, jobs, dispatch_records, disposal_records, refund_iban_requests, refund_ibans, refund_records, refunds, payment_records, "
            + "not_returned_closures, returns, return_rejections, withdrawals, late_delivery_terminations, item_removals, cancellations, internal_notes, transition_records, "
            + "order_email_histories, stock_reservations, cart_items, carts, order_items, orders, coupons, option_values, options, variants, products, customers, admins, "
            + "company_settings, legal_text_versions, provinces CASCADE");
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

    // ---- the host: the shared notification server (D-828) on the manual clock with the fake gateway ----

    private async Task StartHostAsync()
    {
        _app = await PaymentNotificationHost.StartAsync(_collection.Dsn, _clock, _gateway, Ct, services =>
        {
            // The Settings module's trail names the administrator.
            services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin!));

            // The Membership module as the e-mail correction uses it: the real address rules and the real account lookup (T375's proxy).
            services.AddScoped<AddressBooks>();
            services.AddScoped(provider => OrderCorrectionTests.CorrectionMembership.Create(provider.GetRequiredService<AppDbContext>(), provider.GetRequiredService<AddressBooks>()));

            // The card line is open (the provider's gateway is the fake); the transfer orders are seeded already placed, so the confirmation's condition is not asked.
            services.AddSingleton<IReadOnlyList<ConditionalLine>>([new ConditionalLine(ConditionalLine.CardPayment, [])]);
        });
        _client = _app.GetTestClient();
    }

    /// <summary>The fake provider's success notification of the whole total, recorded at the application's endpoint and processed by the job runner (03 2.5.1; T311).</summary>
    private async Task PayByCardAsync(Order order, string providerTransactionId)
    {
        _gateway.EnqueueVerification(new NotificationVerification.SignatureValid(new PaymentNotice(
            providerTransactionId, order.OrderNumber, FakePaymentGateway.PaymentEventType, FakePaymentGateway.SucceededOutcome, order.Total, order.Total, true, true, false)));
        using (HttpResponseMessage response = await _client!.PostAsync(
            new Uri(AntiforgeryExemptions.PaymentNotificationPath, UriKind.Relative), new StringContent("field=value", Encoding.UTF8, "application/x-www-form-urlencoded"), Ct))
        {
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

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

    /// <summary>
    /// What the card refund's handler does with the job row the cancellation wrote (05 4.3.3, 4.6.1; the handler itself is T405's): the rule says send, the "sending"
    /// record is written before the request, the fake provider accepts it, and its success is applied through the transition unit.
    /// </summary>
    private async Task<PaymentTransitionResult> RunCardRefundAsync(Order order, Refund refund)
    {
        Guid recordId;
        await using (AppDbContext db = NewContext())
        {
            Job job = await db.Jobs.AsNoTracking().SingleAsync(j => j.JobType == JobType.CardRefund && j.RelatedRecordId == refund.Id, Ct);
            Assert.Equal(CardRefundStep.SendRequest, CardRefundJobRule.Decide(await CardRefundJobRule.ReadLatestStateAsync(db, refund.Id, job.CreatedAt, Ct)));
            RefundRecord sending = RefundRecord.Sending(refund.Id, _clock.GetUtcNow());
            db.RefundRecords.Add(sending);
            await db.SaveChangesAsync(Ct);
            recordId = sending.Id;
        }

        Assert.IsType<RefundRequestResult.Accepted>(await _gateway.RequestRefundAsync(new CardRefund(order.OrderNumber, refund.Amount, recordId), Ct));
        return await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().ApplyCardRefundOutcomeAsync(new CardRefundOutcome(order.Id, recordId, RefundRecordState.Succeeded), Ct));
    }

    /// <summary>
    /// The firm's transfer refund of a withdrawal (02 7.4.5, 7.4.6; 06 4.33): the panel step that writes the row is a later task's, so the row is written here with the
    /// amount the after-sales rule homes give - the record's amount and the shipping fee when the fee rule includes it - and processed through the transition unit (Ö3-Ö5, B-8).
    /// </summary>
    private async Task<(Refund Refund, PaymentTransitionResult Result)> ProcessWithdrawalRefundAsync(Order order, OrderItem item, Guid withdrawalId, Guid? returnId)
    {
        RefundSubject subject = new(RefundType.Withdrawal, withdrawalId, returnId);
        RefundAmount amount;
        bool feeIncluded;
        await using (AsyncServiceScope scope = _app!.Services.CreateAsyncScope())
        {
            IAfterSalesModule afterSales = scope.ServiceProvider.GetRequiredService<IAfterSalesModule>();
            amount = (await afterSales.RefundAmountAsync(subject, Ct))!;
            DateTimeOffset? handed = (await ReadTrailAsync(order.Id)).Where(r => r.TransitionCode == TransitionCode.S5).Select(r => (DateTimeOffset?)r.CreatedAt).Min();
            feeIncluded = await afterSales.IsShippingFeeIncludedAsync(
                new ShippingFeeQuery([new ShippedItem(item.Id, item.Quantity, (await ReadItemAsync(item.Id)).DeliveryDate)], handed, subject), Ct);
        }

        Refund refund = Refund.ByAdmin(order.Id, RefundType.Withdrawal, PaymentMethod.BankTransfer, amount.Amount + (feeIncluded ? order.ShippingFee : 0m), _admin!.Id, _admin.Name, _clock.GetUtcNow());
        refund.WithdrawalId = withdrawalId;
        refund.ReturnId = returnId;
        refund.OrderItemId = item.Id;
        refund.Quantity = amount.Quantity;
        refund.IsShippingFeeIncluded = feeIncluded;
        await SaveAsync(c => c.Refunds.Add(refund));
        int version = await VersionAsync(order.Id);
        PaymentTransitionResult result = await RunAsync(sp => sp.GetRequiredService<IOrderTransitions>().ProcessTransferRefundAsync(
            new TransferRefundProcessing(order.Id, refund.Id, _admin.Id, _admin.Name, version, true), Ct));
        return (refund, result);
    }

    // One request: its own scope and transaction; the owner of the transaction saves (09 6 item 3).
    private async Task<T> RunAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        await using AsyncServiceScope scope = _app!.Services.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        Boxed<T> boxed = await services.GetRequiredService<ITransactionRunner>().RunAsync(async _ => new Boxed<T>(await work(services)), Ct);
        return boxed.Value;
    }

    // A read outside a transaction (the order page and the panel read so).
    private async Task<T> ReadAsync<T>(Func<IServiceProvider, Task<T>> work)
    {
        await using AsyncServiceScope scope = _app!.Services.CreateAsyncScope();
        return await work(scope.ServiceProvider);
    }

    private sealed record Boxed<T>(T Value) : IOperationResult
    {
        public bool IsRejected => false;
    }

    // ---- the store: the firm's settings with the return address, an administrator ----

    private async Task SeedStoreAsync()
    {
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        _returnProvince = Province.Create("99", "İade İli", Start);
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
        settings.ReturnRecipientName = "Örnek Ticaret İade";
        settings.ReturnProvinceId = _returnProvince.Id;
        settings.ReturnDistrict = "Yenimahalle";
        settings.ReturnAddressLine = "Depo Sokak 3";
        settings.ReturnPhone = "3125550000";
        await SaveAsync(c =>
        {
            c.Admins.Add(_admin);
            c.Provinces.Add(_returnProvince);
            c.CompanySettings.Add(settings);
        });
    }

    /// <summary>One item of a seeded order: its type, quantity, unit price, coupon share and - for a delivered item - its delivery day.</summary>
    private sealed record ItemSpec(ProductType Type, int Quantity, decimal UnitPrice, decimal CouponShare = 0m, DateOnly? DeliveryDate = null);

    /// <summary>A product and variant with <paramref name="stock"/> left and <paramref name="reserved"/> units set aside for the order, as the item's source.</summary>
    private async Task<Guid> SeedVariantAsync(Guid orderId, Guid itemId, int stock, int reserved)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Masa Lambası", $"masa-lambasi-t390-{n}", ProductType.Physical, 20m, Start.AddDays(-30)).WithSeededOrigin();
        Variant variant = Variant.Create(product.Id, $"T390-{n}", 1000m, Start.AddDays(-30), stock);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
            if (reserved > 0)
            {
                c.StockReservations.Add(StockReservation.ForVariant(orderId, variant.Id, reserved, Start));
            }
        });
        await ExecuteAsync($"UPDATE variants SET reserved_quantity = {reserved} WHERE id = '{variant.Id}'");
        await ExecuteAsync($"UPDATE order_items SET product_id = '{product.Id}', variant_id = '{variant.Id}' WHERE id = '{itemId}'");
        return variant.Id;
    }

    /// <summary>
    /// An order at the state its narrative starts from: the frozen firm, documents, addresses and shipping promise, its items, and the two axes set around the
    /// entity (it takes them only through the transition unit; the pair rule of the database still applies). The total is the items less their coupon shares plus the fee.
    /// </summary>
    private async Task<(Order Order, List<OrderItem> Items)> SeedOrderAsync(
        PaymentMethod method, bool isGuest, OrderStatus orderStatus, PaymentStatus paymentStatus, decimal shippingFee, DateTimeOffset orderedAt, params ItemSpec[] specs)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, orderedAt, "metin", orderedAt);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, orderedAt, "metin", orderedAt);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, orderedAt, "metin", orderedAt).WithHosting();
        decimal total = specs.Sum(s => (s.UnitPrice * s.Quantity) - s.CouponShare) + shippingFee;
        DateTimeOffset windowEnds = method == PaymentMethod.Card ? orderedAt.AddMinutes(ProductConstants.CardPaymentMinutes) : orderedAt.AddDays(2);
        Order order = Order.Create(
            $"2026-{n + 390000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, isGuest, ContactEmail, total, shippingFee, windowEnds, orderedAt);
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? FirmIban : null;
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.FreezeShippingPromise(2);
        order.FreezeShippingFeeVatBreakdown(shippingFee > 0m ? [new Shopfolio.Modules.Catalog.Contracts.Pricing.ShippingVatPart(20m, shippingFee, shippingFee / 6m)] : []);
        order.ShippingRecipientName = "Ali Veli";
        order.ShippingPhone = "5321234567";
        order.ShippingProvinceId = province.Id;
        order.ShippingDistrict = "Çankaya";
        order.ShippingAddressLine = "Atatürk Bulvarı 1";
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = province.Id;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = province.Id;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");

        List<OrderItem> items = [];
        foreach (ItemSpec spec in specs)
        {
            OrderItem item = OrderItem.Create(order.Id, null, null, spec.Type, spec.Type.ToString(), spec.Quantity, spec.UnitPrice, 20m, spec.UnitPrice * spec.Quantity / 6m, orderedAt);
            item.CouponShare = spec.CouponShare;
            item.DownloadAllowance = spec.Type == ProductType.Digital ? 5 : null;
            item.FulfilmentPeriod = spec.Type == ProductType.Service ? 3 : null;
            if (spec.DeliveryDate is { } day)
            {
                item.DeliveryDate = day;
                item.DeliveryMarkedAt = TurkeyTime.StartOf(day).AddHours(12);
            }

            items.Add(item);
        }

        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        return (order, items);
    }

    // ---- reads, each from its own connection after the step ----

    /// <summary>KK10: both axes after the step, read from the order row.</summary>
    private async Task AssertAxesAsync(Guid orderId, OrderStatus orderStatus, PaymentStatus paymentStatus)
    {
        Order order = await ReadOrderAsync(orderId);
        Assert.Equal((orderStatus, paymentStatus), (order.OrderStatus, order.PaymentStatus));
    }

    private async Task<int> VersionAsync(Guid orderId) => (int)await ScalarAsync($"SELECT version FROM orders WHERE id = '{orderId}'");

    private async Task<Order> ReadOrderAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
    }

    private async Task<OrderItem> ReadItemAsync(Guid itemId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.OrderItems.AsNoTracking().SingleAsync(i => i.Id == itemId, Ct);
    }

    private async Task<List<OrderItem>> ReadItemsAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).OrderBy(i => i.Id).ToListAsync(Ct);
    }

    private async Task<List<TransitionRecord>> ReadTrailAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.TransitionRecords.AsNoTracking().Where(r => r.OrderId == orderId).ToListAsync(Ct);
    }

    private async Task<List<TransitionCode>> CodesAsync(Guid orderId) =>
        [.. (await ReadTrailAsync(orderId)).OrderBy(r => r.CreatedAt).ThenBy(r => r.TransitionCode).Select(r => r.TransitionCode)];

    private async Task<List<(DispatchType Type, string? To)>> DispatchesAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return [.. (await reader.DispatchRecords.AsNoTracking().Where(d => d.OrderId == orderId).ToListAsync(Ct)).Select(d => (d.DispatchType, d.RecipientEmail))];
    }

    private async Task<int> CountDispatchesAsync(Guid orderId, DispatchType type) => (await DispatchesAsync(orderId)).Count(d => d.Type == type);

    /// <summary>The firm's notices (F-3, F-4) carry no order; they point at their record (08 2.2.7) and go to the firm's contact e-mail.</summary>
    private async Task<int> CountFirmDispatchesAsync(DispatchType type, Guid relatedRecordId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.AsNoTracking().CountAsync(
            d => d.DispatchType == type && d.RelatedRecordId == relatedRecordId && d.OrderId == null && d.RecipientEmail == OrderingTestServices.CompanyEmail, Ct);
    }

    private async Task<OrderOperationsView> BuyerViewAsync(Guid orderId) =>
        (await ReadAsync(sp => sp.GetRequiredService<IOrderOperations>().ReadAsync(orderId, ActorKind.Buyer, Ct)))!;

    private async Task<int> ClosedUnitsAsync(Guid itemId) =>
        (await ReadAsync(sp => sp.GetRequiredService<IAfterSalesModule>().ClosedQuantitiesAsync([itemId], Ct))).GetValueOrDefault(itemId);

    private async Task<ItemMarks> MarksAsync(OrderItem item)
    {
        OrderItem saved = await ReadItemAsync(item.Id);
        IReadOnlyDictionary<Guid, ItemMarks> marks = await ReadAsync(sp => sp.GetRequiredService<IAfterSalesModule>().ItemMarksAsync(
            [new ItemDelivery(saved.Id, saved.ProductType, saved.DeliveryDate)], Ct));
        return marks[saved.Id];
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

    private Task<long> CountAsync(string from) => ScalarAsync($"SELECT count(*) FROM {from}");

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

    private sealed class FixedActor : IAuditActorSource
    {
        private readonly Admin _admin;

        public FixedActor(Admin admin)
        {
            _admin = admin;
        }

        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(_admin.Id, _admin.Name, _admin.Email));
    }
}
