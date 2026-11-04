using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;

namespace Shopfolio.Modules.Catalog;

/// <summary>
/// The code entered at the payment step that reduces the basket total (06 4.13): a percentage or a fixed amount, bounded by a date range and a
/// total usage count. The coupon type is read from which of the two values is filled; there is no type field. A coupon is never deleted
/// (<see cref="CatalogRecordsGuard"/>); the links that will point to it (the order's coupon link, the coupon reservation) are restrictive.
/// That used and reserved rights together do not exceed the total is a condition of the conditional updates, not a database constraint
/// (06 4.13; K-1042 (2)).
/// </summary>
internal sealed class Coupon
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The code as entered; shown as entered. Written only with its normalized form by <see cref="SetCode"/>.</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>The normalized code: the column of the unique index and of the match (06 2.8.2; 09 9.4 item 1).</summary>
    public string NormalizedCode { get; private set; } = string.Empty;

    /// <summary>The value of a percentage coupon: above 0, below 100; empty on a fixed-amount coupon.</summary>
    public decimal? Percentage { get; set; }

    /// <summary>The amount of a fixed-amount coupon: above zero; empty on a percentage coupon.</summary>
    public decimal? Amount { get; set; }

    /// <summary>The minimum basket amount: required on a fixed-amount coupon and above its amount; optional on a percentage coupon.</summary>
    public decimal? MinimumBasketAmount { get; set; }

    public DateOnly StartDate { get; set; }

    /// <summary>The last day, included (06 2.5.2); a campaign is stopped by moving it earlier.</summary>
    public DateOnly EndDate { get; set; }

    /// <summary>The total usage count; a personal code is a code with a count of 1.</summary>
    public int TotalUsageLimit { get; set; }

    /// <summary>The rights of paid orders; written only by the transition unit (06 2.7.1, 4.14).</summary>
    public int UsedCount { get; private set; }

    /// <summary>The rights of orders waiting for payment; written only by the transition unit (06 2.7.1, 4.14).</summary>
    public int ReservedCount { get; private set; }

    /// <summary>The version of the coupon form (06 2.6.2); the rights do not raise it. A concurrency token.</summary>
    public int Version { get; private set; } = 1;

    public static Coupon CreatePercentage(string code, decimal percentage, DateOnly startDate, DateOnly endDate, int totalUsageLimit, DateTimeOffset now)
    {
        Coupon coupon = new() { Id = EntityIds.New(), CreatedAt = now, Percentage = percentage, StartDate = startDate, EndDate = endDate, TotalUsageLimit = totalUsageLimit };
        coupon.SetCode(code);
        return coupon;
    }

    public static Coupon CreateAmount(
        string code,
        decimal amount,
        decimal minimumBasketAmount,
        DateOnly startDate,
        DateOnly endDate,
        int totalUsageLimit,
        DateTimeOffset now)
    {
        Coupon coupon = new()
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            Amount = amount,
            MinimumBasketAmount = minimumBasketAmount,
            StartDate = startDate,
            EndDate = endDate,
            TotalUsageLimit = totalUsageLimit,
        };
        coupon.SetCode(code);
        return coupon;
    }

    /// <summary>Writes the code and its normalized form together (06 2.8.2).</summary>
    public void SetCode(string code)
    {
        ArgumentNullException.ThrowIfNull(code);
        Code = code;
        NormalizedCode = TextNormalizer.NormalizeIdentity(code);
    }

    public void BumpVersion() => Version++;
}

internal sealed class CouponConfiguration : IEntityTypeConfiguration<Coupon>
{
    public void Configure(EntityTypeBuilder<Coupon> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Code).HasColumnType("text");
        builder.Property(c => c.NormalizedCode).HasColumnType("text");
        builder.Property(c => c.Percentage).AsPercent();
        builder.Property(c => c.Amount).AsMoney();
        builder.Property(c => c.MinimumBasketAmount).AsMoney();
        builder.Property(c => c.UsedCount).HasDefaultValue(0);
        builder.Property(c => c.ReservedCount).HasDefaultValue(0);
        builder.Property(c => c.Version).HasDefaultValue(1);

        builder.HasIndex(c => c.NormalizedCode).IsUnique();

        builder.HasRule("exactly_one_value", "(percentage IS NULL) <> (amount IS NULL)");
        builder.HasRule("percentage_range", "percentage IS NULL OR (percentage > 0 AND percentage < 100)");
        builder.HasRule("amount_above_zero", "amount IS NULL OR amount > 0");
        builder.HasRule("amount_needs_larger_minimum_basket", "amount IS NULL OR (minimum_basket_amount IS NOT NULL AND minimum_basket_amount > amount)");
        builder.HasRule("minimum_basket_amount_not_negative", "minimum_basket_amount IS NULL OR minimum_basket_amount >= 0");
        builder.HasRule("end_not_before_start", "end_date >= start_date");
        builder.HasRule("usage_counts_not_negative", "total_usage_limit >= 0 AND used_count >= 0 AND reserved_count >= 0");
    }
}
