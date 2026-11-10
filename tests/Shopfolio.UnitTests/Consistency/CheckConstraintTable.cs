namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The table-specific check constraints of the model as the code lists them (T120; <c>06 7.3.2</c>): per table, the number of items the document's row
/// states (items are separated by a semicolon outside parentheses) and the names of the model's constraints those items stand for. The ones
/// every table carries by rule are not listed: enum value sets, the ten-digit phone and the page address format (<c>06 7.3.1</c>, <c>7.3.2</c>).
/// The equality check compares this list with the document's rows and with the model, in both directions.
/// </summary>
internal static class CheckConstraintTable
{
    public static readonly IReadOnlyDictionary<string, (int DocumentedItems, string[] Constraints)> Specific = new Dictionary<string, (int, string[])>(StringComparer.Ordinal)
    {
        ["AdminInvitation"] = (1, [
            "ck_admin_invitations_at_most_one_outcome",
        ]),
        ["Announcement"] = (1, [
            "ck_announcements_end_not_before_start",
        ]),
        ["AttemptRecord"] = (2, [
            "ck_attempt_records_axis_is_the_limits_axis",
        ]),
        ["Branch"] = (1, [
            "ck_branches_image_alt_text_only_with_image",
        ]),
        ["Branding"] = (1, [
            "ck_brandings_brand_color_format",
        ]),
        ["Cancellation"] = (7, [
            "ck_cancellations_buyer_without_admin",
            "ck_cancellations_coupon_share_within_product",
            "ck_cancellations_product_amount_above_zero",
            "ck_cancellations_quantity_above_zero",
            "ck_cancellations_reason_by_admin",
            "ck_cancellations_reason_note_only_on_other",
            "ck_cancellations_refund_iban_only_by_buyer",
            "ck_cancellations_vat_amount_not_negative",
        ]),
        ["Cart"] = (2, [
            "ck_carts_exactly_one_owner",
            "ck_carts_last_accessed_at_only_for_guest",
        ]),
        ["CartItem"] = (3, [
            "ck_cart_items_first_added_unit_price_above_zero",
            "ck_cart_items_quantity_above_zero",
            "ck_cart_items_removed_name_without_variant",
        ]),
        ["Category"] = (2, [
            "ck_categories_level_fence",
            "ck_categories_level_follows_parent",
        ]),
        ["ChannelStateRecord"] = (2, [
            "ck_channel_state_records_error_count_not_negative",
            "ck_channel_state_records_first_error_at_with_count",
        ]),
        ["CompanySettings"] = (4, [
            "ck_company_settings_amounts_not_negative",
            "ck_company_settings_bank_transfer_iban_required",
            "ck_company_settings_bank_transfer_payment_window_fence",
            "ck_company_settings_default_shipping_period_fence",
            "ck_company_settings_default_vat_rate_range",
            "ck_company_settings_download_allowance_fence",
            "ck_company_settings_return_address_all_or_none",
        ]),
        ["ContactRequest"] = (1, [
            "ck_contact_requests_closed_has_last_closed_at",
        ]),
        ["Country"] = (1, [
            "ck_countries_code_two_capital_letters",
        ]),
        ["Coupon"] = (7, [
            "ck_coupons_amount_above_zero",
            "ck_coupons_amount_needs_larger_minimum_basket",
            "ck_coupons_end_not_before_start",
            "ck_coupons_exactly_one_value",
            "ck_coupons_minimum_basket_amount_not_negative",
            "ck_coupons_percentage_range",
            "ck_coupons_usage_counts_not_negative",
        ]),
        ["Customer"] = (1, [
            "ck_customers_unverified_has_password_and_no_google",
        ]),
        ["DefectClaim"] = (2, [
            "ck_defect_claims_quantity_above_zero",
            "ck_defect_claims_return_address_fields_together",
            "ck_defect_claims_return_phone_only_with_address",
        ]),
        ["DigitalFile"] = (4, [
            "ck_digital_files_at_most_one_owner",
            "ck_digital_files_detached_needs_attached",
            "ck_digital_files_incomplete_upload_unattached",
            "ck_digital_files_size_above_zero",
        ]),
        ["Discount"] = (2, [
            "ck_discounts_end_not_before_start",
            "ck_discounts_percentage_range",
        ]),
        ["DispatchRecord"] = (5, [
            "ck_dispatch_records_attempt_count_not_negative",
            "ck_dispatch_records_next_attempt_only_when_queued",
            "ck_dispatch_records_order_and_related_record_by_type",
            "ck_dispatch_records_related_record_type_with_id",
            "ck_dispatch_records_sent_at_with_state",
        ]),
        ["DisposalRecord"] = (2, [
            "ck_disposal_records_count_and_range_by_type",
            "ck_disposal_records_range_end_not_before_start",
        ]),
        ["Image"] = (1, [
            "ck_images_dimensions_within_limits",
        ]),
        ["InboundNotificationRecord"] = (3, [
            "ck_inbound_notification_records_amount_not_negative",
            "ck_inbound_notification_records_notification_or_transaction",
            "ck_inbound_notification_records_reason_only_when_processed",
        ]),
        ["ItemRemoval"] = (1, [
            "ck_item_removals_coupon_share_within_product",
            "ck_item_removals_product_amount_above_zero",
            "ck_item_removals_quantity_above_zero",
            "ck_item_removals_vat_amount_not_negative",
        ]),
        ["Job"] = (2, [
            "ck_jobs_attempt_count_not_negative",
            "ck_jobs_completed_at_with_state",
            "ck_jobs_related_record_by_type",
        ]),
        ["LateDeliveryTermination"] = (2, [
            "ck_late_delivery_terminations_buyer_without_admin",
            "ck_late_delivery_terminations_coupon_share_within_product",
            "ck_late_delivery_terminations_product_amount_above_zero",
            "ck_late_delivery_terminations_quantity_above_zero",
            "ck_late_delivery_terminations_vat_amount_not_negative",
        ]),
        ["LegalTextVersion"] = (4, [
            "ck_legal_text_versions_draft_only_for_firm_edited_text",
            "ck_legal_text_versions_hosting_location_only_on_privacy_notice",
            "ck_legal_text_versions_number_and_published_at_together",
            "ck_legal_text_versions_template_version_only_generated",
        ]),
        ["LinkRecord"] = (5, [
            "ck_link_records_at_most_one_of_used_and_superseded",
            "ck_link_records_customer_only_types_have_customer",
            "ck_link_records_exactly_one_owner",
            "ck_link_records_invitation_link_only_for_invitation_type",
            "ck_link_records_previous_email_only_for_revert_type",
        ]),
        ["NotReturnedClosure"] = (1, [
            "ck_not_returned_closures_quantity_above_zero",
        ]),
        ["Option"] = (1, [
            "ck_options_sort_order_fence",
        ]),
        ["Order"] = (11, [
            "ck_orders_at_most_one_carrier",
            "ck_orders_card_without_payment_reminder",
            "ck_orders_coupon_and_code_together",
            "ck_orders_deleted_account_without_link",
            "ck_orders_disposal_fields_together",
            "ck_orders_iban_only_on_bank_transfer",
            "ck_orders_own_vehicle_without_carrier",
            "ck_orders_shipping_address_all_or_none",
            "ck_orders_shipping_fee_and_breakdown",
            "ck_orders_shipping_period_not_negative",
            "ck_orders_status_pair_allowed",
            "ck_orders_total_above_zero",
            "ck_orders_tracking_number_with_carrier",
        ]),
        ["OrderAddressHistory"] = (1, [
            "ck_order_address_histories_phones_by_address_type",
        ]),
        ["OrderItem"] = (12, [
            "ck_order_items_coupon_share_not_negative",
            "ck_order_items_delivery_mark_and_date_together",
            "ck_order_items_digital_file_only_digital",
            "ck_order_items_discount_fields_together",
            "ck_order_items_discount_percentage_range",
            "ck_order_items_discounted_unit_price_range",
            "ck_order_items_download_allowance_only_digital",
            "ck_order_items_download_count_within_allowance",
            "ck_order_items_fulfilment_period_only_service",
            "ck_order_items_quantity_above_zero_one_if_digital",
            "ck_order_items_reference_price_above_zero",
            "ck_order_items_second_option_needs_first",
            "ck_order_items_unit_price_above_zero",
            "ck_order_items_vat_amount_not_negative",
            "ck_order_items_vat_rate_range",
            "ck_order_items_withdrawal_exemption_only_physical",
        ]),
        ["OutageRecord"] = (1, [
            "ck_outage_records_longer_than_two_minutes",
        ]),
        ["PaymentRecord"] = (1, [
            "ck_payment_records_transaction_or_transition",
        ]),
        ["PendingEmailChange"] = (1, [
            "ck_pending_email_changes_exactly_one_account",
        ]),
        ["PriceHistory"] = (1, [
            "ck_price_histories_price_above_zero",
        ]),
        ["Product"] = (3, [
            "ck_products_fulfilment_period_fence",
            "ck_products_max_quantity_per_order_fence",
            "ck_products_shipping_period_fence",
            "ck_products_vat_rate_range",
        ]),
        ["Province"] = (1, [
            "ck_provinces_code_two_digits",
        ]),
        ["RecognizedBrowserMark"] = (1, [
            "ck_recognized_browser_marks_exactly_one_account",
        ]),
        ["Refund"] = (8, [
            "ck_refunds_amount_above_zero",
            "ck_refunds_coupon_share_not_negative",
            "ck_refunds_links_by_type",
            "ck_refunds_not_returned_closure_only_amount_based",
            "ck_refunds_not_returned_closure_with_item",
            "ck_refunds_quantity_above_zero",
            "ck_refunds_sent_or_voided",
            "ck_refunds_shipping_fee_only_on_item_records",
            "ck_refunds_system_without_admin",
            "ck_refunds_vat_amount_not_negative",
        ]),
        ["RefundIban"] = (1, [
            "ck_refund_ibans_iban_capitals_and_digits",
        ]),
        ["RefundIbanRequest"] = (4, [
            "ck_refund_iban_requests_fulfilled_or_closed",
            "ck_refund_iban_requests_iban_only_when_fulfilled",
            "ck_refund_iban_requests_itemless_on_refund_failure",
            "ck_refund_iban_requests_refund_by_reason",
        ]),
        ["RefundRecord"] = (3, [
            "ck_refund_records_admin_mark_is_failed",
            "ck_refund_records_late_success_only_when_marked",
            "ck_refund_records_outcome_at_with_outcome",
        ]),
        ["Return"] = (2, [
            "ck_returns_at_most_one_link",
            "ck_returns_quantity_above_zero",
        ]),
        ["ReturnRejection"] = (1, [
            "ck_return_rejections_quantity_above_zero",
        ]),
        ["Session"] = (3, [
            "ck_sessions_admin_without_account_deletion",
            "ck_sessions_exactly_one_account",
            "ck_sessions_reauthentication_fields_together",
        ]),
        ["StockReservation"] = (2, [
            "ck_stock_reservations_exactly_one_of_variant_and_coupon",
            "ck_stock_reservations_quantity_above_zero_one_if_coupon",
        ]),
        ["TransitionRecord"] = (5, [
            "ck_transition_records_at_most_one_reason",
            "ck_transition_records_auto_cancellation_only_system_s3_o2",
            "ck_transition_records_cancellation_reason_by_admin",
            "ck_transition_records_closure_only_s3_s4_s9",
            "ck_transition_records_correction_fields_only_on_correction",
            "ck_transition_records_reason_only_on_cancellation_codes",
        ]),
        ["Variant"] = (5, [
            "ck_variants_net_quantity_above_zero",
            "ck_variants_price_above_zero",
            "ck_variants_reserved_quantity_not_negative",
            "ck_variants_second_value_needs_first",
            "ck_variants_stock_not_below_reserved",
        ]),
        ["Withdrawal"] = (3, [
            "ck_withdrawals_buyer_without_admin",
            "ck_withdrawals_coupon_share_within_product",
            "ck_withdrawals_product_amount_above_zero",
            "ck_withdrawals_quantity_above_zero",
            "ck_withdrawals_return_address_fields_together",
            "ck_withdrawals_return_phone_only_with_address",
            "ck_withdrawals_vat_amount_not_negative",
        ]),
    };
}
