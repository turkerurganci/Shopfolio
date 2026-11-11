using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Pages.Frame;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>A storefront page drawn with the real storefront layout (T540 KK5-KK7): the body is one marked line so the tests can tell the content region from the frame.</summary>
public sealed class FrameLayoutSampleModel : PageModel
{
    public IActionResult OnGet() => Page();
}

/// <summary>A storefront page that asks for the draft band (T540 KK9; 04 2.9.2): a local panel address, the way a draft product's page does.</summary>
public sealed class DraftPreviewSampleModel : PageModel
{
    public IActionResult OnGet()
    {
        HttpContext.SetDraftPreview("/panel/urunler/ornek-kayit");
        return Page();
    }
}

/// <summary>A panel page drawn with the real panel layout: its path is under the Ürünler entry, so the Katalog group is the active one (T540 KK8).</summary>
public sealed class PanelLayoutSampleModel : PageModel
{
    public IActionResult OnGet() => Page();
}


