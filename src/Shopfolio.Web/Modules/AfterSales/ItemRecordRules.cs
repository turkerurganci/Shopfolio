using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Ordering.Contracts;

namespace Shopfolio.Modules.AfterSales;

/// <summary>
/// The amounts an item record sets aside from its item (06 4.22; 02 7.1.7): the product amount, the coupon share and the VAT, each money
/// (<c>numeric(12,2)</c>; 06 2.5.1). The share and the VAT are computed by the one rounding function of the record's writer, not here.
/// </summary>
internal readonly record struct ItemAmounts(decimal ProductAmount, decimal CouponShare, decimal VatAmount);

/// <summary>
/// The firm's return address shown on a withdrawal declaration or a defect claim, copied as a value (06 4.25, 4.30; K-817): the four
/// required fields come together and the phone, ten digits (06 2.5.1), only with them.
/// </summary>
internal readonly record struct ReturnAddress(string RecipientName, Guid ProvinceId, string District, string AddressLine, string? Phone);

/// <summary>
/// The check constraints the item records share (06 4.22-4.30; 06 7.3). Each rule is declared on the record's own table and named by the
/// framework (<c>ck_&lt;table&gt;_&lt;rule&gt;</c>; 06 2.4.3).
/// </summary>
internal static class ItemRecordRules
{
    /// <summary>The administrator's fields on a record whose actor is a customer or an administrator (06 4.22, 4.24, 4.25).</summary>
    public const string BuyerActor = nameof(ActorKind.Buyer);

    public const string AdminActor = nameof(ActorKind.Admin);

    /// <summary>
    /// The amount rules of the cancellation, the removal, the late delivery termination and the withdrawal (06 4.22-4.25): the quantity and
    /// the product amount are above zero, 0 &lt;= coupon share &lt;= product amount, and the VAT is not negative.
    /// </summary>
    public static void HasQuantityAndAmountRules<TRecord>(EntityTypeBuilder<TRecord> builder)
        where TRecord : class
    {
        HasQuantityRule(builder);
        builder.HasRule("product_amount_above_zero", "product_amount > 0");
        builder.HasRule("coupon_share_within_product", "coupon_share >= 0 AND coupon_share <= product_amount");
        builder.HasRule("vat_amount_not_negative", "vat_amount >= 0");
    }

    /// <summary>The quantity of every item record is above zero (06 4.22-4.30).</summary>
    public static void HasQuantityRule<TRecord>(EntityTypeBuilder<TRecord> builder)
        where TRecord : class
    {
        builder.HasRule("quantity_above_zero", "quantity > 0");
    }

    /// <summary>A customer's record carries no administrator link and no administrator name (06 4.22, 4.24, 4.25).</summary>
    public static void HasBuyerWithoutAdminRule<TRecord>(EntityTypeBuilder<TRecord> builder)
        where TRecord : class
    {
        builder.HasRule("buyer_without_admin", $"actor_kind <> '{BuyerActor}' OR num_nonnulls(admin_id, admin_name) = 0");
    }

    /// <summary>
    /// The return address (06 4.25, 4.30): the four required fields are filled together or empty together; the phone is filled only with
    /// them and is ten digits (06 2.5.1).
    /// </summary>
    public static void HasReturnAddressRules<TRecord>(EntityTypeBuilder<TRecord> builder)
        where TRecord : class
    {
        builder.HasRule(
            "return_address_fields_together",
            "num_nonnulls(return_recipient_name, return_province_id, return_district, return_address_line) IN (0, 4)");
        builder.HasRule("return_phone_only_with_address", "return_phone IS NULL OR return_recipient_name IS NOT NULL");
        builder.HasRule("return_phone_ten_digits", "return_phone ~ '^[0-9]{10}$'");
    }
}
