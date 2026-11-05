using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The results of a page's POST handler (07 2.2.2, 2.7.1): a successful writing POST is a 303 to a GET, and a POST that writes nothing shows the page
/// again with the values that were sent, with the status 422 for a validation error or a named business-rule refusal. The framework's own
/// <c>RedirectToPage</c> answers 302 and <c>Page()</c> answers 200, which is why a handler returns one of these instead (07 2.7.4: a validation
/// error is not a 200). The conflict (409), the limit (429) and the other statuses of 07 2.7.1 are the results of the tasks that own them.
/// </summary>
internal static class FormResults
{
    /// <summary>The status of a successful writing POST and of a refusal that names its reason by a redirect (07 2.2.2, 2.7.1).</summary>
    public const int SeeOtherStatus = StatusCodes.Status303SeeOther;

    /// <summary>The status of a validation error and of a named business-rule refusal (07 2.7.1).</summary>
    public const int UnprocessableStatus = StatusCodes.Status422UnprocessableEntity;

    /// <summary>
    /// 303 to a path of this site (07 2.2.2). The path is the site's own - it starts with one slash and is not a scheme-relative or backslash form -
    /// so a handler cannot send the browser to another site by a value that came from the request.
    /// </summary>
    public static IActionResult SeeOther(string localPath)
    {
        ArgumentNullException.ThrowIfNull(localPath);
        if (!IsLocalPath(localPath))
        {
            throw new ArgumentException("The target of a redirect is a path of this site that starts with one slash (05 6.5.6).", nameof(localPath));
        }

        return new SeeOtherResult(localPath);
    }

    /// <summary>The page again with the values that were sent, status 422 (07 2.2.2, 2.7.1): the handler's model state carries the messages.</summary>
    public static IActionResult Redisplay(PageModel page)
    {
        ArgumentNullException.ThrowIfNull(page);
        PageResult result = page.Page();
        result.StatusCode = UnprocessableStatus;
        return result;
    }

    internal static bool IsLocalPath(string path) =>
        path.Length > 0
        && path[0] == '/'
        && (path.Length == 1 || (path[1] != '/' && path[1] != '\\'))
        && !path.Contains('\n', StringComparison.Ordinal)
        && !path.Contains('\r', StringComparison.Ordinal);

    private sealed class SeeOtherResult : IActionResult
    {
        private readonly string _location;

        public SeeOtherResult(string location)
        {
            _location = location;
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            context.HttpContext.Response.StatusCode = SeeOtherStatus;
            context.HttpContext.Response.Headers.Location = _location;
            return Task.CompletedTask;
        }
    }
}
