using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Binding;
using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Operations;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// T531's half of E-37 (04 9.9.26, 9.9.29, 9.9.32, 9.9.33; 07 UC-139, UC-143, UC-152...UC-154): the service item's "Tamamlandı olarak işaretle" - on the item's row and, when no order-level
/// step is left, as the primary button for the first open service item (04 9.9.5; D-1491) -, the digital item's download right renewal, the internal note and the re-sending of the e-mails under the
/// "e-posta ulaşmadı" mark. None asks for a confirmation. Every one is the module's write method in the transaction the page opens (09 8.3 item 3): a stale version is 409 with the order as it is now;
/// an operation that is not open - an unpaid order, a closed item, an item that is not digital, a notice that is no longer marked - is 422 with the message in the operation's place. The note carries no
/// version: its shown text is its condition, and a changed note is 409 with the current note (K-1051); a disposed order takes no note (422; K-1095).
/// </summary>
public sealed partial class OrderDetailModel
{
    public const string CompleteServiceLabel = "Tamamlandı olarak işaretle";

    public const string ResetDownloadLabel = "İndirme hakkını yenile";

    public const string SaveNoteLabel = "Notu kaydet";

    /// <summary>The note's field label (04 9.9.11).</summary>
    public const string NoteFieldLabel = "İç not";

    /// <summary>The first open service item when its mark is the primary button (04 9.9.5; Z-39); null otherwise.</summary>
    public OrderPageItem? PrimaryServiceItem =>
        PrimaryStep == OrderPrimaryStep.CompleteService ? Detail.Items.FirstOrDefault(i => Operations.IsOpen(OrderOperation.MarkServiceCompleted, i.OrderItemId)) : null;

    /// <summary>The service fulfilment period beside the primary service button (04 9.9.5, 2.6.3; Z-39), with the overrun's legal interest warning.</summary>
    internal IReadOnlyList<PeriodIndicator> ServicePeriods => [.. RunningPeriods.Where(p => p.Period == CountedPeriod.ServiceFulfilmentPeriod)];

    /// <summary>The note the field shows and its <c>Original</c> twin carries: the stored note, or after a conflict the note as it is now (K-1051); empty when there is none.</summary>
    public string ShownNoteText => Facts.PersonalDataDisposed ? string.Empty : Facts.InternalNote?.Text ?? string.Empty;

    /// <summary>True when the note was saved against a changed note (07 UC-152: 409): the order notice shows in the note's place and the field carries the current note.</summary>
    public bool IsNoteConflict { get; private set; }

    /// <summary>The note field is open: the order's personal data is not disposed (04 9.9.11; K-1095) - the decision's row (03 1.11.27).</summary>
    public bool CanWriteNote => IsOpen(OrderOperation.WriteInternalNote);

    /// <summary>The item whose operation was refused: its message stands on the item's row (04 9.9.41); null otherwise.</summary>
    public Guid? RefusedItemId { get; private set; }

    /// <summary>True when the note's save was refused (422): its message stands in the note's place.</summary>
    public bool IsNoteRefused { get; private set; }

    /// <summary>UC-139: the service item's mark on its whole open quantity (K-792); not open (unpaid order, closed item) is 422; no confirmation.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostCompleteServiceItemAsync(Guid id, CompleteServiceItemInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelStepResult result = await RunUpkeepAsync((upkeep, token) => upkeep.CompleteServiceItemAsync(new PanelItemStepRequest(id, input.OrderItemId, admin, input.Version), token), ct);
        return await ItemAnswerAsync(id, input.OrderItemId, result, ct);
    }

    /// <summary>UC-143: the digital item's download counter back at zero; no notice (K-683); an item that is not digital is 422.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostResetDownloadCountAsync(Guid id, ResetDownloadCountInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelStepResult result = await RunUpkeepAsync((upkeep, token) => upkeep.ResetDownloadCountAsync(new PanelItemStepRequest(id, input.OrderItemId, admin, input.Version), token), ct);
        return await ItemAnswerAsync(id, input.OrderItemId, result, ct);
    }

    /// <summary>UC-152: the note written or - emptied - deleted; the order's version does not move. A changed note is 409 with the current note; a disposed order is 422.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostSaveInternalNoteAsync(Guid id, SaveInternalNoteInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        SaveInternalNoteRequest request = new(id, admin, input.Text ?? string.Empty, input.OriginalText ?? string.Empty);
        IPanelOrderUpkeep upkeep = HttpContext.RequestServices.GetRequiredService<IPanelOrderUpkeep>();
        OperationRunner runner = HttpContext.RequestServices.GetRequiredService<OperationRunner>();
        PanelNoteResult? outcome = null;
        await runner.RunAsync(
            async token =>
            {
                outcome = await upkeep.SaveInternalNoteAsync(request, token);
                return outcome.Gate;
            },
            SaveInternalNoteRequest.RetryOnUniqueIndexes,
            ct);
        SaveInternalNoteResult note = outcome!.Note; // non-null: the runner runs the write at least once before it returns
        if (!note.IsRejected)
        {
            return FormResults.SeeOther(PathOf(id));
        }

        if (!await LoadAsync(id, ct))
        {
            return NotFound();
        }

        Reject(note.Code!.Value); // non-null: a rejected result carries its code
        if (note.Code == ErrorCode.DisplayedValueConflict)
        {
            IsNoteConflict = true;
            return FormResults.Conflict(this);
        }

        IsNoteRefused = true;
        RefusalText = NotOpenText;
        return FormResults.Redisplay(this);
    }

    /// <summary>UC-153: a new "queued" row in the marked notice's group; the result is read at the next drawing. A row that is no longer marked, or not its group's latest, is 422.</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostResendNotificationAsync(Guid id, ResendNotificationInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelStepResult result = await RunUpkeepAsync((upkeep, token) => upkeep.ResendNotificationAsync(new PanelResendRequest(id, input.DispatchRecordId, admin, input.Version), token), ct);
        return await AnswerAsync(id, result, ct);
    }

    /// <summary>UC-154: every marked notice of the order again, in one transaction - all or none (K-761).</summary>
    [GatedOperation]
    public async Task<IActionResult> OnPostResendAllNotificationsAsync(Guid id, ResendAllNotificationsInput input, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(input);
        TransitionAdmin admin = await AdminAsync(ct);
        PanelStepResult result = await RunUpkeepAsync((upkeep, token) => upkeep.ResendAllNotificationsAsync(new AdminOrderRequest(id, admin, input.Version), token), ct);
        return await AnswerAsync(id, result, ct);
    }

    /// <summary>The refused step's message stands at the line's step section; an item's or the note's stands in their own place (04 9.9.41).</summary>
    public bool ShowsRefusalAtStep => RefusedItemId is null && !IsNoteRefused;

    // 04 9.9.33, 2.5.4: a group whose latest row is not its first is a re-sending that ended "undelivered" again - the row stays and the lasting message says so.
    private static bool ResendFailedAgain(IReadOnlyList<Shopfolio.Modules.Notifications.Contracts.UndeliveredNotice> undelivered) =>
        undelivered.Any(n => n.LatestDispatchId != n.GroupId);

    // The item operation's answer: as the line steps', with a refusal's message on the item's row (04 9.9.41).
    private async Task<IActionResult> ItemAnswerAsync(Guid id, Guid orderItemId, PanelStepResult result, CancellationToken ct)
    {
        if (result.Refusal is not (null or TransitionRefusal.OrderNotFound or TransitionRefusal.VersionConflict))
        {
            RefusedItemId = orderItemId;
        }

        return await AnswerAsync(id, result, ct);
    }

    // The page opens the operation's transaction (09 8.3 item 3); the module's write method locks, compares the version, asks the rule and writes. A rejected one rolls it back.
    private async Task<PanelStepResult> RunUpkeepAsync(Func<IPanelOrderUpkeep, CancellationToken, Task<PanelStepResult>> step, CancellationToken ct)
    {
        IServiceProvider services = HttpContext.RequestServices;
        IPanelOrderUpkeep upkeep = services.GetRequiredService<IPanelOrderUpkeep>();
        OperationRunner runner = services.GetRequiredService<OperationRunner>();
        PanelStepResult? outcome = null;
        await runner.RunAsync(
            async token =>
            {
                outcome = await step(upkeep, token);
                return outcome.Gate;
            },
            ct);
        return outcome!; // non-null: the runner runs the step once before it returns
    }
}

/// <summary>UC-139 <c>CompleteServiceItemInput</c>: the item and the order's version (07 9.9.18); no quantity - the mark falls on the whole open quantity (K-792).</summary>
public sealed class CompleteServiceItemInput : IFormInput
{
    public Guid OrderItemId { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-143 <c>ResetDownloadCountInput</c>: the item and the order's version (07 9.9.22).</summary>
public sealed class ResetDownloadCountInput : IFormInput
{
    public Guid OrderItemId { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-152 <c>SaveInternalNoteInput</c>: the note's text (06 4.20) and the text the form showed - the <c>Original</c> twin (07 2.4.3; K-1051). No version.</summary>
public sealed class SaveInternalNoteInput : IFormInput
{
    public string? Text { get; set; }

    /// <summary>The note as the form showed it (07 2.4.3); not a field of 06.</summary>
    [SpecAssignedName]
    public string? OriginalText { get; set; }
}

/// <summary>UC-153 <c>ResendNotificationInput</c>: the latest row of the marked notice's group (06 4.58) and the order's version (K-1188 (2)).</summary>
public sealed class ResendNotificationInput : IFormInput
{
    public Guid DispatchRecordId { get; set; }

    public int Version { get; set; }
}

/// <summary>UC-154 <c>ResendAllNotificationsInput</c>: the order's version only (07 9.9.33).</summary>
public sealed class ResendAllNotificationsInput : IFormInput
{
    public int Version { get; set; }
}
