using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Disposal;
using Shopfolio.Infrastructure.Forms;
using Shopfolio.Infrastructure.Jobs;
using Shopfolio.Infrastructure.Routing;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>The Ordering module's implementation of its open surface; internal, reached only through <see cref="IOrderingModule"/>.</summary>
internal sealed class OrderingModule : IOrderingModule
{
    private readonly AppDbContext _db;

    private readonly OrderLocks _locks;

    public OrderingModule(AppDbContext db, OrderLocks locks)
    {
        _db = db;
        _locks = locks;
    }

    public async Task<bool> LockOrderAsync(Guid orderId, int? shownVersion, CancellationToken ct)
    {
        await _locks.LockAsync([orderId], ct);
        if (shownVersion is not int shown)
        {
            return true;
        }

        Order order = await _db.Orders.AsNoTracking().SingleAsync(o => o.Id == orderId, ct);
        return ShownVersion.Matches(order, shown);
    }

    public async Task<Guid?> FindMemberOrderAsync(string orderNumber, Guid customerId, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderNumber);
        string number = OrderRoutes.NumberForLookup(orderNumber);

        // A guest order has no account link, so no customer id matches it (07 2.3.4).
        return await _db.Orders.AsNoTracking()
            .Where(o => o.OrderNumber == number && o.CustomerId == customerId)
            .Select(o => (Guid?)o.Id)
            .SingleOrDefaultAsync(ct);
    }

    public async Task<Guid?> FindOrderByAccessKeyAsync(string accessKey, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(accessKey);
        if (accessKey.Length == 0)
        {
            return null;
        }

        return await _db.Orders.AsNoTracking()
            .Where(o => o.OrderAccessKey == accessKey)
            .Select(o => (Guid?)o.Id)
            .SingleOrDefaultAsync(ct);
    }
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddOrderingModule</c> once.</summary>
internal static class OrderingModuleRegistration
{
    public static IServiceCollection AddOrderingModule(this IServiceCollection services)
    {
        services.AddScoped<OrderLocks>();
        services.AddOutageEffect<TransferWindowDeferral>();
        services.AddScoped<IOrderingModule, OrderingModule>();
        services.AddDisposalPart<GuestCartDisposalPart>();
        return services;
    }
}
