using System.Net;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.IntegrationTests.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The one-time values of the forms on the real database and the sample pages (T209 criteria 2, 3; KK2, KK3; 07 2.5.1, 2.5.2; 05 4.5.1, 6.1.5; K-971, K-1084):
/// the reauthentication's value is written to the session row as a digest, is consumed by one conditional update and a second use finds nothing; a wrong
/// value, a wrong operation or a deleted session consume nothing; a validation error on the form does not consume it; the confirmation key opens one order
/// however many times it is sent and every shown form has a key of its own.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed partial class OneTimeValueTests
{
    private const string ReauthPath = "/hesap/ornek-yeniden-dogrulama";
    private const string PayPath = "/odeme/ornek-onay";

    private readonly CollectionDatabaseFixture _database;

    public OneTimeValueTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex("name=\"Input\\.ReauthenticationToken\"[^>]*?value=\"([^\"]+)\"", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex ReauthField();

    [GeneratedRegex("name=\"ConfirmationKey\"[^>]*?value=\"([^\"]+)\"", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 5000)]
    private static partial Regex KeyField();

    private AppDbContext NewContext()
    {
        DbContextOptionsBuilder<AppDbContext> builder = new();
        AppDbContext.Configure(builder, _database.Dsn);
        return new AppDbContext(builder.Options);
    }

    private WebApplicationFactory<Program> Open(out FormSecurityLog log)
    {
        FormSecurityLog writes = new();
        log = writes;
        ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        return factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddSingleton(writes);
            s.AddRazorPages().AddApplicationPart(typeof(PatternFormModel).Assembly);
        }));
    }

    private static async Task<HttpResponseMessage> GetAsync(HttpClient client, string path, string? cookie)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, path);
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation(HeaderNames.Cookie, cookie);
        }

        return await client.SendAsync(request, Ct);
    }

    private static async Task<HttpResponseMessage> PostAsync(HttpClient client, string path, string? cookie, params (string Name, string Value)[] fields)
    {
        using HttpRequestMessage request = new(HttpMethod.Post, path) { Content = new FormUrlEncodedContent(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value))) };
        if (cookie is not null)
        {
            request.Headers.TryAddWithoutValidation(HeaderNames.Cookie, cookie);
        }

        return await client.SendAsync(request, Ct);
    }

    // --- the value on the session row (07 2.5.2; KK3) ---

    [Fact]
    public async Task AGrantedValue_IsConsumedOnce_AndTheSecondUseFindsNothing_KK3()
    {
        (_, _, Guid sessionId) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        await using AppDbContext db = NewContext();
        ReauthenticationValues values = new(db);

        string? value = await values.GrantAsync(sessionId, ReauthenticationOperation.PasswordChange, Ct);

        Assert.NotNull(value);
        Assert.True(await values.TryRedeemAsync(sessionId, ReauthenticationOperation.PasswordChange, value, Ct));
        Assert.False(await values.TryRedeemAsync(sessionId, ReauthenticationOperation.PasswordChange, value, Ct));
    }

    [Fact]
    public async Task OnlyTheDigestIsStored_AndAWrongValueOrAWrongOperationConsumesNothing_KK3()
    {
        (_, _, Guid sessionId) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        await using AppDbContext db = NewContext();
        ReauthenticationValues values = new(db);
        string value = (await values.GrantAsync(sessionId, ReauthenticationOperation.EmailChange, Ct))!;

        Assert.False(await values.TryRedeemAsync(sessionId, ReauthenticationOperation.EmailChange, "wrong", Ct));
        Assert.False(await values.TryRedeemAsync(sessionId, ReauthenticationOperation.PasswordChange, value, Ct));
        Assert.False(await values.TryRedeemAsync(sessionId, ReauthenticationOperation.EmailChange, null, Ct));
        Assert.False(await values.TryRedeemAsync(sessionId, ReauthenticationOperation.EmailChange, string.Empty, Ct));
        string? stored = await db.Sessions.Where(s => s.Id == sessionId).Select(s => s.ReauthenticationTokenHash).SingleAsync(Ct);
        Assert.Equal(SecretKey.Hash(value), stored);
        Assert.NotEqual(value, stored);
        Assert.True(await values.TryRedeemAsync(sessionId, ReauthenticationOperation.EmailChange, value, Ct));
    }

    [Fact]
    public async Task ANewGrant_ReplacesTheOldValue_AndADeletedSessionHasNoValue_KK3()
    {
        (_, _, Guid sessionId) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        await using AppDbContext db = NewContext();
        ReauthenticationValues values = new(db);
        string first = (await values.GrantAsync(sessionId, ReauthenticationOperation.AccountDeletion, Ct))!;
        string second = (await values.GrantAsync(sessionId, ReauthenticationOperation.AccountDeletion, Ct))!;

        Assert.NotEqual(first, second);
        Assert.False(await values.TryRedeemAsync(sessionId, ReauthenticationOperation.AccountDeletion, first, Ct));
        await db.Sessions.Where(s => s.Id == sessionId).ExecuteDeleteAsync(Ct);
        Assert.False(await values.TryRedeemAsync(sessionId, ReauthenticationOperation.AccountDeletion, second, Ct));
        Assert.Null(await values.GrantAsync(sessionId, ReauthenticationOperation.AccountDeletion, Ct));
    }

    // --- the value in the form of the operation (07 2.5.2; KK3) ---

    [Fact]
    public async Task TheFormCarriesTheValueInAHiddenField_AndTheValidSendConsumesIt_SoTheSecondSendAsksAgain_KK3()
    {
        (string key, _, _) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        string cookie = $"{CookieNames.Session}={key}";
        await using WebApplicationFactory<Program> factory = Open(out FormSecurityLog log);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage page = await GetAsync(client, ReauthPath, cookie);
        string value = ReauthField().Match(await page.Content.ReadAsStringAsync(Ct)).Groups[1].Value;
        Assert.NotEmpty(value);

        using HttpResponseMessage first = await PostAsync(client, $"{ReauthPath}?handler=Apply", cookie, ("Input.Note", "ilk"), ("Input.ReauthenticationToken", value));
        using HttpResponseMessage second = await PostAsync(client, $"{ReauthPath}?handler=Apply", cookie, ("Input.Note", "ikinci"), ("Input.ReauthenticationToken", value));

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal("/hesap", first.Headers.Location?.OriginalString);
        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        Assert.Equal("/hesap/ornek-yeniden-dogrulama?asked=1", second.Headers.Location?.OriginalString);
        Assert.Equal(["applied:ilk"], log.Writes);
    }

    [Fact]
    public async Task AValidationErrorOnTheForm_DoesNotConsumeTheValue_AndTheCorrectedSendPasses_KK3()
    {
        (string key, _, _) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        string cookie = $"{CookieNames.Session}={key}";
        await using WebApplicationFactory<Program> factory = Open(out FormSecurityLog log);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage page = await GetAsync(client, ReauthPath, cookie);
        string value = ReauthField().Match(await page.Content.ReadAsStringAsync(Ct)).Groups[1].Value;

        using HttpResponseMessage invalid = await PostAsync(client, $"{ReauthPath}?handler=Apply", cookie, ("Input.Note", new string('n', 30)), ("Input.ReauthenticationToken", value));
        string shown = ReauthField().Match(await invalid.Content.ReadAsStringAsync(Ct)).Groups[1].Value;
        using HttpResponseMessage corrected = await PostAsync(client, $"{ReauthPath}?handler=Apply", cookie, ("Input.Note", "duzgun"), ("Input.ReauthenticationToken", shown));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, invalid.StatusCode);
        Assert.Equal(value, shown);
        Assert.Equal("/hesap", corrected.Headers.Location?.OriginalString);
        Assert.Equal(["applied:duzgun"], log.Writes);
    }

    [Fact]
    public async Task AValueThatWasNeverGranted_OpensNothing_KK3()
    {
        (string key, _, _) = await TestSessions.CustomerWithSessionAsync(_database.Dsn);
        string cookie = $"{CookieNames.Session}={key}";
        await using WebApplicationFactory<Program> factory = Open(out FormSecurityLog log);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);

        using HttpResponseMessage response = await PostAsync(client, $"{ReauthPath}?handler=Apply", cookie, ("Input.Note", "yok"), ("Input.ReauthenticationToken", SecretKey.Create()));

        Assert.Equal("/hesap/ornek-yeniden-dogrulama?asked=1", response.Headers.Location?.OriginalString);
        Assert.Empty(log.Writes);
    }

    // --- the confirmation key of the payment step (07 2.5.1; KK2) ---

    [Fact]
    public async Task TheSameConfirmationKeySentTwice_OpensOneOrder_AndBothAnswersGoToItsConfirmationScreen_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open(out FormSecurityLog log);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage page = await GetAsync(client, PayPath, null);
        string key = KeyField().Match(await page.Content.ReadAsStringAsync(Ct)).Groups[1].Value;
        Assert.NotEmpty(key);

        using HttpResponseMessage first = await PostAsync(client, $"{PayPath}?handler=Pay", null, (ConfirmationKeys.FieldName, key));
        using HttpResponseMessage second = await PostAsync(client, $"{PayPath}?handler=Pay", null, (ConfirmationKeys.FieldName, key));

        Assert.Equal(HttpStatusCode.SeeOther, first.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, second.StatusCode);
        Assert.Equal(first.Headers.Location, second.Headers.Location);
        Assert.Matches("^/siparis/erisim/[A-Za-z0-9_-]+/teyit$", first.Headers.Location?.OriginalString);
        Assert.Equal(1, log.OrderCount);
    }

    [Fact]
    public async Task TwoShownForms_HaveTwoKeys_AndTwoKeysOpenTwoOrders_KK2()
    {
        await using WebApplicationFactory<Program> factory = Open(out FormSecurityLog log);
        using HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        using HttpResponseMessage tabOne = await GetAsync(client, PayPath, null);
        using HttpResponseMessage tabTwo = await GetAsync(client, PayPath, null);
        string keyOne = KeyField().Match(await tabOne.Content.ReadAsStringAsync(Ct)).Groups[1].Value;
        string keyTwo = KeyField().Match(await tabTwo.Content.ReadAsStringAsync(Ct)).Groups[1].Value;

        using HttpResponseMessage one = await PostAsync(client, $"{PayPath}?handler=Pay", null, (ConfirmationKeys.FieldName, keyOne));
        using HttpResponseMessage two = await PostAsync(client, $"{PayPath}?handler=Pay", null, (ConfirmationKeys.FieldName, keyTwo));

        Assert.NotEqual(keyOne, keyTwo);
        Assert.NotEqual(one.Headers.Location, two.Headers.Location);
        Assert.Equal(2, log.OrderCount);
    }
}
