using Shopfolio.Infrastructure;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The account screen's texts (T517; 04 E-25 5.25; 02 3.13.10, 3.13.14, 3.15; K-779 (2), (6), K-785, K-786). Forms and account screens speak "siz" (04 8.2.9). Written here once; the page, the deletion's result
/// state and their tests read them from here. The wording the sources fix is quoted: K-779 (2) and (6), K-785, K-786, the field names of 04 7.1.15 ("Ad", "Yeni e-posta", "Yeni şifre"), the section and
/// operation names of 04 5.25.3-5.25.7 and 9.21.4-9.21.5. Where 04 and 07 give no wording (the form buttons, the password-changed and no-password sentences, the pending-change line, the deletion's
/// result state) the sentence is the minimal one that says what 04 describes; those are open in the backlog (D-1452) and listed as a warning in the task report.
/// </summary>
public static class AccountTexts
{
    /// <summary>04 5.25.2: the screen's heading and tab title ("Hesabım" of the frame, 3.1.6).</summary>
    public const string ScreenName = "Hesabım";

    /// <summary>04 5.25 (K-768): the inner navigation of the account screens.</summary>
    public const string NavAccount = "Hesabım";

    public const string NavAddresses = "Adres defteri";

    public const string NavOrders = "Siparişlerim";

    public const string NavLabel = "Hesap";

    public const string NameHeading = "Ad";

    public const string NameLabel = "Ad";

    public const string NameButton = "Adı kaydet";

    public const string EmailHeading = "E-posta";

    public const string ChangeEmailLink = "E-postayı değiştir";

    public const string NewEmailLabel = "Yeni e-posta";

    public const string NewEmailButton = "Doğrulama bağlantısı gönder";


    /// <summary>04 5.25.11 (K-786): the field message of an address that belongs to another verified account.</summary>
    public const string EmailTaken = "Bu e-posta adresi başka bir hesapta kayıtlı.";

    public const string PasswordHeading = "Şifre";

    public const string ChangePasswordLink = "Şifreyi değiştir";

    public const string SetPasswordLink = "Şifre belirle";

    public const string NewPasswordLabel = "Yeni şifre";

    public const string PasswordButton = "Şifreyi kaydet";

    /// <summary>04 5.25.10 (07 UC-65): the lasting message of a changed password; the session that changed it stays open.</summary>
    public const string PasswordChanged = "Şifreniz değişti; bu oturum dışındaki bütün oturumlarınız kapatıldı.";

    /// <summary>07 UC-65 422: an account without a password is not offered the change; the way is the password-setting.</summary>
    public const string NoPassword = "Hesabınızın şifresi yok. Şifre belirleyebilirsiniz.";

    public const string SignInMethodsHeading = "Giriş yolları";

    public const string DeleteHeading = "Hesabı sil";

    /// <summary>04 5.25.7 (02 3.15.1, 3.15.2): what the deletion does and what stays; the button that starts it follows.</summary>
    public const string DeleteSummary =
        "Hesabınızın adı, giriş bilgileriniz, oturumlarınız, adres defteriniz ve sepetiniz hemen silinir. Siparişleriniz sürer; sipariş e-postanızdaki bağlantıyla ya da sipariş numarası ve e-postanızla izleyebilirsiniz.";

    public const string DeleteLink = "Hesabı sil";

    /// <summary>04 5.25.13 (K-785): the name of the last step's heading and its button's name differ - the button is "Hesabımı sil".</summary>
    public const string DeleteOperationName = "Hesabı sil";

    /// <summary>04 5.25.13 (K-785): the last step's button.</summary>
    public const string DeleteConfirmLabel = "Hesabımı sil";

    /// <summary>04 5.25.13 (K-785): the one sentence of the last step.</summary>
    public const string DeleteSentence = "Hesabınız ve hesabınızdaki adınız, giriş bilgileriniz, adres defteriniz ve sepetiniz hemen silinecek; siparişleriniz sürer.";

    /// <summary>07 UC-67 422: there is no pending change (it ended or was never made).</summary>
    public const string NothingPending = "Bekleyen bir e-posta değişikliği yok.";

    public const string OperationClosedTitle = "Şu anda yapılamaz";

    /// <summary>The line of the e-mail section: the account's address.</summary>
    public static string EmailLine(string email) => email;

    /// <summary>04 5.25.4: the pending change - the new address, the wait and the last day of the link.</summary>
    public static string PendingLine(string newEmail, DateTimeOffset linkEndsAt) =>
        $"{newEmail} adresinin doğrulanması bekleniyor; bağlantı {DisplayFormat.Date(linkEndsAt)} tarihine kadar geçerli.";

    /// <summary>04 5.25.4, 5.25.7 (K-779 (6); 02 3.13.14, 3.15.6): the revert link is open, so the e-mail change and the deletion are closed - the reason and the date.</summary>
    public static string RevertOpen(DateTimeOffset revertEndsAt) =>
        $"Önceki e-posta adresinize gönderilen geri alma bağlantısı {DisplayFormat.Date(revertEndsAt)} tarihine kadar açık. Bu tarihe kadar hesabınız silinemez ve e-posta adresiniz yeniden değiştirilemez.";

    /// <summary>04 5.25.6: the e-mail and password way - the password is there or not.</summary>
    public static string PasswordMethod(bool hasPassword) => hasPassword ? "E-posta ve şifre: şifre var" : "E-posta ve şifre: şifre yok";

    /// <summary>04 5.25.6: the Google way - linked or not; read only (no unlinking, no linking on this screen).</summary>
    public static string GoogleMethod(bool linked) => linked ? "Google: bağlı" : "Google: bağlı değil";
}

/// <summary>
/// The account deletion's result state (T517; 04 5.25.13; 07 UC-69; K-781, K-1184): shown once, at <c>/hesap/silindi</c>. Written here once; the page and its tests read them from here.
/// </summary>
public static class AccountDeletedTexts
{
    public const string ScreenName = "Hesabınız silindi";

    public const string Deleted = "Hesabınız silindi.";

    public const string DeletedThings = "Adınız, giriş bilgileriniz, oturumlarınız, adres defteriniz ve sepetiniz silindi.";

    public const string OrdersGoOn = "Siparişleriniz sürüyor; sipariş numarası ve e-posta adresinizle sipariş takibinden izleyebilirsiniz.";

    public const string EmailSent = "Hesabınızın e-posta adresine silindiğine dair bir e-posta gönderdik.";

    public const string TrackingLink = "Sipariş takibi";

    public const string HomeLink = "Ana sayfa";
}
