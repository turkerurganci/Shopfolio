using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Values;
using Shopfolio.Pages.Components;
using Shopfolio.Pages.Storefront;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The re-authentication's texts and addresses (T516; 04 5.23, 5.24; 07 UC-60 to UC-62): the closed operation list, the sentences, the lasting messages.</summary>
public sealed class ReauthenticationTextsTests
{
    [Theory]
    [InlineData("PasswordChange", ReauthenticationOperation.PasswordChange)]
    [InlineData("EmailChange", ReauthenticationOperation.EmailChange)]
    [InlineData("AccountDeletion", ReauthenticationOperation.AccountDeletion)]
    public void TryParse_ANameOfTheClosedList_GivesTheOperation(string raw, ReauthenticationOperation expected)
    {
        Assert.True(ReauthenticationAddresses.TryParse(raw, out ReauthenticationOperation operation));
        Assert.Equal(expected, operation);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("passwordchange")]
    [InlineData("PASSWORDCHANGE")]
    [InlineData("0")]
    [InlineData("1")]
    [InlineData("PasswordChange,EmailChange")]
    [InlineData("Other")]
    public void TryParse_AnythingOutsideTheClosedList_IsNotAnOperation(string? raw)
    {
        Assert.False(ReauthenticationAddresses.TryParse(raw, out _));
    }

    [Fact]
    public void ScreenAndGranted_CarryTheOperationNameAndTheEscapedValue()
    {
        Assert.Equal("/hesap/yeniden-dogrulama?reauthenticatedOperation=EmailChange", ReauthenticationAddresses.Screen(ReauthenticationOperation.EmailChange));
        string granted = ReauthenticationAddresses.Granted(ReauthenticationOperation.AccountDeletion, "a+b/c=");
        Assert.EndsWith("?reauthenticatedOperation=AccountDeletion&reauthenticationToken=a%2Bb%2Fc%3D", granted);
    }

    [Fact]
    public void OperationLine_EveryOperationHasItsOwnSentence_AndAnUnknownOneThrows()
    {
        var lines = Enum.GetValues<ReauthenticationOperation>().Select(ReauthenticationTexts.OperationLine).ToList();

        Assert.Equal(lines.Count, lines.Distinct().Count());
        Assert.All(lines, line => Assert.Contains("kimliğinizi doğrulayın", line));
        Assert.Throws<ArgumentOutOfRangeException>(() => ReauthenticationTexts.OperationLine((ReauthenticationOperation)999));
    }

    [Theory]
    [InlineData(NoticeCode.SocialReauthenticationFailed, ReauthenticationTexts.GoogleFailed)]
    [InlineData(NoticeCode.SocialReauthenticationUnavailable, ReauthenticationTexts.GoogleUnavailable)]
    public void ResultOf_ACodeOfTheScreen_GivesAnErrorSentence(NoticeCode code, string text)
    {
        (MessageMeaning Meaning, string Text)? result = ReauthenticationTexts.ResultOf(code);

        Assert.NotNull(result);
        Assert.Equal(MessageMeaning.Error, result.Value.Meaning);
        Assert.Equal(text, result.Value.Text);
    }

    [Fact]
    public void ResultOf_NoCodeOrAnotherScreensCode_GivesNothing()
    {
        Assert.Null(ReauthenticationTexts.ResultOf(null));
        Assert.Null(ReauthenticationTexts.ResultOf(NoticeCode.SocialLoginFailed));
    }

    [Fact]
    public void ThePasswordResetSentences_SpeakSiz_AndTheRequestedSentenceNamesNoAccount()
    {
        Assert.Contains("kayıtlıysa", PasswordResetTexts.Requested);
        Assert.DoesNotContain("@", PasswordResetTexts.Requested);
        Assert.Contains("Şifreniz", PasswordResetTexts.Completed);
        Assert.NotEqual(PasswordResetTexts.SetScreenName, PasswordResetTexts.SetPasswordScreenName);
        Assert.StartsWith("En az ", PasswordResetTexts.PasswordHint);
    }
}
