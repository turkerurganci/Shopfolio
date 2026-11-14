using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.UnitTests.Ordering;

/// <summary>
/// The screen's side of the status correction (T530 KK6, KK7; 07 UC-150; K-703): every obstacle has its own message, every correctable transition its own name on the form, and the result's gate
/// rolls the transaction back on every refusal while only the version conflict is the 409.
/// </summary>
public sealed class PanelCorrectionTextsTests
{
    [Fact]
    public void EveryObstacle_HasItsOwnMessage_KK7()
    {
        string[] texts = [.. Enum.GetValues<CorrectionObstacle>().Select(OrderDetailModel.ObstacleTextOf)];

        Assert.All(texts, t => Assert.False(string.IsNullOrWhiteSpace(t)));
        Assert.Equal(texts.Length, texts.Distinct(StringComparer.Ordinal).Count());
    }

    [Theory]
    [InlineData(TransitionCode.S3)]
    [InlineData(TransitionCode.S4)]
    [InlineData(TransitionCode.S5)]
    [InlineData(TransitionCode.S6)]
    [InlineData(TransitionCode.S7)]
    [InlineData(TransitionCode.S8)]
    [InlineData(TransitionCode.S9)]
    [InlineData(TransitionCode.O1)]
    public void EveryCorrectableTransition_IsNamedOnTheForm_NotByItsCode_KK6(TransitionCode code)
    {
        Assert.NotEqual(code.ToString(), OrderDetailModel.TransitionNameOf(code));
    }

    [Fact]
    public void TheBackState_IsTheRecordsOrigin_OnTheShipmentAxisOrThePaymentAxis_KK6()
    {
        PanelCorrectableTransition shipment = new(Guid.NewGuid(), TransitionCode.S5, DateTimeOffset.UnixEpoch, OrderStatus.Preparing, null);
        PanelCorrectableTransition paid = new(Guid.NewGuid(), TransitionCode.O1, DateTimeOffset.UnixEpoch, null, PaymentStatus.Pending);

        Assert.Equal(Shopfolio.Pages.Components.StatusBadges.For(OrderStatus.Preparing).Text, OrderDetailModel.BackStateOf(shipment));
        Assert.Equal(Shopfolio.Pages.Components.StatusBadges.For(PaymentStatus.Pending).Text, OrderDetailModel.BackStateOf(paid));
    }

    [Fact]
    public void TheResultsGate_IsTheConflictOnlyForTheVersion_AndClosedForEveryOtherRefusal()
    {
        Assert.False(PanelCorrectionResult.Done.Gate.IsRejected);
        Assert.Same(GatedOperationResult.Conflict, new PanelCorrectionResult(TransitionRefusal.VersionConflict).Gate);
        Assert.All(
            Enum.GetValues<TransitionRefusal>().Where(r => r != TransitionRefusal.VersionConflict),
            r => Assert.True(new PanelCorrectionResult(r).Gate.IsRejected));
    }
}
