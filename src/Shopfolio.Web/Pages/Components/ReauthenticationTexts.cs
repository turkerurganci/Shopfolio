using Shopfolio.Infrastructure.Notices;
using Shopfolio.Infrastructure.Values;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The re-authentication screen's texts (T516; 04 E-24 5.24; 02 3.13.13; 03 9.2.6, 10.1.3.2; K-968). Forms and account screens speak "siz" (04 8.2.9). Written here once; the page and its tests read them from
/// here. The sentences have no wording in the sources and are written in the voice of the panel's re-verification and of the sign-in screen (listed as a note in the task report).
/// </summary>
public static class ReauthenticationTexts
{
    /// <summary>04 5.24.3: the screen's heading and tab title.</summary>
    public const string ScreenName = "Kimliğinizi doğrulayın";

    public const string PasswordLabel = "Şifre";

    /// <summary>04 5.24.3 item 4: the primary button of the password route.</summary>
    public const string ConfirmButton = "Doğrula";

    /// <summary>04 5.24.3 item 3: the Google route's button (07 UC-62).</summary>
    public const string GoogleButton = "Google ile doğrula";

    /// <summary>04 5.24.3 item 4: the way back to the account screen.</summary>
    public const string CancelLink = "Vazgeç";

    /// <summary>04 5.24.6 (07 UC-61): the field message of a wrong password.</summary>
    public const string PasswordWrong = "Şifre hatalı.";

    /// <summary>04 5.24.3 item 1: the name of the operation the screen asks the proof for.</summary>
    public static string OperationLine(ReauthenticationOperation operation)
    {
        switch (operation)
        {
            case ReauthenticationOperation.PasswordChange:
                return "Şifrenizi değiştirmek için kimliğinizi doğrulayın.";
            case ReauthenticationOperation.EmailChange:
                return "E-posta adresinizi değiştirmek için kimliğinizi doğrulayın.";
            case ReauthenticationOperation.AccountDeletion:
                return "Hesabınızı silmek için kimliğinizi doğrulayın.";
            default:
                throw new ArgumentOutOfRangeException(nameof(operation), operation, "Not a re-authentication operation (06 5.6.8).");
        }
    }

    /// <summary>04 5.24.7 (07 UC-62 422): the Google route is not offered - the account is not linked to Google, or the Google application is not defined; nothing was done.</summary>
    public const string GoogleNotOffered = "Google ile doğrulama şu anda sunulmuyor.";

    /// <summary>04 5.24.7 (03 10.1.3.2): Google could not be reached; an account without a password cannot do the three operations and is shown the way to set one.</summary>
    public const string GoogleUnavailable = "Google'a şu anda ulaşılamıyor; Google ile doğrulama yapılamadı. Daha sonra yeniden deneyebilirsiniz.";

    /// <summary>04 5.24.7 (K-968; 05 6.1.5): Google's sign-in is older than this try, has no moment, or the e-mail is not the account's - nothing was done.</summary>
    public const string GoogleFailed = "Google ile doğrulama tutmadı. Google'da yeniden giriş yapıp tekrar deneyebilirsiniz.";

    /// <summary>04 5.24.7: the account without a password is shown the way to set one - the reset flow works as the password-setting (02 3.13.8).</summary>
    public const string SetPasswordLink = "Şifre belirle";

    /// <summary>The lasting message of a result code on the screen (07 UC-60); null for a code the screen does not tell.</summary>
    public static (MessageMeaning Meaning, string Text)? ResultOf(NoticeCode? code) => code switch
    {
        NoticeCode.SocialReauthenticationFailed => (MessageMeaning.Error, GoogleFailed),
        NoticeCode.SocialReauthenticationUnavailable => (MessageMeaning.Error, GoogleUnavailable),
        _ => null,
    };
}
