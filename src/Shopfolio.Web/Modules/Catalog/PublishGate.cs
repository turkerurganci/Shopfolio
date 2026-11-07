namespace Shopfolio.Modules.Catalog;

/// <summary>What the publish gate says (02 5.1; 03 1.8.1): the conditions that are not met, by name. No unmet condition means the gate is open.</summary>
internal sealed record PublishGateVerdict(IReadOnlyList<string> UnmetConditions)
{
    public static PublishGateVerdict Open { get; } = new([]);

    public bool IsOpen => UnmetConditions.Count == 0;
}

/// <summary>
/// The publish gate (02 5.1; 03 1.8.1): the conditions a product or a variant must meet to move to Published. The transition unit asks it before the move and
/// leaves the status as it was when it refuses. The conditions themselves - the name, the main category and the type, a Published variant, the file of a digital
/// variant, the fulfilment period of a service - are T335's (11 T335 criteria 3 and 4); the unit knows only the question and the answer.
/// </summary>
internal interface IPublishGate
{
    /// <summary>The gate of a product's move to Published.</summary>
    Task<PublishGateVerdict> CheckProductAsync(Guid productId, CancellationToken ct);

    /// <summary>The gate of a variant's move to Published (a digital variant needs its file).</summary>
    Task<PublishGateVerdict> CheckVariantAsync(Guid variantId, CancellationToken ct);
}

/// <summary>
/// The gate until T335 writes the conditions (D-300): it is open for everything. It is replaced by registering the real gate in <c>AddCatalogModule</c>; the
/// transition unit's tests register a gate of their own that refuses.
/// </summary>
internal sealed class UnwrittenPublishGate : IPublishGate
{
    public Task<PublishGateVerdict> CheckProductAsync(Guid productId, CancellationToken ct) => Task.FromResult(PublishGateVerdict.Open);

    public Task<PublishGateVerdict> CheckVariantAsync(Guid variantId, CancellationToken ct) => Task.FromResult(PublishGateVerdict.Open);
}
