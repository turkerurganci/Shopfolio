using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure.Errors;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Ordering.Contracts;
using Shopfolio.Modules.AfterSales.Contracts;
using Shopfolio.Modules.Payment.Contracts;
using Shopfolio.Pages.Components;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The small rules of the form field, the address form, the IBAN field and the reason selection (T543 KK1 to KK10; 04 2.12, 7.3).</summary>
public sealed class FormComponentModelTests
{
    private static readonly Guid Ankara = new("00000000-0000-0000-0000-0000000000a1");

    private static readonly Guid Izmir = new("00000000-0000-0000-0000-0000000000a2");

    private static readonly ProvinceChoice[] Provinces = [new(Ankara, "Ankara", Served: false), new(Izmir, "İzmir")];

    private static string[] NamesOf(AddressFormView form) => [.. form.Fields.Select(f => f.Name)];

    // ---- KK1: label, requirement words, message place and tie ----

    [Theory]
    [InlineData(FieldRequirement.Required, "(zorunlu)")]
    [InlineData(FieldRequirement.Optional, "(isteğe bağlı)")]
    [InlineData(FieldRequirement.RequiredForPublish, "(yayın için zorunlu)")]
    public void ARequiredAndAnOptionalField_AreToldApartInWords_KK1(FieldRequirement requirement, string words)
    {
        FieldView field = Fields.Text("Input.Name", "Ad", requirement);

        Assert.Equal(words, field.RequirementText);
        Assert.Equal(3, Enum.GetValues<FieldRequirement>().Select(FieldRequirements.TextOf).Distinct().Count());
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AFieldWithoutALabel_CanNotBeBuilt_KK1(string label)
    {
        Assert.Throws<ArgumentException>(() => Fields.Text("Input.Name", label, FieldRequirement.Required));
        Assert.Throws<ArgumentException>(() => Fields.Email("Input.Email", label, FieldRequirement.Required));
    }

    [Fact]
    public void TheMessageOfAField_IsUnderIt_AndNamedByItsDescribedBy_KK1()
    {
        FieldView field = Fields.Text("Input.Name", "Ad", FieldRequirement.Required, hint: "Kısa yazın.");
        FormErrorSummary summary = new([new FormFieldError("Input_Name", "Ad boş olamaz.")]);

        FieldView bound = Fields.Bind(field, summary);

        Assert.Equal("Input_Name", bound.Id);
        Assert.Equal("Input_Name-message", bound.MessageId);
        Assert.Equal("Input_Name-message Input_Name-hint", bound.DescribedBy);
        Assert.Equal("Ad boş olamaz.", bound.Error!.Text);
        Assert.Equal(MessagePlace.Field, bound.Error.Place);
        Assert.Equal(bound.MessageId, bound.Error.Id);
        Assert.Null(Fields.Bind(Fields.Text("Input.Sku", "Stok", FieldRequirement.Optional), summary).Error);
        Assert.Null(Fields.Text("Input.Sku", "Stok", FieldRequirement.Optional).DescribedBy);
    }

    [Fact]
    public void OnlyTheFirstWrongField_TakesTheFocus_KK1()
    {
        FormErrorSummary summary = new([new FormFieldError("Input_Name", "a"), new FormFieldError("Input_Sku", "b")]);

        IReadOnlyList<FieldView> bound = Fields.BindAll([Fields.Text("Input.Name", "Ad", FieldRequirement.Required), Fields.Text("Input.Sku", "Stok", FieldRequirement.Required)], summary);

        Assert.True(bound[0].Autofocus);
        Assert.False(bound[1].Autofocus);
        Assert.All(bound, f => Assert.True(f.IsInvalid));
    }

    // ---- KK2: sections with their own save buttons ----

    [Fact]
    public void ASectionedScreen_GivesEachSectionItsOwnSaveButtonAndHandler_KK2()
    {
        FormSectionView[] sections =
        [
            new("brand", "Marka", [Fields.Text("Brand.Name", "Marka adı", FieldRequirement.Required)], "Markayı kaydet", "SaveBrand"),
            new("identity", "Kimlik", [Fields.Text("Identity.Title", "Unvan", FieldRequirement.Required)], "Kimliği kaydet", "SaveIdentity"),
        ];

        IReadOnlyList<FormSectionView> built = FormSectionView.Sections(sections);

        Assert.Equal(["SaveBrand", "SaveIdentity"], built.Select(s => s.Handler));
        Assert.Equal(2, built.Select(s => s.SaveLabel).Distinct().Count());
    }

    [Fact]
    public void TwoSectionsThatShareAHandler_OrAreWithoutASaveButton_AreRefused_KK2()
    {
        FieldView[] none = [];

        Assert.Throws<ArgumentException>(() => FormSectionView.Sections([new("a", "A", none, "Kaydet", "Save"), new("b", "B", none, "Kaydet", "Save")]));
        Assert.Throws<ArgumentException>(() => FormSectionView.Sections([new("a", "A", none, "", "Save")]));
        Assert.Throws<ArgumentException>(() => FormSectionView.Sections([new("a", "A", none, "Kaydet", " ")]));
    }

    // ---- KK3: a closed list takes no free text ----

    [Fact]
    public void AClosedListWithoutOptions_CanNotBeBuilt_KK3()
    {
        Assert.Throws<ArgumentException>(() => Fields.ClosedList("Input.Topic", "Konu", FieldRequirement.Required, []));
    }

    [Theory]
    [InlineData("OutOfStock", true)]
    [InlineData("outofstock", false)]
    [InlineData(" OutOfStock", false)]
    [InlineData("Stokta bulunamadı", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void AValue_IsAChoiceOnlyWhenItIsTheValueOfAnOption_KK3(string? value, bool accepted)
    {
        Assert.Equal(accepted, ClosedLists.Accepts(Reasons.CancellationOptions, value));
    }

    [Fact]
    public void ADisabledOption_IsNotAChoice_KK3()
    {
        FieldOption[] options = [new("a", "A", Disabled: true, Note: "kapalı"), new("b", "B")];

        Assert.False(ClosedLists.Accepts(options, "a"));
        Assert.True(ClosedLists.Accepts(options, "b"));
    }

    [Fact]
    public void AReasonTypedAsText_IsNotAReason_KK3()
    {
        ReasonCheck check = Reasons.Check(ReasonKind.FirmCancellation, "Başka bir sebep", "açıklama");

        Assert.Equal([new ReasonFieldError(Reasons.ReasonMember, FieldCodes.InvalidListValue)], check.Errors);
        Assert.False(check.MayContinue);
    }

    [Fact]
    public void AProvinceThatIsNotOnTheList_IsNoChoice_KK3()
    {
        Assert.True(AddressForms.CheckProvince(AddressVariant.Book, Guid.NewGuid(), Provinces).IsNotOnTheList);
        Assert.True(AddressForms.CheckProvince(AddressVariant.Book, null, Provinces).IsMissing);
        Assert.True(AddressForms.CheckProvince(AddressVariant.Book, Guid.Empty, Provinces).IsMissing);
    }

    // ---- KK4: white space is nothing ----

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("    ")]
    public void AReasonOfWhiteSpaceOnly_IsNoReason_KK4(string? reason)
    {
        ReasonCheck check = Reasons.Check(ReasonKind.StatusCorrection, reason, null);

        Assert.Equal([new ReasonFieldError(Reasons.ReasonMember, FieldCodes.FieldRequired)], check.Errors);
    }

    [Fact]
    public void AnExplanationOfWhiteSpaceOnly_IsNoExplanation_KK4()
    {
        ReasonCheck check = Reasons.Check(ReasonKind.FirmCancellation, "  Other ", "   ");

        Assert.Equal([new ReasonFieldError(Reasons.ExplanationMember, FieldCodes.FieldRequired)], check.Errors);
    }

    // ---- KK5: the address form's fields ----

    [Theory]
    [InlineData(AddressVariant.Delivery, new[] { "RecipientName", "ProvinceId", "District", "AddressLine", "Phone" })]
    [InlineData(AddressVariant.Billing, new[] { "RecipientName", "ProvinceId", "District", "AddressLine" })]
    [InlineData(AddressVariant.Book, new[] { "Name", "RecipientName", "ProvinceId", "District", "AddressLine", "Phone" })]
    [InlineData(AddressVariant.PanelDelivery, new[] { "RecipientName", "ProvinceId", "District", "AddressLine", "Phone" })]
    [InlineData(AddressVariant.PanelBilling, new[] { "RecipientName", "ProvinceId", "District", "AddressLine" })]
    public void EveryVariant_HasItsFields_AndNeverAPostalCodeOrAnIdentityNumber_KK5(AddressVariant variant, string[] members)
    {
        AddressFormView form = AddressForms.Create(variant, "Input.", Provinces);

        Assert.Equal(members.Select(m => "Input." + m), NamesOf(form));
        Assert.DoesNotContain(form.Fields, f => f.Name.Contains("Postal", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("Identity", StringComparison.OrdinalIgnoreCase) || f.Name.Contains("Tckn", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(form.Fields.Select(f => f.Name), form.FieldOrder);
    }

    [Fact]
    public void TheOpenAddress_IsOneTextField_TheProvinceAClosedList_TheDistrictFreeText_KK5()
    {
        AddressFormView form = AddressForms.Create(AddressVariant.Delivery, "", Provinces);

        Assert.Equal(FieldKind.TextArea, form.Fields.Single(f => f.Name == "AddressLine").Kind);
        Assert.Equal(FieldKind.ClosedList, form.Fields.Single(f => f.Name == "ProvinceId").Kind);
        Assert.Equal(FieldKind.Text, form.Fields.Single(f => f.Name == "District").Kind);
        Assert.Equal(1, form.Fields.Count(f => f.Name.Contains("Address", StringComparison.Ordinal)));
    }

    [Fact]
    public void TheTelephone_IsRequiredOnTheDeliveryAddress_AndOptionalInTheBook_KK5()
    {
        Assert.Equal(FieldRequirement.Required, AddressForms.Create(AddressVariant.Delivery, "", Provinces).Fields.Single(f => f.Name == "Phone").Requirement);
        Assert.Equal(FieldRequirement.Optional, AddressForms.Create(AddressVariant.Book, "", Provinces).Fields.Single(f => f.Name == "Phone").Requirement);
        Assert.Equal(FieldKind.Telephone, AddressForms.Create(AddressVariant.Book, "", Provinces).Fields.Single(f => f.Name == "Phone").Kind);
    }

    [Fact]
    public void OnlyTheBillingFormOfAPhysicalBasket_OffersTheSameAsShippingBox_KK5()
    {
        AddressFormView billing = AddressForms.Create(AddressVariant.Billing, "Input.Billing", Provinces, offerSameAsShipping: true, sameAsShippingName: "Input.SameAsShipping");
        AddressFormView withoutPhysical = AddressForms.Create(AddressVariant.Billing, "Input.Billing", Provinces);

        Assert.True(billing.OffersSameAsShipping);
        Assert.True(billing.SameAsShippingChecked);
        Assert.Equal("Input_SameAsShipping", billing.SameAsShippingId);
        Assert.False(withoutPhysical.OffersSameAsShipping);
        Assert.Throws<ArgumentException>(() => AddressForms.Create(AddressVariant.Delivery, "", Provinces, offerSameAsShipping: true, sameAsShippingName: "x"));
        Assert.Throws<ArgumentException>(() => AddressForms.Create(AddressVariant.Billing, "", Provinces, offerSameAsShipping: true));
    }

    // ---- KK6: an unserved province ----

    [Fact]
    public void TheDeliveryForm_RefusesAnUnservedProvince_AndSaysWhyAtTheField_KK6()
    {
        ProvinceVerdict verdict = AddressForms.CheckProvince(AddressVariant.Delivery, Ankara, Provinces);

        Assert.False(verdict.IsAccepted);
        Assert.Equal("Bu firma Ankara iline teslimat yapmıyor.", verdict.Reason);
        Assert.True(AddressForms.CheckProvince(AddressVariant.Delivery, Izmir, Provinces).IsAccepted);
    }

    [Theory]
    [InlineData(AddressVariant.Book)]
    [InlineData(AddressVariant.Billing)]
    [InlineData(AddressVariant.PanelBilling)]
    public void TheBookAndTheBillingAddress_KeepAnUnservedProvince_KK6(AddressVariant variant)
    {
        Assert.True(AddressForms.CheckProvince(variant, Ankara, Provinces).IsAccepted);
    }

    [Fact]
    public void ABookEntryInAnUnservedProvince_IsShownButNotSelectable_WithItsReason_KK6()
    {
        AddressBookEntry unserved = new(Guid.NewGuid(), "Ev", "Ali Veli", Ankara, "Ankara", "Çankaya", "Sokak 1", null, DateTimeOffset.UnixEpoch, IsDeliverable: false);
        AddressBookEntry served = unserved with { ProvinceId = Izmir, ProvinceName = "İzmir", IsDeliverable = true, Phone = "5321234567" };

        AddressBookChoice blocked = AddressBookChoice.From(unserved);
        AddressBookChoice open = AddressBookChoice.From(served);

        Assert.False(blocked.Selectable);
        Assert.Equal("Bu firma Ankara iline teslimat yapmıyor.", blocked.Reason);
        Assert.True(blocked.AsksPhone);
        Assert.True(open.Selectable);
        Assert.Null(open.Reason);
        Assert.False(open.AsksPhone);
    }

    [Fact]
    public void TheModulesRefusals_BecomeTheFormsTexts_AtTheFieldsOfTheForm_KK6()
    {
        ModelStateDictionary state = new();

        AddressForms.AddRefusals(
            state,
            "Input.",
            [
                new MembershipFieldError("ShippingRecipientName", ErrorCode.FieldRequired),
                new MembershipFieldError("ShippingProvinceId", ErrorCode.DeliveryProvinceNotServed, "Ankara"),
                new MembershipFieldError("ShippingPhone", ErrorCode.InvalidPhone),
            ]);

        Assert.Equal("Alıcının adı ve soyadı boş olamaz.", state["Input.RecipientName"]!.Errors[0].ErrorMessage);
        Assert.Equal("Bu firma Ankara iline teslimat yapmıyor.", state["Input.ProvinceId"]!.Errors[0].ErrorMessage);
        Assert.Equal("Telefon geçerli bir telefon numarası olmalıdır.", state["Input.Phone"]!.Errors[0].ErrorMessage);
    }

    // ---- KK7: where the IBAN field is ----

    [Theory]
    [InlineData(PaymentMethod.BankTransfer, IbanOpening.Standard, true)]
    [InlineData(PaymentMethod.Card, IbanOpening.Standard, false)]
    [InlineData(PaymentMethod.Card, IbanOpening.CardRefundNotRealized, true)]
    [InlineData(PaymentMethod.Card, IbanOpening.LatePayment, true)]
    public void TheIbanField_IsOnTheTransferLine_AndOnTheCardLinesTwoExceptions_KK7(PaymentMethod method, IbanOpening opening, bool shown)
    {
        Assert.Equal(shown, IbanFields.IsShown(method, opening));
        Assert.Equal(shown, IbanFields.Create(IbanFieldPlace.OrderPage, method, opening, "Input.Iban") is not null);
    }

    [Fact]
    public void ADeclarationOnTheCardLine_HasNoIbanField_EvenWithAnException_KK7()
    {
        Assert.Null(IbanFields.Create(IbanFieldPlace.Declaration, PaymentMethod.Card, IbanOpening.CardRefundNotRealized, "Input.Iban"));
        Assert.NotNull(IbanFields.Create(IbanFieldPlace.Declaration, PaymentMethod.BankTransfer, IbanOpening.Standard, "Input.Iban"));
    }

    [Theory]
    [InlineData(PaymentMethod.BankTransfer, IbanOpening.Standard, FieldRequirement.Required)]
    [InlineData(PaymentMethod.Card, IbanOpening.CardRefundNotRealized, FieldRequirement.Optional)]
    [InlineData(PaymentMethod.Card, IbanOpening.LatePayment, FieldRequirement.Optional)]
    public void TheIbanFieldOnTheOrderPage_IsOptionalOnTheTwoExceptions_AndRequiredOtherwise_KK7(PaymentMethod method, IbanOpening opening, FieldRequirement requirement)
    {
        IbanFieldView view = IbanFields.Create(IbanFieldPlace.OrderPage, method, opening, "Input.Iban")!;

        Assert.Equal(requirement, view.Field.Requirement);
        Assert.Equal(requirement == FieldRequirement.Optional, view.IsOptional);
        Assert.Equal(IbanFields.Label, view.Field.Label);
    }

    [Fact]
    public void AnIban_IsCorrectedFromTheOrderPage_UntilItIsProcessed_KK7()
    {
        IbanFieldView open = IbanFields.Create(IbanFieldPlace.OrderPage, PaymentMethod.BankTransfer, IbanOpening.Standard, "Input.Iban", "TR330006100519786457841326")!;
        IbanFieldView processed = IbanFields.Create(IbanFieldPlace.OrderPage, PaymentMethod.BankTransfer, IbanOpening.Standard, "Input.Iban", "TR330006100519786457841326", processed: true)!;

        Assert.True(open.Editable);
        Assert.False(open.Field.ReadOnly);
        Assert.Null(open.Field.Hint);
        Assert.False(processed.Editable);
        Assert.True(processed.Field.ReadOnly);
        Assert.Equal(IbanFields.ProcessedHint, processed.Field.Hint);
        Assert.Equal("TR330006100519786457841326", processed.Field.Value);
    }

    [Fact]
    public void TheCustomersIbanField_CarriesThePrivacyNoticeLink_AndThePanelsIsReadOnlyWithoutOne_KK7()
    {
        IbanFieldView customer = IbanFields.Create(IbanFieldPlace.Declaration, PaymentMethod.BankTransfer, IbanOpening.Standard, "Input.Iban")!;
        IbanFieldView panel = IbanFields.Create(IbanFieldPlace.Panel, PaymentMethod.BankTransfer, IbanOpening.Standard, "Input.Iban", "TR330006100519786457841326")!;

        Assert.Equal("/aydinlatma-metni", customer.NoticeHref);
        Assert.Equal("Aydınlatma metni", customer.NoticeText);
        Assert.False(panel.Editable);
        Assert.True(panel.Field.ReadOnly);
        Assert.Null(panel.NoticeHref);
    }

    // ---- KK8: the reason selection ----

    [Fact]
    public void TheCancellationList_IsThe03List_AndEveryReasonOfTheEnumHasATextHere_KK8()
    {
        Assert.Equal(Enum.GetNames<CancellationReason>().OrderBy(n => n), Reasons.CancellationOptions.Select(o => o.Value).OrderBy(n => n));
        Assert.Equal(Enum.GetNames<CorrectionReason>().OrderBy(n => n), Reasons.CorrectionOptions.Select(o => o.Value).OrderBy(n => n));
        Assert.All(Reasons.CancellationOptions.Concat(Reasons.CorrectionOptions), o => Assert.False(string.IsNullOrWhiteSpace(o.Text)));
    }

    [Fact]
    public void WithoutAReason_NeitherTheConfirmationOpensNorTheCorrectionIsSent_KK8()
    {
        Assert.False(Reasons.Check(ReasonKind.FirmCancellation, null, null).MayContinue);
        Assert.False(Reasons.Check(ReasonKind.StatusCorrection, "", null).MayContinue);
        Assert.True(Reasons.Check(ReasonKind.FirmCancellation, "AgreedWithBuyer", null).MayContinue);
        Assert.True(Reasons.Check(ReasonKind.StatusCorrection, "WrongOrder", null).MayContinue);
    }

    [Fact]
    public void OtherMakesTheExplanationRequired_KK8()
    {
        Assert.Equal(FieldRequirement.Required, Reasons.Create(ReasonKind.FirmCancellation, "Input.", "Other").Explanation.Requirement);
        Assert.Equal(FieldRequirement.Optional, Reasons.Create(ReasonKind.FirmCancellation, "Input.", "OutOfStock").Explanation.Requirement);
        Assert.True(Reasons.Check(ReasonKind.StatusCorrection, "Other", "Sebep burada.").IsValid);
        Assert.False(Reasons.Check(ReasonKind.StatusCorrection, "Other", null).IsValid);
    }

    [Fact]
    public void TheReasonErrors_GoIntoTheModelStateByTheirCodes_KK8()
    {
        ModelStateDictionary state = new();

        Reasons.Check(ReasonKind.FirmCancellation, "Other", " ").AddTo(state, "Input.");

        Assert.Single(state["Input.Explanation"]!.Errors);
        Assert.StartsWith(FieldCodes.FieldRequired, state["Input.Explanation"]!.Errors[0].ErrorMessage, StringComparison.Ordinal);
    }

    [Fact]
    public void TheLegalWarning_ShowsBeforeTheConfirmation_ForOutOfStockOnly_KK8()
    {
        Assert.True(Reasons.Create(ReasonKind.FirmCancellation, "Input.", "OutOfStock").LegalWarningBeforeConfirmation);
        Assert.False(Reasons.Create(ReasonKind.FirmCancellation, "Input.", "Other").LegalWarningBeforeConfirmation);
        Assert.False(Reasons.Create(ReasonKind.StatusCorrection, "Input.", "OutOfStock").LegalWarningBeforeConfirmation);
    }

    // ---- KK9: the limit blocks only its own operation; the neutral language ----

    [Fact]
    public void TheNeutralLimitMessage_SaysNoNumberNoTimeNoAxis_AndIsAWarningInPlace_KK9()
    {
        MessageView message = Messages.NeutralLimit("limit");

        Assert.Equal(MessagePlace.InPlace, message.Place);
        Assert.DoesNotContain(message.Text, char.IsDigit);
        Assert.DoesNotContain("dakika", message.Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("kalan", message.Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("kilit", message.Text, StringComparison.OrdinalIgnoreCase);
    }

    // ---- KK10: a limit's value is the parameter's, not the component's ----

    [Fact]
    public void ALimitedField_TakesItsLimitFromTheCaller_AndTheMessageNamesThePValue_KK10()
    {
        FieldView field = Fields.Text("Input.Name", "Ürün adı", FieldRequirement.Required, maxLength: Shopfolio.Infrastructure.ProductConstants.ProductNameMaxLength);
        string message = FieldMessageCatalog.Build(FieldCodes.TextTooLong, field.Label, new TextTooLongAttribute(Shopfolio.Infrastructure.ProductConstants.ProductNameMaxLength).ParameterText);

        Assert.Equal(Shopfolio.Infrastructure.ProductConstants.ProductNameMaxLength, field.MaxLength);
        Assert.Contains(Shopfolio.Infrastructure.ProductConstants.ProductNameMaxLength.ToString(System.Globalization.CultureInfo.InvariantCulture), message, StringComparison.Ordinal);
        Assert.Throws<ArgumentOutOfRangeException>(() => Fields.Text("Input.Name", "Ad", FieldRequirement.Required, maxLength: 0));
    }
}
