using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Pages.Components;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The revert link's landing states (T519; 04 5.28; 07 UC-75, UC-76): the button belongs to the revert step alone, a result code counts only on a used link, every other state is the invalid one.</summary>
public sealed class EmailRevertLandingsTests
{
    [Fact]
    public void AValidLink_IsTheRevertStep_WithTheOneButtonNamedForTheOperation_T519_KK1()
    {
        EmailRevertLanding landing = EmailRevertLandings.Of(VerificationLinkStatus.Valid, done: false, notApplied: false);

        Assert.Equal(EmailRevertLandings.RevertStep, landing.Sentence);
        Assert.Equal(EmailRevertLandings.ButtonLabel, landing.ButtonLabel);
    }

    [Fact]
    public void AResultCode_OnAValidLink_DoesNotTellTheRevertIsDone_T519_KK2()
    {
        EmailRevertLanding landing = EmailRevertLandings.Of(VerificationLinkStatus.Valid, done: true, notApplied: false);

        Assert.Equal(EmailRevertLandings.RevertStep, landing.Sentence);
    }

    [Fact]
    public void AUsedLink_WithTheResultCode_TellsTheResult_AndWithoutItTellsThatTheRevertWasDone_T519_KK2()
    {
        EmailRevertLanding withCode = EmailRevertLandings.Of(VerificationLinkStatus.Used, done: true, notApplied: false);
        EmailRevertLanding without = EmailRevertLandings.Of(VerificationLinkStatus.Used, done: false, notApplied: false);

        Assert.Equal(EmailRevertLandings.Reverted, withCode.Sentence);
        Assert.Equal(EmailRevertLandings.UsedEarlier, without.Sentence);
        Assert.Null(withCode.ButtonLabel);
        Assert.Null(without.ButtonLabel);
    }

    [Theory]
    [InlineData(VerificationLinkStatus.Missing)]
    [InlineData(VerificationLinkStatus.Expired)]
    [InlineData(VerificationLinkStatus.Superseded)]
    public void EveryOtherState_IsTheInvalidOne_WithoutAButton_AndACodeChangesNothing_T519_KK2(VerificationLinkStatus status)
    {
        EmailRevertLanding plain = EmailRevertLandings.Of(status, done: false, notApplied: false);
        EmailRevertLanding coded = EmailRevertLandings.Of(status, done: true, notApplied: false);

        Assert.Equal(EmailRevertLandings.Invalid, plain.Sentence);
        Assert.Equal(EmailRevertLandings.Invalid, coded.Sentence);
        Assert.Null(plain.ButtonLabel);
    }

    [Fact]
    public void TheNotAppliedState_NamesTheOtherAccountAndTheContactPage_WithoutAButton_T519_KK7()
    {
        EmailRevertLanding landing = EmailRevertLandings.Of(VerificationLinkStatus.Valid, done: false, notApplied: true);

        Assert.Equal(EmailRevertLandings.NotApplied, landing.Sentence);
        Assert.Contains("başka bir hesaba ait", landing.Sentence, StringComparison.Ordinal);
        Assert.Contains("İletişim", landing.Sentence, StringComparison.Ordinal);
        Assert.Null(landing.ButtonLabel);
    }
}
