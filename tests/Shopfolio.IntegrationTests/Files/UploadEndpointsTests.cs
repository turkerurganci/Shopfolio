using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure.Files;
using Shopfolio.Infrastructure.Security;
using Shopfolio.IntegrationTests.Database;
using Shopfolio.IntegrationTests.Web;

namespace Shopfolio.IntegrationTests.Files;

/// <summary>
/// UC-83 on the opened application and a real PostgreSQL (T312 criteria 5 to 10; KK5 to KK10; 07 4.31.2): the start opens one file row and answers 201 with the chunk size,
/// and the 400 and 422 ways open none; a chunk is accepted only right after the bytes received, the last one completes the upload in the same transaction, two equal chunks
/// at once give one 200 and one 409, the 413 and the two 400s have no body, the order of the checks is the one of 07 4.31.2, a crash after the last chunk is completed by the
/// next request, the status request and the unknown upload, nothing is written to the trace, and a request without a session or a forgery token writes nothing.
/// The chunk with no declared length is checked on a real Kestrel server too.
/// </summary>
[Collection(FileEndpointsDefinition.Name)]
public sealed class UploadEndpointsTests
{
    private static readonly byte[] Twelve = [.. Enumerable.Range(1, 12).Select(i => (byte)i)];

    private readonly CollectionDatabaseFixture _database;

    public UploadEndpointsTests(CollectionDatabaseFixture database)
    {
        _database = database;
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<FileEndpointsHost> OpenAsync(bool kestrel = false)
    {
        (string key, _) = await TestSessions.AdminAsync(_database.Dsn);
        return new FileEndpointsHost(_database.Dsn, kestrel, key);
    }

    private static async Task<JsonElement> JsonOfAsync(HttpResponseMessage response)
    {
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(Ct));
        return document.RootElement.Clone();
    }

    private static async Task AssertNoBodyAsync(HttpResponseMessage response) => Assert.Empty(await response.Content.ReadAsByteArrayAsync(Ct));

    private static string RowCount(Guid id) => $"SELECT count(*) FROM digital_files WHERE id = '{id}'";

    // --- Start (criteria 5 and 6; KK5, KK6) ---

    [Fact]
    public async Task Start_WithAValidNameAndSize_Opens_OneRow_AndAnswers201WithTheChunkSize()
    {
        await using FileEndpointsHost host = await OpenAsync();

        using HttpResponseMessage response = await host.StartAsync("{\"fileName\":\"kitap.pdf\",\"sizeBytes\":12}");

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        JsonElement body = await JsonOfAsync(response);
        Guid id = body.GetProperty("id").GetGuid();
        Assert.Equal(5L * 1024 * 1024, body.GetProperty("chunkSizeBytes").GetInt64());
        Assert.Equal(0, body.GetProperty("receivedBytes").GetInt64());
        Assert.False(body.GetProperty("isCompleted").GetBoolean());
        Assert.Equal(1L, await host.CountAsync(RowCount(id)));
        Assert.Equal(1L, await host.CountAsync($"SELECT count(*) FROM digital_files WHERE id = '{id}' AND file_name = 'kitap.pdf' AND size_bytes = 12 AND upload_completed_at IS NULL"));
    }

    [Theory]
    [InlineData("{\"fileName\":\"\",\"sizeBytes\":12}")]
    [InlineData("{\"fileName\":\"a/b.pdf\",\"sizeBytes\":12}")]
    [InlineData("{\"fileName\":\"a\\\\b.pdf\",\"sizeBytes\":12}")]
    [InlineData("{\"sizeBytes\":12}")]
    [InlineData("{\"fileName\":\"kitap.pdf\",\"sizeBytes\":0}")]
    [InlineData("{\"fileName\":\"kitap.pdf\",\"sizeBytes\":-5}")]
    [InlineData("{\"fileName\":\"kitap.pdf\",\"sizeBytes\":\"12\"}")]
    [InlineData("not json")]
    [InlineData("null")]
    public async Task Start_WithAnInvalidNameOrSizeOrBody_Answers400_AndOpensNoRow(string json)
    {
        await using FileEndpointsHost host = await OpenAsync();
        long before = await host.CountAsync("SELECT count(*) FROM digital_files");

        using HttpResponseMessage response = await host.StartAsync(json);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertNoBodyAsync(response);
        Assert.Equal(before, await host.CountAsync("SELECT count(*) FROM digital_files"));
    }

    [Fact]
    public async Task Start_WithASizeAboveTheCeiling_Answers422WithTheCeiling_AndOpensNoRow()
    {
        await using FileEndpointsHost host = await OpenAsync();
        long before = await host.CountAsync("SELECT count(*) FROM digital_files");

        using HttpResponseMessage response = await host.StartAsync("{\"fileName\":\"buyuk.bin\",\"sizeBytes\":" + (500L * 1024 * 1024 + 1) + "}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(500L * 1024 * 1024, (await JsonOfAsync(response)).GetProperty("maxSizeBytes").GetInt64());
        Assert.Equal(before, await host.CountAsync("SELECT count(*) FROM digital_files"));
    }

    [Fact]
    public async Task Start_WithASizeThatBringsTheDiskToItsThreshold_Answers422WithTheCeiling_AndOpensNoRow()
    {
        await using FileEndpointsHost host = await OpenAsync();
        host.Disk.Total = 1_000_000_000;
        host.Disk.Free = 100_000_000;
        long before = await host.CountAsync("SELECT count(*) FROM digital_files");

        using HttpResponseMessage response = await host.StartAsync("{\"fileName\":\"kitap.pdf\",\"sizeBytes\":300000000}");

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
        Assert.Equal(500L * 1024 * 1024, (await JsonOfAsync(response)).GetProperty("maxSizeBytes").GetInt64());
        Assert.Equal(before, await host.CountAsync("SELECT count(*) FROM digital_files"));
    }

    // --- Chunks (criteria 7 to 9; KK7 to KK9) ---

    [Fact]
    public async Task Chunk_RightAfterTheReceivedBytes_Answers200_AndTheLastOneCompletesTheUpload()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);

        using HttpResponseMessage first = await host.ChunkAsync(id.ToString(), Twelve[..5], 0);
        JsonElement firstBody = await JsonOfAsync(first);
        using HttpResponseMessage last = await host.ChunkAsync(id.ToString(), Twelve[5..], 5);
        JsonElement lastBody = await JsonOfAsync(last);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(5, firstBody.GetProperty("receivedBytes").GetInt64());
        Assert.False(firstBody.GetProperty("isCompleted").GetBoolean());
        Assert.Equal(HttpStatusCode.OK, last.StatusCode);
        Assert.Equal(12, lastBody.GetProperty("receivedBytes").GetInt64());
        Assert.True(lastBody.GetProperty("isCompleted").GetBoolean());
        Assert.Equal(1L, await host.CountAsync($"SELECT count(*) FROM digital_files WHERE id = '{id}' AND upload_completed_at IS NOT NULL"));
        string key = await KeyOfAsync(host, id);
        await using Stream stored = await host.Files.OpenReadAsync(FileArea.DigitalFile, key, Ct);
        using MemoryStream copy = new();
        await stored.CopyToAsync(copy, Ct);
        Assert.Equal(Twelve, copy.ToArray());
    }

    [Fact]
    public async Task Chunk_ThatDoesNotStartAtTheReceivedBytes_Answers409WithTheReceivedBytes_AndWritesNothing()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);
        (await host.ChunkAsync(id.ToString(), Twelve[..4], 0)).Dispose();

        using HttpResponseMessage ahead = await host.ChunkAsync(id.ToString(), Twelve[6..8], 6);
        using HttpResponseMessage again = await host.ChunkAsync(id.ToString(), Twelve[..4], 0);

        foreach (HttpResponseMessage response in new[] { ahead, again })
        {
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            JsonElement body = await JsonOfAsync(response);
            Assert.Equal(4, body.GetProperty("receivedBytes").GetInt64());
            Assert.False(body.GetProperty("isCompleted").GetBoolean());
        }

        using HttpResponseMessage status = await host.StatusAsync(id.ToString());
        Assert.Equal(4, (await JsonOfAsync(status)).GetProperty("receivedBytes").GetInt64());
    }

    [Fact]
    public async Task Chunk_AfterTheUploadIsComplete_Answers409WithCompletedTrue()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);
        (await host.ChunkAsync(id.ToString(), Twelve, 0)).Dispose();

        using HttpResponseMessage response = await host.ChunkAsync(id.ToString(), Twelve[..2], 12);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        JsonElement body = await JsonOfAsync(response);
        Assert.Equal(12, body.GetProperty("receivedBytes").GetInt64());
        Assert.True(body.GetProperty("isCompleted").GetBoolean());
    }

    [Fact]
    public async Task Chunk_TwoEqualChunksAtOnce_GiveOne200AndOne409_AndTheBytesAreWrittenOnce()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);

        Task<HttpResponseMessage> a = host.ChunkAsync(id.ToString(), Twelve[..6], 0);
        Task<HttpResponseMessage> b = host.ChunkAsync(id.ToString(), Twelve[..6], 0);
        using HttpResponseMessage first = await a;
        using HttpResponseMessage second = await b;

        Assert.Equal([HttpStatusCode.OK, HttpStatusCode.Conflict], new[] { first.StatusCode, second.StatusCode }.Order());
        using HttpResponseMessage status = await host.StatusAsync(id.ToString());
        Assert.Equal(6, (await JsonOfAsync(status)).GetProperty("receivedBytes").GetInt64());
    }

    [Fact]
    public async Task Chunk_ThatGoesPastTheDeclaredSize_Answers400WithNoBody_AndWritesNothing()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);

        using HttpResponseMessage response = await host.ChunkAsync(id.ToString(), [.. Twelve, 13], 0);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertNoBodyAsync(response);
        using HttpResponseMessage status = await host.StatusAsync(id.ToString());
        Assert.Equal(0, (await JsonOfAsync(status)).GetProperty("receivedBytes").GetInt64());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("bytes")]
    [InlineData("items 0-3/12")]
    [InlineData("bytes 0-9/12")]
    [InlineData("bytes 3-0/12")]
    [InlineData("bytes -3/12")]
    public async Task Chunk_WithAMissingOrMalformedOrWrongLengthContentRange_Answers400WithNoBody(string? header)
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);
        using ByteArrayContent content = new(Twelve[..4]);
        if (header is not null)
        {
            content.Headers.TryAddWithoutValidation("Content-Range", header);
        }

        using HttpResponseMessage response = await host.SendChunkAsync(id.ToString(), content);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        await AssertNoBodyAsync(response);
    }

    [Fact]
    public async Task Chunk_AboveFiveMegabytes_Answers413WithNoBody_AndWritesNothing()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("buyuk.bin", 20L * 1024 * 1024);
        byte[] big = new byte[(5 * 1024 * 1024) + 1];

        using HttpResponseMessage response = await host.ChunkAsync(id.ToString(), big, 0);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
        await AssertNoBodyAsync(response);
        using HttpResponseMessage status = await host.StatusAsync(id.ToString());
        Assert.Equal(0, (await JsonOfAsync(status)).GetProperty("receivedBytes").GetInt64());
    }

    [Fact]
    public async Task Chunk_ExactlyFiveMegabytes_IsAccepted()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("buyuk.bin", 20L * 1024 * 1024);

        using HttpResponseMessage response = await host.ChunkAsync(id.ToString(), new byte[5 * 1024 * 1024], 0);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(5L * 1024 * 1024, (await JsonOfAsync(response)).GetProperty("receivedBytes").GetInt64());
    }

    // The order of 07 4.31.2 (KK8): the session, the unknown upload (404), the size (413), the Content-Range (400), then the 409 and the overflow 400.
    [Fact]
    public async Task Chunk_Order_AnUnknownUploadWithABigChunkIs404_NotTheCeilingsAnswer()
    {
        await using FileEndpointsHost host = await OpenAsync();

        using HttpResponseMessage response = await host.ChunkAsync(Guid.NewGuid().ToString(), new byte[(5 * 1024 * 1024) + 1], 0);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertNoBodyAsync(response);
    }

    [Fact]
    public async Task Chunk_Order_ABigChunkWithNoContentRangeIs413_NotTheRangesAnswer()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("buyuk.bin", 20L * 1024 * 1024);
        using ByteArrayContent content = new(new byte[(5 * 1024 * 1024) + 1]);

        using HttpResponseMessage response = await host.SendChunkAsync(id.ToString(), content);

        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, response.StatusCode);
    }

    [Fact]
    public async Task Chunk_Order_AChunkThatDoesNotFitAndGoesPastTheTotalIs409_NotTheOverflowAnswer()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);
        (await host.ChunkAsync(id.ToString(), Twelve[..4], 0)).Dispose();

        using HttpResponseMessage response = await host.ChunkAsync(id.ToString(), new byte[20], 10);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task Chunk_ForAnUnknownOrMalformedIdentity_Answers404WithNoBody(string id)
    {
        await using FileEndpointsHost host = await OpenAsync();

        using HttpResponseMessage response = await host.ChunkAsync(id, Twelve[..4], 0);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        await AssertNoBodyAsync(response);
    }

    [Fact]
    public async Task Chunk_AfterACrashThatLeftEveryByteButNoCompletion_IsCompletedByTheNextRequest()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);
        (await host.ChunkAsync(id.ToString(), Twelve[..5], 0)).Dispose();

        // The crash: the last bytes reached the store and the transaction that would have written the completion did not commit.
        using (MemoryStream rest = new(Twelve[5..]))
        {
            await host.Files.AppendAsync(FileArea.DigitalFile, await KeyOfAsync(host, id), rest, Ct);
        }

        using HttpResponseMessage status = await host.StatusAsync(id.ToString());
        JsonElement statusBody = await JsonOfAsync(status);
        Assert.Equal(12, statusBody.GetProperty("receivedBytes").GetInt64());
        Assert.False(statusBody.GetProperty("isCompleted").GetBoolean());

        using HttpResponseMessage retry = await host.ChunkAsync(id.ToString(), Twelve[5..], 5);

        Assert.Equal(HttpStatusCode.OK, retry.StatusCode);
        Assert.True((await JsonOfAsync(retry)).GetProperty("isCompleted").GetBoolean());
        Assert.Equal(1L, await host.CountAsync($"SELECT count(*) FROM digital_files WHERE id = '{id}' AND upload_completed_at IS NOT NULL"));
        string key = await KeyOfAsync(host, id);
        Assert.Equal(12, await host.Files.GetSizeAsync(FileArea.DigitalFile, key, Ct));
    }

    // --- Status (criterion 9; KK9) ---

    [Fact]
    public async Task Status_SaysTheReceivedBytesAndTheCompletion_AndAnUnknownUploadIs404()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);
        (await host.ChunkAsync(id.ToString(), Twelve[..7], 0)).Dispose();

        using HttpResponseMessage partial = await host.StatusAsync(id.ToString());
        (await host.ChunkAsync(id.ToString(), Twelve[7..], 7)).Dispose();
        using HttpResponseMessage complete = await host.StatusAsync(id.ToString());
        using HttpResponseMessage unknown = await host.StatusAsync(Guid.NewGuid().ToString());

        Assert.Equal(HttpStatusCode.OK, partial.StatusCode);
        JsonElement partialBody = await JsonOfAsync(partial);
        Assert.Equal(7, partialBody.GetProperty("receivedBytes").GetInt64());
        Assert.False(partialBody.GetProperty("isCompleted").GetBoolean());
        Assert.False(partialBody.TryGetProperty("chunkSizeBytes", out _));
        JsonElement completeBody = await JsonOfAsync(complete);
        Assert.Equal(12, completeBody.GetProperty("receivedBytes").GetInt64());
        Assert.True(completeBody.GetProperty("isCompleted").GetBoolean());
        Assert.Equal(HttpStatusCode.NotFound, unknown.StatusCode);
        await AssertNoBodyAsync(unknown);
    }

    // --- Trace and headers (criterion 10; KK10) ---

    [Fact]
    public async Task Upload_WritesNothingToTheTrace_AndEveryAnswerIsNoStoreAndNoIndex()
    {
        await using FileEndpointsHost host = await OpenAsync();
        long before = await host.CountAsync("SELECT count(*) FROM audit_logs");

        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);
        List<HttpResponseMessage> answers =
        [
            await host.ChunkAsync(id.ToString(), Twelve[..4], 0),
            await host.ChunkAsync(id.ToString(), Twelve[..4], 0),
            await host.ChunkAsync(id.ToString(), Twelve[4..], 4),
            await host.StatusAsync(id.ToString()),
            await host.StatusAsync(Guid.NewGuid().ToString()),
            await host.StartAsync("not json"),
        ];

        try
        {
            Assert.Equal(before, await host.CountAsync("SELECT count(*) FROM audit_logs"));
            foreach (HttpResponseMessage answer in answers)
            {
                Assert.Contains("no-store", string.Join(',', answer.Headers.GetValues("Cache-Control")), StringComparison.Ordinal);
                Assert.Equal("noindex", Assert.Single(answer.Headers.GetValues("X-Robots-Tag")));
                Assert.Equal("nosniff", Assert.Single(answer.Headers.GetValues("X-Content-Type-Options")));
            }
        }
        finally
        {
            foreach (HttpResponseMessage answer in answers)
            {
                answer.Dispose();
            }
        }
    }

    // --- Session and forgery token (written by the T207 and T209 mechanisms, met here by the new endpoints) ---

    [Fact]
    public async Task Requests_WithoutASession_AreSentToTheSignIn_AndWriteNothing()
    {
        await using FileEndpointsHost host = new(_database.Dsn);
        long before = await host.CountAsync("SELECT count(*) FROM digital_files");

        using HttpResponseMessage start = await host.StartAsync("{\"fileName\":\"kitap.pdf\",\"sizeBytes\":12}");
        using HttpResponseMessage chunk = await host.ChunkAsync(Guid.NewGuid().ToString(), Twelve[..4], 0);
        using HttpResponseMessage status = await host.StatusAsync(Guid.NewGuid().ToString());

        foreach (HttpResponseMessage response in new[] { start, chunk, status })
        {
            Assert.True(response.StatusCode is HttpStatusCode.Found or HttpStatusCode.SeeOther, $"status {response.StatusCode}");
            Assert.StartsWith("/panel/", response.Headers.Location?.OriginalString, StringComparison.Ordinal);
        }

        Assert.Equal(before, await host.CountAsync("SELECT count(*) FROM digital_files"));
    }

    [Fact]
    public async Task Posts_WithoutTheHeaderToken_Answer400WithNoBody_AndWriteNothing()
    {
        await using FileEndpointsHost host = await OpenAsync();
        Guid id = await host.StartOkAsync("kitap.pdf", Twelve.Length);
        long before = await host.CountAsync("SELECT count(*) FROM digital_files");

        using StringContent startBody = new("{\"fileName\":\"x.pdf\",\"sizeBytes\":12}", System.Text.Encoding.UTF8, "application/json");
        using HttpRequestMessage startRequest = new(HttpMethod.Post, new Uri("/panel/yukleme", UriKind.Relative)) { Content = startBody };
        startRequest.Headers.Add(ForgeryTokenHandler.SkipHeader, "1");
        using HttpResponseMessage start = await host.Client.SendAsync(startRequest, Ct);
        using HttpResponseMessage chunk = await host.ChunkAsync(id.ToString(), Twelve[..4], 0, noToken: true);

        foreach (HttpResponseMessage response in new[] { start, chunk })
        {
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            await AssertNoBodyAsync(response);
        }

        Assert.Equal(before, await host.CountAsync("SELECT count(*) FROM digital_files"));
        using HttpResponseMessage status = await host.StatusAsync(id.ToString());
        Assert.Equal(0, (await JsonOfAsync(status)).GetProperty("receivedBytes").GetInt64());
    }

    // --- A real Kestrel server: the chunk with no declared length ---

    [Fact]
    public async Task Chunk_WithNoDeclaredLength_OnARealServer_IsReadAndAccepted_AndOneAboveTheCeilingIs413()
    {
        await using FileEndpointsHost host = await OpenAsync(kestrel: true);
        Guid id = await host.StartOkAsync("buyuk.bin", 20L * 1024 * 1024);

        using UnknownLengthContent small = new(Twelve[..8]);
        small.Headers.ContentRange = new ContentRangeHeaderValue(0, 7);
        using HttpResponseMessage accepted = await host.SendChunkAsync(id.ToString(), small);

        using UnknownLengthContent big = new(new byte[(5 * 1024 * 1024) + 1]);
        big.Headers.ContentRange = new ContentRangeHeaderValue(8, 8 + (5 * 1024 * 1024));
        using HttpResponseMessage refused = await host.SendChunkAsync(id.ToString(), big);

        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.Equal(8, (await JsonOfAsync(accepted)).GetProperty("receivedBytes").GetInt64());
        Assert.Equal(HttpStatusCode.RequestEntityTooLarge, refused.StatusCode);
        await AssertNoBodyAsync(refused);
    }

    private static async Task<string> KeyOfAsync(FileEndpointsHost host, Guid id)
    {
        await using Shopfolio.Infrastructure.AppDbContext db = host.NewContext();
        return await db.DigitalFiles.Where(f => f.Id == id).Select(f => f.StorageKey).SingleAsync(Ct);
    }

    /// <summary>A body whose length the client does not know: the request goes out chunked.</summary>
    private sealed class UnknownLengthContent : HttpContent
    {
        private readonly byte[] _bytes;

        public UnknownLengthContent(byte[] bytes)
        {
            _bytes = bytes;
        }

        protected override Task SerializeToStreamAsync(Stream stream, System.Net.TransportContext? context) => stream.WriteAsync(_bytes, Ct).AsTask();

        protected override bool TryComputeLength(out long length)
        {
            length = 0;
            return false;
        }
    }
}
