using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The last defense of the product definition's type rules (06 4.5 "Uygulamada"; 02 3.2.2, 7.3.4; P-44): a save that writes a product is refused, before any SQL is sent,
/// when the product carries a withdrawal exemption reason without being a physical product, when it is a published service without a fulfilment period, when it is a
/// physical product without its place of origin or when it is a service carrying one (T336). The rules
/// themselves are <see cref="ProductDefinitionRules"/>; this guard only reads the tracked product and applies them, so no path that saves a product can skip them. The
/// full publication gate with its named missing conditions is the publication task's; this check is the one condition the type adds to it. A product written by a bulk
/// statement does not pass through a save and is not covered here.
/// </summary>
internal sealed class ProductDefinitionGuard : SaveChangesInterceptor
{
    public static ProductDefinitionGuard Instance { get; } = new();

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Check(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Check(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Check(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        foreach (EntityEntry entry in context.ChangeTracker.Entries().Where(e => e.State is EntityState.Added or EntityState.Modified))
        {
            if (entry.Entity is not Product product)
            {
                continue;
            }

            foreach (ProductDefinitionProblem problem in ProductDefinitionRules.Check(
                product.ProductType, product.PublishStatus, product.WithdrawalExemptionReason, product.FulfilmentPeriod, product.OriginCountryId, product.ShippingPeriod))
            {
                throw new InvalidOperationException(problem switch
                {
                    ProductDefinitionProblem.ExemptionOutsideAPhysicalProduct =>
                        $"Product {product.Id} carries a withdrawal exemption reason but is not a physical product: the exemption is only for physical goods (02 7.3.4; 06 4.5).",
                    ProductDefinitionProblem.PhysicalProductWithoutOrigin =>
                        $"Product {product.Id} is a physical product without its place of origin: the field is required on a physical product (02 3.8.5; 06 4.5).",
                    ProductDefinitionProblem.OriginOnAService =>
                        $"Product {product.Id} is a service carrying a place of origin: a service has no such field (02 3.8.5; 04 9.5.22).",
                    ProductDefinitionProblem.ShippingPeriodOutsideTheFence =>
                        $"Product {product.Id} has a shipping period of {product.ShippingPeriod} business days, outside the fence of P-6 (02 3.20.4; 11.1).",
                    _ =>
                        $"Product {product.Id} is a service published without a fulfilment period: the gate asks for it and it has no default (02 3.2.2, 3.7.4; P-44).",
                });
            }
        }
    }
}
