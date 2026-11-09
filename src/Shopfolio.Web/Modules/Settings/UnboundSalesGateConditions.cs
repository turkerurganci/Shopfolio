using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The default of the sales gate's card line, registered with <c>TryAdd</c>; the Payment module registers its own implementation in its place (T352: <c>CardPaymentCondition</c>),
/// so the default stands only where the Payment module is not registered. The transfer line is bound by this module (T353: <c>BankTransferCondition</c>) and the privacy notice
/// condition by T366 (<c>PrivacyNoticeCondition</c>). An unbound condition is not met: the gate stays closed rather than guess (11 T331 criterion 8).
/// </summary>
internal sealed class UnboundCardPaymentCondition : ICardPaymentCondition
{
    public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(false);
}
