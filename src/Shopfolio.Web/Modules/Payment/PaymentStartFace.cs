using System.Diagnostics;

using Shopfolio.Infrastructure.Integrations;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The payment page's way to <see cref="PaymentStarts"/> (07 UC-29; T510): the page's order values become the layer's request, and the rule's step becomes the page's
/// decision. Nothing is written here and nothing is kept: the frame's address goes to the one response that draws it.
/// </summary>
internal sealed class PaymentStartFace : IPaymentStart
{
    private readonly PaymentStarts _starts;

    public PaymentStartFace(PaymentStarts starts)
    {
        ArgumentNullException.ThrowIfNull(starts);
        _starts = starts;
    }

    public TransitionReason UnopenedCancellationReason => PaymentStartRule.UnopenedCancellationReason;

    public async Task<PaymentStartAnswer> StartAsync(PaymentStartOrder order, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(order);
        PaymentStartRequest request = new(
            order.OrderNumber,
            order.Total,
            order.ContactEmail,
            order.CompanyEmail,
            order.BillingRecipientName,
            order.BillingAddressLine,
            order.ShippingPhone,
            order.CompanyPhone,
            order.ClientIpAddress,
            order.PaymentDeadline,
            order.OrderAccessKey);
        PaymentStartOutcome outcome = await _starts.StartAsync(request, ct);
        return new PaymentStartAnswer(DecisionOf(outcome.Step), outcome.FrameAddress);
    }

    public static PaymentStartDecision DecisionOf(PaymentStartStep step) => step switch
    {
        PaymentStartStep.OpenPaymentForm => PaymentStartDecision.OpenPaymentForm,
        PaymentStartStep.CancelAsUnavailable => PaymentStartDecision.CancelAsUnavailable,
        PaymentStartStep.StayOnConfirmation => PaymentStartDecision.StayOnConfirmation,
        _ => throw new UnreachableException(),
    };
}
