using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.CorporateContent.Contracts;

namespace Shopfolio.Modules.Settings;

/// <summary>
/// The settings the company changes from the panel (06 4.2): one row, section by section. Each independently saved section carries its own
/// version column; those versions are plain integers and not concurrency tokens, because a section save must not depend on the other
/// sections' versions (09 10.3 item 1). The row is therefore read without tracking and written by the one section writer, never by
/// <c>SaveChanges</c>. The row is never deleted.
/// </summary>
internal sealed class CompanySettings
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    // Payment methods
    public bool IsBankTransferEnabled { get; set; }

    public string? BankTransferIban { get; set; }

    public int PaymentMethodsVersion { get; set; } = 1;

    // Shipping fee and thresholds
    public decimal ShippingFee { get; set; }

    public decimal? FreeShippingThreshold { get; set; }

    public decimal? MinimumOrderAmount { get; set; }

    public int ShippingFeeVersion { get; set; } = 1;

    // Delivery provinces (the rows are CompanyDeliveryProvince)
    public int DeliveryProvincesVersion { get; set; } = 1;

    // Periods
    public int DefaultShippingPeriod { get; set; }

    public int BankTransferPaymentWindow { get; set; }

    public int PeriodsVersion { get; set; } = 1;

    // Product defaults
    public int DownloadAllowance { get; set; }

    public decimal DefaultVatRate { get; set; }

    public int ProductDefaultsVersion { get; set; } = 1;

    // Return address
    public string? ReturnRecipientName { get; set; }

    public Guid? ReturnProvinceId { get; set; }

    public string? ReturnDistrict { get; set; }

    public string? ReturnAddressLine { get; set; }

    /// <summary>Ten digits (06 2.5.1); optional.</summary>
    public string? ReturnPhone { get; set; }

    public int ReturnAddressVersion { get; set; } = 1;

    // Homepage and menu
    public HomepageLayout HomepageLayout { get; set; }

    public string MenuNameProducts { get; set; } = string.Empty;

    public string MenuNameOfferings { get; set; } = string.Empty;

    public string MenuNamePortfolioItems { get; set; } = string.Empty;

    public string MenuNameAboutPage { get; set; } = string.Empty;

    public string MenuNameFaqItems { get; set; } = string.Empty;

    public string MenuNameContact { get; set; } = string.Empty;

    /// <summary>Ten digits (06 2.5.1); optional.</summary>
    public string? WhatsAppNumber { get; set; }

    public int HomepageVersion { get; set; } = 1;

    /// <summary>The "close sales temporarily" switch; it carries no version (06 2.6.3).</summary>
    public bool IsSalesTemporarilyClosed { get; set; }

    public static CompanySettings Create(DateTimeOffset now)
    {
        return new CompanySettings { Id = EntityIds.New(), CreatedAt = now };
    }
}

internal sealed class CompanySettingsConfiguration : IEntityTypeConfiguration<CompanySettings>
{
    public void Configure(EntityTypeBuilder<CompanySettings> builder)
    {
        builder.HasKey(s => s.Id);
        builder.AsSingleRow();
        builder.Property(s => s.ShippingFee).AsMoney();
        builder.Property(s => s.FreeShippingThreshold).AsMoney();
        builder.Property(s => s.MinimumOrderAmount).AsMoney();
        builder.Property(s => s.DefaultVatRate).AsPercent();
        builder.Property(s => s.ReturnPhone).HasMaxLength(10);
        builder.Property(s => s.WhatsAppNumber).HasMaxLength(10);
        builder.HasOne<Province>().WithMany().HasForeignKey(s => s.ReturnProvinceId).OnDelete(DeleteBehavior.NoAction);


        builder.HasRule("bank_transfer_iban_required", "NOT is_bank_transfer_enabled OR bank_transfer_iban IS NOT NULL");
        builder.HasRule(
            "return_address_all_or_none",
            "(return_recipient_name IS NULL) = (return_province_id IS NULL) AND (return_province_id IS NULL) = (return_district IS NULL) "
            + "AND (return_district IS NULL) = (return_address_line IS NULL)");
        builder.HasRule(
            "amounts_not_negative",
            "shipping_fee >= 0 AND (free_shipping_threshold IS NULL OR free_shipping_threshold >= 0) AND (minimum_order_amount IS NULL OR minimum_order_amount >= 0)");
        builder.HasRule("default_shipping_period_fence", $"default_shipping_period BETWEEN 0 AND {ParameterFences.ShippingPeriodUpper}");
        builder.HasRule(
            "bank_transfer_payment_window_fence",
            $"bank_transfer_payment_window BETWEEN {ParameterFences.BankTransferPaymentWindowLower} AND {ParameterFences.BankTransferPaymentWindowUpper}");
        builder.HasRule("download_allowance_fence", $"download_allowance >= {ParameterFences.DownloadAllowanceLower}");
        builder.HasRule("default_vat_rate_range", "default_vat_rate >= 0 AND default_vat_rate < 100");
        builder.HasRule("return_phone_ten_digits", "return_phone ~ '^[0-9]{10}$'");
        builder.HasRule("whats_app_number_ten_digits", "whats_app_number ~ '^[0-9]{10}$'");

        // The single row (06 2.3.2) is kept by a unique index on a constant expression; EF cannot model it, so the migration writes it.
    }
}
