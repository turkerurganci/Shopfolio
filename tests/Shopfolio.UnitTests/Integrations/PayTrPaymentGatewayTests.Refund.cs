using System.Globalization;
using System.Net;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The provider's card refund and its query (T405 KK1-KK3, KK5, KK6, KK9; 08 2.1.9; 09 11.1 items 3, 6 and 8, 14.2): one form-encoded request to the refund address with
/// five fields and the <c>paytr_token</c> over four inputs - the reference outside it - matched with a value computed outside .NET, within the job limit; the amount's
/// culture-free text; the reference as the record's id without dashes; the three answer classes; the query's four states; and no call without the keys.
/// </summary>
public sealed partial class PayTrPaymentGatewayTests
{
    private const string RefundOrderNumber = "2026-7K4M9P";

    // KK1: `printf '%s' '12345620267K4M9P10.00salt-sample' | openssl dgst -sha256 -hmac key-sample -binary | base64`.
    private const string KnownRefundToken = "f02lP+RU1mGeoMnKm5bS7+ftp3DjTJfMyMDnF3ameFg=";

    private static readonly Guid RecordId = new("0b1c2d3e-4f50-6172-8394-a5b6c7d8e9f0");

    private static CardRefund Refund(decimal amount = 10m) => new(RefundOrderNumber, amount, RecordId);

    private static string ReturnsAnswer(string items) => "{\"status\":\"success\",\"payment_amount\":\"120\",\"payment_total\":\"120\",\"returns\":[" + items + "]}";

    private static string Item(string reference, string amount) => "{\"return_amount\":\"" + amount + "\",\"reference_no\":\"" + reference + "\",\"return_date\":\"2026-11-10 10:00:00\"}";

    // ---- KK1, KK2, KK3: the request's form, its token, its amount and its reference ----

    [Fact]
    public async Task Refund_PostsOnceToTheRefundAddress_FormEncoded_WithFiveFields_AndTheKnownFourInputToken_KK1_KK3()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: "{\"status\":\"success\",\"is_test\":\"1\"}");

        Assert.Equal(new RefundRequestResult.Accepted(), await gateway.RequestRefundAsync(Refund(), Ct));

        CapturedRequest sent = Assert.Single(handler.Sent);
        Assert.Equal(HttpMethod.Post, sent.Method);
        Assert.Equal(new Uri("https://www.paytr.com/odeme/iade"), sent.Address);
        Assert.Equal("application/x-www-form-urlencoded", sent.ContentType);
        Dictionary<string, string> fields = Fields(sent.Body);
        Assert.Equal(["merchant_id", "merchant_oid", "return_amount", "paytr_token", "reference_no"], fields.Keys);
        Assert.Equal((SampleId, "20267K4M9P", "10.00", KnownRefundToken), (fields["merchant_id"], fields["merchant_oid"], fields["return_amount"], fields["paytr_token"]));
        Assert.DoesNotContain(SampleKey, sent.Body, StringComparison.Ordinal);
        Assert.DoesNotContain(SampleSalt, sent.Body, StringComparison.Ordinal);

        // KK3: the reference is the record's id without dashes, and it does not go into the token (the token over the four inputs is the known one above).
        Assert.Equal("0b1c2d3e4f5061728394a5b6c7d8e9f0", fields["reference_no"]);
        Assert.Equal(RecordId.ToString("N"), PayTrRefund.ReferenceOf(RecordId));
        Assert.Equal(KnownRefundToken, PayTrSignature.Of(SampleKey, PayTrRefund.SignedMessage(SampleId, "20267K4M9P", "10.00", SampleSalt)));
        Assert.NotEqual(KnownRefundToken, PayTrSignature.Of(SampleKey, PayTrRefund.SignedMessage(SampleId, "20267K4M9P", "10.00", SampleSalt) + fields["reference_no"]));

        // The test mark outside the live installation writes nothing.
        Assert.DoesNotContain(harness.Lines, line => line.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("tr-TR", 10, "10.00")]
    [InlineData("en-US", 10, "10.00")]
    [InlineData("tr-TR", 1234.5, "1234.50")]
    [InlineData("de-DE", 0.07, "0.07")]
    public async Task Refund_Amount_IsTwoDecimalsWithAPoint_WhateverTheCulture_AndTheTokenIsOverTheSentText_KK2(string culture, double amount, string expected)
    {
        CultureInfo original = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);
            using RunnerHarness harness = new();
            (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: "{\"status\":\"success\"}");

            await gateway.RequestRefundAsync(Refund((decimal)amount), Ct);

            Dictionary<string, string> fields = Fields(Assert.Single(handler.Sent).Body);
            Assert.Equal(expected, fields["return_amount"]);
            Assert.Equal(PaymentAmounts.ToPlainText((decimal)amount), fields["return_amount"]);
            Assert.Equal(PayTrSignature.Of(SampleKey, PayTrRefund.SignedMessage(SampleId, "20267K4M9P", expected, SampleSalt)), fields["paytr_token"]);
        }
        finally
        {
            CultureInfo.CurrentCulture = original;
        }
    }

    [Fact]
    public async Task Refund_ThatHangs_EndsAtTheThirtySecondJobLimit_AsUnknown_WithOneRequest_KK1_KK5()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());
        handler.Hang = true;

        Task<RefundRequestResult> call = gateway.RequestRefundAsync(Refund(), Ct);
        harness.Time.Advance(TimeSpan.FromSeconds(29));
        Assert.False(call.IsCompleted);
        harness.Time.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(TimeSpan.FromSeconds(30), ExternalCallTimeouts.Job);
        Assert.Equal(new RefundRequestResult.Unknown(ExternalCallFailure.TimeoutReason), await call);
        Assert.Single(handler.Sent);
    }

    // ---- KK5: the three answer classes ----

    [Theory]
    [InlineData("{\"status\":\"failed\",\"err_no\":\"005\",\"err_msg\":\"MESSAGE-WITH-20267K4M9P\"}", "err:005")]
    [InlineData("{\"status\":\"error\",\"err_no\":\"011\",\"err_msg\":\"bakiye yetersiz\"}", "err:011")]
    [InlineData("{\"status\":\"failed\",\"err_msg\":\"reddedildi\"}", PayTrRefund.RefusedReason)]
    public async Task Refund_FailedOrError_IsNotCompleted_WithTheCodeWritten_AndTheMessageMasked_KK5(string body, string reason)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: body);

        Assert.Equal(new RefundRequestResult.NotCompleted(reason), await gateway.RequestRefundAsync(Refund(), Ct));
        Assert.Single(handler.Sent);
        string line = Assert.Single(harness.Lines, l => l.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal));
        Assert.Contains(reason, line, StringComparison.Ordinal);
        Assert.DoesNotContain("20267K4M9P", line, StringComparison.Ordinal);
        Assert.DoesNotContain(KnownRefundToken, line, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(500, "{\"status\":\"success\"}")]
    [InlineData(200, "<html>bakim</html>")]
    [InlineData(200, "{\"status\":\"waiting\"}")]
    [InlineData(200, "[]")]
    public async Task Refund_AnswerOutsideTheTable_IsUnknown_NotARefusal_KK5(int status, string body)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: body);
        handler.Status = (HttpStatusCode)status;

        RefundRequestResult result = await gateway.RequestRefundAsync(Refund(), Ct);

        Assert.IsType<RefundRequestResult.Unknown>(result);
        Assert.Single(handler.Sent);
        Assert.Single(harness.Lines, l => l.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Refund_ConnectionError_IsUnknown_AndIsNotSentAgain_KK5()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Refuse };
        PayTrPaymentGateway gateway = GatewayOver(harness, handler);

        Assert.Equal(
            new RefundRequestResult.Unknown(ExternalCallRunner.ConnectionReason(HttpRequestError.ConnectionError)),
            await gateway.RequestRefundAsync(Refund(), Ct));
        Assert.Equal(1, handler.Requests);
    }

    [Fact]
    public async Task Refund_AcceptedAsATest_OnTheLiveInstallation_WritesTheTestLine()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), live: true, answer: "{\"status\":\"success\",\"is_test\":\"1\"}");

        Assert.Equal(new RefundRequestResult.Accepted(), await gateway.RequestRefundAsync(Refund(), Ct));
        string line = Assert.Single(harness.Lines, l => l.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal));
        Assert.Contains(PayTrRefund.TestOnLiveReason, line, StringComparison.Ordinal);
    }

    // ---- KK9: no request without the keys ----

    [Fact]
    public async Task Refund_AndItsQuery_WithoutTheKeys_SendNothing_AndTheRefundDidNotHappen_KK9()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(withKey: false));

        Assert.Equal(new RefundRequestResult.NotCompleted(PayTrPaymentGateway.KeysMissingReason), await gateway.RequestRefundAsync(Refund(), Ct));
        Assert.Equal(new RefundQueryResult.NoResult(), await gateway.QueryRefundAsync(Refund(), Ct));
        Assert.Empty(handler.Sent);
    }

    // ---- KK6: the query's four states ----

    [Fact]
    public async Task RefundQuery_ItemWithTheReferenceAndTheAmount_IsCompleted_AndAsksTheFinalQuerysAddress_KK6()
    {
        using RunnerHarness harness = new();
        string reference = PayTrRefund.ReferenceOf(RecordId);
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(
            harness, KeysContent(), answer: ReturnsAnswer(Item("other", "10") + "," + Item(reference.ToUpperInvariant(), "10")));

        Assert.Equal(new RefundQueryResult.Completed(), await gateway.QueryRefundAsync(Refund(), Ct));

        CapturedRequest sent = Assert.Single(handler.Sent);
        Assert.Equal(new Uri("https://www.paytr.com/odeme/durum-sorgu"), sent.Address);
        Assert.Equal(["merchant_id", "merchant_oid", "paytr_token"], Fields(sent.Body).Keys);
        Assert.Equal(KnownQueryToken, Fields(sent.Body)["paytr_token"]);
        Assert.DoesNotContain(harness.Lines, line => line.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal));
    }

    [Fact]
    public async Task RefundQuery_004_IsNoResult_KK6()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), answer: ErrorAnswer("004"));

        Assert.Equal(new RefundQueryResult.NoResult(), await gateway.QueryRefundAsync(Refund(), Ct));
        Assert.DoesNotContain(harness.Lines, line => line.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal));
    }

    [Fact]
    public async Task RefundQuery_WithoutTheItem_OrWithAnItemOfAnotherAmount_IsNoResult_KK6()
    {
        string reference = PayTrRefund.ReferenceOf(RecordId);
        foreach (string answer in new[] { ReturnsAnswer(string.Empty), ReturnsAnswer(Item("other", "10")), ReturnsAnswer(Item(reference, "9.99")), ReturnsAnswer(Item(reference, "100")) })
        {
            using RunnerHarness harness = new();
            (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), answer: answer);

            Assert.Equal(new RefundQueryResult.NoResult(), await gateway.QueryRefundAsync(Refund(), Ct));
        }
    }

    [Theory]
    [InlineData(500, "{}")]
    [InlineData(200, "not json")]
    [InlineData(200, "{\"status\":\"waiting\"}")]
    public async Task RefundQuery_AnswerOutsideTheTable_IsNoResult_AndWritesItsReason(int status, string body)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: body);
        handler.Status = (HttpStatusCode)status;

        Assert.Equal(new RefundQueryResult.NoResult(), await gateway.QueryRefundAsync(Refund(), Ct));
        Assert.Single(harness.Lines, l => l.Contains(LogEvents.ExternalCallFailed, StringComparison.Ordinal));
    }
}
