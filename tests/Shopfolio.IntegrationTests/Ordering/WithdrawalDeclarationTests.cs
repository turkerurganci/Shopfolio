using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;

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
/// The withdrawal declaration on a real PostgreSQL (T372 criteria 1-9, KK1-KK9; 02 7.3.5-7.3.7, 5.6.6, 8.3.8, 9.2, 10.4.10; 03 1.11.5, 1.11.6, 1.11.34): the customer's
/// declaration from the order page and the firm's record of a notice from another channel through the transition unit's open face in the transaction runner - the
/// date stamp, successive declarations, the return address frozen on a physical item, the screen's notes, the axes the declaration leaves alone and the closure rule
/// on the last open item, B-9 / F-3 / B-14, the notice's date and warnings, the IBAN carried over only from the order's e-mail, the physical item not shipped yet that
/// has no withdrawal but the firm's "müşteriyle anlaşıldı" cancellation, and the confirmation without which nothing is written.
/// </summary>
[Collection(OrderTransitionsDefinition.Name)]
public sealed partial class WithdrawalDeclarationTests : IAsyncLifetime
{
    private const string CompanyEmail = "firma-iletisim@example.com";

    private const string ValidIban = "TR330006100519786457841326";

    // Monday 9 November 2026, 12:00 Turkey time.
    private static readonly DateTimeOffset Start = new(2026, 11, 9, 9, 0, 0, TimeSpan.Zero);

    private static readonly DateOnly Today = TurkeyTime.DateOf(Start);

    private static readonly DateOnly DeliveryDay = Today.AddDays(-5);

    private readonly CollectionDatabaseFixture _collection;

    private readonly ManualTimeProvider _clock = new(Start);

    private Admin? _admin;

    private Province? _returnProvince;

    private int _counter;

    public WithdrawalDeclarationTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private TransitionAdmin AdminCaller => new(_admin!.Id, _admin.Name);

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE dispatch_records, jobs, refund_iban_requests, refund_ibans, refund_records, refunds, payment_records, not_returned_closures, returns, "
            + "return_rejections, withdrawals, late_delivery_terminations, cancellations, internal_notes, transition_records, stock_reservations, order_items, orders, "
            + "admins, company_settings, legal_text_versions, provinces CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", Start);
        _returnProvince = Province.Create("99", "İade İli", Start);

        CompanySettings settings = CompanySettings.Create(Start);
        settings.BankTransferPaymentWindow = 1;
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
        _clock.SetUtcNow(Start);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK2, KK5: the customer's declaration ----

    [Fact]
    public async Task TheCustomersDeclarations_AreStampedRecordsOfTheirOwn_WithTheReturnAddressFrozen_AndB9AndF3_KK1_KK2_KK5()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 3, true));
        OrderItem item = items[0];

        WithdrawalResult first = await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, item.Id, 1, true), t));
        Assert.Null(first.Refusal);
        Assert.Null(first.Closure);

        // The firm changes its return address: the next declaration copies the new one, the first keeps what it was shown (K-665).
        await ExecuteAsync("UPDATE company_settings SET return_address_line = 'Yeni Depo Sokak 9'");
        _clock.SetUtcNow(Start.AddHours(2));
        WithdrawalResult second = await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, item.Id, 2, true), t));
        Assert.Null(second.Refusal);

        await using AppDbContext reader = NewContext();
        List<Withdrawal> rows = await reader.Withdrawals.AsNoTracking().Where(w => w.OrderItemId == item.Id).OrderBy(w => w.CreatedAt).ToListAsync(Ct);
        Assert.Equal(2, rows.Count);
        Assert.Equal((first.WithdrawalId!.Value, 1, Today, ActorKind.Buyer, Start), (rows[0].Id, rows[0].Quantity, rows[0].DeclarationDate, rows[0].ActorKind, rows[0].CreatedAt));
        Assert.Equal((second.WithdrawalId!.Value, 2, Start.AddHours(2)), (rows[1].Id, rows[1].Quantity, rows[1].CreatedAt));
        Assert.Equal(("Örnek Ticaret İade", _returnProvince!.Id, "Yenimahalle", "Depo Sokak 3"), (rows[0].ReturnRecipientName, rows[0].ReturnProvinceId!.Value, rows[0].ReturnDistrict, rows[0].ReturnAddressLine));
        Assert.Equal("Yeni Depo Sokak 9", rows[1].ReturnAddressLine);

        // The amounts are split from the item's frozen values and the last open units take the rest (02 7.1.7).
        Assert.Equal((item.CouponShare, item.VatAmount), (rows.Sum(r => r.CouponShare), rows.Sum(r => r.VatAmount)));
        Assert.Equal(item.UnitPrice * 3, rows.Sum(r => r.ProductAmount));

        // The declaration changes no axis on a delivered order: no transition, the version raised once per declaration (KK4).
        await AssertOrderAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid, order.Version + 2);
        Assert.Empty(await reader.TransitionRecords.Where(t => t.OrderId == order.Id).ToListAsync(Ct));

        foreach (Withdrawal row in rows)
        {
            Assert.Equal([(DispatchType.B9, "ali@example.com"), (DispatchType.F3, CompanyEmail)], await DispatchesAsync(row.Id));
        }
    }

    [Fact]
    public void TheCustomersDeclaration_AsksOnlyTheItemTheUnitsTheConfirmationAndTheTransferIban_KK1()
    {
        // No form, e-mail, notary or other channel is asked (K-207).
        string[] fields = [.. typeof(DeclareWithdrawalRequest).GetProperties().Select(p => p.Name).Order(StringComparer.Ordinal)];
        Assert.Equal(["Confirmed", "Iban", "OrderId", "OrderItemId", "Quantity"], fields);
    }

    // ---- KK2, KK3, KK8: the declaration screen ----

    [Fact]
    public async Task TheScreen_ShowsTheAddressAndPeriodOnlyOnAPhysicalItem_TheCarrierNoteInTransit_AndThePackagingConditionOnlyWhenConditional_KK2_KK3_KK8()
    {
        (Order transit, List<OrderItem> transitItems) = await SeedAsync(
            PaymentMethod.Card, OrderStatus.Shipped, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, false, WithdrawalExemptionReason.HygieneSensitive));
        WithdrawalScreen? onTheRoad = await ScreenAsync(transit.Id, transitItems[0].Id);
        Assert.NotNull(onTheRoad);
        Assert.Equal(("Depo Sokak 3", AfterSalesPeriodRules.ReturnShipmentPeriodDays, true), (onTheRoad.ReturnAddress!.AddressLine, onTheRoad.SendingPeriodDays!.Value, onTheRoad.CarrierReturnsUnreceivedGoods));
        Assert.Equal(WithdrawalExemptionReason.HygieneSensitive, onTheRoad.PackagingCondition);
        Assert.False(onTheRoad.AsksIban);

        (Order delivered, List<OrderItem> deliveredItems) = await SeedAsync(
            PaymentMethod.BankTransfer, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 2, true));
        WithdrawalScreen? arrived = await ScreenAsync(delivered.Id, deliveredItems[0].Id);
        Assert.NotNull(arrived);
        Assert.Equal((false, (WithdrawalExemptionReason?)null, true, 2), (arrived.CarrierReturnsUnreceivedGoods, arrived.PackagingCondition, arrived.AsksIban, arrived.OpenQuantity));

        (Order service, List<OrderItem> serviceItems) = await SeedAsync(PaymentMethod.Card, OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Service, 1, false));
        WithdrawalScreen? serviceScreen = await ScreenAsync(service.Id, serviceItems[0].Id);
        Assert.NotNull(serviceScreen);
        Assert.Equal(((ItemReturnAddress?)null, (int?)null, false, (WithdrawalExemptionReason?)null),
            (serviceScreen.ReturnAddress, serviceScreen.SendingPeriodDays, serviceScreen.CarrierReturnsUnreceivedGoods, serviceScreen.PackagingCondition));

        // A physical item not shipped yet has no withdrawal screen: its way is the cancellation (02 7.3.7).
        (Order preparing, List<OrderItem> preparingItems) = await SeedAsync(PaymentMethod.Card, OrderStatus.Preparing, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, false));
        Assert.Null(await ScreenAsync(preparing.Id, preparingItems[0].Id));
    }

    // ---- KK4: the axes and the closure rule ----

    [Fact]
    public async Task APhysicalItemWithdrawnInTransit_ClosesItsUnits_ButTheShippedOrderWaitsForTheGoods_KK4()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, OrderStatus.Shipped, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, false));

        WithdrawalResult result = await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, items[0].Id, 1, true), t));

        Assert.Null(result.Refusal);
        Assert.Null(result.Closure);
        Assert.Equal(1, await ClosedAsync(items[0].Id));
        await AssertOrderAsync(order.Id, OrderStatus.Shipped, PaymentStatus.Paid, order.Version + 1);
    }

    [Theory]
    [InlineData(OrderStatus.Placed, TransitionCode.S3)]
    [InlineData(OrderStatus.Preparing, TransitionCode.S4)]
    public async Task TheLastOpenServiceItemWithNothingDelivered_ClosesTheOrderToCancelled_ByTheClosureRule_KK4(OrderStatus status, TransitionCode expected)
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, status, PaymentStatus.Paid, new ItemSpec(ProductType.Service, 1, false));

        WithdrawalResult result = await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, items[0].Id, 1, true), t));

        Assert.Equal(expected, result.Closure);

        // The payment axis waits for the refund; the closure is the system's, with no cancellation record and no B-7 (02 5.6.6; K-672).
        await AssertOrderAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Paid, order.Version + 1);
        Assert.Equal(0, await CountAsync(c => c.Cancellations.Where(x => x.OrderItemId == items[0].Id)));
        Assert.Equal(0, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B7)));
    }

    [Fact]
    public async Task TheLastOpenServiceItemBesideADeliveredItem_ClosesTheOrderToDelivered_KK4()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(
            PaymentMethod.Card, OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Digital, 1, true), new ItemSpec(ProductType.Service, 1, false));

        WithdrawalResult result = await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, items[1].Id, 1, true), t));

        Assert.Equal(TransitionCode.S2, result.Closure);
        await AssertOrderAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid, order.Version + 1);
    }

    [Fact]
    public async Task AServiceWithdrawal_GivesTheWithdrawnCapacityBack_APhysicalOneLeavesTheStockToTheReceipt_D453()
    {
        // 03 1.4.4.6: the service capacity goes back with the declaration by the withdrawn units; physical goods go back only when the firm receives them (T373).
        (Order service, List<OrderItem> serviceItems) = await SeedAsync(PaymentMethod.Card, OrderStatus.Placed, PaymentStatus.Paid, new ItemSpec(ProductType.Service, 3, false));
        (Order physical, List<OrderItem> physicalItems) = await SeedAsync(PaymentMethod.Card, OrderStatus.Shipped, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 2, false));
        Guid capacity = await SeedVariantAsync(serviceItems[0].Id, ProductType.Service, 4);
        Guid stock = await SeedVariantAsync(physicalItems[0].Id, ProductType.Physical, 4);

        Assert.Null((await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(service.Id, serviceItems[0].Id, 2, true), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(physical.Id, physicalItems[0].Id, 1, true), t))).Refusal);

        await using AppDbContext reader = NewContext();
        Assert.Equal(4 + 2, (await reader.Variants.AsNoTracking().SingleAsync(v => v.Id == capacity, Ct)).StockQuantity);
        Assert.Equal(4, (await reader.Variants.AsNoTracking().SingleAsync(v => v.Id == stock, Ct)).StockQuantity);
    }

    // ---- KK5, KK6, KK7: the firm's record of another channel ----

    [Fact]
    public async Task TheFirmsRecord_TakesTheNoticesDate_PastButNeverFuture_AndOnlyB9Goes_KK5_KK6_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 2, true));
        Guid item = items[0].Id;

        WithdrawalResult future = await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, 1, 1, Today.AddDays(1)), t));
        Assert.Equal(TransitionRefusal.InvalidInput, future.Refusal);

        // Recorded two days after the notice reached the firm: the date is the notice's, the firm's check of the sender changes nothing (K-607).
        _clock.SetUtcNow(Start.AddDays(2));
        WithdrawalResult recorded = await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, 1, 1, Today), t));
        Assert.Null(recorded.Refusal);
        Assert.Equal(WithdrawalNoticeWarnings.None, recorded.Warnings);

        await using AppDbContext reader = NewContext();
        Withdrawal row = await reader.Withdrawals.AsNoTracking().SingleAsync(w => w.OrderItemId == item, Ct);
        Assert.Equal((Today, ActorKind.Admin, _admin!.Id, _admin.Name, (Guid?)null), (row.DeclarationDate, row.ActorKind, row.AdminId!.Value, row.AdminName, row.RefundIbanId));
        Assert.Equal([(DispatchType.B9, "ali@example.com")], await DispatchesAsync(row.Id));
        Assert.Equal(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item)));
        await AssertOrderAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid, order.Version + 1);
    }

    [Fact]
    public void TheFirmsRecord_HasNoFieldForTheProductToCheckTheSender_KK7()
    {
        // The firm's check that the notice is the order owner's is its own (K-607): the request carries no verification field.
        Assert.DoesNotContain(typeof(RecordWithdrawalRequest).GetProperties(), p => p.Name.Contains("Verif", StringComparison.Ordinal) || p.Name.Contains("Owner", StringComparison.Ordinal));
    }

    [Fact]
    public async Task ANoticeOutsideTheWindow_AsksTheFirmToConfirm_AndIsRecordedThen_KK6()
    {
        DateOnly delivered = Today.AddDays(-30);
        (Order order, List<OrderItem> items) = await SeedAsync(
            PaymentMethod.Card, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, true) { DeliveryDate = delivered });
        Guid item = items[0].Id;
        DateOnly noticeDate = Today.AddDays(-1);

        WithdrawalResult asked = await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, 1, 1, noticeDate), t));
        Assert.Equal(TransitionRefusal.ConfirmationRequired, asked.Refusal);
        Assert.True(asked.Warnings.OutsideWindow);
        Assert.False(asked.Warnings.BeyondOneYear);
        Assert.Equal(0, await CountAsync(c => c.Withdrawals.Where(w => w.OrderItemId == item)));

        WithdrawalResult confirmed = await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, 1, 1, noticeDate) with { WarningsConfirmed = true }, t));
        Assert.Null(confirmed.Refusal);
        Assert.Equal(noticeDate, (await SingleWithdrawalAsync(item)).DeclarationDate);
    }

    [Fact]
    public async Task ANoticeBeyondTheWindowsYear_AndOneOnAnAbsolutelyExemptItem_AreWarnedOf_KK6()
    {
        (Order old, List<OrderItem> oldItems) = await SeedAsync(
            PaymentMethod.Card, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, true) { DeliveryDate = Today.AddDays(-450) });
        WithdrawalResult beyond = await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(old.Id, oldItems[0].Id, 1, 1, Today), t));
        Assert.Equal(TransitionRefusal.ConfirmationRequired, beyond.Refusal);
        Assert.True(beyond.Warnings.BeyondOneYear && beyond.Warnings.OutsideWindow);

        (Order exempt, List<OrderItem> exemptItems) = await SeedAsync(
            PaymentMethod.Card, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, true, WithdrawalExemptionReason.MadeToOrder));
        WithdrawalResult absolute = await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(exempt.Id, exemptItems[0].Id, 1, 1, Today), t));
        Assert.Equal(TransitionRefusal.ConfirmationRequired, absolute.Refusal);
        Assert.True(absolute.Warnings.AbsoluteExemption);
        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(exempt.Id, exemptItems[0].Id, 1, 1, Today) with { WarningsConfirmed = true }, t))).Refusal);
    }

    [Fact]
    public async Task ANoticeOnTheDeliveryDay_AsksWhetherTheDeliveryCameFirst_AndKeepsTheAnswer_KK6()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, true));
        Guid item = items[0].Id;

        WithdrawalResult unanswered = await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, 1, 1, DeliveryDay), t));
        Assert.Equal(TransitionRefusal.InvalidInput, unanswered.Refusal);
        Assert.True(unanswered.Warnings.AsksDeliveryOrder);

        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, 1, 1, DeliveryDay) with { IsDeliveredBeforeDeclaration = false }, t))).Refusal);
        Assert.False((await SingleWithdrawalAsync(item)).IsDeliveredBeforeDeclaration);
    }

    [Fact]
    public async Task OnTheTransferLine_TheNoticesIbanIsCarriedOverOnlyFromTheOrdersEmail_OtherwiseARequestOpensAndB14Goes_KK7()
    {
        (Order fromEmail, List<OrderItem> fromEmailItems) = await SeedAsync(PaymentMethod.BankTransfer, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, true));
        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(fromEmail.Id, fromEmailItems[0].Id, 1, 1, Today) with { CameFromOrderEmail = true, NoticeIban = ValidIban }, t))).Refusal);
        Withdrawal carried = await SingleWithdrawalAsync(fromEmailItems[0].Id);
        await using (AppDbContext reader = NewContext())
        {
            RefundIban iban = await reader.RefundIbans.AsNoTracking().SingleAsync(i => i.OrderId == fromEmail.Id, Ct);
            Assert.Equal((iban.Id, ValidIban), (carried.RefundIbanId!.Value, iban.Iban));

            // After the delivery the IBAN waits for the goods and has its Z-38 deletion day (D-667).
            Assert.Equal(AfterSalesPeriodRules.ReturnIbanDeletionAt(Today), iban.ScheduledDeletionAt);
        }

        Assert.Equal(0, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == fromEmail.Id && d.DispatchType == DispatchType.B14)));

        (Order other, List<OrderItem> otherItems) = await SeedAsync(PaymentMethod.BankTransfer, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, true));
        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(other.Id, otherItems[0].Id, 1, 1, Today) with { CameFromOrderEmail = false, NoticeIban = ValidIban }, t))).Refusal);
        Assert.Null((await SingleWithdrawalAsync(otherItems[0].Id)).RefundIbanId);
        Assert.Equal(0, await CountAsync(c => c.RefundIbans.Where(i => i.OrderId == other.Id)));
        Assert.Equal(1, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == otherItems[0].Id && r.Reason == RefundIbanRequestReason.NoticeWithoutIban)));
        Assert.Equal(1, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == other.Id && d.DispatchType == DispatchType.B14)));

        // The card line takes no IBAN and opens no request: B-14 is the transfer line's only.
        (Order card, List<OrderItem> cardItems) = await SeedAsync(PaymentMethod.Card, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, true));
        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(card.Id, cardItems[0].Id, 1, 1, Today) with { CameFromOrderEmail = true, NoticeIban = ValidIban }, t))).Refusal);
        Assert.Equal(0, await CountAsync(c => c.RefundIbans.Where(i => i.OrderId == card.Id)));
        Assert.Equal(0, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == cardItems[0].Id)));
        Assert.Equal(0, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == card.Id && d.DispatchType == DispatchType.B14)));
    }

    [Fact]
    public async Task TheCustomerOnTheTransferLine_EntersTheIban_OrUsesTheOneOnFile_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.BankTransfer, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 2, true));
        Guid item = items[0].Id;

        WithdrawalResult missing = await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, item, 1, true), t));
        Assert.Equal(TransitionRefusal.InvalidInput, missing.Refusal);
        Assert.Equal(0, await CountAsync(c => c.Withdrawals.Where(w => w.OrderItemId == item)));

        Assert.Null((await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, item, 1, true, ValidIban), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, item, 1, true), t))).Refusal);

        await using AppDbContext reader = NewContext();
        RefundIban iban = await reader.RefundIbans.AsNoTracking().SingleAsync(i => i.OrderId == order.Id, Ct);
        Assert.All(await reader.Withdrawals.AsNoTracking().Where(w => w.OrderItemId == item).ToListAsync(Ct), w => Assert.Equal(iban.Id, w.RefundIbanId));
    }

    [Fact]
    public async Task TwoFirmRecordsWithoutAnIban_OnTheSameTransferItem_AreBothWritten_AndUseTheOneOpenRequest_KK1_KK6_KK7()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.BankTransfer, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 2, true));
        Guid item = items[0].Id;

        // The second notice of another unit is recorded too (02 10.4.10, 7.1.6): the item's IBAN field is already open, the record uses that request (06 4.29).
        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, order.Version, 1, Today), t))).Refusal);
        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, order.Version + 1, 1, Today), t))).Refusal);

        Assert.Equal(2, await CountAsync(c => c.Withdrawals.Where(w => w.OrderItemId == item && w.RefundIbanId == null)));
        Assert.Equal(1, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item && r.FulfilledAt == null && r.ClosedAt == null)));
        Assert.Equal(1, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item)));

        // B-14 goes with every record of an IBAN-less notice on the transfer line (02 9.2 B-14; 03 1.11.34).
        Assert.Equal(2, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B14)));
        await AssertOrderAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid, order.Version + 2);
    }

    [Fact]
    public async Task AnIbanFromTheOrdersEmail_ThatFailsTheCheck_IsNotCarried_TheRecordIsWithoutAnIban_ARequestOpensAndB14Goes_KK7_D798()
    {
        // The notice's IBAN passes the order page's format and checksum check (02 10.4.10, 8.4.2); a failing one leaves the record without an IBAN.
        const string failingChecksum = "TR330006100519786457841327";
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.BankTransfer, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, true));
        Guid item = items[0].Id;

        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, order.Version, 1, Today) with { CameFromOrderEmail = true, NoticeIban = failingChecksum }, t))).Refusal);

        Assert.Null((await SingleWithdrawalAsync(item)).RefundIbanId);
        Assert.Equal(0, await CountAsync(c => c.RefundIbans.Where(i => i.OrderId == order.Id)));
        Assert.Equal(1, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item && r.Reason == RefundIbanRequestReason.NoticeWithoutIban && r.FulfilledAt == null)));
        Assert.Equal(1, await CountAsync(c => c.DispatchRecords.Where(d => d.OrderId == order.Id && d.DispatchType == DispatchType.B14)));
    }

    [Fact]
    public async Task TheCustomersDeclarationWithAnIban_AfterTheFirmsRecordWithoutOne_FulfilsTheItemsOpenRequest_KK7_D797()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.BankTransfer, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 3, true));
        Guid item = items[0].Id;

        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, order.Version, 1, Today), t))).Refusal);
        _clock.SetUtcNow(Start.AddHours(2));
        Assert.Null((await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, item, 1, true, ValidIban), t))).Refusal);

        // The IBAN the customer entered with the declaration fills the item's field: the request is fulfilled and the item leaves "IBAN pending" (06 4.29; 02 7.4.5).
        await using (AppDbContext reader = NewContext())
        {
            RefundIban iban = await reader.RefundIbans.AsNoTracking().SingleAsync(i => i.OrderId == order.Id, Ct);
            RefundIbanRequest request = await reader.RefundIbanRequests.AsNoTracking().SingleAsync(r => r.OrderItemId == item, Ct);
            Assert.Equal((Start.AddHours(2), iban.Id, (DateTimeOffset?)null), (request.FulfilledAt!.Value, request.RefundIbanId!.Value, request.ClosedAt));
        }

        // A later record without an IBAN opens a new request: the fulfilled one is no longer open.
        Assert.Null((await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, order.Version + 2, 1, Today), t))).Refusal);
        Assert.Equal(1, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item && r.FulfilledAt == null && r.ClosedAt == null)));
        Assert.Equal(2, await CountAsync(c => c.RefundIbanRequests.Where(r => r.OrderItemId == item)));
    }

    // ---- KK8: a physical item not shipped yet ----

    [Fact]
    public async Task APhysicalItemNotShippedYet_TakesNoWithdrawal_AndTheNoticeIsTheFirmsAgreedCancellation_KK8()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, OrderStatus.Preparing, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, false));
        Guid item = items[0].Id;

        WithdrawalResult customer = await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, item, 1, true), t));
        WithdrawalResult firm = await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, 1, 1, Today), t));
        Assert.Equal((TransitionRefusal.ConditionNotMet, TransitionRefusal.ConditionNotMet), (customer.Refusal, firm.Refusal));
        Assert.Equal(0, await CountAsync(c => c.Withdrawals.Where(w => w.OrderItemId == item)));
        await AssertOrderAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, order.Version);

        // The firm cancels with "müşteriyle anlaşıldı (müşteri talebi)" (02 7.3.7; K-604).
        OrderTransitionResult cancelled = await RunAsync((u, t) => u.CancelOrderAsync(
            CancelOrderRequest.ByAdministrator(order.Id, AdminCaller, order.Version, true, CancellationReason.AgreedWithBuyer, ownerConfirmed: true), t));
        Assert.Null(cancelled.Refusal);
        await using AppDbContext reader = NewContext();
        Assert.Equal(OrderStatus.Cancelled, (await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == order.Id, Ct)).OrderStatus);
        Assert.Contains(await reader.TransitionRecords.AsNoTracking().Where(t => t.OrderId == order.Id).ToListAsync(Ct), t => t.CancellationReason == CancellationReason.AgreedWithBuyer);
    }

    // ---- KK9: confirmation, no undo, no correction ----

    [Fact]
    public async Task NeitherWayWritesWithoutTheConfirmation_KK9()
    {
        (Order order, List<OrderItem> items) = await SeedAsync(PaymentMethod.Card, OrderStatus.Delivered, PaymentStatus.Paid, new ItemSpec(ProductType.Physical, 1, true));
        Guid item = items[0].Id;

        WithdrawalResult customer = await RunAsync((u, t) => u.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(order.Id, item, 1, false), t));
        WithdrawalResult firm = await RunAsync((u, t) => u.RecordWithdrawalAsync(Record(order.Id, item, 1, 1, Today) with { Confirmed = false }, t));

        Assert.Equal((TransitionRefusal.ConfirmationRequired, TransitionRefusal.ConfirmationRequired), (customer.Refusal, firm.Refusal));
        Assert.Equal(0, await CountAsync(c => c.Withdrawals.Where(w => w.OrderItemId == item)));
        Assert.Empty(await DispatchesByOrderAsync(order.Id));
        await AssertOrderAsync(order.Id, OrderStatus.Delivered, PaymentStatus.Paid, order.Version);
    }

    [Fact]
    public void NoModuleSurface_UndoesOrCorrectsAWithdrawal_KK9()
    {
        // 02 7.6.7: the declaration and the firm's record are never undone or corrected - no open method changes or removes one.
        Type[] surfaces = [typeof(IOrderTransitions), typeof(IAfterSalesModule), typeof(IOrderOperations)];
        string[] changing = [.. surfaces.SelectMany(s => s.GetMethods()).Select(m => m.Name).Where(n => ChangingWithdrawal().IsMatch(n))];
        Assert.Empty(changing);
    }

    // ---- helpers ----

    [GeneratedRegex("^(Update|Change|Correct|Edit|Delete|Remove|Cancel|Undo|Revert|Reset)\\w*Withdrawal", RegexOptions.CultureInvariant)]
    private static partial Regex ChangingWithdrawal();

    private RecordWithdrawalRequest Record(Guid orderId, Guid itemId, int version, int quantity, DateOnly noticeDate) =>
        new(orderId, itemId, AdminCaller, version, true, quantity, noticeDate, false);

    private async Task<WithdrawalScreen?> ScreenAsync(Guid orderId, Guid itemId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IOrderOperations>().ReadWithdrawalScreenAsync(orderId, itemId, Ct);
    }

    private async Task<int> ClosedAsync(Guid itemId)
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IReadOnlyDictionary<Guid, int> closed = await scope.ServiceProvider.GetRequiredService<IAfterSalesModule>().ClosedQuantitiesAsync([itemId], Ct);
        return closed.GetValueOrDefault(itemId);
    }

    private async Task<Withdrawal> SingleWithdrawalAsync(Guid itemId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.Withdrawals.AsNoTracking().SingleAsync(w => w.OrderItemId == itemId, Ct);
    }

    private async Task<List<(DispatchType, string?)>> DispatchesAsync(Guid withdrawalId)
    {
        await using AppDbContext reader = NewContext();
        List<DispatchRecord> rows = await reader.DispatchRecords
            .Where(d => d.RelatedRecordType == DispatchRelatedRecordType.Withdrawal && d.RelatedRecordId == withdrawalId)
            .OrderBy(d => d.DispatchType).ToListAsync(Ct);
        return [.. rows.Select(d => (d.DispatchType, d.RecipientEmail?.ToLowerInvariant()))];
    }

    private async Task<List<DispatchType>> DispatchesByOrderAsync(Guid orderId)
    {
        await using AppDbContext reader = NewContext();
        return await reader.DispatchRecords.Where(d => d.OrderId == orderId).Select(d => d.DispatchType).ToListAsync(Ct);
    }

    private async Task<int> CountAsync<TRow>(Func<AppDbContext, IQueryable<TRow>> query)
    {
        await using AppDbContext reader = NewContext();
        return await query(reader).CountAsync(Ct);
    }

    private async Task AssertOrderAsync(Guid orderId, OrderStatus status, PaymentStatus payment, int version)
    {
        await using AppDbContext reader = NewContext();
        Order saved = await reader.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, Ct);
        Assert.Equal((status, payment, version), (saved.OrderStatus, saved.PaymentStatus, saved.Version));
    }

    private async Task<T> RunAsync<T>(Func<IOrderTransitions, CancellationToken, Task<T>> work)
        where T : IOperationResult
    {
        await using ServiceProvider provider = BuildProvider();
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IOrderTransitions unit = scope.ServiceProvider.GetRequiredService<IOrderTransitions>();
        return await scope.ServiceProvider.GetRequiredService<ITransactionRunner>().RunAsync(token => work(unit, token), Ct);
    }

    private async Task<(Order Order, List<OrderItem> Items)> SeedAsync(PaymentMethod method, OrderStatus orderStatus, PaymentStatus paymentStatus, params ItemSpec[] specs)
    {
        int n = Interlocked.Increment(ref _counter);
        Province province = Province.Create($"{n:D2}", $"İl {n}", Start);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, Start, "metin", Start);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, Start, "metin", Start);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, Start, "metin", Start).WithHosting();
        DateTimeOffset orderedAt = specs.Min(s => s.DeliveryDate) is { } earliest ? TurkeyTime.StartOf(earliest).AddDays(-2) : Start.AddDays(-7);
        Order order = Order.Create(
            $"2026-{n + 372000:D6}", $"access-placeholder-{n}", $"confirm-placeholder-{n}", method, true, "Ali@Example.com", 120m, 0m, orderedAt.AddHours(1), orderedAt);
        order.BankTransferIban = method == PaymentMethod.BankTransfer ? "TR320010009999901234567890" : null;
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.FreezeShippingPromise(2);
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
            OrderItem item = OrderItem.Create(order.Id, null, null, spec.Type, "Ürün", spec.Quantity, 300m, 20m, 50m * spec.Quantity, orderedAt);
            item.CouponShare = 10m * spec.Quantity;
            item.WithdrawalExemptionReason = spec.Exemption;
            item.DownloadAllowance = spec.Type == ProductType.Digital ? 5 : null;
            item.FulfilmentPeriod = spec.Type == ProductType.Service ? 3 : null;
            if (spec.Delivered)
            {
                DateOnly day = spec.DeliveryDate ?? DeliveryDay;
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

        // The states are set around the entity (it takes them only through the unit); the pair rule of the database still applies.
        await ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{order.Id}'");
        return (order, items);
    }

    /// <summary>A product and variant of <paramref name="type"/> with <paramref name="stock"/> left, set as the item's source (the order freezes no other value of it).</summary>
    private async Task<Guid> SeedVariantAsync(Guid itemId, ProductType type, int stock)
    {
        string key = EntityIds.New().ToString("N")[^12..];
        Product product = Product.Create("Kaynak", $"cayma-{key}", type, 20m, Start);
        if (type == ProductType.Physical)
        {
            product.WithSeededOrigin();
        }

        Variant variant = Variant.Create(product.Id, $"CAY-{key}", 300m, Start, stock);
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
        });
        await ExecuteAsync($"UPDATE order_items SET product_id = '{product.Id}', variant_id = '{variant.Id}' WHERE id = '{itemId}'");
        return variant.Id;
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

        // F-3 goes to the firm's contact e-mail (08 2.2.7): a fixed address here.
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

    /// <summary>One item of a seeded order: its type, units, whether it carries its delivery mark (on <see cref="DeliveryDate"/>, default five days ago) and its exemption.</summary>
    private sealed record ItemSpec(ProductType Type, int Quantity, bool Delivered, WithdrawalExemptionReason? Exemption = null)
    {
        public DateOnly? DeliveryDate { get; init; }
    }

    private sealed class FixedCompanyContactEmail : ICompanyContactEmail
    {
        public Task<string?> GetAsync(CancellationToken ct) => Task.FromResult<string?>(CompanyEmail);
    }
}
