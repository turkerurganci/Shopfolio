using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.DataModel;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.Payment.Contracts;

namespace Shopfolio.Modules.Payment;

/// <summary>
/// The money going back to the customer (06 4.33), by the way the payment came: to the card or to the customer's IBAN. The row is born when
/// the refund starts — the system's own card refund with the record that causes it, the firm's refund with the firm's step. The refund
/// carries the links of its type (the type table of 06 4.33): the item record it pays back, the item and the units, or the late payment.
/// The card refund's requests are its refund records (06 4.34). The record is never deleted (<see cref="PaymentRecordsGuard"/>): the order,
/// item, item record and payment record links are restrictive; the defect claim and administrator links are emptied (06 7.2; 2.2.5).
/// </summary>
internal sealed class Refund
{
    public Guid Id { get; set; }

    public DateTimeOffset CreatedAt { get; set; }

    public Guid OrderId { get; set; }

    public RefundType RefundType { get; set; }

    /// <summary>The way: the card or the customer's IBAN; starts as the order's payment method and turns to bank transfer only once.</summary>
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>The amount paid back (money; 06 2.5.1).</summary>
    public decimal Amount { get; set; }

    /// <summary>Whether the order's frozen shipping fee was added; true on at most one refund of the order that is not voided.</summary>
    public bool IsShippingFeeIncluded { get; set; }

    /// <summary>The item of the record paid back; optional on the amount-based refund; empty on the late payment.</summary>
    public Guid? OrderItemId { get; set; }

    /// <summary>The units paid back on a refund of an item record.</summary>
    public int? Quantity { get; set; }

    /// <summary>The coupon share set aside by the defect claim's rescission; empty on the other types.</summary>
    public decimal? CouponShare { get; set; }

    /// <summary>The VAT set aside by the defect claim's rescission; empty on the other types.</summary>
    public decimal? VatAmount { get; set; }

    public Guid? CancellationId { get; set; }

    public Guid? ItemRemovalId { get; set; }

    public Guid? LateDeliveryTerminationId { get; set; }

    public Guid? WithdrawalId { get; set; }

    /// <summary>The receipt that causes the refund: a withdrawal after delivery or a physical item's rescission.</summary>
    public Guid? ReturnId { get; set; }

    /// <summary>The defect claim of the rescission; emptied when the claim is deleted.</summary>
    public Guid? DefectClaimId { get; set; }

    /// <summary>The "good not returned" closure of an amount-based refund.</summary>
    public Guid? NotReturnedClosureId { get; set; }

    /// <summary>The late payment to a cancelled order that is paid back; one refund per payment record.</summary>
    public Guid? PaymentRecordId { get; set; }

    /// <summary>The actor: the system or an administrator.</summary>
    public ActorKind ActorKind { get; set; }

    /// <summary>The administrator who processed; emptied when the administrator is removed and by the disposal.</summary>
    public Guid? AdminId { get; set; }

    /// <summary>The administrator's name as a value; emptied by the disposal.</summary>
    public string? AdminName { get; set; }

    /// <summary>The moment the refund was sent; empty while the refund has started but is not sent.</summary>
    public DateTimeOffset? SentAt { get; set; }

    /// <summary>The moment the refund was voided with the correction of the firm's cancellation that caused it.</summary>
    public DateTimeOffset? VoidedAt { get; set; }

    /// <summary>The refund the system starts on its own (a card refund); the caller sets the links of the type.</summary>
    public static Refund BySystem(Guid orderId, RefundType refundType, PaymentMethod paymentMethod, decimal amount, DateTimeOffset now)
    {
        return New(orderId, refundType, paymentMethod, amount, ActorKind.System, now);
    }

    /// <summary>The refund the firm processes; the caller sets the links of the type.</summary>
    public static Refund ByAdmin(
        Guid orderId,
        RefundType refundType,
        PaymentMethod paymentMethod,
        decimal amount,
        Guid adminId,
        string adminName,
        DateTimeOffset now)
    {
        Refund refund = New(orderId, refundType, paymentMethod, amount, ActorKind.Admin, now);
        refund.AdminId = adminId;
        refund.AdminName = adminName;
        return refund;
    }

    /// <summary>The card refund of a late payment to a cancelled order: the paid total, started by the system with the payment record.</summary>
    public static Refund ForLatePayment(Guid orderId, Guid paymentRecordId, decimal amount, DateTimeOffset now)
    {
        Refund refund = New(orderId, RefundType.LatePayment, PaymentMethod.Card, amount, ActorKind.System, now);
        refund.PaymentRecordId = paymentRecordId;
        return refund;
    }

    private static Refund New(Guid orderId, RefundType refundType, PaymentMethod paymentMethod, decimal amount, ActorKind actorKind, DateTimeOffset now)
    {
        return new Refund
        {
            Id = EntityIds.New(),
            CreatedAt = now,
            OrderId = orderId,
            RefundType = refundType,
            PaymentMethod = paymentMethod,
            Amount = amount,
            ActorKind = actorKind,
        };
    }
}

internal sealed class RefundConfiguration : IEntityTypeConfiguration<Refund>
{
    internal const string OneShippingFeeIndex = "OneShippingFeePerOrder";

    // The order, the item, the item records, the defect claim and the administrator are other modules' entities (09 6 item 2; 10.1 item 1):
    // the links are database foreign keys, so the principals are named.
    private const string OrderingOrderType = "Shopfolio.Modules.Ordering.Order";
    private const string OrderingOrderItemType = "Shopfolio.Modules.Ordering.OrderItem";
    private const string AfterSalesCancellationType = "Shopfolio.Modules.AfterSales.Cancellation";
    private const string AfterSalesItemRemovalType = "Shopfolio.Modules.AfterSales.ItemRemoval";
    private const string AfterSalesLateDeliveryTerminationType = "Shopfolio.Modules.AfterSales.LateDeliveryTermination";
    private const string AfterSalesWithdrawalType = "Shopfolio.Modules.AfterSales.Withdrawal";
    private const string AfterSalesReturnType = "Shopfolio.Modules.AfterSales.Return";
    private const string AfterSalesDefectClaimType = "Shopfolio.Modules.AfterSales.DefectClaim";
    private const string AfterSalesNotReturnedClosureType = "Shopfolio.Modules.AfterSales.NotReturnedClosure";
    private const string AdministrationAdminType = "Shopfolio.Modules.Administration.Admin";

    private const string AmountBased = nameof(RefundType.AmountBased);

    /// <summary>The links whose fill the type decides (06 4.33's type table); the closure link has its own two rules.</summary>
    private static readonly string[] TypedLinks =
    [
        "order_item_id", "quantity", "coupon_share", "vat_amount", "cancellation_id", "item_removal_id", "late_delivery_termination_id",
        "withdrawal_id", "return_id", "defect_claim_id", "payment_record_id",
    ];

    public void Configure(EntityTypeBuilder<Refund> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.ActorKind).HasAllowedValues(ActorKind.System, ActorKind.Admin);
        builder.Property(r => r.Amount).AsMoney();
        builder.Property(r => r.IsShippingFeeIncluded).HasDefaultValue(false);
        builder.Property(r => r.CouponShare).AsMoney();
        builder.Property(r => r.VatAmount).AsMoney();
        builder.Property(r => r.AdminName).HasColumnType("text");

        // The order, item, item record and payment record links are restrictive; the defect claim and administrator links are emptied
        // (06 7.2; 2.2.5).
        builder.HasOne(OrderingOrderType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.OrderId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(OrderingOrderItemType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.OrderItemId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AfterSalesCancellationType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.CancellationId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AfterSalesItemRemovalType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.ItemRemovalId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AfterSalesLateDeliveryTerminationType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.LateDeliveryTerminationId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AfterSalesWithdrawalType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.WithdrawalId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AfterSalesReturnType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.ReturnId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AfterSalesNotReturnedClosureType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.NotReturnedClosureId)).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne<PaymentRecord>().WithMany().HasForeignKey(r => r.PaymentRecordId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(AfterSalesDefectClaimType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.DefectClaimId)).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(AdministrationAdminType, navigationName: null).WithMany().HasForeignKey(nameof(Refund.AdminId)).OnDelete(DeleteBehavior.SetNull);

        // The order's refunds keep their plain index (06 7.2, 7.5); the shipping fee is paid back once: on at most one refund of the order
        // that is not voided (06 7.1; 02 7.2.9, 7.4.6). A late payment has one card refund: the payment record link is unique on the
        // filled rows (06 7.1; K-970). Two indexes on the same column are told apart by the model's index name.
        builder.HasIndex(r => r.OrderId);
        builder.HasIndex(r => r.OrderId, OneShippingFeeIndex).IsUnique().HasFilter("is_shipping_fee_included AND voided_at IS NULL");
        builder.HasIndex(r => r.PaymentRecordId).IsUnique().HasFilter("payment_record_id IS NOT NULL");

        builder.HasRule("amount_above_zero", "amount > 0");
        builder.HasRule("quantity_above_zero", "quantity > 0");
        builder.HasRule("coupon_share_not_negative", "coupon_share >= 0");
        builder.HasRule("vat_amount_not_negative", "vat_amount >= 0");
        builder.HasRule("system_without_admin", $"actor_kind <> '{nameof(ActorKind.System)}' OR num_nonnulls(admin_id, admin_name) = 0");
        builder.HasRule("sent_or_voided", "num_nonnulls(sent_at, voided_at) <= 1");

        // The shipping fee is added only to the refund of a cancellation, a removal, a termination or a withdrawal (06 4.33).
        builder.HasRule(
            "shipping_fee_only_on_item_records",
            "NOT is_shipping_fee_included OR refund_type IN ("
            + $"'{nameof(RefundType.Cancellation)}', '{nameof(RefundType.ItemRemoval)}', "
            + $"'{nameof(RefundType.LateDeliveryTermination)}', '{nameof(RefundType.Withdrawal)}')");

        // The closure link is filled only on the amount-based refund, and then with the closure's item (06 4.33; K-1105).
        builder.HasRule("not_returned_closure_only_amount_based", $"not_returned_closure_id IS NULL OR refund_type = '{AmountBased}'");
        builder.HasRule("not_returned_closure_with_item", "not_returned_closure_id IS NULL OR order_item_id IS NOT NULL");

        // The links by type (06 4.33's type table): the links a type names are filled, the defect claim and the receipt may be empty where
        // the table allows it, and every link the type does not name is empty.
        builder.HasRule("links_by_type", LinksByType());
    }

    private static string LinksByType()
    {
        static string Filled(params string[] links) => string.Join(" AND ", links.Select(l => l + " IS NOT NULL"));

        static string Empty(params string[] allowed) =>
            "num_nonnulls(" + string.Join(", ", TypedLinks.Except(allowed, StringComparer.Ordinal)) + ") = 0";

        string[] cancellation = ["cancellation_id", "order_item_id", "quantity"];
        string[] removal = ["item_removal_id", "order_item_id", "quantity"];
        string[] termination = ["late_delivery_termination_id", "order_item_id", "quantity"];
        string[] withdrawal = ["withdrawal_id", "order_item_id", "quantity"];
        string[] rescission = ["order_item_id", "quantity", "coupon_share", "vat_amount"];

        return "CASE refund_type"
            + $" WHEN '{nameof(RefundType.Cancellation)}' THEN {Filled(cancellation)} AND {Empty(cancellation)}"
            + $" WHEN '{nameof(RefundType.ItemRemoval)}' THEN {Filled(removal)} AND {Empty(removal)}"
            + $" WHEN '{nameof(RefundType.LateDeliveryTermination)}' THEN {Filled(termination)} AND {Empty(termination)}"
            + $" WHEN '{nameof(RefundType.Withdrawal)}' THEN {Filled(withdrawal)} AND {Empty([.. withdrawal, "return_id"])}"
            + $" WHEN '{nameof(RefundType.DefectClaimRescission)}' THEN {Filled(rescission)} AND {Empty([.. rescission, "defect_claim_id", "return_id"])}"
            + $" WHEN '{AmountBased}' THEN {Empty("order_item_id")}"
            + $" WHEN '{nameof(RefundType.LatePayment)}' THEN {Filled("payment_record_id")} AND {Empty("payment_record_id")}"
            + " ELSE false END";
    }
}
