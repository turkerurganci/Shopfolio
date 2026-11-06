using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Ordering.Contracts;

// The namespace is deliberately the application's page namespace, not the folder's: the page rule reads types by that namespace (09 6 item 2).
#pragma warning disable IDE0130
namespace Shopfolio.Pages.Panel;
#pragma warning restore IDE0130

// The available-operations pattern as a REAL page of the application's page namespace uses it (T217 r1 B1; 09 8.3 items 1 and 3; 09 6 items 2 and 7).
// The type lives in the unit test assembly but in the namespace the page rule of ModuleBoundaryTests reads (Shopfolio.Pages), so the architecture test sees it and
// proves the shape clean: the page owns the transaction through OperationRunner (common infrastructure), takes the order lock through the Contracts interface
// IOrderingModule, asks the one rule instance in both the GET and the POST, and reaches no module internal, no entity and no database context. The load and the write
// are the module's open methods in a real screen (Contracts methods joining the page's transaction); here they sit behind the PatternOperations stand-in.

/// <summary>The plain value the pattern's rule reads.</summary>
internal sealed record PatternState(bool Open, int OpenQuantity);

/// <summary>The pattern's rule: one function of a plain value (09 8.3 item 1).</summary>
internal sealed class PatternRule : IOperationRule<PatternState>
{
    public OperationAvailability Evaluate(PatternState context) =>
        context.Open && context.OpenQuantity > 0
            ? OperationAvailability.OpenForQuantity(context.OpenQuantity, context.OpenQuantity, context.OpenQuantity)
            : OperationAvailability.Closed;
}

/// <summary>The stand-in of a module's open load and write methods (Contracts methods in a real screen).</summary>
internal interface IPatternOperations
{
    Task<PatternState> ReadAsync(Guid orderId, CancellationToken ct);

    Task ApplyAsync(Guid orderId, int quantity, CancellationToken ct);
}

/// <summary>A page that shows an operation from the rule's availability and applies it through the runner, the Contracts lock and the same rule (09 8.3 item 3).</summary>
internal sealed class OperationPatternPage(
    IOrderingModule ordering,
    OperationRunner runner,
    IPatternOperations operations,
    IOperationRule<PatternState> rule) : PageModel
{
    internal OperationAvailability Availability { get; private set; }

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        Availability = rule.Evaluate(await operations.ReadAsync(id, ct));
        return Page();
    }

    [GatedOperation]
    public async Task<IActionResult> OnPostAsync(Guid id, int version, int quantity, CancellationToken ct)
    {
        GatedOperationResult result = await runner.RunAsync(
            stepCt => ordering.LockOrderAsync(id, version, stepCt),
            stepCt => operations.ReadAsync(id, stepCt),
            rule,
            quantity,
            (state, availability, stepCt) => operations.ApplyAsync(id, quantity, stepCt),
            ct);
        return result.IsRejected ? StatusCode(StatusCodes.Status409Conflict) : Redirect("/");
    }
}
