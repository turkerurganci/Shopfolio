using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateCatalogCoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The trigram index below needs pg_trgm, which the database creator user installs (09 10.6 item 8; K-1124): the migration does not
            // install it, it checks it first and stops with a clear message.
            migrationBuilder.Sql(
                """
                DO $$ BEGIN
                  IF NOT EXISTS (SELECT 1 FROM pg_extension WHERE extname = 'pg_trgm') THEN
                    RAISE EXCEPTION 'pg_trgm extension is not installed; the database creator user installs it (DEPLOY_RUNBOOK)';
                  END IF;
                END $$;
                """);

            migrationBuilder.CreateTable(
                name: "categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    parent_category_id = table.Column<Guid>(type: "uuid", nullable: true),
                    name = table.Column<string>(type: "text", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    slug = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_categories", x => x.id);
                    table.CheckConstraint("ck_categories_level_fence", "level BETWEEN 1 AND 3");
                    table.CheckConstraint("ck_categories_level_follows_parent", "(parent_category_id IS NULL) = (level = 1)");
                    table.CheckConstraint("ck_categories_slug_format", "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.ForeignKey(
                        name: "fk_categories_categories_parent_category_id",
                        column: x => x.parent_category_id,
                        principalTable: "categories",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    storage_key = table.Column<string>(type: "text", nullable: false),
                    image_format = table.Column<string>(type: "text", nullable: false),
                    width = table.Column<int>(type: "integer", nullable: false),
                    height = table.Column<int>(type: "integer", nullable: false),
                    derivatives_ready_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_images", x => x.id);
                    table.CheckConstraint("ck_images_dimensions_within_limits", "width > 0 AND height > 0 AND width <= 2560 AND height <= 2560");
                    table.CheckConstraint("ck_images_image_format", "\"image_format\" IN ('Jpeg', 'Png', 'WebP')");
                });

            migrationBuilder.CreateTable(
                name: "products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    name_search_form = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    product_type = table.Column<string>(type: "text", nullable: false),
                    publish_status = table.Column<string>(type: "text", nullable: false),
                    vat_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    origin_country_id = table.Column<Guid>(type: "uuid", nullable: true),
                    measurement_unit = table.Column<string>(type: "text", nullable: true),
                    shipping_period = table.Column<int>(type: "integer", nullable: true),
                    fulfilment_period = table.Column<int>(type: "integer", nullable: true),
                    max_quantity_per_order = table.Column<int>(type: "integer", nullable: true),
                    withdrawal_exemption_reason = table.Column<string>(type: "text", nullable: true),
                    slug = table.Column<string>(type: "text", nullable: false),
                    slug_fixed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_products", x => x.id);
                    table.CheckConstraint("ck_products_fulfilment_period_fence", "fulfilment_period >= 1");
                    table.CheckConstraint("ck_products_max_quantity_per_order_fence", "max_quantity_per_order >= 1");
                    table.CheckConstraint("ck_products_measurement_unit", "\"measurement_unit\" IN ('Kilogram', 'Gram', 'Litre', 'Millilitre', 'Metre', 'Centimetre', 'SquareMetre', 'CubicMetre', 'Piece')");
                    table.CheckConstraint("ck_products_product_type", "\"product_type\" IN ('Physical', 'Digital', 'Service')");
                    table.CheckConstraint("ck_products_publish_status", "\"publish_status\" IN ('Draft', 'Published', 'Archived')");
                    table.CheckConstraint("ck_products_shipping_period_fence", "shipping_period BETWEEN 0 AND 10");
                    table.CheckConstraint("ck_products_slug_format", "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                    table.CheckConstraint("ck_products_vat_rate_range", "vat_rate >= 0 AND vat_rate < 100");
                    table.CheckConstraint("ck_products_withdrawal_exemption_reason", "\"withdrawal_exemption_reason\" IN ('MadeToOrder', 'Perishable', 'ExpiryDated', 'HygieneSensitive', 'SealedMedia')");
                    table.ForeignKey(
                        name: "fk_products_countries_origin_country_id",
                        column: x => x.origin_country_id,
                        principalTable: "countries",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "options",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_options", x => x.id);
                    table.CheckConstraint("ck_options_sort_order_fence", "sort_order BETWEEN 1 AND 2");
                    table.ForeignKey(
                        name: "fk_options_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_categories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    category_id = table.Column<Guid>(type: "uuid", nullable: false),
                    is_primary_category = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_categories", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_categories_categories_category_id",
                        column: x => x.category_id,
                        principalTable: "categories",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_product_categories_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "product_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false),
                    alt_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_product_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_product_images_images_image_id",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_product_images_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "option_values",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    option_id = table.Column<Guid>(type: "uuid", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_option_values", x => x.id);
                    table.ForeignKey(
                        name: "fk_option_values_options_option_id",
                        column: x => x.option_id,
                        principalTable: "options",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "variants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    first_option_value_id = table.Column<Guid>(type: "uuid", nullable: true),
                    second_option_value_id = table.Column<Guid>(type: "uuid", nullable: true),
                    sku = table.Column<string>(type: "text", nullable: false),
                    normalized_sku = table.Column<string>(type: "text", nullable: false),
                    price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    net_quantity = table.Column<decimal>(type: "numeric(12,3)", precision: 12, scale: 3, nullable: true),
                    publish_status = table.Column<string>(type: "text", nullable: false),
                    stock_quantity = table.Column<int>(type: "integer", nullable: true),
                    reserved_quantity = table.Column<int>(type: "integer", nullable: false),
                    image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    first_on_sale_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_variants", x => x.id);
                    table.CheckConstraint("ck_variants_net_quantity_above_zero", "net_quantity IS NULL OR net_quantity > 0");
                    table.CheckConstraint("ck_variants_price_above_zero", "price > 0");
                    table.CheckConstraint("ck_variants_publish_status", "\"publish_status\" IN ('Draft', 'Published', 'Archived')");
                    table.CheckConstraint("ck_variants_reserved_quantity_not_negative", "reserved_quantity >= 0");
                    table.CheckConstraint("ck_variants_second_value_needs_first", "second_option_value_id IS NULL OR first_option_value_id IS NOT NULL");
                    table.CheckConstraint("ck_variants_stock_not_below_reserved", "stock_quantity IS NULL OR (stock_quantity >= 0 AND stock_quantity >= reserved_quantity)");
                    table.ForeignKey(
                        name: "fk_variants_images_image_id",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_variants_option_values_first_option_value_id",
                        column: x => x.first_option_value_id,
                        principalTable: "option_values",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_variants_option_values_second_option_value_id",
                        column: x => x.second_option_value_id,
                        principalTable: "option_values",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_variants_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_categories_parent_category_id_name",
                table: "categories",
                columns: new[] { "parent_category_id", "name" });

            migrationBuilder.CreateIndex(
                name: "ix_categories_slug",
                table: "categories",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_images_created_at",
                table: "images",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_images_storage_key",
                table: "images",
                column: "storage_key");

            migrationBuilder.CreateIndex(
                name: "ix_option_values_option_id_sort_order",
                table: "option_values",
                columns: new[] { "option_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_options_product_id_sort_order",
                table: "options",
                columns: new[] { "product_id", "sort_order" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_categories_category_id",
                table: "product_categories",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_categories_product_id",
                table: "product_categories",
                column: "product_id",
                unique: true,
                filter: "is_primary_category");

            migrationBuilder.CreateIndex(
                name: "ix_product_categories_product_id_category_id",
                table: "product_categories",
                columns: new[] { "product_id", "category_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_product_images_image_id",
                table: "product_images",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_product_images_product_id",
                table: "product_images",
                column: "product_id",
                unique: true,
                filter: "is_main");

            migrationBuilder.CreateIndex(
                name: "ix_product_images_product_id_image_id",
                table: "product_images",
                columns: new[] { "product_id", "image_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_products_name_search_form",
                table: "products",
                column: "name_search_form")
                .Annotation("Npgsql:IndexMethod", "gin")
                .Annotation("Npgsql:IndexOperators", new[] { "gin_trgm_ops" });

            migrationBuilder.CreateIndex(
                name: "ix_products_origin_country_id",
                table: "products",
                column: "origin_country_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_publish_status_created_at_id",
                table: "products",
                columns: new[] { "publish_status", "created_at", "id" });

            migrationBuilder.CreateIndex(
                name: "ix_products_slug",
                table: "products",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_variants_first_option_value_id",
                table: "variants",
                column: "first_option_value_id");

            migrationBuilder.CreateIndex(
                name: "ix_variants_image_id",
                table: "variants",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_variants_normalized_sku",
                table: "variants",
                column: "normalized_sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_variants_product_id_first_option_value_id_second_option_val",
                table: "variants",
                columns: new[] { "product_id", "first_option_value_id", "second_option_value_id" },
                unique: true)
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "ix_variants_second_option_value_id",
                table: "variants",
                column: "second_option_value_id");
        }
    }
}
