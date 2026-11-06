using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.IntegrationTests.Pages.Panel;

/// <summary>
/// The test-only sample of the available-operations pattern (T217; 07 8.1; 09 8.3). The GET asks the two rules and draws a form only for an open operation,
/// writing the open quantity, the default, the upper limit and the last day as text; the three POST handlers re-check the same rule instances through
/// <see cref="OperationRunner"/>, which takes the order lock through <see cref="IOrderingModule"/>. The page reaches no module internals and no database context
/// (09 6 item 2): the load and the write sit behind <c>SampleOrderOperations</c>, the stand-in of the module's open methods, and
/// <c>ModuleBoundaryTests</c> shows the same shape, as a page of the <c>Shopfolio.Pages</c> namespace, clean under the page rule. The customer's two operations answer a refusal with 303 and the
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
        SampleRules rules = HttpContext.RequestServices.GetRequiredService<SampleRules>();
        GatedOperationResult result = await RunAsync(id, null, rules.Cancel, input.Quantity, ct);
        return AnswerCustomer(id, result);
    }

    [GatedOperation]
    public async Task<IActionResult> OnPostCustomerWithdrawAsync(Guid id, CancelInput input, CancellationToken ct)
    {
        SampleRules rules = HttpContext.RequestServices.GetRequiredService<SampleRules>();
        GatedOperationResult result = await RunAsync(id, null, rules.Withdraw, input.Quantity, ct);
        return AnswerCustomer(id, result);
    }

    [GatedOperation]
    public async Task<IActionResult> OnPostAdminCancelAsync(Guid id, AdminInput input, CancellationToken ct)
    {
        SampleRules rules = HttpContext.RequestServices.GetRequiredService<SampleRules>();
        GatedOperationResult result = await RunAsync(id, input.Version, rules.Cancel, input.Quantity, ct);
        if (!result.IsRejected)
        {
            return FormResults.SeeOther("/panel/islem-ornegi/" + id);
        }

        await LoadAsync(id, ct);
        return result.VersionChanged
            ? FormResults.Conflict(this)
            : OperationRefusals.Redisplay(this, result.Verdict, AdminClosedMessage);
    }

    private async Task<GatedOperationResult> RunAsync(Guid id, int? shownVersion, IOperationRule<SampleItemState> rule, int quantity, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        IOrderingModule ordering = services.GetRequiredService<IOrderingModule>();
        SampleOrderOperations operations = services.GetRequiredService<SampleOrderOperations>();
        return await services.GetRequiredService<OperationRunner>().RunAsync(
            stepCt => ordering.LockOrderAsync(id, shownVersion, stepCt),
            async stepCt => (await operations.ReadAsync(id, stepCt)).State,
            rule,
            quantity,
            (state, availability, stepCt) => operations.CancelAsync(id, quantity, stepCt),
            ct);
    }

    private IActionResult AnswerCustomer(Guid id, GatedOperationResult result)
    {
        string orderPage = "/panel/islem-ornegi/" + id;
        return result.IsRejected
            ? OperationRefusals.ToOrderPage(this, result.Verdict, orderPage)
            : FormResults.SeeOther(orderPage);
    }

    private async Task LoadAsync(Guid id, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        SampleRules rules = services.GetRequiredService<SampleRules>();
        SampleOrderView view = await services.GetRequiredService<SampleOrderOperations>().ReadAsync(id, ct);
        RecordId = id;
        ItemId = view.ItemId;
        Version = view.Version;
        Cancel = rules.Cancel.Evaluate(view.State);
        Withdraw = rules.Withdraw.Evaluate(view.State);
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
