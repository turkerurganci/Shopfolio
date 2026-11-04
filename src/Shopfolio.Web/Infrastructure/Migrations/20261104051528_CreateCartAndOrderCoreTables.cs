using System;
using System.Net;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Shopfolio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateCartAndOrderCoreTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "carts",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    guest_cart_key_hash = table.Column<string>(type: "text", nullable: true),
                    last_accessed_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    is_payment_unavailable_notice_pending = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_carts", x => x.id);
                    table.CheckConstraint("ck_carts_exactly_one_owner", "num_nonnulls(customer_id, guest_cart_key_hash) = 1");
                    table.CheckConstraint("ck_carts_last_accessed_at_only_for_guest", "(guest_cart_key_hash IS NULL) = (last_accessed_at IS NULL)");
                    table.ForeignKey(
                        name: "fk_carts_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "orders",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_number = table.Column<string>(type: "text", nullable: false),
                    order_access_key = table.Column<string>(type: "text", nullable: true),
                    confirmation_key = table.Column<string>(type: "text", nullable: false),
                    order_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Placed"),
                    payment_status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    payment_method = table.Column<string>(type: "text", nullable: false),
                    customer_id = table.Column<Guid>(type: "uuid", nullable: true),
                    is_guest_order = table.Column<bool>(type: "boolean", nullable: false),
                    is_account_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    contact_email = table.Column<string>(type: "text", nullable: true),
                    normalized_contact_email = table.Column<string>(type: "text", nullable: true),
                    shipping_recipient_name = table.Column<string>(type: "text", nullable: true),
                    shipping_phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    shipping_province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    shipping_district = table.Column<string>(type: "text", nullable: true),
                    shipping_address_line = table.Column<string>(type: "text", nullable: true),
                    billing_recipient_name = table.Column<string>(type: "text", nullable: true),
                    billing_province_id = table.Column<Guid>(type: "uuid", nullable: true),
                    billing_district = table.Column<string>(type: "text", nullable: true),
                    billing_address_line = table.Column<string>(type: "text", nullable: true),
                    ip_address = table.Column<IPAddress>(type: "inet", nullable: true),
                    cart_id = table.Column<Guid>(type: "uuid", nullable: true),
                    coupon_id = table.Column<Guid>(type: "uuid", nullable: true),
                    coupon_code = table.Column<string>(type: "text", nullable: true),
                    total = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    shipping_fee = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    shipping_fee_vat_breakdown = table.Column<string>(type: "jsonb", nullable: true),
                    bank_transfer_iban = table.Column<string>(type: "text", nullable: true),
                    shipping_period = table.Column<int>(type: "integer", nullable: true),
                    payment_window_ends_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    payment_reminder_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    carrier = table.Column<string>(type: "text", nullable: true),
                    other_carrier_name = table.Column<string>(type: "text", nullable: true),
                    tracking_number = table.Column<string>(type: "text", nullable: true),
                    is_own_vehicle_delivery = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    company_type = table.Column<string>(type: "text", nullable: false),
                    company_legal_name = table.Column<string>(type: "text", nullable: false),
                    company_tax_id_number = table.Column<string>(type: "text", nullable: true),
                    company_mersis_number = table.Column<string>(type: "text", nullable: true),
                    company_trade_registry_number = table.Column<string>(type: "text", nullable: true),
                    company_province_id = table.Column<Guid>(type: "uuid", nullable: false),
                    company_district = table.Column<string>(type: "text", nullable: false),
                    company_address_line = table.Column<string>(type: "text", nullable: false),
                    company_phone = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    company_email = table.Column<string>(type: "text", nullable: false),
                    company_kep_address = table.Column<string>(type: "text", nullable: false),
                    pre_information_form_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    distance_sales_contract_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    privacy_notice_version_id = table.Column<Guid>(type: "uuid", nullable: false),
                    pre_information_form_text = table.Column<string>(type: "text", nullable: false),
                    distance_sales_contract_text = table.Column<string>(type: "text", nullable: false),
                    version = table.Column<int>(type: "integer", nullable: false, defaultValue: 1)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_orders", x => x.id);
                    table.CheckConstraint("ck_orders_at_most_one_carrier", "num_nonnulls(carrier, other_carrier_name) <= 1");
                    table.CheckConstraint("ck_orders_card_without_payment_reminder", "payment_method <> 'Card' OR payment_reminder_at IS NULL");
                    table.CheckConstraint("ck_orders_company_phone_ten_digits", "company_phone ~ '^[0-9]{10}$'");
                    table.CheckConstraint("ck_orders_company_type", "\"company_type\" IN ('Tradesman', 'NaturalPersonMerchant', 'LegalEntity')");
                    table.CheckConstraint("ck_orders_coupon_and_code_together", "(coupon_id IS NULL) = (coupon_code IS NULL)");
                    table.CheckConstraint("ck_orders_deleted_account_without_link", "NOT is_account_deleted OR customer_id IS NULL");
                    table.CheckConstraint("ck_orders_disposal_fields_together", "(contact_email IS NULL AND num_nonnulls(normalized_contact_email, order_access_key, ip_address, cart_id, customer_id, billing_recipient_name, billing_province_id, billing_district, billing_address_line, shipping_recipient_name, shipping_phone, shipping_province_id, shipping_district, shipping_address_line) = 0) OR (contact_email IS NOT NULL AND num_nulls(normalized_contact_email, order_access_key, ip_address, cart_id, billing_recipient_name, billing_province_id, billing_district, billing_address_line) = 0)");
                    table.CheckConstraint("ck_orders_iban_only_on_bank_transfer", "(payment_method = 'BankTransfer') = (bank_transfer_iban IS NOT NULL)");
                    table.CheckConstraint("ck_orders_order_status", "\"order_status\" IN ('Placed', 'Preparing', 'Shipped', 'Delivered', 'DeliveryFailed', 'Cancelled')");
                    table.CheckConstraint("ck_orders_own_vehicle_without_carrier", "NOT is_own_vehicle_delivery OR num_nonnulls(carrier, other_carrier_name, tracking_number) = 0");
                    table.CheckConstraint("ck_orders_payment_method", "\"payment_method\" IN ('Card', 'BankTransfer')");
                    table.CheckConstraint("ck_orders_payment_status", "\"payment_status\" IN ('Pending', 'Paid', 'Failed', 'PartiallyRefunded', 'Refunded')");
                    table.CheckConstraint("ck_orders_shipping_address_all_or_none", "num_nonnulls(shipping_recipient_name, shipping_phone, shipping_province_id, shipping_district, shipping_address_line) IN (0, 5)");
                    table.CheckConstraint("ck_orders_shipping_fee_and_breakdown", "shipping_fee >= 0 AND (shipping_fee = 0) = (shipping_fee_vat_breakdown IS NULL)");
                    table.CheckConstraint("ck_orders_shipping_period_not_negative", "shipping_period IS NULL OR shipping_period >= 0");
                    table.CheckConstraint("ck_orders_shipping_phone_ten_digits", "shipping_phone ~ '^[0-9]{10}$'");
                    table.CheckConstraint("ck_orders_status_pair_allowed", "(payment_status <> 'Failed' OR order_status = 'Cancelled') AND (payment_status <> 'Pending' OR order_status = 'Placed')");
                    table.CheckConstraint("ck_orders_total_above_zero", "total > 0");
                    table.CheckConstraint("ck_orders_tracking_number_with_carrier", "(tracking_number IS NOT NULL) = (num_nonnulls(carrier, other_carrier_name) = 1)");
                    table.ForeignKey(
                        name: "fk_orders_coupons_coupon_id",
                        column: x => x.coupon_id,
                        principalTable: "coupons",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_customers_customer_id",
                        column: x => x.customer_id,
                        principalTable: "customers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_orders_legal_text_versions_distance_sales_contract_version_",
                        column: x => x.distance_sales_contract_version_id,
                        principalTable: "legal_text_versions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_legal_text_versions_pre_information_form_version_id",
                        column: x => x.pre_information_form_version_id,
                        principalTable: "legal_text_versions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_legal_text_versions_privacy_notice_version_id",
                        column: x => x.privacy_notice_version_id,
                        principalTable: "legal_text_versions",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_provinces_billing_province_id",
                        column: x => x.billing_province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_provinces_company_province_id",
                        column: x => x.company_province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_orders_provinces_shipping_province_id",
                        column: x => x.shipping_province_id,
                        principalTable: "provinces",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "cart_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    cart_id = table.Column<Guid>(type: "uuid", nullable: false),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    first_added_unit_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    removed_product_name = table.Column<string>(type: "text", nullable: true),
                    is_merge_notice_pending = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_cart_items", x => x.id);
                    table.CheckConstraint("ck_cart_items_first_added_unit_price_above_zero", "first_added_unit_price > 0");
                    table.CheckConstraint("ck_cart_items_quantity_above_zero", "quantity > 0");
                    table.CheckConstraint("ck_cart_items_removed_name_without_variant", "variant_id IS NOT NULL OR removed_product_name IS NOT NULL");
                    table.ForeignKey(
                        name: "fk_cart_items_carts_cart_id",
                        column: x => x.cart_id,
                        principalTable: "carts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "fk_cart_items_variants_variant_id",
                        column: x => x.variant_id,
                        principalTable: "variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "checkbox_acceptances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    checkbox_type = table.Column<string>(type: "text", nullable: false),
                    shown_text = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checkbox_acceptances", x => x.id);
                    table.CheckConstraint("ck_checkbox_acceptances_checkbox_type", "\"checkbox_type\" IN ('Digital', 'Service')");
                    table.ForeignKey(
                        name: "fk_checkbox_acceptances_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "internal_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    text = table.Column<string>(type: "text", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_internal_notes", x => x.id);
                    table.ForeignKey(
                        name: "fk_internal_notes_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_internal_notes_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    product_id = table.Column<Guid>(type: "uuid", nullable: true),
                    variant_id = table.Column<Guid>(type: "uuid", nullable: true),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    product_name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    first_option_value = table.Column<string>(type: "text", nullable: true),
                    second_option_value = table.Column<string>(type: "text", nullable: true),
                    image_id = table.Column<Guid>(type: "uuid", nullable: true),
                    product_type = table.Column<string>(type: "text", nullable: false),
                    unit_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    discount_percentage = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: true),
                    discounted_unit_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    reference_price = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: true),
                    coupon_share = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false, defaultValue: 0m),
                    vat_rate = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    vat_amount = table.Column<decimal>(type: "numeric(12,2)", precision: 12, scale: 2, nullable: false),
                    withdrawal_exemption_reason = table.Column<string>(type: "text", nullable: true),
                    fulfilment_period = table.Column<int>(type: "integer", nullable: true),
                    download_allowance = table.Column<int>(type: "integer", nullable: true),
                    download_count = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    digital_file_id = table.Column<Guid>(type: "uuid", nullable: true),
                    delivery_marked_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    delivery_date = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_order_items", x => x.id);
                    table.CheckConstraint("ck_order_items_coupon_share_not_negative", "coupon_share >= 0");
                    table.CheckConstraint("ck_order_items_delivery_mark_and_date_together", "(delivery_marked_at IS NULL) = (delivery_date IS NULL)");
                    table.CheckConstraint("ck_order_items_digital_file_only_digital", "digital_file_id IS NULL OR product_type = 'Digital'");
                    table.CheckConstraint("ck_order_items_discount_fields_together", "num_nonnulls(discount_percentage, discounted_unit_price, reference_price) IN (0, 3)");
                    table.CheckConstraint("ck_order_items_discount_percentage_range", "discount_percentage IS NULL OR (discount_percentage > 0 AND discount_percentage < 100)");
                    table.CheckConstraint("ck_order_items_discounted_unit_price_range", "discounted_unit_price IS NULL OR (discounted_unit_price > 0 AND discounted_unit_price < unit_price)");
                    table.CheckConstraint("ck_order_items_download_allowance_only_digital", "(download_allowance IS NOT NULL) = (product_type = 'Digital')");
                    table.CheckConstraint("ck_order_items_download_count_within_allowance", "download_count >= 0 AND (product_type = 'Digital' OR download_count = 0) AND (download_allowance IS NULL OR download_count <= download_allowance)");
                    table.CheckConstraint("ck_order_items_fulfilment_period_only_service", "(fulfilment_period IS NOT NULL) = (product_type = 'Service')");
                    table.CheckConstraint("ck_order_items_product_type", "\"product_type\" IN ('Physical', 'Digital', 'Service')");
                    table.CheckConstraint("ck_order_items_quantity_above_zero_one_if_digital", "quantity > 0 AND (product_type <> 'Digital' OR quantity = 1)");
                    table.CheckConstraint("ck_order_items_reference_price_above_zero", "reference_price IS NULL OR reference_price > 0");
                    table.CheckConstraint("ck_order_items_second_option_needs_first", "second_option_value IS NULL OR first_option_value IS NOT NULL");
                    table.CheckConstraint("ck_order_items_unit_price_above_zero", "unit_price > 0");
                    table.CheckConstraint("ck_order_items_vat_amount_not_negative", "vat_amount >= 0");
                    table.CheckConstraint("ck_order_items_vat_rate_range", "vat_rate >= 0 AND vat_rate < 100");
                    table.CheckConstraint("ck_order_items_withdrawal_exemption_only_physical", "withdrawal_exemption_reason IS NULL OR product_type = 'Physical'");
                    table.CheckConstraint("ck_order_items_withdrawal_exemption_reason", "\"withdrawal_exemption_reason\" IN ('MadeToOrder', 'Perishable', 'ExpiryDated', 'HygieneSensitive', 'SealedMedia')");
                    table.ForeignKey(
                        name: "fk_order_items_digital_files_digital_file_id",
                        column: x => x.digital_file_id,
                        principalTable: "digital_files",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_order_items_images_image_id",
                        column: x => x.image_id,
                        principalTable: "images",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_order_items_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_order_items_products_product_id",
                        column: x => x.product_id,
                        principalTable: "products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_order_items_variants_variant_id",
                        column: x => x.variant_id,
                        principalTable: "variants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "transition_records",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    order_id = table.Column<Guid>(type: "uuid", nullable: false),
                    transition_code = table.Column<string>(type: "text", nullable: false),
                    actor_kind = table.Column<string>(type: "text", nullable: false),
                    admin_id = table.Column<Guid>(type: "uuid", nullable: true),
                    admin_name = table.Column<string>(type: "text", nullable: true),
                    transition_reason = table.Column<string>(type: "text", nullable: true),
                    cancellation_reason = table.Column<string>(type: "text", nullable: true),
                    correction_reason = table.Column<string>(type: "text", nullable: true),
                    reason_note = table.Column<string>(type: "text", nullable: true),
                    corrected_transition_record_id = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_transition_records", x => x.id);
                    table.CheckConstraint("ck_transition_records_actor_kind", "\"actor_kind\" IN ('Buyer', 'Admin', 'System')");
                    table.CheckConstraint("ck_transition_records_at_most_one_reason", "num_nonnulls(transition_reason, cancellation_reason, correction_reason) <= 1");
                    table.CheckConstraint("ck_transition_records_auto_cancellation_only_system_s3_o2", "transition_reason IS NULL OR transition_reason = 'Closure' OR (actor_kind = 'System' AND transition_code IN ('S3', 'O2'))");
                    table.CheckConstraint("ck_transition_records_cancellation_reason", "\"cancellation_reason\" IN ('OutOfStock', 'DamagedOrUnsellable', 'CannotShipToAddress', 'UndeliveredBuyerCaused', 'AgreedWithBuyer', 'Other')");
                    table.CheckConstraint("ck_transition_records_cancellation_reason_by_admin", "cancellation_reason IS NULL OR actor_kind = 'Admin'");
                    table.CheckConstraint("ck_transition_records_closure_only_s3_s4_s9", "transition_reason IS DISTINCT FROM 'Closure' OR transition_code IN ('S3', 'S4', 'S9')");
                    table.CheckConstraint("ck_transition_records_correction_fields_only_on_correction", "(transition_code = 'Correction') = (correction_reason IS NOT NULL) AND (transition_code = 'Correction') = (corrected_transition_record_id IS NOT NULL) AND (transition_code <> 'Correction' OR actor_kind = 'Admin')");
                    table.CheckConstraint("ck_transition_records_correction_reason", "\"correction_reason\" IN ('WrongOrder', 'MarkedBeforeOccurred', 'Other')");
                    table.CheckConstraint("ck_transition_records_reason_only_on_cancellation_codes", "num_nonnulls(transition_reason, cancellation_reason) = 0 OR transition_code IN ('S3', 'S4', 'S9', 'O2')");
                    table.CheckConstraint("ck_transition_records_transition_code", "\"transition_code\" IN ('S1', 'S2', 'S3', 'S4', 'S5', 'S6', 'S7', 'S8', 'S9', 'S10', 'S11', 'O1', 'O2', 'O3', 'O4', 'O5', 'Correction')");
                    table.CheckConstraint("ck_transition_records_transition_reason", "\"transition_reason\" IN ('PaymentWindowExpired', 'CardPaymentFailed', 'PaymentPageNotOpened', 'SameCartReorder', 'Closure')");
                    table.ForeignKey(
                        name: "fk_transition_records_admins_admin_id",
                        column: x => x.admin_id,
                        principalTable: "admins",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "fk_transition_records_orders_order_id",
                        column: x => x.order_id,
                        principalTable: "orders",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_transition_records_transition_records_corrected_transition_",
                        column: x => x.corrected_transition_record_id,
                        principalTable: "transition_records",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "checkbox_acceptance_order_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    checkbox_acceptance_id = table.Column<Guid>(type: "uuid", nullable: false),
                    order_item_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_checkbox_acceptance_order_items", x => x.id);
                    table.ForeignKey(
                        name: "fk_checkbox_acceptance_order_items_checkbox_acceptances_checkb",
                        column: x => x.checkbox_acceptance_id,
                        principalTable: "checkbox_acceptances",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "fk_checkbox_acceptance_order_items_order_items_order_item_id",
                        column: x => x.order_item_id,
                        principalTable: "order_items",
                        principalColumn: "id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id",
                table: "cart_items",
                column: "cart_id");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_cart_id_variant_id",
                table: "cart_items",
                columns: new[] { "cart_id", "variant_id" },
                unique: true,
                filter: "removed_product_name IS NULL");

            migrationBuilder.CreateIndex(
                name: "ix_cart_items_variant_id",
                table: "cart_items",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_carts_customer_id",
                table: "carts",
                column: "customer_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carts_guest_cart_key_hash",
                table: "carts",
                column: "guest_cart_key_hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_carts_last_accessed_at",
                table: "carts",
                column: "last_accessed_at",
                filter: "guest_cart_key_hash IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "ix_checkbox_acceptance_order_items_checkbox_acceptance_id_orde",
                table: "checkbox_acceptance_order_items",
                columns: new[] { "checkbox_acceptance_id", "order_item_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_checkbox_acceptance_order_items_order_item_id",
                table: "checkbox_acceptance_order_items",
                column: "order_item_id");

            migrationBuilder.CreateIndex(
                name: "ix_checkbox_acceptances_order_id_checkbox_type",
                table: "checkbox_acceptances",
                columns: new[] { "order_id", "checkbox_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_internal_notes_admin_id",
                table: "internal_notes",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_internal_notes_order_id",
                table: "internal_notes",
                column: "order_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_order_items_digital_file_id",
                table: "order_items",
                column: "digital_file_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_image_id",
                table: "order_items",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_order_id",
                table: "order_items",
                column: "order_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_product_id",
                table: "order_items",
                column: "product_id");

            migrationBuilder.CreateIndex(
                name: "ix_order_items_variant_id",
                table: "order_items",
                column: "variant_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_billing_province_id",
                table: "orders",
                column: "billing_province_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_cart_id",
                table: "orders",
                column: "cart_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_company_province_id",
                table: "orders",
                column: "company_province_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_confirmation_key",
                table: "orders",
                column: "confirmation_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_coupon_id",
                table: "orders",
                column: "coupon_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_customer_id",
                table: "orders",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_distance_sales_contract_version_id",
                table: "orders",
                column: "distance_sales_contract_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_ip_address_created_at",
                table: "orders",
                columns: new[] { "ip_address", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_normalized_contact_email",
                table: "orders",
                column: "normalized_contact_email");

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_access_key",
                table: "orders",
                column: "order_access_key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_number",
                table: "orders",
                column: "order_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_orders_order_status_payment_status_created_at",
                table: "orders",
                columns: new[] { "order_status", "payment_status", "created_at" });

            migrationBuilder.CreateIndex(
                name: "ix_orders_payment_reminder_at",
                table: "orders",
                column: "payment_reminder_at",
                filter: "payment_status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_orders_payment_window_ends_at",
                table: "orders",
                column: "payment_window_ends_at",
                filter: "payment_status = 'Pending'");

            migrationBuilder.CreateIndex(
                name: "ix_orders_pre_information_form_version_id",
                table: "orders",
                column: "pre_information_form_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_privacy_notice_version_id",
                table: "orders",
                column: "privacy_notice_version_id");

            migrationBuilder.CreateIndex(
                name: "ix_orders_shipping_province_id",
                table: "orders",
                column: "shipping_province_id");

            migrationBuilder.CreateIndex(
                name: "ix_transition_records_admin_id",
                table: "transition_records",
                column: "admin_id");

            migrationBuilder.CreateIndex(
                name: "ix_transition_records_corrected_transition_record_id",
                table: "transition_records",
                column: "corrected_transition_record_id");

            migrationBuilder.CreateIndex(
                name: "ix_transition_records_order_id_created_at",
                table: "transition_records",
                columns: new[] { "order_id", "created_at" });
        }
    }
}
