using System.Net;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Primitives;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Notifications.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>The collection of the customer sign-in page's tests (T515): one database copied from the template.</summary>
[CollectionDefinition(CustomerSignInPageTestsDefinition.Name)]
public sealed class CustomerSignInPageTestsDefinition : ICollectionFixture<CollectionDatabaseFixture>
{
    public const string Name = "CustomerSignInPageTests";
}

/// <summary>
/// The customer sign-in screen and the return from Google on the opened application (T515 KK1-KK9; 07 UC-50, UC-51, UC-54, UC-55; 04 E-22). Accounts, sessions and rows are real rows of the
/// collection database; Google is a stub that answers what a test queues, so no call leaves the process; the flow's cookie and state are made by the application's own flow.
/// </summary>
[Collection(CustomerSignInPageTestsDefinition.Name)]
public sealed class CustomerSignInPageTests : IAsyncLifetime
{
    private const string Entered = "orchid-lantern-4471-violet";

    private readonly CollectionDatabaseFixture _database;


    private int _counter;

    public CustomerSignInPageTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public async ValueTask InitializeAsync()
    {
        await using AppDbContext db = NewContext();
        await db.Database.ExecuteSqlRawAsync("TRUNCATE customers, link_records, dispatch_records, attempt_records, disposal_records, orders CASCADE", Ct);

        // Until the cookie policy is published the storefront writes no cookie at all (05 6.7; T224): the session's cookies are observed with the policy published.
        await db.Database.ExecuteSqlRawAsync(
            "INSERT INTO legal_text_versions (id, created_at, legal_text_type, version_number, published_at, text) "
            + "VALUES ('01000000-0000-7000-8000-000000515001', now(), 'CookiePolicy', 1, now(), 'policy') ON CONFLICT DO NOTHING",
            Ct);
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    // ---- KK1: the form, the Google button, the closed gate ----

    [Fact]
    public async Task TheForm_HasTheFieldsAndTheLinks_AndNoGoogleButtonWhileGoogleIsUndefined_T515_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);

        (HttpStatusCode status, string html, _) = await GetAsync(factory, FramePaths.SignIn);

        Assert.Equal(HttpStatusCode.OK, status);
        Assert.Contains("name=\"Input.Email\"", html, StringComparison.Ordinal);
        Assert.Contains("type=\"password\"", html, StringComparison.Ordinal);
        Assert.Contains(SignInTexts.ForgotLink, html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{FramePaths.PasswordReset}\"", html, StringComparison.Ordinal);
        Assert.Contains($"href=\"{FramePaths.Register}\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain(SignInTexts.GoogleLabel, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WhenGoogleIsDefined_TheButtonIsThere_T515_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open(google: new StubGateway(), gateOpen: false);

        (_, string html, _) = await GetAsync(factory, FramePaths.SignIn);

        Assert.Contains(SignInTexts.GoogleLabel, html, StringComparison.Ordinal);
    }

    [Fact]
    public async Task WhileTheGateIsClosed_TheSignInStillWorks_T515_KK1()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedCustomerAsync("ali@example.com", verified: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostSignInAsync(client, "ali@example.com", Entered);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Sessions.CountAsync(Ct));
    }

    // ---- KK2: the return place in three cases ----

    [Theory]
    [InlineData("/urun/ahsap-masa", "/urun/ahsap-masa")]
    [InlineData("/giris", "/")]
    [InlineData("//evil.example", "/")]
    [InlineData("", "/")]
    public async Task TheReturnPlace_IsALocalPageOrTheHomePage_T515_KK2(string returnUrl, string expected)
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedCustomerAsync("ali@example.com", verified: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostSignInAsync(client, "ali@example.com", Entered, returnUrl: returnUrl);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(expected, response.Headers.Location!.OriginalString);
    }

    // ---- KK3: a new session key every time, the lifetime of the box, the mark ----

    [Fact]
    public async Task EverySignIn_OpensANewSessionKey_ShortOrLongByTheBox_AndWritesTheMark_T515_KK3()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedCustomerAsync("ali@example.com", verified: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage first = await PostSignInAsync(client, "ali@example.com", Entered);
        using HttpResponseMessage second = await PostSignInAsync(client, "ali@example.com", Entered, remember: true);

        string shortCookie = SessionCookie(first);
        string longCookie = SessionCookie(second);
        Assert.NotEqual(ValueOf(shortCookie), ValueOf(longCookie));
        Assert.True(MaxAgeOf(longCookie) > MaxAgeOf(shortCookie), $"{longCookie} / {shortCookie}");
        Assert.Contains(SetCookies(first), c => c.StartsWith(CookieNames.KnownDevice + "=", StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Assert.Equal(2, await db.Sessions.CountAsync(Ct));
    }

    // ---- KK4: one sentence for the two failures; the limit is neutral and locks nothing ----

    [Fact]
    public async Task AWrongPassword_AndAnUnknownAddress_GiveTheSameSentenceAnd422_AndOpenNoSession_T515_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedCustomerAsync("ali@example.com", verified: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage wrong = await PostSignInAsync(client, "ali@example.com", Entered + "x");
        using HttpResponseMessage unknown = await PostSignInAsync(client, "nobody@example.com", Entered);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, unknown.StatusCode);
        Assert.Contains(SignInTexts.InvalidCredentials, await BodyOf(wrong), StringComparison.Ordinal);
        Assert.Contains(SignInTexts.InvalidCredentials, await BodyOf(unknown), StringComparison.Ordinal);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("not-an-address", "x")]
    public async Task AnEmptyOrMalformedField_Gives422WithoutTheWrongPairSentence_T515_KK4(string email, string password)
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostSignInAsync(client, email, password);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.DoesNotContain(SignInTexts.InvalidCredentials, await BodyOf(response), StringComparison.Ordinal);
    }

    [Fact]
    public async Task TooManyWrongPasswordsOfOneAddress_Give429WithoutLockingTheAccount_AnotherAddressStillSignsIn_T515_KK4()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedCustomerAsync("ali@example.com", verified: true);
        await SeedCustomerAsync("veli@example.com", verified: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        HttpStatusCode last = HttpStatusCode.OK;
        for (int i = 0; i <= ProductConstants.SignInAttemptsPerEmail + 1 && last != HttpStatusCode.TooManyRequests; i++)
        {
            using HttpResponseMessage attempt = await PostSignInAsync(client, "ali@example.com", Entered + "x");
            last = attempt.StatusCode;
        }

        using HttpResponseMessage another = await PostSignInAsync(client, "veli@example.com", Entered);
        using HttpResponseMessage limited = await PostSignInAsync(client, "ali@example.com", Entered + "x");

        Assert.Equal(HttpStatusCode.TooManyRequests, last);
        Assert.Equal(HttpStatusCode.TooManyRequests, limited.StatusCode);
        Assert.DoesNotContain(SignInTexts.InvalidCredentials, await BodyOf(limited), StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.SeeOther, another.StatusCode);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Customers.CountAsync(c => c.Email == "ali@example.com", Ct));
    }

    // ---- KK5: the unverified account ----

    [Fact]
    public async Task TheRightPairOfAnUnverifiedAccount_OpensNoSession_AndTellsTheAwaitedVerification_T515_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedCustomerAsync("ayse@example.com", verified: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostSignInAsync(client, "ayse@example.com", Entered);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string html = await BodyOf(response);
        Assert.Contains(SignInTexts.EmailNotVerified, html, StringComparison.Ordinal);
        Assert.DoesNotContain(SignInTexts.InvalidCredentials, html, StringComparison.Ordinal);
        Assert.DoesNotContain(SetCookies(response), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    [Fact]
    public async Task TheRightPairOfAnAccountWhoseLinkEnded_IsToldAsAWrongPair_T515_KK5()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        await SeedCustomerAsync("ayse@example.com", verified: false, linkEnded: true);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostSignInAsync(client, "ayse@example.com", Entered);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string html = await BodyOf(response);
        Assert.Contains(SignInTexts.InvalidCredentials, html, StringComparison.Ordinal);
        Assert.DoesNotContain(SignInTexts.EmailNotVerified, html, StringComparison.Ordinal);
    }

    // ---- KK6: the start with Google ----

    [Fact]
    public async Task StartSocialLogin_Gives303ToGoogle_WithStateNonceAndTheProofKey_AndTheFlowCookie_T515_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open(google: new StubGateway(), gateOpen: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAsync(client, FramePaths.SignIn, "StartSocialLogin", [("ReturnUrl", "/sepet")]);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Uri location = new(response.Headers.Location!.OriginalString);
        Assert.StartsWith("https://accounts.google.com/o/oauth2/v2/auth?", location.AbsoluteUri, StringComparison.Ordinal);
        Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(location.Query);
        Assert.False(string.IsNullOrEmpty(query["state"]));
        Assert.False(string.IsNullOrEmpty(query["nonce"]));
        Assert.Equal("S256", query["code_challenge_method"].ToString());
        string cookie = Assert.Single(SetCookies(response), c => c.StartsWith(CookieNames.ExternalLogin + "=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(query["nonce"].ToString(), cookie, StringComparison.Ordinal);
    }

    [Fact]
    public async Task StartSocialLogin_WhileGoogleIsUndefined_Gives422_AndNoFlowCookie_T515_KK6()
    {
        await using WebApplicationFactory<Program> factory = Open(google: null, gateOpen: false);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAsync(client, FramePaths.SignIn, "StartSocialLogin", []);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.DoesNotContain(SetCookies(response), c => c.StartsWith(CookieNames.ExternalLogin + "=", StringComparison.Ordinal));
    }

    // ---- KK7: the three results of the return ----

    [Fact]
    public async Task TheReturn_OfAVerifiedAddressOfAnExistingAccount_SignsIn_AndLinksWhenGoogleIsTheAuthority_T515_KK7()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: false);
        Guid id = await SeedCustomerAsync("ali@gmail.com", verified: true);
        (string cookie, string state, string nonce) = StartFlow(factory, "/sepet");
        gateway.Enqueue(new GoogleIdentityResult.Succeeded("ali@gmail.com", true, "Ali", null, null, nonce));

        (HttpStatusCode status, HttpResponseMessage response) = await ReturnAsync(factory, cookie, $"code=c1&state={Uri.EscapeDataString(state)}");

        Assert.Equal(HttpStatusCode.SeeOther, status);
        Assert.Equal("/sepet", response.Headers.Location!.OriginalString);
        Assert.Single(SetCookies(response), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Sessions.CountAsync(Ct));
        Assert.NotNull((await db.Customers.AsNoTracking().SingleAsync(c => c.Id == id, Ct)).GoogleLinkedAt);
    }

    [Fact]
    public async Task TheReturn_OfAnAddressWithNoAccount_WhileTheGateIsOpen_OpensAnAccountWithoutPassword_AndSignsIn_T515_KK7()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        (string cookie, string state, string nonce) = StartFlow(factory, null);
        gateway.Enqueue(new GoogleIdentityResult.Succeeded("yeni@gmail.com", true, "Yeni", null, null, nonce));

        (_, HttpResponseMessage response) = await ReturnAsync(factory, cookie, $"code=c1&state={Uri.EscapeDataString(state)}");

        Assert.Equal("/", response.Headers.Location!.OriginalString);
        await using AppDbContext db = NewContext();
        Customer created = await db.Customers.AsNoTracking().SingleAsync(Ct);
        Assert.Equal("yeni@gmail.com", created.Email);
        Assert.Null(created.PasswordHash);
        Assert.NotNull(created.EmailVerifiedAt);
        Assert.NotNull(created.GoogleLinkedAt);
        Assert.Equal(1, await db.Sessions.CountAsync(Ct));
    }

    [Fact]
    public async Task TheReturn_OfAnUnlinkedAccountWhereGoogleIsNotTheAuthority_OpensNoSession_AndQueuesTheConfirmationLink_T515_KK7()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: false);
        await SeedCustomerAsync("ali@example.com", verified: true);
        (string cookie, string state, string nonce) = StartFlow(factory, null);
        gateway.Enqueue(new GoogleIdentityResult.Succeeded("ali@example.com", true, "Ali", null, null, nonce));

        (_, HttpResponseMessage response) = await ReturnAsync(factory, cookie, $"code=c1&state={Uri.EscapeDataString(state)}");

        Assert.Equal($"{FramePaths.SignIn}?notice={NoticeCode.SocialLoginLinkConfirmationSent}", response.Headers.Location!.OriginalString);
        Assert.DoesNotContain(SetCookies(response), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal));
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
        Assert.Equal(LinkType.GoogleLinkConfirmation, (await db.LinkRecords.AsNoTracking().SingleAsync(Ct)).LinkType);
        Assert.Equal(DispatchType.GoogleLinkConfirmationLink, (await db.DispatchRecords.AsNoTracking().SingleAsync(Ct)).DispatchType);
    }

    // ---- KK8: the two coded redirects ----

    [Fact]
    public async Task TheReturn_OfAnUnverifiedGoogleAddress_Goes303ToTheRegistration_WithTheCode_AndWritesNothing_T515_KK8()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        (string cookie, string state, string nonce) = StartFlow(factory, null);
        gateway.Enqueue(new GoogleIdentityResult.Succeeded("yeni@gmail.com", false, "Yeni", null, null, nonce));

        (_, HttpResponseMessage response) = await ReturnAsync(factory, cookie, $"code=c1&state={Uri.EscapeDataString(state)}");

        Assert.Equal($"{FramePaths.Register}?notice={NoticeCode.SocialLoginEmailUnverified}", response.Headers.Location!.OriginalString);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Customers.CountAsync(Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    [Fact]
    public async Task TheReturn_OfANewAddressWhileTheGateIsClosed_Goes303ToTheSignIn_WithTheCode_AndWritesNothing_T515_KK8()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: false);
        (string cookie, string state, string nonce) = StartFlow(factory, null);
        gateway.Enqueue(new GoogleIdentityResult.Succeeded("yeni@gmail.com", true, "Yeni", null, null, nonce));

        (_, HttpResponseMessage response) = await ReturnAsync(factory, cookie, $"code=c1&state={Uri.EscapeDataString(state)}");

        Assert.Equal($"{FramePaths.SignIn}?notice={NoticeCode.SocialLoginDataCollectionClosed}", response.Headers.Location!.OriginalString);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Customers.CountAsync(Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    // ---- KK9: the three returns that write nothing ----

    [Fact]
    public async Task ARenewedReturn_HasNoFlowCookie_CallsNoGoogle_AndWritesNothing_T515_KK9()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        (string cookie, string state, string nonce) = StartFlow(factory, null);
        gateway.Enqueue(new GoogleIdentityResult.Succeeded("yeni@gmail.com", true, "Yeni", null, null, nonce));
        (_, HttpResponseMessage first) = await ReturnAsync(factory, cookie, $"code=c1&state={Uri.EscapeDataString(state)}");
        Assert.Equal("/", first.Headers.Location!.OriginalString);
        Assert.Single(gateway.Calls);

        // The browser took the deletion the first return sent: the renewed one carries no cookie.
        (_, HttpResponseMessage renewed) = await ReturnAsync(factory, null, $"code=c1&state={Uri.EscapeDataString(state)}");

        Assert.Equal($"{FramePaths.SignIn}?notice={NoticeCode.SocialLoginFailed}", renewed.Headers.Location!.OriginalString);
        Assert.Single(gateway.Calls);
        await using AppDbContext db = NewContext();
        Assert.Equal(1, await db.Sessions.CountAsync(Ct));
    }

    [Theory]
    [InlineData("error=access_denied")]
    [InlineData("code=")]
    public async Task AReturnWithAnErrorOrNoCode_CallsNoGoogle_AndWritesNothing_T515_KK9(string extra)
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        (string cookie, string state, _) = StartFlow(factory, null);

        (_, HttpResponseMessage response) = await ReturnAsync(factory, cookie, $"{extra}&state={Uri.EscapeDataString(state)}");

        Assert.Equal($"{FramePaths.SignIn}?notice={NoticeCode.SocialLoginFailed}", response.Headers.Location!.OriginalString);
        Assert.Empty(gateway.Calls);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Customers.CountAsync(Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    [Theory]
    [InlineData("other-nonce")]
    [InlineData("failed")]
    [InlineData("unavailable")]
    public async Task AWrongNonce_AFailedVerification_OrAnUnreachableGoogle_AreTheSameEnding_AndWriteNothing_T515_KK9(string answer)
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        (string cookie, string state, _) = StartFlow(factory, null);
        gateway.Enqueue(answer switch
        {
            "failed" => (GoogleIdentityResult)new GoogleIdentityResult.Failed("invalid"),
            "unavailable" => new GoogleIdentityResult.Unavailable("timeout"),
            _ => new GoogleIdentityResult.Succeeded("yeni@gmail.com", true, "Yeni", null, null, answer),
        });

        (_, HttpResponseMessage response) = await ReturnAsync(factory, cookie, $"code=c1&state={Uri.EscapeDataString(state)}");

        Assert.Equal($"{FramePaths.SignIn}?notice={NoticeCode.SocialLoginFailed}", response.Headers.Location!.OriginalString);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Customers.CountAsync(Ct));
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    [Fact]
    public async Task ADamagedState_IsTheSameEnding_AndOpensNoSession_T515_KK9()
    {
        StubGateway gateway = new();
        await using WebApplicationFactory<Program> factory = Open(google: gateway, gateOpen: true);
        (string cookie, _, _) = StartFlow(factory, null);

        (_, HttpResponseMessage response) = await ReturnAsync(factory, cookie, "code=c1&state=garbage");

        Assert.Equal($"{FramePaths.SignIn}?notice={NoticeCode.SocialLoginFailed}", response.Headers.Location!.OriginalString);
        Assert.Empty(gateway.Calls);
        await using AppDbContext db = NewContext();
        Assert.Equal(0, await db.Sessions.CountAsync(Ct));
    }

    // ---- helpers ----

    private WebApplicationFactory<Program> Open(StubGateway? google, bool gateOpen) =>
        new ShopfolioWebApplicationFactory(_database.Dsn).WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddRazorPages().AddApplicationPart(typeof(FrameSampleModel).Assembly);
            s.RemoveAll<IReadOnlyList<ConditionalLine>>();
            s.AddSingleton<IReadOnlyList<ConditionalLine>>([new ConditionalLine(ConditionalLine.GoogleSignIn, google is null ? ["Google:ClientId"] : [])]);
            if (google is not null)
            {
                s.Configure<GoogleOptions>(o => o.ClientId = "client-x");
                s.RemoveAll<IGoogleIdentityGateway>();
                s.AddSingleton<IGoogleIdentityGateway>(google);
            }

            if (gateOpen)
            {
                s.RemoveAll<ISettingsModule>();
                s.AddScoped(sp => CartPageTests.OpenGateSettings.Create(ActivatorUtilities.CreateInstance<Shopfolio.Modules.Settings.SettingsModule>(sp)));
            }
        }));

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private async Task<Guid> SeedCustomerAsync(string email, bool verified, bool linkEnded = false)
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        Customer customer = Customer.Create("Ali", email, new PasswordService().Hash(Entered), verified ? now : null, null, now);
        await using AppDbContext db = NewContext();
        db.Customers.Add(customer);
        if (!verified)
        {
            DateTimeOffset ends = linkEnded ? now.AddHours(-1) : now.AddHours(ProductConstants.EmailVerificationLinkHours);
            db.LinkRecords.Add(LinkRecord.ForCustomer(LinkType.EmailVerification, customer.Id, $"hash-{_counter++}-{Guid.NewGuid():N}", ends, now.AddHours(-2)));
        }

        await db.SaveChangesAsync(Ct);
        return customer.Id;
    }

    // Starts a flow with the application's own flow object; returns the cookie header a browser would send back, the state and the nonce.
    private static (string Cookie, string State, string Nonce) StartFlow(WebApplicationFactory<Program> factory, string? returnUrl)
    {
        GoogleSignInFlow flow = factory.Services.GetRequiredService<GoogleSignInFlow>();
        var context = new DefaultHttpContext();
        Uri address = flow.Start(context, GoogleFlowKind.SignInOrRegistration, null, returnUrl);
        Dictionary<string, StringValues> query = QueryHelpers.ParseQuery(address.Query);
        return (context.Response.Headers.SetCookie.ToString().Split(';')[0], query["state"].ToString(), query["nonce"].ToString());
    }

    private static async Task<(HttpStatusCode Status, HttpResponseMessage Response)> ReturnAsync(WebApplicationFactory<Program> factory, string? cookie, string query)
    {
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
        using HttpRequestMessage request = new(HttpMethod.Get, $"/giris/google?{query}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation("Cookie", cookie);
        }

        HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, response);
    }

    private static async Task<(HttpStatusCode Status, string Html, IReadOnlyList<string> SetCookies)> GetAsync(WebApplicationFactory<Program> factory, string path)
    {
        using HttpClient client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false, HandleCookies = false });
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        using HttpResponseMessage response = await client.SendAsync(request, Ct);
        return (response.StatusCode, await BodyOf(response), SetCookies(response));
    }

    private static Task<HttpResponseMessage> PostSignInAsync(HttpClient client, string email, string password, bool remember = false, string? returnUrl = null)
    {
        List<(string Name, string Value)> fields = [("Input.Email", email), ("Input.Password", password)];
        if (remember)
        {
            fields.Add(("Input.IsLongLived", "true"));
        }

        if (returnUrl is not null)
        {
            fields.Add(("Input.ReturnUrl", returnUrl));
        }

        return PostAsync(client, FramePaths.SignIn, "SignIn", fields);
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string handler, IEnumerable<(string Name, string Value)> fields)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, $"{path}?handler={handler}");
        request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
        request.Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));
        return await client.SendAsync(request, Ct);
    }

    private static string SessionCookie(HttpResponseMessage response) =>
        Assert.Single(SetCookies(response), c => c.StartsWith(CookieNames.Session + "=", StringComparison.Ordinal));

    private static string ValueOf(string setCookie) => setCookie.Split(';')[0];

    private static long MaxAgeOf(string setCookie)
    {
        // A cookie without a lifetime lasts the browser session: zero.
        string? part = setCookie.Split(';').Select(p => p.Trim()).FirstOrDefault(p => p.StartsWith("max-age=", StringComparison.OrdinalIgnoreCase));
        return part is null ? 0 : long.Parse(part["max-age=".Length..], System.Globalization.CultureInfo.InvariantCulture);
    }

    // The default encoder writes Turkish letters as character references; the texts are compared as the browser reads them.
    private static async Task<string> BodyOf(HttpResponseMessage response) => WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

    private static List<string> SetCookies(HttpResponseMessage response) => response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values) ? [.. values] : [];

    // Google as a test queues it: the next answer, or a failure when none is queued; every call is kept.
    private sealed class StubGateway : IGoogleIdentityGateway
    {
        private readonly Queue<GoogleIdentityResult> _results = new();
        private readonly List<IdentityCodeExchange> _calls = [];

        public IReadOnlyList<IdentityCodeExchange> Calls
        {
            get
            {
                lock (_calls)
                {
                    return [.. _calls];
                }
            }
        }

        public void Enqueue(GoogleIdentityResult result)
        {
            lock (_calls)
            {
                _results.Enqueue(result);
            }
        }

        public Task<GoogleIdentityResult> VerifyIdentityAsync(IdentityCodeExchange exchange, CancellationToken ct)
        {
            lock (_calls)
            {
                _calls.Add(exchange);
                return Task.FromResult(_results.Count > 0 ? _results.Dequeue() : new GoogleIdentityResult.Failed("not-queued"));
            }
        }
    }
}
