namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The result of asking a card refund's outcome (08 3.1 table, 2.1.9; 09 11.1 item 2): the refund happened, or there is no result yet. The
/// provider does not show a refused refund in the query; "did not happen" comes only from the refund's answer or the firm's mark.
/// </summary>
internal abstract record RefundQueryResult : IExternalCallResult<RefundQueryResult>
{
    private RefundQueryResult()
    {
    }

    /// <summary>The refund with this reference and this amount is in the provider's list.</summary>
    public sealed record Completed : RefundQueryResult;

    /// <summary>No result: the refund is not (yet) in the list, or the query got no classified answer; the query is asked again by its job.</summary>
    public sealed record NoResult : RefundQueryResult;

    /// <summary>A query without a classified answer is "no result" (08 2.1.9: everything else is no result); its job asks again.</summary>
    public static RefundQueryResult FromFailure(ExternalCallFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new NoResult();
    }
}
