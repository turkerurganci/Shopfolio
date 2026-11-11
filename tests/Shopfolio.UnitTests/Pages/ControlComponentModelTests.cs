using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Infrastructure.Validation;
using Shopfolio.Modules.Catalog.Contracts.Coupons;
using Shopfolio.Modules.Catalog.Contracts.Pricing;
using Shopfolio.Pages.Components;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The small rules of the date field, the coupon field, the confirmation boxes and the upload views (T544 KK1 to KK10; 04 OB-16 to OB-19, 2.12.5 to 2.12.8).</summary>
public sealed class ControlComponentModelTests
{
    private static readonly DateOnly Today = new(2026, 11, 11);

    private static readonly DateOnly ShippedDay = new(2026, 11, 4);

    private static readonly EventDateLimit ShippedLimit = new(ShippedDay, "kargoya verildiği gün");

    private static readonly byte[] PngBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00];

    private static readonly byte[] TextBytes = [0x68, 0x65, 0x6C, 0x6C, 0x6F];

    // ---- KK1: an event date is in the past; the lower limit says why ----

    [Theory]
    [InlineData("2026-11-10")]
    [InlineData("2026-11-11")]
    [InlineData("2026-11-04")]
    public void AnEventDateInThePast_OrToday_IsAccepted_KK1(string text)
    {
        EventDateVerdict verdict = DateFields.CheckEvent(text, Today, ShippedLimit);

        Assert.True(verdict.IsAccepted);
        Assert.Equal(DateFields.Parse(text), verdict.Date);
        Assert.Null(verdict.Reason);
    }

    [Fact]
    public void AnEventDateAfterToday_IsRefusedForBeingInTheFuture_KK1()
    {
        EventDateVerdict verdict = DateFields.CheckEvent("2026-11-12", Today, ShippedLimit);

        Assert.Equal(EventDateOutcome.InTheFuture, verdict.Outcome);
        Assert.Equal(DateFields.FutureMessage, verdict.Reason);
    }

    [Fact]
    public void AnEventDateBelowTheLowerLimit_IsRefusedWithTheLimitAsItsReason_KK1()
    {
        EventDateVerdict verdict = DateFields.CheckEvent("2026-11-03", Today, ShippedLimit);

        Assert.Equal(EventDateOutcome.BelowLowerBound, verdict.Outcome);
        Assert.Contains(ShippedLimit.Name, verdict.Reason, StringComparison.Ordinal);
        Assert.Contains(DisplayFormat.Date(ShippedDay), verdict.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void ADateWithNoLowerLimit_IsOnlyRefusedForTheFuture_KK1()
    {
        Assert.True(DateFields.CheckEvent("2020-01-01", Today).IsAccepted);
        Assert.Equal(EventDateOutcome.InTheFuture, DateFields.CheckEvent("2026-11-12", Today).Outcome);
    }

    [Theory]
    [InlineData(null, EventDateOutcome.Missing)]
    [InlineData("", EventDateOutcome.Missing)]
    [InlineData("   ", EventDateOutcome.Missing)]
    [InlineData("11.11.2026", EventDateOutcome.Malformed)]
    [InlineData("2026-02-30", EventDateOutcome.Malformed)]
    [InlineData("yarın", EventDateOutcome.Malformed)]
    public void AMissingOrMalformedDate_IsToldApart_KK1(string? text, EventDateOutcome expected)
    {
        Assert.Equal(expected, DateFields.CheckEvent(text, Today, ShippedLimit).Outcome);
    }

    [Fact]
    public void TheRefusalOfAnEventDate_GoesIntoTheModelStateAtItsField_KK1()
    {
        ModelStateDictionary state = new();

        DateFields.AddRefusal(state, "Input.DeliveryDate", "Teslim tarihi", DateFields.CheckEvent("2026-11-12", Today));
        DateFields.AddRefusal(state, "Input.ArrivalDate", "Ulaşma tarihi", DateFields.CheckEvent(null, Today));
        DateFields.AddRefusal(state, "Input.OtherDate", "Tarih", DateFields.CheckEvent("2026-11-10", Today));

        Assert.Equal(DateFields.FutureMessage, state["Input.DeliveryDate"]!.Errors[0].ErrorMessage);
        Assert.StartsWith(FieldCodes.FieldRequired, state["Input.ArrivalDate"]!.Errors[0].ErrorMessage, StringComparison.Ordinal);
        Assert.Null(state["Input.OtherDate"]);
    }

    [Fact]
    public void TheDateField_OffersNothingAfterTodayAndNothingBeforeTheLimit_KK1()
    {
        FieldView field = DateFields.EventDate("Input.DeliveryDate", "Teslim tarihi", FieldRequirement.Required, Today, "2026-11-08", ShippedLimit);

        Assert.Equal(FieldKind.Date, field.Kind);
        Assert.Equal("date", field.InputType);
        Assert.Equal(DateFields.Format(Today), field.Max);
        Assert.Equal(DateFields.Format(ShippedDay), field.Min);
        Assert.Equal("2026-11-08", field.Value);
    }

    // ---- KK2: the sequence question is a field of the form ----

    [Fact]
    public void TheSequenceQuestion_IsAskedOnlyWhenTheDateIsADeclarationDay_AsARequiredClosedList_KK2()
    {
        DateOnly declared = new(2026, 11, 9);
        DateOnly[] declarations = [declared];

        FieldView? asked = DateFields.SequenceQuestion("Input.Sequence", declared, declarations);
        FieldView? notAsked = DateFields.SequenceQuestion("Input.Sequence", new DateOnly(2026, 11, 8), declarations);
        FieldView? noDate = DateFields.SequenceQuestion("Input.Sequence", null, declarations);

        Assert.NotNull(asked);
        Assert.Equal(FieldKind.ClosedList, asked.Kind);
        Assert.Equal(FieldRequirement.Required, asked.Requirement);
        Assert.Equal(2, asked.Options.Count);
        Assert.Null(notAsked);
        Assert.Null(noDate);
    }

    [Theory]
    [InlineData(null, SequenceOutcome.Missing, null)]
    [InlineData("", SequenceOutcome.Missing, null)]
    [InlineData("Whenever", SequenceOutcome.NotOnTheList, null)]
    [InlineData(DateFields.BeforeDeclaration, SequenceOutcome.Answered, true)]
    [InlineData(DateFields.AfterDeclaration, SequenceOutcome.Answered, false)]
    public void TheSequenceAnswer_IsRequiredAndOneOfTwo_KK2(string? answer, SequenceOutcome outcome, bool? before)
    {
        DateOnly declared = new(2026, 11, 9);

        SequenceVerdict verdict = DateFields.CheckSequence(declared, [declared], answer);

        Assert.Equal(outcome, verdict.Outcome);
        Assert.Equal(before, verdict.DeliveredBeforeDeclaration);
        Assert.Equal(outcome == SequenceOutcome.Answered, verdict.MayContinue);
    }

    [Fact]
    public void ADateThatIsNoDeclarationDay_AsksNothingAndMayContinue_KK2()
    {
        SequenceVerdict verdict = DateFields.CheckSequence(Today, [new DateOnly(2026, 11, 9)], null);

        Assert.Equal(SequenceOutcome.NotAsked, verdict.Outcome);
        Assert.True(verdict.MayContinue);
    }

    // ---- KK3: the range takes a future date; its ends are T378 k7's ----

    [Fact]
    public void ADiscountRange_TakesAFutureDate_KK3()
    {
        DateRangeVerdict verdict = DateFields.CheckRange("2026-12-01", "2026-12-31", Today, DateRangePolicy.FutureAllowed, startRequired: true, endRequired: true);

        Assert.True(verdict.IsAccepted);
    }

    [Fact]
    public void AReportFilterRange_RefusesAFutureDate_KK3()
    {
        Assert.Equal(DateRangeOutcome.EndInTheFuture, DateFields.CheckRange("2026-11-01", "2026-11-12", Today, DateRangePolicy.NoFuture, true, true).Outcome);
        Assert.Equal(DateRangeOutcome.StartInTheFuture, DateFields.CheckRange("2026-11-12", "2026-11-13", Today, DateRangePolicy.NoFuture, true, true).Outcome);
        Assert.True(DateFields.CheckRange("2026-11-01", "2026-11-11", Today, DateRangePolicy.NoFuture, true, true).IsAccepted);
    }

    [Fact]
    public void ARangeWhoseEndIsBeforeItsStart_IsRefused_ButAOneDayRangeIsNot_KK3()
    {
        Assert.Equal(DateRangeOutcome.EndBeforeStart, DateFields.CheckRange("2026-12-10", "2026-12-09", Today, DateRangePolicy.FutureAllowed, true, true).Outcome);
        Assert.True(DateFields.CheckRange("2026-12-10", "2026-12-10", Today, DateRangePolicy.FutureAllowed, true, true).IsAccepted);
    }

    [Theory]
    [InlineData(null, null, true)]
    [InlineData("2026-12-01", null, true)]
    [InlineData(null, "2026-12-31", true)]
    [InlineData("2026-12-01", "2026-12-31", true)]
    [InlineData("2026-12-31", "2026-12-01", false)]
    public void AnAnnouncementsTwoDates_AreOptional_KK3(string? start, string? end, bool accepted)
    {
        Assert.Equal(accepted, DateFields.CheckRange(start, end, Today, DateRangePolicy.FutureAllowed, startRequired: false, endRequired: false).IsAccepted);
    }

    [Fact]
    public void ARequiredDateOfTheRange_IsToldMissing_KK3()
    {
        Assert.Equal(DateRangeOutcome.StartMissing, DateFields.CheckRange(null, "2026-12-01", Today, DateRangePolicy.FutureAllowed, true, true).Outcome);
        Assert.Equal(DateRangeOutcome.EndMissing, DateFields.CheckRange("2026-12-01", null, Today, DateRangePolicy.FutureAllowed, true, true).Outcome);
        Assert.Equal(DateRangeOutcome.StartMalformed, DateFields.CheckRange("x", "2026-12-01", Today, DateRangePolicy.FutureAllowed, true, true).Outcome);
        Assert.Equal(DateRangeOutcome.EndMalformed, DateFields.CheckRange("2026-12-01", "x", Today, DateRangePolicy.FutureAllowed, true, true).Outcome);
    }

    [Fact]
    public void TheEndsOfARange_AreTheStartDaysMidnightAndTheNextDaysMidnight_ThroughTurkeyTime_KK3()
    {
        DateRangeInstants instants = DateFields.InstantsOf(DateFields.CheckRange("2026-12-01", "2026-12-31", Today, DateRangePolicy.FutureAllowed, true, true));

        Assert.Equal(TurkeyTime.StartOf(new DateOnly(2026, 12, 1)), instants.From);
        Assert.Equal(TurkeyTime.EndOfRange(new DateOnly(2026, 12, 31)), instants.Until);
        Assert.Equal(TurkeyTime.StartOf(new DateOnly(2027, 1, 1)), instants.Until);
        Assert.True(TurkeyTime.IsWithinRange(TurkeyTime.EndOfRange(new DateOnly(2026, 12, 31)).AddMinutes(-1), new DateOnly(2026, 12, 1), new DateOnly(2026, 12, 31)));
    }

    [Fact]
    public void ADateLeftOutOfAnOptionalRange_GivesNoInstant_KK3()
    {
        DateRangeInstants instants = DateFields.InstantsOf(DateFields.CheckRange(null, "2026-12-31", Today, DateRangePolicy.FutureAllowed, false, false));

        Assert.Null(instants.From);
        Assert.NotNull(instants.Until);
        Assert.Throws<ArgumentException>(() => DateFields.InstantsOf(DateFields.CheckRange("2026-12-31", "2026-12-01", Today, DateRangePolicy.FutureAllowed, true, true)));
    }

    [Fact]
    public void TheRangesRefusals_GoIntoTheModelStateAtTheirFields_KK3()
    {
        ModelStateDictionary state = new();

        DateFields.AddRefusal(state, "Input.Start", "Input.End", DateFields.CheckRange("2026-12-10", "2026-12-09", Today, DateRangePolicy.FutureAllowed, true, true));

        Assert.Null(state["Input.Start"]);
        Assert.StartsWith(FieldCodes.InvalidDateRange, state["Input.End"]!.Errors[0].ErrorMessage, StringComparison.Ordinal);
    }

    // ---- KK4: a closed row; the code ignores the case ----

    [Fact]
    public void TheCouponField_OpensAsAClosedRow_KK4()
    {
        CouponFieldView view = CouponFields.Closed("Input.Coupon");

        Assert.Equal(CouponFieldMode.Closed, view.Mode);
        Assert.True(view.IsClosedRow);
        Assert.Equal("Kupon kodum var", CouponFields.ClosedRowText);
        Assert.Null(view.CodeField!.Error);
    }

    [Theory]
    [InlineData("YAZ20", "yaz20", true)]
    [InlineData(" Yaz20 ", "YAZ20", true)]
    [InlineData("KIŞ", "kiş", true)]
    [InlineData("İNDİRİM", "indirim", true)]
    [InlineData("YAZ20", "YAZ21", false)]
    [InlineData("", "YAZ20", false)]
    [InlineData(null, null, false)]
    public void TwoCodes_AreTheSameIgnoringTheCase_KK4(string? first, string? second, bool same)
    {
        Assert.Equal(same, CouponFields.SameCode(first, second));
    }

    // ---- KK5: the applied code, two buttons, one code ----

    [Fact]
    public void TheAppliedCode_ShowsItsNameAndTheAmountItTakesOff_WithTwoButtons_KK5()
    {
        CouponFieldView view = CouponFields.Applied("YAZ20", 50m, "Input.Coupon");

        Assert.Equal(CouponFieldMode.Applied, view.Mode);
        Assert.Equal("YAZ20", view.AppliedCode);
        Assert.Equal("-50 TL", view.DiscountText);
        Assert.True(view.OffersChangeAndRemove);
        Assert.Equal("Değiştir", CouponFields.ChangeLabel);
        Assert.Equal("Kaldır", CouponFields.RemoveLabel);
    }

    [Fact]
    public void TheAcceptedAnswer_IsTheAppliedField_AndANewCodeReplacesTheOldOne_KK5()
    {
        CouponApplication first = CouponApplication.Accepted(Guid.NewGuid(), "YAZ20", 200m, [new ItemAmounts(200m, 50m, 150m, 25m, 125m)]);
        CouponApplication second = CouponApplication.Accepted(Guid.NewGuid(), "KIS10", 200m, [new ItemAmounts(200m, 20m, 180m, 30m, 150m)]);

        CouponFieldView view = CouponFields.From(first, "Input.Coupon", "yaz20");
        CouponFieldView replaced = CouponFields.From(second, "Input.Coupon", "kis10");

        Assert.Equal("YAZ20", view.AppliedCode);
        Assert.Equal("-50 TL", view.DiscountText);
        Assert.Equal("KIS10", replaced.AppliedCode);
        Assert.Equal("-20 TL", replaced.DiscountText);
    }

    [Fact]
    public void ADiscountThatIsNotPositive_CanNotBeShownAsApplied_KK5()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CouponFields.Applied("YAZ20", 0m, "Input.Coupon"));
        Assert.Throws<ArgumentException>(() => CouponFields.Applied(" ", 10m, "Input.Coupon"));
    }

    // ---- KK6: two messages, the limit closes the field, the code is applied again ----

    [Fact]
    public void AnInvalidCodeAndACodeUnderTheMinimum_AreToldWithTwoSeparateMessages_KK6()
    {
        string invalid = CouponFields.RefusalText(CouponOutcome.Rejected);
        string below = CouponFields.RefusalText(CouponOutcome.BelowMinimumBasket, 500m);

        Assert.NotEqual(invalid, below);
        Assert.Contains("500 TL", below, StringComparison.Ordinal);
        Assert.DoesNotContain("TL", invalid, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => CouponFields.RefusalText(CouponOutcome.BelowMinimumBasket));
        Assert.Throws<ArgumentOutOfRangeException>(() => CouponFields.RefusalText(CouponOutcome.AttemptLimitExceeded));
    }

    [Fact]
    public void ARefusedCode_StaysTypedInTheOpenFieldWithItsMessageUnderIt_KK6()
    {
        CouponFieldView view = CouponFields.From(CouponApplication.Rejected(200m), "Input.Coupon", "Yanlis");

        Assert.Equal(CouponFieldMode.Open, view.Mode);
        Assert.Equal("Yanlis", view.CodeField!.Value);
        Assert.Equal(CouponFields.RejectedMessage, view.CodeField.Error!.Text);
        Assert.Equal(MessagePlace.Field, view.CodeField.Error.Place);
    }

    [Fact]
    public void TheMinimumRefusal_NamesTheAmountAtTheField_KK6()
    {
        CouponFieldView view = CouponFields.From(CouponApplication.BelowMinimum(200m, 500m), "Input.Coupon", "YAZ20");

        Assert.Equal(CouponFields.BelowMinimumText(500m), view.CodeField!.Error!.Text);
    }

    [Fact]
    public void TheLimit_ClosesTheFieldAndSaysOnlyTheNeutralSentence_KK6()
    {
        CouponFieldView view = CouponFields.From(CouponApplication.LimitExceeded(), "Input.Coupon", "YAZ20");

        Assert.Equal(CouponFieldMode.LimitClosed, view.Mode);
        Assert.False(view.TakesACode);
        Assert.Null(view.CodeField);
        Assert.Equal(NeutralMessages.LimitExceeded, view.LimitMessage!.Text);
    }

    [Fact]
    public void ACustomerWhoComesBackFromTheSignIn_HasTheCodeAppliedAgain_OrToldItIsInvalid_KK6()
    {
        CouponApplication accepted = CouponApplication.Accepted(Guid.NewGuid(), "YAZ20", 200m, [new ItemAmounts(200m, 50m, 150m, 25m, 125m)]);

        Assert.Equal(CouponFieldMode.Applied, CouponFields.AfterSignIn("Input.Coupon", "yaz20", accepted).Mode);
        CouponFieldView refused = CouponFields.AfterSignIn("Input.Coupon", "yaz20", CouponApplication.Rejected(200m));
        Assert.Equal(CouponFieldMode.Open, refused.Mode);
        Assert.Equal(CouponFields.RejectedMessage, refused.CodeField!.Error!.Text);
        Assert.Equal(CouponFieldMode.Closed, CouponFields.AfterSignIn("Input.Coupon", null, null).Mode);
        Assert.Equal(CouponFieldMode.LimitClosed, CouponFields.AfterSignIn("Input.Coupon", "yaz20", CouponApplication.LimitExceeded()).Mode);
    }

    // ---- KK7: how many boxes, none ticked, where they stand ----

    [Fact]
    public void TheBoxes_AreTwoFixedOnesAndTheDigitalAndServiceOnesByTheBasket_KK7()
    {
        ConsentBasket digital = new(["Dijital kitap"], []);
        ConsentBasket service = new([], ["Kurulum"]);
        ConsentBasket both = new(["Dijital kitap"], ["Kurulum"]);

        Assert.Equal([ConsentKind.PreInformation, ConsentKind.DistanceSales], ConsentBoxes.RequiredFor(ConsentBasket.Physical));
        Assert.Equal([ConsentKind.PreInformation, ConsentKind.DistanceSales, ConsentKind.Digital], ConsentBoxes.RequiredFor(digital));
        Assert.Equal([ConsentKind.PreInformation, ConsentKind.DistanceSales, ConsentKind.Service], ConsentBoxes.RequiredFor(service));
        Assert.Equal(4, ConsentBoxes.RequiredFor(both).Count);
    }

    [Fact]
    public void NoBoxComesTicked_WhenTheStepOpens_KK7()
    {
        ConsentBoxesView view = ConsentBoxes.Create("Input.", new ConsentBasket(["Dijital kitap"], ["Kurulum"]));

        Assert.Equal(4, view.Boxes.Count);
        Assert.All(view.Boxes, box => Assert.False(box.IsTicked));
        Assert.All(view.Boxes, box => Assert.Null(box.Error));
    }

    [Fact]
    public void TheExtraBoxes_NameTheirItems_AndSpeakInThePluralForManyItems_KK7()
    {
        string one = ConsentBoxes.TextOf(ConsentKind.Digital, new ConsentBasket(["Dijital kitap"], []));
        string many = ConsentBoxes.TextOf(ConsentKind.Digital, new ConsentBasket(["Dijital kitap", "Ses paketi"], []));
        string transfer = ConsentBoxes.TextOf(ConsentKind.Digital, new ConsentBasket(["Dijital kitap"], [], BankTransfer: true));
        string service = ConsentBoxes.TextOf(ConsentKind.Service, new ConsentBasket([], ["Kurulum"]));

        Assert.Contains("“Dijital kitap”", one, StringComparison.Ordinal);
        Assert.Contains("bu üründe", one, StringComparison.Ordinal);
        Assert.Contains("“Ses paketi”", many, StringComparison.Ordinal);
        Assert.Contains("bu ürünlerde", many, StringComparison.Ordinal);
        Assert.DoesNotContain("hemen", transfer, StringComparison.Ordinal);
        Assert.NotEqual(one, transfer);
        Assert.Contains("“Kurulum”", service, StringComparison.Ordinal);
    }

    [Fact]
    public void TheBoxes_StandUnderTheLegalTextsAndDirectlyAboveTheButton_KK7()
    {
        Assert.True(ConsentBoxes.IsPlacedRight(ConsentBoxes.SectionOrder));
        Assert.False(ConsentBoxes.IsPlacedRight([ConfirmPart.LegalTexts, ConfirmPart.WithdrawalSummary, ConfirmPart.ConfirmButton, ConfirmPart.Boxes]));
        Assert.False(ConsentBoxes.IsPlacedRight([ConfirmPart.Boxes, ConfirmPart.LegalTexts, ConfirmPart.WithdrawalSummary, ConfirmPart.ConfirmButton]));
        Assert.False(ConsentBoxes.IsPlacedRight([ConfirmPart.LegalTexts, ConfirmPart.Boxes, ConfirmPart.WithdrawalSummary, ConfirmPart.ConfirmButton]));
        Assert.False(ConsentBoxes.IsPlacedRight([ConfirmPart.LegalTexts, ConfirmPart.WithdrawalSummary, ConfirmPart.Boxes]));
    }

    // ---- KK8: a missing box, the marks come off, three boxes that do not exist ----

    [Fact]
    public void AMissingBox_IsListedInTheOrderTheBoxesAreDrawn_AndMarkedWithItsCode_KK8()
    {
        ConsentBasket basket = new(["Dijital kitap"], []);
        ConsentSent sent = new(PreInformation: true, DistanceSales: false, Digital: false, Service: false);
        ModelStateDictionary state = new();

        IReadOnlyList<ConsentKind> missing = ConsentBoxes.Missing(basket, sent);
        ConsentBoxes.AddRefusals(state, "Input.", missing);
        FormErrorSummary summary = FormErrorSummary.From(state, ConsentBoxes.FormOrder("Input.", basket));

        Assert.Equal([ConsentKind.DistanceSales, ConsentKind.Digital], missing);
        Assert.Equal("Input_AcceptDistanceSales", summary.FirstFieldId);
        Assert.StartsWith(FieldCodes.CheckboxNotAccepted, state["Input.AcceptDistanceSales"]!.Errors[0].ErrorMessage, StringComparison.Ordinal);
        Assert.Empty(ConsentBoxes.Missing(basket, new ConsentSent(true, true, true, false)));
    }

    [Fact]
    public void ASentForm_KeepsItsTicks_AndAWrongBoxCarriesItsMessage_KK8()
    {
        ConsentBasket basket = ConsentBasket.Physical;
        FormErrorSummary summary = new([new FormFieldError("Input_AcceptDistanceSales", "Sözleşme kutusu onaylanmalıdır.")]);

        ConsentBoxesView view = ConsentBoxes.Create("Input.", basket, new ConsentSent(true, false, false, false), ConsentReset.None, summary);

        Assert.True(view.Boxes[0].IsTicked);
        Assert.False(view.Boxes[1].IsTicked);
        Assert.Null(view.Boxes[0].Error);
        Assert.Equal("Sözleşme kutusu onaylanmalıdır.", view.Boxes[1].Error!.Text);
        Assert.Equal(view.Boxes[1].MessageId, view.Boxes[1].Error!.Id);
    }

    [Fact]
    public void AChangedTextVersion_TakesTheMarksOffEveryBox_KK8()
    {
        ConsentReset reset = ConsentBoxes.ResetOf(new ConsentState("ps-3,dss-2", "abc"), new ConsentState("ps-4,dss-2", "abc"));
        ConsentSent kept = ConsentBoxes.AfterReset(new ConsentSent(true, true, true, true), reset);

        Assert.Equal(ConsentReset.TextVersionChanged, reset);
        Assert.Equal(new ConsentSent(false, false, false, false), kept);
    }

    [Fact]
    public void AChangedSummaryFingerprint_EmptiesTheTwoLegalBoxes_KK8()
    {
        ConsentReset reset = ConsentBoxes.ResetOf(new ConsentState("ps-3,dss-2", "abc"), new ConsentState("ps-3,dss-2", "xyz"));
        ConsentSent kept = ConsentBoxes.AfterReset(new ConsentSent(true, true, true, true), reset);

        Assert.Equal(ConsentReset.SummaryChanged, reset);
        Assert.False(kept.PreInformation);
        Assert.False(kept.DistanceSales);
        Assert.True(kept.Digital);
        Assert.True(kept.Service);
    }

    [Fact]
    public void WhenNothingChanged_OrTheStepIsFresh_NoMarkComesOff_KK8()
    {
        ConsentState same = new("ps-3,dss-2", "abc");
        ConsentSent ticked = new(true, true, true, true);

        Assert.Equal(ConsentReset.None, ConsentBoxes.ResetOf(same, same));
        Assert.Equal(ConsentReset.None, ConsentBoxes.ResetOf(null, same));
        Assert.Equal(ticked, ConsentBoxes.AfterReset(ticked, ConsentReset.None));
    }

    [Fact]
    public void ThereIsNoConsentBox_ForPersonalDataAgeOrMarketing_KK8()
    {
        string[] kinds = [.. Enum.GetNames<ConsentKind>()];

        Assert.Equal(4, kinds.Length);
        Assert.DoesNotContain(kinds, k => k.Contains("Kvkk", StringComparison.OrdinalIgnoreCase) || k.Contains("Age", StringComparison.OrdinalIgnoreCase) || k.Contains("Marketing", StringComparison.OrdinalIgnoreCase));
        ConsentBoxesView view = ConsentBoxes.Create("Input.", new ConsentBasket(["Dijital kitap"], ["Kurulum"]));
        Assert.DoesNotContain(view.Boxes, b => b.Text.Contains("kişisel", StringComparison.OrdinalIgnoreCase) || b.Text.Contains("18", StringComparison.Ordinal) || b.Text.Contains("pazarlama", StringComparison.OrdinalIgnoreCase));
    }

    // ---- KK9: a gallery with a hand-set order and a main mark of its own ----

    private static GalleryImage Image(int order, bool main = false) => new(Guid.NewGuid(), $"/img/{order.ToString(System.Globalization.CultureInfo.InvariantCulture)}.png", null, main, order);

    [Fact]
    public void TheGalleryRows_FollowTheOrderTheFirmGave_AndTiesKeepTheirPlace_KK9()
    {
        GalleryImage a = Image(3);
        GalleryImage b = Image(1);
        GalleryImage c = Image(1);
        GalleryImage d = Image(2);

        IReadOnlyList<GalleryImage> ordered = GalleryUploads.InOrder([a, b, c, d]);

        Assert.Equal([b.ImageId, c.ImageId, d.ImageId, a.ImageId], ordered.Select(i => i.ImageId));
        Assert.Equal([1, 2, 3, 4], GalleryUploads.Renumber([a, b, c, d]).Select(i => i.Order));
    }

    [Fact]
    public void TheMainMark_IsIndependentOfTheOrder_KK9()
    {
        GalleryImage first = Image(1);
        GalleryImage second = Image(2);
        GalleryImage marked = Image(3, main: true);

        GalleryUploadView view = GalleryUploads.Create(UploadSide.Panel, UploadSurface.ProductGallery, "Input.", "Lamba", [first, second, marked]);

        Assert.Equal(marked.ImageId, view.MainImageId);
        Assert.Equal(marked.ImageId, view.Rows[2].Image.ImageId);
        Assert.Equal(marked.ImageId, GalleryUploads.MainOf([first, second, marked], first.ImageId));
        Assert.Equal(first.ImageId, GalleryUploads.MainOf([first, second], first.ImageId));
        Assert.Equal(first.ImageId, GalleryUploads.MainOf([first, second], null));
        Assert.Null(GalleryUploads.MainOf([], null));
    }

    [Fact]
    public void TheMainChoice_IsOneNameForTheWholeGallery_SoTwoImagesAreNeverBothMain_KK9()
    {
        GalleryUploadView view = GalleryUploads.Create(UploadSide.Panel, UploadSurface.ContentGallery, "Input.", "Hakkımızda", [Image(1), Image(2)]);

        Assert.Equal("Input.MainImageId", view.MainName);
        Assert.Equal("Input.Images[0].Order", view.Rows[0].OrderName);
        Assert.Equal("Input.Images[1].AltText", view.Rows[1].AltName);
    }

    [Fact]
    public void AnEmptyAlternativeText_IsReplacedByTheSystemsText_AndTheFieldSaysSo_KK9()
    {
        GalleryImage empty = Image(1) with { AltText = "  " };
        GalleryImage written = Image(2) with { AltText = "Mavi lamba" };

        GalleryUploadView view = GalleryUploads.Create(UploadSide.Panel, UploadSurface.ProductGallery, "Input.", "Lamba", [empty, written]);

        Assert.True(view.Rows[0].UsesGeneratedText);
        Assert.Equal(GalleryText.Generated("Lamba", 1, 2), view.Rows[0].GeneratedAltText);
        Assert.Contains(view.Rows[0].GeneratedAltText, view.Rows[0].GeneratedTextHint, StringComparison.Ordinal);
        Assert.False(view.Rows[1].UsesGeneratedText);
    }

    [Fact]
    public void TheGallery_CountsItsRoomFromTheProductConstant_KK9()
    {
        GalleryUploadView full = GalleryUploads.Create(
            UploadSide.Panel,
            UploadSurface.ProductGallery,
            "Input.",
            "Lamba",
            [.. Enumerable.Range(1, ProductConstants.ImagesPerProductMax).Select(i => Image(i))]);
        GalleryUploadView one = GalleryUploads.Create(UploadSide.Panel, UploadSurface.ContentGallery, "Input.", "Hakkımızda", [Image(1)]);

        Assert.False(full.HasRoom);
        Assert.True(one.HasRoom);
        Assert.Equal(ProductConstants.ImagesPerContentRecordMax, one.MaxCount);
        Assert.StartsWith("1 / ", one.CountText, StringComparison.Ordinal);
    }

    // ---- KK10: refused files, progress, single files, no customer upload ----

    [Fact]
    public void AnImage_OfASupportedFormatWithinTheLimits_IsAccepted_KK10()
    {
        Assert.Null(UploadChecks.CheckImage(UploadSurface.ProductGallery, PngBytes, 1024, existingCount: 0));
    }

    [Fact]
    public void AFileThatIsNotAnImage_IsRefusedForItsFormat_ReadFromItsBytes_KK10()
    {
        UploadRefusal? refusal = UploadChecks.CheckImage(UploadSurface.ProductGallery, TextBytes, 5, 0);

        Assert.Equal(FieldCodes.InvalidImage, refusal!.Code);
        Assert.Contains(ProductConstants.ImageFormats, refusal.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void AnImageOverTheSizeLimit_IsRefusedWithTheLimitInItsMessage_KK10()
    {
        UploadRefusal? refusal = UploadChecks.CheckImage(UploadSurface.Logo, PngBytes, ProductConstants.ImageMaxBytes + 1, 0);

        Assert.Equal(FieldCodes.InvalidImage, refusal!.Code);
        Assert.Contains(UploadLimits.MaxSizeText(UploadSurface.Logo), refusal.Reason, StringComparison.Ordinal);
        Assert.Null(UploadChecks.CheckImage(UploadSurface.Logo, PngBytes, ProductConstants.ImageMaxBytes, 0));
    }

    [Theory]
    [InlineData(UploadSurface.ProductGallery)]
    [InlineData(UploadSurface.ContentGallery)]
    public void AnImageAboveTheGalleryCount_IsRefusedWithTheCountInItsMessage_KK10(UploadSurface surface)
    {
        int max = UploadLimits.MaxCount(surface);

        UploadRefusal? refusal = UploadChecks.CheckImage(surface, PngBytes, 10, max);

        Assert.Equal(FieldCodes.ImageLimitExceeded, refusal!.Code);
        Assert.Contains(max.ToString(System.Globalization.CultureInfo.InvariantCulture), refusal.Reason, StringComparison.Ordinal);
        Assert.Null(UploadChecks.CheckImage(surface, PngBytes, 10, max - 1));
    }

    [Fact]
    public void TheDigitalFile_TakesAnyFormat_AndRefusesOnlyAboveItsSizeLimit_KK10()
    {
        Assert.Null(UploadChecks.CheckDigitalFile(ProductConstants.DigitalFileMaxBytes));
        UploadRefusal? refusal = UploadChecks.CheckDigitalFile(ProductConstants.DigitalFileMaxBytes + 1);

        Assert.Contains(UploadLimits.MaxSizeText(UploadSurface.DigitalFile), refusal!.Reason, StringComparison.Ordinal);
        Assert.Null(UploadLimits.Accept(UploadSurface.DigitalFile));
        Assert.NotNull(UploadLimits.Accept(UploadSurface.Logo));
    }

    [Theory]
    [InlineData(UploadSurface.Logo)]
    [InlineData(UploadSurface.SiteIcon)]
    [InlineData(UploadSurface.DigitalFile)]
    public void ALogoASiteIconAndADigitalFile_AreOneFileEach_KK10(UploadSurface surface)
    {
        Assert.Equal(1, UploadLimits.MaxCount(surface));
        Assert.False(UploadLimits.IsGallery(surface));
        Assert.Null(UploadChecks.CheckCount(surface, existingCount: 1, incomingCount: 1));
        Assert.NotNull(UploadChecks.CheckCount(surface, 0, 2));
        Assert.Throws<ArgumentException>(() => GalleryUploads.Create(UploadSide.Panel, surface, "Input.", "Ad", []));
    }

    [Fact]
    public void AGalleryRequest_TakesWhatIsLeftOfItsLimit_KK10()
    {
        int max = UploadLimits.MaxCount(UploadSurface.ProductGallery);

        Assert.Null(UploadChecks.CheckCount(UploadSurface.ProductGallery, max - 2, 2));
        Assert.NotNull(UploadChecks.CheckCount(UploadSurface.ProductGallery, max - 2, 3));
    }

    [Fact]
    public void TheUploadsProgress_IsAPercent_AndWhileOneRunsTheFormCanNotBeSaved_KK10()
    {
        UploadProgressView running = new("kitap.pdf", 250, 1000);
        UploadProgressView done = new("logo.png", 10, 10);

        Assert.Equal(25, running.Percent);
        Assert.Equal(UploadState.Uploading, running.State);
        Assert.Equal(100, done.Percent);
        Assert.Contains("%25", running.Text, StringComparison.Ordinal);
        Assert.True(UploadStatusView.Of([running, done]).SaveBlocked);
        Assert.False(UploadStatusView.Of([done]).SaveBlocked);
        Assert.False(UploadStatusView.Of([]).SaveBlocked);
        Assert.Single(UploadStatusView.Of([running, done]).Running);
        Assert.False(UploadChecks.CanSave([UploadState.Completed, UploadState.Uploading]));
        Assert.True(UploadChecks.CanSave([]));
    }

    [Fact]
    public void TheCustomersSide_HasNoUpload_KK10()
    {
        Assert.Empty(UploadChecks.SurfacesOf(UploadSide.Customer));
        Assert.Equal(Enum.GetValues<UploadSurface>().Length, UploadChecks.SurfacesOf(UploadSide.Panel).Count);
        Assert.Throws<InvalidOperationException>(() => GalleryUploads.Create(UploadSide.Customer, UploadSurface.ProductGallery, "Input.", "Lamba", []));
        Assert.Throws<InvalidOperationException>(() => SingleUploads.Create(UploadSide.Customer, UploadSurface.Logo, "Input.Logo", "Logo"));
    }

    [Fact]
    public void TheSingleUpload_SaysWhatItTakes_FromTheProductConstants_KK10()
    {
        SingleUploadView logo = SingleUploads.Create(UploadSide.Panel, UploadSurface.Logo, "Input.Logo", "Logo", "eski.png");
        SingleUploadView digital = SingleUploads.Create(UploadSide.Panel, UploadSurface.DigitalFile, "Input.File", "Dijital dosya");

        Assert.Contains(ProductConstants.ImageFormats, logo.LimitText, StringComparison.Ordinal);
        Assert.Contains(UploadLimits.MaxSizeText(UploadSurface.Logo), logo.LimitText, StringComparison.Ordinal);
        Assert.Equal("eski.png", logo.CurrentFileName);
        Assert.Null(digital.CurrentFileName);
        Assert.DoesNotContain(ProductConstants.ImageFormats, digital.LimitText, StringComparison.Ordinal);
    }

    [Fact]
    public void ASilentlyGoneUpload_AsksForANewOne_AtItsField_KK10()
    {
        ModelStateDictionary state = new();

        UploadChecks.AddMissingUpload(state, "Input.File");
        UploadChecks.AddRefusal(state, "Input.Logo", new UploadRefusal(FieldCodes.InvalidImage, "Sebep."));

        Assert.StartsWith(FieldCodes.UploadMissing, state["Input.File"]!.Errors[0].ErrorMessage, StringComparison.Ordinal);
        Assert.Equal("Sebep.", state["Input.Logo"]!.Errors[0].ErrorMessage);
    }
}
