using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// A POST whose <c>handler</c> parameter is missing or names no handler of the page is answered 400 and the page does not run (07 2.7.1: the answer of
/// an unknown or missing handler is 404 or 400 and the doctrine fixes it by a test). Without this filter the framework shows the page again with
/// the status 200 when no handler matches, which would hide a broken form behind a normal-looking page. The filter reads only the selected handler,
/// so a page that names every POST (07 2.2, 2.2.1) is never touched by it.
/// </summary>
internal sealed class HandlerRequiredFilter : IAsyncPageFilter
{
    /// <summary>The status of a POST without a matching handler (07 2.7.1).</summary>
    public const int Status = StatusCodes.Status400BadRequest;

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context) => Task.CompletedTask;

    public Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        if (context.HandlerMethod is null && IsWritingMethod(context.HttpContext.Request.Method))
        {
            context.Result = new StatusCodeResult(Status);
            return Task.CompletedTask;
        }

        return next();
    }

    internal static bool IsWritingMethod(string method) =>
        HttpMethods.IsPost(method) || HttpMethods.IsPut(method) || HttpMethods.IsPatch(method) || HttpMethods.IsDelete(method);
}
