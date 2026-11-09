using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The defaults of the sales gate's two payment conditions, registered with <c>TryAdd</c>; the task that owns the condition registers its own implementation in their
/// place (T352 the card line - the Payment module's <c>CardPaymentCondition</c>; T353 the transfer line). The card default stands only where the Payment module is not registered. The privacy notice condition is bound by T366 (<c>PrivacyNoticeCondition</c>). An unbound condition is not met: the gate stays closed rather than guess (11 T331 criterion 8).
/// </summary>
internal sealed class UnboundCardPaymentCondition : ICardPaymentCondition
{
    public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(false);
}

/// <summary>See <see cref="UnboundCardPaymentCondition"/>.</summary>
internal sealed class UnboundBankTransferCondition : IBankTransferCondition
{
    public Task<bool> IsOpenAsync(CancellationToken ct) => Task.FromResult(false);
}
