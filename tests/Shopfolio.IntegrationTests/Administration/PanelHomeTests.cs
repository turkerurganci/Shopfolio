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
using Shopfolio.Modules.Notifications;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;

using ItemAmounts = Shopfolio.Modules.Catalog.Contracts.Pricing.ItemAmounts;
using RecordAmounts = Shopfolio.Modules.AfterSales.ItemAmounts;

namespace Shopfolio.IntegrationTests.Administration;

/// <summary>The collection of the panel home's tests (T382): one database copied from the template.</summary>
[CollectionDefinition(PanelHomeDefinition.Name)]
public sealed class PanelHomeDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "PanelHome";
}

/// <summary>
/// The panel home on a real PostgreSQL (T382 KK3-KK9; 02 10.6.1, 10.8.2; 05 10.2): the orders, item records, refunds and dispatch rows are written as the operations leave them and
/// the home is read through the modules' surfaces. The pure rules and the surface are the unit tests' (PanelHomeRulesTests).
/// </summary>
[Collection(PanelHomeDefinition.Name)]
public sealed class PanelHomeTests : IAsyncLifetime
{
    private static readonly DateOnly Day = new(2026, 11, 10);
    private static readonly DateTimeOffset DayStart = TurkeyTime.StartOf(Day);
    private static readonly DateTimeOffset Now = DayStart.AddHours(12);
    private static readonly DateTimeOffset LongAgo = DayStart.AddDays(-90);
    private const string Iban = "TR330006100519786457841326";
    private const string FirmEmail = "firma@example.com";
    private static readonly RecordAmounts Amounts = new(250m, 0m, 50m);

    private static int _versionNumber = 7000;

    private readonly CollectionDatabaseFixture _collection;

    private int _counter;
    private Guid _province;
    private CompanyIdentity? _identity;
    private Guid _shirt;
    private Guid _lamp;
    private Guid _service;
    private Admin? _admin;

    // The card line is a provider contract the checklist's payment item reads; the checklist tests close it so that only the bank transfer switch can open a payment method.
    private bool _cardLineOpen = true;

    public PanelHomeTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE refund_iban_requests, refund_records, refunds, returns, defect_claims, not_returned_closures, return_rejections, withdrawals, late_delivery_terminations, cancellations, "
            + "item_removals, contact_requests, dispatch_records, transition_records, checkbox_acceptance_order_items, checkbox_acceptances, order_email_histories, order_address_histories, "
            + "order_items, orders, price_histories, variants, option_values, options, products, admins CASCADE");
        await ExecuteAsync("UPDATE channel_state_records SET consecutive_channel_error_count = 0, first_channel_error_at = NULL");
        _province = await ProvinceAsync();
        _identity = await SetCompanyAsync();
        _shirt = await SeedVariantAsync(300m);
        _lamp = await SeedVariantAsync(200m);
        _service = await SeedVariantAsync(150m, ProductType.Service);
        _admin = Admin.Create("Firma Yöneticisi", "yonetici@example.com", "digest-placeholder", LongAgo);
        await SaveAsync(c => c.Admins.Add(_admin));
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1, KK2: six counters, named, each leading to its list; an empty shop counts zero ----

    [Fact]
    public async Task AnEmptyShop_ShowsSixCountersAtZero_KK1_KK2_KK6()
    {
        PanelHome home = await HomeAsync();

        Assert.Equal(Enum.GetValues<PanelCounterKind>(), home.Counters.Select(c => c.Kind));
        Assert.All(home.Counters, c => Assert.Equal(0, c.Count));
        Assert.Equal(6, home.Counters.Select(c => c.ListTarget).Distinct().Count());
        Assert.False(home.ChannelWarningOpen);
        Assert.False(home.FirmNoticesWarningOpen);
    }

    // ---- The payment approval counter: the bank transfer orders that wait ----

    [Fact]
    public async Task PaymentApproval_CountsTheBankTransferOrdersThatWait_AndNotTheCardOrPaidOnes_KK2()
    {
        Guid waiting = await SaveOrderAsync(PaymentMethod.BankTransfer, (_shirt, 1));
        Guid card = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        Guid paid = await SaveOrderAsync(PaymentMethod.BankTransfer, (_shirt, 1));
        await SetAxesAsync(waiting, OrderStatus.Placed, PaymentStatus.Pending);
        await SetAxesAsync(card, OrderStatus.Placed, PaymentStatus.Pending);
        await SetAxesAsync(paid, OrderStatus.Placed, PaymentStatus.Paid);

        Assert.Equal(1, await CountOfAsync(PanelCounterKind.PaymentApproval));
    }

    // ---- KK3: the orders to ship ----

    [Fact]
    public async Task ToShip_CountsPreparingOrdersWithAnOpenPhysicalItem_AndTheDeliveryFailedOnes_KK3()
    {
        Guid preparing = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        Guid partlyCancelled = await SaveOrderAsync(PaymentMethod.Card, (_lamp, 2));
        Guid fullyCancelled = await SaveOrderAsync(PaymentMethod.Card, (_lamp, 1));
        Guid serviceOnly = await SaveOrderAsync(PaymentMethod.Card, (_service, 1));
        Guid failedDelivery = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        Guid shipped = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        await SetAxesAsync(preparing, OrderStatus.Preparing, PaymentStatus.Paid);
        await SetAxesAsync(partlyCancelled, OrderStatus.Preparing, PaymentStatus.Paid);
        await SetAxesAsync(fullyCancelled, OrderStatus.Preparing, PaymentStatus.Paid);
        await SetAxesAsync(serviceOnly, OrderStatus.Preparing, PaymentStatus.Paid);
        await SetAxesAsync(failedDelivery, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
        await SetAxesAsync(shipped, OrderStatus.Shipped, PaymentStatus.Paid);
        await SaveAsync(c =>
        {
            c.Cancellations.Add(Cancellation.ByBuyer(ItemOf(partlyCancelled), 1, Amounts, null, DayStart));
            c.Cancellations.Add(Cancellation.ByBuyer(ItemOf(fullyCancelled), 1, Amounts, null, DayStart));
        });

        // The order whose only unit was cancelled is closed; the order of a half-cancelled item still has a unit to ship; a service-only order has nothing to ship.
        Assert.Equal(3, await CountOfAsync(PanelCounterKind.ToShip));
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.DeliveryMarkPending));
    }

    [Fact]
    public async Task ToShip_AnOrderThatCameBackAsDeliveryFailed_IsCountedWhateverItsItemsHold_KK3()
    {
        Guid back = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        await SetAxesAsync(back, OrderStatus.DeliveryFailed, PaymentStatus.Paid);
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(ItemOf(back), 1, Amounts, null, DayStart)));

        Assert.Equal(1, await CountOfAsync(PanelCounterKind.ToShip));
    }

    // ---- The delivery mark and the service counters ----

    [Fact]
    public async Task DeliveryMark_CountsTheShippedOrdersWithAnOpenUnit_AndNotTheDeliveredOrTheClosedOnes_KK2()
    {
        Guid shipped = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        Guid withdrawn = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        Guid delivered = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        await SetAxesAsync(shipped, OrderStatus.Shipped, PaymentStatus.Paid);
        await SetAxesAsync(withdrawn, OrderStatus.Shipped, PaymentStatus.Paid);
        await SetAxesAsync(delivered, OrderStatus.Delivered, PaymentStatus.Paid);
        await SaveAsync(c => c.Withdrawals.Add(Withdrawal.ByBuyer(ItemOf(withdrawn), 1, Day, null, Amounts, null, DayStart)));

        Assert.Equal(1, await CountOfAsync(PanelCounterKind.DeliveryMarkPending));
    }

    [Fact]
    public async Task ServicePending_CountsThePaidUnmarkedServiceOrders_AndNotTheMarkedUnpaidOrCancelledOnes_KK2()
    {
        Guid open = await SaveOrderAsync(PaymentMethod.Card, (_service, 1));
        Guid marked = await SaveOrderAsync(PaymentMethod.Card, (_service, 1));
        Guid unpaid = await SaveOrderAsync(PaymentMethod.BankTransfer, (_service, 1));
        Guid cancelled = await SaveOrderAsync(PaymentMethod.Card, (_service, 1));
        Guid closed = await SaveOrderAsync(PaymentMethod.Card, (_service, 1));
        await SetAxesAsync(open, OrderStatus.Placed, PaymentStatus.Paid);
        await SetAxesAsync(marked, OrderStatus.Placed, PaymentStatus.Paid);
        await SetAxesAsync(unpaid, OrderStatus.Placed, PaymentStatus.Pending);
        await SetAxesAsync(cancelled, OrderStatus.Cancelled, PaymentStatus.Refunded);
        await SetAxesAsync(closed, OrderStatus.Placed, PaymentStatus.Paid);
        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = now(), delivery_date = '{Day:yyyy-MM-dd}' WHERE order_id = '{marked}'");
        await SaveAsync(c => c.Withdrawals.Add(Withdrawal.ByBuyer(ItemOf(closed), 1, Day, null, Amounts, null, DayStart)));

        Assert.Equal(1, await CountOfAsync(PanelCounterKind.ServicePending));
    }

    // ---- The open request counter: contact requests and defect claims ----

    [Fact]
    public async Task OpenRequest_CountsTheOpenContactRequestsAndTheOpenDefectClaims_AndNotTheClosedOnes_KK2()
    {
        Guid claimOrder = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        Guid resolvedOrder = await SaveOrderAsync(PaymentMethod.Card, (_lamp, 1));
        await SetAxesAsync(claimOrder, OrderStatus.Delivered, PaymentStatus.Paid);
        await SetAxesAsync(resolvedOrder, OrderStatus.Delivered, PaymentStatus.Paid);
        ContactRequest closedRequest = ContactRequest.Create("Ayşe", "ayse@example.com", ContactTopic.Other, "Kapalı", DayStart);
        await SaveAsync(c =>
        {
            c.ContactRequests.Add(ContactRequest.Create("Ayşe", "ayse@example.com", ContactTopic.GeneralQuestion, "Açık", DayStart));
            c.ContactRequests.Add(ContactRequest.Create("Ali", "ali@example.com", ContactTopic.PersonalDataRequest, "Açık, başka tip", DayStart));
            c.ContactRequests.Add(closedRequest);
            c.DefectClaims.Add(DefectClaim.Create(ItemOf(claimOrder), 1, "Kusurlu", null, DayStart));
            c.DefectClaims.Add(DefectClaim.Create(ItemOf(resolvedOrder), 1, "Kusurlu", null, DayStart));
        });
        await ExecuteAsync($"UPDATE contact_requests SET contact_request_status = 'Closed', last_closed_at = now() WHERE id = '{closedRequest.Id}'");
        await ExecuteAsync($"UPDATE defect_claims SET defect_claim_status = 'Resolved' WHERE order_item_id = '{ItemOf(resolvedOrder)}'");

        Assert.Equal(3, await CountOfAsync(PanelCounterKind.OpenRequest));
    }

    // ---- KK4: the refund and return counter's three kinds; "IBAN bekleniyor" is not counted ----

    [Fact]
    public async Task RefundAndReturn_CountsTheReturnedGoodsToReceive_TheRefundToProcess_AndTheCardRefundThatFailedOrWaits_KK4()
    {
        // (1) The goods of a withdrawal after delivery, still on their way.
        Guid goods = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        await MakeDeliveredAsync(goods);
        await SaveAsync(c => c.Withdrawals.Add(Withdrawal.ByBuyer(ItemOf(goods), 1, Day, null, Amounts, null, DayStart)));

        // (2) The bank transfer cancellation whose refund the firm has yet to process.
        Guid transfer = await SaveOrderAsync(PaymentMethod.BankTransfer, (_lamp, 1));
        await SetAxesAsync(transfer, OrderStatus.Preparing, PaymentStatus.Paid);
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(ItemOf(transfer), 1, Amounts, null, DayStart)));

        // (3) The card refund the provider refused, and (4) the one whose result is awaited.
        Guid failed = await CardRefundOrderAsync(RefundRecordState.Failed, sent: false);
        Guid waiting = await CardRefundOrderAsync(RefundRecordState.OutcomePending, sent: false);

        // Not counted: the card refund that went through, and the transfer refund the firm already sent.
        await CardRefundOrderAsync(RefundRecordState.Succeeded, sent: true);
        Guid started = await SaveOrderAsync(PaymentMethod.BankTransfer, (_lamp, 1));
        await SetAxesAsync(started, OrderStatus.Preparing, PaymentStatus.Paid);
        Cancellation startedCancellation = Cancellation.ByBuyer(ItemOf(started), 1, Amounts, null, DayStart);
        await SaveAsync(c => c.Cancellations.Add(startedCancellation));
        await SaveAsync(c => c.Refunds.Add(RefundOf(started, RefundType.Cancellation, PaymentMethod.BankTransfer, r =>
        {
            r.CancellationId = startedCancellation.Id;
            r.SentAt = DayStart.AddHours(2);
        })));

        Assert.Equal(4, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
        Assert.NotEqual(Guid.Empty, failed);
        Assert.NotEqual(Guid.Empty, waiting);
    }

    [Fact]
    public async Task RefundAndReturn_CountsABornTransferRefundUntilItIsSent_AndAFailedCardRefundWithItsTransferPathOpenOnlyOnceTheIbanIsEntered_KK4()
    {
        // The transfer refund of a cancellation: its row is born (the firm's step) and not sent - the firm still has to send it; sending drops it.
        Guid born = await SaveOrderAsync(PaymentMethod.BankTransfer, (_lamp, 1));
        await SetAxesAsync(born, OrderStatus.Preparing, PaymentStatus.Paid);
        Cancellation cancellation = Cancellation.ByBuyer(ItemOf(born), 1, Amounts, null, DayStart);
        await SaveAsync(c => c.Cancellations.Add(cancellation));
        Refund transfer = RefundOf(born, RefundType.Cancellation, PaymentMethod.BankTransfer, r => r.CancellationId = cancellation.Id);
        await SaveAsync(c => c.Refunds.Add(transfer));
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        await ExecuteAsync($"UPDATE refunds SET sent_at = now() WHERE id = '{transfer.Id}'");
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        // A card refund the provider refused counts; the firm opens the transfer path: the item waits for the customer's IBAN and drops.
        Guid failed = await CardRefundOrderAsync(RefundRecordState.Failed, sent: false);
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
        Guid refundId = RefundIdOf(failed);
        RefundIbanRequest request = RefundIbanRequest.ForItem(ItemOf(failed), RefundIbanRequestReason.CardRefundFailed, refundId, _admin!.Id, _admin.Name, DayStart.AddHours(3));
        await SaveAsync(c => c.RefundIbanRequests.Add(request));
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        // The customer enters the IBAN: the refund turns to the transfer way and is the firm's to send (K-525); sending drops it.
        await ExecuteAsync($"UPDATE refund_iban_requests SET fulfilled_at = now() WHERE id = '{request.Id}'");
        await ExecuteAsync($"UPDATE refunds SET payment_method = '{nameof(PaymentMethod.BankTransfer)}' WHERE id = '{refundId}'");
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        await ExecuteAsync($"UPDATE refunds SET sent_at = now() WHERE id = '{refundId}'");
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    [Fact]
    public async Task RefundAndReturn_CountsTheDefectRemedyTransferRefundUntilItIsSent_KK4()
    {
        Guid order = await SaveOrderAsync(PaymentMethod.BankTransfer, (_lamp, 1));
        await MakeDeliveredAsync(order);
        Refund refund = RefundOf(order, RefundType.DefectClaimRescission, PaymentMethod.BankTransfer, r =>
        {
            r.CouponShare = 0m;
            r.VatAmount = 0m;
        });
        await SaveAsync(c => c.Refunds.Add(refund));
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        await ExecuteAsync($"UPDATE refunds SET sent_at = now() WHERE id = '{refund.Id}'");
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    [Fact]
    public async Task RefundAndReturn_DoesNotCountAnItemThatWaitsForTheCustomersIban_KK4()
    {
        Guid transfer = await SaveOrderAsync(PaymentMethod.BankTransfer, (_lamp, 1));
        await SetAxesAsync(transfer, OrderStatus.Preparing, PaymentStatus.Paid);
        await SaveAsync(c => c.Cancellations.Add(Cancellation.ByBuyer(ItemOf(transfer), 1, Amounts, null, DayStart)));
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        // The firm asks the customer for the IBAN: the item waits for the customer, not the firm.
        RefundIbanRequest request = RefundIbanRequest.ForItem(ItemOf(transfer), RefundIbanRequestReason.AdminRequest, null, _admin!.Id, _admin.Name, DayStart);
        await SaveAsync(c => c.RefundIbanRequests.Add(request));
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        // The customer enters it: the item is a refund to process again (K-525).
        await ExecuteAsync($"UPDATE refund_iban_requests SET fulfilled_at = now() WHERE id = '{request.Id}'");
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    [Fact]
    public async Task RefundAndReturn_DefectRemedyTransferRefundWithNoIbanOnFile_WaitsForTheCustomer_UntilTheIbanIsEnteredAndTheRefundSent_KK4()
    {
        Guid order = await SaveOrderAsync(PaymentMethod.BankTransfer, (_lamp, 1));
        await MakeDeliveredAsync(order);
        Refund refund = RefundOf(order, RefundType.DefectClaimRescission, PaymentMethod.BankTransfer, r =>
        {
            r.CouponShare = 0m;
            r.VatAmount = 0m;
        });
        await SaveAsync(c => c.Refunds.Add(refund));

        // The file has no IBAN: the remedy's refund row is born and the item's request opens with no refund link (02 10.6.1; K-686) - "IBAN bekleniyor", not the firm's.
        RefundIbanRequest request = RefundIbanRequest.ForItem(ItemOf(order), RefundIbanRequestReason.AdminRequest, null, _admin!.Id, _admin.Name, DayStart.AddHours(1));
        await SaveAsync(c => c.RefundIbanRequests.Add(request));
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        // The customer enters the IBAN: the request is fulfilled and the refund is the firm's to send.
        await ExecuteAsync($"UPDATE refund_iban_requests SET fulfilled_at = now() WHERE id = '{request.Id}'");
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        await ExecuteAsync($"UPDATE refunds SET sent_at = now() WHERE id = '{refund.Id}'");
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    [Fact]
    public async Task RefundAndReturn_FailedCardRefundMovedToTheTransferPathWhileTheItemsRequestWasAlreadyOpen_IsNotCounted_KK4()
    {
        Guid failed = await CardRefundOrderAsync(RefundRecordState.Failed, sent: false);
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        // The item's open request existed before the transfer path opened: it is reused, with no refund link.
        RefundIbanRequest request = RefundIbanRequest.ForItem(ItemOf(failed), RefundIbanRequestReason.AdminRequest, null, _admin!.Id, _admin.Name, DayStart.AddHours(1));
        await SaveAsync(c => c.RefundIbanRequests.Add(request));
        await ExecuteAsync($"UPDATE refunds SET payment_method = '{nameof(PaymentMethod.BankTransfer)}' WHERE id = '{RefundIdOf(failed)}'");
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        await ExecuteAsync($"UPDATE refund_iban_requests SET fulfilled_at = now() WHERE id = '{request.Id}'");
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    [Fact]
    public async Task RefundAndReturn_ARefundVoidedWithTheCorrectionOfTheCancellation_IsNotCounted_KK4()
    {
        Guid order = await SaveOrderAsync(PaymentMethod.BankTransfer, (_lamp, 1));
        await SetAxesAsync(order, OrderStatus.Preparing, PaymentStatus.Paid);
        Cancellation cancellation = Cancellation.ByBuyer(ItemOf(order), 1, Amounts, null, DayStart);
        await SaveAsync(c => c.Cancellations.Add(cancellation));
        Refund refund = RefundOf(order, RefundType.Cancellation, PaymentMethod.BankTransfer, r => r.CancellationId = cancellation.Id);
        await SaveAsync(c => c.Refunds.Add(refund));
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        await ExecuteAsync($"UPDATE refunds SET voided_at = now() WHERE id = '{refund.Id}'");
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    [Fact]
    public async Task AnOrderWithTwoKindsOfWork_IsCountedOnce_KK4()
    {
        Guid both = await CardRefundOrderAsync(RefundRecordState.Failed, sent: false);
        await SaveAsync(c => c.Withdrawals.Add(Withdrawal.ByBuyer(ItemOf(both), 1, Day, null, Amounts, null, DayStart)));
        await MakeDeliveredAsync(both);

        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    // ---- KK5: "mal dönmedi" and the return rejection drop the item; a reopened closure brings it back ----

    [Fact]
    public async Task ANotReturnedClosure_DropsTheItem_AndItsReopeningBringsItBack_KK5()
    {
        Guid order = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        await MakeDeliveredAsync(order);
        Withdrawal withdrawal = Withdrawal.ByBuyer(ItemOf(order), 1, Day, null, Amounts, null, DayStart);
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        NotReturnedClosure closure = NotReturnedClosure.Create(ItemOf(order), withdrawal.Id, 1, _admin!.Id, _admin.Name, DayStart.AddHours(1));
        await SaveAsync(c => c.NotReturnedClosures.Add(closure));
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        // The goods arrived after all: the closure is reopened and the item is awaited again.
        await ExecuteAsync($"UPDATE not_returned_closures SET reopened_at = now() WHERE id = '{closure.Id}'");
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    [Fact]
    public async Task AReturnRejection_DropsTheItem_KK5()
    {
        Guid order = await SaveOrderAsync(PaymentMethod.Card, (_shirt, 1));
        await MakeDeliveredAsync(order);
        Withdrawal withdrawal = Withdrawal.ByBuyer(ItemOf(order), 1, Day, null, Amounts, null, DayStart);
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        await SaveAsync(c => c.ReturnRejections.Add(ReturnRejection.Create(ItemOf(order), withdrawal.Id, 1, Day, _admin!.Id, _admin.Name, DayStart.AddHours(1))));

        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    [Fact]
    public async Task AGoodsReceipt_EndsTheWaitForTheGoods_AndOwesTheRefund_KK4()
    {
        Guid order = await SaveOrderAsync(PaymentMethod.BankTransfer, (_shirt, 1));
        await MakeDeliveredAsync(order);
        Withdrawal withdrawal = Withdrawal.ByBuyer(ItemOf(order), 1, Day, null, Amounts, null, DayStart);
        await SaveAsync(c => c.Withdrawals.Add(withdrawal));
        Return receipt = Return.ForWithdrawal(ItemOf(order), withdrawal.Id, 1, Day, _admin!.Id, _admin.Name, DayStart.AddHours(2));
        await SaveAsync(c => c.Returns.Add(receipt));

        // The goods are here, the refund of the receipt has no row yet: still one order of work, now for the refund.
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));

        Refund refund = RefundOf(order, RefundType.Withdrawal, PaymentMethod.BankTransfer, r =>
        {
            r.WithdrawalId = withdrawal.Id;
            r.ReturnId = receipt.Id;
        });
        await SaveAsync(c => c.Refunds.Add(refund));

        // The row is born, not sent: the firm still has to send it (K-621); sending ends the work.
        Assert.Equal(1, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
        await ExecuteAsync($"UPDATE refunds SET sent_at = now() WHERE id = '{refund.Id}'");
        Assert.Equal(0, await CountOfAsync(PanelCounterKind.RefundAndReturnPending));
    }

    // ---- KK7: the warnings are read, from the thresholds ----

    [Fact]
    public async Task TheChannelWarning_OpensAtFiveErrorsAndFifteenMinutes_AndLiftsOnTheFirstSuccess_KK7()
    {
        await SetChannelAsync(4, Now.AddMinutes(-40));
        Assert.False((await HomeAsync()).ChannelWarningOpen);

        await SetChannelAsync(5, Now.AddMinutes(-14));
        Assert.False((await HomeAsync()).ChannelWarningOpen);

        await SetChannelAsync(5, Now.AddMinutes(-15));
        Assert.True((await HomeAsync()).ChannelWarningOpen);

        await ExecuteAsync("UPDATE channel_state_records SET consecutive_channel_error_count = 0, first_channel_error_at = NULL, last_succeeded_at = now()");
        Assert.False((await HomeAsync()).ChannelWarningOpen);
    }

    [Fact]
    public async Task TheFirmNoticesWarning_IsOpenWhenTheLastSettledNoticeWasNotDeliveredToTheCurrentEmail_KK7()
    {
        Assert.False((await HomeAsync()).FirmNoticesWarningOpen);

        // The last settled notice (F-1 to F-4) was not delivered, to the firm's current contact e-mail (compared without case).
        await SaveDispatchAsync(DispatchType.F1, DispatchState.Sent, FirmEmail, Now.AddHours(-3));
        Guid undelivered = await SaveDispatchAsync(DispatchType.F2, DispatchState.Undelivered, "FIRMA@example.com", Now.AddHours(-2));
        Assert.True((await HomeAsync()).FirmNoticesWarningOpen);

        // A queued row is not settled; a notice that is not a firm notice (F-5) is no firm notice.
        await SaveDispatchAsync(DispatchType.F1, DispatchState.Queued, FirmEmail, Now.AddHours(-1));
        await SaveDispatchAsync(DispatchType.F5, DispatchState.Sent, FirmEmail, Now.AddMinutes(-30));
        Assert.True((await HomeAsync()).FirmNoticesWarningOpen);

        // The next firm notice is sent: the warning lifts by itself.
        Guid sent = await SaveDispatchAsync(DispatchType.F4, DispatchState.Sent, FirmEmail, Now.AddMinutes(-10));
        Assert.False((await HomeAsync()).FirmNoticesWarningOpen);
        Assert.NotEqual(undelivered, sent);
    }

    [Fact]
    public async Task TheFirmNoticesWarning_LiftsWhenTheContactEmailChanges_AndTheTwoWarningsShowTogether_KK7()
    {
        await SaveDispatchAsync(DispatchType.F3, DispatchState.Undelivered, FirmEmail, Now.AddHours(-2));
        await SetChannelAsync(6, Now.AddHours(-1));

        PanelHome both = await HomeAsync();
        Assert.True(both.FirmNoticesWarningOpen);
        Assert.True(both.ChannelWarningOpen);

        // The firm corrected its e-mail: the undelivered notice went to the old address, so it no longer holds.
        await ExecuteAsync("UPDATE companies SET email = 'yeni@example.com'");
        PanelHome corrected = await HomeAsync();
        Assert.False(corrected.FirmNoticesWarningOpen);
        Assert.True(corrected.ChannelWarningOpen);
    }

    // ---- KK8, KK9: the checklist names what is missing; it lifts by itself; there is no wizard ----

    [Fact]
    public async Task TheChecklist_NamesTheMissingIdentityFields_AndTheOtherMissingItems_KK8()
    {
        _cardLineOpen = false;
        await ExecuteAsync("UPDATE companies SET legal_name = NULL, kep_address = NULL; UPDATE company_settings SET is_bank_transfer_enabled = false, bank_transfer_iban = NULL");

        PanelHome home = await HomeAsync();

        Assert.NotNull(home.SetupChecklist);
        Assert.Equal(Enum.GetValues<SetupChecklistItemKind>(), home.SetupChecklist!.Select(i => i.Kind));
        SetupChecklistItem identity = home.SetupChecklist!.Single(i => i.Kind == SetupChecklistItemKind.CompanyIdentity);
        Assert.False(identity.IsDone);
        Assert.Contains("LegalName", identity.MissingFields);
        Assert.Contains("KepAddress", identity.MissingFields);
        Assert.False(home.SetupChecklist!.Single(i => i.Kind == SetupChecklistItemKind.PaymentMethod).IsDone);
    }

    [Fact]
    public async Task TheChecklist_MarksTheItemsAsTheyComplete_KK9()
    {
        _cardLineOpen = false;
        await ExecuteAsync("UPDATE company_settings SET is_bank_transfer_enabled = false, bank_transfer_iban = NULL");
        PanelHome before = await HomeAsync();
        Assert.True(before.SetupChecklist!.Single(i => i.Kind == SetupChecklistItemKind.CompanyIdentity).IsDone);
        Assert.Empty(before.SetupChecklist!.Single(i => i.Kind == SetupChecklistItemKind.CompanyIdentity).MissingFields);
        Assert.False(before.SetupChecklist!.Single(i => i.Kind == SetupChecklistItemKind.PaymentMethod).IsDone);
        int missingBefore = before.SetupChecklist!.Count(i => !i.IsDone);

        await ExecuteAsync($"UPDATE company_settings SET is_bank_transfer_enabled = true, bank_transfer_iban = '{Iban}'");
        PanelHome after = await HomeAsync();

        Assert.True(after.SetupChecklist!.Single(i => i.Kind == SetupChecklistItemKind.PaymentMethod).IsDone);
        Assert.Equal(missingBefore - 1, after.SetupChecklist!.Count(i => !i.IsDone));
    }

    // ---- Harness ----

    private async Task<PanelHome> HomeAsync()
    {
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn, new FixedTime(Now), _cardLineOpen);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        IServiceProvider services = scope.ServiceProvider;
        AppDbContext db = services.GetRequiredService<AppDbContext>();
        TimeProvider time = services.GetRequiredService<TimeProvider>();
        PanelHomeService home = new(
            services.GetRequiredService<IOrderingModule>(),
            services.GetRequiredService<IAfterSalesModule>(),
            services.GetRequiredService<IPaymentModule>(),
            new ContactCount(new ContactRequests(db, new UnusedNotifications(), time)),
            new PanelWarningReader(db, time),
            services.GetRequiredService<ISettingsModule>());
        return await home.GetAsync(Ct);
    }

    private async Task<int> CountOfAsync(PanelCounterKind kind) => (await HomeAsync()).Counters.Single(c => c.Kind == kind).Count;

    // A card order, paid and cancelled in one unit, with the refund the system started: its latest request in the given state and the refund sent or not.
    private async Task<Guid> CardRefundOrderAsync(RefundRecordState state, bool sent)
    {
        Guid order = await SaveOrderAsync(PaymentMethod.Card, (_lamp, 1));
        await SetAxesAsync(order, OrderStatus.Preparing, PaymentStatus.Paid);
        Cancellation cancellation = Cancellation.ByBuyer(ItemOf(order), 1, Amounts, null, DayStart);
        await SaveAsync(c => c.Cancellations.Add(cancellation));
        Refund refund = RefundOf(order, RefundType.Cancellation, PaymentMethod.Card, r => r.CancellationId = cancellation.Id);
        if (sent)
        {
            refund.SentAt = DayStart.AddHours(2);
        }

        RefundRecord record = RefundRecord.Sending(refund.Id, DayStart.AddHours(1));
        record.State = state;
        if (state is RefundRecordState.Succeeded or RefundRecordState.Failed)
        {
            record.OutcomeAt = DayStart.AddHours(1);
        }
        await SaveAsync(c =>
        {
            c.Refunds.Add(refund);
            c.RefundRecords.Add(record);
        });
        return order;
    }

    private Refund RefundOf(Guid orderId, RefundType type, PaymentMethod method, Action<Refund> links)
    {
        Refund refund = Refund.ByAdmin(orderId, type, method, 250m, _admin!.Id, _admin.Name, DayStart);
        links(refund);
        refund.OrderItemId = ItemOf(orderId);
        refund.Quantity = 1;
        return refund;
    }

    private async Task MakeDeliveredAsync(Guid orderId)
    {
        await SetAxesAsync(orderId, OrderStatus.Delivered, PaymentStatus.Paid);
        await ExecuteAsync($"UPDATE order_items SET delivery_marked_at = '{DayStart.AddDays(-5):O}', delivery_date = '{Day.AddDays(-5):yyyy-MM-dd}' WHERE order_id = '{orderId}'");
        await SaveAsync(c => c.TransitionRecords.Add(Record(orderId, TransitionCode.S5, DayStart.AddDays(-7))));
    }

    private Task SetChannelAsync(int errors, DateTimeOffset firstErrorAt) =>
        ExecuteAsync($"UPDATE channel_state_records SET consecutive_channel_error_count = {errors}, first_channel_error_at = '{firstErrorAt:O}'");

    private async Task<Guid> SaveDispatchAsync(DispatchType type, DispatchState state, string recipient, DateTimeOffset at)
    {
        DispatchRelatedRecordType related = type switch
        {
            DispatchType.F1 => DispatchRelatedRecordType.Order,
            DispatchType.F2 => DispatchRelatedRecordType.ContactRequest,
            DispatchType.F3 => DispatchRelatedRecordType.DefectClaim,
            DispatchType.F4 => DispatchRelatedRecordType.Refund,
            DispatchType.F5 => DispatchRelatedRecordType.Order,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, "A firm notice."),
        };
        DispatchRecord row = type == DispatchType.F5
            ? DispatchRecord.Queue(type, recipient, null, null, null, at)
            : DispatchRecord.Queue(type, recipient, null, related, Guid.NewGuid(), at);
        row.DispatchState = state;
        if (state == DispatchState.Sent)
        {
            row.SentAt = at;
            row.LastOutcome = DispatchOutcome.Sent;
            row.AttemptCount = 1;
        }
        else if (state == DispatchState.Undelivered)
        {
            row.LastOutcome = DispatchOutcome.RecipientError;
            row.AttemptCount = 3;
        }
        else
        {
            row.NextAttemptAt = at.AddMinutes(5);
        }

        await SaveAsync(c => c.DispatchRecords.Add(row));
        return row.Id;
    }

    private Guid RefundIdOf(Guid orderId)
    {
        using AppDbContext db = NewContext();
        return db.Refunds.AsNoTracking().Where(r => r.OrderId == orderId).Select(r => r.Id).Single();
    }

    private Guid ItemOf(Guid orderId)
    {
        using AppDbContext db = NewContext();
        return db.OrderItems.AsNoTracking().Where(i => i.OrderId == orderId).Select(i => i.Id).Single();
    }

    private async Task<Guid> SaveOrderAsync(PaymentMethod method, params (Guid VariantId, int Quantity)[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        DateTimeOffset createdAt = DayStart.AddMinutes(n);
        List<OrderItemSource> sources = [];
        foreach ((Guid variantId, _) in lines)
        {
            sources.Add(await SourceAsync(variantId));
        }

        List<ItemAmounts> amounts = [.. lines.Select((l, i) => ItemPricing.Price(new ItemPriceInput(sources[i].UnitPrice, l.Quantity, 0m, sources[i].VatRate)))];
        Order order = Order.Create(
            $"2026-P{n:D5}{Guid.NewGuid().ToString("N")[..4]}", $"access-{Guid.NewGuid():N}", $"confirm-{Guid.NewGuid():N}", method, true, "ali@example.com",
            ItemPricing.SumOrderTotal(amounts, 0m), 0m, createdAt.AddHours(1), createdAt);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = Guid.NewGuid();
        order.FreezeAddresses(("Ali Veli", "5321234567", _province, "Çankaya", "Atatürk Bulvarı 1"), ("Ali Veli", _province, "Çankaya", "Atatürk Bulvarı 1"));
        OrderFreeze.FreezeLegalIdentity(order, _identity!, new LegalTextVersionIds(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), method == PaymentMethod.BankTransfer ? Iban : null);
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        order.FreezeShippingFeeVatBreakdown([]);
        order.FreezeShippingPromise(2);
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
        Product product = Product.Create($"Pano Ürün {n}", $"pano-urun-{n}", type, 20m, LongAgo);
        if (type == ProductType.Physical)
        {
            product.WithSeededOrigin();
        }
        else
        {
            product.FulfilmentPeriod = 3;
        }

        product.PublishStatus = PublishStatus.Published;
        Variant variant = Variant.Create(product.Id, $"PNO-{n}", price, LongAgo, 100);
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
            + $"address_line = 'Kızılay 2', phone = '3121234567', email = '{FirmEmail}', kep_address = 'firma@hs01.kep.tr', chamber_name = 'Ankara Esnaf Odası', professional_conduct_rules = 'Meslek kuralları'");
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetCompanyIdentityAsync(Ct);
    }

    private static TransitionRecord Record(Guid orderId, TransitionCode code, DateTimeOffset at)
        => TransitionRecord.Create(orderId, code, ActorKind.System, null, null, null, null, null, null, null, at);

    private Task SetAxesAsync(Guid orderId, OrderStatus orderStatus, PaymentStatus paymentStatus) =>
        ExecuteAsync($"UPDATE orders SET order_status = '{orderStatus}', payment_status = '{paymentStatus}' WHERE id = '{orderId}'");

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

    /// <summary>The CorporateContent surface with only the open request count, on the real contact request store; every other member fails if it is called.</summary>
    private sealed class ContactCount(ContactRequests requests) : ICorporateContentModule
    {
        public Task<int> CountOpenContactRequestsAsync(CancellationToken ct) => requests.CountOpenAsync(ct);

        public Task<int> CountPanelContactRequestsAsync(IReadOnlyCollection<ContactRequestStatus> statuses, CancellationToken ct) => throw new NotSupportedException();

        public Task<IReadOnlyList<PanelContactRequestRow>> ListPanelContactRequestsAsync(IReadOnlyCollection<ContactRequestStatus> statuses, int take, CancellationToken ct) => throw new NotSupportedException();

        public Task<ContactRequestDetail?> FindContactRequestAsync(Guid requestId, CancellationToken ct) => throw new NotSupportedException();

        public Task<ContactRequestChangeResult> CloseContactRequestAsync(Guid requestId, Guid adminId, CancellationToken ct) => throw new NotSupportedException();

        public Task<ContactRequestChangeResult> ReopenContactRequestAsync(Guid requestId, Guid adminId, CancellationToken ct) => throw new NotSupportedException();

        public Task<int> CountSummaryContactRequestsAsync(DateTimeOffset periodStart, DateTimeOffset periodEnd, CancellationToken ct) => throw new NotSupportedException();

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
