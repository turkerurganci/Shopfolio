namespace Shopfolio.Infrastructure;

/// <summary>The two ways a period set in days is counted (02 4.1.3). Only the code names live here; the behavior lives in <c>PeriodCounting</c>.</summary>
public enum DayCounting
{
    BusinessDay,
    CalendarDay,
}
