using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateBranchContentLinkAndContactRequestTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    district = table.Column<string>(type: "text", nullable: true),
                    address_line = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    working_hours = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    map_url = table.Column<string>(type: "text", nullable: true),
                    image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    image_alt_text = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    publish_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branches", x => x.id);
                    table.CheckConstraint("ck_branches_image_alt_text_only_with_image", "image_id IS NOT NULL OR image_alt_text IS NULL");
                    table.CheckConstraint("ck_branches_phone_ten_digits", "phone ~ '^[0-9]{10}$'");
                    table.CheckConstraint("ck_branches_publish_status", "\"publish_status\" IN ('Draft', 'Published')");
                    table.ForeignKey(
                        name: "fk_branches_images_image_id",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_branches_provinces_province_id",
                        column: x => x.province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "contact_requests",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: false),
                    phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    contact_topic = table.Column<string>(type: "text", nullable: false),
                    message = table.Column<string>(type: "text", nullable: false),
                    contact_request_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Open"),
                    last_closed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_contact_requests", x => x.id);
                    table.CheckConstraint("ck_contact_requests_closed_has_last_closed_at", "contact_request_status <> 'Closed' OR last_closed_at IS NOT NULL");
                    table.CheckConstraint("ck_contact_requests_contact_request_status", "\"contact_request_status\" IN ('Open', 'Closed')");
                    table.CheckConstraint("ck_contact_requests_contact_topic", "\"contact_topic\" IN ('GeneralQuestion', 'AboutOrder', 'AboutProduct', 'PersonalDataRequest', 'Other')");
                    table.CheckConstraint("ck_contact_requests_phone_ten_digits", "phone ~ '^[0-9]{10}$'");
                });

            migrationBuilder.CreateTable(
                name: "offering_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    offering_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offering_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_offering_products_offerings_offering_id",
                        column: x => x.offering_id,
                        principalTable: "offerings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_offering_products_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portfolio_item_products",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    portfolio_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_item_products", x => x.id);
                    table.ForeignKey(
                        name: "fk_portfolio_item_products_portfolio_items_portfolio_item_id",
                        column: x => x.portfolio_item_id,
                        principalTable: "portfolio_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_portfolio_item_products_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_branches_image_id",
                table: "branches",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_branches_province_id",
                table: "branches",
                column: "province_id");

            migrationBuilder.CreateIndex(
                name: "ix_branches_publish_status_sort_order",
                table: "branches",
                columns: new[] { "publish_status", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_contact_requests_contact_request_status_created_at",
                table: "contact_requests",
                columns: new[] { "contact_request_status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_offering_products_offering_id_product_id",
                table: "offering_products",
                columns: new[] { "offering_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_offering_products_product_id",
                table: "offering_products",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_item_products_portfolio_item_id_product_id",
                table: "portfolio_item_products",
                columns: new[] { "portfolio_item_id", "product_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_item_products_product_id",
                table: "portfolio_item_products",
                column: "product_id");

            // The disposal scan of ContactRequest (06 4.57, 7.4): the topic and the start of the retention, which is the last closing moment
            // or, when it was never closed, the creation moment. EF cannot model an expression index, so it is written here by hand (09 10.6 item 1).
            migrationBuilder.Sql("CREATE INDEX ix_contact_requests_contact_topic_retention_start ON public.contact_requests (contact_topic, (COALESCE(last_closed_at, created_at)));");
        }
    }
}
