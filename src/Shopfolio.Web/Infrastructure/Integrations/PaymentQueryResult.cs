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
    /// A successful payment exists. The amounts are read culture-free and are not rounded (08 2.1.6; <see cref="PaymentAmounts.TryParseProviderText"/>).
    /// <paramref name="ProviderTransactionId"/> is the provider's reference of the payment, which the payment record keeps and which makes a second application of
    /// the same payment a repeat (K-970): the layer fills it with the reference it asked about - for PayTR the <c>merchant_oid</c>, the same value the notification
    /// carries (08 2.1.6 "ödeme kimliği"; T403, D-448) - and the core never makes it up.
    /// </summary>
    public sealed record Paid(string ProviderTransactionId, decimal PaymentAmount, decimal PaymentTotal, bool PaidByCard, bool InLira, bool TestMode)
        : PaymentQueryResult;

    /// <summary>The provider says there is no payment for the order.</summary>
    public sealed record NoPayment : PaymentQueryResult;

    /// <summary>
    /// No answer was received - the result is unknown (a connection error class; the order is not cancelled). <paramref name="CodeError"/> tells the two intervals
    /// apart (08 2.1.6; 05 4.3.4): false for an answer that waiting may change - the unreachable provider, the timeout, the answer outside the table - which is asked
    /// again every five minutes; true for an answer that waiting does not change - the provider's request errors, an amount text that is not recognized, missing
    /// keys - which the job's code error rule repeats (1, 5, 15, 60, 240 minutes, then failed).
    /// </summary>
    public sealed record NoResponse(string ReasonCode, bool CodeError = false) : PaymentQueryResult;

    /// <summary>A query without a classified answer has an unknown result (08 2.1.6; 08 3.1: the unlisted answer is a connection error).</summary>
    public static PaymentQueryResult FromFailure(ExternalCallFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return new NoResponse(failure.ReasonCode);
    }
}
