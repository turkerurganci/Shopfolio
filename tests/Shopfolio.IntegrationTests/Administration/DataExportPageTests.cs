using System.Net;
using System.Text;

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Net.Http.Headers;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Pages;
using Shopfolio.IntegrationTests.Web;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.CorporateContent;
using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Modules.Membership;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Panel;

namespace Shopfolio.IntegrationTests.Administration;

/// <summary>
/// E-53 over HTTP on the opened application (T524 KK5-KK10; 07 UC-107, UC-182-UC-184; D-849, D-850): the screen, the three handlers' 200 with the file and its headers, the file that does not stay in
/// the directory, one trail row per download, the 422s of a wrong range and of an empty set with nothing made and nothing written, and the 500 that writes nothing. The file's content rules are the
/// service tests' in DataExportTests.cs.
/// </summary>
public sealed partial class DataExportTests
{
    private const string ExportPath = "/panel/disa-aktarma";

    [Fact]
    public async Task TheScreen_HasTheResponsibilityLine_AndTheThreeSections_EachWithItsDownload_KK5()
    {
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);

        using HttpResponseMessage response = await client.GetAsync(new Uri(ExportPath, UriKind.Relative), Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(DataExportModel.ResponsibilityText, html, StringComparison.Ordinal);
        Assert.Contains("?handler=ExportOrders\"", html, StringComparison.Ordinal);
        Assert.Contains("?handler=ExportCustomers\"", html, StringComparison.Ordinal);
        Assert.Contains("?handler=ExportContactRequests\"", html, StringComparison.Ordinal);
        Assert.Equal(3, CountOf(html, ">" + DataExportModel.DownloadLabel + "<"));
        Assert.Contains("name=\"StartDate\"", html, StringComparison.Ordinal);
        Assert.Contains("name=\"EndDate\"", html, StringComparison.Ordinal);
        Assert.True(html.IndexOf(DataExportModel.ResponsibilityText, StringComparison.Ordinal) < html.IndexOf("Siparişler</h2>", StringComparison.Ordinal), "The line stands above the sections.");
    }

    [Fact]
    public async Task TheOrderFile_Is200WithTheHeaders_AndTheName_ItsFileDoesNotStay_AndEachDownloadIsATrailRow_KK6_KK7_KK9()
    {
        string number = await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 2, 0m), (_book, 1, 0m));
        await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Failed", (_book, 1, 0m));
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);
        string start = Day(Today.AddDays(-5));
        string end = Day(Today);

        using HttpResponseMessage first = await client.PostAsync(OrdersAddress, Range(start, end), Ct);
        byte[] body = await first.Content.ReadAsByteArrayAsync(Ct);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal("text/csv", first.Content.Headers.ContentType?.MediaType);
        Assert.Equal("utf-8", first.Content.Headers.ContentType?.CharSet);
        ContentDispositionHeaderValue disposition = ContentDispositionHeaderValue.Parse(first.Content.Headers.ContentDisposition!.ToString());
        Assert.Equal("attachment", disposition.DispositionType.Value);
        Assert.Equal($"siparisler-{start}-{end}.csv", disposition.FileName.Value);
        Assert.Contains("no-store", first.Headers.CacheControl!.ToString(), StringComparison.Ordinal);
        Assert.Equal("noindex", first.Headers.GetValues("X-Robots-Tag").Single());
        Assert.Equal(body.Length, first.Content.Headers.ContentLength);
        Assert.Null(first.Headers.Location);

        // A line is an item of a paid order: two items of the one paid order, the header line above them (the failed order is not in the set).
        string[] lines = Encoding.UTF8.GetString(body).TrimStart('﻿').Split("\r\n", StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(3, lines.Length);
        Assert.All(lines.Skip(1), l => Assert.Contains(number, l, StringComparison.Ordinal));

        // The file was deleted when the request ended (D-849) and the row was written once.
        Assert.Empty(Directory.EnumerateFiles(_directory));
        Assert.Equal(1, await TrailCountAsync());

        using HttpResponseMessage second = await client.PostAsync(OrdersAddress, Range(start, end), Ct);
        Assert.Equal(HttpStatusCode.OK, second.StatusCode);
        Assert.Equal(body, await second.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal(2, await TrailCountAsync());
        Assert.Equal(2, await ScalarAsync<long>("SELECT count(*) FROM audit_logs WHERE operation = 'ExportOrders'"));
    }

    [Fact]
    public async Task TheOrderFile_OnARealServer_IsTheServicesFileByteForByte_WithItsLength_AndItsFileIsDeleted_D850()
    {
        await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 2, 0m), (_book, 1, 0m));
        Exported expected = await ExportOrdersAsync(Today.AddDays(-5), Today);
        WebApplicationFactory<Program> factory = OpenPages(services => services.Configure<Microsoft.AspNetCore.Builder.ForwardedHeadersOptions>(options =>
        {
            // The real server sees the test client as a peer on the loopback; the client plays the reverse proxy that says the request came over HTTPS.
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("127.0.0.0/8"));
            options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("::1/128"));
        }));
        byte[] body;
        long? length;
        bool? chunked;
        try
        {
            factory.Start();
            (string key, _) = await TestSessions.AdminAsync(_collection.Dsn);
            using HttpClient probe = factory.CreateClient();
            using HttpClient client = new(new ForgeryTokenHandler { InnerHandler = new ForwardedHttpsHandler { InnerHandler = new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false } } })
            {
                BaseAddress = probe.BaseAddress,
            };
            client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={key}");

            using HttpResponseMessage response = await client.PostAsync(OrdersAddress, Range(Day(Today.AddDays(-5)), Day(Today)), Ct);
            body = await response.Content.ReadAsByteArrayAsync(Ct);
            length = response.Content.Headers.ContentLength;
            chunked = response.Headers.TransferEncodingChunked;
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        finally
        {
            // Stopping the server waits for the request's end, where the response's registered stream is closed (09 10.10 item 7).
            await factory.DisposeAsync();
        }

        // Kestrel sends the declared length, not a chunked body, and the bytes are the module's file as it made it (D-850; 09 10.10 item 2).
        Assert.Equal(expected.Bytes, body);
        Assert.Equal(body.Length, length);
        Assert.NotEqual(true, chunked);
        Assert.Empty(Directory.EnumerateFiles(_directory));
        Assert.Equal(2, await TrailCountAsync()); // the module's own export above and the download, a row each
    }

    [Theory]
    [InlineData("", "today")]
    [InlineData("start", "")]
    [InlineData("tomorrow", "tomorrow")]
    [InlineData("start", "tomorrow")]
    [InlineData("today", "start")]
    [InlineData("notadate", "today")]
    public async Task AWrongRange_Is422WithAFieldMessage_NoFileAndNoRow_KK10(string startCase, string endCase)
    {
        await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Paid", (_shirt, 1, 0m));
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);

        using HttpResponseMessage response = await client.PostAsync(OrdersAddress, Range(DayOf(startCase), DayOf(endCase)), Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("alanda hata var", html, StringComparison.Ordinal);
        Assert.Contains(DataExportModel.ResponsibilityText, html, StringComparison.Ordinal);
        Assert.Equal(0, await TrailCountAsync());
        Assert.Empty(Directory.EnumerateFiles(_directory));
    }

    [Fact]
    public async Task ARangeWithNoPaidOrder_Is422WithTheSectionsMessage_NoFileAndNoRow_KK10()
    {
        await SeedAsync(CreatedOn(-3), PaymentMethod.Card, "Failed", (_shirt, 1, 0m));
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);

        using HttpResponseMessage response = await client.PostAsync(OrdersAddress, Range(Day(Today.AddDays(-5)), Day(Today)), Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Contains(DataExportModel.NoOrdersText, html, StringComparison.Ordinal);
        Assert.Contains($"value=\"{Day(Today.AddDays(-5))}\"", html, StringComparison.Ordinal);
        Assert.Equal(0, await TrailCountAsync());
        Assert.Empty(Directory.EnumerateFiles(_directory));
    }

    [Fact]
    public async Task TheMemberAndContactFiles_CarryEveryAccountAndEveryKeptRequest_NamedByTheDay_EachARow_KK8_KK9()
    {
        DateTimeOffset now = DateTimeOffset.UtcNow;
        await SaveAsync(c => c.Customers.AddRange(
            Customer.Create("Ali Veli", "ali@example.com", "digest-placeholder", now, null, now),
            Customer.Create("Bekleyen", "bekleyen@example.com", "digest-placeholder", null, null, now)));
        await SaveAsync(c => c.ContactRequests.Add(ContactRequest.Create("+Ayşe", "ayse@example.com", ContactTopic.GeneralQuestion, "Merhaba", now.AddDays(-1), null)));
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);

        using HttpResponseMessage members = await client.PostAsync(new Uri(ExportPath + "?handler=ExportCustomers", UriKind.Relative), new FormUrlEncodedContent([]), Ct);
        using HttpResponseMessage requests = await client.PostAsync(new Uri(ExportPath + "?handler=ExportContactRequests", UriKind.Relative), new FormUrlEncodedContent([]), Ct);
        string memberText = Encoding.UTF8.GetString(await members.Content.ReadAsByteArrayAsync(Ct));
        string requestText = Encoding.UTF8.GetString(await requests.Content.ReadAsByteArrayAsync(Ct));

        Assert.Equal(HttpStatusCode.OK, members.StatusCode);
        Assert.Equal($"uyeler-{Day(Today)}.csv", ContentDispositionHeaderValue.Parse(members.Content.Headers.ContentDisposition!.ToString()).FileName.Value);
        Assert.Contains("ali@example.com", memberText, StringComparison.Ordinal);
        Assert.Contains("bekleyen@example.com", memberText, StringComparison.Ordinal);
        Assert.Equal(HttpStatusCode.OK, requests.StatusCode);
        Assert.Equal($"iletisim-talepleri-{Day(Today)}.csv", ContentDispositionHeaderValue.Parse(requests.Content.Headers.ContentDisposition!.ToString()).FileName.Value);
        Assert.Contains("ayse@example.com", requestText, StringComparison.Ordinal);

        // A cell of the customer's input that begins with a formula character is made inert (K-1217 (3)).
        Assert.Contains("'+Ayşe", requestText, StringComparison.Ordinal);
        Assert.Equal(2, await TrailCountAsync());
        Assert.Empty(Directory.EnumerateFiles(_directory));
    }

    [Fact]
    public async Task AnEmptyMemberOrContactList_Is422WithTheSectionsMessage_AndWritesNothing_KK10()
    {
        await using WebApplicationFactory<Program> factory = OpenPages();
        using HttpClient client = await PanelClientAsync(factory);

        using HttpResponseMessage members = await client.PostAsync(new Uri(ExportPath + "?handler=ExportCustomers", UriKind.Relative), new FormUrlEncodedContent([]), Ct);
        using HttpResponseMessage requests = await client.PostAsync(new Uri(ExportPath + "?handler=ExportContactRequests", UriKind.Relative), new FormUrlEncodedContent([]), Ct);
        string memberHtml = WebUtility.HtmlDecode(await members.Content.ReadAsStringAsync(Ct));
        string requestHtml = WebUtility.HtmlDecode(await requests.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, members.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, requests.StatusCode);
        Assert.Contains("id=\"export-customers-empty\"", memberHtml, StringComparison.Ordinal);
        Assert.Contains("id=\"export-contact-requests-empty\"", requestHtml, StringComparison.Ordinal);
        Assert.Contains(DataExportModel.NothingToWriteText, memberHtml, StringComparison.Ordinal);
        Assert.Equal(0, await TrailCountAsync());
    }

    [Fact]
    public async Task AFileThatCannotBeMade_Is500WithTheOperationMessageInTheSection_AndWritesNoRow_KK10()
    {
        await using WebApplicationFactory<Program> factory = OpenPages(services =>
        {
            services.RemoveAll<IDataExport>();
            services.AddScoped<IDataExport, FailingExport>();
        });
        using HttpClient client = await PanelClientAsync(factory);
        string start = Day(Today.AddDays(-5));

        using HttpResponseMessage response = await client.PostAsync(OrdersAddress, Range(start, Day(Today)), Ct);
        string html = WebUtility.HtmlDecode(await response.Content.ReadAsStringAsync(Ct));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Contains("id=\"export-orders-error\"", html, StringComparison.Ordinal);
        Assert.Contains(UnexpectedErrorAnswer.PanelOperationMessage, html, StringComparison.Ordinal);
        Assert.Contains($"value=\"{start}\"", html, StringComparison.Ordinal);
        Assert.Equal(0, await TrailCountAsync());
    }

    private static Uri OrdersAddress => new(ExportPath + "?handler=ExportOrders", UriKind.Relative);

    private static string DayOf(string name) => name switch
    {
        "" => string.Empty,
        "today" => Day(Today),
        "tomorrow" => Day(Today.AddDays(1)),
        "start" => Day(Today.AddDays(-5)),
        _ => name,
    };

    private static FormUrlEncodedContent Range(string start, string end) => new([new("StartDate", start), new("EndDate", end)]);

    private static int CountOf(string text, string part)
    {
        int count = 0;
        for (int at = text.IndexOf(part, StringComparison.Ordinal); at >= 0; at = text.IndexOf(part, at + part.Length, StringComparison.Ordinal))
        {
            count++;
        }

        return count;
    }

    private WebApplicationFactory<Program> OpenPages(Action<IServiceCollection>? configure = null) =>
        new ShopfolioWebApplicationFactory(_collection.Dsn, new Dictionary<string, string> { ["Export:TempDirectory"] = _directory }, withJobRunner: false)
            .WithWebHostBuilder(b => b.ConfigureServices(s =>
            {
                s.AddRazorPages().AddApplicationPart(typeof(ForgeryTokenSampleModel).Assembly);
                configure?.Invoke(s);
            }));

    private async Task<HttpClient> PanelClientAsync(WebApplicationFactory<Program> factory)
    {
        (string key, _) = await TestSessions.AdminAsync(_collection.Dsn);
        HttpClient client = ForgeryTokenHandler.CreateClient(factory);
        client.DefaultRequestHeaders.Add(HeaderNames.Cookie, $"{CookieNames.AdminSession}={key}");
        return client;
    }

    /// <summary>Says to the real server that the request came over HTTPS through the trusted proxy (the loopback), as the site's cookies and its forgery token need.</summary>
    private sealed class ForwardedHttpsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.TryAddWithoutValidation("X-Forwarded-Proto", "https");
            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class FailingExport : IDataExport
    {
        public Task<ExportFileResult> ExportOrdersAsync(DateOnly? startDay, DateOnly? endDay, CancellationToken ct) => throw new InvalidOperationException("The file could not be made.");

        public Task<ExportFileResult> ExportCustomersAsync(CancellationToken ct) => throw new InvalidOperationException("The file could not be made.");

        public Task<ExportFileResult> ExportContactRequestsAsync(CancellationToken ct) => throw new InvalidOperationException("The file could not be made.");
    }
}
