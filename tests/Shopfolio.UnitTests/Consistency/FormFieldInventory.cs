namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// One field row of <c>04 7.1</c> or <c>7.2</c> as the code has it (T547; 04 7 introduction): the row id, where the field is in the code, the requirement word the row starts with, the ids the rule cell points to and
/// the attempt limits the limit cell names. <paramref name="Where"/> is either <c>Namespace.Type.Property</c> - a property of a form input model, found by reflection, whose attributes are the code's validation - or
/// <c>Pages/File.cshtml#anchor</c> - an identifier the view or page file holds (the field of a view that builds its fields, the property of a query model).
/// </summary>
internal sealed record FieldEntry(string Row, string Where, string Requirement, string Rules = "", string Limit = "");

/// <summary>
/// The code side of the form inventory (T547; 04 7.1, 7.2; 09 21.4 item 1): each field row with its place in the code, its requirement word and the ids of its rule. The comparison with the document is
/// <see cref="FormFieldCheck"/>: a row without an entry, an entry without a row, a place that is not in the code, a requirement or an id that differs, an attribute of the property that the row's rule does not name.
/// The rule is never a value (04 7 introduction); a value is read from <c>02 11</c> by the id.
/// </summary>
internal static class FormFieldInventory
{
    public static IReadOnlyList<FieldEntry> Entries { get; } =
    [
        // 7.1.1 E-02, E-03: the filter and the search (query models, not form inputs).
        new("7.1.1.1", "Pages/Storefront/CategoryPage.cs#MinPrice", "İsteğe bağlı", "7.3.2.8"),
        new("7.1.1.2", "Pages/Storefront/CategoryPage.cs#InStock", "Seçim"),
        new("7.1.1.3", "Pages/Storefront/SearchPage.cs#Search", "Zorunlu", "7.3.2.17"),

        // 7.1.2 E-04 and 7.1.4 E-12: the cart's inputs.
        new("7.1.2.1", "Storefront.AddCartItemInput.VariantId", "Zorunlu"),
        new("7.1.2.2", "Storefront.AddCartItemInput.Quantity", "Zorunlu", "7.3.2.10 P-9"),
        new("7.1.4.1", "Storefront.UpdateCartItemInput.Quantity", "Zorunlu", "7.3.2.10"),

        // 7.1.3 E-10: the contact form.
        new("7.1.3.1", "Storefront.SendContactRequestInput.Name", "Zorunlu", "7.3.2.3", "L-4"),
        new("7.1.3.2", "Storefront.SendContactRequestInput.Email", "Zorunlu", "7.3.2.1", "L-4"),
        new("7.1.3.3", "Storefront.SendContactRequestInput.Phone", "İsteğe bağlı", "7.3.2.4", "L-4"),
        new("7.1.3.4", "Storefront.SendContactRequestInput.ContactTopic", "Zorunlu", "7.3.2.16", "L-4"),
        new("7.1.3.5", "Storefront.SendContactRequestInput.Message", "Zorunlu", "7.3.2.15", "L-4"),
        new("7.1.3.6", "Storefront.SendContactRequestInput.Website", "—"),

        // 7.1.5 E-13: the payment step.
        new("7.1.5.1", "Storefront.PlaceOrderInput.ContactEmail", "Zorunlu", "7.3.2.1"),
        new("7.1.5.2", "Storefront.PlaceOrderInput.ShippingRecipientName", "Zorunlu", "7.3.2.5"),
        new("7.1.5.3", "Storefront.PlaceOrderInput.ShippingPhone", "Zorunlu", "7.3.2.4"),
        new("7.1.5.4", "Storefront.PlaceOrderInput.SaveToAddressBook", "İsteğe bağlı"),
        new("7.1.5.5", "Storefront.PlaceOrderInput.IsBillingSameAsShipping", "İşaretli gelir"),
        new("7.1.5.6", "Storefront.PlaceOrderInput.BillingRecipientName", "Zorunlu", "7.3.2.5"),
        new("7.1.5.7", "Storefront.PlaceOrderInput.CouponCode", "İsteğe bağlı", "", "L-6"),
        new("7.1.5.8", "Storefront.PlaceOrderInput.PaymentMethod", "Zorunlu", "", "L-8"),
        new("7.1.5.9", "Storefront.PlaceOrderInput.AcceptPreInformation", "Zorunlu"),
        new("7.1.5.10", "Storefront.PlaceOrderInput.ConfirmationKey", "—", "", "L-7"),

        // 7.1.6 E-15: the order tracking entry.
        new("7.1.6.1", "Orders.FindOrderInput.OrderNumber", "Zorunlu", "", "L-5"),
        new("7.1.6.2", "Orders.FindOrderInput.ContactEmail", "Zorunlu", "7.3.2.1", "L-5"),

        // 7.1.7 E-16, 7.1.8 E-17, 7.1.9 E-18, 7.1.10 E-19: the order's operations.
        new("7.1.7.1", "Orders.SaveRefundIbanInput.Iban", "Zorunlu", "7.3.2.7"),
        new("7.1.8.1", "Orders.CancelOrderItemsInput.Items", "Zorunlu"),
        new("7.1.8.2", "Orders.CancelOrderItemsInputLine.Quantity", "Zorunlu", "7.3.2.10"),
        new("7.1.8.3", "Orders.CancelOrderItemsInput.Iban", "Zorunlu", "7.3.2.7"),
        new("7.1.9.1", "Orders.SubmitWithdrawalInput.Items", "Zorunlu"),
        new("7.1.9.2", "Orders.WithdrawalItemRow.Quantity", "Zorunlu", "7.3.2.10"),
        new("7.1.9.3", "Orders.SubmitWithdrawalInput.Iban", "Zorunlu", "7.3.2.7"),
        new("7.1.10.1", "Orders.SubmitDefectClaimInput.OrderItemId", "Zorunlu"),
        new("7.1.10.2", "Orders.SubmitDefectClaimInput.Quantity", "Zorunlu", "7.3.2.10"),
        new("7.1.10.3", "Orders.SubmitDefectClaimInput.Description", "Zorunlu", "7.3.2.15"),

        // 7.1.11 E-20: the registration.
        new("7.1.11.1", "Storefront.RegisterInput.Name", "Zorunlu", "7.3.2.3", "L-3"),
        new("7.1.11.2", "Storefront.RegisterInput.Email", "Zorunlu", "7.3.2.1", "L-3"),
        new("7.1.11.3", "Storefront.RegisterInput.Password", "Zorunlu", "7.3.2.2", "L-3"),
        new("7.1.11.4", "Storefront.ResendVerificationLinkInput.PendingRegistration", "—", "", "L-9"),

        // 7.1.12 E-22, 7.1.13 E-23, 7.1.14 E-24: the sign-in and the password's screens (views that build their fields).
        new("7.1.12.1", "Pages/Storefront/SignIn.cshtml#Fields.Email(\"Input.Email\" + Panel.SignInInput.Email", "Zorunlu", "7.3.2.1", "L-1"),
        new("7.1.12.2", "Pages/Storefront/SignIn.cshtml#Fields.Concealed(\"Input.Password\" + Panel.SignInInput.Password", "Zorunlu", "", "L-1"),
        new("7.1.12.3", "Pages/Storefront/SignIn.cshtml#Input.IsLongLived + Panel.SignInInput.IsLongLived", "İsteğe bağlı", "Z-4 Z-5"),
        new("7.1.12.4", "Pages/Storefront/SignIn.cshtml.cs#OnPostResendVerificationLinkAsync", "—", "7.1.11.4", "L-9"),
        new("7.1.13.1", "Pages/Storefront/PasswordReset.cshtml#Fields.Email(\"Input.Email\" + Panel.RequestPasswordResetInput.Email", "Zorunlu", "7.3.2.1", "L-2"),
        new("7.1.13.2", "Pages/Storefront/PasswordReset.cshtml#Fields.Concealed(\"Input.NewPassword\" + Panel.ResetPasswordInput.NewPassword", "Zorunlu", "7.3.2.2"),
        new("7.1.14.1", "Pages/Account/Reauthentication.cshtml#Fields.Concealed(\"Input.Password\" + Panel.ReauthenticateInput.Password", "Zorunlu", "", "L-1"),

        // 7.1.15 E-25: the account.
        new("7.1.15.1", "Account.ChangeNameInput.Name", "Zorunlu", "7.3.2.3"),
        new("7.1.15.2", "Account.ChangeEmailInput.NewEmail", "Zorunlu", "7.3.2.1", "L-9"),
        new("7.1.15.3", "Account.ChangePasswordInput.NewPassword", "Zorunlu", "7.3.2.2"),
        new("7.1.15.4", "Pages/Account/Profile.cshtml.cs#OnPostResendEmailChangeLinkAsync", "—", "7.1.11.4", "L-9"),

        // 7.1.16 E-26: the address book.
        new("7.1.16.1", "Account.CreateAddressBookInput.Name", "Zorunlu"),
        new("7.1.16.2", "Account.CreateAddressBookInput.RecipientName", "Zorunlu", "7.3.2.5"),
        new("7.1.16.3", "Account.CreateAddressBookInput.Phone", "İsteğe bağlı", "7.3.2.4"),

        // 7.2.1 E-29 and 7.2.2 E-30: the panel's sign-in, reset and invitation.
        new("7.2.1.1", "Panel.SignInInput.Email", "Zorunlu", "7.3.2.1", "L-1"),
        new("7.2.1.2", "Panel.SignInInput.Password", "Zorunlu", "", "L-1"),
        new("7.2.1.3", "Panel.SignInInput.IsLongLived", "İsteğe bağlı", "Z-4 Z-5"),
        new("7.2.1.4", "Panel.RequestPasswordResetInput.Email", "Zorunlu", "7.3.2.1", "L-2"),
        new("7.2.1.5", "Panel.ResetPasswordInput.NewPassword", "Zorunlu", "7.3.2.2"),
        new("7.2.2.1", "Pages/Panel/Invite.cshtml#Fields.AsReadOnly(Fields.Email(\"Email\"", "Salt okunur"),
        new("7.2.2.2", "Panel.AcceptAdminInvitationInput.Name", "Zorunlu", "7.3.2.3"),
        new("7.2.2.3", "Panel.AcceptAdminInvitationInput.Password", "Zorunlu", "7.3.2.2"),

        // 7.2.3 E-32, E-36, E-38, E-40, E-52: the lists' search and filters (query models; a row of two fields names both).
        new("7.2.3.1", "Panel.AdminProductQuery.Search + Panel.AdminProductQuery.PublishStatus", "İsteğe bağlı", "7.3.2.17"),
        new("7.2.3.2", "Panel.OrderQuery.Search + Panel.OrderQuery.OrderStatus + Panel.OrderQuery.PaymentStatus + Panel.OrderQuery.PendingWork", "İsteğe bağlı"),
        new("7.2.3.3", "Panel.RequestQuery.RequestType + Panel.RequestQuery.RequestStatus", "İsteğe bağlı"),
        new("7.2.3.4", "Panel.CustomerQuery.Search", "Zorunlu — arama için", "7.3.2.1"),
        new("7.2.3.5", "Panel.AuditLogQuery.StartDate + Panel.AuditLogQuery.EndDate + Panel.AuditLogQuery.AdminId", "İsteğe bağlı", "7.3.2.12"),

        // 7.2.4 E-33: the product form (the product, its variants, options, images and discount).
        new("7.2.4.1", "Panel.SaveProductInput.ProductType", "Zorunlu"),
        new("7.2.4.2", "Panel.SaveProductInput.Name", "Zorunlu", "7.3.2.14 P-27"),
        new("7.2.4.3", "Panel.SaveProductInput.Categories + Panel.SaveProductCategoryRow.IsPrimaryCategory", "Yayın için zorunlu"),
        new("7.2.4.4", "Panel.SaveProductInput.Description", "İsteğe bağlı"),
        new("7.2.4.5", "Panel.SaveProductInput.Images + Panel.SaveProductImageRow.AltText", "İsteğe bağlı", "7.3.2.18 P-22 P-23 P-24"),
        new("7.2.4.6", "Panel.SaveProductInput.Options + Panel.SaveProductOptionRow.Name + Panel.SaveProductOptionValueRow.Value", "İsteğe bağlı", "P-21"),
        new("7.2.4.7", "Panel.SaveProductVariantRow.SKU", "Zorunlu"),
        new("7.2.4.8", "Panel.SaveProductVariantRow.Price", "Zorunlu", "7.3.2.8"),
        new("7.2.4.9", "Panel.SaveProductVariantRow.StockQuantity + Panel.SaveProductVariantRow.OriginalStockQuantity", "Zorunlu"),
        new("7.2.4.10", "Panel.SaveProductVariantRow.ImageId + Panel.SaveProductVariantRow.DigitalFileId + Panel.SaveProductInput.DigitalFileId", "İsteğe bağlı", "7.3.2.18 P-26"),
        new("7.2.4.11", "Panel.SaveProductInput.VatRate", "Zorunlu", "7.3.2.9 P-10"),
        new("7.2.4.12", "Panel.SaveProductInput.OriginCountryId", "Zorunlu"),
        new("7.2.4.13", "Panel.SaveProductInput.MeasurementUnit + Panel.SaveProductVariantRow.NetQuantity", "İsteğe bağlı", "8.2.3 K-1013"),
        new("7.2.4.14", "Panel.SaveProductInput.ShippingPeriod", "İsteğe bağlı", "7.3.2.11 P-5 P-6"),
        new("7.2.4.15", "Panel.SaveProductInput.FulfilmentPeriod", "Yayın için zorunlu", "7.3.2.11 P-44"),
        new("7.2.4.16", "Panel.SaveProductInput.MaxQuantityPerOrder", "İsteğe bağlı", "P-9"),
        new("7.2.4.17", "Panel.SaveProductInput.WithdrawalExemptionReason", "İsteğe bağlı"),
        new("7.2.4.18", "Panel.SaveProductDiscountRow.Percentage + Panel.SaveProductDiscountRow.StartDate + Panel.SaveProductDiscountRow.EndDate", "İsteğe bağlı", "7.3.2.9 7.3.2.12"),

        // 7.2.5 E-34, 7.2.6 E-35: the category and the coupon.
        new("7.2.5.1", "Panel.CreateCategoryInput.Name + Panel.RenameCategoryInput.Name", "Zorunlu", "7.3.2.14 P-27"),
        new("7.2.5.2", "Panel.MoveCategoryInput.ParentCategoryId", "Seçim", "P-20"),
        new("7.2.6.1", "Panel.SaveCouponInput.Code", "Zorunlu", "7.3.2.19"),
        new("7.2.6.2", "Panel.SaveCouponInput.CouponType", "Zorunlu", "7.3.2.16"),
        new("7.2.6.3", "Panel.SaveCouponInput.Percentage", "Zorunlu", "7.3.2.8 7.3.2.9"),
        new("7.2.6.4", "Panel.SaveCouponInput.MinimumBasketAmount", "Zorunlu", "7.3.2.8"),
        new("7.2.6.5", "Panel.SaveCouponInput.StartDate", "Zorunlu", "7.3.2.12"),
        new("7.2.6.6", "Panel.SaveCouponInput.TotalUsageLimit", "Zorunlu", "7.3.2.10"),

        // 7.2.7 - 7.2.14 E-37: the order's operations (the inputs of the order detail's partial pages; the handler is the validation of a field without an attribute).
        new("7.2.7.1", "Panel.MarkShippedInput.Carrier + Panel.MarkShippedInput.OtherCarrierName", "Zorunlu"),
        new("7.2.7.2", "Panel.MarkShippedInput.TrackingNumber", "Zorunlu"),
        new("7.2.7.3", "Panel.MarkShippedInput.IsOwnVehicleDelivery", "Seçim", "7.2.7.1 7.2.7.2"),
        new("7.2.8.1", "Panel.MarkDeliveredInput.DeliveryDate + Panel.CorrectDeliveryDateInput.DeliveryDate", "Zorunlu", "7.3.2.11"),
        new("7.2.8.2", "Panel.MarkDeliveredInput.IsDeliveredBeforeDeclaration + Panel.CorrectDeliveryDateInput.IsDeliveredBeforeDeclaration", "Zorunlu"),
        new("7.2.8.3", "Panel.ReceiveReturnInput.ArrivalDate + Panel.CorrectArrivalDateInput.ArrivalDate", "Zorunlu", "7.3.2.11"),
        new("7.2.9.1", "Panel.CancelOrderByCompanyInput.Items", "Zorunlu"),
        new("7.2.9.2", "Panel.CancelOrderByCompanyInputLine.Quantity + Pages/Panel/OrderDetail.ItemSteps.cs#TryReadUnits(input.Items", "Zorunlu", "7.3.2.10"),
        new("7.2.9.3", "Panel.CancelOrderByCompanyInput.CancellationReason", "Zorunlu"),
        new("7.2.9.4", "Panel.CancelOrderByCompanyInput.ReasonNote", "Zorunlu"),
        new("7.2.10.1", "Panel.RecordWithdrawalInput.DeclarationDate + Panel.RecordLateDeliveryTerminationInput.DeclarationDate", "Zorunlu"),
        new("7.2.10.2", "Panel.RecordWithdrawalInput.Items + Panel.RecordWithdrawalInputLine.Quantity + Pages/Panel/OrderDetail.ItemSteps.cs#TryReadUnits(input.Items", "Zorunlu", "7.3.2.10"),
        new("7.2.10.3", "Panel.RecordWithdrawalInput.DeclarationDate + Panel.RecordLateDeliveryTerminationInput.DeclarationDate", "Zorunlu", "7.3.2.11"),
        new("7.2.10.4", "Panel.RecordWithdrawalInput.IsFromContactEmail + Panel.RecordWithdrawalInput.Iban + Panel.RecordLateDeliveryTerminationInput.IsFromContactEmail + Panel.RecordLateDeliveryTerminationInput.Iban + Pages/Panel/OrderDetail.ItemSteps.cs#NoticeIbanRule.Allows(iban)", "İsteğe bağlı", "7.3.2.7"),
        new("7.2.10.5", "Panel.RecordWithdrawalInput.IsExceptionAcknowledged", "Zorunlu"),
        new("7.2.10.6", "Panel.RecordWithdrawalInput.IsDeliveredBeforeDeclaration", "Zorunlu", "7.2.8.2"),
        new("7.2.11.1", "Panel.RemoveOrderItemInput.Quantity + Pages/Panel/OrderDetail.ItemSteps.cs#TryReadQuantity(input.Quantity", "Zorunlu", "7.3.2.10"),
        new("7.2.11.2", "Panel.CloseNotReturnedInput.Quantity + Pages/Panel/OrderDetail.ItemSteps.cs#TryReadQuantity(input.Quantity", "Zorunlu", "7.3.2.10"),
        new("7.2.12.1", "Panel.ReceiveReturnInput.ArrivalDate", "Zorunlu", "7.2.8.3"),
        new("7.2.12.2", "Panel.ReceiveReturnInput.Quantity + Pages/Panel/OrderDetail.ItemSteps.cs#TryReadQuantity(input.Quantity", "Zorunlu", "7.3.2.10"),
        new("7.2.12.3", "Panel.ReceiveReturnInput.Confirmed", "Zorunlu"),
        new("7.2.12.4", "Panel.RestockReturnInput.ReturnId", "Seçim"),
        new("7.2.13.1", "Panel.ProcessAmountBasedRefundInput.Amount + Pages/Panel/OrderDetail.Refunds.cs#HasAtMostDecimals(amount", "Zorunlu", "7.3.2.8"),
        new("7.2.13.2", "Panel.ProcessAmountBasedRefundInput.OrderItemId + Panel.ProcessAmountBasedRefundInput.NotReturnedClosureId", "İsteğe bağlı"),
        new("7.2.13.3", "Panel.RequestRefundIbanInput.OrderItemId", "Zorunlu"),
        new("7.2.14.1", "Panel.CorrectAddressInput.ShippingRecipientName + Panel.CorrectAddressInput.ShippingPhone", "Zorunlu", "7.3.2.4 7.3.2.5"),
        new("7.2.14.2", "Panel.CorrectCarrierInput.Carrier + Panel.CorrectCarrierInput.TrackingNumber", "Zorunlu", "7.2.7"),
        new("7.2.14.3", "Panel.CorrectTransitionInput.CorrectionReason + Panel.CorrectTransitionInput.ReasonNote", "Zorunlu"),
        new("7.2.14.4", "Panel.CorrectOrderEmailInput.ContactEmail", "Zorunlu", "7.3.2.1"),

        // 7.2.15 E-37: the internal note.
        new("7.2.15.1", "Panel.SaveInternalNoteInput.Text", "İsteğe bağlı"),

        // 7.2.16 E-41: the content form (every kind of content shares the one input).
        new("7.2.16.1", "Panel.SaveContentInput.Name + Panel.SaveContentInput.Title + Panel.SaveContentInput.Question", "Yayın için zorunlu", "7.3.2.14 P-27 P-28"),
        new("7.2.16.2", "Panel.SaveContentInput.ShortDescription + Panel.SaveContentInput.ShortIntroduction + Panel.SaveContentInput.WorkingHours", "Yayın için zorunlu", "P-28 P-29"),
        new("7.2.16.3", "Panel.SaveContentInput.LongText + Panel.SaveContentInput.Text + Panel.SaveContentInput.Answer", "Yayın için zorunlu"),
        new("7.2.16.4", "Panel.SaveContentInput.Text", "Yayın için zorunlu", "P-30"),
        new("7.2.16.5", "Panel.SaveContentInput.ProvinceId", "Yayın için zorunlu", "7.3.2.6"),
        new("7.2.16.6", "Panel.SaveContentInput.Phone", "İsteğe bağlı", "7.3.2.4"),
        new("7.2.16.7", "Panel.SaveContentInput.LinkUrl + Panel.SaveContentInput.MapUrl", "İsteğe bağlı", "7.3.2.13"),
        new("7.2.16.8", "Panel.SaveContentInput.StartDate + Panel.SaveContentInput.EndDate", "İsteğe bağlı", "7.3.2.12"),
        new("7.2.16.9", "Panel.SaveContentInput.ImageId + Panel.SaveContentInput.ImageAltText + Panel.SaveContentImageRow.AltText", "İsteğe bağlı", "7.3.2.18 P-22 P-23 P-25"),
        new("7.2.16.10", "Panel.SaveContentInput.LinkedProducts + Panel.SaveContentInput.IsShownOnHomepage + Panel.SaveContentInput.IsShownInMenu", "İsteğe bağlı"),

        // 7.2.17 E-42, 7.2.18 E-43, 7.2.19 E-44: the home page and menu, the brand, the firm's identity.
        new("7.2.17.1", "Panel.SaveHomepageInput.HomepageLayout", "Zorunlu"),
        new("7.2.17.2", "Panel.SaveHomepageInput.MenuNameProducts", "Zorunlu"),
        new("7.2.17.3", "Panel.SaveHomepageInput.SocialLinks", "İsteğe bağlı", "7.3.2.13"),
        new("7.2.17.4", "Panel.SaveHomepageInput.WhatsAppNumber", "İsteğe bağlı", "7.3.2.4"),
        new("7.2.18.1", "Panel.SaveBrandingInput.BrandName", "Zorunlu"),
        new("7.2.18.2", "Panel.SaveBrandingInput.LogoId", "İsteğe bağlı", "7.3.2.18 P-22 P-23"),
        new("7.2.18.3", "Panel.SaveBrandingInput.BrandColor", "İsteğe bağlı"),
        new("7.2.18.4", "Panel.SaveBrandingInput.IsPlatformSignatureEnabled", "Seçim"),
        new("7.2.19.1", "Panel.SaveCompanyInput.CompanyType", "Zorunlu"),
        new("7.2.19.2", "Panel.SaveCompanyInput.LegalName", "Zorunlu"),
        new("7.2.19.3", "Panel.SaveCompanyInput.ProvinceId", "Zorunlu", "7.3.2.6"),
        new("7.2.19.4", "Panel.SaveCompanyInput.Phone", "Zorunlu", "7.3.2.4"),
        new("7.2.19.5", "Panel.SaveCompanyInput.Email", "Zorunlu", "7.3.2.1"),
        new("7.2.19.6", "Panel.SaveCompanyInput.ChamberName", "Meslek odası", "7.3.2.13"),
        new("7.2.19.7", "Panel.SaveCompanyInput.EtbisVerificationUrl", "İsteğe bağlı", "7.3.2.13"),

        // 7.2.20 E-45, 7.2.21 E-46, 7.2.22 E-47: the payment methods, the shipping and defaults, the legal texts.
        new("7.2.20.1", "Panel.SavePaymentMethodsInput.IsBankTransferEnabled", "Seçim"),
        new("7.2.20.2", "Panel.SavePaymentMethodsInput.BankTransferIban", "Zorunlu", "7.3.2.7"),
        new("7.2.21.1", "Panel.SaveShippingFeeInput.ShippingFee", "Zorunlu", "7.3.2.8"),
        new("7.2.21.2", "Panel.SaveShippingFeeInput.FreeShippingThreshold", "İsteğe bağlı", "7.3.2.8"),
        new("7.2.21.3", "Panel.SaveDeliveryProvincesInput.ProvinceIds", "Seçim"),
        new("7.2.21.4", "Panel.SavePeriodsInput.DefaultShippingPeriod", "Zorunlu", "7.3.2.11"),
        new("7.2.21.5", "Panel.SaveProductDefaultsInput.DownloadAllowance", "Zorunlu", "P-8"),
        new("7.2.21.6", "Panel.SaveReturnAddressInput.ReturnRecipientName", "Zorunlu", "7.3.2.5"),
        new("7.2.21.7", "Panel.SaveReturnAddressInput.ReturnPhone", "İsteğe bağlı", "7.3.2.4"),
        new("7.2.22.1", "Panel.SaveLegalTextDraftInput.Text", "Zorunlu"),
        new("7.2.22.2", "Panel.SaveLegalTextDraftInput.HostingLocation", "Zorunlu"),

        // 7.2.23 E-49, 7.2.24 E-50, 7.2.25 E-51 and E-53, 7.2.26 E-54: the administrators' accounts, the export and the re-authentication.
        new("7.2.23.1", "Panel.SendAdminInvitationInput.Email", "Zorunlu", "7.3.2.1"),
        new("7.2.24.1", "Panel.ChangeNameInput.Name", "Zorunlu", "7.3.2.3"),
        new("7.2.24.2", "Panel.ChangeEmailInput.NewEmail", "Zorunlu", "7.3.2.1", "L-9"),
        new("7.2.24.3", "Panel.ChangePasswordInput.NewPassword", "Zorunlu", "7.3.2.2"),
        new("7.2.24.4", "Pages/Panel/MyAccount.cshtml.cs#OnPostResendEmailChangeLinkAsync", "—", "7.1.11.4", "L-9"),
        new("7.2.25.1", "Pages/Panel/SalesSummary.cshtml.cs#Period", "Seçim"),
        new("7.2.25.2", "Panel.ExportOrdersInput.StartDate", "Zorunlu", "7.3.2.12"),
        new("7.2.26.1", "Panel.ReauthenticateInput.Password", "Zorunlu", "", "L-1"),
    ];
}
