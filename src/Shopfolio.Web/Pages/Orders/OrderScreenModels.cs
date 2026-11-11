using System.ComponentModel.DataAnnotations;
using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Orders;

/// <summary>
/// E-17 — the cancellation and late delivery termination screen (07 UC-38…UC-41; 04 5.17; T512). The screen hangs under the root it was reached from (07 2.1.3): the member's root by the
/// account's ownership, the access-key root by the key (<see cref="OrderRoots"/>); a root that does not hold is the one 404. Everything the screen shows is the server's: the operation, the
/// items with their open quantity, default and upper limit (K-1170 (2)), the refund's way and period and whether the shipping fee comes back (<see cref="RefundPreview"/>; K-789) - the page
/// works out no amount. An operation that is not open, on the GET or on the POST, is 303 to the root with the reason (<see cref="OperationRefusals.ToOrderPage"/>; K-1170 (1), (4)). The first
/// POST without the confirmation mark is the script-free last step - 200, nothing written (07 2.2.4); the confirmed POST runs the module's write in the page's transaction
/// (<see cref="OperationRunner"/>) and answers 303 to the root, where the result stays (04 5.17.9).
/// </summary>
public abstract class CustomerOperationPageModel : PageModel
{
    private readonly ICustomerOperations _operations;
    private readonly IOrderingModule _ordering;
    private bool _loaded;

    protected CustomerOperationPageModel(ICustomerOperations operations, IOrderingModule ordering)
    {
        _operations = operations;
        _ordering = ordering;
    }

    /// <summary>The screen's address under <c>/siparis</c> (the last segment of the path).</summary>
    protected abstract CustomerScreen ScreenKind { get; }

    /// <summary>Whether the root is the member's (true) or the access key's.</summary>
    protected abstract bool IsMemberRoot { get; }

    public CustomerOperationScreen Screen { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    /// <summary>The root the screen was reached from (07 2.1.3): the path without its last segment.</summary>
    public string RootPath
    {
        get
        {
            string path = Request.Path.Value ?? string.Empty;
            int last = path.LastIndexOf('/');
            return last > 0 ? path[..last] : path;
        }
    }

    /// <summary>The path the forms post to.</summary>
    public string ScreenPath => Request.Path.Value ?? string.Empty;

    /// <summary>The last step of the script-free path (07 2.2.4); null on the first view.</summary>
    public ConfirmView? Confirm { get; protected set; }

    /// <summary>The IBAN field of a paid transfer order (04 5.17.5, 7.1.8): empty when it arrives; null on the card line and on the order's cancellation.</summary>
    public IbanFieldView? IbanField { get; private set; }

    /// <summary>The values of the form as sent, keyed by the field's name, so the 422 page and the last step show them again.</summary>
    public IDictionary<string, string> Sent { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>The refund's way in the sentence (K-785).</summary>
    public string RefundWayText => Screen.PaymentMethod == PaymentMethod.Card ? "kartınıza" : "IBAN'ınıza";

    /// <summary>The way and period line of 04 5.17.5.</summary>
    public string RefundLineText =>
        $"Geri ödeme {DisplayFormat.Number(Screen.RefundPeriodDays)} gün içinde {RefundWayText} yapılır.";

    public static string ItemNameOf(CustomerScreenItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        string[] options = [.. new[] { item.FirstOptionValue, item.SecondOptionValue }.OfType<string>()];
        return options.Length == 0 ? item.ProductName : item.ProductName + " (" + string.Join(", ", options) + ")";
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        IActionResult? refusal = await LoadAsync(ct);
        return refusal ?? Page();
    }

    /// <summary>
    /// The validation filter answers a form whose fields break their rules itself, 422 without the handler (09 3.4 item 3): the page it shows again still needs the root's check and the
    /// screen; a root that does not hold is 404 and an operation no longer open is 303 with the reason, as on the handler's own path.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            KeepSent();
            if (await LoadAsync(HttpContext.RequestAborted) is IActionResult refusal)
            {
                executed.Result = refusal;
            }
        }
    }

    /// <summary>The root's order, the screen as it opens now and the IBAN field; a refusal (404 or 303 with the reason) when one of them does not hold.</summary>
    protected async Task<IActionResult?> LoadAsync(CancellationToken ct)
    {
        _loaded = true;
        OrderRootResult root = await ResolveAsync(ct);
        if (root.OrderId is not Guid orderId)
        {
            return root.Denial ?? AccessDenial.NotFound();
        }

        CustomerOperationScreen? screen = await _operations.ReadScreenAsync(orderId, ScreenKind, ct);
        if (screen is null)
        {
            return OperationRefusals.ToOrderPage(this, new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed), RootPath);
        }

        Screen = screen;
        IbanField = screen.AsksIban
            ? IbanFields.Create(IbanFieldPlace.Declaration, screen.PaymentMethod, IbanOpening.Standard, IbanFieldName, Sent.TryGetValue(IbanFieldName, out string? iban) ? iban : null)
            : null;
        if (IbanField is not null)
        {
            IbanField = IbanField with { Field = Fields.Bind(IbanField.Field, FormErrorSummary.From(ModelState)) };
        }

        return null;
    }

    /// <summary>The form's name of the IBAN field.</summary>
    public const string IbanFieldName = "Iban";

    /// <summary>The order's id of the root, or the refusal.</summary>
    protected async Task<(Guid? OrderId, IActionResult? Refusal)> FindOrderAsync(CancellationToken ct)
    {
        OrderRootResult root = await ResolveAsync(ct);
        return root.OrderId is Guid id ? (id, null) : (null, root.Denial ?? AccessDenial.NotFound());
    }

    /// <summary>
    /// The confirmed write in the page's transaction (07 8.1.2): applied - 303 to the root; refused - nothing written, 303 to the root with the reason, or 422 with the IBAN field's message.
    /// </summary>
    protected async Task<IActionResult> RunAsync(Func<CancellationToken, Task<CustomerOperationResult>> write, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(write);
        CustomerOperationResult? outcome = null;
        OperationRunner runner = HttpContext.RequestServices.GetRequiredService<OperationRunner>();
        GatedOperationResult result = await runner.RunAsync(
            async stepCt =>
            {
                outcome = await write(stepCt);
                return outcome.Gate;
            },
            ct);
        if (!result.IsRejected)
        {
            return FormResults.SeeOther(RootPath);
        }

        switch (outcome?.Outcome)
        {
            case CustomerOperationOutcome.NotFound:
                return AccessDenial.NotFound();
            case CustomerOperationOutcome.InvalidIban:
                KeepSent();
                ModelState.AddFieldError(IbanFieldName, string.IsNullOrWhiteSpace(Sent.TryGetValue(IbanFieldName, out string? sentIban) ? sentIban : null) ? FieldCodes.FieldRequired : FieldCodes.InvalidIban);
                return await LoadAsync(ct) ?? FormResults.Redisplay(this);
            default:
                return OperationRefusals.ToOrderPage(this, result.Verdict, RootPath);
        }
    }

    /// <summary>The script-free last step (07 2.2.4; 04 5.17.6): the page again with the confirmation in place, the form's values carried forward with the mark.</summary>
    protected IActionResult LastStep(string operationName, string sentence, string handler)
    {
        KeepSent();
        List<KeyValuePair<string, string>> fields = [.. Sent.Where(f => f.Key != nameof(CancelOrderInput.Confirmed))];
        fields.Add(new(nameof(CancelOrderInput.Confirmed), "true"));
        Confirm = Confirmations.ForCustomer(operationName, sentence, operationName, ScreenPath, handler, RootPath, fields);
        return Page();
    }

    private void KeepSent()
    {
        if (!Request.HasFormContentType)
        {
            return;
        }

        foreach (KeyValuePair<string, Microsoft.Extensions.Primitives.StringValues> field in Request.Form)
        {
            if (field.Key != "__RequestVerificationToken")
            {
                Sent[field.Key] = field.Value.ToString();
            }
        }
    }

    private Task<OrderRootResult> ResolveAsync(CancellationToken ct) =>
        IsMemberRoot ? OrderRoots.ResolveMemberAsync(this, _ordering, ct) : OrderRoots.ResolveAccessAsync(this, _ordering, ct);
}

/// <summary>
/// <c>{kök}/iptal</c> (07 UC-38…UC-40; K-1162 (3)): the order's cancellation on Alındı + Bekliyor, the item cancellation on a paid order. <c>CancelOrder</c> carries only the mark;
/// <c>CancelOrderItems</c> the chosen items with their units and shown open quantity, and the IBAN on the transfer line (K-1202).
/// </summary>
public abstract class CancellationPageModel(ICustomerOperations operations, IOrderingModule ordering) : CustomerOperationPageModel(operations, ordering)
{
    private readonly ICustomerOperations _operations = operations;

    /// <summary>The 422 message when no item is chosen (04 5.17.12).</summary>
    public const string NoItemText = "İptal etmek istediğiniz en az bir ürünü seçin.";

    protected override CustomerScreen ScreenKind => CustomerScreen.Cancellation;

    /// <summary>UC-39 (07 4.17.2).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCancelOrderAsync(CancelOrderInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        (Guid? found, IActionResult? refusal) = await FindOrderAsync(ct);
        if (found is not Guid orderId)
        {
            return refusal ?? AccessDenial.NotFound();
        }

        if (!input.Confirmed)
        {
            if (await LoadAsync(ct) is IActionResult closed)
            {
                return closed;
            }

            return Screen.Operation == CustomerScreenOperation.CancelOrder
                ? LastStep("Siparişi iptal et", "Siparişiniz iptal edilecek.", "CancelOrder")
                : OperationRefusals.ToOrderPage(this, new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed), RootPath);
        }

        return await RunAsync(stepCt => _operations.CancelOrderAsync(orderId, stepCt), ct);
    }

    /// <summary>UC-40 (07 4.17.3): 422 for no item chosen or a quantity under one; the module checks the whole request against the order as it is now.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCancelOrderItemsAsync(CancelOrderItemsInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        (Guid? found, IActionResult? refusal) = await FindOrderAsync(ct);
        if (found is not Guid orderId)
        {
            return refusal ?? AccessDenial.NotFound();
        }

        // One row per item (07 5.1.5; K-1196): two rows of the same item are an invalid selection - 422 under the list, nothing added up, no row processed.
        if (!ItemRows.RejectRepeated(ModelState, nameof(CancelOrderItemsInput.Items), input.Items.Where(l => l.OrderItemId != Guid.Empty).Select(l => l.OrderItemId)))
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.InvalidListValue);
            return await LoadAsync(ct) ?? FormResults.Redisplay(this);
        }

        List<CancelOrderItemsLine> lines = [];
        for (int index = 0; index < input.Items.Count; index++)
        {
            CancelOrderItemsLineInput line = input.Items[index];
            if (line.OrderItemId == Guid.Empty)
            {
                continue;
            }

            // The quantity's field rule (at least one, a whole number; 09 3.4; D-155) is the page's: a wrong value is 422 under the field, not a closed operation.
            if (!int.TryParse(line.Quantity, NumberStyles.None, CultureInfo.InvariantCulture, out int quantity) || quantity < 1)
            {
                ModelState.AddFieldError($"{nameof(CancelOrderItemsInput.Items)}[{index}].{nameof(CancelOrderItemsLineInput.Quantity)}", FieldCodes.InvalidQuantity, DisplayFormat.Number(Math.Max(1, line.OriginalOpenQuantity)));
                continue;
            }

            lines.Add(new CancelOrderItemsLine(line.OrderItemId, quantity, line.OriginalOpenQuantity));
        }

        if (ModelState.IsValid && lines.Count == 0)
        {
            ModelState.AddModelError(string.Empty, NoItemText);
        }

        if (!ModelState.IsValid)
        {
            HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.InvalidQuantity);
            return await LoadAsync(ct) ?? FormResults.Redisplay(this);
        }

        if (!input.Confirmed)
        {
            if (await LoadAsync(ct) is IActionResult closed)
            {
                return closed;
            }

            if (Screen.Operation != CustomerScreenOperation.CancelOrderItems)
            {
                return OperationRefusals.ToOrderPage(this, new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed), RootPath);
            }

            if (lines.Any(l => !Screen.Items.Any(i => i.OrderItemId == l.OrderItemId)))
            {
                return AccessDenial.NotFound();
            }

            if (Screen.AsksIban && string.IsNullOrWhiteSpace(input.Iban))
            {
                ModelState.AddFieldError(IbanFieldName, FieldCodes.FieldRequired);
                return await LoadAsync(ct) ?? FormResults.Redisplay(this);
            }

            Dictionary<Guid, int> selected = lines.ToDictionary(l => l.OrderItemId, l => l.Quantity);
            bool fee = RefundPreview.CancellationRefundsShippingFee(Screen.OpenPhysicalUnits, selected);
            string amount = fee ? "bu ürünler için ödediğiniz tutar ve kargo ücreti" : "bu ürünler için ödediğiniz tutar";
            string sentence = $"Seçtiğiniz {DisplayFormat.Number(lines.Sum(l => l.Quantity))} ürün iptal edilecek; {amount} {DisplayFormat.Number(Screen.RefundPeriodDays)} gün içinde {RefundWayText} geri ödenecek.";
            return LastStep("Seçilen ürünleri iptal et", sentence, "CancelOrderItems");
        }

        return await RunAsync(stepCt => _operations.CancelOrderItemsAsync(orderId, lines, input.Iban, stepCt), ct);
    }
}

/// <summary>
/// <c>{kök}/gecikme-feshi</c> (07 UC-38, UC-41; K-796): the termination covers every open unit of the undelivered physical items; an item or a quantity sent with the form is ignored.
/// </summary>
public abstract class LateDeliveryTerminationPageModel(ICustomerOperations operations, IOrderingModule ordering) : CustomerOperationPageModel(operations, ordering)
{
    private readonly ICustomerOperations _operations = operations;

    protected override CustomerScreen ScreenKind => CustomerScreen.LateDeliveryTermination;

    /// <summary>UC-41 (07 4.17.4).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostSubmitLateDeliveryTerminationAsync(SubmitLateDeliveryTerminationInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        (Guid? found, IActionResult? refusal) = await FindOrderAsync(ct);
        if (found is not Guid orderId)
        {
            return refusal ?? AccessDenial.NotFound();
        }

        if (!input.Confirmed)
        {
            if (await LoadAsync(ct) is IActionResult closed)
            {
                return closed;
            }

            if (Screen.AsksIban && string.IsNullOrWhiteSpace(input.Iban))
            {
                ModelState.AddFieldError(IbanFieldName, FieldCodes.FieldRequired);
                return await LoadAsync(ct) ?? FormResults.Redisplay(this);
            }

            int units = Screen.Items.Sum(i => i.OpenQuantity);
            string sentence = $"Teslim edilmeyen {DisplayFormat.Number(units)} ürün için sözleşme feshedilecek; bu ürünlerin bedeli ve kargo ücreti {DisplayFormat.Number(Screen.RefundPeriodDays)} gün içinde {RefundWayText} geri ödenecek.";
            return LastStep("Gecikme nedeniyle feshet", sentence, "SubmitLateDeliveryTermination");
        }

        return await RunAsync(stepCt => _operations.TerminateForLateDeliveryAsync(orderId, input.Iban, stepCt), ct);
    }
}

/// <summary>UC-38…UC-40 under the member's root (07 2.1.3).</summary>
[NoStore]
public sealed class MemberCancellationModel(ICustomerOperations operations, IOrderingModule ordering) : CancellationPageModel(operations, ordering)
{
    protected override bool IsMemberRoot => true;
}

/// <summary>UC-38…UC-40 under the access-key root (07 2.1.3).</summary>
[KeyedLinkPage]
[NoStore]
public sealed class AccessCancellationModel(ICustomerOperations operations, IOrderingModule ordering) : CancellationPageModel(operations, ordering)
{
    protected override bool IsMemberRoot => false;
}

/// <summary>UC-38, UC-41 under the member's root (07 2.1.3).</summary>
[NoStore]
public sealed class MemberLateDeliveryTerminationModel(ICustomerOperations operations, IOrderingModule ordering) : LateDeliveryTerminationPageModel(operations, ordering)
{
    protected override bool IsMemberRoot => true;
}

/// <summary>UC-38, UC-41 under the access-key root (07 2.1.3).</summary>
[KeyedLinkPage]
[NoStore]
public sealed class AccessLateDeliveryTerminationModel(ICustomerOperations operations, IOrderingModule ordering) : LateDeliveryTerminationPageModel(operations, ordering)
{
    protected override bool IsMemberRoot => false;
}

/// <summary>UC-39's form (07 5): the confirmation mark only (07 2.2.4).</summary>
public sealed class CancelOrderInput : IFormInput
{
    public bool Confirmed { get; set; }
}

/// <summary>UC-40's form (07 5): the chosen items - a line whose item is empty was not chosen - the transfer line's IBAN and the confirmation mark.</summary>
public sealed class CancelOrderItemsInput : IFormInput
{
    public IList<CancelOrderItemsLineInput> Items { get; } = [];

    [Display(Name = IbanFields.Label)]
    [InvalidIban]
    public string? Iban { get; set; }

    public bool Confirmed { get; set; }
}

/// <summary>One line of UC-40's form: the item, the units as typed (the page's field rule reads them) and the open quantity the screen showed (K-1202).</summary>
public sealed class CancelOrderItemsLineInput
{
    public Guid OrderItemId { get; set; }

    public string? Quantity { get; set; }

    public int OriginalOpenQuantity { get; set; }
}

/// <summary>UC-41's form (07 5): the transfer line's IBAN and the confirmation mark; no item and no quantity (K-1170 (3)).</summary>
public sealed class SubmitLateDeliveryTerminationInput : IFormInput
{
    [Display(Name = IbanFields.Label)]
    [InvalidIban]
    public string? Iban { get; set; }

    public bool Confirmed { get; set; }
}
