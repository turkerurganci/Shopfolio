using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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
/// The customer's cancellation of a paid item's units on a real PostgreSQL and a fake clock (T367 criteria 1-8, KK1-KK8; 03 1.11.2-1.11.4; 02 7.2.1, 7.2.2,
/// 7.2.5-7.2.7, 7.1.7): <see cref="IOrderTransitions.CancelItemAsync"/> is open without the firm's approval until the order passes to Shipped, by units; the
/// last open units close the order with the customer's S4 and its one B-7, a mixed order goes on along its other items' line; an unpaid order is cancelled only
/// as a whole; a digital item never, a service item until its mark; the units go back to stock and the service capacity, the coupon right only with the
/// order's last unit; the coupon share is set aside by the units and the last record takes the rest. Every operation runs in the transaction runner like a
/// page would call it; what it wrote is read back from the tables.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed class CustomerCancellationTests : IAsyncLifetime
{
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = DateOnly.FromDateTime(Start.UtcDateTime);

    private readonly CollectionDatabaseFixture _collection;

    private readonly ManualTimeProvider _clock = new(Start);

    private ServiceProvider _provider = null!;

    private Admin? _admin;

    private int _counter;

    public CustomerCancellationTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_records, refund_iban_requests, refunds, payment_records, returns, return_rejections, defect_claims, withdrawals, "
            + "late_delivery_terminations, item_removals, cancellations, internal_notes, transition_records, stock_reservations, order_items, orders, coupons, "
            + "variants, products, admins, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        await SaveAsync(c => c.Admins.Add(_admin));
        _clock.SetUtcNow(Start);
        _provider = BuildProvider();
    }

    public async ValueTask DisposeAsync() => await _provider.DisposeAsync();

    // ---- KK1, KK2: until Shipped, without the firm's approval; a preparing order included ----

    [Theory]
    [InlineData(OrderStatus.Placed)]
    [InlineData(OrderStatus.Preparing)]
    public async Task APhysicalItem_OfAPaidOrderNotYetShipped_IsCancelledByUnits_WithoutApproval_ThePreparingOrderToo_KK1_KK2(OrderStatus status)
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 5);
        (Order order, List<OrderItem> items) = await SeedAsync(status, PaymentStatus.Paid, null, new Line(ProductType.Physical, 2, variant.Id));

        OrderTransitionResult result = await CancelAsync(order.Id, items[0].Id, 1);

        Assert.False(result.IsRejected);
        Assert.Empty(result.TransitionRecordIds);
        await using AppDbContext reader = NewContext();
        Cancellation cancellation = await reader.Cancellations.SingleAsync(c => c.OrderItemId == items[0].Id, Ct);
        Assert.Equal((result.ItemRecordId, 1, ActorKind.Buyer, (Guid?)null, (Guid?)null), (cancellation.Id, cancellation.Quantity, cancellation.ActorKind, cancellation.AdminId, cancellation.TransitionRecordId));
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((status, PaymentStatus.Paid, 2), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
        Assert.Equal((6, 0), await VariantAsync(variant.Id));
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.DeliveryFailed)]
    [InlineData(OrderStatus.Delivered)]
    public async Task APhysicalItem_OnceTheOrderIsShipped_IsNotCancelled_TheWayIsWithdrawal_AndNothingIsWritten_KK1(OrderStatus status)
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 5);
        (Order order, List<OrderItem> items) = await SeedAsync(status, PaymentStatus.Paid, null, new Line(ProductType.Physical, 2, variant.Id, status == OrderStatus.Delivered));

        OrderTransitionResult result = await CancelAsync(order.Id, items[0].Id, 1);

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, status, 1);
        Assert.Equal((5, 0), await VariantAsync(variant.Id));
    }

    // ---- KK3: partial and whole; the mixed order's line ----

    [Fact]
    public async Task APartialCancellation_KeepsTheOrder_TheLastUnitsCloseItWithTheCustomersS4_LinkedToTheRecord_AndOneB7Each_KK3_KK8()
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 5);
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 2, variant.Id));

        OrderTransitionResult first = await CancelAsync(order.Id, items[0].Id, 1);
        await using (AppDbContext reader = NewContext())
        {
            Assert.Equal(OrderStatus.Preparing, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        }

        _clock.Advance(TimeSpan.FromHours(2));
        OrderTransitionResult last = await CancelAsync(order.Id, items[0].Id, 1);

        Assert.False(first.IsRejected);
        Assert.False(last.IsRejected);
        Guid s4 = Assert.Single(last.TransitionRecordIds);
        await using AppDbContext after = NewContext();
        Order saved = await after.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Paid, 3), (saved.OrderStatus, saved.PaymentStatus, saved.Version));

        // D-306: one path - the customer's own S4, not a closure S4 beside it; one B-7 per cancellation.
        TransitionRecord record = await after.TransitionRecords.SingleAsync(t => t.OrderId == order.Id, Ct);
        Assert.Equal((s4, TransitionCode.S4, ActorKind.Buyer), (record.Id, record.TransitionCode, record.ActorKind));
        Assert.Equal(2, await CountB7Async(order.Id));

        // D-519: the record of the last units is linked to the transition that closed the order; the earlier one is not.
        Assert.Equal(s4, (await after.Cancellations.SingleAsync(c => c.Id == last.ItemRecordId, Ct)).TransitionRecordId);
        Assert.Null((await after.Cancellations.SingleAsync(c => c.Id == first.ItemRecordId, Ct)).TransitionRecordId);
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IReadOnlyList<CancelledUnits> linked = await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>().FindCancellationsOfTransitionAsync(s4, Ct);
        Assert.Equal(last.ItemRecordId, Assert.Single(linked).CancellationId);
        Assert.Equal((7, 0), await VariantAsync(variant.Id));
    }

    [Fact]
    public async Task AMixedOrder_WhosePhysicalUnitsAreAllCancelled_GoesOnAlongItsDeliveredDigitalItem_ToDelivered_NotCancelled_KK3()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 1), new Line(ProductType.Digital, 1, null, true));

        OrderTransitionResult result = await CancelAsync(order.Id, items[0].Id, 1);

        Assert.False(result.IsRejected);
        Assert.Equal(TransitionCode.S10, result.ClosureCode);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Delivered, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == order.Id && t.TransitionCode == TransitionCode.S4, Ct));
        Assert.Equal(1, await CountB7Async(order.Id));
    }

    [Fact]
    public async Task AMixedOrder_WhosePhysicalUnitsAreAllCancelled_StaysOnItsOpenServiceItemsLine_KK3()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 1), new Line(ProductType.Service, 1));

        OrderTransitionResult result = await CancelAsync(order.Id, items[0].Id, 1);

        Assert.False(result.IsRejected);
        Assert.Null(result.ClosureCode);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Preparing, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == order.Id, Ct));
    }

    [Fact]
    public async Task AReturnedShipments_LastOpenServiceUnit_IsCancelled_AndTheOrderWaitsForTheFirmsS9Closing_KK3_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.DeliveryFailed, PaymentStatus.Paid, null, new Line(ProductType.Physical, 1), new Line(ProductType.Service, 1));
        await SaveAsync(c => c.Cancellations.Add(
            Cancellation.ByAdmin(items[0].Id, 1, _admin!.Id, _admin.Name, CancellationReason.OutOfStock, null, new ItemAmounts(100m, 0m, 16.67m), Start)));

        OrderTransitionResult result = await CancelAsync(order.Id, items[1].Id, 1);

        Assert.False(result.IsRejected);
        Assert.Empty(result.TransitionRecordIds);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.DeliveryFailed, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.Equal(1, await CountB7Async(order.Id));
    }

    // ---- KK4: before the payment's approval only the whole order ----

    [Fact]
    public async Task BeforeThePaymentsApproval_AnItemIsNotCancelled_OnlyTheWholeOrderWithS3AndO2_KK4()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Placed, PaymentStatus.Pending, null, new Line(ProductType.Physical, 2), new Line(ProductType.Service, 1));

        OrderTransitionResult item = await CancelAsync(order.Id, items[0].Id, 1);

        Assert.Equal(TransitionRefusal.ConditionNotMet, item.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Placed, 1);

        OrderTransitionResult whole = await RunAsync((unit, token) => unit.CancelOrderAsync(CancelOrderRequest.ByCustomer(order.Id), token));

        Assert.False(whole.IsRejected);
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed), (saved.OrderStatus, saved.PaymentStatus));
        Assert.Equivalent(
            new[] { TransitionCode.S3, TransitionCode.O2 },
            await reader.TransitionRecords.Where(t => t.OrderId == order.Id).Select(t => t.TransitionCode).ToListAsync(Ct));
    }

    // ---- KK5: digital never; service by units until its mark ----

    [Fact]
    public async Task ADigitalItem_IsNotCancelledAfterThePaymentsApproval_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 1), new Line(ProductType.Digital, 1));

        OrderTransitionResult result = await CancelAsync(order.Id, items[1].Id, 1);

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Preparing, 1);
    }

    [Theory]
    [InlineData(OrderStatus.Preparing)]
    [InlineData(OrderStatus.Shipped)]
    public async Task AServiceItem_IsCancelledByUnits_WhateverTheShipment_AndItsCapacityGoesBackToThePool_KK5_KK6(OrderStatus status)
    {
        Variant service = await SeedVariantAsync(ProductType.Service, 4);
        (Order order, List<OrderItem> items) = await SeedAsync(
            status, PaymentStatus.Paid, null, new Line(ProductType.Physical, 1), new Line(ProductType.Service, 3, service.Id));

        OrderTransitionResult result = await CancelAsync(order.Id, items[1].Id, 2);

        Assert.False(result.IsRejected);
        Assert.Equal((6, 0), await VariantAsync(service.Id));
        await using AppDbContext reader = NewContext();
        Assert.Equal(2, (await reader.Cancellations.SingleAsync(c => c.OrderItemId == items[1].Id, Ct)).Quantity);
        Assert.Equal(status, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
    }

    [Fact]
    public async Task AServiceItem_MarkedCompleted_IsNotCancelled_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 1), new Line(ProductType.Service, 2, null, true));

        OrderTransitionResult result = await CancelAsync(order.Id, items[1].Id, 1);

        Assert.Equal(TransitionRefusal.ConditionNotMet, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Preparing, 1);
    }

    [Fact]
    public async Task AServiceItemWhoseUnitsAreAllCancelled_IsNoLongerMarkedCompleted_D685()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 1), new Line(ProductType.Service, 2));
        Assert.False((await CancelAsync(order.Id, items[1].Id, 2)).IsRejected);

        OrderTransitionResult mark = await RunAsync((unit, token) => unit.MarkServiceCompletedAsync(
            new MarkServiceCompletedRequest(order.Id, items[1].Id, new TransitionAdmin(_admin!.Id, _admin.Name), 2), token));

        Assert.Equal(TransitionRefusal.ConditionNotMet, mark.Refusal);
        await using AppDbContext reader = NewContext();
        Assert.Null((await reader.OrderItems.SingleAsync(i => i.Id == items[1].Id, Ct)).DeliveryMarkedAt);
    }

    // ---- KK6: the counters ----

    [Fact]
    public async Task TheCancelledUnitsGoBackToStock_AndTheCouponRightOnlyWithTheOrdersLastUnit_KK6()
    {
        Variant first = await SeedVariantAsync(ProductType.Physical, 5);
        Variant second = await SeedVariantAsync(ProductType.Service, null);
        Coupon coupon = await SeedCouponAsync();
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing, PaymentStatus.Paid, coupon, new Line(ProductType.Physical, 2, first.Id, CouponShare: 20m), new Line(ProductType.Service, 1, second.Id, CouponShare: 10m));
        await ExecuteAsync($"UPDATE coupons SET used_count = 1 WHERE id = '{coupon.Id}'");

        Assert.False((await CancelAsync(order.Id, items[0].Id, 2)).IsRejected);
        Assert.Equal((7, 0), await VariantAsync(first.Id));
        Assert.Equal((1, 0), await CouponAsync(coupon.Id));

        // An unlimited service capacity takes nothing back; the order's last unit gives the right back.
        Assert.False((await CancelAsync(order.Id, items[1].Id, 1)).IsRejected);
        Assert.Equal(((int?)null, 0), await VariantAsync(second.Id));
        Assert.Equal((0, 0), await CouponAsync(coupon.Id));
    }

    // ---- KK7: the coupon share by the units; the rest is kept ----

    [Fact]
    public async Task TheCouponShare_IsSetAsideByTheUnits_TheLastRecordTakesTheRest_AndNoMinimumBasketIsRead_KK7()
    {
        Coupon coupon = await SeedCouponAsync(minimumBasket: 1000m);
        (Order order, List<OrderItem> items) = await SeedAsync(
            OrderStatus.Preparing,
            PaymentStatus.Paid,
            coupon,
            new Line(ProductType.Physical, 3, UnitPrice: 300m, CouponShare: 100m),
            new Line(ProductType.Physical, 1, UnitPrice: 300m, CouponShare: 30m));

        OrderTransitionResult one = await CancelAsync(order.Id, items[0].Id, 1);
        OrderTransitionResult two = await CancelAsync(order.Id, items[0].Id, 1);
        OrderTransitionResult three = await CancelAsync(order.Id, items[0].Id, 1);

        await using AppDbContext reader = NewContext();
        List<Cancellation> records = await reader.Cancellations.Where(c => c.OrderItemId == items[0].Id).ToListAsync(Ct);
        Cancellation Of(OrderTransitionResult result) => records.Single(c => c.Id == result.ItemRecordId);
        Assert.Equal([33.33m, 33.33m, 33.34m], new[] { Of(one).CouponShare, Of(two).CouponShare, Of(three).CouponShare });
        Assert.Equal([266.67m, 266.67m, 266.66m], new[] { one, two, three }.Select(r => Of(r).ProductAmount - Of(r).CouponShare));
        Assert.Equal(items[0].CouponShare, records.Sum(c => c.CouponShare));
        Assert.Equal(items[0].VatAmount, records.Sum(c => c.VatAmount));

        // The order lives on its other item below the coupon's minimum basket: no share is worked out again, the frozen values stay.
        Assert.Equal(OrderStatus.Preparing, (await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        List<OrderItem> saved = await reader.OrderItems.Where(i => i.OrderId == order.Id).ToListAsync(Ct);
        Assert.Equal((100m, 30m), (saved.Single(i => i.Id == items[0].Id).CouponShare, saved.Single(i => i.Id == items[1].Id).CouponShare));
    }

    // ---- KK8: the dated copy ----

    [Fact]
    public async Task EveryCancellation_QueuesOneB7_ToTheOrdersContact_AtTheMomentOfTheCancellation_KK8()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 3));
        _clock.Advance(TimeSpan.FromMinutes(45));
        DateTimeOffset at = _clock.GetUtcNow();

        Assert.False((await CancelAsync(order.Id, items[0].Id, 2)).IsRejected);

        await using AppDbContext reader = NewContext();
        DispatchRecord b7 = await reader.DispatchRecords.SingleAsync(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B7, Ct);
        Assert.Equal(at, b7.CreatedAt);
        Assert.Equal(at, (await reader.Cancellations.SingleAsync(c => c.OrderItemId == items[0].Id, Ct)).CreatedAt);
    }

    // ---- the units' field rule ----

    [Theory]
    [InlineData(0, TransitionRefusal.InvalidInput)]
    [InlineData(-1, TransitionRefusal.InvalidInput)]
    [InlineData(3, TransitionRefusal.InvalidInput)]
    public async Task UnitsUnderOneOrAboveTheOpenUnits_AreAWrongValue_AndNothingIsWritten(int quantity, TransitionRefusal expected)
    {
        Variant variant = await SeedVariantAsync(ProductType.Physical, 5);
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 2, variant.Id));

        OrderTransitionResult result = await CancelAsync(order.Id, items[0].Id, quantity);

        Assert.Equal(expected, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Preparing, 1);
        Assert.Equal((5, 0), await VariantAsync(variant.Id));
    }

    [Fact]
    public async Task UnitsAboveWhatEarlierCancellationsLeftOpen_AreAWrongValue()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 3), new Line(ProductType.Physical, 1));
        Assert.False((await CancelAsync(order.Id, items[0].Id, 2)).IsRejected);

        OrderTransitionResult result = await CancelAsync(order.Id, items[0].Id, 2);

        Assert.Equal(TransitionRefusal.InvalidInput, result.Refusal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(1, await reader.Cancellations.CountAsync(c => c.OrderItemId == items[0].Id, Ct));
    }

    [Fact]
    public async Task AnItemOfAnotherOrder_IsAWrongValue()
    {
        (Order order, _) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 1));
        (_, List<OrderItem> otherItems) = await SeedAsync(OrderStatus.Preparing, PaymentStatus.Paid, null, new Line(ProductType.Physical, 1));

        OrderTransitionResult result = await CancelAsync(order.Id, otherItems[0].Id, 1);

        Assert.Equal(TransitionRefusal.InvalidInput, result.Refusal);
        await AssertNothingWrittenAsync(order.Id, OrderStatus.Preparing, 1);
    }

    // ---- helpers ----

    private Task<OrderTransitionResult> CancelAsync(Guid orderId, Guid itemId, int quantity) =>
        RunAsync((unit, token) => unit.CancelItemAsync(new CancelItemRequest(orderId, itemId, quantity), token));

    /// <summary>A refused cancellation writes nothing: the status and the version stay, no record, no transition and no B-7.</summary>
    private async Task AssertNothingWrittenAsync(Guid orderId, OrderStatus status, int version)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((status, version), (saved.OrderStatus, saved.Version));
        List<Guid> itemIds = await reader.OrderItems.Where(i => i.OrderId == orderId).Select(i => i.Id).ToListAsync(Ct);
        Assert.False(await reader.Cancellations.AnyAsync(c => itemIds.Contains(c.OrderItemId) && c.ActorKind == ActorKind.Buyer, Ct));
        Assert.False(await reader.TransitionRecords.AnyAsync(t => t.OrderId == orderId, Ct));
        Assert.Equal(0, await CountB7Async(orderId));
    }

    private async Task<int> CountB7Async(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.CountAsync(d => d.OrderId == orderId && d.DispatchType == DispatchType.B7, Ct);
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<(int? Stock, int Reserved)> VariantAsync(Guid variantId)
    {
        await using AppDbContext reader = NewContext();
        Variant variant = await reader.Variants.SingleAsync(v => v.Id == variantId, Ct);
        return (variant.StockQuantity, variant.ReservedQuantity);
    }

    private async Task<(int Used, int Reserved)> CouponAsync(Guid couponId)
    {
        await using AppDbContext reader = NewContext();
        Coupon coupon = await reader.Coupons.SingleAsync(c => c.Id == couponId, Ct);
        return (coupon.UsedCount, coupon.ReservedCount);
    }

    private async Task<Variant> SeedVariantAsync(ProductType type, int? stock)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create("Ürün", $"iptal-urun-{n}", type, 20m, Start);
        if (type == ProductType.Physical)
        {
            product.WithSeededOrigin();
        }

        Variant variant = Variant.Create(product.Id, $"IPT-{n}", 10m, Start, stock);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        return variant;
    }

    private async Task<Coupon> SeedCouponAsync(decimal minimumBasket = 100m)
    {
        int n = Interlocked.Increment(ref _counter);
        Coupon coupon = Coupon.CreateAmount($"IPTAL{n}", 50m, minimumBasket, Today.AddDays(-7), Today.AddDays(30), 3, Start);
        await SaveAsync(c => c.Coupons.Add(coupon));
        return coupon;
    }

    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(OrderStatus orderStatus, PaymentStatus paymentStatus, Coupon? coupon, params Line[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n % 100:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        Order order = Order.Create(
            $"2026-{n + 367000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m, Start.AddHours(1), Start);
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
        if (coupon is not null)
        {
            order.CouponId = coupon.Id;
            order.CouponCode = coupon.Code;
        }

        List<OrderItem> items = [];
        foreach (Line line in lines)
        {
            OrderItem item = OrderItem.Create(order.Id, null, line.VariantId, line.Type, "Ürün", line.Quantity, line.UnitPrice, 20m, 16.67m, Start);
            item.CouponShare = line.CouponShare;
            if (line.Type == ProductType.Digital)
            {
                item.DownloadAllowance = 5;
            }

            if (line.Type == ProductType.Service)
            {
                item.FulfilmentPeriod = 3;
            }

            if (line.Delivered)
            {
                item.DeliveryMarkedAt = Start;
                item.DeliveryDate = Today;
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

        // The states are set around the entity (it takes them only through the unit); the pair rule of the database still applies.
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        return (order, items);
    }

    private ServiceProvider BuildProvider()
    {
        ServiceCollection services = new();
        services.AddDbContext<AppDbContext>(options => AppDbContext.Configure(options, _collection.Dsn));
        services.AddSingleton(NpgsqlDataSource.Create(_collection.Dsn));
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
        services.AddLogging(builder => builder.SetMinimumLevel(LogLevel.None));
        services.AddCatalogModule();
        services.AddAfterSalesModule();
        services.AddNoticeDependencies();
        services.AddPaymentModule();
        services.AddOrderingModule();
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

    /// <summary>One item of a seeded order: its type, units, variant, whether it carries the delivery mark, its unit price and its frozen coupon share.</summary>
    public sealed record Line(ProductType Type, int Quantity, Guid? VariantId = null, bool Delivered = false, decimal UnitPrice = 100m, decimal CouponShare = 0m);
}
