using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The shipment and delivery mark forms on the single-field forms' one pattern (T604, D-1812, D-1550; 04 2.3.1.1, 2.3.2, 2.3.3, 9.9.36, 7.2.7.2, 7.2.7.3): a missing or malformed value is the
/// field's message under it with the summary at the form's head, and the field carries <c>aria-invalid</c> and <c>aria-describedby</c>; a rule's bound (a future date, a day before the shipment)
/// is the lasting message in place under the field, described the same way, with no summary. While the refused form is drawn the message is the form's, not the step section's head.
/// </summary>
public sealed partial class OrderDetailModel
{
    /// <summary>The element id of a refused step's lasting message, in its form or at the step section's head.</summary>
    public const string StepMessageId = "order-step-message";

    /// <summary>The shipment's refusal (UC-124, UC-125: the carrier data) while a shipment form - the primary mark or the re-sending - is drawn.</summary>
    public bool RefusalIsShipmentField =>
        RefusedItemId is null && !IsNoteRefused && RefusalText == ShipmentMissingText && (PrimaryStep == OrderPrimaryStep.MarkShipped || IsOpen(OrderOperation.ResendOrder));

    /// <summary>The delivery date's refusal (UC-126: missing, malformed, in the future, before the shipment) while the delivery mark's form is drawn. The order question keeps its own place.</summary>
    public bool RefusalIsDeliveryField =>
        RefusedItemId is null && !IsNoteRefused && RefusalText is DeliveryDateMissingText or DeliveryDateFutureText or DeliveryDateBeforeShipmentText && PrimaryStep == OrderPrimaryStep.MarkDelivered;

    /// <summary>The value written to the refused field's <c>aria-invalid</c>; null (no attribute) when the field is not the refused one.</summary>
    public static string? StepInvalid(bool refused) => refused ? "true" : null;

    /// <summary>The id the refused field is described by (the message under it); null when the field is not the refused one.</summary>
    public static string? StepDescribedBy(bool refused) => refused ? StepMessageId : null;

    /// <summary>The refusal's message under the refused field; null when the field is not the refused one.</summary>
    public MessageView? StepMessageFor(bool refused) => refused && RefusalText is { } text ? Messages.InPlace(MessageMeaning.Error, text, StepMessageId) : null;

    /// <summary>The summary at the head of the refused form when the value was missing or malformed (04 2.3.2); a rule's bound has none (04 2.3.3).</summary>
    public FormErrorSummary? StepSummaryFor(bool refused, string fieldId) =>
        refused && RefusalText is { } text && text is ShipmentMissingText or DeliveryDateMissingText ? new FormErrorSummary([new FormFieldError(fieldId, text)]) : null;
}
