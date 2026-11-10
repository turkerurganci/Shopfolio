using Shopfolio.Infrastructure.InstallationSettings;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The card line of the sales gate's payment method condition (02 3.1.5 condition 2, 3.21.4; 05 8.4.6; D-81, D-198): open when the payment provider's keys are defined at
/// installation. The line's state is worked out once at start-up and kept in the <see cref="ConditionalLine"/> singleton; this reads it and never works it out again. A list
/// without the card line keeps the card closed - the safe side (11 T331 criterion 8).
/// </summary>
internal sealed class CardPaymentCondition : ICardPaymentCondition
{
    private readonly IReadOnlyList<ConditionalLine> _lines;

    public CardPaymentCondition(IReadOnlyList<ConditionalLine> lines)
    {
        ArgumentNullException.ThrowIfNull(lines);
        _lines = lines;
    }

    public bool IsOpen => _lines.Any(line => line.Name == ConditionalLine.CardPayment && line.IsOpen);

    public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(IsOpen);
}

/// <summary>The rule of the offered methods (02 3.21.2, 3.21.4): the card when its line is open, the bank transfer when it is open; nothing else.</summary>
internal static class PaymentMethodRules
{
    public static IReadOnlyList<PaymentMethod> Open(bool cardOpen, bool bankTransferOpen)
    {
        List<PaymentMethod> open = [];
        if (cardOpen)
        {
            open.Add(PaymentMethod.Card);
        }

        if (bankTransferOpen)
        {
            open.Add(PaymentMethod.BankTransfer);
        }

        return open;
    }
}

/// <summary>
/// The open methods (<see cref="IPaymentMethods"/>) from the two conditions the sales gate reads too - the card line here, the bank transfer line the Settings module binds (T353: <c>BankTransferCondition</c>) - so
/// the step, the confirmation and the gate say the same thing. The card's two-minute unavailable state (<see cref="CardUnavailability"/>; 08 2.1.8) takes the card
/// out of the open methods - the step and the confirmation - and leaves the gate, which reads the line only.
/// </summary>
internal sealed class PaymentMethods : IPaymentMethods
{
    private readonly ICardPaymentCondition _card;
    private readonly IBankTransferCondition _bankTransfer;
    private readonly CardUnavailability _unavailability;

    public PaymentMethods(ICardPaymentCondition card, IBankTransferCondition bankTransfer, CardUnavailability unavailability)
    {
        _card = card;
        _bankTransfer = bankTransfer;
        _unavailability = unavailability;
    }

    public async Task<IReadOnlyList<PaymentMethod>> ListOpenAsync(CancellationToken ct)
    {
        bool card = await _card.IsOpenAsync(ct) && !_unavailability.IsUnavailable;
        return PaymentMethodRules.Open(card, await _bankTransfer.IsOpenAsync(ct));
    }

    public async Task<bool> IsCardUnavailableNowAsync(CancellationToken ct) => await _card.IsOpenAsync(ct) && _unavailability.IsUnavailable;
}
