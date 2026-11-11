using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The message, confirmation, empty, loading, error and sales closed components on the opened application (T541; 04 OB-03, OB-04, OB-07, OB-08; 09 16.1): the real shared parts drawn by two test-only
/// sample pages, read from the response. The rules of the small models are in the unit tests; here the markup - what each component carries, where the focus goes, what stays after a 422 - is read.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class MessageComponentTests
{
    private static readonly TimeSpan PatternTimeout = TimeSpan.FromSeconds(5);

    private readonly CollectionDatabaseFixture _database;

    private readonly string _adminKey;

    public MessageComponentTests(CollectionDatabaseFixture database)
    {
        _database = database;
        _adminKey = TestSessions.AdminAsync(database.Dsn).GetAwaiter().GetResult().Key;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));

    private WebApplicationFactory<Program> Open() =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(MessageSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory, bool panel = false)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        if (panel)
        {
            client.DefaultRequestHeaders.Add("Cookie", $"{CookieNames.AdminSession}={_adminKey}");
        }

        return client;
    }

    private static async Task<(HttpStatusCode Status, string Html)> GetAsync(HttpClient client, string path)
    {
        using HttpResponseMessage response = await client.GetAsync(new Uri(path, UriKind.Relative), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static async Task<(HttpStatusCode Status, string Html)> PostAsync(HttpClient client, string path, params (string Name, string Value)[] fields)
    {
        using HttpResponseMessage response = await client.PostAsync(new Uri(path, UriKind.Relative), Form(fields), Ct);
        return (response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    // The markup of one region of a sample page: from its opening section to the next section.
    private static string Region(string html, string id)
    {
        int start = html.IndexOf($"<section id=\"{id}\"", StringComparison.Ordinal);
        Assert.True(start >= 0, $"No region '{id}' in the response.");
        int end = html.IndexOf("</section>", start, StringComparison.Ordinal);
        return html[start..end];
    }

    private static string MessageWith(string html, string text)
    {
        int at = html.IndexOf(text, StringComparison.Ordinal);
        Assert.True(at >= 0, $"No message with the text '{text}'.");
        int start = html.LastIndexOf("<div class=\"message ", at, StringComparison.Ordinal);
        Assert.True(start >= 0, $"The text '{text}' is not inside a message.");
        return html[start..(at + text.Length)];
    }

    private static int CountOf(string html, string text) => Regex.Count(html, Regex.Escape(text), RegexOptions.CultureInvariant, PatternTimeout);

    // ---- KK1: symbol and text for every meaning ----

    [Fact]
    public async Task EveryMessage_CarriesASymbolTheSpokenWordAndItsText_AndTheColourIsNotTheOnlyMeans_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await GetAsync(client, MessageSampleModel.Path);
        string messages = Region(html, "messages");

        Assert.Equal(HttpStatusCode.OK, status);
        (string Text, string Symbol, string Word, string Modifier, string Role)[] expected =
        [
            ("Hata metni.", "✕", "Hata:", "message--error", "alert"),
            ("Uyari metni.", "!", "Uyarı:", "message--warning", "alert"),
            ("Bilgi metni.", "i", "Bilgi:", "message--info", "status"),
            ("Basari metni.", "✓", "Başarılı:", "message--success", "status"),
        ];
        foreach ((string text, string symbol, string word, string modifier, string role) in expected)
        {
            string message = MessageWith(messages, text);
            Assert.Contains($"<span class=\"message__symbol\" aria-hidden=\"true\">{symbol}</span>", message, StringComparison.Ordinal);
            Assert.Contains($"<span class=\"u-visually-hidden\">{word}</span>", message, StringComparison.Ordinal);
            Assert.Contains(modifier, message, StringComparison.Ordinal);
            Assert.Contains($"role=\"{role}\"", message, StringComparison.Ordinal);
        }
    }

    // ---- KK2: the summary, the focus, the kept values ----

    [Fact]
    public async Task ASubmittedFormWithTwoWrongFields_MarksBoth_CountsThem_FocusesTheFirst_AndKeepsWhatWasTyped_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await PostAsync(client, MessageSampleModel.Path + "?handler=Save", ("Input.Name", string.Empty), ("Input.Sku", string.Empty), ("Input.Note", "yazdigim not"));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains("2 alanda hata var", html, StringComparison.Ordinal);
        Assert.Contains("id=\"form-error-summary\"", html, StringComparison.Ordinal);
        Assert.Contains("href=\"#Input_Name\"", html, StringComparison.Ordinal);
        Assert.Contains("href=\"#Input_Sku\"", html, StringComparison.Ordinal);
        Assert.Equal(2, CountOf(html, "aria-invalid=\"true\""));
        Assert.Contains("data-first-field=\"Input_Name\"", html, StringComparison.Ordinal);

        // The focus goes to the first wrong field only, by the field's own attribute; the second wrong field does not take it.
        Assert.Equal(1, CountOf(html, " autofocus="));
        Assert.Matches(new Regex("<input id=\"Input_Name\"[^>]* autofocus", RegexOptions.CultureInvariant, PatternTimeout), html);
        Assert.DoesNotMatch(new Regex("<input id=\"Input_Sku\"[^>]* autofocus", RegexOptions.CultureInvariant, PatternTimeout), html);

        // Each wrong field keeps its own message, tied to the field by the id its aria-describedby names; the typed value stays.
        Assert.Contains("id=\"Input_Name-message\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"Input_Sku-message\"", html, StringComparison.Ordinal);
        Assert.Contains("value=\"yazdigim not\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain(nameof(ErrorCode.FieldRequired), html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AFormWithOneWrongField_SaysOne_FocusesThatField_AndKeepsTheOtherValue_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await PostAsync(client, MessageSampleModel.Path + "?handler=Save", ("Input.Name", "Kalem"), ("Input.Sku", string.Empty), ("Input.Note", string.Empty));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.Contains("1 alanda hata var", html, StringComparison.Ordinal);
        Assert.Equal(1, CountOf(html, "aria-invalid=\"true\""));
        Assert.Matches(new Regex("<input id=\"Input_Sku\"[^>]* autofocus", RegexOptions.CultureInvariant, PatternTimeout), html);
        Assert.Contains("value=\"Kalem\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task AValidForm_HasNoSummary_AndAFreshPageHasNone_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string fresh) = await GetAsync(client, MessageSampleModel.Path);
        using HttpResponseMessage saved = await client.PostAsync(new Uri(MessageSampleModel.Path + "?handler=Save", UriKind.Relative), Form(("Input.Name", "a"), ("Input.Sku", "b")), Ct);

        Assert.DoesNotContain("form-error-summary", fresh, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, saved.StatusCode);
    }

    // ---- KK3, KK4: the lasting block message and the short-lived notice ----

    [Fact]
    public async Task ThePanelBlockMessage_StaysInPlace_SaysReasonCountAndWayOut_AndOpensNoWindow_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, MessageSampleModel.Path);
        string block = MessageWith(html, "Once urunleri taslaga alin.");

        Assert.Contains("message--in-place", block, StringComparison.Ordinal);
        Assert.Contains("role=\"alert\"", block, StringComparison.Ordinal);
        Assert.Contains("Kategori silinemez. Engelleyen kayıt sayısı: 3. Once urunleri taslaga alin.", block, StringComparison.Ordinal);
        Assert.DoesNotContain("data-js=\"message-close\"", block, StringComparison.Ordinal);
        Assert.DoesNotContain("message--short-lived", block, StringComparison.Ordinal);
        Assert.DoesNotContain("<dialog", Region(html, "messages"), StringComparison.Ordinal);
    }

    [Fact]
    public async Task OnlyTheSuccessIsShortLived_AndTheOnceToldMessageIsClosable_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, MessageSampleModel.Path);
        string messages = Region(html, "messages");

        Assert.Equal(1, CountOf(messages, "message--short-lived"));
        Assert.Contains("message--success", MessageWith(messages, "Kaydedildi."), StringComparison.Ordinal);
        Assert.Contains("message--short-lived", MessageWith(messages, "Kaydedildi."), StringComparison.Ordinal);
        Assert.Contains("data-js=\"message-close\"", messages[messages.IndexOf("Bir kez soylenen metin.", StringComparison.Ordinal)..], StringComparison.Ordinal);
        Assert.DoesNotContain("message--short-lived", MessageWith(messages, "Hata metni."), StringComparison.Ordinal);
    }

    // ---- KK5: the neutral limit message beside the button ----

    [Fact]
    public async Task TheNeutralLimitMessage_IsBesideTheBlockedButton_SaysOnlyTheSentence_AndKeepsTheValue_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (HttpStatusCode status, string html) = await PostAsync(client, MessageSampleModel.Path + "?handler=Limit", ("Input.Name", "kalan deger"));
        int form = html.IndexOf("id=\"limit-form\"", StringComparison.Ordinal);
        string limitForm = html[form..html.IndexOf("</form>", form, StringComparison.Ordinal)];

        Assert.Equal(HttpStatusCode.UnprocessableEntity, status);
        Assert.True(limitForm.IndexOf("Giris yap</button>", StringComparison.Ordinal) < limitForm.IndexOf(NeutralMessages.LimitExceeded, StringComparison.Ordinal));
        string message = MessageWith(limitForm, NeutralMessages.LimitExceeded);
        Assert.Contains("id=\"limit-message\"", message, StringComparison.Ordinal);
        Assert.DoesNotMatch(new Regex(@"\d", RegexOptions.CultureInvariant, PatternTimeout), message[(message.IndexOf("message__text", StringComparison.Ordinal))..]);
        Assert.Contains("value=\"kalan deger\"", limitForm, StringComparison.Ordinal);
        Assert.DoesNotContain("form-error-summary", html, StringComparison.Ordinal);
    }

    // ---- KK6, KK7: the anatomy of the confirmation, the two faces ----

    [Fact]
    public async Task TheCustomersConfirmation_IsTheLastStepOfThePage_NotAWindow_WithTheFiveParts_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, MessageSampleModel.Path);
        string step = Region(html, "customer-step");

        Assert.Contains("<section class=\"confirm confirm--step\"", step, StringComparison.Ordinal);
        Assert.DoesNotContain("<dialog", html, StringComparison.Ordinal);
        Assert.Contains(">Hesabı sil</h2>", step, StringComparison.Ordinal);
        Assert.Contains("Hesabınız ve kayıtlı adresleriniz silinir.", step, StringComparison.Ordinal);
        Assert.Contains("Bu işlem geri alınamaz.", step, StringComparison.Ordinal);
        Assert.Matches(new Regex("<button type=\"submit\"[^>]*>Hesabı sil</button>", RegexOptions.CultureInvariant, PatternTimeout), step);
        Assert.Matches(new Regex("<a [^>]*href=\"/hesabim\"[^>]*>Vazgeç</a>", RegexOptions.CultureInvariant, PatternTimeout), step);
        Assert.DoesNotContain("autofocus", step, StringComparison.Ordinal);
        Assert.Contains("<input type=\"hidden\" name=\"Confirmed\" value=\"true\">", step, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheAdministratorsConfirmation_IsAWindowWithTheFocusOnVazgec_AndOpenInTheMarkupForThePathWithoutAScript_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        (HttpStatusCode status, string html) = await GetAsync(client, PanelMessageSampleModel.Path);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("<dialog class=\"confirm confirm--window\" open aria-labelledby=\"confirm-heading\" data-js=\"confirm-window\">", html, StringComparison.Ordinal);
        Assert.Contains(">Kargoya ver</h2>", html, StringComparison.Ordinal);
        Assert.Matches(new Regex("<a [^>]*confirm__cancel[^>]*autofocus[^>]*>Vazgeç</a>", RegexOptions.CultureInvariant, PatternTimeout), html);
        Assert.Matches(new Regex("<button type=\"submit\"[^>]*>Kargoya ver</button>", RegexOptions.CultureInvariant, PatternTimeout), html);
        Assert.Contains("Bu işlem geri alınamaz.", html, StringComparison.Ordinal);
        Assert.Contains("action=\"/panel/mesaj-sss?handler=Ship\"", html, StringComparison.Ordinal);
        Assert.Contains("href=\"/panel/siparisler\"", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheConfirmationsForm_CanNotBeSentTwice_AndSaysThatTheOperationContinues_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);
        using HttpClient panelClient = ClientOf(factory, panel: true);

        (_, string customer) = await GetAsync(client, MessageSampleModel.Path);
        (_, string admin) = await GetAsync(panelClient, PanelMessageSampleModel.Path);

        foreach (string html in new[] { Region(customer, "customer-step"), admin[admin.IndexOf("<dialog", StringComparison.Ordinal)..] })
        {
            Assert.Contains("data-js=\"submit-once\"", html, StringComparison.Ordinal);
            Assert.Matches(new Regex("data-busy-label=\"[^\"]+ sürüyor\"", RegexOptions.CultureInvariant, PatternTimeout), html);
            Assert.Contains("name=\"__RequestVerificationToken\"", html, StringComparison.Ordinal);
        }
    }

    [Fact]
    public async Task AnOutcomeUncertainConfirmation_SendsTheUserToSeeTheResult_Not_ToTheFormAgain_KK7()
    {
        await using WebApplicationFactory<Program> factory = Open();
        await using WebApplicationFactory<Program> withErrors = factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ErrorSampleModel).Assembly)));
        using HttpClient client = ClientOf(withErrors);

        // The marked sample operation fails after its commit: the layer answers 303 to the result screen; the page below only says where to look (04 2.7.3.3).
        (HttpStatusCode status, string html) = await PostAsync(client, "/odeme/hata-ornegi?handler=Confirm", ("Input.Mode", "after"));

        Assert.Equal(HttpStatusCode.SeeOther, status);
        Assert.DoesNotContain("<form", html, StringComparison.Ordinal);
    }

    // ---- KK8: empty states and loading ----

    [Fact]
    public async Task TheTwoEmptyStates_SaySomethingAndOfferAWayOut_WithoutAnErrorSymbol_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, MessageSampleModel.Path);
        string states = Region(html, "states");

        Assert.Equal(2, CountOf(states, "class=\"empty-state\""));
        Assert.Contains("<p class=\"empty-state__sentence\">Henuz urun yok.</p>", states, StringComparison.Ordinal);
        Assert.Contains("<a class=\"empty-state__exit\" href=\"/\">Ana sayfaya don</a>", states, StringComparison.Ordinal);
        Assert.Contains("<a class=\"button button--primary\" href=\"/hesabim/adresler/yeni\">Adres ekle</a>", states, StringComparison.Ordinal);
        string empties = states[..states.IndexOf("class=\"loading\"", StringComparison.Ordinal)];
        Assert.DoesNotContain("message--error", empties, StringComparison.Ordinal);
        Assert.DoesNotContain("role=\"alert\"", empties, StringComparison.Ordinal);
        Assert.DoesNotContain("✕", empties, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheLoadingState_KeepsTheFrame_AndCarriesText_KK8()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string html) = await GetAsync(client, MessageSampleModel.Path);

        Assert.Contains("<div class=\"loading\" role=\"status\" aria-busy=\"true\">", html, StringComparison.Ordinal);
        Assert.Contains("<span class=\"loading__text\">Siparisler yukleniyor</span>", html, StringComparison.Ordinal);
        Assert.Contains("<header class=\"site-header", html, StringComparison.Ordinal);
        Assert.Contains("<footer class=\"site-footer\"", html, StringComparison.Ordinal);
        Assert.Contains("loading__indicator\" aria-hidden=\"true\"", html, StringComparison.Ordinal);
    }

    // ---- KK9: the error page inside the frame, no code ----

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ThePageLevelErrorPage_IsDrawnInsideTheFrameOfItsSurface_WithRetryAndTheWayHome_AndNoCode_KK9(bool panel)
    {
        await using WebApplicationFactory<Program> factory = Open();
        await using WebApplicationFactory<Program> withErrors = factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ErrorSampleModel).Assembly)));
        using HttpClient client = ClientOf(withErrors, panel);
        string path = panel ? "/panel/hata-ornegi?fail=handler" : "/odeme/hata-ornegi?fail=handler";

        (HttpStatusCode status, string html) = await GetAsync(client, path);

        Assert.Equal(HttpStatusCode.InternalServerError, status);
        Assert.Contains("<section class=\"error-page\">", html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{path}\">Yeniden dene</a>", html, StringComparison.Ordinal);
        Assert.Contains(panel ? "kurulumu yapan kişiye" : "İletişim sayfasından", html, StringComparison.Ordinal);
        Assert.Contains(panel ? "<a class=\"button button--secondary\" href=\"/panel\">Panele dön</a>" : "<a class=\"button button--secondary\" href=\"/\">Ana sayfaya dön</a>", html, StringComparison.Ordinal);
        Assert.Contains(panel ? "<header class=\"panel-top\">" : "<header class=\"site-header", html, StringComparison.Ordinal);
        Assert.Contains("message--error", html, StringComparison.Ordinal);
        foreach (string code in Enum.GetNames<ErrorCode>())
        {
            Assert.DoesNotContain(code, html, StringComparison.Ordinal);
        }
    }

    // ---- KK10: sales closed ----

    [Fact]
    public async Task WhileSalesAreClosed_TheStorefrontOnlySaysSo_AtThePlaceOfTheButton_AndNamesNoCondition_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string closed) = await GetAsync(client, MessageSampleModel.Path + "?closed=true");
        (_, string open) = await GetAsync(client, MessageSampleModel.Path);

        string sales = Region(closed, "sales");
        Assert.Contains("Satış şu anda kapalı.", sales, StringComparison.Ordinal);
        Assert.Contains("message--in-place", MessageWith(sales, "Satış şu anda kapalı."), StringComparison.Ordinal);
        Assert.DoesNotContain("Firma kimliği", closed, StringComparison.Ordinal);
        Assert.DoesNotContain("Ödeme yöntemi", closed, StringComparison.Ordinal);
        Assert.DoesNotContain("Satış şu anda kapalı.", open, StringComparison.Ordinal);
        Assert.Contains("id=\"sample-content\"", closed, StringComparison.Ordinal);
    }

    [Fact]
    public async Task TheDataEntryGate_SaysItsOwnSentence_InPlace_AndIsIndependentOfTheSalesState_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory);

        (_, string dataClosed) = await GetAsync(client, MessageSampleModel.Path + "?dataClosed=true");
        (_, string salesOnly) = await GetAsync(client, MessageSampleModel.Path + "?closed=true");

        Assert.Contains("Bu form şu anda kullanıma kapalı.", Region(dataClosed, "sales"), StringComparison.Ordinal);
        Assert.DoesNotContain("Satış şu anda kapalı.", dataClosed, StringComparison.Ordinal);
        Assert.DoesNotContain("Bu form şu anda kullanıma kapalı.", salesOnly, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePanelSalesState_ShowsInFourPlaces_WhenAConditionIsMissing_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        (HttpStatusCode status, string html) = await GetAsync(client, PanelMessageSampleModel.Path + "?unmet=CompanyIdentity,PrivacyNotice");

        Assert.Equal(HttpStatusCode.OK, status);

        // 1: the top row's indicator is the frame's.
        Assert.Contains("Satış kapalı", html, StringComparison.Ordinal);

        // 2: the home page's warning names the unmet conditions.
        Assert.Contains("Satış kapalı: Firma kimliği, Aydınlatma metni tamamlanmadı.", Region(html, "warning"), StringComparison.Ordinal);

        // 3: the checklist, each missing item with the screen that fixes it.
        string checklist = Region(html, "checklist");
        Assert.Contains("Kurulum kontrol listesi", checklist, StringComparison.Ordinal);
        Assert.Contains($"<a href=\"{PanelFramePaths.FirmIdentityAndSales}\">Firma kimliği</a>", checklist, StringComparison.Ordinal);
        Assert.Contains(">Aydınlatma metni</a>", checklist, StringComparison.Ordinal);

        // 4: E-44's section: the four conditions one by one, the unmet ones with the way to fix them.
        string conditions = Region(html, "conditions");
        Assert.Contains("Satış durumu", conditions, StringComparison.Ordinal);
        Assert.Equal(4, CountOf(conditions, "class=\"sales-status__row\""));
        Assert.Contains("Firma kimliği: Eksik", conditions, StringComparison.Ordinal);
        Assert.Contains("Ödeme yöntemi: Tamam", conditions, StringComparison.Ordinal);
        Assert.Contains("Yasal metinler: Aydınlatma metni: Eksik", conditions, StringComparison.Ordinal);
        Assert.Equal(2, CountOf(conditions, ">Düzelt</a>"));
    }

    [Fact]
    public async Task ThePanelSalesState_OfAnOpenGate_HasNoWarningAndNoChecklist_ButTheSectionStays_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        (_, string html) = await GetAsync(client, PanelMessageSampleModel.Path);

        Assert.DoesNotContain("message--warning", Region(html, "warning"), StringComparison.Ordinal);
        Assert.DoesNotContain("Kurulum kontrol listesi", html, StringComparison.Ordinal);
        Assert.Equal(4, CountOf(Region(html, "conditions"), "class=\"sales-status__row\""));
        Assert.Equal(0, CountOf(Region(html, "conditions"), ">Düzelt</a>"));
        Assert.Contains("Satış açık", html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ATemporaryClosure_ShowsTheStateAndTheWarning_ButNoChecklist_KK10()
    {
        await using WebApplicationFactory<Program> factory = Open();
        using HttpClient client = ClientOf(factory, panel: true);

        (_, string html) = await GetAsync(client, PanelMessageSampleModel.Path + "?unmet=TemporaryClosure");

        Assert.Contains("Satış kapalı: satışı geçici kapattınız.", Region(html, "warning"), StringComparison.Ordinal);
        Assert.DoesNotContain("Kurulum kontrol listesi", html, StringComparison.Ordinal);
        Assert.Contains("Satışı geçici kapatma anahtarı kapalı: Eksik", Region(html, "conditions"), StringComparison.Ordinal);
    }
}
