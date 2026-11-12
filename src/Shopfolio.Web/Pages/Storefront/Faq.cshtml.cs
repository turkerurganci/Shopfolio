using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Pages.Storefront;

/// <summary>
/// The FAQ page (E-09; T506; 07 UC-13; 04 5.9): one GET that lists the published questions in the firm's order from the module's one view (<see cref="ICorporateContentModule.GetFaqAsync"/>). A browser
/// that holds a panel session also sees the draft questions in their place, marked "Taslak" (02 3.27.25). There is no paging, no search and no filter; a page opened directly while no question is
/// published shows the empty-state line (04 5.9.8). The structured data is in the first response (05 3.7.2): the page's head is written by the view.
/// </summary>
public sealed class FaqModel : PageModel
{
    private readonly ICorporateContentModule _content;

    public FaqModel(ICorporateContentModule content)
    {
        ArgumentNullException.ThrowIfNull(content);
        _content = content;
    }

    /// <summary>The questions the request may see.</summary>
    public IReadOnlyList<FaqQuestionView> Questions { get; private set; } = [];

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        bool asAdministrator = await HttpContext.HasAdminSessionAsync();
        Questions = await _content.GetFaqAsync(asAdministrator, ct);
        return Page();
    }
}
