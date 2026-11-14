using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// E-37's "Düzeltmeler" (T530; 04 9.9.31; 07 UC-146...UC-151): the address, the carrier, the delivery date, a return's arrival date, a status transition and a guest order's e-mail. None asks the
/// irreversible-action confirmation; the address and the e-mail forms open with K-798's reminder. Each carries the order's version: a stale one is 409 with the order as it is now (04 9.9.34), a closed
/// operation or a refused value 422 with the message in the form's place and the form kept (04 9.9.41). The writes are the module's (<see cref="IPanelOrderCorrections"/>); the page derives no state.
/// </summary>
public sealed partial class OrderDetailModel
{
    public const string CorrectionsHeading = "Düzeltmeler";

    public const string CorrectShippingAddressLabel = "Teslimat adresini düzelt";

    public const string CorrectBillingAddressLabel = "Fatura adresini düzelt";

    public const string CorrectCarrierLabel = "Kargo bilgisini düzelt";

    public const string CorrectDeliveryDateLabel = "Teslim tarihini düzelt";

    public const string CorrectArrivalDateLabel = "Ulaşma tarihini düzelt";

    public const string CorrectTransitionLabel = "Durum geçişini düzelt";

    public const string CorrectOrderEmailLabel = "E-postayı düzelt";

    /// <summary>K-798 (6): the reminder at the head of the address correction (K-601, K-735).</summary>
    public const string AddressReminderText =
        "Talep siparişin e-posta adresinden gelmediyse, işlemden önce siparişin e-posta adresine yazarak ya da teslimat telefonunu arayarak siparişin sahibiyle teyit edin. Sipariş numarası tek başına teyit değildir.";

    /// <summary>K-798 (6): the identity reminder at the head of a guest order's e-mail correction (03 6.2.1.3).</summary>
    public const string EmailReminderText = "Kimliği siparişteki bilgilerle — ad, teslimat telefonu, ürünler ve tutar — teyit edin. Sipariş numarası tek başına teyit değildir.";

    /// <summary>A request that names neither address (the forms always send one): nothing is corrected.</summary>
    public const string AddressTypeMissingText = "Düzeltilecek adres belirtilmedi; formu sayfadan yeniden gönderin.";

    /// <summary>The element ids of the correction forms; a refusal's message is drawn in the form whose id it carries (04 2.3.3, 2.7.3.1).</summary>
    public const string ShippingAddressFormId = "correct-shipping-address-form";

    public const string BillingAddressFormId = "correct-billing-address-form";

    public const string CarrierFormId = "correct-carrier-form";

    public const string DeliveryDateFormId = "correct-delivery-date-form";

    public const string TransitionFormId = "correct-transition-form";

    public const string OrderEmailFormId = "correct-order-email-form";

    /// <summary>The element id of a correction's refusal message.</summary>
    public const string CorrectionMessageId = "correction-message";

    /// <summary>The prefixes of the two address forms' field names (07 UC-146 <c>Shipping…</c>, <c>Billing…</c>): each form has its own names and so its own element ids (04 2.3.1.1).</summary>
    public const string ShippingAddressPrefix = "Input.Shipping";

    public const string BillingAddressPrefix = "Input.Billing";

    public static string ArrivalDateFormId(Guid recordId) => "correct-arrival-date-form-" + recordId.ToString("D");

    public const string UnchangedText = "Girilen değer siparişin kayıtlı değeriyle aynı; düzeltilecek bir şey yok.";

    public const string DeliveryDateAfterArrivalText = "Teslim tarihi kayıtlı bir iade ulaşma tarihinden sonra olamaz.";

    public const string ArrivalDateMissingText = "Ulaşma tarihini girin.";

    /// <summary>K-554 (04 7.2.8.3): one message for the future date and the date before the item's delivery.</summary>
    public const string ArrivalDateInvalidText = "Ulaşma tarihi ileri bir tarih ya da kalemin teslim tarihinden önce olamaz.";

    public const string ReasonMissingText = "Düzeltmenin sebebini seçin; \"Diğer\" seçildiyse açıklama yazın.";

    public const string EmailMissingText = "Yeni e-posta adresini girin.";

    public const string EmailInvalidText = "Geçerli bir e-posta adresi girin.";

    /// <summary>The obstacle's message of a correction that can not be made (07 UC-150; K-703; K-799).</summary>
    public static string ObstacleTextOf(CorrectionObstacle obstacle) => obstacle switch
    {
        CorrectionObstacle.RefundPending => "Bu iptalin geri ödemesi gönderildi ya da sonucu bekleniyor; para çıkmış olabileceği için iptal düzeltilemez.",
        CorrectionObstacle.StockShort => "İptal edilen adetler için yeterli stok ya da kontenjan kalmadığından iptal düzeltilemez.",
        CorrectionObstacle.ItemRecordExists => "Siparişte kargo, teslim, geri ödeme ya da kalem kaydı olduğundan \"ödendi\" işareti düzeltilemez.",
        CorrectionObstacle.CardPayment => "Kartla ödenen siparişin ödeme durumu düzeltilemez.",
        CorrectionObstacle.PaymentNotPaid => "Ödemesi onaylı olmayan siparişin iptali düzeltilemez.",
        CorrectionObstacle.DigitalItem => "Dijital kalemli siparişte indirme açıldığından \"ödendi\" işareti düzeltilemez.",
        _ => "Bu geçiş düzeltilemez.",
    };

    /// <summary>The correction forms' values (04 9.9.31): the order's current addresses, the transition the status correction takes back and the return receipts.</summary>
    public PanelCorrectionChoices Corrections { get; private set; } = new(null, null, [], []);

    /// <summary>The address forms as they are drawn: the order's values, or the sent ones of a refused correction with its field messages.</summary>
    public AddressFormView? ShippingAddressForm { get; private set; }

    public AddressFormView? BillingAddressForm { get; private set; }

    /// <summary>The reason selection of the status correction (OB-15; 04 7.2.14.3).</summary>
    public ReasonSelectView TransitionReason { get; private set; } = TransitionReasonView(null, null);

    /// <summary>The sent values of a refused carrier, date, transition or e-mail correction, so the form is kept (04 9.9.41).</summary>
    public string? SentDeliveryDate { get; private set; }

    public Guid? SentArrivalRecordId { get; private set; }

    public string? SentArrivalDate { get; private set; }

    public Guid? SentTransitionRecordId { get; private set; }

    public string? SentEmail { get; private set; }

    /// <summary>The element id of the correction form the refusal belongs to; its message is drawn in that form (04 2.3.3, 2.7.3.1).</summary>
    public string? RefusedCorrection { get; private set; }

    /// <summary>The refusal's message of a correction (04 2.3.3): in the form it was sent from, not in the next step's place.</summary>
    public string? CorrectionRefusalText { get; private set; }

    /// <summary>The summary at the head of a refused address form (04 2.3.2); its fields carry their messages.</summary>
    public FormErrorSummary? AddressErrors { get; private set; }

    /// <summary>The refusal's message when it belongs to the form <paramref name="formId"/>; null otherwise.</summary>
    public MessageView? CorrectionMessageFor(string formId) =>
        CorrectionRefusalText is { } text && string.Equals(RefusedCorrection, formId, StringComparison.Ordinal) ? Messages.InPlace(MessageMeaning.Error, text, CorrectionMessageId) : null;

    /// <summary>The order question of a corrected delivery date on a declaration's day (K-608).</summary>
    public bool AsksCorrectionDeliveryOrder { get; private set; }

    /// <summary>The corrected transition's name on the form (04 9.9.31): the step the administrator took, not its code.</summary>
    public static string TransitionNameOf(TransitionCode code) => code switch
    {
        TransitionCode.S3 or TransitionCode.S4 or TransitionCode.S9 => "Firma iptali",
        TransitionCode.S5 => "Kargoya verildi işareti",
        TransitionCode.S6 => "Teslim edildi işareti",
        TransitionCode.S7 => "Teslim edilemedi işareti",
        TransitionCode.S8 => "Kargoya yeniden verme",
        TransitionCode.O1 => "\"Ödendi\" işareti",
        _ => code.ToString(),
    };

    /// <summary>The state a correctable transition goes back to, as the badge names it (04 9.9.31: the form shows it).</summary>
    public static string BackStateOf(PanelCorrectableTransition transition)
    {
        ArgumentNullException.ThrowIfNull(transition);
        return transition.BackToOrderStatus is { } order
            ? StatusBadges.For(order).Text
            : transition.BackToPaymentStatus is { } payment ? StatusBadges.For(payment).Text : string.Empty;
    }

    /// <summary>UC-146: the shipping address with its telephone, or the billing address.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCorrectAddressAsync(Guid id, CorrectAddressInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);

        // Which address is corrected comes as text, as the status correction's reason does: a value that names neither address is this form's refusal, not a binding error (K4).
        if (!Enum.TryParse(input.AddressType, false, out AddressType type) || !Enum.IsDefined(type))
        {
            return await RefuseCorrectionAsync(id, AddressFormIdOf(null), AddressTypeMissingText, ErrorCode.FieldRequired, ct);
        }

        AddressValuesView sent = SentAddressOf(type, input);
        Guid? province = Guid.TryParse(sent.ProvinceId, out Guid parsed) ? parsed : null;
        string form = AddressFormIdOf(type);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelCorrectionResult result = await RunCorrectionAsync(
            (corrections, token) => corrections.CorrectAddressAsync(
                new PanelAddressCorrection(id, admin, type, sent.RecipientName, province, sent.District, sent.AddressLine, sent.Phone, input.Version), token),
            ct);
        return result.Fields.Count > 0
            ? await RefuseCorrectionAsync(id, form, null, ErrorCode.FieldRequired, ct, () => KeepAddress(type, sent, result))
            : await AnswerCorrectionAsync(id, form, result, ct);
    }

    /// <summary>UC-147: the carrier and the tracking number, or the own-vehicle declaration; without them 422 (ShipmentInfoMissing).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCorrectCarrierAsync(Guid id, CorrectCarrierInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        Ship = new MarkShippedInput { Carrier = input.Carrier, OtherCarrierName = input.OtherCarrierName, TrackingNumber = input.TrackingNumber, IsOwnVehicleDelivery = input.IsOwnVehicleDelivery };
        ShipmentInfo shipment = new(input.Carrier, input.OtherCarrierName, input.TrackingNumber, input.IsOwnVehicleDelivery);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelCorrectionResult result = await RunCorrectionAsync(
            (corrections, token) => corrections.CorrectCarrierAsync(new CorrectShipmentRequest(id, admin, shipment, input.Version), token),
            ct);
        return result.Problem == CorrectionValueProblem.Missing
            ? await RefuseCorrectionAsync(id, CarrierFormId, ShipmentMissingText, ErrorCode.ShipmentInfoMissing, ct)
            : await AnswerCorrectionAsync(id, CarrierFormId, result, ct);
    }

    /// <summary>UC-148: the delivery date; UC-126's date states and a date after a recorded return's arrival are 422, an order without the delivery mark is 422, a declaration's day asks the order.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCorrectDeliveryDateAsync(Guid id, CorrectDeliveryDateInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        SentDeliveryDate = input.DeliveryDate;
        if (!DateOnly.TryParseExact(input.DeliveryDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            return await RefuseCorrectionAsync(
                id, DeliveryDateFormId, DeliveryDateMissingText, string.IsNullOrWhiteSpace(input.DeliveryDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate, ct);
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelCorrectionResult result = await RunCorrectionAsync(
            (corrections, token) => corrections.CorrectDeliveryDateAsync(new CorrectDeliveryDateRequest(id, admin, date, input.IsDeliveredBeforeDeclaration, input.Version), token),
            ct);
        if (result.AsksDeliveryOrder)
        {
            AsksCorrectionDeliveryOrder = true;
            DeclarationMomentTexts = [.. result.DeclarationMoments.Select(DisplayFormat.DateAndTime)];
            return await RefuseCorrectionAsync(id, DeliveryDateFormId, DeliveryOrderQuestionText, ErrorCode.DeliverySequenceRequired, ct);
        }

        return await AnswerCorrectionAsync(id, DeliveryDateFormId, result, ct);
    }

    /// <summary>UC-149: a return receipt's arrival date; a future date or one before the item's delivery is 422 (K-554).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCorrectArrivalDateAsync(Guid id, CorrectArrivalDateInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        SentArrivalRecordId = input.ArrivalRecordId;
        SentArrivalDate = input.ArrivalDate;
        string form = ArrivalDateFormId(input.ArrivalRecordId);
        if (!DateOnly.TryParseExact(input.ArrivalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date))
        {
            return await RefuseCorrectionAsync(
                id, form, ArrivalDateMissingText, string.IsNullOrWhiteSpace(input.ArrivalDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate, ct);
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelCorrectionResult result = await RunCorrectionAsync(
            (corrections, token) => corrections.CorrectArrivalDateAsync(new PanelArrivalDateCorrection(id, input.ArrivalRecordId, admin, date, input.Version), token),
            ct);
        return result.Problem == CorrectionValueProblem.Invalid
            ? await RefuseCorrectionAsync(id, form, ArrivalDateInvalidText, ErrorCode.InvalidDate, ct)
            : await AnswerCorrectionAsync(id, form, result, ct);
    }

    /// <summary>UC-150: a wrong transition corrected by a new one with a reason; the state it goes back to is the record's. An obstacle is 422 with its message (K-703).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCorrectTransitionAsync(Guid id, CorrectTransitionInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionReason = TransitionReasonView(input.CorrectionReason, input.ReasonNote);
        SentTransitionRecordId = input.TransitionRecordId;
        CorrectionReason? reason = Enum.TryParse(input.CorrectionReason, false, out CorrectionReason parsed) && Enum.IsDefined(parsed) ? parsed : null;
        TransitionAdmin admin = await AdminAsync(ct);
        PanelCorrectionResult result = await RunCorrectionAsync(
            (corrections, token) => corrections.CorrectTransitionAsync(new CorrectTransitionRequest(id, input.TransitionRecordId, admin, input.Version, reason, input.ReasonNote), token),
            ct);
        if (result.Problem == CorrectionValueProblem.Missing)
        {
            return await RefuseCorrectionAsync(id, TransitionFormId, ReasonMissingText, ErrorCode.FieldRequired, ct);
        }

        return result.Obstacle is { } obstacle
            ? await RefuseCorrectionAsync(id, TransitionFormId, ObstacleTextOf(obstacle), ErrorCode.CorrectionNotPossible, ct)
            : await AnswerCorrectionAsync(id, TransitionFormId, result, ct);
    }

    /// <summary>UC-151: a guest order's e-mail; an empty or invalid address, an account's order and a disposed order are 422.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCorrectOrderEmailAsync(Guid id, CorrectOrderEmailInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        SentEmail = input.ContactEmail;
        TransitionAdmin admin = await AdminAsync(ct);
        PanelCorrectionResult result = await RunCorrectionAsync(
            (corrections, token) => corrections.CorrectOrderEmailAsync(new PanelEmailCorrection(id, admin, input.ContactEmail, input.Version), token),
            ct);
        return result.Problem switch
        {
            CorrectionValueProblem.Missing => await RefuseCorrectionAsync(id, OrderEmailFormId, EmailMissingText, ErrorCode.FieldRequired, ct),
            CorrectionValueProblem.Invalid => await RefuseCorrectionAsync(id, OrderEmailFormId, EmailInvalidText, ErrorCode.InvalidEmail, ct),
            _ => await AnswerCorrectionAsync(id, OrderEmailFormId, result, ct),
        };
    }

    // Called by LoadAsync: the correction forms' values and the two address forms drawn from them.
    private async Task LoadCorrectionsAsync(Guid id, CancellationToken ct)
    {
        Corrections = await HttpContext.RequestServices.GetRequiredService<IPanelOrderCorrections>().ReadChoicesAsync(id, ct) ?? new PanelCorrectionChoices(null, null, [], []);
        if (!IsOpen(OrderOperation.CorrectAddress))
        {
            return;
        }

        IReadOnlyList<ProvinceInfo> provinces = await HttpContext.RequestServices.GetRequiredService<ISettingsModule>().GetProvincesAsync(ct);
        _correctionProvinces = [.. provinces.Select(p => new ProvinceChoice(p.Id, p.Name))];
        ShippingAddressForm = HasPhysicalItem ? AddressForm(AddressVariant.PanelDelivery, ShippingAddressPrefix, Corrections.Shipping) : null;
        BillingAddressForm = AddressForm(AddressVariant.PanelBilling, BillingAddressPrefix, Corrections.Billing);
    }

    private IReadOnlyList<ProvinceChoice> _correctionProvinces = [];

    // The status correction's reason (OB-15; 04 7.2.14.3) under 06 4.19's names: the closed list and "diğer"'s note.
    private static ReasonSelectView TransitionReasonView(string? selected, string? note)
    {
        bool other = string.Equals(selected, Reasons.OtherValue, StringComparison.Ordinal);
        FieldView reason = Fields.ClosedList("Input.CorrectionReason", Reasons.ReasonLabel, FieldRequirement.Required, Reasons.CorrectionOptions, selected);
        FieldView explanation = Fields.TextArea(
            "Input.ReasonNote", Reasons.ExplanationLabel, other ? FieldRequirement.Required : FieldRequirement.Optional, note, hint: Reasons.ExplanationHint);
        return new ReasonSelectView(ReasonKind.StatusCorrection, reason, explanation, false);
    }

    private AddressFormView AddressForm(AddressVariant variant, string prefix, PanelAddressValues? values) =>
        AddressForms.Create(
            variant,
            prefix,
            _correctionProvinces,
            values is null ? null : new AddressValuesView(null, values.RecipientName, values.ProvinceId?.ToString("D"), values.District, values.AddressLine, values.Phone));

    // The form a refused address correction came from; none when the request named no address (its message then heads the section).
    private static string AddressFormIdOf(AddressType? type) => type switch
    {
        AddressType.ShippingAddress => ShippingAddressFormId,
        AddressType.BillingAddress => BillingAddressFormId,
        _ => "correct-address",
    };

    // The sent values of the address the request names: the shipping fields with the telephone, or the billing fields (07 UC-146).
    private static AddressValuesView SentAddressOf(AddressType type, CorrectAddressInput input) =>
        type == AddressType.ShippingAddress
            ? new AddressValuesView(null, input.ShippingRecipientName, input.ShippingProvinceId, input.ShippingDistrict, input.ShippingAddressLine, input.ShippingPhone)
            : new AddressValuesView(null, input.BillingRecipientName, input.BillingProvinceId, input.BillingDistrict, input.BillingAddressLine);

    // A refused address keeps the sent values in the form it came from; its wrong fields are marked with their own messages and the summary heads that form (04 2.3.1.1, 2.3.2, 9.9.41).
    private void KeepAddress(AddressType type, AddressValuesView sent, PanelCorrectionResult result)
    {
        AddressVariant variant = type == AddressType.ShippingAddress ? AddressVariant.PanelDelivery : AddressVariant.PanelBilling;
        string prefix = variant == AddressVariant.PanelDelivery ? ShippingAddressPrefix : BillingAddressPrefix;
        AddressFormView created = AddressForms.Create(variant, prefix, _correctionProvinces, sent);
        AddressForms.AddRefusals(ModelState, prefix, result.Fields);
        AddressErrors = FormErrorSummary.From(ModelState, created.FieldOrder);
        AddressFormView form = created.Bind(AddressErrors);
        if (variant == AddressVariant.PanelDelivery)
        {
            ShippingAddressForm = form;
        }
        else
        {
            BillingAddressForm = form;
        }
    }

    private async Task<IActionResult> AnswerCorrectionAsync(Guid id, string form, PanelCorrectionResult result, CancellationToken ct)
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
                (string text, ErrorCode code) = result switch
                {
                    { Problem: CorrectionValueProblem.Unchanged } => (UnchangedText, ErrorCode.ValueOutOfBounds),
                    { DateObstacle: DeliveryDateObstacle.InTheFuture } => (DeliveryDateFutureText, ErrorCode.InvalidDate),
                    { DateObstacle: DeliveryDateObstacle.BeforeShipment } => (DeliveryDateBeforeShipmentText, ErrorCode.InvalidDate),
                    { DateObstacle: DeliveryDateObstacle.AfterReturnArrival } => (DeliveryDateAfterArrivalText, ErrorCode.InvalidDate),
                    _ => (NotOpenText, ErrorCode.OperationNotAvailable),
                };
                return await RefuseCorrectionAsync(id, form, text, code, ct);
        }
    }

    // The refusal's message in the form it was sent from, the form kept (04 2.3.3, 9.9.41): the order is read again, then the refused address form takes the sent values back over the order's. A refused
    // address field has no form-level message: the summary and the field's own message say it.
    private async Task<IActionResult> RefuseCorrectionAsync(Guid id, string form, string? text, ErrorCode code, CancellationToken ct, Action? keep = null)
    {
        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }

        keep?.Invoke();
        Reject(code);
        RefusedCorrection = form;
        CorrectionRefusalText = text;
        return FormResults.Redisplay(this);
    }

    // The page opens the operation's transaction (09 8.3 item 3); the module's method locks, compares the version, asks the rule and writes. A rejected correction rolls it back.
    private async Task<PanelCorrectionResult> RunCorrectionAsync(Func<IPanelOrderCorrections, CancellationToken, Task<PanelCorrectionResult>> correction, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        IPanelOrderCorrections corrections = services.GetRequiredService<IPanelOrderCorrections>();
        OperationRunner runner = services.GetRequiredService<OperationRunner>();
        PanelCorrectionResult? outcome = null;
        await runner.RunAsync(
            async token =>
            {
                outcome = await correction(corrections, token);
                return outcome.Gate;
            },
            ct);
        return outcome!; // non-null: the runner runs the correction once before it returns
    }
}

/// <summary>
/// UC-146 <c>CorrectAddressInput</c>: the shipping address's four fields with its telephone and the billing address's four fields (06 4.17 <c>Shipping…</c>, <c>Billing…</c>), which of the two the form
/// corrects (D-1527; text, so a value that names neither is the form's refusal) and the version. Each form sends its own names, so the two forms on the page have their own element ids (04 2.3.1.1).
/// </summary>
public sealed class CorrectAddressInput : IFormInput
{
    public string? AddressType { get; set; }

    public string? ShippingRecipientName { get; set; }

    public string? ShippingProvinceId { get; set; }

    public string? ShippingDistrict { get; set; }

    public string? ShippingAddressLine { get; set; }

    public string? ShippingPhone { get; set; }

    public string? BillingRecipientName { get; set; }

    public string? BillingProvinceId { get; set; }

    public string? BillingDistrict { get; set; }

    public string? BillingAddressLine { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-147 <c>CorrectCarrierInput</c>: the carrier fields of 9.9.3 (06 4.17) and the version.</summary>
public sealed class CorrectCarrierInput : IFormInput
{
    public string? Carrier { get; set; }

    public string? OtherCarrierName { get; set; }

    public string? TrackingNumber { get; set; }

    public bool IsOwnVehicleDelivery { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-148 <c>CorrectDeliveryDateInput</c>: the delivery date (text, so a wrong value is a refusal), the order question's answer and the version.</summary>
public sealed class CorrectDeliveryDateInput : IFormInput
{
    public string? DeliveryDate { get; set; }

    public bool? IsDeliveredBeforeDeclaration { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-149 <c>CorrectArrivalDateInput</c>: the receipt's record (06 4.26), the arrival date and the version.</summary>
public sealed class CorrectArrivalDateInput : IFormInput
{
    [SpecAssignedName]
    public Guid ArrivalRecordId { get; set; }

    public string? ArrivalDate { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-150 <c>CorrectTransitionInput</c>: the corrected record (06 4.19), the reason from the closed list, "diğer"'s note and the version; the state to go back to is not sent.</summary>
public sealed class CorrectTransitionInput : IFormInput
{
    public Guid TransitionRecordId { get; set; }

    public string? CorrectionReason { get; set; }

    public string? ReasonNote { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-151 <c>CorrectOrderEmailInput</c>: the new contact e-mail (06 4.17 <c>ContactEmail</c>) and the version.</summary>
public sealed class CorrectOrderEmailInput : IFormInput
{
    public string? ContactEmail { get; set; }

    public int Version { get; set; }
}
