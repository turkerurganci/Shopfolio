using System.Reflection;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Forms;

namespace Shopfolio.Infrastructure.Errors;

/// <summary>
/// The first entry point of the single catching layer (09 8.1 item 2a): a page filter around the handler. It is the outermost of the page filters
/// (<see cref="FilterOrder"/>), so an exception of the anti-forgery check, the validation filter, the binding or the handler itself reaches it; and it is
/// the one place in a POST where the bound model and the model state are still at hand, so the page can be shown again with the sent values. The filter
/// decides nothing: <see cref="UnexpectedErrorLayer"/> gives the state and the filter only makes the page framework's result of it.
/// </summary>
internal sealed class UnexpectedErrorPageFilter : IAsyncPageFilter, IOrderedFilter
{
    /// <summary>Below the handler-required filter (-200) and the forgery check (-100): the layer sees every failure of the handler's filters.</summary>
    public const int FilterOrder = -300;

    private readonly UnexpectedErrorLayer _layer;

    public UnexpectedErrorPageFilter(UnexpectedErrorLayer layer)
    {
        _layer = layer;
    }

    public int Order => FilterOrder;

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        OutcomeUncertainAttribute? marked = context.HandlerMethod?.MethodInfo.GetCustomAttribute<OutcomeUncertainAttribute>();
        if (marked is not null)
        {
            context.HttpContext.Items[OperationMark.ItemKey] = new OperationMark(marked.Operation, context.HandlerInstance as IOutcomeTarget);
        }

        return Task.CompletedTask;
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        PageHandlerExecutedContext executed = await next();
        if (executed.Exception is not { } exception || executed.ExceptionHandled)
        {
            return;
        }

        ErrorResolution resolution = _layer.Resolve(context.HttpContext, exception);
        executed.ExceptionHandled = true;
        executed.Result = ResultOf(resolution, context);
    }

    private static IActionResult ResultOf(ErrorResolution resolution, PageHandlerExecutingContext context)
    {
        switch (resolution.State)
        {
            case ErrorState.OutcomeUncertain:
                return FormResults.SeeOther(resolution.Target ?? "/");
            case ErrorState.OperationLevel when !resolution.ClientGone && context.HandlerInstance is PageModel page:
                page.ModelState.AddModelError(string.Empty, UnexpectedErrorAnswer.OperationMessageFor(context.HttpContext.Request));
                return FormResults.UnexpectedFailure(page);
            default:
                return new ErrorAnswerResult(resolution);
        }
    }

    private sealed class ErrorAnswerResult : IActionResult
    {
        private readonly ErrorResolution _resolution;

        public ErrorAnswerResult(ErrorResolution resolution)
        {
            _resolution = resolution;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            return UnexpectedErrorAnswer.WriteAsync(context.HttpContext, _resolution);
        }
    }
}

/// <summary>
/// The second entry point (09 8.1 item 2b): the framework's exception handling middleware equivalent, for what falls outside the handler - before it is
/// reached or while its result is drawn - and for the machine endpoints. It is registered after the cookie header filter (09 15.1 item 6): the filter
/// stays outside every middleware that can write a cookie and this layer writes none (D-113).
/// </summary>
internal sealed class UnexpectedErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly UnexpectedErrorLayer _layer;

    public UnexpectedErrorMiddleware(RequestDelegate next, UnexpectedErrorLayer layer)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(layer);
        _next = next;
        _layer = layer;
    }

#pragma warning disable CA1031 // The single catching layer is one of the six places of the wide catch (09 8.1 item 6).
    public async Task InvokeAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            ErrorResolution resolution = _layer.Resolve(context, exception);
            await UnexpectedErrorAnswer.WriteAsync(context, resolution);
        }
    }
#pragma warning restore CA1031
}

/// <summary>The registration of the single catching layer (09 8.1).</summary>
internal static class UnexpectedErrorSetup
{
    public static IMvcBuilder AddUnexpectedErrorLayer(this IMvcBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        builder.Services.AddSingleton<UnexpectedErrorCounter>();
        builder.Services.AddSingleton<UnexpectedErrorLayer>();
        builder.Services.Configure<MvcOptions>(options => options.Filters.Add<UnexpectedErrorPageFilter>(UnexpectedErrorPageFilter.FilterOrder));
        return builder;
    }

    public static IApplicationBuilder UseUnexpectedErrorLayer(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<UnexpectedErrorMiddleware>();
    }
}
