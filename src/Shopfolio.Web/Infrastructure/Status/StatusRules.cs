using Shopfolio.Infrastructure.InstallationSettings;

namespace Shopfolio.Infrastructure.Status;

/// <summary>What the database says about the log archiving (05 9.4.4, 9.4.10): the last success, the last failure and the files waiting to be shipped.</summary>
internal sealed record LogShippingState(DateTimeOffset? LastArchivedAt, DateTimeOffset? LastFailedAt, int WaitingFiles);

/// <summary>
/// The rules of the status endpoint's warning conditions (05 9.4.2-9.4.18) as pure functions: the measured value, the moment and the threshold in,
/// "open" out. The thresholds are <see cref="StatusThresholdOptions"/> (05 8.4.5); no number is written here. A value that was never measured
/// (empty) is an open condition: nothing proves the source is fine (05 9.4.10, K-975).
/// </summary>
internal static class StatusRules
{
    /// <summary>9.4.3 - the disk is as full as the threshold or fuller.</summary>
    public static bool DiskFull(double usedFraction, StatusThresholdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return usedFraction >= options.DiskUsageFraction;
    }

    /// <summary>
    /// 9.4.4 - more files wait than the threshold, or the last archiving attempt failed (it is newer than the last success) and the
    /// log has not been shipped for the stall limit. With no success at all the stall counts from the failure.
    /// </summary>
    public static bool LogShippingStalled(LogShippingState state, DateTimeOffset now, StatusThresholdOptions options)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(options);
        if (state.WaitingFiles > options.LogFilesWaitingMax)
        {
            return true;
        }

        if (state.LastFailedAt is not { } failedAt || (state.LastArchivedAt is { } archivedAt && failedAt <= archivedAt))
        {
            return false;
        }

        DateTimeOffset stalledSince = state.LastArchivedAt ?? failedAt;
        return now - stalledSince >= TimeSpan.FromMinutes(options.LogShippingStallMinutes);
    }

    /// <summary>9.4.5 - the last success is older than the limit, or there has been none.</summary>
    public static bool BackupTooOld(DateTimeOffset? lastSucceededAt, DateTimeOffset now, StatusThresholdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return TooOld(lastSucceededAt, now, TimeSpan.FromHours(options.BackupAgeHours));
    }

    /// <summary>9.4.6 - the last heartbeat is older than the limit, or there has been none.</summary>
    public static bool HeartbeatTooOld(DateTimeOffset? lastHeartbeatAt, DateTimeOffset now, StatusThresholdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return TooOld(lastHeartbeatAt, now, TimeSpan.FromMinutes(options.HeartbeatAgeMinutes));
    }

    /// <summary>9.4.17 - the nightly destruction and clean-up's last success is older than the limit, or there has been none.</summary>
    public static bool NightlyJobTooOld(DateTimeOffset? lastSucceededAt, DateTimeOffset now, StatusThresholdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return TooOld(lastSucceededAt, now, TimeSpan.FromHours(options.NightlyJobAgeHours));
    }

    /// <summary>9.4.8 - at least the threshold's number of unexpected errors within the window; the caller counts the window.</summary>
    public static bool UnexpectedErrorsHigh(int countInWindow, StatusThresholdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return countInWindow >= options.UnexpectedErrorCount;
    }

    /// <summary>9.4.18 - enough failures in a row and the first of them is old enough; an empty series is closed.</summary>
    public static bool PaymentStartSeriesOpen(int failuresInARow, DateTimeOffset? firstFailureAt, DateTimeOffset now, StatusThresholdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return failuresInARow >= options.PaymentStartFailureCount
            && firstFailureAt is { } first
            && now - first >= TimeSpan.FromMinutes(options.PaymentStartFailureAgeMinutes);
    }

    /// <summary>9.4.9 - fewer days than the threshold are left to the certificate's end; a certificate not read yet is open.</summary>
    public static bool CertificateExpiring(DateTimeOffset? notAfter, DateTimeOffset now, StatusThresholdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return notAfter is not { } end || end - now < TimeSpan.FromDays(options.CertificateDaysLeft);
    }

    /// <summary>9.4.16 - the oldest due pending row has waited longer than the limit, or a failed row exists.</summary>
    public static bool JobQueueStuck(DateTimeOffset? oldestDueAt, bool failedRowExists, DateTimeOffset now, StatusThresholdOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        return failedRowExists
            || (oldestDueAt is { } due && now - due > TimeSpan.FromMinutes(options.JobQueueAgeMinutes));
    }

    private static bool TooOld(DateTimeOffset? last, DateTimeOffset now, TimeSpan limit) => last is not { } moment || now - moment > limit;
}
