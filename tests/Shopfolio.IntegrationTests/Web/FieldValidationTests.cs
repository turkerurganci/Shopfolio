using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The input validation pattern on the opened application and the test-only sample page (T211 criteria 7, 8; KK7, KK8; 09 3.4 items 1-7): the rule
/// attributes' codes become catalog messages, the page filter answers 422 before the handler with every error together and writes one log line with
/// the fields and codes, the code never reaches the page, the binder trims, and the framework's own client-side output is off.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class FieldValidationTests
{
    private const string PagePath = "/panel/model-baglama";

    private readonly CollectionDatabaseFixture _database;

    public FieldValidationTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class Host : IAsyncDisposable
    {
        private readonly ShopfolioWebApplicationFactory _root;
        private readonly WebApplicationFactory<Program> _factory;

        public Host(string dsn, bool clientValidation)
        {
            Store = new BindingRecordStore();
            _root = new ShopfolioWebApplicationFactory(dsn);
            BindingRecordStore records = Store;
            _factory = _root.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.AddSingleton(records);
                s.AddRazorPages().AddApplicationPart(typeof(BindingFormModel).Assembly);
                if (clientValidation)
                {
                    // The switch the application turns off, turned on again: shows that the test below can see the framework's output.
                    s.Configure<MvcViewOptions>(o => o.HtmlHelperOptions.ClientValidationEnabled = true);
                }
            }));
            Client = ForgeryTokenHandler.CreateClient(_factory);
            // The sample page is a panel page (T207: the panel is closed by default), so the client carries the cookie of a panel session.
            string adminKey = TestSessions.AdminAsync(dsn).GetAwaiter().GetResult().Key;
            Client.DefaultRequestHeaders.Add("Cookie", $"{CookieNames.AdminSession}={adminKey}");
        }

        public BindingRecordStore Store { get; }

        public HttpClient Client { get; }

        public IServiceProvider Services => _factory.Services;

        public string LogDirectory => _root.LogDirectory;

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await _factory.DisposeAsync();
            await _root.DisposeAsync();
        }
    }

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));

    private sealed record Answer(HttpStatusCode Status, Uri? Location, string Headers, string Body, string RequestId);

    private static async Task<Answer> Post(Host host, string handler, FormUrlEncodedContent content)
    {
        string requestId = "t211-" + Guid.NewGuid().ToString("N");
        using HttpRequestMessage request = new(HttpMethod.Post, new Uri($"{PagePath}?handler={handler}", UriKind.Relative)) { Content = content };
        request.Headers.TryAddWithoutValidation("X-Request-Id", requestId);
        using HttpResponseMessage response = await host.Client.SendAsync(request, Ct);
        string headers = string.Concat(response.Headers.Select(h => string.Concat(h.Value)));
        return new Answer(response.StatusCode, response.Headers.Location, headers, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)), requestId);
    }

    private static List<JsonElement> LinesOf(Host host, string requestId)
    {
        List<JsonElement> lines = [];
        foreach (string path in Directory.GetFiles(host.LogDirectory, "app-*.jsonl"))
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new(stream);
            while (reader.ReadLine() is { } text)
            {
                JsonElement line = JsonDocument.Parse(text).RootElement.Clone();
                if (line.TryGetProperty("requestId", out JsonElement id) && id.GetString() == requestId)
                {
                    lines.Add(line);
                }
            }
        }

        return lines;
    }

    private static (string Field, string Code)[] FieldsOf(JsonElement line) =>
        [.. line.GetProperty("fields").EnumerateArray().Select(f => (f.GetProperty("field").GetString()!, f.GetProperty("code").GetString()!)).Order()];

    private static (string Name, string Value)[] ValidContact(params (string Name, string Value)[] overrides)
    {
        (string, string)[] baseFields = [("Email", "ad@example.com"), ("Phone", "05321234567"), ("Password", "gizli-sifre"), ("Remark", "not"), ("Amount", "12")];
        return [.. baseFields.Where(b => overrides.All(o => o.Name != b.Item1)), .. overrides];
    }

    // --- KK8: every error together, the catalog text, the code never shown ---

    [Fact]
    public async Task Post_WithSeveralWrongFields_Is422_WithEveryFieldsCatalogMessageTogether_AndNoCodeOnThePage()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        Answer a = await Post(host, "Contact", Form(("Email", "bozuk"), ("Phone", "123"), ("Password", ""), ("Amount", "abc")));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        Assert.Null(a.Location);
        Assert.Contains("data-field=\"Email\">E-posta geçerli bir e-posta adresi olmalıdır.</span>", a.Body, StringComparison.Ordinal);
        Assert.Contains("data-field=\"Phone\">Telefon geçerli bir telefon numarası olmalıdır.</span>", a.Body, StringComparison.Ordinal);
        Assert.Contains("data-field=\"Password\">Şifre boş olamaz.</span>", a.Body, StringComparison.Ordinal);
        Assert.Contains("data-field=\"Amount\">Tutar geçerli bir tutar olmalıdır; en çok iki ondalık yazılabilir.</span>", a.Body, StringComparison.Ordinal);
        foreach (string code in FieldCodes.All)
        {
            Assert.DoesNotContain(code, a.Body, StringComparison.Ordinal);
            Assert.DoesNotContain(code, a.Headers, StringComparison.Ordinal);
        }

        Assert.Empty(host.Store.Contacts());
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("0")]
    [InlineData("1001")]
    public async Task Post_WithAWrongQuantity_AnswersTheRulesCodeMessage_WhetherTheTextIsNoNumberOrOutOfRange(string quantity)
    {
        await using Host host = new(_database.Dsn, clientValidation: false);
        Guid item = Guid.NewGuid();

        Answer a = await Post(
            host, "ReturnItems", Form(("Items[0].OrderItemId", item.ToString()), ("Items[0].Quantity", quantity)));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        Assert.Contains("data-field=\"Items[0].Quantity\">Adet 1 ile 1000 arasında bir tam sayı olmalıdır.</span>", a.Body, StringComparison.Ordinal);
        Assert.Empty(host.Store.Returns());
    }

    [Fact]
    public async Task Post_WithAKeyThatIsNoKey_AnswersTheListValueMessage_WithTheFieldsLabel()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        Answer a = await Post(host, "ReturnItems", Form(("Items[0].OrderItemId", "kalem"), ("Items[0].Quantity", "1")));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        Assert.Contains("data-field=\"Items[0].OrderItemId\">Kalem için geçerli bir seçim yapılmalıdır.</span>", a.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Post_WithoutAnyItemRow_Is422WithTheRequiredMessage()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        Answer a = await Post(host, "ReturnItems", Form());

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        Assert.Contains("data-field=\"Items\">Kalemler boş olamaz.</span>", a.Body, StringComparison.Ordinal);
        Assert.Empty(host.Store.Returns());
    }

    // --- the log line: one per rejected request, with every field and code, and no value ---

    [Fact]
    public async Task Post_Rejection_WritesOneLineWithTheFieldsAndCodes_AndNeitherTheValuesNorTheMessages()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        Answer a = await Post(
            host, "Contact", Form(("Email", "bozuk-deger"), ("Phone", "123"), ("Password", ""), ("Amount", "abc")));

        JsonElement line = Assert.Single(LinesOf(host, a.RequestId));
        Assert.Equal(LogEvents.ValidationRejected, line.GetProperty("event").GetString());
        Assert.Equal("POST", line.GetProperty("method").GetString());
        Assert.Equal(
            [("Amount", "InvalidAmount"), ("Email", "InvalidEmail"), ("Password", "FieldRequired"), ("Phone", "InvalidPhone")],
            FieldsOf(line));
        Assert.False(line.TryGetProperty("code", out _));
        string text = line.GetRawText();
        Assert.DoesNotContain("bozuk-deger", text, StringComparison.Ordinal);
        Assert.DoesNotContain("geçerli", text, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Post_HandlerOwnedRule_GoesThroughTheSameTranslation_AfterTheHandlerReturns()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        Answer a = await Post(host, "Contact", Form(ValidContact(("Email", "taken@example.com"))));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        Assert.Contains("data-field=\"Email\">E-posta geçerli bir e-posta adresi olmalıdır.</span>", a.Body, StringComparison.Ordinal);
        Assert.DoesNotContain("InvalidEmail", a.Body, StringComparison.Ordinal);
        Assert.Equal([("Email", "InvalidEmail")], FieldsOf(Assert.Single(LinesOf(host, a.RequestId))));
        Assert.Empty(host.Store.Contacts());
    }

    [Fact]
    public async Task Post_WithEverythingRight_Is303_WritesTheRow_AndNoRejectionLine()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        Answer a = await Post(host, "Contact", Form(ValidContact()));

        Assert.Equal(HttpStatusCode.SeeOther, a.Status);
        Assert.DoesNotContain(LinesOf(host, a.RequestId), l => l.GetProperty("event").GetString() == LogEvents.ValidationRejected);
        ContactRecord contact = Assert.Single(host.Store.Contacts());
        Assert.Equal(12m, contact.Amount);
    }

    [Fact]
    public async Task Get_WithARejectedQuery_ShowsTheMessageWithStatus200_AndWritesTheLine()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);
        string requestId = "t211-" + Guid.NewGuid().ToString("N");
        using HttpRequestMessage request = new(HttpMethod.Get, new Uri($"{PagePath}?handler=Find&Search=cok-uzun-bir-arama-metni", UriKind.Relative));
        request.Headers.TryAddWithoutValidation("X-Request-Id", requestId);

        using HttpResponseMessage response = await host.Client.SendAsync(request, Ct);
        string body = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("data-field=\"Search\">Arama en çok 10 karakter olabilir.</span>", body, StringComparison.Ordinal);
        Assert.DoesNotContain("TextTooLong", body, StringComparison.Ordinal);
        Assert.Equal([("Search", "TextTooLong")], FieldsOf(Assert.Single(LinesOf(host, requestId))));
    }

    // --- KK7: the binder trims; the password and a marked field are kept as typed ---

    [Fact]
    public async Task Post_TrimsTexts_MakesWhiteSpaceOnlyTheEmptyString_AndKeepsAPasswordAndAMarkedFieldAsTyped()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        Answer a = await Post(
            host, "Contact", Form(("Email", "  ad@example.com \t"), ("Phone", "    "), ("Password", " iki  bosluk "), ("Remark", "  oldugu gibi "), ("Amount", "0")));

        Assert.Equal(HttpStatusCode.SeeOther, a.Status);
        Assert.Equal(new ContactRecord("ad@example.com", string.Empty, " iki  bosluk ", "  oldugu gibi ", 0m, null), Assert.Single(host.Store.Contacts()));
    }

    [Fact]
    public async Task Post_WithWhiteSpaceOnlyInARequiredField_IsTheRequiredMessage_AndAPasswordOfSpacesIsRefusedToo()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        Answer a = await Post(host, "Contact", Form(ValidContact(("Email", "   "), ("Password", "     "))));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        Assert.Contains("data-field=\"Email\">E-posta boş olamaz.</span>", a.Body, StringComparison.Ordinal);
        Assert.Contains("data-field=\"Password\">Şifre boş olamaz.</span>", a.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Post_WithoutANonNullableTextThatHasNoRule_IsNotRefused_AndTheTextStaysNull()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        Answer a = await Post(host, "Contact", Form(ValidContact()));

        Assert.Equal(HttpStatusCode.SeeOther, a.Status);
        Assert.Null(Assert.Single(host.Store.Contacts()).Nickname);
    }

    [Fact]
    public async Task FrameworkOptions_AreSet_AsTheFilterAndTheRulesNeed()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        MvcOptions mvc = host.Services.GetRequiredService<IOptions<MvcOptions>>().Value;
        MvcViewOptions view = host.Services.GetRequiredService<IOptions<MvcViewOptions>>().Value;

        Assert.True(mvc.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes);
        Assert.False(view.HtmlHelperOptions.ClientValidationEnabled);
        Assert.Equal(FieldError.ConversionFailed, mvc.ModelBindingMessageProvider.ValueMustBeANumberAccessor("x"));
    }

    // --- criterion 8: the framework writes no data-val output; the HTML attributes are the constants' ---

    [Fact]
    public async Task Get_RendersNoDataValAttribute_ForAnIntAndATextFieldWrittenWithTheTagHelper()
    {
        await using Host host = new(_database.Dsn, clientValidation: false);

        string body = await host.Client.GetStringAsync(PagePath, Ct);

        Assert.Contains("name=\"PreviewRow.Quantity\"", body, StringComparison.Ordinal);
        Assert.Contains("name=\"Preview.Name\"", body, StringComparison.Ordinal);
        Assert.DoesNotContain("data-val", body, StringComparison.Ordinal);
        Assert.Contains("name=\"Name\" required maxlength=\"20\"", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_WithTheSwitchTurnedOnAgain_RendersDataVal_SoTheTestAboveCanFail()
    {
        await using Host host = new(_database.Dsn, clientValidation: true);

        string body = await host.Client.GetStringAsync(PagePath, Ct);

        Assert.Contains("data-val=\"true\"", body, StringComparison.Ordinal);
    }
}
