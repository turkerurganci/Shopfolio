namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The enforcement lines of the field rows (T547; see <see cref="FormFieldEnforcement"/>): for a field whose check is not an attribute of its property, the line of the code that makes the check -
/// <c>Kind|file#fragment</c>, the fragment a whole piece of one line, or two pieces <c>field@@check</c> within twelve lines of each other, that names the field and the error code (or the symbol of the
/// parameter or of the limit) so the line cannot be another field's. A limit's line is the guard of its form: the limit counts the form, not a field.
/// </summary>
internal static class FormFieldEnforcementTable
{
    private const string Contact = "Modules/CorporateContent/ContactFormSending.cs";
    private const string Product = "Pages/Product.cshtml.cs";
    private const string Checkout = "Pages/Storefront/Checkout.cshtml.cs";
    private const string Detail = "Pages/Panel/OrderDetail.cshtml.cs";
    private const string Corrections = "Pages/Panel/OrderDetail.Corrections.cs";
    private const string Item = "Pages/Panel/OrderDetail.ItemSteps.cs";
    private const string Refunds = "Pages/Panel/OrderDetail.Refunds.cs";
    private const string Steps = "Modules/Ordering/PanelItemSteps.cs";
    private const string Inputs = "Modules/CorporateContent/ContentInputs.cs";
    private const string Gate = "Modules/CorporateContent/ContentRecords.cs";
    private const string Identity = "Modules/Settings/CompanyIdentityRules.cs";
    private const string Saves = "Modules/Catalog/ProductSaves.cs";
    private const string Books = "Modules/Membership/AddressBooks.cs";
    private const string Members = "Modules/Membership/MembershipModule.cs";
    private const string Regime = "Infrastructure/Security/PasswordRegime.cs";
    private const string Session = "Infrastructure/Security/SessionAuthenticationHandler.cs";

    public static IReadOnlyDictionary<string, string[]> Lines { get; } = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        ["7.1.1.1"] = [$"Amount|Pages/Storefront/CategoryPage.cs#ListFilters.ReadPrice(MinPrice, MaxPrice)"],
        ["7.1.2.1"] = [$"Required|{Product}#nameof(AddCartItemInput.VariantId), FieldCodes.FieldRequired"],
        ["7.1.3.1"] = [$"Required|{Contact}#Require(fields, ContactFormFields.Name, name)", $"L-4|{Contact}#AttemptLimit.L4, byIp"],
        ["7.1.3.2"] = [$"Required|{Contact}#ContactFormFields.Email, ErrorCode.FieldRequired", $"Email|{Contact}#ContactFormFields.Email, ErrorCode.InvalidEmail", $"L-4|{Contact}#AttemptLimit.L4, byIp"],
        ["7.1.3.3"] = [$"Phone|{Contact}#ContactFormFields.Phone, ErrorCode.InvalidPhone", $"L-4|{Contact}#AttemptLimit.L4, byIp"],
        ["7.1.3.4"] = [$"Required|{Contact}#ContactFormFields.ContactTopic, ErrorCode.FieldRequired", $"List|{Contact}#ContactFormFields.ContactTopic, ErrorCode.InvalidListValue", $"L-4|{Contact}#AttemptLimit.L4, byIp"],
        ["7.1.5.1"] = [$"Required|{Checkout}#nameof(PlaceOrderInput.ContactEmail), FieldCodes.FieldRequired", $"Email|{Checkout}#nameof(PlaceOrderInput.ContactEmail), FieldCodes.InvalidEmail"],

        ["7.1.6.1"] = ["L-5|Pages/Orders/Tracking.cshtml.cs#AttemptLimit.L5, byEmail"],
        ["7.1.6.2"] = ["L-5|Pages/Orders/Tracking.cshtml.cs#AttemptLimit.L5, byEmail"],
        ["7.1.11.1"] = [$"L-3|{Members}#AttemptLimit.L3, byEmail"],
        ["7.1.11.2"] = [$"L-3|{Members}#AttemptLimit.L3, byEmail"],
        ["7.1.11.3"] = [$"L-3|{Members}#AttemptLimit.L3, byEmail"],
        ["7.1.11.4"] = [$"L-9|{Members}#AttemptLimit.L9, byEmail"],
        ["7.1.12.1"] = [$"L-1|{Regime}#AttemptLimit.L1, subject"],
        ["7.1.12.2"] = [$"L-1|{Regime}#AttemptLimit.L1, subject"],
        ["7.1.12.3"] = [$"Z-4|{Session}#ProductConstants.ShortSessionHours", $"Z-5|{Session}#ProductConstants.LongSessionDays"],
        ["7.1.12.4"] = [$"L-9|{Members}#AttemptLimit.L9, byEmail"],
        ["7.1.13.1"] = [$"L-2|{Regime}#AttemptLimit.L2, byEmail"],
        ["7.1.14.1"] = [$"L-1|{Regime}#AttemptLimit.L1, subject"],
        ["7.1.15.2"] = [$"L-9|{Members}#AttemptLimit.L9, byEmail"],
        ["7.1.15.4"] = [$"L-9|{Members}#AttemptLimit.L9, byEmail"],
        ["7.2.1.1"] = [$"L-1|{Regime}#AttemptLimit.L1, subject"],
        ["7.2.1.2"] = [$"L-1|{Regime}#AttemptLimit.L1, subject"],
        ["7.2.1.3"] = [$"Z-4|{Session}#ProductConstants.ShortSessionHours", $"Z-5|{Session}#ProductConstants.LongSessionDays"],
        ["7.2.1.4"] = [$"L-2|{Regime}#AttemptLimit.L2, byEmail"],
        ["7.2.24.2"] = [$"L-9|{Members}#AttemptLimit.L9, byEmail"],
        ["7.2.24.4"] = [$"L-9|{Members}#AttemptLimit.L9, byEmail"],
        ["7.2.26.1"] = [$"L-1|{Regime}#AttemptLimit.L1, subject"],

        // Pass 2: the checkout, the order's customer screens, the address book.
        ["7.1.5.2"] = [$"Required|{Books}#request.ShippingRecipientName,@@AddressFieldRules.CheckAddress("],
        ["7.1.5.3"] = [$"Required|{Books}#AddressFieldRules.TryStorePhone(nameof(OrderAddressRequest.ShippingPhone), phoneText, true, errors", $"Phone|{Books}#AddressFieldRules.TryStorePhone(nameof(OrderAddressRequest.ShippingPhone), phoneText, true, errors"],
        ["7.1.5.6"] = [$"Required|{Books}#request.BillingRecipientName,@@AddressFieldRules.CheckAddress("],
        ["7.1.5.7"] = ["L-6|Modules/Catalog/Coupons.cs#AttemptLimit.L6, cart"],
        ["7.1.5.8"] = [$"Required|{Checkout}#nameof(PlaceOrderInput.PaymentMethod), FieldCodes.FieldRequired", "L-8|Modules/Ordering/OrderPlacementLimits.cs#return AttemptLimit.L8;"],
        ["7.1.5.10"] = ["L-7|Modules/Ordering/OrderPlacementLimits.cs#return AttemptLimit.L7;"],
        ["7.1.7.1"] = ["Required|Pages/Orders/OrderPageModels.cs#nameof(SaveRefundIbanInput.Iban), FieldCodes.FieldRequired"],
        ["7.1.8.1"] = ["Required|Pages/Orders/OrderScreenModels.cs#ModelState.AddModelError(string.Empty, NoItemText)"],
        ["7.1.8.2"] = ["Quantity|Pages/Orders/OrderScreenModels.cs#nameof(CancelOrderItemsInputLine.Quantity)}\", FieldCodes.InvalidQuantity"],
        ["7.1.8.3"] = ["Required|Pages/Orders/OrderScreenModels.cs#Screen.AsksIban && string.IsNullOrWhiteSpace(input.Iban)@@FieldCodes.FieldRequired"],
        ["7.1.9.1"] = ["Required|Pages/Orders/AfterSalesScreenModels.cs#nameof(SubmitWithdrawalInput.Items), FieldCodes.FieldRequired"],
        ["7.1.9.2"] = ["Quantity|Pages/Orders/AfterSalesScreenModels.cs#nameof(WithdrawalItemRow.Quantity)}\", FieldCodes.InvalidQuantity"],
        ["7.1.9.3"] = ["Required|Pages/Orders/AfterSalesScreenModels.cs#nameof(SubmitWithdrawalInput.Iban), FieldCodes.FieldRequired"],
        ["7.1.10.1"] = ["Required|Pages/Orders/AfterSalesScreenModels.cs#nameof(SubmitDefectClaimInput.OrderItemId), FieldCodes.FieldRequired"],
        ["7.1.10.2"] = ["Quantity|Pages/Orders/AfterSalesScreenModels.cs#nameof(SubmitDefectClaimInput.Quantity), FieldCodes.InvalidQuantity"],
        ["7.1.16.1"] = [$"Required|{Books}#nameof(AddressBookInput.Name), ErrorCode.FieldRequired"],
        ["7.1.16.2"] = [$"Required|{Books}#AddressFieldRules.BookPrefix, input.RecipientName@@AddressFieldRules.CheckAddress("],
        ["7.1.16.3"] = [$"Phone|{Books}#AddressFieldRules.TryStorePhone(nameof(AddressBookInput.Phone), input.Phone, false, errors"],
        ["7.2.3.1"] = ["SearchTrim|Infrastructure/SearchTerm.cs#ProductConstants.ProductNameMaxLength"],

        // Pass 3: the product form, the settings, the corporate content.
        ["7.2.4.1"] = [$"Required|{Saves}#nameof(ProductSaveCommand.ProductType), ErrorCode.FieldRequired"],
        ["7.2.4.2"] = [$"Required|{Saves}#nameof(ProductSaveCommand.Name), ErrorCode.FieldRequired", $"P-27|{Saves}#nameof(ProductSaveCommand.Name), ErrorCode.TextTooLong, ProductConstants.ProductNameMaxLength", $"TextCap|{Saves}#nameof(ProductSaveCommand.Name), ErrorCode.TextTooLong, ProductConstants.ProductNameMaxLength"],
        ["7.2.4.3"] = [$"Required|{Saves}#new ProductSaveFieldError(\"Categories\", ErrorCode.FieldRequired)"],
        ["7.2.4.5"] = ["Image|Pages/Panel/ProductForm.cshtml.cs#RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, UploadRefusalText(result))", "P-22|Pages/Components/UploadComponentModels.cs#ProductConstants.ImageFormats", "P-23|Modules/Catalog/ImageUploads.cs#content.LongLength > ProductConstants.ImageMaxBytes", $"P-24|{Saves}#new ProductSaveFieldError(\"Images\", ErrorCode.ImageLimitExceeded, ProductConstants.ImagesPerProductMax)"],
        ["7.2.4.6"] = ["P-21|Modules/Catalog/OptionStructureRules.cs#order <= ProductConstants.OptionDimensionMax"],
        ["7.2.4.7"] = [$"Required|{Saves}#Field(i, nameof(ProductSaveVariant.SKU)), ErrorCode.FieldRequired"],
        ["7.2.4.8"] = [$"Required|{Saves}#Field(i, nameof(ProductSaveVariant.Price)), ErrorCode.FieldRequired"],
        ["7.2.4.9"] = [$"Required|{Saves}#Field(i, nameof(ProductSaveVariant.StockQuantity)), ErrorCode.FieldRequired"],
        ["7.2.4.10"] = ["Image|Pages/Components/UploadComponentModels.cs#sizeBytes > UploadLimits.MaxBytes(UploadSurface.DigitalFile) ? new UploadRefusal(FieldCodes.InvalidImage", "P-26|Infrastructure/Files/DigitalFileDelivery.cs#sizeBytes > ProductConstants.DigitalFileMaxBytes"],
        ["7.2.4.11"] = [$"Required|{Saves}#nameof(ProductSaveCommand.VatRate), ErrorCode.FieldRequired", "P-10|Pages/Panel/ProductForm.cshtml.cs#VatRate = (await _settings.GetProductDefaultsAsync(ct)).DefaultVatRate"],
        ["7.2.4.12"] = [$"Required|{Saves}#nameof(ProductSaveCommand.OriginCountryId), ErrorCode.FieldRequired"],
        ["7.2.4.14"] = ["Period|Modules/Catalog/ProductDefinitionRules.cs#if (!CheckShippingPeriod(shippingPeriod))", "P-5|Modules/Settings/FirmSettings.cs#nameof(PeriodsInput.DefaultShippingPeriod), \"P-5\"", "P-6|Modules/Catalog/ProductDefinitionRules.cs#FirmSettingFences.Check(\"P-6\", days)"],
        ["7.2.4.16"] = [$"P-9|{Saves}#command.MaxQuantityPerOrder is int maxQuantity && !FirmSettingFences.Check(\"P-9\", maxQuantity)"],
        ["7.2.5.1"] = ["P-28|Pages/Panel/Categories.cshtml.cs#public string? Name { get; set; }@@[TextTooLong(ProductConstants.ShortTextMaxLength)]"],
        ["7.2.5.2"] = ["P-20|Modules/Catalog/CategoryLevels.cs#level <= ProductConstants.CategoryLevelMax"],
        ["7.2.6.1"] = ["Required|Modules/Catalog/CouponRules.cs#nameof(CouponDefinition.Code), ErrorCode.FieldRequired", "CouponCode|Modules/Catalog/Coupons.cs#c.NormalizedCode == normalized && c.Id != selfId"],
        ["7.2.6.3"] = ["Required|Pages/Panel/Coupons.cshtml.cs#yield return FieldError.Result(FieldCodes.FieldRequired, nameof(Percentage))", "Amount|Modules/Catalog/CouponRules.cs#nameof(CouponDefinition.Value), ErrorCode.InvalidAmount"],
        ["7.2.6.4"] = ["Required|Pages/Panel/Coupons.cshtml.cs#yield return FieldError.Result(FieldCodes.FieldRequired, nameof(MinimumBasketAmount))"],
        ["7.2.17.1"] = ["Required|Modules/Settings/FirmSettings.cs#nameof(HomepageInput.HomepageLayout), input.HomepageLayout is null ? FieldCodes.FieldRequired"],
        ["7.2.17.2"] = ["Required|Modules/Settings/FirmSettings.cs#nameof(HomepageInput.MenuNameProducts)@@RequiredName("],
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
        ["7.1.3.5"] = [$"Required|{Contact}#Require(fields, ContactFormFields.Message, message)", $"L-4|{Contact}#AttemptLimit.L4, byIp"],
        ["7.1.5.9"] = ["CheckboxAccepted|Pages/Components/ConsentModels.cs#modelState.AddFieldError(NameOf(kind, namePrefix), FieldCodes.CheckboxNotAccepted)"],
        ["7.2.7.1"] = [$"Required|{Detail}#input.Carrier, input.OtherCarrierName, input.TrackingNumber@@ErrorCode.ShipmentInfoMissing"],
        ["7.2.7.2"] = [$"Required|{Detail}#input.Carrier, input.OtherCarrierName, input.TrackingNumber@@ErrorCode.ShipmentInfoMissing"],
        ["7.2.8.1"] = [$"Required|{Detail}#string.IsNullOrWhiteSpace(input.DeliveryDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate", $"Date|{Detail}#string.IsNullOrWhiteSpace(input.DeliveryDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate", $"Required|{Corrections}#string.IsNullOrWhiteSpace(input.DeliveryDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate", $"Date|{Corrections}#string.IsNullOrWhiteSpace(input.DeliveryDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate"],
        ["7.2.8.2"] = [$"Required|{Steps}#request.IsDeliveredBeforeDeclaration is null@@PanelItemObstacle.DeliveryOrderMissing", $"Required|Modules/Ordering/OrderCorrections.Interventions.cs#sameDay.Count > 0 && request.IsDeliveredBeforeDeclaration is null@@AsksDeliveryOrder = true"],
        ["7.2.8.3"] = [$"Required|{Item}#string.IsNullOrWhiteSpace(input.ArrivalDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate", $"Required|{Corrections}#string.IsNullOrWhiteSpace(input.ArrivalDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate"],
        ["7.2.9.1"] = [$"Required|{Steps}#request.Items.Count == 0@@PanelItemObstacle.ItemNotSelected"],
        ["7.2.9.2"] = [$"Quantity|{Item}#OnPostCancelOrderByCompanyAsync(@@ErrorCode.InvalidQuantity"],
        ["7.2.9.3"] = [$"Required|{Item}#OnPostCancelOrderByCompanyAsync(@@string.IsNullOrWhiteSpace(input.CancellationReason) ? ErrorCode.FieldRequired"],
        ["7.2.9.4"] = ["Required|Pages/Components/ReasonModels.cs#new ReasonFieldError(ExplanationMember, FieldCodes.FieldRequired)"],
        ["7.2.10.1"] = [$"Required|{Item}#string.IsNullOrWhiteSpace(input.DeclarationDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate"],
        ["7.2.10.2"] = [$"Required|{Steps}#items.Count == 0 || items.Any(i => i.OrderItemId == Guid.Empty)@@PanelItemObstacle.ItemNotSelected", $"Quantity|{Item}#OnPostRecordWithdrawalAsync(@@ErrorCode.InvalidQuantity"],
        ["7.2.10.3"] = [$"Required|{Item}#string.IsNullOrWhiteSpace(input.DeclarationDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate"],
        ["7.2.10.4"] = [$"Iban|{Item}#!NoticeIbanRule.Allows(iban)@@ErrorCode.InvalidIban"],
        ["7.2.10.5"] = [$"Required|{Steps}#warnings.NeedsConfirmation && !request.WarningsConfirmed"],
        ["7.2.10.6"] = [$"Required|{Steps}#warnings.AsksDeliveryOrder && request.IsDeliveredBeforeDeclaration is null@@PanelItemObstacle.DeliveryOrderMissing"],
        ["7.2.11.1"] = [$"Required|{Item}#OnPostRemoveOrderItemAsync(@@!TryReadQuantity(input.Quantity, out int quantity)"],
        ["7.2.11.2"] = [$"Required|{Item}#OnPostCloseNotReturnedAsync(@@!TryReadQuantity(input.Quantity, out int read)"],
        ["7.2.13.1"] = [$"Required|{Refunds}#string.IsNullOrWhiteSpace(input.Amount)@@ErrorCode.FieldRequired", $"Amount|{Refunds}#FieldRuleAttribute.HasAtMostDecimals(amount, AmountDecimals)@@ErrorCode.InvalidAmount"],
        ["7.2.13.3"] = [$"Required|{Refunds}#PanelRefundObstacle.ItemNotSelected => await RefuseAsync(id, ItemNotSelectedText, ErrorCode.FieldRequired, ct)"],
        ["7.2.14.3"] = [$"Required|{Corrections}#RefuseCorrectionAsync(id, TransitionFormId, ReasonMissingText, ErrorCode.FieldRequired, ct)"],
        ["7.2.14.4"] = [$"Required|{Corrections}#CorrectionValueProblem.Missing => await RefuseCorrectionAsync(id, OrderEmailFormId, EmailMissingText, ErrorCode.FieldRequired, ct)", $"Email|{Corrections}#CorrectionValueProblem.Invalid => await RefuseCorrectionAsync(id, OrderEmailFormId, EmailInvalidText, ErrorCode.InvalidEmail, ct)"],

        // Pass 5: what is left of the order detail, the content, the identity, the lists.
        ["7.1.1.3"] = ["SearchTrim|Infrastructure/SearchTerm.cs#ProductConstants.ProductNameMaxLength", "Required|Pages/Search.cshtml.cs#if (list is null)@@// No word: no result list"],
        ["7.1.2.2"] = [$"Quantity|{Product}#nameof(AddCartItemInput.Quantity), FieldCodes.InvalidQuantity", "P-9|Pages/Components/CartRefusalCodes.cs#CartAddOutcome.OverOrderLimit => ErrorCode.MaxQuantityPerOrderExceeded"],
        ["7.2.3.4"] = ["Required|Pages/Panel/Members.cshtml.cs#Record = new MemberRecordView { State = MemberSearchState.NotSearched }"],
        ["7.2.3.5"] = ["DateRange|Pages/Panel/AuditTrail.cshtml.cs#Components.DateFields.CheckRange(query.StartDate, query.EndDate, today, DateRangePolicy.NoFuture, false, false)"],
        ["7.2.4.15"] = [$"Required|Modules/Catalog/PublishGate.cs#unmet.Add(PublishCondition.FulfilmentPeriod)", $"P-44|{Saves}#command.FulfilmentPeriod is int fulfilmentPeriod && !FirmSettingFences.Check(\"P-44\", fulfilmentPeriod)"],
        ["7.2.12.1"] = [$"Required|{Item}#string.IsNullOrWhiteSpace(input.ArrivalDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate"],
        ["7.2.12.2"] = [$"Required|{Item}#ArriveAsync(Guid id, ReceiveReturnInput input, bool isRejection, CancellationToken ct)@@@!TryReadQuantity(input.Quantity, out int read)"],
        ["7.2.12.3"] = [$"Required|{Item}#RejectionConfirm(id, input, quantity)@@result.Refusal == TransitionRefusal.ConfirmationRequired"],
        ["7.2.14.1"] = ["Required|Modules/Ordering/OrderCorrections.cs#OrderAddressResolution resolution = await _membership.ResolveOrderAddressesAsync(", "Phone|Modules/Ordering/OrderCorrections.cs#OrderAddressResolution resolution = await _membership.ResolveOrderAddressesAsync("],
        ["7.2.14.2"] = ["Required|Modules/Ordering/PanelOrderCorrections.cs#!OrderTransitions.TryNormalizeShipment(request.Shipment, out _)@@CorrectionValueProblem.Missing"],
        ["7.2.16.1"] = [$"Required|{Inputs}#RequiredWithCeiling(rejections, nameof(OfferingInput.Name), input.Name, ContentRules.TitleMaxLength)", $"TextCap|{Inputs}#RequiredWithCeiling(rejections, nameof(OfferingInput.Name), input.Name, ContentRules.TitleMaxLength)", $"P-27|{Inputs}#RequiredWithCeiling(rejections, nameof(OfferingInput.Name), input.Name, ContentRules.TitleMaxLength)", $"P-28|{Inputs}#RequiredWithCeiling(rejections, nameof(FaqItemInput.Question), input.Question, ContentRules.ShortDescriptionMaxLength)"],
        ["7.2.16.2"] = [$"Required|{Gate}#ContentRecordKind.AboutPage => [ContentGateField.ShortIntroduction]", $"P-28|{Inputs}#Ceiling(rejections, nameof(OfferingInput.ShortDescription), Text(input.ShortDescription), ContentRules.ShortDescriptionMaxLength)", $"P-29|{Inputs}#Ceiling(rejections, nameof(AboutPageInput.ShortIntroduction), Text(input.ShortIntroduction), ContentRules.ShortIntroductionMaxLength)"],
        ["7.2.16.3"] = [$"Required|{Gate}#ContentRecordKind.FaqItem => [ContentGateField.Question, ContentGateField.Answer]"],
        ["7.2.16.4"] = [$"Required|{Gate}#ContentRecordKind.Announcement => [ContentGateField.Text]", $"P-30|{Inputs}#Ceiling(rejections, nameof(AnnouncementInput.Text), Text(input.Text), ContentRules.AnnouncementTextMaxLength)"],
        ["7.2.16.5"] = [$"Required|{Gate}#ContentRecordKind.Branch => [ContentGateField.Name, ContentGateField.Province, ContentGateField.District, ContentGateField.AddressLine]", "Province|Modules/CorporateContent/ContentAuthoring.cs#rejections.Add(new FieldRejection(nameof(BranchInput.ProvinceId), FieldCodes.InvalidProvince))"],
        ["7.2.16.6"] = [$"Phone|{Inputs}#Apply(rejections, nameof(BranchInput.Phone), Phone(input.Phone), new InvalidPhoneAttribute(), FieldCodes.InvalidPhone)"],
        ["7.2.16.7"] = [$"Url|{Inputs}#Apply(rejections, nameof(BranchInput.MapUrl), Text(input.MapUrl), new InvalidUrlAttribute(), FieldCodes.InvalidUrl)", $"Url|{Inputs}#Apply(rejections, nameof(AnnouncementInput.LinkUrl), Text(input.LinkUrl), new InvalidUrlAttribute(), FieldCodes.InvalidUrl)"],
        ["7.2.16.9"] = ["Image|Pages/Panel/ContentForm.cshtml.cs#RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, UploadRefusalText(result, surface))", "P-22|Pages/Components/UploadComponentModels.cs#ProductConstants.ImageFormats", "P-23|Modules/Catalog/ImageUploads.cs#content.LongLength > ProductConstants.ImageMaxBytes", "P-25|Pages/Panel/ContentForm.cshtml.cs#input.Images.Count >= ProductConstants.ImagesPerContentRecordMax"],
        ["7.2.18.1"] = ["Required|Modules/CorporateContent/BrandingSettings.cs#nameof(BrandingInput.BrandName), FieldCodes.FilledFieldCleared"],
        ["7.2.18.2"] = ["Image|Modules/CorporateContent/BrandingSettings.cs#rejections.Add(new BrandingRejection(field, FieldCodes.InvalidImage))", "P-22|Pages/Components/UploadComponentModels.cs#ProductConstants.ImageFormats", "P-23|Modules/Catalog/ImageUploads.cs#content.LongLength > ProductConstants.ImageMaxBytes"],
        ["7.2.19.1"] = [$"Required|{Identity}#else if (input.CompanyType is null && current.CompanyType is not null)@@FieldCodes.FilledFieldCleared"],
        ["7.2.19.2"] = ["Required|Modules/Settings/Contracts/CompanyIdentityFieldSets.cs#nameof(CompanyIdentityInput.LegalName), nameof(CompanyIdentityInput.TaxIdNumber)@@@TradesmanFields"],
        ["7.2.19.3"] = [$"Required|{Identity}#nameof(CompanyIdentityInput.ProvinceId),@@@FieldCodes.FilledFieldCleared", $"Province|{Identity}#rejections.Add(new FieldRejection(nameof(CompanyIdentityInput.ProvinceId), FieldCodes.InvalidProvince))"],
        ["7.2.19.4"] = [$"Required|{Identity}#nameof(CompanyIdentityInput.Phone),@@@FieldCodes.FilledFieldCleared", $"Phone|{Identity}#AddFormRejection(rejections, nameof(CompanyIdentityInput.Phone), input.Phone, new InvalidPhoneAttribute(), FieldCodes.InvalidPhone)"],
        ["7.2.19.5"] = [$"Required|{Identity}#nameof(CompanyIdentityInput.Email),@@@FieldCodes.FilledFieldCleared", $"Email|{Identity}#AddFormRejection(rejections, nameof(CompanyIdentityInput.Email), input.Email, new InvalidEmailAttribute(), FieldCodes.InvalidEmail)"],
        ["7.2.19.6"] = [$"Required|{Identity}#nameof(CompanyIdentityInput.ChamberName),@@@FieldCodes.FilledFieldCleared", $"Required|{Identity}#nameof(CompanyIdentityInput.ProfessionalConductRules),@@@FieldCodes.FilledFieldCleared", $"Url|{Identity}#nameof(CompanyIdentityInput.ProfessionalConductRules), input.ProfessionalConductRules, new InvalidUrlAttribute(), FieldCodes.InvalidUrl"],
        ["7.2.21.6"] = ["Required|Modules/Settings/CompanySettingsWriter.cs#nameof(ReturnAddressInput.RecipientName)@@FieldCodes.FieldRequired"],
        ["7.2.25.2"] = ["Required|Pages/Panel/DataExport.cshtml.cs#DateFields.CheckRange(input.StartDate, input.EndDate, Today, DateRangePolicy.NoFuture, startRequired: true, endRequired: true)", "DateRange|Pages/Panel/DataExport.cshtml.cs#DateFields.CheckRange(input.StartDate, input.EndDate, Today, DateRangePolicy.NoFuture, startRequired: true, endRequired: true)"],
    };
}
