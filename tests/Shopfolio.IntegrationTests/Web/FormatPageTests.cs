using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The date, number and money formats on the opened application (T216 KK1-KK6; 07 2.9; 04 8.1, 8.2; 09 7.2): the storefront and the panel sample show
/// the same value as the same Turkish text, a request with <c>Accept-Language: en</c> - on a host whose thread culture follows the browser's language,
/// the worst case - gets the same text, the instant near midnight falls on its Turkey day, and the form and the query read an amount, a date and a whole
/// number in the one format of 07 2.9 only: a value in another format is the field's message (422 on a form, 200 without the filter on a query) and is
/// never made larger or smaller. The same is checked on a real Kestrel server, whose threads are not the test server's.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed partial class FormatPageTests
{
    private const string StorefrontSample = "/odeme/bicim-ornegi";
    private const string PanelSample = "/panel/bicim-ornegi";
    private const string FormPath = "/panel/bicim-formu";

    private readonly CollectionDatabaseFixture _database;

    public FormatPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex("""<li class="v" data-name="([a-z-]+)">(.*?)</li>""", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ShownValue();

    [GeneratedRegex("""<span class="error" data-field="([^"]*)">(.*?)</span>""", RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex ShownError();

    /// <summary>
    /// The worst case of 07 2.9.9: a host whose request thread takes the browser's language as its culture (as a localization middleware would). The
    /// formats must not follow it.
    /// </summary>
    private sealed class BrowserCultureStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
        {
            app.Use(async (context, call) =>
            {
                string language = context.Request.Headers.AcceptLanguage.ToString();
                if (language.StartsWith("en", StringComparison.OrdinalIgnoreCase))
                {
                    CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en-US");
                    CultureInfo.CurrentUICulture = CultureInfo.GetCultureInfo("en-US");
                }

                await call(context);
            });
            next(app);
        };
    }

    /// <summary>What the reverse proxy adds to every request it passes on: the scheme the visitor used (05 6.3.3).</summary>
    private sealed class ProxyHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class Host : IAsyncDisposable
    {
        private readonly ShopfolioWebApplicationFactory _root;

        public Host(string dsn, bool kestrel)
        {
            _root = new ShopfolioWebApplicationFactory(dsn);
            FormatRecordStore store = Store;
            Factory = _root.WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.AddSingleton(store);
                s.AddTransient<IStartupFilter, BrowserCultureStartupFilter>();
                s.AddRazorPages().AddApplicationPart(typeof(FormatFormModel).Assembly);
                if (kestrel)
                {
                    // The test client stands where the reverse proxy stands in production (05 6.3.3): it ends TLS and tells the scheme, so the
                    // application writes its Secure cookies and forgery tokens over the plain loopback connection as it does behind the proxy.
                    s.PostConfigure<ForwardedHeadersOptions>(o =>
                    {
                        o.KnownProxies.Add(IPAddress.Loopback);
                        o.KnownProxies.Add(IPAddress.IPv6Loopback);
                    });
                }
            }));

            AdminKey = TestSessions.AdminAsync(dsn).GetAwaiter().GetResult().Key;
            if (kestrel)
            {
                Factory.UseKestrel();
                Factory.StartServer();
                using HttpClient probe = Factory.CreateClient();
                HttpClientHandler socket = new() { AllowAutoRedirect = false, UseCookies = false };
                Client = new HttpClient(new ForgeryTokenHandler { InnerHandler = new ProxyHandler { InnerHandler = socket } })
                {
                    BaseAddress = probe.BaseAddress,
                };
            }
            else
            {
                Client = ForgeryTokenHandler.CreateClient(Factory);
            }
        }

        public FormatRecordStore Store { get; } = new();

        public WebApplicationFactory<Program> Factory { get; }

        public HttpClient Client { get; }

        public string AdminKey { get; }

        public async ValueTask DisposeAsync()
        {
            Client.Dispose();
            await Factory.DisposeAsync();
            await _root.DisposeAsync();
        }
    }

    private sealed record Answer(HttpStatusCode Status, string Body);

    private static async Task<Answer> SendAsync(Host host, HttpMethod method, string address, string? language = null, HttpContent? content = null, bool panel = true)
    {
        using HttpRequestMessage request = new(method, new Uri(address, UriKind.Relative)) { Content = content };
        if (panel)
        {
            request.Headers.TryAddWithoutValidation("Cookie", $"{CookieNames.AdminSession}={host.AdminKey}");
        }

        if (language is not null)
        {
            request.Headers.TryAddWithoutValidation("Accept-Language", language);
        }

        using HttpResponseMessage response = await host.Client.SendAsync(request, Ct);
        return new Answer(response.StatusCode, WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct)));
    }

    private static (string Name, string Text)[] ValuesOf(string body) =>
        [.. ShownValue().Matches(body).Select(m => (m.Groups[1].Value, m.Groups[2].Value))];

    private static Dictionary<string, string> ErrorsOf(string body) =>
        ShownError().Matches(body).ToDictionary(m => m.Groups[1].Value, m => m.Groups[2].Value, StringComparer.Ordinal);

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] overrides)
    {
        (string Name, string Value)[] valid =
        [
            ("Price", "1234,50"), ("Rate", "12,5"), ("StartsOn", "2026-10-04"), ("EndsOn", "2026-10-31"), ("Quantity", "5"),
            ("Iban", "TR330006100519786457841326"), ("Note", "not"),
        ];
        return new FormUrlEncodedContent(
            [.. valid.Where(v => overrides.All(o => o.Name != v.Name)).Concat(overrides).Select(f => new KeyValuePair<string, string>(f.Name, f.Value))]);
    }

    private static string PriceMessage() =>
        FieldMessageCatalog.Build(FieldCodes.InvalidAmount, "Fiyat", new InvalidAmountAttribute { Max = FormatFormLimits.PriceMax }.ParameterText);

    // --- KK1, KK2: the same value is the same Turkish text on the storefront and on the panel, whatever the browser's language ---

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task TheStorefrontAndThePanel_ShowTheSameValueAsTheSameTurkishText_AndAcceptLanguageEnChangesNothing(bool kestrel)
    {
        await using Host host = new(_database.Dsn, kestrel);

        Answer storefront = await SendAsync(host, HttpMethod.Get, StorefrontSample, panel: false);
        Answer storefrontEn = await SendAsync(host, HttpMethod.Get, StorefrontSample, "en-US,en;q=0.9", panel: false);
        Answer panel = await SendAsync(host, HttpMethod.Get, PanelSample);
        Answer panelEn = await SendAsync(host, HttpMethod.Get, PanelSample, "en");

        Assert.All([storefront, storefrontEn, panel, panelEn], a => Assert.Equal(HttpStatusCode.OK, a.Status));
        (string, string)[] expected = [.. FormatSampleValues.Texts()];
        Assert.Equal(expected, ValuesOf(storefront.Body));
        Assert.Equal(expected, ValuesOf(storefrontEn.Body));
        Assert.Equal(expected, ValuesOf(panel.Body));
        Assert.Equal(expected, ValuesOf(panelEn.Body));

        // The English request really ran on an English thread: the sameness above is the formats' doing, not an unchanged culture.
        Assert.Contains("<p class=\"thread-culture\">en-US</p>", storefrontEn.Body, StringComparison.Ordinal);
        Assert.Contains("<p class=\"thread-culture\">en-US</p>", panelEn.Body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ThePage_WritesTheFormsOf04_AndTheInstantNearMidnightOnItsTurkeyDay()
    {
        await using Host host = new(_database.Dsn, kestrel: false);

        Dictionary<string, string> shown = ValuesOf((await SendAsync(host, HttpMethod.Get, PanelSample, "en")).Body).ToDictionary(v => v.Name, v => v.Text);

        // 04 8.2's own examples (the table is the source of these texts).
        Assert.Equal("4 Ekim 2026", shown["date"]);
        Assert.Equal("4 Ekim 2026 – 31 Ekim 2026", shown["range"]);
        Assert.Equal("4 Ekim 2026, 14:30", shown["date-time"]);
        Assert.Equal("1.250", shown["count"]);
        Assert.Equal("0,5", shown["fraction"]);
        Assert.Equal("%12,5", shown["rate"]);
        Assert.Equal("266,67 TL", shown["amount"]);
        Assert.Equal("1.200 TL", shown["whole-amount"]);
        Assert.Equal("-50 TL", shown["deduction"]);
        Assert.Equal("3 gün", shown["days"]);
        Assert.Equal("2 iş günü", shown["business-days"]);
        Assert.Equal("0532 123 45 67", shown["phone"]);
        Assert.Equal("TR33 0006 1005 1978 6457 8413 26", shown["iban"]);

        // KK2: 20:59:59 UTC is the last second of 4 October in Turkey; 21:00 UTC is 5 October.
        Assert.Equal("4 Ekim 2026, 23:59", shown["before-midnight"]);
        Assert.Equal("5 Ekim 2026, 00:00", shown["at-midnight"]);
    }

    // --- KK3, KK4: the form reads the one format; another format is the field's message, never another value ---

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_WithTheOneFormat_IsSaved_WithTheValuesWritten_WhateverTheBrowsersLanguage(bool kestrel)
    {
        await using Host host = new(_database.Dsn, kestrel);

        Answer a = await SendAsync(host, HttpMethod.Post, $"{FormPath}?handler=SavePrice", "en-US", Form(("Iban", "TR33 0006 1005 1978 6457 8413 26"), ("Quantity", " 7 ")));

        Assert.Equal(HttpStatusCode.SeeOther, a.Status);
        SavedPrice saved = Assert.Single(host.Store.All());
        Assert.Equal(1234.50m, saved.Price);
        Assert.Equal(12.5m, saved.Rate);
        Assert.Equal(new DateOnly(2026, 10, 4), saved.StartsOn);
        Assert.Equal(new DateOnly(2026, 10, 31), saved.EndsOn);
        Assert.Equal(7, saved.Quantity);

        // KK5: the IBAN with blanks is the IBAN without them.
        Assert.Equal("TR330006100519786457841326", saved.Iban);
        Assert.Null(saved.Deposit);
    }

    [Theory]
    [InlineData("12.50", false)]
    [InlineData("12.50", true)]
    [InlineData("1.250,50", false)]
    [InlineData("12,5,1", false)]
    [InlineData("-5", false)]
    [InlineData("12 TL", false)]
    [InlineData("1e3", false)]
    [InlineData("+5", false)]
    [InlineData("12,345", false)]
    [InlineData("100000,01", false)]
    [InlineData("12,00000000000000000000000000001", false)]
    [InlineData("1,99999999999999999999999999999", true)]
    public async Task Post_WithAnAmountInAnotherFormat_Is422WithTheAmountMessage_AndNothingIsSaved(string price, bool kestrel)
    {
        await using Host host = new(_database.Dsn, kestrel);

        Answer a = await SendAsync(host, HttpMethod.Post, $"{FormPath}?handler=SavePrice", "en", Form(("Price", price)));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        Assert.Equal(PriceMessage(), ErrorsOf(a.Body)["Price"]);
        Assert.Empty(host.Store.All());

        // What the user typed is shown again as typed (07 2.7.1), not as a value the server read.
        Assert.Contains($"name=\"Price\" value=\"{price}\"", a.Body, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("StartsOn", "04.10.2026")]
    [InlineData("StartsOn", "2026-4-1")]
    [InlineData("StartsOn", "2026-10-04T10:00")]
    [InlineData("StartsOn", "2026-02-30")]
    [InlineData("EndsOn", "10/31/2026")]
    public async Task Post_WithADateInAnotherFormat_Is422WithTheDateMessage(string field, string value)
    {
        await using Host host = new(_database.Dsn, kestrel: false);

        Answer a = await SendAsync(host, HttpMethod.Post, $"{FormPath}?handler=SavePrice", "en", Form((field, value)));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        string label = field == "StartsOn" ? "Başlangıç" : "Bitiş";
        Assert.Equal(FieldMessageCatalog.Build(FieldCodes.InvalidDate, label, null), ErrorsOf(a.Body)[field]);
        Assert.Empty(host.Store.All());
    }

    [Theory]
    [InlineData("0x10")]
    [InlineData("#10")]
    [InlineData("+5")]
    [InlineData("1.000")]
    [InlineData("1,5")]
    [InlineData("1e3")]
    [InlineData("99999999999")]
    public async Task Post_WithAQuantityThatIsNotAsciiDigits_Is422WithTheQuantityMessage(string quantity)
    {
        await using Host host = new(_database.Dsn, kestrel: false);

        Answer a = await SendAsync(host, HttpMethod.Post, $"{FormPath}?handler=SavePrice", "en", Form(("Quantity", quantity)));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        Assert.Equal(
            FieldMessageCatalog.Build(FieldCodes.InvalidQuantity, "Adet", new InvalidQuantityAttribute(FormatFormLimits.QuantityMax).ParameterText),
            ErrorsOf(a.Body)["Quantity"]);
        Assert.Empty(host.Store.All());
    }

    [Fact]
    public async Task Post_AnEmptyValue_IsNullForANullableField_AndBlankRequiredFieldsAreEmpty()
    {
        await using Host host = new(_database.Dsn, kestrel: false);

        // KK6 (07 2.9.10): only blanks in a required text is "boş olamaz"; an empty date the type cannot leave empty is "boş olamaz" too.
        Answer refused = await SendAsync(host, HttpMethod.Post, $"{FormPath}?handler=SavePrice", null, Form(("Note", "   "), ("EndsOn", "  ")));
        Answer saved = await SendAsync(host, HttpMethod.Post, $"{FormPath}?handler=SavePrice", null, Form(("Rate", " "), ("StartsOn", "")));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, refused.Status);
        Dictionary<string, string> errors = ErrorsOf(refused.Body);
        Assert.Equal(FieldMessageCatalog.Build(FieldCodes.FieldRequired, "Not", null), errors["Note"]);
        Assert.Equal(FieldMessageCatalog.Build(FieldCodes.FieldRequired, "Bitiş", null), errors["EndsOn"]);
        Assert.Equal(HttpStatusCode.SeeOther, saved.Status);
        SavedPrice price = Assert.Single(host.Store.All());
        Assert.Null(price.Rate);
        Assert.Null(price.StartsOn);
    }

    [Fact]
    public async Task Post_ADecimalWithoutItsRule_GetsTheAmountCodeWithTheMoneyTypesLimit()
    {
        await using Host host = new(_database.Dsn, kestrel: false);

        Answer a = await SendAsync(host, HttpMethod.Post, $"{FormPath}?handler=SavePrice", null, Form(("Deposit", "12.50")));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, a.Status);
        Assert.Equal(FieldMessageCatalog.Build(FieldCodes.InvalidAmount, "Kapora", DisplayFormat.Amount(Money.MaxAmount)), ErrorsOf(a.Body)["Deposit"]);
    }

    // --- the query: the same binder, the message without the filter, status 200 (07 2.6.6) ---

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Get_AQueryInTheOneFormat_IsApplied_AndAnotherFormat_IsTheMessageWithStatus200(bool kestrel)
    {
        await using Host host = new(_database.Dsn, kestrel);

        Answer applied = await SendAsync(host, HttpMethod.Get, $"{FormPath}?handler=Find&minPrice=12,5&from=2026-10-04", "en");
        Answer refused = await SendAsync(host, HttpMethod.Get, $"{FormPath}?handler=Find&minPrice=12.50&from=04.10.2026", "en");

        Assert.Equal(HttpStatusCode.OK, applied.Status);
        Assert.Contains("<p class=\"applied\">12,5|2026-10-04</p>", applied.Body, StringComparison.Ordinal);
        Assert.Empty(ErrorsOf(applied.Body));

        Assert.Equal(HttpStatusCode.OK, refused.Status);
        Assert.Contains("<p class=\"applied\">none</p>", refused.Body, StringComparison.Ordinal);
        Dictionary<string, string> errors = ErrorsOf(refused.Body);
        Assert.Equal(FieldMessageCatalog.Build(FieldCodes.InvalidAmount, "En düşük fiyat", DisplayFormat.Amount(Money.MaxAmount)), errors["MinPrice"]);
        Assert.Equal(FieldMessageCatalog.Build(FieldCodes.InvalidDate, "Başlangıç", null), errors["From"]);
    }
}
