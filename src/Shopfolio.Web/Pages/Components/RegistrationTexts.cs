using Shopfolio.Infrastructure;
using Shopfolio.Modules.Membership.Contracts;
using Shopfolio.Modules.Membership.Contracts.Results;
using Shopfolio.Pages.Frame;

namespace Shopfolio.Pages.Components;

/// <summary>
/// The registration screen's texts (T514; 04 5.20; 03 9.1). Forms and account screens speak "siz" (04 8.2.9; K-779, K-772's form rule). Written here once; the page and its tests read them from here.
/// </summary>
public static class RegistrationTexts
{
    /// <summary>The screen's name: its heading and, with the brand name, its tab title (04 5.20.3).</summary>
    public const string ScreenName = "Kayıt ol";

    public const string NameLabel = "Ad";

    public const string EmailLabel = "E-posta";

    public const string ConcealedLabel = "Şifre";

    /// <summary>04 5.20.3 item 5: the primary button.</summary>
    public const string SubmitLabel = "Kayıt ol";

    /// <summary>04 5.20.9: the field message of an address that belongs to an account, a pending registration included (03 3.4.3).</summary>
    public const string EmailAlreadyRegistered = "Bu e-posta zaten kayıtlı.";

    /// <summary>04 5.20.9: the link beside that message goes to the sign-in (the frame's "Giriş yap").</summary>
    public const string SignInLabel = "Giriş yap";

    /// <summary>04 5.20.4, 5.20.7: the button that asks for the verification link again.</summary>
    public const string ResendLabel = "Bağlantıyı yeniden gönder";

    /// <summary>04 5.20.3 item 4: the link of the privacy notice beside the form (02 3.33.5).</summary>
    public const string PrivacyNoticeLabel = "Kişisel verilerinizin nasıl işlendiğini aydınlatma metninde okuyabilirsiniz.";

    /// <summary>03 9.1.3 (K-779, second text): told in place after the link was asked for again; the date is the first send's last day - asking again does not extend it (K-659).</summary>
    public static string LinkResent(DateTimeOffset linkEndsAt) =>
        "Yeni bir bağlantı gönderdik; önceki bağlantılar artık geçersiz. Bağlantı " + DisplayFormat.Date(linkEndsAt) + " tarihine kadar geçerli.";

    /// <summary>04 5.20.3 item 3: the hint of the secret field - the minimum length P-17 (02 3.13.15); no other rule is imposed (04 5.20.5).</summary>
    public static string ConcealedHint => "En az " + DisplayFormat.Number(ProductConstants.PasswordMinLength) + " karakter";

    /// <summary>04 5.20.4 (K-779): the state after the form was sent - where the link went, that the account opens when the address is verified, and the last day of the link.</summary>
    public static string Submitted(string email, DateTimeOffset linkEndsAt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        return "Doğrulama bağlantısını " + email + " adresine gönderdik. Hesabınız, bağlantıyı " + DisplayFormat.Date(linkEndsAt)
            + " tarihine kadar açtığınızda kullanıma açılır.";
    }
}

/// <summary>What the verification landing page tells for one state of a link (04 5.21.2 to 5.21.8, 5.21.14, 5.21.15): one sentence and one next step.</summary>
/// <param name="Sentence">The one sentence.</param>
/// <param name="Meaning">The message's meaning.</param>
/// <param name="ButtonLabel">The verification step's one button; null when there is no step to take.</param>
/// <param name="NextLabel">The next step's link text, when there is no button.</param>
/// <param name="NextPath">The next step's local path.</param>
public sealed record VerificationLanding(string Sentence, MessageMeaning Meaning, string? ButtonLabel, string? NextLabel, string? NextPath);

/// <summary>The six states of the landing page by link kind (04 5.21): decided from what the module read, never from the request.</summary>
public static class VerificationLandings
{
    public const string ScreenName = "E-posta doğrulama";

    private const string SignInNext = "Giriş yap";

    /// <summary>04 5.21.4, 5.21.12: an unknown, foreign or ended link tells the registration's expired state.</summary>
    public static VerificationLanding Unrecognized { get; } =
        new("Bağlantı geçersiz, yeniden kayıt olun.", MessageMeaning.Warning, null, "Kayıt ol", FramePaths.Register);

    /// <summary>
    /// The landing for what the link reads now. <paramref name="done"/> is the result code of a button that was just pressed (the notice of the 303, 04 5.21.3, 5.21.7, 5.21.14) and counts
    /// only on a link that is now used - the code alone, on the address of a valid or a foreign link, tells nothing. <paramref name="addressTaken"/> is the 422 of the new address's button.
    /// </summary>
    public static VerificationLanding Of(EmailLinkReading reading, bool done, bool addressTaken, bool isMember)
    {
        ArgumentNullException.ThrowIfNull(reading);
        if (reading.Kind is not { } kind)
        {
            return Unrecognized;
        }

        string accountPath = isMember ? FramePaths.Account : FramePaths.SignIn;
        string accountLabel = isMember ? "Hesabım" : SignInNext;
        bool justDone = done && reading.Status == VerificationLinkStatus.Used;
        return kind switch
        {
            EmailLinkKind.Registration => Registration(reading.Status, justDone),
            EmailLinkKind.NewAddress => NewAddress(reading.Status, justDone, addressTaken, accountLabel, accountPath),
            _ => Google(reading.Status, justDone),
        };
    }

    private static VerificationLanding Registration(VerificationLinkStatus status, bool justDone) => status switch
    {
        VerificationLinkStatus.Valid => new("E-posta adresinizi doğrulamak için düğmeye basın.", MessageMeaning.Info, "E-postamı doğrula", null, null),
        VerificationLinkStatus.Used when justDone => new("Hesabınız açıldı. E-postanıza ait önceki misafir siparişleriniz varsa Siparişlerim'de görünür. Doğrulama oturum açmaz; giriş yapabilirsiniz.", MessageMeaning.Success, null, SignInNext, FramePaths.SignIn),
        VerificationLinkStatus.Used => new("Hesabınız zaten doğrulandı.", MessageMeaning.Info, null, SignInNext, FramePaths.SignIn),
        VerificationLinkStatus.Superseded => new("Bu bağlantı geçerliliğini yitirdi; en son gönderilen bağlantıyı kullanın. Doğrulanmamış hesapla girişte bağlantıyı yeniden isteyebilirsiniz.", MessageMeaning.Warning, null, SignInNext, FramePaths.SignIn),
        _ => Unrecognized,
    };

    private static VerificationLanding NewAddress(VerificationLinkStatus status, bool justDone, bool addressTaken, string accountLabel, string accountPath)
    {
        if (addressTaken)
        {
            return new("Bu adres başka bir hesaba ait. Değişiklik bu bağlantıyla geçerli olmadı; hesabınız önceki adresiyle çalışır, değişikliği hesap ekranından yeniden başlatabilirsiniz.", MessageMeaning.Error, null, accountLabel, accountPath);
        }

        return status switch
        {
            VerificationLinkStatus.Valid => new("Hesabınızın e-posta adresini bu yeni adresle değiştirmek için düğmeye basın.", MessageMeaning.Info, "Yeni adresi doğrula", null, null),
            VerificationLinkStatus.Used when justDone => new("Yeni e-posta adresiniz doğrulandı; hesabınız bundan sonra bu adresle çalışır ve önceki adrese bildirim gitti. Yeni adrese ait önceki misafir siparişleriniz varsa hesabınıza düştü.", MessageMeaning.Success, null, accountLabel, accountPath),
            _ => new("Değişiklik bu bağlantıyla geçerli olmadı; hesabınız önceki adresiyle çalışır. Değişikliği hesap ekranından yeniden başlatabilirsiniz.", MessageMeaning.Warning, null, accountLabel, accountPath),
        };
    }

    private static VerificationLanding Google(VerificationLinkStatus status, bool justDone) => status switch
    {
        VerificationLinkStatus.Valid => new("Bu hesaba Google ile girişi onaylıyor musunuz? Onaylarsanız hesabınıza bundan sonra Google ile de girilebilir; Google ile giriş yapmayı siz denemediyseniz onaylamayın.", MessageMeaning.Info, "Google girişini onayla", null, null),
        VerificationLinkStatus.Used when justDone => new("Google girişi hesabınıza bağlandı; artık Google ile giriş yapabilirsiniz.", MessageMeaning.Success, null, SignInNext, FramePaths.SignIn),
        VerificationLinkStatus.Used => new("Google girişi bu bağlantıyla zaten onaylandı.", MessageMeaning.Info, null, SignInNext, FramePaths.SignIn),
        _ => new("Bu bağlantı geçerli değil; hesap Google'a bağlanmadı. Google ile yeniden girişte yeni bir bağlantı gönderilir.", MessageMeaning.Warning, null, SignInNext, FramePaths.SignIn),
    };
}
