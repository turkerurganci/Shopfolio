using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Pages.Components;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The control components on the opened application (T544; 04 OB-16 to OB-19; 09 3.4, 16.1): the real shared parts drawn by a test-only sample page, read from the response. The rules of the small models are
/// in the unit tests; here the markup - the coupon row above the total, the two buttons of an applied code, the boxes unticked and directly above the button, the message of an unticked box, the gallery rows,
/// the closed save button while an upload goes on and the customer's form without a file input - is read.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class ControlComponentTests
{
    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private readonly CollectionDatabaseFixture _database;

    public ControlComponentTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ControlSampleModel).Assembly)));

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string query = "")
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(ControlSampleModel.Path + query, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<(HttpStatusCode Status, string Html)> PostConfirmAsync(HttpClient client, params (string Name, string Value)[] fields)
    {
        using FormUrlEncodedContent content = new(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        using HttpResponseMessage response = await client.PostAsync(new Uri(ControlSampleModel.Path + "?handler=Confirm", UriKind.Relative), content, Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static string Region(string html, string id)
    {
        int start = html.IndexOf($"<section id=\"{id}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, $"No region '{id}' in the response.");
        int end = html.IndexOf("</section>", start, StringComparison.Ordinal);
        return html[start..end];
    }

    // ---- KK4: the coupon row is closed and stands above the total ----

    [Fact]
    public async Task TheCouponField_IsAClosedRowAboveTheTotal_AndOpensWithoutAScript_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await GetAsync(client);
        string summary = Region(html, "summary");

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<summary>Kupon kodum var</summary>", summary, StringComparison.Ordinal);
        Assert.DoesNotContain("<details class=\"coupon__row\" open", summary, StringComparison.Ordinal);
        Assert.True(summary.IndexOf("id=\"coupon-field\"", StringComparison.Ordinal) < summary.IndexOf("id=\"total\"", StringComparison.Ordinal));
        Assert.True(summary.IndexOf("id=\"lines\"", StringComparison.Ordinal) < summary.IndexOf("id=\"coupon-field\"", StringComparison.Ordinal));
        Assert.Contains("handler=ApplyCoupon", summary, StringComparison.Ordinal);
    }

    // ---- KK5: name, amount, two buttons, one code ----

    [Fact]
    public async Task AnAppliedCoupon_ShowsItsNameAndAmount_WithChangeAndRemove_AndOffersNoSecondCodeField_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client, "?coupon=applied");
        string coupon = Region(html, "summary");

        Assert.Contains("YAZ10", coupon, StringComparison.Ordinal);
        Assert.Contains("Değiştir", coupon, StringComparison.Ordinal);
        Assert.Contains("Kaldır", coupon, StringComparison.Ordinal);
        Assert.Contains("handler=RemoveCoupon", coupon, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"coupon__discount"">[^<]*25", RegexOptions.CultureInvariant, PatternTimeout), coupon);

        // The only code input is the one under "Değiştir": a new code replaces the applied one, so the order never holds two.
        Assert.Single(Regex.Matches(coupon, "name=\"Input.Coupon\"", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.True(coupon.IndexOf("Değiştir", StringComparison.Ordinal) < coupon.IndexOf("name=\"Input.Coupon\"", StringComparison.Ordinal));
    }

    // ---- KK6: two refusals, the closed field ----

    [Fact]
    public async Task ARefusedCode_IsToldByTwoDifferentMessages_AndTheLimitClosesTheField_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string rejected) = await GetAsync(client, "?coupon=rejected");
        (_, string below) = await GetAsync(client, "?coupon=below");
        (_, string limit) = await GetAsync(client, "?coupon=limit");

        Assert.Contains(CouponFields.RejectedMessage, Region(rejected, "summary"), StringComparison.Ordinal);
        Assert.Contains("value=\"yanlis\"", Region(rejected, "summary"), StringComparison.Ordinal);
        Assert.Contains(CouponFields.BelowMinimumText(500m), Region(below, "summary"), StringComparison.Ordinal);
        Assert.DoesNotContain(CouponFields.RejectedMessage, Region(below, "summary"), StringComparison.Ordinal);

        string closed = Region(limit, "summary");
        Assert.DoesNotContain("name=\"Input.Coupon\"", closed, StringComparison.Ordinal);
        Assert.DoesNotContain("handler=ApplyCoupon", closed, StringComparison.Ordinal);
        Assert.Contains("coupon-limit-message", closed, StringComparison.Ordinal);
    }

    // ---- KK7: the boxes, none ticked, directly above the button ----

    [Fact]
    public async Task TheConfirmationBoxes_AreTheTwoFixedAndTheDigitalOne_NoneTicked_BelowTheLegalTextsAndDirectlyAboveTheButton_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client);
        string confirm = Region(html, "confirm");

        Assert.Equal(3, Regex.Count(confirm, "type=\"checkbox\"", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.DoesNotMatch(new Regex(@"<input[^>]*type=""checkbox""[^>]*\schecked", RegexOptions.CultureInvariant, PatternTimeout), confirm);
        int legal = confirm.IndexOf("id=\"legal-texts\"", StringComparison.Ordinal);
        int summary = confirm.IndexOf("id=\"withdrawal-summary\"", StringComparison.Ordinal);
        int firstBox = confirm.IndexOf("type=\"checkbox\"", StringComparison.Ordinal);
        int lastBox = confirm.LastIndexOf("type=\"checkbox\"", StringComparison.Ordinal);
        int button = confirm.IndexOf("id=\"confirm-button\"", StringComparison.Ordinal);
        Assert.True(legal < firstBox && summary < firstBox);
        Assert.True(lastBox < button);

        // Directly above: nothing but the closing of the box group stands between the last box and the button.
        string between = confirm[lastBox..button];
        Assert.DoesNotContain("<p", between, StringComparison.Ordinal);
        Assert.DoesNotContain("<section", between, StringComparison.Ordinal);
    }

    // ---- KK8: an unticked box does not make the order and is marked at the box ----

    [Fact]
    public async Task AFormWithAnUntickedBox_MakesNoOrder_AndMarksThatBoxWithItsOwnMessage_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await PostConfirmAsync(
            client,
            ("Input.AcceptPreInformation", "true"),
            ("Input.AcceptDistanceSales", "true"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.DoesNotContain("id=\"confirmed\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"Input_AcceptDigital-message\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"Input_AcceptPreInformation-message\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain("id=\"Input_AcceptDistanceSales-message\"", html, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"<input[^>]*id=""Input_AcceptPreInformation""[^>]*\schecked", RegexOptions.CultureInvariant, PatternTimeout), html);
    }

    [Fact]
    public async Task AFormWithEveryRequiredBoxTicked_MakesTheOrder_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (HttpStatusCode status, string html) = await PostConfirmAsync(
            client,
            ("Input.AcceptPreInformation", "true"),
            ("Input.AcceptDistanceSales", "true"),
            ("Input.AcceptDigital", "true"));

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("id=\"confirmed\"", html, StringComparison.Ordinal);
    }

    // ---- KK9: gallery rows, the main mark, the alternative text hint ----

    [Fact]
    public async Task TheGallery_ShowsItsRowsInTheFirmsOrder_WithOneMainChoice_AndSaysTheSystemTextStandsInForAnEmptyAlt_KK9()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client);
        string form = Region(html, "panel-form");

        Assert.Equal(3, Regex.Count(form, "<li class=\"gallery-upload__row\">", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.True(form.IndexOf("b2.webp", StringComparison.Ordinal) < form.IndexOf("b1.webp", StringComparison.Ordinal));
        Assert.True(form.IndexOf("b1.webp", StringComparison.Ordinal) < form.IndexOf("b3.webp", StringComparison.Ordinal));
        Assert.Equal(3, Regex.Count(form, "type=\"radio\" name=\"Input.Gallery.MainImageId\"", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.Single(Regex.Matches(form, @"type=""radio""[^>]*\schecked", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.Matches(new Regex(@"id=""main-00000000-0000-0000-0000-0000000000b2""[^>]*\schecked", RegexOptions.CultureInvariant, PatternTimeout), form);
        Assert.Single(Regex.Matches(form, "Boş bırakırsanız şu metin kullanılır:", RegexOptions.CultureInvariant, PatternTimeout));
        Assert.Contains("multiple", form, StringComparison.Ordinal);
    }

    // ---- KK10: progress and the closed save button; the customer uploads nothing ----

    [Fact]
    public async Task WhileAnUploadGoesOn_TheProgressShows_AndTheSaveButtonIsClosed_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string idle) = await GetAsync(client);
        (_, string running) = await GetAsync(client, "?uploading=true");

        Assert.DoesNotContain("<progress", Region(idle, "panel-form"), StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"<button[^>]*id=""save-button""[^>]*\sdisabled", RegexOptions.CultureInvariant, PatternTimeout), Region(idle, "panel-form"));
        string form = Region(running, "panel-form");
        Assert.Contains("<progress value=\"30\"", form, StringComparison.Ordinal);
        Assert.Contains(UploadChecks.SaveBlockedText, form, StringComparison.Ordinal);
        Assert.Matches(new Regex(@"<button[^>]*id=""save-button""[^>]*\sdisabled", RegexOptions.CultureInvariant, PatternTimeout), form);
    }

    [Fact]
    public async Task TheSingleFileField_TakesOneFile_AndSaysItsLimitsFromTheConstants_AndTheCustomersFormHasNoFileInput_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        (_, string html) = await GetAsync(client);
        string logo = Region(html, "panel-form");
        string customer = Region(html, "customer-contact");

        Assert.Matches(new Regex(@"<input[^>]*type=""file""[^>]*name=""Input.Logo""[^>]*accept=""image/jpeg,image/png,image/webp""", RegexOptions.CultureInvariant, PatternTimeout), logo);
        Assert.DoesNotMatch(new Regex(@"<input[^>]*name=""Input.Logo""[^>]*\smultiple", RegexOptions.CultureInvariant, PatternTimeout), logo);
        Assert.Contains(UploadLimits.MaxSizeText(UploadSurface.Logo), logo, StringComparison.Ordinal);
        Assert.DoesNotContain("type=\"file\"", customer, StringComparison.Ordinal);
        Assert.DoesNotContain("multipart/form-data", customer, StringComparison.Ordinal);
    }
}
