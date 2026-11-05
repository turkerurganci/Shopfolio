using Shopfolio.Infrastructure.Jobs;

namespace Shopfolio.UnitTests.Jobs;

/// <summary>
/// The outage threshold and the heartbeat's freshness (T221 criterion 2, KK2, KK7; 05 5.4.1, 5.4.3): the boundary is read from
/// <see cref="SchedulerTimings.OutageThreshold"/>. A gap of exactly the threshold is not an outage; a tick more is.
/// </summary>
public sealed class OutageRecoveryRuleTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 5, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void IsOutage_AGapOfTheThreshold_IsNot_AndALongerOneIs()
    {
        Assert.False(OutageRecovery.IsOutage(Now - SchedulerTimings.OutageThreshold, Now));
        Assert.True(OutageRecovery.IsOutage(Now - SchedulerTimings.OutageThreshold - TimeSpan.FromTicks(1), Now));
        Assert.False(OutageRecovery.IsOutage(Now - SchedulerTimings.HeartbeatInterval, Now));
    }

    [Fact]
    public void IsFresh_ABeatWithinTheThreshold_IsFresh_AndOlderOrMissingIsNot()
    {
        Assert.True(OutageRecovery.IsFresh(Now - SchedulerTimings.OutageThreshold, Now));
        Assert.False(OutageRecovery.IsFresh(Now - SchedulerTimings.OutageThreshold - TimeSpan.FromTicks(1), Now));
        Assert.False(OutageRecovery.IsFresh(null, Now));
    }

    [Fact]
    public void Timings_AreOrdered_SoThatSeveralBeatsFitInsideTheThreshold()
    {
        Assert.True(SchedulerTimings.HeartbeatInterval * 2 < SchedulerTimings.OutageThreshold);
        Assert.True(Shopfolio.Infrastructure.ExternalCallTimeouts.UserWaiting < SchedulerTimings.OutageThreshold);
    }
}
