namespace Shopfolio.Modules.Notifications.Contracts;

/// <summary>
/// What the correction of a guest order's contact e-mail needs from the dispatch records (T375; 02 10.4.11 (2), (3), 9.1.6; 06 4.58): the order's first B-1 - the root the B-1 after a correction
/// joins - and the redirection of the order's notices that are not sent yet. Both join the caller's transaction (09 9.1 item 3); the caller holds the order's row lock (09 9.1 item 5).
/// </summary>
public interface IOrderContactNotices
{
    /// <summary>
    /// The row id of the order's first B-1 (the chain's root: the earliest B-1 row that is not itself a re-sending); empty when the order has none left - the row may be gone by its own
    /// period - and the B-1 of the correction is then a fresh e-mail without a chain.
    /// </summary>
    Task<Guid?> FindFirstOrderReceivedAsync(Guid orderId, CancellationToken ct);

    /// <summary>
    /// Points the order's customer notices that are queued - not sent yet, or waiting for a new attempt - at <paramref name="newEmail"/> (02 10.4.11 (2); K-1138): the body is made when the row
    /// is sent, with the new access key, so nothing of the old link goes to the new address and nothing meant for the owner goes to the old one. B-15 is left out - it is the one notice that
    /// goes to the old address - and a row that was sent or marked undelivered is not touched. Returns the number of rows pointed anew.
    /// </summary>
    Task<int> ReroutePendingCustomerNoticesAsync(Guid orderId, string newEmail, CancellationToken ct);
}
