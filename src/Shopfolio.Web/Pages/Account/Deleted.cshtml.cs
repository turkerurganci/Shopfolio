using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Account;

/// <summary>
/// The result state of the account's deletion (T517; 07 UC-69; 04 5.25.13; K-781, K-1184): the one page under <c>/hesap</c> that opens without a session. The sign that it is shown once is the session cookie of the
/// deleted session: the request carries the vitrin's session cookie and the session it points to no longer exists, so the state is drawn and this answer deletes the cookie. A request without the cookie - the page
/// reloaded, opened from the history or by hand - or with a cookie that points to a live session goes to the home page (303). The state is not a short-lived notice (04 2.3.4). The response is personal and not stored.
/// </summary>
[NoStore]
internal sealed class DeletedModel : PageModel
{
    public IActionResult OnGet()
    {
        if (string.IsNullOrEmpty(BrowserCookies.Read(HttpContext, CookieNames.Session)) || User.Identity?.IsAuthenticated == true)
        {
            return FormResults.SeeOther(FramePaths.Home);
        }

        // The sign is spent: a second request carries no cookie and goes home (07 UC-69).
        BrowserCookies.Delete(HttpContext, CookieNames.Session);
        return Page();
    }
}
