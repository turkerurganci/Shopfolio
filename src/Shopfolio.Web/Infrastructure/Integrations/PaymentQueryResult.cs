namespace Shopfolio.Infrastructure.Integrations;

/// <summary>
/// The result of asking a payment's outcome (08 3.1 table, 2.1.6; 09 11.1 item 2). <see cref="NoResponse"/> is the call whose result is unknown:
/// the order is not cancelled and the query is asked again (08 3.1 rules 3 and 4). Whether a paid answer is applied - its amount against the
/// order's total, the payment type, the currency, the test mode - is the core's rule; the layer only reads the fields the field mapping lists.
/// </summary>
internal abstract record PaymentQueryResult : IExternalCallResult<PaymentQueryResult>
{
    private PaymentQueryResult()
    {
    }

    /// <summary>
    /// A successful payment exists. The amounts are read culture-free and are not rounded (08 2.1.6). <paramref name="ProviderTransactionId"/> is the
    /// provider's reference of the payment, which the payment record keeps and which makes a second application of the same payment a repeat (K-970);
    /// the layer fills it from the provider's answer - which field it maps is the provider task's (T403, D-448) - and the core never makes it up.
    /// </summary>
    public sealed record Paid(string ProviderTransactionId, decimal PaymentAmount, decimal PaymentTotal, bool PaidByCard, bool InLira, bool TestMode)
        : PaymentQueryResult;

    /// <summary>The provider says there is no payment for the order.</summary>
    public sealed record NoPayment : PaymentQueryResult;

    /// <summary>No answer was received - the result is unknown (a connection error class; the order is not cancelled).</summary>
    public sealed record NoResponse(string ReasonCode) : PaymentQueryResult;

    /// <summary>A query without a classified answer has an unknown result (08 2.1.6; 08 3.1: the unlisted answer is a connection error).</summary>
    public static PaymentQueryResult FromFailure(ExternalCallFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new NoResponse(failure.ReasonCode);
    }
}
