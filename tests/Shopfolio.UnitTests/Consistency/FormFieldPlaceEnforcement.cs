namespace Shopfolio.UnitTests.Consistency;

/// <summary>
/// The enforcement lines of each property place of a field row that has several property places (T547 correction 4, E1; 04 7 introduction; 09 21.4 item 1). A row's requirement and rule are read from the union
/// of its places, so a check one place keeps in a handler or a module would be "proved" by another place's line of the same kind; this table gives every property place that carries no rule attribute (the places
/// <see cref="FormFieldPlaceMarks"/> does not pin) its own lines, one <c>Kind|file#fragment</c> per (place, kind) and per place of the code that makes the check (two handlers that check the same field are two lines of
/// that place). A place with an empty entry has no check of its own of a kind the row names (a filter, a flag, an optional text): the entry is the statement, and a place with no entry is
/// <c>PlaceEnforcementMissing</c> (nothing passes silently). The key is <c>row|place</c>, the place written as in <see cref="FormFieldInventory"/>. A line is held exactly once by its file
/// (<c>PlaceLineNotInCode</c>: the line is not in the file, or is held more than once), so removing or changing the check of one place reports that place whatever the other places keep. A piece that is the one
/// statement of two places (one call over both fields, a helper both routes call) is the line of both.
/// <see cref="FormFieldEnforcementTable.Lines"/> merges these lines into the row's lines, so the row-level comparison (<see cref="FormFieldEnforcement"/>) is unchanged.
/// </summary>
internal static class FormFieldPlaceEnforcement
{
    private const string Saves = "Modules/Catalog/ProductSaves.cs";
    private const string ProductForm = "Pages/Panel/ProductForm.cshtml.cs";
    private const string Detail = "Pages/Panel/OrderDetail.cshtml.cs";
    private const string Corrections = "Pages/Panel/OrderDetail.Corrections.cs";
    private const string Item = "Pages/Panel/OrderDetail.ItemSteps.cs";
    private const string Steps = "Modules/Ordering/PanelItemSteps.cs";
    private const string Shipment = "Modules/Ordering/OrderTransitions.Shipment.cs";
    private const string PanelCorrections = "Modules/Ordering/PanelOrderCorrections.cs";
    private const string Inputs = "Modules/CorporateContent/ContentInputs.cs";
    private const string Gate = "Modules/CorporateContent/ContentRecords.cs";

    // The pieces of the date checks that read the value: the condition that reads the date and the refusal it gives (D-1671). A piece that is only the refusal passes when the read is changed to a constant, so
    // the condition (the date is read from the field's own input) is part of the line.
    private const string MarkDeliveredRead = "if (!DateOnly.TryParseExact(input.DeliveryDate, \"yyyy-MM-dd\", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)) { return await RefuseAsync(id, DeliveryDateMissingText, string.IsNullOrWhiteSpace(input.DeliveryDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate";
    private const string CorrectDeliveryRead = "if (!DateOnly.TryParseExact(input.DeliveryDate, \"yyyy-MM-dd\", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)) { return await RefuseCorrectionAsync( id, DeliveryDateFormId, DeliveryDateMissingText, string.IsNullOrWhiteSpace(input.DeliveryDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate";
    private const string CorrectArrivalRead = "if (!DateOnly.TryParseExact(input.ArrivalDate, \"yyyy-MM-dd\", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly date)) { return await RefuseCorrectionAsync( id, form, ArrivalDateMissingText, string.IsNullOrWhiteSpace(input.ArrivalDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate";
    private const string UploadRefusal = "Image|" + ProductForm + "#RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, UploadRefusalText(result))";
    private const string DigitalFileImage = "Image|Pages/Components/UploadComponentModels.cs#sizeBytes > UploadLimits.MaxBytes(UploadSurface.DigitalFile) ? new UploadRefusal(FieldCodes.InvalidImage";
    private const string DigitalFileSize = "P-26|Infrastructure/Files/DigitalFileDelivery.cs#sizeBytes > ProductConstants.DigitalFileMaxBytes";
    private const string Carrier = "Required|" + Shipment + "#(carrier is null) == (other is null)";
    private const string Tracking = "Required|" + Shipment + "#|| tracking is null)";
    private const string ShipmentWiring = "Required|" + Detail + "#ShipmentInfo shipment = new(input.Carrier, input.OtherCarrierName, input.TrackingNumber, input.IsOwnVehicleDelivery); TransitionAdmin admin = await AdminAsync(ct); ShipOrderRequest request = new(id, admin, input.Version, input.Confirmed, shipment); PanelStepResult result = await RunStepAsync( (steps, token) => isResending ? steps.MarkReshippedAsync(request, token) : steps.MarkShippedAsync(request, token), ct); return result.Refusal switch { TransitionRefusal.InvalidInput => await RefuseAsync(id, ShipmentMissingText, ErrorCode.ShipmentInfoMissing, ct)";
    private const string CorrectionShipment = "Required|" + PanelCorrections + "#if (request.Shipment is null || !OrderTransitions.TryNormalizeShipment(request.Shipment, out _)) { return new PanelCorrectionResult(TransitionRefusal.InvalidInput) { Problem = CorrectionValueProblem.Missing };";
    private const string NoticeDate = "Required|" + Item + "#Notice = input; if (!TryReadDate(input.DeclarationDate, out DateOnly date)) { return await RefuseAsync(id, NoticeDateMissingText, string.IsNullOrWhiteSpace(input.DeclarationDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate";
    private const string TerminationDate = "Required|" + Item + "#Termination = input; if (!TryReadDate(input.DeclarationDate, out DateOnly date)) { return await RefuseAsync(id, NoticeDateMissingText, string.IsNullOrWhiteSpace(input.DeclarationDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate";
    private const string IbanObstacle = "Iban|" + Item + "#case PanelItemObstacle.IbanInvalid: return await RefuseAsync(id, NoticeIbanInvalidText, ErrorCode.InvalidIban, ct);";
    private const string ReasonPage = "Required|" + Corrections + "#RefuseCorrectionAsync(id, TransitionFormId, ReasonMissingText, ErrorCode.FieldRequired, ct)";
    private const string FaqGate = "Required|" + Gate + "#ContentRecordKind.FaqItem => [ContentGateField.Question, ContentGateField.Answer]";

    private static IReadOnlyDictionary<string, string[]> Own { get; } = new Dictionary<string, string[]>(StringComparer.Ordinal)
    {
        // 7.2.3 the lists' filters (query models): the search term is trimmed and capped; the closed-value filters and the flags carry no check of a kind the rows name.
        ["7.2.3.1|Panel.AdminProductQuery.Search"] = ["SearchTrim|Modules/Catalog/PanelProductListReader.cs#if (SearchTerm.ToLikePattern(search) is { } pattern)", "SearchTrim|Infrastructure/SearchTerm.cs#if (word.Length > ProductConstants.ProductNameMaxLength)"],
        ["7.2.3.1|Panel.AdminProductQuery.PublishStatus"] = [], // the row names the search field (7.3.2.17, K-737); a closed filter has no rule of its own in the cell.
        ["7.2.3.2|Panel.OrderQuery.Search"] = [],
        ["7.2.3.2|Panel.OrderQuery.OrderStatus"] = [], // the cell names exact matching and the e-mail case rule (K-802): rules of the search term only.
        ["7.2.3.2|Panel.OrderQuery.PaymentStatus"] = [], // as OrderStatus: the cell names no rule of a closed filter.
        ["7.2.3.2|Panel.OrderQuery.PendingWork"] = [], // as OrderStatus: the cell names no rule of a flag.
        ["7.2.3.3|Panel.RequestQuery.RequestType"] = [],
        ["7.2.3.3|Panel.RequestQuery.RequestStatus"] = [],
        ["7.2.3.5|Panel.AuditLogQuery.StartDate"] = ["DateRange|Pages/Panel/AuditTrail.cshtml.cs#Components.DateFields.CheckRange(query.StartDate, query.EndDate, today, DateRangePolicy.NoFuture, false, false)"],
        ["7.2.3.5|Panel.AuditLogQuery.EndDate"] = ["DateRange|Pages/Panel/AuditTrail.cshtml.cs#Components.DateFields.CheckRange(query.StartDate, query.EndDate, today, DateRangePolicy.NoFuture, false, false)"],
        ["7.2.3.5|Panel.AuditLogQuery.AdminId"] = [],

        // 7.2.4 the product form.
        ["7.2.4.3|Panel.SaveProductInput.Categories"] = [$"Required|{Saves}#PublishCondition.MainCategory => \"Categories\""],
        ["7.2.4.3|Panel.SaveProductCategoryRow.IsPrimaryCategory"] = [$"Required|{Saves}#else if (wanted.Count > 0 && (command.PrimaryCategoryId is not Guid primary || !wanted.Contains(primary))) {{ errors.Add(new ProductSaveFieldError(\"Categories\", ErrorCode.FieldRequired));"],
        ["7.2.4.5|Panel.SaveProductInput.Images"] = [UploadRefusal, "P-22|Pages/Panel/ProductForm.cshtml.cs#ImageUploadResult.OverPixelLimit) ? PixelLimitText : UploadChecks.FormatMessage()", "P-22|Pages/Components/UploadComponentModels.cs#{ProductConstants.ImageFormats} yüklenebilir.", "P-23|Pages/Panel/ProductForm.cshtml.cs#return UploadChecks.SizeMessage(UploadSurface.ProductGallery);", "P-23|Modules/Catalog/ImageUploads.cs#content.LongLength > ProductConstants.ImageMaxBytes", $"P-24|{Saves}#new ProductSaveFieldError(\"Images\", ErrorCode.ImageLimitExceeded, ProductConstants.ImagesPerProductMax)"],
        ["7.2.4.5|Panel.SaveProductImageRow.AltText"] = [],
        ["7.2.4.6|Panel.SaveProductInput.Options"] = ["P-21|Modules/Catalog/OptionStructureRules.cs#order <= ProductConstants.OptionDimensionMax"],
        ["7.2.4.6|Panel.SaveProductOptionRow.Name"] = [],
        ["7.2.4.6|Panel.SaveProductOptionValueRow.Value"] = [], // the cell names the ceiling of the option dimensions (P-21, 02 3.3.2), not of the values.
        ["7.2.4.9|Panel.SaveProductVariantRow.StockQuantity"] = [$"Required|{Saves}#Field(i, nameof(ProductSaveVariant.StockQuantity)), ErrorCode.FieldRequired"],
        ["7.2.4.9|Panel.SaveProductVariantRow.OriginalStockQuantity"] = [],
        ["7.2.4.10|Panel.SaveProductVariantRow.ImageId"] = [UploadRefusal],
        ["7.2.4.10|Panel.SaveProductVariantRow.DigitalFileId"] = [DigitalFileImage, DigitalFileSize],
        ["7.2.4.10|Panel.SaveProductInput.DigitalFileId"] = [DigitalFileImage, DigitalFileSize],
        ["7.2.4.13|Panel.SaveProductInput.MeasurementUnit"] = [],

        // 7.2.7 - 7.2.14 the order detail's operations: the shipment's carrier and name, the dates, the delivery order question, the notice, the corrections.
        ["7.2.7.1|Panel.MarkShippedInput.Carrier"] = [ShipmentWiring, Carrier],
        ["7.2.7.1|Panel.MarkShippedInput.OtherCarrierName"] = [ShipmentWiring, Carrier],
        ["7.2.8.1|Panel.MarkDeliveredInput.DeliveryDate"] = ["Required|" + Detail + "#" + MarkDeliveredRead, "Date|" + Detail + "#" + MarkDeliveredRead],
        ["7.2.8.1|Panel.CorrectDeliveryDateInput.DeliveryDate"] = ["Required|" + Corrections + "#" + CorrectDeliveryRead, "Date|" + Corrections + "#" + CorrectDeliveryRead],
        ["7.2.8.2|Panel.MarkDeliveredInput.IsDeliveredBeforeDeclaration"] = ["Required|Modules/Ordering/PanelOrderSteps.cs#if (sameDay.Count > 0 && deliveredBeforeDeclaration is null) { return new PanelStepResult(TransitionRefusal.InvalidInput) { AsksDeliveryOrder = true,"],
        ["7.2.8.2|Panel.CorrectDeliveryDateInput.IsDeliveredBeforeDeclaration"] = ["Required|Modules/Ordering/OrderCorrections.Interventions.cs#if (sameDay.Count > 0 && request.IsDeliveredBeforeDeclaration is null) { return new OrderCorrectionResult(TransitionRefusal.InvalidInput, [], null) { AsksDeliveryOrder = true };"],
        ["7.2.8.3|Panel.ReceiveReturnInput.ArrivalDate"] = [$"Required|{Item}#if (!TryReadDate(input.ArrivalDate, out DateOnly arrival)) {{ return await RefuseAsync(id, ArrivalDateMissingText, string.IsNullOrWhiteSpace(input.ArrivalDate) ? ErrorCode.FieldRequired : ErrorCode.InvalidDate, ct);"],
        ["7.2.8.3|Panel.CorrectArrivalDateInput.ArrivalDate"] = ["Required|" + Corrections + "#" + CorrectArrivalRead],
        ["7.2.10.1|Panel.RecordWithdrawalInput.DeclarationDate"] = [NoticeDate],
        ["7.2.10.1|Panel.RecordLateDeliveryTerminationInput.DeclarationDate"] = [TerminationDate],
        ["7.2.10.2|Panel.RecordWithdrawalInput.Items"] = [$"Required|{Steps}#if (items.Count == 0 || items.Any(i => i.OrderItemId == Guid.Empty)) {{ return PanelItemStepResult.Refused(PanelItemObstacle.ItemNotSelected);"],
        ["7.2.10.2|Panel.RecordWithdrawalInputLine.Quantity"] = [$"Quantity|{Item}#ct); return await RefuseAsync(id, QuantityInvalidText, ErrorCode.InvalidQuantity, ct);"],
        ["7.2.10.3|Panel.RecordWithdrawalInput.DeclarationDate"] = [NoticeDate],
        ["7.2.10.3|Panel.RecordLateDeliveryTerminationInput.DeclarationDate"] = [TerminationDate],
        ["7.2.10.4|Panel.RecordWithdrawalInput.IsFromContactEmail"] = [],
        ["7.2.10.4|Panel.RecordWithdrawalInput.Iban"] = [$"Iban|{Steps}#if (request.CameFromOrderEmail && !string.IsNullOrWhiteSpace(request.NoticeIban) && !NoticeIbanRule.Allows(request.NoticeIban)) {{ return PanelItemStepResult.Refused(PanelItemObstacle.IbanInvalid);", IbanObstacle],
        ["7.2.10.4|Panel.RecordLateDeliveryTerminationInput.IsFromContactEmail"] = [],
        ["7.2.10.4|Panel.RecordLateDeliveryTerminationInput.Iban"] = [$"Iban|{Item}#if (iban is not null && !NoticeIbanRule.Allows(iban)) {{ return await RefuseAsync(id, NoticeIbanInvalidText, ErrorCode.InvalidIban, ct);", $"Iban|{Steps}#if (!string.IsNullOrWhiteSpace(request.Iban) && !NoticeIbanRule.Allows(request.Iban)) {{ return PanelItemStepResult.Refused(PanelItemObstacle.IbanInvalid);", IbanObstacle],
        ["7.2.13.2|Panel.ProcessAmountBasedRefundInput.OrderItemId"] = [],
        ["7.2.13.2|Panel.ProcessAmountBasedRefundInput.NotReturnedClosureId"] = [],
        ["7.2.14.1|Panel.CorrectAddressInput.ShippingRecipientName"] = ["Required|Modules/Ordering/OrderCorrections.cs#ResolveOrderAddressesAsync( shipping ? new OrderAddressRequest( null, [ProductType.Physical], null, request.RecipientName,"],
        ["7.2.14.1|Panel.CorrectAddressInput.ShippingPhone"] = ["Required|Modules/Ordering/OrderCorrections.cs#ResolveOrderAddressesAsync( shipping ? new OrderAddressRequest( null, [ProductType.Physical], null, request.RecipientName, request.ProvinceId, request.District, request.AddressLine, request.Phone,", "Phone|Modules/Ordering/OrderCorrections.cs#ResolveOrderAddressesAsync( shipping ? new OrderAddressRequest( null, [ProductType.Physical], null, request.RecipientName, request.ProvinceId, request.District, request.AddressLine, request.Phone,"],
        ["7.2.14.2|Panel.CorrectCarrierInput.Carrier"] = [CorrectionShipment, Carrier],
        ["7.2.14.2|Panel.CorrectCarrierInput.TrackingNumber"] = [CorrectionShipment, Tracking],
        ["7.2.14.3|Panel.CorrectTransitionInput.CorrectionReason"] = [ReasonPage, $"Required|{PanelCorrections}#if (request.Reason is not {{ }} reason || !Enum.IsDefined(reason)"],
        ["7.2.14.3|Panel.CorrectTransitionInput.ReasonNote"] = [ReasonPage, $"Required|{PanelCorrections}#(reason == CorrectionReason.Other && string.IsNullOrWhiteSpace(request.ReasonNote))"],

        // 7.2.16 the corporate content form: each record kind's input checks its own fields (a field of two kinds is two lines), and the publish gate names the fields it needs.
        ["7.2.16.1|Panel.SaveContentInput.Name"] = [$"Required|{Inputs}#RequiredWithCeiling(rejections, nameof(OfferingInput.Name), input.Name, ContentRules.TitleMaxLength)", $"TextCap|{Inputs}#RequiredWithCeiling(rejections, nameof(OfferingInput.Name), input.Name, ContentRules.TitleMaxLength)", $"P-27|{Inputs}#RequiredWithCeiling(rejections, nameof(OfferingInput.Name), input.Name, ContentRules.TitleMaxLength)", $"Required|{Inputs}#RequiredWithCeiling(rejections, nameof(BranchInput.Name), input.Name, ContentRules.TitleMaxLength)", $"TextCap|{Inputs}#RequiredWithCeiling(rejections, nameof(BranchInput.Name), input.Name, ContentRules.TitleMaxLength)", $"P-27|{Inputs}#RequiredWithCeiling(rejections, nameof(BranchInput.Name), input.Name, ContentRules.TitleMaxLength)", $"Required|{Gate}#ContentRecordKind.Offering => [ContentGateField.Name]", $"Required|{Gate}#ContentRecordKind.Branch => [ContentGateField.Name, ContentGateField.Province, ContentGateField.District, ContentGateField.AddressLine]"],
        ["7.2.16.1|Panel.SaveContentInput.Title"] = [$"Required|{Inputs}#RequiredWithCeiling(rejections, nameof(PortfolioItemInput.Title), input.Title, ContentRules.TitleMaxLength)", $"TextCap|{Inputs}#RequiredWithCeiling(rejections, nameof(PortfolioItemInput.Title), input.Title, ContentRules.TitleMaxLength)", $"P-27|{Inputs}#RequiredWithCeiling(rejections, nameof(PortfolioItemInput.Title), input.Title, ContentRules.TitleMaxLength)", $"Required|{Inputs}#RequiredWithCeiling(rejections, nameof(CustomPageInput.Title), input.Title, ContentRules.TitleMaxLength)", $"TextCap|{Inputs}#RequiredWithCeiling(rejections, nameof(CustomPageInput.Title), input.Title, ContentRules.TitleMaxLength)", $"P-27|{Inputs}#RequiredWithCeiling(rejections, nameof(CustomPageInput.Title), input.Title, ContentRules.TitleMaxLength)", $"Required|{Gate}#ContentRecordKind.PortfolioItem => [ContentGateField.Title]", $"Required|{Gate}#ContentRecordKind.CustomPage => [ContentGateField.Title]"],
        ["7.2.16.1|Panel.SaveContentInput.Question"] = [$"Required|{Inputs}#RequiredWithCeiling(rejections, nameof(FaqItemInput.Question), input.Question, ContentRules.ShortDescriptionMaxLength)", $"TextCap|{Inputs}#RequiredWithCeiling(rejections, nameof(FaqItemInput.Question), input.Question, ContentRules.ShortDescriptionMaxLength)", $"P-28|{Inputs}#RequiredWithCeiling(rejections, nameof(FaqItemInput.Question), input.Question, ContentRules.ShortDescriptionMaxLength)", FaqGate],
        ["7.2.16.2|Panel.SaveContentInput.ShortDescription"] = [$"P-28|{Inputs}#Ceiling(rejections, nameof(OfferingInput.ShortDescription), Text(input.ShortDescription), ContentRules.ShortDescriptionMaxLength)", $"P-28|{Inputs}#Ceiling(rejections, nameof(PortfolioItemInput.ShortDescription), Text(input.ShortDescription), ContentRules.ShortDescriptionMaxLength)"],
        ["7.2.16.2|Panel.SaveContentInput.ShortIntroduction"] = [$"Required|{Gate}#ContentRecordKind.AboutPage => [ContentGateField.ShortIntroduction]", $"P-29|{Inputs}#Ceiling(rejections, nameof(AboutPageInput.ShortIntroduction), Text(input.ShortIntroduction), ContentRules.ShortIntroductionMaxLength)"],
        ["7.2.16.2|Panel.SaveContentInput.WorkingHours"] = [$"P-28|{Inputs}#Ceiling(rejections, nameof(BranchInput.WorkingHours), Text(input.WorkingHours), ContentRules.ShortDescriptionMaxLength)"],
        ["7.2.16.3|Panel.SaveContentInput.LongText"] = [],
        ["7.2.16.3|Panel.SaveContentInput.Text"] = [],
        ["7.2.16.3|Panel.SaveContentInput.Answer"] = [FaqGate],
        ["7.2.16.7|Panel.SaveContentInput.LinkUrl"] = [$"Url|{Inputs}#Apply(rejections, nameof(AnnouncementInput.LinkUrl), Text(input.LinkUrl), new InvalidUrlAttribute(), FieldCodes.InvalidUrl)"],
        ["7.2.16.7|Panel.SaveContentInput.MapUrl"] = [$"Url|{Inputs}#Apply(rejections, nameof(BranchInput.MapUrl), Text(input.MapUrl), new InvalidUrlAttribute(), FieldCodes.InvalidUrl)"],
        ["7.2.16.9|Panel.SaveContentInput.ImageId"] = ["Image|Pages/Panel/ContentForm.cshtml.cs#RefuseUpload(input.ImageTarget, ErrorCode.InvalidImage, UploadRefusalText(result, surface))", "P-22|Pages/Panel/ContentForm.cshtml.cs#? ProductFormModel.PixelLimitText : UploadChecks.FormatMessage()", "P-22|Pages/Components/UploadComponentModels.cs#{ProductConstants.ImageFormats} yüklenebilir.", "P-23|Pages/Panel/ContentForm.cshtml.cs#return UploadChecks.SizeMessage(surface);", "P-23|Modules/Catalog/ImageUploads.cs#content.LongLength > ProductConstants.ImageMaxBytes", "P-25|Pages/Panel/ContentForm.cshtml.cs#input.Images.Count >= ProductConstants.ImagesPerContentRecordMax"],
        ["7.2.16.9|Panel.SaveContentInput.ImageAltText"] = [], // the cell names the image file rules (7.3.2.18, P-22, P-23, P-25); the alternative text has none.
        ["7.2.16.9|Panel.SaveContentImageRow.AltText"] = [], // as ImageAltText.
        ["7.2.16.10|Panel.SaveContentInput.LinkedProducts"] = [],
        ["7.2.16.10|Panel.SaveContentInput.IsShownOnHomepage"] = [],
        ["7.2.16.10|Panel.SaveContentInput.IsShownInMenu"] = [],
    };

    /// <summary>The place lines: the table above and the place lines of <see cref="FormFieldRuleEvidence"/> (the code of the plain-word rules and the references of the rule cell), united place by place.</summary>
    public static IReadOnlyDictionary<string, string[]> Lines { get; } = FormFieldRuleEvidence.United(Own, places: true);
}
