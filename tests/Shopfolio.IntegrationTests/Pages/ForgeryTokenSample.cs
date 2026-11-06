using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// The test-only page that hands a test client the forgery token for the request's own user (T209; 07 2.5.4): the cookie half comes with the answer
/// and the request half is the body. The <c>ForgeryTokenHandler</c> of the test clients asks it before every writing request. The panel's identity
/// is read by its own scheme only on the panel's pages, so the handler names the scheme of the page it will write to. It shows a GET only.
/// </summary>
public sealed class ForgeryTokenSampleModel(IAntiforgery antiforgery) : PageModel
{
    public async Task<IActionResult> OnGetAsync(string? scheme)
    {
        if (scheme is not null)
        {
            AuthenticateResult result = await HttpContext.AuthenticateAsync(scheme);
            if (result.Succeeded)
            {
                HttpContext.User = result.Principal;
            }
        }

        return Content(antiforgery.GetAndStoreTokens(HttpContext).RequestToken ?? string.Empty);
    }
}
