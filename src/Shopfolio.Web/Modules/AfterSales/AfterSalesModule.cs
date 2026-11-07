using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.AfterSales.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>The AfterSales module's implementation of its open surface; internal, reached only through <see cref="IAfterSalesModule"/>.</summary>
internal sealed class AfterSalesModule : IAfterSalesModule
{
    private readonly AppDbContext _db;

    public AfterSalesModule(AppDbContext db)
    {
        _db = db;
    }

    public async Task ErasePersonalDataAsync(OrderPersonalDataScope scope, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(scope);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The after-sales records' personal data is emptied inside the caller's transaction (09 6 item 3).");
        }

        if (scope.OrderIds.Count == 0)
        {
            return;
        }

        Guid[] orderIds = [.. scope.OrderIds];
        Guid[] itemIds = [.. scope.OrderItemIds];
        Guid[] refundIds = [.. scope.RefundIds];

        // The records are loaded and tracked, never written by a bulk statement: the order side's version guard reads the tracked rows and finds
        // the order of each through its tracked item or refund (09 10.2; 06 2.6.4). The ids are the caller's: its items and refunds are tracked.
        foreach (Cancellation cancellation in await _db.Cancellations.Where(c => itemIds.Contains(c.OrderItemId)).ToListAsync(ct))
        {
            cancellation.AdminId = null;
            cancellation.AdminName = null;
            cancellation.ReasonNote = null;
            cancellation.RefundIbanId = null;
        }

        foreach (ItemRemoval removal in await _db.ItemRemovals.Where(r => itemIds.Contains(r.OrderItemId)).ToListAsync(ct))
        {
            removal.AdminId = null;
            removal.AdminName = null;
        }

        foreach (LateDeliveryTermination termination in await _db.LateDeliveryTerminations.Where(t => itemIds.Contains(t.OrderItemId)).ToListAsync(ct))
        {
            termination.AdminId = null;
            termination.AdminName = null;
            termination.RefundIbanId = null;
        }

        foreach (Withdrawal withdrawal in await _db.Withdrawals.Where(w => itemIds.Contains(w.OrderItemId)).ToListAsync(ct))
        {
            withdrawal.AdminId = null;
            withdrawal.AdminName = null;
            withdrawal.RefundIbanId = null;
        }

        foreach (Return receipt in await _db.Returns.Where(r => itemIds.Contains(r.OrderItemId)).ToListAsync(ct))
        {
            receipt.AdminId = null;
            receipt.AdminName = null;
        }

        foreach (ReturnRejection rejection in await _db.ReturnRejections.Where(r => itemIds.Contains(r.OrderItemId)).ToListAsync(ct))
        {
            rejection.AdminId = null;
            rejection.AdminName = null;
        }

        foreach (NotReturnedClosure closure in await _db.NotReturnedClosures.Where(c => itemIds.Contains(c.OrderItemId)).ToListAsync(ct))
        {
            closure.AdminId = null;
            closure.AdminName = null;
        }

        // The IBAN requests (linked to an item or to a refund) and the standing IBANs of the orders are deleted; the links to them were emptied above.
        List<RefundIbanRequest> requests = await _db.RefundIbanRequests
            .Where(r => (r.OrderItemId != null && itemIds.Contains(r.OrderItemId.Value)) || (r.RefundId != null && refundIds.Contains(r.RefundId.Value)))
            .ToListAsync(ct);
        _db.RefundIbanRequests.RemoveRange(requests);

        List<RefundIban> ibans = await _db.RefundIbans.Where(i => orderIds.Contains(i.OrderId)).ToListAsync(ct);
        _db.RefundIbans.RemoveRange(ibans);
    }
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddAfterSalesModule</c> once.</summary>
internal static class AfterSalesModuleRegistration
{
    public static IServiceCollection AddAfterSalesModule(this IServiceCollection services)
    {
        services.AddScoped<IAfterSalesModule, AfterSalesModule>();
        return services;
    }
}
