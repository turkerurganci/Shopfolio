using System.Net;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Modules.Settings.Contracts;

namespace Shopfolio.Modules.Ordering;

/// <summary>
/// The order (06 4.17): born at confirmation as Placed + Pending, it freezes its addresses, contact email, the firm's legal identity of that
/// day, the three legal text versions, the bank transfer IBAN and the shipping promise. The order is never deleted
/// (<see cref="OrderingRecordsGuard"/>): its personal fields are emptied by the disposal, and an empty contact email says the order is disposed.
/// The database keeps the forbidden pairs of the two status axes, the fields the disposal empties together, the deleted account's empty link,
/// the payment method's IBAN and the carrier fields. The status fields are written only by the transition unit (09 9.1 item 4); the
/// normalized email only with the email (09 9.4 item 1); the two document copies once (09 9.3 item 1).
/// </summary>
internal sealed class Order : IVersionedRecord
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>The order number: the year and a random block (P-19); kept after the disposal.</summary>
    public string OrderNumber { get; set; } = string.Empty;

    /// <summary>The access key: at least 128 random bits, stored as it is (06 4.17; K-1114); emptied by the disposal.</summary>
    public string? OrderAccessKey { get; set; }

    /// <summary>The single-use confirmation key of the payment step (05 4.5.1); kept after the disposal.</summary>
    public string ConfirmationKey { get; set; } = string.Empty;

    /// <summary>The shipment axis; written only by the transition unit (09 9.1 item 4).</summary>
    public OrderStatus OrderStatus { get; private set; } = OrderStatus.Placed;

    /// <summary>The payment axis; written only by the transition unit (09 9.1 item 4).</summary>
    public PaymentStatus PaymentStatus { get; private set; } = PaymentStatus.Pending;

    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>The account link; emptied by the database when the account is deleted and by the disposal.</summary>
    public Guid? CustomerId { get; set; }

    /// <summary>Whether the order was placed without signing in; separate from the account link.</summary>
    public bool IsGuestOrder { get; set; }

    /// <summary>Whether the linked account was deleted; such an order is never linked again (06 4.17; K-1116).</summary>
    public bool IsAccountDeleted { get; set; }

    /// <summary>The contact email as entered. Written only with its normalized form by <see cref="SetContactEmail"/>; empty means disposed.</summary>
    public string? ContactEmail { get; private set; }

    /// <summary>The normalized contact email: the column of every match (06 2.8.2; 09 9.4 item 1); not unique.</summary>
    public string? NormalizedContactEmail { get; private set; }

    public string? ShippingRecipientName { get; set; }

    /// <summary>Ten digits, without the leading zero or "+90" (06 2.5.1).</summary>
    public string? ShippingPhone { get; set; }

    public Guid? ShippingProvinceId { get; set; }

    public string? ShippingDistrict { get; set; }

    public string? ShippingAddressLine { get; set; }

    public string? BillingRecipientName { get; set; }

    public Guid? BillingProvinceId { get; set; }

    public string? BillingDistrict { get; set; }

    public string? BillingAddressLine { get; set; }

    /// <summary>The client address the reverse proxy passed on (05 6.3.3); emptied by the disposal.</summary>
    public IPAddress? IpAddress { get; set; }

    /// <summary>The id of the cart the order was born from: a value, not a link (06 2.2.6); emptied by the disposal.</summary>
    public Guid? CartId { get; set; }

    public Guid? CouponId { get; set; }

    /// <summary>The coupon code as it was at confirmation; filled exactly when <see cref="CouponId"/> is.</summary>
    public string? CouponCode { get; set; }

    /// <summary>The paid total: the one amount the customer confirmed (02 3.21.9).</summary>
    public decimal Total { get; set; }

    public decimal ShippingFee { get; set; }

    /// <summary>The shipping fee's VAT breakdown over the physical items' rates, a JSON list of values (06 2.5.1); empty when the fee is 0.</summary>
    public string? ShippingFeeVatBreakdown { get; set; }

    /// <summary>The firm's IBAN at confirmation on a bank transfer order; filled exactly on a bank transfer order.</summary>
    public string? BankTransferIban { get; set; }

    /// <summary>The shipping promise in business days; empty when the order has no physical item.</summary>
    public int? ShippingPeriod { get; set; }

    public DateTimeOffset PaymentWindowEndsAt { get; set; }

    /// <summary>The bank transfer reminder's moment; always empty on a card order.</summary>
    public DateTimeOffset? PaymentReminderAt { get; set; }

    /// <summary>The carrier's code name (06 5.3.12); no check of allowed values. Empty for "Other".</summary>
    public string? Carrier { get; set; }

    public string? OtherCarrierName { get; set; }

    public string? TrackingNumber { get; set; }

    public bool IsOwnVehicleDelivery { get; set; }

    /// <summary>The firm's legal identity at confirmation: this and the next ten fields are the frozen copy of the company's fields (06 4.1).</summary>
    public CompanyType CompanyType { get; set; }

    public string CompanyLegalName { get; set; } = string.Empty;

    public string? CompanyTaxIdNumber { get; set; }

    public string? CompanyMersisNumber { get; set; }

    public string? CompanyTradeRegistryNumber { get; set; }

    public Guid CompanyProvinceId { get; set; }

    public string CompanyDistrict { get; set; } = string.Empty;

    public string CompanyAddressLine { get; set; } = string.Empty;

    /// <summary>Ten digits, without the leading zero or "+90" (06 2.5.1).</summary>
    public string CompanyPhone { get; set; } = string.Empty;

    public string CompanyEmail { get; set; } = string.Empty;

    public string CompanyKepAddress { get; set; } = string.Empty;

    public Guid PreInformationFormVersionId { get; set; }

    public Guid DistanceSalesContractVersionId { get; set; }

    public Guid PrivacyNoticeVersionId { get; set; }

    /// <summary>The pre-information form's copy without personal data; written once by <see cref="FreezeDocuments"/> (09 9.3 item 1).</summary>
    public string PreInformationFormText { get; private set; } = string.Empty;

    /// <summary>The distance sales contract's copy without personal data; written once by <see cref="FreezeDocuments"/> (09 9.3 item 1).</summary>
    public string DistanceSalesContractText { get; private set; } = string.Empty;

    /// <summary>The order's version (06 2.6.4, 4.17); a concurrency token. Raised by <see cref="BumpVersion"/>.</summary>
    public int Version { get; private set; } = 1;

    public static Order Create(
        string orderNumber,
        string orderAccessKey,
        string confirmationKey,
        PaymentMethod paymentMethod,
        bool isGuestOrder,
        string contactEmail,
        decimal total,
        decimal shippingFee,
        DateTimeOffset paymentWindowEndsAt,
        DateTimeOffset now)
    {
        Order order = new()
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderNumber = orderNumber,
            OrderAccessKey = orderAccessKey,
            ConfirmationKey = confirmationKey,
            PaymentMethod = paymentMethod,
            IsGuestOrder = isGuestOrder,
            Total = total,
            ShippingFee = shippingFee,
            PaymentWindowEndsAt = paymentWindowEndsAt,
        };
        order.SetContactEmail(contactEmail);
        return order;
    }

    /// <summary>Writes the contact email and its normalized form together (06 2.8.2); null empties both (the disposal).</summary>
    public void SetContactEmail(string? email)
    {
        ContactEmail = email;
        NormalizedContactEmail = email is null ? null : TextNormalizer.NormalizeIdentity(email);
    }

    /// <summary>Writes the two document copies; they are written once, at confirmation, and never again (09 9.3 item 1).</summary>
    public void FreezeDocuments(string preInformationFormText, string distanceSalesContractText)
    {
        ArgumentNullException.ThrowIfNull(preInformationFormText);
        ArgumentNullException.ThrowIfNull(distanceSalesContractText);
        if (PreInformationFormText.Length > 0 || DistanceSalesContractText.Length > 0)
        {
            throw new InvalidOperationException("The order's document copies are written once, at confirmation (09 9.3 item 1).");
        }

        PreInformationFormText = preInformationFormText;
        DistanceSalesContractText = distanceSalesContractText;
    }

    public void BumpVersion() => Version++;
}

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    // The principals of the order's links are other modules' entities (09 6 item 2; 10.1 item 1): the links are database foreign keys, so the
    // principals are named, not referenced.
    private const string MembershipCustomerType = "Shopfolio.Modules.Membership.Customer";
    private const string CatalogCouponType = "Shopfolio.Modules.Catalog.Coupon";
    private const string SettingsProvinceType = "Shopfolio.Modules.Settings.Province";
    private const string SettingsLegalTextVersionType = "Shopfolio.Modules.Settings.LegalTextVersion";

    private const string BankTransfer = nameof(PaymentMethod.BankTransfer);
    private const string Card = nameof(PaymentMethod.Card);

    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).HasColumnType("text");
        builder.Property(o => o.OrderAccessKey).HasColumnType("text");
        builder.Property(o => o.ConfirmationKey).HasColumnType("text");
        builder.Property(o => o.OrderStatus).HasDefaultValue(OrderStatus.Placed);
        builder.Property(o => o.PaymentStatus).HasDefaultValue(PaymentStatus.Pending);
        builder.Property(o => o.IsAccountDeleted).HasDefaultValue(false);
        builder.Property(o => o.ContactEmail).HasColumnType("text");
        builder.Property(o => o.NormalizedContactEmail).HasColumnType("text");
        builder.Property(o => o.ShippingRecipientName).HasColumnType("text");
        builder.Property(o => o.ShippingPhone).HasMaxLength(10);
        builder.Property(o => o.ShippingDistrict).HasColumnType("text");
        builder.Property(o => o.ShippingAddressLine).HasColumnType("text");
        builder.Property(o => o.BillingRecipientName).HasColumnType("text");
        builder.Property(o => o.BillingDistrict).HasColumnType("text");
        builder.Property(o => o.BillingAddressLine).HasColumnType("text");
        builder.Property(o => o.IpAddress).HasColumnType("inet");
        builder.Property(o => o.CouponCode).HasColumnType("text");
        builder.Property(o => o.Total).AsMoney();
        builder.Property(o => o.ShippingFee).AsMoney();
        builder.Property(o => o.ShippingFeeVatBreakdown).HasColumnType("jsonb");
        builder.Property(o => o.BankTransferIban).HasColumnType("text");
        builder.Property(o => o.Carrier).HasColumnType("text");
        builder.Property(o => o.OtherCarrierName).HasColumnType("text");
        builder.Property(o => o.TrackingNumber).HasColumnType("text");
        builder.Property(o => o.IsOwnVehicleDelivery).HasDefaultValue(false);
        builder.Property(o => o.CompanyLegalName).HasColumnType("text");
        builder.Property(o => o.CompanyTaxIdNumber).HasColumnType("text");
        builder.Property(o => o.CompanyMersisNumber).HasColumnType("text");
        builder.Property(o => o.CompanyTradeRegistryNumber).HasColumnType("text");
        builder.Property(o => o.CompanyDistrict).HasColumnType("text");
        builder.Property(o => o.CompanyAddressLine).HasColumnType("text");
        builder.Property(o => o.CompanyPhone).HasMaxLength(10);
        builder.Property(o => o.CompanyEmail).HasColumnType("text");
        builder.Property(o => o.CompanyKepAddress).HasColumnType("text");
        builder.Property(o => o.PreInformationFormText).HasColumnType("text");
        builder.Property(o => o.DistanceSalesContractText).HasColumnType("text");
        builder.Property(o => o.Version).HasDefaultValue(1);

        // The account link is emptied; the coupon, province and legal text links are restrictive (06 7.2; 2.2.5; K-1047).
        builder.HasOne(MembershipCustomerType, navigationName: null).WithMany().HasForeignKey(nameof(Order.CustomerId)).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(CatalogCouponType, navigationName: null).WithMany().HasForeignKey(nameof(Order.CouponId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsProvinceType, navigationName: null).WithMany().HasForeignKey(nameof(Order.ShippingProvinceId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsProvinceType, navigationName: null).WithMany().HasForeignKey(nameof(Order.BillingProvinceId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsProvinceType, navigationName: null).WithMany().HasForeignKey(nameof(Order.CompanyProvinceId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsLegalTextVersionType, navigationName: null).WithMany().HasForeignKey(nameof(Order.PreInformationFormVersionId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsLegalTextVersionType, navigationName: null).WithMany().HasForeignKey(nameof(Order.DistanceSalesContractVersionId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(SettingsLegalTextVersionType, navigationName: null).WithMany().HasForeignKey(nameof(Order.PrivacyNoticeVersionId)).OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(o => o.OrderNumber).IsUnique();
        builder.HasIndex(o => o.OrderAccessKey).IsUnique();
        builder.HasIndex(o => o.ConfirmationKey).IsUnique();
        builder.HasIndex(o => o.NormalizedContactEmail);
        builder.HasIndex(o => o.CartId);
        builder.HasIndex(o => o.PaymentWindowEndsAt).HasFilter($"payment_status = '{nameof(PaymentStatus.Pending)}'");
        builder.HasIndex(o => o.PaymentReminderAt).HasFilter($"payment_status = '{nameof(PaymentStatus.Pending)}'");
        builder.HasIndex(o => new { o.IpAddress, o.CreatedAt });
        builder.HasIndex(o => new { o.OrderStatus, o.PaymentStatus, o.CreatedAt });

        // The forbidden pairs of the two axes: every crossed cell of the combination table (03 1.4.1; 02 5.6.1; K-1046).
        builder.HasRule(
            "status_pair_allowed",
            $"(payment_status <> '{nameof(PaymentStatus.Failed)}' OR order_status = '{nameof(OrderStatus.Cancelled)}') "
            + $"AND (payment_status <> '{nameof(PaymentStatus.Pending)}' OR order_status = '{nameof(OrderStatus.Placed)}')");

        // The disposal empties these fields together; an empty contact email says the order is disposed (K-1046).
        builder.HasRule(
            "disposal_fields_together",
            "(contact_email IS NULL AND num_nonnulls(normalized_contact_email, order_access_key, ip_address, cart_id, customer_id, "
            + "billing_recipient_name, billing_province_id, billing_district, billing_address_line, "
            + "shipping_recipient_name, shipping_phone, shipping_province_id, shipping_district, shipping_address_line) = 0) "
            + "OR (contact_email IS NOT NULL AND num_nulls(normalized_contact_email, order_access_key, ip_address, cart_id, "
            + "billing_recipient_name, billing_province_id, billing_district, billing_address_line) = 0)");
        builder.HasRule("deleted_account_without_link", "NOT is_account_deleted OR customer_id IS NULL");
        builder.HasRule(
            "shipping_address_all_or_none",
            "num_nonnulls(shipping_recipient_name, shipping_phone, shipping_province_id, shipping_district, shipping_address_line) IN (0, 5)");
        builder.HasRule("iban_only_on_bank_transfer", $"(payment_method = '{BankTransfer}') = (bank_transfer_iban IS NOT NULL)");
        builder.HasRule("card_without_payment_reminder", $"payment_method <> '{Card}' OR payment_reminder_at IS NULL");
        builder.HasRule("coupon_and_code_together", "(coupon_id IS NULL) = (coupon_code IS NULL)");
        builder.HasRule("at_most_one_carrier", "num_nonnulls(carrier, other_carrier_name) <= 1");
        builder.HasRule("tracking_number_with_carrier", "(tracking_number IS NOT NULL) = (num_nonnulls(carrier, other_carrier_name) = 1)");
        builder.HasRule("own_vehicle_without_carrier", "NOT is_own_vehicle_delivery OR num_nonnulls(carrier, other_carrier_name, tracking_number) = 0");
        builder.HasRule("total_above_zero", "total > 0");
        builder.HasRule("shipping_fee_and_breakdown", "shipping_fee >= 0 AND (shipping_fee = 0) = (shipping_fee_vat_breakdown IS NULL)");
        builder.HasRule("shipping_period_not_negative", "shipping_period IS NULL OR shipping_period >= 0");
        builder.HasRule("shipping_phone_ten_digits", "shipping_phone ~ '^[0-9]{10}$'");
        builder.HasRule("company_phone_ten_digits", "company_phone ~ '^[0-9]{10}$'");
    }
}
