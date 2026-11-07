using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Status;

/// <summary>
/// The admission check of an upload against the disk (05 7.7.1; 09 15.7 item 5): an upload is refused when its declared size, added to what the disk holds,
/// brings the disk to the status endpoint's disk threshold or past it (05 9.4.3). The threshold is the status endpoint's own setting
/// (<see cref="StatusThresholdOptions.DiskUsageFraction"/>, default 85 percent), so the two never disagree (09 3.5 item 3; K-1392). The disk is left to the
/// database: the limit is reached before the disk is full.
/// </summary>
internal sealed class DiskUsageGuard
{
    private readonly IDiskUsageReader _disk;
    private readonly IOptions<StatusThresholdOptions> _options;

    public DiskUsageGuard(IDiskUsageReader disk, IOptions<StatusThresholdOptions> options)
    {
        ArgumentNullException.ThrowIfNull(disk);
        ArgumentNullException.ThrowIfNull(options);
        _disk = disk;
        _options = options;
    }

    /// <summary>True when <paramref name="sizeBytes"/> more bytes bring the disk to the threshold or past it; one byte less than that still passes.</summary>
    public bool WouldExceed(long sizeBytes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sizeBytes);
        return WouldExceed(_disk.Capacity(), sizeBytes, _options.Value.DiskUsageFraction);
    }

    /// <summary>The check as a pure function; decimal arithmetic keeps the border exact (a fraction of 0.85 of 1000 bytes is exactly 850).</summary>
    public static bool WouldExceed(DiskCapacity capacity, long sizeBytes, double usageFraction)
    {
        if (capacity.TotalBytes <= 0)
        {
            return true;
        }

        decimal used = (decimal)capacity.TotalBytes - capacity.FreeBytes + sizeBytes;
        return used >= (decimal)usageFraction * capacity.TotalBytes;
    }
}
