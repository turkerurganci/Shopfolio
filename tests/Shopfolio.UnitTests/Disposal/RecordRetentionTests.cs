using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Modules.AfterSales;
using Shopfolio.Modules.Notifications;

namespace Shopfolio.UnitTests.Disposal;

/// <summary>The edges of the record periods of the nightly disposal (T302; 06 9.1 Z-29, Z-34, Z-43, Z-44): calendar years in Turkey time, one edge function.</summary>
public sealed class RecordRetentionTests
{
    private static readonly DateTimeOffset Now = new(2026, 11, 7, 22, 30, 0, TimeSpan.Zero);

    [Fact]
    public void YearsBefore_CountsCalendarYearsInTurkeyTime()
    {
        // 22:30 UTC is 01:30 on the 8th in Turkey; three years before that is 01:30 on 2023-11-08 Turkey time.
        DateTimeOffset edge = RetentionCutoff.YearsBefore(Now, 3);

        Assert.Equal(new DateTimeOffset(2023, 11, 7, 22, 30, 0, TimeSpan.Zero), edge);
        Assert.Equal(TimeSpan.Zero, edge.Offset);
    }

    [Fact]
    public void YearsBefore_OnALeapDay_FallsOnTheLastDayOfFebruary()
    {
        DateTimeOffset leapDay = new(2024, 2, 29, 9, 0, 0, TimeSpan.Zero);

        Assert.Equal(new DateTimeOffset(2023, 2, 28, 9, 0, 0, TimeSpan.Zero), RetentionCutoff.YearsBefore(leapDay, 1));
    }

    [Fact]
    public void YearsBefore_ZeroYears_IsTheMoment()
    {
        Assert.Equal(Now, RetentionCutoff.YearsBefore(Now, 0));
    }

    [Fact]
    public void YearsBefore_NegativeYears_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => RetentionCutoff.YearsBefore(Now, -1));
    }

    [Fact]
    public void DisposalRecordPeriod_IsThreeYears()
    {
        Assert.Equal(RetentionCutoff.YearsBefore(Now, 3), DisposalRecordPeriod.Cutoff(Now));
    }

    [Fact]
    public void DispatchRecordRetention_OrderLinkedIsTenYears_AndTheUnpaidPersonalDataThree()
    {
        Assert.Equal(RetentionCutoff.YearsBefore(Now, 10), DispatchRecordRetention.OrderLinkedBefore(Now));
        Assert.Equal(RetentionCutoff.YearsBefore(Now, 3), DispatchRecordRetention.UnpaidPersonalDataBefore(Now));
        Assert.Equal(RetentionCutoff.YearsBefore(Now, ProductConstants.DispatchRecordRetentionYears), DispatchRecordRetention.UnboundBefore(Now));
    }

    [Fact]
    public void DefectClaimLastChange_IsTheLastHistoryEntry_OrTheOpeningWhenThereIsNone()
    {
        DateTimeOffset opened = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);
        string history = """[{"At":"2016-05-05T10:00:00+00:00"},{"At":"2018-06-06T11:00:00+00:00","Other":1}]""";

        Assert.Equal(new DateTimeOffset(2018, 6, 6, 11, 0, 0, TimeSpan.Zero), DefectClaimRetention.LastChange(opened, history));
        Assert.Equal(opened, DefectClaimRetention.LastChange(opened, "[]"));
    }

    [Fact]
    public void DefectClaimBefore_IsTenYears()
    {
        Assert.Equal(RetentionCutoff.YearsBefore(Now, 10), DefectClaimRetention.Before(Now));
    }
}
