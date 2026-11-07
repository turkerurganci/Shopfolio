using System.Globalization;
using System.Reflection;

namespace Shopfolio.Infrastructure;

/// <summary>
/// The business day calendar (05 7.2.3; 02 4.1.2; 09 7.1 item 3): a business day is Monday to Friday except a public holiday. The holidays
/// come from <c>PublicHolidays.txt</c>, a data file that ships inside the application (an embedded resource); it is not in the database and
/// the panel does not edit it. In a year the file does not cover, only the weekend is excluded. The calendar works on <see cref="DateOnly"/>
/// Turkey days; turning a day into an instant is <see cref="TurkeyTime.StartOf"/>.
/// </summary>
internal sealed class BusinessCalendar
{
    /// <summary>The resource name of the product's holiday file.</summary>
    public const string ResourceName = "Shopfolio.Infrastructure.PublicHolidays.txt";

    private static readonly Lazy<BusinessCalendar> _product = new(LoadProductFile);

    private readonly HashSet<DateOnly> _holidays;

    private readonly HashSet<int> _coveredYears;

    private BusinessCalendar(IReadOnlyList<DateOnly> holidays)
    {
        _holidays = [.. holidays];
        _coveredYears = [.. holidays.Select(h => h.Year)];
        Holidays = holidays;
    }

    /// <summary>The calendar of the holiday file that ships with the product.</summary>
    public static BusinessCalendar Product => _product.Value;

    /// <summary>The holidays of the calendar in date order.</summary>
    public IReadOnlyList<DateOnly> Holidays { get; }

    /// <summary>Whether the holiday list covers <paramref name="year"/>; an uncovered year has no holiday.</summary>
    public bool Covers(int year) => _coveredYears.Contains(year);

    public bool IsHoliday(DateOnly day) => _holidays.Contains(day);

    public bool IsBusinessDay(DateOnly day) => day.DayOfWeek is not (DayOfWeek.Saturday or DayOfWeek.Sunday) && !_holidays.Contains(day);

    /// <summary>The first business day after <paramref name="day"/>; the day itself is never the answer.</summary>
    public DateOnly NextBusinessDay(DateOnly day)
    {
        DateOnly next = day.AddDays(1);
        while (!IsBusinessDay(next))
        {
            next = next.AddDays(1);
        }

        return next;
    }

    /// <summary>
    /// The last day of a period set in business days (02 4.1.4): the start day is not counted, the period begins on the next day, and the
    /// <paramref name="businessDays"/>-th business day after <paramref name="startDay"/> is its last day. The same rule as a period in calendar
    /// days (<see cref="TurkeyTime.LastDayOfPeriod"/>) with weekends and holidays left out of the count.
    /// </summary>
    public DateOnly LastDayOfBusinessPeriod(DateOnly startDay, int businessDays)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(businessDays, 1);
        DateOnly day = startDay;
        for (int counted = 0; counted < businessDays; counted++)
        {
            day = NextBusinessDay(day);
        }

        return day;
    }

    /// <summary>
    /// Reads a holiday list: one holiday a line as <c>yyyy-MM-dd</c> and its name; <c>#</c> lines and blank lines are skipped. A day that
    /// cannot be read, a day written twice and a day out of date order are format errors that name the line.
    /// </summary>
    public static BusinessCalendar Parse(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        List<DateOnly> holidays = [];
        string[] lines = text.ReplaceLineEndings("\n").Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.Length == 0 || line.StartsWith('#'))
            {
                continue;
            }

            string dayText = line.Split(' ', 2)[0];
            if (!DateOnly.TryParseExact(dayText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly day))
            {
                throw new FormatException($"Holiday list line {i + 1}: the day is not yyyy-MM-dd.");
            }

            if (holidays.Count > 0 && day <= holidays[^1])
            {
                throw new FormatException($"Holiday list line {i + 1}: the day is written twice or out of date order.");
            }

            holidays.Add(day);
        }

        return new BusinessCalendar(holidays);
    }

    private static BusinessCalendar LoadProductFile()
    {
        using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"The holiday file {ResourceName} is not in the application.");
        using StreamReader reader = new(stream);
        return Parse(reader.ReadToEnd());
    }
}
