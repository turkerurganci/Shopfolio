using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The one dated, percentage discount of a product (06 4.12): it applies to every variant, each on its own price. A new definition replaces
/// the row; the trace of the earlier discounts is in the price history and the frozen order values. The discount is a part of the product form
/// and is covered by the product's version. It goes with its product.
/// </summary>
internal sealed class Discount
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid ProductId { get; set; }

    /// <summary>The discount rate in percent: above 0, below 100.</summary>
    public decimal Percentage { get; set; }

    /// <summary>The first day; the range starts at 00.00 of this day, Turkey time (06 2.5.2).</summary>
    public DateOnly StartDate { get; set; }

    /// <summary>The last day, included; the range ends at 00.00 of the next day (06 2.5.2).</summary>
    public DateOnly EndDate { get; set; }

    public static Discount Create(Guid productId, decimal percentage, DateOnly startDate, DateOnly endDate, DateTimeOffset now)
    {
        return new Discount { Id = EntityIds.New(), CreatedAt = now, ProductId = productId, Percentage = percentage, StartDate = startDate, EndDate = endDate };
    }
}

internal sealed class DiscountConfiguration : IEntityTypeConfiguration<Discount>
{
    public void Configure(EntityTypeBuilder<Discount> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Percentage).AsPercent();
        builder.HasOne<Product>().WithMany().HasForeignKey(d => d.ProductId).OnDelete(DeleteBehavior.Cascade);

        // One discount per product.
        builder.HasIndex(d => d.ProductId).IsUnique();

        builder.HasRule("percentage_range", "percentage > 0 AND percentage < 100");
        builder.HasRule("end_not_before_start", "end_date >= start_date");
    }
}
