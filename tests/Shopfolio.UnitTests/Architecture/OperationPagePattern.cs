using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Operations;

// The namespaces are deliberately the application's: the page in the page namespace, the module's open interface in the Ordering Contracts namespace; the page rule and the
// module rule of ModuleBoundaryTests read types by those namespaces (09 6 item 2).
#pragma warning disable IDE0130
namespace Shopfolio.Modules.Ordering.Contracts
{
    /// <summary>
    /// The stand-in of a module's open interface for an operation (the real module's <c>Contracts</c> interface in a real screen): the read gives the availability the module's
    /// rule decided; the write runs that same rule under the row lock inside the caller's transaction and applies only when it allows (09 8.3 items 1 and 3).
    /// </summary>
    public interface IPatternOrderOperations
    {
        Task<OperationAvailability> GetAsync(Guid orderId, CancellationToken ct);

        Task<GatedOperationResult> ApplyAsync(Guid orderId, int? shownVersion, int quantity, CancellationToken ct);
    }
}

// The available-operations pattern as a REAL page of the application's page namespace uses it (T217 r1 B1, r2 B1-r2; 09 8.3 items 1 and 3; 09 6 items 2 and 7).
// The type lives in the unit test assembly but in the namespace the page rule of ModuleBoundaryTests reads (Shopfolio.Pages), so the architecture test sees it and
// proves the shape clean: the page owns the transaction through OperationRunner (common infrastructure) and reaches the product rule only through the module's Contracts
// interface - the read for the GET, the write for the POST, both running the module's one rule. It writes no rule, applies no rule, and reaches no module internal, no
// entity and no database context.
namespace Shopfolio.Pages.Panel
{
    using Shopfolio.Modules.Ordering.Contracts;

    /// <summary>A page that shows an operation from the availability the module's rule gave and applies it through the runner and the module's Contracts write method (09 8.3 item 3).</summary>
    internal sealed class OperationPatternPage(IPatternOrderOperations operations, OperationRunner runner) : PageModel
    {
        internal OperationAvailability Availability { get; private set; }

        public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
        {
            Availability = await operations.GetAsync(id, ct);
            return Page();
        }

        [GatedOperation]
        public async Task<IActionResult> OnPostAsync(Guid id, int version, int quantity, CancellationToken ct)
        {
            GatedOperationResult result = await runner.RunAsync(stepCt => operations.ApplyAsync(id, version, quantity, stepCt), ct);
            return result.IsRejected ? StatusCode(StatusCodes.Status409Conflict) : Redirect("/");
        }
    }
}
