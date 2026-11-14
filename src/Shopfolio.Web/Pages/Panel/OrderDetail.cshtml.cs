using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Lists;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Administration.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Panel;

/// <summary>The line step the screen offers as its primary button (04 9.9.5; K-803); none when the order has no required step left.</summary>
public enum OrderPrimaryStep
{
    None,
    MarkPaid,
    MarkShipped,
    MarkDelivered,
}

/// <summary>
/// E-37, the order's screen in the panel (T527; 04 9.9; 07 UC-92, UC-123...UC-128): the sections of 04 9.9.3-9.9.13 as the server worked them out, the primary button - the line's next required step -, the
/// order's other open steps, and the six line steps' handlers. The open operations are the decision's (<see cref="IOrderOperations"/>, the administrator's side); the screen derives no state of its own
/// (04 9.9.37). Every step carries the order's version: a stale version is 409 with the order as it is now and no way to overwrite (04 9.9.34); a step that is not open, or a refused value, is 422. A key
/// that is not an order's is 404 (K-803). The GET writes nothing.
/// </summary>
public sealed class OrderDetailModel : PageModel
{
    public const string BackLabel = "Listeye dön";

    public const string MarkPaidLabel = "Ödendi olarak işaretle";

    public const string ShipLabel = "Kargoya ver";

    public const string ReshipLabel = "Kargoya yeniden ver";

    public const string MarkDeliveredLabel = "Teslim edildi olarak işaretle";

    public const string MarkDeliveryFailedLabel = "Teslim edilemedi olarak işaretle";

    public const string CloseLabel = "Siparişi kapat";

    /// <summary>K-799: the step is not open on the order as it is now; nothing was applied.</summary>
    public const string NotOpenText = "Bu işlem siparişin şu anki hâlinde açık değil; işlem uygulanmadı. Siparişin güncel hâli aşağıda.";

    /// <summary>K-799: the shipment form without the carrier and the tracking number or the own-vehicle declaration (04 9.9.36).</summary>
    public const string ShipmentMissingText = "Kargo şirketini ve takip numarasını girin ya da kendi aracınızla teslimi seçin.";

    public const string DeliveryDateMissingText = "Teslim tarihini girin.";

    public const string DeliveryDateFutureText = "Teslim tarihi ileri bir tarih olamaz.";

    public const string DeliveryDateBeforeShipmentText = "Teslim tarihi siparişin kargoya verildiği günden önce olamaz.";

    /// <summary>K-608: the date is a withdrawal declaration's day; the answer of which came first is required.</summary>
    public const string DeliveryOrderQuestionText = "Bu tarihte bir cayma beyanı var. Teslim beyandan önce mi, sonra mı oldu?";

    /// <summary>The internal note's line above the field (04 9.9.11).</summary>
    public const string InternalNoteHiddenText = "Bu not müşteriye hiçbir yerde görünmez.";

    /// <summary>The withdrawal window's name on the item's row (04 9.9.6; Z-13, Z-14).</summary>
    public const string WithdrawalPeriodLabel = "Cayma süresi";

    /// <summary>The defect claim channel's name on the item's row (04 9.9.6; Z-18).</summary>
    public const string DefectPeriodLabel = "Ayıp bildirim süresi";

    /// <summary>The refund period's name beside a pending refund (04 9.9.6; Z-16, Z-17; K-491).</summary>
    public const string RefundPeriodLabel = "Geri ödeme süresi";

    /// <summary>A customer notice's name in the e-mail history (04 9.9.12): its code, B-1…B-16.</summary>
    public static string NoticeNameOf(Shopfolio.Modules.Notifications.Contracts.DispatchType type)
    {
        string code = type.ToString();
        return $"{code[..1]}-{code[1..]}";
    }

    /// <summary>A notice's result in the e-mail history (04 9.9.12): sent, "e-posta ulaşmadı", or still waiting to go.</summary>
    public static string NoticeStateOf(Shopfolio.Modules.Notifications.Contracts.DispatchState state) => state switch
    {
        Shopfolio.Modules.Notifications.Contracts.DispatchState.Sent => "gönderildi",
        Shopfolio.Modules.Notifications.Contracts.DispatchState.Undelivered => "e-posta ulaşmadı",
        _ => "gönderilmeyi bekliyor",
    };

    private readonly IOrderingModule _ordering;
    private readonly IOrderPage _page;
    private readonly IOrderOperations _operations;
    private readonly IAfterSalesModule _afterSales;
    private readonly IOrderDocuments _documents;

    public OrderDetailModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IAfterSalesModule afterSales, IOrderDocuments documents)
    {
        ArgumentNullException.ThrowIfNull(ordering);
        ArgumentNullException.ThrowIfNull(page);
        ArgumentNullException.ThrowIfNull(operations);
        ArgumentNullException.ThrowIfNull(afterSales);
        ArgumentNullException.ThrowIfNull(documents);
        _ordering = ordering;
        _page = page;
        _operations = operations;
        _afterSales = afterSales;
        _documents = documents;
    }

    public OrderPageSummary Summary { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page(); a missing order answers 404 first

    public OrderPageDetail Detail { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public OrderOperationsView Operations { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public OrderBadges Badges { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public OrderShippingView? Shipping { get; private set; }

    public OrderPrimaryStep PrimaryStep { get; private set; }

    public string PagePath { get; private set; } = string.Empty;

    public string BackPath { get; private set; } = PanelFramePaths.Orders;

    public string PreInformationFormText { get; private set; } = string.Empty;

    public string DistanceSalesContractText { get; private set; } = string.Empty;

    /// <summary>The confirmation window of an unconfirmed first POST (07 UC-123...UC-125: 200); null otherwise.</summary>
    public ConfirmView? Confirm { get; private set; }

    /// <summary>The lasting message of a refused step (04 9.9.41: in the step's place, the form kept); null when none.</summary>
    public string? RefusalText { get; private set; }

    /// <summary>True when the version the screen showed is no longer the order's (04 9.9.34): the order notice is shown.</summary>
    public bool IsConflict { get; private set; }

    /// <summary>The refusal as a lasting message in the step's place; null when there is none.</summary>
    public MessageView? RefusalMessage => RefusalText is null ? null : Messages.InPlace(MessageMeaning.Error, RefusalText, "order-step-message");

    /// <summary>The order question of a delivery date on a withdrawal declaration's day, with the declarations' moments (K-608); empty when not asked.</summary>
    public IReadOnlyList<string> DeclarationMomentTexts { get; private set; } = [];

    public bool AsksDeliveryOrder { get; private set; }

    public MarkShippedInput Ship { get; private set; } = new();

    public MarkDeliveredInput Delivery { get; private set; } = new();

    public IReadOnlyList<ItemRecordFact> Records { get; private set; } = [];

    public bool HasPhysicalItem => Detail.Items.Any(i => i.ProductType == ProductType.Physical);

    public IReadOnlyList<CarrierEntry> Carriers => CarrierCatalog.Default.Active;

    public bool IsOpen(OrderOperation operation) => Operations.IsOpen(operation);

    /// <summary>The panel-only facts (04 9.9.4, 9.9.6, 9.9.10-9.9.12): the marks, the items' facts, the pending refunds, the running periods and the three histories.</summary>
    public PanelOrderDetailFacts Facts { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    /// <summary>The order's marks whose condition holds now, in the document's order (04 9.9.4; 2.5.3); the two item marks are drawn on the item's row.</summary>
    public IReadOnlyList<PanelMark> Marks => OrderListTexts.MarksOf(Facts.Marks, PanelMarkPlace.OrderDetail);

    /// <summary>The records on an item's row (04 9.9.6; 2.5.2): the AfterSales module's under their own names, then the delivery mark.</summary>
    public IReadOnlyList<ItemRecordEntry> RecordsOf(OrderPageItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return ItemRecordLabels.EntriesOf(Records, item.OrderItemId, item.Quantity, item.DeliveryMarkedAt);
    }

    /// <summary>The item's panel facts (04 9.9.6): the open quantity, its marks and its periods, as the server read them.</summary>
    public PanelItemFacts? ItemFactsOf(OrderPageItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return Facts.Items.FirstOrDefault(i => i.OrderItemId == item.OrderItemId);
    }

    /// <summary>The item's marks (04 2.5.3.4, 2.5.3.5): "IBAN bekleniyor" and "iade malı bekleniyor", from the server's facts.</summary>
    public IReadOnlyList<PanelMark> ItemMarksOf(PanelItemFacts facts)
    {
        ArgumentNullException.ThrowIfNull(facts);
        return PanelMarks.Of(new PanelMarkConditions(IbanPending: facts.IbanPending, ReturnGoodsPending: facts.ReturnGoodsPendingUnits > 0), PanelMarkPlace.OrderDetail);
    }

    /// <summary>The refunds still pending on the item (04 9.9.6 "bekleyen geri ödeme"); the amount is the one the server started the refund with.</summary>
    public IReadOnlyList<PanelPendingRefund> PendingRefundsOf(OrderPageItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return [.. Facts.PendingRefunds.Where(r => r.OrderItemId == item.OrderItemId)];
    }

    /// <summary>The order's defect claims (04 9.9.7; K-751, K-764), oldest first; no section when there is none.</summary>
    public IReadOnlyList<OrderDefectClaim> Claims { get; private set; } = [];

    /// <summary>The claims whose B-9 ended in an "undelivered" row: the mark shows on the claim's row too (04 2.5.3.1; K-760).</summary>
    public IReadOnlySet<Guid> MarkedClaimIds { get; private set; } = new HashSet<Guid>();

    /// <summary>
    /// The undelivered e-mails under the order's "e-posta ulaşmadı" mark (04 9.9.4, 2.5.3.1): the notice's name, the first sending's date and the address. The re-sending (UC-153) is T531's, so no
    /// row offers its button yet.
    /// </summary>
    public UndeliveredEmailList UndeliveredEmails { get; private set; } = new(string.Empty, string.Empty, string.Empty, [], false);

    private Dictionary<Guid, string> _provinceNames = [];

    /// <summary>The claimed item's name (04 9.9.7).</summary>
    public string ItemNameOf(Guid orderItemId) => Detail.Items.FirstOrDefault(i => i.OrderItemId == orderItemId)?.Name ?? string.Empty;

    /// <summary>The province's name of a claim's return address.</summary>
    public string ProvinceNameOf(Guid provinceId) => _provinceNames.TryGetValue(provinceId, out string? name) ? name : string.Empty;

    /// <summary>The claim's own mark (04 2.5.3.1; K-760): "e-posta ulaşmadı" when its B-9 did not arrive.</summary>
    public IReadOnlyList<PanelMark> ClaimMarksOf(OrderDefectClaim claim)
    {
        ArgumentNullException.ThrowIfNull(claim);
        return PanelMarks.Of(new PanelMarkConditions(EmailNotDelivered: MarkedClaimIds.Contains(claim.Id)), PanelMarkPlace.OrderDetail);
    }

    /// <summary>The pending refunds that name no item (an amount based refund, a late payment's refund).</summary>
    public IReadOnlyList<PanelPendingRefund> OrderPendingRefunds => [.. Facts.PendingRefunds.Where(r => r.OrderItemId is null)];

    /// <summary>A period that ends on <paramref name="lastDay"/> as the panel's period indicator draws it (04 2.6.3): the server's last day placed against the page's instant.</summary>
    internal PeriodIndicator PeriodOf(string label, CountedPeriod period, DateOnly lastDay) => new(label, period, PeriodStanding.Of(period, lastDay, Now, BusinessCalendar.Product));

    /// <summary>The firm's running periods of the order (Z-10, Z-39) with the legal interest warning on overrun (04 9.9.5, 2.6.3): the list's one naming rule.</summary>
    internal IReadOnlyList<PeriodIndicator> RunningPeriods => [.. Facts.RunningPeriods.Select(p => OrderListTexts.PeriodOf(p, Now))];

    private DateTimeOffset Now => HttpContext.RequestServices.GetRequiredService<TimeProvider>().GetUtcNow();

    public async Task<IActionResult> OnGetAsync(Guid id, CancellationToken ct) =>
        await LoadAsync(id, ct) ? Page() : NotFound();

    /// <summary>UC-123: the transfer order's "paid" mark; unconfirmed, the window repeats the frozen IBAN and the total (200); confirmed, Ö1 and its effects (303).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostMarkPaidAsync(Guid id, MarkPaidInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelStepResult result = await RunStepAsync((steps, token) => steps.MarkPaidAsync(id, admin, input.Version, input.Confirmed, token), ct);
        return result.Refusal == TransitionRefusal.ConfirmationRequired
            ? await AskAsync(id, () => MarkPaidConfirm(id, input.Version), ct)
            : await AnswerAsync(id, result, ct);
    }

    /// <summary>UC-124: the shipment; the carrier and the tracking number or the own-vehicle declaration are required (422); unconfirmed 200, confirmed 303.</summary>
    [GatedOperation]
    public Task<IActionResult> OnPostMarkShippedAsync(Guid id, MarkShippedInput input, CancellationToken ct) => SendAsync(id, input, false, ct);

    /// <summary>UC-125: the re-sending of a returned order; as UC-124.</summary>
    [GatedOperation]
    public Task<IActionResult> OnPostMarkReshippedAsync(Guid id, MarkShippedInput input, CancellationToken ct) => SendAsync(id, input, true, ct);

    /// <summary>UC-126: the delivery mark and date; a future date or one before the shipment's day is 422, a declaration's day without the order's answer is 422 with the question (K-608).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostMarkDeliveredAsync(Guid id, MarkDeliveredInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        Delivery = input;
        if (!DateOnly.TryParseExact(input.DeliveryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            return await RefuseAsync(id, DeliveryDateMissingText, string.IsNullOrWhiteSpace(input.DeliveryDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate, ct);
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelStepResult result = await RunStepAsync(
            (steps, token) => steps.MarkDeliveredAsync(new MarkDeliveredRequest(id, admin, input.Version, date), input.IsDeliveredBeforeDeclaration, token),
            ct);
        if (result.AsksDeliveryOrder)
        {
            AsksDeliveryOrder = true;
            DeclarationMomentTexts = [.. result.DeclarationMoments.Select(DisplayFormat.DateAndTime)];
            return await RefuseAsync(id, DeliveryOrderQuestionText, ErrorCode.DeliverySequenceRequired, ct);
        }

        return await AnswerAsync(id, result, ct);
    }

    /// <summary>UC-127: the returned shipment's mark (S7; S11 when a delivered item leaves no open item).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostMarkDeliveryFailedAsync(Guid id, MarkDeliveryFailedInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelStepResult result = await RunStepAsync((steps, token) => steps.MarkDeliveryFailedAsync(new AdminOrderRequest(id, admin, input.Version), token), ct);
        return await AnswerAsync(id, result, ct);
    }

    /// <summary>UC-128: the reasonless closing of a returned order; no cancellation record, no refund, no B-7 (K-717).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCloseDeliveryFailedAsync(Guid id, CloseDeliveryFailedInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelStepResult result = await RunStepAsync((steps, token) => steps.CloseDeliveryFailedAsync(new AdminOrderRequest(id, admin, input.Version), token), ct);
        return await AnswerAsync(id, result, ct);
    }

    // UC-124, UC-125: the carrier data is the module's to check (K-799; one rule with the transition unit): a value it refuses is 422 with the form's message, before the lock and the version.
    private async Task<IActionResult> SendAsync(Guid id, MarkShippedInput input, bool isResending, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        Ship = input;
        ShipmentInfo shipment = new(input.Carrier, input.OtherCarrierName, input.TrackingNumber, input.IsOwnVehicleDelivery);
        TransitionAdmin admin = await AdminAsync(ct);
        ShipOrderRequest request = new(id, admin, input.Version, input.Confirmed, shipment);
        PanelStepResult result = await RunStepAsync(
            (steps, token) => isResending ? steps.MarkReshippedAsync(request, token) : steps.MarkShippedAsync(request, token),
            ct);
        return result.Refusal switch
        {
            TransitionRefusal.InvalidInput => await RefuseAsync(id, ShipmentMissingText, ErrorCode.ShipmentInfoMissing, ct),
            TransitionRefusal.ConfirmationRequired => await AskAsync(id, () => ShipmentConfirm(id, input, isResending), ct),
            _ => await AnswerAsync(id, result, ct),
        };
    }

    // The unconfirmed first POST that passed the module's checks under the lock - the version first (409), then the operation's rule (422) - and wrote nothing: the window (200).
    private async Task<IActionResult> AskAsync(Guid id, Func<ConfirmView> confirm, CancellationToken ct)
    {
        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }

        Confirm = confirm();
        return Page();
    }

    private ConfirmView MarkPaidConfirm(Guid id, int version)
    {
        IrreversibleConfirmation texts = IrreversibleConfirmations.TransferPaidMark(Detail.Items.Any(i => i.ProductType == ProductType.Digital));
        string repeat = $"IBAN: {(Summary.TransferIban is { } iban ? DisplayFormat.Iban(iban) : "-")} · Tutar: {DisplayFormat.Amount(Detail.Breakdown.Total)}";
        IrreversibleConfirmation withAmount = texts with { Sentence = repeat + ". " + texts.Sentence };
        return Confirmations.ForIrreversible(withAmount, MarkPaidLabel, MarkPaidLabel, PathOf(id), "MarkPaid", PathOf(id), [Field("Input.Version", version), new("Input.Confirmed", "true")]);
    }

    private static ConfirmView ShipmentConfirm(Guid id, MarkShippedInput input, bool isResending)
    {
        string label = isResending ? ReshipLabel : ShipLabel;
        List<KeyValuePair<string, string>> fields =
        [
            Field("Input.Version", input.Version),
            new("Input.Carrier", input.Carrier ?? string.Empty),
            new("Input.OtherCarrierName", input.OtherCarrierName ?? string.Empty),
            new("Input.TrackingNumber", input.TrackingNumber ?? string.Empty),
            new("Input.IsOwnVehicleDelivery", input.IsOwnVehicleDelivery ? "true" : "false"),
            new("Input.Confirmed", "true"),
        ];
        return Confirmations.ForIrreversible(
            IrreversibleConfirmations.Shipment(isResending, input.IsOwnVehicleDelivery),
            label,
            label,
            PathOf(id),
            isResending ? "MarkReshipped" : "MarkShipped",
            PathOf(id),
            fields);
    }

    private async Task<IActionResult> AnswerAsync(Guid id, PanelStepResult result, CancellationToken ct)
    {
        switch (result.Refusal)
        {
            case null:
                return FormResults.SeeOther(PathOf(id));
            case TransitionRefusal.OrderNotFound:
                return NotFound();
            case TransitionRefusal.VersionConflict:
                if (!await LoadAsync(id, ct))
                {
                    return NotFound();
                }

                Reject(ErrorCode.OrderVersionConflict);
                IsConflict = true;
                return FormResults.Conflict(this);
            default:
                (string text, ErrorCode code) = result.DateObstacle switch
                {
                    DeliveryDateObstacle.InTheFuture => (DeliveryDateFutureText, ErrorCode.InvalidDate),
                    DeliveryDateObstacle.BeforeShipment => (DeliveryDateBeforeShipmentText, ErrorCode.InvalidDate),
                    _ => (NotOpenText, ErrorCode.OperationNotAvailable),
                };
                return await RefuseAsync(id, text, code, ct);
        }
    }

    private async Task<IActionResult> RefuseAsync(Guid id, string text, ErrorCode code, CancellationToken ct)
    {
        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }

        Reject(code);
        RefusalText = text;
        return FormResults.Redisplay(this);
    }

    // The one rejection line of a refused step with its code (07 6.1.3, 6.2; 09 8.2 item 3; K-1198): the screen shows the text, the log keeps the code.
    private void Reject(ErrorCode code) => HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(code);

    // The page opens the operation's transaction (09 8.3 item 3); the module's write method locks, compares the version, asks the rule and writes. A rejected step rolls it back.
    private async Task<PanelStepResult> RunStepAsync(Func<IPanelOrderSteps, CancellationToken, Task<PanelStepResult>> step, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        IPanelOrderSteps steps = services.GetRequiredService<IPanelOrderSteps>();
        OperationRunner runner = services.GetRequiredService<OperationRunner>();
        PanelStepResult? outcome = null;
        await runner.RunAsync(
            async token =>
            {
                outcome = await step(steps, token);
                return outcome.Gate;
            },
            ct);
        return outcome!; // non-null: the runner runs the step once before it returns
    }

    private async Task<TransitionAdmin> AdminAsync(CancellationToken ct)
    {
        Guid adminId = User.AccountId() ?? throw new InvalidOperationException("A panel request carries the administrator's session (the panel filter refuses the others).");
        string name = await HttpContext.RequestServices.GetRequiredService<IAdministrationModule>().GetAdminNameAsync(adminId, ct) ?? string.Empty;
        return new TransitionAdmin(adminId, name);
    }

    private async Task<bool> LoadAsync(Guid id, CancellationToken ct)
    {
        OrderPageSummary? summary = await _ordering.GetOrderPageSummaryAsync(id, ct);
        OrderPageDetail? detail = await _page.ReadAsync(id, ct);
        OrderOperationsView? operations = await _operations.ReadAsync(id, ActorKind.Admin, ct);
        PanelOrderDetailFacts? facts = await HttpContext.RequestServices.GetRequiredService<IPanelOrderReader>().ReadDetailAsync(id, ct);
        if (summary is null || detail is null || operations is null || facts is null)
        {
            return false;
        }

        Facts = facts;

        Summary = summary;
        Detail = detail;
        Operations = operations;
        Badges = StatusBadges.ForOrder(summary.OrderStatus, summary.PaymentStatus);
        Shipping = await _ordering.GetShippingViewAsync(id, ct);
        Records = await _afterSales.ListItemRecordsAsync([.. detail.Items.Select(i => i.OrderItemId)], ct);
        await LoadClaimsAndUndeliveredAsync(id, ct);
        PreInformationFormText = await _documents.GetTextAsync(id, LegalTextType.PreInformationForm, ct);
        DistanceSalesContractText = await _documents.GetTextAsync(id, LegalTextType.DistanceSalesContract, ct);
        PagePath = PathOf(id);
        BackPath = ListReturn.Target(HttpContext.Request.Query[ReturnPath.ParameterName], PanelFramePaths.Orders);
        PrimaryStep = PrimaryOf(operations);
        return true;
    }

    // 04 9.9.7 and 9.9.4: the claims with their marks, and the undelivered e-mails under the order's mark; each a read for the whole order.
    private async Task LoadClaimsAndUndeliveredAsync(Guid id, CancellationToken ct)
    {
        Shopfolio.Modules.Notifications.Contracts.INotificationsModule notifications = HttpContext.RequestServices.GetRequiredService<Shopfolio.Modules.Notifications.Contracts.INotificationsModule>();
        Claims = await _afterSales.ListDefectClaimsAsync([.. Detail.Items.Select(i => i.OrderItemId)], ct);
        if (Claims.Count > 0)
        {
            MarkedClaimIds = await notifications.FindClaimsWithUndeliveredMarkAsync([.. Claims.Select(c => c.Id)], ct);
            if (Claims.Any(c => c.ReturnAddress is not null))
            {
                IReadOnlyList<ProvinceInfo> provinces = await HttpContext.RequestServices.GetRequiredService<ISettingsModule>().GetProvincesAsync(ct);
                _provinceNames = provinces.ToDictionary(p => p.Id, p => p.Name);
            }
        }

        IReadOnlyList<Shopfolio.Modules.Notifications.Contracts.UndeliveredNotice> undelivered = Facts.Marks.EmailNotDelivered ? await notifications.FindUndeliveredNoticesAsync(id, ct) : [];
        UndeliveredEmails = new UndeliveredEmailList(
            PathOf(id),
            string.Empty,
            string.Empty,
            [.. undelivered.Select(n => new UndeliveredEmailRow(n.LatestDispatchId.ToString("D"), NoticeNameOf(n.Type), n.FirstAt, n.RecipientEmail ?? string.Empty, false))],
            false);
    }

    // 04 9.9.5 (K-803): the line's next required step. The delivery failure, the re-sending and the closing are in "Sipariş işlemleri", never primary. On a shipped order whose physical
    // items all closed by a withdrawal or a late delivery termination no order-level step is required (04 6.3.9.5; 03 1.4.3): the delivery mark is not open then (the catalog's row asks an
    // open physical item, as the writer does), and the way on is "Teslim edilemedi" and the closing.
    private static OrderPrimaryStep PrimaryOf(OrderOperationsView operations)
    {
        if (operations.IsOpen(OrderOperation.MarkTransferPaid))
        {
            return OrderPrimaryStep.MarkPaid;
        }

        if (operations.IsOpen(OrderOperation.ShipOrder))
        {
            return OrderPrimaryStep.MarkShipped;
        }

        return operations.IsOpen(OrderOperation.MarkDelivered) ? OrderPrimaryStep.MarkDelivered : OrderPrimaryStep.None;
    }

    private static KeyValuePair<string, string> Field(string name, int value) => new(name, value.ToString(CultureInfo.InvariantCulture));

    private static string PathOf(Guid id) => $"{PanelFramePaths.Orders}/{id:D}";
}

/// <summary>UC-127 <c>MarkDeliveryFailedInput</c>: the order's version only (07 9.9.6).</summary>
public sealed class MarkDeliveryFailedInput : IFormInput
{
    public int Version { get; set; }
}

/// <summary>UC-128 <c>CloseDeliveryFailedInput</c>: the order's version only (07 9.9.7).</summary>
public sealed class CloseDeliveryFailedInput : IFormInput
{
    public int Version { get; set; }
}

/// <summary>UC-123 <c>MarkPaidInput</c>: the version and the confirmation.</summary>
public sealed class MarkPaidInput : IFormInput
{
    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>UC-124, UC-125 <c>MarkShippedInput</c> · <c>MarkReshippedInput</c>: the carrier data (06 4.17), the version and the confirmation.</summary>
public sealed class MarkShippedInput : IFormInput
{
    public string? Carrier { get; set; }

    public string? OtherCarrierName { get; set; }

    public string? TrackingNumber { get; set; }

    public bool IsOwnVehicleDelivery { get; set; }

    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>UC-126 <c>MarkDeliveredInput</c>: the delivery date (text, so a wrong value is a refusal and not a binding error), the order question's answer and the version.</summary>
public sealed class MarkDeliveredInput : IFormInput
{
    public string? DeliveryDate { get; set; }

    public bool? IsDeliveredBeforeDeclaration { get; set; }

    public int Version { get; set; }
}
