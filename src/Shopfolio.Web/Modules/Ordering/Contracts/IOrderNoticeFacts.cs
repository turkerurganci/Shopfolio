namespace Shopfolio.Modules.Ordering.Contracts;

/// <summary>
/// What the body of a customer notice reads from the order when it is made (T385; 08 2.2.6 items 2-4; 05 4.4.2): the order's number, the access key and the two flags that choose the
/// main button's address, the frozen bank transfer IBAN (B-2, B-3) and whether the order has a digital item (B-4). Read from the order alone; nothing here is personal data of the customer.
/// </summary>
public sealed record OrderNoticeFacts(
    string OrderNumber,
    string? AccessKey,
    bool IsGuestOrder,
    bool IsAccountDeleted,
    string? BankTransferIban,
    bool HasDigitalItem);

/// <summary>
/// The Notifications module's read of an order for a customer notice's body (T385; 09 6 item 2: another module is reached through its Contracts only; D-232). A plain read: it opens no
/// transaction and takes no lock.
/// </summary>
public interface IOrderNoticeFacts
{
    /// <summary>The facts of the order's notices, or null when the order does not exist (a row whose order is gone makes no body).</summary>
    Task<OrderNoticeFacts?> GetAsync(Guid orderId, CancellationToken ct);
}
