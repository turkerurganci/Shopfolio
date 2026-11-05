// Deliberate violation sample of the page handler rule (07 2.2, 2.2.1; T206). Compiled into ModuleBoundaryViolationSample.dll with the module samples.
// A page's GET handler may be unnamed; every other handler is a named POST handler (OnPost<Name>), so an unnamed OnPost / OnPostAsync and every verb
// other than GET and POST is a violation. AllowedHandlers is the quiet neighbour.

using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Shopfolio.HandlerSamples
{
    public sealed class AllowedHandlers : PageModel
    {
        public void OnGet()
        {
        }

        public IActionResult OnPostAdd() => Page();

        public Task<IActionResult> OnPostRemoveAsync() => Task.FromResult<IActionResult>(Page());

        public Task<IActionResult> OnGetExportAsync() => Task.FromResult<IActionResult>(Page());

        public static string OnPostalCode() => "34000";
    }

    public sealed class UnnamedPost : PageModel
    {
        public IActionResult OnPost() => Page();
    }

    public sealed class UnnamedPostAsync : PageModel
    {
        public Task<IActionResult> OnPostAsync() => Task.FromResult<IActionResult>(Page());
    }

    public sealed class OtherVerbs : PageModel
    {
        public IActionResult OnPut() => Page();

        public Task<IActionResult> OnDeleteAsync() => Task.FromResult<IActionResult>(Page());

        public IActionResult OnPatchRename() => Page();
    }
}
