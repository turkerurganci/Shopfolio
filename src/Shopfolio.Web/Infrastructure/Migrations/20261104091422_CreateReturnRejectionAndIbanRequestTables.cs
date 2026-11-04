using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateReturnRejectionAndIbanRequestTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "refund_iban_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: true),
                    reason = table.Column<string>(type: "text", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: false),
                    refund_id = table.Column<Guid>(type: "uuid", nullable: true),
                    refund_iban_id = table.Column<Guid>(type: "uuid", nullable: true),
                    fulfilled_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_refund_iban_requests", x => x.id);
                    table.CheckConstraint("ck_refund_iban_requests_fulfilled_or_closed", "num_nonnulls(fulfilled_at, closed_at) <= 1");
                    table.CheckConstraint("ck_refund_iban_requests_iban_only_when_fulfilled", "refund_iban_id IS NULL OR fulfilled_at IS NOT NULL");
                    table.CheckConstraint("ck_refund_iban_requests_itemless_on_refund_failure", "order_item_id IS NOT NULL OR (reason IN ('CardRefundFailed', 'TransferFailed') AND refund_id IS NOT NULL)");
                    table.CheckConstraint("ck_refund_iban_requests_reason", "\"reason\" IN ('ReturnAfterIbanDeleted', 'TransferFailed', 'CardRefundFailed', 'NoticeWithoutIban', 'CancellationOrRemoval', 'AdminRequest')");
                    table.CheckConstraint("ck_refund_iban_requests_refund_by_reason", "(refund_id IS NOT NULL) = (reason = 'CardRefundFailed' OR (reason = 'TransferFailed' AND order_item_id IS NULL))");
                    table.ForeignKey(
                        name: "fk_refund_iban_requests_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_refund_iban_requests_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_refund_iban_requests_refund_ibans_refund_iban_id",
                        column: x => x.refund_iban_id,
                        principalTable: "refund_ibans",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_refund_iban_requests_refunds_refund_id",
                        column: x => x.refund_id,
                        principalTable: "refunds",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "return_rejections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    withdrawal_id = table.Column<Guid>(type: "uuid", nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    arrival_date = table.Column<DateOnly>(type: "date", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_return_rejections", x => x.id);
                    table.CheckConstraint("ck_return_rejections_quantity_above_zero", "quantity > 0");
                    table.ForeignKey(
                        name: "fk_return_rejections_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_return_rejections_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_return_rejections_withdrawals_withdrawal_id",
                        column: x => x.withdrawal_id,
                        principalTable: "withdrawals",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_refund_iban_requests_admin_id",
                table: "refund_iban_requests",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_iban_requests_order_item_id",
                table: "refund_iban_requests",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_iban_requests_order_item_id1",
                table: "refund_iban_requests",
                column: "order_item_id",
                unique: true,
                filter: "fulfilled_at IS NULL AND closed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_refund_iban_requests_refund_iban_id",
                table: "refund_iban_requests",
                column: "refund_iban_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_iban_requests_refund_id",
                table: "refund_iban_requests",
                column: "refund_id");

            migrationBuilder.CreateIndex(
                name: "ix_refund_iban_requests_refund_id1",
                table: "refund_iban_requests",
                column: "refund_id",
                unique: true,
                filter: "order_item_id IS NULL AND fulfilled_at IS NULL AND closed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_return_rejections_admin_id",
                table: "return_rejections",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_rejections_order_item_id",
                table: "return_rejections",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_return_rejections_withdrawal_id",
                table: "return_rejections",
                column: "withdrawal_id");
        }
    }
}
