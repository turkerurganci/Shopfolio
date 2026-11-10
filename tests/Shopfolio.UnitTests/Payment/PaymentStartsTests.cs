using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Status;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Payment;

/// <summary>
/// The start's outcome in the core (T404 KK2, KK3, KK4, KK7; 08 2.1.8; 05 9.4.18): on a fake clock, a connection error keeps the card unavailable for two minutes -
/// the open methods leave the card out and the step is told - and a successful token or the end of the two minutes takes it back; the confirmed order's start
/// still asks for its token; the token requests' error series of the status endpoint counts the connection errors, a token ends it, the second token refusal
/// neither counts nor ends it; each class of the family has its own case (09 11.1 item 8).
/// </summary>
public sealed class PaymentStartsTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 10, 9, 0, 0, TimeSpan.Zero);

    private static readonly PaymentStartResult Started = new PaymentStartResult.Started(new Uri("https://payment.invalid/frame"));
    private static readonly PaymentStartResult Failed = new PaymentStartResult.ConnectionFailed("timeout");
    private static readonly PaymentStartResult Refused = new PaymentStartResult.SecondTokenRejected(PayTrPaymentGateway.SecondTokenReason);

    private readonly ManualTimeProvider _time = new(Now);
    private readonly FakePaymentGateway _gateway = new();
    private readonly PaymentStartFailureSeries _series;
    private readonly CardUnavailability _unavailability;
    private readonly PaymentStarts _starts;
    private readonly StatusThresholdOptions _thresholds = new();

    public PaymentStartsTests()
    {
        _series = new PaymentStartFailureSeries(_time);
        _unavailability = new CardUnavailability(_time);
        _starts = new PaymentStarts(_series, _unavailability, _gateway);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static PaymentStartRequest Request() =>
        new("2026-7K4M9P", 34.56m, "musteri@example.com", "firma@example.com", "Ayşe Yılmaz", "Bağdat Cad. 1", "5321234567", "2121234567", "203.0.113.7", Now.AddMinutes(30), "ak_x");

    private async Task<PaymentStartOutcome> StartAsync(PaymentStartResult answer)
    {
        _gateway.EnqueueStart(answer);
        return await _starts.StartAsync(Request(), Ct);
    }

    private bool SeriesOpen() => StatusRules.PaymentStartSeriesOpen(_series.Snapshot().Count, _series.Snapshot().FirstAt, _time.GetUtcNow(), _thresholds);

    private PaymentMethods Methods() => new(new FixedCard(true), new FixedTransfer(true), _unavailability);

    // ---- each class has its own case ----

    [Fact]
    public async Task Started_OpensTheForm_WithItsAddress_AndLeavesTheCardAvailable_T404()
    {
        PaymentStartOutcome outcome = await StartAsync(Started);

        Assert.Equal(new PaymentStartOutcome(PaymentStartStep.OpenPaymentForm, new Uri("https://payment.invalid/frame")), outcome);
        Assert.False(_unavailability.IsUnavailable);
    }

    [Fact]
    public async Task ConnectionFailed_CancelsAsUnavailable_WithTheReasonL7DoesNotCount_T404_KK3()
    {
        PaymentStartOutcome outcome = await StartAsync(Failed);

        Assert.Equal(new PaymentStartOutcome(PaymentStartStep.CancelAsUnavailable, null), outcome);
        Assert.Equal(TransitionReason.PaymentPageNotOpened, PaymentStartRule.UnopenedCancellationReason);
        Assert.DoesNotContain(PaymentStartRule.UnopenedCancellationReason, UnpaidCancellationLimit.CountedReasons);
        Assert.Contains(TransitionReason.CardPaymentFailed, UnpaidCancellationLimit.CountedReasons);
    }

    [Fact]
    public async Task SecondTokenRejected_StaysOnTheConfirmation_AndIsNotAConnectionError_T404_KK2()
    {
        PaymentStartOutcome outcome = await StartAsync(Refused);

        Assert.Equal(new PaymentStartOutcome(PaymentStartStep.StayOnConfirmation, null), outcome);
        Assert.False(_unavailability.IsUnavailable);
        Assert.Equal((0, (DateTimeOffset?)null), _series.Snapshot());
    }

    // ---- KK4: two minutes on the fake clock; only before the confirmation ----

    [Fact]
    public async Task AConnectionError_KeepsTheCardUnavailableForTwoMinutes_AndThenItIsBack_T404_KK4()
    {
        PaymentMethods methods = Methods();
        await StartAsync(Failed);

        Assert.Equal(TimeSpan.FromMinutes(2), CardUnavailability.Duration);
        Assert.True(await methods.IsCardUnavailableNowAsync(Ct));
        Assert.Equal([PaymentMethod.BankTransfer], await methods.ListOpenAsync(Ct));

        _time.Advance(CardUnavailability.Duration - TimeSpan.FromSeconds(1));
        Assert.True(await methods.IsCardUnavailableNowAsync(Ct));

        _time.Advance(TimeSpan.FromSeconds(1));
        Assert.False(await methods.IsCardUnavailableNowAsync(Ct));
        Assert.Equal([PaymentMethod.Card, PaymentMethod.BankTransfer], await methods.ListOpenAsync(Ct));
    }

    [Fact]
    public async Task WhileTheCardIsUnavailable_AConfirmedOrdersStart_StillAsksForItsToken_T404_KK4()
    {
        await StartAsync(Failed);
        Assert.True(_unavailability.IsUnavailable);

        PaymentStartOutcome outcome = await StartAsync(Started);

        Assert.Equal(PaymentStartStep.OpenPaymentForm, outcome.Step);
        Assert.Equal(2, _gateway.Calls.Count(c => c.Job == nameof(IPaymentGateway.StartAsync)));
    }

    [Fact]
    public void TheState_IsTheProcesssMemory_ANewProcessStartsWithout_T404_KK4()
    {
        new CardUnavailability(_time).Mark();

        Assert.False(new CardUnavailability(_time).IsUnavailable);
    }

    [Fact]
    public async Task AClosedCardLine_IsNotShownAsUnavailable_ItIsNotOffered_T404_KK4()
    {
        PaymentMethods closed = new(new FixedCard(false), new FixedTransfer(true), _unavailability);
        await StartAsync(Failed);

        Assert.False(await closed.IsCardUnavailableNowAsync(Ct));
        Assert.Equal([PaymentMethod.BankTransfer], await closed.ListOpenAsync(Ct));
    }

    // ---- KK7: the token requests' error series ----

    [Fact]
    public async Task FourErrors_DoNotOpenTheCondition_EvenAfterFifteenMinutes_T404_KK7()
    {
        for (int i = 0; i < _thresholds.PaymentStartFailureCount - 1; i++)
        {
            await StartAsync(Failed);
        }

        _time.Advance(TimeSpan.FromMinutes(_thresholds.PaymentStartFailureAgeMinutes));

        Assert.Equal(4, _series.Snapshot().Count);
        Assert.False(SeriesOpen());
    }

    [Fact]
    public async Task FiveErrors_OpenTheCondition_OnlyWhenTheFirstIsFifteenMinutesOld_T404_KK7()
    {
        await StartAsync(Failed);
        _time.Advance(TimeSpan.FromMinutes(10));
        for (int i = 1; i < _thresholds.PaymentStartFailureCount; i++)
        {
            await StartAsync(Failed);
        }

        _time.Advance(TimeSpan.FromMinutes(5) - TimeSpan.FromSeconds(1));
        Assert.False(SeriesOpen());

        _time.Advance(TimeSpan.FromSeconds(1));
        Assert.Equal((5, (DateTimeOffset?)Now), _series.Snapshot());
        Assert.True(SeriesOpen());
    }

    [Fact]
    public async Task ASuccessfulToken_EndsTheSeries_T404_KK7()
    {
        for (int i = 0; i < _thresholds.PaymentStartFailureCount; i++)
        {
            await StartAsync(Failed);
        }

        _time.Advance(TimeSpan.FromMinutes(15));
        Assert.True(SeriesOpen());

        await StartAsync(Started);

        Assert.Equal((0, (DateTimeOffset?)null), _series.Snapshot());
        Assert.False(SeriesOpen());
    }

    [Fact]
    public async Task TheSecondTokenRefusal_NeitherCountsNorBreaksTheSeries_T404_KK7()
    {
        for (int i = 0; i < _thresholds.PaymentStartFailureCount - 1; i++)
        {
            await StartAsync(Failed);
            await StartAsync(Refused);
        }

        _time.Advance(TimeSpan.FromMinutes(15));
        Assert.Equal(4, _series.Snapshot().Count);
        Assert.False(SeriesOpen());

        await StartAsync(Failed);
        Assert.Equal((5, (DateTimeOffset?)Now), _series.Snapshot());
        Assert.True(SeriesOpen());
    }

    private sealed class FixedCard(bool open) : ICardPaymentCondition
    {
        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(open);
    }

    private sealed class FixedTransfer(bool open) : IBankTransferCondition
    {
        public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(open);
    }
}
