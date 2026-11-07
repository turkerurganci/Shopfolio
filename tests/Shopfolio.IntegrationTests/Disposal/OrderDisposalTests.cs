using System.Net;

using Microsoft.EntityFrameworkCore;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
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

namespace Shopfolio.IntegrationTests.Disposal;

/// <summary>
/// The order's part of the disposal on a real PostgreSQL and a fake clock (T301; 06 9.1, 9.2, 9.3.1; 05 7.4.3 (2)): the two periods and the arm that is
/// read from the payment record (a late card payment keeps a Failed order on Z-28; a corrected payment mark makes the order an unpaid one), the order's
/// row lock and the version, the order picked twice, the fields that go together, the linked records emptied or deleted, the records that must not be
/// touched, and the disposal row with its type and number. Every date is seeded relative to the harness's clock and every period comes from
/// <see cref="OrderRetention"/>; no test waits for real time.
/// </summary>
[Collection(DisposalDefinition.Name)]
public sealed class OrderDisposalTests : IAsyncLifetime
{
    private static readonly DateOnly Today = new(2026, 11, 5);

    private readonly TemplateDatabaseFixture _template;
    private readonly CollectionDatabaseFixture _collection;
    private readonly string _logRoot = Path.Combine(Path.GetTempPath(), "shopfolio-t301-" + Guid.NewGuid().ToString("N"));
    private DisposalHarness _harness = null!;
    private Guid _provinceId;
    private Guid _formId;
    private Guid _contractId;
    private Guid _noticeId;
    private int _orderCounter;

    public OrderDisposalTests(TemplateDatabaseFixture template, CollectionDatabaseFixture collection)
    {
        _template = template;
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>A paid order created in 2015 is past Z-28 at the harness's clock (its period ended at the start of 2026).</summary>
    private static DateTimeOffset PastPaidPeriod => new(2015, 6, 1, 9, 0, 0, TimeSpan.Zero);

    public async ValueTask InitializeAsync()
    {
        string app = Path.Combine(_logRoot, "app");
        string access = Path.Combine(_logRoot, "access");
        Directory.CreateDirectory(app);
        Directory.CreateDirectory(access);
        await ExecuteAsync(
            "TRUNCATE dispatch_records, inbound_notification_records, disposal_records, orders, customers, admins, digital_files, legal_text_versions, provinces CASCADE");
        _harness = new DisposalHarness(_collection.Dsn, new LogDirectories(app, access));

        Province province = Province.Create("06", "Ankara", _harness.Now);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, 1, _harness.Now, "metin", _harness.Now);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, 1, _harness.Now, "metin", _harness.Now);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, 1, _harness.Now, "metin", _harness.Now).WithHosting();
        await _harness.SeedAsync(db =>
        {
            db.Provinces.Add(province);
            db.LegalTextVersions.AddRange(form, contract, notice);
        });
        _provinceId = province.Id;
        _formId = form.Id;
        _contractId = contract.Id;
        _noticeId = notice.Id;
    }

    public async ValueTask DisposeAsync()
    {
        await _harness.DisposeAsync();
        if (Directory.Exists(_logRoot))
        {
            Directory.Delete(_logRoot, recursive: true);
        }
    }

    // ---- The part is a part of the step ----

    [Fact]
    public void TheOrderPart_IsOneOfTheStepsParts_NamedOrders()
    {
        Assert.Contains("Orders", _harness.PartNames);
    }

    // ---- KK1: the two periods and the arm ----

    [Fact]
    public async Task PaidOrder_PastZ28_IsDisposed_AndTheOneInsideItIsKept()
    {
        Order past = await SeedPaidOrderAsync(PastPaidPeriod);
        Order inside = await SeedPaidOrderAsync(new DateTimeOffset(2016, 3, 1, 9, 0, 0, TimeSpan.Zero));

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.False(await HasPersonalDataAsync(past.Id));
        Assert.True(await HasPersonalDataAsync(inside.Id));
    }

    [Fact]
    public async Task PaidOrder_IsDisposedAtTheTurnOfTheCalendarYearInTurkeyTime_NotBefore()
    {
        // Created in 2016: the period ends at 00:00 Turkey time on 1 January 2027 = 21:00 UTC on 31 December 2026.
        Order order = await SeedPaidOrderAsync(new DateTimeOffset(2016, 3, 1, 9, 0, 0, TimeSpan.Zero));
        DateTimeOffset turn = TurkeyTime.StartOf(new DateOnly(2027, 1, 1)).ToUniversalTime();

        _harness.Time.SetUtcNow(turn - TimeSpan.FromSeconds(1));
        await _harness.RunOrderPartAsync(null, Ct);
        Assert.True(await HasPersonalDataAsync(order.Id));

        _harness.Time.SetUtcNow(turn);
        await _harness.RunOrderPartAsync(null, Ct);
        Assert.False(await HasPersonalDataAsync(order.Id));
    }

    [Fact]
    public async Task UnpaidFailedOrder_IsDisposed_ThreeYearsAfterItFailed_NotBefore()
    {
        DateTimeOffset edge = OrderRetention.FailedBefore(_harness.Now);
        Order past = await SeedFailedOrderAsync(failedAt: edge - TimeSpan.FromDays(1));
        Order inside = await SeedFailedOrderAsync(failedAt: edge + TimeSpan.FromDays(1));

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.False(await HasPersonalDataAsync(past.Id));
        Assert.True(await HasPersonalDataAsync(inside.Id));
    }

    [Fact]
    public async Task UnpaidOrder_IsJudgedFromTheMomentItFailed_NotFromItsCreation()
    {
        // Created long ago, failed recently: Z-41 counts from the failure, and the order was never paid so Z-28 does not apply.
        Order order = await SeedFailedOrderAsync(failedAt: _harness.Now - TimeSpan.FromDays(30), createdAt: PastPaidPeriod);

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.True(await HasPersonalDataAsync(order.Id));
    }

    [Fact]
    public async Task FailedOrderWithALatePayment_IsNotDisposedAtZ41_ItWaitsForZ28()
    {
        // The late card payment is a payment record with no confirming transition: the order is a Z-28 order although it is Failed (06 9.1 Z-28; 02 Z-41).
        DateTimeOffset failedAt = OrderRetention.FailedBefore(_harness.Now) - TimeSpan.FromDays(1);
        Order late = await SeedFailedOrderAsync(failedAt: failedAt);
        await SeedCardPaymentAsync(late.Id, transitionId: null);
        Order unpaid = await SeedFailedOrderAsync(failedAt: failedAt);

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.True(await HasPersonalDataAsync(late.Id));
        Assert.False(await HasPersonalDataAsync(unpaid.Id));
    }

    [Fact]
    public async Task FailedOrderWithALatePayment_IsDisposedOnceZ28IsOver()
    {
        Order late = await SeedFailedOrderAsync(failedAt: _harness.Now - TimeSpan.FromDays(400), createdAt: PastPaidPeriod);
        await SeedCardPaymentAsync(late.Id, transitionId: null);

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.False(await HasPersonalDataAsync(late.Id));
    }

    [Fact]
    public async Task PaymentMarkThatWasCorrected_MakesTheOrderAnUnpaidOne()
    {
        // A bank transfer's "paid" mark whose Ö1 a correction row corrected is not a payment: the order follows Z-41. The same order with the mark left
        // standing is paid and (created recently) is not due.
        DateTimeOffset failedAt = OrderRetention.FailedBefore(_harness.Now) - TimeSpan.FromDays(1);
        Admin admin = await SeedAdminAsync();
        Order corrected = await SeedFailedOrderAsync(failedAt: failedAt);
        await SeedBankTransferMarkAsync(corrected.Id, admin, corrections: 1);
        Order standing = await SeedFailedOrderAsync(failedAt: failedAt);
        await SeedBankTransferMarkAsync(standing.Id, admin, corrections: 0);

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.False(await HasPersonalDataAsync(corrected.Id));
        Assert.True(await HasPersonalDataAsync(standing.Id));
    }

    [Fact]
    public async Task OrdersThatAreNeitherPaidNorFailed_AreNeverDisposed()
    {
        Order pending = await SeedOrderAsync(PastPaidPeriod);

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.True(await HasPersonalDataAsync(pending.Id));
    }

    [Fact]
    public async Task TheTwoPeriods_RunInOneChunk_EachByItsOwnClock()
    {
        Order paid = await SeedPaidOrderAsync(PastPaidPeriod);
        Order failed = await SeedFailedOrderAsync(failedAt: OrderRetention.FailedBefore(_harness.Now) - TimeSpan.FromDays(1));
        Order keptPaid = await SeedPaidOrderAsync(_harness.Now - TimeSpan.FromDays(10));

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.False(await HasPersonalDataAsync(paid.Id));
        Assert.False(await HasPersonalDataAsync(failed.Id));
        Assert.True(await HasPersonalDataAsync(keptPaid.Id));
        Assert.Equal(2, await DisposalCountAsync());
        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM disposal_records WHERE disposal_record_type = 'Order'"));
    }

    // ---- KK2, KK3: the lock and the version, the order picked twice ----

    [Fact]
    public async Task Disposal_RaisesTheVersionByOne_AndAPanelOperationThatSawTheOldVersionIsRefused()
    {
        Order order = await SeedPaidOrderAsync(PastPaidPeriod);
        int before = order.Version;

        // The panel operation read the order (and its version) before the disposal; its save compares that version.
        await using AppDbContext panel = NewContext();
        Order seen = await panel.Orders.SingleAsync(o => o.Id == order.Id, Ct);

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.Equal(before + 1, await ScalarAsync<int>("SELECT version FROM orders WHERE id = @id", ("id", order.Id)));
        seen.Total += 1m;
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => panel.SaveChangesAsync(Ct));
    }

    [Fact]
    public async Task DisposedOrder_IsNotPickedASecondTime_AndItsVersionRisesOnce()
    {
        Order order = await SeedPaidOrderAsync(PastPaidPeriod);

        await _harness.RunOrderPartAsync(null, Ct);
        await _harness.RunOrderPartAsync(null, Ct);
        await _harness.RunStepAsync(Ct);

        Assert.Equal(2, await ScalarAsync<int>("SELECT version FROM orders WHERE id = @id", ("id", order.Id)));
        Assert.Equal(1, await DisposalCountAsync());
    }

    [Fact]
    public async Task OrderDisposedBetweenThePickAndTheLock_IsSkipped_AndNotCounted()
    {
        Order raced = await SeedPaidOrderAsync(PastPaidPeriod);
        Order other = await SeedPaidOrderAsync(PastPaidPeriod);

        // Another transaction disposes one of the two after the part picked both and before it locks them.
        await _harness.RunOrderPartAsync(
            async (_, ct) =>
            {
                await using AppDbContext db = NewContext();
                Order order = await db.Orders.SingleAsync(o => o.Id == raced.Id, ct);
                order.ErasePersonalData();
                order.BumpVersion();
                await db.SaveChangesAsync(ct);
            },
            Ct);

        Assert.False(await HasPersonalDataAsync(raced.Id));
        Assert.False(await HasPersonalDataAsync(other.Id));
        Assert.Equal(1, await DisposalCountAsync());
        Assert.Equal(2, await ScalarAsync<int>("SELECT version FROM orders WHERE id = @id", ("id", raced.Id)));
    }

    [Fact]
    public async Task OrderCorrectedBetweenThePickAndTheLock_IsJudgedAgain_AndSkipped()
    {
        // The picked order is an unpaid Failed one; before the lock a payment arrives (a late card payment): under the lock the part reads it again and the
        // order is now a Z-28 order, not due.
        Order order = await SeedFailedOrderAsync(failedAt: OrderRetention.FailedBefore(_harness.Now) - TimeSpan.FromDays(1));

        await _harness.RunOrderPartAsync(async (_, _) => await SeedCardPaymentAsync(order.Id, transitionId: null), Ct);

        Assert.True(await HasPersonalDataAsync(order.Id));
        Assert.Equal(0, await DisposalCountAsync());
    }

    // ---- KK3 and chunks: one transaction and one row per chunk ----

    [Fact]
    public async Task MoreOrdersThanOnePart_AreDisposedInChunks_EachWithItsOwnRow()
    {
        int total = DisposalTimings.PartSize + 1;
        await SeedManyPaidOrdersAsync(total);

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM orders WHERE contact_email IS NOT NULL"));
        Assert.Equal(total, await DisposalCountAsync());
        Assert.Equal(2, await ScalarAsync<long>("SELECT count(*) FROM disposal_records WHERE disposal_record_type = 'Order'"));
    }

    [Fact]
    public async Task PartWithNothingToDispose_WritesNoRow()
    {
        await SeedPaidOrderAsync(_harness.Now - TimeSpan.FromDays(10));

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM disposal_records"));
    }

    [Fact]
    public async Task APartThatFails_UndoesTheWholeChunk_AndTheNextRunDoesIt()
    {
        Order order = await SeedPaidOrderAsync(PastPaidPeriod);

        // The seam throws after the pick: nothing of the chunk is written, no row either.
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _harness.RunOrderPartAsync((_, _) => throw new InvalidOperationException("boom"), Ct));
        Assert.True(await HasPersonalDataAsync(order.Id));
        Assert.Equal(0, await ScalarAsync<long>("SELECT count(*) FROM disposal_records"));

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.False(await HasPersonalDataAsync(order.Id));
    }

    // ---- KK4: the order's own fields ----

    [Fact]
    public async Task OrderFields_EmptyTogether_TheAccessKeyIsEmptyAndTheConfirmationKeyStays()
    {
        Customer customer = await SeedCustomerAsync();
        Order order = await SeedPaidOrderAsync(PastPaidPeriod, customer.Id);
        string confirmationKey = order.ConfirmationKey;

        await _harness.RunOrderPartAsync(null, Ct);

        IReadOnlyList<string?> personal = await ReadStringsAsync(
            "SELECT contact_email, normalized_contact_email, order_access_key, ip_address::text, customer_id::text, cart_id::text, "
            + "shipping_recipient_name, shipping_phone, shipping_province_id::text, shipping_district, shipping_address_line, "
            + "billing_recipient_name, billing_province_id::text, billing_district, billing_address_line FROM orders WHERE id = @id",
            ("id", order.Id));
        Assert.All(personal, value => Assert.Null(value));
        Assert.Equal(confirmationKey, await ScalarAsync<string>("SELECT confirmation_key FROM orders WHERE id = @id", ("id", order.Id)));
    }

    [Fact]
    public async Task OrderCommercialFields_StayTheSame()
    {
        Order order = await SeedPaidOrderAsync(PastPaidPeriod);
        string before = await OrderCommercialFingerprintAsync(order.Id);

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.Equal(before, await OrderCommercialFingerprintAsync(order.Id));
    }

    [Fact]
    public async Task TheDatabaseCheck_RefusesEmptyingTheContactAlone()
    {
        Order order = await SeedPaidOrderAsync(PastPaidPeriod);

        PostgresException error = await Assert.ThrowsAsync<PostgresException>(
            () => ExecuteAsync("UPDATE orders SET contact_email = NULL WHERE id = '" + order.Id + "'"));

        Assert.Equal(PostgresErrorCodes.CheckViolation, error.SqlState);
    }

    // ---- KK5, KK6, KK7: the linked records ----

    [Fact]
    public async Task LinkedRecords_AreEmptiedDeletedOrLeftAsTheListSays()
    {
        Rich rich = await SeedRichOrderAsync();
        Rich neighbour = await SeedRichOrderAsync(paidAt: _harness.Now - TimeSpan.FromDays(10));
        string[] untouched = ["payment_records", "refund_records", "dispatch_records", "checkbox_acceptances", "checkbox_acceptance_order_items", "inbound_notification_records"];
        string before = await FingerprintAsync(untouched, rich.OrderId);
        string neighbourBefore = await FingerprintAllAsync(neighbour.OrderId);

        await _harness.RunOrderPartAsync(null, Ct);

        // KK5: the administrator's name and link, the free notes and the file link are empty.
        Assert.Equal(0, await CountAsync("order_items", "digital_file_id IS NOT NULL", rich.OrderId, "order_id"));
        Assert.Equal(0, await CountAsync("transition_records", "admin_id IS NOT NULL OR admin_name IS NOT NULL OR reason_note IS NOT NULL", rich.OrderId, "order_id"));
        Assert.Equal(0, await ScalarAsync<long>(
            "SELECT count(*) FROM cancellations WHERE order_item_id = @item AND (admin_id IS NOT NULL OR admin_name IS NOT NULL OR reason_note IS NOT NULL OR refund_iban_id IS NOT NULL)",
            ("item", rich.ItemId)));
        foreach (string table in new[] { "item_removals", "returns", "return_rejections", "not_returned_closures", "late_delivery_terminations", "withdrawals" })
        {
            Assert.Equal(1, await ScalarAsync<long>($"SELECT count(*) FROM {table} WHERE order_item_id = @item", ("item", rich.ItemId)));
            Assert.Equal(0, await ScalarAsync<long>(
                $"SELECT count(*) FROM {table} WHERE order_item_id = @item AND (admin_id IS NOT NULL OR admin_name IS NOT NULL)",
                ("item", rich.ItemId)));
        }

        Assert.Equal(0, await ScalarAsync<long>(
            "SELECT count(*) FROM late_delivery_terminations WHERE order_item_id = @item AND refund_iban_id IS NOT NULL", ("item", rich.ItemId)));
        Assert.Equal(0, await ScalarAsync<long>(
            "SELECT count(*) FROM withdrawals WHERE order_item_id = @item AND refund_iban_id IS NOT NULL", ("item", rich.ItemId)));
        Assert.Equal(0, await CountAsync("refunds", "admin_id IS NOT NULL OR admin_name IS NOT NULL", rich.OrderId, "order_id"));

        // KK6: the deleted records are gone from a query.
        Assert.Equal(0, await CountAsync("internal_notes", "true", rich.OrderId, "order_id"));
        Assert.Equal(0, await CountAsync("order_email_histories", "true", rich.OrderId, "order_id"));
        Assert.Equal(0, await CountAsync("order_address_histories", "true", rich.OrderId, "order_id"));
        Assert.Equal(0, await CountAsync("refund_ibans", "true", rich.OrderId, "order_id"));
        Assert.Equal(0, await ScalarAsync<long>(
            "SELECT count(*) FROM refund_iban_requests WHERE order_item_id = @item OR refund_id = @refund", ("item", rich.ItemId), ("refund", rich.RefundId)));

        // KK7: the payment, the refund record, the dispatch rows and the checkbox rows are the same; the refund itself stays with its amount.
        Assert.Equal(before, await FingerprintAsync(untouched, rich.OrderId));
        Assert.Equal(1, await CountAsync("refunds", "amount = 50", rich.OrderId, "order_id"));
        Assert.Equal(1, await CountAsync("order_items", "true", rich.OrderId, "order_id"));
        Assert.Equal(2, await CountAsync("transition_records", "true", rich.OrderId, "order_id"));

        // The neighbour order, not yet due, is untouched in every table.
        Assert.Equal(neighbourBefore, await FingerprintAllAsync(neighbour.OrderId));
    }

    [Fact]
    public async Task ItemRecords_KeepTheirAmountsDatesAndLinksToTheirRefunds()
    {
        Rich rich = await SeedRichOrderAsync();
        string before = await ScalarAsync<string>(
            "SELECT string_agg(concat_ws('|', product_amount, coupon_share, vat_amount, quantity, created_at), ';' ORDER BY id) FROM cancellations WHERE order_item_id = @item",
            ("item", rich.ItemId));

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.Equal(
            before,
            await ScalarAsync<string>(
                "SELECT string_agg(concat_ws('|', product_amount, coupon_share, vat_amount, quantity, created_at), ';' ORDER BY id) FROM cancellations WHERE order_item_id = @item",
                ("item", rich.ItemId)));
    }

    // ---- KK8: the unpaid order's record stays ----

    [Fact]
    public async Task UnpaidOrder_AfterZ41_KeepsItsRowWithoutPersonalData_AndDeletesNothing()
    {
        Order order = await SeedFailedOrderAsync(failedAt: OrderRetention.FailedBefore(_harness.Now) - TimeSpan.FromDays(1));
        long orders = await ScalarAsync<long>("SELECT count(*) FROM orders");
        long transitions = await ScalarAsync<long>("SELECT count(*) FROM transition_records WHERE order_id = @id", ("id", order.Id));

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.Equal(orders, await ScalarAsync<long>("SELECT count(*) FROM orders"));
        Assert.Equal(transitions, await ScalarAsync<long>("SELECT count(*) FROM transition_records WHERE order_id = @id", ("id", order.Id)));
        Assert.False(await HasPersonalDataAsync(order.Id));
        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM orders WHERE id = @id AND payment_status = 'Failed'", ("id", order.Id)));
    }

    [Fact]
    public async Task UnpaidOrder_PastTenYears_IsStillThereWithoutPersonalData()
    {
        // The Z-28 clock never touches the unpaid order's row (06 9.2.3): a later run changes nothing.
        Order order = await SeedFailedOrderAsync(failedAt: PastPaidPeriod, createdAt: PastPaidPeriod);

        await _harness.RunOrderPartAsync(null, Ct);
        _harness.Time.SetUtcNow(_harness.Now.AddYears(15));
        await _harness.RunOrderPartAsync(null, Ct);

        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM orders WHERE id = @id", ("id", order.Id)));
        Assert.False(await HasPersonalDataAsync(order.Id));
        Assert.Equal(1, await DisposalCountAsync());
    }

    // ---- KK9: the disposal row ----

    [Fact]
    public async Task DisposalRow_HasTheOrderTypeAndTheNumber_AndTheNoteAndHistoryDeletionsWriteNoRowOfTheirOwn()
    {
        await SeedRichOrderAsync();
        await SeedPaidOrderAsync(PastPaidPeriod);

        await _harness.RunOrderPartAsync(null, Ct);

        Assert.Equal(1, await ScalarAsync<long>("SELECT count(*) FROM disposal_records"));
        Assert.Equal(2, await ScalarAsync<long>("SELECT sum(count) FROM disposal_records WHERE disposal_record_type = 'Order'"));
    }

    // ---- The module surfaces join the caller's transaction ----

    [Fact]
    public async Task ThePaymentSurface_RefusesToEmptyOutsideATransaction()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _harness.WithServiceAsync<Shopfolio.Modules.Payment.Contracts.IPaymentModule, IReadOnlyList<Guid>>(
                payment => payment.ErasePersonalDataAsync([Guid.NewGuid()], Ct)));
    }

    [Fact]
    public async Task TheAfterSalesSurface_RefusesToEmptyOutsideATransaction()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _harness.WithServiceAsync<Shopfolio.Modules.AfterSales.Contracts.IAfterSalesModule, bool>(
                async afterSales =>
                {
                    await afterSales.ErasePersonalDataAsync(new OrderPersonalDataScope([Guid.NewGuid()], [], []), Ct);
                    return true;
                }));
    }

    // ---- seeds ----

    private async Task<Order> SeedOrderAsync(DateTimeOffset createdAt, Guid? customerId = null)
    {
        int n = Interlocked.Increment(ref _orderCounter);
        Order order = Order.Create(
            $"2026-{n:D6}", $"access-{n}-{Guid.NewGuid():N}", $"confirm-{n}-{Guid.NewGuid():N}", PaymentMethod.Card, customerId is null, "Ali@Example.com", 120m, 0m,
            createdAt.AddHours(1), createdAt);
        order.CustomerId = customerId;
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.ShippingRecipientName = "Ali Veli";
        order.ShippingPhone = "5321234567";
        order.ShippingProvinceId = _provinceId;
        order.ShippingDistrict = "Çankaya";
        order.ShippingAddressLine = "Atatürk Bulvarı 1";
        order.BillingRecipientName = "Ali Veli";
        order.BillingProvinceId = _provinceId;
        order.BillingDistrict = "Çankaya";
        order.BillingAddressLine = "Atatürk Bulvarı 1";
        order.CompanyType = CompanyType.Tradesman;
        order.CompanyLegalName = "Örnek Ticaret";
        order.CompanyTaxIdNumber = "1234567890";
        order.CompanyProvinceId = _provinceId;
        order.CompanyDistrict = "Çankaya";
        order.CompanyAddressLine = "Kızılay 2";
        order.CompanyPhone = "3121234567";
        order.CompanyEmail = "firma@example.com";
        order.CompanyKepAddress = "firma@hs01.kep.tr";
        order.PreInformationFormVersionId = _formId;
        order.DistanceSalesContractVersionId = _contractId;
        order.PrivacyNoticeVersionId = _noticeId;
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        await _harness.SeedAsync(db => db.Orders.Add(order));
        return order;
    }

    /// <summary>A paid order: an uncorrected card payment record (06 4.32) with its confirming transition, and the payment axis set the way the transition unit would.</summary>
    private async Task<Order> SeedPaidOrderAsync(DateTimeOffset createdAt, Guid? customerId = null)
    {
        Order order = await SeedOrderAsync(createdAt, customerId);
        TransitionRecord confirming = TransitionRecord.Create(order.Id, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, createdAt);
        await _harness.SeedAsync(db => db.TransitionRecords.Add(confirming));
        await SeedCardPaymentAsync(order.Id, confirming.Id);
        await ExecuteAsync("UPDATE orders SET payment_status = 'Paid' WHERE id = '" + order.Id + "'");
        return order;
    }

    /// <summary>An order that failed at <paramref name="failedAt"/>: Failed and Cancelled, with the Ö2 transition; no payment record.</summary>
    private async Task<Order> SeedFailedOrderAsync(DateTimeOffset failedAt, DateTimeOffset? createdAt = null)
    {
        Order order = await SeedOrderAsync(createdAt ?? failedAt - TimeSpan.FromHours(2));
        TransitionRecord failed = TransitionRecord.Create(
            order.Id, TransitionCode.O2, ActorKind.System, null, null, TransitionReason.CardPaymentFailed, null, null, null, null, failedAt);
        await _harness.SeedAsync(db => db.TransitionRecords.Add(failed));
        await ExecuteAsync("UPDATE orders SET payment_status = 'Failed', order_status = 'Cancelled' WHERE id = '" + order.Id + "'");
        return order;
    }

    private async Task SeedCardPaymentAsync(Guid orderId, Guid? transitionId)
    {
        PaymentRecord payment = PaymentRecord.ForCardPayment(orderId, "txn-" + Guid.NewGuid().ToString("N"), transitionId, _harness.Now);
        await _harness.SeedAsync(db => db.PaymentRecords.Add(payment));
    }

    /// <summary>A bank transfer's "paid" mark: the payment record with its Ö1, and <paramref name="corrections"/> correction rows that correct that Ö1.</summary>
    private async Task SeedBankTransferMarkAsync(Guid orderId, Admin admin, int corrections)
    {
        TransitionRecord confirming = TransitionRecord.Create(orderId, TransitionCode.O1, ActorKind.Admin, admin.Id, admin.Name, null, null, null, null, null, _harness.Now);
        await _harness.SeedAsync(db => db.TransitionRecords.Add(confirming));
        PaymentRecord mark = PaymentRecord.ForBankTransferMark(orderId, confirming.Id, _harness.Now);
        await _harness.SeedAsync(db => db.PaymentRecords.Add(mark));
        for (int i = 0; i < corrections; i++)
        {
            TransitionRecord correction = TransitionRecord.Create(
                orderId, TransitionCode.Correction, ActorKind.Admin, admin.Id, admin.Name, null, null, CorrectionReason.WrongOrder, null, confirming.Id, _harness.Now);
            await _harness.SeedAsync(db => db.TransitionRecords.Add(correction));
        }
    }

    private async Task<Admin> SeedAdminAsync()
    {
        Admin admin = Admin.Create("Veli", $"veli-{EntityIds.New():N}@example.com", "digest-placeholder", _harness.Now);
        await _harness.SeedAsync(db => db.Admins.Add(admin));
        return admin;
    }

    private async Task<Customer> SeedCustomerAsync()
    {
        Customer customer = Customer.Create("Ali", $"ali-{EntityIds.New():N}@example.com", "digest-placeholder", _harness.Now, null, _harness.Now);
        await _harness.SeedAsync(db => db.Customers.Add(customer));
        return customer;
    }

    private async Task SeedManyPaidOrdersAsync(int count)
    {
        List<Order> orders = [];
        List<TransitionRecord> transitions = [];
        List<PaymentRecord> payments = [];
        for (int i = 0; i < count; i++)
        {
            int n = Interlocked.Increment(ref _orderCounter);
            Order order = Order.Create(
                $"2015-{n:D6}", $"access-{n}-{Guid.NewGuid():N}", $"confirm-{n}-{Guid.NewGuid():N}", PaymentMethod.Card, true, "Ali@Example.com", 120m, 0m,
                PastPaidPeriod.AddHours(1), PastPaidPeriod);
            order.BillingRecipientName = "Ali Veli";
            order.BillingProvinceId = _provinceId;
            order.BillingDistrict = "Çankaya";
            order.BillingAddressLine = "Atatürk Bulvarı 1";
            order.IpAddress = IPAddress.Parse("203.0.113.5");
            order.CartId = EntityIds.New();
            order.CompanyType = CompanyType.Tradesman;
            order.CompanyLegalName = "Örnek Ticaret";
            order.CompanyProvinceId = _provinceId;
            order.CompanyDistrict = "Çankaya";
            order.CompanyAddressLine = "Kızılay 2";
            order.CompanyPhone = "3121234567";
            order.CompanyEmail = "firma@example.com";
            order.CompanyKepAddress = "firma@hs01.kep.tr";
            order.PreInformationFormVersionId = _formId;
            order.DistanceSalesContractVersionId = _contractId;
            order.PrivacyNoticeVersionId = _noticeId;
            order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
            orders.Add(order);
            TransitionRecord confirming = TransitionRecord.Create(order.Id, TransitionCode.O1, ActorKind.System, null, null, null, null, null, null, null, PastPaidPeriod);
            transitions.Add(confirming);
            payments.Add(PaymentRecord.ForCardPayment(order.Id, "txn-" + Guid.NewGuid().ToString("N"), confirming.Id, PastPaidPeriod));
        }

        await _harness.SeedAsync(db => db.Orders.AddRange(orders));
        await _harness.SeedAsync(db => db.TransitionRecords.AddRange(transitions));
        await _harness.SeedAsync(db => db.PaymentRecords.AddRange(payments));
        await ExecuteAsync("UPDATE orders SET payment_status = 'Paid'");
    }

    /// <summary>One order with every kind of linked record the list names; paid at <paramref name="paidAt"/> (the order is created then).</summary>
    private async Task<Rich> SeedRichOrderAsync(DateTimeOffset? paidAt = null)
    {
        DateTimeOffset at = paidAt ?? PastPaidPeriod;
        Admin admin = await SeedAdminAsync();
        Customer customer = await SeedCustomerAsync();
        Order order = await SeedPaidOrderAsync(at, customer.Id);
        DigitalFile file = DigitalFile.Create(EntityIds.NewStorageKey(), "kitap.pdf", 100, at);
        await _harness.SeedAsync(db => db.DigitalFiles.Add(file));
        OrderItem item = OrderItem.Create(order.Id, null, null, ProductType.Digital, "Kitap", 1, 100m, 20m, 16.67m, at);
        item.DownloadAllowance = 3;
        item.DigitalFileId = file.Id;
        await _harness.SeedAsync(db => db.OrderItems.Add(item));

        ItemAmounts amounts = new(100m, 10m, 15m);
        RefundIban iban = RefundIban.Create(order.Id, "TR330006100519786457841326", at);
        await _harness.SeedAsync(db => db.RefundIbans.Add(iban));
        TransitionRecord reasoned = TransitionRecord.Create(
            order.Id, TransitionCode.S3, ActorKind.Admin, admin.Id, admin.Name, null, CancellationReason.Other, null, "Müşteri aradı", null, at);
        await _harness.SeedAsync(db => db.TransitionRecords.Add(reasoned));

        Cancellation cancellation = Cancellation.ByAdmin(item.Id, 1, admin.Id, admin.Name, CancellationReason.Other, "Telefon: 0532 000 00 00", amounts, at);
        Withdrawal withdrawal = Withdrawal.ByBuyer(item.Id, 1, Today, null, amounts, iban.Id, at);
        LateDeliveryTermination termination = LateDeliveryTermination.ByBuyer(item.Id, 1, Today, amounts, iban.Id, at);
        ItemRemoval removal = ItemRemoval.Create(item.Id, 1, admin.Id, admin.Name, amounts, at);
        await _harness.SeedAsync(db => db.AddRange(cancellation, withdrawal, termination, removal));
        Return receipt = Return.ForWithdrawal(item.Id, withdrawal.Id, 1, Today, admin.Id, admin.Name, at);
        ReturnRejection rejection = ReturnRejection.Create(item.Id, withdrawal.Id, 1, Today, admin.Id, admin.Name, at);
        NotReturnedClosure closure = NotReturnedClosure.Create(item.Id, withdrawal.Id, 1, admin.Id, admin.Name, at);
        await _harness.SeedAsync(db => db.AddRange(receipt, rejection, closure));

        Refund refund = Refund.ByAdmin(order.Id, RefundType.AmountBased, Shopfolio.Modules.Payment.Contracts.PaymentMethod.BankTransfer, 50m, admin.Id, admin.Name, at);
        await _harness.SeedAsync(db => db.Refunds.Add(refund));
        RefundRecord refundRecord = RefundRecord.Sending(refund.Id, at);
        RefundIbanRequest itemRequest = RefundIbanRequest.ForItem(item.Id, RefundIbanRequestReason.AdminRequest, null, admin.Id, admin.Name, at);
        RefundIbanRequest refundRequest = RefundIbanRequest.ForRefund(refund.Id, RefundIbanRequestReason.TransferFailed, admin.Id, admin.Name, at);
        InternalNote note = InternalNote.Create(order.Id, "Aranacak: 0532 000 00 00", admin.Id, admin.Name, at);
        OrderEmailHistory emailHistory = OrderEmailHistory.Create(order.Id, "eski@example.com", "Ali@Example.com", admin.Id, admin.Name, at);
        OrderAddressHistory addressHistory = OrderAddressHistory.Create(
            order.Id,
            AddressType.ShippingAddress,
            ("Eski Ad", "5321111111", _provinceId, "Çankaya", "Eski adres"),
            ("Ali Veli", "5320000000", _provinceId, "Çankaya", "Atatürk Bulvarı 1"),
            admin.Id,
            admin.Name,
            at);
        CheckboxAcceptance acceptance = CheckboxAcceptance.Create(order.Id, CheckboxType.Digital, "Metin", at);
        DispatchRecord dispatch = DispatchRecord.Queue(DispatchType.B1, "ali@example.com", order.Id, null, null, at);
        await _harness.SeedAsync(db => db.AddRange(refundRecord, itemRequest, refundRequest, note, emailHistory, addressHistory, acceptance, dispatch));
        await _harness.SeedAsync(db => db.CheckboxAcceptanceOrderItems.Add(CheckboxAcceptanceOrderItem.Create(acceptance.Id, item.Id, at)));
        await _harness.SeedAsync(db => db.InboundNotificationRecords.Add(InboundNotificationRecord.Create("n-" + Guid.NewGuid().ToString("N"), null, "payment", "processed", order.OrderNumber, 120m, at)));
        return new Rich(order.Id, item.Id, refund.Id);
    }

    // ---- reads ----

    private async Task<bool> HasPersonalDataAsync(Guid orderId)
    {
        return await ScalarAsync<bool>("SELECT contact_email IS NOT NULL FROM orders WHERE id = @id", ("id", orderId));
    }

    private Task<int> DisposalCountAsync()
    {
        return ScalarAsync<int>("SELECT coalesce(sum(count), 0)::int FROM disposal_records WHERE disposal_record_type = 'Order'");
    }

    private Task<long> CountAsync(string table, string condition, Guid key, string keyColumn)
    {
        return ScalarAsync<long>($"SELECT count(*) FROM {table} WHERE {keyColumn} = @key AND ({condition})", ("key", key));
    }

    /// <summary>The order's commercial fields as one string: every column the disposal must leave alone.</summary>
    private Task<string> OrderCommercialFingerprintAsync(Guid orderId)
    {
        return ScalarAsync<string>(
            "SELECT concat_ws('|', created_at, order_number, confirmation_key, order_status, payment_status, payment_method, is_guest_order, is_account_deleted, "
            + "coupon_id, coupon_code, total, shipping_fee, shipping_fee_vat_breakdown, bank_transfer_iban, shipping_period, payment_window_ends_at, payment_reminder_at, "
            + "carrier, other_carrier_name, tracking_number, is_own_vehicle_delivery, company_type, company_legal_name, company_tax_id_number, company_province_id, "
            + "company_district, company_address_line, company_phone, company_email, company_kep_address, pre_information_form_version_id, "
            + "distance_sales_contract_version_id, privacy_notice_version_id, pre_information_form_text, distance_sales_contract_text) FROM orders WHERE id = @id",
            ("id", orderId));
    }

    /// <summary>The rows of the tables that belong to the order (by its id, its items or its refunds), as one hash-able string.</summary>
    private async Task<string> FingerprintAsync(string[] tables, Guid orderId)
    {
        List<string> parts = [];
        foreach (string table in tables)
        {
            string sql = table switch
            {
                "payment_records" => "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM payment_records t WHERE order_id = @id",
                "refund_records" => "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM refund_records t WHERE refund_id IN (SELECT id FROM refunds WHERE order_id = @id)",
                "dispatch_records" => "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM dispatch_records t WHERE order_id = @id",
                "checkbox_acceptances" => "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM checkbox_acceptances t WHERE order_id = @id",
                "checkbox_acceptance_order_items" => "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM checkbox_acceptance_order_items t WHERE checkbox_acceptance_id IN (SELECT id FROM checkbox_acceptances WHERE order_id = @id)",
                _ => "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM inbound_notification_records t",
            };
            parts.Add(await ScalarAsync<string>(sql, ("id", orderId)));
        }

        return string.Join("#", parts);
    }

    /// <summary>Every row of the order and of the records linked to it, as one string; the not-yet-due neighbour must read the same before and after.</summary>
    private async Task<string> FingerprintAllAsync(Guid orderId)
    {
        string[] queries =
        [
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM orders t WHERE id = @id",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM order_items t WHERE order_id = @id",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM transition_records t WHERE order_id = @id",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM internal_notes t WHERE order_id = @id",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM order_email_histories t WHERE order_id = @id",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM order_address_histories t WHERE order_id = @id",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM refund_ibans t WHERE order_id = @id",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM refunds t WHERE order_id = @id",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM cancellations t WHERE order_item_id IN (SELECT id FROM order_items WHERE order_id = @id)",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM withdrawals t WHERE order_item_id IN (SELECT id FROM order_items WHERE order_id = @id)",
            "SELECT coalesce(string_agg(t::text, ';' ORDER BY id), '') FROM refund_iban_requests t WHERE order_item_id IN (SELECT id FROM order_items WHERE order_id = @id)",
        ];
        List<string> parts = [];
        foreach (string query in queries)
        {
            parts.Add(await ScalarAsync<string>(query, ("id", orderId)));
        }

        return string.Join("#", parts);
    }

    // ---- database helpers ----

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _collection.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task ExecuteAsync(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<T> ScalarAsync<T>(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    private async Task<IReadOnlyList<string?>> ReadStringsAsync(string sql, params (string Name, object Value)[] parameters)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        foreach ((string name, object value) in parameters)
        {
            command.Parameters.AddWithValue(name, value);
        }

        await using NpgsqlDataReader reader = await command.ExecuteReaderAsync(Ct);
        Assert.True(await reader.ReadAsync(Ct));
        List<string?> values = [];
        for (int i = 0; i < reader.FieldCount; i++)
        {
            values.Add(await reader.IsDBNullAsync(i, Ct) ? null : Convert.ToString(reader.GetValue(i), System.Globalization.CultureInfo.InvariantCulture));
        }

        return values;
    }

    private sealed record Rich(Guid OrderId, Guid ItemId, Guid RefundId);
}
