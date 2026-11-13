using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Routing;
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
/// E-16 — the customer's order page (07 UC-33, UC-34, UC-36; 04 5.16; T511). The two roots give the same page and differ only in how the order is found (07 4.16.1): the member's root by
/// the account's ownership (<see cref="MemberOrderModel"/>), the access-key root by the key (<see cref="AccessOrderModel"/>). Every value on the page is a module's answer: the two axes,
/// the transfer details and the download buttons (<see cref="IOrderingModule.GetOrderPageSummaryAsync"/>), the frozen lists (<see cref="IOrderPage.ReadAsync"/>), the open operations of
/// the decision table (<see cref="IOrderOperations.ReadAsync"/>; 03 1.11 - a closed operation is not on the page at all), the delivery (<see cref="IOrderingModule.GetShippingViewAsync"/>),
/// the two documents as frozen (<see cref="IOrderDocuments"/>) and the privacy notice of the confirmation day. The page derives no status: the view reads the flags written here and
/// never a status name or a date sum (09 8.3 item 2c). A card order whose result has not come says so and does not refresh itself - no meta refresh, no script (04 5.16.4). The one
/// write is <c>DownloadDigitalFile</c> (UC-36): the module runs the operation's rule through the gate inside the page's transaction and the answer is 303 to the one-hour signed
/// address, or 422 with the reason in the button's place.
/// </summary>
public abstract class OrderPageModel : PageModel
{
    /// <summary>The message in the button's place once the right is used up (04 5.16.13; 02 3.12.6): the way to a renewal is the contact form's "Sipariş hakkında" type.</summary>
    public const string AllowanceUsedText = "İndirme hakkınız doldu. Hakkın yenilenmesi için iletişim formundan \"Sipariş hakkında\" konusuyla başvurabilirsiniz.";

    /// <summary>The 422 message when the payment is not confirmed (02 3.12.1): a transfer opens the download with the firm's "ödendi" mark.</summary>
    public const string PaymentNotConfirmedText = "İndirme, ödemeniz onaylandığında açılır. Havale ile ödemede ödemenizin firmaya ulaştığı işaretlenince indirebilirsiniz.";

    /// <summary>The 422 message when the item's file is not there (07 4.16.3): no right was spent.</summary>
    public const string NoFileText = "Bu ürünün dosyası şu anda indirilemiyor; indirme hakkınızdan düşülmedi. Daha sonra yeniden deneyin ya da iletişim formundan bize yazın.";

    /// <summary>The words of the card order whose result has not come yet (04 5.16.4; 03 2.5.1.2): the page does not refresh itself, the customer opens it again.</summary>
    public const string CardAwaitedText = "Kart ödemenizin sonucu bekleniyor. Sonuç gelince bu sayfa güncellenir; sayfayı daha sonra yeniden açarak durumu görebilirsiniz.";

    /// <summary>The words of the card order cancelled because its payment failed (04 5.16.4; 03 2.5.1.3): the cart stays as it was.</summary>
    public const string CardFailedText = "Kart ödemesi gerçekleşmedi ve sipariş iptal edildi. Sepetiniz olduğu gibi duruyor; sepetinizden yeni bir siparişle yeniden deneyebilirsiniz.";

    private readonly IOrderingModule _ordering;
    private readonly IOrderPage _page;
    private readonly IOrderOperations _operations;
    private readonly IOrderDocuments _documents;
    private readonly ISettingsModule _settings;
    private readonly IAfterSalesModule _afterSales;
    private IReadOnlyList<ItemRecordFact> _records = [];
    private IReadOnlyList<ProvinceInfo> _provinces = [];
    private bool _loaded;

    protected OrderPageModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IOrderDocuments documents, ISettingsModule settings, IAfterSalesModule afterSales)
    {
        _ordering = ordering;
        _page = page;
        _operations = operations;
        _documents = documents;
        _settings = settings;
        _afterSales = afterSales;
    }

    /// <summary>
    /// The records on an item's row, oldest first (04 5.16.5; OB-05): the delivery mark from the item, the others from the AfterSales module; the units only on an item of more than
    /// one unit (K-790). The same kind written twice is two entries (K-795).
    /// </summary>
    public IReadOnlyList<ItemRecordEntry> RecordsOf(OrderPageItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        bool showsUnits = item.Quantity > 1;
        List<ItemRecordEntry> entries =
        [
            .. _records.Where(r => r.OrderItemId == item.OrderItemId).Select(r => new ItemRecordEntry(
                NameOf(r.Kind),
                showsUnits ? r.Quantity : null,
                r.At,
                r.ReturnAddress is { } address ? OrderScreenTexts.RecordReturnAddress(address.RecipientName, address.AddressLine, address.District, AfterSalesScreenModel.ProvinceNameOf(_provinces, address.ProvinceId)) : null,
                r.ClaimStatus is { } status ? StatusBadges.For(status) : null)),
        ];
        if (item.DeliveryMarkedAt is { } marked)
        {
            entries.Add(new ItemRecordEntry(ItemRecordName.DeliveryMark, null, marked));
        }

        return entries;
    }

    private static ItemRecordName NameOf(ItemRecordFactKind kind) => kind switch
    {
        ItemRecordFactKind.Cancellation => ItemRecordName.Cancellation,
        ItemRecordFactKind.ItemRemoval => ItemRecordName.ItemRemoval,
        ItemRecordFactKind.LateDeliveryTermination => ItemRecordName.LateDeliveryTermination,
        ItemRecordFactKind.Withdrawal => ItemRecordName.Withdrawal,
        ItemRecordFactKind.ReturnReceipt => ItemRecordName.ReturnReceipt,
        ItemRecordFactKind.ReturnRejection => ItemRecordName.ReturnRejection,
        ItemRecordFactKind.NotReturnedClosure => ItemRecordName.NotReturnedClosure,
        ItemRecordFactKind.DefectClaim => ItemRecordName.DefectClaim,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Not a record of the item's row."),
    };

    /// <summary>The address the page was opened from - the member's root or the access-key root, as written (the key is case sensitive; 07 2.1.7): the forms and the screens hang under it.</summary>
    public string RootPath => Request.Path.Value ?? string.Empty;

    public OrderPageSummary Summary { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public OrderPageDetail Detail { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public OrderOperationsView Operations { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public OrderShippingView? Shipping { get; private set; }

    public OrderBadges Badges { get; private set; } = null!; // non-null: LoadAsync sets it before any handler returns Page()

    public string PreInformationFormText { get; private set; } = string.Empty;

    public string DistanceSalesContractText { get; private set; } = string.Empty;

    public LegalTextVersionInfo? PrivacyNotice { get; private set; }

    /// <summary>The order's date in the heading.</summary>
    public string CreatedOnText => DisplayFormat.Date(Detail.CreatedAt);

    /// <summary>The transfer block of "müşteriden beklenen": open while the order is Alındı + Bekliyor and paid by transfer (04 5.16.4; 03 1.11.13).</summary>
    public bool ShowsTransferDetails => Operations.IsOpen(OrderOperation.ViewTransferDetails) && Summary.TransferIban is not null;

    /// <summary>The last payment day of a transfer, as the module worked it out (a postponed day after an outage included; 04 5.16.4; D-668).</summary>
    public string? TransferEndsOnText => Summary.TransferPaymentEndsAt is { } end ? DisplayFormat.Date(end) : null;

    public string? TransferIbanText => Summary.TransferIban is { } iban ? DisplayFormat.Iban(iban) : null;

    public string PaymentMethodText => Detail.PaymentMethod == PaymentMethod.Card ? "Kart" : "Havale";

    /// <summary>The delivery area exists only for an order with a physical item (04 5.16.8).</summary>
    public bool HasPhysicalItem => Detail.Items.Any(i => i.ProductType == ProductType.Physical);

    /// <summary>The order came back to the firm (Teslim edilemedi; 03 2.5.3.4).</summary>
    public bool IsDeliveryFailed => Summary.OrderStatus == OrderStatus.DeliveryFailed;

    public string? ShippedOnText => Shipping?.ShippedAt is { } at ? DisplayFormat.Date(at) : null;

    public string? PromiseEndsOnText => Shipping?.PromiseEndsAt is { } at ? DisplayFormat.Date(at) : null;

    /// <summary>The open operations as the screen buttons of 04 5.16.6, each once, in the document's order; the screen does the choice of item (04 5.16.11).</summary>
    public IReadOnlyList<OrderScreenLink> ScreenLinks { get; private set; } = [];

    /// <summary>The item whose download was refused and the reason's words, written in the button's place on the 422 page.</summary>
    public Guid? RefusedItemId { get; private set; }

    public string? RefusalText { get; private set; }

    /// <summary>The download button of an item, or null when the item has none (not digital, or the payment is not confirmed; 02 3.12.1).</summary>
    public OrderDownloadButton? DownloadOf(Guid orderItemId) => Summary.Downloads.FirstOrDefault(d => d.OrderItemId == orderItemId);

    /// <summary>The delivery date the firm wrote with the item's delivery mark (04 5.16.5), or null when there is none; the mark's own moment stands in the records.</summary>
    public static string? DeliveryDateText(OrderPageItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return item.DeliveryDate is { } date ? DisplayFormat.Date(date) : null;
    }

    public static string RefundDateText(OrderPageRefund refund)
    {
        ArgumentNullException.ThrowIfNull(refund);
        return DisplayFormat.Date(refund.At);
    }

    /// <summary>The way a refund went (04 5.16.7): back to the card, or to the IBAN the customer gave.</summary>
    public static string RefundWayText(OrderPageRefund refund)
    {
        ArgumentNullException.ThrowIfNull(refund);
        return refund.Way == PaymentMethod.Card ? "Karta" : "IBAN'a";
    }

    public async Task<IActionResult> OnGetAsync(CancellationToken ct)
    {
        OrderRootResult root = await ResolveAsync(ct);
        if (root.OrderId is not Guid orderId)
        {
            return root.Denial ?? AccessDenial.NotFound();
        }

        return await LoadAsync(orderId, ct) ? Page() : AccessDenial.NotFound();
    }

    /// <summary>
    /// The validation filter answers a form whose fields break their rules itself, 422 without the handler (09 3.4 item 3; UC-35's IBAN format): the page it shows again still needs the
    /// root's check and the order, and the refused field its message.
    /// </summary>
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        PageHandlerExecutedContext executed = await next();
        if (_loaded || executed.Exception is not null || executed.Result is not PageResult { StatusCode: StatusCodes.Status422UnprocessableEntity })
        {
            return;
        }

        OrderRootResult root = await ResolveAsync(HttpContext.RequestAborted);
        if (root.OrderId is not Guid orderId || !await LoadAsync(orderId, HttpContext.RequestAborted))
        {
            executed.Result = root.Denial ?? AccessDenial.NotFound();
            return;
        }

        if (context.HandlerArguments.Values.OfType<SaveRefundIbanInput>().FirstOrDefault() is { } input)
        {
            IsIbanRefused = true;
            RefusedIbanItemId = input.OrderItemId;
            SentIban = input.Iban;
        }
    }

    /// <summary>
    /// UC-36 (07 4.16.3): the root first (404 like the GET), then the item - a digital item of this order, else 404 (07 2.3.3) - then the module's write in one transaction. Issued: 303
    /// to the signed address (UC-37). Refused: 422, the page again with the reason in the button's place; the line names <c>DownloadNotAvailable</c>.
    /// </summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostDownloadDigitalFileAsync(DownloadDigitalFileInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        OrderRootResult root = await ResolveAsync(ct);
        if (root.OrderId is not Guid orderId)
        {
            return root.Denial ?? AccessDenial.NotFound();
        }

        OrderPageDetail? detail = await _page.ReadAsync(orderId, ct);
        if (detail is null || !detail.Items.Any(i => i.OrderItemId == input.OrderItemId && i.ProductType == ProductType.Digital))
        {
            return AccessDenial.NotFound();
        }

        DownloadOperationResult? outcome = null;
        OperationRunner runner = HttpContext.RequestServices.GetRequiredService<OperationRunner>();
        GatedOperationResult result = await runner.RunAsync(
            async stepCt =>
            {
                outcome = await _page.TakeDownloadAsync(orderId, input.OrderItemId, stepCt);
                return outcome.Gate;
            },
            ct);
        if (!result.IsRejected && outcome?.LinkValue is string value)
        {
            return FormResults.SeeOther(Endpoints.Files.DownloadEndpoint.Prefix + "/" + value);
        }

        if (!await LoadAsync(orderId, ct))
        {
            return AccessDenial.NotFound();
        }

        RefusedItemId = input.OrderItemId;
        RefusalText = outcome?.Refusal switch
        {
            DownloadRefusal.AllowanceUsed => AllowanceUsedText,
            DownloadRefusal.NoFile => NoFileText,
            _ => PaymentNotConfirmedText,
        };
        HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.DownloadNotAvailable);
        return FormResults.Redisplay(this);
    }

    /// <summary>
    /// UC-35 (07 4.16.2): the root first (404 like the GET), then the item - an item not of this order is 404 (07 2.3.3) - then the module's write in one transaction. Saved: 303 to the root
    /// with "Kaydedildi" (the result stands in the field). A value that fails format or check digits, or a field that is not open now, is 422 with the field's message; no e-mail (K-710).
    /// </summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostSaveRefundIbanAsync(SaveRefundIbanInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        OrderRootResult root = await ResolveAsync(ct);
        if (root.OrderId is not Guid orderId)
        {
            return root.Denial ?? AccessDenial.NotFound();
        }

        if (string.IsNullOrWhiteSpace(input.Iban))
        {
            ModelState.AddFieldError(nameof(SaveRefundIbanInput.Iban), FieldCodes.FieldRequired);
            return await RedisplayIbanAsync(orderId, input, ct);
        }

        CustomerOperationResult? outcome = null;
        ICustomerOperations customer = HttpContext.RequestServices.GetRequiredService<ICustomerOperations>();
        OperationRunner runner = HttpContext.RequestServices.GetRequiredService<OperationRunner>();
        GatedOperationResult result = await runner.RunAsync(
            async stepCt =>
            {
                outcome = await customer.SaveRefundIbanAsync(orderId, input.OrderItemId, input.Iban, stepCt);
                return outcome.Gate;
            },
            ct);
        if (!result.IsRejected)
        {
            return FormResults.SeeOther(RootPath, NoticeCode.Saved);
        }

        switch (outcome?.Outcome)
        {
            case CustomerOperationOutcome.NotFound:
                return AccessDenial.NotFound();
            case CustomerOperationOutcome.InvalidIban:
                ModelState.AddFieldError(nameof(SaveRefundIbanInput.Iban), FieldCodes.InvalidIban);
                break;
            default:
                // The field closed meanwhile - the refund was processed or the request is gone (07 4.16.2: 422, 8.1.3); UC-35's code for a field that is not open (07 6.2.4).
                HttpContext.RequestServices.GetRequiredService<RejectionLog>().Rejected(ErrorCode.RefundIbanNotRequested);
                ModelState.AddModelError(string.Empty, IbanFieldClosedText);
                break;
        }

        return await RedisplayIbanAsync(orderId, input, ct);
    }

    /// <summary>The 422 message when the IBAN field is no longer open (07 4.16.2): the refund was processed or the request was closed meanwhile.</summary>
    public const string IbanFieldClosedText = "Bu IBAN alanı artık açık değil; geri ödeme işlenmiş ya da istek kapanmış olabilir. Siparişin güncel hâli aşağıda.";

    /// <summary>The field whose save was refused (the 422 page draws its message there) and the value as typed.</summary>
    public Guid? RefusedIbanItemId { get; private set; }

    public string? SentIban { get; private set; }

    public bool IsIbanRefused { get; private set; }

    private async Task<IActionResult> RedisplayIbanAsync(Guid orderId, SaveRefundIbanInput input, CancellationToken ct)
    {
        if (!await LoadAsync(orderId, ct))
        {
            return AccessDenial.NotFound();
        }

        IsIbanRefused = true;
        RefusedIbanItemId = input.OrderItemId;
        SentIban = input.Iban;
        return FormResults.Redisplay(this);
    }

    /// <summary>The open IBAN fields of the order (04 5.16.12): the item's field on its row, the late payment's itemless field in "müşteriden beklenen" (D-1194).</summary>
    public IReadOnlyList<RefundIbanFieldView> RefundIbanFields { get; private set; } = [];

    /// <summary>The reason a screen of the order returned with (07 4.16.1; K-1170 (1), (4)), said in the operations' place.</summary>
    public string? OperationReasonText { get; private set; }

    /// <summary>The short-lived "Kaydedildi" of UC-35 (07 6.3.1).</summary>
    public string? SavedText { get; private set; }

    /// <summary>The IBAN field of a place with the 422 message bound when it was the refused one.</summary>
    public IbanFieldView? IbanFieldOf(RefundIbanFieldView field)
    {
        ArgumentNullException.ThrowIfNull(field);
        // On the card line an IBAN exists only through one of the two exceptions (04 2.12.3.1): its correction keeps the exception's field.
        IbanOpening opening = field.Reason == RefundIbanRequestReason.CardRefundFailed || (field.IsCorrection && Detail.PaymentMethod == PaymentMethod.Card) ? IbanOpening.CardRefundNotRealized
            : field.OrderItemId is null && !field.IsCorrection ? IbanOpening.LatePayment
            : IbanOpening.Standard;
        bool refused = IsIbanRefused && RefusedIbanItemId == field.OrderItemId;
        IbanFieldView? view = Components.IbanFields.Create(
            IbanFieldPlace.OrderPage,
            Detail.PaymentMethod,
            opening,
            nameof(SaveRefundIbanInput.Iban),
            refused ? SentIban : field.Iban is { } stored ? DisplayFormat.Iban(stored) : null);
        return view is null || !refused ? view : view with { Field = Fields.Bind(view.Field, FormErrorSummary.From(ModelState)) };
    }

    /// <summary>Finds the order of the root: the order's id, or the denial the root answers with (302 to the sign-in or the one 404).</summary>
    protected abstract Task<OrderRootResult> ResolveAsync(CancellationToken ct);

    private async Task<bool> LoadAsync(Guid orderId, CancellationToken ct)
    {
        _loaded = true;
        OrderPageSummary? summary = await _ordering.GetOrderPageSummaryAsync(orderId, ct);
        OrderPageDetail? detail = await _page.ReadAsync(orderId, ct);
        OrderOperationsView? operations = await _operations.ReadAsync(orderId, ActorKind.Buyer, ct);
        if (summary is null || detail is null || operations is null)
        {
            return false;
        }

        Summary = summary;
        Detail = detail;
        Operations = operations;
        Badges = StatusBadges.ForOrder(summary.OrderStatus, summary.PaymentStatus);
        Shipping = await _ordering.GetShippingViewAsync(orderId, ct);
        PreInformationFormText = await _documents.GetTextAsync(orderId, LegalTextType.PreInformationForm, ct);
        DistanceSalesContractText = await _documents.GetTextAsync(orderId, LegalTextType.DistanceSalesContract, ct);
        PrivacyNotice = await _settings.GetLegalTextVersionAsync(detail.PrivacyNoticeVersionId, ct);
        ScreenLinks = OrderScreenLink.From(operations, RootPath);
        _records = await _afterSales.ListItemRecordsAsync([.. detail.Items.Select(i => i.OrderItemId)], ct);
        _provinces = _records.Any(r => r.ReturnAddress is not null) ? await _settings.GetProvincesAsync(ct) : [];
        RefundIbanFields = await HttpContext.RequestServices.GetRequiredService<ICustomerOperations>().ReadRefundIbanFieldsAsync(orderId, ct);
        // The reason of a return from an operation screen (07 6.3.3; K-1170 (1), (4)): one home of the words for E-17, E-18 and E-19 (04 5.17.11, 5.18.11, 5.19.10).
        OperationReasonText = OrderScreenTexts.ReasonOf(NoticeParameter.Read(Request, NoticeKind.Reason));
        SavedText = NoticeParameter.Read(Request, NoticeKind.ShortLived) is { } saved && NoticeTexts.TryShortLived(saved, out string? text) ? text : null;
        return true;
    }
}

/// <summary>The outcome of a root's lookup: the order's id, or the answer that denies it.</summary>
public sealed record OrderRootResult(Guid? OrderId, IActionResult? Denial)
{
    public static OrderRootResult Found(Guid orderId) => new(orderId, null);

    public static OrderRootResult Denied(IActionResult denial) => new(null, denial);
}

/// <summary>
/// UC-33 — the member's root <c>/siparis/{sipariş numarası}</c> (07 4.16.1, 2.3.2, 2.3.3, 2.3.5; T511). No session at all: 302 to the sign-in with the return path (the customer
/// scheme's challenge). An administrator's session is not a customer (07 2.3.5; 12 VAL-A609): with only that session the root is the one 404, like an order of another account, an
/// order linked to no account or a number that does not exist.
/// </summary>
[NoStore]
public sealed class MemberOrderModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IOrderDocuments documents, ISettingsModule settings, IAfterSalesModule afterSales)
    : OrderPageModel(ordering, page, operations, documents, settings, afterSales)
{
    private readonly IOrderingModule _ordering = ordering;

    protected override Task<OrderRootResult> ResolveAsync(CancellationToken ct) => OrderRoots.ResolveMemberAsync(this, _ordering, ct);
}

/// <summary>
/// UC-34 — the access-key root <c>/siparis/erisim/{erişim anahtarı}</c> (07 4.16.1, 2.3.4; 02 3.22.3, 3.22.5; T511): open to the guest and to the member alike. A key that is not
/// valid, a renewed key and the order whose personal data was disposed are the one 404 (04 5.16.20). The key stays out of the referrer and the cache (keyed link page, no-store).
/// </summary>
[KeyedLinkPage]
[NoStore]
public sealed class AccessOrderModel(IOrderingModule ordering, IOrderPage page, IOrderOperations operations, IOrderDocuments documents, ISettingsModule settings, IAfterSalesModule afterSales)
    : OrderPageModel(ordering, page, operations, documents, settings, afterSales)
{
    private readonly IOrderingModule _ordering = ordering;

    protected override Task<OrderRootResult> ResolveAsync(CancellationToken ct) => OrderRoots.ResolveAccessAsync(this, _ordering, ct);
}

/// <summary>One button of the operations area (04 5.16.6): the words and the screen under the root it opens (07 2.1.3).</summary>
public sealed record OrderScreenLink(string Text, string Href)
{
    /// <summary>The buttons of the open operations, in the order of 04 5.16.6; an operation open for several items is one button - the screen asks for the item (04 5.16.11).</summary>
    public static IReadOnlyList<OrderScreenLink> From(OrderOperationsView operations, string rootPath)
    {
        ArgumentNullException.ThrowIfNull(operations);
        bool Any(params OrderOperation[] wanted) => operations.Open.Any(e => wanted.Contains(e.Operation));
        List<OrderScreenLink> links = [];
        if (Any(OrderOperation.CancelOrder))
        {
            links.Add(new("Siparişi iptal et", OrderRoutes.ScreenPath(rootPath, "iptal")));
        }

        if (Any(OrderOperation.CancelPhysicalItem, OrderOperation.CancelServiceItem, OrderOperation.CancelDigitalItem))
        {
            links.Add(new("Kalemi iptal et", OrderRoutes.ScreenPath(rootPath, "iptal")));
        }

        if (Any(OrderOperation.TerminateForLateDelivery))
        {
            links.Add(new("Gecikme nedeniyle feshet", OrderRoutes.ScreenPath(rootPath, "gecikme-feshi")));
        }

        if (Any(OrderOperation.WithdrawPhysicalItem, OrderOperation.WithdrawServiceItem, OrderOperation.WithdrawDigitalItem))
        {
            links.Add(new("Cayma hakkını kullan", OrderRoutes.ScreenPath(rootPath, "cayma")));
        }

        if (Any(OrderOperation.OpenDefectClaim))
        {
            links.Add(new("Sorun bildir", OrderRoutes.ScreenPath(rootPath, "ayip-talebi")));
        }

        if (Any(OrderOperation.ReopenDefectClaimByCustomer))
        {
            links.Add(new("Ayıp talebini yeniden aç", OrderRoutes.ScreenPath(rootPath, "ayip-talebi")));
        }

        return links;
    }
}

/// <summary>UC-36's form (07 5.2): the item's internal key, which stands only in the page's forms (07 2.1.6).</summary>
public sealed class DownloadDigitalFileInput : IFormInput
{
    public Guid OrderItemId { get; set; }
}

/// <summary>UC-35's form (07 5): the IBAN (blanks ignored; 07 2.9.5) and the item - empty on the late payment's itemless refund (06 4.29; K-1075).</summary>
public sealed class SaveRefundIbanInput : IFormInput
{
    [System.ComponentModel.DataAnnotations.Display(Name = Components.IbanFields.Label)]
    [InvalidIban]
    public string Iban { get; set; } = string.Empty;

    public Guid? OrderItemId { get; set; }
}
