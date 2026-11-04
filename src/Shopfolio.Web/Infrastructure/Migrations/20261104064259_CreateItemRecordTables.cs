using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateItemRecordTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "defect_claims",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "text", nullable: false),
                    defect_claim_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Open"),
                    status_history = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'[]'::jsonb"),
                    return_recipient_name = table.Column<string>(type: "text", nullable: true),
                    return_province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    return_district = table.Column<string>(type: "text", nullable: true),
                    return_address_line = table.Column<string>(type: "text", nullable: true),
                    return_phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_defect_claims", x => x.id);
                    table.CheckConstraint("ck_defect_claims_defect_claim_status", "\"defect_claim_status\" IN ('Open', 'Resolved')");
                    table.CheckConstraint("ck_defect_claims_quantity_above_zero", "quantity > 0");
                    table.CheckConstraint("ck_defect_claims_return_address_fields_together", "num_nonnulls(return_recipient_name, return_province_id, return_district, return_address_line) IN (0, 4)");
                    table.CheckConstraint("ck_defect_claims_return_phone_only_with_address", "return_phone IS NULL OR return_recipient_name IS NOT NULL");
                    table.CheckConstraint("ck_defect_claims_return_phone_ten_digits", "return_phone ~ '^[0-9]{10}$'");
                    table.ForeignKey(
                        name: "fk_defect_claims_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_defect_claims_provinces_return_province_id",
                        column: x => x.return_province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "item_removals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true),
                    product_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    coupon_share = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    vat_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_item_removals", x => x.id);
                    table.CheckConstraint("ck_item_removals_coupon_share_within_product", "coupon_share >= 0 AND coupon_share <= product_amount");
                    table.CheckConstraint("ck_item_removals_product_amount_above_zero", "product_amount > 0");
                    table.CheckConstraint("ck_item_removals_quantity_above_zero", "quantity > 0");
                    table.CheckConstraint("ck_item_removals_vat_amount_not_negative", "vat_amount >= 0");
                    table.ForeignKey(
                        name: "fk_item_removals_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_item_removals_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "refund_ibans",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    iban = table.Column<string>(type: "text", nullable: false),
                    entered_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    scheduled_deletion_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refund_ibans", x => x.id);
                    table.CheckConstraint("ck_refund_ibans_iban_capitals_and_digits", "iban ~ '^[A-Z0-9]+$'");
                    table.ForeignKey(
                        name: "fk_refund_ibans_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cancellations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    actor_kind = table.Column<string>(type: "text", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    reason_note = table.Column<string>(type: "text", nullable: true),
                    product_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    coupon_share = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    vat_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    transition_record_id = table.Column<Guid>(type: "uuid", nullable: true),
                    refund_iban_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cancellations", x => x.id);
                    table.CheckConstraint("ck_cancellations_actor_kind", "\"actor_kind\" IN ('Buyer', 'Admin')");
                    table.CheckConstraint("ck_cancellations_buyer_without_admin", "actor_kind <> 'Buyer' OR num_nonnulls(admin_id, admin_name) = 0");
                    table.CheckConstraint("ck_cancellations_cancellation_reason", "\"cancellation_reason\" IN ('OutOfStock', 'DamagedOrUnsellable', 'CannotShipToAddress', 'UndeliveredBuyerCaused', 'AgreedWithBuyer', 'Other')");
                    table.CheckConstraint("ck_cancellations_coupon_share_within_product", "coupon_share >= 0 AND coupon_share <= product_amount");
                    table.CheckConstraint("ck_cancellations_product_amount_above_zero", "product_amount > 0");
                    table.CheckConstraint("ck_cancellations_quantity_above_zero", "quantity > 0");
                    table.CheckConstraint("ck_cancellations_reason_by_admin", "(actor_kind = 'Admin') = (cancellation_reason IS NOT NULL)");
                    table.CheckConstraint("ck_cancellations_reason_note_only_on_other", "reason_note IS NULL OR cancellation_reason IS NOT DISTINCT FROM 'Other'");
                    table.CheckConstraint("ck_cancellations_refund_iban_only_by_buyer", "refund_iban_id IS NULL OR actor_kind = 'Buyer'");
                    table.CheckConstraint("ck_cancellations_vat_amount_not_negative", "vat_amount >= 0");
                    table.ForeignKey(
                        name: "fk_cancellations_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_cancellations_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_cancellations_refund_ibans_refund_iban_id",
                        column: x => x.refund_iban_id,
                        principalTable: "refund_ibans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_cancellations_transition_records_transition_record_id",
                        column: x => x.transition_record_id,
                        principalTable: "transition_records",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "late_delivery_terminations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    actor_kind = table.Column<string>(type: "text", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true),
                    declaration_date = table.Column<DateOnly>(type: "date", nullable: false),
                    product_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    coupon_share = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    vat_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    refund_iban_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_late_delivery_terminations", x => x.id);
                    table.CheckConstraint("ck_late_delivery_terminations_actor_kind", "\"actor_kind\" IN ('Buyer', 'Admin')");
                    table.CheckConstraint("ck_late_delivery_terminations_buyer_without_admin", "actor_kind <> 'Buyer' OR num_nonnulls(admin_id, admin_name) = 0");
                    table.CheckConstraint("ck_late_delivery_terminations_coupon_share_within_product", "coupon_share >= 0 AND coupon_share <= product_amount");
                    table.CheckConstraint("ck_late_delivery_terminations_product_amount_above_zero", "product_amount > 0");
                    table.CheckConstraint("ck_late_delivery_terminations_quantity_above_zero", "quantity > 0");
                    table.CheckConstraint("ck_late_delivery_terminations_vat_amount_not_negative", "vat_amount >= 0");
                    table.ForeignKey(
                        name: "fk_late_delivery_terminations_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_late_delivery_terminations_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_late_delivery_terminations_refund_ibans_refund_iban_id",
                        column: x => x.refund_iban_id,
                        principalTable: "refund_ibans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "withdrawals",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    actor_kind = table.Column<string>(type: "text", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true),
                    declaration_date = table.Column<DateOnly>(type: "date", nullable: false),
                    is_delivered_before_declaration = table.Column<bool>(type: "boolean", nullable: true),
                    return_recipient_name = table.Column<string>(type: "text", nullable: true),
                    return_province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    return_district = table.Column<string>(type: "text", nullable: true),
                    return_address_line = table.Column<string>(type: "text", nullable: true),
                    return_phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    product_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    coupon_share = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    vat_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    refund_iban_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_withdrawals", x => x.id);
                    table.CheckConstraint("ck_withdrawals_actor_kind", "\"actor_kind\" IN ('Buyer', 'Admin')");
                    table.CheckConstraint("ck_withdrawals_buyer_without_admin", "actor_kind <> 'Buyer' OR num_nonnulls(admin_id, admin_name) = 0");
                    table.CheckConstraint("ck_withdrawals_coupon_share_within_product", "coupon_share >= 0 AND coupon_share <= product_amount");
                    table.CheckConstraint("ck_withdrawals_product_amount_above_zero", "product_amount > 0");
                    table.CheckConstraint("ck_withdrawals_quantity_above_zero", "quantity > 0");
                    table.CheckConstraint("ck_withdrawals_return_address_fields_together", "num_nonnulls(return_recipient_name, return_province_id, return_district, return_address_line) IN (0, 4)");
                    table.CheckConstraint("ck_withdrawals_return_phone_only_with_address", "return_phone IS NULL OR return_recipient_name IS NOT NULL");
                    table.CheckConstraint("ck_withdrawals_return_phone_ten_digits", "return_phone ~ '^[0-9]{10}$'");
                    table.CheckConstraint("ck_withdrawals_vat_amount_not_negative", "vat_amount >= 0");
                    table.ForeignKey(
                        name: "fk_withdrawals_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_withdrawals_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_withdrawals_provinces_return_province_id",
                        column: x => x.return_province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_withdrawals_refund_ibans_refund_iban_id",
                        column: x => x.refund_iban_id,
                        principalTable: "refund_ibans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "not_returned_closures",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    withdrawal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true),
                    reopened_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_not_returned_closures", x => x.id);
                    table.CheckConstraint("ck_not_returned_closures_quantity_above_zero", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_not_returned_closures_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_not_returned_closures_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_not_returned_closures_withdrawals_withdrawal_id",
                        column: x => x.withdrawal_id,
                        principalTable: "withdrawals",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "returns",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    arrival_date = table.Column<DateOnly>(type: "date", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true),
                    withdrawal_id = table.Column<Guid>(type: "uuid", nullable: true),
                    late_delivery_termination_id = table.Column<Guid>(type: "uuid", nullable: true),
                    defect_claim_id = table.Column<Guid>(type: "uuid", nullable: true),
                    stock_added_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_returns", x => x.id);
                    table.CheckConstraint("ck_returns_at_most_one_link", "num_nonnulls(withdrawal_id, late_delivery_termination_id, defect_claim_id) <= 1");
                    table.CheckConstraint("ck_returns_quantity_above_zero", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_returns_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_returns_defect_claims_defect_claim_id",
                        column: x => x.defect_claim_id,
                        principalTable: "defect_claims",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_returns_late_delivery_terminations_late_delivery_terminatio",
                        column: x => x.late_delivery_termination_id,
                        principalTable: "late_delivery_terminations",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_returns_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_returns_withdrawals_withdrawal_id",
                        column: x => x.withdrawal_id,
                        principalTable: "withdrawals",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_cancellations_admin_id",
                table: "cancellations",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_cancellations_order_item_id",
                table: "cancellations",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_cancellations_refund_iban_id",
                table: "cancellations",
                column: "refund_iban_id");

            migrationBuilder.CreateIndex(
                name: "ix_cancellations_transition_record_id",
                table: "cancellations",
                column: "transition_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_defect_claims_defect_claim_status",
                table: "defect_claims",
                column: "defect_claim_status",
                filter: "defect_claim_status = 'Open'");

            migrationBuilder.CreateIndex(
                name: "ix_defect_claims_order_item_id",
                table: "defect_claims",
                column: "order_item_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_defect_claims_return_province_id",
                table: "defect_claims",
                column: "return_province_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_removals_admin_id",
                table: "item_removals",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_item_removals_order_item_id",
                table: "item_removals",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_late_delivery_terminations_admin_id",
                table: "late_delivery_terminations",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_late_delivery_terminations_order_item_id",
                table: "late_delivery_terminations",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_late_delivery_terminations_refund_iban_id",
                table: "late_delivery_terminations",
                column: "refund_iban_id");

            migrationBuilder.CreateIndex(
                name: "ix_not_returned_closures_admin_id",
                table: "not_returned_closures",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_not_returned_closures_order_item_id",
                table: "not_returned_closures",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_not_returned_closures_withdrawal_id",
                table: "not_returned_closures",
                column: "withdrawal_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_ibans_order_id",
                table: "refund_ibans",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_ibans_scheduled_deletion_at",
                table: "refund_ibans",
                column: "scheduled_deletion_at",
                filter: "scheduled_deletion_at IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_returns_admin_id",
                table: "returns",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_returns_defect_claim_id",
                table: "returns",
                column: "defect_claim_id");

            migrationBuilder.CreateIndex(
                name: "ix_returns_late_delivery_termination_id",
                table: "returns",
                column: "late_delivery_termination_id");

            migrationBuilder.CreateIndex(
                name: "ix_returns_order_item_id",
                table: "returns",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_returns_withdrawal_id",
                table: "returns",
                column: "withdrawal_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawals_admin_id",
                table: "withdrawals",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawals_order_item_id",
                table: "withdrawals",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawals_refund_iban_id",
                table: "withdrawals",
                column: "refund_iban_id");

            migrationBuilder.CreateIndex(
                name: "ix_withdrawals_return_province_id",
                table: "withdrawals",
                column: "return_province_id");
        }
    }
}
