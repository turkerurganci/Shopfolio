using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreatePaymentAndRefundTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "payment_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    provider_transaction_id = table.Column<string>(type: "text", nullable: true),
                    transition_record_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_payment_records", x => x.id);
                    table.CheckConstraint("ck_payment_records_transaction_or_transition", "num_nonnulls(provider_transaction_id, transition_record_id) >= 1");
                    table.ForeignKey(
                        name: "fk_payment_records_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_payment_records_transition_records_transition_record_id",
                        column: x => x.transition_record_id,
                        principalTable: "transition_records",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refunds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    refund_type = table.Column<string>(type: "text", nullable: false),
                    payment_method = table.Column<string>(type: "text", nullable: false),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    is_shipping_fee_included = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: true),
                    coupon_share = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    vat_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    cancellation_id = table.Column<Guid>(type: "uuid", nullable: true),
                    item_removal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    late_delivery_termination_id = table.Column<Guid>(type: "uuid", nullable: true),
                    withdrawal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    return_id = table.Column<Guid>(type: "uuid", nullable: true),
                    defect_claim_id = table.Column<Guid>(type: "uuid", nullable: true),
                    not_returned_closure_id = table.Column<Guid>(type: "uuid", nullable: true),
                    payment_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    actor_kind = table.Column<string>(type: "text", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true),
                    sent_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    voided_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refunds", x => x.id);
                    table.CheckConstraint("ck_refunds_actor_kind", "\"actor_kind\" IN ('System', 'Admin')");
                    table.CheckConstraint("ck_refunds_amount_above_zero", "amount > 0");
                    table.CheckConstraint("ck_refunds_coupon_share_not_negative", "coupon_share >= 0");
                    table.CheckConstraint("ck_refunds_links_by_type", "CASE refund_type WHEN 'Cancellation' THEN cancellation_id IS NOT NULL AND order_item_id IS NOT NULL AND quantity IS NOT NULL AND num_nonnulls(coupon_share, vat_amount, item_removal_id, late_delivery_termination_id, withdrawal_id, return_id, defect_claim_id, payment_record_id) = 0 WHEN 'ItemRemoval' THEN item_removal_id IS NOT NULL AND order_item_id IS NOT NULL AND quantity IS NOT NULL AND num_nonnulls(coupon_share, vat_amount, cancellation_id, late_delivery_termination_id, withdrawal_id, return_id, defect_claim_id, payment_record_id) = 0 WHEN 'LateDeliveryTermination' THEN late_delivery_termination_id IS NOT NULL AND order_item_id IS NOT NULL AND quantity IS NOT NULL AND num_nonnulls(coupon_share, vat_amount, cancellation_id, item_removal_id, withdrawal_id, return_id, defect_claim_id, payment_record_id) = 0 WHEN 'Withdrawal' THEN withdrawal_id IS NOT NULL AND order_item_id IS NOT NULL AND quantity IS NOT NULL AND num_nonnulls(coupon_share, vat_amount, cancellation_id, item_removal_id, late_delivery_termination_id, defect_claim_id, payment_record_id) = 0 WHEN 'DefectClaimRescission' THEN order_item_id IS NOT NULL AND quantity IS NOT NULL AND coupon_share IS NOT NULL AND vat_amount IS NOT NULL AND num_nonnulls(cancellation_id, item_removal_id, late_delivery_termination_id, withdrawal_id, payment_record_id) = 0 WHEN 'AmountBased' THEN num_nonnulls(quantity, coupon_share, vat_amount, cancellation_id, item_removal_id, late_delivery_termination_id, withdrawal_id, return_id, defect_claim_id, payment_record_id) = 0 WHEN 'LatePayment' THEN payment_record_id IS NOT NULL AND num_nonnulls(order_item_id, quantity, coupon_share, vat_amount, cancellation_id, item_removal_id, late_delivery_termination_id, withdrawal_id, return_id, defect_claim_id) = 0 ELSE false END");
                    table.CheckConstraint("ck_refunds_not_returned_closure_only_amount_based", "not_returned_closure_id IS NULL OR refund_type = 'AmountBased'");
                    table.CheckConstraint("ck_refunds_not_returned_closure_with_item", "not_returned_closure_id IS NULL OR order_item_id IS NOT NULL");
                    table.CheckConstraint("ck_refunds_payment_method", "\"payment_method\" IN ('Card', 'BankTransfer')");
                    table.CheckConstraint("ck_refunds_quantity_above_zero", "quantity > 0");
                    table.CheckConstraint("ck_refunds_refund_type", "\"refund_type\" IN ('Cancellation', 'ItemRemoval', 'LateDeliveryTermination', 'Withdrawal', 'DefectClaimRescission', 'AmountBased', 'LatePayment')");
                    table.CheckConstraint("ck_refunds_sent_or_voided", "num_nonnulls(sent_at, voided_at) <= 1");
                    table.CheckConstraint("ck_refunds_shipping_fee_only_on_item_records", "NOT is_shipping_fee_included OR refund_type IN ('Cancellation', 'ItemRemoval', 'LateDeliveryTermination', 'Withdrawal')");
                    table.CheckConstraint("ck_refunds_system_without_admin", "actor_kind <> 'System' OR num_nonnulls(admin_id, admin_name) = 0");
                    table.CheckConstraint("ck_refunds_vat_amount_not_negative", "vat_amount >= 0");
                    table.ForeignKey(
                        name: "fk_refunds_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_refunds_cancellations_cancellation_id",
                        column: x => x.cancellation_id,
                        principalTable: "cancellations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_defect_claims_defect_claim_id",
                        column: x => x.defect_claim_id,
                        principalTable: "defect_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_refunds_item_removals_item_removal_id",
                        column: x => x.item_removal_id,
                        principalTable: "item_removals",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_late_delivery_terminations_late_delivery_terminatio",
                        column: x => x.late_delivery_termination_id,
                        principalTable: "late_delivery_terminations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_not_returned_closures_not_returned_closure_id",
                        column: x => x.not_returned_closure_id,
                        principalTable: "not_returned_closures",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_payment_records_payment_record_id",
                        column: x => x.payment_record_id,
                        principalTable: "payment_records",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_returns_return_id",
                        column: x => x.return_id,
                        principalTable: "returns",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refunds_withdrawals_withdrawal_id",
                        column: x => x.withdrawal_id,
                        principalTable: "withdrawals",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refund_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    refund_id = table.Column<Guid>(type: "uuid", nullable: false),
                    state = table.Column<string>(type: "text", nullable: false, defaultValue: "Sending"),
                    outcome_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_marked_failed_by_admin = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    late_success_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refund_records", x => x.id);
                    table.CheckConstraint("ck_refund_records_admin_mark_is_failed", "NOT is_marked_failed_by_admin OR state = 'Failed'");
                    table.CheckConstraint("ck_refund_records_late_success_only_when_marked", "late_success_at IS NULL OR is_marked_failed_by_admin");
                    table.CheckConstraint("ck_refund_records_outcome_at_with_outcome", "(state IN ('Succeeded', 'Failed')) = (outcome_at IS NOT NULL)");
                    table.CheckConstraint("ck_refund_records_state", "\"state\" IN ('Sending', 'OutcomePending', 'Succeeded', 'Failed')");
                    table.ForeignKey(
                        name: "fk_refund_records_refunds_refund_id",
                        column: x => x.refund_id,
                        principalTable: "refunds",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "inbound_notification_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    provider_notification_id = table.Column<string>(type: "text", nullable: true),
                    provider_transaction_id = table.Column<string>(type: "text", nullable: true),
                    event_type = table.Column<string>(type: "text", nullable: false),
                    outcome = table.Column<string>(type: "text", nullable: false),
                    order_reference = table.Column<string>(type: "text", nullable: true),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    order_id = table.Column<Guid>(type: "uuid", nullable: true),
                    refund_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    processed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    unprocessed_reason = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_inbound_notification_records", x => x.id);
                    table.CheckConstraint("ck_inbound_notification_records_amount_not_negative", "amount >= 0");
                    table.CheckConstraint("ck_inbound_notification_records_notification_or_transaction", "num_nonnulls(provider_notification_id, provider_transaction_id) >= 1");
                    table.CheckConstraint("ck_inbound_notification_records_reason_only_when_processed", "unprocessed_reason IS NULL OR processed_at IS NOT NULL");
                    table.CheckConstraint("ck_inbound_notification_records_unprocessed_reason", "\"unprocessed_reason\" IN ('AmountMismatch', 'OrderNotFound', 'RefundMarkedFailed', 'UnrecognizedNotification')");
                    table.ForeignKey(
                        name: "fk_inbound_notification_records_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_inbound_notification_records_refund_records_refund_record_id",
                        column: x => x.refund_record_id,
                        principalTable: "refund_records",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_inbound_notification_records_created_at",
                table: "inbound_notification_records",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_inbound_notification_records_order_id",
                table: "inbound_notification_records",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_inbound_notification_records_provider_notification_id",
                table: "inbound_notification_records",
                column: "provider_notification_id",
                unique: true,
                filter: "provider_notification_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_inbound_notification_records_provider_transaction_id_event_",
                table: "inbound_notification_records",
                columns: new[] { "provider_transaction_id", "event_type", "outcome" },
                unique: true,
                filter: "provider_notification_id IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_inbound_notification_records_refund_record_id",
                table: "inbound_notification_records",
                column: "refund_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_records_order_id",
                table: "payment_records",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_payment_records_provider_transaction_id",
                table: "payment_records",
                column: "provider_transaction_id",
                unique: true,
                filter: "provider_transaction_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_payment_records_transition_record_id",
                table: "payment_records",
                column: "transition_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_records_refund_id",
                table: "refund_records",
                column: "refund_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_records_refund_id1",
                table: "refund_records",
                column: "refund_id",
                unique: true,
                filter: "state IN ('Sending', 'OutcomePending')");

            migrationBuilder.CreateIndex(
                name: "ix_refund_records_refund_id2",
                table: "refund_records",
                column: "refund_id",
                unique: true,
                filter: "state = 'Succeeded'");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_admin_id",
                table: "refunds",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_cancellation_id",
                table: "refunds",
                column: "cancellation_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_defect_claim_id",
                table: "refunds",
                column: "defect_claim_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_item_removal_id",
                table: "refunds",
                column: "item_removal_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_late_delivery_termination_id",
                table: "refunds",
                column: "late_delivery_termination_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_not_returned_closure_id",
                table: "refunds",
                column: "not_returned_closure_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_order_id",
                table: "refunds",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_order_id1",
                table: "refunds",
                column: "order_id",
                unique: true,
                filter: "is_shipping_fee_included AND voided_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_order_item_id",
                table: "refunds",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_payment_record_id",
                table: "refunds",
                column: "payment_record_id",
                unique: true,
                filter: "payment_record_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_return_id",
                table: "refunds",
                column: "return_id");

            migrationBuilder.CreateIndex(
                name: "ix_refunds_withdrawal_id",
                table: "refunds",
                column: "withdrawal_id");
        }
    }
}
