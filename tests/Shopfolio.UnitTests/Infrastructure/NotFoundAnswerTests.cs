using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Security;

namespace Shopfolio.UnitTests.Infrastructure;

/// <summary>
/// The decisions of the catching layer's 404 answer (T504; 07 UC-06; 09 8.1 item 9): which responses become the "page not found" page and which stay empty. The page itself is drawn by the pages'
/// renderer and is the integration tests' (<c>NotFoundPageTests</c>); here a counting renderer stands in for it.
/// </summary>
public sealed class NotFoundAnswerTests
{
    private sealed class CountingRenderer : INotFoundPageRenderer
    {
        public int Calls { get; private set; }

        public bool Fails { get; init; }

        public Task RenderAsync(HttpContext context)
        {
            Calls++;
            if (Fails)
            {
                context.Response.ContentType = "text/html";
                throw new InvalidOperationException("the page cannot be drawn");
            }

            return Task.CompletedTask;
        }
    }

    private static DefaultHttpContext ContextOf(int status, string path, CountingRenderer? renderer, params object[] metadata)
    {
        ServiceCollection services = new();
        if (renderer is not null)
        {
            services.AddSingleton<INotFoundPageRenderer>(renderer);
        }

        DefaultHttpContext context = new() { RequestServices = services.BuildServiceProvider() };
        context.Request.Path = path;
        context.Response.StatusCode = status;
        if (metadata.Length > 0)
        {
            context.SetEndpoint(new Endpoint(null, new EndpointMetadataCollection(metadata), "test"));
        }

        return context;
    }

    [Theory]
    [InlineData(StatusCodes.Status200OK)]
    [InlineData(StatusCodes.Status302Found)]
    [InlineData(StatusCodes.Status403Forbidden)]
    [InlineData(StatusCodes.Status422UnprocessableEntity)]
    [InlineData(StatusCodes.Status500InternalServerError)]
    public async Task OnlyA404_IsTheNotFoundPage(int status)
    {
        CountingRenderer renderer = new();
        DefaultHttpContext context = ContextOf(status, "/bir-adres", renderer);

        await NotFoundAnswer.WriteIfNotFoundAsync(context);

        Assert.Equal(0, renderer.Calls);
        Assert.Equal(status, context.Response.StatusCode);
    }

    [Theory]
    [InlineData("/bilinmeyen-bolum")]
    [InlineData("/siparis/2026-ABC")]
    [InlineData("/panel/kayit/00000000-0000-0000-0000-000000000000")]
    public async Task AnEmpty404OfAPage_IsTheNotFoundPage_AndIsMarkedClosedToTheSearchEngines(string path)
    {
        CountingRenderer renderer = new();
        DefaultHttpContext context = ContextOf(StatusCodes.Status404NotFound, path, renderer);

        await NotFoundAnswer.WriteIfNotFoundAsync(context);

        Assert.Equal(1, renderer.Calls);
        Assert.True(SearchEngineClosureMark(context));
    }

    [Fact]
    public async Task AMachineEndpointsEmpty404_StaysEmpty()
    {
        CountingRenderer renderer = new();
        DefaultHttpContext context = ContextOf(StatusCodes.Status404NotFound, "/panel/yukleme/x", renderer, MachineEndpointMetadata.Instance);

        await NotFoundAnswer.WriteIfNotFoundAsync(context);

        Assert.Equal(0, renderer.Calls);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task TheDownloadAddress_IsAMachineEndpointThatABrowserOpensAsAPage_SoItsNotFoundIsThePage()
    {
        CountingRenderer renderer = new();
        DefaultHttpContext context = ContextOf(StatusCodes.Status404NotFound, "/indir/deger", renderer, MachineEndpointMetadata.Instance, NotFoundPageMetadata.Instance);

        await NotFoundAnswer.WriteIfNotFoundAsync(context);

        Assert.Equal(1, renderer.Calls);
    }

    [Theory]
    [InlineData("/statik/css/yok.css")]
    [InlineData("/STATIK/js/yok.js")]
    public async Task AMissingStaticFile_StaysEmpty(string path)
    {
        CountingRenderer renderer = new();
        DefaultHttpContext context = ContextOf(StatusCodes.Status404NotFound, path, renderer);

        await NotFoundAnswer.WriteIfNotFoundAsync(context);

        Assert.Equal(0, renderer.Calls);
    }

    [Fact]
    public async Task A404ThatAlreadyHasABody_IsLeftAlone()
    {
        CountingRenderer renderer = new();
        DefaultHttpContext context = ContextOf(StatusCodes.Status404NotFound, "/bir-adres", renderer);
        context.Response.ContentType = "application/json";

        await NotFoundAnswer.WriteIfNotFoundAsync(context);

        Assert.Equal(0, renderer.Calls);

        DefaultHttpContext sized = ContextOf(StatusCodes.Status404NotFound, "/bir-adres", renderer);
        sized.Response.ContentLength = 12;

        await NotFoundAnswer.WriteIfNotFoundAsync(sized);

        Assert.Equal(0, renderer.Calls);
    }

    [Fact]
    public async Task WithoutARegisteredPage_TheAnswerIsTheEmpty404()
    {
        DefaultHttpContext context = ContextOf(StatusCodes.Status404NotFound, "/bir-adres", renderer: null);

        await NotFoundAnswer.WriteIfNotFoundAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
    }

    [Fact]
    public async Task WhenThePageCannotBeDrawn_TheAnswerIsTheEmpty404_WithNoStore_AndNoException_05_10_3_6()
    {
        CountingRenderer renderer = new() { Fails = true };
        DefaultHttpContext context = ContextOf(StatusCodes.Status404NotFound, "/bir-adres", renderer);

        await NotFoundAnswer.WriteIfNotFoundAsync(context);

        Assert.Equal(1, renderer.Calls);
        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Null(context.Response.ContentType);
        Assert.Equal(SecurityHeaders.NoStore, context.Response.Headers.CacheControl.ToString());
    }

    private static bool SearchEngineClosureMark(HttpContext context) => Shopfolio.Infrastructure.SearchEngine.SearchEngineClosure.IsClosed(context);
}
