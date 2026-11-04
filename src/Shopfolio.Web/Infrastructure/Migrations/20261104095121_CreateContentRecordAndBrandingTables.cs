using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateContentRecordAndBrandingTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "about_pages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    short_introduction = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: true),
                    long_text = table.Column<string>(type: "text", nullable: true),
                    publish_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_about_pages", x => x.id);
                    table.CheckConstraint("ck_about_pages_publish_status", "\"publish_status\" IN ('Draft', 'Published')");
                });

            migrationBuilder.CreateTable(
                name: "announcements",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    text = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: true),
                    link_url = table.Column<string>(type: "text", nullable: true),
                    start_date = table.Column<DateOnly>(type: "date", nullable: true),
                    end_date = table.Column<DateOnly>(type: "date", nullable: true),
                    publish_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_announcements", x => x.id);
                    table.CheckConstraint("ck_announcements_end_not_before_start", "start_date IS NULL OR end_date IS NULL OR end_date >= start_date");
                    table.CheckConstraint("ck_announcements_publish_status", "\"publish_status\" IN ('Draft', 'Published')");
                });

            migrationBuilder.CreateTable(
                name: "brandings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    brand_name = table.Column<string>(type: "text", nullable: true),
                    logo_id = table.Column<Guid>(type: "uuid", nullable: true),
                    favicon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    brand_color = table.Column<string>(type: "text", nullable: true),
                    is_platform_signature_enabled = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_brandings", x => x.id);
                    table.CheckConstraint("ck_brandings_brand_color_format", "brand_color ~ '^#[0-9A-F]{6}$'");
                    table.ForeignKey(
                        name: "fk_brandings_images_favicon_id",
                        column: x => x.favicon_id,
                        principalTable: "images",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_brandings_images_logo_id",
                        column: x => x.logo_id,
                        principalTable: "images",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "custom_pages",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    text = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_shown_in_menu = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    publish_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    slug = table.Column<string>(type: "text", nullable: false),
                    slug_fixed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_custom_pages", x => x.id);
                    table.CheckConstraint("ck_custom_pages_publish_status", "\"publish_status\" IN ('Draft', 'Published')");
                    table.CheckConstraint("ck_custom_pages_slug_format", "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                });

            migrationBuilder.CreateTable(
                name: "faq_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    question = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    answer = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    publish_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_faq_items", x => x.id);
                    table.CheckConstraint("ck_faq_items_publish_status", "\"publish_status\" IN ('Draft', 'Published')");
                });

            migrationBuilder.CreateTable(
                name: "offerings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    short_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_shown_on_homepage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    publish_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    slug = table.Column<string>(type: "text", nullable: false),
                    slug_fixed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_offerings", x => x.id);
                    table.CheckConstraint("ck_offerings_publish_status", "\"publish_status\" IN ('Draft', 'Published')");
                    table.CheckConstraint("ck_offerings_slug_format", "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                });

            migrationBuilder.CreateTable(
                name: "portfolio_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    title = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    short_description = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false),
                    is_shown_on_homepage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    publish_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Draft"),
                    slug = table.Column<string>(type: "text", nullable: false),
                    slug_fixed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_portfolio_items", x => x.id);
                    table.CheckConstraint("ck_portfolio_items_publish_status", "\"publish_status\" IN ('Draft', 'Published')");
                    table.CheckConstraint("ck_portfolio_items_slug_format", "slug ~ '^[a-z0-9]+(-[a-z0-9]+)*$'");
                });

            migrationBuilder.CreateTable(
                name: "social_links",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    social_platform = table.Column<string>(type: "text", nullable: false),
                    url = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_social_links", x => x.id);
                    table.CheckConstraint("ck_social_links_social_platform", "\"social_platform\" IN ('Instagram', 'Facebook', 'X', 'LinkedIn', 'YouTube', 'TikTok')");
                });

            migrationBuilder.CreateIndex(
                name: "ix_custom_pages_is_shown_in_menu_sort_order",
                table: "custom_pages",
                columns: new[] { "is_shown_in_menu", "sort_order" },
                filter: "is_shown_in_menu AND publish_status = 'Published'");

            migrationBuilder.CreateIndex(
                name: "ix_custom_pages_publish_status_sort_order",
                table: "custom_pages",
                columns: new[] { "publish_status", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_custom_pages_slug",
                table: "custom_pages",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_faq_items_publish_status_sort_order",
                table: "faq_items",
                columns: new[] { "publish_status", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_offerings_is_shown_on_homepage_sort_order",
                table: "offerings",
                columns: new[] { "is_shown_on_homepage", "sort_order" },
                filter: "is_shown_on_homepage AND publish_status = 'Published'");

            migrationBuilder.CreateIndex(
                name: "ix_offerings_publish_status_sort_order",
                table: "offerings",
                columns: new[] { "publish_status", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_offerings_slug",
                table: "offerings",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_items_is_shown_on_homepage_sort_order",
                table: "portfolio_items",
                columns: new[] { "is_shown_on_homepage", "sort_order" },
                filter: "is_shown_on_homepage AND publish_status = 'Published'");

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_items_publish_status_sort_order",
                table: "portfolio_items",
                columns: new[] { "publish_status", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "ix_portfolio_items_slug",
                table: "portfolio_items",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_social_links_social_platform",
                table: "social_links",
                column: "social_platform",
                unique: true);

            // The one row of AboutPage, Announcement and Branding (06 2.3.2): a unique index on a constant expression allows a second row never.
            // EF cannot model an expression index, so it is written here by hand (09 10.6 item 1).
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_about_pages_single_row ON public.about_pages ((true));");
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_announcements_single_row ON public.announcements ((true));");
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_brandings_single_row ON public.brandings ((true));");
        }
    }
}
