using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Payment;

namespace Shopfolio.UnitTests.Payment;

/// <summary>
/// The core's rule over the final query's answer (T403 KK2, KK4, KK5; 08 2.1.6; 05 4.3.4): the two kinds of unanswered call have two intervals - the five-minute
/// reschedule and the job's code error rule -, "no payment" is its own step, and a successful answer is applied only when its five conditions hold; each broken
/// condition names its reason, and a test transaction counts only on the live installation.
/// </summary>
public sealed class FinalPaymentQueryRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    private static PaymentQueryResult.Paid Paid(decimal amount = 120m, decimal total = 124.50m, bool card = true, bool lira = true, bool test = false) =>
        new("20267K4M9P", amount, total, card, lira, test);

    // ---- KK2 ----

    [Fact]
    public void NoPayment_IsItsOwnStep_TheUnpaidOrdersCancellation_KK2()
    {
        Assert.Equal(FinalPaymentQueryStep.CancelUnpaidOrder, FinalPaymentQueryRule.Decide(new PaymentQueryResult.NoPayment()));
    }

    // ---- KK4: the two intervals ----

    [Theory]
    [InlineData("timeout")]
    [InlineData("connection:ConnectionError")]
    [InlineData("body")]
    [InlineData("http:500")]
    public void AnUnansweredCallThatWaitingMayChange_IsAskedAgainAfterFiveMinutes_ItsAttemptsUnchanged_KK4(string reason)
    {
        Assert.Equal(FinalPaymentQueryStep.AskAgain, FinalPaymentQueryRule.Decide(new PaymentQueryResult.NoResponse(reason)));
        Assert.Equal(JobOutcome.RescheduleAfter(JobTimings.FinalPaymentQueryInterval), FinalPaymentQueryRule.AskAgainOutcome());
    }

    [Theory]
    [InlineData("err:001")]
    [InlineData("err:002")]
    [InlineData("err:003")]
    [InlineData("amount-text")]
    [InlineData("keys-missing")]
    public void AnUnansweredCallThatWaitingDoesNotChange_IsRepeatedByTheCodeErrorRule_KK4(string reason)
    {
        Assert.Equal(FinalPaymentQueryStep.RetryByCodeErrorRule, FinalPaymentQueryRule.Decide(new PaymentQueryResult.NoResponse(reason, CodeError: true)));

        // The handler throws; the runner's default rule for the type: 1, 5, 15, 60 and 240 minutes, then failed (05 4.3.4) - not the five-minute reschedule.
        InvalidOperationException failure = FinalPaymentQueryRule.CodeError(reason);
        Assert.EndsWith(reason, failure.Message, StringComparison.Ordinal);
        IReadOnlyList<TimeSpan> schedule = JobTimings.DefaultRetrySchedule;
        for (int attempt = 0; attempt < schedule.Count; attempt++)
        {
            Assert.Equal(
                new JobFailureDecision.RetryAt(Now + schedule[attempt], attempt + 1),
                JobRetryPolicy.AfterFailure(JobType.FinalPaymentQuery, attempt, Now));
        }

        Assert.IsType<JobFailureDecision.GiveUp>(JobRetryPolicy.AfterFailure(JobType.FinalPaymentQuery, schedule.Count, Now));
    }

    // ---- KK5: the five conditions ----

    [Fact]
    public void ASuccessWhoseFiveConditionsHold_IsApplied_TheInstalmentDifferenceIncluded_KK5()
    {
        Assert.Equal(FinalPaymentQueryStep.ApplyPayment, FinalPaymentQueryRule.Decide(Paid()));
        Assert.Null(FinalPaymentQueryRule.NotAppliedReason(Paid(), 120m, isLiveInstallation: true));
        Assert.Null(FinalPaymentQueryRule.NotAppliedReason(Paid(total: 120m), 120m, isLiveInstallation: true));
    }

    // payment_amount short / more, payment_total short, odeme_tipi EFT, the currency not lira, test_mode 1 on the live installation.
    [Theory]
    [InlineData(11999, 12450, true, true, false, FinalPaymentQueryRule.AmountReason)]
    [InlineData(12001, 12450, true, true, false, FinalPaymentQueryRule.AmountReason)]
    [InlineData(12000, 11999, true, true, false, FinalPaymentQueryRule.AmountReason)]
    [InlineData(12000, 12450, false, true, false, FinalPaymentQueryRule.PaymentTypeReason)]
    [InlineData(12000, 12450, true, false, false, FinalPaymentQueryRule.CurrencyReason)]
    [InlineData(12000, 12450, true, true, true, FinalPaymentQueryRule.TestModeReason)]
    public void ASuccessWithOneBrokenCondition_IsNotApplied_AndNamesItsReason_KK5(long amountKurus, long totalKurus, bool card, bool lira, bool test, string reason)
    {
        PaymentQueryResult.Paid paid = Paid(PaymentAmounts.FromKurus(amountKurus), PaymentAmounts.FromKurus(totalKurus), card, lira, test);
        Assert.Equal(reason, FinalPaymentQueryRule.NotAppliedReason(paid, 120m, isLiveInstallation: true));
    }

    [Fact]
    public void ATestTransaction_IsAppliedOutsideTheLiveInstallation_KK5()
    {
        Assert.Null(FinalPaymentQueryRule.NotAppliedReason(Paid(test: true), 120m, isLiveInstallation: false));
    }
}
