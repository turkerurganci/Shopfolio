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
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Orders;

/// <summary>
/// The common half of the customer's two after-sales screens E-18 and E-19 (07 4.18, 4.19; T513): the screen hangs under the root it was reached from (07 2.1.3) and finds the order the same
/// way as the order page (<see cref="OrderRoots"/>), so a screen is never open where the page is not - a root that does not hold is the one 404, no session on the member's root is 302 to the
/// sign-in. An operation that is not open, on the GET or on the POST, is 303 to the root with the reason (<see cref="OperationRefusals.ToOrderPage"/>; K-1170 (1), (4)). The confirmed write
/// runs in the page's transaction (<see cref="OperationRunner"/>; 09 9.1 item 3): any refusal of the module rolls the whole request back.
/// </summary>
public abstract class AfterSalesScreenModel : PageModel
{
    private readonly IOrderingModule _ordering;
    private bool _loaded;

    protected AfterSalesScreenModel(IOrderingModule ordering)
    {
        _ordering = ordering;
    }

    /// <summary>The screen's segment under the root (<c>cayma</c>, <c>ayip-talebi</c>).</summary>
    protected abstract string Screen { get; }

    /// <summary>Whether the root is the member's (true) or the access key's.</summary>
    protected abstract bool IsMemberRoot { get; }

    /// <summary>The root the screen was reached from (07 2.1.3): the answers go back to it.</summary>
    public string RootPath => OrderRoots.RootOf(ScreenPath, Screen);

    /// <summary>The path the forms post to.</summary>
    public string ScreenPath => Request.Path.Value ?? string.Empty;

    /// <summary>The values of the form as sent, keyed by the field's name, so the 422 page and the last step show them again.</summary>
    public IDictionary<string, string> Sent { get; } = new Dictionary<string, string>(StringComparer.Ordinal);

    /// <summary>The value a field was sent with, or null.</summary>
    public string? SentValue(string name) => Sent.TryGetValue(name, out string? value) ? value : null;

    /// <summary>
    /// The validation filter answers a form whose fields break their rules itself, 422 without the handler (09 3.4 item 3): the page it shows again still needs the root and the screen; a root
    /// that does not hold is 404 and an operation no longer open is 303 with the reason, as on the handler's own path.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (!_loaded && executed.Exception is null && executed.Result is PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            if (await ReloadAsync(HttpContext.RequestAborted) is IActionResult refusal)
            {
                executed.Result = refusal;
            }
        }
    }

    /// <summary>The screen of the order as it opens now; a refusal (404, or 303 with the reason) when the root does not hold or the operation is not open.</summary>
    protected async Task<IActionResult?> ReloadAsync(CancellationToken ct)
    {
        _loaded = true;
        KeepSent();
        (Guid? orderId, IActionResult? denial) = await FindOrderAsync(ct);
        return orderId is Guid id ? await LoadScreenAsync(id, ct) : denial;
    }

    /// <summary>Reads the screen of the order; returns the refusal when the operation is not open (or the screen's own 404).</summary>
    protected abstract Task<IActionResult?> LoadScreenAsync(Guid orderId, CancellationToken ct);

    /// <summary>The order's id of the root, or the root's denial.</summary>
    protected async Task<(Guid? OrderId, IActionResult? Denial)> FindOrderAsync(CancellationToken ct)
    {
        OrderRootResult root = IsMemberRoot
            ? await OrderRoots.ResolveMemberAsync(this, _ordering, ct)
            : await OrderRoots.ResolveAccessAsync(this, _ordering, ct);
        return root.OrderId is Guid id ? (id, null) : (null, root.Denial ?? AccessDenial.NotFound());
    }

    /// <summary>303 to the root with the "closed" reason (07 6.3.3 <c>OperationClosed</c>); the line names <c>OperationNotAvailable</c>.</summary>
    protected IActionResult Closed() => OperationRefusals.ToOrderPage(this, new OperationVerdict(OperationDecision.Closed, OperationAvailability.Closed), RootPath);

    /// <summary>
    /// The module's writes in the page's one transaction (07 8.1.2; 09 9.1 item 3): applied - 303 to the root, where the result stays; refused - nothing written, 303 to the root with the
    /// reason: a quantity above the open one, or open units that changed since the screen (K-1202), is <c>QuantityExceedsOpen</c>, every other refusal <c>OperationClosed</c>.
    /// </summary>
    protected async Task<IActionResult> RunAsync(Func<CancellationToken, Task<OperationDecision>> write, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(write);
        OperationRunner runner = HttpContext.RequestServices.GetRequiredService<OperationRunner>();
        GatedOperationResult result = await runner.RunAsync(
            async stepCt => GatedOperationResult.Decided(new OperationVerdict(await write(stepCt), OperationAvailability.Closed)),
            ct);
        return result.IsRejected ? OperationRefusals.ToOrderPage(this, result.Verdict, RootPath) : FormResults.SeeOther(RootPath);
    }

    /// <summary>The decision a refusal of the transition unit stands for on the customer's screen: a quantity the module refused is <c>QuantityExceedsOpen</c>, the rest is closed.</summary>
    protected static OperationDecision DecisionOf(TransitionRefusal? refusal) => refusal switch
    {
        null => OperationDecision.Allowed,
        TransitionRefusal.InvalidInput => OperationDecision.QuantityExceedsOpen,
        _ => OperationDecision.Closed,
    };

    /// <summary>A quantity as typed: a whole number of at least one, else null (04 7.3.2.10).</summary>
    protected static int? ParseQuantity(string? text) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out int value) && value >= 1 ? value : null;

    /// <summary>The province's name of an address, or an empty text when the list does not carry it.</summary>
    protected static string ProvinceNameOf(IReadOnlyList<ProvinceInfo> provinces, Guid provinceId) =>
        provinces.FirstOrDefault(p => p.Id == provinceId)?.Name ?? string.Empty;

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
}

/// <summary>The firm's return address as the screen writes it (04 5.18.5, 5.19.3 (3)): the recipient, the lines and the province's name.</summary>
public sealed record ReturnAddressView(string RecipientName, string AddressLine, string District, string ProvinceName, string? Phone);

/// <summary>
/// One item of the withdrawal screen (04 5.18.4): the frozen name, the type, the units open to the withdrawal - the default and the upper limit of the quantity field (K-1170 (2); K-795) -,
/// the window's last day and the conditional exemption's line (K-509).
/// </summary>
public sealed record WithdrawalRowView(Guid OrderItemId, string Name, ProductType ProductType, int OpenQuantity, DateOnly? LastDay, bool PackagingCondition, bool RefundFromGoodsArrival)
{
    /// <summary>A quantity field only where more than one unit is open (04 2.12.1.8).</summary>
    public bool HasQuantityField => OpenQuantity > 1;
}

/// <summary>
/// E-18 — the withdrawal screen <c>{kök}/cayma</c> (07 UC-42, UC-43; 04 5.18; T513). The GET lists the items the withdrawal is open for now, each with its open units, its window's last day and
/// the conditional exemption's line; when a physical item is on the list the firm's current return address, the sending period with its last day and the way the goods go back (K-665, K-838);
/// the refund's way and the moment its period starts; the IBAN field on the transfer line when no IBAN is on file. Every value is the module's
/// (<see cref="IOrderOperations.ReadWithdrawalScreenAsync"/>) - the page works out no period and no amount. No item open: 303 to the root with the reason.
/// <c>SubmitWithdrawal</c>: one row per item (07 5.1.5); no item chosen, a quantity that is not a whole number of at least one, an empty IBAN where one is asked - 422; an item not of the order
/// - 404; the first POST without the mark is the last step (200, nothing written; 07 2.2.4); the confirmed POST re-reads each item's open units inside the transaction and refuses the whole
/// request when they differ from the ones shown (K-1202) before the module writes the declarations - each with its date stamp, units and the return address of the moment (06 4.25).
/// </summary>
public abstract class WithdrawalPageModel : AfterSalesScreenModel
{
    /// <summary>The 422 message when no item is chosen (04 5.18.12; 7.1.9.1).</summary>
    public const string NoItemText = "Cayacağınız en az bir ürünü seçin.";

    /// <summary>The handler of the form (07 UC-43).</summary>
    public const string Handler = "SubmitWithdrawal";

    private readonly IOrderPage _page;
    private readonly IOrderOperations _operations;
    private readonly IOrderTransitions _transitions;
    private readonly ISettingsModule _settings;

    protected WithdrawalPageModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IOrderTransitions transitions, ISettingsModule settings)
        : base(ordering)
    {
        _page = page;
        _operations = operations;
        _transitions = transitions;
        _settings = settings;
    }

    protected override string Screen => "cayma";

    public IReadOnlyList<WithdrawalRowView> Rows { get; private set; } = [];

    /// <summary>The firm's current return address; null when no physical item is on the list (04 5.18.5) or the firm has not entered one.</summary>
    public ReturnAddressView? ReturnAddress { get; private set; }

    /// <summary>The sending period line (Z-42 with its last day); null without a physical item.</summary>
    public string? SendingPeriodText { get; private set; }

    /// <summary>A shipped physical item not marked delivered is on the list (K-838).</summary>
    public bool CarrierReturns { get; private set; }

    /// <summary>A physical item is on the list: the return information is shown (04 5.18.5).</summary>
    public bool HasPhysicalItem => Rows.Any(r => r.ProductType == ProductType.Physical);

    /// <summary>A service item is on the list: the partly performed service's line (04 5.18.6; K-578).</summary>
    public bool HasServiceItem => Rows.Any(r => r.ProductType == ProductType.Service);

    /// <summary>The refund lines of 04 5.18.6, one per moment the period starts from among the items listed.</summary>
    public IReadOnlyList<string> RefundTexts { get; private set; } = [];

    /// <summary>The IBAN field of the transfer line when the order has no IBAN on file (04 5.18.6, 2.12.3); null otherwise.</summary>
    public IbanFieldView? IbanField { get; private set; }

    /// <summary>The last step of the script-free path (07 2.2.4); null on the first view.</summary>
    public ConfirmView? Confirm { get; private set; }

    /// <summary>The chosen items with their units, as the last step lists them (04 5.18.7).</summary>
    public IReadOnlyList<(string Name, int Quantity)> ChosenItems { get; private set; } = [];

    private PaymentMethod _paymentMethod;
    private int _refundPeriodDays;
    private bool _asksIban;

    public async Task<IActionResult> OnGetAsync(CancellationToken ct) => await ReloadAsync(ct) ?? Page();

    /// <summary>UC-43 (07 4.18.2).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostSubmitWithdrawalAsync(SubmitWithdrawalInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        (Guid? found, IActionResult? denial) = await FindOrderAsync(ct);
        if (found is not Guid orderId)
        {
            return denial ?? AccessDenial.NotFound();
        }

        // An item that is not the order's is the one 404 (07 2.3.3), before any other answer.
        OrderPageDetail? detail = await _page.ReadAsync(orderId, ct);
        List<WithdrawalItemRow> chosen = [.. input.Items.Where(r => r.OrderItemId is not null)];
        if (detail is null || chosen.Any(r => !detail.Items.Any(i => i.OrderItemId == r.OrderItemId)))
        {
            return AccessDenial.NotFound();
        }

        // One row per item (07 5.1.5): two rows of the same item are an invalid selection - 422 under the list, nothing added up, no row processed.
        if (!ItemRows.RejectRepeated(ModelState, nameof(SubmitWithdrawalInput.Items), chosen.Select(r => r.OrderItemId!.Value)))
        {
            return await ReloadAsync(ct) ?? FormResults.Redisplay(this);
        }

        List<(Guid ItemId, int Quantity, int Shown)> lines = [];
        for (int index = 0; index < input.Items.Count; index++)
        {
            WithdrawalItemRow row = input.Items[index];
            if (row.OrderItemId is not Guid itemId)
            {
                continue;
            }

            // The quantity's field rule (a whole number of at least one; 04 7.3.2.10) is the page's: a wrong value is 422 under the field; above the open units is the module's 303.
            if (ParseQuantity(row.Quantity) is not int quantity)
            {
                ModelState.AddFieldError($"{nameof(SubmitWithdrawalInput.Items)}[{index}].{nameof(WithdrawalItemRow.Quantity)}", FieldCodes.InvalidQuantity, DisplayFormat.Number(Math.Max(1, row.OriginalOpenQuantity)));
                continue;
            }

            lines.Add((itemId, quantity, row.OriginalOpenQuantity));
        }

        if (ModelState.IsValid && lines.Count == 0)
        {
            ModelState.AddFieldError(nameof(SubmitWithdrawalInput.Items), FieldCodes.FieldRequired);
        }

        if (!ModelState.IsValid)
        {
            return await ReloadAsync(ct) ?? FormResults.Redisplay(this);
        }

        if (await ReloadAsync(ct) is IActionResult closed)
        {
            return closed;
        }

        // A chosen item the withdrawal is not open for now (the window or the right closed meanwhile) is the closed answer, the request is not applied (04 5.18.11).
        if (lines.Any(l => !Rows.Any(r => r.OrderItemId == l.ItemId)))
        {
            return Closed();
        }

        // The transfer line's IBAN (04 5.18.6, 5.17.12): required when none is on file; its format is the field's rule.
        if (_asksIban && string.IsNullOrWhiteSpace(input.Iban))
        {
            ModelState.AddFieldError(nameof(SubmitWithdrawalInput.Iban), FieldCodes.FieldRequired);
            await ReloadAsync(ct);
            return FormResults.Redisplay(this);
        }

        if (!input.Confirmed)
        {
            ChosenItems = [.. lines.Select(l => (Rows.First(r => r.OrderItemId == l.ItemId).Name, l.Quantity))];
            bool fromArrival = lines.Any(l => Rows.First(r => r.OrderItemId == l.ItemId).RefundFromGoodsArrival);
            string sentence = OrderScreenTexts.WithdrawalSentence(lines.Sum(l => l.Quantity), _paymentMethod, fromArrival, _refundPeriodDays);
            List<KeyValuePair<string, string>> fields = [.. Sent.Where(f => f.Key != nameof(SubmitWithdrawalInput.Confirmed))];
            fields.Add(new(nameof(SubmitWithdrawalInput.Confirmed), "true"));
            Confirm = Confirmations.ForCustomer(OrderScreenTexts.WithdrawalName, sentence, OrderScreenTexts.WithdrawalConfirmLabel, ScreenPath, Handler, RootPath, fields);
            return Page();
        }

        string? iban = string.IsNullOrWhiteSpace(input.Iban) ? null : input.Iban;
        return await RunAsync(
            async stepCt =>
            {
                foreach ((Guid itemId, int quantity, int shown) in lines)
                {
                    // K-1202: the open units are read again in the write's transaction; units that differ from the ones shown refuse the whole request.
                    WithdrawalScreen? now = await _operations.ReadWithdrawalScreenAsync(orderId, itemId, stepCt);
                    if (now is null)
                    {
                        return OperationDecision.Closed;
                    }

                    if (now.OpenQuantity != shown)
                    {
                        return OperationDecision.QuantityExceedsOpen;
                    }

                    WithdrawalResult result = await _transitions.DeclareWithdrawalAsync(new DeclareWithdrawalRequest(orderId, itemId, quantity, true, iban), stepCt);
                    if (result.IsRejected)
                    {
                        return DecisionOf(result.Refusal);
                    }
                }

                return OperationDecision.Allowed;
            },
            ct);
    }

    protected override async Task<IActionResult?> LoadScreenAsync(Guid orderId, CancellationToken ct)
    {
        OrderOperationsView? operations = await _operations.ReadAsync(orderId, ActorKind.Buyer, ct);
        OrderPageDetail? detail = await _page.ReadAsync(orderId, ct);
        if (operations is null || detail is null)
        {
            return AccessDenial.NotFound();
        }

        List<WithdrawalScreen> screens = [];
        foreach (Guid itemId in operations.Open
            .Where(e => e.OrderItemId is not null && e.Operation is OrderOperation.WithdrawPhysicalItem or OrderOperation.WithdrawServiceItem or OrderOperation.WithdrawDigitalItem)
            .Select(e => e.OrderItemId!.Value)
            .Distinct())
        {
            if (await _operations.ReadWithdrawalScreenAsync(orderId, itemId, ct) is WithdrawalScreen screen)
            {
                screens.Add(screen);
            }
        }

        if (screens.Count == 0)
        {
            return Closed();
        }

        Rows =
        [
            .. screens.Select(s => new WithdrawalRowView(
                s.OrderItemId,
                detail.Items.First(i => i.OrderItemId == s.OrderItemId).Name,
                s.ProductType,
                s.OpenQuantity,
                s.LastDay,
                s.PackagingCondition is not null,
                s.RefundFromGoodsArrival)),
        ];
        WithdrawalScreen first = screens[0];
        _paymentMethod = first.PaymentMethod;
        _refundPeriodDays = first.RefundPeriodDays;
        _asksIban = screens.Any(s => s.AsksIban);
        RefundTexts = [.. screens.Select(s => s.RefundFromGoodsArrival).Distinct().Order().Select(arrival => OrderScreenTexts.Refund(first.PaymentMethod, arrival, first.RefundPeriodDays))];
        if (screens.FirstOrDefault(s => s.ProductType == ProductType.Physical) is WithdrawalScreen physical)
        {
            CarrierReturns = screens.Any(s => s.CarrierReturnsUnreceivedGoods);
            if (physical.SendingPeriodDays is int days && physical.SendingLastDay is DateOnly lastDay)
            {
                SendingPeriodText = OrderScreenTexts.SendingPeriod(days, lastDay);
            }

            if (physical.ReturnAddress is { } address)
            {
                IReadOnlyList<ProvinceInfo> provinces = await _settings.GetProvincesAsync(ct);
                ReturnAddress = new ReturnAddressView(address.RecipientName, address.AddressLine, address.District, ProvinceNameOf(provinces, address.ProvinceId), address.Phone);
            }
        }

        IbanField = _asksIban
            ? IbanFields.Create(IbanFieldPlace.Declaration, first.PaymentMethod, IbanOpening.Standard, nameof(SubmitWithdrawalInput.Iban), SentValue(nameof(SubmitWithdrawalInput.Iban)))
            : null;
        if (IbanField is not null)
        {
            IbanField = IbanField with { Field = Fields.Bind(IbanField.Field, FormErrorSummary.From(ModelState)) };
        }

        return null;
    }
}

/// <summary>UC-42/UC-43 under the member's root (07 4.18).</summary>
[NoStore]
public sealed class MemberWithdrawalModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IOrderTransitions transitions, ISettingsModule settings)
    : WithdrawalPageModel(ordering, page, operations, transitions, settings)
{
    protected override bool IsMemberRoot => true;
}

/// <summary>UC-42/UC-43 under the access-key root (07 4.18): the key stays out of the referrer and the cache.</summary>
[KeyedLinkPage]
[NoStore]
public sealed class AccessWithdrawalModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IOrderTransitions transitions, ISettingsModule settings)
    : WithdrawalPageModel(ordering, page, operations, transitions, settings)
{
    protected override bool IsMemberRoot => false;
}

/// <summary>UC-43's form (07 5): the chosen items - a row whose item is empty was not chosen -, the transfer line's IBAN and the confirmation mark (K-1170 (3)).</summary>
public sealed class SubmitWithdrawalInput : IFormInput
{
    [SpecAssignedName]
    public IList<WithdrawalItemRow> Items { get; } = [];

    [Display(Name = IbanFields.Label)]
    [InvalidIban]
    public string? Iban { get; set; }

    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>One row of UC-43's form: the item (empty when not chosen), the units as typed (the page's field rule reads them) and the open units the screen showed (K-1202).</summary>
public sealed class WithdrawalItemRow
{
    public Guid? OrderItemId { get; set; }

    public string? Quantity { get; set; }

    [SpecAssignedName]
    public int OriginalOpenQuantity { get; set; }
}

/// <summary>
/// One item of the new claim (04 5.19.3 (1)): the frozen name, the type, and the defective units' field - default one, upper limit the units open to a claim, the closed units taken off
/// (K-1170 (2); K-830).
/// </summary>
public sealed record DefectClaimRowView(Guid OrderItemId, string Name, ProductType ProductType, int DefaultQuantity, int MaxQuantity)
{
    /// <summary>A quantity field only on an item of more than one unit open to a claim (04 5.19.3; 2.12.1.8).</summary>
    public bool HasQuantityField => MaxQuantity > 1;
}

/// <summary>The claim the reopening shows (04 5.19.4): its item, its units, its first description, its opening day and the "Çözüldü" badge; the units' field starts at the units it has (K-1566).</summary>
public sealed record ReopenClaimView(Guid OrderItemId, string Name, int Quantity, int MaxQuantity, string Description, DateTimeOffset OpenedAt, Badge Badge)
{
    /// <summary>The units can be raised only where more than the claim's units are open to it (K-1566).</summary>
    public bool HasQuantityField => MaxQuantity > Quantity;
}

/// <summary>
/// E-19 — the defect claim screen <c>{kök}/ayip-talebi</c> (07 UC-44…UC-46; 04 5.19; T513). Two states: the new claim (no parameter) lists the items a claim is open for now with the defective
/// units' field and the description, and the firm's current return address when a physical item is on the list; the reopening (<c>orderItemId</c>; K-1182 (3)) shows the item's resolved claim,
/// its "Çözüldü" badge and the firm's e-mail for further information - no description (K-783). No confirmation step (04 5.19.5): <c>SubmitDefectClaim</c> opens the one claim of the item as
/// Açık with the return address of the moment written to it (06 4.30); <c>ReopenDefectClaim</c> makes the resolved claim Açık inside Z-18 and writes the customer's line to its history (K-681).
/// The module decides every condition; the page answers with 303 to the root and the reason when it refuses.
/// </summary>
public abstract class DefectClaimPageModel : AfterSalesScreenModel
{
    /// <summary>The handlers of the two forms (07 UC-45, UC-46).</summary>
    public const string SubmitHandler = "SubmitDefectClaim";

    public const string ReopenHandler = "ReopenDefectClaim";

    private readonly IOrderPage _page;
    private readonly IOrderOperations _operations;
    private readonly IOrderTransitions _transitions;
    private readonly ISettingsModule _settings;
    private readonly IAfterSalesModule _afterSales;
    private Guid? _reopenItemId;

    protected DefectClaimPageModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IOrderTransitions transitions, ISettingsModule settings, IAfterSalesModule afterSales)
        : base(ordering)
    {
        _page = page;
        _operations = operations;
        _transitions = transitions;
        _settings = settings;
        _afterSales = afterSales;
    }

    protected override string Screen => "ayip-talebi";

    /// <summary>The items of the new claim; empty in the reopening.</summary>
    public IReadOnlyList<DefectClaimRowView> Rows { get; private set; } = [];

    /// <summary>The claim of the reopening; null in the new claim.</summary>
    public ReopenClaimView? Reopening { get; private set; }

    /// <summary>The firm's current return address on the new claim with a physical item (04 5.19.3 (3)); null otherwise.</summary>
    public ReturnAddressView? ReturnAddress { get; private set; }

    /// <summary>The line of further information on the reopening (K-783).</summary>
    public string MoreInformationText { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync(DefectClaimQuery query, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (Request.Query.ContainsKey(DefectClaimQuery.Parameter))
        {
            // A value that is not an item key is not this order's item: the one 404 (07 2.3.3).
            if (!Guid.TryParse(query.OrderItemId, out Guid itemId))
            {
                return AccessDenial.NotFound();
            }

            _reopenItemId = itemId;
        }

        return await ReloadAsync(ct) ?? Page();
    }

    /// <summary>UC-45 (07 4.19.2).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostSubmitDefectClaimAsync(SubmitDefectClaimInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        (Guid? found, IActionResult? denial) = await FindOrderAsync(ct);
        if (found is not Guid orderId)
        {
            return denial ?? AccessDenial.NotFound();
        }

        if (input.OrderItemId is Guid chosen && !await IsOrderItemAsync(orderId, chosen, ct))
        {
            return AccessDenial.NotFound();
        }

        if (input.OrderItemId is null)
        {
            ModelState.AddFieldError(nameof(SubmitDefectClaimInput.OrderItemId), FieldCodes.FieldRequired);
        }

        int quantity = 1;
        if (!string.IsNullOrEmpty(input.Quantity))
        {
            if (ParseQuantity(input.Quantity) is int typed)
            {
                quantity = typed;
            }
            else
            {
                ModelState.AddFieldError(nameof(SubmitDefectClaimInput.Quantity), FieldCodes.InvalidQuantity, DisplayFormat.Number(1));
            }
        }

        if (!ModelState.IsValid || input.OrderItemId is not Guid itemId)
        {
            return await ReloadAsync(ct) ?? FormResults.Redisplay(this);
        }

        string description = input.Description ?? string.Empty;
        return await RunAsync(
            async stepCt => DecisionOf((await _transitions.OpenDefectClaimAsync(new OpenDefectClaimRequest(orderId, itemId, quantity, description), stepCt)).Refusal),
            ct);
    }

    /// <summary>UC-46 (07 4.19.3).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostReopenDefectClaimAsync(ReopenDefectClaimInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        (Guid? found, IActionResult? denial) = await FindOrderAsync(ct);
        if (found is not Guid orderId)
        {
            return denial ?? AccessDenial.NotFound();
        }

        // The item not of the order, or an item without a claim: the one 404 (07 4.19.3).
        if (input.OrderItemId is not Guid itemId || !await IsOrderItemAsync(orderId, itemId, ct) || await _afterSales.FindDefectClaimAsync(itemId, ct) is null)
        {
            return AccessDenial.NotFound();
        }

        _reopenItemId = itemId;
        int? quantity = null;
        if (!string.IsNullOrEmpty(input.Quantity))
        {
            if (ParseQuantity(input.Quantity) is not int typed)
            {
                ModelState.AddFieldError(nameof(ReopenDefectClaimInput.Quantity), FieldCodes.InvalidQuantity, DisplayFormat.Number(1));
                return await ReloadAsync(ct) ?? FormResults.Redisplay(this);
            }

            quantity = typed;
        }

        return await RunAsync(
            async stepCt => DecisionOf((await _transitions.ReopenDefectClaimAsync(ReopenDefectClaimRequest.ByCustomer(orderId, itemId, quantity), stepCt)).Refusal),
            ct);
    }

    protected override async Task<IActionResult?> LoadScreenAsync(Guid orderId, CancellationToken ct)
    {
        OrderOperationsView? operations = await _operations.ReadAsync(orderId, ActorKind.Buyer, ct);
        OrderPageDetail? detail = await _page.ReadAsync(orderId, ct);
        if (operations is null || detail is null)
        {
            return AccessDenial.NotFound();
        }

        _reopenItemId ??= ReopenItemOfForm();
        return _reopenItemId is Guid itemId ? await LoadReopeningAsync(operations, detail, itemId, ct) : await LoadNewClaimAsync(operations, detail, ct);
    }

    private async Task<IActionResult?> LoadNewClaimAsync(OrderOperationsView operations, OrderPageDetail detail, CancellationToken ct)
    {
        Rows =
        [
            .. operations.Open
                .Where(e => e.Operation == OrderOperation.OpenDefectClaim && e.OrderItemId is not null)
                .Select(e => (Entry: e, Item: detail.Items.FirstOrDefault(i => i.OrderItemId == e.OrderItemId)))
                .Where(p => p.Item is not null)
                .Select(p => new DefectClaimRowView(p.Item!.OrderItemId, p.Item.Name, p.Item.ProductType, Math.Max(1, p.Entry.Availability.DefaultQuantity), Math.Max(1, p.Entry.Availability.MaxQuantity))),
        ];
        if (Rows.Count == 0)
        {
            return Closed();
        }

        if (Rows.Any(r => r.ProductType == ProductType.Physical))
        {
            ReturnAddressSection section = await _settings.GetReturnAddressAsync(ct);
            if (section.Address is { } address)
            {
                IReadOnlyList<ProvinceInfo> provinces = await _settings.GetProvincesAsync(ct);
                ReturnAddress = new ReturnAddressView(address.RecipientName, address.AddressLine, address.District, ProvinceNameOf(provinces, address.ProvinceId), address.Phone);
            }
        }

        return null;
    }

    private async Task<IActionResult?> LoadReopeningAsync(OrderOperationsView operations, OrderPageDetail detail, Guid itemId, CancellationToken ct)
    {
        OrderPageItem? item = detail.Items.FirstOrDefault(i => i.OrderItemId == itemId);
        DefectClaimInfo? claim = item is null ? null : await _afterSales.FindDefectClaimAsync(itemId, ct);
        if (item is null || claim is null)
        {
            return AccessDenial.NotFound();
        }

        // The claim not resolved, or Z-18 over: the reopening is not open (07 4.19.1; 04 5.19.10).
        OrderOperationEntry? entry = operations.Open.FirstOrDefault(e => e.Operation == OrderOperation.ReopenDefectClaimByCustomer && e.OrderItemId == itemId);
        if (entry is null || claim.Status != DefectClaimStatus.Resolved)
        {
            return Closed();
        }

        Reopening = new ReopenClaimView(itemId, item.Name, claim.Quantity, Math.Max(claim.Quantity, entry.Availability.MaxQuantity), claim.Description, claim.CreatedAt, StatusBadges.For(claim.Status));
        CompanyIdentity identity = await _settings.GetCompanyIdentityAsync(ct);
        MoreInformationText = OrderScreenTexts.MoreInformation(string.IsNullOrWhiteSpace(identity.Email) ? null : identity.Email);
        return null;
    }

    private async Task<bool> IsOrderItemAsync(Guid orderId, Guid itemId, CancellationToken ct) =>
        (await _page.ReadAsync(orderId, ct))?.Items.Any(i => i.OrderItemId == itemId) == true;

    /// <summary>The item of the reopening's form when the validation filter answers it (the handler did not run).</summary>
    private Guid? ReopenItemOfForm() =>
        string.Equals(Request.Query["handler"], ReopenHandler, StringComparison.Ordinal) && Request.HasFormContentType
            && Guid.TryParse(Request.Form[nameof(ReopenDefectClaimInput.OrderItemId)], out Guid id)
            ? id
            : null;
}

/// <summary>UC-44…UC-46 under the member's root (07 4.19).</summary>
[NoStore]
public sealed class MemberDefectClaimModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IOrderTransitions transitions, ISettingsModule settings, IAfterSalesModule afterSales)
    : DefectClaimPageModel(ordering, page, operations, transitions, settings, afterSales)
{
    protected override bool IsMemberRoot => true;
}

/// <summary>UC-44…UC-46 under the access-key root (07 4.19): the key stays out of the referrer and the cache.</summary>
[KeyedLinkPage]
[NoStore]
public sealed class AccessDefectClaimModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IOrderTransitions transitions, ISettingsModule settings, IAfterSalesModule afterSales)
    : DefectClaimPageModel(ordering, page, operations, transitions, settings, afterSales)
{
    protected override bool IsMemberRoot => false;
}

/// <summary>UC-44's query (07 5.3.2 <c>DefectClaimQuery</c>; K-1182 (3)): the item of the reopening; text, so a wrong value is never a binding error (07 2.6.6).</summary>
public sealed class DefectClaimQuery : IQueryModel
{
    public const string Parameter = "orderItemId";

    public string? OrderItemId { get; init; }
}

/// <summary>UC-45's form (07 5): the item, the defective units - on an item of more than one unit - and the description (06 4.30; 04 7.1.10).</summary>
public sealed class SubmitDefectClaimInput : IFormInput
{
    [Display(Name = OrderScreenTexts.DefectClaimItemLabel)]
    public Guid? OrderItemId { get; set; }

    [Display(Name = OrderScreenTexts.DefectClaimQuantityLabel)]
    public string? Quantity { get; set; }

    [Display(Name = OrderScreenTexts.DefectClaimDescriptionLabel)]
    [FieldRequired]
    public string? Description { get; set; }
}

/// <summary>UC-46's form (07 5): the item and - when the units are raised - the new defective units; no description (K-783).</summary>
public sealed class ReopenDefectClaimInput : IFormInput
{
    public Guid? OrderItemId { get; set; }

    [Display(Name = OrderScreenTexts.DefectClaimQuantityLabel)]
    public string? Quantity { get; set; }
}
