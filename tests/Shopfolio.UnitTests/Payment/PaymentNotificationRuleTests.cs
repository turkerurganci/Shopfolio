using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Payment;

/// <summary>
/// The classification of a verified payment notification (T311 criteria 1, 5; KK1, KK5; 08 2.1.5 "classification of the notification" rows 3-7;
/// 05 4.5.5): the first row that holds decides; the amount check is the link layer's and comes back as a class; a failed payment is not checked for its
/// amount. Each arm of row 4 has its own case. The endpoint's signature step asks the link layer and refuses without a gateway.
/// </summary>
public sealed class PaymentNotificationRuleTests
{
    private const decimal Total = 120m;

    private static readonly OrderForPayment CardOrder = new(Guid.Parse("5a7c0a52-3f1b-4f7e-9a51-0c9d1d1e2f30"), Total, PaymentMethod.Card);

    private static readonly FakePaymentGateway Gateway = new();

    private static PaymentNotice Notice(
        string outcome = FakePaymentGateway.SucceededOutcome,
        decimal? total = Total,
        decimal? started = Total,
        bool card = true,
        bool lira = true,
        bool test = false,
        string eventType = FakePaymentGateway.PaymentEventType) =>
        new("2026000001", "2026-000001", eventType, outcome, total, started, card, lira, test);

    [Fact]
    public void SuccessWhoseAmountHolds_IsProcessed_Row7()
    {
        Assert.Null(PaymentNotificationRule.Classify(Notice(), CardOrder, Gateway, isLiveInstallation: true));
    }

    [Fact]
    public void SuccessWithTheInstallmentDifference_Holds_KK4OfTheLayer()
    {
        Assert.Null(PaymentNotificationRule.Classify(Notice(total: Total + 7.35m), CardOrder, Gateway, isLiveInstallation: true));
    }

    [Theory]
    [InlineData(119.99, 120)]
    [InlineData(120, 119.99)]
    [InlineData(120, 120.01)]
    public void SuccessWhoseAmountDoesNotHold_IsMarkedAmountMismatch_Row6_KK5(double total, double started)
    {
        UnprocessedNotificationReason? reason = PaymentNotificationRule.Classify(
            Notice(total: (decimal)total, started: (decimal)started), CardOrder, Gateway, isLiveInstallation: true);

        Assert.Equal(UnprocessedNotificationReason.AmountMismatch, reason);
    }

    [Fact]
    public void Failure_IsProcessed_WithoutAnAmountCheck_Row5()
    {
        Assert.Null(PaymentNotificationRule.Classify(Notice(FakePaymentGateway.FailedOutcome, total: 0m, started: null), CardOrder, Gateway, isLiveInstallation: true));
    }

    [Fact]
    public void ReferenceThatMapsToNoOrder_IsMarkedOrderNotFound_BeforeEveryOtherRow_Row3()
    {
        Assert.Equal(
            UnprocessedNotificationReason.OrderNotFound,
            PaymentNotificationRule.Classify(Notice("unknown-outcome", total: 1m), null, Gateway, isLiveInstallation: true));
    }

    [Theory]
    [InlineData("unknown-outcome", FakePaymentGateway.PaymentEventType)]
    [InlineData(FakePaymentGateway.SucceededOutcome, "refund")]
    public void UnknownOutcomeOrEventType_IsMarkedUnrecognized_Row4(string outcome, string eventType)
    {
        Assert.Equal(
            UnprocessedNotificationReason.UnrecognizedNotification,
            PaymentNotificationRule.Classify(Notice(outcome, eventType: eventType), CardOrder, Gateway, isLiveInstallation: true));
    }

    [Fact]
    public void PaymentNotByCard_IsMarkedUnrecognized_Row4()
    {
        Assert.Equal(UnprocessedNotificationReason.UnrecognizedNotification, PaymentNotificationRule.Classify(Notice(card: false), CardOrder, Gateway, true));
    }

    [Fact]
    public void PaymentNotInLira_IsMarkedUnrecognized_Row4()
    {
        Assert.Equal(UnprocessedNotificationReason.UnrecognizedNotification, PaymentNotificationRule.Classify(Notice(lira: false), CardOrder, Gateway, true));
    }

    [Fact]
    public void TestNotification_IsMarkedUnrecognizedOnTheLiveInstallation_AndProcessedElsewhere_Row4()
    {
        Assert.Equal(UnprocessedNotificationReason.UnrecognizedNotification, PaymentNotificationRule.Classify(Notice(test: true), CardOrder, Gateway, true));
        Assert.Null(PaymentNotificationRule.Classify(Notice(test: true), CardOrder, Gateway, isLiveInstallation: false));
    }

    [Theory]
    [InlineData(null, 120.0)]
    [InlineData(-1.0, 120.0)]
    [InlineData(120.0, null)]
    [InlineData(120.0, -1.0)]
    public void SuccessWhoseAmountCouldNotBeRead_IsMarkedUnrecognized_Row4(double? total, double? started)
    {
        UnprocessedNotificationReason? reason = PaymentNotificationRule.Classify(
            Notice(total: (decimal?)total, started: (decimal?)started), CardOrder, Gateway, isLiveInstallation: true);

        Assert.Equal(UnprocessedNotificationReason.UnrecognizedNotification, reason);
    }

    [Fact]
    public void FailureWhoseTotalCouldNotBeRead_IsMarkedUnrecognized_Row4()
    {
        Assert.Equal(
            UnprocessedNotificationReason.UnrecognizedNotification,
            PaymentNotificationRule.Classify(Notice(FakePaymentGateway.FailedOutcome, total: null), CardOrder, Gateway, isLiveInstallation: true));
    }

    [Fact]
    public void NotificationOfAnOrderOnAnotherPaymentMethod_IsMarkedUnrecognized_Row4()
    {
        OrderForPayment transfer = CardOrder with { PaymentMethod = PaymentMethod.BankTransfer };

        Assert.Equal(
            UnprocessedNotificationReason.UnrecognizedNotification,
            PaymentNotificationRule.Classify(Notice(FakePaymentGateway.FailedOutcome), transfer, Gateway, isLiveInstallation: true));
    }

    [Fact]
    public void RecordedAmount_IsTheCollectedTotal_OrNoneWhenUnreadOrNegative()
    {
        Assert.Equal(127.35m, PaymentNotificationRule.RecordedAmount(Notice(total: 127.35m)));
        Assert.Null(PaymentNotificationRule.RecordedAmount(Notice(total: null)));
        Assert.Null(PaymentNotificationRule.RecordedAmount(Notice(total: -0.01m)));
    }

    [Fact]
    public void Verify_AsksTheLinkLayer_AndGivesItsNoticeOnlyWhenTheSignatureHolds_KK1()
    {
        FakePaymentGateway gateway = new();
        PaymentNotice notice = Notice();
        gateway.EnqueueVerification(new NotificationVerification.SignatureValid(notice), new NotificationVerification.SignatureInvalid());
        PaymentNotificationEndpoint endpoint = Endpoint(gateway);
        Dictionary<string, string> fields = new() { ["field"] = "value" };

        Assert.Same(notice, endpoint.Verify(fields));
        Assert.Null(endpoint.Verify(fields));
        Assert.Equal([nameof(IPaymentGateway.VerifyNotification), nameof(IPaymentGateway.VerifyNotification)], gateway.Calls.Select(c => c.Job));
        Assert.Equal("OK", endpoint.Acknowledgement);
    }

    [Fact]
    public void Verify_WithoutAPaymentGateway_RefusesEveryRequest()
    {
        Assert.Null(Endpoint(null).Verify(new Dictionary<string, string> { ["field"] = "value" }));
    }

    private static PaymentNotificationEndpoint Endpoint(IPaymentGateway? gateway) =>
        new(null!, null!, null!, TimeProvider.System, new LiveEnvironment(), NullLogger<PaymentNotificationEndpoint>.Instance, gateway);

    private sealed class LiveEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = Environments.Production;

        public string ApplicationName { get; set; } = "Shopfolio";

        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;

        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } = new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
