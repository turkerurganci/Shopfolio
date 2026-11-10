using Microsoft.EntityFrameworkCore;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Catalog.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>The read of an order for a customer notice's body (T385; 08 2.2.6; D-232): a plain read of the order's own columns, joined to no transaction.</summary>
internal sealed class OrderNoticeFactsReader : IOrderNoticeFacts
{
    private readonly AppDbContext _db;

    public OrderNoticeFactsReader(AppDbContext db)
    {
        ArgumentNullException.ThrowIfNull(db);
        _db = db;
    }

    public async Task<OrderNoticeFacts?> GetAsync(Guid orderId, CancellationToken ct)
    {
        var order = await _db.Orders.AsNoTracking()
            .Where(o => o.Id == orderId)
            .Select(o => new { o.OrderNumber, o.OrderAccessKey, o.IsGuestOrder, o.IsAccountDeleted, o.PaymentMethod, o.BankTransferIban })
            .SingleOrDefaultAsync(ct);
        if (order is null)
        {
            return null;
        }

        bool hasDigital = await _db.OrderItems.AsNoTracking().AnyAsync(i => i.OrderId == orderId && i.ProductType == ProductType.Digital, ct);
        string? iban = order.PaymentMethod == PaymentMethod.BankTransfer ? order.BankTransferIban : null;
        return new OrderNoticeFacts(order.OrderNumber, order.OrderAccessKey, order.IsGuestOrder, order.IsAccountDeleted, iban, hasDigital);
    }
}
