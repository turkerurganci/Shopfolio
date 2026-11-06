// Deliberate violation sample of the operation source scan (07 8.1.2). The scan reads this file as text; it is not compiled.
// Line numbers are part of the test. A comment that says [GatedOperation] or OperationGate.Decide is not a finding and reaches nothing.
internal sealed class GatedMisuse : PageModel
{
    [GatedOperation]
    public async Task<IActionResult> OnPostForgotAsync(Guid id)
    {
        await Task.Delay(1);
        // OperationGate.Decide(rule, context, 1) in a comment reaches nothing
        string text = "GatedOrderOperation OperationGate.Decide";
        return Page();
    }

    [HttpPost, GatedOperationAttribute]
    public IActionResult OnPostSplit(Guid id)
    {
        return Helper(id);
    }

    private IActionResult Helper(Guid id) => Page();

    [Operations.GatedOperation()]
    public IActionResult OnGet(Guid id)
    {
        GatedOrderOperation run = null;
        return Page();
    }

    [GatedOperation]
    [HttpPost]
    public async Task<IActionResult> OnPostFine(Guid id)
    {
        return await RunGated(id);
    }

    private async Task<IActionResult> RunGated(Guid id)
    {
        OperationVerdict verdict = OperationGate.Decide(rule, state, 1);
        return Page();
    }

    [GatedOperation]
    public IActionResult OnPostDirect(Guid id) => OperationGate.Decide(rule, state, null).IsAllowed ? Page() : Page();

    [GatedOperation(Reason = "x")]
    public IActionResult OnPostAfterRunner(Guid id)
    {
        GatedOrderOperation operation = services.GetRequiredService<GatedOrderOperation>();
        return Page();
    }
}
