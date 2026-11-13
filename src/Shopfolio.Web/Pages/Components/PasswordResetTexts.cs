using Shopfolio.Infrastructure;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The password reset screens' texts (T516; 04 E-23 5.23; 02 3.13.8 to 3.13.11; 03 9.2.4, 9.2.5). Forms and account screens speak "siz" (04 8.2.9). Written here once; the page and its tests read them from
/// here. The neutral sentence of the request is the source's own (02 3.13.11) and says nothing of whether the address is registered (02 8.1.4); the other sentences have no wording in the sources and are written in
/// the same voice as the panel's reset (listed as a note in the task report).
/// </summary>
public static class PasswordResetTexts
{
    /// <summary>04 5.23.3: the request step's heading and the tab title of the screen.</summary>
    public const string ScreenName = "Şifre sıfırlama";

    /// <summary>04 5.23.4: the setting step's heading.</summary>
    public const string SetScreenName = "Yeni şifre";

    /// <summary>04 5.23.4 (02 3.13.8): the setting step's heading for an account without a password - the reset works as the password-setting.</summary>
    public const string SetPasswordScreenName = "Şifre belirle";

    public const string EmailLabel = "E-posta";

    public const string NewPasswordLabel = "Yeni şifre";

    /// <summary>04 5.23.3 item 3: the request step's primary button.</summary>
    public const string RequestButton = "Sıfırlama bağlantısı gönder";

    /// <summary>04 5.23.4 item 3: the setting step's primary button.</summary>
    public const string SetButton = "Şifreyi kur";

    /// <summary>04 5.23.4 item 3: the setting step's button for an account without a password.</summary>
    public const string SetPasswordButton = "Şifreyi belirle";

    /// <summary>02 3.13.11 (04 5.23.3): the neutral sentence in the place of the form after the request; the same whether or not the address is registered.</summary>
    public const string Requested = "Bu adres kayıtlıysa sıfırlama bağlantısını gönderdik.";

    /// <summary>04 5.23.4: the password is set and every session of the account is closed; the link to the sign-in follows.</summary>
    public const string Completed = "Şifreniz kuruldu ve hesabınızın bütün oturumları kapatıldı. Yeni şifrenizle giriş yapabilirsiniz.";

    /// <summary>04 5.23.8 (03 3.4.5): a link that is used, run out, replaced or unknown opens no setting step.</summary>
    public const string LinkInvalid = "Bu sıfırlama bağlantısı artık geçerli değil. Yeni bir bağlantı isteyebilirsiniz.";

    /// <summary>04 5.23.8: the link of the invalid state to the request step.</summary>
    public const string ToRequestLink = "Yeni bağlantı iste";

    /// <summary>04 5.23.4: the link of the completed state to the sign-in (E-22).</summary>
    public const string ToSignInLink = "Giriş yap";

    /// <summary>The hint under the new password: the policy's minimum (P-17).</summary>
    public static string PasswordHint => "En az " + DisplayFormat.Number(ProductConstants.PasswordMinLength) + " karakter";
}
