using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shopfolio.Pages.Frame;

/// <summary>
/// Makes the storefront frame part of every storefront page response (T537; 07 5.5.1; K-1172 (2)): after the handler, when the response is the page itself, the frame is built from the request's
/// context and put where the layout reads it (<see cref="StorefrontFrameAccess"/>). No page model holds it and no page adds it to its own view. A redirect, a handler that failed, a response the
/// error layer wrote and every page of the panel get no frame - the panel has its own (07 5.5.1 <c>PanelFrame</c>) - and nothing of the merge message is read for them (07 4.34.3).
/// </summary>
internal sealed class StorefrontFrameFilter : IAsyncPageFilter, IOrderedFilter
{
    /// <summary>Inside the error layer (-300), the handler-required filter (-200) and the forgery check (-100): the layer still sees a failure of the frame's own reads.</summary>
    public const int FilterOrder = 100;

    /// <summary>The page folder of the panel: its pages have their own frame.</summary>
    private const string PanelFolderPrefix = "/Panel/";

    private readonly StorefrontFrameBuilder _builder;

    public StorefrontFrameFilter(StorefrontFrameBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        _builder = builder;
    }

    public int Order => FilterOrder;

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        PageHandlerExecutedContext executed = await next();
        if (executed.Exception is not null || executed.Result is not (null or PageResult) || IsPanelPage(context))
        {
            return;
        }

        context.HttpContext.Items[StorefrontFrameAccess.ItemKey] = await _builder.BuildAsync(context.HttpContext, readsMergeNotice: true, context.HttpContext.RequestAborted);
    }

    private static bool IsPanelPage(PageHandlerExecutingContext context) =>
        context.ActionDescriptor.RelativePath.StartsWith(PanelFolderPrefix, StringComparison.OrdinalIgnoreCase);
}

/// <summary>Where the layout finds the frame of the request (T537): the request's items, written by <see cref="StorefrontFrameFilter"/> and read here only.</summary>
public static class StorefrontFrameAccess
{
    internal const string ItemKey = "Shopfolio.StorefrontFrame";

    /// <summary>The storefront frame of this request, or null when the response is not a storefront page (a redirect, a machine endpoint, a panel page, the error page).</summary>
    public static StorefrontFrame? GetStorefrontFrame(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return context.Items.TryGetValue(ItemKey, out object? value) ? value as StorefrontFrame : null;
    }
}

/// <summary>The registration of the storefront frame (T537).</summary>
internal static class StorefrontFrameSetup
{
    public static IMvcBuilder AddStorefrontFrame(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddScoped<StorefrontFrameBuilder>();
        builder.Services.AddScoped<PanelFrameBuilder>();
        builder.Services.Configure<MvcOptions>(options =>
        {
            options.Filters.Add<StorefrontFrameFilter>(StorefrontFrameFilter.FilterOrder);
            options.Filters.Add<PanelFrameFilter>(StorefrontFrameFilter.FilterOrder);
        });
        return builder;
    }
}
