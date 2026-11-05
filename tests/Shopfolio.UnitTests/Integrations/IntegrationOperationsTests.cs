using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Integrations;

namespace Shopfolio.UnitTests.Integrations;

/// <summary>
/// The limit of each job of the link layer (T202 criterion 3, KK3; 08 3.1 table "Zaman aşımı" column; 05 10.1.1). The expected limits are the
/// two constants themselves, which the consistency test keeps equal to the 09 3.5 table; no number is written here.
/// </summary>
public sealed class IntegrationOperationsTests
{
    [Theory]
    [InlineData(IntegrationOperation.PaymentStart)]
    [InlineData(IntegrationOperation.IdentityVerify)]
    public void LimitOf_ACallTheUserWaitsFor_IsTheUserWaitingLimit(IntegrationOperation operation)
    {
        Assert.Equal(ExternalCallKind.UserWaiting, IntegrationOperations.KindOf(operation));
        Assert.Equal(ExternalCallTimeouts.UserWaiting, IntegrationOperations.LimitOf(operation));
    }

    [Theory]
    [InlineData(IntegrationOperation.PaymentQuery)]
    [InlineData(IntegrationOperation.RefundRequest)]
    [InlineData(IntegrationOperation.RefundQuery)]
    [InlineData(IntegrationOperation.EmailSend)]
    [InlineData(IntegrationOperation.BounceQuery)]
    public void LimitOf_ACallThatLeavesTheJobQueue_IsTheJobLimit(IntegrationOperation operation)
    {
        Assert.Equal(ExternalCallKind.Job, IntegrationOperations.KindOf(operation));
        Assert.Equal(ExternalCallTimeouts.Job, IntegrationOperations.LimitOf(operation));
    }

    [Fact]
    public void LimitOf_EveryJob_IsOneOfTheTwoConstants()
    {
        Assert.All(
            Enum.GetValues<IntegrationOperation>(),
            operation => Assert.Contains(IntegrationOperations.LimitOf(operation), new[] { ExternalCallTimeouts.UserWaiting, ExternalCallTimeouts.Job }));
        Assert.Equal(ExternalCallTimeouts.UserWaiting, ExternalCallTimeouts.For(ExternalCallKind.UserWaiting));
        Assert.Equal(ExternalCallTimeouts.Job, ExternalCallTimeouts.For(ExternalCallKind.Job));
    }
}
