using Shopfolio.Pages.Components;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The words of the panel's screens that open before a session (T520; 04 9.1, 9.2; E-29, E-30): written here once. The wrong-credentials sentence is K-779's and says neither which field is wrong nor
/// whether the account exists (02 8.1.4); the reset's neutral sentence is the source's (03 9.2.8); the invalid invitation's sentence is K-799's and does not tell the reason (04 9.2.4).
/// </summary>
internal static class PanelAccessTexts
{
    public const string SignInTitle = "Panel girişi";

    public const string ResetRequestTitle = "Şifre sıfırlama";

    public const string ResetSetTitle = "Yeni şifre";

    public const string InvitationTitle = "Panele davet";

    /// <summary>K-779: the one message of a wrong e-mail or password.</summary>
    public const string InvalidCredentials = "E-posta adresi ya da şifre hatalı.";

    /// <summary>04 3.6.5, 6.3.1.3: the session closed by itself.</summary>
    public const string SessionClosed = "Oturumunuz kapandı. Devam etmek için yeniden giriş yapın.";

    /// <summary>04 9.2.6: the account the invitation opened; the sign-in follows.</summary>
    public const string AccountCreated = "Hesabınız açıldı. Belirlediğiniz şifreyle giriş yapabilirsiniz.";

    /// <summary>04 9.1.4, 03 9.2.8: the neutral sentence in the place of the request form.</summary>
    public const string ResetRequested = "Bu adres kayıtlıysa sıfırlama bağlantısını gönderdik.";

    /// <summary>04 9.1.5: the password is set and every session of the account is closed.</summary>
    public const string ResetCompleted = "Şifreniz kuruldu ve hesabınızın bütün oturumları kapatıldı. Yeni şifrenizle giriş yapabilirsiniz.";

    /// <summary>04 9.1.12: a link used, run out, replaced or unknown.</summary>
    public const string ResetLinkInvalid = "Bu sıfırlama bağlantısı artık geçerli değil. Yeni bir bağlantı isteyebilirsiniz.";

    /// <summary>K-799: the invalid invitation's message; the reason is not told.</summary>
    public const string InvitationInvalid = "Bu davet bağlantısı artık geçerli değil. Yeni bir davet için panele erişimi olan bir yöneticiye başvurun.";

    public const string SignInButton = "Giriş yap";

    public const string RequestButton = "Sıfırlama bağlantısı gönder";

    public const string SetPasswordButton = "Şifreyi kur";

    public const string AcceptButton = "Hesabı aç";

    public const string ForgotLink = "Şifremi unuttum";

    public const string ToSignInLink = "Panel girişine git";

    public const string ToRequestLink = "Yeni bağlantı iste";

    public const string RememberMeLabel = "Beni hatırla";

    /// <summary>The hint under a password that is being set: the policy's minimum (P-17).</summary>
    public static string PasswordHint() =>
        "En az " + Shopfolio.Infrastructure.DisplayFormat.Number(Shopfolio.Infrastructure.ProductConstants.PasswordMinLength) + " karakter olmalı.";

    /// <summary>A lasting message in place of a sign-in screen.</summary>
    public static MessageView InPlace(MessageMeaning meaning, string text, string id) => Messages.InPlace(meaning, text, id);
}
