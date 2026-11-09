namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>
/// The payment methods section of the firm settings (02 3.21.5; T353): the bank transfer switch and its IBAN. Apart from <see cref="ISettingsModule"/> because its save tells every
/// administrator about a new IBAN through the Administration module (02 9.3.3 F-5), which the module's other readers do not need.
/// </summary>
public interface IPaymentMethodSettings
{
    /// <summary>The section as stored: the switch, the IBAN in its stored form and the section's version.</summary>
    Task<PaymentMethodsSection> GetAsync(CancellationToken ct);

    /// <summary>
    /// The payment methods form's save (02 3.21.5): in the caller's transaction, behind the settings row lock, by the conditional update of the section's version (09 10.3). A
    /// switched-on transfer without an IBAN is a refusal (<c>FieldRequired</c> on the IBAN), an IBAN that is not a valid Turkish IBAN too (<c>InvalidIban</c>); nothing is stored
    /// then. The IBAN is stored in its compact form. Every changed field reaches the trail with its old and new value (02 10.3.1 (1), 9.3.3); an IBAN entered or changed queues F-5
    /// for every administrator in the same transaction (02 9.3.3; K-616). Switching the transfer off or changing the IBAN does not touch the open orders: they keep the IBAN they
    /// froze (02 3.1.6, 3.21.5; K-663, K-664). A save that changes nothing writes nothing.
    /// </summary>
    Task<SettingsSaveResult> SaveAsync(PaymentMethodsInput input, int formVersion, CancellationToken ct);
}
