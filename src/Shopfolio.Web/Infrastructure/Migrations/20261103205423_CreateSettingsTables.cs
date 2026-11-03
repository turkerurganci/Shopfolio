using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateSettingsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "countries",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_countries", x => x.id);
                    table.CheckConstraint("ck_countries_code_two_capital_letters", "code ~ '^[A-Z]{2}$'");
                });

            migrationBuilder.CreateTable(
                name: "legal_text_versions",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    legal_text_type = table.Column<string>(type: "text", nullable: false),
                    version_number = table.Column<int>(type: "integer", nullable: true),
                    published_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    text = table.Column<string>(type: "text", nullable: false),
                    hosting_location = table.Column<string>(type: "text", nullable: true),
                    template_version = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_legal_text_versions", x => x.id);
                    table.CheckConstraint("ck_legal_text_versions_draft_only_for_firm_edited_text", "published_at IS NOT NULL OR legal_text_type IN ('PrivacyNotice', 'CookiePolicy')");
                    table.CheckConstraint("ck_legal_text_versions_hosting_location_only_on_privacy_notice", "(hosting_location IS NULL OR legal_text_type = 'PrivacyNotice') AND (legal_text_type <> 'PrivacyNotice' OR published_at IS NULL OR hosting_location IS NOT NULL)");
                    table.CheckConstraint("ck_legal_text_versions_legal_text_type", "\"legal_text_type\" IN ('PreInformationForm', 'DistanceSalesContract', 'PrivacyNotice', 'CookiePolicy')");
                    table.CheckConstraint("ck_legal_text_versions_number_and_published_at_together", "(version_number IS NULL) = (published_at IS NULL)");
                    table.CheckConstraint("ck_legal_text_versions_template_version_only_generated", "template_version IS NULL OR (published_at IS NOT NULL AND legal_text_type IN ('PreInformationForm', 'DistanceSalesContract'))");
                });

            migrationBuilder.CreateTable(
                name: "provinces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    code = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_provinces", x => x.id);
                    table.CheckConstraint("ck_provinces_code_two_digits", "code ~ '^[0-9]{2}$'");
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    company_type = table.Column<string>(type: "text", nullable: true),
                    legal_name = table.Column<string>(type: "text", nullable: true),
                    tax_id_number = table.Column<string>(type: "text", nullable: true),
                    mersis_number = table.Column<string>(type: "text", nullable: true),
                    trade_registry_number = table.Column<string>(type: "text", nullable: true),
                    province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    district = table.Column<string>(type: "text", nullable: true),
                    address_line = table.Column<string>(type: "text", nullable: true),
                    phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    email = table.Column<string>(type: "text", nullable: true),
                    kep_address = table.Column<string>(type: "text", nullable: true),
                    chamber_name = table.Column<string>(type: "text", nullable: true),
                    business_name = table.Column<string>(type: "text", nullable: true),
                    professional_conduct_rules = table.Column<string>(type: "text", nullable: true),
                    etbis_verification_url = table.Column<string>(type: "text", nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.CheckConstraint("ck_companies_company_type", "\"company_type\" IN ('Tradesman', 'NaturalPersonMerchant', 'LegalEntity')");
                    table.CheckConstraint("ck_companies_phone_ten_digits", "phone ~ '^[0-9]{10}$'");
                    table.ForeignKey(
                        name: "fk_companies_provinces_province_id",
                        column: x => x.province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_delivery_provinces",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    province_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_delivery_provinces", x => x.id);
                    table.ForeignKey(
                        name: "fk_company_delivery_provinces_provinces_province_id",
                        column: x => x.province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "company_settings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    is_bank_transfer_enabled = table.Column<bool>(type: "boolean", nullable: false),
                    bank_transfer_iban = table.Column<string>(type: "text", nullable: true),
                    payment_methods_version = table.Column<int>(type: "integer", nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    free_shipping_threshold = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    minimum_order_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    shipping_fee_version = table.Column<int>(type: "integer", nullable: false),
                    delivery_provinces_version = table.Column<int>(type: "integer", nullable: false),
                    default_shipping_period = table.Column<int>(type: "integer", nullable: false),
                    bank_transfer_payment_window = table.Column<int>(type: "integer", nullable: false),
                    periods_version = table.Column<int>(type: "integer", nullable: false),
                    download_allowance = table.Column<int>(type: "integer", nullable: false),
                    default_vat_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    product_defaults_version = table.Column<int>(type: "integer", nullable: false),
                    return_recipient_name = table.Column<string>(type: "text", nullable: true),
                    return_province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    return_district = table.Column<string>(type: "text", nullable: true),
                    return_address_line = table.Column<string>(type: "text", nullable: true),
                    return_phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    return_address_version = table.Column<int>(type: "integer", nullable: false),
                    homepage_layout = table.Column<string>(type: "text", nullable: false),
                    menu_name_products = table.Column<string>(type: "text", nullable: false),
                    menu_name_offerings = table.Column<string>(type: "text", nullable: false),
                    menu_name_portfolio_items = table.Column<string>(type: "text", nullable: false),
                    menu_name_about_page = table.Column<string>(type: "text", nullable: false),
                    menu_name_faq_items = table.Column<string>(type: "text", nullable: false),
                    menu_name_contact = table.Column<string>(type: "text", nullable: false),
                    whats_app_number = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    homepage_version = table.Column<int>(type: "integer", nullable: false),
                    is_sales_temporarily_closed = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_company_settings", x => x.id);
                    table.CheckConstraint("ck_company_settings_amounts_not_negative", "shipping_fee >= 0 AND (free_shipping_threshold IS NULL OR free_shipping_threshold >= 0) AND (minimum_order_amount IS NULL OR minimum_order_amount >= 0)");
                    table.CheckConstraint("ck_company_settings_bank_transfer_iban_required", "NOT is_bank_transfer_enabled OR bank_transfer_iban IS NOT NULL");
                    table.CheckConstraint("ck_company_settings_bank_transfer_payment_window_fence", "bank_transfer_payment_window BETWEEN 1 AND 3");
                    table.CheckConstraint("ck_company_settings_default_shipping_period_fence", "default_shipping_period BETWEEN 0 AND 10");
                    table.CheckConstraint("ck_company_settings_default_vat_rate_range", "default_vat_rate >= 0 AND default_vat_rate < 100");
                    table.CheckConstraint("ck_company_settings_download_allowance_fence", "download_allowance >= 1");
                    table.CheckConstraint("ck_company_settings_homepage_layout", "\"homepage_layout\" IN ('ContentFirst', 'StoreFirst')");
                    table.CheckConstraint("ck_company_settings_return_address_all_or_none", "(return_recipient_name IS NULL) = (return_province_id IS NULL) AND (return_province_id IS NULL) = (return_district IS NULL) AND (return_district IS NULL) = (return_address_line IS NULL)");
                    table.CheckConstraint("ck_company_settings_return_phone_ten_digits", "return_phone ~ '^[0-9]{10}$'");
                    table.CheckConstraint("ck_company_settings_whats_app_number_ten_digits", "whats_app_number ~ '^[0-9]{10}$'");
                    table.ForeignKey(
                        name: "fk_company_settings_provinces_return_province_id",
                        column: x => x.return_province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_company_delivery_provinces_province_id",
                table: "company_delivery_provinces",
                column: "province_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_countries_code",
                table: "countries",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_legal_text_versions_legal_text_type",
                table: "legal_text_versions",
                column: "legal_text_type",
                unique: true,
                filter: "published_at IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_legal_text_versions_legal_text_type_version_number",
                table: "legal_text_versions",
                columns: new[] { "legal_text_type", "version_number" },
                unique: true,
                filter: "version_number IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_provinces_code",
                table: "provinces",
                column: "code",
                unique: true);

            // The one row of Company and CompanySettings (06 2.3.2): a unique index on a constant expression allows a second row never.
            // EF cannot model an expression index, so it is written here by hand (09 10.6 item 1).
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_companies_single_row ON public.companies ((true));");
            migrationBuilder.Sql("CREATE UNIQUE INDEX ix_company_settings_single_row ON public.company_settings ((true));");

            // The migration history table is closed to the application and read-only users (09 10.8 item 3). The first migration creates the
            // privilege split of that table; the user names are read from the schema owner's role settings (09 10.8 item 5), never written here.
            migrationBuilder.Sql(@"DO $$ BEGIN
  EXECUTE format('REVOKE ALL ON TABLE public.""__EFMigrationsHistory"" FROM %I, %I', current_setting('shopfolio.application_user'), current_setting('shopfolio.reader_user'));
END $$;");
        }
    }
}
