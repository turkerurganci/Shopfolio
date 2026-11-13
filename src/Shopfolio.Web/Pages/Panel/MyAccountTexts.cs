using Shopfolio.Infrastructure;

namespace Shopfolio.Pages.Panel;

/// <summary>
/// The words of the administrator's own account page (T523; 04 9.21; E-50): written here once. The page has three sections - the name, the e-mail address and the password - and each section is its own form.
/// </summary>
internal static class MyAccountTexts
{
    public const string Title = "Hesabım";

    public const string NameHeading = "Ad";

    public const string NameLabel = "Ad";

    public const string NameButton = "Adı değiştir";

    /// <summary>04 9.21.3: where the name is read; the customer never sees it (02 10.2.4; K-736).</summary>
    public const string NameNote = "Adınız yönetici listesinde, işlem izinde ve yöneticilere giden bildirimlerde okunur; müşteriye görünmez.";

    /// <summary>K-819's field message of an address that belongs to another administrator account (04 7.2.24.2, 9.21.8); not K-820's message of the administrators page.</summary>
    public const string AdminEmailTaken = "Bu e-posta adresi başka bir yönetici hesabında kayıtlı.";

    public const string EmailHeading = "E-posta";

    public const string EmailLabel = "E-posta adresi";

    public const string NewEmailLabel = "Yeni e-posta adresi";

    public const string EmailButton = "E-postayı değiştir";

    public const string EmailSaveButton = "Doğrulama bağlantısı gönder";

    /// <summary>04 9.21: the address is the account's sign-in address; the firm's contact address is another record and is not touched.</summary>
    public const string EmailNote = "Bu adres panele giriş adresinizdir; firmanın iletişim e-posta adresi değildir ve bu değişiklikten etkilenmez.";

    public const string PendingHeading = "Bekleyen e-posta değişikliği";

    public const string ResendButton = "Bağlantıyı yeniden gönder";

    public const string PasswordHeading = "Şifre";

    public const string PasswordButton = "Şifreyi değiştir";

    public const string NewPasswordLabel = "Yeni şifre";

    public const string PasswordSaveButton = "Şifreyi kaydet";

    /// <summary>The result of a password change: it stays in place of the section (04 9.21; 07 6.3.2).</summary>
    public const string PasswordChanged = "Şifreniz değişti; bu oturum dışındaki bütün oturumlarınız kapatıldı.";

    /// <summary>No pending change to ask the link of (07 UC-180).</summary>
    public const string NoPendingChange = "Bekleyen bir e-posta değişikliği yok.";

    /// <summary>K-819: an open revert link closes the change until its last day.</summary>
    public static string RevertOpen(DateTimeOffset endsAt) =>
        "Önceki e-posta adresinize gönderilen geri alma bağlantısı " + DisplayFormat.Date(endsAt) + " tarihine kadar açık. Bu tarihe kadar e-posta adresiniz yeniden değiştirilemez.";

    /// <summary>04 9.21: the pending change - the new address waits for its verification link, which lives until the day.</summary>
    public static string Pending(string newEmail, DateTimeOffset linkEnd) =>
        newEmail + " adresi doğrulanmayı bekliyor. Doğrulama bağlantısı " + DisplayFormat.Date(linkEnd) + " tarihine kadar geçerli; o zamana kadar hesabınız mevcut adresinizle çalışır.";
}
