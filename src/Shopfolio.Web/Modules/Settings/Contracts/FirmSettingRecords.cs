namespace Shopfolio.Modules.Settings.Contracts;

/// <summary>The values the shipping fee form sends (02 11.1 P-1, P-3, P-4; 04 7.2.21.1) without the section version, which travels apart. The fee is required; an empty threshold is "Boş" (the feature is off).</summary>
public sealed record ShippingFeeInput(decimal? ShippingFee, decimal? FreeShippingThreshold, decimal? MinimumOrderAmount);

/// <summary>The shipping fee section as stored (06 4.2): the fee with VAT included, the two optional amounts and the section's own version.</summary>
public sealed record ShippingFeeSection(decimal ShippingFee, decimal? FreeShippingThreshold, decimal? MinimumOrderAmount, int Version);

/// <summary>The values the periods form sends (02 11.1 P-5, P-7; 04 7.2.21.4): business days, without the section version.</summary>
public sealed record PeriodsInput(int? DefaultShippingPeriod, int? BankTransferPaymentWindow);

/// <summary>The periods section as stored (06 4.2): the firm default of the shipping period (P-5), the bank transfer payment window (P-7) and the section's version.</summary>
public sealed record PeriodsSection(int DefaultShippingPeriod, int BankTransferPaymentWindow, int Version);

/// <summary>The values the product defaults form sends (02 11.1 P-8, P-10; 04 7.2.21.5), without the section version.</summary>
public sealed record ProductDefaultsInput(int? DownloadAllowance, decimal? DefaultVatRate);

/// <summary>The product defaults section as stored (06 4.2): the download allowance per order line (P-8), the VAT rate default (P-10) and the section's version.</summary>
public sealed record ProductDefaultsSection(int DownloadAllowance, decimal DefaultVatRate, int Version);
