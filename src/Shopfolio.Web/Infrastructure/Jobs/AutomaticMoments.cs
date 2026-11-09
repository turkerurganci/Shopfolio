namespace Shopfolio.Infrastructure.Jobs;

/// <summary>One moment of 03 4.2 and the system actions that carry it: each action is named "Namespace.Type.Member" and resolves in the product's assembly.</summary>
internal sealed record AutomaticMoment(string Number, string Moment, IReadOnlyList<string> Actions);

/// <summary>
/// The register of the twelve moments that run by themselves (03 4.2; 02 4.3; T327 criterion 1): every moment of the table is one row here with the
/// system actions that make it happen. The actions are named, not referenced, so the register crosses no module border (09 6); a unit test resolves
/// every name by reflection and compares the numbers with 03 4.2, so a renamed action or a moment without an action breaks the build.
/// The outage's rules (03 4.2.11, 4.2.12) live in the scheduler: time runs on during an outage - no source adds the outage to a period, no switch closes the
/// site for upkeep and no upkeep period is announced (02 3.34.7; 10 KD-60) - and the only change the return makes is the transfer window's deferral.
/// </summary>
internal static class AutomaticMoments
{
    public static readonly IReadOnlyList<AutomaticMoment> All =
    [
        new("4.2.1", "Order confirmation", [
            "Shopfolio.Modules.Ordering.OrderConfirmation.ConfirmAsync",
            "Shopfolio.Modules.Ordering.OrderIdentifiers.IssueAsync",
            "Shopfolio.Modules.Ordering.Order.FreezeDocuments",
            "Shopfolio.Modules.Ordering.StockReservation.ForVariant",
            "Shopfolio.Modules.Ordering.StockReservation.ForCoupon",
            "Shopfolio.Modules.Ordering.PaymentWindows.StartAsync",
        ]),
        new("4.2.2", "Payment confirmation", [
            "Shopfolio.Modules.Ordering.OrderTransitions.ApplyCardPaymentAsync",
            "Shopfolio.Modules.Ordering.OrderTransitions.MarkTransferPaidAsync",
        ]),
        new("4.2.3", "Automatic cancellation", [
            "Shopfolio.Modules.Ordering.TransferPaymentExpiry.ProcessAsync",
            "Shopfolio.Modules.Ordering.CardPaymentExpiry.ProcessAsync",
            "Shopfolio.Modules.Payment.FinalPaymentQueryJobHandler.HandleAsync",
        ]),
        new("4.2.4", "Cancellation, removal and late delivery termination", [
            "Shopfolio.Modules.Ordering.OrderTransitions.CancelOrderAsync",
            "Shopfolio.Modules.Ordering.OrderTransitions.EvaluateClosureAsync",
        ]),
        new("4.2.5", "Return", [
            "Shopfolio.Modules.AfterSales.Return.ForWithdrawal",
            "Shopfolio.Modules.AfterSales.Return.StockAddedAt",
        ]),
        new("4.2.6", "Partial cancellation and return", [
            "Shopfolio.Modules.Payment.Refund.IsShippingFeeIncluded",
        ]),
        new("4.2.7", "Automatic card refund", [
            "Shopfolio.Modules.Payment.CardRefundJobRule.Decide",
            "Shopfolio.Modules.Ordering.OrderTransitions.ApplyCardRefundOutcomeAsync",
        ]),
        new("4.2.8", "Refund IBAN deletion on a withdrawal whose goods did not come back", [
            "Shopfolio.Modules.Ordering.RefundIbanDeletionDay.ProcessAsync",
        ]),
        new("4.2.9", "Periodic disposal", [
            "Shopfolio.Infrastructure.Disposal.DisposalStep.RunAsync",
        ]),
        new("4.2.10", "E-mail retry", [
            "Shopfolio.Modules.Notifications.DispatchRetrySchedule.NextAttemptAfterFailure",
        ]),
        new("4.2.11", "Site outage", [
            "Shopfolio.Infrastructure.Jobs.OutageRecovery.ApplyAsync",
            "Shopfolio.Infrastructure.Jobs.SchedulerScan.RunAsync",
        ]),
        new("4.2.12", "Transfer payment window running out during an outage", [
            "Shopfolio.Modules.Ordering.TransferWindowDeferral.ApplyAsync",
        ]),
    ];
}
