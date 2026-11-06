using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The answers of a request the page has no handler for (07 2.7.1; VAL-A424, VAL-A425, VAL-A433). Without this filter the framework shows the page
/// again with the status 200 when no handler matches, which would hide a broken form behind a normal-looking page. Two cases, told apart by what the
/// page offers: a method the page has no handler of at all (a PUT, DELETE or PATCH, or a POST to a page that only shows) is 405 with the methods the
/// page answers in <c>Allow</c>; a POST to a page that has POST handlers, with no <c>handler</c> parameter or an unknown one, is 400 and the sent
/// form is not run. The filter runs before the forgery check (<see cref="FilterOrder"/>): a method the page does not take is 405 whatever the token says.
/// </summary>
internal sealed class HandlerRequiredFilter : IAsyncPageFilter, IOrderedFilter
{
    /// <summary>The status of a POST without a matching handler on a page that takes POST (07 2.7.1; VAL-A433).</summary>
    public const int Status = StatusCodes.Status400BadRequest;

    /// <summary>The status of a method the page has no handler of (07 2.7.1; VAL-A424, VAL-A425).</summary>
    public const int MethodNotAllowedStatus = StatusCodes.Status405MethodNotAllowed;

    /// <summary>Below the forgery check (-100) and above the single catching layer's page filter (-300).</summary>
    public const int FilterOrder = -200;

    public int Order => FilterOrder;

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        if (context.HandlerMethod is null && IsWritingMethod(context.HttpContext.Request.Method))
        {
            context.Result = ResultFor(context);
            return Task.CompletedTask;
        }

        return next();
    }

    internal static bool IsWritingMethod(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);

    private static StatusCodeResult ResultFor(PageHandlerExecutingContext context)
    {
        string method = context.HttpContext.Request.Method;
        string[] taken = context.ActionDescriptor is CompiledPageActionDescriptor compiled
            ? [.. compiled.HandlerMethods.Select(handler => handler.HttpMethod ?? string.Empty).Where(name => name.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase)]
            : [];
        bool pageTakesPost = taken.Contains(HttpMethods.Post, StringComparer.OrdinalIgnoreCase);
        if (HttpMethods.IsPost(method) && pageTakesPost)
        {
            return new StatusCodeResult(Status);
        }

        context.HttpContext.Response.Headers.Allow = string.Join(", ", taken.Prepend(HttpMethods.Get).Distinct(StringComparer.OrdinalIgnoreCase));
        return new StatusCodeResult(MethodNotAllowedStatus);
    }
}
