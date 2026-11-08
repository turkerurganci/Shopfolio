using Shopfolio.Infrastructure.Integrations;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Architecture;

/// <summary>
/// Card payment works with one provider and has no second implementation (T202 criterion 2, KK2; 08 3.1 rule 1; 10 KD-64): the application
/// assembly holds at most one concrete <see cref="IPaymentGateway"/> - none until the payment provider's task adds it. The count is shown to go
/// above one on a sample of two implementations.
/// </summary>
public sealed class PaymentGatewayImplementationTests
{
    private static IReadOnlyList<Type> ImplementationsOf(Type contract, IEnumerable<Type> types) =>
        [.. types.Where(t => t is { IsClass: true, IsAbstract: false } && contract.IsAssignableFrom(t))];

    [Fact]
    public void ApplicationAssembly_HasAtMostOnePaymentGateway()
    {
        IReadOnlyList<Type> implementations = ImplementationsOf(typeof(IPaymentGateway), typeof(IPaymentGateway).Assembly.GetTypes());

        Assert.True(implementations.Count <= 1, string.Join(", ", implementations.Select(t => t.FullName)));
    }

    [Fact]
    public void ImplementationsOf_OnASampleWithASecondGateway_CountsBoth()
    {
        IReadOnlyList<Type> implementations = ImplementationsOf(
            typeof(IPaymentGateway), [typeof(FakePaymentGateway), typeof(SecondPaymentGateway), typeof(IPaymentGateway), typeof(FakeEmailGateway)]);

        Assert.Equal([typeof(FakePaymentGateway), typeof(SecondPaymentGateway)], implementations);
    }

    // Deliberate violation sample: a second card payment implementation.
    private sealed class SecondPaymentGateway : IPaymentGateway
    {
        public Task<PaymentStartResult> StartAsync(PaymentStartRequest request, CancellationToken ct) => throw new NotSupportedException();

        public Task<PaymentQueryResult> QueryPaymentAsync(string orderNumber, CancellationToken ct) => throw new NotSupportedException();

        public Task<RefundRequestResult> RequestRefundAsync(CardRefund refund, CancellationToken ct) => throw new NotSupportedException();

        public Task<RefundQueryResult> QueryRefundAsync(CardRefund refund, CancellationToken ct) => throw new NotSupportedException();

        public NotificationVerification VerifyNotification(IReadOnlyDictionary<string, string> formFields) => throw new NotSupportedException();

        public NotificationResult ReadNotificationResult(string eventType, string outcome) => throw new NotSupportedException();

        public NotificationAmountCheck CheckNotificationAmount(PaymentNotice notice, decimal orderTotal) => throw new NotSupportedException();
    }
}
