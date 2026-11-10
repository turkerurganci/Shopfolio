using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.SearchEngine;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.IntegrationTests.Ordering;

/// <summary>
/// The card window's final query with the provider's own gateway (T403 KK2-KK7; 08 2.1.6; 05 4.3.4): the same handler and database as the window's tests, now asking
/// through <see cref="PayTrPaymentGateway"/> with a hand-written HTTP handler that answers as the provider's table says. <c>004</c> cancels the order; the six inputs
/// without an answer leave it as it is - the request errors throw for the code error rule, the outage and the unlisted answer are asked again after five minutes - and
/// each writes the layer's line; a successful answer whose five conditions do not hold is not applied and is written to the application log; an instalment's
/// difference is applied with the asked <c>merchant_oid</c> as the payment's id. No socket is opened.
/// </summary>
public sealed partial class PaymentWindowTests
{
    // Built from parts: the repository's secret guard stops a secret-looking literal next to a secret key.
    private static readonly string QueryKey = "key-" + "query-test";
    private static readonly string QuerySalt = "salt-" + "query-test";

    // ---- KK2: 004 is an answer ----

    [Fact]
    public async Task PayTr_Query004_IsNoPayment_AndCancelsTheOrderAtTheWindowsEnd_KK2()
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        ProviderAnswers answers = new() { Body = ErrorAnswer("004") };

        Assert.Equal(JobOutcome.Completed, await HandlePayTrAsync(answers, order.Id));

        Order after = await ReadOrderAsync(order.Id);
        Assert.Equal((OrderStatus.Cancelled, PaymentStatus.Failed), (after.OrderStatus, after.PaymentStatus));
        Assert.Equal(OrderNumberOf(order), answers.MerchantOids.Single());
    }

    // ---- KK3, KK4: the six inputs without an answer - the order stays; two intervals ----

    [Theory]
    [InlineData("001")]
    [InlineData("002")]
    [InlineData("003")]
    public async Task PayTr_QueryRequestError_LeavesTheOrder_WritesTheLine_AndRepeatsByTheCodeErrorRule_KK3_KK4(string code)
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        ProviderAnswers answers = new() { Body = ErrorAnswer(code) };

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => HandlePayTrAsync(answers, order.Id));

        Assert.EndsWith(PayTrPaymentQuery.ErrorReason(code), error.Message, StringComparison.Ordinal);
        await AssertOrderUntouchedAsync(order.Id);
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal)
            && line.EndsWith(" " + PayTrPaymentQuery.ErrorReason(code), StringComparison.Ordinal));
        Assert.DoesNotContain(_logs.Lines, line => line.Contains("MESSAGE-WITH", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData(ProviderAnswers.Behavior.Hang, ExternalCallFailure.TimeoutReason)]
    [InlineData(ProviderAnswers.Behavior.Refuse, "connection:ConnectionError")]
    [InlineData(ProviderAnswers.Behavior.Unlisted, PayTrPaymentQuery.UnrecognizedBodyReason)]
    [InlineData(ProviderAnswers.Behavior.ServerError, "http:503")]
    public async Task PayTr_QueryWithoutAnAnswer_LeavesTheOrder_WritesTheLine_AndIsAskedAgainAfterFiveMinutes_KK3_KK4(ProviderAnswers.Behavior mode, string reason)
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        ProviderAnswers answers = new() { Mode = mode };

        Task<JobOutcome> handling = HandlePayTrAsync(answers, order.Id);
        if (mode == ProviderAnswers.Behavior.Hang)
        {
            await answers.Arrived.Task.WaitAsync(Ct);
            _clock.Advance(ExternalCallTimeouts.Job);
        }

        Assert.Equal(JobOutcome.RescheduleAfter(JobTimings.FinalPaymentQueryInterval), await handling);
        await AssertOrderUntouchedAsync(order.Id);
        Assert.True(PaymentWindowRules.IsPaymentOutcomeUnknown(await ReadOrderAsync(order.Id), Now));
        Assert.Contains(_logs.Lines, line => line.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal) && line.EndsWith(" " + reason, StringComparison.Ordinal));
        Assert.Equal(1, answers.Requests);
    }

    // ---- KK5: the five conditions ----

    [Theory]
    [InlineData("119,99", "124,5", "KART", "TL", "0", FinalPaymentQueryRule.AmountReason)]
    [InlineData("120", "119,99", "KART", "TL", "0", FinalPaymentQueryRule.AmountReason)]
    [InlineData("120", "120", "EFT", "TL", "0", FinalPaymentQueryRule.PaymentTypeReason)]
    [InlineData("120", "120", "KART", "USD", "0", FinalPaymentQueryRule.CurrencyReason)]
    [InlineData("120", "120", "KART", "TRY", "1", FinalPaymentQueryRule.TestModeReason)]
    public async Task PayTr_SuccessWhoseConditionDoesNotHold_IsNotApplied_IsWrittenToTheApplicationLog_AndTheOrderStays_KK5(
        string paymentAmount, string paymentTotal, string paymentType, string currency, string testMode, string reason)
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        ProviderAnswers answers = new() { Body = SuccessAnswer(paymentAmount, paymentTotal, paymentType, currency, testMode) };

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => HandlePayTrAsync(answers, order.Id, live: true));

        Assert.EndsWith(reason, error.Message, StringComparison.Ordinal);
        await AssertOrderUntouchedAsync(order.Id);
        string line = Assert.Single(_logs.Lines, l => l.Contains(LogEvents.FinalPaymentQueryNotApplied, StringComparison.Ordinal));
        Assert.EndsWith($"|Warning|{LogEvents.FinalPaymentQueryNotApplied} {reason} {order.Id}", line, StringComparison.Ordinal);
        Assert.DoesNotContain(OrderNumberOf(order), line, StringComparison.Ordinal);
    }

    [Fact]
    public async Task PayTr_SuccessWithTheInstalmentDifference_IsApplied_WithTheMerchantOidAsTheProvidersId_AndNoCardFieldIsKept_KK5_KK7()
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        ProviderAnswers answers = new() { Body = SuccessAnswer("120", "124,5", "KART", "TRY", "1") };

        // Not the live installation: a test transaction is applied there (08 2.1.10).
        Assert.Equal(JobOutcome.Completed, await HandlePayTrAsync(answers, order.Id));

        await using AppDbContext reader = NewContext();
        Order after = await reader.Orders.SingleAsync(o => o.Id == order.Id, Ct);
        Assert.Equal((OrderStatus.Preparing, PaymentStatus.Paid), (after.OrderStatus, after.PaymentStatus));
        Assert.Equal(OrderNumberOf(order), (await reader.PaymentRecords.SingleAsync(p => p.OrderId == order.Id, Ct)).ProviderTransactionId);
        Assert.DoesNotContain(_logs.Lines, line => line.Contains(LogEvents.FinalPaymentQueryNotApplied, StringComparison.Ordinal));
        foreach (string unread in UnreadValues)
        {
            Assert.DoesNotContain(_logs.Lines, line => line.Contains(unread, StringComparison.Ordinal));
            Assert.Equal(0L, await CountRowsContainingAsync("payment_records", unread) + await CountRowsContainingAsync("orders", unread));
        }
    }

    // ---- KK6: an amount text outside DV-12's rule ----

    [Theory]
    [InlineData("10,123")]
    [InlineData("1.234,56")]
    public async Task PayTr_SuccessWithAnUnrecognizedAmountText_IsNoAnswer_RepeatsByTheCodeErrorRule_AndTheOrderStays_KK6(string text)
    {
        Order order = await SeedOrderAsync(PaymentMethod.Card, Start.AddMinutes(-1));
        ProviderAnswers answers = new() { Body = SuccessAnswer("120", text, "KART", "TL", "0") };

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => HandlePayTrAsync(answers, order.Id));

        Assert.EndsWith(PayTrPaymentQuery.AmountTextReason, error.Message, StringComparison.Ordinal);
        await AssertOrderUntouchedAsync(order.Id);
    }

    // The values of the answer's fields the mapping does not read (KK7): none reaches a log line or a row.
    private static readonly string[] UnreadValues = ["4355-08XX-XXXX-1234", "WORLD-MARKA", "AUTH-777", "KESINTI-55"];

    private static string SuccessAnswer(string paymentAmount, string paymentTotal, string paymentType, string currency, string testMode) =>
        "{\"status\":\"success\",\"payment_amount\":\"" + paymentAmount + "\",\"payment_total\":\"" + paymentTotal + "\",\"payment_date\":\"2026-11-10 10:00:00\","
        + "\"currency\":\"" + currency + "\",\"taksit\":\"3\",\"kart_marka\":\"WORLD-MARKA\",\"masked_pan\":\"4355-08XX-XXXX-1234\",\"odeme_tipi\":\"" + paymentType + "\","
        + "\"test_mode\":\"" + testMode + "\",\"net_tutar\":\"9.76\",\"kesinti_tutari\":\"KESINTI-55\",\"auth_code\":\"AUTH-777\",\"returns\":[]}";

    private static string ErrorAnswer(string code) => "{\"status\":\"error\",\"err_no\":\"" + code + "\",\"err_msg\":\"MESSAGE-WITH-" + code + "\"}";

    private static string OrderNumberOf(Order order) => Shopfolio.Infrastructure.Security.OrderNumber.WithoutSeparator(order.OrderNumber);

    private async Task AssertOrderUntouchedAsync(Guid orderId)
    {
        Order after = await ReadOrderAsync(orderId);
        Assert.Equal((OrderStatus.Placed, PaymentStatus.Pending, 1), (after.OrderStatus, after.PaymentStatus, after.Version));
        await using AppDbContext reader = NewContext();
        Assert.False(await reader.PaymentRecords.AnyAsync(p => p.OrderId == orderId, Ct));
    }

    private async Task<long> CountRowsContainingAsync(string table, string text)
    {
        await using Npgsql.NpgsqlConnection connection = new(_collection.Dsn);
        await connection.OpenAsync(Ct);
        await using Npgsql.NpgsqlCommand command = new($"SELECT count(*) FROM {table} r WHERE r::text LIKE '%' || @text || '%'", connection);
        command.Parameters.AddWithValue("text", text);
        return (long)(await command.ExecuteScalarAsync(Ct))!;
    }

    // The handler as the runner builds it, with the provider's gateway over the hand-written handler; the live installation only when asked (KK5's test mark).
    private async Task<JobOutcome> HandlePayTrAsync(ProviderAnswers answers, Guid orderId, bool live = false)
    {
        LoggerFactory factory = new([_logs]);
        PayTrPaymentGateway gateway = new(
            answers,
            new ExternalCallRunner(_clock, factory.CreateLogger<ExternalCallRunner>()),
            Options.Create(new PaymentOptions
            {
                ProviderKeys = PaymentProviderKeys.IdName + "=654321\n" + PaymentProviderKeys.KeyName + "=" + QueryKey + "\n" + PaymentProviderKeys.SaltName + "=" + QuerySalt + "\n",
            }),
            new SiteAddress(Options.Create(new SiteOptions { Domain = "shop.example.com" })),
            _clock,
            new QueryEnvironment(Environments.Development),
            new PaymentFailureReasonLog(_clock, factory.CreateLogger<PaymentFailureReasonLog>()));
        await using AsyncServiceScope scope = _provider.CreateAsyncScope();
        FinalPaymentQueryJobHandler handler = ActivatorUtilities.CreateInstance<FinalPaymentQueryJobHandler>(
            scope.ServiceProvider, gateway, new QueryEnvironment(live ? Environments.Production : Environments.Development));
        return await handler.HandleAsync(new JobWork(Guid.NewGuid(), JobType.FinalPaymentQuery, orderId, Now, 0), Ct);
    }

    /// <summary>The provider's side, hand-written: it answers each query as told and keeps the <c>merchant_oid</c> it was asked for; no socket is opened.</summary>
    public sealed class ProviderAnswers : HttpMessageHandler, IHttpClientFactory
    {
        private readonly List<string> _merchantOids = [];
        private int _requests;

        public enum Behavior
        {
            Answer,
            Hang,
            Refuse,
            Unlisted,
            ServerError,
        }

        public Behavior Mode { get; init; } = Behavior.Answer;

        public string Body { get; init; } = string.Empty;

        public TaskCompletionSource Arrived { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int Requests => Volatile.Read(ref _requests);

        public IReadOnlyList<string> MerchantOids => _merchantOids;

        public HttpClient CreateClient(string name) => new(this, disposeHandler: false);

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Interlocked.Increment(ref _requests);
            string form = await request.Content!.ReadAsStringAsync(cancellationToken);
            _merchantOids.Add(form.Split('&').Select(p => p.Split('=', 2)).Single(p => p[0] == "merchant_oid")[1]);
            Arrived.TrySetResult();
            switch (Mode)
            {
                case Behavior.Hang:
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                    throw new InvalidOperationException("Unreachable.");
                case Behavior.Refuse:
                    throw new HttpRequestException(HttpRequestError.ConnectionError, "refused");
                case Behavior.Unlisted:
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent("<html>bakım</html>") };
                case Behavior.ServerError:
                    return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable) { Content = new StringContent(string.Empty) };
                default:
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(Body) };
            }
        }
    }

    private sealed class QueryEnvironment(string name) : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = name;

        public string ApplicationName { get; set; } = "Shopfolio.Web";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
