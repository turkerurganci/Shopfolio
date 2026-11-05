namespace Shopfolio.Infrastructure.SignedEndpoints;

/// <summary>
/// One signed machine endpoint (08 3.2; 05 3.6.1, 4.5.2): a request from an outside service that brings data into the product. Today there is
/// one - the payment provider's notification (UC-80) -, whose implementation is the notification endpoint's task; a new one comes with a decision
/// record (08 3.2 principle 6). The common pipeline (<see cref="SignedEndpointHandler"/>) owns the order, the limits and the answers; this
/// interface gives it only the two things that are the endpoint's own.
/// </summary>
/// <typeparam name="TNotice">The verified request as the link layer gives it: only the fields the field mapping reads, no personal field.</typeparam>
internal interface ISignedEndpoint<TNotice>
    where TNotice : class
{
    /// <summary>
    /// The body of the "received" answer in the service's own form (08 3.2 principle 3). It is sent with 200 and only after the record has
    /// committed; every other answer of the endpoint has an empty body (07 2.7.2).
    /// </summary>
    string Acknowledgement { get; }

    /// <summary>
    /// The link layer's signature check over the form fields (08 3.2 principle 1; 09 11.1 item 5): a local computation that compares the
    /// computed signature with the one that came in fixed time (<see cref="Integrations.SignatureComparison"/>). It returns the verified notice,
    /// or null when the signature is missing or does not match. It runs before anything is written and outside the transaction.
    /// </summary>
    TNotice? Verify(IReadOnlyDictionary<string, string> formFields);

    /// <summary>
    /// Writes the verified notice - the inbound record and the job row that processes it - into the caller's open transaction and does not save
    /// (05 4.5.2, 4.3.1; 09 9.1 item 3): the pipeline's transaction commits it. The same notice that is already recorded is
    /// <see cref="InboundRecordOutcome.AlreadyRecorded"/> and writes nothing (K-970); a duplicate the unique index catches at the insert is the
    /// same outcome, so the insert does not fail on it (<c>INSERT ... ON CONFLICT DO NOTHING</c>).
    /// </summary>
    Task<InboundRecordOutcome> RecordAsync(TNotice notice, CancellationToken ct);
}

/// <summary>What recording a verified notice came to (08 3.2 principle 3). Both classes are answered "received".</summary>
internal abstract record InboundRecordOutcome : IOperationResult
{
    private InboundRecordOutcome()
    {
    }

    /// <summary>A refusal rolls the transaction back; neither class is one - a duplicate simply wrote nothing.</summary>
    public bool IsRejected => false;

    /// <summary>The notice was written with its job row; the answer waits for the commit.</summary>
    public sealed record Recorded : InboundRecordOutcome;

    /// <summary>The same notice is already recorded: nothing is written and nothing is processed again.</summary>
    public sealed record AlreadyRecorded : InboundRecordOutcome;
}
