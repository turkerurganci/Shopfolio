using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.RateLimiting;

/// <summary>
/// The body ceiling per endpoint class (T215 KK1, KK2; 07 2.8.1, 2.8.7; 09 15.4): every class has its number, a GET carries no body, an over-long body
/// is refused with 413 before anything reads it and the page shown is no form, a machine endpoint has no body.
/// </summary>
public sealed class RequestBodyLimitMiddlewareTests
{
    private const long Kilobyte = 1024;
    private const long Megabyte = 1024 * Kilobyte;

    private sealed class Run
    {
        public bool NextCalled { get; set; }

        public int NextReadBytes { get; set; }

        public DefaultHttpContext Context { get; init; } = NewContext();

        public MemoryStream Response { get; } = new();

        public async Task<Run> InvokeAsync()
        {
            Context.Response.Body = Response;
            RequestBodyLimitMiddleware middleware = new(async http =>
            {
                NextCalled = true;
                using MemoryStream copy = new();
                await http.Request.Body.CopyToAsync(copy);
                NextReadBytes = (int)copy.Length;
            });
            await middleware.InvokeAsync(Context);
            return this;
        }

        public string Html => Encoding.UTF8.GetString(Response.ToArray());
    }

    // The refusal writes its line through the rejection log, which the application registers (09 8.2 item 3).
    private static DefaultHttpContext NewContext() => new() { RequestServices = new ServiceCollection().AddLogging().AddSingleton<RejectionLog>().BuildServiceProvider() };

    private static RequestBodyClass? Class(string? name) => name is null ? null : Enum.Parse<RequestBodyClass>(name);

    private static Run Make(string method, RequestBodyClass? bodyClass, long? declaredLength, int actualBytes, bool machine = false, string path = "/panel/urun")
    {
        DefaultHttpContext context = NewContext();
        context.Request.Method = method;
        context.Request.Path = path;
        context.Request.Body = new MemoryStream(new byte[actualBytes]);
        if (declaredLength is long length)
        {
            context.Request.ContentLength = length;
        }
        else if (actualBytes > 0)
        {
            context.Request.Headers.TransferEncoding = "chunked";
        }

        List<object> metadata = [];
        if (bodyClass is RequestBodyClass declared)
        {
            metadata.Add(new RequestBodyClassAttribute(declared));
        }

        if (machine)
        {
            metadata.Add(MachineEndpointMetadata.Instance);
        }

        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, new EndpointMetadataCollection(metadata), "test"));
        return new Run { Context = context };
    }

    [Fact]
    public void TheCeilings_AreTheDocumentsNumbers()
    {
        Assert.Equal(64 * Kilobyte, RequestBodyLimits.Of(RequestBodyClass.General));
        Assert.Equal(Megabyte, RequestBodyLimits.Of(RequestBodyClass.LongText));
        Assert.Equal(ProductConstants.ImageMaxBytes + Megabyte, RequestBodyLimits.Of(RequestBodyClass.ImageUpload));
        Assert.Equal(5 * Megabyte, RequestBodyLimits.Of(RequestBodyClass.Chunk));
        Assert.Equal(64 * Kilobyte, RequestBodyLimits.Of(RequestBodyClass.Notification));
    }

    [Fact]
    public void EveryClass_HasACeiling_AndAnUnknownOneIsAnError()
    {
        foreach (RequestBodyClass bodyClass in Enum.GetValues<RequestBodyClass>())
        {
            Assert.True(RequestBodyLimits.Of(bodyClass) > 0, bodyClass.ToString());
        }

        Assert.Throws<ArgumentOutOfRangeException>(() => RequestBodyLimits.Of((RequestBodyClass)999));
    }

    [Theory]
    [InlineData("GET")]
    [InlineData("HEAD")]
    public void AGetOrHead_HasNoBody_WhateverTheClass(string method)
    {
        Assert.Equal(0, RequestBodyLimits.Of(method, RequestBodyClass.Chunk));
        Assert.Equal(0, RequestBodyLimits.Of(method, null));
    }

    [Fact]
    public void APostWithNoClass_IsInTheGeneralClass()
    {
        Assert.Equal(RequestBodyLimits.GeneralMaxBytes, RequestBodyLimits.Of("POST", null));
    }

    [Theory]
    [InlineData(null, 64 * Kilobyte)]
    [InlineData("General", 64 * Kilobyte)]
    [InlineData("LongText", Megabyte)]
    [InlineData("Chunk", 5 * Megabyte)]
    [InlineData("Notification", 64 * Kilobyte)]
    public async Task ABodyAtTheCeiling_PassesWithItsDeclaredLength_ForEveryClass(string? className, long ceiling)
    {
        Run run = await Make("POST", Class(className), ceiling, (int)ceiling).InvokeAsync();

        Assert.True(run.NextCalled);
        Assert.Equal(StatusCodes.Status200OK, run.Context.Response.StatusCode);
        Assert.Equal((int)ceiling, run.NextReadBytes);
    }

    [Theory]
    [InlineData(null, 64 * Kilobyte)]
    [InlineData("General", 64 * Kilobyte)]
    [InlineData("LongText", Megabyte)]
    [InlineData("Notification", 64 * Kilobyte)]
    public async Task ABodyOneByteOverTheCeiling_IsRefusedWith413_ForEveryClass_WithoutReadingIt(string? className, long ceiling)
    {
        Run run = await Make("POST", Class(className), ceiling + 1, 1).InvokeAsync();

        Assert.False(run.NextCalled);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, run.Context.Response.StatusCode);
        Assert.Equal(0, run.Context.Request.Body.Position);
    }

    // T312 (07 4.31.2; D-148): the chunk class is measured by its own endpoint, after the session and the 404, so the middleware refuses nothing, reads nothing and
    // lifts the server's own ceiling; the endpoint answers the 413 itself (UploadEndpointsTests: a big chunk of an unknown upload is a 404, not a 413).
    [Theory]
    [InlineData(5 * Megabyte + 1, true)]
    [InlineData(100 * Megabyte, true)]
    [InlineData(null, false)]
    public async Task TheChunkClass_IsPassedOnUnmeasured_WhateverTheLength_AndTheServersCeilingIsLifted(long? declaredLength, bool declared)
    {
        Run run = Make("POST", RequestBodyClass.Chunk, declaredLength, 10);
        MaxBodyFeature feature = new() { MaxRequestBodySize = 1234 };
        run.Context.Features.Set<IHttpMaxRequestBodySizeFeature>(feature);

        await run.InvokeAsync();

        Assert.Equal(declared, run.Context.Request.ContentLength is not null);
        Assert.True(run.NextCalled);
        Assert.Equal(StatusCodes.Status200OK, run.Context.Response.StatusCode);
        Assert.Equal(10, run.NextReadBytes);
        Assert.Null(feature.MaxRequestBodySize);
    }

    [Fact]
    public async Task TheEndpointsOwnRefusal_IsTheSame413AsTheMiddlewares_NoBodyOnAMachineEndpoint()
    {
        Run run = Make("POST", RequestBodyClass.Chunk, RequestBodyLimits.ChunkMaxBytes + 1, 1, machine: true, path: "/panel/yukleme/x");

        run.Context.Response.Body = run.Response;

        await RequestBodyLimitMiddleware.RefuseAsync(run.Context, run.Context.GetEndpoint());

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, run.Context.Response.StatusCode);
        Assert.Equal(0, run.Response.Length);
        Assert.Equal("close", run.Context.Response.Headers.Connection.ToString());
    }

    [Fact]
    public async Task TheImageUploadClass_IsTheImageCeilingPlusOneMegabyte()
    {
        long ceiling = ProductConstants.ImageMaxBytes + Megabyte;

        Run atCeiling = await Make("POST", RequestBodyClass.ImageUpload, ceiling, 1).InvokeAsync();
        Run over = await Make("POST", RequestBodyClass.ImageUpload, ceiling + 1, 1).InvokeAsync();

        Assert.True(atCeiling.NextCalled);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, over.Context.Response.StatusCode);
    }

    [Fact]
    public async Task APostOfALargeClassBody_OnAnEndpointWithoutAClass_IsRefused_TheClassIsNotInferred()
    {
        Run run = await Make("POST", null, Megabyte, 1).InvokeAsync();

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, run.Context.Response.StatusCode);
    }

    [Fact]
    public async Task AGetWithABody_IsRefusedWith413()
    {
        Run run = await Make("GET", null, 10, 10).InvokeAsync();

        Assert.False(run.NextCalled);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, run.Context.Response.StatusCode);
    }

    [Fact]
    public async Task AGetWithNoBody_PassesOn()
    {
        Run run = await Make("GET", null, null, 0).InvokeAsync();

        Assert.True(run.NextCalled);
    }

    [Fact]
    public async Task APageRefusal_IsThePlainOperationPage_NoForm_NoStoredValues()
    {
        Run run = await Make("POST", null, 64 * Kilobyte + 1, 1).InvokeAsync();

        Assert.Equal("text/html; charset=utf-8", run.Context.Response.ContentType);
        Assert.Equal("no-store", run.Context.Response.Headers.CacheControl.ToString());
        Assert.Contains(System.Net.WebUtility.HtmlEncode(RequestBodyLimitMiddleware.Message), run.Html, StringComparison.Ordinal);
        Assert.DoesNotContain("<form", run.Html, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("<input", run.Html, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(true, "/sistem/odeme-bildirimi")]
    [InlineData(false, "/sistem/odeme-bildirimi")]
    [InlineData(false, "/panel/yukleme/parca")]
    [InlineData(false, "/indir/x")]
    public async Task AMachineEndpointsRefusal_HasNoBody(bool marked, string path)
    {
        Run run = await Make("POST", RequestBodyClass.Notification, 64 * Kilobyte + 1, 1, machine: marked, path: path).InvokeAsync();

        Assert.Equal(StatusCodes.Status413PayloadTooLarge, run.Context.Response.StatusCode);
        Assert.Equal(0, run.Response.Length);
        Assert.Null(run.Context.Response.ContentType);
    }

    [Fact]
    public async Task ABodyWithNoDeclaredLength_IsReadOnlyToTheCeilingPlusOne_AndRefusedWhenLonger()
    {
        Run over = await Make("POST", null, null, (int)RequestBodyLimits.GeneralMaxBytes + 1).InvokeAsync();
        Run within = await Make("POST", null, null, (int)RequestBodyLimits.GeneralMaxBytes).InvokeAsync();

        Assert.False(over.NextCalled);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, over.Context.Response.StatusCode);
        Assert.True(within.NextCalled);
        Assert.Equal((int)RequestBodyLimits.GeneralMaxBytes, within.NextReadBytes);
    }

    [Fact]
    public async Task TheServersOwnCeiling_IsSetToTheClassCeiling_WhenTheServerHasOne()
    {
        Run run = Make("POST", RequestBodyClass.LongText, 100, 100);
        MaxBodyFeature feature = new();
        run.Context.Features.Set<IHttpMaxRequestBodySizeFeature>(feature);

        await run.InvokeAsync();

        Assert.Equal(Megabyte, feature.MaxRequestBodySize);
    }

    [Fact]
    public async Task TheServersOwnCeiling_IsLifted_ForABodyWithNoDeclaredLength_BecauseTheServerCountsTheChunkFraming()
    {
        Run run = Make("POST", RequestBodyClass.General, null, 100);
        MaxBodyFeature feature = new();
        run.Context.Features.Set<IHttpMaxRequestBodySizeFeature>(feature);

        await run.InvokeAsync();

        Assert.True(run.NextCalled);
        Assert.Null(feature.MaxRequestBodySize);
    }

    [Fact]
    public async Task AServerThatThrowsItsOwn413WhileTheBodyIsRead_IsAnsweredWith413_NotLeftToTheUnexpectedErrorLayer()
    {
        Run run = Make("POST", RequestBodyClass.General, null, 0);
        run.Context.Request.Headers.TransferEncoding = "chunked";
        run.Context.Request.Body = new ThrowingBody();

        await run.InvokeAsync();

        Assert.False(run.NextCalled);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, run.Context.Response.StatusCode);
        Assert.DoesNotContain("<form", run.Html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ARefusal_ClosesTheConnection_OnHttp1AndAddsNoConnectionHeader_OnHttp2()
    {
        Run http1 = Make("POST", null, RequestBodyLimits.GeneralMaxBytes + 1, 1);
        http1.Context.Request.Protocol = "HTTP/1.1";
        Run http2 = Make("POST", null, RequestBodyLimits.GeneralMaxBytes + 1, 1);
        http2.Context.Request.Protocol = "HTTP/2";

        await http1.InvokeAsync();
        await http2.InvokeAsync();

        Assert.Equal("close", http1.Context.Response.Headers.Connection.ToString());
        Assert.False(http2.Context.Response.Headers.ContainsKey("Connection"));
    }

    private sealed class ThrowingBody : Stream
    {
        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
            => throw new BadHttpRequestException("Request body too large.", StatusCodes.Status413PayloadTooLarge);

        public override int Read(byte[] buffer, int offset, int count) => throw new BadHttpRequestException("Request body too large.", StatusCodes.Status413PayloadTooLarge);

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    }

    private sealed class MaxBodyFeature : IHttpMaxRequestBodySizeFeature
    {
        public bool IsReadOnly => false;

        public long? MaxRequestBodySize { get; set; }
    }
}
