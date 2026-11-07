using System.Net;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Files;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.Modules.Catalog.Contracts;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// UC-37 on the opened application and a real PostgreSQL (T312 criteria 1 to 4 and 10; KK1 to KK4, KK10; 07 4.31.1; 09 15.7 items 1 to 3): the signed address opens with no session;
/// the three reasons for the 404 (a bad signature, a spent life, an item that cannot be downloaded) and a file that did not complete are all the same empty answer; the answer
/// is the variant's current file as an attachment of a general binary type with <c>no-store</c>, <c>nosniff</c>, a strong tag that shows no storage key, <c>Last-Modified</c> and
/// <c>X-Robots-Tag</c>; a changed file is a new file and a new tag, a range is 206 and an <c>If-Range</c> of an old tag gets the whole file; the downloads left to the buyer
/// do not change and the trace gets no line; a life that ends while the file is sent does not cut the transfer, and the next range request is 404; a file missing from the
/// store is an unexpected error (500, no body).
/// </summary>
[Collection(FileEndpointsDefinition.Name)]
public sealed partial class DownloadEndpointTests
{
    private static readonly byte[] Hundred = [.. Enumerable.Range(0, 100).Select(i => (byte)i)];

    private static readonly byte[] Other = [.. Enumerable.Range(0, 60).Select(i => (byte)(255 - i))];

    private readonly CollectionDatabaseFixture _database;

    public DownloadEndpointTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [GeneratedRegex("^\"[0-9a-f]{32}\"$", RegexOptions.CultureInvariant, matchTimeoutMilliseconds: 1000)]
    private static partial Regex StrongTag();

    private static Uri Link(string value) => new("/indir/" + value, UriKind.Relative);

    private static async Task AssertEmpty404Async(HttpResponseMessage response)
    {
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync(Ct));
    }

    // --- KK1 and the headers (criteria 1, 3, 10) ---

    [Fact]
    public async Task Download_OfASignedValue_OpensWithNoSession_AndGivesTheVariantsFileWithTheRightHeaders()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred, "kitap sayfa.pdf");

        using HttpResponseMessage response = await host.Client.GetAsync(Link(host.LinkFor(item)), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(Hundred, await response.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal("application/octet-stream", response.Content.Headers.ContentType?.MediaType);
        Assert.Equal("attachment", response.Content.Headers.ContentDisposition?.DispositionType);
        Assert.Equal("kitap sayfa.pdf", response.Content.Headers.ContentDisposition?.FileName?.Trim('"'));
        Assert.Contains("no-store", string.Join(',', response.Headers.GetValues("Cache-Control")), StringComparison.Ordinal);
        Assert.Equal("nosniff", Assert.Single(response.Headers.GetValues("X-Content-Type-Options")));
        Assert.Equal("noindex", Assert.Single(response.Headers.GetValues("X-Robots-Tag")));
        Assert.Equal("bytes", Assert.Single(response.Headers.AcceptRanges));
        Assert.NotNull(response.Content.Headers.LastModified);
        EntityTagHeaderValue tag = Assert.IsType<EntityTagHeaderValue>(response.Headers.ETag);
        Assert.False(tag.IsWeak);
        Assert.Matches(StrongTag(), tag.Tag);
    }

    [Fact]
    public async Task Download_TagAndHeaders_NeverShowTheStorageKey()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, Guid fileId) = await host.SeedDownloadAsync(Hundred);
        await using Shopfolio.Infrastructure.AppDbContext db = host.NewContext();
        string key = await db.DigitalFiles.Where(f => f.Id == fileId).Select(f => f.StorageKey).SingleAsync(Ct);

        using HttpResponseMessage response = await host.Client.GetAsync(Link(host.LinkFor(item)), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        string headers = response.Headers.ToString() + response.Content.Headers;
        Assert.DoesNotContain(key, headers, StringComparison.OrdinalIgnoreCase);
    }

    // --- KK2: the three 404 reasons, apart (criterion 2) ---

    [Fact]
    public async Task Download_WithABadSignature_Is404WithNoBody()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred);
        string value = host.LinkFor(item);
        string tampered = value[..^2] + (value[^2] == 'A' ? 'B' : 'A') + value[^1];

        using HttpResponseMessage response = await host.Client.GetAsync(Link(tampered), Ct);
        using HttpResponseMessage garbage = await host.Client.GetAsync(Link("not-a-signed-value"), Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertEmpty404Async(response);
        await AssertEmpty404Async(garbage);
    }

    [Fact]
    public async Task Download_WithASpentLife_Is404WithNoBody_AndTheLastSecondOfTheLifeStillOpens()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred);
        string value = host.LinkFor(item);

        host.Time.Advance(TimeSpan.FromMinutes(59));
        using HttpResponseMessage inside = await host.Client.GetAsync(Link(value), Ct);
        host.Time.Advance(TimeSpan.FromMinutes(2));
        using HttpResponseMessage spent = await host.Client.GetAsync(Link(value), Ct);

        Assert.Equal(HttpStatusCode.OK, inside.StatusCode);
        await AssertEmpty404Async(spent);
    }

    [Fact]
    public async Task Download_OfAnItemThatCannotBeDownloaded_Is404WithNoBody()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid physical, _, _) = await host.SeedDownloadAsync(Hundred, itemType: ProductType.Physical);

        using HttpResponseMessage notDigital = await host.Client.GetAsync(Link(host.LinkFor(physical)), Ct);
        using HttpResponseMessage unknownItem = await host.Client.GetAsync(Link(host.LinkFor(Guid.NewGuid())), Ct);

        Assert.Equal(HttpStatusCode.NotFound, notDigital.StatusCode);
        await AssertEmpty404Async(notDigital);
        await AssertEmpty404Async(unknownItem);
    }

    [Fact]
    public async Task Download_OfAFileWhoseUploadDidNotComplete_Is404WithNoBody()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        Guid item = await host.SeedItemOfAnIncompleteFileAsync();

        using HttpResponseMessage response = await host.Client.GetAsync(Link(host.LinkFor(item)), Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertEmpty404Async(response);
    }

    // --- KK3: a changed file, the range (criterion 3) ---

    [Fact]
    public async Task Download_AfterTheFileIsReplaced_GivesTheNewFileUnderANewTag()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, Guid variant, _) = await host.SeedDownloadAsync(Hundred, "eski.pdf");
        string value = host.LinkFor(item);
        using HttpResponseMessage before = await host.Client.GetAsync(Link(value), Ct);

        await host.ReplaceFileAsync(variant, Other, "yeni.pdf");
        using HttpResponseMessage after = await host.Client.GetAsync(Link(value), Ct);

        Assert.Equal(Hundred, await before.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal(Other, await after.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal("yeni.pdf", after.Content.Headers.ContentDisposition?.FileName?.Trim('"'));
        Assert.NotEqual(before.Headers.ETag?.Tag, after.Headers.ETag?.Tag);
    }

    [Fact]
    public async Task Download_WithARange_Is206WithTheSpan_AndAnIfRangeOfAnOldTagGetsTheWholeFile()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, Guid variant, _) = await host.SeedDownloadAsync(Hundred);
        string value = host.LinkFor(item);
        using HttpResponseMessage whole = await host.Client.GetAsync(Link(value), Ct);
        EntityTagHeaderValue oldTag = whole.Headers.ETag!;

        using HttpResponseMessage part = await Ranged(host, value, 10, 19, null);
        using HttpResponseMessage current = await Ranged(host, value, 10, 19, oldTag);
        await host.ReplaceFileAsync(variant, Other);
        using HttpResponseMessage stale = await Ranged(host, value, 10, 19, oldTag);

        Assert.Equal(HttpStatusCode.PartialContent, part.StatusCode);
        Assert.Equal(Hundred[10..20], await part.Content.ReadAsByteArrayAsync(Ct));
        Assert.Equal(new ContentRangeHeaderValue(10, 19, 100), part.Content.Headers.ContentRange);
        Assert.Equal(HttpStatusCode.PartialContent, current.StatusCode);
        Assert.Equal(HttpStatusCode.OK, stale.StatusCode);
        Assert.Equal(Other, await stale.Content.ReadAsByteArrayAsync(Ct));
    }

    [Fact]
    public async Task Download_WithARangeBeyondTheFile_Is416()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred);

        using HttpResponseMessage response = await Ranged(host, host.LinkFor(item), 500, 600, null);

        Assert.Equal(HttpStatusCode.RequestedRangeNotSatisfiable, response.StatusCode);
    }

    private static async Task<HttpResponseMessage> Ranged(FileEndpointsHost host, string value, long from, long to, EntityTagHeaderValue? ifRange)
    {
        using HttpRequestMessage request = new(HttpMethod.Get, Link(value));
        request.Headers.Range = new RangeHeaderValue(from, to);
        if (ifRange is not null)
        {
            request.Headers.IfRange = new RangeConditionHeaderValue(ifRange);
        }

        return await host.Client.SendAsync(request, Ct);
    }

    // --- KK4 and KK10: nothing is counted or traced; the life is read at the acceptance only (criterion 4) ---

    [Fact]
    public async Task Download_SecondDownloadAndRange_ChangeNoCounter_AndWriteNothingToTheTrace()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, _) = await host.SeedDownloadAsync(Hundred);
        string value = host.LinkFor(item);
        string counters = $"SELECT count(*) FROM order_items WHERE id = '{item}' AND download_count = 0 AND download_allowance = 3";
        long trace = await host.CountAsync("SELECT count(*) FROM audit_logs");
        Assert.Equal(1L, await host.CountAsync(counters));

        (await host.Client.GetAsync(Link(value), Ct)).Dispose();
        (await host.Client.GetAsync(Link(value), Ct)).Dispose();
        (await Ranged(host, value, 0, 9, null)).Dispose();

        Assert.Equal(1L, await host.CountAsync(counters));
        Assert.Equal(trace, await host.CountAsync("SELECT count(*) FROM audit_logs"));
    }

    [Fact]
    public async Task Download_WhoseLifeEndsWhileTheFileIsSent_IsCompleted_AndTheNextRangeRequestIs404()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        byte[] big = [.. Enumerable.Range(0, 3 * 1024 * 1024).Select(i => (byte)(i % 251))];
        (Guid item, _, _) = await host.SeedDownloadAsync(big);
        string value = host.LinkFor(item);

        using HttpResponseMessage response = await host.Client.GetAsync(Link(value), HttpCompletionOption.ResponseHeadersRead, Ct);
        await using Stream body = await response.Content.ReadAsStreamAsync(Ct);
        byte[] head = new byte[1024];
        await body.ReadExactlyAsync(head, Ct);
        host.Time.Advance(TimeSpan.FromHours(2));
        using MemoryStream rest = new();
        await body.CopyToAsync(rest, Ct);
        using HttpResponseMessage next = await Ranged(host, value, 0, 9, null);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(big.Length, head.Length + rest.Length);
        Assert.Equal(big[..1024], head);
        Assert.Equal(big[1024..], rest.ToArray());
        await AssertEmpty404Async(next);
    }

    // --- A missing file in the store (09 15.7 item 3; 09 8.1) ---

    [Fact]
    public async Task Download_OfACompletedRowWhoseFileIsMissingFromTheStore_Is500WithNoBody()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        (Guid item, _, Guid fileId) = await host.SeedDownloadAsync(Hundred);
        await using Shopfolio.Infrastructure.AppDbContext db = host.NewContext();
        string key = await db.DigitalFiles.Where(f => f.Id == fileId).Select(f => f.StorageKey).SingleAsync(Ct);
        await host.Files.DeleteAsync(FileArea.DigitalFile, key, Ct);

        using HttpResponseMessage response = await host.Client.GetAsync(Link(host.LinkFor(item)), Ct);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        Assert.Empty(await response.Content.ReadAsByteArrayAsync(Ct));
    }
}
