using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateStockReservationAndHistoryTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "order_address_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    address_type = table.Column<string>(type: "text", nullable: false),
                    old_recipient_name = table.Column<string>(type: "text", nullable: false),
                    old_phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    old_province_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_district = table.Column<string>(type: "text", nullable: false),
                    old_address_line = table.Column<string>(type: "text", nullable: false),
                    new_recipient_name = table.Column<string>(type: "text", nullable: false),
                    new_phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    new_province_id = table.Column<Guid>(type: "uuid", nullable: false),
                    new_district = table.Column<string>(type: "text", nullable: false),
                    new_address_line = table.Column<string>(type: "text", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_address_histories", x => x.id);
                    table.CheckConstraint("ck_order_address_histories_address_type", "\"address_type\" IN ('ShippingAddress', 'BillingAddress')");
                    table.CheckConstraint("ck_order_address_histories_new_phone_ten_digits", "new_phone ~ '^[0-9]{10}$'");
                    table.CheckConstraint("ck_order_address_histories_old_phone_ten_digits", "old_phone ~ '^[0-9]{10}$'");
                    table.CheckConstraint("ck_order_address_histories_phones_by_address_type", "(address_type = 'ShippingAddress' AND old_phone IS NOT NULL AND new_phone IS NOT NULL) OR (address_type = 'BillingAddress' AND old_phone IS NULL AND new_phone IS NULL)");
                    table.ForeignKey(
                        name: "fk_order_address_histories_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_order_address_histories_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_order_address_histories_provinces_new_province_id",
                        column: x => x.new_province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_order_address_histories_provinces_old_province_id",
                        column: x => x.old_province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_email_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    old_email = table.Column<string>(type: "text", nullable: false),
                    new_email = table.Column<string>(type: "text", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_email_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_order_email_histories_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_order_email_histories_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "stock_reservations",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    coupon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_stock_reservations", x => x.id);
                    table.CheckConstraint("ck_stock_reservations_exactly_one_of_variant_and_coupon", "num_nonnulls(variant_id, coupon_id) = 1");
                    table.CheckConstraint("ck_stock_reservations_quantity_above_zero_one_if_coupon", "quantity > 0 AND (coupon_id IS NULL OR quantity = 1)");
                    table.ForeignKey(
                        name: "fk_stock_reservations_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stock_reservations_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_stock_reservations_variants_variant_id",
                        column: x => x.variant_id,
                        principalTable: "variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_order_address_histories_admin_id",
                table: "order_address_histories",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_address_histories_new_province_id",
                table: "order_address_histories",
                column: "new_province_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_address_histories_old_province_id",
                table: "order_address_histories",
                column: "old_province_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_address_histories_order_id_address_type_created_at",
                table: "order_address_histories",
                columns: new[] { "order_id", "address_type", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_order_email_histories_admin_id",
                table: "order_email_histories",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_email_histories_order_id_created_at",
                table: "order_email_histories",
                columns: new[] { "order_id", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_coupon_id",
                table: "stock_reservations",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_order_id",
                table: "stock_reservations",
                column: "order_id",
                unique: true,
                filter: "coupon_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_order_id_variant_id",
                table: "stock_reservations",
                columns: new[] { "order_id", "variant_id" },
                unique: true,
                filter: "variant_id IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_stock_reservations_variant_id",
                table: "stock_reservations",
                column: "variant_id");
        }
    }
}
