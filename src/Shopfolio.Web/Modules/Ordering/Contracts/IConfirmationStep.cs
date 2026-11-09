namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// The confirmation step of the payment page (T356; 02 3.24; 04 5.13.9): what the customer reads and ticks before "Siparişi onayla — ödeme yükümlülüğü doğar", and the check the
/// confirmation makes of the ticks and of the texts shown. The order itself is born in T348's transaction; this unit has no write. Both members only read and open no transaction.
/// </summary>
public interface IConfirmationStep
{
    /// <summary>
    /// The step's content for a summary (T356; 02 3.24.1-3.24.3, 3.24.5): the form as the summary followed by the generated text with the customer's own tokens filled from the
    /// summary (the order's number is not born yet and reads as "sipariş onayında verilir"), the contract, the fixed text, the withdrawal summary, the boxes the lines call for -
    /// each with its text and the covered items' names - and the button. The versions shown are the last published ones; a missing one is an <see cref="InvalidOperationException"/>
    /// (the sales gate keeps the step closed while a text is missing; 02 3.1.5).
    /// </summary>
    Task<ConfirmationStepView> GetViewAsync(ConfirmationSummary summary, CancellationToken ct);

    /// <summary>
    /// The check the confirmation makes first (T356; 02 3.24.1, 3.24.2, 3.24.3; K-833): a form or contract that is no longer the last published version stops the order - the customer
    /// confirms the current text, there is no notice and no re-consent window - and an unticked box the order needs stops it too. An order with no line is a programming error.
    /// </summary>
    Task<ConfirmationStepCheck> CheckAsync(ConfirmationStepSubmission submission, CancellationToken ct);
}
