using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// Draws the "page not found" page E-06 inside the storefront's full frame (T504; 04 5.6.3; 07 UC-06). It is the one writer of that page: the single catching layer calls it through
/// <see cref="INotFoundPageRenderer"/> for every 404 with no body of its own - the panel's and the order pages' denied access included, the panel's record that is not there too, which an
/// administrator's browser sees with the administrator strip (04 2.9.1) - and falls back to the empty 404 when this one fails (05 10.3.6). The frame is built from the request, the merge message is
/// not read (the visitor does not see a page that would consume it) and nothing here writes a cookie (09 15.1 item 6).
/// </summary>
internal sealed class FramedNotFoundPageRenderer : INotFoundPageRenderer
{
    /// <summary>The view of the page; an application path, because the response is written outside the page framework's own lookup.</summary>
    private const string ViewPath = "/Pages/Shared/_NotFound.cshtml";

    private readonly StorefrontFrameBuilder _storefront;
    private readonly IModelMetadataProvider _metadata;

    public FramedNotFoundPageRenderer(StorefrontFrameBuilder storefront, IModelMetadataProvider metadata)
    {
        ArgumentNullException.ThrowIfNull(storefront);
        ArgumentNullException.ThrowIfNull(metadata);
        _storefront = storefront;
        _metadata = metadata;
    }

    public async Task RenderAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        StorefrontFrame frame = await _storefront.BuildAsync(context, readsMergeNotice: false, context.RequestAborted);
        context.Items[StorefrontFrameAccess.ItemKey] = frame;

        // The way to the products is the home page's own list: the menu's first-level categories, none while no product is published (04 5.6.3).
        IReadOnlyList<MenuCategoryNode> productLinks = frame.Menu is { } menu ? FrameMenuTree.Build(menu.Categories) : [];
        NotFoundPageView model = new(FramePaths.Home, productLinks);

        ViewDataDictionary<NotFoundPageView> viewData = new(_metadata, new ModelStateDictionary()) { Model = model };
        ViewResult result = new() { ViewName = ViewPath, ViewData = viewData, StatusCode = StatusCodes.Status404NotFound };
        await result.ExecuteResultAsync(new ActionContext(context, context.GetRouteData(), new ActionDescriptor()));
    }
}
