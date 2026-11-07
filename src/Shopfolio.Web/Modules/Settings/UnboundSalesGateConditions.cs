using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The defaults of the sales gate's three outside conditions, registered with <c>TryAdd</c> until the task that owns the condition registers its own implementation in their
/// place (T352 the card line, T353 the transfer line, T366 the privacy notice). An unbound condition is not met: the gate stays closed rather than guess (11 T331 criterion 8).
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

/// <summary>See <see cref="UnboundCardPaymentCondition"/>.</summary>
internal sealed class UnboundPrivacyNoticeCondition : IPrivacyNoticeCondition
{
    public Task<bool> IsPublishedWithHostingLocationAsync(CancellationToken ct) => Task.FromResult(false);
}
