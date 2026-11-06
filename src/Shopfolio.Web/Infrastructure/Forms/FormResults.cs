using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Notices;

namespace Shopfolio.Infrastructure.Forms;

/// <summary>
/// The results of a page's POST handler (07 2.2.2, 2.7.1): a successful writing POST is a 303 to a GET, and a POST that writes nothing shows the page
/// again with the values that were sent, with the status 422 for a validation error or a named business-rule refusal. The framework's own
/// <c>RedirectToPage</c> answers 302 and <c>Page()</c> answers 200, which is why a handler returns one of these instead (07 2.7.4: a validation
/// error is not a 200). The conflict (409; T208) is the result of a write refused because the version or the shown value changed; the limit (429) and the other statuses of 07 2.7.1 are the results of the tasks that own them.
/// </summary>
internal static class FormResults
{
    /// <summary>The status of a successful writing POST and of a refusal that names its reason by a redirect (07 2.2.2, 2.7.1).</summary>
    public const int SeeOtherStatus = StatusCodes.Status303SeeOther;

    /// <summary>The status of a validation error and of a named business-rule refusal (07 2.7.1).</summary>
    public const int UnprocessableStatus = StatusCodes.Status422UnprocessableEntity;

    /// <summary>The status of a write refused because the record, the order or the shown value changed (07 2.4.5, 2.4.6, 2.7.1).</summary>
    public const int ConflictStatus = StatusCodes.Status409Conflict;

    /// <summary>The status of a POST whose anti-forgery value is missing or does not match (07 2.5.4, 2.7.1, 6.2.1): nothing was written.</summary>
    public const int ForgeryRefusedStatus = StatusCodes.Status400BadRequest;

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

    /// <summary>
    /// 303 to a path of this site with a notice code in the <c>notice</c> parameter (07 2.2.3, 6.3): the short-lived notice of a successful write, the
    /// result state of a form or the reason of a return. The code is a member of the closed list and the address is the only place it travels - no cookie
    /// and no record on the server (<see cref="NoticeParameter.Address"/>).
    /// </summary>
    public static IActionResult SeeOther(string localPath, NoticeCode notice) => SeeOther(NoticeParameter.Address(localPath, notice));

    /// <summary>
    /// The same 303 written straight on a response, for the catching layer's middleware that has no action to return (09 8.1 item 2b). The path goes
    /// through the same check as <see cref="SeeOther"/>: the redirect is written in this file and in the access denial only (05 6.5.6).
    /// </summary>
    public static void WriteSeeOther(HttpResponse response, string localPath)
    {
        ArgumentNullException.ThrowIfNull(response);
        ArgumentNullException.ThrowIfNull(localPath);
        if (!IsLocalPath(localPath))
        {
            throw new ArgumentException("The target of a redirect is a path of this site that starts with one slash (05 6.5.6).", nameof(localPath));
        }

        response.StatusCode = SeeOtherStatus;
        response.Headers.Location = localPath;
    }

    /// <summary>The page again with the values that were sent, status 422 (07 2.2.2, 2.7.1): the handler's model state carries the messages.</summary>
    public static IActionResult Redisplay(PageModel page)
    {
        ArgumentNullException.ThrowIfNull(page);
        PageResult result = page.Page();
        result.StatusCode = UnprocessableStatus;
        return result;
    }

    /// <summary>
    /// The page again after a POST whose anti-forgery value failed, status 400 (07 2.5.4): the sent values and a message at the level of the action;
    /// the form tag writes a new value. The handler did not run (<see cref="Security.ForgeryProtectionFilter"/>).
    /// </summary>
    public static IActionResult ForgeryRefused(PageModel page)
    {
        ArgumentNullException.ThrowIfNull(page);
        PageResult result = page.Page();
        result.StatusCode = ForgeryRefusedStatus;
        return result;
    }

    /// <summary>
    /// The page again after a write that was refused because what the user saw is no longer what is there, status 409 (07 2.4.5, 2.4.6): nothing was
    /// written. A record variant shows the values the user sent, with the record's current version in the form and "Yine de kaydet" as its one button
    /// (<see cref="ConflictControls.RecordSaveControl"/>); an order variant shows the order as it is now, with no way to overwrite. The handler sets the
    /// version it wants the form to carry in the model, and the form writes it with <see cref="VersionFields"/> - never from the posted model state.
    /// </summary>
    public static IActionResult Conflict(PageModel page)
    {
        ArgumentNullException.ThrowIfNull(page);
        PageResult result = page.Page();
        result.StatusCode = ConflictStatus;
        return result;
    }

    /// <summary>
    /// The page again after an unexpected error in a POST, status 500 (05 10.3.3; 09 8.1 item 3): the sent values and a lasting message at the level
    /// of the action. Only the single catching layer returns it; a handler never does (09 8.1 item 1).
    /// </summary>
    internal static IActionResult UnexpectedFailure(PageModel page)
    {
        ArgumentNullException.ThrowIfNull(page);

        // The same header set as the layer's own 500 (UnexpectedErrorAnswer; 09 15.2 item 5; D-140): the page again carries the sent values. Written when
        // the response starts, after the form tag's token has set its own pair, as the no-store attribute does.
        page.HttpContext.Response.OnStarting(
            static state =>
            {
                ((HttpContext)state).Response.Headers.CacheControl = Security.SecurityHeaders.NoStore;
                return Task.CompletedTask;
            },
            page.HttpContext);
        PageResult result = page.Page();
        result.StatusCode = StatusCodes.Status500InternalServerError;
        return result;
    }

    // The one rule of the site's own path (05 6.5.6; D-118): the control characters - the tab too - are refused as well as the network and backslash forms.
    internal static bool IsLocalPath(string path) => Security.ReturnPath.IsLocalPath(path);

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
