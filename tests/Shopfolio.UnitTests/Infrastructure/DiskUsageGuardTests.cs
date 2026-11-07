using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Status;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The disk admission check of an upload (T312 criterion 6; 09 15.7 item 5; 05 7.7.1, 9.4.3; 10.1.8): a declared size that brings the disk to the status endpoint's threshold or
/// past it is refused; one byte less passes. The border is exact (decimal arithmetic): at 85 percent of 1000 bytes the border is 850. The threshold is the status endpoint's own
/// setting, so the two never disagree.
/// </summary>
public sealed class DiskUsageGuardTests
{
    private sealed class FakeDisk : IDiskUsageReader
    {
        public FakeDisk(long total, long free)
        {
            Total = total;
            Free = free;
        }

        public long Total { get; set; }

        public long Free { get; set; }

        public double UsedFraction() => 1.0 - ((double)Free / Total);

        public DiskCapacity Capacity() => new(Total, Free);
    }

    [Theory]
    [InlineData(1000, 1000, 849, false)]
    [InlineData(1000, 1000, 850, true)]
    [InlineData(1000, 1000, 851, true)]
    [InlineData(1000, 500, 349, false)]
    [InlineData(1000, 500, 350, true)]
    [InlineData(1000, 150, 0, true)]
    [InlineData(1000, 151, 0, false)]
    [InlineData(1000, 0, 1, true)]
    public void StaticCheck_ReachingTheBorderIsRefused_OneByteLessPasses(long total, long free, long size, bool expected)
    {
        Assert.Equal(expected, DiskUsageGuard.WouldExceed(new DiskCapacity(total, free), size, 0.85));
    }

    [Fact]
    public void StaticCheck_ADiskOfUnknownSize_IsRefused_NotDividedByZero()
    {
        Assert.True(DiskUsageGuard.WouldExceed(new DiskCapacity(0, 0), 1, 0.85));
    }

    [Theory]
    [InlineData(0.5, 499, false)]
    [InlineData(0.5, 500, true)]
    [InlineData(0.95, 949, false)]
    [InlineData(0.95, 950, true)]
    public void Guard_ReadsTheStatusEndpointsOwnSetting_AndTheDisksCurrentCapacity(double fraction, long size, bool expected)
    {
        DiskUsageGuard guard = new(new FakeDisk(1000, 1000), Options.Create(new StatusThresholdOptions { DiskUsageFraction = fraction }));

        Assert.Equal(expected, guard.WouldExceed(size));
    }

    [Fact]
    public void Guard_UsesTheDefaultThreshold_OfThePeerStatusEndpoint()
    {
        DiskUsageGuard guard = new(new FakeDisk(1000, 1000), Options.Create(new StatusThresholdOptions()));

        Assert.False(guard.WouldExceed(849));
        Assert.True(guard.WouldExceed(850));
    }

    [Fact]
    public void Guard_ReadsTheCapacityAtEveryCall_NotOnce()
    {
        FakeDisk disk = new(1000, 1000);
        DiskUsageGuard guard = new(disk, Options.Create(new StatusThresholdOptions()));
        Assert.False(guard.WouldExceed(100));

        disk.Free = 200;

        Assert.True(guard.WouldExceed(100));
    }

    [Fact]
    public void Guard_ANegativeSize_IsAProgrammingError()
    {
        DiskUsageGuard guard = new(new FakeDisk(1000, 1000), Options.Create(new StatusThresholdOptions()));

        Assert.Throws<ArgumentOutOfRangeException>(() => guard.WouldExceed(-1));
    }
}
