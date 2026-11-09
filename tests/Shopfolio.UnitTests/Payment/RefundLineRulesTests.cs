using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.UnitTests.Payment;

/// <summary>
/// The refund line's rules (T370; 02 7.2.8, 7.4.1, 7.4.5; K-214, K-569, K-940): the way follows the payment, the system starts the card refund of a cancellation
/// without approval and asks for no IBAN, the period starts where 7.4.1 says, a failed card refund opens the retry and the transfer path until an IBAN is entered,
/// and a pending result closes both.
/// </summary>
public sealed class RefundLineRulesTests
{
    private static readonly Guid OrderId = Guid.NewGuid();

    [Theory]
    [InlineData(PaymentMethod.Card, PaymentMethod.Card)]
    [InlineData(PaymentMethod.BankTransfer, PaymentMethod.BankTransfer)]
    public void TheRefundGoesTheWayThePaymentCame_KK1(PaymentMethod payment, PaymentMethod way)
    {
        Assert.Equal(way, RefundLineRules.WayOf(payment));
    }

    [Theory]
    [InlineData(RefundType.Cancellation)]
    [InlineData(RefundType.ItemRemoval)]
    [InlineData(RefundType.LateDeliveryTermination)]
    [InlineData(RefundType.LatePayment)]
    public void OnTheCard_TheSystemStartsTheCancellationsRefund_AndAsksForNoIban_KK2(RefundType type)
    {
        Assert.True(RefundLineRules.IsStartedBySystem(type, PaymentMethod.Card));
        Assert.False(RefundLineRules.NeedsIbanField(PaymentMethod.Card, false));
    }

    [Theory]
    [InlineData(RefundType.Withdrawal, PaymentMethod.Card)]
    [InlineData(RefundType.DefectClaimRescission, PaymentMethod.Card)]
    [InlineData(RefundType.AmountBased, PaymentMethod.Card)]
    [InlineData(RefundType.Cancellation, PaymentMethod.BankTransfer)]
    [InlineData(RefundType.LateDeliveryTermination, PaymentMethod.BankTransfer)]
    public void TheWithdrawal_AndTheTransferLine_AreTheFirmsStepOnThePanel_KK2(RefundType type, PaymentMethod way)
    {
        Assert.False(RefundLineRules.IsStartedBySystem(type, way));
    }

    [Fact]
    public void TheTransferLine_AsksForAnIban_OnlyWhenNoneCameWithTheRecord_KK6()
    {
        Assert.True(RefundLineRules.NeedsIbanField(PaymentMethod.BankTransfer, false));
        Assert.False(RefundLineRules.NeedsIbanField(PaymentMethod.BankTransfer, true));
    }

    [Fact]
    public void ThePeriod_StartsAtTheCancellation_AtTheDeclarationBeforeTheDelivery_AndAtTheLaterOfArrivalAndDeclarationAfter_KK3()
    {
        DateOnly declared = new(2026, 11, 9);
        DateTimeOffset cancelledAt = TurkeyTime.StartOf(declared).AddHours(10);

        // The cancellation: from the cancellation's moment (the transfer line shows it; the card refund started at once).
        Assert.Equal(AfterSalesPeriodRules.RefundStanding(declared, cancelledAt), AfterSalesPeriodRules.CancellationRefundStanding(PaymentMethod.BankTransfer, cancelledAt, cancelledAt));

        // Before the delivery and on the service: the declaration.
        Assert.Equal(declared, AfterSalesPeriodRules.WithdrawalRefundStartDay(false, declared, null));

        // After the delivery: the later of the arrival and the declaration - goods that arrived before the declaration count from the declaration.
        Assert.Equal(declared.AddDays(3), AfterSalesPeriodRules.WithdrawalRefundStartDay(true, declared, declared.AddDays(3)));
        Assert.Equal(declared, AfterSalesPeriodRules.WithdrawalRefundStartDay(true, declared, declared.AddDays(-2)));
        Assert.Null(AfterSalesPeriodRules.WithdrawalRefundStartDay(true, declared, null));
    }

    [Fact]
    public void AFailedCardRefund_OpensTheRetryAndTheTransferPath_UntilAnIbanIsEntered_KK8()
    {
        CardRefundLine failed = Line(RefundRecordState.Failed);

        Assert.Equal(new CardRefundActions(true, true, false), RefundLineRules.ActionsOf(failed, false));

        // The customer's IBAN closes the retry: the same money never goes two ways; it turns the way to the transfer.
        Assert.Equal(new CardRefundActions(false, false, true), RefundLineRules.ActionsOf(failed, true));
    }

    [Fact]
    public void APendingResult_ClosesTheRetryAndTheTransferPath_KK9()
    {
        Assert.Equal(default, RefundLineRules.ActionsOf(Line(RefundRecordState.OutcomePending), false));
        Assert.Equal(default, RefundLineRules.ActionsOf(Line(RefundRecordState.OutcomePending), true));
        Assert.Equal(default, RefundLineRules.ActionsOf(Line(RefundRecordState.Sending), false));
    }

    [Fact]
    public void ASentOrVoidedRefund_OrOneOnTheTransferWay_OpensNothing()
    {
        Assert.Equal(default, RefundLineRules.ActionsOf(Line(RefundRecordState.Failed) with { IsSent = true }, false));
        Assert.Equal(default, RefundLineRules.ActionsOf(Line(RefundRecordState.Failed) with { IsVoided = true }, false));
        Assert.Equal(default, RefundLineRules.ActionsOf(Line(RefundRecordState.Failed) with { Way = PaymentMethod.BankTransfer }, false));
        Assert.Equal(default, RefundLineRules.ActionsOf(Line(null), false));
    }

    private static CardRefundLine Line(RefundRecordState? latest) => new(Guid.NewGuid(), OrderId, Guid.NewGuid(), PaymentMethod.Card, false, false, latest);
}
