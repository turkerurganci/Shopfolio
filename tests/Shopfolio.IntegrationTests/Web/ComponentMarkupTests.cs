using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;

using Shopfolio.Infrastructure.Forms;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The shared components of T542 drawn on the opened application (04 2.5, 2.6, 2.10; KK1-KK9): the markup the panel and the storefront layouts carry for badges, item records, panel marks,
/// the undelivered e-mail block, the six counters, the IBAN list, the period indicators, the threshold message and the conflict controls. The two sample pages draw fixed values; every text
/// asserted here is read from the component's own model, so the screen and the document cannot drift apart.
/// </summary>
public sealed partial class StorefrontFrameTests
{
    private const string ComponentPage = "/panel/siparisler/bilesen-ornegi";

    private async Task<string> ComponentHtmlAsync(string path = ComponentPage)
    {
        (string adminKey, _) = await TestSessions.AdminAsync(_database.Dsn);
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();
        (HttpStatusCode status, string html, _) = await GetHtmlAsync(client, path, Admin(adminKey));
        Assert.Equal(HttpStatusCode.OK, status);
        return html;
    }

    private static string Section(string html, string id)
    {
        Match match = Regex.Match(html, "<section id=\"" + id + "\">(?<body>.*?)</section>\\s*(?=<section id=|<form id=|<div id=)", RegexOptions.CultureInvariant | RegexOptions.Singleline, PatternTimeout);
        Assert.True(match.Success, $"The page has no sample section '{id}'.");
        return match.Groups["body"].Value;
    }

    private static int Count(string html, string needle) => Regex.Count(html, Regex.Escape(needle), RegexOptions.CultureInvariant, PatternTimeout);

    [Fact]
    public async Task TheOrderShowsTwoSeparateBadgesSideBySide_EachWithText_T542_KK1()
    {
        string badges = Section(await ComponentHtmlAsync(), "badges");

        Assert.Equal(2, Count(badges, "<span class=\"badge badge--"));
        Assert.Contains(StatusBadges.For(OrderStatus.Shipped).Text, badges, StringComparison.Ordinal);
        Assert.Contains(StatusBadges.For(PaymentStatus.Paid).Text, badges, StringComparison.Ordinal);
        Assert.True(badges.IndexOf(StatusBadges.For(OrderStatus.Shipped).Text, StringComparison.Ordinal) < badges.IndexOf(StatusBadges.For(PaymentStatus.Paid).Text, StringComparison.Ordinal));
    }

    [Fact]
    public async Task AnItemRecord_ShowsUnderItsOwnName_OnTheItemsRow_WithoutABadge_T542_KK3()
    {
        string records = Section(await ComponentHtmlAsync(), "item-records");

        Assert.Contains(ItemRecordLabels.Of(ItemRecordName.Withdrawal), records, StringComparison.Ordinal);
        Assert.Contains(ItemRecordLabels.Of(ItemRecordName.DeliveryMark), records, StringComparison.Ordinal);
        Assert.Contains("2 adet", records, StringComparison.Ordinal);
        Assert.DoesNotContain("badge", records, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePanelMarksOnThePage_AreTheOnesWhoseConditionHolds_EachAsText_T542_KK4()
    {
        string marks = Section(await ComponentHtmlAsync(), "marks");

        Assert.Equal(2, Count(marks, "class=\"panel-marks__item\""));
        Assert.Contains(PanelMarks.TextOf(PanelMark.EmailNotDelivered), marks, StringComparison.Ordinal);
        Assert.Contains(PanelMarks.TextOf(PanelMark.RefundNotCompleted), marks, StringComparison.Ordinal);
        Assert.DoesNotContain(PanelMarks.TextOf(PanelMark.IbanPending), marks, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheUndeliveredEmailBlock_HasAResendFormWithTheTokenAndHandlerPerResendableRow_AndNoneOnTheInvitation_T542_KK5()
    {
        string emails = Section(await ComponentHtmlAsync(), "emails");

        Assert.Equal(3, Count(emails, "class=\"undelivered-emails__row\""));
        Assert.Equal(2, Count(emails, "?handler=ResendEmail\""));
        Assert.Equal(1, Count(emails, "?handler=ResendAllEmails\""));
        Assert.Equal(3, Count(emails, "name=\"__RequestVerificationToken\""));
        Assert.Equal(3, Count(emails, "data-js=\"resend-once\""));
        Assert.Equal(2, Count(emails, ">" + UndeliveredEmailList.ResendLabel + "<"));
        Assert.Equal(1, Count(emails, ">" + UndeliveredEmailList.ResendAllLabel + "<"));
        Assert.Contains("Yönetici daveti", emails, StringComparison.Ordinal);

        string invitationRow = Regex.Match(emails, "<li class=\"undelivered-emails__row\">(?:(?!</li>).)*Yönetici daveti(?:(?!</li>).)*</li>", RegexOptions.CultureInvariant | RegexOptions.Singleline, PatternTimeout).Value;
        Assert.NotEmpty(invitationRow);
        Assert.DoesNotContain("<form", invitationRow, StringComparison.Ordinal);
        Assert.DoesNotContain("<button", invitationRow, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheHomeCounters_AreSix_ASeventhIsNotDrawn_AZeroCounterSaysZero_AndEachLeadsToItsList_T542_KK6()
    {
        string counters = Section(await ComponentHtmlAsync(), "counters");

        Assert.Equal(6, Count(counters, "class=\"pending-counters__item\""));
        Assert.Contains("<span class=\"pending-counters__count\">0</span>", counters, StringComparison.Ordinal);
        foreach (string pendingWork in new[] { "AwaitingPayment", "AwaitingShipment", "AwaitingDeliveryMark", "AwaitingServiceCompletion", "AwaitingReturnOrRefund" })
        {
            Assert.Contains("?pendingWork=" + pendingWork, counters, StringComparison.Ordinal);
        }

        Assert.Contains("href=\"/panel/talepler?requestStatus=Open\"", counters, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheAwaitingCustomerList_IsLines_NotACounter_ShowsTenAndLeadsToTheFilteredList_T542_KK6()
    {
        string awaiting = Section(await ComponentHtmlAsync(), "awaiting");

        Assert.Equal(AwaitingCustomerList.ShownLines, Count(awaiting, "class=\"awaiting-customer__row\""));
        Assert.Contains("href=\"" + AwaitingCustomerList.AllPath + "\"", awaiting, StringComparison.Ordinal);
        Assert.DoesNotContain("pending-counters", awaiting, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePeriodIndicators_SayDaysAndTheLastDay_NamePeriodsInBusinessDays_AndSayTheOverrunInWords_T542_KK7()
    {
        string html = await ComponentHtmlAsync();
        string periods = Section(html, "periods");

        Assert.Equal(3, Regex.Count(periods, "<span class=\"period-indicator(?: period-indicator--over)?\">", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.Contains("iş günü", periods, StringComparison.Ordinal);
        Assert.Contains(PeriodIndicator.OverrunWord, periods, StringComparison.Ordinal);
        Assert.Contains(PeriodIndicator.LegalInterestMessage, periods, StringComparison.Ordinal);
        Assert.Equal(1, Count(periods, "period-indicator--over"));
        Assert.DoesNotContain("<script", periods, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex("\\b\\d+\\s*(saat|dakika|saniye)\\b", RegexOptions.CultureInvariant, PatternTimeout), periods);
    }

    [Theory]
    [InlineData("?left=80", "free-shipping", "Ücretsiz kargoya", "minimum-order")]
    [InlineData("?left=0", "free-shipping", "Kargo ücretsiz", "minimum-order")]
    [InlineData("?missing=130", "minimum-order", "daha ürün ekleyin", "free-shipping")]
    public async Task TheThresholdMessage_IsALastingLineInItsPlace_AndAbsentWithoutTheCondition_T542_KK8(string query, string shown, string text, string absent)
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (HttpStatusCode status, string html, _) = await GetHtmlAsync(client, "/odeme/esik-ornegi" + query);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<section id=\"" + shown + "\">", html, StringComparison.Ordinal);
        Assert.Contains(text, html, StringComparison.Ordinal);
        Assert.DoesNotContain("<section id=\"" + absent + "\">", html, StringComparison.Ordinal);
        Assert.DoesNotContain("data-js=\"announcement", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheThresholdMessage_IsNotDrawnWhenTheBasketHasNoCondition_T542_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = factory.CreateClient();

        (_, string html, _) = await GetHtmlAsync(client, "/odeme/esik-ornegi");

        Assert.DoesNotContain("threshold-message", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheConflictNotice_ShowsOnlyOnTheSectionThatWasSaved_WithTheLinkAndTheOverwriteButton_T542_KK9()
    {
        string html = await ComponentHtmlAsync(ComponentPage + "?conflict=Legal");

        string legal = Regex.Match(html, "<form id=\"legal-section\".*?</form>", RegexOptions.CultureInvariant | RegexOptions.Singleline, PatternTimeout).Value;
        string contact = Regex.Match(html, "<form id=\"contact-section\".*?</form>", RegexOptions.CultureInvariant | RegexOptions.Singleline, PatternTimeout).Value;
        Assert.Contains(ConflictControls.RecordMessage, legal, StringComparison.Ordinal);
        Assert.Contains(ConflictControls.OpenCurrentLabel, legal, StringComparison.Ordinal);
        Assert.Contains(ConflictControls.OverwriteLabel, legal, StringComparison.Ordinal);
        Assert.DoesNotContain(ConflictControls.RecordMessage, contact, StringComparison.Ordinal);
        Assert.DoesNotContain(ConflictControls.OverwriteLabel, contact, StringComparison.Ordinal);
        Assert.Contains("İletişimi kaydet", contact, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WithoutAConflict_EverySectionKeepsItsOwnSaveButton_AndTheOrderVariantNeverOffersTheOverwrite_T542_KK9()
    {
        string html = await ComponentHtmlAsync();

        Assert.Contains("Yasal metni kaydet", html, StringComparison.Ordinal);
        Assert.Contains("İletişimi kaydet", html, StringComparison.Ordinal);
        Assert.DoesNotContain(ConflictControls.RecordMessage, html, StringComparison.Ordinal);

        string order = Regex.Match(html, "<div id=\"order-notice\">.*?</div>", RegexOptions.CultureInvariant | RegexOptions.Singleline, PatternTimeout).Value;
        Assert.Contains(ConflictControls.OrderMessage, order, StringComparison.Ordinal);
        Assert.DoesNotContain(ConflictControls.OverwriteLabel, order, StringComparison.Ordinal);
        Assert.DoesNotContain("<button", order, StringComparison.Ordinal);
    }
}
