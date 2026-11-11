using System.Globalization;

using Microsoft.AspNetCore.Mvc.ModelBinding;

using Shopfolio.Infrastructure.Operations;
using Shopfolio.Infrastructure.Security;
using Shopfolio.Modules.Settings.Contracts;
using Shopfolio.Modules.Settings.Contracts.Gates;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The small rules of the message, confirmation, empty, loading and sales closed models (T541 KK1 to KK10; 04 2.3, 2.4, 2.7, 2.8).</summary>
public sealed class MessageStateModelTests
{
    private static readonly MessageMeaning[] AllMeanings = Enum.GetValues<MessageMeaning>();

    private static readonly KeyValuePair<string, string>[] NoFields = [];

    private static SalesGate GateWith(params SalesGateCondition[] unmet) => new(unmet, []);

    // ---- KK1: symbol and text; the meaning is not the colour alone ----

    [Fact]
    public void EveryMeaning_HasItsOwnSymbolWordAndStyleKey_KK1()
    {
        Assert.Equal(4, AllMeanings.Length);
        Assert.Equal(4, AllMeanings.Select(MessageSymbols.Of).Distinct().Count());
        Assert.Equal(4, AllMeanings.Select(MessageSymbols.WordOf).Distinct().Count());
        Assert.Equal(4, AllMeanings.Select(MessageSymbols.KeyOf).Distinct().Count());
        Assert.All(AllMeanings, m => Assert.False(string.IsNullOrWhiteSpace(MessageSymbols.Of(m))));
    }

    [Fact]
    public void AMessage_CarriesSymbolAndText_AndErrorsAndWarningsAreAlerts_KK1()
    {
        MessageView error = Messages.InPlace(MessageMeaning.Error, "  Kayıt yapılamadı.  ");
        MessageView info = Messages.InPlace(MessageMeaning.Info, "Bilgi metni.");

        Assert.Equal("Kayıt yapılamadı.", error.Text);
        Assert.Equal(MessageSymbols.Of(MessageMeaning.Error), error.Symbol);
        Assert.Equal("alert", error.Role);
        Assert.Equal("alert", Messages.InPlace(MessageMeaning.Warning, "Dikkat.").Role);
        Assert.Equal("status", info.Role);
        Assert.Equal("message--error", error.Modifier);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AMessageWithoutText_IsRefused_KK1(string text)
    {
        Assert.ThrowsAny<ArgumentException>(() => Messages.InPlace(MessageMeaning.Info, text));
        Assert.ThrowsAny<ArgumentException>(() => Messages.OnceTold(MessageMeaning.Info, text));
        Assert.ThrowsAny<ArgumentException>(() => Messages.ForField("Input_Name", text));
        Assert.ThrowsAny<ArgumentException>(() => Messages.ShortLivedSuccess(text));
    }

    // ---- KK2: summary, focus, kept values ----

    [Fact]
    public void TheSummary_CountsTheWrongFields_AndPointsTheFocusToTheFirst_KK2()
    {
        ModelStateDictionary state = new();
        state.AddModelError("Input.Name", "Ad boş olamaz.");
        state.AddModelError("Input.Name", "İkinci hata sayılmaz.");
        state.AddModelError("Input.Sku", "Stok numarası gerekli.");
        state.AddModelError("Variants[0].Price", "Fiyat gerekli.");
        state.AddModelError(string.Empty, "İşlem reddedildi.");
        state.SetModelValue("Input.Other", "typed", "typed");

        FormErrorSummary summary = FormErrorSummary.From(state, ["Input.Name", "Input.Sku", "Variants[0].Price"]);

        Assert.Equal(3, summary.ErrorCount);
        Assert.Equal("3 alanda hata var", summary.Heading);
        Assert.Equal("Input_Name", summary.FirstFieldId);
        Assert.Equal(["Input_Name", "Input_Sku", "Variants_0__Price"], summary.Fields.Select(f => f.FieldId).ToArray());
        Assert.Equal("Ad boş olamaz.", summary.Fields[0].Text);
        Assert.Equal(["İşlem reddedildi."], summary.FormMessages.ToArray());
        Assert.Equal("autofocus", summary.AutofocusFor("Input_Name"));
        Assert.Equal(string.Empty, summary.AutofocusFor("Input_Sku"));
        Assert.True(summary.IsInvalid("Input_Sku"));
        Assert.False(summary.IsInvalid("Input_Other"));
    }

    [Fact]
    public void TheFormOrder_DecidesTheFirstField_AndFieldsNotNamedComeAfterTheNamedOnes_KK2()
    {
        ModelStateDictionary state = new();
        state.AddModelError("Input.Alpha", "a");
        state.AddModelError("Input.Zulu", "z");
        state.AddModelError("Input.Mike", "m");

        Assert.Equal("Input_Zulu", FormErrorSummary.From(state, ["Input.Zulu", "Input.Mike"]).FirstFieldId);
        Assert.Equal(["Input_Zulu", "Input_Mike", "Input_Alpha"], FormErrorSummary.From(state, ["Input.Zulu", "Input.Mike"]).Fields.Select(f => f.FieldId).ToArray());
        Assert.Equal(3, FormErrorSummary.From(state).ErrorCount);
    }

    [Fact]
    public void TheFormsSpelling_IsTheElementId_WhenTheHandlersParameterSpellsThePrefixInLowerCase_KK2()
    {
        ModelStateDictionary state = new();
        state.AddModelError("input.Sku", "s");
        state.AddModelError("input.Name", "n");

        FormErrorSummary summary = FormErrorSummary.From(state, ["Input.Name", "Input.Sku"]);

        Assert.Equal(["Input_Name", "Input_Sku"], summary.Fields.Select(f => f.FieldId).ToArray());
        Assert.Equal("Input_Name", summary.FirstFieldId);
    }

    [Fact]
    public void TheSummary_OfAFormMessageOnly_IsNotDrawn_KeepsTheMessageForTheScreen_AndHasNoFieldToFocus_KK2()
    {
        ModelStateDictionary state = new();
        state.AddModelError(string.Empty, "Bu işlem şu anda yapılamıyor.");

        FormErrorSummary summary = FormErrorSummary.From(state);

        Assert.False(summary.HasContent);
        Assert.Equal(["Bu işlem şu anda yapılamıyor."], summary.FormMessages.ToArray());
        Assert.Equal(0, summary.ErrorCount);
        Assert.Null(summary.FirstFieldId);
        Assert.Equal(string.Empty, summary.AutofocusFor("Input_Name"));
    }

    [Fact]
    public void TheSummary_OfAValidState_HasNoContent_KK2()
    {
        Assert.False(FormErrorSummary.From(new ModelStateDictionary()).HasContent);
    }

    [Fact]
    public void TheHeading_OfOneField_IsSingular_AndTheFieldMessageIdIsTheOneTheFieldNames_KK2()
    {
        FormErrorSummary one = new([new FormFieldError("Input_Name", "Ad boş olamaz.")]);

        Assert.Equal("1 alanda hata var", one.Heading);
        Assert.Equal("Input_Name-message", Messages.FieldMessageId("Input_Name"));
        Assert.Equal("Input_Name-message", Messages.ForField("Input_Name", "Ad boş olamaz.").Id);
        Assert.Equal(MessagePlace.Field, Messages.ForField("Input_Name", "Ad boş olamaz.").Place);
    }

    // ---- KK3: the lasting block message ----

    [Fact]
    public void ThePanelBlock_SaysTheReasonTheCountAndTheWayOut_InPlace_KK3()
    {
        MessageView block = Messages.PanelBlock("Kategori silinemez.", 12, "Önce ürünleri taslağa alın.");

        Assert.Equal(MessagePlace.InPlace, block.Place);
        Assert.Equal(MessageMeaning.Error, block.Meaning);
        Assert.Equal("Kategori silinemez. Engelleyen kayıt sayısı: 12. Önce ürünleri taslağa alın.", block.Text);
        Assert.Equal("Kategori silinemez. Önce ürünleri taslağa alın.", Messages.PanelBlock("Kategori silinemez.", null, "Önce ürünleri taslağa alın.").Text);
    }

    [Fact]
    public void ThePanelBlockCount_IsWrittenWithTheThousandsSeparator_AndZeroIsRefused_KK3()
    {
        Assert.Contains("1.234", Messages.PanelBlock("Silinemez.", 1234, "Taslağa alın.").Text, StringComparison.Ordinal);
        Assert.Throws<ArgumentOutOfRangeException>(() => Messages.PanelBlock("Silinemez.", 0, "Taslağa alın."));
        Assert.ThrowsAny<ArgumentException>(() => Messages.PanelBlock("Silinemez.", 2, " "));
    }

    // ---- KK4: the short-lived notice is a success and nothing else ----

    [Fact]
    public void TheShortLivedNotice_IsASuccess_AndTheOtherMeaningsHaveNoWayToBeShortLived_KK4()
    {
        MessageView notice = Messages.ShortLivedSuccess("Kaydedildi.");

        Assert.Equal(MessageMeaning.Success, notice.Meaning);
        Assert.Equal(MessagePlace.ShortLived, notice.Place);
        Assert.All(
            AllMeanings.Where(m => m != MessageMeaning.Success),
            m =>
            {
                Assert.NotEqual(MessagePlace.ShortLived, Messages.InPlace(m, "Metin.").Place);
                Assert.NotEqual(MessagePlace.ShortLived, Messages.OnceTold(m, "Metin.").Place);
            });
        Assert.NotEqual(MessagePlace.ShortLived, Messages.PanelBlock("Silinemez.", 3, "Taslağa alın.").Place);
        Assert.NotEqual(MessagePlace.ShortLived, Messages.NeutralLimit().Place);
    }

    // ---- KK5: the neutral limit message ----

    [Fact]
    public void TheNeutralLimitMessage_IsTheOneSentence_WithoutNumbersTimesOrAxis_KK5()
    {
        MessageView limit = Messages.NeutralLimit("limit-message");

        Assert.Equal(NeutralMessages.LimitExceeded, limit.Text);
        Assert.Equal(MessagePlace.InPlace, limit.Place);
        Assert.Equal("limit-message", limit.Id);
        Assert.DoesNotContain(limit.Text, char.IsDigit);
        Assert.DoesNotContain("saniye", limit.Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("dakika", limit.Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("kalan", limit.Text, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("IP", limit.Text, StringComparison.Ordinal);
    }

    // ---- KK6, KK7: the anatomy of the confirmation ----

    private static IReadOnlyList<KeyValuePair<string, string>> FieldsOfOrder() => [new("Id", "7"), new("Confirmed", "true")];

    [Fact]
    public void TheIrreversibleConfirmation_TakesTheTextsOfTheOperation_AndAddsTheIrreversibleSentence_KK6()
    {
        IrreversibleConfirmation text = IrreversibleConfirmations.Shipment(false, false);

        ConfirmView view = Confirmations.ForIrreversible(text, "Kargoya ver", "Kargoya ver", "/panel/siparisler/7", "Ship", "/panel/siparisler/7", FieldsOfOrder());

        Assert.Equal(ConfirmSurface.AdminWindow, view.Surface);
        Assert.True(view.IsIrreversible);
        Assert.Equal(text.Lines.ToArray(), view.Lines.ToArray());
        Assert.Equal(IrreversibleConfirmations.IrreversibleSentence, view.Lines[^1]);
        Assert.Equal("Vazgeç", ConfirmView.CancelLabel);
        Assert.Equal("Kargoya ver", view.ConfirmLabel);
        Assert.Equal(2, view.Fields.Count);
    }

    [Fact]
    public void TheSettingChange_CarriesNoIrreversibleSentence_AndTheCustomerStepDoes_KK6()
    {
        ConfirmView setting = Confirmations.ForSettingChange("Satışı kapat", "Satış kapanır; sepetler korunur.", "Satışı kapat", "/panel/ayarlar", "Close", "/panel/ayarlar", NoFields);
        ConfirmView customer = Confirmations.ForCustomer("Hesabı sil", "Hesabınız silinir.", "Hesabı sil", "/hesabim/sil", "Delete", "/hesabim", NoFields);

        Assert.False(setting.IsIrreversible);
        Assert.Equal(["Satış kapanır; sepetler korunur."], setting.Lines.ToArray());
        Assert.Equal(ConfirmSurface.CustomerStep, customer.Surface);
        Assert.Equal(["Hesabınız silinir.", IrreversibleConfirmations.IrreversibleSentence], customer.Lines.ToArray());
    }

    [Theory]
    [InlineData("Evet")]
    [InlineData("tamam")]
    [InlineData(" Onayla ")]
    [InlineData("Devam")]
    public void AConfirmButtonNamedLikeAnAnswer_IsRefused_KK6(string label)
    {
        Assert.Throws<ArgumentException>(() =>
            Confirmations.ForSettingChange("Satışı kapat", "Cümle.", label, "/panel/ayarlar", "Close", "/panel/ayarlar", NoFields));
    }

    [Theory]
    [InlineData("https://other.example/x", "/panel")]
    [InlineData("//other.example/x", "/panel")]
    [InlineData("/panel", "https://other.example/x")]
    [InlineData("/panel", "javascript:alert(1)")]
    public void AConfirmationWithAForeignAddress_IsRefused_KK6(string actionPath, string cancelPath)
    {
        Assert.Throws<ArgumentException>(() =>
            Confirmations.ForCustomer("Hesabı sil", "Cümle.", "Hesabı sil", actionPath, "Delete", cancelPath, NoFields));
    }

    [Fact]
    public void TheBusyButton_SaysThatTheOperationContinues_KK7()
    {
        Assert.Equal("Kargoya ver sürüyor", LoadingViews.BusyLabelOf(" Kargoya ver "));
    }

    // ---- KK8: the empty states and the loading state ----

    [Fact]
    public void AnEmptyState_AsksForTheSentenceAndALocalWayOut_KK8()
    {
        EmptyStateView link = EmptyStates.WithExit("Henüz ürün yok.", "Ana sayfaya dön", "/");
        EmptyStateView button = EmptyStates.WithFirstRecordButton("Henüz adresiniz yok.", "Adres ekle", "/hesabim/adresler/yeni");

        Assert.False(link.IsFirstRecordButton);
        Assert.True(button.IsFirstRecordButton);
        Assert.Throws<ArgumentException>(() => EmptyStates.WithExit("Boş.", "Git", "https://other.example"));
        Assert.ThrowsAny<ArgumentException>(() => EmptyStates.WithExit(" ", "Git", "/"));
        Assert.ThrowsAny<ArgumentException>(() => EmptyStates.WithFirstRecordButton("Boş.", "", "/panel"));
    }

    [Fact]
    public void TheLoadingState_AlwaysCarriesText_KK8()
    {
        Assert.Equal("Siparişler yükleniyor", LoadingViews.Of(" Siparişler yükleniyor ").Text);
        Assert.ThrowsAny<ArgumentException>(() => LoadingViews.Of(""));
    }

    // ---- KK9: the three error states ----

    [Fact]
    public void TheUnexpectedErrorHasThreeStates_AndTheErrorPageHasThreeSurfaces_KK9()
    {
        Assert.Equal([UnexpectedErrorKind.Operation, UnexpectedErrorKind.Page, UnexpectedErrorKind.OutcomeUncertain], Enum.GetValues<UnexpectedErrorKind>());
        Assert.Equal([ErrorSurface.Storefront, ErrorSurface.Panel, ErrorSurface.PanelBare], Enum.GetValues<ErrorSurface>());
    }

    // ---- KK10: sales closed ----

    [Theory]
    [InlineData(true, SalesGateOperation.AddToCart, true)]
    [InlineData(true, SalesGateOperation.ProceedToCheckout, true)]
    [InlineData(true, SalesGateOperation.ConfirmOrder, true)]
    [InlineData(true, SalesGateOperation.BrowseShowcase, false)]
    [InlineData(true, SalesGateOperation.KeepExistingCart, false)]
    [InlineData(true, SalesGateOperation.ContinueOpenOrder, false)]
    [InlineData(false, SalesGateOperation.AddToCart, false)]
    [InlineData(false, SalesGateOperation.ConfirmOrder, false)]
    public void TheStorefront_SaysSalesAreClosedAtTheThreeClosedPlaces_AndNamesNoCondition_KK10(bool closed, SalesGateOperation operation, bool shows)
    {
        MessageView? message = SalesClosedMessages.ForStorefront(new FrameSales(closed, false, true), operation);

        Assert.Equal(shows, message is not null);
        if (message is not null)
        {
            Assert.Equal(SalesClosedMessages.StorefrontText, message.Text);
            Assert.Equal(MessagePlace.InPlace, message.Place);
        }
    }

    [Theory]
    [InlineData(false, DataCollectionEntry.AccountRegistration, true)]
    [InlineData(false, DataCollectionEntry.NewAccountGoogleSignIn, true)]
    [InlineData(false, DataCollectionEntry.ContactForm, true)]
    [InlineData(false, DataCollectionEntry.ExistingAccountSignIn, false)]
    [InlineData(true, DataCollectionEntry.AccountRegistration, false)]
    [InlineData(true, DataCollectionEntry.ContactForm, false)]
    public void TheDataEntryGate_IsItsOwnState_AndTheSignInOfExistingAccountsIsNeverClosed_KK10(bool open, DataCollectionEntry entry, bool shows)
    {
        MessageView? message = SalesClosedMessages.ForDataEntry(new FrameSales(true, false, open), entry);

        Assert.Equal(shows, message is not null);
        if (message is not null)
        {
            Assert.Equal(SalesClosedMessages.DataEntryText, message.Text);
        }
    }

    [Fact]
    public void ThePanelStatus_OfAnOpenGate_HasNoWarningAndNoChecklist_ButTheFourConditionRowsAndTheSwitch_KK10()
    {
        PanelSalesStatus status = PanelSalesStatus.From(GateWith());

        Assert.True(status.IsOpen);
        Assert.Equal(PanelFramePaths.SalesOpenText, status.IndicatorText);
        Assert.Null(status.WarningText);
        Assert.Empty(status.Checklist);
        Assert.Equal(4, status.Conditions.Count);
        Assert.All(status.Conditions, c => Assert.True(c.IsMet));
        Assert.True(status.TemporaryClosureSwitchVisible);
        Assert.True(status.DataEntriesOpen);
        Assert.Empty(status.DataEntryConditionNames);
    }

    [Fact]
    public void ThePanelStatus_OfMissingConditions_NamesThemAndTheScreenThatFixesEach_KK10()
    {
        PanelSalesStatus status = PanelSalesStatus.From(GateWith(SalesGateCondition.CompanyIdentity, SalesGateCondition.PrivacyNotice, SalesGateCondition.ReturnAddress));

        Assert.False(status.IsOpen);
        Assert.Equal(PanelFramePaths.SalesClosedText, status.IndicatorText);
        Assert.Equal("Satış kapalı: Firma kimliği, Aydınlatma metni, İade adresi tamamlanmadı.", status.WarningText);
        Assert.Equal(["Firma kimliği", "Aydınlatma metni", "İade adresi"], status.Checklist.Select(c => c.Name).ToArray());
        Assert.Equal(PanelFramePaths.FirmIdentityAndSales, status.Checklist[0].FixPath);
        Assert.Equal(PanelFramePaths.LegalTexts, status.Checklist[1].FixPath);
        Assert.Equal(PanelFramePaths.ShippingAndDefaults, status.Checklist[2].FixPath);
        Assert.False(status.Conditions[0].IsMet);
        Assert.True(status.Conditions[1].IsMet);
        Assert.False(status.Conditions[3].IsMet);
        Assert.False(status.DataEntriesOpen);
        Assert.Equal(["Aydınlatma metni"], status.DataEntryConditionNames.ToArray());
        Assert.All(status.Checklist, c => Assert.True(ReturnPath.IsLocalPath(c.FixPath)));
    }

    [Fact]
    public void ATemporaryClosure_KeepsTheSwitch_HasNoChecklist_AndSaysItInTheWarning_KK10()
    {
        PanelSalesStatus status = PanelSalesStatus.From(GateWith(SalesGateCondition.TemporaryClosure));

        Assert.False(status.IsOpen);
        Assert.Equal("Satış kapalı: satışı geçici kapattınız.", status.WarningText);
        Assert.Empty(status.Checklist);
        Assert.True(status.TemporaryClosureSwitchVisible);
        Assert.False(status.Conditions[2].IsMet);
        Assert.True(status.DataEntriesOpen);
    }

    [Fact]
    public void ATemporaryClosureWithAMissingCondition_StillShowsTheMissingCondition_KK10()
    {
        PanelSalesStatus status = PanelSalesStatus.From(GateWith(SalesGateCondition.PaymentMethod, SalesGateCondition.TemporaryClosure));

        Assert.Equal("Satış kapalı: Ödeme yöntemi, Geçici kapatma tamamlanmadı.", status.WarningText);
        Assert.Equal(["Ödeme yöntemi"], status.Checklist.Select(c => c.Name).ToArray());
    }

    [Fact]
    public void EveryCondition_HasANameAndALocalScreen_KK10()
    {
        foreach (SalesGateCondition condition in Enum.GetValues<SalesGateCondition>())
        {
            Assert.False(string.IsNullOrWhiteSpace(PanelSalesStatus.NameOf(condition)));
            Assert.True(ReturnPath.IsLocalPath(PanelSalesStatus.PathOf(condition)), condition.ToString());
        }
    }

    [Fact]
    public void TheCountOfTheSummary_IsWrittenInTurkishCulture_KK2()
    {
        FormErrorSummary many = new(Enumerable.Range(1, 1200).Select(i => new FormFieldError("F" + i.ToString(CultureInfo.InvariantCulture), "x")));

        Assert.Equal("1.200 alanda hata var", many.Heading);
    }
}
