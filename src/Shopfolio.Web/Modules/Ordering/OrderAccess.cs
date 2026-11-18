using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The ways into the order page and what the page carries (02 3.22.3-3.22.5; 07 2.3.4; T354). Three ways open an order: the member's history
/// (<see cref="IOrderingModule.FindMemberOrderAsync"/>: the account owns the order), the guest's number with the e-mail (<see cref="FindByNumberAndEmailAsync"/>) and the
/// access key of the e-mail link (<see cref="IOrderingModule.FindOrderByAccessKeyAsync"/>). The number alone opens nothing: it is on invoices and in letters
/// (02 3.22.5), so every way that takes it also takes a second thing. The failed-lookup limits (L-5, two axes) are the attempt layer's (T376), not this class's.
/// All of it is plain reads; nothing here writes.
/// </summary>
internal sealed class OrderAccess
{
    private readonly AppDbContext _db;

    public OrderAccess(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    /// <summary>
    /// The guest's lookup (07 UC-32): the order with that number whose contact e-mail, as it was frozen on the order, is that address. The number is matched without regard
    /// to letter case (07 2.1.7) and the address in its normalized form (06 2.8.2). The frozen address is what matches - a deleted account's order and an order of an account
    /// whose address changed match by it too (04 5.15.6); a guest order whose address was mistyped is not found by the right one. An order whose personal data is disposed has
    /// no address and no key and is never found. Any missing, empty or wrong part gives the same null: the caller says nothing about which part, or whether the order exists.
    /// </summary>
    public async Task<OrderAccessLookup?> FindByNumberAndEmailAsync(string orderNumber, string email, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderNumber);
        ArgumentNullException.ThrowIfNull(email);
        if (string.IsNullOrWhiteSpace(orderNumber) || string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        string number = OrderRoutes.NumberForLookup(orderNumber.Trim());
        string normalized = TextNormalizer.NormalizeIdentity(email);
        var found = await _db.Orders.AsNoTracking()
            .Where(o => o.OrderNumber == number && o.NormalizedContactEmail == normalized && o.OrderAccessKey != null)
            .Select(o => new { o.Id, o.OrderAccessKey })
            .SingleOrDefaultAsync(ct);

        // The query keeps only orders that hold a key, so the pattern below always matches a found row.
        return found is { OrderAccessKey: string accessKey } ? new OrderAccessLookup(found.Id, accessKey) : null;
    }

    /// <summary>
    /// The four things the page carries besides its lists (02 3.22.4): the status, the tracking link, the download buttons and the frozen IBAN; null when the order does not
    /// exist. The order is already opened by one of the three ways, so nothing here asks for an e-mail or waits for one (02 6.1.1).
    /// </summary>
    public async Task<OrderPageSummary?> GetSummaryAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new
            {
                o.Id,
                o.OrderNumber,
                o.OrderStatus,
                o.PaymentStatus,
                o.PaymentMethod,
                o.BankTransferIban,
                o.PaymentWindowEndsAt,
                o.Carrier,
                o.OtherCarrierName,
                o.TrackingNumber,
                o.IsOwnVehicleDelivery,
            })
            .SingleOrDefaultAsync(ct);
        if (order is null)
        {
            return null;
        }

        // A button exists for a digital item of a confirmed payment (02 3.12.1); it is open while the counter is below the allowance frozen at the order (02 3.12.5).
        List<OrderDownloadButton> downloads = [];
        if (DownloadRules.IsPaymentConfirmed(order.PaymentStatus))
        {
            var items = await _db.OrderItems.AsNoTracking()
                .Where(i => i.OrderId == orderId && i.ProductType == ProductType.Digital)
                .OrderBy(i => i.CreatedAt).ThenBy(i => i.Id)
                .Select(i => new { i.Id, i.ProductName, i.DownloadCount, i.DownloadAllowance })
                .ToListAsync(ct);
            downloads = [.. items.Select(i => new OrderDownloadButton(i.Id, i.ProductName, i.DownloadAllowance is int allowance && i.DownloadCount < allowance))];
        }

        string? iban = order.PaymentMethod == PaymentMethod.BankTransfer ? order.BankTransferIban : null;
        ShippingDisplay shipping = CarrierCatalog.Default.Describe(order.Carrier, order.OtherCarrierName, order.TrackingNumber, order.IsOwnVehicleDelivery);
        DateTimeOffset? transferEndsAt = order.PaymentMethod == PaymentMethod.BankTransfer && order.PaymentStatus == PaymentStatus.Pending && order.OrderStatus == OrderStatus.Placed
            ? order.PaymentWindowEndsAt
            : null;
        return new OrderPageSummary(order.Id, order.OrderNumber, order.OrderStatus, order.PaymentStatus, shipping, downloads, iban, transferEndsAt);
    }
}
