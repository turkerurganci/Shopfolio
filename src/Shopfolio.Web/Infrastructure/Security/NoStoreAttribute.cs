using Microsoft.AspNetCore.Mvc.Filters;

namespace Shopfolio.Infrastructure.Security;

/// <summary>
/// <c>Cache-Control: no-store</c> on a page's response (09 15.2 item 5; 05 6.5.4; 07 2.5.6): the response carries personal data, a session, a cart or
/// an access key. The panel and the account folders get it by a folder convention (<see cref="ForgerySetup"/>), the cart, the payment step and the
/// pages of the keyed links by this attribute; the static files, the images and the public content pages do not carry it. The header is written when the
/// response starts, after the form tag's anti-forgery token has set its own <c>no-cache, no-store</c> pair, so the value is the one of the rule.
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class NoStoreAttribute : Attribute, IResultFilter
{
    public void OnResultExecuting(ResultExecutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        context.HttpContext.Response.OnStarting(
            static state =>
            {
                ((HttpContext)state).Response.Headers.CacheControl = SecurityHeaders.NoStore;
                return Task.CompletedTask;
            },
            context.HttpContext);
    }

    public void OnResultExecuted(ResultExecutedContext context)
    {
    }
}
