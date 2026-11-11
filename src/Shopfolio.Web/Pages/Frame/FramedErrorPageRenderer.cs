using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// Draws the page-level error page inside the frame of the surface the failed request belonged to (T541; 04 2.7.3.2; 09 8.1 item 3; D-135): the storefront's frame for a storefront page, the panel's frame for a
/// panel page and the bare panel title for the panel screens that have no frame (04 2.2.9) or when no panel session is open. It is the one writer of the framed error page: the single catching layer calls
/// it through <see cref="IErrorPageRenderer"/> (the infrastructure does not know the pages) and falls back to the plainest page when this one fails (05 10.3.6). The frame is built here from the request, the
/// merge message is not read (it would be consumed by a page the visitor does not see), and nothing here writes a cookie (09 15.1 item 6).
/// </summary>
internal sealed class FramedErrorPageRenderer : IErrorPageRenderer
{
    /// <summary>The view of the error page; an application path, because the response is written outside the page framework's own lookup.</summary>
    private const string ViewPath = "/Pages/Shared/_UnexpectedError.cshtml";

    private readonly StorefrontFrameBuilder _storefront;
    private readonly PanelFrameBuilder _panel;
    private readonly IModelMetadataProvider _metadata;

    public FramedErrorPageRenderer(StorefrontFrameBuilder storefront, PanelFrameBuilder panel, IModelMetadataProvider metadata)
    {
        ArgumentNullException.ThrowIfNull(storefront);
        ArgumentNullException.ThrowIfNull(panel);
        ArgumentNullException.ThrowIfNull(metadata);
        _storefront = storefront;
        _panel = panel;
        _metadata = metadata;
    }

    public async Task RenderAsync(HttpContext context, ErrorPageText page)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(page);

        ErrorSurface surface = await PrepareFrameAsync(context);
        string retry = context.Request.Path.ToUriComponent() + context.Request.QueryString.ToUriComponent();
        bool storefront = surface == ErrorSurface.Storefront;
        ErrorPageView model = new(
            surface,
            page.Title,
            page.Message,
            retry,
            storefront ? "/" : PanelFramePaths.Home,
            storefront ? "Ana sayfaya dön" : "Panele dön");

        ViewDataDictionary<ErrorPageView> viewData = new(_metadata, new ModelStateDictionary()) { Model = model };
        ViewResult result = new() { ViewName = ViewPath, ViewData = viewData, StatusCode = StatusCodes.Status500InternalServerError };
        await result.ExecuteResultAsync(new ActionContext(context, context.GetRouteData(), new ActionDescriptor()));
    }

    private async Task<ErrorSurface> PrepareFrameAsync(HttpContext context)
    {
        if (!UnexpectedErrorAnswer.IsPanel(context.Request))
        {
            context.Items[StorefrontFrameAccess.ItemKey] = await _storefront.BuildAsync(context, readsMergeNotice: false, context.RequestAborted);
            return ErrorSurface.Storefront;
        }

        if (PanelFrameBuilder.IsFramelessScreen(context.Request))
        {
            return ErrorSurface.PanelBare;
        }

        PanelFrame? frame = await _panel.BuildAsync(context, context.RequestAborted);
        if (frame is null)
        {
            return ErrorSurface.PanelBare;
        }

        context.Items[PanelFrameAccess.ItemKey] = frame;
        return ErrorSurface.Panel;
    }
}
