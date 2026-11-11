using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Modules.CorporateContent.Contracts;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.Pages;

/// <summary>
/// The home page (E-01; T502; 07 UC-01; 04 5.1): one GET, four blocks in the order of the layout the firm chose - the corporate block, "Hizmetlerimiz", "Referanslarımız" and the product showcase -
/// from the module's one view (<see cref="ICorporateContentModule.GetHomepageAsync"/>). A block without anything to show is not in it and its link goes with it; the corporate block falls back to
/// the brand name and logo. The page asks nothing of the sales gate: the showcase stays while sales are closed (04 5.1.10), and the search engine data is in the first response (05 3.7.2).
/// </summary>
public sealed class IndexModel : PageModel
{
    private readonly ICorporateContentModule _content;

    public IndexModel(ICorporateContentModule content)
    {
        ArgumentNullException.ThrowIfNull(content);
        _content = content;
    }

    public HomePage Home { get; private set; } = HomePage.Empty;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        Home = HomePage.Of(await _content.GetHomepageAsync(ct));
        return Page();
    }
}
