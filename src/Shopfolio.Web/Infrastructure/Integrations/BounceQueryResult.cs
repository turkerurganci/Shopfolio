namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The result of asking the bounced addresses (08 3.1 table, 2.2.3; 09 11.1 item 2): the list, or a query error - the round passes empty and
/// is not written to the channel state; the next successful round's 24 hour window catches it.
/// </summary>
internal abstract record BounceQueryResult : IExternalCallResult<BounceQueryResult>
{
    private BounceQueryResult()
    {
    }

    /// <summary>The permanently bounced addresses of the window, as the service holds them (the product compares them case-free, 08 2.2.3).</summary>
    public sealed record List(IReadOnlyList<string> Addresses) : BounceQueryResult;

    public sealed record QueryFailed(string ReasonCode) : BounceQueryResult;

    /// <summary>A query without a classified answer is a query error.</summary>
    public static BounceQueryResult FromFailure(ExternalCallFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new QueryFailed(failure.ReasonCode);
    }
}
