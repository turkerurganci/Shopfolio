using System.Net;
using System.Reflection;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

using Npgsql;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Model;
using Shopfolio.IntegrationTests.Ordering;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Catalog;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

using Sales = Shopfolio.Modules.AfterSales;

namespace Shopfolio.IntegrationTests.Administration;

/// <summary>The collection of the export tests (T384): one database copied from the template, the application opened on it.</summary>
[CollectionDefinition(DataExportDefinition.Name)]
public sealed class DataExportDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "DataExport";
}

/// <summary>
/// The three exports on a real PostgreSQL, through the application's own services (T384 KK1-KK9; 02 10.7; 09 10.10): the order file's set, lines, frozen values, payment, shipping and contact columns,
/// the late payment and the disposed order; the member list and the contact request list; the trail row written after the file and never for an empty set, a wrong range or a file past a ceiling; the
/// file's life in the temporary directory (made, closed, swept at start and at the next request, a directory that cannot be written refuses the start); and the pages of the reads. The pure rules
/// are the unit tests' (CsvExportWriterTests, OrderExportTests, DataExportRulesTests).
/// </summary>
[Collection(DataExportDefinition.Name)]
public sealed class DataExportTests : IAsyncLifetime
{
    private static readonly DateOnly Today = TurkeyTime.DateOf(DateTimeOffset.UtcNow);

    private readonly CollectionDatabaseFixture _collection;
    private readonly string _directory = Path.Combine(Path.GetTempPath(), "shopfolio-t384-" + Guid.NewGuid().ToString("N"));

    private ShopfolioWebApplicationFactory? _root;
    private WebApplicationFactory<Program>? _app;
    private Admin _admin = null!;
    private Guid _province;
    private CompanyIdentity _identity = null!;
    private Guid _shirt;
    private Guid _book;
    private int _counter;

    public DataExportTests(CollectionDatabaseFixture collection)
    {
        _collection = collection;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private IServiceProvider Services => (_app ??= OpenApplication(_directory)).Services;

    public async ValueTask InitializeAsync()
    {
        await ExecuteAsync(
            "TRUNCATE audit_logs, transition_records, checkbox_acceptance_order_items, checkbox_acceptances, order_email_histories, order_address_histories, order_items, orders, digital_files, "
            + "product_images, price_histories, discounts, variants, option_values, options, products, customers, contact_requests CASCADE");
        _admin = Admin.Create("Firma Yöneticisi", $"yonetici-{Guid.NewGuid():N}@example.com", "digest-placeholder", DateTimeOffset.UtcNow);
        await SaveAsync(c => c.Admins.Add(_admin));
        _province = await ProvinceAsync();
        _identity = await SetCompanyAsync("Örnek Ticaret", "Kızılay 2");
        _shirt = await SeedVariantAsync(500m, 20m);
        _book = await SeedVariantAsync(200m, 10m);
        Directory.CreateDirectory(_directory);
    }

    public async ValueTask DisposeAsync()
    {
        if (_app is not null)
        {
            await _app.DisposeAsync();
        }

        if (_root is not null)
        {
            await _root.DisposeAsync();
        }

        if (Directory.Exists(_directory))
        {
            Directory.Delete(_directory, recursive: true);
        }
    }

    // ---- KK1: the set - created in the range by the Turkey day, paid, nothing pending or failed ----

    [Fact]
    public async Task TheOrderFile_CarriesTheOrdersCreatedInTheRange_ThatWerePaid_AndNotThePendingTheFailedOrTheOnesOutsideTheRange_KK1()
    {
        string paid = await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 1, 0m));
        string pending = await SeedAsync(CreatedOn(-3), PaymentMethod.BankTransfer, "Pending", (_shirt, 1, 0m));
        string failed = await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Failed", (_shirt, 1, 0m));
        string paidLater = await SeedAsync(CreatedOn(-5), PaymentMethod.BankTransfer, "Paid", (_shirt, 1, 0m));
        string outside = await SeedAsync(CreatedOn(-10), PaymentMethod.Card, "Paid", (_shirt, 1, 0m));
        string partly = await SeedAsync(CreatedOn(-2), PaymentMethod.Card, "PartiallyRefunded", (_shirt, 1, 0m));
        string refunded = await SeedAsync(CreatedOn(-2), PaymentMethod.Card, "Refunded", (_shirt, 1, 0m));

        Exported wide = await ExportOrdersAsync(Today.AddDays(-5), Today.AddDays(-2));
        Exported oneDay = await ExportOrdersAsync(Today.AddDays(-3), Today.AddDays(-3));

        Assert.Equal(new[] { paid, paidLater, partly, refunded }.Order(StringComparer.Ordinal), wide.Values("Sipariş numarası").Distinct().Order(StringComparer.Ordinal));
        Assert.DoesNotContain(wide.Values("Sipariş numarası"), n => n == pending || n == failed || n == outside);
        Assert.Equal([paid], oneDay.Values("Sipariş numarası").Distinct());
    }

    [Fact]
    public async Task TheRange_IsTheCreationDayInTurkey_TheLastSecondBeforeMidnightIsIn_MidnightItselfIsOut_KK1()
    {
        DateTimeOffset midnight = TurkeyTime.StartOf(Today.AddDays(-3));
        string lastSecond = await SeedAsync(midnight.AddSeconds(-1), PaymentMethod.Card, "Paid", (_shirt, 1, 0m));
        string firstSecond = await SeedAsync(midnight, PaymentMethod.Card, "Paid", (_shirt, 1, 0m));

        Exported before = await ExportOrdersAsync(Today.AddDays(-4), Today.AddDays(-4));
        Exported after = await ExportOrdersAsync(Today.AddDays(-3), Today.AddDays(-3));

        Assert.Equal([lastSecond], before.Values("Sipariş numarası"));
        Assert.Equal([firstSecond], after.Values("Sipariş numarası"));
    }

    // ---- KK2: a line is an item, the order's values repeat ----

    [Fact]
    public async Task ALine_IsAnOrderItem_AndTheOrdersValuesRepeatOnEachLine_KK2()
    {
        string number = await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 2, 20m), (_book, 1, 0m));

        Exported file = await ExportOrdersAsync(Today.AddDays(-3), Today.AddDays(-3));

        Assert.Equal(2, file.Rows.Count);
        Assert.Equal(["Ürün 1", "Ürün 2"], file.Values("Ürün adı").Order(StringComparer.Ordinal));
        Assert.Equal(["1", "2"], file.Values("Adet").Order(StringComparer.Ordinal));
        foreach (string header in new[] { "Sipariş numarası", "Sipariş tarihi", "Sipariş toplamı", "Kargo ücreti", "Ticaret unvanı", "Fatura adresi" })
        {
            Assert.Single(file.Values(header).Distinct());
        }

        Assert.Equal(number, file.Values("Sipariş numarası")[0]);
    }

    // ---- KK3: the invoice columns come from the frozen values ----

    [Fact]
    public async Task TheInvoiceColumns_AreTheFrozenValues_AfterTheProductAndTheFirmsIdentityChanged_KK3()
    {
        await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 2, 20m));
        Exported before = await ExportOrdersAsync(Today.AddDays(-3), Today.AddDays(-3));

        await ExecuteAsync("UPDATE products SET vat_rate = 1; UPDATE variants SET price = price + 7; UPDATE companies SET legal_name = 'Yeni Unvan', address_line = 'Yeni Sokak 9'");
        Exported after = await ExportOrdersAsync(Today.AddDays(-3), Today.AddDays(-3));

        Assert.Equal(before.Bytes, after.Bytes);
        Assert.Equal("Örnek Ticaret", after.Values("Ticaret unvanı")[0]);
        Assert.Equal("Kızılay 2", after.Values("Firma adresi")[0]);
        Assert.Equal("500,00", after.Values("Birim fiyat")[0]);
        Assert.Equal("20", after.Values("KDV oranı")[0]);
        Assert.Equal("20,00", after.Values("Kupon payı")[0]);
        Assert.Equal("Ali Veli", after.Values("Fatura alıcı adı")[0]);
        Assert.Equal("Çankaya", after.Values("Fatura ilçesi")[0]);
        Assert.Equal("Atatürk Bulvarı 1", after.Values("Fatura adresi")[0]);
        Assert.NotEqual(string.Empty, after.Values("Fatura ili")[0]);
        Assert.Equal("1111111111", after.Values("Vergi numarası")[0]);
        Assert.Matches(@"^\d+=\d+,\d{2}$", after.Values("Kargo ücretinin KDV dökümü")[0]);
    }

    // ---- KK4: payment, shipping and contact columns ----

    [Fact]
    public async Task ThePaymentShippingAndContactColumns_AreFilled_FromTheTransitionsAndTheFrozenContact_KK4()
    {
        CarrierEntry carrier = CarrierCatalog.Default.Active[0];
        string number = await SeedAsync(
            CreatedOn(-3),
            PaymentMethod.Card,
            "Paid",
            o => { o.Carrier = carrier.Code; o.TrackingNumber = "123456"; },
            [(_shirt, 1, 0m), (_book, 1, 0m)]);
        Guid id = await OrderIdAsync(number);
        DateTimeOffset approvedAt = TurkeyTime.StartOf(Today.AddDays(-3)).AddHours(11);
        DateTimeOffset shippedAt = TurkeyTime.StartOf(Today.AddDays(-2)).AddHours(15);
        await SaveAsync(c => c.TransitionRecords.AddRange(Record(id, TransitionCode.O1, approvedAt), Record(id, TransitionCode.S5, shippedAt)));

        Exported file = await ExportOrdersAsync(Today.AddDays(-3), Today.AddDays(-3));

        Assert.Equal(["Kart"], file.Values("Ödeme yöntemi").Distinct());
        Assert.Equal([Day(Today.AddDays(-3))], file.Values("Ödeme onay tarihi").Distinct());
        Assert.Equal([Day(Today.AddDays(-2))], file.Values("Kargoya verme tarihi").Distinct());
        Assert.Equal([carrier.Name], file.Values("Kargo şirketi").Distinct());
        Assert.Equal(["localhost"], file.Values("Sitenin alan adı").Distinct());
        Assert.Equal(["ali@example.com"], file.Values("İletişim e-postası").Distinct());
        Assert.Equal(["Ali Veli"], file.Values("Teslimat alıcı adı").Distinct());
    }

    [Fact]
    public async Task TheBankTransferOrder_ShowsTheTransferTerm_AndAnUnshippedOrderHasNoShippingDay_KK4()
    {
        await SeedAsync(CreatedOn(-3), PaymentMethod.BankTransfer, "Paid", (_shirt, 1, 0m));

        Exported file = await ExportOrdersAsync(Today.AddDays(-3), Today.AddDays(-3));

        Assert.Equal(["Havale/EFT"], file.Values("Ödeme yöntemi"));
        Assert.Equal([string.Empty], file.Values("Kargoya verme tarihi"));
        Assert.Equal([string.Empty], file.Values("Kargo şirketi"));
    }

    // ---- KK4 / D-736: the shipping day of an item closed before the shipment (09 10.10 item 5) ----

    [Fact]
    public async Task AnItemWhoseWholeQuantityWasCancelledBeforeTheShipment_HasNoShippingDay_KK4()
    {
        Exported file = await ExportShippedItemAsync(2, [(2, false)]);

        Assert.Equal([string.Empty], file.Values("Kargoya verme tarihi"));
    }

    [Fact]
    public async Task AnItemWhoseQuantityWasCancelledBeforeTheShipmentInTwoRecords_HasNoShippingDay_KK4()
    {
        Exported file = await ExportShippedItemAsync(2, [(1, false), (1, false)]);

        Assert.Equal([string.Empty], file.Values("Kargoya verme tarihi"));
    }

    [Fact]
    public async Task AnItemCancelledInPartBeforeTheShipment_KeepsTheShippingDay_KK4()
    {
        Exported file = await ExportShippedItemAsync(2, [(1, false)]);

        Assert.Equal([Day(Today.AddDays(-2))], file.Values("Kargoya verme tarihi"));
    }

    [Fact]
    public async Task AnItemCancelledInPartBeforeTheShipment_AndWithdrawnAfterIt_KeepsTheShippingDay_KK4()
    {
        Exported file = await ExportShippedItemAsync(2, [(1, false), (1, true)]);

        Assert.Equal([Day(Today.AddDays(-2))], file.Values("Kargoya verme tarihi"));
    }

    [Fact]
    public async Task AnItemWithdrawnWholeAfterTheShipment_KeepsTheShippingDay_KK4()
    {
        Exported file = await ExportShippedItemAsync(2, [(2, true)]);

        Assert.Equal([Day(Today.AddDays(-2))], file.Values("Kargoya verme tarihi"));
    }

    // ---- KK5: a cancelled order that a late card payment came to, paid back ----

    [Fact]
    public async Task ACancelledOrder_ThatALateCardPaymentCameTo_HasTheTwoDatesAndAmounts_KK5()
    {
        string number = await SeedAsync(CreatedOn(-4), PaymentMethod.Card, "Failed", (_shirt, 1, 0m));
        Guid id = await OrderIdAsync(number);
        DateTimeOffset paidAt = TurkeyTime.StartOf(Today.AddDays(-2)).AddHours(14);
        DateTimeOffset sentAt = TurkeyTime.StartOf(Today.AddDays(-1)).AddHours(9);
        PaymentRecord payment = PaymentRecord.ForCardPayment(id, "late-" + Guid.NewGuid().ToString("N"), null, paidAt);
        Refund refund = Refund.ForLatePayment(id, payment.Id, 540m, paidAt.AddMinutes(1));
        refund.SentAt = sentAt;
        await SaveAsync(c =>
        {
            c.PaymentRecords.Add(payment);
            c.Refunds.Add(refund);
        });
        decimal total = await ScalarAsync<decimal>($"SELECT total FROM orders WHERE id = '{id}'");

        Exported file = await ExportOrdersAsync(Today.AddDays(-4), Today.AddDays(-4));

        Assert.Equal([Day(Today.AddDays(-2))], file.Values("Geç ödeme tarihi"));
        Assert.Equal([total.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture).Replace('.', ',')], file.Values("Geç ödeme tutarı"));
        Assert.Equal([Day(Today.AddDays(-1))], file.Values("Geri ödeme tarihi"));
        Assert.Equal(["540,00"], file.Values("Geri ödeme tutarı"));
        Assert.Equal([number], file.Values("Sipariş numarası"));
    }

    [Fact]
    public async Task ACancelledOrderWithoutALatePayment_IsNotInTheFile_AndAVoidedLateRefundDoesNotCount_KK5()
    {
        string plain = await SeedAsync(CreatedOn(-4), PaymentMethod.Card, "Failed", (_shirt, 1, 0m));
        string voided = await SeedAsync(CreatedOn(-4), PaymentMethod.Card, "Failed", (_shirt, 1, 0m));
        Guid id = await OrderIdAsync(voided);
        PaymentRecord payment = PaymentRecord.ForCardPayment(id, "late-" + Guid.NewGuid().ToString("N"), null, DateTimeOffset.UtcNow.AddDays(-2));
        Refund refund = Refund.ForLatePayment(id, payment.Id, 540m, DateTimeOffset.UtcNow.AddDays(-2));
        refund.VoidedAt = DateTimeOffset.UtcNow.AddDays(-1);
        await SaveAsync(c =>
        {
            c.PaymentRecords.Add(payment);
            c.Refunds.Add(refund);
        });

        ExportFileResult result = await RunAsync(e => e.ExportOrdersAsync(Today.AddDays(-4), Today.AddDays(-4), Ct));

        Assert.Equal(ExportOutcome.Empty, result.Outcome);
        Assert.NotEqual(plain, voided);
    }

    // ---- KK6: a disposed order stays, with the personal columns empty and the amounts kept ----

    [Fact]
    public async Task ADisposedOrder_StaysInTheFile_WithTheAddressEmailAndRecipientEmpty_AndTheAmountsAndTheFirmsIdentityKept_KK6()
    {
        string number = await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", o => o.ErasePersonalData(), [(_shirt, 2, 0m)]);

        Exported file = await ExportOrdersAsync(Today.AddDays(-3), Today.AddDays(-3));

        Assert.Equal([number], file.Values("Sipariş numarası"));
        foreach (string header in new[] { "Fatura alıcı adı", "Fatura ili", "Fatura ilçesi", "Fatura adresi", "İletişim e-postası", "Teslimat alıcı adı" })
        {
            Assert.Equal([string.Empty], file.Values(header));
        }

        Assert.NotEqual(string.Empty, file.Values("Sipariş toplamı")[0]);
        Assert.Equal("500,00", file.Values("Birim fiyat")[0]);
        Assert.Equal("2", file.Values("Adet")[0]);
        Assert.Equal("Örnek Ticaret", file.Values("Ticaret unvanı")[0]);
        Assert.Equal("Kızılay 2", file.Values("Firma adresi")[0]);
    }

    // ---- KK7: every member account and every kept contact request, no range ----

    [Fact]
    public async Task TheMemberList_HoldsEveryAccount_TheOneThatNeverOrderedToo_WithNameAndEmail_KK7()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Customer ordered = Customer.Create("Ali Veli", "ali@example.com", "digest-placeholder", now, null, now);
        Customer never = Customer.Create("Hiç Almayan", "hic@example.com", "digest-placeholder", now, null, now);
        Customer pending = Customer.Create("Doğrulanmamış", "bekleyen@example.com", "digest-placeholder", null, null, now);
        Customer formula = Customer.Create("=HYPERLINK(\"x\")", "formul@example.com", "digest-placeholder", now, null, now);
        await SaveAsync(c => c.Customers.AddRange(ordered, never, pending, formula));
        await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 1, 0m));

        Exported file = await ExportAsync(e => e.ExportCustomersAsync(Ct));

        Assert.Equal(["Ad", "E-posta"], file.Header);
        Assert.Equal(4, file.Rows.Count);
        Assert.Equal(
            ["ali@example.com", "bekleyen@example.com", "formul@example.com", "hic@example.com"],
            file.Values("E-posta").Order(StringComparer.Ordinal));
        Assert.Contains("hic@example.com", file.Values("E-posta"));
        Assert.Contains("Hiç Almayan", file.Values("Ad"));
        Assert.Contains("\"'=HYPERLINK(\"\"x\"\")\";formul@example.com", file.Text, StringComparison.Ordinal);
        Assert.Matches(@"^uyeler-\d{4}-\d{2}-\d{2}\.csv$", file.Result.FileName!);
        Assert.Equal("text/csv; charset=utf-8", ExportFileResult.ContentType);
    }

    [Fact]
    public async Task TheContactRequestList_HoldsEveryKeptRequest_OpenOrClosed_WithNameAndEmailOnly_KK7()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        ContactRequest open = ContactRequest.Create("Ayşe Yılmaz", "ayse@example.com", ContactTopic.GeneralQuestion, "Merhaba, gizli ileti metni", now, "5321234567");
        ContactRequest closed = ContactRequest.Create("Mehmet Kaya", "mehmet@example.com", ContactTopic.AboutOrder, "Sipariş sorusu", now.AddDays(-5));
        closed.ContactRequestStatus = ContactRequestStatus.Closed;
        closed.LastClosedAt = now.AddDays(-4);
        ContactRequest formula = ContactRequest.Create("+Ad", "@x@example.com", ContactTopic.Other, "ileti", now.AddDays(-1));
        await SaveAsync(c => c.ContactRequests.AddRange(open, closed, formula));

        Exported file = await ExportAsync(e => e.ExportContactRequestsAsync(Ct));

        Assert.Equal(["Ad", "E-posta"], file.Header);
        Assert.Equal(3, file.Rows.Count);
        Assert.Equal(["'+Ad", "Ayşe Yılmaz", "Mehmet Kaya"], file.Values("Ad").Order(StringComparer.Ordinal));
        Assert.DoesNotContain("gizli ileti metni", file.Text, StringComparison.Ordinal);
        Assert.DoesNotContain("5321234567", file.Text, StringComparison.Ordinal);
        Assert.Contains("'@x@example.com", file.Values("E-posta"));
        Assert.Matches(@"^iletisim-talepleri-\d{4}-\d{2}-\d{2}\.csv$", file.Result.FileName!);
    }

    [Fact]
    public async Task EmptyMemberAndContactRequestLists_AreTheEmptyOutcome_WithNoFileAndNoTrailRow_KK7_KK8()
    {
        ExportFileResult customers = await RunAsync(e => e.ExportCustomersAsync(Ct));
        ExportFileResult requests = await RunAsync(e => e.ExportContactRequestsAsync(Ct));

        Assert.Equal(ExportOutcome.Empty, customers.Outcome);
        Assert.Equal(ExportOutcome.Empty, requests.Outcome);
        Assert.Null(customers.File);
        Assert.Equal(0, await TrailCountAsync());
        Assert.Empty(Directory.GetFiles(_directory));
    }

    // ---- KK8: every download is in the trail, written after the file; the ones that gave no file are not ----

    [Fact]
    public async Task EveryDownload_IsInTheTrail_WithTheKindTheRangeAndTheCount_AndNoTargetRecord_KK8()
    {
        await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 1, 0m), (_book, 1, 0m));
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await SaveAsync(c =>
        {
            c.Customers.Add(Customer.Create("Ali", "ali@example.com", "digest-placeholder", now, null, now));
            c.ContactRequests.Add(ContactRequest.Create("Ayşe", "ayse@example.com", ContactTopic.Other, "ileti", now));
        });

        await ExportOrdersAsync(Today.AddDays(-4), Today.AddDays(-2));
        await ExportAsync(e => e.ExportCustomersAsync(Ct));
        await ExportAsync(e => e.ExportContactRequestsAsync(Ct));

        List<AuditLog> rows = await TrailAsync();
        Assert.Equal(["ExportOrders", "ExportCustomers", "ExportContactRequests"], rows.Select(r => r.Operation));
        Assert.All(rows, r =>
        {
            Assert.Equal(_admin.Id, r.AdminId);
            Assert.Equal("Firma Yöneticisi", r.AdminName);
            Assert.Null(r.TargetRecordType);
            Assert.Null(r.TargetRecordId);
            Assert.Equal(AuditArea.Export, r.AuditArea);
        });
        Assert.Equal(
            [("exportType", "Orders"), ("startDate", Day(Today.AddDays(-4))), ("endDate", Day(Today.AddDays(-2))), ("count", "2")],
            rows[0].Details!.Values.Select(v => (v.Name, v.Value!)));
        Assert.Equal([("exportType", "Customers"), ("count", "1")], rows[1].Details!.Values.Select(v => (v.Name, v.Value!)));
        Assert.Equal([("exportType", "ContactRequests"), ("count", "1")], rows[2].Details!.Values.Select(v => (v.Name, v.Value!)));
    }

    [Fact]
    public async Task AnEmptySet_AWrongRange_AndAMissingRange_WriteNoTrailRow_AndLeaveNoFile_KK8()
    {
        await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 1, 0m));

        ExportFileResult empty = await RunAsync(e => e.ExportOrdersAsync(Today.AddDays(-30), Today.AddDays(-29), Ct));
        ExportFileResult reversed = await RunAsync(e => e.ExportOrdersAsync(Today.AddDays(-2), Today.AddDays(-3), Ct));
        ExportFileResult future = await RunAsync(e => e.ExportOrdersAsync(Today.AddDays(1), Today.AddDays(2), Ct));
        ExportFileResult missing = await RunAsync(e => e.ExportOrdersAsync(null, Today, Ct));

        Assert.Equal(ExportOutcome.Empty, empty.Outcome);
        Assert.Equal((ExportOutcome.RangeInvalid, ExportRangeError.EndBeforeStart), (reversed.Outcome, reversed.RangeError));
        Assert.Equal(ExportRangeError.StartInFuture, future.RangeError);
        Assert.Equal(ExportRangeError.StartMissing, missing.RangeError);
        Assert.Equal(0, await TrailCountAsync());
        Assert.Empty(Directory.GetFiles(_directory));
    }

    [Fact]
    public async Task AFilePastTheSizeCeiling_ThrowsTheLimitException_LeavesNoFileAndNoTrailRow_KK8()
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        DataExport export = NewExport(scope, TimeProvider.System, (stream, max, ct) => throw new ExportLimitExceededException("grew past the size ceiling"));

        await Assert.ThrowsAsync<ExportLimitExceededException>(() => export.ExportContactRequestsAsync(Ct));

        Assert.Equal(0, await TrailCountAsync());
        Assert.Empty(Directory.GetFiles(_directory));
    }

    [Fact]
    public async Task TheOrderRead_WithASmallSizeCeiling_ThrowsTheLimitException_AfterWritingTheFirstRows()
    {
        await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 1, 0m), (_book, 1, 0m));
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        using MemoryStream stream = new();

        await Assert.ThrowsAsync<ExportLimitExceededException>(
            () => scope.ServiceProvider.GetRequiredService<IOrderingModule>().WriteOrderExportAsync(Today.AddDays(-3), Today.AddDays(-3), stream, 600, Ct));
        using MemoryStream whole = new();
        int rows = await scope.ServiceProvider.GetRequiredService<IOrderingModule>().WriteOrderExportAsync(Today.AddDays(-3), Today.AddDays(-3), whole, ExportLimits.MaxFileBytes, Ct);

        Assert.Equal(2, rows);
        Assert.True(whole.Length > 600);
    }

    [Fact]
    public async Task AFileThatTakesLongerThanTheTimeCeiling_ThrowsTheLimitException_LeavesNoFileAndNoTrailRow_KK8()
    {
        ManualTimeProvider time = new(DateTimeOffset.UtcNow);
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        DataExport export = NewExport(
            scope,
            time,
            async (stream, max, ct) =>
            {
                await stream.WriteAsync(new byte[] { 1, 2, 3 }, ct);
                started.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
                return 1;
            });

        Task<ExportFileResult> running = export.ExportContactRequestsAsync(Ct);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        time.Advance(ExportLimits.MaxProductionTime + TimeSpan.FromSeconds(1));

        await Assert.ThrowsAsync<ExportLimitExceededException>(() => running);
        Assert.Equal(0, await TrailCountAsync());
        Assert.Empty(Directory.GetFiles(_directory));
    }

    [Fact]
    public async Task AClientThatLeaves_IsTheCancellation_NotTheLimitException_AndLeavesNoFileAndNoTrailRow_KK8()
    {
        TaskCompletionSource started = new(TaskCreationOptions.RunContinuationsAsynchronously);
        using CancellationTokenSource leaving = new();
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        DataExport export = NewExport(
            scope,
            TimeProvider.System,
            async (stream, max, ct) =>
            {
                await stream.WriteAsync(new byte[] { 1, 2, 3 }, ct);
                started.SetResult();
                await Task.Delay(Timeout.InfiniteTimeSpan, ct);
                return 1;
            });

        Task<ExportFileResult> running = export.ExportContactRequestsAsync(leaving.Token);
        await started.Task.WaitAsync(TimeSpan.FromSeconds(30), Ct);
        await leaving.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => running);
        Assert.Equal(0, await TrailCountAsync());
        Assert.Empty(Directory.GetFiles(_directory));
    }

    [Fact]
    public async Task TheTrailRow_IsWrittenInItsOwnTransaction_AfterTheFileIsMade_AndAFailedTrailLeavesNoFile_KK8()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await SaveAsync(c => c.ContactRequests.Add(ContactRequest.Create("Ayşe", "ayse@example.com", ContactTopic.Other, "ileti", now)));
        bool fileExistedWhenTheTrailWasWritten = false;
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        DataExport export = NewExport(
            scope,
            TimeProvider.System,
            async (stream, max, ct) =>
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes("Ad;E-posta\r\n"), ct);
                return 1;
            },
            trail => new FileCheckingTrail(trail, _directory, exists => fileExistedWhenTheTrailWasWritten = exists));

        ExportFileResult result = await export.ExportContactRequestsAsync(Ct);
        await result.File!.DisposeAsync();

        Assert.True(fileExistedWhenTheTrailWasWritten);
        Assert.Equal(1, await TrailCountAsync());
    }

    // ---- the file: made in the directory, closed, gone ----

    [Fact]
    public async Task TheFile_IsInTheDirectoryWhileTheStreamIsOpen_PositionedAtTheStart_AndGoneWhenItCloses()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await SaveAsync(c => c.Customers.Add(Customer.Create("Ali", "ali@example.com", "digest-placeholder", now, null, now)));
        await using AsyncServiceScope scope = Services.CreateAsyncScope();

        ExportFileResult result = await scope.ServiceProvider.GetRequiredService<IDataExport>().ExportCustomersAsync(Ct);

        FileStream file = result.File!;
        string[] inDirectory = Directory.GetFiles(_directory);
        Assert.Equal([file.Name], inDirectory);
        Assert.Matches("^[0-9a-f]{32}\\.csv$", Path.GetFileName(file.Name));
        Assert.Equal(0, file.Position);
        byte[] bytes = new byte[file.Length];
        await file.ReadExactlyAsync(bytes, Ct);
        Assert.Equal([0xEF, 0xBB, 0xBF], bytes[..3]);
        Assert.Equal("Ad;E-posta\r\nAli;ali@example.com\r\n", Encoding.UTF8.GetString(bytes[3..]));
        Assert.Equal(1, result.Rows);
        await file.DisposeAsync();
        Assert.Empty(Directory.GetFiles(_directory));
    }

    [Fact]
    public async Task AnOldFileInTheDirectory_IsSweptByTheNextRequest_AndAFreshOneStays()
    {
        _ = Services;
        string old = Path.Combine(_directory, "old.csv");
        string fresh = Path.Combine(_directory, "fresh.csv");
        await File.WriteAllTextAsync(old, "x", Ct);
        await File.WriteAllTextAsync(fresh, "x", Ct);
        File.SetCreationTimeUtc(old, DateTime.UtcNow - ExportLimits.SweepAge - TimeSpan.FromMinutes(5));
        File.SetCreationTimeUtc(fresh, DateTime.UtcNow - ExportLimits.SweepAge + TimeSpan.FromMinutes(5));

        ExportFileResult result = await RunAsync(e => e.ExportCustomersAsync(Ct));

        Assert.Equal(ExportOutcome.Empty, result.Outcome);
        Assert.False(File.Exists(old));
        Assert.True(File.Exists(fresh));
    }

    [Fact]
    public async Task EveryFileInTheDirectory_IsSweptWhenTheApplicationStarts()
    {
        string leftover = Path.Combine(_directory, "leftover.csv");
        await File.WriteAllTextAsync(leftover, "x", Ct);
        string recent = Path.Combine(_directory, "recent.csv");
        await File.WriteAllTextAsync(recent, "x", Ct);

        _ = Services;

        Assert.False(File.Exists(leftover));
        Assert.False(File.Exists(recent));
        Assert.True(Directory.Exists(_directory));
    }

    [Fact]
    public async Task ADirectoryThatCannotBeWritten_RefusesTheStart()
    {
        string blocked = Path.Combine(_directory, "blocked");
        await File.WriteAllTextAsync(blocked, "a file where the directory should be", Ct);
        WebApplicationFactory<Program> app = OpenApplication(Path.Combine(blocked, "inside"));

        try
        {
            Exception failure = Assert.ThrowsAny<Exception>(() => app.Services);

            Assert.Contains("Export:TempDirectory", failure.ToString(), StringComparison.Ordinal);
        }
        finally
        {
            await app.DisposeAsync();
        }
    }

    // ---- the pages of the reads: nothing lost or repeated at a page boundary ----

    [Theory]
    [InlineData(5, 2)]
    [InlineData(4, 2)]
    [InlineData(3, 3)]
    [InlineData(1, 2)]
    [InlineData(0, 2)]
    public async Task TheOrderRead_ByPages_LosesAndRepeatsNothing_WhenEveryOrderHasTheSameCreationMoment_AtTheBoundaryTheLastPageAndAnEmptyRange(int orders, int pageSize)
    {
        DateTimeOffset moment = TurkeyTime.StartOf(Today.AddDays(-3)).AddHours(10);
        List<string> numbers = [];
        for (int i = 0; i < orders; i++)
        {
            numbers.Add(await SeedAsync(moment, PaymentMethod.Card, "Paid", (_shirt, 1, 0m), (_book, 1, 0m)));
        }

        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        OrderExport read = scope.ServiceProvider.GetRequiredService<OrderExport>();
        List<OrderExportRow> rows = [];
        await foreach (OrderExportRow row in read.ReadAsync(Today.AddDays(-3), Today.AddDays(-3), Ct, pageSize))
        {
            rows.Add(row);
        }

        Assert.Equal(orders * 2, rows.Count);
        Assert.Equal(numbers.Order(StringComparer.Ordinal), rows.Select(r => r.Frozen.OrderNumber).Distinct().Order(StringComparer.Ordinal));
        Assert.Equal(rows.Select(r => r.Frozen.OrderId).Distinct().Count(), orders);
        Assert.Equal(rows.Select(r => r.Frozen.OrderId).Distinct(), rows.Select(r => r.Frozen.OrderId).Distinct().OrderBy(id => id));
    }

    [Theory]
    [InlineData(5, 2)]
    [InlineData(4, 2)]
    [InlineData(0, 2)]
    public async Task TheMemberAndContactRequestReads_ByPages_LoseAndRepeatNothing_AtTheBoundaryAndOnAnEmptySet(int count, int pageSize)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await SaveAsync(c =>
        {
            for (int i = 0; i < count; i++)
            {
                c.Customers.Add(Customer.Create($"Üye {i}", $"uye{i}@example.com", "digest-placeholder", now, null, now));
                c.ContactRequests.Add(ContactRequest.Create($"Talep {i}", $"talep{i}@example.com", ContactTopic.Other, "ileti", now));
            }
        });
        await using AppDbContext db = NewContext();

        List<string> members = [];
        await foreach (CustomerExportRow row in new CustomerExport(db).ReadAsync(Ct, pageSize))
        {
            members.Add(row.Email);
        }

        List<string> requests = [];
        await foreach (ContactRequestExportRow row in new ContactRequestExport(db).ReadAsync(Ct, pageSize))
        {
            requests.Add(row.Email);
        }

        Assert.Equal(Enumerable.Range(0, count).Select(i => $"uye{i}@example.com").Order(StringComparer.Ordinal), members.Order(StringComparer.Ordinal));
        Assert.Equal(Enumerable.Range(0, count).Select(i => $"talep{i}@example.com").Order(StringComparer.Ordinal), requests.Order(StringComparer.Ordinal));
        Assert.Equal(count, members.Distinct().Count());
        Assert.Equal(count, requests.Distinct().Count());
    }

    // ---- the application as it is wired ----

    private WebApplicationFactory<Program> OpenApplication(string directory)
    {
        _root = new ShopfolioWebApplicationFactory(_collection.Dsn, new Dictionary<string, string> { ["Export:TempDirectory"] = directory }, withJobRunner: false);
        return _root.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAuditActorSource>();
            services.AddScoped<IAuditActorSource>(_ => new FixedActor(_admin));
        }));
    }

    private static DataExport NewExport(AsyncServiceScope scope, TimeProvider time, Func<Stream, long, CancellationToken, Task<int>> write, Func<IAuditTrail, IAuditTrail>? trail = null)
    {
        IServiceProvider p = scope.ServiceProvider;
        ICorporateContentModule contact = DispatchProxy.Create<ICorporateContentModule, ContactRequestWriteProxy>();
        ((ContactRequestWriteProxy)(object)contact).Write = write;
        IAuditTrail audit = p.GetRequiredService<IAuditTrail>();
        return new DataExport(
            p.GetRequiredService<IOrderingModule>(),
            p.GetRequiredService<IMembershipModule>(),
            contact,
            trail is null ? audit : trail(audit),
            p.GetRequiredService<ITransactionRunner>(),
            p.GetRequiredService<ExportTempDirectory>(),
            time);
    }

    private async Task<ExportFileResult> RunAsync(Func<IDataExport, Task<ExportFileResult>> run)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        ExportFileResult result = await run(scope.ServiceProvider.GetRequiredService<IDataExport>());
        if (result.File is not null)
        {
            await result.File.DisposeAsync();
        }

        return result;
    }

    private Task<Exported> ExportOrdersAsync(DateOnly start, DateOnly end) => ExportAsync(e => e.ExportOrdersAsync(start, end, Ct));

    // Runs the export, reads the whole file as the response would send it and closes the stream, which deletes the file.
    private async Task<Exported> ExportAsync(Func<IDataExport, Task<ExportFileResult>> run)
    {
        await using AsyncServiceScope scope = Services.CreateAsyncScope();
        ExportFileResult result = await run(scope.ServiceProvider.GetRequiredService<IDataExport>());
        Assert.Equal(ExportOutcome.Done, result.Outcome);
        FileStream file = result.File!;
        string path = file.Name;
        Assert.True(File.Exists(path));
        using MemoryStream copy = new();
        await file.CopyToAsync(copy, Ct);
        await file.DisposeAsync();
        Assert.False(File.Exists(path));
        return new Exported(result, copy.ToArray());
    }

    // ---- seeding, as the confirmation freezes an order ----

    private static DateTimeOffset CreatedOn(int offsetFromToday) => TurkeyTime.StartOf(Today.AddDays(offsetFromToday)).AddHours(10);

    /// <summary>One paid order with one line of the given quantity shipped two days ago; each record closes the quantity before the shipment (a cancellation) or after it (a withdrawal).</summary>
    private async Task<Exported> ExportShippedItemAsync(int quantity, (int Quantity, bool AfterShipment)[] records)
    {
        string number = await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, quantity, 0m));
        Guid id = await OrderIdAsync(number);
        DateTimeOffset shippedAt = TurkeyTime.StartOf(Today.AddDays(-2)).AddHours(15);
        Guid itemId;
        await using (AppDbContext read = NewContext())
        {
            itemId = await read.OrderItems.AsNoTracking().Where(i => i.OrderId == id).Select(i => i.Id).SingleAsync(Ct);
        }

        Sales.ItemAmounts amounts = new(100m, 10m, 15m);
        await SaveAsync(c =>
        {
            c.TransitionRecords.Add(Record(id, TransitionCode.S5, shippedAt));
            foreach ((int closed, bool after) in records)
            {
                if (after)
                {
                    c.Withdrawals.Add(Sales.Withdrawal.ByBuyer(itemId, closed, Today.AddDays(-1), null, amounts, null, shippedAt.AddHours(20)));
                }
                else
                {
                    c.Cancellations.Add(Sales.Cancellation.ByBuyer(itemId, closed, amounts, null, shippedAt.AddHours(-2)));
                }
            }
        });
        return await ExportOrdersAsync(Today.AddDays(-3), Today.AddDays(-3));
    }

    private static string Day(DateOnly day) => day.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

    private Task<string> SeedAsync(DateTimeOffset createdAt, PaymentMethod method, string paymentStatus, params (Guid VariantId, int Quantity, decimal CouponShare)[] lines)
        => SeedAsync(createdAt, method, paymentStatus, _ => { }, lines);

    private async Task<string> SeedAsync(DateTimeOffset createdAt, PaymentMethod method, string paymentStatus, Action<Order> change, (Guid VariantId, int Quantity, decimal CouponShare)[] lines)
    {
        int n = Interlocked.Increment(ref _counter);
        List<OrderItemSource> sources = [];
        foreach ((Guid variantId, _, _) in lines)
        {
            sources.Add(await SourceAsync(variantId));
        }

        List<ItemAmounts> amounts = [.. lines.Select((l, i) => ItemPricing.Price(new ItemPriceInput(sources[i].UnitPrice, l.Quantity, l.CouponShare, sources[i].VatRate)))];
        decimal fee = 40m;
        string orderNumber = $"2026-E{n:D5}";
        Order order = Order.Create(
            orderNumber, $"access-{n}-{Guid.NewGuid():N}", $"confirm-{n}-{Guid.NewGuid():N}", method, true, "ali@example.com",
            ItemPricing.SumOrderTotal(amounts, fee), fee, createdAt.AddHours(1), createdAt);
        order.IpAddress = IPAddress.Parse("203.0.113.5");
        order.CartId = EntityIds.New();
        order.FreezeAddresses(("Ali Veli", "5321234567", _province, "Çankaya", "Atatürk Bulvarı 1"), ("Ali Veli", _province, "Çankaya", "Atatürk Bulvarı 1"));
        OrderFreeze.FreezeLegalIdentity(order, _identity, new LegalTextVersionIds(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()), method == PaymentMethod.BankTransfer ? "TR330006100519786457841326" : null);
        order.FreezeDocuments("ön bilgilendirme", "mesafeli satış");
        order.FreezeShippingFeeVatBreakdown(ItemPricing.SplitShippingFee(fee, [.. lines.Select((l, i) => new ShippingBasisLine(sources[i].ProductType, sources[i].VatRate, sources[i].UnitPrice * l.Quantity))]));
        order.FreezeShippingPromise(2);
        change(order);
        List<OrderItem> items = [.. lines.Select((l, i) => OrderFreeze.FreezeItem(order.Id, sources[i], l.Quantity, amounts[i], 3, order.CreatedAt))];
        await SaveWithVersionsAsync(order, items);
        string status = paymentStatus == "Failed" ? ", order_status = 'Cancelled'" : string.Empty;
        await ExecuteAsync($"UPDATE orders SET payment_status = '{paymentStatus}'{status} WHERE id = '{order.Id}'");
        return orderNumber;
    }

    private async Task SaveWithVersionsAsync(Order order, List<OrderItem> items)
    {
        int n = Random.Shared.Next(10_000, int.MaxValue);
        LegalTextVersion form = LegalTextVersion.CreatePublished(LegalTextType.PreInformationForm, n, order.CreatedAt, "metin", order.CreatedAt);
        LegalTextVersion contract = LegalTextVersion.CreatePublished(LegalTextType.DistanceSalesContract, n, order.CreatedAt, "metin", order.CreatedAt);
        LegalTextVersion notice = LegalTextVersion.CreatePublished(LegalTextType.PrivacyNotice, n, order.CreatedAt, "metin", order.CreatedAt).WithHosting();
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

    private async Task<OrderItemSource> SourceAsync(Guid variantId)
    {
        await using AppDbContext db = NewContext();
        TimeProvider time = TimeProvider.System;
        return Assert.Single(await new OrderItemSourceReader(db, time, new ProductGallery(db, time), new DigitalFileReader(db)).ListAsync([variantId], Ct));
    }

    private async Task<Guid> SeedVariantAsync(decimal price, decimal vatRate)
    {
        int n = Interlocked.Increment(ref _counter);
        DateTimeOffset longAgo = DateTimeOffset.UtcNow.AddDays(-60);
        Product product = Product.Create($"Ürün {n}", $"disa-aktarma-urun-{n}", ProductType.Physical, vatRate, longAgo);
        product.WithSeededOrigin();
        product.PublishStatus = PublishStatus.Published;
        Variant variant = Variant.Create(product.Id, $"DAK-{n}", price, longAgo, 100);
        variant.PublishStatus = PublishStatus.Published;
        variant.FirstOnSaleAt = longAgo;
        await SaveAsync(c =>
        {
            c.Products.Add(product);
            c.Variants.Add(variant);
            c.PriceHistories.Add(PriceHistory.Create(variant.Id, price, longAgo, longAgo));
        });
        return variant.Id;
    }

    private async Task<CompanyIdentity> SetCompanyAsync(string legalName, string addressLine)
    {
        await ExecuteAsync(
            $"UPDATE companies SET company_type = 'Tradesman', legal_name = '{legalName}', tax_id_number = '1111111111', province_id = '{_province}', district = 'Çankaya', "
            + $"address_line = '{addressLine}', phone = '3121234567', email = 'firma@example.com', kep_address = 'firma@hs01.kep.tr'");
        await using ServiceProvider provider = OrderingTestServices.Build(_collection.Dsn);
        await using AsyncServiceScope scope = provider.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<ISettingsModule>().GetCompanyIdentityAsync(Ct);
    }

    private static TransitionRecord Record(Guid orderId, TransitionCode code, DateTimeOffset at)
        => TransitionRecord.Create(orderId, code, code == TransitionCode.O1 ? ActorKind.System : ActorKind.Admin, null, null, null, null, null, null, null, at);

    private async Task<Guid> OrderIdAsync(string orderNumber)
    {
        await using AppDbContext db = NewContext();
        return await db.Orders.AsNoTracking().Where(o => o.OrderNumber == orderNumber).Select(o => o.Id).SingleAsync(Ct);
    }

    private async Task<Guid> ProvinceAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.Provinces.AsNoTracking().OrderBy(p => p.Code).Select(p => p.Id).FirstAsync(Ct);
    }

    private async Task<List<AuditLog>> TrailAsync()
    {
        await using AppDbContext db = NewContext();
        return await db.AuditLogs.AsNoTracking().OrderBy(l => l.CreatedAt).ThenBy(l => l.Id).ToListAsync(Ct);
    }

    private async Task<int> TrailCountAsync() => (int)await ScalarAsync<long>("SELECT count(*) FROM audit_logs");

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

    private async Task<T> ScalarAsync<T>(string sql)
    {
        await using NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new(sql, connection);
        return (T)(await command.ExecuteScalarAsync(Ct))!;
    }

    // The file as the response would send it: its bytes, and the table the semicolons and line ends make of it (no test value needs quoting except where the raw text is read).
    private sealed record Exported(ExportFileResult Result, byte[] Bytes)
    {
        public string Text => Encoding.UTF8.GetString(Bytes, 3, Bytes.Length - 3);

        public string[] Header => Lines[0].Split(';');

        public IReadOnlyList<string[]> Rows => [.. Lines.Skip(1).Select(l => l.Split(';'))];

        private string[] Lines => Text.Split("\r\n", StringSplitOptions.RemoveEmptyEntries);

        public IReadOnlyList<string> Values(string header)
        {
            int index = Array.IndexOf(Header, header);
            Assert.True(index >= 0, $"the file has no column '{header}'");
            return [.. Rows.Select(r => r[index])];
        }
    }

    private sealed class FixedActor(Admin admin) : IAuditActorSource
    {
        public Task<AuditActor?> CurrentAsync(CancellationToken ct) => Task.FromResult<AuditActor?>(new AuditActor(admin.Id, admin.Name, admin.Email));
    }

    private sealed class FileCheckingTrail(IAuditTrail inner, string directory, Action<bool> seen) : IAuditTrail
    {
        public Task WriteAsync(AuditEntry entry, CancellationToken ct)
        {
            seen(Directory.GetFiles(directory).Length == 1);
            return inner.WriteAsync(entry, ct);
        }

        public Task<AuditPage> ListAsync(AuditQuery query, CancellationToken ct) => inner.ListAsync(query, ct);

        public Task<IReadOnlyList<AuditAdminOption>> ListAdminsAsync(CancellationToken ct) => inner.ListAdminsAsync(ct);
    }

    /// <summary>A stand-in for the content module that answers the contact request write only; every other call is a test mistake.</summary>
    public class ContactRequestWriteProxy : DispatchProxy
    {
        public Func<Stream, long, CancellationToken, Task<int>> Write { get; set; } = (_, _, _) => Task.FromResult(0);

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod?.Name == nameof(ICorporateContentModule.WriteContactRequestExportAsync))
            {
                return Write((Stream)args![0]!, (long)args[1]!, (CancellationToken)args[2]!);
            }

            throw new NotSupportedException(targetMethod?.Name);
        }
    }
}
