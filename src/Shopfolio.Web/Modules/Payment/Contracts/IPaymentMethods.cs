namespace Shopfolio.Modules.Payment.Contracts;

/// <summary>
/// The payment methods the payment step offers now (02 3.21.2, 3.21.4, 6.9.3; 07 4.13.1, 4.13.6): the card when the provider's keys are defined at installation, the bank transfer
/// when it is switched on with its IBAN. There is no other method - no cash on delivery - and the card has no switch of its own in the panel. The page shows these and the order
/// confirmation refuses a method that is not among them (<c>PaymentMethodUnavailable</c>). A scoped service of its own beside the module's face, so the module's constructor is not
/// touched; it reads and writes nothing else and opens no transaction (09 6 item 3).
/// </summary>
public interface IPaymentMethods
{
    /// <summary>The open methods in the fixed order card, bank transfer; empty when neither is open (the sales gate is then closed - 02 3.1.5, 6.9.4).</summary>
    /// <remarks>The card whose provider could not be reached in the last two minutes is not among them (08 2.1.8): the confirmation refuses it without a token request.</remarks>
    Task<IReadOnlyList<PaymentMethod>> ListOpenAsync(CancellationToken ct);

    /// <summary>
    /// True when the card's line is open but a token request found the provider unreachable within the last two minutes (08 2.1.8; 03 10.1.1.1): the payment step shows
    /// the card as "şu an kullanılamıyor" and does not let it be chosen. The sales gate does not read it.
    /// </summary>
    Task<bool> IsCardUnavailableNowAsync(CancellationToken ct);
}
