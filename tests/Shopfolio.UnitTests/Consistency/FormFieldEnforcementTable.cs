namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The enforcement lines of the field rows (T547; see <see cref="FormFieldEnforcement"/>): for a field whose check is not an attribute of its property, the line of the code that makes the check -
/// <c>Kind|file#fragment</c>, the fragment one piece of contiguous code (white space read as one space; a statement over several lines is one piece) that names the field and the error code (or the symbol
/// of the parameter or of the limit), so the line cannot be another field's or another flow's: it is held exactly once in its file and a row has at least one line no row of another form uses. A limit's row
/// has the form's own call that reaches the limit (the sign-in call, the reset call) and may add the shared counting line.
/// </summary>
internal static class FormFieldEnforcementTable
{
    private const string Product = "Pages/Product.cshtml.cs";
    private const string Checkout = "Pages/Storefront/Checkout.cshtml.cs";
    private const string Corrections = "Pages/Panel/OrderDetail.Corrections.cs";
    private const string Item = "Pages/Panel/OrderDetail.ItemSteps.cs";
    private const string Refunds = "Pages/Panel/OrderDetail.Refunds.cs";
    private const string Steps = "Modules/Ordering/PanelItemSteps.cs";
    private const string Inputs = "Modules/CorporateContent/ContentInputs.cs";
    private const string Gate = "Modules/CorporateContent/ContentRecords.cs";
    private const string Saves = "Modules/Catalog/ProductSaves.cs";
    private const string Books = "Modules/Membership/AddressBooks.cs";

    /// <summary>The lines of the rows that have one place or whose places carry the rule as attributes (a row with several property places that carry no attribute is in <see cref="FormFieldPlaceEnforcement"/>).</summary>
    private static IReadOnlyDictionary<string, string[]> OwnRowLines { get; } = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["7.1.1.1"] = [$"Amount|Pages/Storefront/CategoryPage.cs#ListFilters.ReadPrice(MinPrice, MaxPrice)"],
        ["7.1.2.1"] = [$"Required|{Product}#nameof(AddCartItemInput.VariantId), FieldCodes.FieldRequired"],
        ["7.1.3.1"] = ["Required|Modules/CorporateContent/ContactFormSending.cs#Require(fields, ContactFormFields.Name, name)", "L-4|Modules/CorporateContent/ContactFormSending.cs#_attempts.RecordAsync(AttemptLimit.L4, byIp, ct)"],
        ["7.1.3.2"] = ["Required|Modules/CorporateContent/ContactFormSending.cs#ContactFormFields.Email, ErrorCode.FieldRequired", "Email|Modules/CorporateContent/ContactFormSending.cs#ContactFormFields.Email, ErrorCode.InvalidEmail", "L-4|Modules/CorporateContent/ContactFormSending.cs#_attempts.RecordAsync(AttemptLimit.L4, byIp, ct)"],
        ["7.1.3.3"] = ["Phone|Modules/CorporateContent/ContactFormSending.cs#ContactFormFields.Phone, ErrorCode.InvalidPhone", "L-4|Modules/CorporateContent/ContactFormSending.cs#_attempts.RecordAsync(AttemptLimit.L4, byIp, ct)"],
        ["7.1.3.4"] = ["Required|Modules/CorporateContent/ContactFormSending.cs#ContactFormFields.ContactTopic, ErrorCode.FieldRequired", "List|Modules/CorporateContent/ContactFormSending.cs#ContactFormFields.ContactTopic, ErrorCode.InvalidListValue", "L-4|Modules/CorporateContent/ContactFormSending.cs#_attempts.RecordAsync(AttemptLimit.L4, byIp, ct)"],
        ["7.1.5.1"] = ["Required|Pages/Storefront/Checkout.cshtml.cs#nameof(PlaceOrderInput.ContactEmail), FieldCodes.FieldRequired", "Email|Pages/Storefront/Checkout.cshtml.cs#{ ModelState.AddFieldError(nameof(PlaceOrderInput.ContactEmail), FieldCodes.InvalidEmail);"],
        ["7.1.6.1"] = ["L-5|Pages/Orders/Tracking.cshtml.cs#counter.RecordAsync(AttemptLimit.L5, byEmail, ct)"],
        ["7.1.6.2"] = ["L-5|Pages/Orders/Tracking.cshtml.cs#counter.RecordAsync(AttemptLimit.L5, byEmail, ct)"],
        ["7.1.11.1"] = ["L-3|Modules/Membership/MembershipModule.cs#_attempts.RecordAsync(AttemptLimit.L3, byEmail, ct)"],
        ["7.1.11.2"] = ["L-3|Modules/Membership/MembershipModule.cs#_attempts.RecordAsync(AttemptLimit.L3, byEmail, ct)"],
        ["7.1.11.3"] = ["L-3|Modules/Membership/MembershipModule.cs#_attempts.RecordAsync(AttemptLimit.L3, byEmail, ct)"],
        ["7.1.11.4"] = ["L-9|Pages/Storefront/Register.cshtml.cs#await PendingRegistrations.ResendAsync(_membership, _notifications, _transactions, customerId, ClientAddress.Of(HttpContext), ct);", "L-9|Modules/Membership/MembershipModule.cs#_attempts.RecordAsync(AttemptLimit.L9, byEmail, ct)"],
        ["7.1.12.1"] = ["L-1|Modules/Membership/MembershipModule.cs#_passwords.CheckPasswordAsync(_accounts, email, entered, ipAddress, recognizedBrowserMark, ct)", "L-1|Infrastructure/Security/PasswordRegime.cs#_attempts.RecordAsync(AttemptLimit.L1, subject, ct)"],
        ["7.1.12.2"] = ["L-1|Modules/Membership/MembershipModule.cs#_passwords.CheckPasswordAsync(_accounts, email, entered, ipAddress, recognizedBrowserMark, ct)", "L-1|Infrastructure/Security/PasswordRegime.cs#_attempts.RecordAsync(AttemptLimit.L1, subject, ct)"],
        ["7.1.12.3"] = ["Z-4|Pages/Storefront/SignIn.cshtml.cs#_sessions.SignInAsync(customerId, input.IsLongLived, presentedMark, token)", "Z-5|Pages/Storefront/SignIn.cshtml.cs#_sessions.SignInAsync(customerId, input.IsLongLived, presentedMark, token)", "Z-4|Infrastructure/Security/SessionAuthenticationHandler.cs#ProductConstants.ShortSessionHours", "Z-5|Infrastructure/Security/SessionAuthenticationHandler.cs#ProductConstants.LongSessionDays"],
        ["7.1.12.4"] = ["L-9|Pages/Storefront/SignIn.cshtml.cs#await PendingRegistrations.ResendAsync(_membership, _notifications, _transactions, customerId, ClientAddress.Of(HttpContext), ct);", "L-9|Modules/Membership/MembershipModule.cs#_attempts.RecordAsync(AttemptLimit.L9, byEmail, ct)"],
        ["7.1.13.1"] = ["L-2|Modules/Membership/MembershipModule.cs#_passwords.RequestResetAsync(_accounts, email, ipAddress, ct)", "L-2|Infrastructure/Security/PasswordRegime.cs#_attempts.RecordAsync(AttemptLimit.L2, byEmail, ct)"],
        ["7.1.14.1"] = ["L-1|Modules/Membership/MembershipModule.cs#_passwords.ReauthenticateAsync(_accounts, customerId, sessionId, operation, password, ipAddress, recognizedBrowserMark, ct)", "L-1|Infrastructure/Security/PasswordRegime.cs#_attempts.RecordAsync(AttemptLimit.L1, subject, ct)"],
        ["7.1.15.2"] = ["L-9|Modules/Membership/EmailChanges.cs#replaces && await EmailChangeRules.CountedOverLimitAsync(_attempts, account.NormalizedEmail, ipAddress, ct)", "L-9|Infrastructure/Security/EmailChangeRules.cs#attempts.RecordAsync(AttemptLimit.L9, byEmail, ct)"],
        ["7.1.15.4"] = ["L-9|Modules/Membership/EmailChanges.cs#await EmailChangeRules.CountedOverLimitAsync(_attempts, normalizedEmail, ipAddress, ct)", "L-9|Infrastructure/Security/EmailChangeRules.cs#attempts.RecordAsync(AttemptLimit.L9, byEmail, ct)"],
        ["7.2.1.1"] = ["L-1|Modules/Administration/AdministrationModule.cs#_passwords.CheckPasswordAsync(_accounts, email, password, ipAddress, recognizedBrowserMark, ct)", "L-1|Infrastructure/Security/PasswordRegime.cs#_attempts.RecordAsync(AttemptLimit.L1, subject, ct)"],
        ["7.2.1.2"] = ["L-1|Modules/Administration/AdministrationModule.cs#_passwords.CheckPasswordAsync(_accounts, email, password, ipAddress, recognizedBrowserMark, ct)", "L-1|Infrastructure/Security/PasswordRegime.cs#_attempts.RecordAsync(AttemptLimit.L1, subject, ct)"],
        ["7.2.1.3"] = ["Z-4|Pages/Panel/SignIn.cshtml.cs#_administration.SignInAsync(adminId, input.IsLongLived, presentedMark, token)", "Z-5|Pages/Panel/SignIn.cshtml.cs#_administration.SignInAsync(adminId, input.IsLongLived, presentedMark, token)", "Z-4|Infrastructure/Security/SessionAuthenticationHandler.cs#ProductConstants.ShortSessionHours", "Z-5|Infrastructure/Security/SessionAuthenticationHandler.cs#ProductConstants.LongSessionDays"],
        ["7.2.1.4"] = ["L-2|Modules/Administration/AdministrationModule.cs#_passwords.RequestResetAsync(_accounts, email, ipAddress, ct)", "L-2|Infrastructure/Security/PasswordRegime.cs#_attempts.RecordAsync(AttemptLimit.L2, byEmail, ct)"],
        ["7.2.24.2"] = ["L-9|Modules/Administration/AdminEmailChanges.cs#replaces && await EmailChangeRules.CountedOverLimitAsync(_attempts, ownNormalized, ipAddress, ct)", "L-9|Infrastructure/Security/EmailChangeRules.cs#attempts.RecordAsync(AttemptLimit.L9, byEmail, ct)"],
        ["7.2.24.4"] = ["L-9|Modules/Administration/AdminEmailChanges.cs#await EmailChangeRules.CountedOverLimitAsync(_attempts, normalizedEmail, ipAddress, ct)", "L-9|Infrastructure/Security/EmailChangeRules.cs#attempts.RecordAsync(AttemptLimit.L9, byEmail, ct)"],
        ["7.2.26.1"] = ["L-1|Modules/Administration/AdministrationModule.cs#_passwords.ReauthenticateAsync(_accounts, adminId, sessionId, operation, password, ipAddress, recognizedBrowserMark, ct)", "L-1|Infrastructure/Security/PasswordRegime.cs#_attempts.RecordAsync(AttemptLimit.L1, subject, ct)"],
        // Pass 2: the checkout, the order's customer screens, the address book.
        ["7.1.5.2"] = ["Required|Modules/Membership/AddressBooks.cs#AddressFieldRules.CheckAddress( AddressFieldRules.ShippingPrefix, request.ShippingRecipientName,", "Required|Modules/Membership/AddressFieldRules.cs#RequiredText(prefix + nameof(AddressBookInput.RecipientName), recipientName, errors)"],
        ["7.1.5.3"] = [$"Required|{Books}#AddressFieldRules.TryStorePhone(nameof(OrderAddressRequest.ShippingPhone), phoneText, true, errors", $"Phone|{Books}#AddressFieldRules.TryStorePhone(nameof(OrderAddressRequest.ShippingPhone), phoneText, true, errors"],
        ["7.1.5.6"] = ["Required|Modules/Membership/AddressBooks.cs#AddressFieldRules.CheckAddress( AddressFieldRules.BillingPrefix, request.BillingRecipientName,", "Required|Modules/Membership/AddressFieldRules.cs#RequiredText(prefix + nameof(AddressBookInput.RecipientName), recipientName, errors)"],
        ["7.1.5.7"] = ["L-6|Modules/Catalog/Coupons.cs#_attempts.RecordAsync(AttemptLimit.L6, cart, ct)"],
        ["7.1.5.8"] = [$"Required|{Checkout}#nameof(PlaceOrderInput.PaymentMethod), FieldCodes.FieldRequired", "L-8|Modules/Ordering/OrderPlacementLimits.cs#return AttemptLimit.L8;"],
        ["7.1.5.10"] = ["L-7|Modules/Ordering/OrderPlacementLimits.cs#return AttemptLimit.L7;"],
        ["7.1.7.1"] = ["Required|Pages/Orders/OrderPageModels.cs#nameof(SaveRefundIbanInput.Iban), FieldCodes.FieldRequired"],
        ["7.1.8.1"] = ["Required|Pages/Orders/OrderScreenModels.cs#ModelState.AddModelError(string.Empty, NoItemText)"],
        ["7.1.8.2"] = ["Quantity|Pages/Orders/OrderScreenModels.cs#nameof(CancelOrderItemsInputLine.Quantity)}\", FieldCodes.InvalidQuantity"],
        ["7.1.8.3"] = ["Required|Pages/Orders/OrderScreenModels.cs#RootPath); } if (Screen.AsksIban && string.IsNullOrWhiteSpace(input.Iban)) { ModelState.AddFieldError(IbanFieldName, FieldCodes.FieldRequired);"],
        ["7.1.9.1"] = ["Required|Pages/Orders/AfterSalesScreenModels.cs#nameof(SubmitWithdrawalInput.Items), FieldCodes.FieldRequired"],
        ["7.1.9.2"] = ["Quantity|Pages/Orders/AfterSalesScreenModels.cs#nameof(WithdrawalItemRow.Quantity)}\", FieldCodes.InvalidQuantity"],
        ["7.1.9.3"] = ["Required|Pages/Orders/AfterSalesScreenModels.cs#nameof(SubmitWithdrawalInput.Iban), FieldCodes.FieldRequired"],
        ["7.1.10.1"] = ["Required|Pages/Orders/AfterSalesScreenModels.cs#nameof(SubmitDefectClaimInput.OrderItemId), FieldCodes.FieldRequired"],
        ["7.1.10.2"] = ["Quantity|Pages/Orders/AfterSalesScreenModels.cs#nameof(SubmitDefectClaimInput.Quantity), FieldCodes.InvalidQuantity"],
        ["7.1.16.1"] = [$"Required|{Books}#nameof(AddressBookInput.Name), ErrorCode.FieldRequired"],
        ["7.1.16.2"] = ["Required|Modules/Membership/AddressBooks.cs#AddressFieldRules.CheckAddress( AddressFieldRules.BookPrefix, input.RecipientName,", "Required|Modules/Membership/AddressFieldRules.cs#RequiredText(prefix + nameof(AddressBookInput.RecipientName), recipientName, errors)"],
        ["7.1.16.3"] = [$"Phone|{Books}#AddressFieldRules.TryStorePhone(nameof(AddressBookInput.Phone), input.Phone, false, errors"],
        // Pass 3: the product form, the settings, the corporate content.
        ["7.2.4.1"] = [$"Required|{Saves}#nameof(ProductSaveCommand.ProductType), ErrorCode.FieldRequired"],
        ["7.2.4.2"] = [$"Required|{Saves}#nameof(ProductSaveCommand.Name), ErrorCode.FieldRequired", $"P-27|{Saves}#nameof(ProductSaveCommand.Name), ErrorCode.TextTooLong, ProductConstants.ProductNameMaxLength", $"TextCap|{Saves}#nameof(ProductSaveCommand.Name), ErrorCode.TextTooLong, ProductConstants.ProductNameMaxLength"],
        ["7.2.4.7"] = [$"Required|{Saves}#Field(i, nameof(ProductSaveVariant.SKU)), ErrorCode.FieldRequired"],
        ["7.2.4.8"] = [$"Required|{Saves}#Field(i, nameof(ProductSaveVariant.Price)), ErrorCode.FieldRequired"],
        ["7.2.4.11"] = [$"Required|{Saves}#nameof(ProductSaveCommand.VatRate), ErrorCode.FieldRequired", "P-10|Pages/Panel/ProductForm.cshtml.cs#VatRate = (await _settings.GetProductDefaultsAsync(ct)).DefaultVatRate"],
        ["7.2.4.12"] = [$"Required|{Saves}#nameof(ProductSaveCommand.OriginCountryId), ErrorCode.FieldRequired"],
        ["7.2.4.14"] = ["Period|Modules/Catalog/ProductDefinitionRules.cs#if (!CheckShippingPeriod(shippingPeriod))", "P-5|Modules/Settings/FirmSettings.cs#nameof(PeriodsInput.DefaultShippingPeriod), \"P-5\"", "P-6|Modules/Catalog/ProductDefinitionRules.cs#FirmSettingFences.Check(\"P-6\", days)"],
        ["7.2.4.16"] = [$"P-9|{Saves}#command.MaxQuantityPerOrder is int maxQuantity && !FirmSettingFences.Check(\"P-9\", maxQuantity)"],
        ["7.2.5.1"] = ["P-27|Pages/Panel/Categories.cshtml.cs#public sealed class CreateCategoryInput : IFormInput { [Display(Name = \"Ad\")] [FieldRequired] [TextTooLong(ProductConstants.ProductNameMaxLength)] public string? Name { get; set; }", "P-27|Pages/Panel/Categories.cshtml.cs#public sealed class RenameCategoryInput : IFormInput { public Guid CategoryId { get; set; } [Display(Name = \"Ad\")] [FieldRequired] [TextTooLong(ProductConstants.ProductNameMaxLength)] public string? Name { get; set; }"],
        ["7.2.5.2"] = ["P-20|Modules/Catalog/CategoryLevels.cs#level <= ProductConstants.CategoryLevelMax"],
        ["7.2.6.1"] = ["Required|Modules/Catalog/CouponRules.cs#nameof(CouponDefinition.Code), ErrorCode.FieldRequired", "CouponCode|Modules/Catalog/Coupons.cs#c.NormalizedCode == normalized && c.Id != selfId"],
        ["7.2.6.3"] = ["Required|Pages/Panel/Coupons.cshtml.cs#yield return FieldError.Result(FieldCodes.FieldRequired, nameof(Percentage))", "Amount|Modules/Catalog/CouponRules.cs#nameof(CouponDefinition.Value), ErrorCode.InvalidAmount"],
        ["7.2.6.4"] = ["Required|Pages/Panel/Coupons.cshtml.cs#yield return FieldError.Result(FieldCodes.FieldRequired, nameof(MinimumBasketAmount))"],
        ["7.2.17.1"] = ["Required|Modules/Settings/FirmSettings.cs#nameof(HomepageInput.HomepageLayout), input.HomepageLayout is null ? FieldCodes.FieldRequired"],
        ["7.2.17.2"] = ["Required|Modules/Settings/FirmSettings.cs#string? products = RequiredName(rejections, nameof(HomepageInput.MenuNameProducts), input.MenuNameProducts); string? offerings = RequiredName(rejections, nameof(HomepageInput.MenuNameOfferings), input.MenuNameOfferings); string? portfolio = RequiredName(rejections, nameof(HomepageInput.MenuNamePortfolioItems), input.MenuNamePortfolioItems); string? about = RequiredName(rejections, nameof(HomepageInput.MenuNameAboutPage), input.MenuNameAboutPage); string? faq = RequiredName(rejections, nameof(HomepageInput.MenuNameFaqItems), input.MenuNameFaqItems); string? contact = RequiredName(rejections, nameof(HomepageInput.MenuNameContact), input.MenuNameContact);"],
        ["7.2.17.3"] = ["Url|Modules/CorporateContent/SocialLinks.cs#ContentInputRules.Apply(rejections, name, url, new InvalidUrlAttribute(), FieldCodes.InvalidUrl)"],
        ["7.2.17.4"] = ["Phone|Modules/Settings/FirmSettings.cs#nameof(HomepageInput.WhatsAppNumber), FieldCodes.InvalidPhone"],
        ["7.2.19.7"] = ["Url|Modules/Settings/CompanyIdentityRules.cs#nameof(CompanyIdentityInput.EtbisVerificationUrl), input.EtbisVerificationUrl, new InvalidUrlAttribute(), FieldCodes.InvalidUrl"],
        ["7.2.20.2"] = ["Required|Modules/Settings/PaymentMethodSettings.cs#nameof(PaymentMethodsInput.BankTransferIban), FieldCodes.FieldRequired"],
        ["7.2.21.4"] = ["Required|Modules/Settings/FirmSettings.cs#CheckFenced(rejections, nameof(PeriodsInput.DefaultShippingPeriod)", "Period|Modules/Settings/FirmSettings.cs#CheckFenced(rejections, nameof(PeriodsInput.DefaultShippingPeriod)"],
        ["7.2.21.5"] = ["P-8|Modules/Settings/FirmSettings.cs#nameof(ProductDefaultsInput.DownloadAllowance), \"P-8\""],
        ["7.2.21.7"] = ["Phone|Modules/Settings/CompanySettingsWriter.cs#nameof(ReturnAddressInput.Phone), FieldCodes.InvalidPhone"],
        ["7.2.22.1"] = ["Required|Modules/Settings/LegalTexts.cs#nameof(LegalTextVersion.Text), FieldCodes.FieldRequired"],
        ["7.2.22.2"] = ["Required|Modules/Settings/LegalTexts.cs#nameof(LegalTextVersion.HostingLocation), FieldCodes.FieldRequired"],

        // Pass 4: the order detail's operations.
        ["7.1.3.5"] = ["Required|Modules/CorporateContent/ContactFormSending.cs#Require(fields, ContactFormFields.Message, message)", "L-4|Modules/CorporateContent/ContactFormSending.cs#_attempts.RecordAsync(AttemptLimit.L4, byIp, ct)"],
        ["7.1.5.9"] = ["CheckboxAccepted|Pages/Components/ConsentModels.cs#modelState.AddFieldError(NameOf(kind, namePrefix), FieldCodes.CheckboxNotAccepted)"],
        ["7.2.7.2"] = ["Required|Pages/Panel/OrderDetail.cshtml.cs#ShipmentInfo shipment = new(input.Carrier, input.OtherCarrierName, input.TrackingNumber, input.IsOwnVehicleDelivery); TransitionAdmin admin = await AdminAsync(ct); ShipOrderRequest request = new(id, admin, input.Version, input.Confirmed, shipment); PanelStepResult result = await RunStepAsync( (steps, token) => isResending ? steps.MarkReshippedAsync(request, token) : steps.MarkShippedAsync(request, token), ct); return result.Refusal switch { TransitionRefusal.InvalidInput => await RefuseAsync(id, ShipmentMissingText, ErrorCode.ShipmentInfoMissing, ct)"],
        ["7.2.9.1"] = ["Required|Modules/Ordering/PanelItemSteps.cs#if (request.Items.Count == 0) { return PanelItemStepResult.Refused(PanelItemObstacle.ItemNotSelected);"],
        ["7.2.9.2"] = ["Quantity|Pages/Panel/OrderDetail.ItemSteps.cs#units)) { return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);"],
        ["7.2.9.3"] = ["Required|Pages/Panel/OrderDetail.ItemSteps.cs#string.IsNullOrWhiteSpace(input.CancellationReason) ? ErrorCode.FieldRequired"],
        ["7.2.9.4"] = ["Required|Pages/Components/ReasonModels.cs#new ReasonFieldError(ExplanationMember, FieldCodes.FieldRequired)"],
        ["7.2.10.5"] = [$"Required|{Steps}#warnings.NeedsConfirmation && !request.WarningsConfirmed"],
        ["7.2.10.6"] = ["Required|Modules/Ordering/PanelItemSteps.cs#if (warnings.AsksDeliveryOrder && request.IsDeliveredBeforeDeclaration is null) { return new PanelItemStepResult(TransitionRefusal.InvalidInput) { Obstacle = PanelItemObstacle.DeliveryOrderMissing,"],
        ["7.2.11.1"] = ["Required|Pages/Panel/OrderDetail.ItemSteps.cs#if (!TryReadQuantity(input.Quantity, out int quantity)) { return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);"],
        ["7.2.11.2"] = ["Required|Pages/Panel/OrderDetail.ItemSteps.cs#ErrorCode.FieldRequired, ct); } int? quantity = null; if (!string.IsNullOrWhiteSpace(input.Quantity)) { if (!TryReadQuantity(input.Quantity, out int read)) { return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);"],
        ["7.2.13.1"] = ["Required|Pages/Panel/OrderDetail.Refunds.cs#if (string.IsNullOrWhiteSpace(input.Amount)) { return await RefuseAsync(id, AmountMissingText, ErrorCode.FieldRequired, ct);", "Amount|Pages/Panel/OrderDetail.Refunds.cs#!FieldRuleAttribute.HasAtMostDecimals(amount, AmountDecimals)) { return await RefuseAsync(id, AmountInvalidText, ErrorCode.InvalidAmount, ct);"],
        ["7.2.13.3"] = [$"Required|{Refunds}#PanelRefundObstacle.ItemNotSelected => await RefuseAsync(id, ItemNotSelectedText, ErrorCode.FieldRequired, ct)"],
        ["7.2.14.4"] = [$"Required|{Corrections}#CorrectionValueProblem.Missing => await RefuseCorrectionAsync(id, OrderEmailFormId, EmailMissingText, ErrorCode.FieldRequired, ct)", $"Email|{Corrections}#CorrectionValueProblem.Invalid => await RefuseCorrectionAsync(id, OrderEmailFormId, EmailInvalidText, ErrorCode.InvalidEmail, ct)"],

        // Pass 5: what is left of the order detail, the content, the identity, the lists.
        ["7.1.1.3"] = ["SearchTrim|Modules/Catalog/ProductListReader.cs#string? pattern = SearchTerm.ToLikePattern(search);", "SearchTrim|Infrastructure/SearchTerm.cs#if (word.Length > ProductConstants.ProductNameMaxLength)", "Required|Pages/Search.cshtml.cs#if (list is null) { // No word: no result list"],
        ["7.1.2.2"] = ["Quantity|Pages/Product.cshtml.cs#catalog. ModelState.AddFieldError(nameof(AddCartItemInput.Quantity), FieldCodes.InvalidQuantity", "P-9|Pages/Components/CartRefusalCodes.cs#CartAddOutcome.OverOrderLimit => ErrorCode.MaxQuantityPerOrderExceeded"],
        ["7.2.3.4"] = ["Required|Pages/Panel/Members.cshtml.cs#Record = new MemberRecordView { State = MemberSearchState.NotSearched }"],
        ["7.2.4.15"] = [$"Required|Modules/Catalog/PublishGate.cs#unmet.Add(PublishCondition.FulfilmentPeriod)", $"P-44|{Saves}#command.FulfilmentPeriod is int fulfilmentPeriod && !FirmSettingFences.Check(\"P-44\", fulfilmentPeriod)"],
        ["7.2.12.1"] = [$"Required|{Item}#if (!TryReadDate(input.ArrivalDate, out DateOnly arrival)) {{ return await RefuseAsync(id, ArrivalDateMissingText, string.IsNullOrWhiteSpace(input.ArrivalDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate"],
        ["7.2.12.2"] = ["Required|Pages/Panel/OrderDetail.ItemSteps.cs#ErrorCode.InvalidDate, ct); } int? quantity = null; if (!string.IsNullOrWhiteSpace(input.Quantity)) { if (!TryReadQuantity(input.Quantity, out int read)) { return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);"],
        ["7.2.12.3"] = ["Required|Pages/Panel/OrderDetail.ItemSteps.cs#if (result.Refusal == TransitionRefusal.ConfirmationRequired) { return await AskAsync(id, () => RejectionConfirm(id, input, quantity), ct);"],
        ["7.2.16.4"] = [$"Required|{Gate}#ContentRecordKind.Announcement => [ContentGateField.Text]", $"P-30|{Inputs}#Ceiling(rejections, nameof(AnnouncementInput.Text), Text(input.Text), ContentRules.AnnouncementTextMaxLength)"],
        ["7.2.16.5"] = [$"Required|{Gate}#ContentRecordKind.Branch => [ContentGateField.Name, ContentGateField.Province, ContentGateField.District, ContentGateField.AddressLine]", "Province|Modules/CorporateContent/ContentAuthoring.cs#rejections.Add(new FieldRejection(nameof(BranchInput.ProvinceId), FieldCodes.InvalidProvince))"],
        ["7.2.16.6"] = [$"Phone|{Inputs}#Apply(rejections, nameof(BranchInput.Phone), Phone(input.Phone), new InvalidPhoneAttribute(), FieldCodes.InvalidPhone)"],
        ["7.2.18.1"] = ["Required|Modules/CorporateContent/BrandingSettings.cs#nameof(BrandingInput.BrandName), FieldCodes.FilledFieldCleared"],
        ["7.2.18.2"] = ["Image|Modules/CorporateContent/BrandingSettings.cs#rejections.Add(new BrandingRejection(field, FieldCodes.InvalidImage))", "P-22|Pages/Panel/Brand.cshtml.cs#? ProductFormModel.PixelLimitText : UploadChecks.FormatMessage()", "P-22|Pages/Components/UploadComponentModels.cs#{ProductConstants.ImageFormats} yüklenebilir.", "P-23|Pages/Panel/Brand.cshtml.cs#return UploadChecks.SizeMessage(surface);", "P-23|Modules/Catalog/ImageUploads.cs#content.LongLength > ProductConstants.ImageMaxBytes"],
        ["7.2.19.1"] = ["Required|Modules/Settings/CompanyIdentityRules.cs#else if (input.CompanyType is null && current.CompanyType is not null) { rejections.Add(new FieldRejection(typeField, FieldCodes.FilledFieldCleared));"],
        ["7.2.19.2"] = ["Required|Modules/Settings/Contracts/CompanyIdentityFieldSets.cs#TradesmanFields = [nameof(CompanyIdentityInput.LegalName), nameof(CompanyIdentityInput.TaxIdNumber)];", "Required|Modules/Settings/Contracts/CompanyIdentityFieldSets.cs#MerchantFields = [ nameof(CompanyIdentityInput.LegalName), nameof(CompanyIdentityInput.MersisNumber), nameof(CompanyIdentityInput.TradeRegistryNumber), ];"],
        ["7.2.19.3"] = ["Province|Modules/Settings/CompanyIdentityRules.cs#rejections.Add(new FieldRejection(nameof(CompanyIdentityInput.ProvinceId), FieldCodes.InvalidProvince))", "Required|Modules/Settings/CompanyIdentityRules.cs#CommonFields = [ nameof(CompanyIdentityInput.ProvinceId), nameof(CompanyIdentityInput.District), nameof(CompanyIdentityInput.AddressLine),"],
        ["7.2.19.4"] = ["Phone|Modules/Settings/CompanyIdentityRules.cs#AddFormRejection(rejections, nameof(CompanyIdentityInput.Phone), input.Phone, new InvalidPhoneAttribute(), FieldCodes.InvalidPhone)", "Required|Modules/Settings/CompanyIdentityRules.cs#CommonFields = [ nameof(CompanyIdentityInput.ProvinceId), nameof(CompanyIdentityInput.District), nameof(CompanyIdentityInput.AddressLine), nameof(CompanyIdentityInput.Phone),"],
        ["7.2.19.5"] = ["Email|Modules/Settings/CompanyIdentityRules.cs#AddFormRejection(rejections, nameof(CompanyIdentityInput.Email), input.Email, new InvalidEmailAttribute(), FieldCodes.InvalidEmail)", "Required|Modules/Settings/CompanyIdentityRules.cs#CommonFields = [ nameof(CompanyIdentityInput.ProvinceId), nameof(CompanyIdentityInput.District), nameof(CompanyIdentityInput.AddressLine), nameof(CompanyIdentityInput.Phone), nameof(CompanyIdentityInput.Email), nameof(CompanyIdentityInput.KepAddress),"],
        ["7.2.19.6"] = ["Required|Modules/Settings/CompanyIdentityRules.cs#CommonFields = [ nameof(CompanyIdentityInput.ProvinceId), nameof(CompanyIdentityInput.District), nameof(CompanyIdentityInput.AddressLine), nameof(CompanyIdentityInput.Phone), nameof(CompanyIdentityInput.Email), nameof(CompanyIdentityInput.KepAddress), nameof(CompanyIdentityInput.ChamberName), nameof(CompanyIdentityInput.ProfessionalConductRules), ];", "Url|Modules/Settings/CompanyIdentityRules.cs#nameof(CompanyIdentityInput.ProfessionalConductRules), input.ProfessionalConductRules, new InvalidUrlAttribute(), FieldCodes.InvalidUrl"],
        ["7.2.21.6"] = ["Required|Modules/Settings/CompanySettingsWriter.cs#string missingCode = current.IsComplete ? FieldCodes.FilledFieldCleared : FieldCodes.FieldRequired; string[] names = [ nameof(ReturnAddressInput.RecipientName),"],
        ["7.2.25.2"] = ["Required|Pages/Panel/DataExport.cshtml.cs#DateFields.CheckRange(input.StartDate, input.EndDate, Today, DateRangePolicy.NoFuture, startRequired: true, endRequired: true)", "DateRange|Pages/Panel/DataExport.cshtml.cs#DateFields.CheckRange(input.StartDate, input.EndDate, Today, DateRangePolicy.NoFuture, startRequired: true, endRequired: true)"],
    };

    /// <summary>
    /// The checks the code makes with a field and its error code in one expression and that the lines above did not hold (D-1656; <see cref="FormFieldCodeCalls"/> finds them in the sources): each is the check of
    /// the row's field in the module or the page that makes it, so a change of the check reports the row. A check that is no check of a row of <c>04 7</c> is in <see cref="FormFieldCodeInventory"/> with the reason.
    /// </summary>
    private static IReadOnlyDictionary<string, string[]> CodeCheckLines { get; } = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["7.1.4.1"] = ["Quantity|Pages/Storefront/Cart.cshtml.cs#ModelState.AddFieldError(nameof(UpdateCartItemInput.Quantity), FieldCodes.InvalidQuantity, DisplayFormat.Number(1))"],
        ["7.1.7.1"] = ["Iban|Pages/Orders/OrderPageModels.cs#ModelState.AddFieldError(nameof(SaveRefundIbanInput.Iban), FieldCodes.InvalidIban)"],
        ["7.1.8.3"] = ["Iban|Pages/Orders/OrderScreenModels.cs#ModelState.AddFieldError(IbanFieldName, string.IsNullOrWhiteSpace(Sent.TryGetValue(IbanFieldName, out string? sentIban) ? sentIban : null) ? FieldCodes.FieldRequired : FieldCodes.InvalidIban"],
        ["7.1.11.2"] = ["Required|Modules/Membership/MembershipModule.cs#fields.Add(new MembershipFieldError(nameof(RegisterInput.Email), ErrorCode.FieldRequired))", "Email|Modules/Membership/MembershipModule.cs#fields.Add(new MembershipFieldError(nameof(RegisterInput.Email), ErrorCode.InvalidEmail))"],
        ["7.1.16.2"] = ["Required|Modules/Membership/AddressFieldRules.cs#errors.Add(new MembershipFieldError(prefix + nameof(AddressBookInput.ProvinceId), ErrorCode.FieldRequired))", "Province|Modules/Membership/AddressFieldRules.cs#errors.Add(new MembershipFieldError(prefix + nameof(AddressBookInput.ProvinceId), ErrorCode.InvalidProvince))"],
        ["7.2.4.18"] = ["DateRange|Modules/Catalog/ProductSaves.cs#\"EndBeforeStart\" => new ProductSaveFieldError(\"Discount.EndDate\", ErrorCode.InvalidDateRange)"],
        ["7.2.6.3"] = ["Percentage|Modules/Catalog/CouponRules.cs#problems.Add(new CouponProblem(nameof(CouponDefinition.Value), ErrorCode.InvalidPercentage))", "Required|Pages/Panel/Coupons.cshtml.cs#yield return FieldError.Result(FieldCodes.FieldRequired, nameof(Amount))"],
        ["7.2.6.4"] = ["Amount|Modules/Catalog/CouponRules.cs#if (definition.MinimumBasketAmount is { } minimum && !AmountField.Allows(minimum)) { problems.Add(new CouponProblem(nameof(CouponDefinition.MinimumBasketAmount), ErrorCode.InvalidAmount)); }", "Amount|Modules/Catalog/CouponRules.cs#if (minimum is { } floor && !AmountField.Allows(floor)) { problems.Add(new CouponProblem(nameof(CouponDefinition.MinimumBasketAmount), ErrorCode.InvalidAmount)); }"],
        ["7.2.6.5"] = ["DateRange|Modules/Catalog/CouponRules.cs#if (definition.EndDate < definition.StartDate) { problems.Add(new CouponProblem(nameof(CouponDefinition.EndDate), ErrorCode.InvalidDateRange)); }"],
        ["7.2.21.5"] = ["Required|Modules/Settings/FirmSettings.cs#nameof(ProductDefaultsInput.DefaultVatRate), FieldCodes.FieldRequired"],
        ["7.2.21.6"] = ["Province|Modules/Settings/CompanySettingsWriter.cs#rejections.Add(new FieldRejection(nameof(ReturnAddressInput.ProvinceId), FieldCodes.InvalidProvince))"],
        ["7.2.22.1"] = ["Required|Modules/Settings/LegalTexts.cs#nameof(LegalTextVersion.Text), FieldCodes.FilledFieldCleared"],
        ["7.2.22.2"] = ["Required|Modules/Settings/LegalTexts.cs#nameof(LegalTextVersion.HostingLocation), FieldCodes.FilledFieldCleared"],
    };

    /// <summary>The row lines: the table above, the code checks and the row lines of <see cref="FormFieldRuleEvidence"/> (the code of the plain-word rules and the references of the rule cell), united row by row.</summary>
    internal static IReadOnlyDictionary<string, string[]> RowLines { get; } = FormFieldRuleEvidence.United(WithCodeChecks(OwnRowLines, CodeCheckLines), places: false);

    private static Dictionary<string, string[]> WithCodeChecks(IReadOnlyDictionary<string, string[]> own, IReadOnlyDictionary<string, string[]> checks)
    {
        Dictionary<string, string[]> united = new(own, StringComparer.Ordinal);
        foreach ((string row, string[] lines) in checks)
        {
            united[row] = united.TryGetValue(row, out string[]? held) ? [.. held, .. lines] : lines;
        }

        return united;
    }

    /// <summary>
    /// The lines of every row as the row's own table and <see cref="FormFieldPlaceEnforcement"/> give them: a row with one place has its lines in <see cref="RowLines"/>; a row with several property places has the
    /// lines of each place in the place table (E1: the evidence of a place is its own code), and the row's lines are the union of the places' lines. The row-level comparison (<see cref="FormFieldEnforcement"/>) reads this union.
    /// </summary>
    public static IReadOnlyDictionary<string, string[]> Lines { get; } = Merge(RowLines, FormFieldPlaceEnforcement.Lines);

    private static Dictionary<string, string[]> Merge(IReadOnlyDictionary<string, string[]> rows, IReadOnlyDictionary<string, string[]> places)
    {
        Dictionary<string, string[]> merged = new(rows, StringComparer.Ordinal);
        foreach ((string key, string[] lines) in places)
        {
            string row = key[..key.IndexOf('|', StringComparison.Ordinal)];
            string[] held = merged.TryGetValue(row, out string[]? own) ? own : [];
            string[] union = [.. held.Concat(lines).Distinct(StringComparer.Ordinal)];
            if (union.Length > 0)
            {
                merged[row] = union;
            }
        }

        return merged;
    }
}
