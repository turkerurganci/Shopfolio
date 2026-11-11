using Shopfolio.Infrastructure;

namespace Shopfolio.Pages.Components;

/// <summary>
/// A running period as a screen shows it (T542; 04 2.6.3, 2.6.4, 2.6.6; K-767): the label of the period, and where it stands (<see cref="PeriodStanding"/> - a value computed from the record's own
/// dates at the moment of the read). The text is the days left and the last day's date; a period counted in business days says "iş günü" (02 4.1); a period that has run out says how many days
/// have passed and says the overrun in words, never by colour alone. There is no live countdown and no hour, minute or second: nothing here changes while the page is open.
/// </summary>
/// <param name="Label">What the period is ("Kargoya verme sözü", "Geri ödeme süresi"): the period's name on the screen.</param>
/// <param name="Period">The period's kind; it decides the counting unit.</param>
/// <param name="Standing">Where the period stands now.</param>
/// <param name="WarnsLegalInterest">True for the periods whose overrun carries the legal interest warning on the panel (03 8.2.4; 02 7.2.3): the shipping promise and the service fulfilment period.</param>
internal sealed record PeriodIndicator(string Label, CountedPeriod Period, PeriodStanding Standing, bool WarnsLegalInterest = false)
{
    /// <summary>The overrun in words (04 2.6.6): a period that has run out is "aşıldı", not only red.</summary>
    public const string OverrunWord = "Süre aşıldı";

    /// <summary>The panel's lasting message for an overrun of the shipping promise or the service period (04 2.6.3; 03 8.2.4).</summary>
    public const string LegalInterestMessage = "Kanuni faiz yükümlülüğü doğar.";

    /// <summary>Whether the period has run out.</summary>
    public bool IsOver => Standing.IsOver;

    /// <summary>Whether the legal interest message shows: the period warns and has run out.</summary>
    public bool ShowsLegalInterest => WarnsLegalInterest && IsOver;

    /// <summary>The indicator's text: "3 gün kaldı · son gün 14 Kasım 2026", "Son gün bugün · 14 Kasım 2026", "Süre aşıldı: 2 gün geçti · son gün 14 Kasım 2026".</summary>
    public string Text
    {
        get
        {
            string lastDay = DisplayFormat.Date(Standing.LastDay);
            if (Standing.IsOver)
            {
                return $"{OverrunWord}: {DisplayFormat.Days(Standing.DaysOver)} geçti · son gün {lastDay}";
            }

            if (Standing.DaysLeft == 0)
            {
                return $"Son gün bugün · {lastDay}";
            }

            string left = PeriodCounting.CountingOf(Period) == DayCounting.BusinessDay
                ? DisplayFormat.BusinessDays(Standing.DaysLeft)
                : DisplayFormat.Days(Standing.DaysLeft);
            return $"{left} kaldı · son gün {lastDay}";
        }
    }
}
