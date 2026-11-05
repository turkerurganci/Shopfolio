using System.Net;
using System.Text.Json;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Logging;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The common face of the outside calls (T202 criteria 3, 5, 7 and 8, KK3, KK5, KK7, KK8; 08 3.1 rules 2, 3 and 5; 09 11.1 items 3, 4 and 8):
/// the limit runs on the fake clock with a hanging fake handler - one tick short of the limit the call still waits, at the limit it ends as its
/// family's timeout class -, every call goes once, and a failure's log line carries only the job, the kind and the reason code.
/// </summary>
public sealed class ExternalCallRunnerTests
{
    private const string RequestMarker = "REQUEST-FIELD-VALUE-7731";
    private const string KeyMarker = "PLACEHOLDER-KEY-4410";
    private const string UnreadFieldMarker = "UNREAD-ANSWER-FIELD-9902";

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static HttpRequestMessage Request()
    {
        HttpRequestMessage request = new(HttpMethod.Post, "https://service.invalid/job")
        {
            Content = new FormUrlEncodedContent([new("field", RequestMarker), new("signature", KeyMarker)]),
        };
        request.Headers.TryAddWithoutValidation("Authorization", "Bearer " + KeyMarker);
        return request;
    }

    // Sends one request through the fake handler; an answered request is a success of the family here (the mapping of an answer is the provider's).
    private static Func<CancellationToken, Task<TResult>> Send<TResult>(HttpClient client, TResult answered) =>
        async token =>
        {
            using HttpRequestMessage request = Request();
            using HttpResponseMessage response = await client.SendAsync(request, token);
            return answered;
        };

    private static async Task<TResult> HangUntilTheLimitAsync<TResult>(IntegrationOperation operation, TResult answered, TimeSpan limit, ScriptedHandler handler, RunnerHarness harness)
        where TResult : IExternalCallResult<TResult>
    {
        using HttpClient client = new(handler) { Timeout = Timeout.InfiniteTimeSpan };
        Task<TResult> call = harness.Runner.RunAsync(operation, Send(client, answered), Ct);

        harness.Time.Advance(limit - TimeSpan.FromTicks(1));
        Assert.False(call.IsCompleted);

        harness.Time.Advance(TimeSpan.FromTicks(1));
        return await call;
    }

    [Fact]
    public async Task RunAsync_PaymentStartThatHangs_EndsAtTheUserWaitingLimitAsAConnectionError()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Hang };

        PaymentStartResult result = await HangUntilTheLimitAsync<PaymentStartResult>(
            IntegrationOperation.PaymentStart, new PaymentStartResult.Started(new Uri("https://service.invalid/frame")), ExternalCallTimeouts.UserWaiting, handler, harness);

        Assert.Equal(new PaymentStartResult.ConnectionFailed(ExternalCallFailure.TimeoutReason), result);
        Assert.Equal(1, handler.Requests);
    }

    [Fact]
    public async Task RunAsync_IdentityVerificationThatHangs_EndsAtTheUserWaitingLimitAsUnavailable()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Hang };

        GoogleIdentityResult result = await HangUntilTheLimitAsync<GoogleIdentityResult>(
            IntegrationOperation.IdentityVerify, new GoogleIdentityResult.Failed("answered"), ExternalCallTimeouts.UserWaiting, handler, harness);

        Assert.Equal(new GoogleIdentityResult.Unavailable(ExternalCallFailure.TimeoutReason), result);
        Assert.Equal(1, handler.Requests);
    }

    [Fact]
    public async Task RunAsync_RefundRequestThatHangs_EndsAtTheJobLimitAsUnknown_AndIsNotSentAgain()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Hang };

        RefundRequestResult result = await HangUntilTheLimitAsync<RefundRequestResult>(
            IntegrationOperation.RefundRequest, new RefundRequestResult.Accepted(), ExternalCallTimeouts.Job, handler, harness);

        Assert.Equal(new RefundRequestResult.Unknown(ExternalCallFailure.TimeoutReason), result);
        Assert.Equal(1, handler.Requests);
    }

    [Fact]
    public async Task RunAsync_PaymentQueryThatHangs_EndsAtTheJobLimitAsNoResponse()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Hang };

        PaymentQueryResult result = await HangUntilTheLimitAsync<PaymentQueryResult>(
            IntegrationOperation.PaymentQuery, new PaymentQueryResult.NoPayment(), ExternalCallTimeouts.Job, handler, harness);

        Assert.Equal(new PaymentQueryResult.NoResponse(ExternalCallFailure.TimeoutReason), result);
        Assert.Equal(1, handler.Requests);
    }

    [Fact]
    public async Task RunAsync_EmailSendThatHangs_EndsAtTheJobLimitAsAChannelError()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Hang };

        EmailSendResult result = await HangUntilTheLimitAsync<EmailSendResult>(
            IntegrationOperation.EmailSend, new EmailSendResult.Sent(), ExternalCallTimeouts.Job, handler, harness);

        Assert.Equal(new EmailSendResult.ChannelError(ExternalCallFailure.TimeoutReason), result);
        Assert.Equal(1, handler.Requests);
    }

    [Fact]
    public async Task RunAsync_RefusedConnection_IsTheFamilysConnectionClass_WithTheTransportErrorName_AndOneRequest()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Refuse };
        using HttpClient client = new(handler);

        PaymentStartResult start = await harness.Runner.RunAsync(
            IntegrationOperation.PaymentStart, Send<PaymentStartResult>(client, new PaymentStartResult.Started(new Uri("https://service.invalid/frame"))), Ct);
        RefundRequestResult refund = await harness.Runner.RunAsync(IntegrationOperation.RefundRequest, Send<RefundRequestResult>(client, new RefundRequestResult.Accepted()), Ct);

        string reason = ExternalCallRunner.ConnectionReason(HttpRequestError.ConnectionError);
        Assert.Equal(new PaymentStartResult.ConnectionFailed(reason), start);
        Assert.Equal(new RefundRequestResult.Unknown(reason), refund);
        Assert.Equal(2, handler.Requests);
    }

    [Fact]
    public async Task RunAsync_AnsweredCall_ReturnsTheAnswer_AndWritesNoLine()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Answer, Status = HttpStatusCode.ServiceUnavailable };
        using HttpClient client = new(handler);

        EmailSendResult result = await harness.Runner.RunAsync(IntegrationOperation.EmailSend, Send<EmailSendResult>(client, new EmailSendResult.Sent()), Ct);

        Assert.Equal(new EmailSendResult.Sent(), result);
        Assert.Equal(1, handler.Requests);
        Assert.Empty(harness.Sink.Events);
    }

    [Fact]
    public async Task RunAsync_CallerCancels_IsNotATimeout_TheCancellationPassesUp_AndNoLineIsWritten()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Hang };
        using HttpClient client = new(handler);
        using var caller = new CancellationTokenSource();

        Task<EmailSendResult> call = harness.Runner.RunAsync(IntegrationOperation.EmailSend, Send<EmailSendResult>(client, new EmailSendResult.Sent()), caller.Token);
        await caller.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => call);
        Assert.Empty(harness.Sink.Events);
    }

    [Fact]
    public void Failed_AnAnswerNotInTheTable_IsTheFamilysClass_AndWritesItsReason()
    {
        using RunnerHarness harness = new();

        PaymentStartResult start = harness.Runner.Failed<PaymentStartResult>(IntegrationOperation.PaymentStart, ExternalCallFailure.Unrecognized("http:418"));
        EmailSendResult email = harness.Runner.Failed<EmailSendResult>(IntegrationOperation.EmailSend, ExternalCallFailure.Unrecognized("UnknownServiceError"));
        GoogleIdentityResult identity = harness.Runner.Failed<GoogleIdentityResult>(IntegrationOperation.IdentityVerify, ExternalCallFailure.Unrecognized("http:302"));

        Assert.Equal(new PaymentStartResult.ConnectionFailed("http:418"), start);
        Assert.Equal(new EmailSendResult.ChannelError("UnknownServiceError"), email);
        Assert.Equal(new GoogleIdentityResult.Failed("http:302"), identity);
        Assert.Equal(3, harness.Lines.Count);
        Assert.Contains("http:418", harness.Lines[0], StringComparison.Ordinal);
    }

    [Fact]
    public async Task FailureLine_CarriesTheEventTheJobTheKindAndTheReason_AndNoRequestFieldKeyOrUnreadAnswerField()
    {
        using RunnerHarness harness = new();
        ScriptedHandler handler = new() { Mode = ScriptedHandler.Behavior.Refuse };
        using HttpClient client = new(handler);

        await harness.Runner.RunAsync(IntegrationOperation.PaymentStart, Send<PaymentStartResult>(client, new PaymentStartResult.Started(new Uri("https://service.invalid/frame"))), Ct);

        // An answer whose status is not in the table and whose body holds a field the mapping does not read: only the status reaches the line.
        ScriptedHandler answering = new() { Mode = ScriptedHandler.Behavior.Answer, Body = "status=odd&masked_pan=" + UnreadFieldMarker };
        using HttpClient answeringClient = new(answering);
        PaymentQueryResult query = await harness.Runner.RunAsync(
            IntegrationOperation.PaymentQuery,
            async token =>
            {
                using HttpRequestMessage request = Request();
                using HttpResponseMessage response = await answeringClient.SendAsync(request, token);
                string body = await response.Content.ReadAsStringAsync(token);
                string status = body.Split('&')[0].Split('=')[1];
                return harness.Runner.Failed<PaymentQueryResult>(IntegrationOperation.PaymentQuery, ExternalCallFailure.Unrecognized("status:" + status));
            },
            Ct);

        Assert.Equal(new PaymentQueryResult.NoResponse("status:odd"), query);
        Assert.Equal(2, harness.Lines.Count);
        foreach (string line in harness.Lines)
        {
            Assert.DoesNotContain(RequestMarker, line, StringComparison.Ordinal);
            Assert.DoesNotContain(KeyMarker, line, StringComparison.Ordinal);
            Assert.DoesNotContain(UnreadFieldMarker, line, StringComparison.Ordinal);
            Assert.DoesNotContain("service.invalid", line, StringComparison.Ordinal);
            Assert.DoesNotContain("refused", line, StringComparison.Ordinal);
        }

        using JsonDocument first = JsonDocument.Parse(harness.Lines[0]);
        JsonElement json = first.RootElement;
        Assert.Equal(LogEvents.ExternalCallFailed, json.GetProperty("event").GetString());
        Assert.Equal("Warning", json.GetProperty("level").GetString());
        Assert.Equal(nameof(IntegrationOperation.PaymentStart), json.GetProperty("operation").GetString());
        Assert.Equal(nameof(ExternalCallFailureKind.Unreachable), json.GetProperty("failureKind").GetString());
        Assert.Equal(ExternalCallRunner.ConnectionReason(HttpRequestError.ConnectionError), json.GetProperty("reason").GetString());
        Assert.False(json.TryGetProperty("exceptionType", out _));
        Assert.False(json.TryGetProperty("stackTrace", out _));
    }
}
