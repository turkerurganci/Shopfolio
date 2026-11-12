using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.IntegrationTests.CorporateContent;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Ordering;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

using ItemAmounts = Shopfolio.Modules.Catalog.Contracts.Pricing.ItemAmounts;
using RecordAmounts = Shopfolio.Modules.AfterSales.ItemAmounts;

namespace Shopfolio.IntegrationTests.Administration;

/// <summary>The collection of the sales summary's tests (T383): one database copied from the template.</summary>
[CollectionDefinition(SalesSummaryDefinition.Name)]
public sealed class SalesSummaryDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "SalesSummary";
}

/// <summary>
/// The sales summary on a real PostgreSQL (T383 KK2-KK8, KK10; 02 10.6.2, 10.6.3, H-2): the orders, transition records, item records and contact requests are written as the
/// operations leave them, and the summary is read through the modules' surfaces - the order facts from Ordering, the item records from AfterSales and the contact count from the
/// real contact request store. The pure rules and the surface are the unit tests' (SalesSummaryRulesTests).
/// </summary>
[Collection(SalesSummaryDefinition.Name)]
public sealed class SalesSummaryTests : IAsyncLifetime
{
    private static readonly DateOnly Day = new(2026, 11, 10);
    private static readonly DateTimeOffset DayStart = TurkeyTime.StartOf(Day);
    private static readonly DateTimeOffset Now = DayStart.AddDays(6);
    private static readonly DateTimeOffset LongAgo = DayStart.AddDays(-90);
    private const string Iban = "TR330006100519786457841326";
    private static readonly RecordAmounts Amounts = new(250m, 0m, 50m);

    private static int _versionNumber = 5000;

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;
    private Guid _province;
    private CompanyIdentity? _identity;
    private Guid _shirt;
    private Guid _lamp;
    private Guid _service;

    public SalesSummaryTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE not_returned_closures, return_rejections, withdrawals, late_delivery_terminations, cancellations, item_removals, contact_requests, transition_records, "
            + "checkbox_acceptance_order_items, checkbox_acceptances, order_email_histories, order_address_histories, order_items, orders, price_histories, variants, option_values, "
            + "options, products, admins CASCADE");
        _province = await ProvinceAsync();
        _identity = await SetCompanyAsync();
        _shirt = await SeedVariantAsync(300m);
        _lamp = await SeedVariantAsync(200m);
        _service = await SeedVariantAsync(150m, ProductType.Service);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK2, KK3, KK4, KK10: approved orders of their birth day; the approved total; the peak day's daily number ----

    [Fact]
    public async Task TheCountAndTheRevenue_AreTheApprovedOrdersBornInThePeriod_RefundsNotSubtracted_KK2_KK3_KK4_KK10()
    {
        Guid paid = await SaveOrderAsync(DayStart.AddHours(2), PaymentMethod.Card, null, null, (_shirt, 2));
        Guid partly = await SaveOrderAsync(DayStart.AddHours(3), PaymentMethod.Card, null, null, (_lamp, 1));
        Guid refunded = await SaveOrderAsync(DayStart.AddHours(4), PaymentMethod.Card, null, null, (_shirt, 1));
        Guid transfer = await SaveOrderAsync(DayStart.AddHours(5), PaymentMethod.BankTransfer, null, null, (_lamp, 3));
        Guid failed = await SaveOrderAsync(DayStart.AddHours(6), PaymentMethod.Card, null, null, (_shirt, 5));
        Guid pending = await SaveOrderAsync(DayStart.AddHours(7), PaymentMethod.BankTransfer, null, null, (_shirt, 5));
        Guid yesterday = await SaveOrderAsync(DayStart.AddHours(-1), PaymentMethod.Card, null, null, (_shirt, 1));
        await SetAxesAsync(paid, OrderStatus.Placed, PaymentStatus.Paid);
        await SetAxesAsync(partly, OrderStatus.Placed, PaymentStatus.PartiallyRefunded);
        await SetAxesAsync(refunded, OrderStatus.Cancelled, PaymentStatus.Refunded);
        await SetAxesAsync(failed, OrderStatus.Cancelled, PaymentStatus.Failed);
        await SetAxesAsync(yesterday, OrderStatus.Placed, PaymentStatus.Paid);

        // The transfer is paid two days later, from the day before's order the money came in on this day: each order stays on its birth day (K-474).
        await SetAxesAsync(transfer, OrderStatus.Placed, PaymentStatus.Paid);
        await SaveAsync(c => c.TransitionRecords.AddRange(
            Record(transfer, TransitionCode.O1, DayStart.AddDays(2)),
            Record(yesterday, TransitionCode.O1, DayStart.AddHours(1))));

        SalesSummary summary = await SummaryAsync(SalesSummaryPeriod.OfDay(Day));
        decimal[] totals = await TotalsAsync(paid, partly, refunded, transfer);

        Assert.Equal(4, summary.OrderCount);
        Assert.Equal(totals.Sum(), summary.Revenue);
        Assert.Equal(
            [(PaymentMethod.BankTransfer, 1), (PaymentMethod.Card, 3)],
            summary.PaymentMethods.OrderBy(m => m.Method.ToString(), StringComparer.Ordinal).Select(m => (m.Method, m.OrderCount)));
        Assert.Equal([(4, 800m), (3, 900m)], summary.TopSellers.Select(t => (t.Quantity, t.Amount)));
        Assert.Equal(1, (await SummaryAsync(SalesSummaryPeriod.OfDay(Day.AddDays(-1)))).OrderCount);
        Assert.Equal(0, (await SummaryAsync(SalesSummaryPeriod.OfDay(Day.AddDays(2)))).OrderCount);
        Assert.Contains(pending, await OrderIdsAsync());

        // KK10: the one-day summary is the daily number the peak day trigger reads.
        Assert.Equal(summary.OrderCount, await DailyCountAsync(Day));
        Assert.Equal(1, await DailyCountAsync(Day.AddDays(-1)));
    }

    // ---- KK5: the orders of one cart are one attempt; another cart's order is its own attempt; an open window waits ----

    [Fact]
    public async Task ThePaymentCompletionRatio_CountsACartsOrdersAsOneAttempt_AndWaitsForAnOpenWindow_KK5()
    {
        Guid cart = Guid.NewGuid();
        Guid waitingCart = Guid.NewGuid();
        Guid firstTry = await SaveOrderAsync(DayStart.AddHours(1), PaymentMethod.Card, cart, null, (_shirt, 1));
        Guid secondTry = await SaveOrderAsync(DayStart.AddHours(2), PaymentMethod.Card, cart, null, (_shirt, 1));
        Guid otherCart = await SaveOrderAsync(DayStart.AddHours(3), PaymentMethod.Card, null, null, (_lamp, 1));
        Guid waiting = await SaveOrderAsync(DayStart.AddHours(4), PaymentMethod.BankTransfer, waitingCart, null, (_lamp, 1));
        await SetAxesAsync(firstTry, OrderStatus.Cancelled, PaymentStatus.Failed);
        await SetAxesAsync(secondTry, OrderStatus.Placed, PaymentStatus.Paid);
        await SetAxesAsync(otherCart, OrderStatus.Cancelled, PaymentStatus.Failed);
        await ExecuteAsync($"UPDATE orders SET payment_window_ends_at = '{Now.AddDays(1):O}' WHERE id = '{waiting}'");

        SalesSummary summary = await SummaryAsync(SalesSummaryPeriod.OfDay(Day));

        Assert.Equal(new SummaryRatio(1, 2), summary.PaymentCompletion);

        // When the window has run out the waiting cart enters as an attempt that did not end with a payment.
        await ExecuteAsync($"UPDATE orders SET payment_window_ends_at = '{Now.AddDays(-1):O}' WHERE id = '{waiting}'");
        Assert.Equal(new SummaryRatio(1, 3), (await SummaryAsync(SalesSummaryPeriod.OfDay(Day))).PaymentCompletion);
    }

    // ---- KK6: the KVKK request and "about an order" are not counted ----

    [Fact]
    public async Task TheContactRequestCount_TakesGeneralProductAndOther_Only_KK6()
    {
        await SaveAsync(c =>
        {
            foreach (ContactTopic topic in Enum.GetValues<ContactTopic>())
            {
                c.ContactRequests.Add(ContactRequest.Create("Ayşe", "ayse@example.com", topic, "Merhaba", DayStart.AddHours(2)));
            }

            c.ContactRequests.Add(ContactRequest.Create("Ayşe", "ayse@example.com", ContactTopic.GeneralQuestion, "Dün", DayStart.AddHours(-2)));
        });

        SalesSummary summary = await SummaryAsync(SalesSummaryPeriod.OfDay(Day));

        Assert.Equal(ContactRequests.SummaryTopics.Length, summary.ContactRequestCount);
        Assert.Equal(3, summary.ContactRequestCount);
    }

    // ---- KK7: shipped late or cancelled after the promise ran out has not kept it; cancelled before it is out ----

    [Fact]
    public async Task TheShippingPromiseRatio_CountsTheLateAsNotKept_AndLeavesOutTheCancelledInTime_KK7()
    {
        // Same-day promises (0 business days) approved the day before run out at this day's 00:00.
        DateTimeOffset approvedAt = DayStart.AddHours(-10);
        Guid inTime = await SavePaidPhysicalAsync(approvedAt);
        Guid late = await SavePaidPhysicalAsync(approvedAt);
        Guid cancelledLate = await SavePaidPhysicalAsync(approvedAt);
        Guid cancelledInTime = await SavePaidPhysicalAsync(approvedAt);
        Guid correctedShipment = await SavePaidPhysicalAsync(approvedAt);
        Guid otherDay = await SavePaidPhysicalAsync(approvedAt.AddDays(-3));
        TransitionRecord takenBack = Record(correctedShipment, TransitionCode.S5, approvedAt.AddHours(1));
        await SaveAsync(c => c.TransitionRecords.AddRange(
            Record(inTime, TransitionCode.S5, approvedAt.AddHours(2)),
            Record(late, TransitionCode.S5, DayStart.AddDays(1)),
            Record(cancelledLate, TransitionCode.S4, DayStart.AddDays(1)),
            Record(cancelledInTime, TransitionCode.S3, approvedAt.AddHours(1)),
            takenBack,
            Correction(correctedShipment, takenBack.Id, approvedAt.AddHours(3)),
            Record(otherDay, TransitionCode.S5, DayStart.AddDays(1))));

        SalesSummary summary = await SummaryAsync(SalesSummaryPeriod.OfDay(Day));

        Assert.Equal(new SummaryRatio(1, 4), summary.ShippingPromiseCompliance);
        Assert.Equal(new SummaryRatio(0, 1), (await SummaryAsync(SalesSummaryPeriod.OfDay(Day.AddDays(-3)))).ShippingPromiseCompliance);
    }

    // ---- KK7 (K-535): the mixed order whose physical items were all cancelled or removed one by one before the promise ran out is not counted ----

    [Fact]
    public async Task AMixedOrderWhosePhysicalItemsWereCancelledAndRemovedInTime_IsLeftOut_TheOpenServiceItemDoesNotBringItIn_KK7()
    {
        DateTimeOffset approvedAt = DayStart.AddHours(-10);
        Guid mixed = await SavePaidPromiseAsync(approvedAt, (_shirt, 2), (_lamp, 1), (_service, 1));
        Dictionary<Guid, Guid> items = await ItemsOfAsync(mixed);
        Admin admin = await SaveAdminAsync();

        // The two shirts cancelled one at a time and the lamp removed by the firm, all before the promise ran out at 00:00; the service item stays open. No S3/S4 is written.
        await SaveAsync(c =>
        {
            c.Cancellations.Add(Cancellation.ByBuyer(items[_shirt], 1, Amounts, null, approvedAt.AddHours(1)));
            c.Cancellations.Add(Cancellation.ByBuyer(items[_shirt], 1, Amounts, null, approvedAt.AddHours(2)));
            c.ItemRemovals.Add(ItemRemoval.Create(items[_lamp], 1, admin.Id, admin.Name, Amounts, approvedAt.AddHours(3)));
        });

        SummaryRatio ratio = (await SummaryAsync(SalesSummaryPeriod.OfDay(Day))).ShippingPromiseCompliance;

        Assert.Equal(new SummaryRatio(0, 0), ratio);
        Assert.False(ratio.IsAssessable);
    }

    [Fact]
    public async Task APhysicalItemPartlyCancelledInTime_WhoseRestWasNotShippedInTime_IsCountedAsNotKept_KK7()
    {
        DateTimeOffset approvedAt = DayStart.AddHours(-10);
        Guid partly = await SavePaidPromiseAsync(approvedAt, (_shirt, 3), (_service, 1));
        Dictionary<Guid, Guid> items = await ItemsOfAsync(partly);

        // One shirt of three cancelled before the promise ran out; the other two are shipped the next day.
        await SaveAsync(c =>
        {
            c.Cancellations.Add(Cancellation.ByBuyer(items[_shirt], 1, Amounts, null, approvedAt.AddHours(1)));
            c.TransitionRecords.Add(Record(partly, TransitionCode.S5, DayStart.AddDays(1)));
        });

        Assert.Equal(new SummaryRatio(0, 1), (await SummaryAsync(SalesSummaryPeriod.OfDay(Day))).ShippingPromiseCompliance);
    }

    [Fact]
    public async Task PhysicalItemsCancelledAfterThePromiseRanOut_StayInTheDenominator_AsNotKept_KK7()
    {
        DateTimeOffset approvedAt = DayStart.AddHours(-10);
        Guid atTheEnd = await SavePaidPromiseAsync(approvedAt, (_shirt, 1), (_lamp, 1), (_service, 1));
        Guid afterTheEnd = await SavePaidPromiseAsync(approvedAt, (_shirt, 2), (_service, 1));
        Dictionary<Guid, Guid> items = await ItemsOfAsync(atTheEnd);
        Dictionary<Guid, Guid> laterItems = await ItemsOfAsync(afterTheEnd);
        Admin admin = await SaveAdminAsync();

        // The first order: the shirt cancelled one hour before the end, the lamp removed at the end itself - one physical unit was open when the promise ran out.
        // The second: both shirts cancelled the next morning, after the promise ran out.
        await SaveAsync(c =>
        {
            c.Cancellations.Add(Cancellation.ByBuyer(items[_shirt], 1, Amounts, null, DayStart.AddHours(-1)));
            c.ItemRemovals.Add(ItemRemoval.Create(items[_lamp], 1, admin.Id, admin.Name, Amounts, DayStart));
            c.Cancellations.Add(Cancellation.ByBuyer(laterItems[_shirt], 2, Amounts, null, DayStart.AddHours(9)));
        });

        Assert.Equal(new SummaryRatio(0, 2), (await SummaryAsync(SalesSummaryPeriod.OfDay(Day))).ShippingPromiseCompliance);
    }

    // ---- KK8: the termination on its notice's stamp, the withdrawal when declared; the closure and the rejection change nothing ----

    [Fact]
    public async Task TheCancellationAndReturnCount_IsByEventDay_AndTheClosureOrTheRejectionChangesNothing_KK8()
    {
        Guid withdrawn = await SaveOrderAsync(DayStart.AddHours(1), PaymentMethod.Card, null, null, (_shirt, 2));
        Guid terminated = await SaveOrderAsync(DayStart.AddHours(2), PaymentMethod.Card, null, null, (_shirt, 1));
        Guid undone = await SaveOrderAsync(DayStart.AddHours(3), PaymentMethod.Card, null, null, (_lamp, 1));
        Guid unpaid = await SaveOrderAsync(DayStart.AddHours(4), PaymentMethod.Card, null, null, (_lamp, 1));
        Guid untouched = await SaveOrderAsync(DayStart.AddHours(5), PaymentMethod.Card, null, null, (_lamp, 1));
        foreach (Guid order in new[] { withdrawn, terminated, undone, untouched })
        {
            await SetAxesAsync(order, OrderStatus.Placed, PaymentStatus.Paid);
        }

        await SetAxesAsync(unpaid, OrderStatus.Cancelled, PaymentStatus.Failed);
        Guid withdrawnItem = await ItemOfAsync(withdrawn);
        Guid terminatedItem = await ItemOfAsync(terminated);
        Guid undoneItem = await ItemOfAsync(undone);
        Guid unpaidItem = await ItemOfAsync(unpaid);
        RecordAmounts amounts = new(250m, 0m, 50m);

        // The withdrawal declared on day+1 (one unit of two); the termination's notice stamped day+2 but recorded on day+3; a firm cancellation undone by the correction of
        // its closing S4; and the cancellation of an order whose payment never came.
        Withdrawal withdrawal = Withdrawal.ByBuyer(withdrawnItem, 1, Day.AddDays(1), null, amounts, null, DayStart.AddDays(1).AddHours(9));
        TransitionRecord closing = Record(undone, TransitionCode.S4, DayStart.AddDays(1));
        Cancellation cancellation = Cancellation.ByBuyer(undoneItem, 1, amounts, null, DayStart.AddDays(1));
        cancellation.TransitionRecordId = closing.Id;
        await SaveAsync(c =>
        {
            c.Withdrawals.Add(withdrawal);
            c.LateDeliveryTerminations.Add(LateDeliveryTermination.ByBuyer(terminatedItem, 1, Day.AddDays(2), amounts, null, DayStart.AddDays(3).AddHours(9)));
            c.TransitionRecords.Add(closing);
            c.Cancellations.Add(cancellation);
            c.Cancellations.Add(Cancellation.ByBuyer(unpaidItem, 1, amounts, null, DayStart.AddDays(1)));
        });
        await SaveAsync(c => c.TransitionRecords.Add(Correction(undone, closing.Id, DayStart.AddDays(1).AddHours(2))));

        Assert.Equal(1, (await SummaryAsync(SalesSummaryPeriod.OfDay(Day.AddDays(1)))).CancellationAndReturnCount);
        Assert.Equal(1, (await SummaryAsync(SalesSummaryPeriod.OfDay(Day.AddDays(2)))).CancellationAndReturnCount);
        Assert.Equal(0, (await SummaryAsync(SalesSummaryPeriod.OfDay(Day.AddDays(3)))).CancellationAndReturnCount);
        Assert.Equal(new SummaryRatio(2, 4), (await SummaryAsync(SalesSummaryPeriod.OfDay(Day))).CancellationAndReturn);

        // The goods did not come back, and the returned goods were rejected: neither changes the count nor the ratio.
        Admin admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", LongAgo);
        await SaveAsync(c => c.Admins.Add(admin));
        await SaveAsync(c =>
        {
            c.NotReturnedClosures.Add(NotReturnedClosure.Create(withdrawnItem, withdrawal.Id, 1, admin.Id, admin.Name, DayStart.AddDays(4)));
            c.ReturnRejections.Add(ReturnRejection.Create(withdrawnItem, withdrawal.Id, 1, Day.AddDays(4), admin.Id, admin.Name, DayStart.AddDays(4)));
        });

        Assert.Equal(1, (await SummaryAsync(SalesSummaryPeriod.OfDay(Day.AddDays(1)))).CancellationAndReturnCount);
        Assert.Equal(0, (await SummaryAsync(SalesSummaryPeriod.OfDay(Day.AddDays(4)))).CancellationAndReturnCount);
        Assert.Equal(new SummaryRatio(2, 4), (await SummaryAsync(SalesSummaryPeriod.OfDay(Day))).CancellationAndReturn);
    }

    // ---- Harness ----

    private async Task<SalesSummary> SummaryAsync(SalesSummaryPeriod period)
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn, new FixedTime(Now));
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await NewSummary(scope.ServiceProvider).GetAsync(period, Ct);
    }

    private async Task<int> DailyCountAsync(DateOnly day)
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn, new FixedTime(Now));
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await NewSummary(scope.ServiceProvider).CountDailyOrdersAsync(day, Ct);
    }

    private static SalesSummaryService NewSummary(IServiceProvider services)
    {
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        TimeProvider time = services.GetRequiredService<TimeProvider>();
        ContactCount content = new(new ContactRequests(db, new UnusedNotifications(), time));
        return new SalesSummaryService(services.GetRequiredService<IOrderingModule>(), services.GetRequiredService<IAfterSalesModule>(), content, time);
    }

    private Task<Guid> SavePaidPhysicalAsync(DateTimeOffset approvedAt) => SavePaidPromiseAsync(approvedAt, (_shirt, 1));

    // A paid order with a same-day promise (0 business days) approved at approvedAt.
    private async Task<Guid> SavePaidPromiseAsync(DateTimeOffset approvedAt, params (Guid VariantId, int Quantity)[] lines)
    {
        Guid order = await SaveOrderAsync(approvedAt.AddMinutes(-5), PaymentMethod.Card, null, 0, lines);
        await SetAxesAsync(order, OrderStatus.Placed, PaymentStatus.Paid);
        await SaveAsync(c => c.TransitionRecords.Add(Record(order, TransitionCode.O1, approvedAt)));
        return order;
    }

    private async Task<Dictionary<Guid, Guid>> ItemsOfAsync(Guid orderId)
    {
        await using AppDbContext db = NewContext();
        return await db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).ToDictionaryAsync(i => i.VariantId!.Value, i => i.Id, Ct);
    }

    private async Task<Admin> SaveAdminAsync()
    {
        Admin admin = Admin.Create("Firma Yöneticisi", $"yonetici{Interlocked.Increment(ref _counter)}@example.com", "digest-placeholder", LongAgo);
        await SaveAsync(c => c.Admins.Add(admin));
        return admin;
    }

    private async Task<Guid> SaveOrderAsync(DateTimeOffset createdAt, PaymentMethod method, Guid? cartId, int? shippingPeriod, params (Guid VariantId, int Quantity)[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        List<OrderItemSource> sources = [];
        foreach ((Guid variantId, _) in lines)
        {
            sources.Add(await SourceAsync(variantId));
        }

        List<ItemAmounts> amounts = [.. lines.Select((l, i) => ItemPricing.Price(new ItemPriceInput(sources[i].UnitPrice, l.Quantity, 0m, sources[i].VatRate)))];
        Order order = Order.Create(
            $"2026-S{n:D5}{Guid.NewGuid().ToString("N")[..4]}", $"access-{Guid.NewGuid():N}", $"confirm-{Guid.NewGuid():N}", method, true, "ali@example.com",
            ItemPricing.SumOrderTotal(amounts, 0m), 0m, createdAt.AddHours(1), createdAt);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = cartId ?? Guid.NewGuid();
        order.FreezeAddresses(("Ali Veli", "5321234567", _province, "Çankaya", "Atatürk Bulvarı 1"), ("Ali Veli", _province, "Çankaya", "Atatürk Bulvarı 1"));
        OrderFreeze.FreezeLegalIdentity(order, _identity!, new LegalTextVersionIds(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), method == PaymentMethod.BankTransfer ? Iban : null);
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        order.FreezeShippingFeeVatBreakdown([]);
        order.FreezeShippingPromise(shippingPeriod ?? 2);
        List<OrderItem> items = [.. lines.Select((l, i) => OrderFreeze.FreezeItem(order.Id, sources[i], l.Quantity, amounts[i], 3, order.CreatedAt))];
        await SaveWithVersionsAsync(order, items);
        return order.Id;
    }

    private async Task SaveWithVersionsAsync(Order order, List<OrderItem> items)
    {
        int n = Interlocked.Increment(ref _versionNumber);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, LongAgo, "metin", LongAgo);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, LongAgo, "metin", LongAgo);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, LongAgo, "metin", LongAgo).WithHosting();
        order.PreInformationFormVersionId = form.Id;
        order.DistanceSalesContractVersionId = contract.Id;
        order.PrivacyNoticeVersionId = notice.Id;
        await SaveAsync(c =>
        {
            c.LegalTextVersions.AddRange(form, contract, notice);
            c.Orders.Add(order);
            c.OrderItems.AddRange(items);
        });
    }

    private async Task<Guid> SeedVariantAsync(decimal price, ProductType type = ProductType.Physical)
    {
        int n = Interlocked.Increment(ref _counter);
        Product product = Product.Create($"Özet Ürün {n}", $"ozet-urun-{n}", type, 20m, LongAgo);
        if (type == ProductType.Physical)
        {
            product.WithSeededOrigin();
        }
        else
        {
            product.FulfilmentPeriod = 3;
        }

        product.PublishStatus = PublishStatus.Published;
        Variant variant = Variant.Create(product.Id, $"OZT-{n}", price, LongAgo, 100);
        variant.PublishStatus = PublishStatus.Published;
        variant.FirstOnSaleAt = LongAgo;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
            c.PriceHistories.Add(PriceHistory.Create(variant.Id, price, LongAgo, LongAgo));
        });
        return variant.Id;
    }

    private async Task<OrderItemSource> SourceAsync(Guid variantId)
    {
        await using AppDbContext db = NewContext();
        TimeProvider time = new FixedTime(LongAgo.AddDays(1));
        return Assert.Single(await new OrderItemSourceReader(db, time, new ProductGallery(db, time), new DigitalFileReader(db)).ListAsync([variantId], Ct));
    }

    private async Task<Guid> ProvinceAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Provinces.AsNoTracking().OrderBy(p => p.Code).Select(p => p.Id).FirstAsync(Ct);
    }

    private async Task<CompanyIdentity> SetCompanyAsync()
    {
        await ExecuteAsync(
            $"UPDATE companies SET company_type = 'Tradesman', legal_name = 'Örnek Ticaret', tax_id_number = '1111111111', province_id = '{_province}', district = 'Çankaya', "
            + "address_line = 'Kızılay 2', phone = '3121234567', email = 'firma@example.com', kep_address = 'firma@hs01.kep.tr'");
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetCompanyIdentityAsync(Ct);
    }

    private static TransitionRecord Record(Guid orderId, TransitionCode code, DateTimeOffset at)
        => TransitionRecord.Create(orderId, code, ActorKind.System, null, null, null, null, null, null, null, at);

    private static TransitionRecord Correction(Guid orderId, Guid corrected, DateTimeOffset at)
        => TransitionRecord.Create(orderId, TransitionCode.Correction, ActorKind.Admin, null, null, null, null, CorrectionReason.WrongOrder, null, corrected, at);

    private Task SetAxesAsync(Guid orderId, OrderStatus orderStatus, PaymentStatus paymentStatus) =>
        ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{orderId}'");

    private async Task<decimal[]> TotalsAsync(params Guid[] orderIds)
    {
        await using AppDbContext db = NewContext();
        return await db.Orders.AsNoTracking().Where(o => orderIds.Contains(o.Id)).Select(o => o.Total).ToArrayAsync(Ct);
    }

    private async Task<Guid[]> OrderIdsAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Orders.AsNoTracking().Select(o => o.Id).ToArrayAsync(Ct);
    }

    private async Task<Guid> ItemOfAsync(Guid orderId)
    {
        await using AppDbContext db = NewContext();
        return await db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).Select(i => i.Id).SingleAsync(Ct);
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

    private sealed class FixedTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    /// <summary>The CorporateContent surface with only the summary's count, on the real contact request store; every other member fails if it is called.</summary>
    private sealed class ContactCount(ContactRequests requests) : ICorporateContentModule
    {
        public Task<int> CountSummaryContactRequestsAsync(DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken ct) => requests.CountForSummaryAsync(periodStart, periodEnd, ct);

        public Task<int> CountOpenContactRequestsAsync(CancellationToken ct) => requests.CountOpenAsync(ct);

        public Task<int> CountPanelContactRequestsAsync(IReadOnlyCollection<ContactRequestStatus> statuses, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<PanelContactRequestRow>> ListPanelContactRequestsAsync(IReadOnlyCollection<ContactRequestStatus> statuses, int take, CancellationToken ct) => throw new NotSupportedException();

        public Task<ContactRequestDetail?> FindContactRequestAsync(Guid requestId, CancellationToken ct) => throw new NotSupportedException();

        public Task<ContactRequestChangeResult> CloseContactRequestAsync(Guid requestId, Guid adminId, CancellationToken ct) => throw new NotSupportedException();

        public Task<ContactRequestChangeResult> ReopenContactRequestAsync(Guid requestId, Guid adminId, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<string>> ListSitemapPathsAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<StorefrontMenu> GetMenuAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<HomepageView> GetHomepageAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<AnnouncementView?> GetAnnouncementAsync(bool asAdministrator, CancellationToken ct) => throw new NotSupportedException();

        public Task<BrandingSection> GetBrandingAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<BrandingSaveResult> SaveBrandingAsync(BrandingInput input, int formVersion, CancellationToken ct) => throw new NotSupportedException();

        public Task<BrandIdentity> GetBrandIdentityAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<ContentListView> GetContentListAsync(ContentListKind kind, int page, CancellationToken ct) => throw new NotSupportedException();

        public Task<ContentRecordPage?> GetContentPageAsync(ContentPageKind kind, string? slug, bool asAdministrator, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<SocialLinkItem>> ListSocialLinksAsync(CancellationToken ct) => throw new NotSupportedException();

        public Task<ContactFormView> GetContactFormAsync(string? memberName, string? memberEmail, CancellationToken ct) => throw new NotSupportedException();

        public Task<ContactFormResult> SendContactRequestAsync(ContactFormInput input, IPAddress? ipAddress, CancellationToken ct) => throw new NotSupportedException();

        public Task<int> WriteContactRequestExportAsync(Stream destination, long maxBytes, CancellationToken ct) => throw new NotSupportedException();
    }
}
