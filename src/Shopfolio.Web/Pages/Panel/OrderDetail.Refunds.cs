using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// E-37's refund steps (T529; 04 9.9.21-9.9.24; 07 UC-131...UC-137): the refund lines the server worked out - each with its amount, its IBAN on the transfer and the steps open now - and the seven
/// handlers. The page works out no amount and no state (04 9.9.37; 09 8.3 item 5): it carries the line's key, the version and the confirmation, and turns the module's answer into 303, 200 (the
/// confirmation window that repeats the amount and the IBAN, K-797), 404, 409 or 422 with the step's message. Every handler is a gated operation (D-153).
/// </summary>
public sealed partial class OrderDetailModel
{
    public const string RefundSectionTitle = "Geri ödemeler";

    public const string ProcessRefundLabel = "Geri ödemeyi işle";

    public const string ReportTransferFailedLabel = "Havale gerçekleşmedi";

    public const string AmountBasedRefundLabel = "Tutar bazlı geri ödeme yap";

    public const string RequestRefundIbanLabel = "Müşteriden IBAN iste";

    public const string RetryCardRefundLabel = "Kart iadesini yeniden dene";

    public const string SwitchRefundToTransferLabel = "Havale yolunu aç";

    public const string MarkCardRefundFailedLabel = "Gerçekleşmedi olarak işaretle";

    /// <summary>04 9.9.21 (K-686): on the transfer line without an IBAN the button's place says the IBAN request is awaited.</summary>
    public const string IbanAwaitedText = "Müşterinin IBAN'ı bekleniyor; geri ödeme IBAN girildikten sonra işlenir.";

    /// <summary>07 UC-131, UC-133 (<c>RefundIbanMissing</c>): the transfer line has no IBAN on file.</summary>
    public const string RefundIbanMissingText = "Müşterinin IBAN'ı yok; önce müşteriden IBAN isteyin.";

    /// <summary>07 UC-133 (<c>AmountBasedRefundExceeded</c>; 04 7.2.13.1): the field's message.</summary>
    public const string AmountExceededText = "Tutar sıfırdan büyük olmalı ve siparişte ödenmiş, henüz geri ödenmemiş tutarı aşmamalı.";

    public const string AmountMissingText = "Geri ödenecek tutarı girin.";

    public const string AmountInvalidText = "Tutarı rakamla ve en çok iki ondalık haneyle girin.";

    /// <summary>07 UC-134 (04 7.2.13.3): the IBAN request names an item.</summary>
    public const string ItemNotSelectedText = "IBAN istenecek kalemi seçin.";

    /// <summary>07 UC-131 (<c>RefundIbanChanged</c>; 03 3.5.2.15): the customer corrected the IBAN while the window was open.</summary>
    public const string RefundIbanChangedText = "Müşteri IBAN'ını düzeltti; geri ödeme işlenmedi. Güncel IBAN aşağıda.";

    /// <summary>K-798 (4): the "geri ödeme gerçekleşmedi" warning, in the step's place before the window (04 9.9.24; K-705, K-612).</summary>
    public const string CardRefundNotRealizedWarning =
        "Kart iadesi ödeme sağlayıcısında gerçekleşmedi. Yeniden denemeden ya da havale yolunu açmadan önce sağlayıcının panelinde iadenin gerçekleşip gerçekleşmediğine ve ters ibraz olup olmadığına bakın; para müşteriye zaten döndüyse ikinci ödemenin sonucu firmadadır.";

    /// <summary>04 9.9.4, 9.9.24 (K-940): the "geri ödeme sonucu bekleniyor" warning above "Gerçekleşmedi olarak işaretle".</summary>
    public const string CardRefundPendingWarning = "Kart iadesinin sonucu ödeme sağlayıcısından bekleniyor; iade sağlayıcıda gerçekleşmiş olabilir. İşaretlemeden önce sağlayıcının panelinde bakın.";

    /// <summary>04 9.9.8 (K-978): the lasting warning of a success that came after the firm's "gerçekleşmedi" mark.</summary>
    public const string LateRefundSuccessWarning = "Bu iade gerçekleşmedi olarak işaretlendikten sonra ödeme sağlayıcısında gerçekleşti; aynı para müşteriye ikinci kez gönderilmiş olabilir.";

    /// <summary>The order's refund lines (04 9.9.6, 9.9.21-9.9.24), with the steps open now - the module's one decision.</summary>
    public IReadOnlyList<PanelRefundLine> RefundLines { get; private set; } = [];

    /// <summary>The amount based refund form as sent, kept on a refusal (04 9.9.41).</summary>
    public AmountBasedRefundInput AmountBased { get; private set; } = new();

    /// <summary>The name of a refund line's item, or the order's own line (a late payment's refund, an itemless amount based refund).</summary>
    public string RefundLineNameOf(PanelRefundLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return line.OrderItemId is { } item ? ItemNameOf(item) : line.Type == RefundType.LatePayment ? "Geç gelen ödemenin geri ödemesi" : "Sipariş";
    }

    /// <summary>The refund line's way in words (04 9.9.8).</summary>
    public static string WayTextOf(PanelRefundLine line)
    {
        ArgumentNullException.ThrowIfNull(line);
        return line.Way == PaymentMethod.Card ? "Kart" : "Havale";
    }

    /// <summary>UC-131: the refund the line's record causes; unconfirmed, the window repeats the amount and - on the transfer - the IBAN (200); confirmed 303.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostProcessRefundAsync(Guid id, RefundStepInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Key is not { } key)
        {
            return NotFound();
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelRefundStepResult result = await RunRefundStepAsync((steps, token) => steps.ProcessRefundAsync(new PanelRefundRequest(id, key, admin, input.Version, input.Confirmed), token), ct);
        if (result.Refusal == TransitionRefusal.ConfirmationRequired)
        {
            return await AskAsync(id, () => ProcessRefundConfirm(id, input, result), ct);
        }

        if (result.Refusal == TransitionRefusal.VersionConflict && input.ShownIban is { Length: > 0 } shown)
        {
            if (!await LoadAsync(id, ct))
            {
                return NotFound();
            }

            // 03 3.5.2.15: the customer corrected the IBAN while the window was open - the conflict names it and the screen shows the IBAN as it is now.
            string? now = RefundLines.FirstOrDefault(l => l.Key == key)?.Iban;
            Reject(now is not null && now != shown ? ErrorCode.RefundIbanChanged : ErrorCode.OrderVersionConflict);
            IsConflict = true;
            RefusalText = now is not null && now != shown ? RefundIbanChangedText : null;
            return FormResults.Conflict(this);
        }

        return await AnswerRefundAsync(id, result, ct);
    }

    /// <summary>UC-132: "havale gerçekleşmedi" - no confirmation; a processed refund is 422.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostMarkBankTransferRefundFailedAsync(Guid id, RefundStepInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.Key is not { } key)
        {
            return NotFound();
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelRefundStepResult result = await RunRefundStepAsync((steps, token) => steps.MarkBankTransferRefundFailedAsync(new PanelRefundRequest(id, key, admin, input.Version), token), ct);
        return await AnswerRefundAsync(id, result, ct);
    }

    /// <summary>UC-133: the amount based partial refund; the amount is the administrator's, the cap the server's (K-1078); unconfirmed 200, confirmed 303.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostProcessAmountBasedRefundAsync(Guid id, AmountBasedRefundInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        AmountBased = input;
        if (string.IsNullOrWhiteSpace(input.Amount))
        {
            return await RefuseAsync(id, AmountMissingText, ErrorCode.FieldRequired, ct);
        }

        if (!RequestFormat.TryParseDecimal(input.Amount.Trim(), out decimal amount))
        {
            return await RefuseAsync(id, AmountInvalidText, ErrorCode.InvalidAmount, ct);
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelRefundStepResult result = await RunRefundStepAsync(
            (steps, token) => steps.ProcessAmountBasedRefundAsync(new AmountBasedRefundRequest(id, amount, input.OrderItemId, admin, input.Version, input.Confirmed), token),
            ct);
        return result.Refusal == TransitionRefusal.ConfirmationRequired
            ? await AskAsync(id, () => AmountBasedConfirm(id, input, result), ct)
            : await AnswerRefundAsync(id, result, ct);
    }

    /// <summary>UC-134: the IBAN request on an item; the reason is the server's (K-1201), no confirmation.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostRequestRefundIbanAsync(Guid id, RequestRefundIbanInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelRefundStepResult result = await RunRefundStepAsync(
            (steps, token) => steps.RequestRefundIbanAsync(new PanelRefundRequest(id, input.OrderItemId ?? Guid.Empty, admin, input.Version), token),
            ct);
        return await AnswerRefundAsync(id, result, ct);
    }

    /// <summary>UC-135: a new card refund request; confirmed (03 1.6.1.4) with K-798's warning; unconfirmed 200, confirmed 303.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostRetryCardRefundAsync(Guid id, CardRefundStepInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelRefundStepResult result = await RunRefundStepAsync(
            (steps, token) => steps.RetryCardRefundAsync(new PanelRefundRequest(id, input.RefundId ?? Guid.Empty, admin, input.Version, input.Confirmed), token),
            ct);
        return result.Refusal == TransitionRefusal.ConfirmationRequired
            ? await AskAsync(id, () => RetryConfirm(id, input, result), ct)
            : await AnswerRefundAsync(id, result, ct);
    }

    /// <summary>UC-136: the transfer path of a failed card refund; no confirmation.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostSwitchRefundToBankTransferAsync(Guid id, CardRefundStepInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelRefundStepResult result = await RunRefundStepAsync(
            (steps, token) => steps.SwitchRefundToBankTransferAsync(new PanelRefundRequest(id, input.RefundId ?? Guid.Empty, admin, input.Version), token),
            ct);
        return await AnswerRefundAsync(id, result, ct);
    }

    /// <summary>UC-137: the "gerçekleşmedi" mark on a card refund whose result is awaited; no confirmation (04 2.4.2), no notice.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostMarkCardRefundFailedAsync(Guid id, CardRefundStepInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelRefundStepResult result = await RunRefundStepAsync(
            (steps, token) => steps.MarkCardRefundFailedAsync(new PanelRefundRequest(id, input.RefundId ?? Guid.Empty, admin, input.Version), token),
            ct);
        return await AnswerRefundAsync(id, result, ct);
    }

    private async Task LoadRefundLinesAsync(Guid id, CancellationToken ct) =>
        RefundLines = await HttpContext.RequestServices.GetRequiredService<IPanelRefundSteps>().ReadLinesAsync(id, ct);

    // The named refusals carry their own code and message (07 6.2.7); the rest is the line steps' one answer (409, 404, 422 "açık değil").
    private async Task<IActionResult> AnswerRefundAsync(Guid id, PanelRefundStepResult result, CancellationToken ct) => result.Obstacle switch
    {
        PanelRefundObstacle.IbanMissing => await RefuseAsync(id, RefundIbanMissingText, ErrorCode.RefundIbanMissing, ct),
        PanelRefundObstacle.AmountExceeded => await RefuseAsync(id, AmountExceededText, ErrorCode.AmountBasedRefundExceeded, ct),
        PanelRefundObstacle.ItemNotSelected => await RefuseAsync(id, ItemNotSelectedText, ErrorCode.FieldRequired, ct),
        _ => await AnswerAsync(id, new PanelStepResult(result.Refusal), ct),
    };

    // The page opens the operation's transaction (09 8.3 item 3); the module's write method locks, compares the version, asks the rule and writes. A rejected step rolls it back.
    private async Task<PanelRefundStepResult> RunRefundStepAsync(Func<IPanelRefundSteps, CancellationToken, Task<PanelRefundStepResult>> step, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        IPanelRefundSteps steps = services.GetRequiredService<IPanelRefundSteps>();
        OperationRunner runner = services.GetRequiredService<OperationRunner>();
        PanelRefundStepResult? outcome = null;
        await runner.RunAsync(
            async token =>
            {
                outcome = await step(steps, token);
                return outcome.Gate;
            },
            ct);
        return outcome!; // non-null: the runner runs the step once before it returns
    }

    private static ConfirmView ProcessRefundConfirm(Guid id, RefundStepInput input, PanelRefundStepResult result)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("Input." + input.KeyName, input.Key?.ToString("D") ?? string.Empty),
            Field("Input.Version", input.Version),
            new("Input.ShownIban", result.Iban ?? string.Empty),
            new("Input.Confirmed", "true"),
        ];
        return Confirmations.ForIrreversible(
            IrreversibleConfirmations.RefundProcessing(result.Amount ?? 0m, result.Iban), ProcessRefundLabel, ProcessRefundLabel, PathOf(id), "ProcessRefund", PathOf(id), fields);
    }

    private static ConfirmView AmountBasedConfirm(Guid id, AmountBasedRefundInput input, PanelRefundStepResult result)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("Input.Amount", input.Amount ?? string.Empty),
            new("Input.OrderItemId", input.OrderItemId?.ToString("D") ?? string.Empty),
            Field("Input.Version", input.Version),
            new("Input.Confirmed", "true"),
        ];
        return Confirmations.ForIrreversible(
            IrreversibleConfirmations.RefundProcessing(result.Amount ?? 0m, result.Iban), AmountBasedRefundLabel, AmountBasedRefundLabel, PathOf(id), "ProcessAmountBasedRefund", PathOf(id), fields);
    }

    private static ConfirmView RetryConfirm(Guid id, CardRefundStepInput input, PanelRefundStepResult result)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("Input.RefundId", input.RefundId?.ToString("D") ?? string.Empty),
            Field("Input.Version", input.Version),
            new("Input.Confirmed", "true"),
        ];
        IrreversibleConfirmation texts = IrreversibleConfirmations.RefundProcessing(result.Amount ?? 0m, null) with { Warning = CardRefundNotRealizedWarning };
        return Confirmations.ForIrreversible(texts, RetryCardRefundLabel, RetryCardRefundLabel, PathOf(id), "RetryCardRefund", PathOf(id), fields);
    }
}

/// <summary>
/// UC-131, UC-132 <c>ProcessRefundInput</c> · <c>MarkBankTransferRefundFailedInput</c>: exactly one of the links of 06 4.33 - the record that causes the refund (07 9.9.10) - or the refund's own id
/// for a refund that has none; the version, the confirmation and the IBAN the window showed.
/// </summary>
public sealed class RefundStepInput : IFormInput
{
    public Guid? CancellationId { get; set; }

    public Guid? ItemRemovalId { get; set; }

    public Guid? LateDeliveryTerminationId { get; set; }

    public Guid? WithdrawalId { get; set; }

    public Guid? ReturnId { get; set; }

    public Guid? DefectClaimId { get; set; }

    public Guid? PaymentRecordId { get; set; }

    public Guid? RefundId { get; set; }

    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }

    /// <summary>The IBAN the confirmation window repeated (03 3.5.2.15): a change meanwhile is <c>RefundIbanChanged</c>; not a field of 06.</summary>
    [SpecAssignedName]
    public string? ShownIban { get; set; }

    /// <summary>The one link the request carries; null when none or more than one is given (07 UC-131: exactly one - otherwise 404).</summary>
    public Guid? Key
    {
        get
        {
            Guid?[] given = [.. Links().Select(l => l.Value).Where(v => v is not null)];
            return given.Length == 1 ? given[0] : null;
        }
    }

    /// <summary>The field name of <see cref="Key"/> (<see cref="PanelRefundKeys"/>), for the confirmation window's hidden field.</summary>
    public string KeyName => Links().FirstOrDefault(l => l.Value is not null).Name ?? PanelRefundKeys.RefundId;

    private IEnumerable<(string Name, Guid? Value)> Links() =>
    [
        (PanelRefundKeys.CancellationId, CancellationId),
        (PanelRefundKeys.ItemRemovalId, ItemRemovalId),
        (PanelRefundKeys.LateDeliveryTerminationId, LateDeliveryTerminationId),
        (PanelRefundKeys.WithdrawalId, WithdrawalId),
        (PanelRefundKeys.ReturnId, ReturnId),
        (PanelRefundKeys.DefectClaimId, DefectClaimId),
        (PanelRefundKeys.PaymentRecordId, PaymentRecordId),
        (PanelRefundKeys.RefundId, RefundId),
    ];
}

/// <summary>UC-133 <c>ProcessAmountBasedRefundInput</c>: the amount (text, so a wrong value is a refusal and not a binding error; 07 2.9.4), the optional item, the version and the confirmation.</summary>
public sealed class AmountBasedRefundInput : IFormInput
{
    public string? Amount { get; set; }

    public Guid? OrderItemId { get; set; }

    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>UC-134 <c>RequestRefundIbanInput</c>: the item and the version; the reason is the server's (K-1201).</summary>
public sealed class RequestRefundIbanInput : IFormInput
{
    public Guid? OrderItemId { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-135...UC-137 <c>RetryCardRefundInput</c> · <c>SwitchRefundToBankTransferInput</c> · <c>MarkCardRefundFailedInput</c>: the refund, the version and - on the retry - the confirmation.</summary>
public sealed class CardRefundStepInput : IFormInput
{
    public Guid? RefundId { get; set; }

    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}
