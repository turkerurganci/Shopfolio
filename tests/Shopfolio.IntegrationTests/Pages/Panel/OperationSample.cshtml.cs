using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.IntegrationTests.Pages.Panel;

/// <summary>
/// The test-only sample of the available-operations pattern (T217; 07 8.1; 09 8.3). The rule lives in the module, not here: the GET asks the module's <c>Contracts</c>
/// read method (<see cref="ISampleOrderOperations"/>) for the availability and draws a form only for an open operation, writing the open quantity, the default, the upper
/// limit and the last day as text; the three POST handlers open the transaction through <see cref="OperationRunner"/> and call the module's <c>Contracts</c> write method,
/// which under the order's row lock runs the same rule instance (lock, version, load, rule, write) and applies only when it allows. The page writes and applies no rule
/// and reaches no module internals and no database context (09 6 item 2); <c>ModuleBoundaryTests</c> shows the same shape, as a page of the <c>Shopfolio.Pages</c>
/// namespace, clean under the page rule. The customer's two operations answer a refusal with 303 and the
/// reason; the panel's operation answers a changed version with 409 and a closed operation with 422. The page has no script and decides nothing from what the browser sent.
/// </summary>
public sealed class OperationSampleModel : PageModel
{
    public const string AdminClosedMessage = "Bu işlem şu anda yapılamaz (örnek).";

    internal OperationAvailability Cancel { get; private set; }

    internal OperationAvailability Withdraw { get; private set; }

    public Guid RecordId { get; private set; }

    public Guid ItemId { get; private set; }

    public int Version { get; private set; }

    public string Path => "/panel/islem-ornegi/" + RecordId;

    public CancelInput CancelForm { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct)
    {
        await LoadAsync(id, ct);
        return Page();
    }

    [GatedOperation]
    public async Task<IActionResult> OnPostCustomerCancelAsync(Guid id, CancelInput input, CancellationToken ct)
    {
        ISampleOrderOperations operations = HttpContext.RequestServices.GetRequiredService<ISampleOrderOperations>();
        GatedOperationResult result = await RunAsync(stepCt => operations.CancelAsync(id, null, input.Quantity, stepCt), ct);
        return AnswerCustomer(id, result);
    }

    [GatedOperation]
    public async Task<IActionResult> OnPostCustomerWithdrawAsync(Guid id, CancelInput input, CancellationToken ct)
    {
        ISampleOrderOperations operations = HttpContext.RequestServices.GetRequiredService<ISampleOrderOperations>();
        GatedOperationResult result = await RunAsync(stepCt => operations.WithdrawAsync(id, null, input.Quantity, stepCt), ct);
        return AnswerCustomer(id, result);
    }

    [GatedOperation]
    public async Task<IActionResult> OnPostAdminCancelAsync(Guid id, AdminInput input, CancellationToken ct)
    {
        ISampleOrderOperations operations = HttpContext.RequestServices.GetRequiredService<ISampleOrderOperations>();
        GatedOperationResult result = await RunAsync(stepCt => operations.CancelAsync(id, input.Version, input.Quantity, stepCt), ct);
        if (!result.IsRejected)
        {
            return FormResults.SeeOther("/panel/islem-ornegi/" + id);
        }

        await LoadAsync(id, ct);
        return result.VersionChanged
            ? FormResults.Conflict(this)
            : OperationRefusals.Redisplay(this, result.Verdict, AdminClosedMessage);
    }

    // The page opens the transaction (09 9.1 item 3) and calls the module's Contracts write method inside it; the module runs the rule.
    private Task<GatedOperationResult> RunAsync(Func<CancellationToken, Task<GatedOperationResult>> operation, CancellationToken ct) =>
        HttpContext.RequestServices.GetRequiredService<OperationRunner>().RunAsync(operation, ct);

    private IActionResult AnswerCustomer(Guid id, GatedOperationResult result)
    {
        string orderPage = "/panel/islem-ornegi/" + id;
        return result.IsRejected
            ? OperationRefusals.ToOrderPage(this, result.Verdict, orderPage)
            : FormResults.SeeOther(orderPage);
    }

    private async Task LoadAsync(Guid id, CancellationToken ct)
    {
        SampleOrderOperationsView view = await HttpContext.RequestServices.GetRequiredService<ISampleOrderOperations>().ReadAsync(id, ct);
        RecordId = id;
        ItemId = view.ItemId;
        Version = view.Version;
        Cancel = view.Cancel;
        Withdraw = view.Withdraw;
    }
}

/// <summary>The quantity form of the customer's sample operations: the quantity only; nothing the server derived travels back.</summary>
public sealed class CancelInput : IFormInput
{
    public int Quantity { get; set; }
}

/// <summary>The panel's sample operation form: the order's version and the quantity.</summary>
public sealed class AdminInput : IFormInput
{
    public int Version { get; set; }

    public int Quantity { get; set; }
}
