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

    public const string VerifyEmailTitle = "Yeni e-posta adresi";

    public const string RevertEmailTitle = "E-posta değişikliği";

    public const string ReauthTitle = "Kimliğinizi doğrulayın";

    /// <summary>04 9.25.4: the valid new-address link; one sentence and the one button (K-904).</summary>
    public const string VerifyEmailValid = "Bu adresi hesabınızın yeni e-posta adresi yapmak için doğrulayın.";

    public const string VerifyEmailButton = "Yeni adresi doğrula";

    /// <summary>04 9.25.4: the new address is verified; the account works with it from now on and the earlier address was told.</summary>
    public const string EmailVerified = "Yeni e-posta adresiniz doğrulandı. Hesabınız bundan sonra bu adresle çalışır; önceki adresinize bildirim gönderdik.";

    /// <summary>04 9.25.4: the link is used, run out, replaced or unknown; the change is not valid with it and is started again from the account page.</summary>
    public const string VerifyEmailInvalid = "Bu doğrulama bağlantısı artık geçerli değil. E-posta değişikliği bu bağlantıyla geçerli olmaz; \"Hesabım\" sayfasından yeniden başlatabilirsiniz.";

    /// <summary>04 9.25.4: the new address became another administrator account's address in between; nothing was written.</summary>
    public const string VerifyEmailTaken = "Bu e-posta adresi başka bir hesaba ait olduğu için değişiklik uygulanmadı.";

    /// <summary>04 9.25.5: the valid revert link; one sentence about the result and the one button (K-839).</summary>
    public const string RevertValid = "Geri alırsanız hesabın e-postası önceki adrese döner, bütün oturumları kapanır, şifresi geçersiz olur ve önceki adrese şifre sıfırlama bağlantısı gönderilir.";

    public const string RevertButton = "E-posta değişikliğini geri al";

    /// <summary>04 9.25.5: the change is reverted.</summary>
    public const string EmailReverted =
        "E-posta değişikliği geri alındı. Hesabın e-postası önceki adrese döndü, bütün oturumları kapandı, tanınan tarayıcı işaretleri düştü ve şifresi geçersiz oldu. Önceki adrese şifre sıfırlama bağlantısı gönderildi.";

    /// <summary>04 9.25.5: the link is used or run out; the way back is another administrator or, when none can sign in, the person who set the product up.</summary>
    public const string RevertInvalid =
        "Bu geri alma bağlantısı artık geçerli değil; ürünün içinden geri alma yolu kalmadı. Panele erişimi olan başka bir yöneticiye başvurun; hiçbir yönetici giremiyorsa kurulumu yapan kişiye başvurun.";

    /// <summary>04 9.25.5: the earlier address belongs to another administrator account now; the revert is not applied.</summary>
    public const string RevertNotApplied = "Geri alma uygulanmadı: önceki e-posta adresi artık başka bir hesaba ait. Panele erişimi olan başka bir yöneticiye ya da kurulumu yapan kişiye başvurun.";

    public const string ToMyAccountLink = "Hesabım sayfasına git";

    public const string ReauthPasswordChangeLine = "Şifrenizi değiştirmek için kimliğinizi doğrulayın.";

    public const string ReauthEmailChangeLine = "E-posta adresinizi değiştirmek için kimliğinizi doğrulayın.";

    public const string ReauthButton = "Doğrula";

    /// <summary>04 9.25.9: the wrong password is the field's message.</summary>
    public const string ReauthPasswordWrong = "Şifre hatalı.";

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
