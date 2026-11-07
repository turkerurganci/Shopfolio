using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Status;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The status endpoint's warning conditions as pure rules (T314 criteria 3-10, KK3-KK10; 05 9.4.2-9.4.18). Every boundary is derived from the
/// thresholds object, never written as a second copy of the table; one test pins the defaults to the document's table. The moment is a fixed
/// instant and no test waits.
/// </summary>
public sealed class StatusRulesTests
{
    private static readonly DateTimeOffset _now = new(2026, 11, 7, 12, 0, 0, TimeSpan.Zero);

    private static readonly StatusThresholdOptions _options = new();

    // --- The defaults are the document's table (criteria 3-9; 05 9.4.2) ---

    [Fact]
    public void TheDefaults_AreTheValuesOfTheTable_05_9_4_2()
    {
        Assert.Equal(0.85, _options.DiskUsageFraction);
        Assert.Equal(10, _options.LogShippingStallMinutes);
        Assert.Equal(10, _options.LogFilesWaitingMax);
        Assert.Equal(26, _options.BackupAgeHours);
        Assert.Equal(5, _options.HeartbeatAgeMinutes);
        Assert.Equal(10, _options.UnexpectedErrorCount);
        Assert.Equal(15, _options.UnexpectedErrorWindowMinutes);
        Assert.Equal(5, _options.PaymentStartFailureCount);
        Assert.Equal(15, _options.PaymentStartFailureAgeMinutes);
        Assert.Equal(14, _options.CertificateDaysLeft);
        Assert.Equal(15, _options.JobQueueAgeMinutes);
        Assert.Equal(26, _options.NightlyJobAgeHours);
    }

    // --- KK3: disk and log shipping ---

    [Fact]
    public void Disk_BelowTheThreshold_IsClosed_AtAndAboveIt_IsOpen()
    {
        Assert.False(StatusRules.DiskFull(_options.DiskUsageFraction - 0.01, _options));
        Assert.True(StatusRules.DiskFull(_options.DiskUsageFraction, _options));
        Assert.True(StatusRules.DiskFull(_options.DiskUsageFraction + 0.01, _options));
    }

    [Fact]
    public void LogShipping_WithNothingFailedAndFewFiles_IsClosed()
    {
        LogShippingState state = new(_now.AddMinutes(-1), null, 0);

        Assert.False(StatusRules.LogShippingStalled(state, _now, _options));
    }

    [Fact]
    public void LogShipping_FailingForLessThanTheStallLimit_IsClosed_AtTheLimit_IsOpen()
    {
        TimeSpan limit = TimeSpan.FromMinutes(_options.LogShippingStallMinutes);
        DateTimeOffset lastArchived = _now - limit;

        LogShippingState justUnder = new(lastArchived.AddSeconds(1), _now, 1);
        LogShippingState atLimit = new(lastArchived, _now, 1);

        Assert.False(StatusRules.LogShippingStalled(justUnder, _now, _options));
        Assert.True(StatusRules.LogShippingStalled(atLimit, _now, _options));
    }

    [Fact]
    public void LogShipping_AFailureOlderThanTheLastSuccess_IsNotAStall()
    {
        LogShippingState recovered = new(_now.AddMinutes(-1), _now.AddHours(-3), 0);

        Assert.False(StatusRules.LogShippingStalled(recovered, _now, _options));
    }

    [Fact]
    public void LogShipping_NeverArchivedButFailing_CountsTheStallFromTheFailure()
    {
        TimeSpan limit = TimeSpan.FromMinutes(_options.LogShippingStallMinutes);

        Assert.False(StatusRules.LogShippingStalled(new LogShippingState(null, _now - limit + TimeSpan.FromSeconds(1), 0), _now, _options));
        Assert.True(StatusRules.LogShippingStalled(new LogShippingState(null, _now - limit, 0), _now, _options));
    }

    [Fact]
    public void LogShipping_FilesWaiting_AtTheMaximumIsClosed_OneMoreIsOpen_WithoutAnyFailure()
    {
        LogShippingState atMax = new(_now, null, _options.LogFilesWaitingMax);
        LogShippingState over = new(_now, null, _options.LogFilesWaitingMax + 1);

        Assert.False(StatusRules.LogShippingStalled(atMax, _now, _options));
        Assert.True(StatusRules.LogShippingStalled(over, _now, _options));
    }

    // --- KK4: the two backups are separate conditions on the same rule; never succeeded is open ---

    [Fact]
    public void Backup_WithinTheAge_IsClosed_PastIt_IsOpen_AndNeverSucceeded_IsOpen()
    {
        TimeSpan limit = TimeSpan.FromHours(_options.BackupAgeHours);

        Assert.False(StatusRules.BackupTooOld(_now - limit, _now, _options));
        Assert.True(StatusRules.BackupTooOld(_now - limit - TimeSpan.FromSeconds(1), _now, _options));
        Assert.True(StatusRules.BackupTooOld(null, _now, _options));
    }

    // --- KK5: the heartbeat ---

    [Fact]
    public void Heartbeat_WithinTheAge_IsClosed_PastIt_IsOpen_AndNone_IsOpen()
    {
        TimeSpan limit = TimeSpan.FromMinutes(_options.HeartbeatAgeMinutes);

        Assert.False(StatusRules.HeartbeatTooOld(_now - limit, _now, _options));
        Assert.True(StatusRules.HeartbeatTooOld(_now - limit - TimeSpan.FromSeconds(1), _now, _options));
        Assert.True(StatusRules.HeartbeatTooOld(null, _now, _options));
    }

    // --- KK7: the unexpected errors; the counter takes only what the layer records ---

    [Fact]
    public void Errors_OneBelowTheCountIsClosed_TheCountIsOpen()
    {
        Assert.False(StatusRules.UnexpectedErrorsHigh(_options.UnexpectedErrorCount - 1, _options));
        Assert.True(StatusRules.UnexpectedErrorsHigh(_options.UnexpectedErrorCount, _options));
    }

    [Fact]
    public void ErrorCounter_CountsOnlyTheWindow_AnErrorOlderThanTheWindowDoesNotCount()
    {
        ManualTimeProvider time = new(_now);
        UnexpectedErrorCounter counter = new(time);
        TimeSpan window = TimeSpan.FromMinutes(_options.UnexpectedErrorWindowMinutes);

        for (int i = 0; i < _options.UnexpectedErrorCount - 1; i++)
        {
            counter.Record();
        }

        Assert.False(StatusRules.UnexpectedErrorsHigh(counter.CountWithin(window), _options));

        counter.Record();
        Assert.True(StatusRules.UnexpectedErrorsHigh(counter.CountWithin(window), _options));

        time.Advance(window + TimeSpan.FromSeconds(1));
        Assert.False(StatusRules.UnexpectedErrorsHigh(counter.CountWithin(window), _options));
    }

    // --- KK8: the payment start's series and the certificate ---

    [Fact]
    public void PaymentSeries_FewerFailuresThanTheCount_IsClosed_EvenWhenOld()
    {
        DateTimeOffset first = _now - TimeSpan.FromMinutes(_options.PaymentStartFailureAgeMinutes + 5);

        Assert.False(StatusRules.PaymentStartSeriesOpen(_options.PaymentStartFailureCount - 1, first, _now, _options));
    }

    [Fact]
    public void PaymentSeries_EnoughFailuresButTheFirstIsYounger_IsClosed_ThenOpenOnceItIsOldEnough()
    {
        TimeSpan age = TimeSpan.FromMinutes(_options.PaymentStartFailureAgeMinutes);

        Assert.False(StatusRules.PaymentStartSeriesOpen(_options.PaymentStartFailureCount, _now - age + TimeSpan.FromSeconds(1), _now, _options));
        Assert.True(StatusRules.PaymentStartSeriesOpen(_options.PaymentStartFailureCount, _now - age, _now, _options));
    }

    [Fact]
    public void PaymentSeriesMemory_StampsTheFirstFailure_AndTheFirstSuccessEndsTheSeries()
    {
        ManualTimeProvider time = new(_now);
        PaymentStartFailureSeries series = new(time);
        TimeSpan age = TimeSpan.FromMinutes(_options.PaymentStartFailureAgeMinutes);

        series.RecordFailure();
        time.Advance(age);
        for (int i = 1; i < _options.PaymentStartFailureCount; i++)
        {
            series.RecordFailure();
        }

        (int count, DateTimeOffset? first) = series.Snapshot();
        Assert.Equal(_options.PaymentStartFailureCount, count);
        Assert.Equal(_now, first);
        Assert.True(StatusRules.PaymentStartSeriesOpen(count, first, time.GetUtcNow(), _options));

        series.RecordSuccess();

        (int afterCount, DateTimeOffset? afterFirst) = series.Snapshot();
        Assert.Equal(0, afterCount);
        Assert.Null(afterFirst);
        Assert.False(StatusRules.PaymentStartSeriesOpen(afterCount, afterFirst, time.GetUtcNow(), _options));
    }

    [Fact]
    public void Certificate_WithMoreDaysLeftThanTheThreshold_IsClosed_FewerIsOpen_AndUnread_IsOpen()
    {
        TimeSpan threshold = TimeSpan.FromDays(_options.CertificateDaysLeft);

        Assert.False(StatusRules.CertificateExpiring(_now + threshold, _now, _options));
        Assert.True(StatusRules.CertificateExpiring(_now + threshold - TimeSpan.FromSeconds(1), _now, _options));
        Assert.True(StatusRules.CertificateExpiring(null, _now, _options));
    }

    // --- KK9: the job queue and the nightly job ---

    [Fact]
    public void JobQueue_NoRows_IsClosed()
    {
        Assert.False(StatusRules.JobQueueStuck(null, false, _now, _options));
    }

    [Fact]
    public void JobQueue_TheOldestDueRowWithinTheLimit_IsClosed_PastIt_IsOpen()
    {
        TimeSpan limit = TimeSpan.FromMinutes(_options.JobQueueAgeMinutes);

        Assert.False(StatusRules.JobQueueStuck(_now - limit, false, _now, _options));
        Assert.True(StatusRules.JobQueueStuck(_now - limit - TimeSpan.FromSeconds(1), false, _now, _options));
    }

    [Fact]
    public void JobQueue_AFailedRowOpensIt_EvenWhenNothingIsDue()
    {
        Assert.True(StatusRules.JobQueueStuck(null, true, _now, _options));
    }

    [Fact]
    public void NightlyJob_WithinTheAge_IsClosed_PastIt_IsOpen_AndNeverSucceeded_IsOpen()
    {
        TimeSpan limit = TimeSpan.FromHours(_options.NightlyJobAgeHours);

        Assert.False(StatusRules.NightlyJobTooOld(_now - limit, _now, _options));
        Assert.True(StatusRules.NightlyJobTooOld(_now - limit - TimeSpan.FromSeconds(1), _now, _options));
        Assert.True(StatusRules.NightlyJobTooOld(null, _now, _options));
    }

    // --- KK10: the thresholds are the setting's, not the rules' ---

    [Fact]
    public void ChangingTheThreshold_MovesTheRule()
    {
        DateTimeOffset beat = _now - TimeSpan.FromMinutes(_options.HeartbeatAgeMinutes + 1);
        StatusThresholdOptions looser = new() { HeartbeatAgeMinutes = _options.HeartbeatAgeMinutes + 10 };

        Assert.True(StatusRules.HeartbeatTooOld(beat, _now, _options));
        Assert.False(StatusRules.HeartbeatTooOld(beat, _now, looser));
    }
}
