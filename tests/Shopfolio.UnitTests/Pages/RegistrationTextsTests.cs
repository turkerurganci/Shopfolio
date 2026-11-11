using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Frame;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The registration screen's texts, the verification landing's six states and the concealed field (T514 KK1, KK7; 04 5.20, 5.21, 7.3.2.2).</summary>
public sealed class RegistrationTextsTests
{
    private static EmailLinkReading Reading(EmailLinkKind? kind, VerificationLinkStatus status) => new(kind, status, null);

    // ---- The concealed field: a sent secret is never drawn again ----

    [Fact]
    public void AConcealedField_HidesWhatIsTyped_AndNeverHoldsAValue_KK2()
    {
        FieldView field = Fields.Concealed("Input.Concealed", RegistrationTexts.ConcealedLabel, FieldRequirement.Required);

        Assert.Equal(FieldKind.Concealed, field.Kind);
        Assert.Equal("password", field.InputType);
        Assert.Null(field.Value);
        Assert.Equal("new-password", field.Autocomplete);
    }

    [Fact]
    public void TheOtherKinds_DoNotHideWhatIsTyped_KK2()
    {
        Assert.Equal("text", Fields.Text("Input.Name", "Ad", FieldRequirement.Required).InputType);
        Assert.Equal("email", Fields.Email("Input.Email", "E-posta", FieldRequirement.Required).InputType);
    }

    [Fact]
    public void TheHintOfTheSecretField_NamesTheMinimumLengthOnly_KK4()
    {
        Assert.StartsWith("En az ", RegistrationTexts.ConcealedHint, StringComparison.Ordinal);
        Assert.Contains(Shopfolio.Infrastructure.ProductConstants.PasswordMinLength.ToString(System.Globalization.CultureInfo.InvariantCulture), RegistrationTexts.ConcealedHint, StringComparison.Ordinal);
    }

    // ---- The submitted state ----

    [Fact]
    public void TheSubmittedState_NamesTheAddress_AndTheLastDayOfTheLink_KK3()
    {
        string sentence = RegistrationTexts.Submitted("ali@example.com", new DateTimeOffset(2026, 11, 20, 10, 0, 0, TimeSpan.Zero));

        Assert.Contains("ali@example.com", sentence, StringComparison.Ordinal);
        Assert.Contains("tarihine kadar geçerlidir", sentence, StringComparison.Ordinal);
        Assert.Throws<ArgumentException>(() => RegistrationTexts.Submitted(" ", DateTimeOffset.UtcNow));
    }

    // ---- KK7: six states ----

    [Fact]
    public void AnUnknownLink_AndAForeignLink_TellTheExpiredState_KK7()
    {
        VerificationLanding unknown = VerificationLandings.Of(Reading(null, VerificationLinkStatus.Missing), done: false, addressTaken: false, isMember: false);
        VerificationLanding foreign = VerificationLandings.Of(Reading(null, VerificationLinkStatus.Used), done: true, addressTaken: false, isMember: false);

        Assert.Same(VerificationLandings.Unrecognized, unknown);
        Assert.Same(VerificationLandings.Unrecognized, foreign);
        Assert.Null(unknown.ButtonLabel);
        Assert.Equal(FramePaths.Register, unknown.NextPath);
    }

    [Theory]
    [InlineData(EmailLinkKind.Registration)]
    [InlineData(EmailLinkKind.NewAddress)]
    [InlineData(EmailLinkKind.GoogleConfirmation)]
    public void AValidLink_OfferTheVerificationStep_AsItsOnlyAction_KK7(EmailLinkKind kind)
    {
        VerificationLanding landing = VerificationLandings.Of(Reading(kind, VerificationLinkStatus.Valid), done: false, addressTaken: false, isMember: false);

        Assert.NotNull(landing.ButtonLabel);
        Assert.Null(landing.NextPath);
        Assert.Equal(MessageMeaning.Info, landing.Meaning);
    }

    [Theory]
    [InlineData(EmailLinkKind.Registration)]
    [InlineData(EmailLinkKind.NewAddress)]
    [InlineData(EmailLinkKind.GoogleConfirmation)]
    public void TheResultCode_CountsOnlyOnALinkThatIsNowUsed_KK7(EmailLinkKind kind)
    {
        VerificationLanding validWithCode = VerificationLandings.Of(Reading(kind, VerificationLinkStatus.Valid), done: true, addressTaken: false, isMember: false);
        VerificationLanding validWithout = VerificationLandings.Of(Reading(kind, VerificationLinkStatus.Valid), done: false, addressTaken: false, isMember: false);
        VerificationLanding usedWithCode = VerificationLandings.Of(Reading(kind, VerificationLinkStatus.Used), done: true, addressTaken: false, isMember: false);
        VerificationLanding usedWithout = VerificationLandings.Of(Reading(kind, VerificationLinkStatus.Used), done: false, addressTaken: false, isMember: false);

        Assert.Equal(validWithout, validWithCode);
        Assert.Equal(MessageMeaning.Success, usedWithCode.Meaning);
        Assert.NotEqual(usedWithCode.Sentence, usedWithout.Sentence);
    }

    [Fact]
    public void ASupersededRegistrationLink_TellsThatTheNewestLinkShouldBeUsed_KK7()
    {
        VerificationLanding landing = VerificationLandings.Of(Reading(EmailLinkKind.Registration, VerificationLinkStatus.Superseded), done: false, addressTaken: false, isMember: false);

        Assert.Equal(MessageMeaning.Warning, landing.Meaning);
        Assert.Contains("en son gönderilen", landing.Sentence, StringComparison.Ordinal);
    }

    [Fact]
    public void ANewAddressTakenElsewhere_TellsAnError_AndLeadsToTheAccount_KK10()
    {
        VerificationLanding signedOut = VerificationLandings.Of(Reading(EmailLinkKind.NewAddress, VerificationLinkStatus.Valid), done: false, addressTaken: true, isMember: false);
        VerificationLanding member = VerificationLandings.Of(Reading(EmailLinkKind.NewAddress, VerificationLinkStatus.Valid), done: false, addressTaken: true, isMember: true);

        Assert.Equal(MessageMeaning.Error, signedOut.Meaning);
        Assert.Null(signedOut.ButtonLabel);
        Assert.Equal(FramePaths.SignIn, signedOut.NextPath);
        Assert.Equal(FramePaths.Account, member.NextPath);
    }

    [Fact]
    public void TheRegistrationDone_DoesNotSignAnyoneIn_KK9()
    {
        VerificationLanding landing = VerificationLandings.Of(Reading(EmailLinkKind.Registration, VerificationLinkStatus.Used), done: true, addressTaken: false, isMember: false);

        Assert.Contains("oturum açmaz", landing.Sentence, StringComparison.Ordinal);
        Assert.Equal(FramePaths.SignIn, landing.NextPath);
    }

    [Fact]
    public void ALandingWithoutAReading_CanNotBeBuilt_KK7()
    {
        Assert.Throws<ArgumentNullException>(() => VerificationLandings.Of(null!, done: false, addressTaken: false, isMember: false));
    }
}
