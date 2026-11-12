using Shopfolio.Pages.Components;
using Shopfolio.Infrastructure.Notices;

namespace Shopfolio.UnitTests.Pages;

/// <summary>The sign-in screen's lasting messages (T515; 04 5.22.6, 5.22.10, 5.22.12, 5.22.15, 5.20.8): each result code of the screen has its sentence, any other has none.</summary>
public sealed class SignInTextsTests
{
    [Theory]
    [InlineData(NoticeCode.SessionExpired, MessageMeaning.Warning, SignInTexts.SessionClosed)]
    [InlineData(NoticeCode.SocialLoginFailed, MessageMeaning.Error, SignInTexts.SocialLoginFailed)]
    [InlineData(NoticeCode.SocialLoginDataCollectionClosed, MessageMeaning.Info, SignInTexts.SocialLoginDataCollectionClosed)]
    [InlineData(NoticeCode.SocialLoginLinkConfirmationSent, MessageMeaning.Info, SignInTexts.SocialLoginLinkConfirmationSent)]
    public void ResultOf_AResultCodeOfTheScreen_GivesItsMeaningAndSentence(NoticeCode code, MessageMeaning meaning, string text)
    {
        (MessageMeaning Meaning, string Text)? result = SignInTexts.ResultOf(code);

        Assert.NotNull(result);
        Assert.Equal(meaning, result.Value.Meaning);
        Assert.Equal(text, result.Value.Text);
    }

    [Fact]
    public void ResultOf_NoCodeOrAnotherScreensCode_GivesNothing()
    {
        Assert.Null(SignInTexts.ResultOf(null));
        Assert.Null(SignInTexts.ResultOf(NoticeCode.SocialLoginEmailUnverified));
    }

    [Fact]
    public void RegisterMessageOf_OnlyTheUnverifiedGoogleEmailLandsOnTheRegistrationScreen()
    {
        (MessageMeaning Meaning, string Text)? result = SignInTexts.RegisterMessageOf(NoticeCode.SocialLoginEmailUnverified);

        Assert.NotNull(result);
        Assert.Equal(MessageMeaning.Warning, result.Value.Meaning);
        Assert.Equal(SignInTexts.SocialLoginEmailUnverified, result.Value.Text);
        Assert.Null(SignInTexts.RegisterMessageOf(NoticeCode.SocialLoginFailed));
        Assert.Null(SignInTexts.RegisterMessageOf(null));
    }

    [Fact]
    public void TheWrongCredentialsSentence_SaysNeitherWhichOneNorWhetherTheAccountExists()
    {
        Assert.Equal("E-posta adresi ya da şifre hatalı.", SignInTexts.InvalidCredentials);
        Assert.NotEqual(SignInTexts.InvalidCredentials, SignInTexts.EmailNotVerified);
    }
}
