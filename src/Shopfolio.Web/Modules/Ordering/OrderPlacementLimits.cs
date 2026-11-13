using System.Net;

using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The two limits a new order's confirmation meets (02 8.2, 8.2.2, 8.2.3, 8.2.4; T377), asked together at the confirmation's one gate. Both are counted from the orders and both
/// count on two axes separately - the client's IP and, for a signed-in member only, the account's e-mail -; whichever axis is full stops the order, and the other axis is not asked
/// to agree. <b>L-7</b> (<see cref="UnpaidCancellationLimit"/>) stops any new order whatever its payment method: the user "cannot confirm a new order for a while".
/// <b>L-8</b> (<see cref="OpenUnpaidOrderLimit"/>) closes the bank transfer only: a full ceiling leaves the card open, because a card order's hold lasts minutes (02 8.2.3; 3.21.5; 6.2.21).
/// The administrator is not exempt (02 8.2.4): the gate takes no account type - an administrator has no cart and no order (02 10.2.5), and what a signed-in customer meets here is
/// what any member meets. Nothing here counts the cart's items or the stock lookups: they have no limit (02 8.2.5).
/// </summary>
internal sealed class OrderPlacementLimits
{
    private readonly UnpaidCancellationLimit _cancellations;
    private readonly OpenUnpaidOrderLimit _open;

    public OrderPlacementLimits(UnpaidCancellationLimit cancellations, OpenUnpaidOrderLimit open)
    {
        ArgumentNullException.ThrowIfNull(cancellations);
        ArgumentNullException.ThrowIfNull(open);
        _cancellations = cancellations;
        _open = open;
    }

    /// <summary>
    /// The limit that stops a new order, or null when none does. <paramref name="memberId"/> is the customer id of a signed-in member - the e-mail axis of the account, read by
    /// the id (D-808) - and null for a guest, who is counted on the IP axis only (02 8.2.2). L-7 is asked first - it stops every method; L-8 only when the method is the bank transfer.
    /// </summary>
    public async Task<AttemptLimit?> FindExceededAsync(PaymentMethod method, IPAddress? address, Guid? memberId, CancellationToken ct)
    {
        if (await _cancellations.IsFullByIpAsync(address, ct) || (memberId is { } cancelling && await _cancellations.IsFullByMemberAsync(cancelling, ct)))
        {
            return AttemptLimit.L7;
        }

        if (method == PaymentMethod.BankTransfer && await ClosesBankTransferAsync(address, memberId, null, ct))
        {
            return AttemptLimit.L8;
        }

        return null;
    }

    /// <summary>
    /// L-8's verdict on the bank transfer (02 8.2.3): true when either axis is full - the IP axis, and for a signed-in member the account's. The payment step's read asks it with the
    /// cart's id <paramref name="exceptCartId"/>, whose placed unpaid orders the confirmation cancels before it asks (K-837), so the list shows the transfer unselectable exactly
    /// when the confirmation would refuse it (07 4.13.1; 04 5.13.8); the confirmation asks it without, after that cancellation.
    /// </summary>
    public async Task<bool> ClosesBankTransferAsync(IPAddress? address, Guid? memberId, Guid? exceptCartId, CancellationToken ct) =>
        await _open.IsFullByIpAsync(address, exceptCartId, ct) || (memberId is { } member && await _open.IsFullByMemberAsync(member, exceptCartId, ct));
}
