using System.Net;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The provider's final query (T403 KK1-KK3, KK6, KK7; 08 2.1.6; 09 11.1 items 3, 4, 6 and 8): one form-encoded request to the query address with three fields and the
/// <c>paytr_token</c> over three inputs (matched with a value computed outside .NET), within the job limit; the answer table - a successful payment, <c>004</c>, the
/// three request errors, the unanswered call and the answer outside the table - each a test; an amount text outside DV-12's rule; and the card and personal fields of the
/// answer, which reach neither the result nor a log line.
/// </summary>
public sealed partial class PayTrPaymentGatewayTests
{
    private const string OrderNumberSample = "2026-7K4M9P";
    private const string MerchantOidSample = "20267K4M9P";

    // KK1: `printf '%s' '12345620267K4M9Psalt-sample' | openssl dgst -sha256 -hmac key-sample -binary | base64`.
    private const string KnownQueryToken = "iXALazpQWzs78C0/qLAFO5ebuRd0IKileoJRGSQiroQ=";

    // A successful answer as the provider's document lists it, with every field the mapping does not read filled with a value a test can look for.
    private static string SuccessAnswer(
        string paymentAmount = "120", string paymentTotal = "124,5", string paymentType = "KART", string currency = "TL", string testMode = "0") =>
        "{\"status\":\"success\",\"payment_amount\":\"" + paymentAmount + "\",\"payment_total\":\"" + paymentTotal + "\",\"payment_date\":\"2026-11-10 10:00:00\","
        + "\"currency\":\"" + currency + "\",\"taksit\":\"3\",\"kart_marka\":\"WORLD-MARKA\",\"masked_pan\":\"4355-08XX-XXXX-1234\",\"odeme_tipi\":\"" + paymentType + "\","
        + "\"test_mode\":\"" + testMode + "\",\"net_tutar\":\"9.76\",\"kesinti_tutari\":\"KESINTI-55\",\"auth_code\":\"AUTH-777\",\"auth_date\":\"2026-11-10\",\"returns\":[]}";

    private static string ErrorAnswer(string code) => "{\"status\":\"error\",\"err_no\":\"" + code + "\",\"err_msg\":\"MESSAGE-WITH-" + MerchantOidSample + "\"}";

    // ---- KK1: the request's form and its limit ----

    [Fact]
    public async Task Query_PostsOnceToTheQueryAddress_FormEncoded_WithThreeFields_AndTheKnownToken_KK1()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: ErrorAnswer("004"));

        await gateway.QueryPaymentAsync(OrderNumberSample, Ct);

        CapturedRequest sent = Assert.Single(handler.Sent);
        Assert.Equal(HttpMethod.Post, sent.Method);
        Assert.Equal(new Uri("https://www.paytr.com/odeme/durum-sorgu"), sent.Address);
        Assert.Equal("application/x-www-form-urlencoded", sent.ContentType);
        Dictionary<string, string> fields = Fields(sent.Body);
        Assert.Equal(["merchant_id", "merchant_oid", "paytr_token"], fields.Keys);
        Assert.Equal((SampleId, MerchantOidSample, KnownQueryToken), (fields["merchant_id"], fields["merchant_oid"], fields["paytr_token"]));
        Assert.DoesNotContain(SampleKey, sent.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(SampleSalt, sent.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Query_ThatHangs_EndsAtTheThirtySecondJobLimit_AsNoResponse_WithOneRequest_KK1_KK3()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());
        handler.Hang = true;

        Task<PaymentQueryResult> call = gateway.QueryPaymentAsync(OrderNumberSample, Ct);
        harness.Time.Advance(TimeSpan.FromSeconds(29));
        Assert.False(call.IsCompleted);
        harness.Time.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(TimeSpan.FromSeconds(30), ExternalCallTimeouts.Job);
        Assert.Equal(new PaymentQueryResult.NoResponse(ExternalCallFailure.TimeoutReason), await call);
        Assert.Single(handler.Sent);
    }

    [Fact]
    public async Task Query_WithoutTheKeys_SendsNothing_AndIsACodeErrorClass()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(withSalt: false));

        Assert.Equal(new PaymentQueryResult.NoResponse(PayTrPaymentGateway.KeysMissingReason, CodeError: true), await gateway.QueryPaymentAsync(OrderNumberSample, Ct));
        Assert.Empty(handler.Sent);
    }

    // ---- KK2: 004 is an answer ----

    [Fact]
    public async Task Query_004_IsNoPayment_ASeparateClass_KK2()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), answer: ErrorAnswer("004"));

        Assert.Equal(new PaymentQueryResult.NoPayment(), await gateway.QueryPaymentAsync(OrderNumberSample, Ct));
        Assert.DoesNotContain(harness.Lines, line => line.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal));
    }

    // ---- KK3, KK4: the six error inputs - three request errors (code error rule), the timeout (above), the connection error and the answer outside the table ----

    [Theory]
    [InlineData("001")]
    [InlineData("002")]
    [InlineData("003")]
    public async Task Query_RequestErrors_AreNoResponse_OfTheCodeErrorClass_AndTheirCodeIsWritten_NotTheMessage_KK3_KK4(string code)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), answer: ErrorAnswer(code));

        Assert.Equal(new PaymentQueryResult.NoResponse("err:" + code, CodeError: true), await gateway.QueryPaymentAsync(OrderNumberSample, Ct));
        string line = Assert.Single(harness.Lines, l => l.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal));
        Assert.Contains("err:" + code, line, StringComparison.Ordinal);
        Assert.DoesNotContain("MESSAGE-WITH", line, StringComparison.Ordinal);
        Assert.DoesNotContain(MerchantOidSample, line, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Query_ConnectionError_IsNoResponse_OfTheFiveMinuteClass_KK3_KK4()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Refuse };
        PayTrPaymentGateway gateway = GatewayOver(harness, handler);

        PaymentQueryResult result = await gateway.QueryPaymentAsync(OrderNumberSample, Ct);

        Assert.Equal(new PaymentQueryResult.NoResponse(ExternalCallRunner.ConnectionReason(HttpRequestError.ConnectionError)), result);
        Assert.False(((PaymentQueryResult.NoResponse)result).CodeError);
        Assert.Equal(1, handler.Requests);
    }

    [Theory]
    [InlineData(200, "{\"status\":\"waiting\"}", "body")]
    [InlineData(200, "not json", "body")]
    [InlineData(200, "[1,2]", "body")]
    [InlineData(200, "{\"status\":\"error\",\"err_no\":\"009\"}", "err:009")]
    [InlineData(200, "{\"status\":\"error\",\"err_msg\":\"x\"}", "body")]
    [InlineData(200, "{\"status\":\"error\",\"err_no\":\"PROVIDER TEXT WITH A FIELD\"}", "body")]
    [InlineData(500, "", "http:500")]
    [InlineData(404, "{\"status\":\"success\"}", "http:404")]
    public async Task Query_AnAnswerOutsideTheTable_IsNoResponse_OfTheFiveMinuteClass_AndIsWritten_KK3_KK4(int status, string body, string reason)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: body);
        handler.Status = (HttpStatusCode)status;

        Assert.Equal(new PaymentQueryResult.NoResponse(reason), await gateway.QueryPaymentAsync(OrderNumberSample, Ct));
        Assert.Contains(harness.Lines, line => line.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal) && line.Contains(reason, StringComparison.Ordinal));
    }

    // ---- KK5 (the layer's part), KK6, KK7: the successful answer ----

    [Fact]
    public async Task Query_Success_IsPaid_WithTheAskedReferenceAsTheId_TheAmountsReadAndTheMarks_KK5_KK7()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), answer: SuccessAnswer());

        PaymentQueryResult result = await gateway.QueryPaymentAsync(OrderNumberSample, Ct);

        Assert.Equal(new PaymentQueryResult.Paid(MerchantOidSample, 120m, 124.50m, PaidByCard: true, InLira: true, TestMode: false), result);
    }

    [Theory]
    [InlineData("EFT", "TL", "0", false, true, false)]
    [InlineData("KART", "TRY", "0", true, true, false)]
    [InlineData("KART", "USD", "0", true, false, false)]
    [InlineData("KART", "tl", "0", true, false, false)]
    [InlineData("kart", "TL", "1", false, true, true)]
    public async Task Query_Success_ReadsThePaymentType_TheCurrency_AndTheTestMark_AsTheyCame_KK5(
        string paymentType, string currency, string testMode, bool byCard, bool inLira, bool test)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), answer: SuccessAnswer(paymentType: paymentType, currency: currency, testMode: testMode));

        PaymentQueryResult.Paid paid = Assert.IsType<PaymentQueryResult.Paid>(await gateway.QueryPaymentAsync(OrderNumberSample, Ct));

        Assert.Equal((byCard, inLira, test), (paid.PaidByCard, paid.InLira, paid.TestMode));
    }

    [Theory]
    [InlineData("10,123", "120")]
    [InlineData("1.234,56", "120")]
    [InlineData("120", "1,234.56")]
    [InlineData("120", "")]
    public async Task Query_SuccessWithAnAmountTextOutsideTheRule_IsNoResponse_OfTheCodeErrorClass_KK6(string paymentAmount, string paymentTotal)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), answer: SuccessAnswer(paymentAmount, paymentTotal));

        Assert.Equal(
            new PaymentQueryResult.NoResponse(PayTrPaymentQuery.AmountTextReason, CodeError: true),
            await gateway.QueryPaymentAsync(OrderNumberSample, Ct));
        Assert.Contains(harness.Lines, line => line.Contains(PayTrPaymentQuery.AmountTextReason, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Query_TheCardAndPersonalFieldsOfTheAnswer_AreInNeitherTheResultNorALogLine_KK7()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), answer: SuccessAnswer(paymentTotal: "1.234,56"));
        string[] unread = ["4355-08XX-XXXX-1234", "WORLD-MARKA", "AUTH-777", "2026-11-10 10:00:00", "9.76", "KESINTI-55"];

        PaymentQueryResult unrecognized = await gateway.QueryPaymentAsync(OrderNumberSample, Ct);
        (PayTrPaymentGateway readable, _) = Gateway(harness, KeysContent(), answer: SuccessAnswer());
        PaymentQueryResult paid = await readable.QueryPaymentAsync(OrderNumberSample, Ct);

        // The class has no field for them (the record's members are the six of the mapping), and its text carries none of them.
        Assert.Equal(
            ["ProviderTransactionId", "PaymentAmount", "PaymentTotal", "PaidByCard", "InLira", "TestMode"],
            typeof(PaymentQueryResult.Paid).GetProperties().Where(p => p.DeclaringType == typeof(PaymentQueryResult.Paid)).Select(p => p.Name));
        foreach (string value in unread)
        {
            Assert.DoesNotContain(value, paid.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain(value, unrecognized.ToString(), StringComparison.Ordinal);
            Assert.DoesNotContain(harness.Lines, line => line.Contains(value, StringComparison.Ordinal));
        }
    }

    private static PayTrPaymentGateway GatewayOver(RunnerHarness harness, HttpMessageHandler handler) =>
        new(
            new OneClientFactory(handler),
            harness.Runner,
            Microsoft.Extensions.Options.Options.Create(new Shopfolio.Infrastructure.InstallationSettings.PaymentOptions { ProviderKeys = KeysContent() }),
            new Shopfolio.Infrastructure.SearchEngine.SiteAddress(
                Microsoft.Extensions.Options.Options.Create(new Shopfolio.Infrastructure.InstallationSettings.SiteOptions { Domain = Domain })),
            harness.Time,
            new Environment(Microsoft.Extensions.Hosting.Environments.Development),
            new PaymentFailureReasonLog(harness.Time, harness.LoggerFor<PaymentFailureReasonLog>()));
}
