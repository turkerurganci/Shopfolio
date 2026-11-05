using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Payment;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Outages;

/// <summary>
/// The core's rules over an outside service's failed answer (T204 criteria 2, 3, 6 and 7, KK2, KK3, KK6, KK7; 08 2.1.8, 2.3 error table, 3.1
/// rule 4, 3.3): every class of each family has its own case (09 11.1 item 8), and every failure kind the fakes produce lands on the class the
/// criterion names - the 15 second limit of the start included (its limit on the fake clock is <c>ExternalCallRunnerTests</c>'s).
/// </summary>
public sealed class OutageRulesTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public static TheoryData<ExternalCallFailureKind> Kinds() => [.. FakeFailures.AllKinds];

    // ---- KK2: the start's two failure classes are apart; a timed-out start is a connection error ----

    [Fact]
    public void PaymentStart_EachClass_HasItsOwnStep()
    {
        Assert.Equal(PaymentStartStep.OpenPaymentForm, PaymentStartRule.Decide(new PaymentStartResult.Started(new Uri("https://payment.invalid/frame"))));
        Assert.Equal(PaymentStartStep.CancelAsUnavailable, PaymentStartRule.Decide(new PaymentStartResult.ConnectionFailed("connection:ConnectionError")));
        Assert.Equal(PaymentStartStep.StayOnConfirmation, PaymentStartRule.Decide(new PaymentStartResult.SecondTokenRejected("token-already-issued")));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task PaymentStart_EveryFailureKindOfTheFake_IsAConnectionError_NotASecondTokenRefusal(ExternalCallFailureKind kind)
    {
        FakePaymentGateway fake = new() { Outage = kind };

        PaymentStartResult answer = await fake.StartAsync(StartRequest(), Ct);

        Assert.IsType<PaymentStartResult.ConnectionFailed>(answer);
        Assert.Equal(PaymentStartStep.CancelAsUnavailable, PaymentStartRule.Decide(answer));
    }

    [Fact]
    public async Task PaymentStart_Timeout_IsAConnectionError_WithTheTimeoutReason()
    {
        FakePaymentGateway fake = new() { Outage = ExternalCallFailureKind.TimedOut };

        PaymentStartResult answer = await fake.StartAsync(StartRequest(), Ct);

        Assert.Equal(new PaymentStartResult.ConnectionFailed(ExternalCallFailure.TimeoutReason), answer);
    }

    // ---- KK3, KK7: an unanswered final query leaves the order and is asked again after the queue's interval ----

    [Fact]
    public void FinalQuery_EachClass_HasItsOwnStep()
    {
        Assert.Equal(FinalPaymentQueryStep.ApplyPayment, FinalPaymentQueryRule.Decide(new PaymentQueryResult.Paid(120m, 120m, true, true, false)));
        Assert.Equal(FinalPaymentQueryStep.CancelUnpaidOrder, FinalPaymentQueryRule.Decide(new PaymentQueryResult.NoPayment()));
        Assert.Equal(FinalPaymentQueryStep.AskAgain, FinalPaymentQueryRule.Decide(new PaymentQueryResult.NoResponse("timeout")));
    }

    [Theory]
    [MemberData(nameof(Kinds))]
    public async Task FinalQuery_EveryFailureKindOfTheFake_IsAskedAgain_NeverCancels(ExternalCallFailureKind kind)
    {
        FakePaymentGateway fake = new() { Outage = kind };

        FinalPaymentQueryStep step = FinalPaymentQueryRule.Decide(await fake.QueryPaymentAsync("2026-000001", Ct));

        Assert.Equal(FinalPaymentQueryStep.AskAgain, step);
    }

    [Fact]
    public void FinalQuery_AskAgain_IsTheQueuesFiveMinuteRescheduling()
    {
        Assert.Equal(JobOutcome.RescheduleAfter(JobTimings.FinalPaymentQueryInterval), FinalPaymentQueryRule.AskAgainOutcome());
    }

    // ---- KK6: Google unavailable - sign-in SocialLoginFailed, re-authentication SocialReauthenticationUnavailable; no retry ----

    [Fact]
    public void Google_EachClassInEachFlow_HasItsCode()
    {
        GoogleIdentityResult success = new GoogleIdentityResult.Succeeded("ali@example.com", true, "Ali", null, null, "nonce");
        GoogleIdentityResult unavailable = new GoogleIdentityResult.Unavailable("timeout");
        GoogleIdentityResult failed = new GoogleIdentityResult.Failed("unrecognized");

        Assert.Null(GoogleIdentityFailureRule.FailureOf(GoogleIdentityFlow.SignInOrRegistration, success));
        Assert.Null(GoogleIdentityFailureRule.FailureOf(GoogleIdentityFlow.Reauthentication, success));
        Assert.Equal(GoogleIdentityFailure.SocialLoginFailed, GoogleIdentityFailureRule.FailureOf(GoogleIdentityFlow.SignInOrRegistration, unavailable));
        Assert.Equal(GoogleIdentityFailure.SocialReauthenticationUnavailable, GoogleIdentityFailureRule.FailureOf(GoogleIdentityFlow.Reauthentication, unavailable));
        Assert.Equal(GoogleIdentityFailure.SocialLoginFailed, GoogleIdentityFailureRule.FailureOf(GoogleIdentityFlow.SignInOrRegistration, failed));
        Assert.Equal(GoogleIdentityFailure.SocialReauthenticationFailed, GoogleIdentityFailureRule.FailureOf(GoogleIdentityFlow.Reauthentication, failed));
    }

    [Theory]
    [InlineData(ExternalCallFailureKind.TimedOut)]
    [InlineData(ExternalCallFailureKind.Unreachable)]
    public async Task Google_Unanswered_SignInFails_ReauthenticationIsUnavailable_WithOneCallEach(ExternalCallFailureKind kind)
    {
        FakeGoogleIdentityGateway fake = new() { Outage = kind };
        IdentityCodeExchange exchange = new("code", "verifier");

        GoogleIdentityFailure? signIn = GoogleIdentityFailureRule.FailureOf(GoogleIdentityFlow.SignInOrRegistration, await fake.VerifyIdentityAsync(exchange, Ct));
        GoogleIdentityFailure? reauthentication = GoogleIdentityFailureRule.FailureOf(GoogleIdentityFlow.Reauthentication, await fake.VerifyIdentityAsync(exchange, Ct));

        Assert.Equal(GoogleIdentityFailure.SocialLoginFailed, signIn);
        Assert.Equal(GoogleIdentityFailure.SocialReauthenticationUnavailable, reauthentication);
        Assert.Equal(2, fake.Calls.Count);
    }

    private static PaymentStartRequest StartRequest() => new(
        "2026-000001", 120m, "ali@example.com", "firma@example.com", "Ali Veli", "Atatürk Bulvarı 1", null, "3121234567", "203.0.113.5",
        new DateTimeOffset(2026, 11, 5, 10, 0, 0, TimeSpan.Zero), "access-placeholder-1");
}
