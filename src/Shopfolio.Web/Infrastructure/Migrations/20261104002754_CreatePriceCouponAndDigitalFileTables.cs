using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreatePriceCouponAndDigitalFileTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "coupons",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    normalized_code = table.Column<string>(type: "text", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    minimum_basket_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false),
                    total_usage_limit = table.Column<int>(type: "integer", nullable: false),
                    used_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    reserved_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_coupons", x => x.id);
                    table.CheckConstraint("ck_coupons_amount_above_zero", "amount IS NULL OR amount > 0");
                    table.CheckConstraint("ck_coupons_amount_needs_larger_minimum_basket", "amount IS NULL OR (minimum_basket_amount IS NOT NULL AND minimum_basket_amount > amount)");
                    table.CheckConstraint("ck_coupons_end_not_before_start", "end_date >= start_date");
                    table.CheckConstraint("ck_coupons_exactly_one_value", "(percentage IS NULL) <> (amount IS NULL)");
                    table.CheckConstraint("ck_coupons_minimum_basket_amount_not_negative", "minimum_basket_amount IS NULL OR minimum_basket_amount >= 0");
                    table.CheckConstraint("ck_coupons_percentage_range", "percentage IS NULL OR (percentage > 0 AND percentage < 100)");
                    table.CheckConstraint("ck_coupons_usage_counts_not_negative", "total_usage_limit >= 0 AND used_count >= 0 AND reserved_count >= 0");
                });

            migrationBuilder.CreateTable(
                name: "digital_files",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    storage_key = table.Column<string>(type: "text", nullable: false),
                    file_name = table.Column<string>(type: "text", nullable: false),
                    size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    upload_completed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    attached_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    detached_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_digital_files", x => x.id);
                    table.CheckConstraint("ck_digital_files_at_most_one_owner", "product_id IS NULL OR variant_id IS NULL");
                    table.CheckConstraint("ck_digital_files_detached_needs_attached", "detached_at IS NULL OR attached_at IS NOT NULL");
                    table.CheckConstraint("ck_digital_files_incomplete_upload_unattached", "upload_completed_at IS NOT NULL OR (product_id IS NULL AND variant_id IS NULL AND attached_at IS NULL)");
                    table.CheckConstraint("ck_digital_files_size_above_zero", "size_bytes > 0");
                    table.ForeignKey(
                        name: "fk_digital_files_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_digital_files_variants_variant_id",
                        column: x => x.variant_id,
                        principalTable: "variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "discounts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    start_date = table.Column<DateOnly>(type: "date", nullable: false),
                    end_date = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_discounts", x => x.id);
                    table.CheckConstraint("ck_discounts_end_not_before_start", "end_date >= start_date");
                    table.CheckConstraint("ck_discounts_percentage_range", "percentage > 0 AND percentage < 100");
                    table.ForeignKey(
                        name: "fk_discounts_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "price_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    started_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_price_histories", x => x.id);
                    table.CheckConstraint("ck_price_histories_price_above_zero", "price > 0");
                    table.ForeignKey(
                        name: "fk_price_histories_variants_variant_id",
                        column: x => x.variant_id,
                        principalTable: "variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_coupons_normalized_code",
                table: "coupons",
                column: "normalized_code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_digital_files_created_at",
                table: "digital_files",
                column: "created_at",
                filter: "upload_completed_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_digital_files_product_id",
                table: "digital_files",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_digital_files_product_id1",
                table: "digital_files",
                column: "product_id",
                unique: true,
                filter: "detached_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_digital_files_storage_key",
                table: "digital_files",
                column: "storage_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_digital_files_variant_id",
                table: "digital_files",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_digital_files_variant_id1",
                table: "digital_files",
                column: "variant_id",
                unique: true,
                filter: "detached_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_discounts_product_id",
                table: "discounts",
                column: "product_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_price_histories_variant_id_started_at",
                table: "price_histories",
                columns: new[] { "variant_id", "started_at" });
        }
    }
}
