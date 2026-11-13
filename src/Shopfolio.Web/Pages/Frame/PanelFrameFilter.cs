using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Security;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// Makes the panel frame part of every panel page response (T540; 07 5.5.1; 04 2.2.8): after the handler, when the response is the page itself, the frame is built from the request's context and put
/// where the panel layout reads it (<see cref="PanelFrameAccess"/>). No page model holds it. A redirect, a handler that failed, a response the error layer wrote, a page outside the panel folder and
/// the screens that open before a session (04 2.2.9) get no panel frame. Same position as the storefront's filter (<see cref="StorefrontFrameFilter.FilterOrder"/>).
/// </summary>
/// <remarks>
/// The one page result made outside this filter is the error layer's operation-level answer (<c>UnexpectedErrorPageFilter</c>, -300): it draws the failed page again with the operation message and the
/// sent values (04 2.7.3.1), but this filter saw the handler's exception and built nothing. For that page the frame is built when its result is about to be drawn (T524; 04 9.24.10): the handler's own
/// failure is already recorded by the layer, and a failure of the frame's reads here is a failure while the result is drawn, the layer's middleware entry point (09 8.1 item 2b).
/// </remarks>
internal sealed class PanelFrameFilter : IAsyncPageFilter, IAsyncAlwaysRunResultFilter, IOrderedFilter
{
    private readonly PanelFrameBuilder _builder;

    public PanelFrameFilter(PanelFrameBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        _builder = builder;
    }

    public int Order => StorefrontFrameFilter.FilterOrder;

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        PageHandlerExecutedContext executed = await next();
        if (executed.Exception is not null
            || executed.Result is not (null or PageResult)
            || !IsFramedPanelPage(context.ActionDescriptor, context.HttpContext.Request))
        {
            return;
        }

        await PutFrameAsync(context.HttpContext);
    }

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        // Only the page the error layer draws again after a failed handler reaches here without its frame (see the remarks); every other framed page got it after its handler.
        if (context.Result is PageResult
            && context.HttpContext.GetPanelFrame() is null
            && context.ActionDescriptor is CompiledPageActionDescriptor page
            && IsFramedPanelPage(page, context.HttpContext.Request))
        {
            await PutFrameAsync(context.HttpContext);
        }

        await next();
    }

    private static bool IsFramedPanelPage(CompiledPageActionDescriptor descriptor, HttpRequest request) =>
        descriptor.ViewEnginePath.StartsWith(SessionSchemes.PanelFolder + "/", StringComparison.OrdinalIgnoreCase) && !PanelFrameBuilder.IsFramelessScreen(request);

    private async Task PutFrameAsync(HttpContext context)
    {
        PanelFrame? frame = await _builder.BuildAsync(context, context.RequestAborted);
        if (frame is not null)
        {
            context.Items[PanelFrameAccess.ItemKey] = frame;
        }
    }
}

/// <summary>Where the panel layout finds the frame of the request (T540): the request's items, written by <see cref="PanelFrameFilter"/> and read here only.</summary>
public static class PanelFrameAccess
{
    internal const string ItemKey = "Shopfolio.PanelFrame";

    /// <summary>The panel frame of this request, or null when the response is not a framed panel page (a redirect, a machine endpoint, a storefront page, the error page, a screen before the session).</summary>
    public static PanelFrame? GetPanelFrame(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items.TryGetValue(ItemKey, out object? value) ? value as PanelFrame : null;
    }
}
