using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Membership.Contracts;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The member's sign-out (T519; 07 4.34.1 UC-77; 03 2.3.7, 9.2.3): the frame's form posts here with the page the request came from. The session row and the session cookie are deleted; the cart stays on the account
/// and the browser then shows an empty one (the guest cart's cookie went at the sign-in); the known-browser mark stays (06 4.41). The answer is 303 to the page the form came from when it needs no session, else to the
/// home page (<see cref="ReturnPath.AfterSignOut"/>; 04 3.6.4; K-1186 (6)). A request without a customer session reaches the page too - it is marked as a sign-out, so the challenge sends it to the same redirect and
/// nothing is written (07 2.3.2; 09 15.1 item 2); a repeated post finds no session and gets the same answer. The page has no GET: it is the target of the frame's form only.
/// </summary>
[Authorize(Policy = SessionSchemes.CustomerPolicy)]
[SignOutPage]
[NoStore]
internal sealed class SignOutModel : PageModel
{
    private readonly ISessionService _sessions;
    private readonly ITransactionRunner _transactions;

    public SignOutModel(ISessionService sessions, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(sessions);
        ArgumentNullException.ThrowIfNull(transactions);
        _sessions = sessions;
        _transactions = transactions;
    }

    // The sign-out is a POST (07 4.34.1): the address opened by hand is not a page.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822", Justification = "Razor Pages ignores static handler methods.")]
    public IActionResult OnGet() => AccessDenial.NotFound();

    public async Task<IActionResult> OnPostSignOutAsync(string? returnUrl, CancellationToken ct)
    {
        if (Guid.TryParse(User.FindFirstValue(SessionSchemes.SessionIdClaim), out Guid sessionId))
        {
            await _transactions.RunAsync(async cancel => new SessionEnd(await _sessions.EndAsync(sessionId, cancel)), ct);
        }

        BrowserCookies.Delete(HttpContext, CookieNames.Session);
        return FormResults.SeeOther(ReturnPath.AfterSignOut(returnUrl));
    }

    private sealed record SessionEnd(bool Ended) : IOperationResult
    {
        public bool IsRejected => false;
    }
}
