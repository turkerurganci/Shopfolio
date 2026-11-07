using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// One time zone and one Turkey time helper (T378 criteria 1, 9; KK1, KK9; 02 4.1.1; 09 7.1 items 1, 3): Turkey time is a fixed offset with
/// no daylight saving change and no zone choice, and every day and period count goes through <c>TurkeyTime</c>. The source scan names the
/// only file that may hold the Turkey offset; the server-zone reads are the architecture test <c>TimeAndMoneyRulesTests</c>'.
/// </summary>
public sealed partial class SingleTimeZoneTests
{
    private const string TurkeyTimeFile = "TurkeyTime.cs";

    [Fact]
    public void StartOf_EveryDayOfTheYearsAroundTheEuropeanChanges_IsExactlyTwentyFourHoursApart()
    {
        // European daylight saving changes fall on the last Sundays of March and October; Turkey has none (02 4.1.1).
        for (DateOnly day = new(2026, 1, 1); day < new DateOnly(2027, 12, 31); day = day.AddDays(1))
        {
            Assert.Equal(TimeSpan.FromHours(24), TurkeyTime.StartOf(day.AddDays(1)) - TurkeyTime.StartOf(day));
            Assert.Equal(TimeSpan.FromHours(21), TurkeyTime.StartOf(day).TimeOfDay);
        }
    }

    [Fact]
    public void TurkeyTime_HasNoZoneChoice_NoPublicMemberTakesOrReturnsATimeZone()
    {
        System.Reflection.MemberInfo[] members = [.. typeof(TurkeyTime).GetMembers(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.DeclaredOnly)];

        Assert.DoesNotContain(members, m => m.ToString()!.Contains("TimeZone", StringComparison.Ordinal));
        Assert.Equal(TimeSpan.FromHours(3), TurkeyTime.Offset);
    }

    [Fact]
    public void Source_OnlyTurkeyTimeDefinesTheOffsetAndNothingReadsAZone()
    {
        string root = Path.Combine(RepositoryRoot.Find(), "src");
        List<string> findings = [];
        foreach (string file in Directory.EnumerateFiles(root, "*.cs", SearchOption.AllDirectories))
        {
            if (file.Contains($"{Path.DirectorySeparatorChar}Migrations{Path.DirectorySeparatorChar}", StringComparison.Ordinal))
            {
                continue;
            }

            string code = CommentLine().Replace(File.ReadAllText(file), string.Empty);
            if (Path.GetFileName(file) != TurkeyTimeFile && TurkeyOffsetDefinition().IsMatch(code))
            {
                findings.Add($"{Path.GetFileName(file)} writes a Turkey offset of its own");
            }

            if (TimeZoneName().IsMatch(code))
            {
                findings.Add($"{Path.GetFileName(file)} names a time zone");
            }
        }

        Assert.Empty(findings);
    }

    [Fact]
    public void Source_PeriodCountingFiles_CountThroughTurkeyTimeAndComparesNoClockTime()
    {
        string folder = Path.Combine(RepositoryRoot.Find(), "src", "Shopfolio.Web", "Infrastructure");
        foreach (string name in new[] { "PeriodCounting.cs", "PeriodFences.cs", "BusinessCalendar.cs" })
        {
            string code = CommentLine().Replace(File.ReadAllText(Path.Combine(folder, name)), string.Empty);

            Assert.DoesNotMatch(@"ToOffset\(|new DateTimeOffset\(|TimeSpan\.FromHours\(3\)|23:59|\.Hour\b|\.Minute\b", code);
        }

        string counting = File.ReadAllText(Path.Combine(folder, "PeriodCounting.cs"));
        Assert.Contains("TurkeyTime.StartOf(", counting, StringComparison.Ordinal);
        Assert.Contains("TurkeyTime.DateOf(", counting, StringComparison.Ordinal);
    }

    [GeneratedRegex(@"^\s*//.*$", RegexOptions.Multiline)]
    private static partial Regex CommentLine();

    [GeneratedRegex(@"TimeSpan\.FromHours\(\s*3\s*\)|new TimeSpan\(\s*3\s*,\s*0\s*,\s*0\s*\)")]
    private static partial Regex TurkeyOffsetDefinition();

    [GeneratedRegex(@"TimeZoneInfo|Europe/Istanbul|Turkey Standard Time|FindSystemTimeZoneById")]
    private static partial Regex TimeZoneName();
}
