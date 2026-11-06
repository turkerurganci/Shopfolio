using System.Net;
using System.Text.Json;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Logging;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The single catching layer and the statuses of the pages on the opened application and the test-only sample pages (T212; 09 8.1 items 2-9; 07 2.7;
/// 05 10.3). Every state of the classification is shown with its answer, its error line and the counter; the answers carry no code, no cookie and keep
/// the security headers; an operation is rolled back or, when marked, answered with the 303 wherever it failed.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed class UnexpectedErrorLayerTests
{
    private const string StorefrontPath = "/odeme/hata-ornegi";

    private const string PanelPath = "/panel/hata-ornegi";

    private static readonly string[] _closedCodes = [UnexpectedErrorCodes.UnexpectedPageError, UnexpectedErrorCodes.UnexpectedOperationError, UnexpectedErrorCodes.OutcomeUncertain];

    private readonly CollectionDatabaseFixture _database;

    private readonly string _adminKey;

    public UnexpectedErrorLayerTests(CollectionDatabaseFixture database)
    {
        _database = database;
        _adminKey = TestSessions.AdminAsync(database.Dsn).GetAwaiter().GetResult().Key;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static FormUrlEncodedContent Form(params (string Name, string Value)[] fields) =>
        new(fields.Select(f => new KeyValuePair<string, string>(f.Name, f.Value)));

    private ShopfolioWebApplicationFactory Open()
    {
        ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        return factory;
    }

    private static WebApplicationFactory<Program> WithSamples(ShopfolioWebApplicationFactory factory) =>
        factory.WithWebHostBuilder(b => b.ConfigureServices(s => s.AddRazorPages().AddApplicationPart(typeof(ErrorSampleModel).Assembly)));

    private HttpClient ClientOf(WebApplicationFactory<Program> factory, bool panel = false)
    {
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        if (panel)
        {
            client.DefaultRequestHeaders.Add("Cookie", $"{CookieNames.AdminSession}={_adminKey}");
        }

        return client;
    }

    // --- The classification table (09 8.1 item 3, 9): one row each ---

    [Theory]
    [InlineData("GET", false, false, "PageLevel")]
    [InlineData("HEAD", false, false, "PageLevel")]
    [InlineData("POST", false, false, "OperationLevel")]
    [InlineData("POST", false, true, "OutcomeUncertain")]
    [InlineData("POST", true, false, "MachineEndpoint")]
    [InlineData("GET", true, false, "MachineEndpoint")]
    [InlineData("GET", false, true, "PageLevel")]
    public void Classify_FollowsTheTableOfTheGuideline_KK7(string method, bool machine, bool marked, string expected)
    {
        Assert.Equal(expected, ErrorClassifier.Classify(method, machine, marked).ToString());
    }

    // --- Page level: a GET (05 10.3.2) ---

    [Fact]
    public async Task Get_WhenTheHandlerFails_IsA500ErrorPage_OneLine_TheCounter_NoCode_NoCookie_AndTheSecurityHeaders_KK1_KK7()
    {
        await using ShopfolioWebApplicationFactory baseFactory = Open();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);
        UnexpectedErrorCounter counter = factory.Services.GetRequiredService<UnexpectedErrorCounter>();
        int before = counter.CountWithin(TimeSpan.FromMinutes(15));

        using HttpResponseMessage response = await client.GetAsync(new Uri(StorefrontPath + "?fail=handler", UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("Sayfa şu anda açılamadı", body, StringComparison.Ordinal);
        AssertNoCode(response, body);
        Assert.False(response.Headers.Contains("Set-Cookie"));
        Assert.True(response.Headers.Contains("X-Content-Type-Options"));
        Assert.Equal(before + 1, counter.CountWithin(TimeSpan.FromMinutes(15)));
        JsonElement line = Assert.Single(ErrorLines(baseFactory));
        Assert.Equal(UnexpectedErrorCodes.UnexpectedPageError, line.GetProperty("code").GetString());
        Assert.Equal(nameof(ErrorState.PageLevel), line.GetProperty("state").GetString());
        Assert.Equal("GET", line.GetProperty("method").GetString());
        Assert.Equal(typeof(InvalidOperationException).FullName, line.GetProperty("exceptionType").GetString());
        Assert.DoesNotContain("t212 failure", File.ReadAllText(Directory.GetFiles(baseFactory.LogDirectory, "app-*.jsonl")[0], System.Text.Encoding.UTF8), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Get_WhenThePageFailsWhileItIsDrawn_TheMiddlewareAnswersThe500Page_KK7()
    {
        await using ShopfolioWebApplicationFactory baseFactory = Open();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.GetAsync(new Uri(StorefrontPath + "?fail=render", UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        AssertNoCode(response, await response.Content.ReadAsStringAsync(Ct));
        JsonElement line = Assert.Single(ErrorLines(baseFactory));
        Assert.Equal(UnexpectedErrorCodes.UnexpectedPageError, line.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Get_WhenTheDatabaseTimesOut_Is500_AndTheLineCarriesTheSqlState_D84()
    {
        await using ShopfolioWebApplicationFactory baseFactory = Open();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.GetAsync(new Uri(StorefrontPath + "?fail=timeout", UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        JsonElement line = Assert.Single(ErrorLines(baseFactory));
        Assert.Equal(PostgresErrorCodes.QueryCanceled, line.GetProperty("sqlState").GetString());
        Assert.Equal(UnexpectedErrorCodes.UnexpectedPageError, line.GetProperty("code").GetString());
    }

    // --- Operation level: a POST (05 10.3.3) ---

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Post_WhenTheOperationFails_IsA500_TheFormAgainWithTheSentValues_AMessage_AndTheOperationIsRolledBack_KK7(bool panel)
    {
        await DropAsync(ErrorSampleModel.RolledBackTable);
        await using ShopfolioWebApplicationFactory baseFactory = Open();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory, panel);
        string path = panel ? PanelPath : StorefrontPath;

        using HttpResponseMessage response = await client.PostAsync(new Uri(path + "?handler=Save", UriKind.Relative), Form(("Input.Name", "kept-value")), Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("kept-value", body, StringComparison.Ordinal);
        Assert.Contains(panel ? "kurulumu yapan kişiye" : "İletişim sayfasından", System.Net.WebUtility.HtmlDecode(body), StringComparison.Ordinal);
        AssertNoCode(response, body);
        Assert.False(await TableExistsAsync(ErrorSampleModel.RolledBackTable));
        JsonElement line = Assert.Single(ErrorLines(baseFactory));
        Assert.Equal(UnexpectedErrorCodes.UnexpectedOperationError, line.GetProperty("code").GetString());
        Assert.Equal("POST", line.GetProperty("method").GetString());
        Assert.DoesNotContain("kept-value", File.ReadAllText(Directory.GetFiles(baseFactory.LogDirectory, "app-*.jsonl")[0], System.Text.Encoding.UTF8), StringComparison.Ordinal);
    }

    // --- Uncertain outcome: a marked operation answers 303 wherever it fails (05 10.3.4; 09 8.1 item 4) ---

    [Theory]
    [InlineData("inside", false)]
    [InlineData("after", true)]
    [InlineData("render", true)]
    public async Task Post_OfAMarkedOperation_AnswersTheResultScreen303_WhereverTheErrorFalls_KK8(string mode, bool committed)
    {
        await DropAsync(ErrorSampleModel.RolledBackTable);
        await DropAsync(ErrorSampleModel.CommittedTable);
        await using ShopfolioWebApplicationFactory baseFactory = Open();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(new Uri(StorefrontPath + "?handler=Confirm", UriKind.Relative), Form(("Input.Mode", mode)), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(ErrorSampleModel.ResultScreen, response.Headers.Location?.OriginalString);
        AssertNoCode(response, await response.Content.ReadAsStringAsync(Ct));
        Assert.Equal(committed, await TableExistsAsync(ErrorSampleModel.CommittedTable));
        Assert.False(await TableExistsAsync(ErrorSampleModel.RolledBackTable));
        JsonElement line = Assert.Single(ErrorLines(baseFactory));
        Assert.Equal(UnexpectedErrorCodes.OutcomeUncertain, line.GetProperty("code").GetString());
        Assert.Equal("SampleConfirm", line.GetProperty("operation").GetString());
    }

    [Fact]
    public async Task Post_OfAMarkedOperation_WhoseNamedRefusalIs422_StaysA422_KK8()
    {
        await using ShopfolioWebApplicationFactory baseFactory = Open();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(new Uri(StorefrontPath + "?handler=Confirm", UriKind.Relative), Form(("Input.Mode", "reject")), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Empty(ErrorLines(baseFactory));
    }

    // --- The statuses of the pages (07 2.7.1) ---

    [Fact]
    public async Task Statuses_GetIs200_WritingPostIs303_AClosedPageWithoutASessionIs302_AndNothingIsCounted_KK2()
    {
        await using ShopfolioWebApplicationFactory baseFactory = Open();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);
        using HttpClient anonymous = ClientOf(factory);
        UnexpectedErrorCounter counter = factory.Services.GetRequiredService<UnexpectedErrorCounter>();

        using HttpResponseMessage get = await client.GetAsync(new Uri(StorefrontPath, UriKind.Relative), Ct);
        using HttpResponseMessage post = await client.PostAsync(new Uri(StorefrontPath + "?handler=Plain", UriKind.Relative), Form(("Input.Name", "x")), Ct);
        using HttpResponseMessage closed = await anonymous.GetAsync(new Uri(PanelPath, UriKind.Relative), Ct);

        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Equal(HttpStatusCode.SeeOther, post.StatusCode);
        Assert.Equal(HttpStatusCode.Redirect, closed.StatusCode);
        Assert.Equal(0, counter.CountWithin(TimeSpan.FromMinutes(15)));
        Assert.Empty(ErrorLines(baseFactory));
    }

    [Theory]
    [InlineData("Rule")]
    [InlineData("Upload")]
    public async Task Post_OfANamedRefusal_Is422_WithTheSentValues_AndNoErrorLine_KK3_KK9(string handler)
    {
        await using ShopfolioWebApplicationFactory baseFactory = Open();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);
        UnexpectedErrorCounter counter = factory.Services.GetRequiredService<UnexpectedErrorCounter>();

        using HttpResponseMessage response = await client.PostAsync(new Uri(StorefrontPath + "?handler=" + handler, UriKind.Relative), Form(("Input.Name", "kept-value")), Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        string body = await response.Content.ReadAsStringAsync(Ct);
        Assert.Contains("kept-value", body, StringComparison.Ordinal);
        AssertNoCode(response, body);
        Assert.Equal(0, counter.CountWithin(TimeSpan.FromMinutes(15)));
        Assert.Empty(ErrorLines(baseFactory));
    }

    // --- The framework's own answers (07 2.7.1; D-117): they do not fall into the layer ---

    [Theory]
    [InlineData("PUT", "/odeme/okuma-ornegi", HttpStatusCode.MethodNotAllowed)]
    [InlineData("DELETE", "/odeme/okuma-ornegi", HttpStatusCode.MethodNotAllowed)]
    [InlineData("PATCH", "/odeme/okuma-ornegi", HttpStatusCode.MethodNotAllowed)]
    [InlineData("POST", "/odeme/okuma-ornegi", HttpStatusCode.MethodNotAllowed)]
    [InlineData("PUT", StorefrontPath, HttpStatusCode.MethodNotAllowed)]
    [InlineData("POST", StorefrontPath, HttpStatusCode.BadRequest)]
    public async Task AMethodTheMarkedPageDoesNotTake_Is405_AndAPostWithoutAHandlerOnAPostingPageIs400_D117(string method, string path, HttpStatusCode expected)
    {
        await using ShopfolioWebApplicationFactory baseFactory = Open();
        await using WebApplicationFactory<Program> factory = WithSamples(baseFactory);
        using HttpClient client = ClientOf(factory);
        using HttpRequestMessage request = new(new HttpMethod(method), new Uri(path, UriKind.Relative)) { Content = Form(("Input.Name", "x")) };

        using HttpResponseMessage response = await client.SendAsync(request, Ct);

        Assert.Equal(expected, response.StatusCode);
        if (expected == HttpStatusCode.MethodNotAllowed)
        {
            Assert.Contains("GET", response.Content.Headers.Allow.Concat(response.Headers.GetValues("Allow")).SelectMany(v => v.Split(", ")), StringComparer.Ordinal);
        }

        Assert.Empty(ErrorLines(baseFactory));
    }

    // --- Helpers ---

    private static void AssertNoCode(HttpResponseMessage response, string body)
    {
        string headers = response.ToString();
        foreach (string code in _closedCodes)
        {
            Assert.DoesNotContain(code, body, StringComparison.Ordinal);
            Assert.DoesNotContain(code, headers, StringComparison.Ordinal);
        }
    }

    // The error lines of the application log (event UnexpectedError): the writer keeps its file open, so the reader allows its access.
    private static List<JsonElement> ErrorLines(ShopfolioWebApplicationFactory factory)
    {
        List<JsonElement> lines = [];
        foreach (string path in Directory.GetFiles(factory.LogDirectory, "app-*.jsonl"))
        {
            using FileStream stream = new(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new(stream);
            while (reader.ReadLine() is { } text)
            {
                JsonElement line = JsonDocument.Parse(text).RootElement.Clone();
                if (line.TryGetProperty("event", out JsonElement name) && name.GetString() == LogEvents.UnexpectedError)
                {
                    lines.Add(line);
                }
            }
        }

        return lines;
    }

    private async Task DropAsync(string table)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"DROP TABLE IF EXISTS {table}", connection);
        await command.ExecuteNonQueryAsync(Ct);
    }

    private async Task<bool> TableExistsAsync(string table)
    {
        await using NpgsqlConnection connection = new(_database.Dsn);
        await connection.OpenAsync(Ct);
        await using NpgsqlCommand command = new($"SELECT to_regclass('{table}') IS NOT NULL", connection);
        return (bool)(await command.ExecuteScalarAsync(Ct))!;
    }
}
