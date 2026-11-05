using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Routing;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Pages.Panel;

namespace Shopfolio.IntegrationTests.Web;

/// <summary>
/// The page-form pattern on the opened application and a test-only sample page (T206 criteria 2, 5-9; KK2, KK5-KK9; 07 2.1.2, 2.1.7, 2.2, 2.7.1, 2.10):
/// the first path segment is the closed list on every mapped endpoint, a page answers with another letter case and a trailing slash without a
/// redirect, a POST reaches its handler by the <c>handler</c> parameter, a writing POST is a 303, a refused one is shown again with 422, the export is a
/// CSV, a GET changes nothing, a changed static file gets a new address and there is no API or version. The sample page lives in the test host only.
/// </summary>
[Collection(DatabaseInfrastructureDefinition.Name)]
public sealed partial class PageFormPatternTests
{
    private const string PagePath = "/panel/ornek-form";

    private readonly CollectionDatabaseFixture _database;

    public PageFormPatternTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private WebApplicationFactory<Program> Open(out PatternRecordStore store)
    {
        PatternRecordStore records = new();
        store = records;
        ShopfolioWebApplicationFactory factory = new(_database.Dsn);
        return factory.WithWebHostBuilder(b => b.ConfigureServices(s =>
        {
            s.AddSingleton(records);
            s.AddRazorPages().AddApplicationPart(typeof(PatternFormModel).Assembly);
        }));
    }

    private static HttpClient ClientOf(WebApplicationFactory<Program> factory) =>
        factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false, HandleCookies = false, BaseAddress = new Uri("https://localhost") });

    private static FormUrlEncodedContent Form(string name) => new([new KeyValuePair<string, string>("Input.Name", name)]);

    // --- KK2: the first path segment is the closed list ---

    [Fact]
    public async Task EveryMappedEndpoint_StartsWithAFirstSegmentOfTheClosedList()
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);
        await client.GetAsync(PagePath, Ct);

        List<string> patterns = [.. AllRoutePatterns(factory)];

        Assert.Contains(patterns, p => p == PagePath);
        Assert.Contains(patterns, p => p == "/sistem/canlilik");
        Assert.Contains(patterns, p => p.StartsWith(AddressRules.StaticArea + "/", StringComparison.Ordinal));
        // The static files carry the file's own name, so only their first segment is checked (07 2.1.2, 3.1.6).
        Assert.Empty(patterns.Where(p => !p.StartsWith(AddressRules.StaticArea + "/", StringComparison.Ordinal)).SelectMany(p => AddressTemplates.Check(p).Select(problem => $"{p}: {problem}")));
        Assert.All(patterns, p => Assert.True(AddressRules.IsClosedFirstSegment(FirstSegment(p)), p));
    }

    [Theory]
    [InlineData("/ahsap-masa")]
    [InlineData("/product/ahsap-masa")]
    [InlineData("/api/orders")]
    [InlineData("/v1/orders")]
    [InlineData("/css/base.css")]
    [InlineData("/js/common.js")]
    [InlineData("/admin")]
    [InlineData("/yonetim/siparisler")]
    [InlineData("/tr/sepet")]
    public async Task Get_OnAFirstSegmentOutsideTheList_Is404(string path)
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.GetAsync(path, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // --- KK5: letter case and the trailing slash ---

    [Theory]
    [InlineData("/panel/ornek-form")]
    [InlineData("/PANEL/Ornek-Form")]
    [InlineData("/panel/ornek-form/")]
    [InlineData("/Panel/ORNEK-FORM/")]
    public async Task Get_WithAnotherLetterCaseOrATrailingSlash_ReturnsTheSamePageWithoutARedirect(string path)
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.GetAsync(path, Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Contains("<h1>Ornek form</h1>", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Post_WithAnotherLetterCaseAndATrailingSlash_ReachesTheHandlerAndDoesNotRedirectOnTheWay()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync("/PANEL/Ornek-Form/?handler=Add", Form("masa"), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(["masa"], store.Snapshot());
    }

    // --- KK6: the handler is named by the handler parameter ---

    [Fact]
    public async Task Post_WithTheHandlerParameter_ReachesTheNamedHandlerOnly()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage refused = await client.PostAsync(PagePath + "?handler=Refuse", new FormUrlEncodedContent([]), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, refused.StatusCode);
        Assert.Equal("/panel/ornek-form?notice=refused", refused.Headers.Location?.OriginalString);
        Assert.Empty(store.Snapshot());
    }

    [Fact]
    public async Task Post_WithAnUnknownHandler_IsNotAnswered200AndWritesNothing()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=Delete", Form("masa"), Ct);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
        Assert.NotEqual(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Empty(store.Snapshot());
    }

    [Fact]
    public async Task Post_WithoutAHandler_WritesNothing_AndIsTheFrameworksAnswer()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(PagePath, Form("masa"), Ct);

        // 07 2.7.1: a POST without a handler is the framework's answer (a page without an unnamed OnPost shows no write).
        Assert.NotEqual(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Empty(store.Snapshot());
    }

    // --- KK7: the writing POST is a 303, the refused one is shown again with 422 ---

    [Fact]
    public async Task Post_ThatWrites_Answers303ToTheGet_AndTheGetShowsTheRecord()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=Add", Form("masa"), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(PagePath, response.Headers.Location?.OriginalString);
        Assert.Equal(["masa"], store.Snapshot());
        using HttpResponseMessage get = await client.GetAsync(response.Headers.Location, Ct);
        Assert.Equal(HttpStatusCode.OK, get.StatusCode);
        Assert.Contains("<li class=\"record\">masa</li>", await get.Content.ReadAsStringAsync(Ct), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("")]
    [InlineData("bu-ad-yirmi-karakterden-uzundur")]
    public async Task Post_ThatFailsValidation_DoesNotRedirect_Answers422_AndShowsTheSentValues(string name)
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ClientOf(factory);

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=Add", Form(name), Ct);
        string body = await response.Content.ReadAsStringAsync(Ct);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Contains($"value=\"{name}\"", body, StringComparison.Ordinal);
        Assert.Contains("field-validation-error", body, StringComparison.Ordinal);
        Assert.Empty(store.Snapshot());
    }

    [Fact]
    public async Task Post_ThatNamesItsReasonByARedirect_Answers303_AndWritesNothing()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ClientOf(factory);
        store.Add("one");

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=Refuse", new FormUrlEncodedContent([]), Ct);

        Assert.Equal(HttpStatusCode.SeeOther, response.StatusCode);
        Assert.Equal(["one"], store.Snapshot());
    }

    [Fact]
    public async Task Export_Answers200WithTheCsvAsAnAttachment_AndDoesNotRedirect()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ClientOf(factory);
        store.Add("masa");
        store.Add("sandalye");

        using HttpResponseMessage response = await client.PostAsync(PagePath + "?handler=Export", new FormUrlEncodedContent([]), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.Headers.Location);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", response.Content.Headers.ContentType?.CharSet);
        ContentDispositionHeaderValue disposition = response.Content.Headers.ContentDisposition!;
        Assert.Equal("attachment", disposition.DispositionType);
        Assert.Equal("ornek-liste.csv", disposition.FileName);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
        Assert.Equal("noindex", Assert.Single(response.Headers.GetValues("X-Robots-Tag")));
        Assert.Equal("masa\nsandalye", await response.Content.ReadAsStringAsync(Ct));
    }

    // --- KK8: a GET changes no record ---

    [Fact]
    public async Task Get_ChangesNoRecord_NotEvenWithAHandlerOrTheFormValuesInTheQuery()
    {
        await using WebApplicationFactory<Program> factory = Open(out PatternRecordStore store);
        using HttpClient client = ClientOf(factory);
        store.Add("one");

        foreach (string path in new[] { PagePath, PagePath + "?handler=Add&Input.Name=sneaky", PagePath + "?handler=Export", PagePath + "?Input.Name=sneaky" })
        {
            using HttpResponseMessage response = await client.GetAsync(path, Ct);
            Assert.NotEqual(HttpStatusCode.SeeOther, response.StatusCode);
            Assert.Equal(["one"], store.Snapshot());
        }
    }

    // --- Criterion 9: static files get a new address with their content; no API, no version ---

    [Fact]
    public async Task StaticFiles_AreUnderStatik_AndEachHasAnAddressDerivedFromItsContent()
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);
        string webRoot = Path.Combine(RepositoryRoot(), "src", "Shopfolio.Web", "wwwroot");
        string[] files = [.. Directory.EnumerateFiles(webRoot, "*", SearchOption.AllDirectories).Select(f => Path.GetRelativePath(webRoot, f).Replace('\\', '/'))];
        Assert.NotEmpty(files);
        HashSet<string> patterns = [.. AllRoutePatterns(factory)];
        HashSet<string> fingerprints = [];

        foreach (string file in files)
        {
            string plain = $"{AddressRules.StaticArea}/{file}";
            string extension = Path.GetExtension(file);
            Regex fingerprinted = new("^" + Regex.Escape($"{AddressRules.StaticArea}/{file[..^extension.Length]}.") + "([a-z0-9]{10})" + Regex.Escape(extension) + "$", RegexOptions.CultureInvariant, TimeSpan.FromSeconds(5));
            string address = Assert.Single(patterns.Where(p => fingerprinted.IsMatch(p)));
            fingerprints.Add(fingerprinted.Match(address).Groups[1].Value);

            using HttpResponseMessage current = await client.GetAsync(address, Ct);
            using HttpResponseMessage unchanged = await client.GetAsync(plain, Ct);

            Assert.Equal(HttpStatusCode.OK, current.StatusCode);
            Assert.Contains("immutable", current.Headers.CacheControl?.ToString(), StringComparison.Ordinal);
            string contentHash = Convert.ToBase64String(SHA256.HashData(await File.ReadAllBytesAsync(Path.Combine(webRoot, file), Ct)));
            Assert.Equal($"\"{contentHash}\"", current.Headers.ETag?.Tag);
            Assert.Equal(HttpStatusCode.OK, unchanged.StatusCode);
            Assert.DoesNotContain("immutable", unchanged.Headers.CacheControl?.ToString() ?? string.Empty, StringComparison.Ordinal);
        }

        Assert.Equal(files.Length, fingerprints.Count);
    }

    [Fact]
    public async Task Application_HasNoApiAndNoVersionInTheAddressTheHeaderOrTheParameter()
    {
        await using WebApplicationFactory<Program> factory = Open(out _);
        using HttpClient client = ClientOf(factory);
        using HttpResponseMessage response = await client.GetAsync(PagePath, Ct);

        foreach (string pattern in AllRoutePatterns(factory))
        {
            Assert.DoesNotMatch(VersionOrApiSegment(), pattern);
        }

        Assert.DoesNotContain(response.Headers.Select(h => h.Key), n => n.Contains("version", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(AppDomain.CurrentDomain.GetAssemblies(), a => a.GetName().Name!.StartsWith("Asp.Versioning", StringComparison.OrdinalIgnoreCase));
    }

    // --- D-73: the class the Razor compiler makes of a .cshtml is not in the namespace of the page rule ---

    [Fact]
    public void RazorCompiler_PutsACompiledPageInTheGeneratedDocumentNamespace()
    {
        Type[] generated = [.. typeof(PatternFormModel).Assembly.GetTypes().Where(t => t.Name.Contains("PatternForm", StringComparison.Ordinal) && t.Namespace == "AspNetCoreGeneratedDocument")];

        Assert.NotEmpty(generated);
        Assert.Contains(generated, t => t.Name == "Pages_Panel_PatternForm");
    }

    private static IEnumerable<string> AllRoutePatterns(WebApplicationFactory<Program> factory) =>
        factory.Services.GetServices<EndpointDataSource>()
            .SelectMany(d => d.Endpoints)
            .OfType<RouteEndpoint>()
            .Select(e => "/" + (e.RoutePattern.RawText ?? string.Empty).TrimStart('/'))
            .Distinct(StringComparer.Ordinal);

    private static string FirstSegment(string pattern)
    {
        string[] parts = pattern.Trim('/').Split('/');
        return parts[0];
    }

    private static string RepositoryRoot() => Shopfolio.UnitTests.Support.RepositoryRoot.Find();

    [GeneratedRegex(@"(^|/)(api|v[0-9]+|\{version[^}]*\}|\{api-version[^}]*\})(/|$)", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase, matchTimeoutMilliseconds: 5000)]
    private static partial Regex VersionOrApiSegment();
}
