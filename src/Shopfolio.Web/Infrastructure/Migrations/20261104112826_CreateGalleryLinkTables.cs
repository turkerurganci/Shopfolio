using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateGalleryLinkTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "about_page_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    about_page_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    alt_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_about_page_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_about_page_images_about_pages_about_page_id",
                        column: x => x.about_page_id,
                        principalTable: "about_pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_about_page_images_images_image_id",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "custom_page_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    custom_page_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    alt_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_page_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_custom_page_images_custom_pages_custom_page_id",
                        column: x => x.custom_page_id,
                        principalTable: "custom_pages",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_custom_page_images_images_image_id",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "faq_item_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    faq_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    alt_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_faq_item_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_faq_item_images_faq_items_faq_item_id",
                        column: x => x.faq_item_id,
                        principalTable: "faq_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_faq_item_images_images_image_id",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "offering_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    offering_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    alt_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offering_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_offering_images_images_image_id",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_offering_images_offerings_offering_id",
                        column: x => x.offering_id,
                        principalTable: "offerings",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "portfolio_item_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    portfolio_item_id = table.Column<Guid>(type: "uuid", nullable: false),
                    image_id = table.Column<Guid>(type: "uuid", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_main = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    alt_text = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_item_images", x => x.id);
                    table.ForeignKey(
                        name: "fk_portfolio_item_images_images_image_id",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_portfolio_item_images_portfolio_items_portfolio_item_id",
                        column: x => x.portfolio_item_id,
                        principalTable: "portfolio_items",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "ix_about_page_images_about_page_id",
                table: "about_page_images",
                column: "about_page_id",
                unique: true,
                filter: "is_main");

            migrationBuilder.CreateIndex(
                name: "ix_about_page_images_about_page_id_image_id",
                table: "about_page_images",
                columns: new[] { "about_page_id", "image_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_about_page_images_image_id",
                table: "about_page_images",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_custom_page_images_custom_page_id",
                table: "custom_page_images",
                column: "custom_page_id",
                unique: true,
                filter: "is_main");

            migrationBuilder.CreateIndex(
                name: "ix_custom_page_images_custom_page_id_image_id",
                table: "custom_page_images",
                columns: new[] { "custom_page_id", "image_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_custom_page_images_image_id",
                table: "custom_page_images",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_faq_item_images_faq_item_id",
                table: "faq_item_images",
                column: "faq_item_id",
                unique: true,
                filter: "is_main");

            migrationBuilder.CreateIndex(
                name: "ix_faq_item_images_faq_item_id_image_id",
                table: "faq_item_images",
                columns: new[] { "faq_item_id", "image_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_faq_item_images_image_id",
                table: "faq_item_images",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_offering_images_image_id",
                table: "offering_images",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_offering_images_offering_id",
                table: "offering_images",
                column: "offering_id",
                unique: true,
                filter: "is_main");

            migrationBuilder.CreateIndex(
                name: "ix_offering_images_offering_id_image_id",
                table: "offering_images",
                columns: new[] { "offering_id", "image_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_item_images_image_id",
                table: "portfolio_item_images",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_item_images_portfolio_item_id",
                table: "portfolio_item_images",
                column: "portfolio_item_id",
                unique: true,
                filter: "is_main");

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_item_images_portfolio_item_id_image_id",
                table: "portfolio_item_images",
                columns: new[] { "portfolio_item_id", "image_id" },
                unique: true);
        }
    }
}
