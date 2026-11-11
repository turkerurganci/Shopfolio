using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>
/// The test-only page of a panel screen that has no frame (T504; 04 2.2.9): it stands under the invitation's open address, opens without a session and fails in the handler, so the page-level error
/// page of a frameless screen is drawn bare, with the panel title only.
/// </summary>
public sealed class FramelessErrorSampleModel : PageModel
{
    public const string Path = "/panel/davet/hata-ornegi"; // the template is the open "/panel/davet/{}" of 07 2.3.1

    public const string FailureText = "t504 failure in the frameless handler";

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1822", Justification = "Razor Pages ignores static handler methods.")]
    public IActionResult OnGet(string key) => throw new InvalidOperationException(FailureText);
}
