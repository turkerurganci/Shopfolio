using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure.Jobs;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// Keeps the job queue's times equal to the rows of the 09 3.5 table that name them (T220; 09 13.1 item 5, 21.4 item 1): the five-second look at
/// the queue, the default retry waits, the unanswered final query's five minutes and the returned-address query's fifteen. The constants of
/// <c>JobTimings</c> are the only place the code has these numbers; the retry rule and the tests read them. Runs in the Lint step.
/// </summary>
[Trait("Consistency", "true")]
public sealed partial class JobTimingsConsistencyTests
{
    private static string TableRow(string start)
    {
        string guidelines = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "09_CODING_GUIDELINES.md"));
        return guidelines.ReplaceLineEndings("\n").Split('\n').Single(line => line.StartsWith(start, StringComparison.Ordinal));
    }

    [Fact]
    public void PollInterval_EqualsTheQueueRowOfTheTable()
    {
        string row = TableRow("| İş kuyruğu çalıştırıcısının kuyruğa bakma aralığı");

        Match match = PollRow().Match(row);

        Assert.True(match.Success, "The queue row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(TimeSpan.FromSeconds(Number(match.Groups[1])), JobTimings.PollInterval);
    }

    [Fact]
    public void RetryRow_EqualsTheDefaultScheduleAndTheTwoQueryIntervals()
    {
        string row = TableRow("| İş satırının varsayılan yeniden denemesi");

        Match match = RetryRow().Match(row);

        Assert.True(match.Success, "The retry row of the 09 3.5 table has changed shape; update this check with it.");
        int[] minutes = [.. match.Groups[1].Value.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries).Where(part => part != "ve").Select(part => int.Parse(part, CultureInfo.InvariantCulture))];
        Assert.Equal(minutes.Select(m => TimeSpan.FromMinutes(m)), JobTimings.DefaultRetrySchedule);
        Assert.Equal(JobTimings.DefaultRetrySchedule.Count, WordToCount(match.Groups[2].Value));
        Assert.Equal(TimeSpan.FromMinutes(Number(match.Groups[3])), JobTimings.FinalPaymentQueryInterval);
        Assert.Equal(TimeSpan.FromMinutes(Number(match.Groups[4])), JobTimings.BounceQueryInterval);
        Assert.Equal(TimeSpan.FromHours(Number(match.Groups[5])), JobTimings.BounceQueryWindow);
    }

    [Fact]
    public void RetryRow_CheckCanGoRed_WhenTheScheduleDiffers()
    {
        // The check can go red: a schedule that is not the table's is not equal to what the row says.
        string row = TableRow("| İş satırının varsayılan yeniden denemesi");
        Match match = RetryRow().Match(row);
        int[] minutes = [.. match.Groups[1].Value.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries).Where(part => part != "ve").Select(part => int.Parse(part, CultureInfo.InvariantCulture))];

        Assert.NotEqual(minutes.Select(m => TimeSpan.FromMinutes(m + 1)), JobTimings.DefaultRetrySchedule);
    }

    [Fact]
    public void RefundOutcomeQueryRow_EqualsTheFirstDelayTheFourIntervalsAndTheHourlyRepeat_T405()
    {
        string row = TableRow("| Sonucu bilinmeyen kart iadesinin sorgusu");

        Match match = RefundQueryRow().Match(row);

        Assert.True(match.Success, "The refund query row of the 09 3.5 table has changed shape; update this check with it.");
        Assert.Equal(TimeSpan.FromMinutes(Number(match.Groups[1])), JobTimings.RefundOutcomeQueryFirstDelay);
        int[] minutes = [.. match.Groups[2].Value.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries).Where(part => part != "ve").Select(part => int.Parse(part, CultureInfo.InvariantCulture))];
        Assert.Equal(minutes.Select(m => TimeSpan.FromMinutes(m)), JobTimings.RefundOutcomeQueryIntervals);
        Assert.Equal(TimeSpan.FromHours(1), JobTimings.RefundOutcomeQueryHourly);
    }

    private static int Number(Group group) => int.Parse(group.Value, CultureInfo.InvariantCulture);

    private static int WordToCount(string word)
    {
        return word switch
        {
            "bir" => 1,
            "iki" => 2,
            "üç" => 3,
            "dört" => 4,
            "beş" => 5,
            "altı" => 6,
            _ => throw new ArgumentOutOfRangeException(nameof(word), word, "The count word of the retry row is not known to this check."),
        };
    }

    [GeneratedRegex(@"kuyruğa bakma aralığı (\d+) saniye", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex PollRow();

    [GeneratedRegex(@"varsayılan yeniden denemesi ([\d, ]+ve \d+) dakika \(en çok (\w+) kez\); yanıtsız son sorgu (\d+) dakika; geri dönen adres sorgusu (\d+) dakika ve okuduğu pencere (\d+) saat", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RetryRow();

    [GeneratedRegex(@"zaman aşımından (\d+) dakika sonra, sonra her biri bir öncekinden ([\d, ]+ve \d+) dakika sonra, sonra saatte bir \(üst sınırı yok\)", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex RefundQueryRow();
}
