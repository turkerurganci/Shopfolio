using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the scheduler's times equal to the rows of the 09 3.5 table that name them (T221; 09 13.1 items 6, 8, 9; 21.4 item 1): the one-minute
/// scan, the thirty-second heartbeat, the nightly job's hour and retry wait, the transfer reminder's hour, the two-minute outage threshold, and
/// the two outside-call limits. The constants of <c>SchedulerTimings</c> and <c>ExternalCallTimeouts</c> are the only place the code has these
/// numbers; the schedule functions and the tests read them. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class SchedulerTimingsConsistencyTests
{
    private static string TableRow(string start)
    {
        string guidelines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md"));
        return guidelines.ReplaceLineEndings("\n").Split('\n').Single(line => line.StartsWith(start, StringComparison.Ordinal));
    }

    private static int Number(Group group) => int.Parse(group.Value, CultureInfo.InvariantCulture);

    [Fact]
    public void SchedulerRow_EqualsTheScanTheHeartbeatTheNightlyHourAndTheRetryWait()
    {
        string row = TableRow("| Zamanlayıcının tarama aralığı");

        Match match = SchedulerRow().Match(row);

        Assert.True(match.Success, "The scheduler row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(TimeSpan.FromMinutes(Number(match.Groups[1])), SchedulerTimings.ScanInterval);
        Assert.Equal(TimeSpan.FromSeconds(Number(match.Groups[2])), SchedulerTimings.HeartbeatInterval);
        Assert.Equal(SchedulerTimings.NightlyHour, Number(match.Groups[3]));
        Assert.Equal(TimeSpan.FromHours(Number(match.Groups[4])), SchedulerTimings.NightlyRetryWait);
    }

    [Fact]
    public void ReminderRow_EqualsTheReminderHourAndTheOutageThreshold()
    {
        string row = TableRow("| Havale hatırlatması");

        Match match = ReminderRow().Match(row);

        Assert.True(match.Success, "The transfer reminder row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(SchedulerTimings.TransferReminderHour, Number(match.Groups[1]));
        Assert.Equal(TimeSpan.FromMinutes(Number(match.Groups[2])), SchedulerTimings.OutageThreshold);
    }

    [Fact]
    public void OutsideCallRow_EqualsTheTwoLimits()
    {
        string row = TableRow("| Dış çağrı süresi");

        Match match = CallRow().Match(row);

        Assert.True(match.Success, "The outside call row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(TimeSpan.FromSeconds(Number(match.Groups[1])), ExternalCallTimeouts.UserWaiting);
        Assert.Equal(TimeSpan.FromSeconds(Number(match.Groups[2])), ExternalCallTimeouts.Job);
    }

    [Fact]
    public void Checks_CanGoRed_WhenAValueDiffersFromTheRow()
    {
        // The checks can go red: a value one more than the row's is not equal to what the row says.
        Match scheduler = SchedulerRow().Match(TableRow("| Zamanlayıcının tarama aralığı"));
        Match reminder = ReminderRow().Match(TableRow("| Havale hatırlatması"));

        Assert.NotEqual(TimeSpan.FromMinutes(Number(scheduler.Groups[1]) + 1), SchedulerTimings.ScanInterval);
        Assert.NotEqual(SchedulerTimings.TransferReminderHour, Number(reminder.Groups[1]) + 1);
    }

    [GeneratedRegex(@"tarama aralığı (\d+) dakika; nabız (\d+) saniye; gecelik iş (\d+):00'te \(Türkiye saati\), başarısız gecelik işin yeniden deneme beklemesi (\d+) saat", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex SchedulerRow();

    [GeneratedRegex(@"Havale hatırlatması (\d+):00 \(Türkiye saati\); kesinti eşiği ve son nabız tazeliği (\d+) dakika", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ReminderRow();

    [GeneratedRegex(@"kullanıcının beklediği çağrıda (\d+), iş kuyruğundan giden çağrıda (\d+) saniye", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex CallRow();
}
