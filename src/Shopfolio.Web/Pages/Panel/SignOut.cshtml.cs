using System.Security.Claims;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration.Contracts;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The administrator's sign-out (T520; 07 9.26.1 UC-109; 03 9.2.3): the frame's form posts here. The session row and the panel's session cookie are deleted; the storefront's session and cart are another scheme and
/// are not touched (K-899); the known-browser mark stays (06 4.41). The answer is 303 to the panel's sign-in. A request without a session reaches the page too - the page is marked as a sign-out, so the
/// challenge sends it to the same redirect and nothing is written (07 2.3.2; 09 15.1 item 2). The page has no GET: it is the target of the frame's form only.
/// </summary>
[NoStore]
[SignOutPage]
internal sealed class SignOutModel : PageModel
{
    private readonly IAdministrationModule _administration;
    private readonly ITransactionRunner _transactions;

    public SignOutModel(IAdministrationModule administration, ITransactionRunner transactions)
    {
        ArgumentNullException.ThrowIfNull(administration);
        ArgumentNullException.ThrowIfNull(transactions);
        _administration = administration;
        _transactions = transactions;
    }

    // The sign-out is a POST (07 9.26.1): the address opened by hand is not a page.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822", Justification = "Razor Pages ignores static handler methods.")]
    public IActionResult OnGet() => AccessDenial.NotFound();

    public async Task<IActionResult> OnPostSignOutAsync(CancellationToken ct)
    {
        if (Guid.TryParse(User.FindFirstValue(SessionSchemes.SessionIdClaim), out Guid sessionId))
        {
            await _transactions.RunAsync(async token => new SessionEnd(await _administration.EndSessionAsync(sessionId, token)), ct);
        }

        BrowserCookies.Delete(HttpContext, CookieNames.AdminSession);
        return FormResults.SeeOther(SessionSchemes.AdminSignInPath);
    }

    private sealed record SessionEnd(bool Ended) : IOperationResult
    {
        public bool IsRejected => false;
    }
}
