using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shopfolio.IntegrationTests.Pages;

/// <summary>A test-only page that only shows (GET): the other methods have no handler on it (T212; 07 2.7.1; VAL-A424, VAL-A425).</summary>
public sealed class ReadOnlySampleModel : PageModel
{
    public static void OnGet()
    {
    }
}
