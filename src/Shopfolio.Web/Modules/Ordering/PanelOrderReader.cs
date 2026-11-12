using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>The Ordering module's reads for the panel's order screens (T527); internal, reached only through <see cref="IPanelOrderReader"/>.</summary>
internal sealed class PanelOrderReader : IPanelOrderReader
{
    private readonly OrderPeriods _periods;

    public PanelOrderReader(OrderPeriods periods)
    {
        ArgumentNullException.ThrowIfNull(periods);
        _periods = periods;
    }

    public Task<IReadOnlyList<OrderRunningPeriod>> FindRunningPeriodsAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct) => _periods.RunningPeriodsAsync(orderIds, ct);
}
