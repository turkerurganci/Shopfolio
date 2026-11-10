using System.Net;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The start's table and the layer's one call (T404 KK1, KK2, KK5, KK6; 08 2.1.8; 09 11.1 items 3, 4, 6 and 8): each of the table's six rows is a case - the unreachable
/// provider, the 15 second limit and a server error; a <c>failed</c> answer whose <c>reason</c> is written without the request's values; the second token refusal;
/// the token; the failed payment, which is the notification's and not the start's; and an answer outside the table. Every call goes once, whatever its answer.
/// </summary>
public sealed partial class PayTrPaymentGatewayTests
{
    private const string SecondTokenSample = "Bu sipariş numarası için daha önce token alınmış";

    // ---- KK1 row 1: no connection, the 15 second limit, 5xx ----

    [Fact]
    public async Task Row1_TheProviderNotReached_IsAConnectionError_WrittenWithTheTransportsName_T404_KK1()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Refuse };

        PaymentStartResult result = await GatewayOver(harness, handler).StartAsync(Request(), Ct);

        string reason = ExternalCallRunner.ConnectionReason(HttpRequestError.ConnectionError);
        Assert.Equal(new PaymentStartResult.ConnectionFailed(reason), result);
        Assert.Equal(1, handler.Requests);
        Assert.Contains(harness.Lines, line => line.Contains(nameof(IntegrationOperation.PaymentStart), StringComparison.Ordinal) && line.Contains(reason, StringComparison.Ordinal));
    }

    [Fact]
    public async Task Row1_TheFifteenSecondLimit_IsAConnectionError_AndOneSecondLessIsNot_T404_KK1()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Hang };

        Task<PaymentStartResult> call = GatewayOver(harness, handler).StartAsync(Request(), Ct);
        harness.Time.Advance(ExternalCallTimeouts.UserWaiting - TimeSpan.FromSeconds(1));
        Assert.False(call.IsCompleted);
        harness.Time.Advance(TimeSpan.FromSeconds(1));

        Assert.Equal(new PaymentStartResult.ConnectionFailed(ExternalCallFailure.TimeoutReason), await call);
        Assert.Equal(TimeSpan.FromSeconds(15), ExternalCallTimeouts.UserWaiting);
        Assert.Equal(1, handler.Requests);
    }

    [Theory]
    [InlineData(500)]
    [InlineData(502)]
    [InlineData(503)]
    [InlineData(504)]
    public async Task Row1_AServerError_IsAConnectionError_WithItsNumber_T404_KK1(int status)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent());
        handler.Status = (HttpStatusCode)status;

        PaymentStartResult result = await gateway.StartAsync(Request(), Ct);

        Assert.Equal(new PaymentStartResult.ConnectionFailed(PayTrPaymentGateway.HttpReason(status)), result);
        Assert.Single(handler.Sent);
    }

    // ---- KK1 row 2: failed + reason - the reason is written, a field's value is not ----

    [Fact]
    public async Task Row2_AFailedAnswer_IsAConnectionError_AndWritesTheReason_WithoutTheRequestsValues_T404_KK1()
    {
        using RunnerHarness harness = new();
        const string answer = "{\"status\":\"failed\",\"reason\":\"Zorunlu alan degeri gecersiz: email MUSTERI@example.com, merchant_oid 20267K4M9P, user_ip 203.0.113.7\\nyeni satir\"}";
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: answer);

        PaymentStartResult result = await gateway.StartAsync(Request(), Ct);

        const string written = "failed:Zorunlu alan degeri gecersiz: email [field], merchant_oid [field], user_ip [field] yeni satir";
        Assert.Equal(new PaymentStartResult.ConnectionFailed(written), result);
        Assert.Single(handler.Sent);
        Assert.Contains(harness.Lines, line => line.Contains(nameof(IntegrationOperation.PaymentStart), StringComparison.Ordinal) && line.Contains(written, StringComparison.Ordinal));
        Assert.DoesNotContain(harness.Lines, line => line.Contains("musteri@example.com", StringComparison.OrdinalIgnoreCase)
            || line.Contains("20267K4M9P", StringComparison.Ordinal)
            || line.Contains("203.0.113.7", StringComparison.Ordinal)
            || line.Contains(SampleId, StringComparison.Ordinal));
    }

    [Fact]
    public void Row2_EveryValueTheRequestCarried_IsTakenOut_TheLongestFirst_AndAShortFlagStays_T404_KK1()
    {
        KeyValuePair<string, string>[] sent = [new("email", "a@b.co"), new("user_basket", "W1siU2lwYXJpxZ8"), new("lang", "tr"), new("no_installment", "0"), new("merchant_oid", "W1si")];

        Assert.Equal(
            "basket [field] / [field] / tr 0",
            PayTrPaymentGateway.WithoutFieldValues("basket W1siU2lwYXJpxZ8 / A@B.CO / tr 0", sent));
        Assert.Equal("a b c", PayTrPaymentGateway.WithoutFieldValues("a\rb\tc", sent));
        Assert.Equal("failed:", PayTrPaymentGateway.FailedReason + ":" + PayTrPaymentGateway.WithoutFieldValues(string.Empty, sent));
    }

    // ---- KK1 row 3, KK2: the second token refusal is not a connection error ----

    [Fact]
    public void Row3_AFailedAnswerWithTheSecondTokenText_IsTheSecondTokenRefusal_NotAConnectionError_T404_KK2()
    {
        string body = "{\"status\":\"failed\",\"reason\":\" " + SecondTokenSample + " \"}";

        PaymentStartResult? result = PayTrPaymentGateway.ReadAnswer(200, body, [], [SecondTokenSample], out ExternalCallFailure? failure);

        Assert.Equal(new PaymentStartResult.SecondTokenRejected(PayTrPaymentGateway.SecondTokenReason), result);
        Assert.Null(failure);
        Assert.IsNotType<PaymentStartResult.ConnectionFailed>(result);
    }

    [Fact]
    public async Task Row3_UntilDv11GivesTheText_AFailedAnswerCannotBeToldApart_AndIsTheConnectionErrorRow_T404_KK2()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent(), answer: "{\"status\":\"failed\",\"reason\":\"" + SecondTokenSample + "\"}");

        PaymentStartResult result = await gateway.StartAsync(Request(), Ct);

        // 08 2.1.8: "ayırt edilemeyen failed yanıtı yukarıdaki satırdadır" - the text is observed on the test store (DV-11, D-1075).
        Assert.Empty(PayTrPaymentGateway.SecondTokenReasonTexts);
        Assert.Equal(new PaymentStartResult.ConnectionFailed(PayTrPaymentGateway.FailedReason + ":" + SecondTokenSample), result);
    }

    // ---- KK1 row 4: the token ----

    [Fact]
    public void Row4_SuccessWithAToken_IsTheFramesAddress_T404_KK1()
    {
        PaymentStartResult? result = PayTrPaymentGateway.ReadAnswer(200, "{\"status\":\"success\",\"token\":\"abc\"}", [], [], out ExternalCallFailure? failure);

        Assert.Equal(new PaymentStartResult.Started(new Uri(PayTrStartForm.FrameAddressRoot + "abc")), result);
        Assert.Null(failure);
    }

    // ---- KK1 row 5: the failed payment is the notification's result, never the start's ----

    [Fact]
    public void Row5_AFailedPayment_IsTheNotificationsResult_TheStartHasNoSuchClass_T404_KK1()
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, _) = Gateway(harness, KeysContent());

        Assert.Equal(NotificationResult.PaymentFailed, gateway.ReadNotificationResult(PayTrNotification.SaleEventType, PayTrNotification.FailedStatus));
        Assert.Equal(
            [nameof(PaymentStartResult.ConnectionFailed), nameof(PaymentStartResult.SecondTokenRejected), nameof(PaymentStartResult.Started)],
            typeof(PaymentStartResult).GetNestedTypes().Where(t => t.IsSubclassOf(typeof(PaymentStartResult))).Select(t => t.Name).Order(StringComparer.Ordinal));
    }

    // ---- KK1 row 6: every answer outside the table is a connection error ----

    [Theory]
    [InlineData(200, "{\"status\":\"success\"}", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(200, "{\"status\":\"success\",\"token\":5}", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(200, "{\"status\":\"waiting\"}", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(200, "{\"reason\":\"x\"}", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(200, "", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(200, "<html>bakim</html>", PayTrPaymentGateway.UnrecognizedBodyReason)]
    [InlineData(302, "", "http:302")]
    [InlineData(400, "{\"status\":\"failed\",\"reason\":\"x\"}", "http:400")]
    [InlineData(404, "", "http:404")]
    [InlineData(429, "", "http:429")]
    public async Task Row6_AnAnswerOutsideTheTable_IsAConnectionError_AndIsWritten_T404_KK1(int status, string body, string reason)
    {
        using RunnerHarness harness = new();
        (PayTrPaymentGateway gateway, CapturingHandler handler) = Gateway(harness, KeysContent(), answer: body);
        handler.Status = (HttpStatusCode)status;

        PaymentStartResult result = await gateway.StartAsync(Request(), Ct);

        Assert.Equal(new PaymentStartResult.ConnectionFailed(reason), result);
        Assert.Single(handler.Sent);
        Assert.Contains(harness.Lines, line => line.Contains(nameof(IntegrationOperation.PaymentStart), StringComparison.Ordinal) && line.Contains(reason, StringComparison.Ordinal));
    }

    // ---- KK5, KK6: an unanswered call is not sent again; every call is one request ----

    [Theory]
    [InlineData("Hang")]
    [InlineData("Refuse")]
    [InlineData("Answer")]
    public async Task EveryStart_IsOneRequest_WhateverItsAnswer_T404_KK6(string behavior)
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = Enum.Parse<ScriptedHandler.Behavior>(behavior), Status = HttpStatusCode.ServiceUnavailable };

        Task<PaymentStartResult> call = GatewayOver(harness, handler).StartAsync(Request(), Ct);
        harness.Time.Advance(ExternalCallTimeouts.UserWaiting);

        Assert.IsType<PaymentStartResult.ConnectionFailed>(await call);
        Assert.Equal(1, handler.Requests);
    }

    [Theory]
    [InlineData("Hang")]
    [InlineData("Refuse")]
    [InlineData("Answer")]
    public async Task AnUnansweredQuery_IsUnknown_AndNoSecondRequestGoes_T404_KK5_KK6(string behavior)
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = Enum.Parse<ScriptedHandler.Behavior>(behavior), Status = HttpStatusCode.BadGateway };

        Task<PaymentQueryResult> call = GatewayOver(harness, handler).QueryPaymentAsync(OrderNumberSample, Ct);
        harness.Time.Advance(ExternalCallTimeouts.Job);

        Assert.IsType<PaymentQueryResult.NoResponse>(await call);
        Assert.Equal(1, handler.Requests);
    }
}
