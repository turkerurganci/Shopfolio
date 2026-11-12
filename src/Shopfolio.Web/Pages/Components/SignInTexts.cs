using Shopfolio.Infrastructure.Notices;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The customer sign-in screen's texts (T515; 04 5.22; 03 9.2; K-779). Forms and account screens speak "siz" (04 8.2.9). Written here once; the pages and their tests read them from here. The sentences of
/// the wrong credentials and of the awaited verification are the decision record's (K-779 (3), (4)); the result-state sentences of the Google return have no sentence in the sources and are written
/// here in the same voice (listed as a note in the task report).
/// </summary>
public static class SignInTexts
{
    /// <summary>04 5.22.3: the screen's heading and its primary button.</summary>
    public const string ScreenName = "Giriş yap";

    public const string EmailLabel = "E-posta";

    public const string PasswordLabel = "Şifre";

    /// <summary>04 5.22.3: "beni hatırla" gives the long session Z-5, unticked the short one Z-4; the box arrives unticked.</summary>
    public const string RememberMeLabel = "Beni hatırla";

    /// <summary>04 5.22.3: the button of the sign-in with Google (shown only when the Google application is defined, 04 5.22.11).</summary>
    public const string GoogleLabel = "Google ile giriş";

    /// <summary>04 5.22.3: the link of the password reset (T516).</summary>
    public const string ForgotLink = "Şifremi unuttum";

    /// <summary>04 5.22.3: the link to the registration.</summary>
    public const string RegisterLink = "Hesabınız yok mu? Kayıt olun";

    /// <summary>04 5.22.3: the link of the privacy notice beside the Google button (02 3.33.5).</summary>
    public const string PrivacyNoticeLabel = "Google ile girişte kişisel verilerinizin nasıl işlendiğini aydınlatma metninde okuyabilirsiniz.";

    /// <summary>04 5.22.7 (K-779 (4)): one sentence for a wrong e-mail or a wrong password; it says neither which one nor whether the account exists.</summary>
    public const string InvalidCredentials = "E-posta adresi ya da şifre hatalı.";

    /// <summary>04 5.22.8 (K-779 (3)): the right e-mail and password of an account whose address is not verified yet - the verification is awaited.</summary>
    public const string EmailNotVerified = "E-posta adresiniz henüz doğrulanmadı. Size gönderdiğimiz bağlantıyı açın ya da yenisini isteyin.";

    /// <summary>04 5.22.12: the session that closed by itself.</summary>
    public const string SessionClosed = "Oturumunuz kapandı. Devam etmek için yeniden giriş yapın.";

    /// <summary>04 5.22.6 (07 UC-51): Google could not be reached, gave an error or the user gave up - nothing was opened.</summary>
    public const string SocialLoginFailed = "Google ile giriş tamamlanamadı. Yeniden deneyebilir ya da e-posta adresiniz ve şifrenizle giriş yapabilirsiniz.";

    /// <summary>04 5.22.10 (07 UC-51): the gate of the data-collecting entries is closed and the Google address names no account.</summary>
    public const string SocialLoginDataCollectionClosed = "Şu anda yeni hesap açılamıyor; bu Google hesabının e-posta adresiyle kayıtlı bir hesap da yok. Hesabınız varsa e-posta adresiniz ve şifrenizle giriş yapabilirsiniz.";

    /// <summary>04 5.22.15 (K-1281, K-1282): the account exists, is not linked to Google and Google is not the authority of its address - no sign-in; a link was sent to the account's address.</summary>
    public const string SocialLoginLinkConfirmationSent = "Bu e-posta adresiyle kayıtlı hesabınız Google'a bağlı değil. Google ile girişi onaylamanız için hesabınızın e-posta adresine bir bağlantı gönderdik; onaylayana kadar Google ile giriş yapılamaz. E-posta adresiniz ve şifrenizle girebilirsiniz.";

    /// <summary>04 5.20.8 (K-779 (5), K-697): told on the registration screen when Google did not give the e-mail as verified.</summary>
    public const string SocialLoginEmailUnverified = "Google bu e-posta adresini doğrulanmış olarak vermediği için Google ile hesap açılamadı. E-posta adresiniz ve bir şifreyle kaydolabilirsiniz.";

    /// <summary>
    /// The lasting message of a result code on the sign-in screen (04 5.22.12, 5.22.15; 07 2.2.3); null for a code the screen does not tell. The Google result codes that end on the registration screen are
    /// <see cref="RegisterMessageOf"/>'s.
    /// </summary>
    public static (MessageMeaning Meaning, string Text)? ResultOf(NoticeCode? code) => code switch
    {
        NoticeCode.SessionExpired => (MessageMeaning.Warning, SessionClosed),
        NoticeCode.SocialLoginFailed => (MessageMeaning.Error, SocialLoginFailed),
        NoticeCode.SocialLoginDataCollectionClosed => (MessageMeaning.Info, SocialLoginDataCollectionClosed),
        NoticeCode.SocialLoginLinkConfirmationSent => (MessageMeaning.Info, SocialLoginLinkConfirmationSent),
        _ => null,
    };

    /// <summary>The lasting message of a result code on the registration screen (04 5.20.8): only the unverified Google e-mail lands there.</summary>
    public static (MessageMeaning Meaning, string Text)? RegisterMessageOf(NoticeCode? code) =>
        code == NoticeCode.SocialLoginEmailUnverified ? (MessageMeaning.Warning, SocialLoginEmailUnverified) : null;
}
