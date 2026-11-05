namespace Shopfolio.Infrastructure.Jobs;

/// <summary>
/// The times of the scheduler (09 3.5 table rows "Zamanlayıcının tarama aralığı…" and "Havale hatırlatması ve kesinti eşiği…"; 05 5.1.1, 5.3,
/// 5.4.1). Every value is a row of that table and has no other place: the scheduler, the schedule functions and the tests read these constants
/// and never write a number of their own (<c>SchedulerTimingsConsistencyTests</c> keeps them equal to the table). The clock times are Turkey
/// times (09 7.1 item 3).
/// </summary>
internal static class SchedulerTimings
{
    /// <summary>How often the scheduler wakes to look for records whose time has come: 1 minute (05 5.1.1).</summary>
    public static readonly TimeSpan ScanInterval = TimeSpan.FromMinutes(1);

    /// <summary>How often the heartbeat loop sends its request: 30 seconds (05 5.4.1).</summary>
    public static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// A gap between two heartbeats longer than this is an outage, and a last heartbeat older than this holds back the transfer order's
    /// cancellation: 2 minutes (05 5.4.1, 5.4.3).
    /// </summary>
    public static readonly TimeSpan OutageThreshold = TimeSpan.FromMinutes(2);

    /// <summary>The hour of the nightly job (Turkey time): 03:00 (05 5.3.2; MK-43).</summary>
    public const int NightlyHour = 3;

    /// <summary>A nightly job that ended in failure is tried again an hour after its last attempt at the earliest (05 5.1.1).</summary>
    public static readonly TimeSpan NightlyRetryWait = TimeSpan.FromHours(1);

    /// <summary>The hour of the bank transfer reminder on the last business day of the payment window (Turkey time): 08:00 (05 5.3.1; MK-43).</summary>
    public const int TransferReminderHour = 8;

    /// <summary>
    /// The most records one source hands the scheduler in one wake-up. A safety bound for a long outage; what is left is found at the next
    /// wake-up, still in order of time (design value, 09 13.1 item 6).
    /// </summary>
    public const int DueBatchLimit = 500;

    /// <summary>The name of the scheduler's single-run lock (09 13.1 item 7).</summary>
    public const string SchedulerLockName = "Scheduler";

    /// <summary>The name of the nightly job's single-run lock (09 13.1 item 7).</summary>
    public const string NightlyLockName = "NightlyJob";
}
