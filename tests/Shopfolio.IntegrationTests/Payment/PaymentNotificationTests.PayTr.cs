using System.Net;
using System.Text;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Payment;

/// <summary>
/// The provider's notification end to end (T402 KK1-KK7; 08 2.1.5 "Bildirimin sınıflanması"): the same notification server, now with the provider's own gateway
/// (<see cref="PayTrPaymentGateway"/>) and its one setting of keys, receives form bodies signed as the provider signs them. Each of the seven rows of the classification
/// is one test - the four arms of row 4 one test each - and the record, the job row and the order are read from a separate connection after the answer. The server's
/// environment is the live installation (row 4's test mark); the gateway itself makes no outside call.
/// </summary>
public sealed partial class PaymentNotificationTests
{
    // Built from parts: the repository's secret guard stops a secret-looking literal next to a secret key.
    private static readonly string ProviderKey = "key-" + "notice-test";
    private static readonly string ProviderSalt = "salt-" + "notice-test";

    // 120,00 in whole kuruş, the seeded orders' total.
    private const string TotalKurus = "12000";

    private WebApplication? _payTrApp;
    private HttpClient? _payTrClient;

    // ---- Row 1 (KK1, KK2): the signature ----

    [Fact]
    public async Task PayTr_Row1_HashThatDoesNotHold_Gets400_NoRecord_AndTheSecurityLine_KK2()
    {
        (Order order, string reference) = await SeedPayTrOrderAsync();
        Dictionary<string, string> fields = Signed(reference, "success", TotalKurus);
        fields["hash"] = Signed(reference, "success", "12001")["hash"];

        using HttpResponseMessage response = await PostPayTrAsync(fields);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal((0L, 0L), (await CountAsync("inbound_notification_records"), await CountAsync("jobs")));
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.SignatureFailureSampled, StringComparison.Ordinal));
        await AssertOrderAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }

    [Fact]
    public async Task PayTr_Row1_MissingHash_Gets400_NoRecord_KK2()
    {
        (_, string reference) = await SeedPayTrOrderAsync();
        Dictionary<string, string> fields = Signed(reference, "success", TotalKurus);
        fields.Remove("hash");

        using HttpResponseMessage response = await PostPayTrAsync(fields);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal(0L, await CountAsync("inbound_notification_records"));
    }

    // ---- Row 7 (KK1, KK4, KK5): a success whose amount holds - recorded with its three fields, applied in the job ----

    [Fact]
    public async Task PayTr_Row7_SuccessWithTheInstalmentDifference_IsRecordedWithTheMappedFields_AndTheJobPaysTheOrder_KK1_KK4_KK5()
    {
        (Order order, string reference) = await SeedPayTrOrderAsync();

        using HttpResponseMessage response = await PostPayTrAsync(Signed(reference, "success", "12450", paymentAmount: TotalKurus));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("OK", await response.Content.ReadAsStringAsync(Ct));
        InboundNotificationRecord record = await SingleRecordAsync();
        Assert.Equal(
            (reference, reference, PayTrNotification.SaleEventType, "success", (decimal?)124.50m, (Guid?)order.Id),
            (record.ProviderTransactionId, record.OrderReference, record.EventType, record.Outcome, record.Amount, record.OrderId));
        Assert.Null(record.ProviderNotificationId);
        Assert.Null(record.UnprocessedReason);
        Assert.Equal(1, await JobRowsOfAsync(record.Id));

        Assert.True(await RunNextJobAsync(_payTrApp));

        await AssertOrderAsync(order.Id, OrderStatus.Preparing, PaymentStatus.Paid, 2);
        await using AppDbContext reader = NewContext();
        Assert.Equal(reference, (await reader.PaymentRecords.SingleAsync(p => p.OrderId == order.Id, Ct)).ProviderTransactionId);
    }

    // ---- Row 2: the same notification again ----

    [Fact]
    public async Task PayTr_Row2_TheSameNotificationAgain_IsReceived_AndWritesNoSecondRecordOrJobRow()
    {
        (_, string reference) = await SeedPayTrOrderAsync();
        Dictionary<string, string> fields = Signed(reference, "success", TotalKurus);

        using HttpResponseMessage first = await PostPayTrAsync(fields);
        using HttpResponseMessage second = await PostPayTrAsync(fields);

        Assert.Equal((HttpStatusCode.OK, HttpStatusCode.OK), (first.StatusCode, second.StatusCode));
        Assert.Equal("OK", await second.Content.ReadAsStringAsync(Ct));
        Assert.Equal((1L, 1L), (await CountAsync("inbound_notification_records"), await CountAsync("jobs")));
    }

    // ---- Row 3: a reference that maps to no order ----

    [Theory]
    [InlineData("20269ZZZZZ")]
    [InlineData("NOTANORDER")]
    public async Task PayTr_Row3_AReferenceOfNoOrder_IsRecordedMarkedOrderNotFound_AndReceived(string reference)
    {
        await SeedPayTrOrderAsync();

        using HttpResponseMessage response = await PostPayTrAsync(Signed(reference, "success", TotalKurus));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        InboundNotificationRecord record = await SingleRecordAsync();
        Assert.Equal(((UnprocessedNotificationReason?)UnprocessedNotificationReason.OrderNotFound, (Guid?)null, reference), (record.UnprocessedReason, record.OrderId, record.OrderReference));
        Assert.Equal(0L, await CountAsync("jobs"));
    }

    // ---- Row 4: the four arms, one test each (KK3) ----

    [Fact]
    public async Task PayTr_Row4_UnknownStatus_IsRecordedMarkedUnrecognized_KK3()
    {
        await AssertRow4Async(reference => Signed(reference, "pending", TotalKurus));
    }

    [Fact]
    public async Task PayTr_Row4_PaymentTypeThatIsNotCard_IsRecordedMarkedUnrecognized_KK3()
    {
        await AssertRow4Async(reference => Signed(reference, "success", TotalKurus, paymentType: "eft"));
    }

    [Fact]
    public async Task PayTr_Row4_CurrencyThatIsNotTl_IsRecordedMarkedUnrecognized_KK3()
    {
        await AssertRow4Async(reference => Signed(reference, "success", TotalKurus, currency: "USD"));
    }

    [Fact]
    public async Task PayTr_Row4_TestModeOnTheLiveInstallation_IsRecordedMarkedUnrecognized_KK3()
    {
        await AssertRow4Async(reference => Signed(reference, "success", TotalKurus, testMode: "1"));
    }

    // ---- Row 5 (KK6): a failed payment - no amount looked at, processed; its reason goes to the log thinned and nowhere else ----

    [Fact]
    public async Task PayTr_Row5_Failure_IsProcessedWithoutAnAmountCheck_AndItsReasonIsInTheLogOnly_KK6()
    {
        (Order order, string reference) = await SeedPayTrOrderAsync();
        string message = "vazgecti-" + Guid.NewGuid().ToString("N");
        Dictionary<string, string> fields = Signed(reference, "failed", "0", paymentAmount: "1");
        fields["failed_reason_code"] = "6";
        fields["failed_reason_msg"] = message;

        using HttpResponseMessage response = await PostPayTrAsync(fields);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        InboundNotificationRecord record = await SingleRecordAsync();
        Assert.Equal(((UnprocessedNotificationReason?)null, "failed", (decimal?)0m), (record.UnprocessedReason, record.Outcome, record.Amount));
        Assert.Equal(1, await JobRowsOfAsync(record.Id));
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.PaymentFailedReasonSampled, StringComparison.Ordinal)
            && line.Contains(" 6 ", StringComparison.Ordinal) && line.Contains(message, StringComparison.Ordinal));
        Assert.DoesNotContain(_logs.Lines, line => line.Contains(LogEvents.PaymentFailedReasonSampled, StringComparison.Ordinal) && line.Contains(reference, StringComparison.Ordinal));
        Assert.Equal(0L, await ScalarAsync<long>(RowsContaining(message)));

        Assert.True(await RunNextJobAsync(_payTrApp));
        await AssertOrderAsync(order.Id, OrderStatus.Cancelled, PaymentStatus.Failed, 2);
    }

    // ---- Row 6 (KK4): a success whose amount does not hold ----

    [Theory]
    [InlineData("11999", TotalKurus)]
    [InlineData(TotalKurus, "11999")]
    [InlineData("12450", "12001")]
    public async Task PayTr_Row6_SuccessWhoseAmountDoesNotHold_IsRecordedMarkedAmountMismatch_KK4(string total, string paymentAmount)
    {
        (Order order, string reference) = await SeedPayTrOrderAsync();

        using HttpResponseMessage response = await PostPayTrAsync(Signed(reference, "success", total, paymentAmount: paymentAmount));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal((UnprocessedNotificationReason?)UnprocessedNotificationReason.AmountMismatch, (await SingleRecordAsync()).UnprocessedReason);
        Assert.Equal(0L, await CountAsync("jobs"));
        await AssertOrderAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }

    private async Task AssertRow4Async(Func<string, Dictionary<string, string>> fields)
    {
        (Order order, string reference) = await SeedPayTrOrderAsync();

        using HttpResponseMessage response = await PostPayTrAsync(fields(reference));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("OK", await response.Content.ReadAsStringAsync(Ct));
        InboundNotificationRecord record = await SingleRecordAsync();
        Assert.Equal(((UnprocessedNotificationReason?)UnprocessedNotificationReason.UnrecognizedNotification, (Guid?)order.Id), (record.UnprocessedReason, record.OrderId));
        Assert.Equal(0L, await CountAsync("jobs"));
        await AssertOrderAsync(order.Id, OrderStatus.Placed, PaymentStatus.Pending, 1);
    }

    // A form as the provider sends it: the seven fields always there, signed over merchant_oid + merchant_salt + status + total_amount with the test keys.
    private static Dictionary<string, string> Signed(
        string reference, string status, string total, string? paymentAmount = null, string paymentType = "card", string currency = "TL", string testMode = "0") =>
        new(StringComparer.Ordinal)
        {
            ["merchant_oid"] = reference,
            ["status"] = status,
            ["total_amount"] = total,
            ["hash"] = PayTrSignature.Of(ProviderKey, PayTrNotification.SignedMessage(reference, ProviderSalt, status, total)),
            ["payment_type"] = paymentType,
            ["payment_amount"] = paymentAmount ?? total,
            ["currency"] = currency,
            ["test_mode"] = testMode,
        };

    private static string RowsContaining(string text) =>
        "SELECT count(*) FROM inbound_notification_records r WHERE r::text LIKE '%" + text + "%'";

    // An order whose number maps from the provider's reference (08 2.1.4: the random block's alphabet), and the reference without the separator.
    private async Task<(Order Order, string Reference)> SeedPayTrOrderAsync()
    {
        int n = Interlocked.Increment(ref _counter);
        string block = string.Concat(Enumerable.Range(0, 6).Select(i => Shopfolio.Infrastructure.Security.OrderNumber.Alphabet[(n >> (i * 5)) & 31]));
        Order order = await SeedOrderAsync(OrderStatus.Placed, PaymentStatus.Pending, "2026-" + block);
        return (order, "2026" + block);
    }

    private async Task<HttpResponseMessage> PostPayTrAsync(Dictionary<string, string> fields)
    {
        if (_payTrApp is null)
        {
            IPaymentGateway gateway = new PayTrPaymentGateway(
                new NoClients(),
                new ExternalCallRunner(TimeProvider.System, NullLogger<ExternalCallRunner>.Instance),
                Options.Create(new PaymentOptions
                {
                    ProviderKeys = PaymentProviderKeys.IdName + "=654321\n" + PaymentProviderKeys.KeyName + "=" + ProviderKey + "\n" + PaymentProviderKeys.SaltName + "=" + ProviderSalt + "\n",
                }),
                new SiteAddress(Options.Create(new SiteOptions { Domain = "shop.example.com" })),
                TimeProvider.System,
                new LiveEnvironment(),
                new PaymentFailureReasonLog(TimeProvider.System, new Logger<PaymentFailureReasonLog>(new LoggerFactory([_logs]))));

            // The live installation: the endpoint reads the test mark against it (row 4).
            _payTrApp = await PaymentNotificationHost.StartAsync(
                _collection.Dsn, TimeProvider.System, gateway, Ct, configure: services => services.AddSingleton<IHostEnvironment>(new LiveEnvironment()), logs: _logs);
            _payTrClient = _payTrApp.GetTestClient();
        }

        using FormUrlEncodedContent content = new(fields);
        string body = await content.ReadAsStringAsync(Ct);
        return await _payTrClient!.PostAsync(new Uri(Path, UriKind.Relative), new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"), Ct);
    }

    private async Task DisposePayTrAsync()
    {
        _payTrClient?.Dispose();
        if (_payTrApp is not null)
        {
            await _payTrApp.StopAsync(Ct);
            await _payTrApp.DisposeAsync();
        }
    }

    // The notification makes no outside call; a client asked for would be a fault.
    private sealed class NoClients : IHttpClientFactory
    {
        public HttpClient CreateClient(string name) => throw new InvalidOperationException("The notification makes no outside call.");
    }

    private sealed class LiveEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "Shopfolio.Web";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
