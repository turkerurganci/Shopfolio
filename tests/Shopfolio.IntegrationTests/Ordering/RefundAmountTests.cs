using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Imaging;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
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

/// <summary>
/// The unit of an operation and the refund's amount on a real PostgreSQL (T369 KK1-KK8; 02 7.1.6, 7.1.7, 7.2.9, 7.4.6; 06 2.7.2, 4.33): the units rescinded on a defect
/// claim leave the quantity open to an operation and their share leaves the last record's rest, the units one record closed are no other record's subject, a receipt's
/// refund is its part of the withdrawal's amounts, and the shipping fee goes to the one refund at its trigger. The records are the module's rows; the reads are
/// <see cref="IAfterSalesModule.RefundAmountAsync"/> and <see cref="IAfterSalesModule.IsShippingFeeIncludedAsync"/>, the expected shares come from <see cref="ItemShares"/>.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class RefundAmountTests : IAsyncLifetime
{
    private const string CompanyEmail = "firma-iletisim@example.com";

    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateTimeOffset DeliveredAt = Start.AddDays(-5);

    private static readonly DateOnly DeliveryDay = DateOnly.FromDateTime(DeliveredAt.UtcDateTime);

    /// <summary>02 7.1.7's example: a unit price of 300, the item's coupon share 100 and VAT 50 over three units.</summary>
    private static readonly ItemFrozenAmounts Frozen = new(300m, 100m, 50m);

    private readonly CollectionDatabaseFixture _collection;

    private readonly ManualTimeProvider _clock = new(Start);

    private Admin? _admin;

    private int _counter;

    public RefundAmountTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync("TRUNCATE dispatch_records, cancellations, transition_records, order_items, orders, admins, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        await SaveAsync(c => c.Admins.Add(_admin));
        _clock.SetUtcNow(Start);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK2: the units open to an operation ----

    [Fact]
    public async Task KK1_RescindedUnits_LeaveTheOpenQuantity_AndTheLastRecordTakesTheRestTheRescissionLeft()
    {
        (Order order, OrderItem item) = await SeedAsync(OrderStatus.Preparing, (ProductType.Physical, 3, false));
        Refund rescission = Rescission(order.Id, item.Id, 1, 33.33m, 16.66m);
        await SaveAsync(c => c.Refunds.Add(rescission));

        ItemRecordWriteResult tooMany = await WriteAsync(order.Id, (m, t) => m.AddCancellationAsync(Cancel(order, item, 3), t));
        ItemRecordWriteResult rest = await WriteAsync(order.Id, (m, t) => m.AddCancellationAsync(Cancel(order, item, 2), t));

        Assert.Equal(ItemRecordRefusal.QuantityExceedsOpen, tooMany.Refusal);
        Assert.False(rest.IsRejected);
        await using AppDbContext reader = NewContext();
        Cancellation saved = await reader.Cancellations.SingleAsync(c => c.Id == rest.RecordId, Ct);
        Assert.Equal((2, 600m, 66.67m, 33.34m), (saved.Quantity, saved.ProductAmount, saved.CouponShare, saved.VatAmount));
        Assert.Equal(Frozen.CouponShare, saved.CouponShare + rescission.CouponShare);
    }

    [Fact]
    public async Task KK1_AVoidedRescission_GivesItsUnitsBack()
    {
        (Order order, OrderItem item) = await SeedAsync(OrderStatus.Preparing, (ProductType.Physical, 3, false));
        Refund voided = Rescission(order.Id, item.Id, 1, 33.33m, 16.66m);
        voided.VoidedAt = Start;
        await SaveAsync(c => c.Refunds.Add(voided));

        ItemRecordWriteResult all = await WriteAsync(order.Id, (m, t) => m.AddCancellationAsync(Cancel(order, item, 3), t));

        Assert.False(all.IsRejected);
    }

    [Fact]
    public async Task KK2_TheUnitsOneRecordClosed_AreNoOtherRecordsSubject()
    {
        (Order order, OrderItem item) = await SeedAsync(OrderStatus.Preparing, (ProductType.Physical, 3, false));

        ItemRecordWriteResult two = await WriteAsync(order.Id, (m, t) => m.AddCancellationAsync(Cancel(order, item, 2), t));
        ItemRecordWriteResult twoMore = await WriteAsync(order.Id, (m, t) => m.AddCancellationAsync(Cancel(order, item, 2), t));
        ItemRecordWriteResult last = await WriteAsync(order.Id, (m, t) => m.AddCancellationAsync(Cancel(order, item, 1), t));
        ItemRecordWriteResult none = await WriteAsync(order.Id, (m, t) => m.AddCancellationAsync(Cancel(order, item, 1), t));

        Assert.False(two.IsRejected);
        Assert.Equal(ItemRecordRefusal.QuantityExceedsOpen, twoMore.Refusal);
        Assert.False(last.IsRejected);
        Assert.Equal(ItemRecordRefusal.QuantityExceedsOpen, none.Refusal);
    }

    // ---- KK3, KK4, KK6: the record's refund ----

    [Fact]
    public async Task KK3_ThreeSingleUnitCancellations_Refund266_67_266_67_266_66_AndTheSharesAddUpToTheItems()
    {
        (Order order, OrderItem item) = await SeedAsync(OrderStatus.Preparing, (ProductType.Physical, 3, false));
        List<RefundAmount> refunds = [];
        foreach (int _ in Enumerable.Range(0, 3))
        {
            ItemRecordWriteResult written = await WriteAsync(order.Id, (m, t) => m.AddCancellationAsync(Cancel(order, item, 1), t));
            refunds.Add((await ReadAsync(m => m.RefundAmountAsync(new RefundSubject(RefundType.Cancellation, written.RecordId!.Value), Ct)))!);
        }

        Assert.Equal([266.67m, 266.67m, 266.66m], refunds.Select(r => r.Amount));
        Assert.Equal([33.33m, 33.33m, 33.34m], refunds.Select(r => r.CouponShare));
        Assert.Equal(Frozen.VatAmount, refunds.Sum(r => r.VatAmount));
    }

    [Fact]
    public async Task KK5_EachReceiptOfAWithdrawal_RefundsItsPartOfTheDeclaration_TheLastTheRest()
    {
        (Order order, OrderItem item) = await SeedAsync(OrderStatus.Delivered, (ProductType.Physical, 3, true));
        Withdrawal withdrawal = Withdrawal.ByBuyer(item.Id, 3, DeliveryDay.AddDays(1), null, new ItemAmounts(900m, 100m, 50m), null, Start);
        Return first = Return.ForWithdrawal(item.Id, withdrawal.Id, 1, DeliveryDay.AddDays(2), _admin!.Id, _admin.Name, Start.AddHours(1));
        Return second = Return.ForWithdrawal(item.Id, withdrawal.Id, 2, DeliveryDay.AddDays(3), _admin.Id, _admin.Name, Start.AddHours(2));
        await SaveAsync(c =>
        {
            c.Withdrawals.Add(withdrawal);
            c.Returns.AddRange(first, second);
        });

        RefundAmount? one = await ReadAsync(m => m.RefundAmountAsync(new RefundSubject(RefundType.Withdrawal, withdrawal.Id, first.Id), Ct));
        RefundAmount? two = await ReadAsync(m => m.RefundAmountAsync(new RefundSubject(RefundType.Withdrawal, withdrawal.Id, second.Id), Ct));
        RefundAmount? declaration = await ReadAsync(m => m.RefundAmountAsync(new RefundSubject(RefundType.Withdrawal, withdrawal.Id), Ct));
        RefundAmount? missing = await ReadAsync(m => m.RefundAmountAsync(new RefundSubject(RefundType.Withdrawal, Guid.NewGuid(), first.Id), Ct));

        Assert.Equal(new RefundAmount(1, 300m, 33.33m, 16.66m), one);
        Assert.Equal(new RefundAmount(2, 600m, 66.67m, 33.34m), two);
        Assert.Equal([266.67m, 533.33m], new[] { one!.Amount, two!.Amount });
        Assert.Equal(withdrawal.CouponShare, one.CouponShare + two.CouponShare);
        Assert.Equal(new RefundAmount(3, 900m, 100m, 50m), declaration);
        Assert.Null(missing);
    }

    // ---- KK7: the shipping fee on the cancellation ----

    [Fact]
    public async Task KK7_TheLastCancellationOfAllPhysicalUnitsBeforeTheShipment_CarriesTheFee_EvenWithADigitalItemLeft()
    {
        (Order order, OrderItem[] items) = await SeedManyAsync(OrderStatus.Preparing, (ProductType.Physical, 2, false), (ProductType.Physical, 1, false), (ProductType.Digital, 1, false));
        Cancellation first = Cancellation.ByBuyer(items[0].Id, 2, new ItemAmounts(600m, 0m, 100m), null, Start);
        Cancellation last = Cancellation.ByBuyer(items[1].Id, 1, new ItemAmounts(300m, 0m, 50m), null, Start.AddHours(1));
        await SaveAsync(c => c.Cancellations.AddRange(first, last));

        Assert.False(await FeeAsync(items, null, RefundType.Cancellation, first.Id));
        Assert.True(await FeeAsync(items, null, RefundType.Cancellation, last.Id));
    }

    [Fact]
    public async Task KK7_APartialCancellation_KeepsTheFee()
    {
        (Order order, OrderItem[] items) = await SeedManyAsync(OrderStatus.Preparing, (ProductType.Physical, 2, false));
        Cancellation partial = Cancellation.ByBuyer(items[0].Id, 1, new ItemAmounts(300m, 0m, 50m), null, Start);
        await SaveAsync(c => c.Cancellations.Add(partial));

        Assert.False(await FeeAsync(items, null, RefundType.Cancellation, partial.Id));
    }

    [Theory]
    [InlineData(CancellationReason.UndeliveredBuyerCaused, false)]
    [InlineData(CancellationReason.CannotShipToAddress, true)]
    public async Task KK7_TheS9CancellationAfterTheShipment_FollowsTheReason(CancellationReason reason, bool included)
    {
        (Order order, OrderItem[] items) = await SeedManyAsync(OrderStatus.DeliveryFailed, (ProductType.Physical, 1, false));
        Cancellation s9 = Cancellation.ByAdmin(items[0].Id, 1, _admin!.Id, _admin.Name, reason, null, new ItemAmounts(300m, 0m, 50m), Start);
        await SaveAsync(c => c.Cancellations.Add(s9));

        Assert.Equal(included, await FeeAsync(items, Start.AddDays(-2), RefundType.Cancellation, s9.Id));
    }

    [Fact]
    public async Task KK7_TheTermination_AlwaysCarriesTheFee()
    {
        (Order order, OrderItem[] items) = await SeedManyAsync(OrderStatus.Shipped, (ProductType.Physical, 2, false));
        LateDeliveryTermination termination = LateDeliveryTermination.ByBuyer(items[0].Id, 2, DateOnly.FromDateTime(Start.UtcDateTime), new ItemAmounts(600m, 0m, 100m), null, Start);
        await SaveAsync(c => c.LateDeliveryTerminations.Add(termination));

        Assert.True(await FeeAsync(items, Start.AddDays(-20), RefundType.LateDeliveryTermination, termination.Id));
    }

    // ---- KK8: the shipping fee on the withdrawal ----

    [Fact]
    public async Task KK8_TheReceiptOfTheLastShippedUnits_CarriesTheFee_NotTheDeclarationNorTheEarlierReceipt()
    {
        (Order order, OrderItem[] items) = await SeedManyAsync(OrderStatus.Delivered, (ProductType.Physical, 2, true));
        Withdrawal withdrawal = Withdrawal.ByBuyer(items[0].Id, 2, DeliveryDay.AddDays(1), null, new ItemAmounts(600m, 0m, 100m), null, Start);
        Return first = Return.ForWithdrawal(items[0].Id, withdrawal.Id, 1, DeliveryDay.AddDays(2), _admin!.Id, _admin.Name, Start.AddHours(1));
        Return last = Return.ForWithdrawal(items[0].Id, withdrawal.Id, 1, DeliveryDay.AddDays(3), _admin.Id, _admin.Name, Start.AddHours(2));
        await SaveAsync(c =>
        {
            c.Withdrawals.Add(withdrawal);
            c.Returns.AddRange(first, last);
        });

        DateTimeOffset handed = DeliveredAt.AddDays(-1);
        Assert.False(await FeeAsync(items, handed, RefundType.Withdrawal, withdrawal.Id));
        Assert.False(await FeeAsync(items, handed, RefundType.Withdrawal, withdrawal.Id, first.Id));
        Assert.True(await FeeAsync(items, handed, RefundType.Withdrawal, withdrawal.Id, last.Id));
    }

    [Fact]
    public async Task KK8_ARejectedUnit_StaysWithTheCustomer_AndNoReceiptCarriesTheFee()
    {
        (Order order, OrderItem[] items) = await SeedManyAsync(OrderStatus.Delivered, (ProductType.Physical, 2, true));
        Withdrawal withdrawal = Withdrawal.ByBuyer(items[0].Id, 2, DeliveryDay.AddDays(1), null, new ItemAmounts(600m, 0m, 100m), null, Start);
        Return received = Return.ForWithdrawal(items[0].Id, withdrawal.Id, 1, DeliveryDay.AddDays(2), _admin!.Id, _admin.Name, Start.AddHours(1));
        ReturnRejection rejected = ReturnRejection.Create(items[0].Id, withdrawal.Id, 1, DeliveryDay.AddDays(2), _admin.Id, _admin.Name, Start.AddHours(1));
        await SaveAsync(c =>
        {
            c.Withdrawals.Add(withdrawal);
            c.Returns.Add(received);
            c.ReturnRejections.Add(rejected);
        });

        Assert.False(await FeeAsync(items, DeliveredAt.AddDays(-1), RefundType.Withdrawal, withdrawal.Id, received.Id));
    }

    [Fact]
    public async Task KK8_AWithdrawalBeforeTheDelivery_CarriesTheFeeAtItsDeclaration_TheItemCancelledBeforeTheShipmentLeftOut()
    {
        (Order order, OrderItem[] items) = await SeedManyAsync(OrderStatus.Shipped, (ProductType.Physical, 1, false), (ProductType.Physical, 1, false));
        DateTimeOffset handed = Start.AddDays(-1);
        Cancellation cancelled = Cancellation.ByBuyer(items[1].Id, 1, new ItemAmounts(300m, 0m, 50m), null, handed.AddHours(-1));
        Withdrawal withdrawal = Withdrawal.ByBuyer(items[0].Id, 1, DateOnly.FromDateTime(Start.UtcDateTime), null, new ItemAmounts(300m, 0m, 50m), null, Start);
        await SaveAsync(c =>
        {
            c.Cancellations.Add(cancelled);
            c.Withdrawals.Add(withdrawal);
        });

        Assert.True(await FeeAsync(items, handed, RefundType.Withdrawal, withdrawal.Id));
        Assert.False(await FeeAsync(items, null, RefundType.Withdrawal, withdrawal.Id));
    }

    // ---- helpers ----

    private static ItemRecordSubject Subject(Order order, OrderItem item) =>
        new(order.Id, item.Id, item.Quantity, item.ProductType, OrderStatus.Preparing, item.DeliveryDate, item.WithdrawalExemptionReason);

    private CustomerCancellationEntry Cancel(Order order, OrderItem item, int quantity) => new(Subject(order, item), quantity, Frozen, _clock.GetUtcNow());

    private Refund Rescission(Guid orderId, Guid itemId, int quantity, decimal coupon, decimal vat)
    {
        Refund refund = Refund.ByAdmin(orderId, RefundType.DefectClaimRescission, PaymentMethod.Card, (Frozen.UnitPrice * quantity) - coupon, _admin!.Id, _admin.Name, Start);
        refund.OrderItemId = itemId;
        refund.Quantity = quantity;
        refund.CouponShare = coupon;
        refund.VatAmount = vat;
        return refund;
    }

    private Task<bool> FeeAsync(OrderItem[] items, DateTimeOffset? handed, RefundType type, Guid recordId, Guid? returnId = null) =>
        ReadAsync(m => m.IsShippingFeeIncludedAsync(
            new ShippingFeeQuery(
                [.. items.Where(i => i.ProductType == ProductType.Physical).Select(i => new ShippedItem(i.Id, i.Quantity, i.DeliveryDate))],
                handed,
                new RefundSubject(type, recordId, returnId)),
            Ct));

    private async Task<T> ReadAsync<T>(Func<IAfterSalesModule, Task<T>> read)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await read(scope.ServiceProvider.GetRequiredService<IAfterSalesModule>());
    }

    /// <summary>An AfterSales writer called like the transition unit calls it: in the runner's transaction, after the order's row lock (06 2.6.4).</summary>
    private async Task<ItemRecordWriteResult> WriteAsync(Guid orderId, Func<IAfterSalesModule, CancellationToken, Task<ItemRecordWriteResult>> work)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        AppDbContext db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        OrderLocks locks = scope.ServiceProvider.GetRequiredService<OrderLocks>();
        IAfterSalesModule module = scope.ServiceProvider.GetRequiredService<IAfterSalesModule>();
        _clock.Advance(TimeSpan.FromMinutes(1));
        Written written = await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(
            async token =>
            {
                await locks.LockAsync([orderId], token);
                Order order = await db.Orders.SingleAsync(o => o.Id == orderId, token);

                // The transition unit loads the items under the lock before it writes their records (D-95; 09 10.2 item 3).
                await db.OrderItems.Where(i => i.OrderId == orderId).LoadAsync(token);
                ItemRecordWriteResult result = await work(module, token);
                if (!result.IsRejected)
                {
                    OrderTransitions.RaiseVersionOnce(db, order);
                }

                return new Written(result);
            },
            Ct);
        return written.Result;
    }

    private async Task<(Order Order, OrderItem Item)> SeedAsync(OrderStatus status, (ProductType Type, int Quantity, bool Delivered) item)
    {
        (Order order, OrderItem[] items) = await SeedManyAsync(status, item);
        return (order, items[0]);
    }

    private async Task<(Order Order, OrderItem[] Items)> SeedManyAsync(OrderStatus orderStatus, params (ProductType Type, int Quantity, bool Delivered)[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 1000m, 0m, Start.AddHours(1), Start);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
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

        OrderItem[] items =
        [
            .. lines.Select(line =>
            {
                OrderItem item = OrderItem.Create(order.Id, null, null, line.Type, "Ürün", line.Quantity, Frozen.UnitPrice, 20m, Frozen.VatAmount, Start);
                item.CouponShare = Frozen.CouponShare;
                if (line.Type == ProductType.Digital)
                {
                    item.DownloadAllowance = 5;
                }

                if (line.Delivered)
                {
                    item.DeliveryMarkedAt = DeliveredAt;
                    item.DeliveryDate = DeliveryDay;
                }

                return item;
            }),
        ];

        await SaveAsync(c =>
        {
            c.Provinces.Add(province);
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{PaymentStatus.Paid}' WHERE id = '{order.Id}'");
        return (order, items);
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddScoped<JobEnqueueTracker>();
        services.AddSingleton<IJobSignal, JobSignal>();
        services.AddScoped<ITransactionRunner, TransactionRunner>();
        services.AddAppCache();
        services.AddScoped<AdvisoryLocks>();
        services.AddSingleton<TimeProvider>(_clock);
        services.AddScoped<IJobQueue, JobQueue>();
        services.AddFileStore();
        services.AddOptions<Shopfolio.Infrastructure.InstallationSettings.StatusThresholdOptions>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.IDiskUsageReader, Shopfolio.Infrastructure.Status.DriveDiskUsageReader>();
        services.AddSingleton<Shopfolio.Infrastructure.Status.DiskUsageGuard>();
        services.AddSingleton<ImageProcessor>();
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddNoticeDependencies();
        services.AddOrderingModule();
        services.AddPaymentModule();
        services.AddScoped<ICompanyContactEmail, FixedCompanyContactEmail>();
        return services.BuildServiceProvider();
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

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private sealed record Written(ItemRecordWriteResult Result) : IOperationResult
    {
        public bool IsRejected => Result.IsRejected;
    }

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>(CompanyEmail);
    }
}
