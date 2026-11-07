using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Shopfolio.Infrastructure;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>The Payment module's implementation of its open surface; internal, reached only through <see cref="IPaymentModule"/>.</summary>
internal sealed class PaymentModule : IPaymentModule
{
    private readonly AppDbContext _db;

    public PaymentModule(AppDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<PaymentSource>> FindPaymentSourcesAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        if (orderIds.Count == 0)
        {
            return [];
        }

        Guid[] ids = [.. orderIds];
        return await _db.PaymentRecords.AsNoTracking()
            .Where(p => ids.Contains(p.OrderId))
            .Select(p => new PaymentSource(p.OrderId, p.TransitionRecordId))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Guid>> ErasePersonalDataAsync(IReadOnlyCollection<Guid> orderIds, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(orderIds);
        if (_db.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("The refunds' personal data is emptied inside the caller's transaction (09 6 item 3).");
        }

        if (orderIds.Count == 0)
        {
            return [];
        }

        // Every refund of the orders is loaded (the caller needs all their ids) and tracked: the order side's version guard reads the tracked rows
        // (09 10.2). Only the administrator link and name are emptied (06 9.2.1); the amount, the type, the links and the moments stay.
        Guid[] ids = [.. orderIds];
        List<Refund> refunds = await _db.Refunds.Where(r => ids.Contains(r.OrderId)).ToListAsync(ct);
        foreach (Refund refund in refunds)
        {
            refund.AdminId = null;
            refund.AdminName = null;
        }

        return [.. refunds.Select(r => r.Id)];
    }
}

/// <summary>The module's single registration (09 6 item 3): <c>Program.cs</c> calls <c>AddPaymentModule</c> once.</summary>
internal static class PaymentModuleRegistration
{
    public static IServiceCollection AddPaymentModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentModule, PaymentModule>();
        return services;
    }
}
