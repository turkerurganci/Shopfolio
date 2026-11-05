using Shopfolio.Infrastructure.Integrations;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The hand-written fakes of the three gateways (T202 criterion 9, KK9; 09 19.1 items 3 and 6): each answers the queued results in order and
/// then its default, keeps the calls in the order they were made with their arguments, and honours a cancelled token. They never leave the
/// process (the source scan's rule 5 keeps network types out of <c>Fakes/</c>). Each also produces the three ways a call ends without an answer -
/// the timeout, the broken connection and the unlisted answer - one call at a time or as a lasting outage (T204 criterion 1, KK1).
/// </summary>
public sealed class GatewayFakesTests
{
    private static readonly CardRefund Refund = new("SF-0001", 125.50m, Guid.Parse("7b0b3c55-5d59-4d4b-9d8b-1f0f6f0d2a11"));

    private static readonly PaymentStartRequest StartRequest = new(
        "SF-0001", 125.50m, "musteri@example.com", "firma@example.com", "Ali Veli", "Adres 1", null, "3121234567", "203.0.113.5",
        DateTimeOffset.UnixEpoch, "access-placeholder");

    private static EmailMessage Message(string recipient) => new(recipient, "Örnek Mağaza", null, "Konu", "<p>x</p>", "x");

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task PaymentFake_AnswersTheQueueInOrderThenTheDefault_AndKeepsEveryCall()
    {
        FakePaymentGateway fake = new();
        fake.EnqueueRefund(new RefundRequestResult.Unknown("timeout"), new RefundRequestResult.NotCompleted("reason"));

        RefundRequestResult first = await fake.RequestRefundAsync(Refund, Ct);
        RefundRequestResult second = await fake.RequestRefundAsync(Refund, Ct);
        RefundRequestResult third = await fake.RequestRefundAsync(Refund, Ct);
        RefundQueryResult query = await fake.QueryRefundAsync(Refund, Ct);

        Assert.Equal(new RefundRequestResult.Unknown("timeout"), first);
        Assert.Equal(new RefundRequestResult.NotCompleted("reason"), second);
        Assert.Equal(fake.DefaultRefund, third);
        Assert.Equal(fake.DefaultRefundQuery, query);
        Assert.Equal(
            [
                new FakeCall(nameof(IPaymentGateway.RequestRefundAsync), Refund),
                new FakeCall(nameof(IPaymentGateway.RequestRefundAsync), Refund),
                new FakeCall(nameof(IPaymentGateway.RequestRefundAsync), Refund),
                new FakeCall(nameof(IPaymentGateway.QueryRefundAsync), Refund),
            ],
            fake.Calls);
    }

    [Fact]
    public async Task PaymentFake_EachJobHasItsOwnQueue()
    {
        FakePaymentGateway fake = new();
        fake.EnqueueStart(new PaymentStartResult.ConnectionFailed("timeout"));
        fake.EnqueueQuery(new PaymentQueryResult.NoResponse("timeout"));

        PaymentQueryResult query = await fake.QueryPaymentAsync("SF-0001", Ct);
        NotificationVerification verification = fake.VerifyNotification(new Dictionary<string, string>());

        Assert.Equal(new PaymentQueryResult.NoResponse("timeout"), query);
        Assert.Equal(fake.DefaultVerification, verification);
        Assert.Equal([nameof(IPaymentGateway.QueryPaymentAsync), nameof(IPaymentGateway.VerifyNotification)], fake.Calls.Select(c => c.Job));
    }

    [Fact]
    public async Task EmailFake_KeepsTheMessagesInOrder_WhateverResultEachGot()
    {
        FakeEmailGateway fake = new();
        fake.EnqueueSend(new EmailSendResult.RecipientError("bounce"));

        EmailSendResult first = await fake.SendAsync(Message("a@example.com"), Ct);
        EmailSendResult second = await fake.SendAsync(Message("b@example.com"), Ct);
        BounceQueryResult bounces = await fake.QueryBouncesAsync(DateTimeOffset.UnixEpoch, Ct);

        Assert.Equal(new EmailSendResult.RecipientError("bounce"), first);
        Assert.Equal(fake.DefaultSend, second);
        Assert.Same(fake.DefaultBounceQuery, bounces);
        Assert.Equal(["a@example.com", "b@example.com"], fake.Messages.Select(m => m.RecipientEmail));
        Assert.Equal(3, fake.Calls.Count);
    }

    [Fact]
    public async Task GoogleFake_AnswersTheQueueThenTheDefault()
    {
        FakeGoogleIdentityGateway fake = new();
        fake.Enqueue(new GoogleIdentityResult.Unavailable("timeout"));
        IdentityCodeExchange exchange = new("code", "verifier");

        GoogleIdentityResult first = await fake.VerifyIdentityAsync(exchange, Ct);
        GoogleIdentityResult second = await fake.VerifyIdentityAsync(exchange, Ct);

        Assert.Equal(new GoogleIdentityResult.Unavailable("timeout"), first);
        Assert.Equal(fake.DefaultResult, second);
        Assert.Equal([new FakeCall(nameof(IGoogleIdentityGateway.VerifyIdentityAsync), exchange), new FakeCall(nameof(IGoogleIdentityGateway.VerifyIdentityAsync), exchange)], fake.Calls);
    }

    // ---- T204 criterion 1, KK1: each fake produces the timeout, the broken connection and the unlisted answer ----

    public static TheoryData<ExternalCallFailureKind> Kinds() => [.. FakeFailures.AllKinds];

    [Fact]
    public void FakeFailures_HaveTheRunnersReasonCodes_ForTheThreeKinds()
    {
        Assert.Equal([ExternalCallFailureKind.TimedOut, ExternalCallFailureKind.Unreachable, ExternalCallFailureKind.Unrecognized], FakeFailures.AllKinds);
        Assert.Equal(ExternalCallFailure.TimedOut(), FakeFailures.Of(ExternalCallFailureKind.TimedOut));
        Assert.Equal(ExternalCallFailure.Unreachable(ExternalCallRunner.ConnectionReason(HttpRequestError.ConnectionError)), FakeFailures.Of(ExternalCallFailureKind.Unreachable));
        Assert.Equal(ExternalCallFailure.Unrecognized(FakeFailures.UnrecognizedReason), FakeFailures.Of(ExternalCallFailureKind.Unrecognized));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task PaymentFake_QueuedFailure_IsTheFamilysClassOfThatFailure(ExternalCallFailureKind kind)
    {
        FakePaymentGateway fake = new();
        fake.EnqueueStart(FakeFailures.ResultOf<PaymentStartResult>(kind));
        fake.EnqueueQuery(FakeFailures.ResultOf<PaymentQueryResult>(kind));
        fake.EnqueueRefund(FakeFailures.ResultOf<RefundRequestResult>(kind));
        fake.EnqueueRefundQuery(FakeFailures.ResultOf<RefundQueryResult>(kind));
        string reason = FakeFailures.Of(kind).ReasonCode;

        Assert.Equal(new PaymentStartResult.ConnectionFailed(reason), await fake.StartAsync(StartRequest, Ct));
        Assert.Equal(new PaymentQueryResult.NoResponse(reason), await fake.QueryPaymentAsync("SF-0001", Ct));
        Assert.Equal(new RefundRequestResult.Unknown(reason), await fake.RequestRefundAsync(Refund, Ct));
        Assert.Equal(new RefundQueryResult.NoResult(), await fake.QueryRefundAsync(Refund, Ct));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task EmailAndGoogleFakes_QueuedFailure_IsTheFamilysClassOfThatFailure(ExternalCallFailureKind kind)
    {
        FakeEmailGateway email = new();
        FakeGoogleIdentityGateway google = new();
        email.EnqueueSend(FakeFailures.ResultOf<EmailSendResult>(kind));
        email.EnqueueBounceQuery(FakeFailures.ResultOf<BounceQueryResult>(kind));
        google.Enqueue(FakeFailures.ResultOf<GoogleIdentityResult>(kind));
        string reason = FakeFailures.Of(kind).ReasonCode;
        GoogleIdentityResult expectedGoogle = kind == ExternalCallFailureKind.Unrecognized
            ? new GoogleIdentityResult.Failed(reason)
            : new GoogleIdentityResult.Unavailable(reason);

        Assert.Equal(new EmailSendResult.ChannelError(reason), await email.SendAsync(Message("a@example.com"), Ct));
        Assert.Equal(new BounceQueryResult.QueryFailed(reason), await email.QueryBouncesAsync(DateTimeOffset.UnixEpoch, Ct));
        Assert.Equal(expectedGoogle, await google.VerifyIdentityAsync(new("code", "verifier"), Ct));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task Outage_AnswersEveryCallWithTheFailure_KeepsTheCalls_AndLeavesTheQueues(ExternalCallFailureKind kind)
    {
        FakePaymentGateway payment = new() { Outage = kind };
        FakeEmailGateway email = new() { Outage = kind };
        FakeGoogleIdentityGateway google = new() { Outage = kind };
        payment.EnqueueQuery(new PaymentQueryResult.NoPayment());
        email.EnqueueSend(new EmailSendResult.Sent());
        google.Enqueue(new GoogleIdentityResult.Failed("queued"));

        for (int call = 0; call < 3; call++)
        {
            Assert.Equal(FakeFailures.ResultOf<PaymentQueryResult>(kind), await payment.QueryPaymentAsync("SF-0001", Ct));
            Assert.Equal(FakeFailures.ResultOf<PaymentStartResult>(kind), await payment.StartAsync(StartRequest, Ct));
            Assert.Equal(FakeFailures.ResultOf<EmailSendResult>(kind), await email.SendAsync(Message("a@example.com"), Ct));
            Assert.Equal(FakeFailures.ResultOf<GoogleIdentityResult>(kind), await google.VerifyIdentityAsync(new("code", "verifier"), Ct));
        }

        Assert.Equal(6, payment.Calls.Count);
        Assert.Equal(3, email.Messages.Count);
        Assert.Equal(3, google.Calls.Count);

        // The verification of a notification is a local computation and the outage does not touch it; the queues answer again once it ends.
        Assert.Equal(payment.DefaultVerification, payment.VerifyNotification(new Dictionary<string, string>()));
        payment.Outage = null;
        email.Outage = null;
        google.Outage = null;
        Assert.Equal(new PaymentQueryResult.NoPayment(), await payment.QueryPaymentAsync("SF-0001", Ct));
        Assert.Equal(new EmailSendResult.Sent(), await email.SendAsync(Message("a@example.com"), Ct));
        Assert.Equal(new GoogleIdentityResult.Failed("queued"), await google.VerifyIdentityAsync(new("code", "verifier"), Ct));
    }

    [Fact]
    public async Task Fakes_OnACancelledToken_Throw_AndKeepNoCall()
    {
        using CancellationTokenSource source = new();
        await source.CancelAsync();
        FakePaymentGateway payment = new();
        FakeEmailGateway email = new();
        FakeGoogleIdentityGateway google = new();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => payment.QueryRefundAsync(Refund, source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => email.SendAsync(Message("a@example.com"), source.Token));
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => google.VerifyIdentityAsync(new("code", "verifier"), source.Token));
        Assert.Empty(payment.Calls);
        Assert.Empty(email.Calls);
        Assert.Empty(google.Calls);
    }
}
