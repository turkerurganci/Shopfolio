using System.Globalization;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// E-37's item steps (T528; 04 9.9.19, 9.9.20, 9.9.25, 9.9.27, 9.9.28, 9.9.30; 07 UC-129, UC-130, UC-138, UC-140...UC-142, UC-144, UC-145): the firm's cancellation, the record of a notice from
/// another channel, the removal, the receipt and the rejection of returned goods, "Stoğa ekle", the "mal dönmedi" closure and the defect claim's two steps. The page works out no rule (09 8.3 item 5):
/// it reads the form, hands it to <see cref="IPanelItemSteps"/> inside the operation's transaction and turns the answer into 303, 200 (the confirmation window, K-797), 404, 409 or 422 with the
/// step's message and code (07 6.2.7). Every handler is a gated operation (D-153).
/// </summary>
public sealed partial class OrderDetailModel
{
    private static readonly InvalidIbanAttribute NoticeIbanRule = new();

    public const string CancelByCompanyLabel = "Siparişi iptal et";

    public const string RecordWithdrawalLabel = "Cayma bildirimini kaydet";

    public const string RecordLateTerminationLabel = "Gecikme feshi bildirimini kaydet";

    public const string RemoveItemLabel = "Kalemi çıkar";

    public const string ReceiveReturnLabel = "İade malını teslim al";

    public const string RejectReturnLabel = "İadeyi reddet — koruyucu ambalaj açılmış";

    public const string RestockReturnLabel = "Stoğa ekle";

    public const string CloseNotReturnedLabel = "Mal dönmedi olarak kapat";

    public const string ResolveDefectClaimLabel = "Çözüldü olarak işaretle";

    public const string ReopenDefectClaimLabel = "Yeniden aç";

    /// <summary>K-798 (6), K-607: the reminder at the head of the record of a notice from another channel and in the "müşteriyle anlaşıldı" cancellation's window.</summary>
    public const string OwnerCheckReminderText =
        "Bildirim siparişin e-posta adresinden gelmediyse, kaydetmeden önce siparişin e-posta adresine yazarak ya da teslimat telefonunu arayarak siparişin sahibiyle teyit edin. Sipariş numarası tek başına teyit değildir.";

    public const string ReasonIncompleteText = "Bir sebep seçin; \"Diğer\" seçildiyse açıklamayı yazın.";

    public const string ItemNotChosenText = "İşlem yapılacak kalemi seçin.";

    public const string ItemChoiceInvalidText = "Kalem seçimi geçersiz; her kalem bir kez seçilebilir. İstek uygulanmadı.";

    public const string QuantityInvalidText = "Adet birden küçük olmayan bir tam sayı olmalı.";

    public const string QuantityExceedsOpenText = "Adet bu işleme açık adedi aşıyor; istek uygulanmadı.";

    public const string NoticeDateMissingText = "Bildirimin firmaya ulaştığı tarihi girin.";

    /// <summary>07 UC-130 <c>DeclarationOutsideWindow</c>: a date in the future, or a late delivery termination outside its range.</summary>
    public const string NoticeDateInvalidText = "Bildirimin tarihi ileri olamaz; gecikme feshinde süre dolmuş ve teslim edilmemiş açık kalem bulunmalı.";

    public const string NoticeIbanInvalidText = "IBAN'ın biçimi ya da sağlama basamağı tutmuyor.";

    public const string ExceptionUnacknowledgedText = "Bildirim cayma süresinin dışında ya da kalem mutlak istisnalı; kaydetmek için ayrı onay kutusunu işaretleyin.";

    public const string NotShippedItemText = "Kalem kargoya verilmemiş; müşterinin vazgeçmesi \"Müşteriyle anlaşıldı (müşteri talebi)\" sebebiyle firma iptalidir.";

    public const string ReturnRejectionNotAllowedText = "Ret yalnız koşullu istisnalı kalemde, koruyucu ambalajı açılmış malda seçilebilir.";

    /// <summary>The withdrawal and return facts the receipt, "Stoğa ekle" and the closure act on (04 9.9.27, 9.9.28).</summary>
    public ReturnStepFacts ReturnSteps { get; private set; } = ReturnStepFacts.Empty;

    /// <summary>The warnings of the notice record that the last POST found (K-709); none before a POST.</summary>
    public WithdrawalNoticeWarnings NoticeWarnings { get; private set; } = WithdrawalNoticeWarnings.None;

    /// <summary>True when the notice record asks the order of the delivery and the declaration (K-608).</summary>
    public bool AsksNoticeDeliveryOrder { get; private set; }

    /// <summary>The withdrawal record as sent, kept on a refusal (04 9.9.41).</summary>
    public RecordWithdrawalInput Notice { get; private set; } = new();

    /// <summary>Today in Turkey as the date fields' upper bound (OB-16: no date in the future).</summary>
    public string TodayText => TurkeyTime.DateOf(Now).ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);

    /// <summary>The item's returns not yet added to the stock (04 9.9.27 "Stoğa ekle").</summary>
    public IReadOnlyList<ReturnReceiptFact> RestockableOf(OrderPageItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return [.. ReturnSteps.Receipts.Where(r => r.OrderItemId == item.OrderItemId && r.StockAddedAt is null)];
    }

    /// <summary>The item's units still on the way back (the receipt's and the closure's default; K-794).</summary>
    public int AwaitedUnitsOf(OrderPageItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        return ReturnSteps.Withdrawals.Where(w => w.OrderItemId == item.OrderItemId).Sum(w => w.Awaiting);
    }

    /// <summary>UC-129: the firm's cancellation; unconfirmed 200 with the window (K-797), confirmed 303.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCancelOrderByCompanyAsync(Guid id, CancelOrderByCompanyInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!Enum.TryParse(input.CancellationReason, false, out CancellationReason reason) || !Enum.IsDefined(reason) || int.TryParse(input.CancellationReason, out _))
        {
            return await RefuseAsync(id, ReasonIncompleteText, string.IsNullOrWhiteSpace(input.CancellationReason) ? ErrorCode.FieldRequired : ErrorCode.InvalidListValue, ct);
        }

        if (!TryReadUnits(input.Items.Select(l => (l.OrderItemId, l.Quantity)), out IReadOnlyList<PanelItemUnits> units))
        {
            return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);
        }

        // 04 7.2.9.3: the note belongs to "Diğer"; "müşteriyle anlaşıldı" carries the owner check the window reminds of (K-607).
        string? note = reason == CancellationReason.Other ? input.ReasonNote : null;
        TransitionAdmin admin = await AdminAsync(ct);
        PanelItemStepResult result = await RunItemStepAsync(
            (steps, token) => steps.CancelByFirmAsync(
                new PanelFirmCancellation(id, admin, input.Version, input.Confirmed, reason, note, reason == CancellationReason.AgreedWithBuyer, units), token),
            ct);
        return result.Refusal == TransitionRefusal.ConfirmationRequired
            ? await AskAsync(id, () => CancelConfirm(id, input, reason, units, result.IsTransferLine), ct)
            : await AnswerItemStepAsync(id, result, ErrorCode.OperationNotAvailable, ct);
    }

    /// <summary>UC-130: a withdrawal notified by another channel, on one item and its units; unconfirmed 200, confirmed 303.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostRecordWithdrawalAsync(Guid id, RecordWithdrawalInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        Notice = input;
        if (!TryReadDate(input.DeclarationDate, out DateOnly date))
        {
            return await RefuseAsync(id, NoticeDateMissingText, string.IsNullOrWhiteSpace(input.DeclarationDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate, ct);
        }

        if (!TryReadUnits(input.Items.Select(l => (l.OrderItemId, l.Quantity)), out IReadOnlyList<PanelItemUnits> units))
        {
            return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);
        }

        // One record is one item (06 4.25); two rows of a hand-made request are the item choice's refusal (07 5.1.5).
        if (units.Count > 1)
        {
            return await RefuseAsync(id, ItemChoiceInvalidText, ErrorCode.InvalidListValue, ct);
        }

        PanelItemUnits chosen = units.Count == 1 ? units[0] : new PanelItemUnits(Guid.Empty, 0);
        string? iban = string.IsNullOrWhiteSpace(input.Iban) ? null : input.Iban.Trim();
        TransitionAdmin admin = await AdminAsync(ct);
        PanelItemStepResult result = await RunItemStepAsync(
            (steps, token) => steps.RecordWithdrawalAsync(
                new RecordWithdrawalRequest(
                    id, chosen.OrderItemId, admin, input.Version, input.Confirmed, units.Count == 1 ? chosen.Quantity : 1, date, input.IsFromContactEmail, iban, input.IsDeliveredBeforeDeclaration,
                    input.IsExceptionAcknowledged),
                token),
            ct);
        NoticeWarnings = result.Warnings;
        AsksNoticeDeliveryOrder = result.Warnings.AsksDeliveryOrder;
        return result.Refusal == TransitionRefusal.ConfirmationRequired
            ? await AskAsync(id, () => WithdrawalConfirm(id, input, chosen), ct)
            : await AnswerItemStepAsync(id, result, ErrorCode.OperationNotAvailable, ct);
    }

    /// <summary>UC-130: a late delivery termination notified by another channel; no item and no unit (K-796); unconfirmed 200, confirmed 303.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostRecordLateDeliveryTerminationAsync(Guid id, RecordLateDeliveryTerminationInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!TryReadDate(input.DeclarationDate, out DateOnly date))
        {
            return await RefuseAsync(id, NoticeDateMissingText, string.IsNullOrWhiteSpace(input.DeclarationDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate, ct);
        }

        // OB-14: a given IBAN passes the check; the termination's IBAN home is not wired yet (D-1590) - the transfer line asks the customer (K-706).
        if (input.IsFromContactEmail && !string.IsNullOrWhiteSpace(input.Iban) && !NoticeIbanRule.Allows(input.Iban.Trim()))
        {
            return await RefuseAsync(id, NoticeIbanInvalidText, ErrorCode.InvalidIban, ct);
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelItemStepResult result = await RunItemStepAsync(
            (steps, token) => steps.RecordLateDeliveryTerminationAsync(TerminateForLateDeliveryRequest.FromOtherChannel(id, admin, input.Version, input.Confirmed, date), token),
            ct);
        return result.Refusal == TransitionRefusal.ConfirmationRequired
            ? await AskAsync(id, () => TerminationConfirm(id, input, date), ct)
            : await AnswerItemStepAsync(id, result, ErrorCode.DeclarationOutsideWindow, ct);
    }

    /// <summary>UC-138: the removal or the reduction of an item; the confirmation only on the card line (03 1.6.1.5).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostRemoveOrderItemAsync(Guid id, RemoveOrderItemInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.OrderItemId is not { } itemId)
        {
            return await RefuseAsync(id, ItemNotChosenText, ErrorCode.FieldRequired, ct);
        }

        if (!TryReadQuantity(input.Quantity, out int quantity))
        {
            return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelItemStepResult result = await RunItemStepAsync(
            (steps, token) => steps.RemoveItemAsync(new RemoveItemRequest(id, itemId, admin, input.Version, quantity, input.Confirmed), token),
            ct);
        return result.Refusal == TransitionRefusal.ConfirmationRequired
            ? await AskAsync(id, () => RemovalConfirm(id, input, itemId, quantity), ct)
            : await AnswerItemStepAsync(id, result, ErrorCode.OperationNotAvailable, ct);
    }

    /// <summary>UC-140: the receipt of returned goods; every receipt its own record, no refund starts; 303.</summary>
    [GatedOperation]
    public Task<IActionResult> OnPostReceiveReturnAsync(Guid id, ReceiveReturnInput input, CancellationToken ct) => ArriveAsync(id, input, false, ct);

    /// <summary>UC-140: the rejection of returned goods - only on a conditional exemption's item; unconfirmed 200, confirmed 303.</summary>
    [GatedOperation]
    public Task<IActionResult> OnPostRejectReturnAsync(Guid id, ReceiveReturnInput input, CancellationToken ct) => ArriveAsync(id, input, true, ct);

    /// <summary>UC-141: "Stoğa ekle" of one receipt - its units, once; no confirmation.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostRestockReturnAsync(Guid id, RestockReturnInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.ReturnId is not { } returnId)
        {
            return NotFound();
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelItemStepResult result = await RunItemStepAsync((steps, token) => steps.RestockReturnAsync(id, returnId, admin, input.Version, token), ct);
        return await AnswerItemStepAsync(id, result, ErrorCode.OperationNotAvailable, ct);
    }

    /// <summary>UC-142: the "mal dönmedi" closure after Z-42; the quantity defaults to the units not come back; no confirmation, no refund (K-596).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCloseNotReturnedAsync(Guid id, CloseNotReturnedInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.OrderItemId is not { } itemId)
        {
            return await RefuseAsync(id, ItemNotChosenText, ErrorCode.FieldRequired, ct);
        }

        int? quantity = null;
        if (!string.IsNullOrWhiteSpace(input.Quantity))
        {
            if (!TryReadQuantity(input.Quantity, out int read))
            {
                return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);
            }

            quantity = read;
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelItemStepResult result = await RunItemStepAsync((steps, token) => steps.CloseNotReturnedAsync(id, itemId, quantity, admin, input.Version, token), ct);
        return await AnswerItemStepAsync(id, result, ErrorCode.OperationNotAvailable, ct);
    }

    /// <summary>UC-144: an open defect claim to "Çözüldü"; no confirmation, no notice.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostResolveDefectClaimAsync(Guid id, ResolveDefectClaimInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelItemStepResult result = await RunItemStepAsync(
            (steps, token) => steps.ResolveDefectClaimAsync(id, input.DefectClaimId ?? Guid.Empty, admin, input.Version, token), ct);
        return await AnswerItemStepAsync(id, result, ErrorCode.OperationNotAvailable, ct);
    }

    /// <summary>UC-145: a resolved defect claim back to "Açık" inside Z-18; never a new claim (K-676).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostReopenDefectClaimAsync(Guid id, AdminReopenDefectClaimInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelItemStepResult result = await RunItemStepAsync(
            (steps, token) => steps.ReopenDefectClaimAsync(id, input.DefectClaimId ?? Guid.Empty, admin, input.Version, token), ct);
        return await AnswerItemStepAsync(id, result, ErrorCode.OperationNotAvailable, ct);
    }

    private async Task LoadReturnStepsAsync(CancellationToken ct) =>
        ReturnSteps = await _afterSales.ReturnStepFactsAsync([.. Detail.Items.Select(i => i.OrderItemId)], ct);

    private async Task<IActionResult> ArriveAsync(Guid id, ReceiveReturnInput input, bool isRejection, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (input.OrderItemId is not { } itemId)
        {
            return await RefuseAsync(id, ItemNotChosenText, ErrorCode.FieldRequired, ct);
        }

        if (!TryReadDate(input.ArrivalDate, out DateOnly arrival))
        {
            return await RefuseAsync(id, ArrivalDateMissingText, string.IsNullOrWhiteSpace(input.ArrivalDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate, ct);
        }

        int? quantity = null;
        if (!string.IsNullOrWhiteSpace(input.Quantity))
        {
            if (!TryReadQuantity(input.Quantity, out int read))
            {
                return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);
            }

            quantity = read;
        }

        TransitionAdmin admin = await AdminAsync(ct);
        PanelReturnArrival request = new(id, itemId, admin, input.Version, quantity, arrival, input.Confirmed);
        PanelItemStepResult result = await RunItemStepAsync(
            (steps, token) => isRejection ? steps.RejectReturnAsync(request, token) : steps.ReceiveReturnAsync(request, token), ct);
        if (result.Refusal == TransitionRefusal.ConfirmationRequired)
        {
            return await AskAsync(id, () => RejectionConfirm(id, input, quantity), ct);
        }

        return await AnswerItemStepAsync(id, result, isRejection ? ErrorCode.ReturnRejectionNotAllowed : ErrorCode.OperationNotAvailable, ct, ErrorCode.InvalidDate);
    }

    // The named obstacles carry their own code and message (07 6.2.7); the rest is the page's one answer (303, 404, 409). A closed step without a named obstacle answers with the step's
    // own "not open" code: UC-130's termination outside its range is DeclarationOutsideWindow, UC-140's rejection outside the conditional exemption ReturnRejectionNotAllowed.
    private async Task<IActionResult> AnswerItemStepAsync(Guid id, PanelItemStepResult result, ErrorCode notOpenCode, CancellationToken ct, ErrorCode dateCode = ErrorCode.DeclarationOutsideWindow)
    {
        switch (result.Obstacle)
        {
            case PanelItemObstacle.ReasonIncomplete:
                return await RefuseAsync(id, ReasonIncompleteText, ErrorCode.FieldRequired, ct);
            case PanelItemObstacle.ItemNotSelected:
                return await RefuseAsync(id, ItemNotChosenText, ErrorCode.FieldRequired, ct);
            case PanelItemObstacle.ItemSelectionInvalid:
                return await RefuseAsync(id, ItemChoiceInvalidText, ErrorCode.InvalidListValue, ct);
            case PanelItemObstacle.QuantityInvalid:
                return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);
            case PanelItemObstacle.QuantityExceedsOpen:
                return await RefuseAsync(id, QuantityExceedsOpenText, ErrorCode.QuantityExceedsOpen, ct);
            case PanelItemObstacle.DateInvalid:
                return await RefuseAsync(id, dateCode == ErrorCode.InvalidDate ? ArrivalDateInvalidText : NoticeDateInvalidText, dateCode, ct);
            case PanelItemObstacle.IbanInvalid:
                return await RefuseAsync(id, NoticeIbanInvalidText, ErrorCode.InvalidIban, ct);
            case PanelItemObstacle.WarningsUnconfirmed:
                return await RefuseAsync(id, ExceptionUnacknowledgedText, ErrorCode.ExceptionAcknowledgementRequired, ct);
            case PanelItemObstacle.DeliveryOrderMissing:
                return await RefuseAsync(id, DeliveryOrderQuestionText, ErrorCode.DeliverySequenceRequired, ct);
            case PanelItemObstacle.ItemNotShipped:
                return await RefuseAsync(id, NotShippedItemText, ErrorCode.NotShippedPhysicalItem, ct);
        }

        if (result.Refusal is TransitionRefusal.ConditionNotMet && notOpenCode != ErrorCode.OperationNotAvailable)
        {
            return await RefuseAsync(id, notOpenCode == ErrorCode.ReturnRejectionNotAllowed ? ReturnRejectionNotAllowedText : NoticeDateInvalidText, notOpenCode, ct);
        }

        return await AnswerAsync(id, new PanelStepResult(result.Refusal), ct);
    }

    // The page opens the operation's transaction (09 8.3 item 3); the module's write method locks, compares the version, asks the rule and writes. A rejected step rolls it back.
    private async Task<PanelItemStepResult> RunItemStepAsync(Func<IPanelItemSteps, CancellationToken, Task<PanelItemStepResult>> step, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        IPanelItemSteps steps = services.GetRequiredService<IPanelItemSteps>();
        OperationRunner runner = services.GetRequiredService<OperationRunner>();
        PanelItemStepResult? outcome = null;
        await runner.RunAsync(
            async token =>
            {
                outcome = await step(steps, token);
                return outcome.Gate;
            },
            ct);
        return outcome!; // non-null: the runner runs the step once before it returns
    }

    private static ConfirmView CancelConfirm(Guid id, CancelOrderByCompanyInput input, CancellationReason reason, IReadOnlyList<PanelItemUnits> units, bool isTransferLine)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("Input.CancellationReason", reason.ToString()),
            new("Input.ReasonNote", input.ReasonNote ?? string.Empty),
            Field("Input.Version", input.Version),
            new("Input.Confirmed", "true"),
        ];
        for (int i = 0; i < units.Count; i++)
        {
            fields.Add(new($"Input.Items[{i}].OrderItemId", units[i].OrderItemId.ToString("D")));
            fields.Add(Field($"Input.Items[{i}].Quantity", units[i].Quantity));
        }

        IrreversibleConfirmation texts = IrreversibleConfirmations.FirmCancellation(
            units.Count > 0, Math.Max(1, units.Sum(u => u.Quantity)), !isTransferLine, reason == CancellationReason.UndeliveredBuyerCaused, reason == CancellationReason.OutOfStock);
        if (reason == CancellationReason.AgreedWithBuyer)
        {
            texts = texts with { Warning = texts.Warning is null ? OwnerCheckReminderText : texts.Warning + " " + OwnerCheckReminderText };
        }

        return Confirmations.ForIrreversible(texts, CancelByCompanyLabel, CancelByCompanyLabel, PathOf(id), "CancelOrderByCompany", PathOf(id), fields);
    }

    private static ConfirmView WithdrawalConfirm(Guid id, RecordWithdrawalInput input, PanelItemUnits chosen)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("Input.Items[0].OrderItemId", chosen.OrderItemId.ToString("D")),
            Field("Input.Items[0].Quantity", chosen.Quantity),
            new("Input.DeclarationDate", input.DeclarationDate ?? string.Empty),
            new("Input.IsFromContactEmail", input.IsFromContactEmail ? "true" : "false"),
            new("Input.Iban", input.Iban ?? string.Empty),
            new("Input.IsExceptionAcknowledged", input.IsExceptionAcknowledged ? "true" : "false"),
            new("Input.IsDeliveredBeforeDeclaration", input.IsDeliveredBeforeDeclaration is { } before ? (before ? "true" : "false") : string.Empty),
            Field("Input.Version", input.Version),
            new("Input.Confirmed", "true"),
        ];
        _ = TryReadDate(input.DeclarationDate, out DateOnly date);
        return Confirmations.ForIrreversible(
            IrreversibleConfirmations.OtherChannelWithdrawal(Math.Max(1, chosen.Quantity), date), RecordWithdrawalLabel, RecordWithdrawalLabel, PathOf(id), "RecordWithdrawal", PathOf(id), fields);
    }

    private ConfirmView TerminationConfirm(Guid id, RecordLateDeliveryTerminationInput input, DateOnly date)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("Input.DeclarationDate", input.DeclarationDate ?? string.Empty),
            new("Input.IsFromContactEmail", input.IsFromContactEmail ? "true" : "false"),
            new("Input.Iban", input.Iban ?? string.Empty),
            Field("Input.Version", input.Version),
            new("Input.Confirmed", "true"),
        ];

        // K-796: the termination falls on the undelivered physical items' open units - the server's open quantity, read for the screen.
        int units = Detail.Items.Where(i => i.ProductType == ProductType.Physical && i.DeliveryMarkedAt is null).Sum(i => ItemFactsOf(i)?.OpenQuantity ?? 0);
        return Confirmations.ForIrreversible(
            IrreversibleConfirmations.OtherChannelTermination(Math.Max(1, units), date),
            RecordLateTerminationLabel,
            RecordLateTerminationLabel,
            PathOf(id),
            "RecordLateDeliveryTermination",
            PathOf(id),
            fields);
    }

    private ConfirmView RemovalConfirm(Guid id, RemoveOrderItemInput input, Guid itemId, int quantity)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("Input.OrderItemId", itemId.ToString("D")),
            Field("Input.Quantity", quantity),
            Field("Input.Version", input.Version),
            new("Input.Confirmed", "true"),
        ];

        // 03 1.6.1.5: the card line's removal; the amount is the record's and is written with it (D-1591: the window does not repeat it yet).
        IrreversibleConfirmation texts = new(
            IrreversibleAction.CardRefundingRemoval,
            null,
            DisplayFormat.Number(quantity) + " adet " + ItemNameOf(itemId) + " siparişten çıkarılacak ve tutarı müşterinin kartına geri ödenecek.");
        return Confirmations.ForIrreversible(texts, RemoveItemLabel, RemoveItemLabel, PathOf(id), "RemoveOrderItem", PathOf(id), fields);
    }

    private ConfirmView RejectionConfirm(Guid id, ReceiveReturnInput input, int? quantity)
    {
        List<KeyValuePair<string, string>> fields =
        [
            new("Input.OrderItemId", input.OrderItemId?.ToString("D") ?? string.Empty),
            new("Input.ArrivalDate", input.ArrivalDate ?? string.Empty),
            new("Input.Quantity", input.Quantity ?? string.Empty),
            Field("Input.Version", input.Version),
            new("Input.Confirmed", "true"),
        ];
        int units = quantity ?? ReturnSteps.Withdrawals.FirstOrDefault(w => w.OrderItemId == input.OrderItemId)?.Awaiting ?? 1;
        return Confirmations.ForIrreversible(
            IrreversibleConfirmations.ReturnRejection(Math.Max(1, units)), RejectReturnLabel, RejectReturnLabel, PathOf(id), "RejectReturn", PathOf(id), fields);
    }

    // The chosen rows: a row without an item is not chosen (an unticked box); a chosen row's quantity is a whole number of at least one (04 2.12.1.8).
    private static bool TryReadUnits(IEnumerable<(Guid? OrderItemId, string? Quantity)> rows, out IReadOnlyList<PanelItemUnits> units)
    {
        List<PanelItemUnits> chosen = [];
        units = chosen;
        foreach ((Guid? orderItemId, string? text) in rows)
        {
            if (orderItemId is not { } itemId)
            {
                continue;
            }

            if (!TryReadQuantity(text, out int quantity))
            {
                return false;
            }

            chosen.Add(new PanelItemUnits(itemId, quantity));
        }

        return true;
    }

    private static bool TryReadQuantity(string? text, out int quantity) =>
        int.TryParse(text?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out quantity) && quantity >= 1;

    private static bool TryReadDate(string? text, out DateOnly date) =>
        DateOnly.TryParseExact(text?.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date);
}

/// <summary>One line of UC-129's <c>Items</c> (07 5.1.5): the item's key - empty when the line was not chosen - and its units as typed, so a wrong value is a refusal and not a binding error (07 2.9.4).</summary>
public sealed class CancelOrderByCompanyInputLine
{
    public Guid? OrderItemId { get; set; }

    public string? Quantity { get; set; }
}

/// <summary>One line of UC-130's <c>Items</c> (07 5.1.5): as <see cref="CancelOrderByCompanyInputLine"/>.</summary>
public sealed class RecordWithdrawalInputLine
{
    public Guid? OrderItemId { get; set; }

    public string? Quantity { get; set; }
}

/// <summary>UC-129 <c>CancelOrderByCompanyInput</c>: the items after the payment, the reason and "Diğer"'s note, the version and the confirmation.</summary>
public sealed class CancelOrderByCompanyInput : IFormInput
{
    [SpecAssignedName]
    public IList<CancelOrderByCompanyInputLine> Items { get; } = [];

    public string? CancellationReason { get; set; }

    public string? ReasonNote { get; set; }

    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>UC-130 <c>RecordWithdrawalInput</c>: the item and units, the notice's date, the contact e-mail option with its IBAN, the separate box, the order answer, the version and the confirmation.</summary>
public sealed class RecordWithdrawalInput : IFormInput
{
    [SpecAssignedName]
    public IList<RecordWithdrawalInputLine> Items { get; } = [];

    public string? DeclarationDate { get; set; }

    [SpecAssignedName]
    public bool IsFromContactEmail { get; set; }

    public string? Iban { get; set; }

    [SpecAssignedName]
    public bool IsExceptionAcknowledged { get; set; }

    public bool? IsDeliveredBeforeDeclaration { get; set; }

    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>UC-130 <c>RecordLateDeliveryTerminationInput</c>: no item and no unit (K-796) - the notice's date, the contact e-mail option with its IBAN, the version and the confirmation.</summary>
public sealed class RecordLateDeliveryTerminationInput : IFormInput
{
    public string? DeclarationDate { get; set; }

    [SpecAssignedName]
    public bool IsFromContactEmail { get; set; }

    public string? Iban { get; set; }

    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>UC-138 <c>RemoveOrderItemInput</c>: the item, the units, the version and - on the card line - the confirmation.</summary>
public sealed class RemoveOrderItemInput : IFormInput
{
    public Guid? OrderItemId { get; set; }

    public string? Quantity { get; set; }

    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>UC-140 <c>ReceiveReturnInput</c> · <c>RejectReturnInput</c>: the item, the arrival date and the units, the version and - on the rejection - the confirmation.</summary>
public sealed class ReceiveReturnInput : IFormInput
{
    public Guid? OrderItemId { get; set; }

    public string? ArrivalDate { get; set; }

    public string? Quantity { get; set; }

    public int Version { get; set; }

    /// <summary>The confirmation mark of the second send (07 2.2.4); not a field of 06.</summary>
    [SpecAssignedName]
    public bool Confirmed { get; set; }
}

/// <summary>UC-141 <c>RestockReturnInput</c>: the receipt and the version.</summary>
public sealed class RestockReturnInput : IFormInput
{
    public Guid? ReturnId { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-142 <c>CloseNotReturnedInput</c>: the item, the units (default all not come back) and the version.</summary>
public sealed class CloseNotReturnedInput : IFormInput
{
    public Guid? OrderItemId { get; set; }

    public string? Quantity { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-144 <c>ResolveDefectClaimInput</c>: the claim and the version.</summary>
public sealed class ResolveDefectClaimInput : IFormInput
{
    public Guid? DefectClaimId { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-145 <c>AdminReopenDefectClaimInput</c> (07 5.1.5): the claim and the version; the customer's page has its own model.</summary>
public sealed class AdminReopenDefectClaimInput : IFormInput
{
    public Guid? DefectClaimId { get; set; }

    public int Version { get; set; }
}
