using System.Globalization;
using System.Text.RegularExpressions;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Modules.Administration;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.UnitTests.Support;

namespace Shopfolio.UnitTests.Pages;

/// <summary>
/// The counters, the period indicator, the threshold message and the concurrent edit controls (T542 KK5-KK9; 04 2.6, 2.10): the six counters' names come from <c>02 10.6.1</c> itself; the period
/// texts are checked on fixed instants (the standing is computed from the record's dates and the instant the caller read, so nothing here reads a clock - 09 7.1).
/// </summary>
public sealed partial class IndicatorComponentTests
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");

    private static readonly DateTimeOffset Now = new(2026, 11, 11, 10, 30, 0, TimeSpan.FromHours(3));

    private static PeriodIndicator Indicator(CountedPeriod period, DateOnly lastDay, bool warns = false) =>
        new("Süre", period, PeriodStanding.Of(period, lastDay, Now, BusinessCalendar.Product), warns);

    private static IReadOnlyList<PanelCounter> CountersWith(int count) =>
        PanelHomeRules.Counters(Enum.GetValues<PanelCounterKind>().ToDictionary(k => k, _ => count));

    // ---- KK6: six counters ----

    [Fact]
    public void TheCounterNames_AreTheSixOf_02_10_6_1_InItsOrder_KK6()
    {
        string line = File.ReadAllText(Path.Combine(RepositoryRoot.Find(), "Docs", "02_PRODUCT_REQUIREMENTS.md")).ReplaceLineEndings("\n")
            .Split('\n').Single(l => l.StartsWith("**10.6.1 ", StringComparison.Ordinal));
        string[] documented = [.. BoldSegment().Matches(line).Select(m => m.Groups[1].Value).Skip(1).Take(6)];

        Assert.Equal(6, documented.Length);
        string[] shown = [.. Enum.GetValues<PanelCounterKind>().Select(PendingCounters.NameOf)];
        for (int i = 0; i < 6; i++)
        {
            Assert.True(string.Compare(documented[i], shown[i], Turkish, CompareOptions.IgnoreCase) == 0, $"Document says '{documented[i]}', the screen says '{shown[i]}'.");
        }
    }

    [Fact]
    public void TheCounters_AreSix_AZeroCounterIsDrawnWithItsZero_AndEveryOneLeadsToItsOwnList_KK6()
    {
        IReadOnlyList<PendingCounterItem> counters = PendingCounters.Of(CountersWith(0));

        Assert.Equal(6, counters.Count);
        Assert.All(counters, c => Assert.Equal(0, c.Count));
        Assert.Equal(6, counters.Select(c => c.ListPath).Distinct().Count());
        Assert.Equal("/panel/siparisler?pendingWork=AwaitingPayment", counters[0].ListPath);
        Assert.Equal("/panel/siparisler?pendingWork=AwaitingShipment", counters[1].ListPath);
        Assert.Equal("/panel/siparisler?pendingWork=AwaitingDeliveryMark", counters[2].ListPath);
        Assert.Equal("/panel/siparisler?pendingWork=AwaitingServiceCompletion", counters[3].ListPath);
        Assert.Equal("/panel/talepler?requestStatus=Open", counters[4].ListPath);
        Assert.Equal("/panel/siparisler?pendingWork=AwaitingReturnOrRefund", counters[5].ListPath);
    }

    [Fact]
    public void ASeventhCounter_OrAMissingOne_IsRefused_KK6()
    {
        IReadOnlyList<PanelCounter> six = CountersWith(1);

        Assert.Throws<ArgumentException>(() => PendingCounters.Of([.. six, six[0]]));
        Assert.Throws<ArgumentException>(() => PendingCounters.Of([.. six.Take(5)]));
        Assert.Throws<ArgumentException>(() => PendingCounters.Of([.. six.Take(5), six[0]]));
    }

    [Fact]
    public void TheIbanListIsNotACounter_ItHoldsLinesAndShowsTheFirstTen_KK6()
    {
        Assert.DoesNotContain(Enum.GetNames<PanelCounterKind>(), n => n.Contains("Iban", StringComparison.OrdinalIgnoreCase));
        AwaitingCustomerRow[] rows =
        [
            .. Enumerable.Range(1, 12).Select(i => new AwaitingCustomerRow($"S-{i:00}", $"/panel/siparisler/{i}", "IBAN bekleniyor", null)),
        ];

        AwaitingCustomerList list = AwaitingCustomerList.Of(rows);

        Assert.Equal(AwaitingCustomerList.ShownLines, list.Shown.Count);
        Assert.True(list.HasMore);
        Assert.Equal("/panel/siparisler?pendingWork=IbanPendingMark", AwaitingCustomerList.AllPath);
        Assert.False(AwaitingCustomerList.Of(rows.Take(10)).HasMore);
        Assert.Empty(AwaitingCustomerList.Of([]).Shown);
    }

    [Fact]
    public void TheIbanList_PutsTheLineThatRunsOutSoonestFirst_AndTheLinesWithoutAPeriodLast_KK6()
    {
        AwaitingCustomerRow later = new("S-1", "/panel/siparisler/1", "IBAN bekleniyor", Indicator(CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 20)));
        AwaitingCustomerRow soon = new("S-2", "/panel/siparisler/2", "IBAN bekleniyor", Indicator(CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 13)));
        AwaitingCustomerRow over = new("S-3", "/panel/siparisler/3", "IBAN bekleniyor", Indicator(CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 9)));
        AwaitingCustomerRow none = new("S-0", "/panel/siparisler/0", "IBAN bekleniyor", null);

        AwaitingCustomerList list = AwaitingCustomerList.Of([none, later, soon, over]);

        Assert.Equal(["S-3", "S-2", "S-1", "S-0"], list.Shown.Select(r => r.OrderNumber).ToArray());
    }

    // ---- KK7: days, date, business days, overrun ----

    [Fact]
    public void ARunningPeriod_SaysTheDaysLeftAndTheLastDaysDate_NoClockTime_KK7()
    {
        PeriodIndicator indicator = Indicator(CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 14));

        Assert.Equal("3 gün kaldı · son gün 14 Kasım 2026", indicator.Text);
        Assert.False(indicator.IsOver);
        Assert.DoesNotMatch(new Regex(@"\d{1,2}:\d{2}", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5)), indicator.Text);
    }

    [Fact]
    public void APeriodCountedInBusinessDays_WritesIsGunu_AndACalendarPeriodWritesGun_KK7()
    {
        // 11 November 2026 is a Wednesday: Thursday and Friday are two business days; the weekend does not count.
        Assert.Equal("2 iş günü kaldı · son gün 13 Kasım 2026", Indicator(CountedPeriod.ShippingPeriod, new DateOnly(2026, 11, 13)).Text);
        Assert.Equal("2 gün kaldı · son gün 13 Kasım 2026", Indicator(CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 13)).Text);
        Assert.Equal("3 iş günü kaldı · son gün 16 Kasım 2026", Indicator(CountedPeriod.BankTransferPaymentWindow, new DateOnly(2026, 11, 16)).Text);
    }

    [Fact]
    public void OnTheLastDay_TheTextSaysSo_AndAPeriodThatHasRunOutSaysTheDaysPassedAndTheOverrunInWords_KK7()
    {
        Assert.Equal("Son gün bugün · 11 Kasım 2026", Indicator(CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 11)).Text);

        PeriodIndicator over = Indicator(CountedPeriod.ShippingPeriod, new DateOnly(2026, 11, 9));
        Assert.True(over.IsOver);
        Assert.Equal("Süre aşıldı: 2 gün geçti · son gün 9 Kasım 2026", over.Text);
        Assert.Contains(PeriodIndicator.OverrunWord, over.Text, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLegalInterestMessage_ShowsOnlyWhenAWarningPeriodHasRunOut_KK7()
    {
        Assert.True(Indicator(CountedPeriod.ShippingPeriod, new DateOnly(2026, 11, 9), warns: true).ShowsLegalInterest);
        Assert.False(Indicator(CountedPeriod.ShippingPeriod, new DateOnly(2026, 11, 13), warns: true).ShowsLegalInterest);
        Assert.False(Indicator(CountedPeriod.RefundPeriod, new DateOnly(2026, 11, 9)).ShowsLegalInterest);
    }

    // ---- KK8: threshold messages ----

    [Theory]
    [InlineData(80, "Ücretsiz kargoya 80 TL kaldı")]
    [InlineData(0, "Kargo ücretsiz")]
    public void TheFreeShippingLine_IsTheSentenceOfTheShippingAssessment_KK8(int remaining, string text)
    {
        ShippingAssessment assessment = new(420m, true, remaining == 0 ? 0m : 29.90m, remaining == 0, remaining, null);

        ThresholdMessage? message = ThresholdMessages.FreeShipping(assessment);

        Assert.NotNull(message);
        Assert.Equal(ThresholdKind.FreeShipping, message.Kind);
        Assert.Equal(text, message.Text);
        Assert.Null(ThresholdMessages.MinimumOrder(assessment));
    }

    [Fact]
    public void TheMinimumOrderLine_NamesTheMissingAmount_AndABasketWithoutConditionHasNoMessageAtAll_KK8()
    {
        ShippingAssessment under = new(120m, true, 29.90m, false, null, 130m);
        ShippingAssessment none = new(900m, false, 0m, false, null, null);

        ThresholdMessage? minimum = ThresholdMessages.MinimumOrder(under);
        Assert.NotNull(minimum);
        Assert.Equal(ThresholdKind.MinimumOrder, minimum.Kind);
        Assert.Contains("130 TL", minimum.Text, StringComparison.Ordinal);
        Assert.Null(ThresholdMessages.FreeShipping(none));
        Assert.Null(ThresholdMessages.MinimumOrder(none));
    }

    // ---- KK9: concurrent edit controls ----

    [Fact]
    public void ASectionedScreen_ShowsTheConflictOnTheSavedSectionOnly_KK9()
    {
        string saved = System.Net.WebUtility.HtmlDecode(VersionFields.Render(ConflictControls.SectionSaveControl("Legal", "Legal", "/panel/yasal-metinler/gizlilik")));
        string other = System.Net.WebUtility.HtmlDecode(VersionFields.Render(ConflictControls.SectionSaveControl("Notice", "Legal", "/panel/yasal-metinler/gizlilik")));
        string clean = System.Net.WebUtility.HtmlDecode(VersionFields.Render(ConflictControls.SectionSaveControl("Legal", null, "/panel/yasal-metinler/gizlilik")));

        Assert.Contains(ConflictControls.RecordMessage, saved, StringComparison.Ordinal);
        Assert.Contains(ConflictControls.OpenCurrentLabel, saved, StringComparison.Ordinal);
        Assert.Contains(ConflictControls.OverwriteLabel, saved, StringComparison.Ordinal);
        Assert.DoesNotContain(ConflictControls.RecordMessage, other, StringComparison.Ordinal);
        Assert.Contains(ConflictControls.SaveLabel, other, StringComparison.Ordinal);
        Assert.DoesNotContain(ConflictControls.RecordMessage, clean, StringComparison.Ordinal);
    }

    [Fact]
    public void ASectionKeepsItsOwnSaveLabel_WhenThereIsNoConflict_KK9()
    {
        string html = VersionFields.Render(ConflictControls.SectionSaveControl("Notice", null, "/panel/yasal-metinler/gizlilik", "Bildirimi kaydet"));

        Assert.Contains("Bildirimi kaydet", html, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => ConflictControls.SectionSaveControl("", null, "/panel/x"));
    }

    [Fact]
    public void TheOrderVariant_OffersNoWayToOverwrite_KK9()
    {
        string html = System.Net.WebUtility.HtmlDecode(VersionFields.Render(ConflictControls.OrderNotice(ConflictControls.OrderMessage)));

        Assert.Contains(ConflictControls.OrderMessage, html, StringComparison.Ordinal);
        Assert.DoesNotContain("button", html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(ConflictControls.OverwriteLabel, html, StringComparison.Ordinal);
    }

    [GeneratedRegex(@"\*\*(.+?)\*\*", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex BoldSegment();
}
