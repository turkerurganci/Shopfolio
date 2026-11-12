using System.Net;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Ordering;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Panel;
using Shopfolio.UnitTests.Fakes;

namespace Shopfolio.IntegrationTests.Administration;

/// <summary>
/// E-51 over HTTP on the opened application with the clock at <see cref="Now"/> (T524 KK1-KK4; 07 UC-105; 04 9.22): the default period, the ready periods' dates, the custom range and its three refusals
/// with a 200 and no summary, a period outside the list, the window that is not offered when sales never opened, the numbers of the period and the "Değerlendirilemez" of a zero denominator.
/// </summary>
public sealed partial class SalesSummaryTests
{
    private const string SummaryPath = "/panel/satis-ozeti";

    private static DateOnly Today => TurkeyTime.DateOf(Now);

    [Fact]
    public async Task WithNoParameter_ThePeriodIsTheLast30Days_AndEveryReadyPeriodHasItsDays_KK1()
    {
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, SummaryPath);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<option value=\"Last30Days\" selected", html, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.DateRange(Today.AddDays(-29), Today), html, StringComparison.Ordinal);
        (string Period, DateOnly Start, DateOnly End)[] periods =
        [
            ("Today", Today, Today),
            ("Yesterday", Today.AddDays(-1), Today.AddDays(-1)),
            ("Last7Days", Today.AddDays(-6), Today),
            ("ThisMonth", new DateOnly(Today.Year, Today.Month, 1), Today),
            ("LastMonth", new DateOnly(Today.Year, Today.Month, 1).AddMonths(-1), new DateOnly(Today.Year, Today.Month, 1).AddDays(-1)),
        ];
        foreach ((string period, DateOnly start, DateOnly end) in periods)
        {
            (_, string page) = await GetAsync(client, $"{SummaryPath}?period={period}");
            Assert.Contains($"id=\"summary-period\">{DisplayFormat.DateRange(start, end)}<", page, StringComparison.Ordinal);
        }

        (_, string custom) = await GetAsync(client, $"{SummaryPath}?period=Custom&startDate={Day.AddDays(-2):yyyy-MM-dd}&endDate={Day:yyyy-MM-dd}");
        Assert.Contains(DisplayFormat.DateRange(Day.AddDays(-2), Day), custom, StringComparison.Ordinal);
    }

    [Fact]
    public async Task APeriodOutsideTheList_AndTheWindowWhenSalesNeverOpened_AreIgnored_TheDefaultIsShown_KK1_KK3()
    {
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);

        (HttpStatusCode unknownStatus, string unknown) = await GetAsync(client, $"{SummaryPath}?period=Last90Days");
        (_, string window) = await GetAsync(client, $"{SummaryPath}?period=MeasurementWindow");

        Assert.Equal(HttpStatusCode.OK, unknownStatus);
        Assert.Contains(DisplayFormat.DateRange(Today.AddDays(-29), Today), unknown, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.DateRange(Today.AddDays(-29), Today), window, StringComparison.Ordinal);
        Assert.DoesNotContain("value=\"MeasurementWindow\"", window, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("", "0")]
    [InlineData("1", "")]
    [InlineData("1", "-1")]
    [InlineData("0", "2")]
    public async Task AWrongCustomRange_IsAFieldMessageWithA200_AndNoSummary_KK4(string startOffset, string endOffset)
    {
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);
        string start = startOffset.Length == 0 ? string.Empty : Today.AddDays(-int.Parse(startOffset, System.Globalization.CultureInfo.InvariantCulture)).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);
        string end = endOffset.Length == 0 ? string.Empty : Today.AddDays(-int.Parse(endOffset, System.Globalization.CultureInfo.InvariantCulture)).ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture);

        (HttpStatusCode status, string html) = await GetAsync(client, $"{SummaryPath}?period=Custom&startDate={start}&endDate={end}");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("alanda hata var", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"summary-order-count\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"summary-period\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheNumbers_AreTheApprovedOrdersOfTheirBirthDay_AndAZeroDenominatorIsNotAssessable_KK2_KK3()
    {
        Guid paid = await SaveOrderAsync(DayStart.AddHours(2), PaymentMethod.Card, null, null, (_shirt, 2));
        Guid failed = await SaveOrderAsync(DayStart.AddHours(3), PaymentMethod.Card, null, null, (_shirt, 1));
        await SetAxesAsync(paid, OrderStatus.Placed, PaymentStatus.Paid);
        await SetAxesAsync(failed, OrderStatus.Cancelled, PaymentStatus.Failed);
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);
        string oneDay = $"{SummaryPath}?period=Custom&startDate={Day:yyyy-MM-dd}&endDate={Day:yyyy-MM-dd}";
        string nextDay = $"{SummaryPath}?period=Custom&startDate={Day.AddDays(1):yyyy-MM-dd}&endDate={Day.AddDays(1):yyyy-MM-dd}";

        (_, string born) = await GetAsync(client, oneDay);
        (_, string after) = await GetAsync(client, nextDay);

        Assert.Contains("id=\"summary-order-count\"><h3 class=\"summary-card__name\">Sipariş sayısı</h3><p class=\"summary-card__value\">1<", Squeeze(born), StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Amount(600m), born, StringComparison.Ordinal);
        Assert.Contains(SalesSummaryModel.RevenueNote, born, StringComparison.Ordinal);
        Assert.Contains("Kart", born, StringComparison.Ordinal);
        Assert.Contains("id=\"summary-order-count\"><h3 class=\"summary-card__name\">Sipariş sayısı</h3><p class=\"summary-card__value\">0<", Squeeze(after), StringComparison.Ordinal);
        Assert.Contains($"{SalesSummaryModel.NotAssessable} — bu dönemde ödemesi tamamlanmış sipariş yok.", after, StringComparison.Ordinal);
        Assert.Contains(SalesSummaryModel.NoTopSellerText, after, StringComparison.Ordinal);
        Assert.Contains(SalesSummaryModel.NoPaymentText, after, StringComparison.Ordinal);
    }

    private static string Squeeze(string html) => System.Text.RegularExpressions.Regex.Replace(html, @">\s+<", "><", System.Text.RegularExpressions.RegexOptions.None, TimeSpan.FromSeconds(5));

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private WebApplicationFactory<Program> OpenPages() =>
        new ShopfolioWebApplicationFactory(_collection.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly);
            s.RemoveAll<TimeProvider>();
            s.AddSingleton<TimeProvider>(new FixedTime(Now));
        }));

    private async Task<HttpClient> PanelClientAsync(WebApplicationFactory<Program> factory)
    {
        (string key, _) = await TestSessions.AdminAsync(_collection.Dsn, lastActivity: Now);
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={key}");
        return client;
    }
}
